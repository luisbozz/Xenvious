using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Xenvious
{
    /// <summary>
    /// Mission › General › "Solo &amp; controller": the fields that decide whether an LTS or Capture
    /// starts with one player and which mission controller runs it, side by side for testing.
    /// fmmc_launcher and all three controllers take the minimum players from "min", or with trel
    /// bit 16 (or menubs5 bit 14) from the sum of the team minimums. dlcrel picks the controller:
    /// below 2 fm_mission_controller, 2 to 13 fm_mission_controller_2020, 14 and up _v3 (1.73).
    /// Greyed out outside the LTS and Capture creators.
    /// </summary>
    public class SoloTestCard : Border
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private TextBox _trel, _min, _tnum, _mtnum;
        private readonly TextBox[] _teamMin = new TextBox[4];
        private ComboBox _dlcrel;
        private TextBlock _bit16, _effective;
        private bool _built, _sync;

        public SoloTestCard()
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

        private TextBlock Label(string key, string fallback, string style = "FieldLabel")
        {
            var text = new TextBlock { Style = (Style)FindResource(style) };
            text.SetBinding(TextBlock.TextProperty, Text(key, fallback));
            return text;
        }

        private TextBlock Hint(string key, string fallback)
        {
            var text = new TextBlock { FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, -4, 0, 10) };
            text.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            text.SetBinding(TextBlock.TextProperty, Text(key, fallback));
            return text;
        }

        private TextBox Field(Func<long> address)
        {
            var box = new TextBox { Style = (Style)FindResource("Watermark"), Height = 30, Margin = new Thickness(0, 0, 0, 10) };
            box.TextChanged += (_, __) =>
            {
                long at = address();
                if (_sync || at == 0 || !MainWindow.m.IsProcOpen)
                    return;
                if (int.TryParse(box.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                    new Global(at).SetInt(value);
            };
            return box;
        }

        // A label above a field, for the rows of small number fields.
        private StackPanel Cell(string key, string fallback, TextBox box, int column, Grid grid)
        {
            var cell = new StackPanel();
            cell.Children.Add(Label(key, fallback, "FieldLabelRaw"));
            cell.Children.Add(box);
            Grid.SetColumn(cell, column);
            grid.Children.Add(cell);
            return cell;
        }

        private static Grid Columns(int count)
        {
            var grid = new Grid();
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            return grid;
        }

        private void Build()
        {
            if (_built)
                return;
            _built = true;
            Style = (Style)FindResource("DashCard");
            Margin = new Thickness(0, 0, 0, 12);

            var header = new Border { Style = (Style)FindResource("DashCardHeader") };
            header.Child = Label("solo_card", "Solo & controller", "DashCardTitle");
            var body = new StackPanel { Margin = new Thickness(14, 12, 14, 4) };

            body.Children.Add(Label("solo_trel", "trel"));
            _trel = Field(() => GTA.Offsets.Editor.trel);
            body.Children.Add(_trel);
            _bit16 = new TextBlock { FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, -4, 0, 10) };
            _bit16.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            body.Children.Add(_bit16);

            var counts = Columns(3);
            _min = Field(() => GTA.Offsets.Editor.min);
            _tnum = Field(() => GTA.Offsets.Editor.tnum);
            _mtnum = Field(() => GTA.Offsets.Editor.mtnum);
            Cell("solo_min", "min (players)", _min, 0, counts);
            Cell("solo_tnum", "tnum (min teams)", _tnum, 2, counts);
            Cell("solo_mtnum", "mtnum (max teams)", _mtnum, 4, counts);
            body.Children.Add(counts);

            body.Children.Add(Label("solo_numpt", "numpt (minimum players per team)", "FieldLabelRaw"));
            var teams = Columns(4);
            for (int i = 0; i < 4; i++)
            {
                int team = i;
                _teamMin[i] = Field(() => GTA.Offsets.Editor.numpt == 0 ? 0 : GTA.Offsets.Editor.numpt + team);
                var cell = new StackPanel();
                var axis = new TextBlock { Style = (Style)FindResource("FieldAxis"), Text = (i + 1).ToString(CultureInfo.InvariantCulture) };
                cell.Children.Add(axis);
                cell.Children.Add(_teamMin[i]);
                Grid.SetColumn(cell, i * 2);
                teams.Children.Add(cell);
            }
            body.Children.Add(teams);

            _effective = new TextBlock { FontSize = 12, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, -2, 0, 12) };
            _effective.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            body.Children.Add(_effective);

            body.Children.Add(Label("solo_dlcrel", "Mission controller (dlcrel)"));
            _dlcrel = new ComboBox { Height = 30, Margin = new Thickness(0, 0, 0, 10) };
            for (int value = -1; value <= 14; value++)
            {
                string key = value < 2 ? "solo_ctrl_classic" : value < 14 ? "solo_ctrl_2020" : "solo_ctrl_v3";
                string fallback = value < 2 ? "{0} · classic controller" : value < 14 ? "{0} · 2020 controller" : "{0} · v3 controller";
                var item = new ComboBoxItem { Tag = value, Content = value.ToString(CultureInfo.InvariantCulture) };
                item.SetBinding(ContentControl.ContentStringFormatProperty, Text(key, fallback));
                _dlcrel.Items.Add(item);
            }
            _dlcrel.SelectionChanged += (_, __) =>
            {
                if (_sync || !(_dlcrel.SelectedItem is ComboBoxItem item) || GTA.Offsets.Editor.dlcrel == 0 || !MainWindow.m.IsProcOpen)
                    return;
                new Global(GTA.Offsets.Editor.dlcrel).SetInt((int)item.Tag);
            };
            body.Children.Add(_dlcrel);
            body.Children.Add(Hint("solo_dlcrel_hint", "Below 2 the classic fm_mission_controller runs the job, 2 to 13 fm_mission_controller_2020, 14 fm_mission_controller_v3. Restart GTA before each test."));

            var dock = new DockPanel();
            DockPanel.SetDock(header, Dock.Top);
            dock.Children.Add(header);
            dock.Children.Add(body);
            Child = dock;
            Read();
        }

        private static int Get(long at) => at == 0 ? 0 : new Global(at).Get<int>();

        private static void Show(TextBox box, int value)
        {
            if (!box.IsKeyboardFocusWithin)
                box.Text = value.ToString(CultureInfo.InvariantCulture);
        }

        private void Read()
        {
            if (!_built || !IsVisible || !MainWindow.m.IsProcOpen)
                return;
            // Only the LTS and Capture creators read these fields for solo; elsewhere greyed out.
            string creator = MainWindow.Instance.CurrentCreatorScript ?? "";
            IsEnabled = creator == "fm_lts_creator" || creator == "fm_capture_creator";
            _sync = true;
            try
            {
                int trel = Get(GTA.Offsets.Editor.trel);
                Show(_trel, trel);
                bool bit16 = (trel & (1 << 16)) != 0;
                _bit16.Text = string.Format(MainWindow.Instance.TranslateOr(bit16 ? "solo_bit16_on" : "solo_bit16_off",
                    bit16 ? "Bit 16 is set ({0}): the minimum is the sum of the team minimums." : "Bit 16 is clear: the minimum is \"min\". Add {0} to set it."), 1 << 16);

                int min = Get(GTA.Offsets.Editor.min);
                Show(_min, min);
                Show(_tnum, Get(GTA.Offsets.Editor.tnum));
                Show(_mtnum, Get(GTA.Offsets.Editor.mtnum));
                int sum = 0;
                for (int i = 0; i < 4; i++)
                {
                    int team = GTA.Offsets.Editor.numpt == 0 ? 0 : Get(GTA.Offsets.Editor.numpt + i);
                    Show(_teamMin[i], team);
                    sum += team;
                }
                // The launcher adds up the teams below dtn; menubs5 bit 14 does the same as trel bit 16.
                bool alt = GTA.Offsets.Editor.menubs5 != 0 && Functions.Read.checkbinary(14, GTA.Offsets.Editor.menubs5);
                int teams = GTA.Offsets.Editor.dtn == 0 ? 4 : Math.Max(0, Math.Min(4, Get(GTA.Offsets.Editor.dtn)));
                if (bit16 || alt)
                {
                    sum = 0;
                    for (int i = 0; i < teams; i++)
                        sum += Get(GTA.Offsets.Editor.numpt + i);
                }
                int effective = bit16 || alt ? sum : min;
                _effective.Text = string.Format(MainWindow.Instance.TranslateOr("solo_effective", "Players needed to start: {0}"), effective);

                if (GTA.Offsets.Editor.dlcrel != 0 && !_dlcrel.IsDropDownOpen)
                {
                    int dlcrel = Get(GTA.Offsets.Editor.dlcrel);
                    _dlcrel.SelectedIndex = dlcrel >= -1 && dlcrel <= 14 ? dlcrel + 1 : -1;
                }
            }
            finally
            {
                _sync = false;
            }
        }
    }
}
