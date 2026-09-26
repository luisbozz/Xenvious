using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Xenvious.JSON;
using Xenvious.Logging;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: DashboardPage page.
    public partial class MainWindow
    {
        public Thread force_publishThread;

        private void cb_dev_Checked(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
                m.memory(GTA.Offsets.Editor.dev).SetInt(cb_dev.IsChecked == true ? GTA.DevPatched : GTA.DevOriginal);
        }

        public void IntegerPasteHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)) && sender is TextBox)
            {
                string pastedText = (e.DataObject.GetData(typeof(string)) as string);
                string result = pastedText.Substring(0, pastedText.Length > 11 ? 11 : pastedText.Length);
                result = result.Substring(0, 1) + Regex.Replace(result.Substring(1), "[^0-9]+", "");
                result = Regex.Replace(result, "[^0-9\\-]+", "");

                DataObject d = new DataObject();
                d.SetData(DataFormats.Text, result);
                e.DataObject = d;

                e.Handled = true;
            }
            else
            {
                e.CancelCommand();
            }
        }



        private void ddweather_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                new Global(GTA.Offsets.Editor.weth).SetInt(ddweather.SelectedIndex);
            }
        }

        private void ddtimeofday_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                new Global(GTA.Offsets.Editor.tod).SetInt(ddtimeofday.SelectedIndex);
            }
        }

        private void cb_customdaytime_Checked(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                Functions.Write.writebinary(16, GTA.Offsets.Editor.menubs2, true);

                tb_cdth.IsEnabled = true;
                tb_cdtm.IsEnabled = true;
                tb_cdth.Visibility = Visibility.Visible;
                tb_cdtm.Visibility = Visibility.Visible;
                ddtimeofday.IsEnabled = false;
            }
        }

        private void cb_customdaytime_Unchecked(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                Functions.Write.writebinary(16, GTA.Offsets.Editor.menubs2, false);

                tb_cdth.IsEnabled = false;
                tb_cdtm.IsEnabled = false;
                tb_cdth.Visibility = Visibility.Collapsed;
                tb_cdtm.Visibility = Visibility.Collapsed;
                ddtimeofday.IsEnabled = true;
            }
        }

        private void FormatTextForInteger(object sender, KeyEventArgs e)
        {
            DataObject.AddPastingHandler((DependencyObject)sender, new DataObjectPastingEventHandler(IntegerPasteHandler));

            if (!char.IsControl(GetCharFromKey(e.Key)) && !char.IsDigit(GetCharFromKey(e.Key)))
            {
                e.Handled = true;
            }
            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                creatorRefresh();
            }
        }

        private void tb_cdth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tb_cdth.Text, false) && m.IsProcOpen)
            {
                if (Convert.ToInt32(tb_cdth.Text) > 23)
                {
                    tb_cdth.Text = "23";
                    tb_cdth.SelectionStart = tb_cdth.Text.Length;
                }

                new Global(GTA.Offsets.Editor.todhr).SetInt(tb_cdth.Text);
            }
            else
            {
                tb_cdth.Text = "";
            }
        }

        private void tb_cdtm_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tb_cdtm.Text, false) && m.IsProcOpen)
            {
                if (Convert.ToInt32(tb_cdtm.Text) > 59)
                {
                    tb_cdtm.Text = "59";
                    tb_cdtm.SelectionStart = tb_cdtm.Text.Length;
                }

                new Global(GTA.Offsets.Editor.todmn).SetInt(tb_cdtm.Text);
            }
            else
            {
                tb_cdtm.Text = "";
            }
        }

        private void ddradio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                new Global(GTA.Offsets.Editor.musx).SetInt(GTA.Editor.radioArray.GetValue(ddradio.SelectedIndex).ToString());
            }
        }

        private void ddmusic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                new Global(GTA.Offsets.Editor.ausc).SetInt(ddmusic.SelectedIndex);
            }
        }

        private void cb_hdvision_Unchecked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(7, GTA.Offsets.Editor.menubs10, false);
        }

        private void cb_hdvision_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(7, GTA.Offsets.Editor.menubs10, true);
        }

        private void cb_blackout_Unchecked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(28, GTA.Offsets.Editor.menubs14, false);
        }

        private void cb_blackout_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(28, GTA.Offsets.Editor.menubs14, true);
        }

        private void tb_Rounds_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_Rounds.Text) || !m.IsProcOpen)
                return;
            if (!int.TryParse(tb_Rounds.Text, out int rounds))
                return;

            // The LTS creator offers 1..7 rounds with two teams, 1..5 with three and
            // 1..3 with four; one team gets the two-team limit. Anything else is left
            // unwritten.
            if (Functions.Read.isLTS() && (rounds < 1 || rounds > MaxLTSRounds(new Global(GTA.Offsets.Editor.tnum).Get<int>())))
                return;

            string temp = (rounds - (Functions.Read.isMission() ? 0 : 1)).ToString();

            if (IsValidInt(temp, false))
            {
                new Global(Functions.Read.isMission() ? GTA.Offsets.Editor.numRounds : GTA.Offsets.Editor.Race.Checkpoints.lap).SetInt(temp);
            }
        }

        private static int MaxLTSRounds(int teams)
        {
            if (teams >= 4)
                return 3;
            return teams == 3 ? 5 : 7;
        }

        private void tb_Teams_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_Teams.Text) || !m.IsProcOpen)
                return;

            // Team data holds four teams. The LTS creator raises a count below 2, but
            // a one-team LTS is allowed here on purpose. Anything else is left unwritten.
            if (int.TryParse(tb_Teams.Text, out int teams) && teams >= 1 && teams <= 4)
            {
                new Global(GTA.Offsets.Editor.tnum).SetInt(teams);
            }
        }

        private void tb_minp_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_minp.Text))
                return;
            if (IsValidInt(tb_minp.Text, false) && m.IsProcOpen)
            {
                new Global(GTA.Offsets.Editor.min).SetInt(tb_minp.Text);
            }
        }

        private void tb_maxp_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_maxp.Text))
                return;
            if (IsValidInt(tb_maxp.Text, false) && m.IsProcOpen)
            {
                int maxpadjustvalue = 0;
                if (Functions.Read.isRace() || Functions.Read.isDeathmatch())
                {
                    maxpadjustvalue = 2;
                }
                else if (Functions.Read.isMission())
                {
                    maxpadjustvalue = 1;
                }
                new Global(GTA.Offsets.Editor.num).SetInt(Functions.int_parse(tb_maxp.Text) - maxpadjustvalue);
            }
        }

        private void tb_JobDec_TextChanged(object sender, TextChangedEventArgs e)
        {
            setDescribtionNew(tb_JobDec.Text);
        }

        public void setDescribtionNew(string describtion)
        {
            if (m == null || !m.IsProcOpen)
                return;

            const int ChunkSize = 63;   // Nutzbytes je Segment
            const int ClearSize = ChunkSize + 1; // +1 für das Terminator-Byte im Segment
            const int SlotStride = 16;   // Abstand zwischen Segment-Basen
            const int MaxSlots = 8;    // 0..7
            var baseAddr = GTA.Offsets.Editor.dec;

            var bytes = Encoding.UTF8.GetBytes(describtion ?? string.Empty);
            int maxData = ChunkSize * MaxSlots;
            int total = Math.Min(bytes.Length, maxData);

            // 1) Erst alles löschen (wichtig für "Text löschen"):
            //    Wir schreiben pro Segment 64 NUL-Bytes (63 + Terminator-Position).
            var zeros64 = new byte[ClearSize];
            for (int i = 0; i < MaxSlots; i++)
                new Global(baseAddr + SlotStride * i).SetBytes(zeros64);

            // Kein Text? Dann sind wir fertig (alles genullt).
            if (total == 0)
                return;

            // 2) Daten in 63-Byte-Chunks schreiben
            int slotsNeeded = (total + ChunkSize - 1) / ChunkSize; // ceil(total/63)
            for (int i = 0; i < slotsNeeded; i++)
            {
                int start = i * ChunkSize;
                int length = Math.Min(ChunkSize, total - start);
                if (length <= 0) break;

                var chunk = new byte[length];
                Buffer.BlockCopy(bytes, start, chunk, 0, length);

                new Global(baseAddr + SlotStride * i).SetBytes(chunk);
            }

            // 3) Explizit nach dem letzten Zeichen einen 0-Byte-Terminator setzen.
            //    (Durch das Pre-Clear wäre das schon 0, aber so ist’s eindeutig.)
            int lastSlot = slotsNeeded - 1;
            int lastLen = total - lastSlot * ChunkSize;
            new Global(baseAddr + SlotStride * lastSlot + lastLen).SetBytes(new byte[] { 0 });

            if (bytes.Length > maxData) Log.Debug($"Description truncated to {maxData} bytes.");
        }

        private void tb_JobTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                new Global(GTA.Offsets.Editor.nm).SetString(tb_JobTitle.Text);
            }
        }

        private void ddjobtype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ddjobsubtype.Items.Clear();
            ddjobsubtype.IsEnabled = true;
            Binding b = new Binding();
            Binding b1 = new Binding();
            ObservableCollection<string> values = new ObservableCollection<string>();

            switch (ddjobtype.SelectedIndex)
            {
                case (int)GTA.Editor.JobTypes.Mission:
                    b.Path = new PropertyPath("Translation[jobsubtypeheadermission]");

                    var missionitem = new ComboBoxItem();
                    missionitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_mission_capture]"));
                    var missionitem1 = new ComboBoxItem();
                    missionitem1.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_mission_lts]"));
                    var missionitem2 = new ComboBoxItem();
                    missionitem2.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_mission_mission]"));

                    ddjobsubtype.Items.Add(missionitem);
                    ddjobsubtype.Items.Add(missionitem1);
                    ddjobsubtype.Items.Add(missionitem2);

                    break;
                case (int)GTA.Editor.JobTypes.Deathmatch:
                    b.Path = new PropertyPath("Translation[jobsubtypeheaderdeathmatch]");

                    var dmitem = new ComboBoxItem();
                    dmitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_deathmatch_dm]"));
                    var dmitem1 = new ComboBoxItem();
                    dmitem1.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_deathmatch_vdm]"));
                    var dmitem2 = new ComboBoxItem();
                    dmitem2.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_deathmatch_koth]"));

                    ddjobsubtype.Items.Add(dmitem);
                    ddjobsubtype.Items.Add(dmitem1);
                    ddjobsubtype.Items.Add(dmitem2);

                    break;
                case (int)GTA.Editor.JobTypes.Race:
                    b.Path = new PropertyPath("Translation[jobsubtypeheaderrace]");

                    var raceitem = new ComboBoxItem();
                    raceitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_race_landrace]"));
                    var raceitem1 = new ComboBoxItem();
                    raceitem1.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_race_searace]"));
                    var raceitem2 = new ComboBoxItem();
                    raceitem2.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_race_flyrace]"));
                    var raceitem3 = new ComboBoxItem();
                    raceitem3.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_race_stunt]"));
                    var raceitem4 = new ComboBoxItem();
                    raceitem4.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_race_parachute]"));
                    var raceitem5 = new ComboBoxItem();
                    raceitem5.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_race_specialveh]"));
                    var raceitem6 = new ComboBoxItem();
                    raceitem6.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_race_target]"));
                    var raceitem7 = new ComboBoxItem();
                    raceitem7.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_race_transform]"));
                    var raceitem8 = new ComboBoxItem();
                    raceitem8.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_race_arena]"));
                    var raceitem9 = new ComboBoxItem();
                    raceitem9.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_race_openwheel]"));
                    var raceitem10 = new ComboBoxItem();
                    raceitem10.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_race_pursuit]"));
                    var raceitem11 = new ComboBoxItem();
                    raceitem11.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_race_street]"));

                    ddjobsubtype.Items.Add(raceitem);
                    ddjobsubtype.Items.Add(raceitem1);
                    ddjobsubtype.Items.Add(raceitem2);
                    ddjobsubtype.Items.Add(raceitem3);
                    ddjobsubtype.Items.Add(raceitem4);
                    ddjobsubtype.Items.Add(raceitem5);
                    ddjobsubtype.Items.Add(raceitem6);
                    ddjobsubtype.Items.Add(raceitem7);
                    ddjobsubtype.Items.Add(raceitem8);
                    ddjobsubtype.Items.Add(raceitem9);
                    ddjobsubtype.Items.Add(raceitem10);
                    ddjobsubtype.Items.Add(raceitem11);

                    break;
                case (int)GTA.Editor.JobTypes.Survival:
                    b.Path = new PropertyPath("Translation[jobsubtypeheadersurvival]");

                    var survitem = new ComboBoxItem();
                    survitem.SetBinding(ComboBoxItem.ContentProperty, new Binding("Translation[jobsubtype_survival]"));

                    ddjobsubtype.Items.Add(survitem);

                    ddjobsubtype.IsEnabled = false;
                    break;
                default:
                    break;
            }

            Lbljobsubtype.SetBinding(ContentProperty, b);
            ddjobsubtype.SelectedIndex = -1;

            if (m.IsProcOpen)
            {
                switch (ddjobtype.SelectedIndex)
                {
                    case (int)GTA.Editor.JobTypes.Mission:
                        new Global(GTA.Offsets.Editor.type).SetInt(0);
                        break;
                    case (int)GTA.Editor.JobTypes.Deathmatch:
                        new Global(GTA.Offsets.Editor.type).SetInt(1);
                        break;
                    case (int)GTA.Editor.JobTypes.Race:
                        new Global(GTA.Offsets.Editor.type).SetInt(2);
                        break;
                    case (int)GTA.Editor.JobTypes.Survival:
                        new Global(GTA.Offsets.Editor.type).SetInt(3);
                        break;
                    default:
                        break;
                }
            }
        }

        private void ddjobsubtype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen && ddjobsubtype.SelectedIndex != -1)
            {
                switch (ddjobtype.SelectedIndex)
                {
                    case (int)GTA.Editor.JobTypes.Race:

                        bool p2p = new Global(GTA.Offsets.Editor.Race.Checkpoints.ptp).Get<int>() == 0 ? false : true;

                        //set Arena War values if index matches
                        SetArenaWarValues(ddjobsubtype.SelectedIndex != 8 ? false : true);

                        switch (ddjobsubtype.SelectedIndex)
                        {
                            case 0:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(0);
                                new Global(GTA.Offsets.Editor.racetype).SetInt(p2p ? 1 : 0);
                                break;
                            case 1:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(0);
                                new Global(GTA.Offsets.Editor.racetype).SetInt(p2p ? 3 : 2);
                                break;
                            case 2:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(0);
                                new Global(GTA.Offsets.Editor.racetype).SetInt(p2p ? 5 : 4);
                                break;
                            case 3:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(0);
                                new Global(GTA.Offsets.Editor.racetype).SetInt(p2p ? 7 : 6);
                                break;
                            case 4:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(0);
                                new Global(GTA.Offsets.Editor.racetype).SetInt(p2p ? 8 : 9);
                                break;
                            case 5:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(21);
                                new Global(GTA.Offsets.Editor.racetype).SetInt(p2p ? 7 : 6);
                                break;
                            case 6:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(0);
                                new Global(GTA.Offsets.Editor.racetype).SetInt(p2p ? 19 : 18);
                                break;
                            case 7:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(20);
                                new Global(GTA.Offsets.Editor.racetype).SetInt(p2p ? 7 : 6);
                                break;
                            case 8:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(0);
                                new Global(GTA.Offsets.Editor.racetype).SetInt(p2p ? 7 : 6);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.icv).SetInt(17);
                                break;
                            case 9:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(0);
                                new Global(GTA.Offsets.Editor.racetype).SetInt(p2p ? 25 : 24);
                                break;
                            case 10:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(24);
                                new Global(GTA.Offsets.Editor.racetype).SetInt(26);
                                break;
                            case 11:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(24);
                                new Global(GTA.Offsets.Editor.racetype).SetInt(27);
                                break;
                            default:
                                break;
                        }

                        break;

                    case (int)GTA.Editor.JobTypes.Deathmatch:

                        SetArenaWarValues(ddjobsubtype.SelectedIndex != 3 ? false : true);

                        switch (ddjobsubtype.SelectedIndex)
                        {
                            case 0:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(0);
                                new Global(GTA.Offsets.Editor.vdm).SetInt(0);
                                break;
                            case 1:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(2);
                                new Global(GTA.Offsets.Editor.vdm).SetInt(1);
                                break;
                            case 2:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(3);
                                new Global(GTA.Offsets.Editor.vdm).SetInt(0);
                                break;
                            default:
                                break;
                        }
                        break;


                    case (int)GTA.Editor.JobTypes.Mission:
                        switch (ddjobsubtype.SelectedIndex)
                        {
                            case 0:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(6);
                                break;
                            case 1:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(5);
                                break;
                            case 2:
                                new Global(GTA.Offsets.Editor.subtype).SetInt(4);
                                break;
                            default:
                                break;
                        }
                        break;


                    default:
                        break;
                }
            }
        }

        public void SetArenaWarValues(bool enable)
        {
            Functions.Write.writebinary(19, GTA.Offsets.Editor.menubs5, enable);
            Functions.Write.writebinary(27, GTA.Offsets.Editor.menubs9, enable);
            Functions.Write.writebinary(22, GTA.Offsets.Editor.menubs11, enable);
            Functions.Write.writebinary(12, GTA.Offsets.Editor.menubs20, enable);
            Functions.Write.writebinary(13, GTA.Offsets.Editor.menubs20, enable);
            Functions.Write.writebinary(24, GTA.Offsets.Editor.menubs20, enable);
            Functions.Write.writebinary(2, GTA.Offsets.Editor.menubs21, enable);
            Functions.Write.writebinary(9, GTA.Offsets.Editor.menubs21, enable);
            Functions.Write.writebinary(10, GTA.Offsets.Editor.menubs21, enable);
            Functions.Write.writebinary(22, GTA.Offsets.Editor.menubs21, enable);
            Functions.Write.writebinary(23, GTA.Offsets.Editor.menubs21, enable);
            Functions.Write.writebinary(24, GTA.Offsets.Editor.menubs21, enable);
            Functions.Write.writebinary(25, GTA.Offsets.Editor.menubs21, enable);
            Functions.Write.writebinary(26, GTA.Offsets.Editor.menubs21, enable);
            Functions.Write.writebinary(17, GTA.Offsets.Editor.menubs22, enable);
            Functions.Write.writebinary(21, GTA.Offsets.Editor.menubs22, enable);
            Functions.Write.writebinary(29, GTA.Offsets.Editor.menubs22, enable);
            Functions.Write.writebinary(30, GTA.Offsets.Editor.menubs22, enable);
            Functions.Write.writebinary(31, GTA.Offsets.Editor.menubs22, enable);
            Functions.Write.writebinary(10, GTA.Offsets.Editor.menubs23, enable);
            Functions.Write.writebinary(11, GTA.Offsets.Editor.menubs23, enable);

            Functions.Write.writebinary(13, GTA.Offsets.Editor.intop2, enable);
            Functions.Write.writebinary(18, GTA.Offsets.Editor.tmbts, 4, GTA.Offsets.Editor.team_NEXT, enable);

            if (enable)
            {
                new Global(GTA.Offsets.Editor.subtype).SetInt(0);
                new Global(GTA.Offsets.Editor.racetype).SetInt(6);
                new Global(GTA.Offsets.Editor.Race.Checkpoints.icv).SetInt(17);
                new Global(GTA.Offsets.Editor.Race.Checkpoints.icv + 1).SetInt(17);
                new Global(GTA.Offsets.Editor.adverm).SetInt(999);
                new Global(GTA.Offsets.Editor.cordmbs).SetInt(60);
                new Global(GTA.Offsets.Editor.turgudm).SetInt(60);
                new Global(GTA.Offsets.Editor.turammo).SetFloat(0);
                new Global(GTA.Offsets.Editor.min).SetInt(1);
                new Global(GTA.Offsets.Editor.Race.Checkpoints.gtar).SetInt(1);
                new Global(GTA.Offsets.Editor.Race.Checkpoints.clbs).SetInt(135168);

                new Global(GTA.Offsets.Editor.Race.aveh + 0).SetInt(15);
                new Global(GTA.Offsets.Editor.Race.aveh + 1).SetInt(15);
                new Global(GTA.Offsets.Editor.Race.aveh + 2).SetInt(2047);
                new Global(GTA.Offsets.Editor.Race.aveh + 3).SetInt(32767);
                new Global(GTA.Offsets.Editor.Race.aveh + 4).SetInt(127);
                new Global(GTA.Offsets.Editor.Race.aveh + 5).SetInt(511);
                new Global(GTA.Offsets.Editor.Race.aveh + 6).SetInt(15);
                new Global(GTA.Offsets.Editor.Race.aveh + 7).SetInt(2047);
                new Global(GTA.Offsets.Editor.Race.aveh + 8).SetInt(127);
                new Global(GTA.Offsets.Editor.Race.aveh + 9).SetInt(16383);
                new Global(GTA.Offsets.Editor.Race.aveh + 10).SetInt(65535);
                new Global(GTA.Offsets.Editor.Race.aveh + 11).SetInt(31);
                new Global(GTA.Offsets.Editor.Race.aveh + 12).SetInt(31);
                new Global(GTA.Offsets.Editor.Race.aveh + 13).SetInt(0);
                new Global(GTA.Offsets.Editor.Race.aveh + 14).SetInt(63);
            }
            else
            {
                new Global(GTA.Offsets.Editor.adverm).SetInt(0);
                new Global(GTA.Offsets.Editor.cordmbs).SetInt(0);
                new Global(GTA.Offsets.Editor.turgudm).SetInt(0);
            }

        }

        private void BtnSkipTest_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {

                //    //Call and return the pointer

                //    ////Create new Variable to Store the result
                //    //byte[] returnedResult = new byte[4];

                //    ////Copy from result pointer to the C# variable
                //    //Marshal.Copy(returnedPtr, returnedResult, 0, 4);

                //    ////The returned value is saved in the returnedResult variable
                //    //byte[] val1 = new byte [] { returnedResult[0], returnedResult[1], returnedResult[2], returnedResult[3] };

                //    //MessageBox.Show(BitConverter.ToInt32(val1, 0).ToString());

                //    System.Windows.Forms.MessageBox.Show(readVal((ulong)GTA.Offsets.Editor.version, new long[0x130AD8]).ToString());

                //    //Marshal.Copy(data, arr, 0, 4);


                //}
                //return;
                bool needscan = curcreatorscanneeded();
                if (needscan)
                    GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy();

                long addy = getCurrentCreatorBase();

                m.memory((addy + GTA.Offsets.Editor.OFFSET_current_creator_pre_test1 * 8).ToString("X")).SetInt(1);
                m.memory((addy + GTA.Offsets.Editor.OFFSET_current_creator_pre_test2 * 8).ToString("X")).SetInt(1);

                if (Functions.Read.isMission())
                {
                    new Global(GTA.Offsets.Editor.testcomplete).SetInt(15);
                }
            }
        }

        private void cb_3dcam_Checked(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                bool ischecked = cb_3dcam.IsChecked ?? true;
                if (isepic)
                {
                    m.memory(GTA.Offsets.Editor.camptr + GTA.Offsets.Editor.cam_mode).SetInt(ischecked ? 18 : -1);
                    m.memory(GTA.Offsets.Editor.camptr + GTA.Offsets.Editor.cam_zoom).SetFloat(ischecked ? 25F : 1);
                }
                else if (issteam)
                {
                    m.memory(GTA.Offsets.Editor.camptr + GTA.Offsets.Editor.cam_mode).SetInt(ischecked ? 18 : -1);
                    m.memory(GTA.Offsets.Editor.camptr + GTA.Offsets.Editor.cam_zoom).SetFloat(ischecked ? 25F : 1);
                }
                else
                {
                    m.memory(GTA.Offsets.Editor.camptr + GTA.Offsets.Editor.cam_mode).SetInt(ischecked ? 18 : -1);
                    m.memory(GTA.Offsets.Editor.camptr + GTA.Offsets.Editor.cam_zoom).SetFloat(ischecked ? 25F : 1);
                }
            }
        }

        private void JobImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            img.Source = ((System.Windows.Controls.Image)MainImageContainer.Template.FindName("JobImage", MainImageContainer)).Source;
            img.VerticalAlignment = VerticalAlignment.Stretch;
            img.HorizontalAlignment = HorizontalAlignment.Stretch;
            img.Style = (Style)FindResource("PopupImage");

            ScreenMessageContainer.Children.Add(img);
            //IMGBackgroundSource.Source = JobImage.Source;

            ScreenMessage.Visibility = Visibility.Visible;
        }

        private void JobImage_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            img.Source = ((System.Windows.Controls.Image)MainImageContainer.Template.FindName("JobImage", MainImageContainer)).Source;
            img.VerticalAlignment = VerticalAlignment.Stretch;
            img.HorizontalAlignment = HorizontalAlignment.Stretch;
            img.Style = (Style)FindResource("PopupImage");

            ScreenMessageContainer.Children.Add(img);
            //IMGBackgroundSource.Source = JobImage.Source;

            ScreenMessage.Visibility = Visibility.Visible;
        }


        private void ddmissionteamno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckTeamSettingSection(true);

            SelectActiveTextBox();
        }

        private void cbhidecreatormenu_Checked(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                new Global(GTA.Offsets.Editor.hide_creator_menu).SetInt(cbhidecreatormenu.IsChecked == true ? 1 : 0);
            }
        }



        private void cbforce_publish_Checked(object sender, RoutedEventArgs e)
        {
            bool needscan = curcreatorscanneeded();
            if (needscan)
                GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy();

            bool ischecked = cbforce_publish.IsChecked ?? true;
            freeze = ischecked;
            if (freeze)
            {
                long addy = getCurrentCreatorBase();

                force_publishThread = new Thread(new ParameterizedThreadStart(force_publish));
                force_publishThread.Priority = ThreadPriority.Highest;
                force_publishThread.IsBackground = true;
                force_publishThread.Start(addy);
            }
            else
            {
                force_publishThread.Abort();
            }
        }

        public static void force_publish(object addy)
        {
            long addr = (long)addy + GTA.Offsets.Editor.OFFSET_current_creator_pre_publish * 8;
            while (true)
            {
                m.memory(addr.ToString("X")).SetInt(255);
                //Functions.Write.writebinarytoaddy(4, addr);
            }
        }

        private void Btnjobtypeswitch_Click(object sender, RoutedEventArgs e)
        {
            bool visible = ddjobtype.Visibility == Visibility.Visible;

            if (visible)
            {
                tbjobtype.Visibility = Visibility.Visible;
                tbjobsubtype.Visibility = Visibility.Visible;
                tbjobracetype.Visibility = Visibility.Visible;
                ddjobtype.Visibility = Visibility.Hidden;
                ddjobsubtype.Visibility = Visibility.Hidden;
            }
            else
            {
                tbjobtype.Visibility = Visibility.Hidden;
                tbjobsubtype.Visibility = Visibility.Hidden;
                tbjobracetype.Visibility = Visibility.Collapsed;
                ddjobtype.Visibility = Visibility.Visible;
                ddjobsubtype.Visibility = Visibility.Visible;
            }
        }

        private void tbjobtype_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.type).SetInt(tbjobtype.Text);
        }

        private void tbjobsubtype_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.subtype).SetInt(tbjobsubtype.Text);
        }

        private void tbjobracetype_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.racetype).SetInt(tbjobracetype.Text);
        }

        public void testJobRace()
        {
            m.memory((getCreatorScriptLocalWorkerBase() + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_menu * 8).ToString("X")).SetInt(4);
        }

        public void testJobRaceSecondary()
        {
            m.memory((getCreatorScriptLocalWorkerBase() + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_menu * 8).ToString("X")).SetInt(5);
        }

        public void saveJob()
        {
            m.memory((getCreatorScriptLocalWorkerBase() + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_refresh * 8).ToString("X")).SetInt(5);
        }

        public void publishJob()
        {
            m.memory((getCreatorScriptLocalWorkerBase() + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_refresh * 8).ToString("X")).SetInt(91);
        }

        private void BtnTestMain_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                bool needscan = curcreatorscanneeded();
                if (needscan)
                    GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy();


                if (GTA.Offsets.Editor.localptr != null)
                {
                    int index = ddteamtest.SelectedIndex;
                    switch (GTA.ReadScriptName(GTA.Offsets.Editor.localptr[0], GTA.Offsets.Editor.localptr[1]))
                    {
                        case "fm_survival_creator":
                            new Global(GTA.Offsets.Editor.current_team_test).SetInt(index);
                            Functions.Write.writebinarytoaddy(26, m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, GTA.Offsets.Editor.OFFSET_current_creator_test_survival * 8 }).GetAddress());
                            break;
                        case "fm_capture_creator":
                            new Global(GTA.Offsets.Editor.current_team_test).SetInt(index);
                            Functions.Write.writebinarytoaddy(26, m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, GTA.Offsets.Editor.OFFSET_current_creator_test_capture * 8 }).GetAddress());
                            break;
                        case "fm_mission_creator":
                        case "fm_lts_creator":
                            new Global(GTA.Offsets.Editor.current_team_test).SetInt(index);
                            Functions.Write.writebinarytoaddy(26, m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, GTA.Offsets.Editor.OFFSET_current_creator_test_lts * 8 }).GetAddress());
                            break;
                        case "fm_deathmatch_creator":
                            displayScreenMessage("currently not possible for deathmatch");
                            break;
                        case "fm_race_creator":
                            if (index == 0 || index == 2 || index == 3)
                            {
                                testJobRace();
                            }
                            else
                            {
                                testJobRaceSecondary();
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void BtnSaveMain_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                saveJob();
            }
        }

        private void BtnPublishMain_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                publishJob();
            }
        }

        private void BtnTryFixBlackScreen_Click(object sender, RoutedEventArgs e)
        {
            string addy = (getCreatorScriptLocalWorkerBase() + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_refresh * 8).ToString("X");
            m.memory(addy).SetInt(0);
            m.memory(addy).SetInt(7);
        }

        private void BtnJobImageSave_Click(object sender, RoutedEventArgs e)
        {
            ImageSource imgsrc = ((System.Windows.Controls.Image)MainImageContainer.Template.FindName("JobImage", MainImageContainer)).Source;
            if (imgsrc == null)
            {
                displayScreenMessage("there is no image to save");
                return;
            }
            try
            {
                var saveFileDialog = new SaveFileDialog()
                {
                    Filter = "Image Files (*.jpg)|*.jpg"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    // The game's own JPEG, byte for byte; re-encoding would lose quality.
                    if (lastJobImage != null)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, lastJobImage);
                        return;
                    }
                    var encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)imgsrc));
                    using (FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                        encoder.Save(stream);
                }

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
