using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xenvious.Helper_Classes;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Misc / MenuSwitcher page.
    public partial class MainWindow
    {
        private void Btnmschange_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                if (String.IsNullOrEmpty(tbmsprev.Text))
                {
                    displayScreenMessage("Please input a value for the return menu.");
                    return;
                }
                if (String.IsNullOrEmpty(tbmscurr.Text))
                {
                    displayScreenMessage("Please input a value for the menu.");
                    return;
                }

                bool needscan = curcreatorscanneeded();
                if (needscan)
                    GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy();

                long addy = getCurrentCreatorBase();

                if (IsValidInt(tbmsprev.Text, false)) m.memory((addy + GTA.Offsets.Editor.OFFSET_current_creator_pre_previous_menu * 8).ToString("X")).SetInt(tbmsprev.Text);
                if (IsValidInt(tbmscurr.Text, false)) m.memory((addy + GTA.Offsets.Editor.OFFSET_current_creator_pre_current_menu * 8).ToString("X")).SetInt(tbmscurr.Text);
            }
        }

        private void Btnmsread_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                bool needscan = curcreatorscanneeded();
                if (needscan)
                    GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy();

                long addy = getCurrentCreatorBase();

                tbmsprev.Text = m.memory((addy + GTA.Offsets.Editor.OFFSET_current_creator_pre_previous_menu * 8).ToString("X")).Get<int>().ToString();
                tbmscurr.Text = m.memory((addy + GTA.Offsets.Editor.OFFSET_current_creator_pre_current_menu * 8).ToString("X")).Get<int>().ToString();

            }
        }

        private void BtnmsSavePreset_Click(object sender, RoutedEventArgs e)
        {
            var name = tbmsPresetName.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(name))
            {
                displayScreenMessage("Please specify a preset name.");
                return;
            }

            if (!int.TryParse(tbmscurr.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var menuId))
            {
                displayScreenMessage("Please input a valid value for the menu.");
                return;
            }

            if (!int.TryParse(tbmsprev.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var returnMenuId))
            {
                displayScreenMessage("Please input a valid value for the return menu.");
                return;
            }

            if (cbmsPresets.SelectedItem is not MenuSwitcherPreset selectedPreset)
            {
                displayScreenMessage("Select a preset to save or use Add Preset for new entries.");
                return;
            }

            var duplicate = MenuSwitcherPresets.FirstOrDefault(p => !ReferenceEquals(p, selectedPreset) && string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            if (duplicate != null)
            {
                displayScreenMessage("Another preset already uses this name.");
                return;
            }

            selectedPreset.Name = name;
            selectedPreset.MenuId = menuId;
            selectedPreset.ReturnMenuId = returnMenuId;

            cbmsPresets.SelectedItem = selectedPreset;

            SaveMenuPresets();
            displayScreenMessage("Menu preset saved.");
        }

        private void BtnmsDeletePreset_Click(object sender, RoutedEventArgs e)
        {
            if (cbmsPresets.SelectedItem is MenuSwitcherPreset preset && MenuSwitcherPresets.Contains(preset))
            {
                MenuSwitcherPresets.Remove(preset);
                SaveMenuPresets();

                if (MenuSwitcherPresets.Count > 0)
                {
                    cbmsPresets.SelectedIndex = 0;
                }
                else
                {
                    cbmsPresets.SelectedIndex = -1;
                    tbmsPresetName.Clear();
                }

                displayScreenMessage("Menu preset deleted.");
            }
            else
            {
                displayScreenMessage("Please select a preset to delete.");
            }
        }

        private void cbmsPresets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbmsPresets.SelectedItem is MenuSwitcherPreset preset)
            {
                tbmsPresetName.Text = preset.Name;
                tbmscurr.Text = preset.MenuId.ToString(CultureInfo.InvariantCulture);
                tbmsprev.Text = preset.ReturnMenuId.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                tbmsPresetName.Clear();
                tbmscurr.Clear();
                tbmsprev.Clear();
            }
        }

        private void BtnmsAddPreset_Click(object sender, RoutedEventArgs e)
        {
            var name = tbmsPresetName.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(name))
            {
                displayScreenMessage("Please specify a preset name.");
                return;
            }

            if (!int.TryParse(tbmscurr.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var menuId))
            {
                displayScreenMessage("Please input a valid value for the menu.");
                return;
            }

            if (!int.TryParse(tbmsprev.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var returnMenuId))
            {
                displayScreenMessage("Please input a valid value for the return menu.");
                return;
            }

            if (MenuSwitcherPresets.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                displayScreenMessage("A preset with this name already exists. Use Save to overwrite it.");
                return;
            }

            var newPreset = new MenuSwitcherPreset(name, menuId, returnMenuId);
            MenuSwitcherPresets.Add(newPreset);
            cbmsPresets.SelectedItem = newPreset;

            SaveMenuPresets();
            displayScreenMessage("Menu preset added.");
        }
    }
}
