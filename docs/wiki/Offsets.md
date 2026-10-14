# Offsets (offsets.ini)

All build-specific numbers live in two files, one per edition:

```text
Xenvious/OfflineData/legacy/offsets.ini
Xenvious/OfflineData/enhanced/offsets.ini
```

The model hash lists (booster props, slowdown props, blacklisted props, ...)
are in `prop_lists.json` next to them, also per edition. The struct offsets of
the job image (`0x1C`) and the Social Club name (`0x3CF`) never moved and are
constants in `GTA.cs`.

They are the **sources** of the build data, not files the app reads.
`tools/builddata/gen.py` turns them (and `scrpatches.json`) into C# under
`Xenvious/Generated/`, with every path already resolved to a number, so the
start parses nothing. Run it after every change and commit what it writes; CI
runs `python -m unittest discover -s tools/builddata`, which fails on stale
files.

## Sections

| Section | Content |
|---|---|
| `[BUILD]` | `label`: the game build the values are for (`1.73-3889`, `enhanced-1.73-1158`); the updater migrates from it |
| `[AOB]` | Byte patterns (`?` = wildcard) for engine pointers: globals, script threads, script programs, camera, image, session, ... |
| `[SCRIPTSPACE]`, `[SCRIPTVICTIM]` | Room for the custom funcs payload, see [Script patches](Script-Patches.md) |
| `[OFFSETS]` | Globals, locals, struct fields and strides for everything the creator stores |

## Value formats

| Example | Parsed by | Meaning |
|---|---|---|
| `"Global_5242880.f_1[i /*170*/].f_3"` | number | a global index, see [Game memory](Game-Memory.md) |
| `"fLocal_51531"`, `"f_565"` | number | a local index or a field offset |
| `170`, `0x1C`, `-0x24` | number | plain number, decimal or hex, sign allowed |
| `"8,DD0,30,50"`, AOB patterns, model lists | string | split at runtime where needed |

Keys are **case-insensitive** and the **first** occurrence of a key wins;
quotes and a trailing `;comment` are dropped. Each key becomes a member named
after it without `OFFSET_` (`OFFSET_actor_model` -> `d.OFFSETS.actor_model`).
A key only one edition has is `0` or `""` in the other one, so guard against
`0`.

## Where values end up

`Helper Classes/OffsetLoader.cs` (`OffsetLoader.Load()`) copies every value of
`BuildData.Current` into
static fields of `GTA.Offsets.Editor` in `GTA.cs`, grouped by what they
describe (`Props`, `DProps`, `Vehicle`, `Actor`, `Race.Checkpoints`, `Zones`,
`Doors`, ...). The rest of the code only uses those fields.

## Adding an offset

1. Find the global or local in the decompiled creator script (see
   [Updating for a new GTA build](Game-Updates.md) for where scripts come from).
2. Add the key to **both** `legacy/offsets.ini` and `enhanced/offsets.ini`,
   in decompiler notation. If you can only verify one edition, say so in the
   PR.
   Write the whole path: a field that is itself an array (one value per
   team, per rule) ends in `[i]`, so its size slot is counted.
3. Run `python3 tools/builddata/gen.py`.
4. Add the static field to the matching class in `GTA.cs` and set it in
   `OffsetLoader.Load()`: `GTA.Offsets.Editor.X = d.OFFSETS.my_key;`.
5. Nothing else for the updater: ysc-global-updater migrates every key of
   these files at the next game update.
6. Use it through `new Global(...)`, guarded against `0`.
