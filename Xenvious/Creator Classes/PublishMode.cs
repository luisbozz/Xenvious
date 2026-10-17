using System;
using System.Text;
using System.Text.RegularExpressions;
using Xenvious.Logging;
using E = Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    /// <summary>
    /// Whether the creator's Publish makes a new job or updates a published one. All five
    /// creators decide the same way when they publish (worker state 91; fm_capture_creator
    /// func_568, 1.73 Legacy and Enhanced):
    ///   job_published set       UGC_PUBLISH(saved copy, base id): updates the published job
    ///   else editing_published  publishes the saved job under its own id
    ///   else                    a new job
    /// The base id is the save state's content id (f_24631). The creator sets the published flag
    /// only when it loads a job by search (UGC_GET_CONTENT_IS_PUBLISHED) or after a publish, so
    /// a job that came in through Copy Job or a job file always publishes as a new job.
    /// <see cref="SetUpdate"/> writes base id and flag like a load by search does; the cloud only
    /// accepts the update for the player's own job.
    /// </summary>
    public static class PublishMode
    {
        public enum Mode { Unknown, NewJob, PublishSaved, Update }

        // The save state's content id is a TEXT_LABEL_23: 24 bytes over three locals.
        private const int IdBytes = 24;

        /// <summary>The job loaded last through Xenvious (Copy Job), as the default for an update.</summary>
        public static string LastLoadedId { get; set; }

        public static bool Available =>
            MainWindow.m != null && MainWindow.m.IsProcOpen && E.jobpublished != 0 && E.job_save_content_id != 0 && E.jobid != 0;

        /// <summary>What the next Publish does, and the base id it would update.</summary>
        public static (Mode Mode, string BaseId) Current()
        {
            if (!Available)
                return (Mode.Unknown, null);
            string creator = CreatorMap.CurrentCreator();
            long save = JobDatafile.SaveOffset(creator);
            long worker = CreatorMap.WorkerOffset(creator);
            if (save == 0 || worker == 0)
                return (Mode.Unknown, null);
            string baseId = ReadId(save + E.job_save_content_id);
            if (new Global(E.jobpublished).Get<int>() != 0 && IsContentId(baseId))
                return (Mode.Update, baseId);
            if (CreatorMap.ReadLocal(worker + E.OFFSET_current_creator_worker_offset_editing_published) != 0 && IsContentId(baseId))
                return (Mode.PublishSaved, baseId);
            return (Mode.NewJob, null);
        }

        /// <summary>Makes the next Publish update the published job <paramref name="contentId"/>.</summary>
        public static bool SetUpdate(string contentId)
        {
            if (!Available || !IsContentId(contentId))
                return false;
            long save = JobDatafile.SaveOffset(CreatorMap.CurrentCreator());
            if (save == 0)
                return false;
            byte[] id = new byte[IdBytes];
            Encoding.ASCII.GetBytes(contentId, 0, contentId.Length, id, 0);
            CreatorMap.WriteLocalBytes(save + E.job_save_content_id, id);
            // The creator keeps the loaded id here as well and copies it back after a publish.
            new Global(E.jobid).SetBytes(id);
            new Global(E.jobpublished).SetInt(1);
            Log.Info($"publish: set to update {contentId}", source: "publish");
            return true;
        }

        /// <summary>A content id from a typed id or a job link (…/job/gtav/&lt;id&gt;), or null.</summary>
        public static string ParseId(string text)
        {
            var m = Regex.Match(text ?? "", @"([A-Za-z0-9_-]{22})(?![A-Za-z0-9_-])");
            return m.Success ? m.Groups[1].Value : null;
        }

        private static bool IsContentId(string id) => id != null && Regex.IsMatch(id, @"^[A-Za-z0-9_-]{22}$");

        private static string ReadId(long index)
        {
            byte[] bytes = CreatorMap.ReadLocalBytes(index, IdBytes);
            int end = Array.IndexOf(bytes, (byte)0);
            return Encoding.ASCII.GetString(bytes, 0, end < 0 ? IdBytes : end);
        }
    }
}
