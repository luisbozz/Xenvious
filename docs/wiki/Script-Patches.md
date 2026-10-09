# Script patches (scrpatches)

Some things the creator refuses outright (more actors, precise templates, the
dev menu, skipping forced rules). Xenvious changes the creator's compiled
script bytecode in memory while the game runs.

## Data

```text
Xenvious/OfflineData/<edition>/scrpatches.json      patches shipped to users
Xenvious/OfflineData/<edition>/scrpatchesdev.json   developer patches (usually empty)
```

One entry:

```json
{
  "patch_name": "dev mode",
  "script_name": "fm_capture_creator",
  "pattern": "2D 00 02 00 00 71 2E ? ? 5D ? ? ?",
  "offset": 5,
  "bytes_to_patch": "72 2E 00 01",
  "dev": false
}
```

- `pattern`: bytes to find in the script's bytecode, `?` = any byte.
  Wildcards absorb operand shifts (native indices, global and function
  addresses) between builds.
- `offset`: where to write, relative to the pattern hit.
- `bytes_to_patch`: what to write. Often `00` (NOP) to remove instructions.
- `values` (optional): dynamic bytes. `{{ id }}` placeholders in
  `bytes_to_patch` are filled with `bytes_to_read` bytes read at a second
  pattern (`pattern`, `offset`) in the same script. Used when a patch has to
  call a function whose address changes per build.

The class for an entry is `GTA.ScrPatches` / `GTA.ScrPatchValue` in `GTA.cs`.

## Runtime

- `Creator Classes/ScrProgramScanner.cs` finds a script's `rage::scrProgram`
  by the hash of its name (registry from the `scrProgramptr` pattern) and
  scans its bytecode, which is stored in `0x4000`-byte pages.
- `Creator Classes/ScrPatchesRunner.cs` runs on its own thread. Every 250 ms it
  applies every patch whose script is loaded, once per address
  (`appliedPatches`), and remembers the original bytes so it can revert.
- Some patches are only active while a feature needs them ("triggered"
  patches), e.g. the precise templates set that `PreciseTemplates` switches
  in and out.
- The Misc page lists all patches with descriptions
  (`MainWindow.Misc.ScrPatches.cs`, descriptions are translation keys).

## Custom functions

`Creator Classes/CustomCreatorFuncsBuilder.cs` assembles small bytecode
functions that are injected into the creator (for example to check a model or
call natives the creator does not). Native indices and call targets differ per
creator script and per build, so they are table-driven per `CreatorType`.

The injected functions are switched on and off through dispatch bits, next to
variables for the hovered model, the model dimensions and the race test
tuning that the functions report back. `Creator Classes/ScriptVars.cs` reads
and writes them; its bit helpers count from 1 like the ones in `Functions.cs`.
**The custom function patch must be active whenever a patch that calls it is
active**; otherwise the creator calls into bytecode that is not there and GTA
crashes.

On Enhanced the creators load with extra 16 KB pages behind their code
(`Creator Classes/ScriptSpace.cs`, `[SCRIPTSPACE]` in `offsets.ini`). The
custom functions and the shape drawer (`ScriptDrawer`) run from the first
extra page; the last one is a data page the script reaches as string page 63,
and it holds the variables. Legacy resources cannot grow, so there the same
payload lives in a creator function given up for it, the vehicle mod preset
table (`[SCRIPTVICTIM]` in the legacy `offsets.ini`). The patch "give up the
vehicle preset table" makes it return at once; one whole 16 KB page of it is
the data page. Placed vehicles with a mod preset then look stock in the
creator; jobs are not affected. The payload sources (custom functions,
drawer, zone labels) and their builder are in ysc-global-updater
(`scrpatches/scrasm/customfuncs/page/`).

## Opcode cheat sheet (GTA V YSC)

| Byte | Instruction |
|---|---|
| `00` | NOP |
| `2C` | NATIVE (index is big-endian) |
| `2D` | ENTER (function start) |
| `2E` | LEAVE (return) |
| `5D` | CALL (3-byte little-endian address) |
| `61` | global (3-byte index) |

For more, use a GTA V script decompiler/disassembler and the `scrasm`
assembler in ysc-global-updater.

## When a patch breaks

A pattern stops matching when Rockstar changes the instruction sequence of
that function, not just its operands. The updater's `check_patches.py` shows
which patches no longer match on a new build, and `repair_scrpatches.py`
re-derives them where it can. See [Updating for a new GTA build](Game-Updates.md).

A patch that matches **more than once** is as bad as none: it may write into
the wrong function. Make patterns long enough to be unique.
