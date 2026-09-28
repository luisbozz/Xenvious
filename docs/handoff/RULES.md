# Rules: how a mission's objectives are wired

Research for the Rules page. Nothing here is built yet, and nothing here was tested in game. It
was read from the scripts and checked against six published Rockstar jobs.

Sources:

- Rockstar's source (`D:\decompiled scripts gta 5`): `MP_globals_FM.sch`, `FMMC_Cloud_loader.sch`,
  `FMMC_Menus*.sch`, `FM_Mission_Controller*`. This is the old creator and old controllers; the
  new creator is not in it.
- Decompiled 1.73-3889 Legacy scripts: `public_mission_creator.c`, `fmmc_launcher.c`
  (ysc-global-updater `scripts/1.73-3889`), and `public_mission_controller.c`,
  `fm_mission_controller.c`, `fm_mission_controller_2020.c` (calamity-inc, same build).
- Jobs (German JSON, `prod.cloud.rockstargames.com/ugc/gta5mission/...`):
  - "Leg dich nicht mit Dre an" `7208/_gYr4YkTe0mxBOOZsDT24A`: old format, 1 team, 17 rules.
  - Creator Content I `2598/AcWB5wwf20W9WRngL1cykw`: new format, 8 rules.
  - Creator Content II `4827/B_wycZclg06ILVsX6mdkSA`: 14 rules, extra objectives on go-tos.
  - Creator Content III `9005/Mj9aGvJ8Skq_rFlngoV2iA`: 10 rules.
  - Creator Content IV `7383/5HNIUeF7M0-Zgq9twuxUTQ`: 2 teams, 11 and 10 rules.
  - Creator Content V `7906/5txELJtdtkiU_gCL_0zmpA`: 15 rules. Uses jumps, next-objective
    override and objective limits.
  - The LTS example `4983/CYaUJTEy9UKDwwhVbRcONw` (3 rules), used before.

## Two families: old creator and public creator

The same data (`Global_4718592` / `Global_4980736`, i.e. `g_FMMC_STRUCT` / `g_FMMC_STRUCT_ENTITIES`)
is read by different controllers, and the jump values mean different things.

| | Old content (Rockstar missions, old creator) | Public Mission Creator (since 2026-01-22) |
|---|---|---|
| `gen.subtype` | e.g. 2 (Dre job) | **14** |
| Runs in | `fm_mission_controller` (or `_2020`, `_v3`) | `public_mission_controller` |
| JSON keys | long: `jtop0`, `jtof0`, `kill.pri0/rul0/lim0` | compact: `pjtp0`, `vjtp0`, `ojtp0`, `ljtp0`, `kill.prpri0/prrul0/prlim0` |
| Jump value "none" | `0` | `-1` |
| Jump to rule 1 | `-1` (`ciJumpToObjective_MissionStart`) | `0` |
| Jump to rule N+1 | `N` | `N` |
| Backward jumps (entity jtp/jtf) | yes (`RETURNING TO AN OLDER RULE`) | **no**: ignored when the target ≤ current rule |

- `fmmc_launcher` picks the controller (`func_7030` in 1.73): mission type 0 and subtype 14 give
  `public_mission_controller`. `joaat` of it is `-1694114956`.
- The compact save (`func_5928` in the creator) writes a key only if one value differs from the
  default. It writes all entities of that type once it writes it. On load, a missing key gets the
  default (`-1` for jumps, `99999` for priority, `0` for rule type…). The public creator still
  reads the old long keys too.
- The mapping of JSON key to global is in `scratchpad/rules/pkeys.py` (public creator) and
  `keymap.py` (old loader) during this session. The tables below have the results.

## Model

Rule numbers in this file are 1-based like the creator menu ("rule 1"); JSON and memory use the
index, which is the number − 1.

- Every team has a list of rules. The count is `endcon.nrl[team]` (`Global_4718592.f_3838[team].f_60`,
  offset `nrl`). Rule 0 is the first objective. The public creator allows **17 rules**
  (0–16; `FMMC_MAX_RULES`); CC5 uses 15.
- A rule is not an object of its own. It exists because something points at it with
  **priority = rule index** and a **rule type**:
  - peds, vehicles, objects, go-to locations: per team a priority and a type
    (`FMMC_OBJECTIVE_LOGIC_*`),
  - player rules (`kill` section, up to 17): kill players, cutscene, holding rule, points…,
  - extra objectives: more (type, priority) pairs for one entity (see below).
- Several entities on one rule: the rule is done when the target is reached (all of them, or
  `tsc` of them, see "Objective limits").
