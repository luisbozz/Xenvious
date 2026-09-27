# Rules: how a mission's objectives are wired (research, not built)

Sources: Rockstar's source (`D:\decompiled scripts gta 5`, `FMMC_*`, `fm_Mission_Creator_*`,
`FM_Mission_Controller_*`) and the LTS example job `CYaUJTEy9UKDwwhVbRcONw` ("LTS Mission",
JSON `prod.cloud.rockstargames.com/ugc/gta5mission/4983/CYaUJTEy9UKDwwhVbRcONw/0_0_de.json`).

## Model

- Every team has a list of rules (`endcon.nrl[team]`, offset `nrl`). Rule 0 is the first
  objective; the team moves to the next rule when the current one is done.
- A rule is not an object of its own. It exists because entities and player rules point at it:
  - peds (`ene.rule<team>` / `ene.pri<team>`), vehicles (`veh`), objects (`obj`), go-to
    locations (`goto.rl` / `goto.pri`): per team a rule type (`FMMC_OBJECTIVE_LOGIC_*` in the
    JSON) and a priority = rule number.
  - player rules (`kill` section, `sPlayerRuleData`): kill players, kill team X, get masks,
    arrest, go to team, loot / points threshold, holding rule; per team type + priority.
  - extra objectives (eoid/eoet/eoir/eoep, see EXTRA-OBJECTIVES.md): more rules for an entity.
- Per rule and team there are many settings in `endcon` (arrays indexed by rule): objective text
  `txt0` (one string per rule), time limit, wanted level, gang chase (`gbtp`, `gbnum` …), outfits,
  bounds (`outb`, `outr`), rule bitsets (`irbs*`, `rlbs*`), etc. Xenvious already edits many of
  them with a "rule" (Gang Index) dropdown.
- The JSON stores the rule logic (`FMMC_OBJECTIVE_LOGIC_*`, e.g. go-to 4, kill 2, kill players 6);
  the creator menu works with selection values (`ciSELECTION_*`), and the extra objectives store
  those (converted by `GET_FMMC_RULE_FROM_CREATOR_RULE`).

## Example LTS (team 1, 3 rules)

| Rule | Objective text | Wired by |
|---|---|---|
| 1 | "Begib dich auf das Dach und sammel die Minigun ein." | go-to location 1, go to (rl 4) |
| 2 | "Suche die Gegner und töte diese …" | peds 1-3, kill (rule 2, pri 1) |
| 3 | "Zerstöre das Auto." | ped 4, kill (pri 2) |
| – | | player rule 1: kill players (6) |

## Defaults (from the creator code, to be checked in game)

- LTS: the creator adds one player rule "kill any player" (`SET_UP_PLAYER_RULE`,
  `ciSELECTION_PLAYER_RULE_KILL_ANY` → `FMMC_OBJECTIVE_LOGIC_KILL_PLAYERS`) for every team: the
  last team standing wins. Extra rules come from placed entities.
- Capture: the capture objects / vehicles carry the rule (capture, get and deliver) per team;
  delivery zones (`dpos`, `dpost`) belong to that rule.

## Plan (mockup first)

- A "Rules" page: per team the rule list in order; every rule shows its objective text, the
  entities and player rules wired to it (with type), extra objectives, and links to the per-rule
  settings (time, wanted, gang chase, outfits, bounds …). Add / move / remove rules.
- On every entity page a "Rule" card: per team rule number (dropdown of the team's rules with
  their text) + type (dropdown filtered by entity type), plus the extra rules card.
- Per-rule pages (gang chase, SMS, kill, …) take the rule dropdown with the rule's text instead of
  a bare index.
- Moving / deleting a rule has to renumber every pointer (entities, player rules, extra objectives,
  per-rule arrays). That is the risky part; do it after the display works.
