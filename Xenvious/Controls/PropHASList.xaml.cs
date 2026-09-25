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

namespace Xenvious
{
    /// <summary>
    /// Interaktionslogik für PropHASList.xaml
    /// </summary>
    public partial class PropHASList : UserControl
    {
        public PropHASList()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PropListProperty = DependencyProperty.Register(
            "PropList",
            typeof(List<PropHAS>),
            typeof(PropHASList));

        public List<PropHAS> HasList
        {
            get { return (List<PropHAS>)GetValue(PropListProperty); }
            set 
            {                                                                                                                                
                SetValue(PropListProperty, value);
                haslist.Children.Clear();
                if (HasList != null)
                {
                    for (int i = 0; i < HasList.Count; i++)
                    {
                        Grid g = new Grid();
                        g.Height = 20;
                        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.15, GridUnitType.Star) });
                        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.25, GridUnitType.Star) });
                        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.60, GridUnitType.Star) });

                        TextBlock tb = new TextBlock();
                        tb.VerticalAlignment = VerticalAlignment.Center;
                        tb.FontSize = 12.0;
                        tb.FontFamily = new FontFamily("{DynamicResource Volte}");
                        tb.Text = HasList[i].Id.ToString();
                        tb.TextTrimming = TextTrimming.CharacterEllipsis;
                        tb.ToolTip = MainWindow.ExistsInPropList(HasList[i].Prop) ? GTA.Editor.PropList.Where(x => x.Integer == HasList[i].Prop).Select(x => x.Name).First() : HasList[i].ToString();

                        TextBlock tb2 = new TextBlock();
                        tb2.VerticalAlignment = VerticalAlignment.Center;
                        tb2.FontSize = 12.0;
                        tb2.FontFamily = new FontFamily("{DynamicResource Volte}");
                        tb2.Text = HasList[i].Time.ToString();
                        tb2.TextTrimming = TextTrimming.CharacterEllipsis;

                        TextBlock tb3 = new TextBlock();
                        tb3.VerticalAlignment = VerticalAlignment.Center;
                        tb3.FontSize = 12.0;
                        tb3.FontFamily = new FontFamily("{DynamicResource Volte}");
                        tb3.Text = HasList[i].ToString();
                        tb3.TextTrimming = TextTrimming.CharacterEllipsis;


                        ////b2.Effect = new DropShadowEffect
                        ////{
                        ////    ShadowDepth = 2,
                        ////    BlurRadius = 5
                        ////};

                        //tempbtnnewvalue.Click += delegate
                        //tempbtnplus.Click += delegate

                        //tempbtnminus.Click += delegate



                        //tempbtnplus.Content = imgplus;
                        //tempbtnminus.Content = imgminus;





                        g.Children.Add(tb);
                        g.Children.Add(tb2);
                        g.Children.Add(tb3);
                        Grid.SetColumn(tb, 0);
                        Grid.SetColumn(tb2, 1);
                        Grid.SetColumn(tb3, 2);
                        haslist.Children.Add(g);
                    }
                }
            }
        }
    }
}
