using System;

namespace Xenvious
{
    /// <summary>
    /// Precise placement ("Advanced Options" → Override Position/Rotation) for creator
    /// templates, which Rockstar only offers for single props.
    ///
    /// Two parts:
    ///  * the "precise templates" scrpatches (trigger "templates") re-add the menu entry and
    ///    let the template follow the override. They are only kept in the script while the
    ///    template category is selected -- left in permanently, switching from another
    ///    category to templates leaves the old prop on screen;
    ///  * while Override Position is open, the creator's "displayed prop" is pointed at the
    ///    template's first object, so the menu shows and moves real coordinates instead of 0,
    ///    and the alignment is set to world (-1). In Override Rotation / Advanced Options the
    ///    displayed prop is cleared again.
    ///
    /// Tick() runs every 50 ms on the scrpatch thread, so applying and reverting patches never
    /// races with the patch pass.
    /// </summary>
    public static class PreciseTemplates
    {
        /// <summary>The template category is selected in a creator that supports this.</summary>
        public static bool Active { get; private set; }

        private static string _lastReason = "";

        // The template object Xenvious made the creator's displayed prop, 0 when none.
        //
        // While it is set, one entity has two owners: the template's object list and the
        // displayed prop. The creator deletes the displayed prop on several paths (picking
        // up a placed prop, the prop limit check that runs every frame in the template
        // category, ...), and placing the template moves its objects into the placed-prop
        // array. Deleting the displayed prop after that deletes a placed prop and leaves a
        // dead handle in the array, which crashes GET_ENTITY_MODEL on a later frame. So the
        // alias only lives while Override Position is open on an unplaced template.
        private static int _alias;

        public static void Tick()
        {
            string reason;
            bool active;
            try
            {
                active = Evaluate(out reason);
            }
            catch (Exception ex)
            {
                active = false;
                reason = "error: " + ex.Message;
            }
            if (active != Active || reason != _lastReason)
            {
                Logging.Log.Info($"precise templates {(active ? "on" : "off")}: {reason}", source: "templates");
                _lastReason = reason;
            }
            Active = active;
        }

        private static bool Evaluate(out string reason)
        {
            // Without a creator thread the alias is gone with it; it only comes back below
            // once the same creator is found again.
            int alias = _alias;
            _alias = 0;

            reason = "no game";
            if (!MainWindow.m.IsProcOpen)
                return false;
            reason = "not supported by this game version (no offsets)";
            if (GTA.Offsets.Editor.OFFSET_precise_template_category == 0)
                return false;
            reason = "no creator running";
            if (!MainWindow.Instance.IsCreatorRunning())
                return false;

            // The rest of Xenvious finds the creator thread on first use; so does this.
            string creator = GTA.CurrentCreatorName();
            if (GTA.Offsets.Editor.localptr == null || string.IsNullOrEmpty(creator))
            {
                GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy();
                creator = GTA.CurrentCreatorName();
            }
            reason = "creator thread not found";
            if (GTA.Offsets.Editor.localptr == null || string.IsNullOrEmpty(creator))
                return false;

            long pre = MainWindow.getCurrentCreatorPresetOffset();
            long placement = PlacementOffset(creator);
            reason = $"no offsets for {creator}";
            if (pre == 0 || placement == 0)
                return false;
            _alias = alias;

            long category = ReadLocal(pre + GTA.Offsets.Editor.OFFSET_current_creator_pre_category_num);
            bool templates = category == GTA.Offsets.Editor.OFFSET_precise_template_category;
            long menu = templates ? ReadLocal(pre + GTA.Offsets.Editor.OFFSET_current_creator_pre_current_menu) : -1;
            bool positionMenu = menu == GTA.Offsets.Editor.OFFSET_precise_menu_position;
            long display = placement + GTA.Offsets.Editor.OFFSET_current_creator_placement_display;
            long firstObject = placement + GTA.Offsets.Editor.OFFSET_current_creator_placement_template_objects + 1;

            ReleaseAlias(display, firstObject, positionMenu);

            reason = $"{creator}: category {category}";
            if (!templates)
                return false;

            if (positionMenu)
            {
                // World alignment, and the template's first object as the prop the menu edits.
                WriteLocal(pre + GTA.Offsets.Editor.OFFSET_current_creator_pre_alignment, -1);
                if (ReadLocal(display) == 0)
                {
                    int first = (int)ReadLocal(firstObject);
                    if (first != 0)
                    {
                        WriteLocal(display, first);
                        _alias = first;
                    }
                }
            }
            else if (menu == GTA.Offsets.Editor.OFFSET_precise_menu_rotation || menu == GTA.Offsets.Editor.OFFSET_precise_menu_advanced)
            {
                // Also catches an alias this session did not set (an earlier Xenvious run).
                long shown = ReadLocal(display);
                if (shown != 0 && shown == ReadLocal(firstObject))
                    WriteLocal(display, 0);
            }
            return true;
        }

        private static void ReleaseAlias(long display, long firstObject, bool positionMenu)
        {
            if (_alias == 0)
                return;
            long shown = ReadLocal(display);
            if (shown != _alias)
            {
                // The creator replaced or deleted the displayed prop itself. If it deleted it,
                // the template object went with it; the creator rebuilds missing template
                // objects on its own.
                if (shown == 0)
                    Logging.Log.Warn($"precise templates: the creator deleted the displayed template object {_alias}", source: "templates");
                _alias = 0;
                return;
            }
            if (!positionMenu || ReadLocal(firstObject) != _alias)
            {
                WriteLocal(display, 0);
                _alias = 0;
            }
        }

        private static long PlacementOffset(string creator)
        {
            switch (creator)
            {
                case "fm_race_creator": return GTA.Offsets.Editor.OFFSET_current_creator_placement_race;
                case "fm_lts_creator": return GTA.Offsets.Editor.OFFSET_current_creator_placement_lts;
                case "fm_capture_creator": return GTA.Offsets.Editor.OFFSET_current_creator_placement_capture;
                case "fm_deathmatch_creator": return GTA.Offsets.Editor.OFFSET_current_creator_placement_dm;
                case "fm_survival_creator": return GTA.Offsets.Editor.OFFSET_current_creator_placement_survival;
                default: return 0;
            }
        }

        private static long LocalAddress(long index)
        {
            var e = GTA.Offsets.Editor.localptr;
            return MainWindow.m.memory(e[0], new long[] { e[1], GTA.Offsets.Editor.OFFSET_script_local_start, index * 8 }).GetAddress();
        }

        private static long ReadLocal(long index) => MainWindow.m.memory(LocalAddress(index).ToString("X")).Get<int>();

        private static void WriteLocal(long index, int value) => MainWindow.m.memory(LocalAddress(index).ToString("X")).SetInt(value);
    }
}
