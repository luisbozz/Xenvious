using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Xenvious
{
    // Part of MainWindow: Player Settings layout (start point list, heading slider, start in a vehicle with its seat).
    public partial class MainWindow
    {
        private ToggleButton _spFoot, _spVehicle;
        private ComboBox _spSeat;
        private FrameworkElement _spVehicleRow;
        private bool _spLoading;

        private void InitPlayerLayout()
        {
            // The team goes into the header next to the point index; its own card goes away.
            if (ddplyrno.Parent is FrameworkElement inner && inner.Parent is Border indexBox && indexBox.Parent is Panel bar)
            {
                var teamCard = FindCard(ddplyrteam);
                if (ddplyrteam.Parent is Panel teamPanel)
                {
                    teamPanel.Children.Remove(ddplyrteam);
                    ddplyrteam.Width = 84;
                    ddplyrteam.Margin = new Thickness(0, 0, 16, 0);
                    ddplyrteam.Height = 32;
                    var label = new TextBlock { Text = TranslateOr("actorteam", "Team"), FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };
                    label.SetResourceReference(TextBlock.ForegroundProperty, "NavMutedBrush");
                    bar.Children.Insert(0, ddplyrteam);
                    bar.Children.Insert(0, label);
                }
                if (teamCard?.Parent is Panel teamColumn)
                    teamColumn.Visibility = Visibility.Collapsed;
            }

            // The start point list leads the page.
            var locCard = FindCard(tbplyrlocx);
            if (locCard?.Parent is Panel locColumn && locColumn.Parent is Panel masonry)
            {
                var list = new StackPanel();
                list.Children.Add(new StartPointsView(ddplyrteam, ddplyrno, ddplyrveh));
                masonry.Children.Insert(0, list);
            }

            // The point count belongs to the raw values: the list shows it already.
            var rawPanel = FindRawPanel(tbplyrbits);
            if (tbplyrno.Parent is Panel numberPanel && rawPanel != null)
            {
                int at = numberPanel.Children.IndexOf(tbplyrno);
                var moved = new System.Collections.Generic.List<UIElement>();
                if (at > 0 && numberPanel.Children[at - 1] is TextBlock numberLabel)
                    moved.Add(numberLabel);
                moved.Add(tbplyrno);
                if (at + 1 < numberPanel.Children.Count && numberPanel.Children[at + 1] is Rectangle line)
                    numberPanel.Children.Remove(line);
                foreach (var e in moved)
                {
                    numberPanel.Children.Remove(e);
                    rawPanel.Children.Insert(rawPanel.Children.Count, e);
                }
            }

            HeadingSlider.Attach(tbplyrhead, creatorRefresh);
            BuildStartChoice();
        }

        private Border FindCard(FrameworkElement inside)
        {
            var dashCard = (Style)FindResource("DashCard");
            FrameworkElement e = inside;
            while (e != null && !(e is Border b && b.Style == dashCard))
                e = e.Parent as FrameworkElement;
            return e as Border;
        }

        private static Panel FindRawPanel(FrameworkElement inside)
        {
            FrameworkElement e = inside;
            while (e != null && !(e is Expander))
                e = e.Parent as FrameworkElement;
            return (e as Expander)?.Content as Panel;
        }

        // "On foot" / "In a vehicle" tiles, then the vehicle and its seat (f_7 / f_8).
        private void BuildStartChoice()
        {
            if (!(ddplyrveh.Parent is Panel panel))
                return;
            int at = panel.Children.IndexOf(ddplyrveh);
            if (at > 0 && panel.Children[at - 1] is TextBlock oldLabel)
                oldLabel.Text = TranslateOr("sp_start", "Starts");

            var tiles = new UniformGrid { Columns = 2, Margin = new Thickness(0, 0, -6, 4) };
            _spFoot = StartTile(TranslateOr("sp_onfoot", "On foot"), "M12,4 A2,2 0 1 0 12,8 A2,2 0 1 0 12,4 M12,8 L12,15 M12,15 L9,21 M12,15 L15,21 M8,11 L16,11");
            _spVehicle = StartTile(TranslateOr("sp_invehicle", "In a vehicle"), "M4,15 L4,11 L7,6 L17,6 L20,11 L20,15 Z M7,15 A1.8,1.8 0 1 0 7,18.6 M17,15 A1.8,1.8 0 1 0 17,18.6 M4,11 L20,11");
            _spFoot.Click += (_, __) => SetStartVehicle(-1);
            _spVehicle.Click += (_, __) => SetStartVehicle(Math.Max(0, ddplyrveh.SelectedIndex - 1));
            tiles.Children.Add(_spFoot);
            tiles.Children.Add(_spVehicle);
            panel.Children.Insert(at, tiles);

            // Vehicle and seat side by side, only when the point starts in a vehicle.
            panel.Children.Remove(ddplyrveh);
            ddplyrveh.Margin = new Thickness(0);
            _spSeat = new ComboBox { Height = 30, MinWidth = 150 };
            foreach (int seat in new[] { -1, 0, 1, 2, 3, 4, 5, 6 })
                _spSeat.Items.Add(new ComboBoxItem { Content = StartPointsView.SeatName(seat), Tag = seat });
            _spSeat.SelectionChanged += (_, __) =>
            {
                if (_spLoading || !m.IsProcOpen || !(_spSeat.SelectedItem is ComboBoxItem item) || GTA.Offsets.Editor.player_seat == 0)
                    return;
                new Global(StartField(GTA.Offsets.Editor.player_seat)).SetInt((int)item.Tag);
            };
            var row = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });
            var vehStack = new StackPanel();
            vehStack.Children.Add(new TextBlock { Text = TranslateOr("sp_vehicle", "Vehicle") }.Also(t => t.SetResourceReference(StyleProperty, "FieldAxis")));
            vehStack.Children.Add(ddplyrveh);
            var seatStack = new StackPanel();
            seatStack.Children.Add(new TextBlock { Text = TranslateOr("sp_seat", "Seat") }.Also(t => t.SetResourceReference(StyleProperty, "FieldAxis")));
            seatStack.Children.Add(_spSeat);
            Grid.SetColumn(seatStack, 2);
            row.Children.Add(vehStack);
            row.Children.Add(seatStack);
            panel.Children.Insert(at + 1, row);
            _spVehicleRow = row;

            ddplyrveh.SelectionChanged += (_, __) => UpdateStartChoice();
            ddplyrno.SelectionChanged += (_, __) => UpdateStartChoice();
            ddplyrteam.SelectionChanged += (_, __) => UpdateStartChoice();
            UpdateStartChoice();
        }

        private ToggleButton StartTile(string text, string icon)
        {
            var tile = new ToggleButton { Margin = new Thickness(0, 0, 6, 6), MinHeight = 56 };
            tile.SetResourceReference(StyleProperty, "ChoiceTile");
            var path = new Path { Data = Geometry.Parse(icon), StrokeThickness = 1.6, Width = 20, Height = 20, Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Center,
                StrokeLineJoin = PenLineJoin.Round, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round };
            path.SetBinding(Shape.StrokeProperty, new System.Windows.Data.Binding("Foreground") { Source = tile });
            var content = new StackPanel();
            content.Children.Add(path);
            content.Children.Add(new TextBlock { Text = text, FontSize = 12.5, FontWeight = FontWeights.SemiBold, TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 4, 0, 0) });
            tile.Content = content;
            return tile;
        }

        private long StartField(long field) => field + ddplyrteam.SelectedIndex * GTA.Offsets.Editor.team_NEXT_settings + ddplyrno.SelectedIndex * GTA.Offsets.Editor.next_settings;

        private bool StartPointPicked => m.IsProcOpen && ddplyrteam.SelectedIndex >= 0 && ddplyrno.SelectedIndex >= 0 && GTA.Offsets.Editor.player_veh != 0;

        // The creator puts a point on foot with vehicle -1 and seat -3; in a vehicle the driver's seat is its default.
        private void SetStartVehicle(int vehicle)
        {
            if (!StartPointPicked)
                return;
            new Global(StartField(GTA.Offsets.Editor.player_veh)).SetInt(vehicle);
            if (GTA.Offsets.Editor.player_seat != 0)
            {
                int seat = new Global(StartField(GTA.Offsets.Editor.player_seat)).Get<int>();
                if (vehicle < 0)
                    new Global(StartField(GTA.Offsets.Editor.player_seat)).SetInt(-3);
                else if (seat < -2)
                    new Global(StartField(GTA.Offsets.Editor.player_seat)).SetInt(-1);
            }
            GetPlyrValues(true);
            UpdateStartChoice();
        }

        private void UpdateStartChoice()
        {
            if (_spFoot == null)
                return;
            bool picked = StartPointPicked;
            int vehicle = picked ? new Global(StartField(GTA.Offsets.Editor.player_veh)).Get<int>() : -1;
            bool hasVehicles = ddplyrveh.Items.Count > 1;
            _spFoot.IsChecked = vehicle < 0;
            _spVehicle.IsChecked = vehicle >= 0;
            _spFoot.IsEnabled = picked;
            _spVehicle.IsEnabled = picked && hasVehicles;
            ToolTipService.SetShowOnDisabled(_spVehicle, true);
            _spVehicle.ToolTip = hasVehicles ? null : TranslateOr("sp_novehicles", "Place a vehicle first.");
            _spVehicleRow.Visibility = vehicle >= 0 ? Visibility.Visible : Visibility.Collapsed;

            _spLoading = true;
            int seat = picked && GTA.Offsets.Editor.player_seat != 0 ? new Global(StartField(GTA.Offsets.Editor.player_seat)).Get<int>() : -1;
            ComboBoxItem match = null;
            foreach (ComboBoxItem item in _spSeat.Items)
                if ((int)item.Tag == seat)
                    match = item;
            _spSeat.SelectedItem = match;
            _spSeat.IsEnabled = GTA.Offsets.Editor.player_seat != 0;
            _spLoading = false;
        }
    }
}
