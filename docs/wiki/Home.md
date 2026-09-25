# Xenvious Wiki

Xenvious is a Windows companion tool for the GTA V Content Creator (Race, Last
Team Standing, Capture, Deathmatch, Survival). It attaches to the running game,
reads and writes the creator's job data in memory, and patches the creator's
script bytecode where the game itself refuses something.

This wiki explains how the code works, so you can find your way around and
change it safely.

## Start here

| Page | Read it when |
|---|---|
| [Getting started](Getting-Started.md) | You want to build and run Xenvious |
| [Architecture](Architecture.md) | You want the big picture: threads, windows, data flow |
| [Game memory: globals and locals](Game-Memory.md) | You touch anything that reads or writes the game |
| [Offsets (offsets.ini)](Offsets.md) | You add or fix an offset |
| [Script patches (scrpatches)](Script-Patches.md) | You add or fix a bytecode patch |
| [Creator internals](Creator-Internals.md) | You work on rebuild, map backup, templates, copy jobs |
| [UI and translations](UI-and-Translations.md) | You add a page, a control or a text |
| [Updating for a new GTA build](Game-Updates.md) | Rockstar shipped a patch and things broke |
| [Releases and updates](Releases.md) | You want to ship a version or understand the updater |
| [Contributing](Contributing.md) | You want to open a pull request |
| [Troubleshooting](Troubleshooting.md) | Something does not build, attach or behave |
| [Known issues](Known-Issues.md) | You are looking for something to work on |
| [Glossary](Glossary.md) | A word in the code means nothing to you |

## Related repositories

- **[mry](https://github.com/luisbozz/mry)**: the process memory library
  Xenvious uses (`mry.mem`). Built separately, see
  [Getting started](Getting-Started.md).
- **ysc-global-updater**: Python toolchain that regenerates `offsets.ini` and
  `scrpatches.json` from the decompiled scripts of a new game build. See
  [Updating for a new GTA build](Game-Updates.md).
