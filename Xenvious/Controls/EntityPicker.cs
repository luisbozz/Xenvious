using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Xenvious
{
    /// <summary>
    /// Picks "follow / bind to" targets the way the creator stores them: a type
    /// (ciSPAWN_NEAR_ENTITY_TYPE_: 0 none, 1 actor, 2 vehicle, 3 object, 4 go-to, 5 team,
    /// 6 last player, 7 train, 8 lobby leader) and an index, with the job's entities listed by
    /// name instead of numbers. Used by the play areas; zones, warp portals and the spawn-near
    /// options store the same pair (docs/handoff/PLAY-AREA.md).
    /// </summary>
    public class EntityPicker : StackPanel
    {
        public const int None = 0, Actor = 1, Vehicle = 2, Object = 3, GoTo = 4, Team = 5, LastPlayer = 6, Train = 7, LobbyLeader = 8;

        private static readonly (int Type, string Key, string Fallback, string Icon)[] Types =
        {
            (None, "ep_none", "Fixed", "M6,10 A7,7 0 1 1 20,10 A7,7 0 1 1 6,10 M8,15 L18,5"),
            (Actor, "ep_actor", "Actor", "M13,2 A3,3 0 1 1 12.99,2 M13,8 V14 M9,11 H17 M13,14 L9,19 M13,14 L17,19"),
            (Vehicle, "ep_vehicle", "Vehicle", "M3,13 H23 V9 L19,5 H8 L5,9 H3 Z M6,14 A2,2 0 1 1 10,14 A2,2 0 1 1 6,14 M16,14 A2,2 0 1 1 20,14 A2,2 0 1 1 16,14"),
            (Object, "ep_object", "Object", "M6,6 L13,2 L20,6 V14 L13,18 L6,14 Z M6,6 L13,10 L20,6 M13,10 V18"),
            (GoTo, "ep_goto", "Go-to", "M13,18 C8,12 7,10 7,8 A6,6 0 0 1 19,8 C19,10 18,12 13,18 Z"),
            (Team, "ep_team", "Team", "M6,6 A3,3 0 1 1 12,6 A3,3 0 1 1 6,6 M15,6 A3,3 0 1 1 21,6 A3,3 0 1 1 15,6 M3,18 C3,12 15,12 15,18 M12,18 C12,13 23,12 23,18"),
            (LastPlayer, "ep_lastplayer", "Last player", "M10,6 A3,3 0 1 1 16,6 A3,3 0 1 1 10,6 M7,18 C7,12 19,12 19,18 M20,3 L23,6 L20,9"),
            (Train, "ep_train", "Train", "M4,4 H22 V14 H4 Z M4,17 H22 M8,14 V17 M18,14 V17"),
            (LobbyLeader, "ep_leader", "Lobby leader", "M6,16 L8,6 L13,11 L18,6 L20,16 Z"),
        };

        private readonly UniformGrid _tiles = new UniformGrid { Columns = 3, Margin = new Thickness(0, 0, -6, 6) };
        private readonly ComboBox _list = new ComboBox { Height = 30, Margin = new Thickness(0, 0, 0, 6) };
        private readonly TextBlock _hint = new TextBlock { FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 6) };
        private readonly List<ToggleButton> _typeButtons = new List<ToggleButton>();
        private int _type, _id = -1;
        private bool _sync;
        private bool _built;

        /// <summary>Type and id changed by the user.</summary>
        public event Action<int, int> Changed;

        /// <summary>Label of an entity of a type (index 0-based), e.g. "#2 Valkyrie"; set by the window.</summary>
        public static Func<int, int, string> Label = (type, index) => "#" + (index + 1).ToString(CultureInfo.CurrentCulture);

        /// <summary>How many entities of a type the job has.</summary>
        public static Func<int, int> Count = type => 0;

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;

        public EntityPicker()
        {
            Loaded += (_, __) => Build();
        }

        private void Build()
        {
            if (_built)
                return;
            _built = true;
            _hint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            foreach (var t in Types)
            {
                var content = new StackPanel();
                content.Children.Add(new Path
                {
                    Data = Geometry.Parse(t.Icon), StrokeThickness = 1.5, Width = 26, Height = 20, Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Center,
                });
                ((Path)content.Children[0]).SetBinding(Shape.StrokeProperty, new System.Windows.Data.Binding("Foreground") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.FindAncestor, typeof(ToggleButton), 1) });
                content.Children.Add(new TextBlock { Text = T(t.Key, t.Fallback), FontSize = 12.5, FontWeight = FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis, Margin = new Thickness(0, 4, 0, 0) });
                var button = new ToggleButton { Style = (Style)FindResource("ChoiceTile"), Content = content, Margin = new Thickness(0, 0, 6, 6), Tag = t.Type };
                int type = t.Type;
                button.Click += (_, __) =>
                {
                    int id = type == None ? -1 : (type == LastPlayer || type == LobbyLeader ? 0 : Math.Max(0, _type == type ? _id : 0));
                    Set(type, id);
                    Changed?.Invoke(_type, _id);
                };
                _typeButtons.Add(button);
                _tiles.Children.Add(button);
            }
            _list.SelectionChanged += (_, __) =>
            {
                if (_sync || !(_list.SelectedItem is ComboBoxItem item))
                    return;
                _id = (int)item.Tag;
                Changed?.Invoke(_type, _id);
            };
            Children.Add(_tiles);
            Children.Add(_list);
            Children.Add(_hint);
            Show();
        }

        public void Set(int type, int id)
        {
            _type = type;
            _id = id;
            Show();
        }

        private void Show()
        {
            if (!_built)
                return;
            _sync = true;
            foreach (var b in _typeButtons)
                b.IsChecked = (int)b.Tag == _type;
            _list.Items.Clear();
            bool hasList = _type == Actor || _type == Vehicle || _type == Object || _type == GoTo || _type == Team || _type == Train;
            _list.Visibility = hasList ? Visibility.Visible : Visibility.Collapsed;
            if (hasList)
            {
                int count = _type == Team ? 4 : Math.Max(0, Count(_type));
                // -2 has a meaning of its own for these two (controller: GET_FMMC_AREA_BOUNDS_POS).
                if (_type == Vehicle)
                    _list.Items.Add(new ComboBoxItem { Content = T("ep_lastveh", "Last vehicle used"), Tag = -2 });
                if (_type == Team)
                    _list.Items.Add(new ComboBoxItem { Content = T("ep_leading", "Leading runner"), Tag = -2 });
                for (int i = 0; i < count; i++)
                    _list.Items.Add(new ComboBoxItem { Content = _type == Team ? T("actorteam", "Team") + " " + (i + 1) : Label(_type, i), Tag = i });
                // An index the job does not have (yet) still shows, so nothing is lost silently.
                if (_id >= count || (_id < 0 && _id != -2))
                    _list.Items.Add(new ComboBoxItem { Content = "#" + (_id + 1).ToString(CultureInfo.CurrentCulture) + " (" + T("ep_missing", "not in the job") + ")", Tag = _id });
                _list.SelectedItem = _list.Items.OfType<ComboBoxItem>().FirstOrDefault(i => (int)i.Tag == _id);
            }
            _hint.Text = _type == None ? T("ep_hint_none", "The area stays where it is.")
                : _type == LastPlayer || _type == LobbyLeader ? T("ep_hint_player", "Follows that player during the job.")
                : T("ep_hint_follow", "The centre moves with the target every frame.");
            _sync = false;
        }
    }
}
