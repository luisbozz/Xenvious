using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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

        public void preparemoddedpropjsontoread(ref string json)
        {
            json = json.Replace("[", "{").Replace("]", "}");
        }
        public void preparemoddedpropjsontoexport(ref string json)
        {
            json = json.Replace("{", "[").Replace("}", "]");
        }

        // ----- page (mockup: https://claude.ai/artifact/UHkkxoYy3ySGxcsLxh87vq) -----
        //
        // Left the categories of the prop menu, in the middle the slots of the chosen category as
        // tiles, on the right the editor for one slot, import and the creator options. Writes go
        // to the chosen creator or, with "all loaded creators", to every creator whose table is
        // known (MPEntry.Address holds one address per source).

        private sealed class MPCategory
        {
            public string Key, Fallback, Group;
            public int Table;
        }

        // Order and names of the old drop-down; Table is the index in allprops (PropCategory).
        private static readonly MPCategory[] MPCategories = BuildMPCategories();

        private static MPCategory[] BuildMPCategories()
        {
            var list = new List<MPCategory>();
            void Add(string group, string key, string fallback, PropCategory table) => list.Add(new MPCategory { Group = group, Key = key, Fallback = fallback, Table = (int)table });
            Add("std", "mp_barrier", "Barriers", PropCategory.mp_barrier);
            Add("std", "mp_banks", "Banks", PropCategory.mp_banks);
            Add("std", "mp_boje", "Buoys", PropCategory.mp_boje);
            Add("std", "mp_cabins", "Cabins", PropCategory.mp_cabins);
            Add("std", "mp_bags", "Bags", PropCategory.mp_bags);
            Add("std", "mp_container", "Container", PropCategory.mp_container);
            Add("std", "mp_kaesten", "Boxes", PropCategory.mp_crates);
            Add("std", "mp_trash_container", "Trash Container", PropCategory.mp_trash_container);
            Add("std", "mp_machinery", "Machinery", PropCategory.mp_machinery);
            Add("std", "mp_ramps", "Ramps", PropCategory.mp_ramps);
            Add("std", "mp_signs", "Signs", PropCategory.mp_signs);
            Add("std", "mp_trailer", "Trailer", PropCategory.mp_trailer);
            Add("std", "mp_wrecks", "Wrecks", PropCategory.mp_wrecks);
            Add("std", "mp_trees", "Trees", PropCategory.mp_trees);
            Add("std", "mp_dynamics", "Dynamics", PropCategory.mp_dynamics);
            Add("std", "mp_drugs", "Drugs", PropCategory.mp_drugs);
            Add("std", "mp_gunrunning", "Gunrunning", PropCategory.mp_gunrunning);
            Add("std", "mp_cctv", "CCTV", PropCategory.mp_cctv);
            Add("std", "mp_paintedsigns", "Painted Signs", PropCategory.mp_paintedsigns);
            Add("std", "mp_holidays", "Holidays", PropCategory.mp_holidays);
            Add("std", "mp_race_buildings", "Race Buildings", PropCategory.mp_race_buildings);
            Add("stunt", "mp_stunt_tracks", "Stunt Track", PropCategory.mp_stunt_tracks);
            Add("stunt", "mp_stunt_tracks_wb", "Stunt Track with Barriers", PropCategory.mp_stunt_tracks_wb);
            Add("stunt", "mp_stunt_tracks_high", "Stunt Raised Tracks", PropCategory.mp_stunt_tracks_high);
            Add("stunt", "mp_stunt_barriers", "Stunt Barriers", PropCategory.mp_stunt_barriers);
            Add("stunt", "mp_stunt_tubes", "Stunt Tubes", PropCategory.mp_stunt_tubes);
            Add("stunt", "mp_stunt_tubes_neon", "Stunt Neon Tubes", PropCategory.mp_stunt_tubes_neon);
            Add("stunt", "mp_stunt_arrow", "Stunt Arrows", PropCategory.mp_stunt_bis_neon_arrows);
            Add("stunt", "mp_stunt_tubes_big", "Stunt Air Tubes", PropCategory.mp_stunt_air_tubes);
            Add("stunt", "mp_stunt_cp_rings", "Stunt Checkpoint Rings", PropCategory.mp_stunt_checkpoint_rings);
            Add("stunt", "mp_stunt_air_gates", "Stunt Air Gates", PropCategory.mp_stunt_air_gates);
            Add("stunt", "mp_stunt_inflateable_gates", "Stunt Inflatable Gates", PropCategory.mp_stunt_inflateable_gates);
            Add("stunt", "mp_stunt_building_blocks", "Stunt Building Blocks", PropCategory.mp_stunt_building_blocks);
            Add("stunt", "mp_stunt_neon_blocks", "Stunt Neon Blocks", PropCategory.mp_stunt_neon_blocks);
            Add("stunt", "mp_stunt_ramps", "Stunt Ramps", PropCategory.mp_stunt_ramps);
            Add("stunt", "mp_stunt_set_pieces", "Stunt Set Pieces", PropCategory.mp_stunt_set_pieces);
            Add("stunt", "mp_stunt_signs", "Stunt Signs", PropCategory.mp_stunt_signs);
            Add("stunt", "mp_stunt_special", "Stunt Special", PropCategory.mp_stunt_special);
            Add("stunt", "mp_stunt_target", "Stunt Targets", PropCategory.mp_stunt_targets);
            Add("stunt", "mp_stunt_targets", "Target Assault", PropCategory.mp_stunt_targets_assault);
            Add("hidden", "mp_special", "Special", PropCategory.mp_special);
            Add("hidden", "mp_tracksmoothing", "Track Smoothing", PropCategory.mp_tracksmoothing);
            Add("hidden", "mp_precision", "Precision", PropCategory.mp_precision);
            int n = 1;
            foreach (var hidden in new[] { PropCategory.mp_hidden, PropCategory.mp_hidden2, PropCategory.mp_hidden3, PropCategory.mp_hidden4, PropCategory.mp_hidden5, PropCategory.mp_hidden6, PropCategory.mp_hidden7 })
                list.Add(new MPCategory { Group = "hidden", Key = "mp_hidden", Fallback = "Hidden", Table = (int)hidden, });
            foreach (var c in list.Where(c => c.Key == "mp_hidden"))
                c.Fallback = "Hidden " + n++;
            return list.ToArray();
        }

        // .cprp files: field sN holds the hashes of a table. Import and export map the fields
        // differently (13/14, 23/24, 33/34, 40, 41); both are kept exactly as they always were so
        // files written by earlier versions still come back the same way.
        private static readonly int[] CprpImportTable = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 14, 13, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 50, 48 };
        private static readonly int[] CprpExportTable = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 14, 13, 15, 16, 17, 18, 19, 20, 21, 22, 24, 23, 25, 26, 27, 28, 29, 30, 31, 32, 34, 33, 35, 36, 37, 38, 39, 40, 41 };

        private int _mpCreator = -1;
        private MPCategory _mpCategory = MPCategories[0];
        private int _mpSlot;
        private bool _mpAllCreators = true;
        private bool _mpListMode;
        private bool _mpLoading;
        private CatalogItem _mpPick;
        private string _mpCatQuery = "", _mpPickQuery = "";
        private bool _mpBuilt;

        private StackPanel _mpCreators, _mpCatList, _mpEditor, _mpImport, _mpForce;
        private WrapPanel _mpSlots;
        private TextBlock _mpCatTitle, _mpSlotHint, _mpChanged, _mpBulkInfo, _mpStatus, _mpSource;
        private TextBox _mpBulk;
        private FrameworkElement _mpListPanel;
        private StackPanel _mpModeSeg;
        private CheckBox _mpAllBox, _mpMurica;
        private JSON.ModdedPropJSON.Rootobject _mpImported;
        private StackPanel _mpSaved;
        private string _mpSaveName = "";
        private string _mpImportedName;

        private string MPT(string key, string fallback) => TranslateOr(key, fallback);

        // Set when a model was brought over from the props or dynamic props page.
        private string _mpFrom;

        // Props / Dynamic props: take the model of the selected entry over to Modded Props, so it
        // can replace a slot of the prop menu. The creator places everything from the Dynamic
        // library (PROP_LIBRARY_DYNAMIC) as a dynamic prop and everything else as a static one,
        // so a dynamic prop is sent to the Dynamics category.
        private void PropToMenu_Click(object sender, RoutedEventArgs e)
        {
            if (!m.IsProcOpen)
                return;
            bool dynamic = (sender as FrameworkElement)?.Tag as string == "dynamic";
            long model = dynamic ? GTA.Offsets.Editor.DProps.model : GTA.Offsets.Editor.Props.model;
            long next = dynamic ? GTA.Offsets.Editor.DProps.NEXT : GTA.Offsets.Editor.Props.NEXT;
            int index = dynamic ? dddpropno.SelectedIndex : ddpropno.SelectedIndex;
            if (model == 0 || index < 0)
                return;
            int hash = new Global(model + next * index).Get<int>();
            _mpPick = PropCatalog.FirstOrDefault(c => c.Int32 == hash) ?? new CatalogItem(MPHex(hash), "", unchecked((uint)hash), "", "prop");
            _mpFrom = dynamic ? "dynamic" : "static";
            if (dynamic)
                _mpCategory = MPCategories.First(c => c.Table == (int)PropCategory.mp_dynamics);
            _mpSlot = 0;
            BtnModdedProps_Click(sender, e);
            RenderMPAll();
        }

        private void BtnModdedProps_Click(object sender, RoutedEventArgs e)
        {
            BtnNormalProps.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnDynamicProps.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnModdedProps.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            PageInnerProps.SelectedItem = PageInnerModdedProps;
            BuildModdedPropsPage();
            if (m.IsProcOpen)
                EnsureModdedPropSourcesInitialized();
        }

        /// <summary>Finds which creators have their prop table in memory; picks the first one.</summary>
        private void EnsureModdedPropSourcesInitialized()
        {
            BuildModdedPropsPage();
            var sources = GTA.Editor.ModdedPropSources;
            var loaded = new bool[sources.Count];
            for (int i = 0; i < sources.Count; i++)
                loaded[i] = sources[i].ScriptPointer != 0 || sources[i].RefreshScriptPointer();
            string key = string.Concat(loaded.Select(l => l ? "1" : "0"));
            if (!Equals(_mpCreators.Tag, key))
            {
                _mpCreators.Tag = key;
                RenderMPCreators(loaded);
            }
            if (_mpCreator < 0 || _mpCreator >= loaded.Length || !loaded[_mpCreator])
            {
                int first = Array.IndexOf(loaded, true);
                if (first >= 0 && first != _mpCreator)
                    SelectMPCreator(first);
            }
            if (_mpMurica != null && !_mpMurica.IsFocused && GTA.Offsets.Editor.enable_murica != 0)
                _mpMurica.IsChecked = new Global(GTA.Offsets.Editor.enable_murica).Get<int>() == 1;
        }

        private async void SelectMPCreator(int index)
        {
            _mpCreator = index;
            _mpLoading = true;
            RenderMPCreators(null);
            RenderMPAll();
            await loadMProps(index);
            _mpLoading = false;
            RenderMPAll();
        }

        private void BuildModdedPropsPage()
        {
            if (_mpBuilt)
                return;
            _mpBuilt = true;

            var root = new DockPanel { Margin = new Thickness(16, 12, 16, 12) };

            // Top bar: creators, "all loaded creators", import / export / restore.
            var bar = new DockPanel();
            var actions = new StackPanel { Orientation = Orientation.Horizontal };
            DockPanel.SetDock(actions, Dock.Right);
            var allRow = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0) };
            allRow.Children.Add(new TextBlock { Text = MPT("mp_allcreators", "All loaded creators"), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), Foreground = ThemeBrush("MutedTextBrush") });
            _mpAllBox = new CheckBox { Style = (Style)FindResource("FormToggle"), IsChecked = true };
            _mpAllBox.Click += (_, __) => { _mpAllCreators = _mpAllBox.IsChecked == true; RenderMPEditor(); };
            allRow.Children.Add(_mpAllBox);
            actions.Children.Add(allRow);
            actions.Children.Add(MPButton(MPT("mp_import", "Import"), (_, __) => ImportMProps(), icon: "M8,2 V10 M4.5,6.5 L8,10 L11.5,6.5 M2.5,13.5 H13.5"));
            actions.Children.Add(MPButton(MPT("mp_export", "Export"), (_, __) => ExportMProps(), icon: "M8,10 V2 M4.5,5.5 L8,2 L11.5,5.5 M2.5,13.5 H13.5"));
            actions.Children.Add(MPButton(MPT("mp_restoreall", "Restore all"), (_, __) => RestoreMProps(), icon: "M3,8 A5,5 0 1 0 4.5,4.4 M3,2.5 V5 H5.5"));
            bar.Children.Add(actions);
            _mpCreators = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            bar.Children.Add(_mpCreators);
            var barCard = new Border { Style = (Style)FindResource("DashCard"), Padding = new Thickness(12, 8, 12, 8), Margin = new Thickness(0, 0, 0, 12), Child = bar };
            DockPanel.SetDock(barCard, Dock.Top);
            root.Children.Add(barCard);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(230) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320) });

            // Categories
            var catSearch = new BareSearchBox(MPT("mp_catsearch", "Search category…")) { Margin = new Thickness(12, 4, 12, 4) };
            catSearch.Changed += q => { _mpCatQuery = q.Trim(); RenderMPCategories(); };
            _mpCatList = new StackPanel();
            var catBody = new DockPanel();
            DockPanel.SetDock(catSearch, Dock.Top);
            catBody.Children.Add(catSearch);
            catBody.Children.Add(new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = _mpCatList });
            var catCard = MPCard(MPT("mp_categories", "Categories"), catBody, null);
            grid.Children.Add(catCard);

            // Slots
            _mpCatTitle = new TextBlock();
            _mpModeSeg = new StackPanel { Orientation = Orientation.Horizontal };
            var seg = MPSegmentTrack(_mpModeSeg);
            seg.Padding = new Thickness(2);
            var slotsBody = new DockPanel();
            var hintRow = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
            _mpChanged = new TextBlock { FontWeight = FontWeights.Bold };
            _mpChanged.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
            DockPanel.SetDock(_mpChanged, Dock.Right);
            hintRow.Children.Add(_mpChanged);
            _mpSlotHint = new TextBlock { FontSize = 12 };
            _mpSlotHint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            _mpSource = new TextBlock { FontSize = 11.5, Margin = new Thickness(12, 0, 12, 0), VerticalAlignment = VerticalAlignment.Center };
            DockPanel.SetDock(_mpSource, Dock.Right);
            hintRow.Children.Add(_mpSource);
            hintRow.Children.Add(_mpSlotHint);
            DockPanel.SetDock(hintRow, Dock.Top);
            slotsBody.Children.Add(hintRow);
            _mpStatus = new TextBlock { FontSize = 13, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 8) };
            _mpStatus.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            DockPanel.SetDock(_mpStatus, Dock.Top);
            slotsBody.Children.Add(_mpStatus);
            _mpSlots = new WrapPanel();
            var listPanel = new DockPanel();
            var listHint = new TextBlock { Text = MPT("mp_list_hint", "One model per line, name or hash. The order is the order of the slots."), FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 6) };
            listHint.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            DockPanel.SetDock(listHint, Dock.Top);
            listPanel.Children.Add(listHint);
            var listFoot = new DockPanel { Margin = new Thickness(0, 8, 0, 0) };
            var apply = MPButton(MPT("mp_apply", "Apply"), (_, __) => ApplyMPList(), primary: true);
            DockPanel.SetDock(apply, Dock.Right);
            listFoot.Children.Add(apply);
            _mpBulkInfo = new TextBlock { FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
            _mpBulkInfo.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            listFoot.Children.Add(_mpBulkInfo);
            DockPanel.SetDock(listFoot, Dock.Bottom);
            listPanel.Children.Add(listFoot);
            _mpBulk = new TextBox { AcceptsReturn = true, FontFamily = new FontFamily("Consolas"), FontSize = 12.5, MinHeight = 220, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, TextWrapping = TextWrapping.NoWrap };
            _mpBulk.SetResourceReference(StyleProperty, "Watermark");
            _mpBulk.TextChanged += (_, __) => UpdateMPBulkInfo();
            listPanel.Children.Add(_mpBulk);
            _mpListPanel = listPanel;
            var slotHost = new Grid();
            slotHost.Children.Add(new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = _mpSlots });
            slotHost.Children.Add(listPanel);
            slotsBody.Children.Add(slotHost);
            var slotCard = MPCard(null, slotsBody, seg, _mpCatTitle);
            Grid.SetColumn(slotCard, 2);
            grid.Children.Add(slotCard);

            // Editor, import, creator options
            var side = new StackPanel();
            _mpEditor = new StackPanel();
            side.Children.Add(MPCard(MPT("mp_slot", "Slot"), _mpEditor, null));
            _mpSaved = new StackPanel();
            side.Children.Add(MPCard(MPT("mp_saved", "Saved lists"), _mpSaved, null));
            _mpImport = new StackPanel();
            var importCard = MPCard(MPT("mp_import", "Import"), _mpImport, null);
            importCard.Visibility = Visibility.Collapsed;
            importCard.Name = "mpImportCard";
            side.Children.Add(importCard);
            var creatorBody = new StackPanel();
            creatorBody.Children.Add(new TextBlock { Style = (Style)FindResource("FieldLabel"), Text = MPT("mp_force", "Force a category") });
            _mpForce = new StackPanel();
            creatorBody.Children.Add(_mpForce);
            var forceHint = new TextBlock { Text = MPT("mp_force_hint", "Opens a hidden category in the prop menu (e.g. Hidden, Templates)."), FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 8) };
            forceHint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            creatorBody.Children.Add(forceHint);
            var murica = new Grid { Style = (Style)FindResource("FormRow") };
            murica.ColumnDefinitions.Add(new ColumnDefinition());
            murica.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            murica.Children.Add(new TextBlock { Style = (Style)FindResource("FormLabel"), Text = MPT("mp_murica", "Unlock Murica colour") });
            _mpMurica = new CheckBox { Style = (Style)FindResource("FormToggle") };
            _mpMurica.Click += (_, __) =>
            {
                if (m.IsProcOpen && GTA.Offsets.Editor.enable_murica != 0)
                    new Global(GTA.Offsets.Editor.enable_murica).SetInt(_mpMurica.IsChecked == true ? 1 : 0);
            };
            Grid.SetColumn(_mpMurica, 1);
            murica.Children.Add(_mpMurica);
            creatorBody.Children.Add(murica);
            side.Children.Add(MPCard(MPT("mp_increator", "In the creator"), creatorBody, null));
            var sideScroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = side };
            Grid.SetColumn(sideScroll, 4);
            grid.Children.Add(sideScroll);

            root.Children.Add(grid);
            mpRoot.Children.Clear();
            mpRoot.Children.Add(root);
            RenderMPForce();
            RenderMPCreators(null);
            RenderMPAll();
        }

        private Button MPButton(string text, RoutedEventHandler click, bool primary = false, string icon = null)
        {
            object content = text;
            if (icon != null)
            {
                var panel = new StackPanel { Orientation = Orientation.Horizontal };
                var path = new System.Windows.Shapes.Path { Data = Geometry.Parse(icon), StrokeThickness = 1.6, Width = 14, Height = 14, Stretch = Stretch.Uniform, Margin = new Thickness(0, 0, 7, 0), VerticalAlignment = VerticalAlignment.Center };
                path.SetBinding(System.Windows.Shapes.Shape.StrokeProperty, new System.Windows.Data.Binding("Foreground") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.FindAncestor, typeof(Button), 1) });
                panel.Children.Add(path);
                panel.Children.Add(new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center });
                content = panel;
            }
            var b = new Button { Style = (Style)FindResource(primary ? "FormButtonPrimary" : "FormButton"), Content = content, Padding = new Thickness(12, 0, 12, 0), Margin = new Thickness(6, 0, 0, 0), MinWidth = 70 };
            b.Click += click;
            return b;
        }

        private System.Windows.Controls.Primitives.ToggleButton MPSegment(string text)
            => new System.Windows.Controls.Primitives.ToggleButton { Style = (Style)FindResource("ChoiceTile"), Content = text, Height = 28, MinHeight = 28, Padding = new Thickness(12, 0, 12, 0), Margin = new Thickness(4, 0, 0, 0), FontSize = 12.5 };

        // Segmented control of the mockup: a deep track, the chosen segment raised with an
        // accent line under it.
        private static Border MPSegmentTrack(Panel items)
        {
            var track = new Border { CornerRadius = new CornerRadius(7), Padding = new Thickness(3), BorderThickness = new Thickness(1), Child = items, VerticalAlignment = VerticalAlignment.Center };
            track.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
            track.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            return track;
        }

        private static Border MPSegmentItem(object content, bool selected, bool enabled, Action click, string tooltip = null)
        {
            var text = content as FrameworkElement ?? new TextBlock { Text = content as string, FontWeight = FontWeights.Bold, FontSize = 13 };
            if (text is TextBlock tb)
                tb.SetResourceReference(TextBlock.ForegroundProperty, selected ? "TextColor" : "MutedTextBrush");
            var item = new Border { CornerRadius = new CornerRadius(5), Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 0, 3, 0), Child = text, ToolTip = tooltip,
                Cursor = enabled ? Cursors.Hand : Cursors.Arrow, Opacity = enabled ? 1 : 0.45, BorderThickness = new Thickness(0, 0, 0, selected ? 2 : 0) };
            item.SetResourceReference(Border.BackgroundProperty, selected ? "SectionBackgroundBrush" : "DeepBrush");
            item.SetResourceReference(Border.BorderBrushProperty, "AccentBrush");
            if (enabled)
                item.MouseLeftButtonUp += (_, __) => click();
            return item;
        }

        private Border MPCard(string title, FrameworkElement body, FrameworkElement right, TextBlock titleBlock = null)
        {
            var header = new DockPanel();
            if (right != null)
            {
                DockPanel.SetDock(right, Dock.Right);
                header.Children.Add(right);
            }
            var t = titleBlock ?? new TextBlock();
            t.Style = (Style)FindResource("DashCardTitle");
            t.VerticalAlignment = VerticalAlignment.Center;
            if (title != null)
                t.Text = title;
            header.Children.Add(t);
            var dock = new DockPanel();
            header.Height = 30;
            header.LastChildFill = true;
            var head = new Border { Style = (Style)FindResource("DashCardHeader"), Child = header };
            DockPanel.SetDock(head, Dock.Top);
            dock.Children.Add(head);
            body.Margin = new Thickness(12, 10, 12, 10);
            dock.Children.Add(body);
            return new Border { Style = (Style)FindResource("DashCard"), Margin = new Thickness(0, 0, 0, 12), Child = dock, VerticalAlignment = VerticalAlignment.Top };
        }

        // A cube while there is no picture (none found, or still loading); the picture covers it
        // once the catalogue has it (CatalogItem.Thumb loads on first use and notifies).
        private static FrameworkElement MPThumb(CatalogItem item, double iconSize)
        {
            var grid = new Grid();
            var cube = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M4,8 L12,4 L20,8 L20,16 L12,20 L4,16 Z M4,8 L12,12 L20,8 M12,12 L12,20"),
                StrokeThickness = 1.3, Width = iconSize, Height = iconSize, Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
            };
            cube.SetResourceReference(System.Windows.Shapes.Shape.StrokeProperty, "FaintTextBrush");
            grid.Children.Add(cube);
            if (item != null)
            {
                var image = new System.Windows.Controls.Image { Stretch = Stretch.Uniform };
                image.SetBinding(System.Windows.Controls.Image.SourceProperty, new System.Windows.Data.Binding(nameof(CatalogItem.Thumb)) { Source = item });
                grid.Children.Add(image);
            }
            return grid;
        }

        private void RenderMPAll()
        {
            RenderMPCategories();
            RenderMPSlots();
            RenderMPEditor();
            RenderMPImport();
            RenderMPSaved();
        }

        // ----- saved lists: the models of one category under a name, applied with one click -----

        private sealed class MPSavedList
        {
            public string Name { get; set; }
            public int Table { get; set; }
            public List<string> Hashes { get; set; }
        }

        private const string MPSavedConfigKey = "mplists";

        private static List<MPSavedList> LoadMPSaved()
        {
            try
            {
                string stored = new ini_reader(Functions.getRoamingConfigFilePath()).ReadString("Settings", MPSavedConfigKey);
                if (!string.IsNullOrEmpty(stored))
                    return JsonConvert.DeserializeObject<List<MPSavedList>>(ConfigText.Decode(stored)) ?? new List<MPSavedList>();
            }
            catch (Exception ex)
            {
                Log.Error("Reading the saved prop lists failed", ex, source: "mprops");
            }
            return new List<MPSavedList>();
        }

        private static void StoreMPSaved(List<MPSavedList> lists)
            => new ini_reader(Functions.getRoamingConfigFilePath()).Write("Settings", MPSavedConfigKey, ConfigText.Encode(JsonConvert.SerializeObject(lists)));

        private void RenderMPSaved()
        {
            if (_mpSaved == null)
                return;
            _mpSaved.Children.Clear();
            var lists = LoadMPSaved();
            var slots = MPSlots(_mpCategory);

            var save = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
            var name = new BareSearchBox(string.Format(CultureInfo.CurrentCulture, MPT("mp_save_hint", "Name for {0}…"), MPCategoryName(_mpCategory)));
            name.Box.Text = _mpSaveName;
            var button = MPButton(MPT("mp_save", "Save"), (_, __) =>
            {
                string n = name.Text.Trim();
                if (n.Length == 0 || slots.Count == 0)
                    return;
                lists.RemoveAll(l => l.Table == _mpCategory.Table && string.Equals(l.Name, n, StringComparison.CurrentCultureIgnoreCase));
                lists.Add(new MPSavedList { Name = n, Table = _mpCategory.Table, Hashes = slots.Select(x => x.IntegerValue.ToString("X8", CultureInfo.InvariantCulture)).ToList() });
                StoreMPSaved(lists);
                _mpSaveName = "";
                RenderMPSaved();
            }, icon: "M3,2 H11 L13,4 V14 H3 Z M5,2 V6 H10 V2 M5,14 V9 H11 V14");
            button.IsEnabled = slots.Count > 0;
            name.Changed += t => _mpSaveName = t;
            DockPanel.SetDock(button, Dock.Right);
            save.Children.Add(button);
            save.Children.Add(name);
            _mpSaved.Children.Add(save);

            if (lists.Count == 0)
            {
                var none = new TextBlock { Text = MPT("mp_saved_none", "Save the models of a category to put them back with one click."), FontSize = 12, TextWrapping = TextWrapping.Wrap };
                none.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                _mpSaved.Children.Add(none);
                return;
            }
            foreach (var list in lists.OrderBy(l => l.Table == _mpCategory.Table ? 0 : 1).ThenBy(l => l.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                var entry = list;
                var category = MPCategories.FirstOrDefault(c => c.Table == entry.Table);
                var row = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
                var remove = MPButton("✕", (_, __) => { lists.Remove(entry); StoreMPSaved(lists); RenderMPSaved(); });
                remove.MinWidth = 0;
                remove.ToolTip = MPT("mp_delete", "Delete");
                DockPanel.SetDock(remove, Dock.Right);
                row.Children.Add(remove);
                var apply = MPButton(MPT("mp_apply", "Apply"), (_, __) =>
                {
                    writeCategory(string.Join(",", entry.Hashes), entry.Table, _mpAllCreators);
                    if (category != null)
                        _mpCategory = category;
                    RenderMPAll();
                }, primary: true);
                apply.IsEnabled = entry.Table < allprops.Count;
                DockPanel.SetDock(apply, Dock.Right);
                row.Children.Add(apply);
                var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                text.Children.Add(new TextBlock { Text = entry.Name, FontWeight = FontWeights.Bold, TextTrimming = TextTrimming.CharacterEllipsis });
                var sub = new TextBlock { Text = (category != null ? MPCategoryName(category) : "#" + entry.Table) + " · " + string.Format(CultureInfo.CurrentCulture, MPT("mp_props_n", "{0} props"), entry.Hashes.Count), FontSize = 11.5 };
                sub.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                text.Children.Add(sub);
                row.Children.Add(text);
                _mpSaved.Children.Add(row);
            }
        }

        private void RenderMPCreators(bool[] loaded)
        {
            if (_mpCreators == null)
                return;
            var sources = GTA.Editor.ModdedPropSources;
            if (loaded == null)
            {
                string key = _mpCreators.Tag as string ?? "";
                loaded = Enumerable.Range(0, sources.Count).Select(i => i < key.Length && key[i] == '1').ToArray();
            }
            _mpCreators.Children.Clear();
            var items = new StackPanel { Orientation = Orientation.Horizontal };
            for (int i = 0; i < sources.Count; i++)
            {
                int index = i;
                var content = new StackPanel { Orientation = Orientation.Horizontal };
                var dot = new System.Windows.Shapes.Ellipse { Width = 7, Height = 7, Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center };
                dot.SetResourceReference(System.Windows.Shapes.Shape.FillProperty, loaded[i] ? "OkBrush" : "FaintTextBrush");
                content.Children.Add(dot);
                var name = new TextBlock { Text = sources[i].DisplayName, FontWeight = FontWeights.Bold, FontSize = 13 };
                name.SetResourceReference(TextBlock.ForegroundProperty, i == _mpCreator ? "TextColor" : "MutedTextBrush");
                content.Children.Add(name);
                items.Children.Add(MPSegmentItem(content, i == _mpCreator, loaded[i], () => { if (index != _mpCreator) SelectMPCreator(index); },
                    loaded[i] ? MPT("mp_tableloaded", "Prop table loaded") : MPT("mp_notloaded", "Creator not loaded")));
            }
            var label = new TextBlock { Text = MPT("mp_creator", "Creator"), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0), FontSize = 12 };
            label.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            _mpCreators.Children.Add(label);
            _mpCreators.Children.Add(MPSegmentTrack(items));
        }

        private List<GTA.MPEntry> MPSlots(MPCategory category)
            => category != null && category.Table < allprops.Count ? allprops[category.Table].prop : new List<GTA.MPEntry>();

        // Originals per creator script: OfflineData/<edition>/mprops.json, made from the decompiled
        // scripts (ysc-global-updater tools/mprops_originals.py). Null entries: not a constant.
        private static Dictionary<string, List<int?[]>> _mpOriginals;
        private static string _mpOriginalsBuild;

        private static Dictionary<string, List<int?[]>> MPOriginals()
        {
            if (_mpOriginals != null)
                return _mpOriginals;
            _mpOriginals = new Dictionary<string, List<int?[]>>();
            try
            {
                var root = JObject.Parse(OfflineData.ModdedPropOriginals);
                _mpOriginalsBuild = (string)root["build"];
                foreach (var creator in (JObject)root["creators"])
                    _mpOriginals[creator.Key] = creator.Value.Select(g => g.Select(h => h.Type == JTokenType.Null ? (int?)null
                        : unchecked((int)uint.Parse((string)h, NumberStyles.HexNumber, CultureInfo.InvariantCulture))).ToArray()).ToList();
            }
            catch (Exception ex)
            {
                Log.Error("Reading the prop originals failed", ex, source: "mprops");
            }
            return _mpOriginals;
        }

        // The originals only count when the table in the game has the same shape as the one the
        // data was made from; a different build shows no "changed" at all instead of wrong ones.
        private bool MPOriginalsMatch(out List<int?[]> table)
        {
            table = null;
            if (_mpCreator < 0 || allprops.Count == 0 || !MPOriginals().TryGetValue(GTA.Editor.ModdedPropSources[_mpCreator].ScriptName, out table))
                return false;
            if (table.Count != allprops.Count)
                return false;
            for (int i = 0; i < table.Count; i++)
                if (table[i].Length != allprops[i].prop.Count)
                    return false;
            return true;
        }

        private int?[] MPDefaults(int table)
            => MPOriginalsMatch(out var data) && table < data.Count ? data[table] : null;

        private int MPChangedCount(MPCategory category)
        {
            var slots = MPSlots(category);
            var defaults = MPDefaults(category.Table);
            if (defaults == null)
                return 0;
            int n = 0;
            for (int i = 0; i < slots.Count && i < defaults.Length; i++)
                if (defaults[i].HasValue && slots[i].IntegerValue != defaults[i])
                    n++;
            return n;
        }

        private void RenderMPCategories()
        {
            if (_mpCatList == null)
                return;
            _mpCatList.Children.Clear();
            foreach (var group in new[] { ("std", "mp_grp_std", "Standard"), ("stunt", "mp_grp_stunt", "Stunt"), ("hidden", "mp_grp_hidden", "Hidden") })
            {
                var items = MPCategories.Where(c => c.Group == group.Item1 && MPCategoryName(c).IndexOf(_mpCatQuery, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList();
                if (items.Count == 0)
                    continue;
                var head = new TextBlock { Text = MPT(group.Item2, group.Item3).ToUpper(CultureInfo.CurrentCulture), FontSize = 11, FontWeight = FontWeights.Bold, Margin = new Thickness(12, 10, 12, 4) };
                head.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                _mpCatList.Children.Add(head);
                foreach (var c in items)
                {
                    var category = c;
                    int changed = allprops.Count > 0 ? MPChangedCount(c) : 0;
                    int count = MPSlots(c).Count;
                    // Name left, slot count right; changed categories get an amber badge with the number.
                    var row = new Grid { Width = 150 };
                    row.ColumnDefinitions.Add(new ColumnDefinition());
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    row.Children.Add(new TextBlock { Text = MPCategoryName(c), TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center });
                    FrameworkElement right;
                    if (changed > 0)
                    {
                        var n = new TextBlock { Text = string.Format(CultureInfo.CurrentCulture, MPT("mp_changed_n", "{0} changed"), changed), FontSize = 11, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) };
                        n.SetResourceReference(TextBlock.ForegroundProperty, "WarnBrush");
                        right = n;
                    }
                    else
                    {
                        var t = new TextBlock { Text = count > 0 ? count.ToString(CultureInfo.CurrentCulture) : "", FontSize = 11.5, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) };
                        t.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                        right = t;
                    }
                    Grid.SetColumn(right, 1);
                    row.Children.Add(right);
                    var button = new Button { Style = (Style)FindResource("SideNavButton"), Content = row, Tag = c == _mpCategory ? "active" : null };
                    button.Click += (_, __) => { _mpCategory = category; _mpSlot = 0; if (_mpFrom == null) _mpPick = null; RenderMPAll(); };
                    _mpCatList.Children.Add(button);
                }
            }
        }

        private string MPCategoryName(MPCategory c) => c.Key == "mp_hidden" ? MPT("mp_hidden", "Hidden") + " " + c.Fallback.Substring(7) : MPT(c.Key, c.Fallback);

        private string MPModelName(int hash)
        {
            var item = PropCatalog.FirstOrDefault(c => c.Int32 == hash);
            if (item != null)
                return item.Name;
            var info = GTA.Editor.PropList?.FirstOrDefault(p => p.Integer == hash);
            return string.IsNullOrWhiteSpace(info?.Name) ? "0x" + hash.ToString("X8", CultureInfo.InvariantCulture) : info.Name;
        }

        // The model name the game knows (prop_...), for lists; the hash when there is none.
        private string MPNativeName(int hash)
        {
            var item = PropCatalog.FirstOrDefault(c => c.Int32 == hash);
            if (!string.IsNullOrWhiteSpace(item?.Native))
                return item.Native;
            var info = GTA.Editor.PropList?.FirstOrDefault(p => p.Integer == hash);
            return string.IsNullOrWhiteSpace(info?.Name) ? MPHex(hash) : info.Name;
        }

        // "Barriers 3, Ramps 12": where a model already is in the loaded creator's menu.
        // Models on the prop blacklist (offsets.ini [OTHER] prop_model_blacklisted), as marked on
        // the props pages too.
        private static HashSet<int> _mpBlacklist;
        private static bool MPBlacklisted(int hash)
        {
            if (_mpBlacklist == null)
            {
                var list = GTA.Editor.prop_model_blacklisted;
                if (list == null || list.Count == 0)
                    return false;
                _mpBlacklist = new HashSet<int>(list.Select(x => int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 0));
            }
            return _mpBlacklist.Contains(hash);
        }

        // A small outlined pill, as in the mockup ("changed", "Blacklist").
        private static Border MPPill(string text, string brush)
        {
            var t = new TextBlock { Text = text, FontSize = 10.5, FontWeight = FontWeights.Bold };
            t.SetResourceReference(TextBlock.ForegroundProperty, brush);
            var pill = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(6, 0, 6, 1), BorderThickness = new Thickness(1), Child = t, Margin = new Thickness(4, 0, 0, 0) };
            pill.SetResourceReference(Border.BorderBrushProperty, brush);
            pill.SetResourceReference(Border.BackgroundProperty, "SectionBackgroundBrush");
            return pill;
        }

        private string MPWhere(int hash)
        {
            var hits = new List<string>();
            foreach (var c in MPCategories)
            {
                var slots = MPSlots(c);
                for (int i = 0; i < slots.Count; i++)
                    if (slots[i].IntegerValue == hash)
                        hits.Add(MPCategoryName(c) + " " + (i + 1).ToString(CultureInfo.CurrentCulture));
            }
            return string.Join(", ", hits.Take(4)) + (hits.Count > 4 ? " …" : "");
        }

        private static string MPHex(int hash) => "0x" + hash.ToString("X8", CultureInfo.InvariantCulture);

        private void RenderMPSlots()
        {
            if (_mpSlots == null)
                return;
            _mpCatTitle.Text = MPCategoryName(_mpCategory);
            _mpModeSeg.Children.Clear();
            foreach (var (list, key, fallback) in new[] { (false, "mp_tiles", "Tiles"), (true, "mp_list", "List") })
            {
                var item = MPSegmentItem(MPT(key, fallback), _mpListMode == list, true, () => { _mpListMode = list; RenderMPSlots(); });
                item.Padding = new Thickness(10, 2, 10, 2);
                ((TextBlock)item.Child).FontSize = 12.5;
                _mpModeSeg.Children.Add(item);
            }
            _mpSlots.Children.Clear();
            var slots = MPSlots(_mpCategory);
            var defaults = MPDefaults(_mpCategory.Table);
            _mpStatus.Text = !m.IsProcOpen ? MPT("mp_nogame", "GTA is not connected.")
                : _mpCreator < 0 ? MPT("mp_nocreator", "Open a creator once so its prop table is in memory.")
                : _mpLoading ? MPT("mp_loading", "Reading the prop table…")
                : slots.Count == 0 ? MPT("mp_empty", "This category has no slots in this creator.") : "";
            _mpStatus.Visibility = _mpStatus.Text.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
            _mpSlotHint.Text = slots.Count == 0 ? "" : string.Format(CultureInfo.CurrentCulture, MPT("mp_slots_n", "{0} slots in the prop menu"), slots.Count);
            MPOriginals();
            bool match = MPOriginalsMatch(out _);
            _mpSource.Text = allprops.Count == 0 ? ""
                : match ? string.Format(CultureInfo.CurrentCulture, MPT("mp_source", "Originals: scripts {0}"), _mpOriginalsBuild)
                : string.Format(CultureInfo.CurrentCulture, MPT("mp_source_mismatch", "Originals ({0}) do not fit this game build – \"changed\" is hidden"), _mpOriginalsBuild);
            _mpSource.SetResourceReference(TextBlock.ForegroundProperty, match ? "FaintTextBrush" : "WarnBrush");
            int changed = 0, blacklisted = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                int index = i;
                var slot = slots[i];
                bool isChanged = defaults != null && i < defaults.Length && defaults[i].HasValue && slot.IntegerValue != defaults[i];
                if (isChanged)
                    changed++;
                var tile = new StackPanel { Width = 138 };
                var head = new DockPanel { Height = 18 };
                var pills = new StackPanel { Orientation = Orientation.Horizontal };
                DockPanel.SetDock(pills, Dock.Right);
                head.Children.Add(pills);
                var no = new TextBlock { Text = (i + 1).ToString(CultureInfo.CurrentCulture), FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
                no.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                head.Children.Add(no);
                if (MPBlacklisted(slot.IntegerValue))
                {
                    blacklisted++;
                    pills.Children.Add(MPPill(MPT("mp_blacklist", "Blacklist"), "BadBrush"));
                }
                if (isChanged)
                    pills.Children.Add(MPPill(MPT("mp_changed", "changed"), "WarnBrush"));
                tile.Children.Add(head);
                var item = PropCatalog.FirstOrDefault(c => c.Int32 == slot.IntegerValue);
                var thumb = new Border { Height = 80, CornerRadius = new CornerRadius(5), Margin = new Thickness(0, 4, 0, 6), ClipToBounds = true, Child = MPThumb(item, 34) };
                thumb.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
                tile.Children.Add(thumb);
                tile.Children.Add(new TextBlock { Text = MPModelName(slot.IntegerValue), FontSize = 12.5, FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis, ToolTip = MPModelName(slot.IntegerValue) });
                var sub = new TextBlock { FontSize = 11, TextTrimming = TextTrimming.CharacterEllipsis };
                sub.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                if (isChanged)
                {
                    sub.Text = MPModelName(defaults[i].Value);
                    sub.TextDecorations = TextDecorations.Strikethrough;
                }
                else
                {
                    sub.Text = MPHex(slot.IntegerValue);
                    sub.FontFamily = new FontFamily("Consolas");
                }
                tile.Children.Add(sub);
                var button = new System.Windows.Controls.Primitives.ToggleButton { Style = (Style)FindResource("ChoiceTile"), Content = tile, IsChecked = i == _mpSlot, Margin = new Thickness(0, 0, 8, 8), Padding = new Thickness(8), HorizontalContentAlignment = HorizontalAlignment.Stretch };
                button.Click += (_, __) => { _mpSlot = index; if (_mpFrom == null) _mpPick = null; RenderMPSlots(); RenderMPEditor(); };
                _mpSlots.Children.Add(button);
            }
            _mpChanged.Text = changed > 0 ? string.Format(CultureInfo.CurrentCulture, MPT("mp_changed_of", "{0} of {1} changed"), changed, slots.Count) : "";
            if (blacklisted > 0)
                _mpChanged.Text += (_mpChanged.Text.Length > 0 ? " · " : "") + string.Format(CultureInfo.CurrentCulture, MPT("mp_blacklisted_n", "{0} blacklisted"), blacklisted);
            _mpSlots.Visibility = _mpListMode ? Visibility.Collapsed : Visibility.Visible;
            _mpListPanel.Visibility = _mpListMode ? Visibility.Visible : Visibility.Collapsed;
            if (_mpListMode && !_mpBulk.IsKeyboardFocused)
                _mpBulk.Text = string.Join(Environment.NewLine, slots.Select(s => MPNativeName(s.IntegerValue)));
        }

        // A line of the list: a catalogue name or a hash (hex or decimal, see ModelIdParser).
        private bool TryParseMPLine(string line, out int hash)
        {
            hash = 0;
            line = (line ?? "").Trim();
            if (line.Length == 0)
                return false;
            var item = PropCatalog.FirstOrDefault(c => string.Equals(c.Native, line, StringComparison.OrdinalIgnoreCase))
                ?? PropCatalog.FirstOrDefault(c => string.Equals(c.Name, line, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                hash = item.Int32;
                return true;
            }
            var info = GTA.Editor.PropList?.FirstOrDefault(p => string.Equals(p.Name, line, StringComparison.OrdinalIgnoreCase));
            if (info != null)
            {
                hash = info.Integer;
                return true;
            }
            return ModelIdParser.TryParseModelIdInt(line.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? line.Substring(2) : line, out hash, out _);
        }

        private void UpdateMPBulkInfo()
        {
            var slots = MPSlots(_mpCategory);
            var lines = _mpBulk.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
            int changes = 0, bad = 0;
            for (int i = 0; i < lines.Count && i < slots.Count; i++)
            {
                if (!TryParseMPLine(lines[i], out int hash)) bad++;
                else if (hash != slots[i].IntegerValue) changes++;
            }
            int extra = Math.Max(0, lines.Count - slots.Count);
            var parts = new List<string> { string.Format(CultureInfo.CurrentCulture, MPT("mp_bulk_changes", "{0} slots change"), changes) };
            if (bad > 0) parts.Add(string.Format(CultureInfo.CurrentCulture, MPT("mp_bulk_bad", "{0} lines not understood"), bad));
            if (extra > 0) parts.Add(string.Format(CultureInfo.CurrentCulture, MPT("mp_bulk_extra", "{0} lines too many (ignored)"), extra));
            _mpBulkInfo.Text = string.Join(" · ", parts);
        }

        private void ApplyMPList()
        {
            var slots = MPSlots(_mpCategory);
            var lines = _mpBulk.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
            for (int i = 0; i < lines.Count && i < slots.Count; i++)
                if (TryParseMPLine(lines[i], out int hash) && hash != slots[i].IntegerValue)
                    WriteMPSlot(slots[i], hash);
            _mpListMode = false;
            RenderMPAll();
        }

        // Writes one slot to the chosen creator, or to every creator whose table is known.
        // Originals differ per creator, so putting them back only touches the chosen one.
        private void WriteMPSlot(GTA.MPEntry slot, int hash, bool onlyChosen = false)
        {
            if (!m.IsProcOpen || slot == null || _mpCreator < 0)
                return;
            var addresses = _mpAllCreators && !onlyChosen
                ? slot.Address.Where(a => !string.IsNullOrWhiteSpace(a))
                : new[] { slot.Address[_mpCreator] }.Where(a => !string.IsNullOrWhiteSpace(a));
            foreach (var address in addresses)
                m.memory(address).SetInt(hash);
            UpdateModdedPropEntry(slot, hash);
        }

        private void RenderMPEditor()
        {
            if (_mpEditor == null)
                return;
            _mpEditor.Children.Clear();
            var slots = MPSlots(_mpCategory);
            if (_mpSlot >= slots.Count)
            {
                _mpEditor.Children.Add(new TextBlock { Text = MPT("mp_pickslot", "Pick a slot."), Foreground = ThemeBrush("NavMutedBrush") });
                return;
            }
            var slot = slots[_mpSlot];
            var defaults = MPDefaults(_mpCategory.Table);
            int? known = defaults != null && _mpSlot < defaults.Length ? defaults[_mpSlot] : null;
            int original = known ?? slot.IntegerValue;

            FrameworkElement Box(string label, int hash)
            {
                var box = new Border { CornerRadius = new CornerRadius(6), Padding = new Thickness(8, 6, 8, 6), BorderThickness = new Thickness(1) };
                box.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
                box.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
                var p = new StackPanel();
                var l = new TextBlock { Text = label, FontSize = 11.5 };
                l.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
                p.Children.Add(l);
                p.Children.Add(new TextBlock { Text = MPModelName(hash), FontWeight = FontWeights.Bold, FontSize = 12.5, TextTrimming = TextTrimming.CharacterEllipsis, ToolTip = MPModelName(hash) });
                var h = new TextBlock { Text = MPHex(hash), FontSize = 10.5, FontFamily = new FontFamily("Consolas") };
                h.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                p.Children.Add(h);
                box.Child = p;
                return box;
            }
            var cmp = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            cmp.ColumnDefinitions.Add(new ColumnDefinition());
            cmp.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(22) });
            cmp.ColumnDefinitions.Add(new ColumnDefinition());
            cmp.Children.Add(Box(MPT("mp_original", "Original"), original));
            var arrow = new TextBlock { Text = "→", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            arrow.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            Grid.SetColumn(arrow, 1);
            cmp.Children.Add(arrow);
            var now = Box(MPT("mp_now", "Now"), _mpPick?.Int32 ?? slot.IntegerValue);
            Grid.SetColumn(now, 2);
            cmp.Children.Add(now);
            if (_mpFrom != null && _mpPick != null)
            {
                string already = MPWhere(_mpPick.Int32);
                string text = (_mpFrom == "dynamic"
                    ? MPT("mp_from_dynamic", "From Dynamic props: pick a slot in Dynamics and press Set. Only the Dynamics category places props as dynamic; in any other category the model is placed static.")
                    : MPT("mp_from_static", "From Props: pick a category and slot and press Set. In Dynamics the model would be placed as a dynamic prop."))
                    + (already.Length > 0 ? "\n" + string.Format(CultureInfo.CurrentCulture, MPT("mp_already", "Already in the menu: {0}"), already) : "");
                var note = new Border { CornerRadius = new CornerRadius(0, 6, 6, 0), BorderThickness = new Thickness(3, 0, 0, 0), Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(0, 0, 0, 10) };
                note.SetResourceReference(Border.BorderBrushProperty, "AccentBrush");
                note.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
                var nt = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, FontSize = 12.5 };
                nt.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
                note.Child = nt;
                _mpEditor.Children.Add(note);
            }
            int shown = _mpPick?.Int32 ?? slot.IntegerValue;
            if (MPBlacklisted(shown))
            {
                var warn = new Border { CornerRadius = new CornerRadius(0, 6, 6, 0), BorderThickness = new Thickness(3, 0, 0, 0), Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(0, 0, 0, 10) };
                warn.SetResourceReference(Border.BorderBrushProperty, "BadBrush");
                warn.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
                var wt = new TextBlock { Text = MPT("mp_blacklist_warn", "This model is on the prop blacklist (the same list the props pages mark)."), TextWrapping = TextWrapping.Wrap, FontSize = 12.5 };
                wt.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
                warn.Child = wt;
                _mpEditor.Children.Add(warn);
            }
            _mpEditor.Children.Add(new TextBlock { Style = (Style)FindResource("FieldLabel"), Text = string.Format(CultureInfo.CurrentCulture, MPT("mp_slot_n", "Slot {0}"), _mpSlot + 1) });
            _mpEditor.Children.Add(cmp);

            var search = new BareSearchBox(MPT("mp_modelsearch", "Search model (name or hash)…")) { Margin = new Thickness(0, 0, 0, 6) };
            search.Box.Text = _mpPickQuery;
            var list = new StackPanel();
            void Fill()
            {
                list.Children.Clear();
                string q = _mpPickQuery.Trim();
                // Without a search the prop favourites (the star in the model catalogue) are shown.
                var hits = q.Length == 0
                    ? PropCatalog.Where(c => c.IsFavorite).Take(200)
                    : PropCatalog.Where(c => c.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 || (c.Native ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                        || MPHex(c.Int32).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0).Take(60);
                foreach (var hit in hits)
                {
                    var item = hit;
                    var row = new DockPanel();
                    var img = new Border { Width = 38, Height = 28, CornerRadius = new CornerRadius(4), Margin = new Thickness(0, 0, 8, 0), ClipToBounds = true, Child = MPThumb(item, 14) };
                    img.SetResourceReference(Border.BackgroundProperty, "DeepBrush");
                    row.Children.Add(img);
                    var t = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                    var nameRow = new DockPanel();
                    if (MPBlacklisted(item.Int32))
                    {
                        var pill = MPPill(MPT("mp_blacklist", "Blacklist"), "BadBrush");
                        DockPanel.SetDock(pill, Dock.Right);
                        nameRow.Children.Add(pill);
                    }
                    nameRow.Children.Add(new TextBlock { Text = item.Name, FontSize = 12.5, TextTrimming = TextTrimming.CharacterEllipsis });
                    t.Children.Add(nameRow);
                    var h = new TextBlock { Text = string.IsNullOrWhiteSpace(item.Native) || item.Native == item.Name ? MPHex(item.Int32) : item.Native + " · " + MPHex(item.Int32), FontSize = 10.5, FontFamily = new FontFamily("Consolas"), TextTrimming = TextTrimming.CharacterEllipsis };
                    h.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                    t.Children.Add(h);
                    row.Children.Add(t);
                    var b = new Button { Style = (Style)FindResource("SideNavButton"), Content = row, HorizontalContentAlignment = HorizontalAlignment.Stretch, Tag = _mpPick == item ? "active" : null };
                    b.Click += (_, __) => { _mpPick = item; RenderMPEditor(); };
                    list.Children.Add(b);
                }
                if (list.Children.Count == 0 && q.Length == 0)
                {
                    var none = new TextBlock { Text = MPT("mp_nofav", "No prop favourites yet. Search, or mark props with the star in the model catalogue."), TextWrapping = TextWrapping.Wrap, FontSize = 12, Margin = new Thickness(8) };
                    none.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                    list.Children.Add(none);
                }
                if (list.Children.Count == 0 && TryParseMPLine(q, out int raw))
                {
                    var b = MPButton(string.Format(CultureInfo.CurrentCulture, MPT("mp_usehash", "Use {0}"), MPHex(raw)), (_, __) => { _mpPick = new CatalogItem(MPHex(raw), "", unchecked((uint)raw), "", "prop"); RenderMPEditor(); });
                    b.Margin = new Thickness(0);
                    list.Children.Add(b);
                }
            }
            search.Changed += q => { _mpPickQuery = q; Fill(); };
            _mpEditor.Children.Add(search);
            var listBorder = new Border { Height = 250, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6), Margin = new Thickness(0, 0, 0, 8), Child = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = list } };
            listBorder.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            _mpEditor.Children.Add(listBorder);
            Fill();

            var buttons = new System.Windows.Controls.Primitives.UniformGrid { Columns = 2 };
            var reset = MPButton(MPT("mp_original", "Original"), (_, __) => { WriteMPSlot(slot, original, onlyChosen: true); _mpPick = null; RenderMPAll(); });
            reset.Margin = new Thickness(0, 0, 4, 0);
            var set = MPButton(MPT("mp_set", "Set"), (_, __) => { if (_mpPick != null) { WriteMPSlot(slot, _mpPick.Int32); _mpPick = null; _mpFrom = null; RenderMPAll(); } }, primary: true);
            set.Margin = new Thickness(4, 0, 0, 0);
            set.IsEnabled = _mpPick != null;
            buttons.Children.Add(reset);
            buttons.Children.Add(set);
            _mpEditor.Children.Add(buttons);
            var sources = GTA.Editor.ModdedPropSources;
            string where = _mpAllCreators
                ? MPT("mp_where_all", "Applies to: ") + string.Join(", ", sources.Where((s, i) => slot.Address.Count > i && !string.IsNullOrWhiteSpace(slot.Address[i])).Select(s => s.DisplayName))
                : MPT("mp_where_one", "Applies only to: ") + (_mpCreator >= 0 ? sources[_mpCreator].DisplayName : "–");
            var whereText = new TextBlock { Text = where, FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) };
            whereText.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            _mpEditor.Children.Add(whereText);
        }

        private void RenderMPForce()
        {
            // Category the prop menu shows (pre_category_num), for menus the creator never offers.
            var options = new (string Key, string Fallback, GTA.Editor.SpecialPropCategorys Value)[]
            {
                ("mp_f_special", "Special", GTA.Editor.SpecialPropCategorys.Special), ("mp_f_targets", "Targets", GTA.Editor.SpecialPropCategorys.Targets),
                ("mp_f_drugs", "Drugs", GTA.Editor.SpecialPropCategorys.Drugs), ("mp_f_gunrunning", "Gunrunning", GTA.Editor.SpecialPropCategorys.Gunrunning),
                ("mp_f_landing", "Landing places", GTA.Editor.SpecialPropCategorys.LandingPlaces), ("mp_f_hidden1", "Hidden 1", GTA.Editor.SpecialPropCategorys.Hidden),
                ("mp_f_hidden2", "Hidden 2", GTA.Editor.SpecialPropCategorys.Hidden2), ("mp_f_hidden3", "Hidden 3", GTA.Editor.SpecialPropCategorys.Hidden3),
                ("mp_f_hidden4", "Hidden 4", GTA.Editor.SpecialPropCategorys.Hidden4), ("mp_f_hidden5", "Hidden 5", GTA.Editor.SpecialPropCategorys.Hidden5),
                ("mp_f_custom", "Custom", GTA.Editor.SpecialPropCategorys.Custom), ("mp_f_templates", "Templates", GTA.Editor.SpecialPropCategorys.Templates),
            };
            var grid = new System.Windows.Controls.Primitives.UniformGrid { Columns = 3, Margin = new Thickness(0, 0, -6, 4) };
            foreach (var option in options)
            {
                var o = option;
                var b = MPSegment(MPT(o.Key, o.Fallback));
                b.Margin = new Thickness(0, 0, 6, 6);
                b.Click += (_, __) =>
                {
                    foreach (var other in grid.Children.OfType<System.Windows.Controls.Primitives.ToggleButton>())
                        other.IsChecked = other == b;
                    if (!m.IsProcOpen || GTA.Offsets.Editor.OFFSET_current_creator_pre_category_num == 0)
                        return;
                    if (curcreatorscanneeded())
                        GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy();
                    long addy = getCurrentCreatorBase();
                    if (addy != 0)
                        m.memory((addy + GTA.Offsets.Editor.OFFSET_current_creator_pre_category_num * 8).ToString("X")).SetInt((int)o.Value);
                };
                grid.Children.Add(b);
            }
            _mpForce.Children.Add(grid);
        }

        // ----- import / export / restore -----

        private void ImportMProps()
        {
            var ofd = new OpenFileDialog
            {
                Title = MPT("mp_import_title", "Choose a prop file"),
                Filter = "Custom Prop File (*.cprp)|*.cprp",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            };
            if (ofd.ShowDialog() != true)
                return;
            try
            {
                string content = File.ReadAllText(ofd.FileName);
                preparemoddedpropjsontoread(ref content);
                _mpImported = JsonConvert.DeserializeObject<JSON.ModdedPropJSON.Rootobject>(content);
                _mpImportedName = Path.GetFileName(ofd.FileName);
            }
            catch (Exception ex)
            {
                Log.Error("Reading the prop file failed", ex, source: "mprops");
                _mpImported = null;
                displayScreenMessage(MPT("mp_import_bad", "The file could not be read."));
            }
            RenderMPImport();
        }

        private static string CprpField(JSON.ModdedPropJSON.Rootobject file, int n)
            => typeof(JSON.ModdedPropJSON.Rootobject).GetProperty("s" + n.ToString(CultureInfo.InvariantCulture))?.GetValue(file) as string;

        private void RenderMPImport()
        {
            if (_mpImport == null)
                return;
            _mpImport.Children.Clear();
            var owner = FindMPImportCard();
            if (_mpImported == null)
            {
                if (owner != null) owner.Visibility = Visibility.Collapsed;
                return;
            }
            if (owner != null) owner.Visibility = Visibility.Visible;
            var head = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
            var close = MPButton("✕", (_, __) => { _mpImported = null; RenderMPImport(); });
            DockPanel.SetDock(close, Dock.Right);
            head.Children.Add(close);
            head.Children.Add(new TextBlock { Text = _mpImportedName, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis });
            _mpImport.Children.Add(head);
            int total = 0;
            for (int n = 0; n < CprpImportTable.Length; n++)
            {
                string field = CprpField(_mpImported, n);
                if (string.IsNullOrWhiteSpace(field))
                    continue;
                int count = field.Split(',').Length;
                total += count;
                var category = MPCategories.FirstOrDefault(c => c.Table == CprpImportTable[n]);
                var row = new DockPanel { Margin = new Thickness(0, 0, 0, 4) };
                int table = CprpImportTable[n];
                string hashes = field;
                // One category on its own, like the category choice of the old import.
                var only = MPButton(MPT("mp_import_one", "Only this"), (_, __) => { writeCategory(hashes, table, _mpAllCreators); RenderMPAll(); });
                only.IsEnabled = table < allprops.Count;
                only.Height = 24;
                DockPanel.SetDock(only, Dock.Right);
                row.Children.Add(only);
                var c1 = new TextBlock { Text = string.Format(CultureInfo.CurrentCulture, MPT("mp_props_n", "{0} props"), count), FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
                c1.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
                DockPanel.SetDock(c1, Dock.Right);
                row.Children.Add(c1);
                row.Children.Add(new TextBlock { Text = category != null ? MPCategoryName(category) : "#" + CprpImportTable[n], FontSize = 13, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis });
                _mpImport.Children.Add(row);
            }
            var apply = MPButton(string.Format(CultureInfo.CurrentCulture, MPT("mp_import_apply", "Import {0} props"), total), (_, __) =>
            {
                for (int n = 0; n < CprpImportTable.Length; n++)
                {
                    string field = CprpField(_mpImported, n);
                    if (!string.IsNullOrWhiteSpace(field) && CprpImportTable[n] < allprops.Count)
                        writeCategory(field, CprpImportTable[n], _mpAllCreators);
                }
                _mpImported = null;
                RenderMPAll();
            }, primary: true);
            apply.Margin = new Thickness(0, 8, 0, 0);
            apply.IsEnabled = allprops.Count > 0;
            _mpImport.Children.Add(apply);
        }

        private FrameworkElement FindMPImportCard()
        {
            DependencyObject d = _mpImport;
            while (d != null && !(d is FrameworkElement fe && fe.Name == "mpImportCard"))
                d = LogicalTreeHelper.GetParent(d);
            return d as FrameworkElement;
        }

        public void writeCategory(string basecategoryproplist, int category, bool all)
        {
            if (string.IsNullOrWhiteSpace(basecategoryproplist) || category >= allprops.Count || _mpCreator < 0)
                return;
            var props = basecategoryproplist.Split(',').Select(Functions.int_parse).ToList();
            var proplist = allprops[category].prop;
            for (int i = 0; i < Math.Min(props.Count, proplist.Count); i++)
            {
                var addresses = all ? proplist[i].Address.Where(a => !string.IsNullOrWhiteSpace(a)) : new[] { proplist[i].Address[_mpCreator] };
                foreach (var address in addresses)
                    m.memory(address).SetInt(props[i]);
                UpdateModdedPropEntry(proplist[i], props[i]);
            }
        }

        private void ExportMProps()
        {
            if (!m.IsProcOpen || _mpCreator < 0 || allprops.Count == 0)
                return;
            var file = new JSON.ModdedPropJSON.Rootobject();
            for (int n = 0; n < CprpExportTable.Length; n++)
            {
                if (CprpExportTable[n] >= allprops.Count)
                    continue;
                string value = string.Join(",", allprops[CprpExportTable[n]].prop.Select(x => m.memory(x.Address[_mpCreator]).Get<int>().ToString("X8", CultureInfo.InvariantCulture)));
                typeof(JSON.ModdedPropJSON.Rootobject).GetProperty("s" + n.ToString(CultureInfo.InvariantCulture))?.SetValue(file, value);
            }
            string json = JsonConvert.SerializeObject(file);
            preparemoddedpropjsontoexport(ref json);
            var dialog = new SaveFileDialog
            {
                Title = MPT("mp_export_title", "Save the prop file"),
                Filter = "Custom Prop File (*.cprp)|*.cprp",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            };
            if (dialog.ShowDialog() == true)
                File.WriteAllText(dialog.FileName, json);
        }

        // Puts the creator's own models back into every table (chosen creator, or all loaded).
        private void RestoreMProps()
        {
            if (!m.IsProcOpen || _mpCreator < 0)
                return;
            foreach (var category in MPCategories)
            {
                var defaults = MPDefaults(category.Table);
                if (defaults == null)
                    continue;
                var slots = MPSlots(category);
                for (int i = 0; i < slots.Count && i < defaults.Length; i++)
                    if (defaults[i].HasValue && slots[i].IntegerValue != defaults[i])
                        WriteMPSlot(slots[i], defaults[i].Value, onlyChosen: true);
            }
            RenderMPAll();
        }
    }
}