- Rules without any entity are player rules: a scripted cutscene (16) at rule 0 in every
  tutorial job, and a "holding rule" (36) with a short time limit as the last rule.
- Per rule settings live in `endcon` arrays indexed by rule: text, limits, next-rule override,
  bitsets, wanted, gang chase, bounds…

### The creator's own rule list (public creator, in memory)

The public creator keeps a rule list per team that its Rules menu shows:

- `Global_1837536.f_6[team /*104*/].f_103`: number of rules (same as `nrl`).
- `Global_1837536.f_6[team /*104*/][rule /*6*/]`: the rule's **creator selection** (type as
  chosen in the menu, `ciSELECTION_*`, table below). `.f_2` onwards is a bitset of the entities
  on that rule (`func_692`).
- It is **not saved**. `func_7258` rebuilds it when a job loads, from the entity priorities and
  types and from the extra objectives. If Xenvious changes priorities or types in memory, this list
  is stale until the job is reloaded (or Xenvious rewrites it). To check in game.
- Adding a rule in the creator (`func_1369`) writes the selection and presets per type: objective
  score, take-over time, time limit, etc.

## Where the wiring is stored

Per team `t` (0–3). Memory offsets are for 1.73-3889 Legacy; `offsets.ini` names in brackets where
they exist already. Enhanced has the same fields at other numbers; it needs ysc-global-updater.

| Entity | Priority | Rule type | Jump on pass | Jump on fail |
|---|---|---|---|---|
| Ped `ene` | `pri<t>` `f_93162[i /*1277*/].f_41` (`actor_pri`) | `rule<t>` `.f_36` (`actor_rule`) | `pjtp<t>` / `jtop<t>` `.f_686` | `pjtf<t>` / `jtof<t>` `.f_712` |
| Vehicle `veh` | `pri<t>` `f_71084[i /*631*/].f_19` (`veh_pri`) | `rule<t>` `.f_14` (`veh_rule`) | `vjtp<t>` `.f_242` | `vjtf<t>` `.f_268` |
| Object `obj` | `pri<t>` `f_7128[i /*668*/].f_22` (`obj_pri`) | `rule<t>` `.f_17` (`obj_rule`) | `ojtp<t>` `.f_222` | `ojtf<t>` `.f_248` |
| Go-to `goto` | `pri<t>` `f_5[i /*339*/].f_128` (`goto_pri`) | `rl<t>` `.f_123` (`goto_rule`) | `ljtp<t>` `.f_143` | none in the public creator |
| Player rule `kill` | `prpri<t>` `Global_4718592.f_114365[i /*56*/].f_5` (`kill_pri`) | `prrul<t>` `[i][t]` (`kill_rule`) | `.f_15` (`kill_jtop`) | `.f_41` (`kill_jtof`) |

- `Global_4980736.*` is the entity data, `Global_4718592.*` the mission data.
- Priority `99999` = not part of any rule for that team.
- Player rules: count per team `Global_4718592.f_115318[t]` (`kill_number`), limit `prlim<t>`
  `.f_10` (`kill_lim`), bitset `prbs<t>` `.f_46` (`kill_prbs`). The public creator **does not save**
  player rule jumps; the fields exist, the menu shows them.
- Per-entity jump extras (1.73, new): a 4-int prerequisite bitset per team (ped `.f_691[t /*5*/]`,
  vehicle `.f_247`, object `.f_227`, go-to `.f_148`, player rule `.f_20`) and an "aggro blocks the
  jump" bit (menu `FMMC_JTO_PP` and `FMMC_JTO_AB`). See "Jumps".
- `objt` (+ `team`, `spawn`) on peds / vehicles / props: the entity spawns with rule `objt` of
  team `team`. Cleanup fields (`clnrend`, `objcr`, `prpcr`, `vclnrl`…) remove it at a rule. These
  are rule references too and must move when rules move.

### Rule types

JSON and memory store the logic (`FMMC_OBJECTIVE_LOGIC_*`):

| # | Logic | # | Logic | # | Logic |
|---|---|---|---|---|---|
| 0 | none | 13 | minigame | 26 | phone |
| 1 | get and deliver | 14 | get masks | 27–30 | go to team 0–3 |
| 2 | kill | 15 | crowd control | 31 | damage |
| 3 | protect | 16 | scripted cutscene | 32 | leave entity |
| 4 | go to | 17 | charm | 33 | loot threshold |
| 5 | capture | 18–22 | arrest | 34 | points threshold |
| 6 | kill players | 23 | destroy | 35 | ped go to location |
| 7–10 | kill team 0–3 | 24 | leave location | 36 | holding rule |
| 11 | get and hold | 25 | phone | | |
| 12 | photo | | | | |

