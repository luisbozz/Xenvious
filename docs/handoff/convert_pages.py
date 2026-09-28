"""Converts the remaining settings pages with auto_page.py. Run once; the pages are then done.
python3 docs/handoff/convert_pages.py [--write]"""
import sys
sys.path.insert(0, 'docs/handoff')
import auto_page as ap

PAGES = [
    ('PageInnerMissionGeneral', None),
    ('PageInnerMissionMenubs', None),
    ('PageInnerMissionPlayerSettings', dict(combos=['ddplyrno'])),
    ('PageInnerMissionTeamSettings', dict(combos=['ddmissionteamno'])),
    ('PageInnerMissionPA', dict(combos=['ddoutbno'])),
    ('PageInnerMissionRA', dict(combos=['ddbd2no'])),
    ('PageInnerMissionTeleportMarkers', dict(combos=['ddmissiontpteamno', 'ddmissiontpno'])),
    ('PageInnerMissionKill', dict(combos=['ddkillteamno', 'ddkillno'])),
    ('PageInnerMissionGang', dict(combos=['ddmissiongangteamno', 'ddmissiongangno'])),
    ('PageInnerMissionotzone', dict(combos=['ddmissionotzoneno'])),
    ('PageInnerMissionBlips', dict(combos=['ddmissionddblipno'], add='BtnmissionddblipAdd', delete='BtnmissionddblipDelete', count=('blips_placed', 'blips placed'))),
    ('PageInnerMissionSMS', dict(combos=['ddsmsno'])),
    ('PageInnerMissionInventory', None),
    ('PageInnerMissionGoto', dict(combos=['ddgotono'], add='BtngotAdd', delete='BtngotoDelete', count=('goto_placed', 'goto points'))),
    ('PageInnerCaptureGeneral', dict(combos=['ddcaptureteamno', 'ddcaptureno'])),
    ('PageInnerCaptureObjects', dict(combos=['ddobjno'], add='BtnobjAdd', delete='BtnobjDelete', count=('capobj_placed', 'capture objects placed'))),
    ('PageInnerCaptureDelivery', dict(combos=['dddzteamno', 'dddzno'])),
    ('PageInnerRaceGeneral', None),
    ('PageInnerRaceCP', dict(combos=['ddcpno'], count=('cps_placed', 'checkpoints'))),
    ('PageInnerRaceAVEH', None),
    ('PageInnerSurvivalGeneral', None),
]

if __name__ == '__main__':
    only = [a for a in sys.argv[1:] if not a.startswith('--')]
    for tab, bar in PAGES:
        if only and tab not in only:
            continue
        # MyScrollViewer only fed the width of its own WrapPanel, which the cards replace.
        out = ap.run(tab, bar=bar, allow_missing=['MyScrollViewer'] if tab == 'PageInnerRaceAVEH' else ())
        ap.summary(out, tab)
