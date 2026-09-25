using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Xenvious.AdvancedPlacement;
using Xenvious.Helper_Classes;

namespace Xenvious.ViewModels
{
    /// <summary>Shapes offered by the quick start. Each one presets the repeat step.</summary>
    public enum QuickShape
    {
        Straight,
        Curve,
        Wallride,
        Loop,
        Spiral,
        Corkscrew
    }

    public enum StartSource
    {
        Cursor,
        LastProp,
        PropIndex
    }

    /// <summary>One line of the expert result table.</summary>
    public sealed class PlanRow
    {
        public int Index { get; set; }
        public string X { get; set; } = "";
        public string Y { get; set; } = "";
        public string Z { get; set; } = "";
        public string Pitch { get; set; } = "";
        public string Roll { get; set; } = "";
        public string Yaw { get; set; } = "";
    }

    /// <summary>A template slot as shown in the "copy to templates" dialog.</summary>
    public sealed class TemplateSlotRow : ObservableObject
    {
        public int Index { get; set; }
        public string Title { get; set; } = "";
        public string Detail { get; set; } = "";
        public bool InUse { get; set; }

        private bool _isTarget;
        /// <summary>This slot will be written by the current plan.</summary>
        public bool IsTarget
        {
            get => _isTarget;
            set => SetProperty(ref _isTarget, value);
        }
    }

    /// <summary>
    /// Advanced prop placement. Everything the user sets up ends in one plan:
    /// a start pose, a <see cref="RepeatStep"/> and a count. The quick start writes
    /// that plan from a few intuitive choices; the expert view edits it directly.
    /// </summary>
    public partial class AdvancedPropPlacementViewModel : ObservableObject
    {
        // Properties that change the plan: the preview, the table and live mode follow them.
        private static readonly HashSet<string> PlanProperties = new HashSet<string>
        {
            nameof(StartX), nameof(StartY), nameof(StartZ),
            nameof(StartPitch), nameof(StartRoll), nameof(StartYaw), nameof(IncludeStart),
            nameof(Axis), nameof(CustomAxisYaw), nameof(CustomAxisPitch),
            nameof(PivotX), nameof(PivotY), nameof(PivotZ),
            nameof(AngleDeg), nameof(Advance),
            nameof(OffsetX), nameof(OffsetY), nameof(OffsetZ),
            nameof(Count)
        };

        // Quick start inputs: changing one rewrites the plan.
        private static readonly HashSet<string> QuickProperties = new HashSet<string>
        {
            nameof(Shape), nameof(TurnLeft), nameof(Radius), nameof(Rise),
            nameof(Bank), nameof(Overlap), nameof(PieceCount), nameof(AutoPitch)
        };

        private static readonly V3 DefaultBoxMin = new V3(-2, -4, -0.5);
        private static readonly V3 DefaultBoxMax = new V3(2, 4, 0.5);

        private readonly DimensionsProvider _dimensionsProvider;
        private readonly PropPlacementService _placement;
        private readonly CreatorTemplateService _templates;
        private readonly ScriptFeatureMonitor _monitor;
        private readonly Func<Vector3> _getCursorLocation;
        private readonly Action _refreshCreator;
        private readonly Action _enableScriptFeatures;
        private readonly Func<int, string> _modelName;
        private readonly Func<string, string, string> _t;
        private readonly SemaphoreSlim _placementSemaphore = new SemaphoreSlim(1, 1);
        private readonly DispatcherTimer _pollTimer;

        private CancellationTokenSource _liveCts;
        private CancellationTokenSource _dimensionCts;
        private V3 _boxMin = DefaultBoxMin;
        private V3 _boxMax = DefaultBoxMax;
        private int? _modelId;
        private int _lastPlacementCount;
        private bool _lastPlacementWasLive;
        private bool _applyingQuick;
        private bool _checking;
        private DateTime _lastCheck = DateTime.MinValue;
        private ScriptFeatureState _lastPrecheck = ScriptFeatureState.NoGame;
        private int _startPropSource = -1;

        public AdvancedPropPlacementViewModel(
            DimensionsProvider dimensionsProvider,
            PropPlacementService placement,
            CreatorTemplateService templates,
            ScriptFeatureMonitor monitor,
            Func<Vector3> getCursorLocation,
            Action refreshCreator,
            Action enableScriptFeatures,
            Func<int, string> modelName,
            Func<string, string, string> translate)
        {
            _dimensionsProvider = dimensionsProvider ?? throw new ArgumentNullException(nameof(dimensionsProvider));
            _placement = placement ?? throw new ArgumentNullException(nameof(placement));
            _templates = templates ?? throw new ArgumentNullException(nameof(templates));
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _getCursorLocation = getCursorLocation ?? throw new ArgumentNullException(nameof(getCursorLocation));
            _refreshCreator = refreshCreator ?? throw new ArgumentNullException(nameof(refreshCreator));
            _enableScriptFeatures = enableScriptFeatures ?? throw new ArgumentNullException(nameof(enableScriptFeatures));
            _modelName = modelName ?? (_ => "");
            _t = translate ?? ((_, fallback) => fallback);

            ShapeOptions = Array.AsReadOnly((QuickShape[])Enum.GetValues(typeof(QuickShape)));
            AxisOptions = Array.AsReadOnly((StepAxis[])Enum.GetValues(typeof(StepAxis)));

            LoadDimensionsCommand = new AsyncRelayCommand(LoadDimensionsAsync, () => _modelId.HasValue && FeaturesReady);
            PlaceCommand = new AsyncRelayCommand(() => PlaceAsync(false), () => CanPlace);
            UndoCommand = new RelayCommand(UndoPlacement, () => CanUndo);
            UseCursorCommand = new RelayCommand(UseCursor);
            ReadStartPropCommand = new RelayCommand(ReadStartProp);
            UseHoveredCommand = new RelayCommand(() => ModelIdText = HoveredModel.ToString(CultureInfo.InvariantCulture), () => HoveredModel != 0);
            EnableFeaturesCommand = new RelayCommand(EnableFeatures);
            RecheckFeaturesCommand = new AsyncRelayCommand(() => CheckFeaturesAsync(true));
            DetectCommand = new RelayCommand(DetectFromProps);
            NextStepCommand = new RelayCommand(() => QuickStep = Math.Min(QuickStep + 1, QuickStepCount - 1), () => QuickStep < QuickStepCount - 1);
            PreviousStepCommand = new RelayCommand(() => QuickStep = Math.Max(QuickStep - 1, 0), () => QuickStep > 0);
            OpenTemplatesCommand = new RelayCommand(OpenTemplateDialog);
            ConfirmTemplatesCommand = new RelayCommand(WriteTemplates);
            CancelTemplatesCommand = new RelayCommand(() => IsTemplateDialogOpen = false);
            SelectTemplateSlotCommand = new RelayCommand<TemplateSlotRow>(row => { if (row != null) TemplateStartSlot = row.Index; });
            OpenInExpertCommand = new RelayCommand(() => ModeIndex = 1);

            _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(700) };
            _pollTimer.Tick += async (_, __) => await PollAsync().ConfigureAwait(true);

            _applyingQuick = true;
            AnchorModelText = "1899123601"; // Cement Bags Pallet: small, flat, easy to hit with the cursor
            _applyingQuick = false;
            LoadPresets();
            LoadSettings();
            ApplyQuickStart();
            Invalidate();
            UpdateFeatureTexts();
        }