The creator menu and the extra objectives use the **selection** instead. The public creator
converts with `func_1378(logic, entityType)` (entity type 0 ped, 1 vehicle, 3 object, 16 go-to,
-1 player):

| Selection | Meaning | Selection | Meaning |
|---|---|---|---|
| 1–5 | ped: deliver, kill, protect, go to, capture | 30 | object minigame |
| 6–10 | vehicle: deliver, kill, protect, go to, capture | 31 / 32 / 33 / 34 | masks / crowd / charm / cutscene |
| 11–15 | object: deliver, kill, protect, go to, capture | 35–39 | arrest |
| 16 / 17 | go-to: go to / capture | 40 / 41 | destroy / leave location |
| 18 / 19–22 | kill players / kill team 0–3 | 42–45 | go to team 0–3 |
| 23 / 24 / 25 | get and hold ped / vehicle / object | 46 / 50 / 51 | damage ped / vehicle / object |
| 26–29 | photo ped / vehicle / object / go-to | 47 / 48 / 49 | leave ped / vehicle / object |
| 53 / 54 / 55 / 56 | loot / points / ped go to location / holding | 0 | none |

## Extra objectives

Details are in [EXTRA-OBJECTIVES.md](EXTRA-OBJECTIVES.md). Summary:

- One entity takes part in more rules. It has up to 13 extra (type, priority) pairs per team;
  up to 30 entities have them.
- JSON in `gen`: `eoid[30]` entity index, `eoet[30]` entity type (1 ped, 2 vehicle, 3 object,
  4 go-to, -1 free), `eoir` dict `eor<e>_<n>_<t>` = **selection** (table above), `eoep` dict
  `eop<e>_<n>_<t>` = priority. The old format writes all slots (-1 when empty), the new one only the
  used ones.
- Memory (offsets exist: `eoid`, `eoet`, `eoir`, `eoep`): `Global_4718592.f_127220[30]`,
  `.f_127189[30]`, `.f_127251[e /*66*/][n /*5*/][t]`, `.f_129232[e /*66*/][n /*5*/][t]`.
- The tutorials use them wherever one entity carries several rules in a row:
  - CC2: go-tos 2–4 are the server room for rules 3–5.
  - CC4: the getaway vehicle is "get in" three times, then "destroy".
  - CC5: Burrito captured (rule 3), then destroyed (rule 4). Lost MC members from wave 1 stay
    kill targets in waves 2 and 3.
- The Dre job has 22 entities with extra objectives (mercenaries kept as targets over rules 4–6,
  go-to 4 over rules 8–11, object 1 over rules 16–17).

## Jumps (`jtp` / `jtf`): "Missionszielsprünge"

An entity (or player rule) can send its team to another rule when **its** objective is passed
(`jtp`) or failed (`jtf`), instead of the next rule.

`public_mission_controller` (`func_125`, `func_147`), for content from the public creator:

1. On pass, only if the prerequisite bitset for that team is satisfied (`func_150`: every set bit
   must be set on the player) and not "aggro blocks it" with aggro triggered (`func_149`).
2. The value is taken as the rule index. `-1` = no jump; a target ≤ the current rule is
   **ignored** (`func_147`).
3. `func_146` stores it as the team's next objective (the larger one wins if several fire). The
   progression then invalidates all rules up to target−1 and starts the target.
4. On fail (the entity died, the vehicle was destroyed…), `jtf` works the same way without the
   conditions. A special case: when the objective ended with reason 53 and the rule has an
   override entity (`f_3838[t].f_1413[rule]`), that entity's fail jump is used.
5. Pass and fail also set "next mission" and end cutscene values, which do not matter for us.

`fm_mission_controller` / `_2020` (old content): `0` = none, `-1` = rule 0, `N` = rule `N`, and
jumping back works (it re-invalidates from that rule).

Creator menu (both): per team "Jump to objective on pass / fail", shown as rule `value+1`, `0`
shown as off. Public creator also has "prerequisites" and "aggro blocks" rows. When a rule is
deleted or moved, the public creator renumbers the jumps like priorities (`func_1400`):

- a jump to a later rule moves down by one,
- a jump to the deleted rule becomes `-1`,
- a swap of two rules swaps the values.

