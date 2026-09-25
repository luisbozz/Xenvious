using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xenvious.JSON;
using Xenvious.Logging;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: ModdedProps page.
    public partial class MainWindow
    {
        [System.Diagnostics.DebuggerHidden()]
        public static bool ExistsInPropList(int integer)
        {
            try
            {
                GTA.Editor.PropList.Where(x => x.Integer == integer).Select(x => x.Name).First();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void BtnModdedProps_Click(object sender, RoutedEventArgs e)
        {
            BtnNormalProps.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnDynamicProps.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];

            BtnModdedProps.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];

            PageInnerProps.SelectedItem = PageInnerModdedProps;

            // Initialise now instead of on the next 1 s timer tick, so the loading dots
            // appear when the tab opens and not only just before the list is ready.
            if (m.IsProcOpen)
                EnsureModdedPropSourcesInitialized();
        }


        private void EnsureModdedPropSourcesInitialized()
        {
            var labelMap = new Dictionary<string, TextBlock>
            {
                { "fm_race_creator", lblmpropsinitialisedrace },
                { "fm_lts_creator", lblmpropsinitialisedlts },
                { "fm_dm_creator", lblmpropsinitialiseddm },
                { "fm_capture_creator", lblmpropsinitialisedcapture },
                { "fm_survival_creator", lblmpropsinitialisedsurvival }
            };

            for (int i = 0; i < GTA.Editor.ModdedPropSources.Count; i++)
            {
                var source = GTA.Editor.ModdedPropSources[i];
                // Use the refresh result right away. Reading "available" before the refresh
                // made every creator wait one more timer tick before it could be selected.
                bool available = source.ScriptPointer != 0 || source.RefreshScriptPointer();

                if (available && labelMap.TryGetValue(source.ScriptName, out var label))
                {
                    if (ddMPROPSCreator.SelectedIndex == -1)
                    {
                        ddMPROPSCreator.SelectedIndex = i;
                    }
                    setlabelsinitialized(label);
                }

                var existingItem = ddMPROPSCreator.Items
                    .OfType<ComboBoxItem>()
                    .FirstOrDefault(item => string.Equals(item.Tag as string, source.ScriptName, StringComparison.OrdinalIgnoreCase));

                if (existingItem == null)
                {
                    existingItem = new ComboBoxItem
                    {
                        Tag = source.ScriptName,
                        Content = source.DisplayName
                    };
                    ddMPROPSCreator.Items.Insert(Math.Min(i, ddMPROPSCreator.Items.Count), existingItem);
                }
                else
                {
                    existingItem.Tag = source.ScriptName;
                    existingItem.Content = source.DisplayName;
                    int currentIndex = ddMPROPSCreator.Items.IndexOf(existingItem);
                    if (currentIndex != i)
                    {
                        ddMPROPSCreator.Items.RemoveAt(currentIndex);
                        ddMPROPSCreator.Items.Insert(Math.Min(i, ddMPROPSCreator.Items.Count), existingItem);
                    }
                }

                if (existingItem != null)
                {
                    existingItem.IsEnabled = available;
                }
            }
        }

        public void setlabelsinitialized(TextBlock lbl)
        {
            lbl.Text = "Initialized";
            lbl.Foreground = new SolidColorBrush(Colors.Green);
        }

        public enum PropCategory : int
        {
            mp_barrier = 0,
            mp_banks = 1,
            mp_boje = 2,
            mp_cabins = 3,
            mp_bags = 4,
            mp_container = 5,
            mp_crates = 6,
            mp_trash_container = 7,
            mp_machinery = 8,
            mp_ramps = 9,
            mp_signs = 10,
            mp_trailer = 11,
            mp_wrecks = 12,
            mp_trees = 13,
            mp_dynamics = 14,
            mp_special = 15,
            mp_hidden = 16,
            mp_stunt_ramps = 17,
            mp_stunt_building_blocks = 18,
            mp_stunt_set_pieces = 19,
            mp_stunt_special = 20,
            mp_stunt_targets_assault = 21,
            mp_stunt_signs = 22,
            mp_stunt_bis_neon_arrows = 23,
            mp_stunt_tracks = 24,
            mp_stunt_tracks_wb = 25,
            mp_stunt_tracks_high = 26,
            mp_stunt_barriers = 27,
            mp_stunt_tubes = 28,
            mp_drugs = 29,
            mp_gunrunning = 30,
            mp_stunt_tubes_neon = 31,
            mp_stunt_neon_blocks = 32,
            mp_stunt_targets = 33,
            mp_stunt_air_tubes = 34,
            mp_stunt_checkpoint_rings = 35,
            mp_stunt_air_gates = 36,
            mp_stunt_inflateable_gates = 37,
            mp_race_buildings = 38,
            mp_hidden2 = 39,
            mp_hidden3 = 50,
            mp_hidden4 = 48,
            mp_cctv = 49,
            mp_paintedsigns = 51,
            mp_holidays = 52,
            mp_tracksmoothing = 53,
            mp_precision = 54,
            mp_hidden5 = 55,
            mp_hidden6 = 59,
            mp_hidden7 = 60,
        }

        public class PropC : INotifyPropertyChanged
        {
            private List<GTA.MPEntry> _prop;
            private int _category;
            public List<GTA.MPEntry> prop
            {
                get => _prop;
                set
                {
                    _prop = value;
                    OnPropertyChanged();
                }
            }
            public int category
            {
                get => _category;
                set
                {
                    _category = value;
                    OnPropertyChanged();
                }
            }

            public PropC(List<GTA.MPEntry> prop, int category)
            {
                this._prop = prop;
                this._category = category;
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                if (this.PropertyChanged != null)
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        List<PropC> allprops = new List<PropC>();
        private static readonly byte[] MPropsPlaceholder = new byte[] { 0x2E, 0x02, 0x01, 0x28 };

        // Checked against all five creators on both builds: the first run of 3 or more
        // placeholders 7-8 bytes apart is the table start in every case, and still is at 8.
        private const int MPropsRunLength = 4;

        private static bool PlaceholderAt(byte[] buffer, int at)
        {
            if (at < 0 || at + MPropsPlaceholder.Length > buffer.Length)
                return false;
            for (int k = 0; k < MPropsPlaceholder.Length; k++)
                if (buffer[at + k] != MPropsPlaceholder[k])
                    return false;
            return true;
        }

        private static int IndexOfPlaceholderRun(byte[] buffer, int runLength)
        {
            for (int i = IndexOfPattern(buffer, MPropsPlaceholder); i >= 0; i = IndexOfPattern(buffer, MPropsPlaceholder, i + 1))
            {
                // Entries are normally 8 bytes apart, occasionally 7.
                int n = 1, at = i;
                while (n < runLength)
                {
                    if (PlaceholderAt(buffer, at + 8)) at += 8;
                    else if (PlaceholderAt(buffer, at + 7)) at += 7;
                    else break;
                    n++;
                }
                if (n >= runLength)
                    return i;
            }
            return -1;
        }
        private void UpdateModdedPropEntry(GTA.MPEntry entry, int value)
        {
            if (entry == null)
            {
                return;
            }

            entry.IntegerValue = value;

            var propInfo = GTA.Editor.PropList.FirstOrDefault(x => x.Integer == value);
            entry.Name = propInfo?.Name ?? string.Empty;
        }




        public async Task<bool> loadMProps(int index)
        {
            await Task.Run(() =>
            {
                const string logSource = "mprops.load";
                var totalTimer = Stopwatch.StartNew();

                void Abort(string message)
                {
                    totalTimer.Stop();
                    Log.Debug($"{message} (elapsed {totalTimer.ElapsedMilliseconds} ms)", source: logSource);
                    allprops = new List<PropC>();
                }

                var sources = GTA.Editor.ModdedPropSources;
                if (index < 0 || index >= sources.Count)
                {
                    Abort($"loadMProps aborted: index {index} out of range (sources: {sources.Count})");
                    return;
                }

                var source = sources[index];

                if (!source.RefreshScriptPointer())
                {
                    Abort($"loadMProps aborted: script pointer not found for {source.DisplayName}");
                    return;
                }

                Log.Debug($"loadMProps start for {source.DisplayName} (index {index})", source: logSource);

                ulong dataRegion;
                if (source.DataRegion.HasValue)
                {
                    dataRegion = source.DataRegion.Value;
                }
                else
                {
                    if (!TryLocateModdedPropDataRegion(source, logSource, out dataRegion))
                    {
                        Abort($"loadMProps aborted: data region not found for {source.DisplayName}");
                        return;
                    }
                }

                byte[] buffer;
                var readTimer = Stopwatch.StartNew();
                try
                {
                    buffer = m.memory(dataRegion.ToString("X")).GetBytes(50000);
                }
                catch (Exception ex)
                {
                    readTimer.Stop();
                    Abort($"loadMProps aborted while reading buffer for {source.DisplayName}: {ex.GetType().Name}");
                    return;
                }
                readTimer.Stop();
                Log.Debug($"Read {buffer.Length} bytes for {source.DisplayName} in {readTimer.ElapsedMilliseconds} ms", source: logSource);

                var dataRegions = new ulong?[sources.Count];
                dataRegions[index] = dataRegion;
                int resolvedRegions = 1;

                var regionTimer = Stopwatch.StartNew();
                for (int i = 0; i < sources.Count; i++)
                {
                    if (i == index)
                    {
                        continue;
                    }

                    if (sources[i].DataRegion.HasValue)
                    {
                        dataRegions[i] = sources[i].DataRegion;
                        resolvedRegions++;
                    }
                }
                regionTimer.Stop();

                Log.Debug($"Resolved {resolvedRegions} creator region(s) in {regionTimer.ElapsedMilliseconds} ms", source: logSource);

                var categories = new List<PropC>();
                var currentCategory = new List<GTA.MPEntry>();
                int previousPlaceholderOffset = -MPropsPlaceholder.Length;
                int placeholderMatches = 0;
                var propNameCache = new Dictionary<int, string>();
                var propList = GTA.Editor.PropList;
                Dictionary<int, string> propLookup = null;
                if (propList != null)
                {
                    try
                    {
                        propLookup = propList
                        .GroupBy(x => x.Integer)
                        .Select(g => g.First())
                        .ToDictionary(x => x.Integer, x => x.Name);
                    }
                    catch
                    {
                        propLookup = null;
                    }
                }

                var parseTimer = Stopwatch.StartNew();

                for (int offset = IndexOfPattern(buffer, MPropsPlaceholder); offset >= 0; offset = IndexOfPattern(buffer, MPropsPlaceholder, offset + MPropsPlaceholder.Length))
                {
                    if (offset < 4)
                    {
                        continue;
                    }

                    if (offset - previousPlaceholderOffset > 40 && currentCategory.Count > 0)
                    {
                        categories.Add(new PropC(currentCategory, categories.Count));
                        currentCategory = new List<GTA.MPEntry>();
                    }

                    previousPlaceholderOffset = offset;
                    placeholderMatches++;

                    int propId = BitConverter.ToInt32(buffer, offset - 4);
                    int propOffset = offset - 4;

                    var addresses = new List<string>(sources.Count);
                    for (int i = 0; i < sources.Count; i++)
                    {
                        var region = dataRegions[i];
                        addresses.Add(region.HasValue ? (region.Value + (ulong)propOffset).ToString("X") : string.Empty);
                    }

                    if (!propNameCache.TryGetValue(propId, out string propName))
                    {
                        if (propLookup != null && propLookup.TryGetValue(propId, out var lookupName))
                        {
                            propName = lookupName?.Trim();
                        }

                        if (string.IsNullOrEmpty(propName))
                        {
                            propName = string.Empty;
                        }

                        propNameCache[propId] = propName;
                    }

                    string hexValue = propId.ToString("X8");
                    currentCategory.Add(new GTA.MPEntry(propName, addresses, propId, hexValue));
                }

                if (currentCategory.Count > 0)
                {
                    categories.Add(new PropC(currentCategory, categories.Count));
                }

                parseTimer.Stop();
                Log.Debug($"Processed {placeholderMatches} placeholder entries in {parseTimer.ElapsedMilliseconds} ms", source: logSource);

                allprops = categories;

                totalTimer.Stop();
                int totalProps = categories.Sum(c => c.prop.Count);
                Log.Debug($"loadMProps complete for {source.DisplayName}: categories={categories.Count}, props={totalProps}, total time={totalTimer.ElapsedMilliseconds} ms", source: logSource);
            });

            ddMPropsReplaceCategory.IsEnabled = true;
            ddMPropsReplaceCategory.SelectedIndex = 0;
            mpropsloadanimation.IsEnabled = false;
            mpropsloadanimation.Visibility = Visibility.Collapsed;
            mpropspanelmainmain.Visibility = Visibility.Visible;
            return true;
        }
        private bool TryLocateModdedPropDataRegion(ModdedPropSource source, string logSource, out ulong dataRegion)
        {
            var locateTimer = Stopwatch.StartNew();
            dataRegion = 0;

            var codePages = ScrProgramScanner.GetScrProgramByteCodeRegion(source.ScriptPointer);
            if (codePages == null || codePages.Count == 0)
            {
                locateTimer.Stop();
                Log.Debug($"Failed to read code pages for {source.DisplayName}", source: logSource);
                return false;
            }

            foreach (var (pagePtr, pageSize) in codePages)
            {
                byte[] pageBytes;
                try
                {
                    pageBytes = m.memory(pagePtr.ToString("X")).GetBytes(pageSize);
                }
                catch
                {
                    continue;
                }

                // The table is a run of "PUSH_CONST_U32 <hash>; LEAVE 2,1" entries, one every
                // 8 bytes. The placeholder alone also occurs in ordinary code -- in
                // fm_race_creator eleven times before the table, which put the region 165 KB
                // too early and filled the list with CALL operands. Those stray matches stand
                // alone; the table is the first place where they follow each other. That is
                // structure, not a value, so it still holds after the props were edited.
                int idx = IndexOfPlaceholderRun(pageBytes, MPropsRunLength);
                if (idx >= 0)
                {
                    if (idx < 4 && pagePtr < (ulong)(4 - idx))
                    {
                        continue;
                    }

                    ulong candidateAddress = idx >= 4
                        ? pagePtr + (ulong)(idx - 4)
                        : pagePtr - (ulong)(4 - idx);

                    dataRegion = candidateAddress;
                    source.DataRegion = dataRegion;
                    locateTimer.Stop();
                    Log.Debug($"Located prop data for {source.DisplayName} at 0x{dataRegion:X} in {locateTimer.ElapsedMilliseconds} ms", source: logSource);
                    return true;
                }
            }

            locateTimer.Stop();
            Log.Debug($"Failed to locate prop data for {source.DisplayName} ({locateTimer.ElapsedMilliseconds} ms)", source: logSource);
            return false;
        }

        private static int IndexOfPattern(byte[] buffer, byte[] pattern, int startIndex = 0)
        {
            if (buffer == null || pattern == null || pattern.Length == 0 || buffer.Length < pattern.Length || startIndex < 0)
            {
                return -1;
            }

            for (int i = startIndex; i <= buffer.Length - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (buffer[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return i;
                }
            }

            return -1;
        }

        public void loadMPropClass(PropCategory category)
        {
            try
            {
                mpropModelList.ItemsSource = allprops[(int)category].prop;

                tbmpropsbulk.Text = String.Join(",", allprops[(int)category].prop.Select(x => x.HexValue));
            }
            catch (Exception)
            {
                displayScreenMessage("Something went wrong loading the selected Prop Category.");
            }
        }

        private void Btnmpropsreplace_Click(object sender, RoutedEventArgs e)
        {
            if (!m.IsProcOpen)
            {
                return;
            }

            if (cbmpbulk.IsChecked == true)
            {
                try
                {
                    var props = tbmpropsbulk.Text.Split(',').Select(Functions.int_parse).ToList();
                    int counter = 0;

                    if (mpropModelList.ItemsSource is List<GTA.MPEntry> entries)
                    {
                        foreach (var item in entries)
                        {
                            if (counter >= props.Count)
                            {
                                break;
                            }

                            try
                            {
                                int newValue = props[counter];
                                m.memory(item.Address[ddMPROPSCreator.SelectedIndex]).SetInt(newValue);
                                UpdateModdedPropEntry(item, newValue);
                            }
                            catch (Exception)
                            {
                                counter++;
                                continue;
                            }

                            counter++;
                        }
                    }
                }
                catch (Exception)
                {

                }
                return;
            }

            var entry = mpropModelList.SelectedItem as GTA.MPEntry;
            if (entry == null)
            {
                return;
            }

            if (!ModelIdParser.TryParseModelIdInt(tbmpropsmodelchange.Text, out int prop, out var kind))
            {
                return;
            }

            try
            {
                m.memory(entry.Address[ddMPROPSCreator.SelectedIndex]).SetInt(prop);
                UpdateModdedPropEntry(entry, prop);
            }
            catch (Exception)
            {

            }
        }

        public void enableMProps()
        {
            if (m.IsProcOpen)
            {
                List<string> temp_mprop_list = GTA.Editor.moddedpropson.Split(',').ToList();
                var parsedValues = temp_mprop_list.Select(Functions.int_parse).ToList();

                foreach (var source in GTA.Editor.ModdedPropSources)
                {
                    if (!source.RefreshScriptPointer())
                    {
                        continue;
                    }

                    if (!source.DataRegion.HasValue)
                    {
                        if (!TryLocateModdedPropDataRegion(source, "mprops.enable", out _))
                        {
                            continue;
                        }
                    }

                    ulong baseAddress = source.DataRegion.Value;
                    for (int d = 0; d < parsedValues.Count; d++)
                    {
                        m.memory((baseAddress + (ulong)(d * 0x8)).ToString("X")).SetInt(parsedValues[d]);
                    }
                }
            }
        }

        public void disableMProps()
        {
            if (m.IsProcOpen)
            {
                List<string> temp_mprop_list = GTA.Editor.moddedpropsoff.Split(',').ToList();
                var parsedValues = temp_mprop_list.Select(Functions.int_parse).ToList();

                foreach (var source in GTA.Editor.ModdedPropSources)
                {
                    if (!source.RefreshScriptPointer())
                    {
                        continue;
                    }

                    if (!source.DataRegion.HasValue)
                    {
                        if (!TryLocateModdedPropDataRegion(source, "mprops.disable", out _))
                        {
                            continue;
                        }
                    }

                    ulong baseAddress = source.DataRegion.Value;
                    for (int d = 0; d < parsedValues.Count; d++)
                    {
                        m.memory((baseAddress + (ulong)(d * 0x8)).ToString("X")).SetInt(parsedValues[d]);
                    }
                }
            }
        }

        private void IMGBackground_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ScreenMessage.Visibility = Visibility.Collapsed;
            ScreenMessageContainer.Children.Clear();
        }

        private void cbmpbulk_Checked(object sender, RoutedEventArgs e)
        {
            if (cbmpbulk.IsChecked ?? true)
            {
                mpropModelList.Visibility = Visibility.Hidden;
                tbmpropsmodelchange.Visibility = Visibility.Hidden;

                tbmpropsbulk.Visibility = Visibility.Visible;
            }
            else
            {
                tbmpropsbulk.Visibility = Visibility.Hidden;

                mpropModelList.Visibility = Visibility.Visible;
                tbmpropsmodelchange.Visibility = Visibility.Visible;
            }
        }

        private void ddMPropsCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddMPropsCategory.SelectedIndex > -1)
            {
                bool needscan = curcreatorscanneeded();
                if (needscan)
                    GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy();

                int category_index = GetPropCategoryIndex();

                long addy = getCurrentCreatorBase();

                m.memory((addy + GTA.Offsets.Editor.OFFSET_current_creator_pre_category_num * 8).ToString("X")).SetInt(category_index);
            }
        }

        public int GetPropCategoryIndex()
        {
            switch (ddMPropsCategory.SelectedIndex)
            {
                case 0:
                    return (int)GTA.Editor.SpecialPropCategorys.Special;
                case 1:
                    return (int)GTA.Editor.SpecialPropCategorys.Targets;
                case 2:
                    return (int)GTA.Editor.SpecialPropCategorys.Drugs;
                case 3:
                    return (int)GTA.Editor.SpecialPropCategorys.Gunrunning;
                case 4:
                    return (int)GTA.Editor.SpecialPropCategorys.LandingPlaces;
                case 5:
                    return (int)GTA.Editor.SpecialPropCategorys.Hidden;
                case 6:
                    return (int)GTA.Editor.SpecialPropCategorys.Hidden2;
                case 7:
                    return (int)GTA.Editor.SpecialPropCategorys.Hidden3;
                case 8:
                    return (int)GTA.Editor.SpecialPropCategorys.Hidden4;
                case 9:
                    return (int)GTA.Editor.SpecialPropCategorys.Hidden5;
                case 10:
                    return (int)GTA.Editor.SpecialPropCategorys.Custom;
                case 11:
                    return (int)GTA.Editor.SpecialPropCategorys.Templates;
                default:
                    return 0;
            }
        }

        private void cbMPropsForceMurica_Checked(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                new Global(GTA.Offsets.Editor.enable_murica).SetInt(cbMPropsForceMurica.IsChecked == true ? 1 : 0);
            }
        }

        private void mpropModelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string addy = ((GTA.MPEntry)mpropModelList.SelectedItem).Address[ddMPROPSCreator.SelectedIndex];
                int value = m.memory(addy).Get<int>();

                tbmpropsmodelchange.Text = value.ToString();
            }
            catch (Exception)
            {

            }
        }

        public void preparemoddedpropjsontoread(ref string json)
        {
            json = json.Replace("[", "{").Replace("]", "}");
        }
        public void preparemoddedpropjsontoexport(ref string json)
        {
            json = json.Replace("{", "[").Replace("}", "]");
        }

        private void ddMPropsReplaceCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (ddMPropsReplaceCategory.SelectedIndex)
            {
                case 0:
                    loadMPropClass(PropCategory.mp_barrier);
                    break;
                case 1:
                    loadMPropClass(PropCategory.mp_banks);
                    break;
                case 2:
                    loadMPropClass(PropCategory.mp_boje);
                    break;
                case 3:
                    loadMPropClass(PropCategory.mp_cabins);
                    break;
                case 4:
                    loadMPropClass(PropCategory.mp_bags);
                    break;
                case 5:
                    loadMPropClass(PropCategory.mp_container);
                    break;
                case 6:
                    loadMPropClass(PropCategory.mp_crates);
                    break;
                case 7:
                    loadMPropClass(PropCategory.mp_trash_container);
                    break;
                case 8:
                    loadMPropClass(PropCategory.mp_machinery);
                    break;
                case 9:
                    loadMPropClass(PropCategory.mp_ramps);
                    break;
                case 10:
                    loadMPropClass(PropCategory.mp_signs);
                    break;
                case 11:
                    loadMPropClass(PropCategory.mp_trailer);
                    break;
                case 12:
                    loadMPropClass(PropCategory.mp_wrecks);
                    break;
                case 13:
                    loadMPropClass(PropCategory.mp_trees);
                    break;
                case 14:
                    loadMPropClass(PropCategory.mp_dynamics);
                    break;
                case 15:
                    loadMPropClass(PropCategory.mp_special);
                    break;
                case 16:
                    loadMPropClass(PropCategory.mp_hidden);
                    break;
                case 17:
                    loadMPropClass(PropCategory.mp_stunt_tracks);
                    break;
                case 18:
                    loadMPropClass(PropCategory.mp_stunt_tracks_wb);
                    break;
                case 19:
                    loadMPropClass(PropCategory.mp_stunt_tracks_high);
                    break;
                case 20:
                    loadMPropClass(PropCategory.mp_stunt_barriers);
                    break;
                case 21:
                    loadMPropClass(PropCategory.mp_stunt_tubes);
                    break;
                case 22:
                    loadMPropClass(PropCategory.mp_stunt_tubes_neon);
                    break;
                case 23:
                    loadMPropClass(PropCategory.mp_stunt_bis_neon_arrows);
                    break;
                case 24:
                    loadMPropClass(PropCategory.mp_stunt_air_tubes);
                    break;
                case 25:
                    loadMPropClass(PropCategory.mp_stunt_checkpoint_rings);
                    break;
                case 26:
                    loadMPropClass(PropCategory.mp_stunt_air_gates);
                    break;
                case 27:
                    loadMPropClass(PropCategory.mp_stunt_inflateable_gates);
                    break;
                case 28:
                    loadMPropClass(PropCategory.mp_stunt_building_blocks);
                    break;
                case 29:
                    loadMPropClass(PropCategory.mp_stunt_neon_blocks);
                    break;
                case 30:
                    loadMPropClass(PropCategory.mp_stunt_ramps);
                    break;
                case 31:
                    loadMPropClass(PropCategory.mp_stunt_set_pieces);
                    break;
                case 32:
                    loadMPropClass(PropCategory.mp_stunt_signs);
                    break;
                case 33:
                    loadMPropClass(PropCategory.mp_stunt_special);
                    break;
                case 34:
                    loadMPropClass(PropCategory.mp_stunt_targets);
                    break;
                case 35:
                    loadMPropClass(PropCategory.mp_stunt_targets_assault);
                    break;
                case 36:
                    loadMPropClass(PropCategory.mp_race_buildings);
                    break;
                case 37:
                    loadMPropClass(PropCategory.mp_drugs);
                    break;
                case 38:
                    loadMPropClass(PropCategory.mp_gunrunning);
                    break;
                case 39:
                    loadMPropClass(PropCategory.mp_hidden2);
                    break;
                case 40:
                    loadMPropClass(PropCategory.mp_hidden3);
                    break;
                case 41:
                    loadMPropClass(PropCategory.mp_hidden4);
                    break;
                case 42:
                    loadMPropClass(PropCategory.mp_cctv);
                    break;
                case 43:
                    loadMPropClass(PropCategory.mp_paintedsigns);
                    break;
                case 44:
                    loadMPropClass(PropCategory.mp_holidays);
                    break;
                case 45:
                    loadMPropClass(PropCategory.mp_tracksmoothing);
                    break;
                case 46:
                    loadMPropClass(PropCategory.mp_precision);
                    break;
                case 47:
                    loadMPropClass(PropCategory.mp_hidden5);
                    break;
                case 48:
                    loadMPropClass(PropCategory.mp_hidden6);
                    break;
                case 49:
                    loadMPropClass(PropCategory.mp_hidden6);
                    break;
                default:
                    break;
            }
        }

        private async void ddMPROPSCreator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            //while (!loadmpropsthreadfinished) ;


            mpropsloadanimation.IsEnabled = true;
            mpropsloadanimation.Visibility = Visibility.Visible;
            mpropspanelmainmain.Visibility = Visibility.Collapsed;

            await loadMProps(ddMPROPSCreator.SelectedIndex);
        }

        private void Btnmpropsreplaceall_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                if (cbmpbulk.IsChecked == false)
                {
                    try
                    {
                        var entry = mpropModelList.SelectedItem as GTA.MPEntry;
                        int prop = Functions.int_parse(tbmpropsmodelchange.Text);
                        if (entry != null && IsValidInt(prop.ToString()))
                        {
                            foreach (var address in entry.Address.Where(addr => !string.IsNullOrWhiteSpace(addr)))
                            {
                                m.memory(address).SetInt(prop);
                            }
                            UpdateModdedPropEntry(entry, prop);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (cbmpbulk.IsChecked == true)
                {
                    try
                    {
                        var props = tbmpropsbulk.Text.Split(',').Select(Functions.int_parse).ToList();
                        int counter = 0;
                        foreach (var entry in (List<GTA.MPEntry>)mpropModelList.ItemsSource)
                        {
                            if (counter >= props.Count)
                            {
                                break;
                            }

                            int newValue = props[counter];

                            try
                            {
                                foreach (var address in entry.Address.Where(addr => !string.IsNullOrWhiteSpace(addr)))
                                {
                                    m.memory(address).SetInt(newValue);
                                }

                                UpdateModdedPropEntry(entry, newValue);
                            }
                            catch (Exception)
                            {
                                counter++;
                                continue;
                            }

                            counter++;
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        private void Btnmpropsimport_Click(object sender, RoutedEventArgs e)
        {
            mpropsdrop.Visibility = Visibility.Visible;
        }

        private void Btnmpropsexport_Click(object sender, RoutedEventArgs e)
        {
            JSON.ModdedPropJSON.Rootobject exportmpropjsonobj = new JSON.ModdedPropJSON.Rootobject();

            var temp = allprops[0].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s0 = string.Join(",", temp);
            temp = allprops[1].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s1 = string.Join(",", temp);
            temp = allprops[2].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s2 = string.Join(",", temp);
            temp = allprops[3].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s3 = string.Join(",", temp);
            temp = allprops[4].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s4 = string.Join(",", temp);
            temp = allprops[5].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s5 = string.Join(",", temp);
            temp = allprops[6].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s6 = string.Join(",", temp);
            temp = allprops[7].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s7 = string.Join(",", temp);
            temp = allprops[8].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s8 = string.Join(",", temp);
            temp = allprops[9].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s9 = string.Join(",", temp);
            temp = allprops[10].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s10 = string.Join(",", temp);
            temp = allprops[11].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s11 = string.Join(",", temp);
            temp = allprops[12].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s12 = string.Join(",", temp);
            temp = allprops[14].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s13 = string.Join(",", temp);
            temp = allprops[13].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s14 = string.Join(",", temp);
            temp = allprops[15].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s15 = string.Join(",", temp);
            temp = allprops[16].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s16 = string.Join(",", temp);
            temp = allprops[17].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s17 = string.Join(",", temp);
            temp = allprops[18].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s18 = string.Join(",", temp);
            temp = allprops[19].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s19 = string.Join(",", temp);
            temp = allprops[20].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s20 = string.Join(",", temp);
            temp = allprops[21].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s21 = string.Join(",", temp);
            temp = allprops[22].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s22 = string.Join(",", temp);
            temp = allprops[24].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s23 = string.Join(",", temp);
            temp = allprops[23].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s24 = string.Join(",", temp);
            temp = allprops[25].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s25 = string.Join(",", temp);
            temp = allprops[26].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s26 = string.Join(",", temp);
            temp = allprops[27].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s27 = string.Join(",", temp);
            temp = allprops[28].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s28 = string.Join(",", temp);
            temp = allprops[29].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s29 = string.Join(",", temp);
            temp = allprops[30].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s30 = string.Join(",", temp);
            temp = allprops[31].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s31 = string.Join(",", temp);
            temp = allprops[32].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s32 = string.Join(",", temp);
            temp = allprops[34].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s33 = string.Join(",", temp);
            temp = allprops[33].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s34 = string.Join(",", temp);
            temp = allprops[35].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s35 = string.Join(",", temp);
            temp = allprops[36].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s36 = string.Join(",", temp);
            temp = allprops[37].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s37 = string.Join(",", temp);
            temp = allprops[38].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s38 = string.Join(",", temp);
            temp = allprops[39].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s39 = string.Join(",", temp);
            temp = allprops[40].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s40 = string.Join(",", temp);
            temp = allprops[41].prop.Select(x => m.memory(x.Address[ddMPROPSCreator.SelectedIndex]).Get<int>().ToString("X8")).ToList();
            exportmpropjsonobj.s41 = string.Join(",", temp);

            string jsontoexport = JsonConvert.SerializeObject(exportmpropjsonobj);
            preparemoddedpropjsontoexport(ref jsontoexport);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Select any path to safe the file";
            saveFileDialog.Filter = "Custom Prop File (*.cprp) | *.cprp";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            Nullable<bool> result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                File.WriteAllText(saveFileDialog.FileName, jsontoexport);
            }
        }

        JSON.ModdedPropJSON.Rootobject importedmpropjsonobj = null;
        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Select any Propfile";
            ofd.Filter = "Custom Prop File (*.cprp) | *.cprp";
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ofd.Multiselect = false;

            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = ofd.ShowDialog();
            // Get the selected file name and display in a TextBox.
            // Load content of file in a TextBlock

            if (result == true)
            {
                // get content from file
                string content = File.ReadAllText(ofd.FileName);
                // make json deserializeable
                preparemoddedpropjsontoread(ref content);
                // read json into json obj
                importedmpropjsonobj = JsonConvert.DeserializeObject<JSON.ModdedPropJSON.Rootobject>(content);
                // initialize objects for interaction
                initializempropsimport();
                ddmpropsimport.SelectedIndex = 0;
            }
        }

        private void Btnmpropsclose_Click(object sender, RoutedEventArgs e)
        {
            mpropsdrop.Visibility = Visibility.Collapsed;
        }

        private void Btnmpropsimportreplace_Click(object sender, RoutedEventArgs e)
        {
            replaceimportedProps(false);
        }

        private void Btnmpropsimportreplaceall_Click(object sender, RoutedEventArgs e)
        {
            replaceimportedProps(true);
        }

        public void replaceimportedProps(bool all)
        {
            switch (ddmpropsimport.SelectedIndex)
            {
                case 0:
                    writeCategory(importedmpropjsonobj.s0, (int)PropCategory.mp_barrier, all);
                    writeCategory(importedmpropjsonobj.s1, (int)PropCategory.mp_banks, all);
                    writeCategory(importedmpropjsonobj.s2, (int)PropCategory.mp_boje, all);
                    writeCategory(importedmpropjsonobj.s3, (int)PropCategory.mp_cabins, all);
                    writeCategory(importedmpropjsonobj.s4, (int)PropCategory.mp_bags, all);
                    writeCategory(importedmpropjsonobj.s5, (int)PropCategory.mp_container, all);
                    writeCategory(importedmpropjsonobj.s6, (int)PropCategory.mp_crates, all);
                    writeCategory(importedmpropjsonobj.s7, (int)PropCategory.mp_trash_container, all);
                    writeCategory(importedmpropjsonobj.s8, (int)PropCategory.mp_machinery, all);
                    writeCategory(importedmpropjsonobj.s9, (int)PropCategory.mp_ramps, all);
                    writeCategory(importedmpropjsonobj.s10, (int)PropCategory.mp_signs, all);
                    writeCategory(importedmpropjsonobj.s11, (int)PropCategory.mp_trailer, all);
                    writeCategory(importedmpropjsonobj.s12, (int)PropCategory.mp_wrecks, all);
                    writeCategory(importedmpropjsonobj.s13, (int)PropCategory.mp_dynamics, all);
                    writeCategory(importedmpropjsonobj.s14, (int)PropCategory.mp_trees, all);
                    writeCategory(importedmpropjsonobj.s15, (int)PropCategory.mp_special, all);
                    writeCategory(importedmpropjsonobj.s16, (int)PropCategory.mp_hidden, all);
                    writeCategory(importedmpropjsonobj.s17, (int)PropCategory.mp_stunt_ramps, all);
                    writeCategory(importedmpropjsonobj.s18, (int)PropCategory.mp_stunt_building_blocks, all);
                    writeCategory(importedmpropjsonobj.s19, (int)PropCategory.mp_stunt_set_pieces, all);
                    writeCategory(importedmpropjsonobj.s20, (int)PropCategory.mp_stunt_special, all);
                    writeCategory(importedmpropjsonobj.s21, (int)PropCategory.mp_stunt_targets_assault, all);
                    writeCategory(importedmpropjsonobj.s22, (int)PropCategory.mp_stunt_signs, all);
                    writeCategory(importedmpropjsonobj.s23, (int)PropCategory.mp_stunt_bis_neon_arrows, all);
                    writeCategory(importedmpropjsonobj.s24, (int)PropCategory.mp_stunt_tracks, all);
                    writeCategory(importedmpropjsonobj.s25, (int)PropCategory.mp_stunt_tracks_wb, all);
                    writeCategory(importedmpropjsonobj.s26, (int)PropCategory.mp_stunt_tracks_high, all);
                    writeCategory(importedmpropjsonobj.s27, (int)PropCategory.mp_stunt_barriers, all);
                    writeCategory(importedmpropjsonobj.s28, (int)PropCategory.mp_stunt_tubes, all);
                    writeCategory(importedmpropjsonobj.s29, (int)PropCategory.mp_drugs, all);
                    writeCategory(importedmpropjsonobj.s30, (int)PropCategory.mp_gunrunning, all);
                    writeCategory(importedmpropjsonobj.s31, (int)PropCategory.mp_stunt_tubes_neon, all);
                    writeCategory(importedmpropjsonobj.s32, (int)PropCategory.mp_stunt_neon_blocks, all);
                    writeCategory(importedmpropjsonobj.s33, (int)PropCategory.mp_stunt_targets, all);
                    writeCategory(importedmpropjsonobj.s34, (int)PropCategory.mp_stunt_air_tubes, all);
                    writeCategory(importedmpropjsonobj.s35, (int)PropCategory.mp_stunt_checkpoint_rings, all);
                    writeCategory(importedmpropjsonobj.s36, (int)PropCategory.mp_stunt_air_gates, all);
                    writeCategory(importedmpropjsonobj.s37, (int)PropCategory.mp_stunt_inflateable_gates, all);
                    writeCategory(importedmpropjsonobj.s38, (int)PropCategory.mp_race_buildings, all);
                    writeCategory(importedmpropjsonobj.s39, (int)PropCategory.mp_hidden2, all);
                    writeCategory(importedmpropjsonobj.s40, (int)PropCategory.mp_hidden3, all);
                    writeCategory(importedmpropjsonobj.s41, (int)PropCategory.mp_hidden4, all);
                    break;
                case 1:
                    writeCategory(importedmpropjsonobj.s0, (int)PropCategory.mp_barrier, all);
                    break;
                case 2:
                    writeCategory(importedmpropjsonobj.s1, (int)PropCategory.mp_banks, all);
                    break;
                case 3:
                    writeCategory(importedmpropjsonobj.s2, (int)PropCategory.mp_boje, all);
                    break;
                case 4:
                    writeCategory(importedmpropjsonobj.s3, (int)PropCategory.mp_cabins, all);
                    break;
                case 5:
                    writeCategory(importedmpropjsonobj.s4, (int)PropCategory.mp_bags, all);
                    break;
                case 6:
                    writeCategory(importedmpropjsonobj.s5, (int)PropCategory.mp_container, all);
                    break;
                case 7:
                    writeCategory(importedmpropjsonobj.s6, (int)PropCategory.mp_crates, all);
                    break;
                case 8:
                    writeCategory(importedmpropjsonobj.s7, (int)PropCategory.mp_trash_container, all);
                    break;
                case 9:
                    writeCategory(importedmpropjsonobj.s8, (int)PropCategory.mp_machinery, all);
                    break;
                case 10:
                    writeCategory(importedmpropjsonobj.s9, (int)PropCategory.mp_ramps, all);
                    break;
                case 11:
                    writeCategory(importedmpropjsonobj.s10, (int)PropCategory.mp_signs, all);
                    break;
                case 12:
                    writeCategory(importedmpropjsonobj.s11, (int)PropCategory.mp_trailer, all);
                    break;
                case 13:
                    writeCategory(importedmpropjsonobj.s12, (int)PropCategory.mp_wrecks, all);
                    break;
                case 14:
                    writeCategory(importedmpropjsonobj.s13, (int)PropCategory.mp_dynamics, all);
                    break;
                case 15:
                    writeCategory(importedmpropjsonobj.s14, (int)PropCategory.mp_trees, all);
                    break;
                case 16:
                    writeCategory(importedmpropjsonobj.s15, (int)PropCategory.mp_stunt_special, all);
                    break;
                case 17:
                    writeCategory(importedmpropjsonobj.s16, (int)PropCategory.mp_hidden, all);
                    break;
                case 18:
                    writeCategory(importedmpropjsonobj.s17, (int)PropCategory.mp_stunt_ramps, all);
                    break;
                case 19:
                    writeCategory(importedmpropjsonobj.s18, (int)PropCategory.mp_stunt_building_blocks, all);
                    break;
                case 20:
                    writeCategory(importedmpropjsonobj.s19, (int)PropCategory.mp_stunt_set_pieces, all);
                    break;
                case 21:
                    writeCategory(importedmpropjsonobj.s20, (int)PropCategory.mp_stunt_special, all);
                    break;
                case 22:
                    writeCategory(importedmpropjsonobj.s21, (int)PropCategory.mp_stunt_targets_assault, all);
                    break;
                case 23:
                    writeCategory(importedmpropjsonobj.s22, (int)PropCategory.mp_stunt_signs, all);
                    break;
                case 24:
                    writeCategory(importedmpropjsonobj.s23, (int)PropCategory.mp_stunt_bis_neon_arrows, all);
                    break;
                case 25:
                    writeCategory(importedmpropjsonobj.s24, (int)PropCategory.mp_stunt_tracks, all);
                    break;
                case 26:
                    writeCategory(importedmpropjsonobj.s25, (int)PropCategory.mp_stunt_tracks_wb, all);
                    break;
                case 27:
                    writeCategory(importedmpropjsonobj.s26, (int)PropCategory.mp_stunt_tracks_high, all);
                    break;
                case 28:
                    writeCategory(importedmpropjsonobj.s27, (int)PropCategory.mp_stunt_barriers, all);
                    break;
                case 29:
                    writeCategory(importedmpropjsonobj.s28, (int)PropCategory.mp_stunt_tubes, all);
                    break;
                case 30:
                    writeCategory(importedmpropjsonobj.s29, (int)PropCategory.mp_drugs, all);
                    break;
                case 31:
                    writeCategory(importedmpropjsonobj.s30, (int)PropCategory.mp_gunrunning, all);
                    break;
                case 32:
                    writeCategory(importedmpropjsonobj.s31, (int)PropCategory.mp_stunt_tubes_neon, all);
                    break;
                case 33:
                    writeCategory(importedmpropjsonobj.s32, (int)PropCategory.mp_stunt_neon_blocks, all);
                    break;
                case 34:
                    writeCategory(importedmpropjsonobj.s33, (int)PropCategory.mp_stunt_targets, all);
                    break;
                case 35:
                    writeCategory(importedmpropjsonobj.s34, (int)PropCategory.mp_stunt_air_tubes, all);
                    break;
                case 36:
                    writeCategory(importedmpropjsonobj.s35, (int)PropCategory.mp_stunt_checkpoint_rings, all);
                    break;
                case 37:
                    writeCategory(importedmpropjsonobj.s36, (int)PropCategory.mp_stunt_air_gates, all);
                    break;
                case 38:
                    writeCategory(importedmpropjsonobj.s37, (int)PropCategory.mp_stunt_inflateable_gates, all);
                    break;
                case 39:
                    writeCategory(importedmpropjsonobj.s38, (int)PropCategory.mp_race_buildings, all);
                    break;
                case 40:
                    writeCategory(importedmpropjsonobj.s39, (int)PropCategory.mp_hidden2, all);
                    break;
                case 41:
                    writeCategory(importedmpropjsonobj.s40, (int)PropCategory.mp_hidden3, all);
                    break;
                case 42:
                    writeCategory(importedmpropjsonobj.s41, (int)PropCategory.mp_hidden4, all);
                    break;
                default:
                    break;
            }
        }

        public void writeCategory(string basecategoryproplist, int category, bool all)
        {
            if (string.IsNullOrWhiteSpace(basecategoryproplist))
            {
                return;
            }
            //get all props from the list and convert them to int
            var props = basecategoryproplist.Split(',').Select(x => Functions.int_parse(x)).ToList();
            var proplist = allprops[category].prop;
            int counter = 0;

            counter = props.Count > proplist.Count ? proplist.Count : props.Count;

            if (all)
            {
                for (int i = 0; i < counter; i++)
                {
                    foreach (var address in proplist[i].Address.Where(addr => !string.IsNullOrWhiteSpace(addr)))
                    {
                        m.memory(address).SetInt(props[i]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < counter; i++)
                {
                    m.memory(proplist[i].Address[ddMPROPSCreator.SelectedIndex]).SetInt(props[i]);
                }
            }
        }

        private void Btnmpropscycle_Click(object sender, RoutedEventArgs e)
        {
            resetmpropsimport();
        }

        public void initializempropsimport()
        {
            ddmpropsimport.IsEnabled = true;
            Btnmpropsimportreplace.IsEnabled = true;
            Btnmpropsimportreplaceall.IsEnabled = true;
            Lblmpropsimportinitialize.Visibility = Visibility.Hidden;
            tbmpropsimport.Visibility = Visibility.Visible;
        }

        public void resetmpropsimport()
        {
            ddmpropsimport.IsEnabled = false;
            Btnmpropsimportreplace.IsEnabled = false;
            Btnmpropsimportreplaceall.IsEnabled = false;
            Lblmpropsimportinitialize.Visibility = Visibility.Visible;
            tbmpropsimport.Visibility = Visibility.Hidden;
            importedmpropjsonobj = null;
        }

        private void ddmpropsimport_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddmpropsimport.SelectedIndex == 0)
            {
                tbmpropsimport.Text = "all props from categorys";
            }
            else if (ddmpropsimport.SelectedIndex > 0)
            {
                switch (ddmpropsimport.SelectedIndex)
                {
                    case 1:
                        tbmpropsimport.Text = importedmpropjsonobj.s0;
                        break;
                    case 2:
                        tbmpropsimport.Text = importedmpropjsonobj.s1;
                        break;
                    case 3:
                        tbmpropsimport.Text = importedmpropjsonobj.s2;
                        break;
                    case 4:
                        tbmpropsimport.Text = importedmpropjsonobj.s3;
                        break;
                    case 5:
                        tbmpropsimport.Text = importedmpropjsonobj.s4;
                        break;
                    case 6:
                        tbmpropsimport.Text = importedmpropjsonobj.s5;
                        break;
                    case 7:
                        tbmpropsimport.Text = importedmpropjsonobj.s6;
                        break;
                    case 8:
                        tbmpropsimport.Text = importedmpropjsonobj.s7;
                        break;
                    case 9:
                        tbmpropsimport.Text = importedmpropjsonobj.s8;
                        break;
                    case 10:
                        tbmpropsimport.Text = importedmpropjsonobj.s9;
                        break;
                    case 11:
                        tbmpropsimport.Text = importedmpropjsonobj.s10;
                        break;
                    case 12:
                        tbmpropsimport.Text = importedmpropjsonobj.s11;
                        break;
                    case 13:
                        tbmpropsimport.Text = importedmpropjsonobj.s12;
                        break;
                    case 14:
                        tbmpropsimport.Text = importedmpropjsonobj.s13;
                        break;
                    case 15:
                        tbmpropsimport.Text = importedmpropjsonobj.s14;
                        break;
                    case 16:
                        tbmpropsimport.Text = importedmpropjsonobj.s15;
                        break;
                    case 17:
                        tbmpropsimport.Text = importedmpropjsonobj.s16;
                        break;
                    case 18:
                        tbmpropsimport.Text = importedmpropjsonobj.s17;
                        break;
                    case 19:
                        tbmpropsimport.Text = importedmpropjsonobj.s18;
                        break;
                    case 20:
                        tbmpropsimport.Text = importedmpropjsonobj.s19;
                        break;
                    case 21:
                        tbmpropsimport.Text = importedmpropjsonobj.s20;
                        break;
                    case 22:
                        tbmpropsimport.Text = importedmpropjsonobj.s21;
                        break;
                    case 23:
                        tbmpropsimport.Text = importedmpropjsonobj.s22;
                        break;
                    case 24:
                        tbmpropsimport.Text = importedmpropjsonobj.s23;
                        break;
                    case 25:
                        tbmpropsimport.Text = importedmpropjsonobj.s24;
                        break;
                    case 26:
                        tbmpropsimport.Text = importedmpropjsonobj.s25;
                        break;
                    case 27:
                        tbmpropsimport.Text = importedmpropjsonobj.s26;
                        break;
                    case 28:
                        tbmpropsimport.Text = importedmpropjsonobj.s27;
                        break;
                    case 29:
                        tbmpropsimport.Text = importedmpropjsonobj.s28;
                        break;
                    case 30:
                        tbmpropsimport.Text = importedmpropjsonobj.s29;
                        break;
                    case 31:
                        tbmpropsimport.Text = importedmpropjsonobj.s30;
                        break;
                    case 32:
                        tbmpropsimport.Text = importedmpropjsonobj.s31;
                        break;
                    case 33:
                        tbmpropsimport.Text = importedmpropjsonobj.s32;
                        break;
                    case 34:
                        tbmpropsimport.Text = importedmpropjsonobj.s33;
                        break;
                    case 35:
                        tbmpropsimport.Text = importedmpropjsonobj.s34;
                        break;
                    case 36:
                        tbmpropsimport.Text = importedmpropjsonobj.s35;
                        break;
                    case 37:
                        tbmpropsimport.Text = importedmpropjsonobj.s36;
                        break;
                    case 38:
                        tbmpropsimport.Text = importedmpropjsonobj.s37;
                        break;
                    case 39:
                        tbmpropsimport.Text = importedmpropjsonobj.s38;
                        break;
                    case 40:
                        tbmpropsimport.Text = importedmpropjsonobj.s39;
                        break;
                    case 41:
                        tbmpropsimport.Text = importedmpropjsonobj.s40;
                        break;
                    case 42:
                        tbmpropsimport.Text = importedmpropjsonobj.s41;
                        break;
                    default:
                        break;
                }
            }
        }

        private void tbmpropsimport_TextChanged(object sender, TextChangedEventArgs e)
        {
            switch (ddmpropsimport.SelectedIndex)
            {
                case 1:
                    importedmpropjsonobj.s0 = tbmpropsimport.Text;
                    break;
                case 2:
                    importedmpropjsonobj.s1 = tbmpropsimport.Text;
                    break;
                case 3:
                    importedmpropjsonobj.s2 = tbmpropsimport.Text;
                    break;
                case 4:
                    importedmpropjsonobj.s3 = tbmpropsimport.Text;
                    break;
                case 5:
                    importedmpropjsonobj.s4 = tbmpropsimport.Text;
                    break;
                case 6:
                    importedmpropjsonobj.s5 = tbmpropsimport.Text;
                    break;
                case 7:
                    importedmpropjsonobj.s6 = tbmpropsimport.Text;
                    break;
                case 8:
                    importedmpropjsonobj.s7 = tbmpropsimport.Text;
                    break;
                case 9:
                    importedmpropjsonobj.s8 = tbmpropsimport.Text;
                    break;
                case 10:
                    importedmpropjsonobj.s9 = tbmpropsimport.Text;
                    break;
                case 11:
                    importedmpropjsonobj.s10 = tbmpropsimport.Text;
                    break;
                case 12:
                    importedmpropjsonobj.s11 = tbmpropsimport.Text;
                    break;
                case 13:
                    importedmpropjsonobj.s12 = tbmpropsimport.Text;
                    break;
                case 14:
                    importedmpropjsonobj.s13 = tbmpropsimport.Text;
                    break;
                case 15:
                    importedmpropjsonobj.s14 = tbmpropsimport.Text;
                    break;
                case 16:
                    importedmpropjsonobj.s15 = tbmpropsimport.Text;
                    break;
                case 17:
                    importedmpropjsonobj.s16 = tbmpropsimport.Text;
                    break;
                case 18:
                    importedmpropjsonobj.s17 = tbmpropsimport.Text;
                    break;
                case 19:
                    importedmpropjsonobj.s18 = tbmpropsimport.Text;
                    break;
                case 20:
                    importedmpropjsonobj.s19 = tbmpropsimport.Text;
                    break;
                case 21:
                    importedmpropjsonobj.s20 = tbmpropsimport.Text;
                    break;
                case 22:
                    importedmpropjsonobj.s21 = tbmpropsimport.Text;
                    break;
                case 23:
                    importedmpropjsonobj.s22 = tbmpropsimport.Text;
                    break;
                case 24:
                    importedmpropjsonobj.s23 = tbmpropsimport.Text;
                    break;
                case 25:
                    importedmpropjsonobj.s24 = tbmpropsimport.Text;
                    break;
                case 26:
                    importedmpropjsonobj.s25 = tbmpropsimport.Text;
                    break;
                case 27:
                    importedmpropjsonobj.s26 = tbmpropsimport.Text;
                    break;
                case 28:
                    importedmpropjsonobj.s27 = tbmpropsimport.Text;
                    break;
                case 29:
                    importedmpropjsonobj.s28 = tbmpropsimport.Text;
                    break;
                case 30:
                    importedmpropjsonobj.s29 = tbmpropsimport.Text;
                    break;
                case 31:
                    importedmpropjsonobj.s30 = tbmpropsimport.Text;
                    break;
                case 32:
                    importedmpropjsonobj.s31 = tbmpropsimport.Text;
                    break;
                case 33:
                    importedmpropjsonobj.s32 = tbmpropsimport.Text;
                    break;
                case 34:
                    importedmpropjsonobj.s33 = tbmpropsimport.Text;
                    break;
                case 35:
                    importedmpropjsonobj.s34 = tbmpropsimport.Text;
                    break;
                case 36:
                    importedmpropjsonobj.s35 = tbmpropsimport.Text;
                    break;
                case 37:
                    importedmpropjsonobj.s36 = tbmpropsimport.Text;
                    break;
                case 38:
                    importedmpropjsonobj.s37 = tbmpropsimport.Text;
                    break;
                case 39:
                    importedmpropjsonobj.s38 = tbmpropsimport.Text;
                    break;
                case 40:
                    importedmpropjsonobj.s39 = tbmpropsimport.Text;
                    break;
                case 41:
                    importedmpropjsonobj.s40 = tbmpropsimport.Text;
                    break;
                case 42:
                    importedmpropjsonobj.s41 = tbmpropsimport.Text;
                    break;
                default:
                    break;
            }
        }

        private void Btnmpropsrestore_Click(object sender, RoutedEventArgs e)
        {


            List<string> tempmainarr = new List<string>();

            switch ((ddMPROPSCreator.SelectedItem as ComboBoxItem).Content.ToString())
            {
                case "Race":
                    tempmainarr = GTA.Defaults.MPropsDefaultsRace;
                    break;
                case "LTS":
                    tempmainarr = GTA.Defaults.MPropsDefaultsLTS;
                    break;
                case "Capture":
                    tempmainarr = GTA.Defaults.MPropsDefaultsCapture;
                    break;
                case "Deathmatch":
                    tempmainarr = GTA.Defaults.MPropsDefaultsDM;
                    break;
                case "Survival":
                    tempmainarr = GTA.Defaults.MPropsDefaultsSurvival;
                    break;
                default:
                    break;
            }


            for (int i = 0; i < tempmainarr.Count; i++)
            {
                try
                {
                    var temparr = tempmainarr[i].Split(',');
                    for (int d = 0; d < allprops[i].prop.Count; d++)
                    {
                        try
                        {
                            m.memory(allprops[i].prop[d].Address[ddMPROPSCreator.SelectedIndex]).SetInt(Functions.int_parse(temparr[d]));
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        private void tbmpropsmodelchange_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //GTA.MPEntry proptemp = ((GTA.MPEntry)mpropModelList.SelectedItem);
            //Clipboard.SetText(proptemp.Address[ddMPROPSCreator.SelectedIndex]);
        }
    }
}
