using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Xenvious.AdvancedPlacement;

namespace Xenvious.ViewModels
{
    /// <summary>A preset as a card in step 1.</summary>
    public sealed class PresetCard
    {
        public PlacementPreset Preset { get; set; }
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string Icon { get; set; } = "";
        public bool IsUser => Preset != null && !Preset.BuiltIn;
    }

    /// <summary>A run found in the job by "learn from job".</summary>
    public sealed class LearnedRow
    {
        public LearnedPattern Pattern { get; set; }
        public string Title { get; set; } = "";
        public string Detail { get; set; } = "";
    }

    public partial class AdvancedPropPlacementViewModel
    {
        private List<PlacementPreset> _userPresets = new List<PlacementPreset>();
        private PlacementPreset _pendingSave;

        public ObservableCollection<PresetCard> Presets { get; } = new ObservableCollection<PresetCard>();
        public ObservableCollection<LearnedRow> LearnedRows { get; } = new ObservableCollection<LearnedRow>();

        private IRelayCommand<PresetCard> _applyPresetCommand, _deletePresetCommand;
        private IRelayCommand<LearnedRow> _continueLearnedCommand, _saveLearnedCommand;
        private IRelayCommand _savePresetCommand, _confirmSavePresetCommand, _cancelSavePresetCommand,
                              _exportPresetsCommand, _importPresetsCommand, _learnCommand, _closeLearnCommand;

        public IRelayCommand<PresetCard> ApplyPresetCommand => _applyPresetCommand ??= new RelayCommand<PresetCard>(ApplyPreset);
        public IRelayCommand<PresetCard> DeletePresetCommand => _deletePresetCommand ??= new RelayCommand<PresetCard>(DeletePreset);
        public IRelayCommand SavePresetCommand => _savePresetCommand ??= new RelayCommand(BeginSavePreset);
        public IRelayCommand ConfirmSavePresetCommand => _confirmSavePresetCommand ??= new RelayCommand(ConfirmSavePreset);
        public IRelayCommand CancelSavePresetCommand => _cancelSavePresetCommand ??= new RelayCommand(() => IsSavePresetOpen = false);
        public IRelayCommand ExportPresetsCommand => _exportPresetsCommand ??= new RelayCommand(ExportPresets);
        public IRelayCommand ImportPresetsCommand => _importPresetsCommand ??= new RelayCommand(ImportPresets);
        public IRelayCommand LearnCommand => _learnCommand ??= new RelayCommand(LearnFromJob);
        public IRelayCommand CloseLearnCommand => _closeLearnCommand ??= new RelayCommand(() => IsLearnOpen = false);
        public IRelayCommand<LearnedRow> ContinueLearnedCommand => _continueLearnedCommand ??= new RelayCommand<LearnedRow>(ContinueLearned);
        public IRelayCommand<LearnedRow> SaveLearnedCommand => _saveLearnedCommand ??= new RelayCommand<LearnedRow>(SaveLearned);

        private bool _isSavePresetOpen;
        public bool IsSavePresetOpen { get => _isSavePresetOpen; set => SetProperty(ref _isSavePresetOpen, value); }

        private string _newPresetName = "";
        public string NewPresetName { get => _newPresetName; set => SetProperty(ref _newPresetName, value); }

        private bool _isLearnOpen;
        public bool IsLearnOpen { get => _isLearnOpen; set => SetProperty(ref _isLearnOpen, value); }

        private string _learnText = "";
        public string LearnText { get => _learnText; private set => SetProperty(ref _learnText, value); }

        public bool HasLearnedRows => LearnedRows.Count > 0;

        private static string ShapeIcon(string shape)
        {
            switch (shape)
            {
                case "Straight": return "━";
                case "Wallride": return "◜";
                case "Loop": return "◯";
                case "Spiral": return "↻";
                case "Corkscrew": return "∿";
                default: return "◠";
            }
        }

        private void LoadPresets()
        {
            _userPresets = PlacementPresetStore.LoadUser();
            RefreshPresetCards();
        }

