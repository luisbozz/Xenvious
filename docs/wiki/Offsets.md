# Offsets (offsets.ini)

All build-specific numbers live in two files, one per edition:

```text
Xenvious/OfflineData/legacy/offsets.ini
Xenvious/OfflineData/enhanced/offsets.ini
```

They are **embedded resources**, loaded by `OfflineData/OfflineData.cs` via
`GetManifestResourceStream`. Do not change their build action, and do not
read them from disk.

## Sections

| Section | Content |
|---|---|
| `[AOB]` | Byte patterns (`?` = wildcard) for engine pointers: globals, script threads, script programs, camera, image, session, ... |
| `[OTHER]` | Model hash lists (booster props, slowdown props, blacklisted props, ...) |
| `[IMAGE]`, `[SESSION]` | Struct offsets for the job image and the Social Club session |
| `[OFFSETS]` | Globals, locals, struct fields and strides for everything the creator stores |

## Value formats

| Example | Parsed by | Meaning |
|---|---|---|
| `"Global_5242880.f_1[i /*170*/].f_3"` | `GetGlobalOffset` | a global index, see [Game memory](Game-Memory.md) |
| `"fLocal_51531"`, `"f_565"` | `GetGlobalOffset` | a local index or a field offset |
| `170`, `0x1C`, `-0x24` | `IniText.ReadInteger` | plain number, decimal or hex, sign allowed |
| `"8,DD0,30,50"` | split + hex parse | pointer chain |

`Helper Classes/IniText.cs` reads the file once. Keys are
**case-insensitive**, the **first** occurrence of a key wins, quotes are
stripped, and a **missing key returns `0` or `""` without an error**. That last
point is the most common source of silent bugs after an update.

## Where values end up

`Helper Classes/OffsetLoader.cs` (`OffsetLoader.Load()`) copies every value into
static fields of `GTA.Offsets.Editor` in `GTA.cs`, grouped by what they
describe (`Props`, `DProps`, `Vehicle`, `Actor`, `Race.Checkpoints`, `Zones`,
`Doors`, ...). The rest of the code only uses those fields.

## Adding an offset

1. Find the global or local in the decompiled creator script (see
   [Updating for a new GTA build](Game-Updates.md) for where scripts come from).
2. Add the key to **both** `legacy/offsets.ini` and `enhanced/offsets.ini`,
   in decompiler notation. If you can only verify one edition, say so in the
   PR.
3. Add the static field to the matching class in `GTA.cs`.
4. Load it in `OffsetLoader.Load()` with the right `add` (1 for fields inside
   an array element, 0 for plain globals).
5. Also add the key to the updater's `offsets.ini` in ysc-global-updater, so
   the next game update migrates it automatically.
6. Use it through `new Global(...)`, guarded against `0`.
