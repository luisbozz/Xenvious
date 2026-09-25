# Updating for a new GTA build

Every GTA V title update recompiles the scripts. Struct fields get inserted,
globals move, functions change. After a patch, two files per edition must be
regenerated:

| File | What breaks | Tool |
|---|---|---|
| `OfflineData/<edition>/offsets.ini` | globals, locals, strides move | ysc-global-updater, `tools/run_pipeline.py` |
| `OfflineData/<edition>/scrpatches.json` | patterns stop matching, native indices and call targets change | ysc-global-updater, `scrpatches/check_patches.py` and `repair_scrpatches.py` |

Engine patterns in `[AOB]` break less often; when an attach fails after an
update, check those too.

## The workflow

ysc-global-updater (branch with Enhanced support) runs the whole chain with one
command and asks before every step that writes something:

```bash
python3 update_xenvious.py --new 1.74-4012                      # Legacy
python3 update_xenvious.py --variant enhanced --new 1.74-1200   # Enhanced
python3 update_xenvious.py --new 1.74-4012 --dry-run            # report only
```

```text
1  Fetch the build        fetch_update.sh (decompiled scripts + bytecode dumps)
2  Migrate offsets        tools/run_pipeline.py
3  Check/repair patterns  scrpatches/update_patches.py
4  Repair payloads        scrpatches/repair_scrpatches.py (injected custom functions)
5  Deploy                 Xenvious/OfflineData/<variant>/{offsets.ini,scrpatches.json}
6  Rebuild                reminder only: OfflineData is compiled into the exe
```

Step 5 refuses to deploy while any pattern is still broken. Rollback is git:
`git checkout -- Xenvious/OfflineData`. Each tool is documented in the
updater's README.

Decompiled scripts per build are stored under `scripts/<build>/`; Enhanced
builds carry an `enhanced-` prefix (`scripts/enhanced-1.73-1158/`) and the
tools never compare across editions. Sources are community dumps (for
example calamity-inc/GTA-V-Decompiled-Scripts), or your own export with OpenIV
and a script decompiler when those are behind.

## How the updater finds the new offsets

It does not guess by position. For each key it uses anchors that survive a
recompile: text labels used near the field (e.g. save keys and `MC_H_*`
strings), native calls, function roles, and the family of neighbouring keys
in the same struct. It sets roughly 98 % of the offsets automatically and
lists the rest for manual review.

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
