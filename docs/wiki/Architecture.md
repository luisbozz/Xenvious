# Architecture

## At a glance

```text
 GTA5.exe / GTA5_Enhanced.exe
        ^   read/write memory (mry)            ^ patch bytecode
        |                                      |
 +------+-------------------+       +----------+-----------+
 | MainWindow (UI thread)   |       | ScrPatchesRunner     |
 |  partial files per page  |       |  own thread, 50 ms   |
 |  Worker (BackgroundWorker)|      |  PreciseTemplates    |
 +------+-------------------+       +----------+-----------+
        |                                      |
        v                                      v
 GTA.Offsets.Editor (static fields)  <-  OffsetLoader  <-  OfflineData/<edition>/offsets.ini
 GTA.Editor.ScrPatches               <-  OfflineData/<edition>/scrpatches.json
 GTA.Editor.PropList/VehList/...     <-  OfflineData/*.json (shared)
```

## Startup

1. `Startup.cs` installs an assembly resolver (any loaded version of a
   requested assembly is accepted).
2. `App.xaml.cs` hooks the global exception handlers; crashes go to the log.
3. `MainWindow` constructor: sets the thread culture to `de-DE` (number
   formatting uses a decimal comma, keep that in mind when parsing), loads settings and
   offline data, starts the timers, the worker and the patch thread.
4. `Timercheckgta_Tick` (`MainWindow.GameConnection.cs`) waits for the game,
   detects the edition, loads its data and resolves pointers.

## Main parts

| Area | Files |
|---|---|
| Window and shared state | `MainWindow.xaml`, `MainWindow.xaml.cs` |
| One partial file per page or concern | `MainWindow/MainWindow.<Page>.cs` (Props, DynamicProps, Race.Checkpoints, Mission.TeamSettings, Misc.CopyJobs, ...) |
| Game connection and polling | `MainWindow.GameConnection.cs`, `MainWindow.Worker.cs` |
| Memory primitives | `Global.cs`, `Functions.cs`, `GTA.cs` (pointers, script threads), `mry` |
| Offsets | `GTA.cs` (`GTA.Offsets.Editor.*`), `Helper Classes/OffsetLoader.cs`, `IniText.cs`, `AobCache.cs` |
| Edition | `GameVariant.cs` |
| Embedded data | `OfflineData/OfflineData.cs`, `OfflineData/StaticData.cs` |
| Creator logic without UI | `Creator Classes/` |
| Job JSON model | `JSON/` |
| Controls, view models | `Controls/`, `ViewModels/` |
| Texts | `Translation/Translation.cs` |
| Logging | `Logging/Logger.cs`, `Logging/LogView.xaml` |

## Patterns in the code

- Most pages are **code-behind**: the partial `MainWindow` reads globals into
  controls and writes them back on change. Newer parts (advanced placement)
  use MVVM with CommunityToolkit.Mvvm; Caliburn.Micro is also referenced.
- Offsets are **static fields** filled once per edition. There is no
  dependency injection.
- `MainWindow.m` (the mry instance) and `MainWindow.Instance` are global
  singletons used everywhere.
- UI-free logic goes into `Creator Classes/` as static classes
  (`CreatorMap`, `PropMover`, `PreciseTemplates`, `ScrPatchesRunner`). Put
  new logic there, not into the window.

## Network and online services

Offsets, patches and the prop, vehicle and weapon lists are embedded, so
editing works offline. These parts go online:

| Service | Code | Used for |
|---|---|---|
| Rockstar Social Club API | `Functions.API.jobdetails` | job details for Copy jobs |
| Rockstar cloud | `MainWindow.Misc.CopyJobs.cs` | the job JSON next to the job image |
| xenvious.com | `OnlineEnable.cs`, `Functions.API.getonlineenablerchecksum` | Online Enabler: download of `online_enabler.dll` and its checksum |
| xenvious.com resources | `MainWindow.xaml.cs`, `MainWindow.Mission.TeamSettings.cs`, `MainWindow.Converter.cs` | outfit and prop preview images |
| cdn.rage.mp | `MainWindow.Converter.cs` | prop preview images |

The Online Enabler is a separate, closed DLL that Xenvious downloads and
injects into the game; its source is not part of this repository. It is
switched off (`OnlineEnable.Available`) until it has a new source, because the
xenvious.com backend that served the DLL and its checksum is offline.

Four values in `%AppData%\Xenvious\config.ini` are JSON stored as `b64:` +
Base64 (`Helper Classes/ConfigText.cs`); older versions encrypted them with
AES, and `AES/AES.cs` is only kept to read those.

Older versions logged in against a Xenvious backend that also delivered
offsets and patches. That login has been removed.