        private void RefreshPresetCards()
        {
            Presets.Clear();
            foreach (PlacementPreset p in PlacementPresetStore.LoadBuiltIn().Concat(_userPresets))
            {
                string name = p.NameKey != null ? _t(p.NameKey, p.Name) : p.Name;
                string model = _modelName(p.Model);
                if (string.IsNullOrEmpty(model)) model = p.Model.ToString(CultureInfo.InvariantCulture);
                Presets.Add(new PresetCard
                {
                    Preset = p,
                    Title = name,
                    Subtitle = string.Format(CultureInfo.CurrentCulture, _t("adv_preset_sub", "{0} · {1} Teile"), model, p.Count)
                               + (p.BuiltIn ? "" : " · " + _t("adv_preset_own", "eigenes")),
                    Icon = ShapeIcon(p.Shape)
                });
            }
        }

        /// <summary>The first built-in quick preset of a shape: its prop is the default for that shape.</summary>
        private PlacementPreset DefaultPresetFor(QuickShape shape)
        {
            return PlacementPresetStore.LoadBuiltIn().FirstOrDefault(p => !p.IsExpert && p.Shape == shape.ToString());
        }

        private void ApplyPreset(PresetCard card)
        {
            PlacementPreset p = card?.Preset;
            if (p == null)
            {
                return;
            }
            try
            {
                _applyingQuick = true;
                StartPitch = p.Pitch ?? 0;
                StartRoll = p.Roll ?? 0;
                if (p.IsExpert)
                {
                    // An exact step (tilted plane, skewed axis): the quick start keeps it as is and
                    // leaves only count and start to the user. The expert view shows the same values.
                    if (Enum.TryParse(p.Shape, out QuickShape lockedShape)) Shape = lockedShape;
                    PieceCount = p.Count;
                    Count = p.Count;
                    SetStep(p.ToStep());
                    IncludeStart = StartSource == StartSource.Cursor;
                    _lockedPreset = p;
                }
                else
                {
                    _lockedPreset = null;
                    if (Enum.TryParse(p.Shape, out QuickShape shape)) Shape = shape;
                    TurnLeft = p.TurnLeft ?? (Shape == QuickShape.Loop);
                    Radius = p.Radius ?? Radius;
                    Rise = p.Rise ?? 0;
                    Bank = p.Bank ?? Bank;
                    Overlap = p.Overlap ?? 0;
                    AutoPitch = p.AutoPitch ?? false;
                    PieceCount = p.Count;
                }
            }
            finally
            {
                _applyingQuick = false;
            }
            string modelText = p.Model.ToString(CultureInfo.InvariantCulture);
            if (ModelIdText != modelText)
            {
                ModelIdText = modelText;
            }
            OnPropertyChanged(nameof(IsPresetLocked));
            OnPropertyChanged(nameof(LockedPresetText));
            ApplyQuickStart();
            PreviewFrameVersion++;
            Invalidate();
            if (ModeIndex == 0)
            {
                QuickStep = 3;
            }
            Status = string.Format(CultureInfo.CurrentCulture, _t("adv_preset_loaded", "Preset „{0}“ geladen – Startpunkt wählen und platzieren."), card.Title);
        }

        private PlacementPreset CurrentAsPreset(string name)
        {
            if (!_modelId.HasValue)
            {
                return null;
            }
            var p = new PlacementPreset
            {
                Name = name,
                Model = _modelId.Value,
                Shape = Shape.ToString(),
                Pitch = Math.Round(StartPitch, 4),
                Roll = Math.Round(StartRoll, 4)
            };
            if (ModeIndex == 0)
            {
                p.Kind = "quick";
                p.Count = PieceCount;
                p.TurnLeft = TurnLeft;
                p.Radius = Radius;
                p.Rise = Rise;
                p.Bank = Bank;
                p.Overlap = Overlap;
                p.AutoPitch = AutoPitch;
            }
            else
            {
                RepeatStep step = BuildStep();
                p.Kind = "expert";
                p.Count = Count;
                p.Axis = step.Axis.ToString();
                if (step.Axis == StepAxis.Custom) p.CustomAxis = PlacementPreset.Arr(step.CustomAxis);
                p.Pivot = PlacementPreset.Arr(step.Pivot);
                p.Angle = Math.Round(step.AngleDeg, 5);
                p.Advance = Math.Round(step.Advance, 5);
                if (step.Offset.Length > 1e-9) p.Offset = PlacementPreset.Arr(step.Offset);
            }
            return p;
        }

