using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// Jobs the player looked up by id, in My Jobs or in Copy Job, newest first. Kept in
    /// %AppData%\Xenvious\recent_jobs.json so the list survives a restart.
    /// </summary>
    public static class RecentJobs
    {
        private const int Max = 50;
        private static List<UgcList.Job> _jobs;

        /// <summary>Raised on the thread that added a job.</summary>
        public static event Action Changed;

        private static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Xenvious", "recent_jobs.json");

        public static IReadOnlyList<UgcList.Job> All
        {
            get
            {
                if (_jobs == null)
                {
                    try
                    {
                        _jobs = File.Exists(FilePath) ? JsonConvert.DeserializeObject<List<UgcList.Job>>(File.ReadAllText(FilePath)) : null;
                    }
                    catch (Exception ex)
                    {
                        Log.Debug("recent jobs: " + ex.Message, source: "recentjobs");
                    }
                    _jobs = _jobs ?? new List<UgcList.Job>();
                }
                return _jobs;
            }
        }

        /// <summary>Puts the job on top (once, the newest version).</summary>
        public static void Add(UgcList.Job job)
        {
            if (string.IsNullOrEmpty(job?.ContentId))
                return;
            var list = All as List<UgcList.Job>;
            list.RemoveAll(j => j.ContentId == job.ContentId);
            list.Insert(0, job);
            if (list.Count > Max)
                list.RemoveRange(Max, list.Count - Max);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(list));
            }
            catch (Exception ex)
            {
                Log.Debug("recent jobs: " + ex.Message, source: "recentjobs");
            }
            Changed?.Invoke();
        }
    }
}
