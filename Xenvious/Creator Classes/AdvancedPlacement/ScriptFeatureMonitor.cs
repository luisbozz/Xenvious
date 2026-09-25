using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Xenvious.AdvancedPlacement
{
    public enum ScriptFeatureState
    {
        /// <summary>GTA is not running.</summary>
        NoGame,
        /// <summary>No creator is open.</summary>
        NoCreator,
        /// <summary>This creator gets no injected functions (deathmatch, survival).</summary>
        Unsupported,
        /// <summary>The "experimental script features" setting is off.</summary>
        Disabled,
        /// <summary>Not checked yet.</summary>
        Checking,
        /// <summary>Injected, but the dimension function did not answer.</summary>
        NoResponse,
        /// <summary>The dimension and hover functions run.</summary>
        Active
    }

    /// <summary>
    /// Whether the injected creator functions (model dimensions, hovered model) work.
    ///
    /// The dispatch bits in custom_check do not answer that on their own: the LTS
    /// creator runs a function when its bit is clear, race and capture when it is set,
    /// and a GTA process can keep stale bytecode across Xenvious restarts. So the check
    /// asks the function itself: write two known models and see whether the dimensions
    /// follow.
    /// </summary>
    public sealed class ScriptFeatureMonitor
    {
        // Small, always valid models with different bounding boxes.
        private static readonly int ProbeA = unchecked((int)MainWindow.Joaat("prop_mp_cone_01"));
        private static readonly int ProbeB = unchecked((int)MainWindow.Joaat("prop_barrier_work05"));

        private readonly Func<bool> _isProcessOpen;
        private readonly Func<bool> _isCreatorRunning;
        private readonly Func<string> _creatorName;
        private readonly Func<bool> _isEnabled;

        public ScriptFeatureMonitor(Func<bool> isProcessOpen, Func<bool> isCreatorRunning, Func<string> creatorName, Func<bool> isEnabled)
        {
            _isProcessOpen = isProcessOpen;
            _isCreatorRunning = isCreatorRunning;
            _creatorName = creatorName;
            _isEnabled = isEnabled;
        }

        /// <summary>The state that needs no probe; Checking when only a probe can tell.</summary>
        public ScriptFeatureState Precheck()
        {
            try
            {
                if (!_isProcessOpen())
                    return ScriptFeatureState.NoGame;
                if (!_isCreatorRunning())
                    return ScriptFeatureState.NoCreator;
                string name = _creatorName() ?? "";
                if (name == "fm_deathmatch_creator" || name == "fm_survival_creator")
                    return ScriptFeatureState.Unsupported;
                if (!_isEnabled())
                    return ScriptFeatureState.Disabled;
                return ScriptFeatureState.Checking;
            }
            catch
            {
                return ScriptFeatureState.NoGame;
            }
        }

        /// <summary>Full check including the probe (~0.3 s).</summary>
        public async Task<ScriptFeatureState> CheckAsync()
        {
            ScriptFeatureState pre = Precheck();
            if (pre != ScriptFeatureState.Checking)
                return pre;

            try
            {
                Vector3 a = await ProbeAsync(ProbeA).ConfigureAwait(true);
                Vector3 b = await ProbeAsync(ProbeB).ConfigureAwait(true);
                bool answered = a != Vector3.Zero && b != Vector3.Zero && a != b;
                return answered ? ScriptFeatureState.Active : ScriptFeatureState.NoResponse;
            }
            catch
            {
                return ScriptFeatureState.NoResponse;
            }
        }

        private static async Task<Vector3> ProbeAsync(int model)
        {
            new Global(GTA.Offsets.Editor.custom_dimension_model).SetInt(model);
            // The function runs once per frame; give it a few.
            await Task.Delay(150).ConfigureAwait(true);
            return new Global(GTA.Offsets.Editor.custom_dimension_max).GetVector3()
                 - new Global(GTA.Offsets.Editor.custom_dimension_min).GetVector3();
        }

        /// <summary>Model under the creator cursor, 0 when none.</summary>
        public int ReadHoveredModel()
        {
            try
            {
                return GTA.Offsets.Editor.custom_hovered_model == 0 ? 0 : new Global(GTA.Offsets.Editor.custom_hovered_model).Get<int>();
            }
            catch
            {
                return 0;
            }
        }
    }
}
