using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Mission / Gang page.
    public partial class MainWindow
    {
        private void cbmissionganghfm_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(11, GTA.Offsets.Editor.irbs8 + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT, cbmissionganghfm);
        }

        private void ddmissiongangno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            getGangValues(true);
            SelectActiveTextBox();
        }

        private void tbmissiongangv1locx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbv1 + 0 + ddmissiongangno.SelectedIndex * 3 + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbmissiongangv1locx.Text);
        }

        private void tbmissiongangv1locy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbv1 + 1 + ddmissiongangno.SelectedIndex * 3 + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbmissiongangv1locy.Text);
        }

        private void tbmissiongangv1locz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbv1 + 2 + ddmissiongangno.SelectedIndex * 3 + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbmissiongangv1locz.Text);
        }

        private void tbmissiongangv2locx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbv2 + 0 + ddmissiongangno.SelectedIndex * 3 + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbmissiongangv2locx.Text);
        }

        private void tbmissiongangv2locy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbv2 + 1 + ddmissiongangno.SelectedIndex * 3 + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbmissiongangv2locy.Text);
        }

        private void tbmissiongangv2locz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbv2 + 2 + ddmissiongangno.SelectedIndex * 3 + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbmissiongangv2locz.Text);
        }

        private void Btmissiongangv1getloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.getlocation();

            tbmissiongangv1locx.Text = loc[0].ToString();
            tbmissiongangv1locy.Text = loc[1].ToString();
            tbmissiongangv1locz.Text = loc[2].ToString();
            gangArea.StartPicked();
        }

        private void Btmissiongangv2getloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.getlocation();

            tbmissiongangv2locx.Text = loc[0].ToString();
            tbmissiongangv2locy.Text = loc[1].ToString();
            tbmissiongangv2locz.Text = loc[2].ToString();
            gangArea.EndPicked();
        }

        public void getGangValues(bool ignore_focus = false)
        {
            if (ddmissiongangno == null || ddmissiongangteamno == null)
                return;

            int index = ddmissiongangno.SelectedIndex;
            int tindex = ddmissiongangteamno.SelectedIndex;
            bool enable = index > -1 && tindex > -1 ? true : false;
            if (m.IsProcOpen)
            {
                Btmissiongangv1getloc.IsEnabled = enable;
                Btmissiongangv2getloc.IsEnabled = enable;
                tbmissiongangv1locx.IsEnabled = enable;
                tbmissiongangv1locy.IsEnabled = enable;
                tbmissiongangv1locz.IsEnabled = enable;
                tbmissiongangv2locx.IsEnabled = enable;
                tbmissiongangv2locy.IsEnabled = enable;
                tbmissiongangv2locz.IsEnabled = enable;
                ddmissiongangteamno.IsEnabled = enable;
                tbmissiongbnum.IsEnabled = enable;
                tbmissiongbmax.IsEnabled = enable;
                tbmissiongbdel.IsEnabled = enable;
                tbmissiongbaw.IsEnabled = enable;
                tbmissiongbfnr.IsEnabled = enable;
                tbmissiongacc.IsEnabled = enable;
                ddmissiongbcol.IsEnabled = enable;
                cbmissionganghfm.IsEnabled = enable;
                cbmissiongangnewrule.IsEnabled = enable;
                ddmissiongangtype.IsEnabled = enable;
                cbmissiongangtyperw.IsEnabled = enable;
                ddmissiongbat.IsEnabled = enable;

                if (enable)
                {
                    int gangtype = new Global(GTA.Offsets.Editor.gbtp + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<int>();
                    ShowGangType(gangtype);

                    ddmissiongbat.SelectedIndex = new Global(GTA.Offsets.Editor.gbat + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<int>();

                    if (!tbmissiongangv1locx.IsFocused || ignore_focus) tbmissiongangv1locx.Text = new Global(GTA.Offsets.Editor.gbv1 + 0 + index * 3 + tindex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                    if (!tbmissiongangv1locy.IsFocused || ignore_focus) tbmissiongangv1locy.Text = new Global(GTA.Offsets.Editor.gbv1 + 1 + index * 3 + tindex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                    if (!tbmissiongangv1locz.IsFocused || ignore_focus) tbmissiongangv1locz.Text = new Global(GTA.Offsets.Editor.gbv1 + 2 + index * 3 + tindex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                    if (!tbmissiongangv2locx.IsFocused || ignore_focus) tbmissiongangv2locx.Text = new Global(GTA.Offsets.Editor.gbv2 + 0 + index * 3 + tindex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                    if (!tbmissiongangv2locy.IsFocused || ignore_focus) tbmissiongangv2locy.Text = new Global(GTA.Offsets.Editor.gbv2 + 1 + index * 3 + tindex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                    if (!tbmissiongangv2locz.IsFocused || ignore_focus) tbmissiongangv2locz.Text = new Global(GTA.Offsets.Editor.gbv2 + 2 + index * 3 + tindex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                    if (!tbmissiongbnum.IsFocused || ignore_focus) tbmissiongbnum.Text = new Global(GTA.Offsets.Editor.gbnum + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissiongbmax.IsFocused || ignore_focus) tbmissiongbmax.Text = new Global(GTA.Offsets.Editor.gbmax + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissiongbdel.IsFocused || ignore_focus) tbmissiongbdel.Text = new Global(GTA.Offsets.Editor.gbdel + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!tbmissiongbaw.IsFocused || ignore_focus) tbmissiongbaw.Text = new Global(GTA.Offsets.Editor.gbaw + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                    if (!tbmissiongbfnr.IsFocused || ignore_focus) tbmissiongbfnr.Text = new Global(GTA.Offsets.Editor.gbfnr + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                    if (!tbmissiongacc.IsFocused || ignore_focus) tbmissiongacc.Text = new Global(GTA.Offsets.Editor.gacc + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<int>().ToString();
                    if (!ddmissiongbcol.IsDropDownOpen || ignore_focus) ddmissiongbcol.SelectedIndex = new Global(GTA.Offsets.Editor.gbcol + index + tindex * GTA.Offsets.Editor.team_NEXT).Get<int>() + 1;
                    Functions.Read.checkbinary(11, GTA.Offsets.Editor.irbs8 + index + tindex * GTA.Offsets.Editor.team_NEXT, cbmissionganghfm);
                    Functions.Read.checkbinary(17, GTA.Offsets.Editor.irbs3 + index + tindex * GTA.Offsets.Editor.team_NEXT, cbmissiongangnewrule);
                    Functions.Read.checkbinary(11, GTA.Offsets.Editor.irbs3 + index + tindex * GTA.Offsets.Editor.team_NEXT, cbmissiongangtyperw);
                }
            }
        }

        private void tbmissiongbnum_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiongbnum.Text))
                new Global(GTA.Offsets.Editor.gbnum + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiongbnum.Text);
        }

        private void tbmissiongbmax_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiongbmax.Text))
                new Global(GTA.Offsets.Editor.gbmax + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiongbmax.Text);
        }

        private void tbmissiongbdel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiongbdel.Text))
                new Global(GTA.Offsets.Editor.gbdel + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiongbdel.Text);
        }

        private void tbmissiongbaw_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbaw + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbmissiongbaw.Text);
        }

        private void ddmissiongbcol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbcol + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(ddmissiongbcol.SelectedIndex - 1);
        }

        private void ddmissiongangteamno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            getGangValues(true);
            SelectActiveTextBox();
        }

        private void tbmissiongbfnr_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.gbfnr + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbmissiongbfnr.Text);
        }

        private void ddmissiongangtype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_gangTypeSync || _gangSearch == null || _gangSearch.Syncing || !(ddmissiongangtype.SelectedItem is SearchItem item))
                return;
            if (m.IsProcOpen && ddmissiongangno.SelectedIndex > -1 && ddmissiongangteamno.SelectedIndex > -1)
                new Global(GTA.Offsets.Editor.gbtp + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(item.Id);
            ShowGangType(item.Id);
        }

        // ----- Gang type picker: all 53 types by group, with what each one sends -----

        private bool _gangTypeSync;
        private string _gangTypeLang;

        private SearchableCombo _gangSearch;

        private static string GangTypeText(GangTypes.GangType t, Func<string, string, string> tr)
        {
            string faction = tr("gt_f_" + t.Faction, t.Faction);
            return t.Vehicle == null ? $"{t.Id}  {faction}" : $"{t.Id}  {faction} · {t.Vehicle}";
        }

        // Grouped list with a search box, the same as the zone types. Built again only when the
        // language changed.
        private void FillGangTypes()
        {
            string lang = TranslateOr("gt_f_none", "");
            if (_gangSearch != null && lang == _gangTypeLang)
                return;
            _gangTypeLang = lang;
            if (_gangSearch == null)
                _gangSearch = new SearchableCombo(ddmissiongangtype);
            _gangTypeSync = true;
            _gangSearch.SetItems(GangTypes.All.Select(t => new SearchItem { Id = t.Id, Text = GangTypeText(t, TranslateOr), Group = t.Group }),
                GangTypes.Groups, g => g == "none" ? "" : TranslateOr("gt_grp_" + g, g));
            _gangTypeSync = false;
        }

        private void ddmissiongangtype_DropDownOpened(object sender, EventArgs e) => FillGangTypes();

        private void ShowGangType(int id)
        {
            FillGangTypes();
            var type = GangTypes.Find(id);
            _gangSearch.Select(id);

            ShowGangDetails(id, type);
            if (type == null)
                lblgangtypeinfo.Text = string.Format(TranslateOr("gt_unknown", "Type {0} is not a gang chase type."), id);
            else if (type.Group == "none")
                lblgangtypeinfo.Text = TranslateOr("gt_none_info", "No gang chase on this rule.");
            else if (type.Group == "custom")
                lblgangtypeinfo.Text = TranslateOr("gt_custom_info", "Uses a gang chase unit the job sets up itself (vehicle, peds and weapon come from there).");
            else
                lblgangtypeinfo.Text = string.Format(TranslateOr("gt_info", "{0} with {1} peds ({2}), weapon: {3}."), type.Vehicle, type.Peds, type.Ped, type.Weapon);
        }

        // Name, the game's label, weapon, random weapons, vehicle, ped model and count, like the
        // first versions showed them. The label and the random weapon list come from Gangtypes.json.
        private void ShowGangDetails(int id, GangTypes.GangType type)
        {
            GangDetails.Children.Clear();
            GangDetails.RowDefinitions.Clear();
            GangDetails.ColumnDefinitions.Clear();
            if (type == null || type.Vehicle == null)
                return;
            var json = GTA.Editor.GangTypes.FirstOrDefault(g => g.Value == id);
            var rows = new (string Key, string Fallback, string Value)[]
            {
                ("gt_d_name", "Name", TranslateOr("gt_f_" + type.Faction, type.Faction)),
                ("gt_d_label", "Game name", json?.InfoName),
                ("gt_d_weapon", "Weapon", type.Weapon),
                ("gt_d_random", "Random weapons", json?.RandonizedWeapons?.Replace("weapon_", "").Replace(",", ", ")),
                ("gt_d_vehicle", "Vehicle", type.Vehicle),
                ("gt_d_ped", "Enemy", type.Ped),
                ("gt_d_count", "Enemies per vehicle", type.Peds.ToString(System.Globalization.CultureInfo.CurrentCulture)),
            };
            GangDetails.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            GangDetails.ColumnDefinitions.Add(new ColumnDefinition());
            int r = 0;
            foreach (var row in rows.Where(x => !string.IsNullOrEmpty(x.Value)))
            {
                GangDetails.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var key = new TextBlock { Text = TranslateOr(row.Key, row.Fallback), FontSize = 12.5, Margin = new Thickness(0, 2, 8, 2) };
                key.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                var value = new TextBox { Text = row.Value, Style = null, IsReadOnly = true, BorderThickness = new Thickness(0), Background = Brushes.Transparent, FontSize = 12.5, Margin = new Thickness(-2, 2, 0, 2), TextWrapping = TextWrapping.Wrap };
                value.SetResourceReference(TextBox.ForegroundProperty, "TextColor");
                Grid.SetRow(key, r); Grid.SetRow(value, r); Grid.SetColumn(value, 1);
                GangDetails.Children.Add(key);
                GangDetails.Children.Add(value);
                r++;
            }
        }

        private void cbmissiongangnewrule_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(17, GTA.Offsets.Editor.irbs3 + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT, cbmissiongangnewrule);
        }

        private void cbmissiongangtyperw_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(11, GTA.Offsets.Editor.irbs3 + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT, cbmissiongangtyperw);
        }

        private void ddmissiongbat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddmissiongbat.SelectedIndex > -1)
                new Global(GTA.Offsets.Editor.gbat + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(ddmissiongbat.SelectedIndex);
        }

        private void tbmissiongacc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbmissiongacc.Text))
                new Global(GTA.Offsets.Editor.gacc + ddmissiongangno.SelectedIndex + ddmissiongangteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(tbmissiongacc.Text);
        }
    }
}