Example CC5, rule 9 "protect the Aztecas contact" (ped 14): `pjtp0 = 11` (he survives → rule 12,
kill waves), `pjtf0 = 9` (he dies → rule 10, get in his Caracara). The Caracara vehicles (rules
10/11) have `vjtp0 = 14` (→ rule 15, the end). Go-tos 2/3 on rule 2 (choose a chapter) have
`ljtp0 = 4` (→ rule 5, destroy the vehicles); go-tos 0/1 go on to rule 3 (hack the Burrito).

## Override next objective (`nxtrulb`): "Nächstes Missionsziel überschreiben"

Per team and rule, `endcon.nxtrulb<t>[rule]` (`Global_4718592.f_3838[t /*26988*/].f_19063[rule]`)
is a **bitset of rules**. When the rule completes, `func_551`/`func_552` in
`public_mission_controller`:

- `0`: normal order.
- One bit set: go to that rule.
- Several bits: pick one of them at random (seeded per team when the rule bitset `f_8576` bit 15
  is set, or by the mission if `Global_4718592.f_38` bit 10). This is the "random next rule" of
  the old source (`rnrbs`, `iRandomNextRuleBS`).
- Not applied when the rule has bit 28 in `f_8630` and the objective ended with reason 18.
- The controller can go back as well as forward. The **creator** only accepts later rules: its
  check `func_1327` marks the override invalid when the highest target ≤ the rule or the rule is
  ≥ 15.

Then the conditional jump (`cojr<t>` `f_19081[rule]` = target rule, `cojc<t>` `f_19099[rule]`
= condition) can replace it. It compares team 0 and team 1 scores; with condition 0, it jumps
on a draw.

Example CC5: rule 4 "destroy the Burrito" has `nxtrulb0[3] = 64` (bit 6), so after it the team
skips rules 5–6 (the other branch) and continues with rule 7.

## Objective limits: "Missionsziellimits"

What makes a rule end besides "all entities done". All per team and rule in `endcon`:

| JSON | Memory `f_3838[t]` | Old name | Meaning |
|---|---|---|---|
| `tsc<t>` | `.f_6221[r]` | `iTargetScore` | entities (points) needed, e.g. "destroy 6 of 10" (CC5 rule 4), "go to 1 of 4" (CC5 rule 1) |
| `tms<t>` | `.f_6203[r]` | `iObjectiveScore` | points one objective gives |
| `ttime<t>` | `.f_6275[r]` | `iTakeoverTime` | capture / control time in ms (CC5 rule 2: 60000) |
| `tmt<t>` | `.f_1083[r]` (`tmt`) | `iObjectiveTimeLimitRule` | time limit, as a selection index (table below) |
| `mrtl<t>` | | `iTimeLimit` | multi-rule timer |
| `fail` | `.f_14688` | `iTeamFailBitset` | bit per rule: the mission fails when that rule fails (time runs out) |
| `PPRBS<t>_<n>` | `.f_1327[r /*5*/][n]` | | prerequisites that complete the rule |
| `prlim<t>` | `f_114365[i].f_10` | `iPlayerRuleLimit` | kills / points a player rule needs |

- The public creator checks (`public_mission_creator.c` line 430816) for rules with no target
  score, no time limit and no prerequisite; such a rule cannot finish on its own.
- When the time runs out and the rule's `fail` bit is set, the mission fails. With the bit clear
  the rule ends and the team moves on (old controller; to check in the public one).
- `tmt` index to seconds (1.73 `public_mission_controller`):

  ```text
  0=off 20=1 21=2 22=3 23=4 24=5 25=6 26=7 27=8 28=9 1=10 29=11 30=12 31=13 32=14 33=15 34=16
  35=17 36=18 37=19 2=20 63=25 3=30 64=35 4=40 65=45 5=50 66=55 6=60 67=65 47=70 68=75 48=80
  69=85 38=90 70=95 49=100 71=105 50=110 72=115 7=120 51=130 52=140 39=150 53=160 54=170 8=180
  55=190 56=200 40=210 57=220 58=230 9=240 59=250 60=260 41=270 61=280 62=290 10=300 42=330
  11=360 43=390 12=420 44=450 13=480 45=510 14=540 46=570 15=600 16=900 17=1200 18=1500 19=1800
  ```

Four of the five tutorials end with a holding rule (36) with `tmt` 21–33 (2–15 s).

## Worked examples

### Creator Content V "Multiple Choice" (1 team, 15 rules)

