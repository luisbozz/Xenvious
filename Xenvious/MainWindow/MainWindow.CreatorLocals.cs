using System;
using System.ComponentModel;
using System.Linq;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Where the running creator keeps its locals.
    public partial class MainWindow
    {
        public bool IsTargetRace()
        {
            if (new Global(GTA.Offsets.Editor.type).Get<int>() == 2 && (
                new Global(GTA.Offsets.Editor.racetype).Get<int>() == 18 ||
                new Global(GTA.Offsets.Editor.racetype).Get<int>() == 19))
            {
                return true;
            }
            return false;
        }

        public static long getCurrentCreatorBase()
        {
            if (GTA.Offsets.Editor.localptr == null)
            {
                GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy();
            }

            if (GTA.Offsets.Editor.localptr != null)
            {
                return m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, getCurrentCreatorPresetOffset() * 8 }).GetAddress();
            }
            return 0;
        }

        public static long getCreatorScriptLocalWorkerBase()
        {
            if (GTA.Offsets.Editor.localptr == null)
            {
                GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy();
            }

            if (GTA.Offsets.Editor.localptr != null)
            {
                return m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, getCurrentCreatorWorkerOffset() * 8 }).GetAddress();
            }

            return 0;
        }

        public static long getCreatorScriptLocalHeadingBase()
        {
            if (GTA.Offsets.Editor.localptr == null)
            {
                GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy();
            }

            if (GTA.Offsets.Editor.localptr != null)
            {
                return m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, getCurrentCreatorHeadingOffset() * 8 }).GetAddress();
            }

            return 0;
        }

        public static long getCreatorScriptLocalRefreshState()
        {
            if (GTA.Offsets.Editor.localptr == null)
            {
                GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy();
            }

            if (GTA.Offsets.Editor.localptr != null)
            {
                return m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, getCurrentCreatorRefreshState() * 8 }).GetAddress();
            }
            return 0;
        }

        public static long getCurrentCreatorRefreshState()
        {
            if (GTA.Offsets.Editor.localptr != null)
            {
                switch (GTA.ReadScriptName(GTA.Offsets.Editor.localptr[0], GTA.Offsets.Editor.localptr[1]))
                {
                    case "fm_survival_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_refresh_survival;
                    case "fm_capture_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_refresh_capture;
                    case "fm_lts_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_refresh_lts;
                    case "fm_deathmatch_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_refresh_dm;
                    case "fm_race_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_refresh_race;
                    case "fm_mission_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_refresh_lts;
                    default:
                        return 0;
                }
            }

            return 0;
        }

        public static long getCurrentCreatorWorkerOffset()
        {
            if (GTA.Offsets.Editor.localptr != null)
            {
                switch (GTA.ReadScriptName(GTA.Offsets.Editor.localptr[0], GTA.Offsets.Editor.localptr[1]))
                {
                    case "fm_survival_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_worker_survival;
                    case "fm_capture_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_worker_capture;
                    case "fm_lts_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_worker_lts;
                    case "fm_deathmatch_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_worker_dm;
                    case "fm_race_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_worker_race;
                    case "fm_mission_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_worker_lts;
                    default:
                        return 0;
                }
            }

            return 0;
        }

        public static long getCurrentCreatorHeadingOffset()
        {
            if (GTA.Offsets.Editor.localptr != null)
            {
                switch (GTA.ReadScriptName(GTA.Offsets.Editor.localptr[0], GTA.Offsets.Editor.localptr[1]))
                {
                    case "fm_survival_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_cam_heading_survival;
                    case "fm_capture_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_cam_heading_capture;
                    case "fm_lts_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_cam_heading_lts;
                    case "fm_deathmatch_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_cam_heading_dm;
                    case "fm_race_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_cam_heading_race;
                    case "fm_mission_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_cam_heading_lts;
                    default:
                        return 0;
                }
            }

            return 0;
        }

        public static long getCurrentCreatorPresetOffset()
        {
            if (GTA.Offsets.Editor.localptr != null)
            {
                switch (GTA.ReadScriptName(GTA.Offsets.Editor.localptr[0], GTA.Offsets.Editor.localptr[1]))
                {
                    case "fm_survival_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_pre_survival;
                    case "fm_capture_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_pre_capture;
                    case "fm_lts_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_pre_lts;
                    case "fm_deathmatch_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_pre_dm;
                    case "fm_race_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_pre_race;
                    case "fm_mission_creator":
                        return GTA.Offsets.Editor.OFFSET_current_creator_pre_lts;
                    default:
                        return 0;
                }
            }

            return 0;
        }
    


        public bool IsLTS()
        {
            if (new Global(GTA.Offsets.Editor.type).Get<int>() == 0 &&
                new Global(GTA.Offsets.Editor.subtype).Get<int>() == 5)
            {
                return true;
            }
            return false;
        }

        public bool IsCapture()
        {
            if (new Global(GTA.Offsets.Editor.type).Get<int>() == 0 &&
                new Global(GTA.Offsets.Editor.subtype).Get<int>() == 6)
            {
                return true;
            }
            return false;
        }

        public bool IsLandRace()
        {
            if (new Global(GTA.Offsets.Editor.type).Get<int>() == 2 && (new Global(GTA.Offsets.Editor.racetype).Get<int>() == 0 || new Global(GTA.Offsets.Editor.racetype).Get<int>() == 1))
            {
                return true;
            }
            return false;
        }
        public bool IsWaterRace()
        {
            if (new Global(GTA.Offsets.Editor.type).Get<int>() == 2 && (
                new Global(GTA.Offsets.Editor.racetype).Get<int>() == 2 ||
                new Global(GTA.Offsets.Editor.racetype).Get<int>() == 3))
            {
                return true;
            }
            return false;
        }
        public bool IsAirRace()
        {
            if (new Global(GTA.Offsets.Editor.type).Get<int>() == 2 && (
                new Global(GTA.Offsets.Editor.racetype).Get<int>() == 4 ||
                new Global(GTA.Offsets.Editor.racetype).Get<int>() == 5))
            {
                return true;
            }
            return false;
        }
        public bool IsStuntRace()
        {
            if (new Global(GTA.Offsets.Editor.type).Get<int>() == 2 &&
                new Global(GTA.Offsets.Editor.subtype).Get<int>() == 0 && (
                new Global(GTA.Offsets.Editor.racetype).Get<int>() == 6 ||
                new Global(GTA.Offsets.Editor.racetype).Get<int>() == 7))
            {
                return true;
            }
            return false;
        }
        public bool IsParachuteRace()
        {
            if (new Global(GTA.Offsets.Editor.type).Get<int>() == 2 &&
                new Global(GTA.Offsets.Editor.subtype).Get<int>() == 0 && (
                new Global(GTA.Offsets.Editor.racetype).Get<int>() == 8 ||
                new Global(GTA.Offsets.Editor.racetype).Get<int>() == 9))
            {
                return true;
            }
            return false;
        }
        public bool IsSpecialVehRace()
        {
            if (new Global(GTA.Offsets.Editor.type).Get<int>() == 2 &&
                new Global(GTA.Offsets.Editor.subtype).Get<int>() == 21 && (
                new Global(GTA.Offsets.Editor.racetype).Get<int>() == 6 ||
                new Global(GTA.Offsets.Editor.racetype).Get<int>() == 7))
            {
                return true;
            }
            return false;
        }
        public bool IsTransformRace()
        {
            // ((new Global(GTA.Offsets.Editor.racetype).Get<int>() == 6 || new Global(GTA.Offsets.Editor.racetype).Get<int>() == 7) ||
            if (new Global(GTA.Offsets.Editor.type).Get<int>() == 2 && new Global(GTA.Offsets.Editor.subtype).Get<int>() == 20 &&
                (new Global(GTA.Offsets.Editor.racetype).Get<int>() == 6 || new Global(GTA.Offsets.Editor.racetype).Get<int>() == 7))
            {
                return true;
            }
            return false;
        }
        public bool IsOpenWheelRace()
        {
            if (new Global(GTA.Offsets.Editor.type).Get<int>() == 2 && (
                new Global(GTA.Offsets.Editor.racetype).Get<int>() == 24 ||
                new Global(GTA.Offsets.Editor.racetype).Get<int>() == 25))
            {
                return true;
            }
            return false;
        }
        public bool IsArenaRace()
        {
            if (new Global(GTA.Offsets.Editor.type).Get<int>() == 2 &&
                new Global(GTA.Offsets.Editor.adverm).Get<int>() == 999 &&
                Functions.Read.checkbinary(13, GTA.Offsets.Editor.intop2))
            {
                return true;
            }
            return false;
        }
        public bool IsPursuitRace()
        {
            if (new Global(GTA.Offsets.Editor.type).Get<int>() == 2 &&
                new Global(GTA.Offsets.Editor.subtype).Get<int>() == 24 &&
                new Global(GTA.Offsets.Editor.racetype).Get<int>() == 26)
            {
                return true;
            }
            return false;
        }
        public bool IsStreetRace()
        {
            if (new Global(GTA.Offsets.Editor.type).Get<int>() == 2 &&
                new Global(GTA.Offsets.Editor.subtype).Get<int>() == 24 &&
                new Global(GTA.Offsets.Editor.racetype).Get<int>() == 27)
            {
                return true;
            }
            return false;
        }

        public bool curcreatorscanneeded()
        {
            return GTA.Offsets.Editor.localptr == null
                || !GTA.CreatorScripts.Contains(GTA.ReadScriptName(GTA.Offsets.Editor.localptr[0], GTA.Offsets.Editor.localptr[1]));
        }

        public void creatorRefresh()
        {
            if (cbsettingsoldcreatorrefresh.IsChecked ?? true)
            {
                // Race/LTS/capture: the rebuild that keeps menu and camera. Deathmatch and
                // survival (other state numbering) keep the plain state 7.
                if (CreatorMap.CanRebuild(CreatorMap.CurrentCreator()))
                    _ = CreatorMap.RebuildAsync();
                else
                    m.memory((getCreatorScriptLocalWorkerBase() + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_refresh * 8).ToString("X")).SetInt(7);
            }
            else
                m.memory((getCreatorScriptLocalRefreshState()).ToString("X")).SetInt(0);
        }
}
}
