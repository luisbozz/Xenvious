# Creator internals

How Xenvious makes the creator do things. Read
[Game memory](Game-Memory.md) first.

## Where the job lives

- **Job data** (what gets saved and published) is in globals, in three
  big structs. Judging by the keys in `offsets.ini`: `Global_4718592` holds
  the job header (title, description, image flags), checkpoints, play area,
  zones, teleports and rules; `Global_4980736` holds actors, vehicles,
  dynamic props, objects, weapons, goto locations and doors;
  `Global_5242880` holds the placed (static) props. Each list is an array with a count global, e.g.
  `Props.number`, `Props.loc` and the stride `Props.NEXT`.
- **Creator UI state** (current menu, camera, the worker state machine) is in
  the locals of the creator script, see `MainWindow.CreatorLocals.cs`.
- Which creator is running: `CreatorMap.CurrentCreator()` /
  `GTA.CurrentCreatorName()` return the script name (`fm_race_creator`, ...).
  `Functions.isRace()`, `isLTS()`, `isCapture()` ... check the job type
  global instead.

Limits differ by edition: 300 placed props on Enhanced, 200 on Legacy. Other
Enhanced limits are not known yet.

Text fields (title, description) are stored in chunks of 63 bytes plus a
terminator, one chunk every 16 slots; see `setDescribtion` in
`MainWindow.Misc.CopyJobs.cs`.

## Options the creator does not show

The option bitsets `menubs` to `menubs32` (`Global_4718592.f_12` to `f_43`)
hold more bits than the creator menus offer. The creator saves each bitset
as a whole, so a bit set from outside stays in the published job, and the
mission controller acts on some bits the LTS creator never sets. The Mission
page's Extras tab (`MainWindow.Mission.Extras.cs`) offers the ones whose
effect was found in the decompiled `fm_mission_controller`, the controller LTS
jobs run on; `fm_mission_controller_2020` was not checked.

To add one, find `BitTest(Global_4718592.f_N, b)` in the controller and read
what the code does when it is set. `menubsN` is `f_(11+N)`, and script bit `b`
is Xenvious bit `b + 1` (`writebinary` counts from 1). Per-entity bitsets
count the same way; for example `vbs2` bits 3 to 6 lock a vehicle for team 1
to 4.

## Rebuild: making changes visible

Writing job data changes the saved job, but the creator still shows the old
entities. `Creator Classes/CreatorMap.cs` forces a rebuild:

- The creator's main state (`worker.f_565`) is `3` while editing. Setting it
  to `7` runs the "back from testing" path: it deletes every placed entity
  without touching the job data, builds everything again from the globals,
  then returns to `3`.
- That path also resets the menu and moves the camera. For the one frame the
  rebuild takes, those two statements are NOPed in the bytecode and restored
  afterwards, so the user keeps their menu and camera.
- Race, LTS and Capture share the state numbering. **Deathmatch and Survival
  do not**, so `CanRebuild()` is false there until someone verifies them.
- Other states used: `5` = save, `91` = publish (see
  `MainWindow.CopyJobModes.cs`).

Anything that writes job data in bulk (map restore, copy job, moving props
between lists) ends with a rebuild.

## Map backup

`CreatorMap` snapshots the raw global arrays (props, dynamic props,
checkpoints, templates) into an `.xvmap` file (JSON with gzip+base64 slot
data) under `%AppData%\Xenvious\maps\`. Raw slots keep every field, even ones
Xenvious does not know, but tie the file to the edition and build it was taken
on; restore refuses anything else. Before a restore the current state is saved
as an undo file. UI: `MainWindow.MapBackup.cs`.

The snapshot format has a `version`; bump it when the meaning of saved data
changes (version 1 files saved a wrong dynamic prop count and are handled
specially).

## Static and dynamic props

`Creator Classes/PropMover.cs` moves a prop between the static list
(`Props`) and the dynamic list (`DProps`): copy the slots field by field (the
two strides differ), shift the rest of the source array left, clear the last
element, update both counts, rebuild.

## Precise templates

`Creator Classes/PreciseTemplates.cs` gives templates the "Advanced Options"
(override position/rotation) that Rockstar only offers for single props. It
switches a set of triggered script patches in while the template category is
selected and points the creator's "displayed prop" at the template's first
object while Override Position is open. The class comment explains why that
alias must be short-lived (a dead handle crashes the game later).

## Advanced prop placement

`Creator Classes/AdvancedPlacement/` (`RepeatPlanner`, `PropPlacementService`,
dimensions) with `ViewModels/AdvancedPropPlacementViewModel*.cs` and
`Controls/AdvancedPropPlacementView.xaml` / `PlacementPreview3D.xaml`: repeat
a prop along a line, curve, spiral or loop, with a live 3D preview and
presets (`OfflineData/placement_presets.json`). This part uses MVVM
(CommunityToolkit.Mvvm source generators).

## Copy jobs

`MainWindow.Misc.CopyJobs.cs`: takes a Social Club job link or the bare
22-character job id, loads the job details, then the job JSON that sits next
to the job image on Rockstar's cloud (`<part>_<version>_<language>.json`,
trying languages and versions). The JSON maps to the classes in
`JSON/job_json.cs`; each field is written into the matching global, then a
rebuild, then optionally save or publish (publishing asks first).

## Freezers

`GlobalFreezer` / `LocalFreezer` keep writing a value to a global or local, for
values the creator keeps resetting. Used by the global/local editor
(`MainWindow.GlobalLocalEditor.cs`).
