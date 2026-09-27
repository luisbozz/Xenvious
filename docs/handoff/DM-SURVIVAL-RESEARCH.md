# Deathmatch and Survival: what is known (2026-09-27)

Research notes for the last todo ("fill DM and Survival with functions"). Line numbers and
globals are from the Legacy dumps (calamity-inc, branch senpai). New offsets must come from
ysc-global-updater for both editions; nothing here is in `offsets.ini` yet unless marked.

## Deathmatch

`fm_deathmatch_controler` copies the job's lobby defaults into its own struct
(`func_1452`, `Global_1689077.f_n = Global_4718592.f_m`):

| Lobby field | Job global | Meaning (from the controller / lobby label) |
|---|---|---|
| f_1 | f_3803 == 1 | team deathmatch flag |
| f_2 | f_3772 | ? |
| f_3 | f_3804 | ? |
| f_5 | f_3793 | duration (LOB_DURATION_) |
| f_6 | f_3796 | target score (LOB_TARG_SCR_) |
| f_8 | f_121960 (`traf`, in offsets.ini) | traffic: 0 off, 1-5 = 20/30/50/70/100 % (func_13330); 1 also mutes traffic ambience |
| f_9 | f_126278 (`pol`, in offsets.ini) | police: 0 normal, 1 off (func_5874) |
| f_10..f_16 | f_3807, f_3797, f_3799, f_3801, f_3800, f_3791, f_3770 | ? (lobby options, to map) |
| f_17 | f_133393 (`tod`, in offsets.ini) | time of day (LOB_TIME_DAY_) |
| f_18 | f_133350 | weapons list (FMMC_MS_W0_) |
| f_21, f_23, f_24 | f_3805, f_3790, f_3798 | ? |

Done: police and traffic on the dashboard (DashDMSection). The rest needs the creator menu
functions mapped (most DM_* labels are DLC text that is not in public label dumps).

## Survival

- Two controllers: `fm_horde_controler` (old survivals) and `fm_survival_controller` (new).
  Both only read the time of day from the job's ambient values.
- UFO props (`imp_prop_ship_01a`, `gr_prop_damship_01a`, `p_spinning_anus_s`): prpbs2 bit 14
  spins the ship, bit 15 shines a beam now and then (fm_survival_controller 325400).
  Done: switches on the props page (PropUfoOptions).
- Alien mode is already on the Survival page (cbsurvalienmode).
- Wave settings (squads, vehicles, air) live in `Global_4718592.f_198736` (SC_WVB_*): to map.
