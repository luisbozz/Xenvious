# Updating for a new GTA build

Every GTA V title update recompiles the scripts. Struct fields get inserted,
globals move, functions change. Per edition, Xenvious keeps everything that
depends on the build in `OfflineData/<edition>/`, and ysc-global-updater brings
all of it to the new build:

| File | What breaks |
|---|---|
| `offsets.ini` | globals, locals, strides move; `[SCRIPTVICTIM]` (Legacy) moves with the code; `[OTHER]` prop lists grow |
| `scrpatches.json`, `scrpatchesdev.json` | patterns stop matching, operands in the patch bytes move, the custom funcs payloads call into moved code |
| `race_vehicles.json` | new (ADLC) vehicles in the race creator |
| `mprops.json` | the prop menus change |

`offsets.ini` says in `[BUILD] label` which build it is for; the updater migrates
from there. offsets.ini and the patches reach the app as generated code
(`Generated/*.g.cs`, written by `tools/builddata/gen.py`), so the app parses no
file at start.

Engine patterns in `[AOB]` break less often; when an attach fails after an
update, check those too.

## The workflow

In ysc-global-updater, one command per edition:

```bash
./xupdate.sh 1.74-4012              # Legacy
./xupdate.sh enhanced-1.74-1200     # Enhanced
```

```text
1  scripts    the new build's decompiled .c and decrypted .ysc.full (it prints how to get them)
2  offsets    every key migrated, then checked against the code around its use
3  patches    every patch found again through its function, patch bytes carried over
4  payloads   victim function / extra page, custom funcs sources moved to the build
5  lists      prop lists, race vehicles, prop-menu originals
6  review     a browser page: old code left, new code right, the guess filled in
7  build      answers applied, payloads assembled
8  deploy     OfflineData/<edition>/ written, tools/builddata/gen.py run
```

Only step 6 needs you, and only for what the tool is not sure about. A run
can be stopped any time; the same command continues, answers included. The
runbook is `xupdate/README.md` in the updater.

After the run: build Xenvious, test in game (below), commit both repositories
(Xenvious: OfflineData + Generated; updater: the moved payload sources).

Decompiled scripts per build are stored under `scripts/<build>/`; Enhanced
builds carry an `enhanced-` prefix (`scripts/enhanced-1.73-1158/`) and the
tools never compare across editions. Sources are community dumps (for
example calamity-inc/GTA-V-Decompiled-Scripts), or your own export from the
game (`tools/extract_ysc.py`, CodeWalker) when those are behind.

## How the updater finds the new offsets

It does not guess by position. For each key it uses anchors that survive a
recompile: text labels used near the field (e.g. save keys and `MC_H_*`
strings), native calls, function roles, and the family of neighbouring keys
in the same struct. Then every result is checked a second way: where the old
path is used in the old script, the same lines (numbers ignored) are looked up
in the new script, and the path at that place is the second answer. Two answers
that agree are taken; anything else goes to the review with both places shown.

## Legacy and Enhanced

Both editions share the script content and the Online version number but are
compiled separately. Run the update for each edition and never copy one
edition's file over the other.

## Pitfalls seen in past updates

- A global path that no longer appears anywhere in the new build's scripts is
  proof that the offset is wrong.
- Everything that moves has to move: all three components of a vector, the
  `*_NEXT` strides, and the struct fields inside payloads. Missing the payload
  struct offsets once broke the creator entirely.
- A key that silently disappears from the migrated file loads as 0 in the app.
  Compare key counts before and after.
- The decompilers differ in dialect (`*Global_`, `iParam`/`uParam`, text label
  helpers); the updater normalises them.

## After updating

0. Restart GTA before each test run, so no bytecode injected by an earlier
   run is still in memory.
1. Start both editions, check that Xenvious attaches and the dashboard shows
   the running creator.
2. Open each creator type you can and check the pages you use: prop lists,
   rebuild (map restore), precise templates.
3. Check the Misc -> script patches page: every patch should be applied, none
   should report a missing pattern.
4. In the pull request, list the build numbers and what you tested.
