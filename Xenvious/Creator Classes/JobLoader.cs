using System;
using System.Text;
using System.Threading.Tasks;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// Makes the open creator load a job by its content id, the way it loads one of the
    /// player's own saved jobs: with a content id set, the creator's start state asks
    /// the cloud for that job and loads all of it. It works for other players' jobs as
    /// well, so this copies a job completely, including everything Xenvious has no
    /// fields for.
    ///
    /// Race, LTS and Capture creator only; deathmatch and survival number their states
    /// differently. Afterwards the job counts as a new one, so saving it creates a job
    /// in the player's account instead of trying to update the original.
    /// </summary>
    public static class JobLoader
    {
        public enum Result { Loaded, Unsupported, NotEditing, TimedOut }

        private const int StateStart = 0;
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(90);

        public static bool CanLoad(string creator) =>
            CreatorMap.CanRebuild(creator)
            && GTA.Offsets.Editor.load_job_flag != 0
            && GTA.Offsets.Editor.load_job_id != 0
            && GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_editing_published != 0;

        /// <summary>The creator script a job of this type opens in, or null.</summary>
        public static string CreatorFor(int type, int subtype)
        {
            if (type == 2)
                return "fm_race_creator";
            if (type == 0 && subtype == 5)
                return "fm_lts_creator";
            if (type == 0 && subtype == 6)
                return "fm_capture_creator";
            return null;
        }

        public static async Task<Result> LoadAsync(string contentId)
        {
            string creator = CreatorMap.CurrentCreator();
            if (!CanLoad(creator) || string.IsNullOrEmpty(contentId))
                return Result.Unsupported;

            long worker = CreatorMap.WorkerOffset(creator);
            long state = worker + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_refresh;
            if (CreatorMap.ReadLocal(state) != CreatorMap.StateEditing)
                return Result.NotEditing;

            // The id is a plain text label: ASCII, zero-terminated.
            byte[] id = Encoding.ASCII.GetBytes(contentId + "\0");
            new Global(GTA.Offsets.Editor.load_job_id).SetBytes(id);
            new Global(GTA.Offsets.Editor.load_job_flag).SetInt(1);
            CreatorMap.WriteLocal(state, StateStart);
            Log.Info($"load job {contentId}: creator restarted ({creator})", source: "copyjob");

            var started = DateTime.UtcNow;
            bool left = false;
            try
            {
                while (DateTime.UtcNow - started < Timeout)
                {
                    await Task.Delay(100).ConfigureAwait(true);
                    int now = CreatorMap.ReadLocal(state);
                    if (now != CreatorMap.StateEditing)
                        left = true;
                    else if (left)
                    {
                        // Loaded as someone else's published job; make it a new one.
                        CreatorMap.WriteLocal(worker + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_editing_published, 0);
                        Log.Info($"load job {contentId}: loaded after {(DateTime.UtcNow - started).TotalSeconds:0.0} s", source: "copyjob");
                        return Result.Loaded;
                    }
                }
                Log.Warn($"load job {contentId}: creator not back after {Timeout.TotalSeconds:0} s", source: "copyjob");
                return Result.TimedOut;
            }
            finally
            {
                // Left set, the creator loads the job again on its next start.
                new Global(GTA.Offsets.Editor.load_job_flag).SetInt(0);
            }
        }
    }
}
