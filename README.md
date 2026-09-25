# Xenvious

A companion tool for the GTA V Content Creator (Race, Last Team Standing,
Capture, Deathmatch, Survival). It edits the creator's job data while the game
runs, so you can do what the in-game menus do not allow: exact positions and
rotations, hidden properties, repeated prop layouts, precise templates, map
backups, copies of other jobs and more.

Works with **GTA V Legacy** and **GTA V Enhanced** on PC.

## Download

Get `Xenvious.exe` from the [latest release](https://github.com/luisbozz/Xenvious/releases/latest).
It is a single file; later versions are offered inside Xenvious on start
(Settings → "Check for updates on start").

Or paste this into PowerShell. It puts the latest release into
`%LOCALAPPDATA%\Programs\Xenvious`, adds Xenvious to the Start menu and starts
it:

```powershell
$dir = "$env:LOCALAPPDATA\Programs\Xenvious"
New-Item -ItemType Directory -Force $dir | Out-Null
curl.exe -fL -o "$dir\Xenvious.exe" https://github.com/luisbozz/Xenvious/releases/latest/download/Xenvious.exe
if ($LASTEXITCODE -eq 0) {
    $link = (New-Object -ComObject WScript.Shell).CreateShortcut("$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Xenvious.lnk")
    $link.TargetPath = "$dir\Xenvious.exe"
    $link.Save()
    Start-Process "$dir\Xenvious.exe"
}
```

Every release exe is built by GitHub Actions from the tagged commit. To check
that a download is exactly that build:

```bat
gh attestation verify Xenvious.exe --repo luisbozz/Xenvious
```

## Documentation

- [Wiki](https://github.com/luisbozz/Xenvious/wiki): building from source,
  architecture, game memory, offsets, script patches, creator internals,
  translations, game updates
- [AGENTS.md](AGENTS.md): short guide for AI coding agents

Offsets and script patches for each game build are generated with
[ysc-global-updater](https://github.com/luisbozz/ysc-global-updater).

## Contributing

Bug reports and pull requests are welcome. Read
[Contributing](https://github.com/luisbozz/Xenvious/wiki/Contributing) first:
it covers commit messages, code style and how to test in the game.

## Code signing policy (hopefully soon)

Free code signing provided by [SignPath.io](https://about.signpath.io/),
certificate by [SignPath Foundation](https://signpath.org/).

- Committers and reviewers: [luisbozz](https://github.com/luisbozz)
- Approvers: [luisbozz](https://github.com/luisbozz)

Only release builds made by `.github/workflows/release.yml` from a tagged commit
on `main` are signed; nothing built on a personal machine is.

## Privacy

Xenvious collects no personal data and sends nothing about you anywhere. It
only connects to the internet for these features:

- **Update check** (on start, can be turned off in Settings): asks the GitHub
  API for the latest release of this repository and downloads it if you choose
  to update.
- **Copy Jobs** (when you load a job): asks the Rockstar Social Club API and
  Rockstar's cloud for the job you entered.
- **Preview images** of props and outfits, loaded from public image hosts.

## License

[GPL-3.0](LICENSE). You may use, change and share Xenvious; if you distribute a
changed version, its source has to be available under the same license.
