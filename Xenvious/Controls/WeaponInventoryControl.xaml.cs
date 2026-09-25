using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Xenvious
{
    /// <summary>
    /// Interaction logic for WeaponInventoryControl2.xaml
    /// </summary>
    public partial class WeaponInventoryControl : UserControl
    {
        private long teamoffset = 0;
        private List<int> bits = new List<int>();
        private List<long> offsets = new List<long>();
        private List<CheckBox> cbs = new List<CheckBox>();

        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(
            "HeaderText",
            typeof(string),
            typeof(WeaponInventoryControl));

        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }

        public static readonly DependencyProperty WeaponListProperty = DependencyProperty.Register(
            "WeaponList",
            typeof(List<WeaponInventory>),
            typeof(WeaponInventoryControl));

        public List<WeaponInventory> WeaponList
        {
            get { return (List<WeaponInventory>)GetValue(WeaponListProperty); }
            set 
            { 
                SetValue(WeaponListProperty, value);
                weaponList.Children.Clear();
                if (WeaponList != null)
                {
                    DispatcherTimer dispatcherTimer = new DispatcherTimer();

                    for (int i = 0; i < WeaponList.Count; i++)
                    {
                        long temp_offset = 0;
                        if (WeaponList[i].InventoryOffset == InventoryOffset._1)
                        {
                            temp_offset = GTA.Offsets.Editor.inv;
                        }
                        else if (WeaponList[i].InventoryOffset == InventoryOffset._2)
                        {
                            temp_offset = GTA.Offsets.Editor.inv2;
                        }
                        else if (WeaponList[i].InventoryOffset == InventoryOffset._3)
                        {
                            temp_offset = GTA.Offsets.Editor.inv3;
                        }
                        else if (WeaponList[i].InventoryOffset == InventoryOffset._4)
                        {
                            temp_offset = GTA.Offsets.Editor.inv4;
                        }

                        ChangeTeamOffset();

                        Grid g = new Grid();
                        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.80, GridUnitType.Star) });
                        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.20, GridUnitType.Star) });

                        TextBlock tb = new TextBlock();
                        tb.VerticalAlignment = VerticalAlignment.Center;
                        tb.FontSize = 14.0;
                        tb.FontFamily = new FontFamily("{DynamicResource Volte}");
                        tb.Text = WeaponList[i].Name;
                        tb.TextTrimming = TextTrimming.CharacterEllipsis;
                        tb.ToolTip = WeaponList[i].Name;

                        CheckBox cb = new CheckBox();
                        cb.Margin = new Thickness(2);
                        cb.VerticalAlignment = VerticalAlignment.Center;
                        cb.HorizontalAlignment = HorizontalAlignment.Right;
                        cb.SetResourceReference(Control.BackgroundProperty, "SectionBackgroundBrush");
                        int bit = WeaponList[i].Bit;

                        cb.Checked += delegate
                        {
                            Functions.Write.writebinary(bit, temp_offset + teamoffset, true);
                        };
                        cb.Unchecked += delegate
                        {
                            Functions.Write.writebinary(bit, temp_offset + teamoffset, false);
                        };

                        offsets.Add(temp_offset);
                        bits.Add(WeaponList[i].Bit);
                        cbs.Add(cb);

                        g.Children.Add(tb);
                        g.Children.Add(cb);
                        Grid.SetColumn(tb, 0);
                        Grid.SetColumn(cb, 1);
                        Grid.SetRow(tb, i);
                        Grid.SetRow(cb, i);
                        weaponList.Children.Add(g);
                    }

                    dispatcherTimer.Tick += delegate
                    {
                        CheckValues();
                    };
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
                    dispatcherTimer.Start();
                }
            }
        }

        public static readonly DependencyProperty ChangeTeamProperty = DependencyProperty.Register(
            "ChangeTeam",
            typeof(Team),
            typeof(WeaponInventoryControl));

        private void CheckValues()
        {
            if (WeaponList == null)
                return;

            for (int i = 0; i < WeaponList.Count; i++)
            {
                Functions.Read.checkbinary(bits[i], offsets[i] + teamoffset, cbs[i]);
            }
        }

        private void ChangeTeamOffset()
        {
            switch (ChangeTeam)
            {
                case Team._1:
                    teamoffset = 0;
                    break;
                case Team._2:
                    teamoffset = 1 * GTA.Offsets.Editor.team_NEXT;
                    break;
                case Team._3:
                    teamoffset = 2 * GTA.Offsets.Editor.team_NEXT;
                    break;
                case Team._4:
                    teamoffset = 3 * GTA.Offsets.Editor.team_NEXT;
                    break;
                case Team.All:
                    break;
                default:
                    break;
            }
        }

        public Team ChangeTeam
        {
            get { return (Team)GetValue(ChangeTeamProperty); }
            set 
            { 
                SetValue(ChangeTeamProperty, value);
                ChangeTeamOffset();
                CheckValues();
            }
        }

        public enum Team
        {
            _1,
            _2,
            _3,
            _4,
            All
        }

        public WeaponInventoryControl()
        {
            ChangeTeam = Team._1;
            InitializeComponent();
        }
    }
}