| Rule | Objective text (short) | Wired by | Flow |
|---|---|---|---|
| 1 | – | player rule: cutscene | |
| 2 | Choose a Lost MC chapter | go-to 0–3, go to, `tsc` 1 | go-to 2/3 jump → 5 |
| 3 | Approach the Burrito to hack it | vehicle 0 capture, 60 s | |
| 4 | Destroy the Burrito | extra objective: vehicle 0 kill | override next → 7 |
| 5 | Destroy the Lost MC vehicles | vehicles 1–10 kill, `tsc` 6 | |
| 6 | Leave the area | go-to 4 leave location | |
| 7 | Meet the Aztecas contact | go-to 5 | |
| 8 | – | player rule: cutscene | |
| 9 | Protect the Aztecas contact | ped 14 protect | pass → 12, fail → 10 |
| 10 / 11 | Get in the Caracara 4x4 | vehicle 11 / 12 deliver | pass → 15 |
| 12–14 | Kill the Lost MC members | peds 18–39 kill + extra objectives, `tsc` 4 / 6 / – | |
| 15 | – | player rule: holding, 4 s | |

(Rule numbers here are 1-based like the creator menu; JSON / memory index = number − 1.)

### "Leg dich nicht mit Dre an" (old format, 1 team, 17 rules)

1 go to, 2 cutscene (player rule), 3–6 kill mercenaries (`tsc` 6 / 8 / 2, with extra
objectives), 7 go to airport, 8–11 find Johnny Guns in the hangar (go-tos, `tsc` 1, ped go-to
location), 12 damage Johnny Guns, 13 minigame, 14 cutscene, 15 deliver (vehicles 4 and 10),
16–17 protect object 1 with `tmt` 22 (3 s). No jumps, no override.

### Creator Content I–IV

- I "Secure & Deliver": cutscene, go to, kill 6, cutscene, kill 1, collect bag, steal Banshee,
  holding.
- II "Obtain & Impair": go to 1 of 5 (`tsc` 1), server room over three rules via extra
  objectives, holding rule "steal the tablet", leave, deliver tablet, damage the Juggernaut.
- III "Stealth Extraction": free the hacker, kill, two "wait for the hacker" rules (the second
  has `tmt` 25 = 6 s), silent kills, get in the Buzzard.
- IV "Thick as Thieves": 2 teams with separate rule lists that wait for each other (holding rules
  "wait until the money is stolen"), the getaway vehicle shared through extra objectives.

## What this means for Xenvious

1. **Show**: per team, read `nrl`, the rule texts (`txt0`, existing offsets), and every entity /
   player rule / extra objective with priority < 99999. Group them by rule. Everything but the
   jumps, `nxtrulb`, `tsc`/`tms`/`ttime` and the entity prerequisite fields has offsets already.
2. **New offsets needed** (both editions, from ysc-global-updater, not by hand): ped `f_686`/`f_712`,
   vehicle `f_242`/`f_268`, object `f_222`/`f_248`, go-to `f_143`, `nxtrulb` `f_19063`, `cojr`/`cojc`,
   `tsc` `f_6221`, `tms` `f_6203`, `ttime` `f_6275`, `fail` `f_14688`, and the creator's rule list
   `Global_1837536.f_6` (count `.f_103`).
3. **Jump values depend on the creator.** Public creator: -1 = none, only later rules. Old
   creators: 0 = none. Xenvious must write the right encoding for the creator it is attached to.
4. **Moving or deleting a rule** renumbers every reference like `func_1400`:
   - entity and player rule priorities, and extra objective priorities,
   - jumps, `nxtrulb` bits, `cojr`,
   - spawn / cleanup rules (`objt`, cleanup fields),
   - the per-rule `endcon` arrays and the texts.
   The public creator does this in `func_1435` and its helpers (`func_1400`). Whether Xenvious
   can call it instead of copying it is to look into.
5. The public creator's rule list (`Global_1837536`) has to match, or its menus show old data.

## Open questions (check in game)

- Does the public creator pick up priority / type changes made by Xenvious without a reload? What
  happens to `Global_1837536`?
- The prerequisite and "aggro blocks" fields: which menu rows exactly, what the four ints hold.
- `mrtl` / multi-rule timer and `fail` in the public creator menus.
- Enhanced: all numbers above are Legacy 1.73-3889.

## Notes from earlier

- The creator draws some per-rule things only for the rule selected in its rules menu
  (`sSelDetails.iRow`): the play area bounds (sBoundsStruct / sBoundsStruct2) through the
  "bounds" visibility group (bit 5) and the mission drop-offs (visibility group 6). A rules page
  could switch that row to show the rule being edited.
