# Getting started

## Requirements

- Windows 10/11, x64. Xenvious is a WPF app on **.NET Framework 4.8**; it does
  not build or run on Linux, macOS or WSL.
- Visual Studio 2022 (or its Build Tools) with the **.NET desktop development**
  workload. `nuget.exe` on the `PATH` for command-line restores.
- GTA V on PC, **Legacy** (`GTA5.exe`) or **Enhanced** (`GTA5_Enhanced.exe`),
  for anything beyond compiling.

## Folder layout

Xenvious uses the memory library [mry](https://github.com/luisbozz/mry).
`Xenvious.csproj` references it by a relative path, so the two repositories
sit next to each other:

```text
<parent>/
  mry/        memory library (separate repository)
  Xenvious/   this repository
```

`Xenvious.code-workspace` opens both folders in VS Code.

## Build

```bat
:: 0. clone both repositories into the same parent folder
git clone https://github.com/luisbozz/mry
git clone <this repository> Xenvious
:: 1. build mry first (Release, x64), so ..\mry\mry\bin\x64\Release\mry.dll exists
:: 2. restore packages (classic packages.config, not PackageReference)
nuget restore Xenvious.sln
:: 3. build
msbuild Xenvious.sln /p:Configuration=Debug /p:Platform=x64
msbuild Xenvious.sln /p:Configuration=Release /p:Platform=x64
```

After switching branches or package versions, run `/t:Clean,Build`.

The project is a classic (non-SDK) `.csproj` with `packages.config` and
`LangVersion` 9.0. Keep the version in `packages.config` and the `HintPath`,
`Import` and `Error` lines in `Xenvious.csproj` in sync when you bump a
package; a mismatch builds on a machine that still has the old package folder
and fails on a clean restore.

GitHub Actions builds every pull request the same way from a clean checkout
(`.github/workflows/build.yml`) and attaches the output folder to the run. A
green build means it compiles, not that it works in the game.

## Editors

- **Visual Studio** is the most reliable for building and debugging.
- **VS Code** works for editing. Use the C# extension with OmniSharp
  (`ms-dotnettools.csharp`); the newer Roslyn LSP / C# Dev Kit handles
  `packages.config` and the MVVM Toolkit source generators poorly and shows
  warnings such as "MVVM Toolkit source generators might not be loaded". Those
  are editor warnings, not build errors.
- The large embedded JSON files in `Xenvious/OfflineData/` (`props.json` is
  several MB) slow editors down; exclude them from search and file watching.

## Run

1. Start GTA V (either edition) and load into story mode or GTA Online.
2. Start Xenvious. It polls for the game process, detects the edition, loads
   that edition's offsets and script patches, scans for its pointers and
   starts the patch thread.
3. Open a creator in game. The dashboard shows which creator is running.

Logs and user data go to `%AppData%\Xenvious\` (log files, settings, map
backups under `maps\`). The in-app log view is in `Logging/LogView.xaml`.

## Next

Read [Architecture](Architecture.md), then
[Game memory](Game-Memory.md) before you change anything that touches the
game.
