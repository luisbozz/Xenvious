using System;
using System.Text;
using System.Text.RegularExpressions;
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
    /// With the job file known (version and language), the creator fetches that file
    /// directly instead of searching the job first. The search only finds jobs of the
    /// own platform, so a job made on another platform fails there with "The Job failed
    /// to download"; the file itself loads on every platform.
    ///
    /// Race, Deathmatch, LTS, Capture and Survival creator; all five load in state 0 and
    /// edit in state 3. Afterwards the job counts as a new one, so saving it creates a job
    /// in the player's account instead of trying to update the original.
    /// </summary>
    public static class JobLoader
    {
        public enum Result { Loaded, Unsupported, NotEditing, TimedOut }

        private const int StateStart = 0;
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(90);

        // The Public Mission Creator loads jobs outside its start state, so not this way.
        public static bool CanLoad(string creator) =>
            creator != "public_mission_creator"
            && WorkerOffset(creator) != 0
            && GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_refresh != 0
            && GTA.Offsets.Editor.load_job_flag != 0
            && GTA.Offsets.Editor.load_job_id != 0
            && GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_editing_published != 0;

        /// <summary>The creator script a job of this type opens in, or null.</summary>
        public static string CreatorFor(int type, int subtype)
        {
            if (type == 1)
                return "fm_deathmatch_creator";
            if (type == 2)
                return "fm_race_creator";
            if (type == 3)
                return "fm_survival_creator";
            if (type == 0 && subtype == 5)
                return "fm_lts_creator";
            if (type == 0 && subtype == 6)
                return "fm_capture_creator";
            return null;
        }

        // The creator's language numbers (fm_*_creator.c turns them into game languages),
        // by the language part of the job file name.
        private static readonly string[] FileLanguages =
            { "zh", "en", "fr", "de", "it", "ja", "ko", "pl", "pt-pt", "pt", "ru", "es", "es-mx", "zh-cn" };

        // CreatorMap leaves deathmatch and survival out, since their rebuild is not verified;
        // loading a job only needs the state field, which they share with the others.
        private static long WorkerOffset(string creator)
        {
            switch (creator)
            {
                case "fm_deathmatch_creator": return GTA.Offsets.Editor.OFFSET_current_creator_worker_dm;
                case "fm_survival_creator": return GTA.Offsets.Editor.OFFSET_current_creator_worker_survival;
                default: return CreatorMap.WorkerOffset(creator);
            }
        }

        private static long LoaderOffset(string creator)
        {
            switch (creator)
            {
                case "fm_race_creator": return GTA.Offsets.Editor.load_job_loader_race;
                case "fm_lts_creator": return GTA.Offsets.Editor.load_job_loader_lts;
                case "fm_capture_creator": return GTA.Offsets.Editor.load_job_loader_capture;
                case "fm_deathmatch_creator": return GTA.Offsets.Editor.load_job_loader_dm;
                case "fm_survival_creator": return GTA.Offsets.Editor.load_job_loader_survival;
                default: return 0;
            }
        }

        /// <summary>Version and language number from a job file link (.../0_&lt;version&gt;_&lt;language&gt;.json), or -1.</summary>
        public static (int Version, int Language) ParseJobFile(string jobFile)
        {
            var match = Regex.Match(jobFile ?? "", @"/\d+_(\d+)_([\w-]+)\.json$");
            if (!match.Success || !int.TryParse(match.Groups[1].Value, out int version))
                return (-1, -1);
            return (version, Array.IndexOf(FileLanguages, match.Groups[2].Value.ToLowerInvariant()));
        }

        /// <summary>The language part of the job file name for a language number.</summary>
        public static string LanguageCode(int language) =>
            language >= 0 && language < FileLanguages.Length ? FileLanguages[language] : null;

        public static async Task<Result> LoadAsync(string contentId, string jobFile = null)
        {
            string creator = CreatorMap.CurrentCreator();
            if (!CanLoad(creator) || string.IsNullOrEmpty(contentId))
                return Result.Unsupported;

            long worker = WorkerOffset(creator);
            long state = worker + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_refresh;
            if (CreatorMap.ReadLocal(state) != CreatorMap.StateEditing)
                return Result.NotEditing;

            // The id is a plain text label: ASCII, zero-terminated.
            byte[] id = Encoding.ASCII.GetBytes(contentId + "\0");
            new Global(GTA.Offsets.Editor.load_job_id).SetBytes(id);

            // Both set (neither -1), the creator requests this file instead of searching the job.
            long loader = LoaderOffset(creator);
            var (version, language) = ParseJobFile(jobFile);
            bool direct = loader != 0 && GTA.Offsets.Editor.load_job_loader_version != 0 && GTA.Offsets.Editor.load_job_loader_language != 0
                && version >= 0 && language >= 0;
            if (direct)
            {
                CreatorMap.WriteLocal(loader + GTA.Offsets.Editor.load_job_loader_version, version);
                CreatorMap.WriteLocal(loader + GTA.Offsets.Editor.load_job_loader_language, language);
            }
            new Global(GTA.Offsets.Editor.load_job_flag).SetInt(1);
            CreatorMap.WriteLocal(state, StateStart);
            Log.Info($"load job {contentId}: creator restarted ({creator}, {(direct ? $"file version {version}, language {language}" : "search")})", source: "copyjob");

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
                // Left set, its next load would fetch this file instead of the job it asks for.
                if (direct && MainWindow.m.IsProcOpen)
                {
                    CreatorMap.WriteLocal(loader + GTA.Offsets.Editor.load_job_loader_version, -1);
                    CreatorMap.WriteLocal(loader + GTA.Offsets.Editor.load_job_loader_language, -1);
                }
            }
        }
    }
}
