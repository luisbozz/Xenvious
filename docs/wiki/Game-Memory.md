# Game memory: globals and locals

Everything Xenvious shows or changes lives in the memory of the GTA V process.
This page explains how the code gets from a name like
`Global_5242880.f_1[i /*170*/].f_3` to a byte in the game, and what to watch
out for.

## Attaching to the game

- `GameVariant.cs` decides the edition by process name: `GTA5_Enhanced` is
  checked first, then `GTA5`. Legacy and Enhanced are compiled separately, so
  **every address and every pattern differs**; data of one edition is never
  valid for the other.
- `MainWindow.GameConnection.cs` (`Timercheckgta_Tick`) polls for the process,
  reloads data when the edition changes (`OffsetLoader.Load()`), opens the
  process with the memory library and resolves the engine pointers by pattern
  scan (AOB).
- Memory access goes through **mry** (`mry.mem`, the static
  `MainWindow.m`). `m.memory(address)` or `m.memory(base, offsets[])` returns a
  handle with `Get<T>()`, `Write<T>()`, `GetString()`, `GetBytes()` and so on.
- Patterns are scanned in the **running process**, not the exe on disk (the
  file is packed). A miss is logged and returns `IntPtr.Zero`; callers must
  handle that.
- Keep patterns RIP-relative (`48 8B 05 ? ? ? ?`). A looser `48 8B ?` also
  matches other encodings and gives false hits. When a pattern breaks on one
  edition, try the other edition's pattern first; they often still match.
- Pattern scans read the whole game module (~90 MB). `Helper Classes/AobCache.cs`
  remembers each hit per game binary so later starts skip the scan.

## Script globals

GTA's scripts share one big array of 8-byte slots, the globals. The decompiler
names them `Global_<index>`. The engine stores them in 64 blocks of `0x40000`
slots; `Global.cs` turns an index into an address:

```csharp
block   = GlobalPTRversion + 8 * ((index >> 18) & 0x3F)   // pointer to the block
address = *block + 8 * (index & 0x3FFFF)                 // slot inside it
```

Use the `Global` class, never raw addresses:

```csharp
int count = new Global(GTA.Offsets.Editor.Props.number).Get<int>();
new Global(GTA.Offsets.Editor.Props.loc + i * GTA.Offsets.Editor.Props.NEXT).WriteVector3(pos);
```

### Reading the decompiler notation

| Notation | Meaning |
|---|---|
| `Global_5242880` | slot 5242880 |
| `.f_3` | field 3 of a struct, i.e. 3 slots further |
| `[i /*170*/]` | array element `i`; each element is 170 slots long |

Arrays start with a **size slot**: element 0 begins one slot after the
array's base. Never overwrite that size slot; the creator misbehaves
(flickering menus, jobs that will not load). That is why `OffsetLoader` passes `add = 1` for array fields
(`GetGlobalOffset(..., 1)`), and why `CreatorMap` treats the slot before
`FirstElement` as the array header.

`OffsetLoader.GetGlobalOffset` strips everything in `[...]` and adds up the
remaining numbers, so the stride in the comment is **not** used at runtime. Strides come
from separate `*_NEXT` keys in `offsets.ini` (`OFFSET_props_next = 170`), and
code computes an element as `base + index * NEXT + field`.

Example, the rotation of placed prop `i`:

```text
OFFSET_props_vrot = "Global_5242880.f_1[i /*170*/].f_3"   ->  5242880 + 1 + 3 (+1 array header)
OFFSET_props_next = 170
address slot     = Props.vrot + i * Props.NEXT
```

## Script locals

A creator's UI state (menus, camera, the "worker" state machine) lives in the
**locals** of its script thread, not in globals.

- `GTA.getCurrentCreatorAddy()` walks the script thread list (found via the
  `localptr` pattern) and finds the running creator (`fm_race_creator`,
  `fm_lts_creator`, `fm_capture_creator`, `fm_deathmatch_creator`,
  `fm_survival_creator`, ...). It matches by script hash
  (`OFFSET_script_hash`) where the edition's ini has it, else by name
  (`OFFSET_script_name`).
- A local's address is `thread + OFFSET_script_local_start -> + 8 * localIndex`.
  See `MainWindow.CreatorLocals.cs` (`getCurrentCreatorBase`,
  `getCreatorScriptLocalWorkerBase`, ...).
- Locals differ per creator script. `offsets.ini` has one key per creator,
  e.g. `OFFSET_current_creator_worker_race = "fLocal_51531"`.
- **Only the creator you are in has its script loaded.** Reading another
  creator's locals (say capture while you are in LTS) returns garbage. Test
  locals in the creator they belong to.
- The worker struct layout (`f_4` cursor, `f_533` menu, `f_565` state) has
  stayed the same across builds; only the base local moves.
- The thread list is long on Enhanced (~190 threads), so searches scan
  `0x800` bytes of it, not the first few entries.

## Bit fields

Many creator settings are bits in one int. Use the helpers in
`Functions.cs` (`checkbinary`, `writebinary`, `is_bit_set`). Careful: their
bit index is **1-based** (`1 << index - 1`), unlike most decompiled code.

## Threads

- The UI thread and a `BackgroundWorker` (`MainWindow.Worker.cs`) poll the game
  and refresh the UI.
- `ScrPatchesRunner.RunPatcher` runs on its own thread (every 50 ms tick,
  patch pass every 250 ms). `PreciseTemplates.Tick()` runs on that thread too,
  so patch changes never race.
- Timers (`DispatcherTimer`) handle process checks and the job image.
- From a background thread, update the UI with `Dispatcher.BeginInvoke`, not
  `Dispatcher.Invoke`. A blocking `Invoke` from the logger while the UI
  thread waited on a parallel scan once deadlocked the startup.

## Rules for code that touches memory

1. Check `MainWindow.m.IsProcOpen` before reading or writing; the game can
   close at any moment.
2. Never assume an offset is valid: a missing ini key silently loads as `0`.
   Guard against `0` for anything you write to.
3. Never mix editions. Everything edition-specific comes from
   `OfflineData/<edition>/` through `OfflineData`/`OffsetLoader`.
4. Writing the wrong slot corrupts unrelated script state and can crash the
   game or get a job rejected. Test on the edition you changed and say which.
5. Changes that must become visible in the creator usually need a rebuild,
   see [Creator internals](Creator-Internals.md).
