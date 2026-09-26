using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using Xenvious.AdvancedPlacement;
using Xenvious.Helper_Classes;
using Xenvious.JSON;
using Xenvious.Logging;
using Xenvious.Translation;
using Xenvious.ViewModels;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;
using static Xenvious.Kill;

namespace Xenvious
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static mry.mem m = new mry.mem();
        public static BackgroundWorker mWorker;
        public static DispatcherTimer timercheckgta = new DispatcherTimer();
        public static DispatcherTimer TimerGetJobImage = new DispatcherTimer();
        public static bool isrstar = false;
        public static bool issteam = false;
        public static bool isepic = false;
        public static bool gamemodeWarning = false;
        public static float incrementsize = 1F;

        // Die Local-Basis-Funktionen sind statisch, die Thread-Suche braucht aber den
        // Zwischenspeicher der Instanz.
        public static MainWindow Instance;
        public static Kill kill = new Kill(new int[] { 1, 1, 1, 1 }, new Kill.Values[]
        {
            new Kill.Values(new int[] { 6,6,6,6 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1),
            new Kill.Values(new int[] { 0,0,0,0 }, new int[] { 99999,99999,99999,99999 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1),
            new Kill.Values(new int[] { 0,0,0,0 }, new int[] { 99999,99999,99999,99999 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1),
            new Kill.Values(new int[] { 0,0,0,0 }, new int[] { 99999,99999,99999,99999 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1),
            new Kill.Values(new int[] { 0,0,0,0 }, new int[] { 99999,99999,99999,99999 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1),
            new Kill.Values(new int[] { 0,0,0,0 }, new int[] { 99999,99999,99999,99999 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1),
            new Kill.Values(new int[] { 0,0,0,0 }, new int[] { 99999,99999,99999,99999 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1),
            new Kill.Values(new int[] { 0,0,0,0 }, new int[] { 99999,99999,99999,99999 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1),
            new Kill.Values(new int[] { 0,0,0,0 }, new int[] { 99999,99999,99999,99999 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1),
            new Kill.Values(new int[] { 0,0,0,0 }, new int[] { 99999,99999,99999,99999 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1),
            new Kill.Values(new int[] { 0,0,0,0 }, new int[] { 99999,99999,99999,99999 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1),
            new Kill.Values(new int[] { 0,0,0,0 }, new int[] { 99999,99999,99999,99999 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1),
            new Kill.Values(new int[] { 0,0,0,0 }, new int[] { 99999,99999,99999,99999 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1),
            new Kill.Values(new int[] { 0,0,0,0 }, new int[] { 99999,99999,99999,99999 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1),
            new Kill.Values(new int[] { 0,0,0,0 }, new int[] { 99999,99999,99999,99999 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1),
            new Kill.Values(new int[] { 0,0,0,0 }, new int[] { 99999,99999,99999,99999 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1),
            new Kill.Values(new int[] { 0,0,0,0 }, new int[] { 99999,99999,99999,99999 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, new int[] { 0,0,0,0 }, -1,-1)
        });
        public static List<int> plylfreeze;
        public static List<List<string>> bfmfreeze = new List<List<string>>
        {
            new List<string>{ "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!" },
            new List<string>{ "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!" },
            new List<string>{ "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!" },
            new List<string>{ "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!", "Get back to Action!" }
        };
        public static nrcidcopy nrcidcopy;

        public HashSet<ulong> appliedPatches = new HashSet<ulong>();

        private const string MenuSwitcherPresetsConfigKey = "menuswitcherpresets";

        private static readonly System.Text.Json.JsonSerializerOptions MenuPresetSerializerOptions = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public ObservableCollection<MenuSwitcherPreset> MenuSwitcherPresets { get; } = new ObservableCollection<MenuSwitcherPreset>();

        public Thread runscrPatches;

        private Translator<string, string> _Translation;
        private static readonly string[] LanguageCodes = { "de", "en", "ru", "pl", "fr", "zh_cn" };
        private string _currentLanguageCode = _Language.DefaultCode;
        private bool _suppressLanguageChange;

        public AdvancedPropPlacementViewModel AdvancedPropPlacementVm { get; }

        public Translator<string, string> Translation
        {
            get => _Translation;
            private set
            {
                if (ReferenceEquals(_Translation, value))
                {
                    return;
                }

                _Translation = value;
                OnPropertyChanged();
                AdvancedPropPlacementVm?.RefreshTexts();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            Log.LogDirectory = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)) + "\\Xenvious\\logs\\";

            Log.MinimumLevel = LogLevel.Debug;
            Log.MinimumLevelFile = LogLevel.Debug;

            // Diagnose: bis die Settings geladen sind, schaltet nichts das
            // Dateilog ein -- ein Start, der vorher haengt, hinterlaesst dann
            // keine Spur. Die Settings-Checkbox setzt den Wert spaeter ohnehin
            // wieder auf das, was der Benutzer eingestellt hat.
            Log.FileLoggingEnabled = true;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            // Rendering stays on the GPU. SoftwareOnly (set since the first commit)
            // made effects such as the blurred overlay run on the CPU; the image
            // zoom stuttered. Tested next to GTA: smooth, no flicker.

            var dimensionsProvider = new DimensionsProvider(
                id =>
                {
                    try
                    {
                        return new Global(GTA.Offsets.Editor.custom_dimension_model).SetInt(id);
                    }
                    catch
                    {
                        return false;
                    }
                },
                () => new Global(GTA.Offsets.Editor.custom_dimension_min).GetVector3(),
                () => new Global(GTA.Offsets.Editor.custom_dimension_max).GetVector3());

            var propPlacementService = new PropPlacementService();
            var scriptFeatures = new ScriptFeatureMonitor(
                () => m.IsProcOpen,
                () => IsCreatorRunning(),
                () => GTA.CurrentCreatorName(),
                () => cbsettingsexpscrfeat.IsChecked == true);

            AdvancedPropPlacementVm = new AdvancedPropPlacementViewModel(
                dimensionsProvider,
                propPlacementService,
                new CreatorTemplateService(),
                scriptFeatures,
                () => Functions.Read.GetLocationVec(),
                creatorRefresh,
                () => cbsettingsexpscrfeat.IsChecked = true,
                PropDisplayName,
                (key, fallback) =>
                {
                    try { return Translation?[key] ?? fallback; }
                    catch (KeyNotFoundException) { return fallback; }
                });

            Instance = this;

            InitializeComponent();
            this.DataContext = this;

            // Die Familienliste des Map Movers gleich bauen. Baut man sie erst beim ersten
            // Klick, steht die Karte da mit einer leeren Auswahl daneben.
            MapMoverFamilies();
            LoadMapCalibration();

            LoadConfig();
            Log.Debug("Start Xenvious", source: "init");
            Log.Debug("Init…", source: "init");

            ini_reader ini = new ini_reader(Functions.getRoamingConfigFilePath());
            Log.Debug("Load Config", source: "init");
            InitializeTranslation(ini);

            System.Windows.Forms.Screen screen = null;

            Log.Debug("Set Window size", source: "init");
            try
            {
                System.Windows.Forms.Screen[] searchscreens = System.Windows.Forms.Screen.AllScreens.Where(x => x.DeviceName == ini.ReadString("Settings", "screen")).ToArray();

                screen = System.Windows.Forms.Screen.AllScreens.First();

                if (!String.IsNullOrEmpty(ini.ReadString("Settings", "screen")))
                {
                    if (searchscreens.Any())
                    {
                        screen = searchscreens.First();
                    }
                }
            }
            catch (Exception)
            {

            }

            var screenwidth = screen.WorkingArea.Width;
            var screenheight = screen.WorkingArea.Height;

            if (String.IsNullOrEmpty(ini.ReadString("Settings", "width")))
            {
                this.Width = screenwidth / 100 * 80;
                this.Height = screenheight / 100 * 80;

                this.Left = (screenwidth - Width) / 2;
                this.Top = (screenheight - Height) / 2;
            }
            else
            {
                double width = screenwidth;
                double height = screenheight;
                double left = (screenwidth - this.Width) / 2;
                double top = (screenheight - this.Height) / 2;


                try
                {
                    width = Convert.ToDouble(ini.ReadString("Settings", "width"));
                }
                catch (Exception) { }
                try
                {
                    height = Convert.ToDouble(ini.ReadString("Settings", "height"));
                }
                catch (Exception) { }
                try
                {
                    left = Convert.ToDouble(ini.ReadString("Settings", "startx"));
                }
                catch (Exception) { }
                try
                {
                    top = Convert.ToDouble(ini.ReadString("Settings", "starty"));
                }
                catch (Exception) { }

                if (Convert.ToDouble(width) > screenwidth)
                {
                    this.Width = screenwidth;
                }
                else
                {
                    this.Width = width;
                }

                if (Convert.ToDouble(height) > screenheight)
                {
                    this.Height = screenheight;
                }
                else
                {
                    this.Height = height;
                }

                if (left > (screenwidth - 100))
                {
                    this.Left = (screenwidth - this.Width) / 2;
                }
                else
                {
                    this.Left = left;
                }

                if (top > (screenheight - 100))
                {
                    this.Top = (screenheight - this.Height) / 2;
                }
                else
                {
                    this.Top = top;
                }
            }


            Log.Debug("Load Language", source: "init");
            switch (ini.ReadString("Settings", "translation"))
            {
                case "de":
                    ddlanguage.SelectedIndex = 0;
                    break;
                case "en":
                    ddlanguage.SelectedIndex = 1;
                    break;
                case "ru":
                    ddlanguage.SelectedIndex = 2;
                    break;
                case "pl":
                    ddlanguage.SelectedIndex = 3;
                    break;
                case "fr":
                    ddlanguage.SelectedIndex = 4;
                    break;
                case "zh_cn":
                    ddlanguage.SelectedIndex = 5;
                    break;
                default:
                    ddlanguage.SelectedIndex = 1;
                    break;
            }

            Log.Debug("Load Color", source: "init");
            switch (ini.ReadString("Settings", "color"))
            {
                case "gray":
                    ddcolor.SelectedIndex = 0;
                    break;
                case "white":
                    ddcolor.SelectedIndex = 1;
                    break;
                default:
                    ddcolor.SelectedIndex = 0;
                    break;
            }

            tbsettingsincrementsize.Text = incrementsize.ToString();

            Style s = new Style();
            s.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Collapsed));
            MainPages.ItemContainerStyle = s;
            EditPages.ItemContainerStyle = s;
            PageInnerProps.ItemContainerStyle = s;
            PageInnerRace.ItemContainerStyle = s;
            PageInnerMission.ItemContainerStyle = s;
            PageInnerMod.ItemContainerStyle = s;
            PageInnerCapture.ItemContainerStyle = s;

            //panelTopSwitcher.Visibility = Visibility.Hidden;
            BtnOnlineEnabler.Visibility = Visibility.Collapsed;
            BtnLaunchCreator.Visibility = Visibility.Collapsed;
            mpropspanelmainmain.Visibility = Visibility.Collapsed;


            Log.Debug("Start timercheckgta", source: "init");
            timercheckgta.Interval = new TimeSpan(0, 0, 1);
            timercheckgta.Tick += Timercheckgta_Tick;

            Log.Debug("Start TimerGetJobImage", source: "init");
            TimerGetJobImage.Interval = new TimeSpan(0, 0, 3);
            TimerGetJobImage.Tick += TimerGetJobImage_Tick;

            InitializeComponent();

            Log.Debug("Run ScrPatchesRunner.RunPatcher", source: "init");
            runscrPatches = new Thread(new ThreadStart(ScrPatchesRunner.RunPatcher));
            runscrPatches.Priority = ThreadPriority.Highest;
            runscrPatches.IsBackground = true;
            runscrPatches.Start();

            Log.Debug("Start Main BackgroundWorker", source: "init");
            mWorker = new BackgroundWorker();
            mWorker.DoWork += new DoWorkEventHandler(worker_DoWork);

            StartDashboardStatus();
        }

        public bool globalPtrSanityCheck(long value)
        {
            return (value > 0x10000 && value < 0x7FFFFFF);
        }

        // Reads all 8 description labels: a label shorter than 63 bytes does not end the
        // text, because labels are cut at character boundaries.
        public string getDescribtion()
        {
            var describtion = new StringBuilder();
            try
            {
                for (int i = 0; i < DescriptionChunks; i++)
                    describtion.Append(new Global(GTA.Offsets.Editor.dec + i * 16).GetString());
            }
            catch (Exception) { }
            return describtion.ToString();
        }


        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Hides the "Start GTA" message only for the current launch.
        private bool _hideGtaMessageThisLaunch = false;

        private void BtnDismissSGTALaunch_Click(object sender, RoutedEventArgs e)
        {
            _hideGtaMessageThisLaunch = true;
            SGTAMessage.Visibility = Visibility.Collapsed;
        }

        private void BtnDismissSGTAAlways_Click(object sender, RoutedEventArgs e)
        {
            SGTAMessage.Visibility = Visibility.Collapsed;
            if (cbsettingshidegtamessage != null)
            {
                cbsettingshidegtamessage.IsChecked = true;
            }
            else
            {
                new ini_reader(Functions.getRoamingConfigFilePath()).Write("Settings", "hidegtamessage", true);
            }
        }

        private bool HideGtaMessage()
        {
            return _hideGtaMessageThisLaunch || (cbsettingshidegtamessage != null && (cbsettingshidegtamessage.IsChecked ?? false));
        }


        /// <summary>
        /// Whether the game itself reports the player being in a creator.
        ///
        /// Separate from <see cref="IsInCreator"/>, which also returns true for
        /// the "load values anyway" setting. That override exists so the editor
        /// reads values outside a creator; it does not mean a creator is open,
        /// and anything that acts on the game rather than on our own data has
        /// to ask this instead.
        /// </summary>
        public bool IsCreatorRunning()
        {
            return new Global(GTA.Offsets.Editor.check_creator).Get<int>() == 1;
        }

        public bool IsInCreator()
        {
            return IsCreatorRunning() || cbsettingslva.IsChecked == true;
        }



        public class Rootobject2
        {
            public string status { get; set; }
            public string msg { get; set; }
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainPages.SelectedIndex = -1;

            // Offline mode: no authentication, no updater, no roles.
            // Every feature is unlocked for everyone; the default control
            // visibility is defined directly in MainWindow.xaml.

            // Which GTA V build is running decides which offsets and patches
            // are valid, so detect it before any of that data is read. If no
            // game is running yet the detection defaults to Legacy and the
            // timer below reloads once a game appears.
            GameVariant.Detect();
            Log.Info($"Offline data for GTA {GameVariant.DisplayName(GameVariant.Current)}",
                source: "startup");

            // Load the embedded offline data (offsets + detail lists).
            OffsetLoader.Load();
            BindLoadedData();

            MainPages.SelectedItem = PageDashboard;

            timercheckgta.Start();
            TimerGetJobImage.Start();

            await StartupUpdateCheckAsync();
        }

        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        private void BtnRaceCPAdd_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int cpnum = new Global(GTA.Offsets.Editor.Race.Checkpoints.number).Get<int>();
                if (cpnum < 100 && cpnum > -1)
                {
                    int new_index = cpnum + 1;

                    new Global(GTA.Offsets.Editor.Race.Checkpoints.number).SetInt(new_index);
                    //ddcpno.SelectedIndex = new_index - 1;
                }
            }
        }

        public Translator<string, string> GetLanguageFromSettings()
        {
            var code = new ini_reader(Functions.getRoamingConfigFilePath()).ReadString("Settings", "translation");
            return _Language.FromCode(code);
        }

        private static void WriteLanguageToConfig(string languageCode)
        {
            new ini_reader(Functions.getRoamingConfigFilePath()).Write("Settings", "translation", languageCode);
        }

        private void cb_veh_neon_Unchecked(object sender, RoutedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Vehicle.ncol + GTA.Offsets.Editor.Vehicle.NEXT * ddvehno.SelectedIndex)).SetInt(-1);
        }

        private void OpenLinkSettings(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }


        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.Width = tempSize.Width;
                    this.Height = tempSize.Height;
                    this.WindowState = WindowState.Normal;
                }
                else
                {
                    this.MaxHeight = getWorkingScreen().WorkingArea.Height + 6;
                    this.WindowState = WindowState.Maximized;
                }
            }
        }

        System.Windows.Size tempSize;

        protected override void OnStateChanged(EventArgs e)
        {
            tempSize = new System.Windows.Size(this.Width, this.Height);
            base.OnStateChanged(e);
        }

        public System.Windows.Forms.Screen getWorkingScreen()
        {
            return System.Windows.Forms.Screen.FromRectangle(
                  new System.Drawing.Rectangle(
                    (int)this.Left, (int)this.Top,
                    (int)this.Width, (int)this.Height));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SafeConfig();
        }
        string full_link = "";
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public static List<T> right<T>(List<T> A)
        {
            var tempo = A[0];
            for (var i = 0; i < A.Count() - 1; i++)
            {
                var yolo = A[i + 1];
                A[i + 1] = tempo;
                tempo = yolo;
            }
            A[0] = tempo;
            return A;
        }

        public static List<T> left<T>(List<T> A)
        {
            var tempo = A[A.Count() - 1];
            for (var i = A.Count() - 1; i > 0; i--)
            {
                var yolo = A[i - 1];
                A[i - 1] = tempo;
                tempo = yolo;
            }
            A[A.Count() - 1] = tempo;
            return A;
        }

        private void UIElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        public class Gear
        {
            public int gear;
            public int geard;

            public Gear(int gear, int geard)
            {
                this.gear = gear;
                this.geard = geard;
            }
        }

        private void XenviousImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //var data4 = new byte[] { 0x53, 0x68, 0x6F, 0x70, 0x55, 0x49, 0x5F, 0x54, 0x69, 0x74, 0x6C, 0x65, 0x5F, 0x43, 0x61, 0x72, 0x6D, 0x6F, 0x64, 0x32 };
            //(await m.AOBScanFast(data4)).Select(i => MessageBox.Show(i.ToUInt64().ToString("X")));
        }

        public static T FindUpVisualTree<T>(DependencyObject initial) where T : DependencyObject
        {
            DependencyObject current = initial;

            while (current != null && current.GetType() != typeof(T))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            return current as T;
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var item = FindUpVisualTree<ComboBox>(Mouse.DirectlyOver as UIElement);
            bool parentSV = FindUpVisualTree<ScrollViewer>(item) != null;
            if (item is ComboBox && item.IsFocused == false)
            {
                int index = item.SelectedIndex;
                if (e.Delta > 0)
                {
                    if (index > 0)
                    {
                        item.SelectedIndex = index - 1;
                    }
                }
                else if (e.Delta < 0)
                {
                    if (index < item.Items.Count)
                    {
                        item.SelectedIndex = index + 1;
                    }
                }
                //stop scrollviewer to scroll if mouse is over combobox
                if (parentSV)
                {
                    e.Handled = true;
                }
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();

        public void EnableToolTipsWhenControlDisabled()
        {
            ToolTipService.ShowOnDisabledProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(true));
        }

        public string getSCName()
        {
            // Name from the session struct (+OFFSET_username) when the pattern resolved it;
            // the per-launcher fixed addresses below are only the fallback.
            if (GTA.Offsets.Editor.Session.patternPointer != 0)
            {
                long addr = (long)m.getBaseAddress() + GTA.Offsets.Editor.Session.patternPointer + GTA.Offsets.Editor.Session.username;
                return m.memory(addr.ToString("X")).GetString(64).TrimEnd('\0');
            }
            return issteam ? m.memory(GTA.Offsets.Editor.steam_accname).GetString() : isrstar ? m.memory(GTA.Offsets.Editor.rstar_accname).GetString() : m.memory(GTA.Offsets.Editor.epic_accname).GetString();
        }

        private void cb_race_nononcontact_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void mpropspanelmain_DragEnter(object sender, DragEventArgs e)
        {
            mpropsdrop.Visibility = Visibility.Visible;
        }

        public static bool freeze = false;

        private void tbcapturetext_KeyDown(object sender, KeyEventArgs e)
        {
            DataObject.AddPastingHandler((DependencyObject)sender, new DataObjectPastingEventHandler(CaptureTextPasteHandler));

            if (Encoding.UTF8.GetBytes((sender as TextBox).Text).Length < 255)
            {
                e.Handled = true;
            }
        }

        public void SelectActiveTextBox()
        {
            IInputElement focusedControl = FocusManager.GetFocusedElement(this);

            if (focusedControl is TextBox)
            {
                (focusedControl as TextBox).SelectAll();
            }
        }

        private void tbmissionrloft_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            try
            {

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(@"https://xenvious.com/resources/outfits/" + tbmissionrloft.Text + ".jpg", UriKind.Absolute);
                bitmap.EndInit();

                imgmissionrloft.Source = bitmap;
            }
            catch (Exception)
            {

            }
        }

        private void ddmissionoutfit_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            try
            {

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(@"https://xenvious.com/resources/outfits/" + ((Outfit)ddmissionoutfit.SelectedItem).Value + ".jpg", UriKind.Absolute);
                bitmap.EndInit();

                imgmissionrloftdd.Source = bitmap;
            }
            catch (Exception)
            {

            }
        }

        private void tbmissionoutfitcustom_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            try
            {

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(@"https://xenvious.com/resources/outfits/" + tbmissionoutfitcustom.Text + ".jpg", UriKind.Absolute);
                bitmap.EndInit();

                imgmissionrloftcst.Source = bitmap;
            }
            catch (Exception)
            {

            }
        }}
}
