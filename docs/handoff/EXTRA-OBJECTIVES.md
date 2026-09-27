# Extra objectives (eoid, eoet, eoir, eoep)

Research for a Xenvious page, from Rockstar's source (`D:\decompiled scripts gta 5`,
`FMMC_header.sch`, `FMMC_Cloud_loader.sch`, `FMMC_Creation.sch`) and the Enhanced
`public_mission_creator.c`.

## What it is

A mission entity (ped, vehicle, object or go-to location) normally belongs to one rule per
team. "Extra objectives" give the same entity more rules: it takes part in several objectives
of the mission, for example first "go to vehicle 3", later "deliver vehicle 3".

- Up to 30 entities (`MAX_NUM_EXTRA_OBJECTIVE_ENTITIES`) can have extra objectives.
- Each of them up to 13 extra rules (`MAX_NUM_EXTRA_OBJECTIVE_PER_ENTITY`) per team (4 teams).
- The entity must already have a normal rule; the extra ones come on top.

## Data

| JSON | Global (g_FMMC_STRUCT) | Shape | Meaning |
|---|---|---|---|
| `eoid` | `iExtraObjectiveEntityID[30]` | array | index of the entity inside its type, -1 = slot free |
| `eoet` | `iExtraObjectiveEntityType[30]` | array | `ciRULE_TYPE_*`: 1 ped, 2 vehicle, 3 object, 4 go-to (0 none, 5 player) |
| `eoir` | `iExtraObjectiveRule[30][13][4]` | dict `eor<e>_<n>_<team>` | rule type (kill, go to, deliver …), -1 = empty |
| `eop` / `eoep` | `iExtraObjectivePriority[30][13][4]` | dict `eop<e>_<n>_<team>` | rule number (priority) in the team's rule list |

The user's note says "eoer"; the source uses `eoir` for the rule dict (keys `eor…`) and `eoep`
for the priority dict (keys `eop…`). Old ("legacy FMMC content") jobs write the keys without
underscores (`eor000`), new ones with (`eor0_0_0`).

Offsets exist already in both `offsets.ini` (`OFFSET_eoid/eoet/eoir/eoep`, fields in `GTA.cs`),
no code uses them yet. Strides on Enhanced: `eoep - eoir = 1981 = 1 + 30 * (1 + 13 * (1 + 4))`,
so `eoir[e]` = base + 1 + e * 66, `[n]` = + 1 + n * 5, `[team]` = + 1 + team. Same for `eoep`.
Check the Legacy numbers the same way before use.

## How the game uses it

- Creator: `GIVE_EXTRA_OBJECTIVE_ENTITY_NEW_RULE(entity, type, priority, rule, team)` finds the
  entity's slot (or the first free one, `eoid = -1`), then the first free rule slot for the team,
  and writes rule, priority, type and id. Errors: `FMMC_ER_EOE` (all 30 slots used),
  `FMMC_ER_EOER` (all 13 rule slots of the entity used).
- `CHECK_FOR_BAD_EXTRA_OBJECTIVE_DATA` (creator start) clears slots whose entity index is past
  the number of placed peds / vehicles / objects / go-tos.
- `REMOVE_EXTRA_OBJECTIVE_ENTITY` and `MOVE_EXTRA_OBJECTIVE_ENTITIES_DOWN_1` (FMMC_Creation) run
  when an entity is deleted: the indices of later entities of that type move down by one.
- The mission controller reads the arrays at mission start and gives the entity its extra rules.

## What this affects (why it is not a small page)

1. Deleting or duplicating peds, vehicles, objects or go-tos in Xenvious must shift `eoid` like
   the creator does, otherwise extra rules point to the wrong entity.
2. Copy Jobs (JSON import) should carry `eoid/eoet/eoir/eoep`, with the two key styles.
3. Rules: removing or moving a rule changes priorities; extra objectives refer to priorities.
4. The rule type values (`eoir`) are the same as the normal per-entity rule types; the page
   needs their names (from the mission creator source).

## Proposal

- `Creator Classes/ExtraObjectives.cs`: read / write one slot, find or add a slot for an entity
  (same logic as `GIVE_EXTRA_OBJECTIVE_ENTITY_NEW_RULE`), remove, shift on delete, validate.
- UI: an "Extra rules" card on the Actor, Vehicle, Object and Go-to pages for the selected
  entity: list per team (rule + priority), add / remove. Plus a mission overview page that lists
  all 30 slots.
- Hook the shift into the existing delete / duplicate code of those pages.
- Copy Jobs: import the four keys.
- Test in the Mission creator (Enhanced first, then Legacy for the Legacy offsets).

## Built so far (not tested in game)

- `eoir` holds the creator's selection value (`ciSELECTION_*`), not the rule logic; the controller
  converts it with `GET_FMMC_RULE_FROM_CREATOR_RULE`. The rule lists per entity type are in
  `ExtraObjectives.RuleTypesFor` (names are ours, key `eo_r_<value>`; Rockstar's `FMMC_RLM_*`
  labels are in no dump on disk).
- `Creator Classes/ExtraObjectives.cs`: find / add (like the creator) / edit / remove, free a slot
  when its last rule goes, shift on delete.
- `Controls/ExtraRulesCard.cs`: the card (team tabs, rule rows, add row, "?" explanation dialog),
  on the Vehicle and Actor pages; the delete buttons of both pages shift the indices.
- Explanation with example: `eo_help_text` (dialog from the card's "?").

Next: card for objects and go-tos, the overview page of all 30 slots with the explanation on it
(the "?" then jumps there), warnings (rule number past the team's rules, entity without own rule
for the team), duplicate handling.