        // ------------------------------------------------------------------
        // Commands
        // ------------------------------------------------------------------
        public IAsyncRelayCommand LoadDimensionsCommand { get; }
        public IAsyncRelayCommand PlaceCommand { get; }
        public IRelayCommand UndoCommand { get; }
        public IRelayCommand UseCursorCommand { get; }
        public IRelayCommand ReadStartPropCommand { get; }
        public IRelayCommand UseHoveredCommand { get; }
        public IRelayCommand EnableFeaturesCommand { get; }
        public IAsyncRelayCommand RecheckFeaturesCommand { get; }
        public IRelayCommand DetectCommand { get; }
        public IRelayCommand NextStepCommand { get; }
        public IRelayCommand PreviousStepCommand { get; }
        public IRelayCommand OpenTemplatesCommand { get; }
        public IRelayCommand ConfirmTemplatesCommand { get; }
        public IRelayCommand CancelTemplatesCommand { get; }
        public IRelayCommand<TemplateSlotRow> SelectTemplateSlotCommand { get; }
        public IRelayCommand OpenInExpertCommand { get; }

        public IReadOnlyList<QuickShape> ShapeOptions { get; }
        public IReadOnlyList<StepAxis> AxisOptions { get; }

        public sealed class AxisChoice
        {
            public StepAxis Value { get; set; }
            public string Label { get; set; } = "";
        }

        /// <summary>Axis list with translated labels (items in a ComboBox popup cannot reach the window's translation).</summary>
        public IReadOnlyList<AxisChoice> AxisChoices => new[]
        {
            new AxisChoice { Value = StepAxis.None, Label = _t("adv_axis_none", "Keine (nur verschieben)") },
            new AxisChoice { Value = StepAxis.WorldUp, Label = _t("adv_axis_world", "Welt senkrecht (Z)") },
            new AxisChoice { Value = StepAxis.LocalX, Label = _t("adv_axis_x", "Teil X (quer)") },
            new AxisChoice { Value = StepAxis.LocalY, Label = _t("adv_axis_y", "Teil Y (längs)") },
            new AxisChoice { Value = StepAxis.LocalZ, Label = _t("adv_axis_z", "Teil Z (hoch)") },
            new AxisChoice { Value = StepAxis.Custom, Label = _t("adv_axis_custom", "Frei") }
        };

        /// <summary>Raised whenever the plan changes, so the 3D preview can redraw.</summary>
        public event EventHandler PreviewInvalidated;

        /// <summary>
        /// Bumped when the preview should frame the build again (new prop, new shape).
        /// Other changes, like turning the first piece, redraw in place so the view holds still.
        /// </summary>
        public int PreviewFrameVersion { get; private set; }

        // ------------------------------------------------------------------
        // Script features + hovered prop
        // ------------------------------------------------------------------
        private ScriptFeatureState _featureState = ScriptFeatureState.NoGame;
        public ScriptFeatureState FeatureState
        {
            get => _featureState;
            private set
            {
                if (SetProperty(ref _featureState, value))
                {
                    OnPropertyChanged(nameof(FeaturesReady));
                    OnPropertyChanged(nameof(CanEnableFeatures));
                    OnPropertyChanged(nameof(CanPlace));
                    OnPropertyChanged(nameof(CanUseTemplates));
                    LoadDimensionsCommand.NotifyCanExecuteChanged();
                    PlaceCommand.NotifyCanExecuteChanged();
                    UpdateFeatureTexts();
                    if (FeaturesReady && _modelId.HasValue && !HasDimensions)
                    {
                        ScheduleDimensionLoad();
                    }
                }
            }
        }

        public bool FeaturesReady => FeatureState == ScriptFeatureState.Active;
        public bool CanEnableFeatures => FeatureState == ScriptFeatureState.Disabled;

        private string _featureStatusText = "";
        public string FeatureStatusText { get => _featureStatusText; private set => SetProperty(ref _featureStatusText, value); }

        private string _featureHintText = "";
        /// <summary>Why buttons are disabled; also their tooltip.</summary>
        public string FeatureHintText { get => _featureHintText; private set => SetProperty(ref _featureHintText, value); }

