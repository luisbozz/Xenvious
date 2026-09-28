# Play area 1 and 2 (research for the redesign)

Sources: `LEGACY_BOUNDS_STRUCT` (MP_globals_FM.sch), `GET_FMMC_AREA_BOUNDS_POS`
(FM_Mission_Controller_Bounds.sch), `LEGACY_GET_BOUNDS_POS` (FMMC_header.sch), the LTS creator's
bounds menu (FM_LTS_Creator.sc). Both play areas are the same struct, per team and rule:
`sBoundsStruct` (f_3155, page "Play area") and `sBoundsStruct2` (f_3768, page "Play area 2").

## Fields (index in the struct, Xenvious key)

| f | Field | Key (PA / PA2) | Meaning |
|---|---|---|---|
| 0 | MNIgnoreWantedVeh | – / out2iv | no wanted level in this vehicle model |
| 1 | iIgnoreWantedOutfit | – / out2io | no wanted level in this outfit |
| 2 | iWantedToGive | – / out2wg | wanted level for leaving |
| 3 | iBoundsBS | outbs / out2bs | flags, see below |
| 4 | iMinimapAlpha | outmm / out2mm | minimap alpha of the area (default 120) |
| 5 | iPercentageLikelihoodToSpawnInFrontOfPlyr | – / out2fp | respawn ahead of the bound player (entity = team) |
| 6 | iLinkedSpawnGroup | – / out2sg | spawn group |
| 7 | iBoundsBlipHeightThreshold | – / out2bh | blip height threshold |
| 8 | iMaxPlayersAllowedInBounds | – | not in Xenvious yet |
| 9 | iVehicleRegenAndCap | – | not in Xenvious yet |
| 10 | MNIgnoreLeaveAreaVeh | outilv / out2ilv | leaving is fine in this vehicle model |
| 11 | mnOnlyAffectVeh | outonfv / out2onfv | only players in this vehicle model count |
| 12 | hudColouring | outhc / out2hc | colour |
| 13-15 | vPos | outb / bd2v | sphere centre |
| 16-21 | vPos1, vPos2 | pastart, paend / bd2pa | angled box points |
| 22 | fRadius | outr / bd2r | sphere radius |
| 23 | fWidth | outw, pawidth / bd2pawidth | angled box width |
| 24, 25 | fMaxHeight, fMinHeight | – | only used while placing (see FEEDBACK) |
| 29 | iType | outbt / bd2t | 0 sphere, 1 angled box |
| 30 | iEntityType | outety / out2et | bind: ciRULE_TYPE_* |
| 31 | iEntityID | outeid / out2id | bind: index of the entity, -1 none |

Rule level, outside the struct: `bfm` (leave text), `pribt` (idle blip timer).

## iBoundsBS (ciBounds_*)

0-3 only one member of team 0/1/2/3 needs to be inside (combinable), 4 delayed spawn area
position, 5 look towards entity, 6 show bounds in game, 7 only affect active overtime player,
8 ignore Z checks for spheres, 9 keep entity spawn in bounds, 10 GPS for the blip,
11 out-of-bounds objective text, 12 out-of-bounds shard, 13 don't block the objective when out.

## Binding to an entity (outety / outeid)

`GET_FMMC_AREA_BOUNDS_POS`: when iEntityType != 0 the area's position is taken from the entity
every frame, so the area moves with it. Only `vPos` is replaced, so this works for the SPHERE
(for an angled box both points would become the same position).

The creator writes the type from ciSPAWN_NEAR_ENTITY_TYPE_ (fm_Mission_Creator_Menu_Right.sch,
ciLEGACY_BOUNDS_ENTITY_TYPE / _ENTITY_ID): 0 none, 1 actor, 2 vehicle, 3 object, 4 go-to
location, 5 team, 6 last player, 7 train, 8 lobby leader. The id runs from -2 to the number of
placed entities of that type (peds, vehicles, objects, go-tos of team 0, or 4 teams); the
controller reads the same numbers as ciRULE_TYPE_ (1-5 line up). Next to it the menu has "Look
towards entity" (iBoundsBS bit 5, effect not checked yet) and the spawn-ahead
percentage (f_5).

Special ids: team with id -2 = the leading runner (GET_FMMC_WINNING_RUNNER_LOCATION); vehicle
with id -2 = the last vehicle used. With team binding, f_5 > 0 makes players respawn ahead of the
nearest team member instead of at the centre.

## What area 1 and 2 are for (text for the page)

Both are per team and rule and work at the same time. Each has its own mode, kept outside the
struct in sFMMCEndConditions: iPlayAreaBitset (stay inside) / iLeaveAreaBitset (stay outside) /
iSpawnAreaBitset + iSpawnWhileInArea (respawn in it), timer iPlayAreaTimer (60000), wanted
iWantedLevelBounds; area 2: iPlayArea2Bitset / iLeaveArea2Bitset / iSecSpawnAreaBitset +
iSecSpawnWhileInArea, iPlayArea2Timer, iWantedLevelBounds2 (FMMC_Cloud_loader: dfaBoundsArea,
dfaBounds2Area, dfaSecSpawnArea ...). None of these are in offsets.ini yet (ysc-global-updater).
Area 1 = the main area of the rule; area 2 = a second one on top: a no-go zone inside area 1,
a second region, or only a respawn place (hence the old name "Respawn area").

## Redesign idea (mockup first)

One page for both areas (tabs "Play area 1 / 2"), team + rule on top, shape tiles like Advanced
Prop Placement (Sphere / Box), only the fields of the chosen shape, the AreaEditor preview, a
"Follow" card with entity type tiles and an entity picker (names from the job instead of numbers),
flags as named toggles instead of the bits number.

## Other bindings (type + id) in the job data

Same pattern elsewhere (MP_globals_FM.sch): zones (iEntityLinkType/Index: the zone follows the
entity; iSpawnFacingEntityType/Index), warp portals (iAttachToEntityType/ID), placed peds,
vehicles and objects (iSpawnNearEntityType/ID), rule airstrikes (RULE_AIRSTRIKE_STRUCT),
forced hint cam per rule and dialogue trigger hint cam, dummy blips, ambush, IPL continuity,
extra objectives. A shared "entity picker" (type tiles + list of the job's entities by name)
would serve all of them.
