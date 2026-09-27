namespace Xenvious
{
    /// <summary>
    /// The creator's "visibility groups" (sFMMCmenu.iVisGroupBitSet, ciBS_visGroup_*): which
    /// placed things the creator draws all the time - zones, ped ranges, go-to ranges, delivery
    /// areas and so on.
    ///
    /// Why Xenvious sets them: the creator's init only sets every bit when it is not coming back
    /// from a test (bFromTestMiss), and the retail scripts always pass TRUE there, so in LTS,
    /// Capture and the others all groups stay off and e.g. placed zones are never drawn. The
    /// wanted bits are kept in config.ini and written again whenever the creator has other bits
    /// (a new creator session starts with 0).
    /// </summary>
    public static class VisibilityGroups
    {
        /// <summary>Bit order of ciBS_visGroup_* (FMMC_Vars.sch), with the fallback labels.</summary>
        public static readonly (string Key, string Name)[] Groups =
        {
            ("vis_locate", "Go-to ranges"),
            ("vis_pedrange", "Actor ranges"),
            ("vis_pedgoto", "Actor go-to points"),
            ("vis_zones", "Zones"),
            ("vis_dialogue", "Dialogue triggers"),
            ("vis_bounds", "Bounds zones"),
            ("vis_delivery", "Delivery areas (not Capture)"),
        };

        /// <summary>
        /// Two more switches that are not creator groups but Xenvious' injected custom functions
        /// (customfuncs fn3 / fn4 in LTS, Capture and Race): the play area of every team and rule,
        /// and the gang chase areas. They only draw while the experimental script features are on,
        /// because the injected code is a dev patch. Kept in the same config value, bits 7 and 8.
        /// </summary>
        public static readonly (string Key, string Name)[] Extra =
        {
            ("vis_playarea", "Play areas (custom)"),
            ("vis_gangchase", "Gang chase areas (custom)"),
        };

        public const int PlayAreaBit = 7;
        public const int GangChaseBit = 8;

        private const int Mask = (1 << 7) - 1;
        private const int AllBits = (1 << 9) - 1;
        private static int? _wanted;

        public static int Wanted
        {
            get
            {
                if (_wanted == null)
                    _wanted = new ini_reader(Functions.getRoamingConfigFilePath()).ReadInteger("Settings", "showingame", AllBits) & AllBits;
                return _wanted.Value;
            }
        }

        public static bool IsOn(int bit) => (Wanted & (1 << bit)) != 0;

        public static void Set(int bit, bool on)
        {
            _wanted = on ? Wanted | (1 << bit) : Wanted & ~(1 << bit);
            new ini_reader(Functions.getRoamingConfigFilePath()).Write("Settings", "showingame", _wanted.Value);
            Apply();
            if (bit >= PlayAreaBit)
                MainWindow.Instance?.RefreshScriptFeatureBits();
        }

        /// <summary>Called by the dashboard every second while a creator runs.</summary>
        public static void Apply()
        {
            if (!MainWindow.m.IsProcOpen || GTA.Offsets.Editor.OFFSET_current_creator_pre_visgroups == 0)
                return;
            if (MainWindow.Instance.curcreatorscanneeded())
                GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy();
            if (MainWindow.getCurrentCreatorPresetOffset() == 0)
                return;
            long address = MainWindow.getCurrentCreatorBase();
            if (address == 0)
                return;
            string at = (address + GTA.Offsets.Editor.OFFSET_current_creator_pre_visgroups * 8).ToString("X");
            int current = MainWindow.m.memory(at).Get<int>();
            // Only the seven groups; bits above them belong to the game.
            int value = (current & ~Mask) | (Wanted & Mask);
            if (value != current)
                MainWindow.m.memory(at).SetInt(value);
        }
    }
}
