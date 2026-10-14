using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Xenvious
{
    /// <summary>
    /// Deathmatch and Survival › General: the job settings of the creator's details menu that the
    /// dashboard does not have. "Match" holds time limit, target score and lock weapons (deathmatch),
    /// "Players" camera, aim mode and helmets; with LockWeapons the players card also holds lock
    /// weapons (survival). Values are the creator's own menu indices.
    /// </summary>
    public class MatchSettingsCard : Border
    {
        public enum CardKind { Match, Players }

        public CardKind Kind { get; set; }
        public bool LockWeapons { get; set; }

        // Target score of a deathmatch: LOB_TARG_KIL_ index -> kills; 7 (and 14 in old jobs) is off.
        private static readonly (int Index, int Kills)[] Scores =
        {
            (7, 0), (0, 10), (1, 15), (2, 20), (3, 25), (4, 30), (5, 35), (6, 40), (8, 45), (9, 50),
            (10, 60), (11, 70), (12, 80), (13, 90), (14, 100), (15, 110), (16, 120), (17, 130), (18, 140), (19, 150),
        };
        private static readonly int[] Minutes = { 5, 10, 15, 20, 30, 45, 60 };

        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private ComboBox _time, _score, _lock, _camera, _aim;
        private CheckBox _helmets;
        private bool _built, _sync;

        public MatchSettingsCard()
        {
            Loaded += (_, __) => Build();
            IsVisibleChanged += (_, __) =>
            {
                if (IsVisible) { Read(); _timer.Start(); }
                else _timer.Stop();
            };
            _timer.Tick += (_, __) => Read();
        }

        private static Binding Text(string key, string fallback) => new Binding($"Translation[{key}]") { FallbackValue = fallback };

        private TextBlock Label(string key, string fallback, string style)
        {
            var text = new TextBlock { Style = (Style)FindResource(style) };
            text.SetBinding(TextBlock.TextProperty, Text(key, fallback));
            return text;
        }

        private static ComboBoxItem Item(string key, string fallback, int tag)
        {
            var item = new ComboBoxItem { Tag = tag };
            item.SetBinding(ContentControl.ContentProperty, Text(key, fallback));
            return item;
        }

        // An item whose text is a translated format with one number ("{0} minutes").
        private static ComboBoxItem NumberItem(string key, string fallback, int number, int tag)
        {
            var item = new ComboBoxItem { Tag = tag, Content = number.ToString(CultureInfo.InvariantCulture) };
            item.SetBinding(ContentControl.ContentStringFormatProperty, Text(key, fallback));
            return item;
        }

        private ComboBox Pick(StackPanel body, string key, string fallback)
        {
            body.Children.Add(Label(key, fallback, "FieldLabel"));
            var box = new ComboBox { Height = 30, Margin = new Thickness(0, 0, 0, 10) };
            box.SelectionChanged += (_, __) => Write(box);
            body.Children.Add(box);
            return box;
        }

        private void Build()
        {
            if (_built)
                return;
            _built = true;
            Style = (Style)FindResource("DashCard");
            Margin = new Thickness(0, 0, 0, 12);

            var header = new Border { Style = (Style)FindResource("DashCardHeader") };
            header.Child = Kind == CardKind.Match ? Label("ms_card_match", "Match", "DashCardTitle") : Label("ms_card_players", "Players", "DashCardTitle");
            var body = new StackPanel { Margin = new Thickness(14, 12, 14, 4) };

            if (Kind == CardKind.Match)
            {
                _time = Pick(body, "ms_time", "Time limit");
                for (int i = 0; i < Minutes.Length; i++)
                    _time.Items.Add(NumberItem("ms_minutes", "{0} minutes", Minutes[i], i));
                _score = Pick(body, "ms_score", "Target score");
                foreach (var (index, kills) in Scores)
                    _score.Items.Add(kills == 0 ? Item("ms_score_off", "Off (time limit only)", index) : NumberItem("ms_kills", "{0} kills", kills, index));
            }
            if (Kind == CardKind.Match || LockWeapons)
            {
                _lock = Pick(body, "lockweapons", "Lock Weapons");
                _lock.Items.Add(Item("invglw1", "Owned + Pickups", 0));
                _lock.Items.Add(Item("invglw2", "Forced + Pickups", 1));
                _lock.Items.Add(Item("invglw3", "Forced Only", 2));
            }
            if (Kind == CardKind.Players)
            {
                _camera = Pick(body, "ms_camera", "Force camera view");
                _camera.Items.Add(Item("ms_camera_0", "No forced view", 0));
                _camera.Items.Add(Item("ms_camera_1", "First person only", 1));
                _camera.Items.Add(Item("ms_camera_2", "Third person only", 2));
                _aim = Pick(body, "ms_aim", "Aim mode");
                _aim.Items.Add(Item("ms_aim_0", "Assisted aim: full", 0));
                _aim.Items.Add(Item("ms_aim_1", "Assisted aim: partial", 1));
                _aim.Items.Add(Item("ms_aim_2", "Free aim: assisted", 2));
                _aim.Items.Add(Item("ms_aim_3", "Free aim: no lock-on", 3));

                var row = new Grid { Style = (Style)FindResource("FormRow") };
                row.ColumnDefinitions.Add(new ColumnDefinition());
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(46) });
                row.Children.Add(Label("ms_helmets", "Remove helmets", "FormLabel"));
                _helmets = new CheckBox { Style = (Style)FindResource("FormToggle") };
                Grid.SetColumn(_helmets, 1);
                _helmets.Click += (_, __) =>
                {
                    if (MainWindow.m.IsProcOpen && GTA.Offsets.Editor.menubs3 != 0)
                        Functions.Write.writebinary(7, GTA.Offsets.Editor.menubs3, _helmets.IsChecked == true);
                };
                row.Children.Add(_helmets);
                body.Children.Add(row);
            }

            var dock = new DockPanel();
            DockPanel.SetDock(header, Dock.Top);
            dock.Children.Add(header);
            dock.Children.Add(body);
            Child = dock;
            Read();
        }

        private static void Show(ComboBox box, int value)
        {
            if (box == null || box.IsDropDownOpen)
                return;
            foreach (ComboBoxItem item in box.Items)
                if ((int)item.Tag == value)
                {
                    box.SelectedItem = item;
                    return;
                }
            box.SelectedIndex = -1;
        }

        private void Read()
        {
            if (!_built || !IsVisible || !MainWindow.m.IsProcOpen)
                return;
            _sync = true;
            try
            {
                if (_time != null && GTA.Offsets.Editor.time != 0)
                    Show(_time, new Global(GTA.Offsets.Editor.time).Get<int>());
                if (_score != null && GTA.Offsets.Editor.score != 0)
                    Show(_score, new Global(GTA.Offsets.Editor.score).Get<int>());
                if (_lock != null && GTA.Offsets.Editor.menubs != 0)
                    Show(_lock, Functions.Read.checkbinary(18, GTA.Offsets.Editor.menubs) ? 2 : Functions.Read.checkbinary(19, GTA.Offsets.Editor.menubs) ? 1 : 0);
                if (_camera != null && GTA.Offsets.Editor.fxc != 0)
                    Show(_camera, new Global(GTA.Offsets.Editor.fxc).Get<int>());
                if (_aim != null && GTA.Offsets.Editor.mam != 0)
                    Show(_aim, new Global(GTA.Offsets.Editor.mam).Get<int>());
                if (_helmets != null && GTA.Offsets.Editor.menubs3 != 0)
                    Functions.Read.checkbinary(7, GTA.Offsets.Editor.menubs3, _helmets);
            }
            finally
            {
                _sync = false;
            }
        }

        private void Write(ComboBox box)
        {
            if (_sync || !(box.SelectedItem is ComboBoxItem item) || !MainWindow.m.IsProcOpen)
                return;
            int value = (int)item.Tag;
            if (box == _lock)
            {
                if (GTA.Offsets.Editor.menubs == 0)
                    return;
                // Bit 18 = forced only, bit 19 = forced + pickups (FMMC_LKW_2 / _1), none = owned + pickups.
                Functions.Write.writebinary(18, GTA.Offsets.Editor.menubs, value == 2);
                Functions.Write.writebinary(19, GTA.Offsets.Editor.menubs, value == 1);
                return;
            }
            long field = box == _time ? GTA.Offsets.Editor.time : box == _score ? GTA.Offsets.Editor.score
                : box == _camera ? GTA.Offsets.Editor.fxc : box == _aim ? GTA.Offsets.Editor.mam : 0;
            if (field != 0)
                new Global(field).SetInt(value);
        }
    }
}