        private void BeginSavePreset()
        {
            if (!_modelId.HasValue)
            {
                Status = _t("adv_preset_need_model", "Erst ein Prop wählen.");
                return;
            }
            _pendingSave = null;
            string model = _modelName(_modelId.Value);
            NewPresetName = $"{_t("adv_shape_" + Shape.ToString().ToLowerInvariant(), Shape.ToString())} · {(string.IsNullOrEmpty(model) ? _modelId.Value.ToString(CultureInfo.InvariantCulture) : model)}";
            IsSavePresetOpen = true;
        }

        private void ConfirmSavePreset()
        {
            string name = (NewPresetName ?? "").Trim();
            if (name.Length == 0)
            {
                return;
            }
            PlacementPreset p = _pendingSave ?? CurrentAsPreset(name);
            if (p == null)
            {
                return;
            }
            p.Name = name;
            _userPresets.RemoveAll(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            _userPresets.Add(p);
            SaveUserPresets();
            _pendingSave = null;
            IsSavePresetOpen = false;
            Status = string.Format(CultureInfo.CurrentCulture, _t("adv_preset_saved", "Preset „{0}“ gespeichert."), name);
        }

        private void DeletePreset(PresetCard card)
        {
            if (card?.Preset == null || card.Preset.BuiltIn)
            {
                return;
            }
            _userPresets.Remove(card.Preset);
            SaveUserPresets();
            Status = string.Format(CultureInfo.CurrentCulture, _t("adv_preset_deleted", "Preset „{0}“ gelöscht."), card.Title);
        }

        private void SaveUserPresets()
        {
            try
            {
                PlacementPresetStore.SaveUser(_userPresets);
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
            RefreshPresetCards();
        }

        private void ExportPresets()
        {
            if (_userPresets.Count == 0)
            {
                Status = _t("adv_preset_none_to_export", "Noch keine eigenen Presets zum Exportieren.");
                return;
            }
            var dialog = new Microsoft.Win32.SaveFileDialog { Filter = PlacementPresetStore.FileFilter, FileName = "presets.xvpresets" };
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            try
            {
                PlacementPresetStore.Export(dialog.FileName, _userPresets);
                Status = string.Format(CultureInfo.CurrentCulture, _t("adv_preset_exported", "{0} Preset(s) exportiert."), _userPresets.Count);
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
        }

        private void ImportPresets()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = PlacementPresetStore.FileFilter };
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            try
            {
                List<PlacementPreset> incoming = PlacementPresetStore.Import(dialog.FileName);
                int added = 0;
                foreach (PlacementPreset p in incoming)
                {
                    // Same name as an existing one: keep both, never overwrite silently.
                    string name = p.Name.Trim();
                    string unique = name;
                    for (int n = 2; _userPresets.Any(x => string.Equals(x.Name, unique, StringComparison.OrdinalIgnoreCase)); n++)
                    {
                        unique = $"{name} ({n})";
                    }
                    p.Name = unique;
                    p.NameKey = null;
                    p.BuiltIn = false;
                    _userPresets.Add(p);
                    added++;
                }
                SaveUserPresets();
                Status = string.Format(CultureInfo.CurrentCulture, _t("adv_preset_imported", "{0} Preset(s) importiert."), added);
            }
            catch (Exception ex)
            {
                Status = string.Format(CultureInfo.CurrentCulture, _t("adv_preset_import_failed", "Import fehlgeschlagen: {0}"), ex.Message);
            }
        }

        // ------------------------------------------------------------------
        // Learn from job
        // ------------------------------------------------------------------
        private void LearnFromJob()
        {
            LearnedRows.Clear();
            try
            {
                int count = _placement.GetCurrentPropCount();
                var props = new List<(int, PropPose)>(count);
                for (int i = 0; i < count; i++)
                {
                    props.Add((_placement.GetModel(i), _placement.GetPose(i)));
                }
                foreach (LearnedPattern pattern in JobPatternFinder.Find(props).Take(30))
                {
                    string model = _modelName(pattern.Model);
                    RepeatStep s = pattern.Step;
                    LearnedRows.Add(new LearnedRow
                    {
                        Pattern = pattern,
                        Title = $"{(string.IsNullOrEmpty(model) ? pattern.Model.ToString(CultureInfo.InvariantCulture) : model)}",
                        Detail = string.Format(CultureInfo.CurrentCulture,
                            _t("adv_learn_detail", "Props {0}–{1} · {2} Teile · {3:0.#}° pro Teil · Versatz {4:0.##} m"),
                            pattern.Indices.Min(), pattern.Indices.Max(), pattern.Indices.Count, Math.Abs(s.AngleDeg), s.Advance + s.Offset.Length)
                    });
                }
                LearnText = LearnedRows.Count == 0
                    ? _t("adv_learn_none", "Keine Reihen gefunden. Gesucht werden mindestens 3 gleiche Props, die mit demselben Schritt aufeinander folgen.")
                    : string.Format(CultureInfo.CurrentCulture, _t("adv_learn_found", "{0} Reihe(n) im Job gefunden."), LearnedRows.Count);
            }
            catch (Exception ex)
            {
                LearnText = ex.Message;
            }
            OnPropertyChanged(nameof(HasLearnedRows));
            IsLearnOpen = true;
        }

        /// <summary>Loads a found run into the expert view and continues after its last piece.</summary>
        private void ContinueLearned(LearnedRow row)
        {
            LearnedPattern pattern = row?.Pattern;
            if (pattern == null)
            {
                return;
            }
            int last = pattern.Indices[pattern.Indices.Count - 1];
            V3 e = pattern.Last.EulerDeg;
            _applyingQuick = true;
            try
            {
                ModeIndex = 1;
                SetStep(pattern.Step);
                StartSource = StartSource.PropIndex;
                StartPropIndex = last;
                StartX = Math.Round(pattern.Last.Position.X, 4);
                StartY = Math.Round(pattern.Last.Position.Y, 4);
                StartZ = Math.Round(pattern.Last.Position.Z, 4);
                StartPitch = Math.Round(e.X, 4);
                StartRoll = Math.Round(e.Y, 4);
                StartYaw = Math.Round(e.Z, 4);
                IncludeStart = false;
                _startPropSource = last;
            }
            finally
            {
                _applyingQuick = false;
            }
            ModelIdText = pattern.Model.ToString(CultureInfo.InvariantCulture);
            PreviewFrameVersion++;
            Invalidate();
            IsLearnOpen = false;
            Status = string.Format(CultureInfo.CurrentCulture, _t("adv_status_fromprop", "Weiterbauen ab Prop {0}."), last);
        }

        /// <summary>Turns a found run into a preset (asks for the name).</summary>
        private void SaveLearned(LearnedRow row)
        {
            LearnedPattern pattern = row?.Pattern;
            if (pattern == null)
            {
                return;
            }
            RepeatStep s = pattern.Step;
            V3 e = pattern.First.EulerDeg;
            _pendingSave = new PlacementPreset
            {
                Kind = "expert",
                Model = pattern.Model,
                Shape = GuessShape(s),
                Count = pattern.Indices.Count,
                Pitch = Math.Round(e.X, 4),
                Roll = Math.Round(e.Y, 4),
                Axis = s.Axis.ToString(),
                CustomAxis = s.Axis == StepAxis.Custom ? PlacementPreset.Arr(s.CustomAxis) : null,
                Pivot = PlacementPreset.Arr(s.Pivot),
                Angle = Math.Round(s.AngleDeg, 5),
                Advance = Math.Round(s.Advance, 5),
                Offset = s.Offset.Length > 1e-9 ? PlacementPreset.Arr(s.Offset) : null
            };
            NewPresetName = row.Title;
            IsLearnOpen = false;
            IsSavePresetOpen = true;
        }

        private static string GuessShape(RepeatStep s)
        {
            if (s.Axis == StepAxis.None || Math.Abs(s.AngleDeg) < 0.01) return "Straight";
            if (s.Axis == StepAxis.LocalX) return "Loop";
            if (s.Axis == StepAxis.LocalY) return "Corkscrew";
            if (Math.Abs(s.Advance) > 0.05) return "Spiral";
            return s.Axis == StepAxis.LocalZ ? "Wallride" : "Curve";
        }
    }
}
