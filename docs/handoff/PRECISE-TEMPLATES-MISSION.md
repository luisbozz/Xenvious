# Precise templates in the Public Mission Creator (not built)

Research from 2026-09-28: the 1.73-3889 Legacy scripts plus a live memory read in game (Legacy,
`public_mission_creator`, templates placed and a placed prop overridden).

## Result

The LTS solution does not carry over. In the LTS creator, five script patches add "Advanced Options →
Override Position / Rotation" to the template placement menu. Then Xenvious makes the template's
first object the creator's displayed prop (`PreciseTemplates.cs`). The Mission Creator builds its
prop menu with a new menu system, and the old one those patches extend is not used there. So the
patches either find nothing or would change code that never runs.

The Mission Creator's own Override Position / Rotation only exists after placing, and only for a
single prop. On a placed template it moves one piece, not the template.

## What the live read showed

| | LTS creator (what Xenvious reads) | Mission Creator (measured) |
|---|---|---|
| Category | `pre.f_991`, templates = 64 | `Local_43036.f_1582`, templates = 64. `pre.f_991` stayed 0 |
| Template index | `pre.f_992` | probably `Local_43036.f_1583` (stayed 0, one template tried) |
| Current menu | `pre.f_273` (910 advanced, 920 position, 921 rotation) | `pre.f_273` stayed 0. Menus live in a stack (`Local_46677`, `.f_11`/`.f_12`, plus `Global_1059360`) |
| Template objects | `placement.f_240` (count), `.f_241` first object | same |
| Displayed prop | `placement.f_239` | same, 0 while a template is shown |
| Override Position menu | – | alignment row writes `pre.f_3455` (2/3/4); "Use Override" looks like bit 3 of `pre.f_1175` |

Legacy statics: `pre` = `Local_8402`, `placement` = `Local_6167`, `worker` = `Local_7467`,
`Local_43036` and `Local_46677` are new creator state. The thread was found by the script hash
`0x80A96CAF` at thread + 0xC, statics pointer at thread + 0xB0.

## The five LTS patches in the Mission Creator

| Part | LTS | Mission Creator |
|---|---|---|
| 1 | template overlap check only in menu 4: `pre.f_273 == 4` → `!= 6` | the check exists, but asks the new menu stack (`func_1568(82)`) |
| 2 | templates (category 64) get menu 910 "Advanced Options" instead of the base menu 6 | same code exists (`pre.f_991 == 64`), but the placement menu is not built through it |
| 3, 4 | menu code for position / rotation | patterns hit once; same issue as part 2 |
| 5 | do not delete the displayed prop when its model does not match (`placement.f_239` → `f_238`) | found: `4F ? ? 4F ? ? 5D ? ? ? 4F ? ? 41 EF 2C ? ? ? 56 ? ? 38 ? 4F ? ? 41 EF 2C ? ? ? 58`, offset 14, byte `EE`, once in Legacy |

## Options

- Extend the new "Add New Prop" menu with position / rotation rows in bytecode. A big change in both
  editions, and a wrong jump crashes the game.
- Proposed instead: Xenvious moves and turns a template **after** it was placed. It writes the
  positions of all its pieces into the prop data (around one pivot) and rebuilds the map; the
  rebuild works in the Mission Creator. No script patches, both editions, also usable in LTS and
  Capture. Open: how to find the pieces of one template (a group id on the props, or the last N
  props placed).
