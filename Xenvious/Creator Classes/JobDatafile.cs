using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// The complete job as a file: the JSON the creator itself writes when it saves, the
    /// same format as the job files in Rockstar's cloud. It holds every field, including
    /// the ones Xenvious has no offsets for, and fits both editions and every build.
    ///
    /// The game keeps a job file as a tree of nodes (sveDict, sveArray, sveInt, ...) in one
    /// of its five script datafiles; the creators read and write datafile 0.
    ///
    /// Export: the creator's save builds the file in datafile 0 and then uploads it. With
    /// the upload step of the save state set to 4 ("finished") beforehand, the save builds
    /// the file and returns without uploading; Xenvious then reads the tree.
    ///
    /// Import: Xenvious builds the tree in memory it allocates in GTA, hangs it into
    /// datafile 0 and starts the creator's load with the loader at step 2, the offline path
    /// that reads the job from datafile 0 instead of downloading it. Afterwards datafile 0
    /// gets its old file back, so the game never frees memory it did not allocate.
    ///
    /// Race, Deathmatch, LTS, Capture and Survival creator, the ones <see cref="JobLoader"/>
    /// loads jobs in.
    /// </summary>
    public static class JobDatafile
    {
        public enum Result { Done, Unsupported, NotEditing, NoTitle, WrongCreator, NoJob, TimedOut, Failed }

        private enum NodeType { Bool = 1, Int = 2, Float = 3, String = 4, Vec3 = 5, Dict = 6, Array = 7 }

        // Datafile object: vtable, list node (16), the file (root dict) at +0x18, request id at +0x20.
        private const int FileStride = 40;
        private const int FileRoot = 0x18;

        private const int CreatorStateLoad = 0;
        private const int CreatorStateSave = 5;
        private const int LoaderStateFromDatafile = 2;
        private const int UploadFinished = 4;

        // Shown as the job's content id after an import, like a copied job's original id.
        private const string ImportedContentId = "XENVIOUS_IMPORT";

        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(90);

        // ---- availability ---------------------------------------------------------------

        private static long SaveOffset(string creator)
        {
            switch (creator)
            {
                case "fm_race_creator": return GTA.Offsets.Editor.job_save_race;
                case "fm_lts_creator": return GTA.Offsets.Editor.job_save_lts;
                case "fm_capture_creator": return GTA.Offsets.Editor.job_save_capture;
                case "fm_deathmatch_creator": return GTA.Offsets.Editor.job_save_dm;
                case "fm_survival_creator": return GTA.Offsets.Editor.job_save_survival;
                default: return 0;
            }
        }

        public static bool CanExport(string creator) =>
            JobLoader.CanLoad(creator)
            && SaveOffset(creator) != 0
            && GTA.Offsets.Editor.job_save_upload_state != 0
            && GTA.Offsets.Editor.job_save_uploaded != 0
            && FilesAddress() != 0;

        public static bool CanImport(string creator) =>
            JobLoader.CanLoad(creator)
            && JobLoader.LoaderOffset(creator) != 0
            && GTA.Offsets.Editor.load_job_loader_state != 0
            && GTA.Offsets.Editor.load_job_loader_read_stage != 0
            && FilesAddress() != 0
            && Vtables() != null;

        private static long ModuleBase => (long)MainWindow.m.getBaseAddress();

        /// <summary>
        /// Address of the game's datafile array, or 0 when the configured offset does not
        /// point at five datafile objects (another build).
        /// </summary>
        private static long FilesAddress()
        {
            long rva = GTA.Offsets.Editor.Datafile.files;
            if (rva <= 0 || !MainWindow.m.IsProcOpen)
                return 0;
            long files = ModuleBase + rva;
            byte[] raw = Read(files, 5 * FileStride);
            long vtable = BitConverter.ToInt64(raw, 0);
            if (!InModule(vtable))
                return 0;
            for (int i = 1; i < 5; i++)
            {
                if (BitConverter.ToInt64(raw, i * FileStride) != vtable)
                    return 0;
            }
            return files;
        }

        private static bool InModule(long address)
        {
            long size = MainWindow.m.getMainModule()?.ModuleMemorySize ?? 0;
            return address > ModuleBase && address < ModuleBase + size;
        }

        /// <summary>
        /// The node vtables by type, or null when the configured offsets do not look like
        /// them. All node types share the parser's virtual functions at the start of their
        /// vtables; another build's offsets point somewhere else.
        /// </summary>
        private static Dictionary<NodeType, long> Vtables()
        {
            var rvas = new Dictionary<NodeType, long>
            {
                [NodeType.Bool] = GTA.Offsets.Editor.Datafile.vt_bool,
                [NodeType.Int] = GTA.Offsets.Editor.Datafile.vt_int,
                [NodeType.Float] = GTA.Offsets.Editor.Datafile.vt_float,
                [NodeType.String] = GTA.Offsets.Editor.Datafile.vt_string,
                [NodeType.Vec3] = GTA.Offsets.Editor.Datafile.vt_vec3,
                [NodeType.Dict] = GTA.Offsets.Editor.Datafile.vt_dict,
                [NodeType.Array] = GTA.Offsets.Editor.Datafile.vt_array,
            };
            if (rvas.Values.Any(v => v <= 0) || !MainWindow.m.IsProcOpen)
                return null;
            var result = rvas.ToDictionary(p => p.Key, p => ModuleBase + p.Value);
            byte[] shared = Read(result[NodeType.Dict], 6 * 8);
            foreach (long vtable in result.Values)
            {
                byte[] head = Read(vtable, 6 * 8);
                if (!head.SequenceEqual(shared) || !InModule(BitConverter.ToInt64(head, 0)))
                    return null;
            }
            return result;
        }

        // ---- export ---------------------------------------------------------------------

        /// <summary>
        /// Lets the creator build the open job's file without uploading it and reads it.
        /// The job needs a title and a description, or the save would ask for them first.
        /// </summary>
        public static async Task<(Result Result, JObject Job)> ExportAsync()
        {
            string creator = CreatorMap.CurrentCreator();
            if (!CanExport(creator))
                return (Result.Unsupported, null);
            if (string.IsNullOrWhiteSpace(new Global(GTA.Offsets.Editor.nm).GetString())
                || string.IsNullOrWhiteSpace(new Global(GTA.Offsets.Editor.dec).GetString()))
                return (Result.NoTitle, null);

            long state = CreatorMap.WorkerOffset(creator) + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_refresh;
            if (CreatorMap.ReadLocal(state) != CreatorMap.StateEditing)
                return (Result.NotEditing, null);

            long save = SaveOffset(creator);
            CreatorMap.WriteLocal(save + GTA.Offsets.Editor.job_save_upload_state, UploadFinished);
            CreatorMap.WriteLocal(save + GTA.Offsets.Editor.job_save_uploaded, 0);
            CreatorMap.WriteLocal(state, CreatorStateSave);
            Log.Info($"job file export: save started without upload ({creator})", source: "jobfile");

            if (!await WaitForEditingAsync(state, save).ConfigureAwait(true))
            {
                Log.Warn($"job file export: creator not back after {Timeout.TotalSeconds:0} s", source: "jobfile");
                return (Result.TimedOut, null);
            }

            long root = ReadPtr(FilesAddress() + FileRoot);
            if (root == 0)
            {
                Log.Warn("job file export: datafile 0 is empty", source: "jobfile");
                return (Result.NoJob, null);
            }
            var reader = new TreeReader(Vtables());
            var job = reader.ReadNode(root) as JObject;
            if (job?["mission"] == null)
            {
                Log.Warn($"job file export: datafile 0 holds no job, keys: {string.Join(", ", job?.Properties().Select(p => p.Name) ?? new string[0])}", source: "jobfile");
                return (Result.NoJob, null);
            }
            Log.Info($"job file export: {reader.Nodes} nodes, {reader.UnknownKeys} keys without a name", source: "jobfile");
            return (Result.Done, job);
        }

        /// <summary>
        /// Waits until the creator left its editing state and came back to it. A save the creator
        /// gives up on returns within a frame or two, so the save stage also counts as having left.
        /// With <paramref name="save"/> every change of the save state goes to the log.
        /// </summary>
        private static async Task<bool> WaitForEditingAsync(long state, long save = 0)
        {
            var started = DateTime.UtcNow;
            bool left = false;
            string last = null;
            while (DateTime.UtcNow - started < Timeout)
            {
                await Task.Delay(10).ConfigureAwait(true);
                if (!MainWindow.m.IsProcOpen)
                    return false;
                int now = CreatorMap.ReadLocal(state);
                if (save != 0)
                {
                    int stage = CreatorMap.ReadLocal(save + GTA.Offsets.Editor.job_save_stage);
                    string trace = $"state {now}, stage {stage}, upload {CreatorMap.ReadLocal(save + GTA.Offsets.Editor.job_save_upload_state)}, "
                        + $"uploaded {CreatorMap.ReadLocal(save + GTA.Offsets.Editor.job_save_uploaded)}, datafile {ReadPtr(FilesAddress() + FileRoot):X}";
                    if (trace != last)
                        Log.Info($"job file export: {trace} after {(DateTime.UtcNow - started).TotalMilliseconds:0} ms", source: "jobfile");
                    last = trace;
                    if (stage != 0)
                        left = true;
                }
                if (now != CreatorMap.StateEditing)
                    left = true;
                else if (left)
                    return true;
            }
            return false;
        }

        /// <summary>The job as text, laid out the way people read it.</summary>
        public static string ToText(JObject job) => job.ToString(Formatting.Indented);

        // ---- import ---------------------------------------------------------------------

        /// <summary>Reads a job file; null when it is no job.</summary>
        public static JObject Parse(string text)
        {
            // Dates stay text and numbers keep int or float, the way the game tells them apart.
            var settings = new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None,
                FloatParseHandling = FloatParseHandling.Double
            };
            var job = JsonConvert.DeserializeObject<JToken>(text, settings) as JObject;
            return job?["mission"]?["gen"] is JObject ? job : null;
        }

        /// <summary>The creator script the job file belongs in, or null.</summary>
        public static string CreatorOf(JObject job)
        {
            var gen = job["mission"]?["gen"];
            int type = gen?["type"]?.Type == JTokenType.Integer ? (int)gen["type"] : -1;
            int subtype = gen?["subtype"]?.Type == JTokenType.Integer ? (int)gen["subtype"] : 0;
            return JobLoader.CreatorFor(type, subtype);
        }

        /// <summary>
        /// The open job's content id, and whether saving updates that job (a job of one's own
        /// that was saved before) instead of creating a new one.
        /// </summary>
        public static (string Id, bool Updates) Identity()
        {
            string creator = CreatorMap.CurrentCreator();
            long worker = CreatorMap.WorkerOffset(creator);
            string id = GTA.Offsets.Editor.jobid != 0 ? new Global(GTA.Offsets.Editor.jobid).GetString()?.Trim() : null;
            if (string.IsNullOrEmpty(id) || id == ImportedContentId || worker == 0)
                return (null, false);
            return (id, CreatorMap.ReadLocal(worker + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_editing_published) != 0);
        }

        /// <summary>
        /// Loads the job file into the open creator, replacing the job there. Without an
        /// <paramref name="identity"/> saving creates a new job. With one, the job is loaded under
        /// that content id: the creator takes the load id as the job's id (f_24631 of its save
        /// state), and with Updates set saving updates that job (pass <see cref="Identity"/> of
        /// the open job to keep it the same job).
        /// </summary>
        public static async Task<Result> ImportAsync(JObject job, (string Id, bool Updates) identity = default)
        {
            string creator = CreatorMap.CurrentCreator();
            if (!CanImport(creator))
                return Result.Unsupported;
            if (CreatorOf(job) != creator)
                return Result.WrongCreator;

            long worker = CreatorMap.WorkerOffset(creator);
            long state = worker + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_refresh;
            if (CreatorMap.ReadLocal(state) != CreatorMap.StateEditing)
                return Result.NotEditing;

            string keptId = string.IsNullOrEmpty(identity.Id) ? null : identity.Id;
            bool updates = keptId != null && identity.Updates;
            var builder = new TreeBuilder(Vtables());
            builder.Add(job);
            long memory = Allocate(builder.Size);
            if (memory == 0)
                return Result.Failed;
            long root = memory;   // the root is laid out first
            long slot = FilesAddress() + FileRoot;
            long original = ReadPtr(slot);
            bool hung = false;
            try
            {
                Write(memory, builder.Build(memory));

                long loader = JobLoader.LoaderOffset(creator);
                CreatorMap.WriteLocal(loader + GTA.Offsets.Editor.load_job_loader_read_stage, 0);
                CreatorMap.WriteLocal(loader + GTA.Offsets.Editor.load_job_loader_state, LoaderStateFromDatafile);
                new Global(GTA.Offsets.Editor.load_job_id).SetBytes(Encoding.ASCII.GetBytes((keptId ?? ImportedContentId) + "\0"));
                WritePtr(slot, root);
                hung = true;
                new Global(GTA.Offsets.Editor.load_job_flag).SetInt(1);
                CreatorMap.WriteLocal(state, CreatorStateLoad);
                Log.Info($"job file import: {builder.Nodes} nodes, {builder.Size} bytes, creator restarted ({creator})", source: "jobfile");

                if (!await WaitForEditingAsync(state).ConfigureAwait(true))
                {
                    Log.Warn($"job file import: creator not back after {Timeout.TotalSeconds:0} s", source: "jobfile");
                    return Result.TimedOut;
                }
                // Loaded like someone else's published job: a new one, unless it stays the open job.
                CreatorMap.WriteLocal(worker + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_editing_published, updates ? 1 : 0);
                Log.Info($"job file import: loaded{(keptId != null ? $" as {keptId}, saving {(updates ? "updates it" : "creates a new job")}" : "")}", source: "jobfile");
                return Result.Done;
            }
            finally
            {
                if (MainWindow.m.IsProcOpen)
                {
                    // Left set, the creator loads the job again on its next start.
                    new Global(GTA.Offsets.Editor.load_job_flag).SetInt(0);
                    bool ours = !hung || ReadPtr(slot) == root;
                    if (hung && ours)
                        WritePtr(slot, original);
                    // Only freed once the game can no longer reach it.
                    if (ours)
                        Free(memory);
                    else
                        Log.Warn("job file import: datafile 0 changed during the load, the tree stays allocated", source: "jobfile");
                }
            }
        }

        // ---- key names ------------------------------------------------------------------

        private static Dictionary<uint, string> names;

        /// <summary>
        /// Hash to key name. Keys numbered by the scripts ("cpbs0", "vcol12", ...) are
        /// covered by trying numbers behind every known name.
        /// </summary>
        private static Dictionary<uint, string> Names()
        {
            if (names != null)
                return names;
            var map = new Dictionary<uint, string>();
            var known = OfflineData.JobKeys.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim()).Where(k => k.Length > 0).ToList();
            foreach (string key in known)
                map[Hash(key)] = key;
            foreach (string key in known)
            {
                for (int i = 0; i < 200; i++)
                {
                    string numbered = key + i.ToString(CultureInfo.InvariantCulture);
                    uint hash = Hash(numbered);
                    if (!map.ContainsKey(hash))
                        map[hash] = numbered;
                }
            }
            return names = map;
        }

        // A key without a name is written as "#" and its hash; the import reads it back.
        private static string KeyName(uint hash, ref int unknown)
        {
            if (Names().TryGetValue(hash, out string name))
                return name;
            unknown++;
            return "#" + hash.ToString("X8", CultureInfo.InvariantCulture);
        }

        private static uint KeyHash(string key)
        {
            if (key.Length == 9 && key[0] == '#'
                && uint.TryParse(key.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint hash))
                return hash;
            return Hash(key);
        }

        /// <summary>The game's key hash (atStringHash): one-at-a-time over the lower-case key.</summary>
        private static uint Hash(string key)
        {
            uint h = 0;
            foreach (char c in key)
            {
                char lower = c >= 'A' && c <= 'Z' ? (char)(c + 32) : c == '\\' ? '/' : c;
                h += (byte)lower;
                h += h << 10;
                h ^= h >> 6;
            }
            h += h << 3;
            h ^= h >> 11;
            h += h << 15;
            return h;
        }

        // ---- reading the tree -----------------------------------------------------------

        private sealed class TreeReader
        {
            private readonly Dictionary<long, NodeType> types;
            public int Nodes;
            public int UnknownKeys;

            public TreeReader(Dictionary<NodeType, long> vtables)
            {
                types = vtables?.ToDictionary(p => p.Value, p => p.Key) ?? new Dictionary<long, NodeType>();
            }

            public JToken ReadNode(long node, int depth = 0)
            {
                if (node == 0 || depth > 32)
                    return JValue.CreateNull();
                Nodes++;
                byte[] raw = Read(node, 32);
                long vtable = BitConverter.ToInt64(raw, 0);
                // The root is a sCloudFile, a dict with a vtable of its own.
                if (!types.TryGetValue(vtable, out NodeType type))
                    type = depth == 0 ? NodeType.Dict : throw new InvalidDataException($"unknown node at {node:X}");
                switch (type)
                {
                    case NodeType.Bool: return new JValue(raw[8] != 0);
                    case NodeType.Int: return new JValue(BitConverter.ToInt32(raw, 8));
                    case NodeType.Float: return Float(BitConverter.ToSingle(raw, 8));
                    case NodeType.String:
                        {
                            long text = BitConverter.ToInt64(raw, 8);
                            int length = BitConverter.ToUInt16(raw, 16);
                            return new JValue(text == 0 || length == 0 ? "" : Encoding.UTF8.GetString(Read(text, length)));
                        }
                    case NodeType.Vec3:
                        return new JObject
                        {
                            ["x"] = Float(BitConverter.ToSingle(raw, 16)),
                            ["y"] = Float(BitConverter.ToSingle(raw, 20)),
                            ["z"] = Float(BitConverter.ToSingle(raw, 24))
                        };
                    case NodeType.Array:
                        {
                            var array = new JArray();
                            long elements = BitConverter.ToInt64(raw, 8);
                            int count = BitConverter.ToUInt16(raw, 16);
                            if (elements == 0 || count == 0)
                                return array;
                            byte[] pointers = Read(elements, count * 8);
                            for (int i = 0; i < count; i++)
                                array.Add(ReadNode(BitConverter.ToInt64(pointers, i * 8), depth + 1));
                            return array;
                        }
                    default:
                        {
                            var dict = new JObject();
                            long buckets = BitConverter.ToInt64(raw, 8);
                            int slots = BitConverter.ToUInt16(raw, 16);
                            if (buckets == 0 || slots == 0)
                                return dict;
                            byte[] heads = Read(buckets, slots * 8);
                            for (int i = 0; i < slots; i++)
                            {
                                long entry = BitConverter.ToInt64(heads, i * 8);
                                for (int guard = 0; entry != 0 && guard < 0x10000; guard++)
                                {
                                    byte[] e = Read(entry, 24);
                                    string key = KeyName(BitConverter.ToUInt32(e, 0), ref UnknownKeys);
                                    dict[key] = ReadNode(BitConverter.ToInt64(e, 8), depth + 1);
                                    entry = BitConverter.ToInt64(e, 16);
                                }
                            }
                            return dict;
                        }
                }
            }

            // Shortest text that reads back as the same float, so 0.3f stays 0.3.
            private static JValue Float(float value) =>
                new JValue(double.Parse(value.ToString("R", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture));
        }

        // ---- building the tree ----------------------------------------------------------

        /// <summary>
        /// Lays the nodes out in one block, the way the game's JSON reader would create them
        /// (sveNode::ReadJsonValue): a dict of exactly three floats x, y, z is a vector,
        /// numbers with a fraction or exponent are floats, null is the int 0.
        /// </summary>
        private sealed class TreeBuilder
        {
            private readonly Dictionary<NodeType, long> vtables;
            private readonly List<(JToken Token, NodeType Type, int Offset)> nodes = new List<(JToken, NodeType, int)>();
            private readonly Dictionary<JToken, int> offsets = new Dictionary<JToken, int>(ReferenceEqualityComparer.Instance);
            public int Size;
            public int Nodes => nodes.Count;

            public TreeBuilder(Dictionary<NodeType, long> vtables)
            {
                this.vtables = vtables;
            }

            private static NodeType TypeOf(JToken token)
            {
                switch (token.Type)
                {
                    case JTokenType.String: return NodeType.String;
                    case JTokenType.Boolean: return NodeType.Bool;
                    case JTokenType.Float: return NodeType.Float;
                    case JTokenType.Array: return NodeType.Array;
                    case JTokenType.Object:
                        var o = (JObject)token;
                        return o.Count == 3 && new[] { "x", "y", "z" }.All(k => o[k]?.Type == JTokenType.Float)
                            ? NodeType.Vec3 : NodeType.Dict;
                    default: return NodeType.Int;   // integers, and null like the game
                }
            }

            private static int Align(int value) => (value + 15) & ~15;

            // Room behind the node object: buckets and entries, element pointers, text.
            private static int Extra(JToken token, NodeType type)
            {
                switch (type)
                {
                    case NodeType.Dict:
                        int count = ((JObject)token).Count;
                        return Align(Slots(count) * 8) + Align(count * 24);
                    case NodeType.Array: return Align(((JArray)token).Count * 8);
                    case NodeType.String: return Align(Encoding.UTF8.GetByteCount((string)token) + 1);
                    default: return 0;
                }
            }

            private static int ObjectSize(NodeType type) =>
                type == NodeType.Dict || type == NodeType.Array || type == NodeType.String || type == NodeType.Vec3 ? 32 : 16;

            // Any slot count works (key % slots); a prime near the count keeps chains short.
            private static int Slots(int count)
            {
                if (count == 0)
                    return 0;
                for (int n = Math.Max(count, 3); ; n++)
                {
                    bool prime = true;
                    for (int d = 2; d * d <= n && prime; d++)
                        prime = n % d != 0;
                    if (prime)
                        return n;
                }
            }

            public void Add(JToken token)
            {
                NodeType type = TypeOf(token);
                offsets[token] = Size;
                nodes.Add((token, type, Size));
                Size += ObjectSize(type) + Extra(token, type);
                if (type == NodeType.Dict)
                {
                    foreach (var property in ((JObject)token).Properties())
                        Add(property.Value);
                }
                else if (type == NodeType.Array)
                {
                    foreach (var item in (JArray)token)
                        Add(item);
                }
            }

            /// <summary>The block's bytes for the address it will be written to.</summary>
            public byte[] Build(long baseAddress)
            {
                var block = new byte[Size];
                void Put64(int at, long v) => BitConverter.GetBytes(v).CopyTo(block, at);
                void Put32(int at, int v) => BitConverter.GetBytes(v).CopyTo(block, at);
                void Put16(int at, int v) => BitConverter.GetBytes((ushort)v).CopyTo(block, at);

                foreach (var (token, type, at) in nodes)
                {
                    Put64(at, vtables[type]);
                    int extra = at + ObjectSize(type);
                    switch (type)
                    {
                        case NodeType.Bool:
                            block[at + 8] = (bool)token ? (byte)1 : (byte)0;
                            break;
                        case NodeType.Int:
                            // Out-of-range numbers wrap like the hashes the scripts store as negative ints.
                            Put32(at + 8, token.Type == JTokenType.Integer && ((JValue)token).Value is long number ? unchecked((int)number) : 0);
                            break;
                        case NodeType.Float:
                            BitConverter.GetBytes((float)(double)token).CopyTo(block, at + 8);
                            break;
                        case NodeType.Vec3:
                            BitConverter.GetBytes((float)(double)token["x"]).CopyTo(block, at + 16);
                            BitConverter.GetBytes((float)(double)token["y"]).CopyTo(block, at + 20);
                            BitConverter.GetBytes((float)(double)token["z"]).CopyTo(block, at + 24);
                            break;
                        case NodeType.String:
                            {
                                byte[] text = Encoding.UTF8.GetBytes((string)token);
                                text.CopyTo(block, extra);   // zero-terminated by the empty block
                                Put64(at + 8, baseAddress + extra);
                                Put16(at + 16, text.Length);
                                Put16(at + 18, text.Length + 1);
                                break;
                            }
                        case NodeType.Array:
                            {
                                var array = (JArray)token;
                                for (int i = 0; i < array.Count; i++)
                                    Put64(extra + i * 8, baseAddress + offsets[array[i]]);
                                Put64(at + 8, array.Count == 0 ? 0 : baseAddress + extra);
                                Put16(at + 16, array.Count);
                                Put16(at + 18, array.Count);
                                break;
                            }
                        case NodeType.Dict:
                            {
                                var properties = ((JObject)token).Properties().ToList();
                                int slots = Slots(properties.Count);
                                int buckets = extra;
                                int entries = extra + Align(slots * 8);
                                for (int i = 0; i < properties.Count; i++)
                                {
                                    uint hash = KeyHash(properties[i].Name);
                                    int entry = entries + i * 24;
                                    int bucket = buckets + (int)(hash % (uint)slots) * 8;
                                    Put32(entry, unchecked((int)hash));
                                    Put64(entry + 8, baseAddress + offsets[properties[i].Value]);
                                    Put64(entry + 16, BitConverter.ToInt64(block, bucket));   // chain the bucket's head
                                    Put64(bucket, baseAddress + entry);
                                }
                                Put64(at + 8, slots == 0 ? 0 : baseAddress + buckets);
                                Put16(at + 16, slots);
                                Put16(at + 18, properties.Count);
                                break;
                            }
                    }
                }
                return block;
            }
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<JToken>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
            public bool Equals(JToken a, JToken b) => ReferenceEquals(a, b);
            public int GetHashCode(JToken token) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(token);
        }

        // ---- memory ---------------------------------------------------------------------

        private static byte[] Read(long address, int length) =>
            MainWindow.m.memory(address.ToString("X")).GetBytes(length);

        private static long ReadPtr(long address) => BitConverter.ToInt64(Read(address, 8), 0);

        private static void Write(long address, byte[] bytes) =>
            MainWindow.m.memory(address.ToString("X")).SetBytes(bytes);

        private static void WritePtr(long address, long value) => Write(address, BitConverter.GetBytes(value));

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr process, IntPtr address, UIntPtr size, uint type, uint protect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFreeEx(IntPtr process, IntPtr address, UIntPtr size, uint type);

        private const uint MemCommitReserve = 0x3000;
        private const uint MemRelease = 0x8000;
        private const uint PageReadWrite = 0x04;

        private static long Allocate(int size)
        {
            IntPtr at = VirtualAllocEx(MainWindow.m.getModuleHandle(), IntPtr.Zero, (UIntPtr)(uint)size, MemCommitReserve, PageReadWrite);
            if (at == IntPtr.Zero)
                Log.Warn($"job file import: could not allocate {size} bytes in GTA (error {Marshal.GetLastWin32Error()})", source: "jobfile");
            return at.ToInt64();
        }

        private static void Free(long address)
        {
            if (address != 0)
                VirtualFreeEx(MainWindow.m.getModuleHandle(), (IntPtr)address, UIntPtr.Zero, MemRelease);
        }
    }
}
