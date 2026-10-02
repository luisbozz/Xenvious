using System;
using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    // Part of MainWindow: Mission / Kill page (raw player rule values, read and written in the game).
    public partial class MainWindow
    {
        private bool _killLoading;

        private void tbkillrule_TextChanged(object sender, TextChangedEventArgs e) => SetKillField(GTA.Offsets.Editor.Kill.rule, tbkillrule);

        private void tbkillpri_TextChanged(object sender, TextChangedEventArgs e) => SetKillField(GTA.Offsets.Editor.Kill.pri, tbkillpri);

        private void tbkilllim_TextChanged(object sender, TextChangedEventArgs e) => SetKillField(GTA.Offsets.Editor.Kill.lim, tbkilllim);

        private void tbkillprbs_TextChanged(object sender, TextChangedEventArgs e) => SetKillField(GTA.Offsets.Editor.Kill.prbs, tbkillprbs);

        // mcf / mcp are per rule, not per team.
        private void tbkillmcf_TextChanged(object sender, TextChangedEventArgs e) => SetKillField(GTA.Offsets.Editor.Kill.mcf, tbkillmcf, false);

        private void tbkillmcp_TextChanged(object sender, TextChangedEventArgs e) => SetKillField(GTA.Offsets.Editor.Kill.mcp, tbkillmcp, false);

        private void tbkillno_TextChanged(object sender, TextChangedEventArgs e)
        {
            int team = ddkillteamno.SelectedIndex;
            if (!_killLoading && m.IsProcOpen && team >= 0 && team <= 3 && GTA.Offsets.Editor.Kill.number != 0 && IsValidInt(tbkillno.Text))
                new Global(GTA.Offsets.Editor.Kill.number + team).SetInt(Convert.ToInt32(tbkillno.Text));
        }

        private long KillField(long field, bool perTeam = true) => field + (perTeam ? ddkillteamno.SelectedIndex : 0) + ddkillno.SelectedIndex * GTA.Offsets.Editor.Kill.NEXT;

        private bool KillPicked => m.IsProcOpen && GTA.Offsets.Editor.Kill.NEXT != 0 && ddkillno != null && ddkillteamno != null
            && ddkillno.SelectedIndex >= 0 && ddkillteamno.SelectedIndex >= 0 && ddkillteamno.SelectedIndex <= 3;

        private void SetKillField(long field, TextBox box, bool perTeam = true)
        {
            // Only typing writes; filling the boxes from the game must not write back.
            if (_killLoading || field == 0 || !KillPicked || !box.IsFocused || !IsValidInt(box.Text))
                return;
            new Global(KillField(field, perTeam)).SetInt(Convert.ToInt32(box.Text));
        }

        private void ddkillteamno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetKillValues();
            SelectActiveTextBox();
        }

        private void ddkillno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetKillValues();
            SelectActiveTextBox();
        }

        public void GetKillValues(bool ignore_focus = true)
        {
            if (!m.IsProcOpen)
                return;
            if (ddkillno == null || ddkillteamno == null)
                return;

            bool enabled = KillPicked;
            foreach (var box in new[] { tbkillno, tbkillrule, tbkillpri, tbkilllim, tbkilljtop, tbkilljtof, tbkillprbs, tbkillmcf, tbkillmcp })
                box.IsEnabled = enabled;
            if (!enabled)
                return;

            _killLoading = true;
            Show(tbkillno, new Global(GTA.Offsets.Editor.Kill.number + ddkillteamno.SelectedIndex).Get<int>());
            Show(tbkillrule, new Global(KillField(GTA.Offsets.Editor.Kill.rule)).Get<int>());
            Show(tbkillpri, new Global(KillField(GTA.Offsets.Editor.Kill.pri)).Get<int>());
            Show(tbkilllim, new Global(KillField(GTA.Offsets.Editor.Kill.lim)).Get<int>());
            Show(tbkilljtop, new Global(KillField(GTA.Offsets.Editor.Kill.jtop)).Get<int>());
            Show(tbkilljtof, new Global(KillField(GTA.Offsets.Editor.Kill.jtof)).Get<int>());
            Show(tbkillprbs, new Global(KillField(GTA.Offsets.Editor.Kill.prbs)).Get<int>());
            Show(tbkillmcf, new Global(KillField(GTA.Offsets.Editor.Kill.mcf, false)).Get<int>());
            Show(tbkillmcp, new Global(KillField(GTA.Offsets.Editor.Kill.mcp, false)).Get<int>());
            _killLoading = false;

            void Show(TextBox box, int value)
            {
                if (!box.IsFocused || ignore_focus)
                    box.Text = value.ToString();
            }
        }
    }
}
