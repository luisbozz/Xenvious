using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xenvious.Helper_Classes;
using Xenvious.JSON;
using Xenvious.Logging;
using Xenvious.Translation;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Settings page, config.ini and language.
    public partial class MainWindow
    {
        private void cbsettingshidegtamessage_Checked(object sender, RoutedEventArgs e)
        {
            new ini_reader(Functions.getRoamingConfigFilePath()).Write("Settings", "hidegtamessage", cbsettingshidegtamessage.IsChecked ?? false);

            if ((cbsettingshidegtamessage.IsChecked ?? false) && SGTAMessage.Visibility == Visibility.Visible)
            {
                SGTAMessage.Visibility = Visibility.Collapsed;
            }
        }

        private void ddcolor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (ddcolor.SelectedIndex)
            {
                case 0:
                    new ini_reader(Functions.getRoamingConfigFilePath()).Write("Settings", "color", "gray");
                    break;
                case 1:
                    new ini_reader(Functions.getRoamingConfigFilePath()).Write("Settings", "color", "white");
                    break;
                default:
                    new ini_reader(Functions.getRoamingConfigFilePath()).Write("Settings", "color", "gray");
                    break;
            }

            switch (ddcolor.SelectedIndex)
            {
                case 0:
                    this.Resources["SelectedBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#232529"));
                    this.Resources["HoverBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2C30"));
                    this.Resources["DisabledForegroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888"));
                    this.Resources["ButtonHoverBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34373C"));
                    this.Resources["ButtonClickBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#37393F"));
                    this.Resources["BackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202225"));
                    this.Resources["SeactionHeaderBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#36393F"));
                    this.Resources["CheckBoxBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#36393F"));
                    this.Resources["TextBoxBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303339"));
                    this.Resources["TextBoxBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#131517"));
                    this.Resources["TextBoxBorderInner"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#232529"));
                    this.Resources["SectionBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2F3136"));
                    this.Resources["ComboBoxBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#303339"));
                    this.Resources["ComboBoxBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#131517"));
                    this.Resources["ComboBoxBorderInner"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#232529"));
                    this.Resources["ComboBoxSelected"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#232529"));
                    this.Resources["ComboBoxHighlighted"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2C30"));
                    this.Resources["ComboBoxArrow"] = new SolidColorBrush(Colors.White);
                    this.Resources["TextColor"] = new SolidColorBrush(Colors.White);
                    this.Resources["TextBoxForegroundThemeBrush"] = new SolidColorBrush(Colors.White);
                    this.Resources["ShadowColor"] = Colors.Black;
                    this.XenviousImage.Source = (BitmapImage)FindResource("ogimage256");

                    break;
                case 1:
                    this.Resources["SelectedBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCDDDE"));
                    this.Resources["HoverBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F6F6F7"));
                    this.Resources["DisabledForegroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888"));
                    this.Resources["ButtonHoverBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4D7DC"));
                    this.Resources["ButtonClickBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBDBDB"));
                    this.Resources["BackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3E5E8"));
                    this.Resources["SeactionHeaderBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EBEDEF"));
                    this.Resources["CheckBoxBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBDBDB"));
                    this.Resources["TextBoxBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCFCFC"));
                    this.Resources["TextBoxBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B9BBBE"));
                    this.Resources["TextBoxBorderInner"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CFD0D2"));
                    this.Resources["SectionBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2F3F5"));
                    this.Resources["ComboBoxBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCFCFC"));
                    this.Resources["ComboBoxBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B9BBBE"));
                    this.Resources["ComboBoxBorderInner"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CFD0D2"));
                    this.Resources["ComboBoxSelected"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCDDDE"));
                    this.Resources["ComboBoxHighlighted"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F6F6F7"));
                    this.Resources["ComboBoxArrow"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B9BBBE"));
                    this.Resources["TextColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4F5660"));
                    this.Resources["TextBoxForegroundThemeBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4F5660"));
                    this.Resources["ShadowColor"] = Colors.White;
                    this.XenviousImage.Source = (BitmapImage)FindResource("ogimageb256");
                    break;
                case 2:
                    this.Resources["SelectedBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCDDDE"));
                    this.Resources["HoverBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3755"));
                    this.Resources["DisabledForegroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888"));
                    this.Resources["ButtonHoverBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3755"));
                    this.Resources["ButtonClickBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#96A0BE"));
                    this.Resources["BackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#161C32"));
                    this.Resources["SeactionHeaderBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27304B"));
                    this.Resources["TextBoxBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E253D"));
                    this.Resources["TextBoxBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#90A0BC"));
                    this.Resources["TextBoxBorderInner"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3755"));
                    this.Resources["SectionBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C233D"));
                    this.Resources["ComboBoxBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E253D"));
                    this.Resources["ComboBoxBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#90A0BC"));
                    this.Resources["ComboBoxBorderInner"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3755"));
                    this.Resources["ComboBoxSelected"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C233D"));
                    this.Resources["ComboBoxHighlighted"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3755"));
                    this.Resources["ComboBoxArrow"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B9BBBE"));
                    this.Resources["TextColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#90A0BC"));
                    this.Resources["TextBoxForegroundThemeBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#90A0BC"));
                    this.Resources["ShadowColor"] = Colors.Black;
                    break;
                default:
                    break;
            }
        }

        private void InitializeTranslation(ini_reader ini)
        {
            var configuredCode = ini?.ReadString("Settings", "translation");
            var normalized = _Language.NormalizeCode(configuredCode);

            ApplyLanguage(normalized, persistToConfig: false);

            if (ddlanguage != null)
            {
                var index = Array.IndexOf(LanguageCodes, normalized);
                if (index < 0)
                {
                    index = Array.IndexOf(LanguageCodes, _Language.DefaultCode);
                }

                _suppressLanguageChange = true;
                ddlanguage.SelectedIndex = index;
                _suppressLanguageChange = false;
            }
        }

        private void ApplyLanguage(string languageCode, bool persistToConfig)
        {
            var normalized = _Language.NormalizeCode(languageCode);

            if (string.Equals(_currentLanguageCode, normalized, StringComparison.OrdinalIgnoreCase) && Translation != null)
            {
                if (persistToConfig)
                {
                    WriteLanguageToConfig(normalized);
                }
                return;
            }

            _currentLanguageCode = normalized;
            Translation = _Language.FromCode(normalized);

            if (persistToConfig)
            {
                WriteLanguageToConfig(normalized);
            }
        }

        private void ddlanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressLanguageChange || ddlanguage == null)
            {
                return;
            }

            var index = ddlanguage.SelectedIndex;
            if (index < 0 || index >= LanguageCodes.Length)
            {
                return;
            }

            var code = LanguageCodes[index];
            ApplyLanguage(code, persistToConfig: true);
        }

        public void checkSection()
        {
            if (m.IsProcOpen)
            {
                if (!gamemodeWarning)
                {
                    if (Functions.Read.isMission() || Functions.Read.isSurvival() || Functions.Read.isDeathmatch())
                    {
                        restrictionsectionRace.Visibility = Visibility.Visible;
                        restrictionsectionMission.Visibility = Visibility.Collapsed;
                        MissionrestrictionsectionRace.Visibility = Visibility.Visible;
                        MissionrestrictionsectionMission.Visibility = Visibility.Collapsed;
                        SectionRestrictionMission.Visibility = Visibility.Collapsed;
                        if (EditPages.SelectedItem == PageRace)
                        {
                            SectionRestriction.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            SectionRestriction.Visibility = Visibility.Collapsed;
                        }
                    }
                    else if (Functions.Read.isRace())
                    {
                        restrictionsectionMission.Visibility = Visibility.Visible;
                        restrictionsectionRace.Visibility = Visibility.Collapsed;
                        MissionrestrictionsectionMission.Visibility = Visibility.Visible;
                        MissionrestrictionsectionRace.Visibility = Visibility.Collapsed;
                        if (EditPages.SelectedItem == PageRace || EditPages.SelectedItem == PageProps || EditPages.SelectedItem == PageWeapon || EditPages.SelectedItem == Pagecentity || EditPages.SelectedItem == PageZone)
                        {
                            SectionRestriction.Visibility = Visibility.Collapsed;
                        }
                        else if (EditPages.SelectedItem == PageMission)
                        {
                            SectionRestriction.Visibility = Visibility.Collapsed;
                            if (PageInnerMission.SelectedItem == PageInnerMissionGeneral || PageInnerMission.SelectedItem == PageInnerMissionMenubs)
                            {
                                SectionRestrictionMission.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                SectionRestrictionMission.Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            SectionRestriction.Visibility = Visibility.Visible;
                        }
                    }
                }
                else
                {
                    SectionRestriction.Visibility = Visibility.Collapsed;
                    restrictionsectionRace.Visibility = Visibility.Collapsed;
                    restrictionsectionMission.Visibility = Visibility.Collapsed;
                    MissionrestrictionsectionRace.Visibility = Visibility.Collapsed;
                    MissionrestrictionsectionMission.Visibility = Visibility.Collapsed;
                    SectionRestrictionMission.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void LoadConfig()
        {
            ini_reader ini = new ini_reader(Functions.getRoamingConfigFilePath());
            MenuSwitcherPresets.Clear();

            try
            {
                cbgmwarning.IsChecked = ini.ReadBoolean("Settings", "gmwarning");
                cbsettingslva.IsChecked = ini.ReadBoolean("Settings", "lva");
                cbsettingsoldcreatorrefresh.IsChecked = ini.ReadBoolean("Settings", "oldcreatorrefresh");
                tbgeglobal.Text = ini.ReadString("Settings", "geglobal");
                tblelocal.Text = ini.ReadString("Settings", "lelocal");
                tblelocalscript.Text = ini.ReadString("Settings", "lelocalscript");
                cbsettingsexpscrfeat.IsChecked = ini.ReadBoolean("Settings", "expscrfeat");
                cbsettingsswitchcamkey.IsChecked = ini.ReadBoolean("Settings", "switchcamkey");
                cbsettingswritelogstofile.IsChecked = ini.ReadBoolean("Settings", "writelogstofile");
                cbsettingshidegtamessage.IsChecked = ini.ReadBoolean("Settings", "hidegtamessage");
            }
            catch (Exception e)
            {
                Log.Error("Error loading some settings", e, "init");
            }

            try
            {
                var s = ini.ReadString("Settings", "loglevel");
                int idx;
                if (!int.TryParse(s, out idx)) idx = 3;
                if (idx < 0 || idx >= ddsettingsloglevel.Items.Count) idx = 3;
                ddsettingsloglevel.SelectedIndex = idx;
            }
            catch (Exception e)
            {
                Log.Error("Error loading loglevel.. defaults to Warn", e, "init");
            }

            try
            {
                haslistcontainer.Visibility = ini.ReadBoolean("Settings", "propshasexpanded") ? Visibility.Visible : Visibility.Collapsed;
                changeHasExpandButtonPath();
            }
            catch (Exception e)
            {
                Log.Error("Error loading propshasexpanded", e, "init");
            }

            try
            {
                if (ini.ReadBoolean("Settings", "killload"))
                {
                    var tempkillfromconfig = JsonConvert.DeserializeObject<Kill>(ConfigText.Decode(ini.ReadString("Settings", "killvalues")));
                    if (tempkillfromconfig != null)
                    {
                        kill = tempkillfromconfig;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Error loading killload", e, "init");
            }

            try
            {
                var temp_current_creator_ptr = ini.ReadString("Settings", "lastpresets").Split(',');
                if (temp_current_creator_ptr[0].Length == 7)
                {
                    GTA.Offsets.Editor.localptr = temp_current_creator_ptr.Select(x => long.Parse(x, NumberStyles.HexNumber)).ToArray();
                }
            }
            catch (Exception e)
            {
                Log.Error("Error loading lastpresets", e, "init");
            }

            try
            {
                var encryptedPresets = ini.ReadString("Settings", MenuSwitcherPresetsConfigKey);
                if (!string.IsNullOrWhiteSpace(encryptedPresets))
                {
                    var decrypted = ConfigText.Decode(encryptedPresets);
                    var presets = System.Text.Json.JsonSerializer.Deserialize<List<MenuSwitcherPreset>>(decrypted, MenuPresetSerializerOptions);
                    if (presets != null)
                    {
                        foreach (var preset in presets)
                        {
                            if (preset == null || string.IsNullOrWhiteSpace(preset.Name))
                            {
                                continue;
                            }

                            MenuSwitcherPresets.Add(preset);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Error loading Menu Switcher presets", e, "init");
                MenuSwitcherPresets.Clear();
            }

            if (cbmsPresets != null)
            {
                if (MenuSwitcherPresets.Count > 0)
                {
                    cbmsPresets.SelectedIndex = 0;
                }
                else
                {
                    cbmsPresets.SelectedIndex = -1;
                    tbmsPresetName?.Clear();
                }
            }

            try
            {
                string decrypted = ConfigText.Decode(ini.ReadString("Settings", "gefreeze"));

                var entries = System.Text.Json.JsonSerializer.Deserialize<List<GlobalFreezeEntry>>(decrypted) ?? new List<GlobalFreezeEntry>()
;
                foreach (var e in entries)
                {
                    switch (e.Type)
                    {
                        case "int":
                            freezeList.Items.Add(
                                new GlobalFreezeer(e.Name, e.GlobalName, int.Parse(e.ValueString!, CultureInfo.InvariantCulture)));
                            break;

                        case "long":
                            freezeList.Items.Add(
                                new GlobalFreezeer(e.Name, e.GlobalName, long.Parse(e.ValueString!, CultureInfo.InvariantCulture)));
                            break;

                        case "float":
                            freezeList.Items.Add(
                                new GlobalFreezeer(e.Name, e.GlobalName, float.Parse(e.ValueString!, CultureInfo.InvariantCulture)));
                            break;

                        case "double":
                            freezeList.Items.Add(
                                new GlobalFreezeer(e.Name, e.GlobalName, double.Parse(e.ValueString!, CultureInfo.InvariantCulture)));
                            break;

                        case "short":
                            freezeList.Items.Add(
                                new GlobalFreezeer(e.Name, e.GlobalName, short.Parse(e.ValueString!, CultureInfo.InvariantCulture)));
                            break;

                        case "string":
                            freezeList.Items.Add(
                                new GlobalFreezeer(e.Name, e.GlobalName, e.ValueString ?? string.Empty));
                            break;

                        case "bytes":
                            var bytes = string.IsNullOrEmpty(e.ValueBase64)
                                        ? Array.Empty<byte>()
                                        : Convert.FromBase64String(e.ValueBase64);
                            freezeList.Items.Add(
                                new GlobalFreezeer(e.Name, e.GlobalName, bytes));
                            break;

                        default:
                            // unbekannter Typ -> überspringen oder loggen
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Error loading Global Editor Freeze List", e, "init");
            }

            try
            {
                string decrypted = ConfigText.Decode(ini.ReadString("Settings", "lefreeze"));
                List<LocalFreezeEntry>  entries = System.Text.Json.JsonSerializer.Deserialize<List<LocalFreezeEntry>>(decrypted) ?? new List<LocalFreezeEntry>();

                foreach (var e in entries)
                {
                    switch (e.Type)
                    {
                        case "int":
                            localfreezeList.Items.Add(
                                new LocalFreezeer(e.Name, e.Script, e.LocalName, int.Parse(e.ValueString!, CultureInfo.InvariantCulture)));
                            break;

                        case "long":
                            localfreezeList.Items.Add(
                                new LocalFreezeer(e.Name, e.Script, e.LocalName, long.Parse(e.ValueString!, CultureInfo.InvariantCulture)));
                            break;

                        case "float":
                            localfreezeList.Items.Add(
                                new LocalFreezeer(e.Name, e.Script, e.LocalName, float.Parse(e.ValueString!, CultureInfo.InvariantCulture)));
                            break;

                        case "double":
                            localfreezeList.Items.Add(
                                new LocalFreezeer(e.Name, e.Script, e.LocalName, double.Parse(e.ValueString!, CultureInfo.InvariantCulture)));
                            break;

                        case "short":
                            localfreezeList.Items.Add(
                                new LocalFreezeer(e.Name, e.Script, e.LocalName, short.Parse(e.ValueString!, CultureInfo.InvariantCulture)));
                            break;

                        case "string":
                            localfreezeList.Items.Add(
                                new LocalFreezeer(e.Name, e.Script, e.LocalName, e.ValueString ?? string.Empty));
                            break;

                        case "bytes":
                            var bytes = string.IsNullOrEmpty(e.ValueBase64)
                                        ? Array.Empty<byte>()
                                        : Convert.FromBase64String(e.ValueBase64);
                            localfreezeList.Items.Add(
                                new LocalFreezeer(e.Name, e.Script, e.LocalName, bytes));
                            break;

                        default:
                            // unbekannter Typ -> ggf. loggen
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Error loading Local Editor Freeze List", e, "init");
            }
        }

        public void SafeConfig()
        {
            try
            {
                ini_reader ini = new ini_reader(Functions.getRoamingConfigFilePath());

                var geentries = new List<GlobalFreezeEntry>();

                foreach (var item in freezeList.Items)
                {
                    var obj = (GlobalFreezeer)item;
                    var type = obj.val?.GetType();

                    var e = new GlobalFreezeEntry
                    {
                        Name = obj.Name,
                        GlobalName = obj.GlobalName
                    };

                    if (type == typeof(byte[]))
                    {
                        e.Type = "bytes";
                        e.ValueBase64 = Convert.ToBase64String(obj.Value ?? Array.Empty<byte>());
                    }
                    else if (type == typeof(string))
                    {
                        e.Type = "string";
                        e.ValueString = (string)obj.val;
                    }
                    else if (type == typeof(int))
                    {
                        e.Type = "int";
                        e.ValueString = ((int)obj.val).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (type == typeof(long))
                    {
                        e.Type = "long";
                        e.ValueString = ((long)obj.val).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (type == typeof(float))
                    {
                        e.Type = "float";
                        e.ValueString = ((float)obj.val).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (type == typeof(double))
                    {
                        e.Type = "double";
                        e.ValueString = ((double)obj.val).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (type == typeof(short))
                    {
                        e.Type = "short";
                        e.ValueString = ((short)obj.val).ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        // Fallback: nimm die Bytes aus obj.Value
                        e.Type = "bytes";
                        e.ValueBase64 = Convert.ToBase64String(obj.Value ?? Array.Empty<byte>());
                    }

                    geentries.Add(e);
                }

                var gejson = System.Text.Json.JsonSerializer.Serialize(geentries, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
                ini.Write("Settings", "gefreeze", ConfigText.Encode(gejson));

                var leentries = new List<LocalFreezeEntry>();

                foreach (var item in localfreezeList.Items)
                {
                    var obj = (LocalFreezeer)item;
                    var type = obj.val?.GetType();

                    var e = new LocalFreezeEntry
                    {
                        Name = obj.Name,
                        Script = obj.Script,
                        LocalName = obj.LocalName
                    };

                    if (type == typeof(byte[]))
                    {
                        e.Type = "bytes";
                        e.ValueBase64 = Convert.ToBase64String(obj.Value ?? Array.Empty<byte>());
                    }
                    else if (type == typeof(string))
                    {
                        e.Type = "string";
                        e.ValueString = (string)obj.val;
                    }
                    else if (type == typeof(int))
                    {
                        e.Type = "int";
                        e.ValueString = ((int)obj.val).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (type == typeof(long))
                    {
                        e.Type = "long";
                        e.ValueString = ((long)obj.val).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (type == typeof(float))
                    {
                        e.Type = "float";
                        e.ValueString = ((float)obj.val).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (type == typeof(double))
                    {
                        e.Type = "double";
                        e.ValueString = ((double)obj.val).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (type == typeof(short))
                    {
                        e.Type = "short";
                        e.ValueString = ((short)obj.val).ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        // Fallback: nimm die Bytes, falls val-Typ unbekannt
                        e.Type = "bytes";
                        e.ValueBase64 = Convert.ToBase64String(obj.Value ?? Array.Empty<byte>());
                    }

                    leentries.Add(e);
                }

                var lejson = System.Text.Json.JsonSerializer.Serialize(leentries, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
                ini.Write("Settings", "lefreeze", ConfigText.Encode(lejson));

                SaveMenuPresets(ini);

                if (cbmissionkills2c.IsChecked ?? true)
                {
                    var killjsonstring = JsonConvert.SerializeObject(kill);

                    string storedkilljson = ConfigText.Encode(killjsonstring);

                    ini.Write("Settings", "killload", true);
                    ini.Write("Settings", "killvalues", storedkilljson);
                }
                else
                {
                    ini.Write("Settings", "killload", false);
                }

                //if (GTA.Editor.mpropsaddys != null)
                //    ini.Write("Settings", "lastmprops", String.Join(",", GTA.Editor.mpropsaddys));
                ini.Write("Settings", "gmwarning", cbgmwarning.IsChecked ?? true);
                ini.Write("Settings", "lva", cbsettingslva.IsChecked ?? true);
                ini.Write("Settings", "oldcreatorrefresh", cbsettingsoldcreatorrefresh.IsChecked ?? true);
                ini.Write("Settings", "geglobal", tbgeglobal.Text);
                ini.Write("Settings", "lelocal", tblelocal.Text);
                ini.Write("Settings", "lelocalscript", tblelocalscript.Text);
                ini.Write("Settings", "expscrfeat", cbsettingsexpscrfeat.IsChecked ?? true);
                ini.Write("Settings", "switchcamkey", cbsettingsswitchcamkey.IsChecked ?? true);
                ini.Write("Settings", "writelogstofile", cbsettingswritelogstofile.IsChecked ?? true);
                ini.Write("Settings", "loglevel", ddsettingsloglevel.SelectedIndex.ToString());

                // Only known while a creator was found; without the guard the exception
                // skipped every setting written below (screen, language, colour, ...).
                if (GTA.Offsets.Editor.localptr != null)
                    ini.Write("Settings", "lastpresets", "\"" + string.Join(",", GTA.Offsets.Editor.localptr.Select(x => x.ToString("X"))) + "\"");

                var screen = getWorkingScreen();

                ini.Write("Settings", "screen", screen.DeviceName);

                ini.Write("Settings", "width", this.Width.ToString());
                ini.Write("Settings", "height", this.Height.ToString());

                ini.Write("Settings", "startx", this.Left.ToString());
                ini.Write("Settings", "starty", this.Top.ToString());

                switch (ddlanguage.SelectedIndex)
                {
                    case 0:
                        ini.Write("Settings", "translation", "de");
                        break;
                    case 1:
                        ini.Write("Settings", "translation", "en");
                        break;
                    case 2:
                        ini.Write("Settings", "translation", "ru");
                        break;
                    case 3:
                        ini.Write("Settings", "translation", "pl");
                        break;
                    case 4:
                        ini.Write("Settings", "translation", "fr");
                        break;
                    case 5:
                        ini.Write("Settings", "translation", "zh_cn");
                        break;
                    default:
                        ini.Write("Settings", "translation", "en");
                        break;
                }
                switch (ddcolor.SelectedIndex)
                {
                    case 0:
                        ini.Write("Settings", "color", "gray");
                        break;
                    case 1:
                        ini.Write("Settings", "color", "white");
                        break;
                    default:
                        ini.Write("Settings", "color", "gray");
                        break;
                }
                ini.Write("Settings", "propshasexpanded", haslistcontainer.Visibility == Visibility.Visible ? true : false);
            }
            catch (Exception e)
            {
                Log.Error("writing config file", e, "save");
            }
        }

        private void SaveMenuPresets(ini_reader? iniOverride = null)
        {
            try
            {
                var ini = iniOverride ?? new ini_reader(Functions.getRoamingConfigFilePath());

                if (MenuSwitcherPresets.Count > 0)
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(MenuSwitcherPresets, MenuPresetSerializerOptions);
                    ini.Write("Settings", MenuSwitcherPresetsConfigKey, ConfigText.Encode(json));
                }
                else
                {
                    ini.DeleteKey("Settings", MenuSwitcherPresetsConfigKey);
                }
            }
            catch (Exception e)
            {
                Log.Error("Error saving Menu Switcher presets", e, "init");
            }
        }

        private void cbgmwarning_Checked(object sender, RoutedEventArgs e)
        {
            gamemodeWarning = cbgmwarning.IsChecked == true;
        }

        private void cbsettingsexpscrfeat_Checked(object sender, RoutedEventArgs e)
        {
            WriteScriptFeatureBits(cbsettingsexpscrfeat.IsChecked ?? true);
        }

        // The script feature bits of custom_check: dispatch bits 1..5 for the injected
        // functions, bit 30 for the dev patches. Bit 5 makes the race creator flicker, so it
        // is only set inside the other creators (and outside a creator it waits until one
        // is known, so the race creator never runs a frame with it).
        private static void WriteScriptFeatureBits(bool enable)
        {
            string creator = GTA.CurrentCreatorName();
            for (int bit = 1; bit <= 5; bit++)
            {
                bool wanted = enable && (bit != 5 || (creator != "" && creator != "fm_race_creator"));
                Functions.Write.writebinary(bit, GTA.Offsets.Editor.custom_check, wanted);
            }
            Functions.Write.writebinary(30, GTA.Offsets.Editor.custom_check, enable);
        }

        private void cbsettingsswitchcamkey_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(31, GTA.Offsets.Editor.custom_check, cbsettingsswitchcamkey);
        }

        private void cbsettingswritelogstofile_Checked(object sender, RoutedEventArgs e)
        {
            Log.FileLoggingEnabled = cbsettingswritelogstofile.IsChecked == true;
        }

        private void ddsettingsloglevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ddsettingsloglevel.SelectedItem as ComboBoxItem;
            if (item == null) return;

            var text = (item.Content ?? "").ToString();
            LogLevel level;
            if (Enum.TryParse(text, ignoreCase: true, out level))
            {
                Log.MinimumLevel = level;
                Log.MinimumLevelFile = level;
            }
        }
    }
}
