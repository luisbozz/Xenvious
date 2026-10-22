using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// The player's own jobs, all of them, from the cloud.
    ///
    /// The creator hub keeps only 31 published and 31 saved jobs (creator.c func_645/func_419:
    /// UGC_QUERY_MY_CONTENT from offset 0, Global_995355.f_4 and Global_1015489.f_33). The
    /// customfuncs payload of every creator has ul_main (ugclist.py in ysc-global-updater), which
    /// runs the same query with an offset Xenvious picks and copies the results into the data
    /// page, one page at a time.
    /// </summary>
    public static class UgcList
    {
        /// <summary>The creator hub's query kinds (creator.c func_622), as ul_main numbers them.</summary>
        public enum Kind { Published = 1, Saved = 2, Bookmarked = 3, RecentlyPlayed = 4, Friends = 5, Category = 6, TopRated = 7, Newest = 8, ById = 9 }

        // UGC_GET_CONTENT_CATEGORY values the creators always accept (creator.c func_435).
        public const int RockstarCreated = 1, RockstarVerified = 3;

        [Flags]
        public enum JobFlags
        {
            Published = 1, Verified = 2, Bookmarked = 4, HasRecord = 8, LoResPhoto = 16, HiResPhoto = 32, Mine = 64,
        }

        public sealed class Job
        {
            public string ContentId, RootContentId, Name, Creator;
            public int Version, Language, Category, RatingCount, Positive;
            public float Rating;
            public JobFlags Flags;
            public int[] Updated;   // raw UGC_GET_CONTENT_UPDATED_DATE slots
            public string Path0, Path1;   // UGC_GET_CONTENT_PATH of the job's files 0 and 1
            public bool HasPhoto => (Flags & (JobFlags.LoResPhoto | JobFlags.HiResPhoto)) != 0;
        }

        // Data page offsets, same as ugclist.py.
        private const int State = 0x2710, KindAt = 0x2718, OffsetAt = 0x2720, CountAt = 0x2728, Got = 0x2730, Total = 0x2738, Type = 0x2740;
        private const int CategoryAt = 0x2750, ContentIdAt = 0x2760;
        private const int Results = 0x800, Size = 0x180;
        public const int PageSize = 20;
        private const int Requested = 1, Done = 3, Failed = 4;

        /// <summary>Whether the payload that runs the query is there (a creator open, script features on).</summary>
        public static bool Available => DataPage() != 0;

        private static long DataPage()
        {
            if (MainWindow.m == null || !MainWindow.m.IsProcOpen)
                return 0;
            string script = GTA.CurrentCreatorName();
            if (script == "" || !ScriptVars.FeaturesOn || !ScriptSpace.Configured(script) || !ScrPatchesRunner.PageReady(script))
                return 0;
            return ScriptSpace.Get(script)?.Data ?? 0;
        }

        /// <summary>One job by its content id (the latest version); null when the cloud has none.</summary>
        public static async Task<Job> ByIdAsync(string contentId, CancellationToken cancel = default)
        {
            var page = await PageAsync(Kind.ById, 0, 1, cancel, contentId: contentId);
            return page?.Jobs.Count > 0 ? page.Value.Jobs[0] : null;
        }

        /// <summary>One page of up to <see cref="PageSize"/> jobs from <paramref name="offset"/>; null when the payload is not there or the cloud failed.</summary>
        public static async Task<(List<Job> Jobs, int Total)?> PageAsync(Kind kind, int offset, int count, CancellationToken cancel = default,
            int category = 0, string contentId = null)
        {
            long page = DataPage();
            if (page == 0)
                return null;
            count = Math.Min(count, PageSize);
            var type = new byte[16];
            Encoding.ASCII.GetBytes("gta5mission").CopyTo(type, 0);
            Write(page + Type, type);
            Write(page + KindAt, BitConverter.GetBytes((long)kind));
            Write(page + CategoryAt, BitConverter.GetBytes((long)category));
            var id = new byte[24];
            Encoding.ASCII.GetBytes((contentId ?? "").Trim(), 0, Math.Min(23, (contentId ?? "").Trim().Length), id, 0);
            Write(page + ContentIdAt, id);
            Write(page + OffsetAt, BitConverter.GetBytes((long)offset));
            Write(page + CountAt, BitConverter.GetBytes((long)count));
            Write(page + Got, new byte[8]);
            Write(page + State, BitConverter.GetBytes((long)Requested));

            // The cloud answers within a second or two; ul_main waits for a query of the creator's own first.
            var until = DateTime.UtcNow.AddSeconds(10);
            long state;
            while ((state = Read(page + State)) != Done && state != Failed)
            {
                if (DateTime.UtcNow > until || DataPage() != page)
                {
                    Log.Warn($"job list: no answer (state {state})", source: "ugclist");
                    return null;
                }
                await Task.Delay(100, cancel);
            }
            if (state == Failed)
            {
                Log.Warn($"job list: query failed (kind {kind}, offset {offset})", source: "ugclist");
                return null;
            }
            int got = (int)Math.Min(Read(page + Got), PageSize);
            int total = (int)Read(page + Total);
            var jobs = new List<Job>(got);
            byte[] all = MainWindow.m.memory((page + Results).ToString("X")).GetBytes(got * Size);
            for (int i = 0; i < got; i++)
                jobs.Add(Parse(all, i * Size));
            Log.Debug($"job list: kind {kind}, offset {offset}: {got} of {total}", source: "ugclist");
            return (jobs, total);
        }

        private static Job Parse(byte[] b, int at)
        {
            var updated = new int[7];
            for (int k = 0; k < updated.Length; k++)
                updated[k] = BitConverter.ToInt32(b, at + 0xC8 + k * 8);
            return new Job
            {
                ContentId = Text(b, at + 0x00, 24),
                RootContentId = Text(b, at + 0x18, 24),
                Name = Text(b, at + 0x30, 64),
                Creator = Text(b, at + 0x70, 32),
                Version = BitConverter.ToInt32(b, at + 0x90),
                Language = BitConverter.ToInt32(b, at + 0x98),
                Category = BitConverter.ToInt32(b, at + 0xA0),
                Flags = (JobFlags)BitConverter.ToInt32(b, at + 0xA8),
                Rating = BitConverter.ToSingle(b, at + 0xB0),
                RatingCount = BitConverter.ToInt32(b, at + 0xB8),
                Positive = BitConverter.ToInt32(b, at + 0xC0),
                Updated = updated,
                Path0 = Text(b, at + 0x100, 64),
                Path1 = Text(b, at + 0x140, 64),
            };
        }

        private static string Text(byte[] b, int at, int size)
        {
            int end = Array.IndexOf(b, (byte)0, at, size);
            return Encoding.UTF8.GetString(b, at, (end < 0 ? at + size : end) - at);
        }

        private static long Read(long at) => BitConverter.ToInt64(MainWindow.m.memory(at.ToString("X")).GetBytes(8), 0);

        private static void Write(long at, byte[] bytes) => MainWindow.m.memory(at.ToString("X")).SetBytes(bytes);
    }
}
