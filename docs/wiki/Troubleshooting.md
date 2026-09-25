# Troubleshooting

## Build

| Symptom | Cause and fix |
|---|---|
| `mry` reference not found | Build the mry repository first so `..\mry\mry\bin\x64\Release\mry.dll` exists next to this repository. |
| Package restore works, build fails on a `CommunityToolkit.Mvvm` targets file | `packages.config` and `Xenvious.csproj` name different versions. Make them match. |
| VS Code shows "MVVM Toolkit source generators might not be loaded" | Editor warning from the Roslyn LSP with `packages.config`. Use Visual Studio or the OmniSharp-based C# extension. The build itself is fine. |
| Build does nothing on Linux or WSL | Expected. WPF on .NET Framework needs Windows. |
| Editor very slow | The big JSON files in `OfflineData/`. Exclude them from search and file watching. |

## Runtime

| Symptom | Cause and fix |
|---|---|
| Xenvious does not attach | Check the log (`%AppData%\Xenvious\`). Usually a pattern in `[AOB]` no longer matches after a game update. |
| A field shows 0 or nonsense after an update | Its offset moved, or its key is missing in `offsets.ini` (missing keys load as 0 silently). |
| A creator feature stopped working after an update | A script patch no longer matches. See the script patches page in Misc and [Script patches](Script-Patches.md). |
| A change is saved but not visible in the creator | The creator needs a rebuild, see [Creator internals](Creator-Internals.md). Deathmatch and Survival have no verified rebuild yet. |
| Numbers parse wrong | The UI thread runs with the `de-DE` culture (decimal comma). Use `CultureInfo.InvariantCulture` for data, the current culture only for display. |
| Game crashes after a write | A write hit the wrong slot or left a dangling entity handle. Reproduce with the log open and check the offsets and strides involved. |
