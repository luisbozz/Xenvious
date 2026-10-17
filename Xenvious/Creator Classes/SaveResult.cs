using System;
using System.Collections.Generic;
using System.Globalization;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// Why the cloud refused a save or publish. The creators show "The Rockstar cloud servers are
    /// unavailable" for every refusal: they read UGC_GET_CREATE_RESULT (new job) or
    /// UGC_GET_MODIFY_RESULT (publish, update of a saved job) and drop it. The patches "save
    /// result", "publish result" and "update result" (saveresult.py in ysc-global-updater) keep
    /// that result in the data page with a counter each; when a counter moves, Xenvious shows the
    /// code in its own message. Needs the script features (the patches call into the payload).
    /// </summary>
    public static class SaveResult
    {
        // Data page: result at +0, count at +8 for each kind.
        private static readonly (int At, string Kind, string Title, string TitleFallback, string Reason, string ReasonFallback)[] Kinds =
        {
            (0x37A0, "save", "err_save_title", "Save refused", "err_save_reason", "The cloud refused to save the job."),
            (0x37B0, "publish", "err_publish_title", "Publish refused", "err_publish_reason", "The cloud refused to publish the job."),
            (0x37C0, "update", "err_update_title", "Update refused", "err_update_reason", "The cloud refused the update of the saved job."),
        };
        private static long _page;
        private static readonly long[] _count = { -1, -1, -1 };

        /// <summary>Called by the patch loop (every 250 ms), after the patch pass.</summary>
        public static void Tick()
        {
            if (!ScriptVars.FeaturesOn || !MainWindow.m.IsProcOpen)
                return;
            string creator = GTA.CurrentCreatorName();
            long page = creator != "" && ScrPatchesRunner.PageReady(creator) ? ScriptSpace.Get(creator)?.Data ?? 0 : 0;
            if (page == 0)
                return;
            // A page seen for the first time: what is in it already was before Xenvious looked.
            bool fresh = page != _page;
            _page = page;
            for (int i = 0; i < Kinds.Length; i++)
            {
                var k = Kinds[i];
                long count = MainWindow.m.memory((page + k.At + 8).ToString("X")).Get<long>();
                if (fresh || _count[i] < 0)
                {
                    _count[i] = count;
                    continue;
                }
                if (count == _count[i])
                    continue;
                _count[i] = count;
                int code = MainWindow.m.memory((page + k.At).ToString("X")).Get<int>();
                Log.Warn($"{k.Kind} refused by the cloud in {creator}: result {code}", source: "SaveResult");
                var facts = new List<(string, string)>
                {
                    (T("err_f_creator", "Creator"), MainWindow.CreatorDisplayName(creator)),
                    (T("err_f_code", "Code"), code.ToString(CultureInfo.InvariantCulture)),
                    (T("err_f_time", "Time"), DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture)),
                };
                string title = T(k.Title, k.TitleFallback), reason = T(k.Reason, k.ReasonFallback);
                string hint = T("err_cloud_hint", "GTA calls every refusal \"cloud servers unavailable\"; the code is the cloud's real answer.");
                MainWindow.Instance?.Dispatcher.BeginInvoke(new Action(() => MainWindow.Instance.ShowErrorCard(title, reason, facts, hint)));
            }
        }

        private static string T(string key, string fallback) => MainWindow.Instance?.TranslateOr(key, fallback) ?? fallback;
    }
}
