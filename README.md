# Xenvious

A companion tool for the GTA V Content Creator (Race, Last Team Standing,
Capture, Deathmatch, Survival). It edits the creator's job data while the game
runs, so you can do what the in-game menus do not allow: exact positions and
rotations, hidden properties, repeated prop layouts, precise templates, map
backups, copies of other jobs and more.

Works with **GTA V Legacy** and **GTA V Enhanced** on PC.

## Build

Windows, Visual Studio 2022 (.NET desktop workload), .NET Framework 4.8, and
the [mry](https://github.com/luisbozz/mry) memory library built next to this
repository.

```bat
nuget restore Xenvious.sln
msbuild Xenvious.sln /p:Configuration=Release /p:Platform=x64
```

Details: [Getting started](docs/wiki/Getting-Started.md).

## Documentation

- [Wiki](docs/wiki/Home.md): architecture, game memory, offsets, script
  patches, creator internals, translations, game updates
- [Contributing](docs/wiki/Contributing.md)
- [AGENTS.md](AGENTS.md): short guide for AI coding agents

Offsets and script patches for each game build are generated with
[ysc-global-updater](https://github.com/luisbozz/ysc-global-updater).