        private int _hoveredModel;
        public int HoveredModel
        {
            get => _hoveredModel;
            private set
            {
                if (SetProperty(ref _hoveredModel, value))
                {
                    string name = value == 0 ? "" : _modelName(value);
                    HoveredText = value == 0
                        ? _t("adv_hover_none", "— im Creator auf ein Prop zielen")
                        : (string.IsNullOrEmpty(name) ? value.ToString(CultureInfo.InvariantCulture) : $"{name}  ({value})");
                    UseHoveredCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _hoveredText = "";
        public string HoveredText { get => _hoveredText; private set => SetProperty(ref _hoveredText, value); }

        /// <summary>Re-reads every text the view model builds itself (language switch).</summary>
        public void RefreshTexts()
        {
            UpdateFeatureTexts();
            int hovered = _hoveredModel;
            _hoveredModel = -1;
            HoveredModel = hovered;
            OnPropertyChanged(nameof(QuickStepTitle));
            OnPropertyChanged(nameof(RiseLabel));
            OnPropertyChanged(nameof(DirectionALabel));
            OnPropertyChanged(nameof(DirectionBLabel));
            OnPropertyChanged(nameof(AxisChoices));
            RefreshPresetCards();
            ApplyQuickStart();
            Invalidate();
        }

        /// <summary>Called by the view: poll only while the page is shown.</summary>
        public void SetActive(bool active)
        {
            if (active)
            {
                RefreshPresetCards(); // prop names are only known once the prop list is loaded
                _lastCheck = DateTime.MinValue;
                _pollTimer.Start();
                _ = PollAsync();
            }
            else
            {
                _pollTimer.Stop();
            }
        }

        private async Task PollAsync()
        {
            // The limit follows the game edition, which is only known once the game was
            // found -- usually after this view model was built with the Legacy value.
            if (_shownPropLimit != PropLimit)
            {
                _shownPropLimit = PropLimit;
                OnPropertyChanged(nameof(PropLimit));
            }

            HoveredModel = FeaturesReady ? _monitor.ReadHoveredModel() : 0;

            ScriptFeatureState pre = _monitor.Precheck();
            bool changed = pre != _lastPrecheck;
            _lastPrecheck = pre;
            if (pre != ScriptFeatureState.Checking)
            {
                FeatureState = pre;
                return;
            }
            // Probe when the situation changed, and re-probe a failed or pending check.
            bool due = changed
                || (FeatureState != ScriptFeatureState.Active && DateTime.UtcNow - _lastCheck > TimeSpan.FromSeconds(4))
                || DateTime.UtcNow - _lastCheck > TimeSpan.FromSeconds(30);
            if (due)
            {
                await CheckFeaturesAsync(false).ConfigureAwait(true);
            }
        }

        private async Task CheckFeaturesAsync(bool manual)
        {
            if (_checking || IsPlacing)
            {
                return;
            }
            _checking = true;
            try
            {
                if (manual || FeatureState != ScriptFeatureState.Active)
                {
                    FeatureState = ScriptFeatureState.Checking;
                }
                FeatureState = await _monitor.CheckAsync().ConfigureAwait(true);
                _lastCheck = DateTime.UtcNow;
                // The box is cached, so a probe (which writes its own models into the
                // dimension global) only needs a load when none was loaded yet.
                if (FeaturesReady && _modelId.HasValue && !HasDimensions)
                {
                    ScheduleDimensionLoad();
                }
            }
            finally
            {
                _checking = false;
            }
        }

        private void EnableFeatures()
        {
            _enableScriptFeatures();
            _lastCheck = DateTime.MinValue;
            FeatureState = ScriptFeatureState.Checking;
        }

        private void UpdateFeatureTexts()
        {
            switch (FeatureState)
            {
                case ScriptFeatureState.Active:
                    FeatureStatusText = _t("adv_state_active", "Aktiv");
                    FeatureHintText = "";
                    break;
                case ScriptFeatureState.Checking:
                    FeatureStatusText = _t("adv_state_checking", "Wird geprüft …");
                    FeatureHintText = FeatureStatusText;
                    break;
                case ScriptFeatureState.Disabled:
                    FeatureStatusText = _t("adv_state_disabled", "Ausgeschaltet");
                    FeatureHintText = _t("adv_hint_disabled", "Ohne die Script-Features lassen sich keine Maße laden. Mit „Aktivieren“ einschalten.");
                    break;
                case ScriptFeatureState.NoResponse:
                    FeatureStatusText = _t("adv_state_noresponse", "Antworten nicht");
                    FeatureHintText = _t("adv_hint_noresponse", "Eingeschaltet, aber die eingeschleuste Funktion läuft nicht. Creator neu betreten oder GTA neu starten.");
                    break;
                case ScriptFeatureState.Unsupported:
                    FeatureStatusText = _t("adv_state_unsupported", "In diesem Creator nicht verfügbar");
                    FeatureHintText = _t("adv_hint_unsupported", "Deathmatch- und Survival-Creator bekommen keine eingeschleusten Funktionen. Race, LTS oder Capture nutzen.");
                    break;
                case ScriptFeatureState.NoCreator:
                    FeatureStatusText = _t("adv_state_nocreator", "Kein Creator geöffnet");
                    FeatureHintText = FeatureStatusText;
                    break;
                default:
                    FeatureStatusText = _t("adv_state_nogame", "GTA läuft nicht");
                    FeatureHintText = FeatureStatusText;
                    break;
            }
        }

        // ------------------------------------------------------------------
        // Model and dimensions
        // ------------------------------------------------------------------
        private string _modelIdText = "";
        public string ModelIdText { get => _modelIdText; set => SetProperty(ref _modelIdText, value); }

        private string _modelInfoText = "";
        public string ModelInfoText { get => _modelInfoText; private set => SetProperty(ref _modelInfoText, value); }

        private bool _hasDimensions;
        public bool HasDimensions
        {
            get => _hasDimensions;
            private set
            {
                if (SetProperty(ref _hasDimensions, value))
                {
                    OnPropertyChanged(nameof(CanPlace));
                    OnPropertyChanged(nameof(CanUseTemplates));
                    PlaceCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

        /// <summary>Bounding box of the prop around its pivot, for the preview.</summary>
        public V3 BoxMin => _boxMin;
        public V3 BoxMax => _boxMax;
        public V3 BoxSize => _boxMax - _boxMin;

        private void HandleModelChanged()
        {
            string text = ModelIdText;
            HasDimensions = false;
            _boxMin = DefaultBoxMin;
            _boxMax = DefaultBoxMax;
            if (string.IsNullOrWhiteSpace(text))
            {
                _modelId = null;
                ModelInfoText = _t("adv_model_enter", "Modell-ID oder Name eingeben, oder ein Prop im Creator anvisieren und übernehmen.");
            }
            else if (ModelIdParser.TryParseModelIdInt(text, out var parsed, out _))
            {
                _modelId = parsed;
                string name = _modelName(parsed);
                ModelInfoText = string.IsNullOrEmpty(name) ? parsed.ToString(CultureInfo.InvariantCulture) : name;
                ScheduleDimensionLoad();
            }
            else
            {
                _modelId = null;
                ModelInfoText = _t("adv_model_invalid", "Modell nicht erkannt.");
            }
            LoadDimensionsCommand.NotifyCanExecuteChanged();
            ApplyQuickStart();
            Invalidate();
        }

        private async void ScheduleDimensionLoad()
        {
            _dimensionCts?.Cancel();
            var cts = new CancellationTokenSource();
            _dimensionCts = cts;
            try
            {
                await Task.Delay(300, cts.Token).ConfigureAwait(true);
                if (!cts.IsCancellationRequested && FeaturesReady && _modelId.HasValue)
                {
                    await LoadDimensionsAsync().ConfigureAwait(true);
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        private async Task LoadDimensionsAsync()
        {
            if (!_modelId.HasValue || !FeaturesReady || IsBusy)
            {
                return;
            }
            IsBusy = true;
            try
            {
                var result = await _dimensionsProvider.RequestDimensionsAsync(_modelId.Value, CancellationToken.None).ConfigureAwait(true);
                var min = new V3(result.Min.X, result.Min.Y, result.Min.Z);
                var max = new V3(result.Max.X, result.Max.Y, result.Max.Z);
                V3 size = max - min;
                if (!result.IsSuccess || size.Length < 0.01)
                {
                    HasDimensions = false;
                    ModelInfoText = _t("adv_dims_failed", "Maße konnten nicht geladen werden – ist das ein gültiges Prop?");
                    return;
                }
                bool boxChanged = (min - _boxMin).Length > 1e-4 || (max - _boxMax).Length > 1e-4;
                _boxMin = min;
                _boxMax = max;
                HasDimensions = true;
                if (boxChanged)
                {
                    PreviewFrameVersion++;
                }
                string name = _modelName(_modelId.Value);
                ModelInfoText = string.Format(CultureInfo.InvariantCulture, "{0}   {1:0.##} × {2:0.##} × {3:0.##} m",
                    string.IsNullOrEmpty(name) ? _modelId.Value.ToString(CultureInfo.InvariantCulture) : name, size.X, size.Y, size.Z);
                ApplyQuickStart();
                Invalidate();
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ------------------------------------------------------------------
        // Mode and quick start
        // ------------------------------------------------------------------
        private int _modeIndex;
        /// <summary>0 = quick start, 1 = expert.</summary>
        public int ModeIndex
        {
            get => _modeIndex;
            set
            {
                if (SetProperty(ref _modeIndex, value) && value == 0)
                {
                    ApplyQuickStart();
                }
            }
        }

        public const int QuickStepCount = 5;

        private int _quickStep;
        public int QuickStep
        {
            get => _quickStep;
            set
            {
                if (SetProperty(ref _quickStep, value))
                {
                    OnPropertyChanged(nameof(QuickStepTitle));
                    NextStepCommand.NotifyCanExecuteChanged();
                    PreviousStepCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private QuickShape _shape = QuickShape.Curve;
        public QuickShape Shape { get => _shape; set => SetProperty(ref _shape, value); }

        private bool _turnLeft; // right by default
        /// <summary>Left/right for curves, up/down for loops.</summary>
        public bool TurnLeft { get => _turnLeft; set => SetProperty(ref _turnLeft, value); }

        private double _radius = 20;
        public double Radius { get => _radius; set => SetProperty(ref _radius, Math.Max(0.5, value)); }

        private double _rise;
        /// <summary>Height per piece (spiral, curve, straight), side drift per piece (loop), forward per piece (corkscrew).</summary>
        public double Rise { get => _rise; set => SetProperty(ref _rise, value); }

        private bool _autoPitch;
        /// <summary>Tilt the first piece so it follows the rise (a climbing curve drives like a ramp).</summary>
        public bool AutoPitch { get => _autoPitch; set => SetProperty(ref _autoPitch, value); }

        public bool ShapeSupportsAutoPitch => Shape == QuickShape.Straight || Shape == QuickShape.Curve || Shape == QuickShape.Wallride || Shape == QuickShape.Spiral;

        private double _bank = 15;
        public double Bank { get => _bank; set => SetProperty(ref _bank, value); }

        private double _overlap;
        /// <summary>-0.5 = half a piece gap, 0 = flush, 0.5 = half overlapped.</summary>
        public double Overlap { get => _overlap; set => SetProperty(ref _overlap, Math.Max(-3, Math.Min(0.95, value))); }

        private PlacementPreset _lockedPreset;

        /// <summary>A preset with an exact step drives the quick start; shape and size are fixed.</summary>
        public bool IsPresetLocked => _lockedPreset != null;

        public string LockedPresetText => _lockedPreset == null ? "" : string.Format(CultureInfo.CurrentCulture,
            _t("adv_locked_text", "Form und Schritt kommen aus dem Preset „{0}“. Anzahl und Startpunkt lassen sich ändern; eine Änderung hier löst das Preset."),
            _lockedPreset.NameKey != null ? _t(_lockedPreset.NameKey, _lockedPreset.Name) : _lockedPreset.Name);

        private IRelayCommand _unlockPresetCommand;
        public IRelayCommand UnlockPresetCommand => _unlockPresetCommand ??= new RelayCommand(() => SetLockedPreset(null));

        private void SetLockedPreset(PlacementPreset preset)
        {
            _lockedPreset = preset;
            OnPropertyChanged(nameof(IsPresetLocked));
            OnPropertyChanged(nameof(LockedPresetText));
            ApplyQuickStart();
        }

        /// <summary>"flush", "20 % gap", "30 % overlapped" for the joint slider.</summary>
        public string OverlapText => Math.Abs(Overlap) < 0.005
            ? _t("adv_fit_flush", "bündig")
            : Overlap < 0
                ? string.Format(CultureInfo.CurrentCulture, _t("adv_fit_gap_pct", "{0:0} % Lücke"), -Overlap * 100)
                : string.Format(CultureInfo.CurrentCulture, _t("adv_fit_overlap_pct", "{0:0} % ineinander"), Overlap * 100);

        private int _pieceCount = 18;
        /// <summary>Upper bound of the piece count slider; depends on the game edition.</summary>
        public int PropLimit => PropPlacementService.PropLimit;
        private int _shownPropLimit;

        public int PieceCount { get => _pieceCount; set => SetProperty(ref _pieceCount, Math.Max(1, Math.Min(PropPlacementService.PropLimit, value))); }

        private static readonly string[] StepTitles = { "Preset oder Prop", "Form wählen", "Größe und Anschluss", "Start und Ausrichtung", "Fertig" };

        public string QuickStepTitle => string.Format(CultureInfo.CurrentCulture, _t("adv_qs_step_of", "Schritt {0} von {1}"), QuickStep + 1, QuickStepCount)
            + " · " + _t("adv_qs_step" + QuickStep, StepTitles[Math.Max(0, Math.Min(StepTitles.Length - 1, QuickStep))]);

        /// <summary>What "Rise" means for the chosen shape.</summary>
        public string RiseLabel
        {
            get
            {
                switch (Shape)
                {
                    case QuickShape.Loop: return _t("adv_rise_loop", "Seitenversatz pro Teil (m)");
                    case QuickShape.Corkscrew: return _t("adv_rise_cork", "Vorschub pro Teil (m)");
                    case QuickShape.Straight: return _t("adv_rise_straight", "Steigung pro Teil (m)");
                    case QuickShape.Wallride: return _t("adv_rise_wallride", "Anstieg pro Teil (m, 0 = waagerecht)");
                    default: return _t("adv_rise_height", "Höhe pro Teil (m)");
                }
            }
        }

        public string DirectionALabel => Shape == QuickShape.Loop ? _t("adv_dir_up", "Nach oben") : _t("adv_dir_left", "Links");
        public string DirectionBLabel => Shape == QuickShape.Loop ? _t("adv_dir_down", "Nach unten") : _t("adv_dir_right", "Rechts");

        public bool ShapeIsStraight => Shape == QuickShape.Straight;
        public bool ShapeIsCurved => Shape != QuickShape.Straight;
        public bool ShapeIsWallride => Shape == QuickShape.Wallride;
        public bool ShapeIsLoop => Shape == QuickShape.Loop;

        private string _quickSummary = "";
        public string QuickSummary { get => _quickSummary; private set => SetProperty(ref _quickSummary, value); }

        /// <summary>Writes the plan from the quick start choices.</summary>
        private void ApplyQuickStart()
        {
            // The expert edits the plan directly; only the quick start page rewrites it.
            if (_applyingQuick || ModeIndex != 0)
            {
                return;
            }
            _applyingQuick = true;
            try
            {
                OnPropertyChanged(nameof(OverlapText));
                if (_lockedPreset != null)
                {
                    // An exact preset step: only the count follows the quick start.
                    RepeatStep locked = _lockedPreset.ToStep();
                    SetStep(locked);
                    Count = PieceCount;
                    QuickSummary = string.Format(CultureInfo.CurrentCulture,
                        _t("adv_summary_locked", "{0} Teile · {1:0.#}° pro Teil · aus Preset"), PieceCount, Math.Abs(locked.AngleDeg));
                    return;
                }
                V3 size = BoxSize;
                double spacingFactor = 1 - Overlap;
                double dir = TurnLeft ? 1 : -1;
                double r = Radius;
                var step = new RepeatStep();

                if (Shape == QuickShape.Wallride && StartSource == StartSource.Cursor)
                {
                    // Lean the pieces into the turn; the turn itself stays level.
                    StartRoll = Math.Round(-dir * Bank, 3);
                }

                if (AutoPitch && ShapeSupportsAutoPitch && StartSource == StartSource.Cursor)
                {
                    // Nose up by the climb over one piece: rise against the horizontal part of the step.
                    double stepLength = size.Y * spacingFactor;
                    double horizontal = Math.Sqrt(Math.Max(1e-6, stepLength * stepLength - Rise * Rise));
                    StartPitch = Math.Round(Math.Atan2(Rise, horizontal) * 180 / Math.PI, 3);
                }

                // A pivot "r metres to the left" means left on the ground, whatever the
                // first piece's pitch or roll: express that world direction in its frame.
                PropPose start = StartPose;
                V3 levelSide = Rot3.AxisAngle(V3.UnitZ, StartYaw) * new V3(-dir * r, 0, 0);
                V3 levelPivot = start.Rotation.Transposed() * levelSide;

                switch (Shape)
                {
                    case QuickShape.Straight:
                        step.Axis = StepAxis.None;
                        step.Offset = new V3(0, size.Y * spacingFactor, Rise);
                        break;
                    case QuickShape.Curve:
                    case QuickShape.Wallride:
                    case QuickShape.Spiral:
                        step.Axis = StepAxis.WorldUp;
                        step.Pivot = levelPivot;
                        step.Advance = Rise;
                        break;
                    case QuickShape.Loop:
                        // Up: pivot above, nose goes up. Down: pivot below.
                        step.Axis = StepAxis.LocalX;
                        step.Pivot = new V3(0, 0, dir * r);
                        step.Advance = Rise;
                        break;
                    case QuickShape.Corkscrew:
                        step.Axis = StepAxis.LocalY;
                        step.Pivot = new V3(-dir * r, 0, 0);
                        step.Advance = Rise;
                        break;
                }

                int count = PieceCount;
                double magnitude = 0;
                if (step.Axis != StepAxis.None)
                {
                    // Turn per piece so neighbours sit one piece length apart along the
                    // direction they actually travel (which depends on the turn itself).
                    double length = size.Y;
                    magnitude = 1;
                    for (int i = 0; i < 4; i++)
                    {
                        magnitude = RepeatPlanner.AngleForSpacing(length * spacingFactor, r, step.Advance);
                        if (magnitude < 0.05) magnitude = 0.05;
                        step.AngleDeg = dir * magnitude;
                        length = RepeatPlanner.ExtentAlong(size, RepeatPlanner.TravelDirection(step));
                    }
                }

                SetStep(step);
                Count = count;

                QuickSummary = step.Axis == StepAxis.None
                    ? string.Format(CultureInfo.CurrentCulture, _t("adv_summary_straight", "{0} Teile, {1:0.##} m Abstand"), count, size.Y * spacingFactor)
                    : string.Format(CultureInfo.CurrentCulture, _t("adv_summary_curved2", "{0} Teile · {1:0.#}° pro Teil · zusammen {2:0}° · Radius {3:0.#} m"), count, magnitude, magnitude * count, r);
            }
            finally
            {
                _applyingQuick = false;
            }
            Invalidate();
        }

        private void SetStep(RepeatStep step)
        {
            Axis = step.Axis;
            PivotX = Math.Round(step.Pivot.X, 4);
            PivotY = Math.Round(step.Pivot.Y, 4);
            PivotZ = Math.Round(step.Pivot.Z, 4);
            AngleDeg = Math.Round(step.AngleDeg, 4);
            Advance = Math.Round(step.Advance, 4);
            OffsetX = Math.Round(step.Offset.X, 4);
            OffsetY = Math.Round(step.Offset.Y, 4);
            OffsetZ = Math.Round(step.Offset.Z, 4);
            if (step.Axis == StepAxis.Custom)
            {
                V3 a = step.CustomAxis.Normalized();
                CustomAxisPitch = Math.Round(Math.Asin(Math.Max(-1, Math.Min(1, a.Z))) * 180 / Math.PI, 4);
                CustomAxisYaw = Math.Round(Math.Atan2(-a.X, a.Y) * 180 / Math.PI, 4);
            }
        }

        // ------------------------------------------------------------------
        // The plan (expert fields)
        // ------------------------------------------------------------------
        private StartSource _startSource = StartSource.Cursor;
        public StartSource StartSource { get => _startSource; set => SetProperty(ref _startSource, value); }

        private int _startPropIndex;
        public int StartPropIndex { get => _startPropIndex; set => SetProperty(ref _startPropIndex, Math.Max(0, value)); }

        private double _startX, _startY, _startZ, _startPitch, _startRoll, _startYaw;
        public double StartX { get => _startX; set => SetProperty(ref _startX, value); }
        public double StartY { get => _startY; set => SetProperty(ref _startY, value); }
        public double StartZ { get => _startZ; set => SetProperty(ref _startZ, value); }
        public double StartPitch { get => _startPitch; set => SetProperty(ref _startPitch, value); }
        public double StartRoll { get => _startRoll; set => SetProperty(ref _startRoll, value); }
        public double StartYaw { get => _startYaw; set => SetProperty(ref _startYaw, value); }

        private bool _includeStart = true;
        /// <summary>Place a piece at the start pose too (off when continuing from a placed prop).</summary>
        public bool IncludeStart { get => _includeStart; set => SetProperty(ref _includeStart, value); }

        private StepAxis _axis = StepAxis.WorldUp;
        public StepAxis Axis { get => _axis; set { if (SetProperty(ref _axis, value)) OnPropertyChanged(nameof(AxisIsCustom)); } }
        public bool AxisIsCustom => Axis == StepAxis.Custom;

        private double _customAxisYaw, _customAxisPitch = 90;
        /// <summary>Custom axis direction in the piece's frame: yaw from +Y (forward) and pitch up.</summary>
        public double CustomAxisYaw { get => _customAxisYaw; set => SetProperty(ref _customAxisYaw, value); }
        public double CustomAxisPitch { get => _customAxisPitch; set => SetProperty(ref _customAxisPitch, value); }

        private double _pivotX, _pivotY, _pivotZ, _angleDeg, _advance, _offsetX, _offsetY, _offsetZ;
        public double PivotX { get => _pivotX; set => SetProperty(ref _pivotX, value); }
        public double PivotY { get => _pivotY; set => SetProperty(ref _pivotY, value); }
        public double PivotZ { get => _pivotZ; set => SetProperty(ref _pivotZ, value); }
        public double AngleDeg { get => _angleDeg; set => SetProperty(ref _angleDeg, value); }
        public double Advance { get => _advance; set => SetProperty(ref _advance, value); }
        public double OffsetX { get => _offsetX; set => SetProperty(ref _offsetX, value); }
        public double OffsetY { get => _offsetY; set => SetProperty(ref _offsetY, value); }
        public double OffsetZ { get => _offsetZ; set => SetProperty(ref _offsetZ, value); }

        private int _count = 10;
        public int Count { get => _count; set => SetProperty(ref _count, Math.Max(1, Math.Min(PropPlacementService.PropLimit, value))); }

        private int _detectA, _detectB = 1;
        public int DetectA { get => _detectA; set => SetProperty(ref _detectA, Math.Max(0, value)); }
        public int DetectB { get => _detectB; set => SetProperty(ref _detectB, Math.Max(0, value)); }

        public ObservableCollection<PlanRow> Rows { get; } = new ObservableCollection<PlanRow>();

        private string _planInfoText = "";
        public string PlanInfoText { get => _planInfoText; private set => SetProperty(ref _planInfoText, value); }

        public RepeatStep BuildStep()
        {
            double yaw = CustomAxisYaw * Math.PI / 180, pitch = CustomAxisPitch * Math.PI / 180;
            return new RepeatStep
            {
                Axis = Axis,
                CustomAxis = new V3(-Math.Sin(yaw) * Math.Cos(pitch), Math.Cos(yaw) * Math.Cos(pitch), Math.Sin(pitch)),
                Pivot = new V3(PivotX, PivotY, PivotZ),
                AngleDeg = AngleDeg,
                Advance = Advance,
                Offset = new V3(OffsetX, OffsetY, OffsetZ)
            };
        }

        public PropPose StartPose => PropPose.FromGta(new V3(StartX, StartY, StartZ), new V3(StartPitch, StartRoll, StartYaw));

        /// <summary>The props this plan places, in order.</summary>
        public List<PropPose> BuildPlan()
        {
            try
            {
                return RepeatPlanner.Build(StartPose, BuildStep(), Count, IncludeStart);
            }
            catch
            {
                return new List<PropPose>();
            }
        }

        private void Invalidate()
        {
            List<PropPose> plan = BuildPlan();
            Rows.Clear();
            for (int i = 0; i < plan.Count; i++)
            {
                V3 e = plan[i].EulerDeg;
                Rows.Add(new PlanRow
                {
                    Index = i + 1,
                    X = plan[i].Position.X.ToString("0.###", CultureInfo.InvariantCulture),
                    Y = plan[i].Position.Y.ToString("0.###", CultureInfo.InvariantCulture),
                    Z = plan[i].Position.Z.ToString("0.###", CultureInfo.InvariantCulture),
                    Pitch = e.X.ToString("0.##", CultureInfo.InvariantCulture),
                    Roll = e.Y.ToString("0.##", CultureInfo.InvariantCulture),
                    Yaw = e.Z.ToString("0.##", CultureInfo.InvariantCulture)
                });
            }

            int current = SafePropCount();
            PlanInfoText = string.Format(CultureInfo.CurrentCulture,
                _t("adv_plan_info", "{0} neue Props · im Job: {1} / {2}"), plan.Count, current, PropPlacementService.PropLimit);

            PreviewInvalidated?.Invoke(this, EventArgs.Empty);
            TriggerLivePlacement();
        }

        private int SafePropCount()
        {
            try { return FeatureState == ScriptFeatureState.NoGame ? 0 : _placement.GetCurrentPropCount(); }
            catch { return 0; }
        }

        // ------------------------------------------------------------------
        // Remember the last setup (roaming config.ini, [ADVPLACEMENT])
        // ------------------------------------------------------------------
        private const string SettingsSection = "ADVPLACEMENT";

        // Everything that describes the build, not where it is (the start position
        // belongs to one spot in one job and is not restored).
        private static readonly string[] SavedProperties =
        {
            nameof(ModelIdText), nameof(ModeIndex), nameof(Shape), nameof(TurnLeft), nameof(Radius), nameof(Rise),
            nameof(Bank), nameof(Overlap), nameof(PieceCount), nameof(AutoPitch), nameof(StartPitch), nameof(StartRoll), nameof(StartYaw),
            nameof(AnchorModelText), nameof(TemplateName),
            nameof(Axis), nameof(CustomAxisYaw), nameof(CustomAxisPitch), nameof(PivotX), nameof(PivotY), nameof(PivotZ),
            nameof(AngleDeg), nameof(Advance), nameof(OffsetX), nameof(OffsetY), nameof(OffsetZ), nameof(Count), nameof(IncludeStart)
        };

        private bool _loadingSettings;
        private CancellationTokenSource _saveCts;

        private bool _rememberSettings;
        /// <summary>Keep the last setup across restarts.</summary>
        public bool RememberSettings { get => _rememberSettings; set => SetProperty(ref _rememberSettings, value); }

        private static ini_reader SettingsFile() => new ini_reader(Functions.getRoamingConfigFilePath());

        private void LoadSettings()
        {
            try
            {
                ini_reader ini = SettingsFile();
                _rememberSettings = ini.ReadString(SettingsSection, "remember") == "1";
                if (!_rememberSettings)
                {
                    return;
                }
                _loadingSettings = true;
                _applyingQuick = true;
                foreach (string name in SavedProperties)
                {
                    string text = ini.ReadString(SettingsSection, name);
                    if (string.IsNullOrEmpty(text)) continue;
                    var prop = GetType().GetProperty(name);
                    if (prop == null || !prop.CanWrite) continue;
                    Type type = prop.PropertyType;
                    object value;
                    if (type == typeof(string)) value = text;
                    else if (type.IsEnum) value = Enum.Parse(type, text);
                    else if (type == typeof(bool)) value = text == "1";
                    else value = Convert.ChangeType(text, type, CultureInfo.InvariantCulture);
                    if (name == nameof(ModelIdText))
                    {
                        _applyingQuick = false; // the model change has its own handling
                        prop.SetValue(this, value);
                        _applyingQuick = true;
                    }
                    else
                    {
                        prop.SetValue(this, value);
                    }
                }
            }
            catch
            {
                // A broken entry must not break the page; the defaults stay.
            }
            finally
            {
                _applyingQuick = false;
                _loadingSettings = false;
            }
        }

        private async void ScheduleSave()
        {
            _saveCts?.Cancel();
            var cts = new CancellationTokenSource();
            _saveCts = cts;
            try
            {
                await Task.Delay(800, cts.Token).ConfigureAwait(true);
                SaveSettings();
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void SaveSettings()
        {
            try
            {
                ini_reader ini = SettingsFile();
                ini.Write(SettingsSection, "remember", RememberSettings ? "1" : "0");
                if (!RememberSettings)
                {
                    return;
                }
                foreach (string name in SavedProperties)
                {
                    object value = GetType().GetProperty(name)?.GetValue(this);
                    string text = value is bool b ? (b ? "1" : "0")
                        : value is IFormattable f ? f.ToString(null, CultureInfo.InvariantCulture)
                        : value?.ToString() ?? "";
                    ini.Write(SettingsSection, name, text);
                }
            }
            catch
            {
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            string name = e.PropertyName ?? "";

            if (!_loadingSettings && (name == nameof(RememberSettings) || (RememberSettings && Array.IndexOf(SavedProperties, name) >= 0)))
            {
                ScheduleSave();
            }

            if (name == nameof(ModelIdText))
            {
                HandleModelChanged();
                return;
            }
            if (name == nameof(Shape))
            {
                PreviewFrameVersion++;
            }
            if (name == nameof(Shape))
            {
                OnPropertyChanged(nameof(ShapeIsStraight));
                OnPropertyChanged(nameof(ShapeIsCurved));
                OnPropertyChanged(nameof(ShapeIsWallride));
                OnPropertyChanged(nameof(ShapeIsLoop));
                OnPropertyChanged(nameof(ShapeSupportsAutoPitch));
                OnPropertyChanged(nameof(RiseLabel));
                OnPropertyChanged(nameof(DirectionALabel));
                OnPropertyChanged(nameof(DirectionBLabel));
                if (Shape != QuickShape.Wallride && !_applyingQuick)
                {
                    StartRoll = 0;
                }
                // The same toggle means up/down for a loop: loops start going up,
                // everything else turns right.
                if (!_applyingQuick)
                {
                    _turnLeft = Shape == QuickShape.Loop;
                    OnPropertyChanged(nameof(TurnLeft));
                    // No prop chosen yet: take the shape's default prop.
                    if (string.IsNullOrWhiteSpace(ModelIdText))
                    {
                        PlacementPreset preset = DefaultPresetFor(Shape);
                        if (preset != null)
                        {
                            ModelIdText = preset.Model.ToString(CultureInfo.InvariantCulture);
                        }
                    }
                }
            }
            if (name == nameof(AutoPitch) && !AutoPitch && StartSource == StartSource.Cursor && !_loadingSettings)
            {
                _applyingQuick = true;
                StartPitch = 0;
                _applyingQuick = false;
            }
            if (QuickProperties.Contains(name))
            {
                if (_lockedPreset != null && !_applyingQuick && name != nameof(PieceCount) && name != nameof(AutoPitch))
                {
                    _lockedPreset = null;
                    OnPropertyChanged(nameof(IsPresetLocked));
                    OnPropertyChanged(nameof(LockedPresetText));
                }
                ApplyQuickStart();
                return;
            }
            if (PlanProperties.Contains(name) && !_applyingQuick)
            {
                Invalidate();
            }
            if (name == nameof(LiveChanges))
            {
                if (LiveChanges) TriggerLivePlacement();
                else { _liveCts?.Cancel(); _liveCts = null; }
            }
        }

        // ------------------------------------------------------------------
        // Start pose sources
        // ------------------------------------------------------------------
        private void UseCursor()
        {
            try
            {
                Vector3 c = _getCursorLocation();
                _applyingQuick = true;
                StartSource = StartSource.Cursor;
                StartX = Math.Round(c.X, 3);
                StartY = Math.Round(c.Y, 3);
                StartZ = Math.Round(c.Z, 3);
                IncludeStart = true;
                _startPropSource = -1;
                _applyingQuick = false;
                Invalidate();
                Status = _t("adv_status_cursor", "Cursor-Position übernommen.");
            }
            catch (Exception ex)
            {
                _applyingQuick = false;
                Status = ex.Message;
            }
        }

        /// <summary>Takes position, rotation and model from a placed prop and continues after it.</summary>
        private void ReadStartProp()
        {
            try
            {
                int count = _placement.GetCurrentPropCount();
                int index = StartSource == StartSource.LastProp ? count - 1 : StartPropIndex;
                if (index < 0 || index >= count)
                {
                    Status = string.Format(CultureInfo.CurrentCulture, _t("adv_status_noprop", "Prop {0} gibt es nicht (im Job: {1})."), index, count);
                    return;
                }
                PropPose pose = _placement.GetPose(index);
                V3 e = pose.EulerDeg;
                _applyingQuick = true;
                StartX = Math.Round(pose.Position.X, 4);
                StartY = Math.Round(pose.Position.Y, 4);
                StartZ = Math.Round(pose.Position.Z, 4);
                StartPitch = Math.Round(e.X, 4);
                StartRoll = Math.Round(e.Y, 4);
                StartYaw = Math.Round(e.Z, 4);
                IncludeStart = false;
                _startPropSource = index;
                _applyingQuick = false;
                ModelIdText = _placement.GetModel(index).ToString(CultureInfo.InvariantCulture);
                Invalidate();
                Status = string.Format(CultureInfo.CurrentCulture, _t("adv_status_fromprop", "Weiterbauen ab Prop {0}."), index);
            }
            catch (Exception ex)
            {
                _applyingQuick = false;
                Status = ex.Message;
            }
        }

        /// <summary>Fills the step from two placed props: B = A after one step. Continues after B.</summary>
        private void DetectFromProps()
        {
            try
            {
                int count = _placement.GetCurrentPropCount();
                if (DetectA >= count || DetectB >= count || DetectA == DetectB)
                {
                    Status = _t("adv_status_detect_bad", "Zwei verschiedene, vorhandene Props angeben.");
                    return;
                }
                PropPose a = _placement.GetPose(DetectA), b = _placement.GetPose(DetectB);
                RepeatStep step = RepeatPlanner.Detect(a, b);
                V3 e = b.EulerDeg;
                _applyingQuick = true;
                SetStep(step);
                StartSource = StartSource.PropIndex;
                StartPropIndex = DetectB;
                StartX = Math.Round(b.Position.X, 4);
                StartY = Math.Round(b.Position.Y, 4);
                StartZ = Math.Round(b.Position.Z, 4);
                StartPitch = Math.Round(e.X, 4);
                StartRoll = Math.Round(e.Y, 4);
                StartYaw = Math.Round(e.Z, 4);
                IncludeStart = false;
                _startPropSource = DetectB;
                _applyingQuick = false;
                ModelIdText = _placement.GetModel(DetectB).ToString(CultureInfo.InvariantCulture);
                Invalidate();
                Status = string.Format(CultureInfo.CurrentCulture,
                    _t("adv_status_detected", "Erkannt: {0}, {1:0.###}° pro Teil, Drehpunkt {2}, Versatz {3:0.###} m."),
                    step.Axis, step.AngleDeg, step.Pivot, step.Advance);
            }
            catch (Exception ex)
            {
                _applyingQuick = false;
                Status = ex.Message;
            }
        }

        // ------------------------------------------------------------------
        // Placing
        // ------------------------------------------------------------------
        private bool _isPlacing;
        public bool IsPlacing
        {
            get => _isPlacing;
            private set
            {
                if (SetProperty(ref _isPlacing, value))
                {
                    OnPropertyChanged(nameof(CanPlace));
                    OnPropertyChanged(nameof(CanUseTemplates));
                    PlaceCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private bool _canUndo;
        public bool CanUndo { get => _canUndo; private set { if (SetProperty(ref _canUndo, value)) UndoCommand.NotifyCanExecuteChanged(); } }

        private bool _liveChanges;
        public bool LiveChanges { get => _liveChanges; set => SetProperty(ref _liveChanges, value); }

        private string _status = "";
        public string Status { get => _status; set => SetProperty(ref _status, value); }

        public bool CanPlace => FeaturesReady && HasDimensions && _modelId.HasValue && !IsPlacing;
        public bool CanUseTemplates => CanPlace && _templates.IsAvailable;

        private void TriggerLivePlacement()
        {
            if (!LiveChanges || !CanPlace || _applyingQuick)
            {
                return;
            }
            ScheduleLivePlacement();
        }

        private async void ScheduleLivePlacement()
        {
            _liveCts?.Cancel();
            var cts = new CancellationTokenSource();
            _liveCts = cts;
            try
            {
                await Task.Delay(250, cts.Token).ConfigureAwait(true);
                if (!cts.IsCancellationRequested)
                {
                    await PlaceAsync(true).ConfigureAwait(true);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
        }

        private async Task PlaceAsync(bool live)
        {
            if (!CanPlace)
            {
                return;
            }
            await _placementSemaphore.WaitAsync().ConfigureAwait(true);
            IsPlacing = true;
            try
            {
                List<PropPose> plan = BuildPlan();
                if (plan.Count == 0)
                {
                    Status = _t("adv_status_empty", "Nichts zu platzieren.");
                    return;
                }
                int replace = live && _lastPlacementWasLive ? _lastPlacementCount : 0;
                int current = _placement.GetCurrentPropCount() - replace;
                if (current + plan.Count > PropPlacementService.PropLimit)
                {
                    Status = string.Format(CultureInfo.CurrentCulture, _t("adv_status_limit", "Prop-Limit: {0} im Job + {1} neu > {2}."), current, plan.Count, PropPlacementService.PropLimit);
                    return;
                }
                if (replace > 0)
                {
                    _placement.UndoPlacement(replace);
                }
                int model = _modelId.Value;
                _placement.PlaceProps(plan.Select(p => (model, p)).ToList(), _startPropSource);
                _refreshCreator();

                _lastPlacementCount = plan.Count;
                _lastPlacementWasLive = live;
                CanUndo = true;
                Status = string.Format(CultureInfo.CurrentCulture, _t("adv_status_placed", "{0} Props platziert."), plan.Count);
                PlanInfoText = string.Format(CultureInfo.CurrentCulture,
                    _t("adv_plan_info", "{0} neue Props · im Job: {1} / {2}"), plan.Count, SafePropCount(), PropPlacementService.PropLimit);
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
            finally
            {
                IsPlacing = false;
                _placementSemaphore.Release();
            }
        }

        private void UndoPlacement()
        {
            if (_lastPlacementCount <= 0)
            {
                return;
            }
            _placement.UndoPlacement(_lastPlacementCount);
            _refreshCreator();
            Status = string.Format(CultureInfo.CurrentCulture, _t("adv_status_undone", "{0} Props entfernt."), _lastPlacementCount);
            _lastPlacementCount = 0;
            _lastPlacementWasLive = false;
            CanUndo = false;
        }

        // ------------------------------------------------------------------
        // Copy to templates
        // ------------------------------------------------------------------
        private bool _isTemplateDialogOpen;
        public bool IsTemplateDialogOpen { get => _isTemplateDialogOpen; set => SetProperty(ref _isTemplateDialogOpen, value); }

        public ObservableCollection<TemplateSlotRow> TemplateSlots { get; } = new ObservableCollection<TemplateSlotRow>();

        private int _templateStartSlot;
        public int TemplateStartSlot
        {
            get => _templateStartSlot;
            set { if (SetProperty(ref _templateStartSlot, value)) UpdateTemplatePlan(); }
        }

        private string _templateName = "";
        public string TemplateName { get => _templateName; set { if (SetProperty(ref _templateName, value)) UpdateTemplatePlan(); } }

        private string _anchorModelText = "";
        public string AnchorModelText { get => _anchorModelText; set { if (SetProperty(ref _anchorModelText, value)) UpdateTemplatePlan(); } }

        private string _templatePlanText = "";
        public string TemplatePlanText { get => _templatePlanText; private set => SetProperty(ref _templatePlanText, value); }

        private bool _templatePlanValid;
        public bool TemplatePlanValid { get => _templatePlanValid; private set => SetProperty(ref _templatePlanValid, value); }

        private void OpenTemplateDialog()
        {
            if (!CanUseTemplates)
            {
                return;
            }
            try
            {
                TemplateSlots.Clear();
                IReadOnlyList<TemplateSlotInfo> slots = _templates.ReadSlots();
                foreach (TemplateSlotInfo s in slots)
                {
                    string main = s.MainModel == 0 ? "" : _modelName(s.MainModel);
                    TemplateSlots.Add(new TemplateSlotRow
                    {
                        Index = s.Index,
                        InUse = s.InUse,
                        Title = string.IsNullOrEmpty(s.Name) ? $"Template {s.Index + 1}" : $"{s.Index + 1}: {s.Name}",
                        Detail = s.InUse
                            ? string.Format(CultureInfo.CurrentCulture, _t("adv_tpl_slot_used", "{0} Props · {1}"), s.PropCount, string.IsNullOrEmpty(main) ? s.MainModel.ToString(CultureInfo.InvariantCulture) : main)
                            : _t("adv_tpl_slot_free", "frei")
                    });
                }
                int firstFree = slots.FirstOrDefault(s => !s.InUse)?.Index ?? 0;
                _templateStartSlot = -1;
                TemplateStartSlot = firstFree;
                IsTemplateDialogOpen = true;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
        }

        private List<List<TemplateEntry>> BuildTemplateChunks(out int anchorModel)
        {
            anchorModel = 0;
            if (!_modelId.HasValue || !ModelIdParser.TryParseModelIdInt(AnchorModelText, out anchorModel, out _))
            {
                return null;
            }
            List<PropPose> plan = BuildPlan();
            if (plan.Count == 0)
            {
                return null;
            }
            PropPose anchor = TemplateAnchor(plan);
            int model = _modelId.Value;
            return CreatorTemplateService.BuildChunks(anchorModel, anchor, plan.Select(p => (model, p)).ToList());
        }

        /// <summary>
        /// Where the template's start prop goes: on the ground under the back edge of the
        /// first piece, facing the start heading. Every template of a chain starts with it,
        /// so placing each one on that spot continues the chain.
        /// </summary>
        public PropPose TemplateAnchor(List<PropPose> plan = null)
        {
            plan = plan ?? BuildPlan();
            PropPose first = plan.Count > 0 ? plan[0] : StartPose;
            V3 lo = _boxMin, hi = _boxMax;
            V3[] backCorners =
            {
                first.ToWorld(new V3(lo.X, lo.Y, lo.Z)), first.ToWorld(new V3(hi.X, lo.Y, lo.Z)),
                first.ToWorld(new V3(lo.X, lo.Y, hi.Z)), first.ToWorld(new V3(hi.X, lo.Y, hi.Z))
            };
            double x = backCorners.Average(c => c.X), y = backCorners.Average(c => c.Y), z = backCorners.Min(c => c.Z);
            return new PropPose(new V3(x, y, z), Rot3.AxisAngle(V3.UnitZ, first.EulerDeg.Z));
        }

        /// <summary>World pivot and axis of the first step, for the preview guide; false without a turn.</summary>
        public bool TryGetStepGuide(out V3 pivot, out V3 axis)
        {
            RepeatStep step = BuildStep();
            PropPose start = StartPose;
            pivot = start.ToWorld(step.Pivot);
            axis = step.WorldAxis(start);
            return step.Axis != StepAxis.None && Math.Abs(step.AngleDeg) > 1e-6;
        }

        private void UpdateTemplatePlan()
        {
            List<List<TemplateEntry>> chunks = BuildTemplateChunks(out _);
            foreach (TemplateSlotRow row in TemplateSlots)
            {
                row.IsTarget = chunks != null && row.Index >= TemplateStartSlot && row.Index < TemplateStartSlot + chunks.Count;
            }
            if (chunks == null)
            {
                TemplatePlanValid = false;
                TemplatePlanText = _t("adv_tpl_bad_anchor", "Start-Prop nicht erkannt.");
                return;
            }
            int last = TemplateStartSlot + chunks.Count - 1;
            int pieces = chunks.Sum(c => c.Count - 1);
            if (last >= CreatorTemplateService.MaxTemplates)
            {
                TemplatePlanValid = false;
                TemplatePlanText = string.Format(CultureInfo.CurrentCulture,
                    _t("adv_tpl_too_many", "{0} Teile brauchen {1} Templates – ab Platz {2} ist dafür kein Platz. Früheren Platz wählen oder weniger Teile."),
                    pieces, chunks.Count, TemplateStartSlot + 1);
                return;
            }
            int overwritten = TemplateSlots.Count(r => r.IsTarget && r.InUse);
            int inJob = SafePropCount();
            string text = string.Format(CultureInfo.CurrentCulture,
                _t("adv_tpl_plan", "{0} Teile → {1} Template(s), Platz {2}–{3}. Jedes beginnt mit dem Start-Prop am selben Punkt: Template setzen, Start-Prop löschen, nächstes Template auf denselben Punkt setzen."),
                pieces, chunks.Count, TemplateStartSlot + 1, last + 1);
            if (overwritten > 0)
            {
                text += " " + string.Format(CultureInfo.CurrentCulture, _t("adv_tpl_overwrite", "{0} vorhandene(s) Template(s) werden überschrieben."), overwritten);
            }
            if (inJob + pieces + 1 > PropPlacementService.PropLimit)
            {
                text += " " + string.Format(CultureInfo.CurrentCulture, _t("adv_tpl_limit", "Achtung: {0} Props im Job + {1} passen nicht unter das Limit von {2}."), inJob, pieces + 1, PropPlacementService.PropLimit);
            }
            TemplatePlanText = text;
            TemplatePlanValid = true;
        }

        private void WriteTemplates()
        {
            List<List<TemplateEntry>> chunks = BuildTemplateChunks(out _);
            if (chunks == null || !TemplatePlanValid)
            {
                return;
            }
            try
            {
                string baseName = (TemplateName ?? "").Trim();
                for (int i = 0; i < chunks.Count; i++)
                {
                    string name = baseName;
                    if (chunks.Count > 1)
                    {
                        string suffix = $" {i + 1}/{chunks.Count}";
                        name = (baseName.Length + suffix.Length > CreatorTemplateService.NameLength
                            ? baseName.Substring(0, Math.Max(0, CreatorTemplateService.NameLength - suffix.Length))
                            : baseName) + suffix;
                    }
                    _templates.Write(TemplateStartSlot + i, name.Trim(), chunks[i]);
                }
                IsTemplateDialogOpen = false;
                Status = string.Format(CultureInfo.CurrentCulture,
                    _t("adv_tpl_written", "{0} Template(s) geschrieben (Platz {1}–{2}). Job speichern, damit sie bleiben."),
                    chunks.Count, TemplateStartSlot + 1, TemplateStartSlot + chunks.Count);
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
        }
    }
}
