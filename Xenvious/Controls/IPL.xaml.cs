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
    /// Interaction logic for IPL.xaml
    /// </summary>
    public partial class IPL : UserControl
    {
        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(
            "HeaderText",
            typeof(string),
            typeof(IPL));

        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }

        public static readonly DependencyProperty IPLEntryProperty = DependencyProperty.Register(
            "IPLEntry",
            typeof(GTA.IPLEntry),
            typeof(IPL));

        public GTA.IPLEntry IPLEntry
        {
            get { return (GTA.IPLEntry)GetValue(IPLEntryProperty); }
            set 
            {
                SetValue(IPLEntryProperty, value);

                HeaderText = IPLEntry.name;

                locx.Text = IPLEntry.vector3.X.ToString();
                locy.Text = IPLEntry.vector3.Y.ToString();
                locz.Text = IPLEntry.vector3.Z.ToString();

                DispatcherTimer dispatcherTimer = new DispatcherTimer();

                dispatcherTimer.Tick += delegate
                {
                    List<bool> checkbits = new List<bool>();

                    foreach (var item in IPLEntry.bits)
                    {
                        checkbits.Add(Functions.Read.checkbinary(item.bit, item.offset));
                    }

                    cbenable.IsChecked = !checkbits.Contains(false);
                };
                dispatcherTimer.Interval = new TimeSpan(1000);
                dispatcherTimer.Start();
            }
        }


        public IPL()
        {
            InitializeComponent();
        }

        private void cbenable_Checked(object sender, RoutedEventArgs e)
        {
            if (MainWindow.m.IsProcOpen)
            {
                foreach (var item in IPLEntry.bits)
                {
                    Functions.Write.writebinary(item.bit, item.offset, cbenable);
                }
            }
        }

        private void Btnteleport_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.m.IsProcOpen)
            {
                GTA.Teleport(IPLEntry.vector3);
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Clipboard.SetText($"{{ \n\r\t\"x\": {IPLEntry.vector3.X},\n\r\t\"y\": {IPLEntry.vector3.Y},\n\r\t \"z\": {IPLEntry.vector3.Z}\n\r}}");
            }
        }
    }
}
