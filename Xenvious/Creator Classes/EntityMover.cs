using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// Shows a changed position, rotation or heading of one placed prop, dynamic prop, capture
    /// object, vehicle, actor, weapon, team start point or spawn point at once, by moving what already stands in the
    /// world, instead of a rebuild (CreatorMap), which deletes and recreates every entity and takes
    /// seconds.
    ///
    /// Each creator keeps the handles of its placed entities in arrays in its locals
    /// ([ENTITYHANDLES] in offsets.ini, found by ysc-global-updater). Xenvious reads the handle
    /// there and the customfuncs payload (mover.py in ysc-global-updater) applies the pose from the
    /// job data with the natives the creator places that kind with. Start and spawn points are decals:
    /// the payload removes the decal and the creator adds it again with the new position and
    /// heading. Without a handle, or when the payload does not run, the caller falls back to the
    /// rebuild.
    ///
    /// Data page (string page 63) at 0x3700: state (1 request, 2 done, 3 no such entity), mode
    /// (1 SET_ENTITY_COORDS, 2 _NO_OFFSET, 3 remove the decal), handle, x/y/z, rotation x/y/z,
    /// rotate (1 rotation, 0 heading), heading; one 8 byte slot each, the value in the low 4 bytes.
    /// Mode 4 (SyncPropsAsync) brings a whole block of props in line with the job data: it creates
    /// missing entities, moves the others and deletes those of props after the block.
    /// </summary>
    public static class EntityMover
    {
        public enum Kind { Prop, DynamicProp, Object, Vehicle, Actor, Weapon, StartPoint, SpawnPoint }

        private const int Base = 0x3700;
        private const int StateAt = Base, ModeAt = Base + 0x08, HandleAt = Base + 0x10, PosAt = Base + 0x18,
            RotAt = Base + 0x30, RotateAt = Base + 0x48, HeadingAt = Base + 0x50;
        private const int SetCoords = 1, SetCoordsNoOffset = 2, RemoveDecal = 3, SyncBlock = 4;
        // Mode 4: pointers and counts after the mode.
        private const int BlockHandlesAt = Base + 0x10, BlockDataAt = Base + 0x18, BlockStrideAt = Base + 0x20,
            BlockCountAt = Base + 0x28, BlockDeleteAt = Base + 0x30, BlockRotAt = Base + 0x38, BlockModelAt = Base + 0x40;
        // One request at a time: single moves and block syncs share the data page slots.
        private static readonly SemaphoreSlim Gate = new SemaphoreSlim(1, 1);

        // The [ENTITYHANDLES] name of each kind.
        private static string Key(Kind kind)
        {
            switch (kind)
            {
                case Kind.Prop: return "props";
                case Kind.DynamicProp: return "dprops";
                case Kind.Object: return "obj";
                case Kind.Vehicle: return "veh";
                case Kind.Actor: return "actor";
                case Kind.Weapon: return "weap";
                case Kind.SpawnPoint: return "spawn";
                default: return "team";
            }
        }

        private struct HandleArray { public long First, Size, TeamStride, EntryStride; }

        // "props=7593:200, ..., team=2731:0+313, cps=52465:55/8" -> per kind.
        private static readonly Dictionary<string, Dictionary<string, HandleArray>> Parsed = new Dictionary<string, Dictionary<string, HandleArray>>();

        private static HandleArray? Array(string script, Kind kind)
        {
            if (!Parsed.TryGetValue(script, out var kinds))
            {
                kinds = new Dictionary<string, HandleArray>();
                if (GTA.Offsets.Editor.EntityHandles.TryGetValue(script, out string text))
                {
                    foreach (string part in text.Split(','))
                    {
                        string[] kv = part.Trim().Split('=');
                        if (kv.Length != 2)
                            continue;
                        string[] at = kv[1].Split(':');
                        string[] size = at.Length == 2 ? at[1].Split('+', '/') : new string[0];
                        if (at.Length == 2 && long.TryParse(at[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out long first)
                            && long.TryParse(size[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out long n))
                        {
                            long stride = 0;
                            if (size.Length == 2)
                                long.TryParse(size[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out stride);
                            bool entries = at[1].Contains("/");
                            kinds[kv[0]] = new HandleArray { First = first, Size = n, TeamStride = entries ? 0 : stride, EntryStride = entries ? stride : 1 };
                        }
                    }
                }
                Parsed[script] = kinds;
            }
            return kinds.TryGetValue(Key(kind), out var a) ? a : (HandleArray?)null;
        }

        private static HandleArray? Array(string script, string key)
        {
            Array(script, Kind.Prop);
            return Parsed.TryGetValue(script, out var kinds) && kinds.TryGetValue(key, out var a) ? a : (HandleArray?)null;
        }

        private static long LocalAddress(long index)
        {
            if (GTA.Offsets.Editor.localptr == null)
                GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy();
            var e = GTA.Offsets.Editor.localptr;
            if (e == null)
                return 0;
            return MainWindow.m.memory(e[0], new long[] { e[1], GTA.Offsets.Editor.OFFSET_script_local_start, index * 8 }).GetAddress();
        }

        private static int ReadLocal(long index)
        {
            long at = LocalAddress(index);
            return at == 0 ? 0 : MainWindow.m.memory(at.ToString("X")).Get<int>();
        }

        /// <summary>
        /// The handle the creator keeps for this entity, 0 when there is none. The slot before
        /// element 0 holds the array size; when it does not, the offsets are not for this build
        /// and nothing is read from that array.
        /// </summary>
        private static int Handle(string script, Kind kind, int team, int index)
        {
            long first = CheckedArray(script, kind, team, out int size);
            return first != 0 && index >= 0 && index < size ? ReadLocal(first + index) : 0;
        }

        // Local index of element 0 of the kind's handle array, 0 when it is unknown or its size slot
        // does not hold the size the build data names.
        private static long CheckedArray(string script, Kind kind, int team, out int size)
        {
            size = 0;
            var a = Array(script, kind);
            if (a == null)
                return 0;
            long first = a.Value.First + (kind == Kind.StartPoint ? team * a.Value.TeamStride : 0);
            size = ReadLocal(first - 1);
            if (a.Value.Size != 0 ? size != a.Value.Size : size <= 0 || size > 1000)
            {
                Log.Warn($"entity mover: {script} {kind} array size {size}, expected {a.Value.Size}", source: "EntityMover");
                size = 0;
                return 0;
            }
            return first;
        }

        private struct Pose { public float X, Y, Z, Rx, Ry, Rz, Heading; public bool Rotate; public int Mode; }

        private static float F(long global) => new Global(global).Get<float>();

        // The pose the job data gives, and how the creator applies it when it places that kind
        // (fm_lts_creator 1.73: func_1161 props, func_1146 dynamic props, func_2679 objects,
        // func_1019 vehicles, func_949 actors).
        private static Pose? Read(Kind kind, int team, int index)
        {
            long loc, rot = 0, head;
            int mode;
            switch (kind)
            {
                case Kind.Prop:
                    loc = GTA.Offsets.Editor.Props.loc + index * GTA.Offsets.Editor.Props.NEXT;
                    rot = GTA.Offsets.Editor.Props.vrot + index * GTA.Offsets.Editor.Props.NEXT;
                    head = GTA.Offsets.Editor.Props.head + index * GTA.Offsets.Editor.Props.NEXT;
                    mode = SetCoords;
                    break;
                case Kind.DynamicProp:
                    loc = GTA.Offsets.Editor.DProps.loc + index * GTA.Offsets.Editor.DProps.NEXT;
                    rot = GTA.Offsets.Editor.DProps.vrot + index * GTA.Offsets.Editor.DProps.NEXT;
                    head = GTA.Offsets.Editor.DProps.head + index * GTA.Offsets.Editor.DProps.NEXT;
                    mode = SetCoords;
                    break;
                case Kind.Object:
                    loc = GTA.Offsets.Editor.Objects.loc + index * GTA.Offsets.Editor.Objects.NEXT;
                    rot = GTA.Offsets.Editor.Objects.vrot + index * GTA.Offsets.Editor.Objects.NEXT;
                    head = GTA.Offsets.Editor.Objects.head + index * GTA.Offsets.Editor.Objects.NEXT;
                    mode = SetCoordsNoOffset;
                    break;
                case Kind.Vehicle:
                    loc = GTA.Offsets.Editor.Vehicle.loc + index * GTA.Offsets.Editor.Vehicle.NEXT;
                    rot = GTA.Offsets.Editor.Vehicle.vrot + index * GTA.Offsets.Editor.Vehicle.NEXT;
                    head = GTA.Offsets.Editor.Vehicle.head + index * GTA.Offsets.Editor.Vehicle.NEXT;
                    // CREATE_VEHICLE and the interior fix-up (func_1136) take the job position as
                    // the ground point; _NO_OFFSET would put the vehicle's centre there, half in the ground.
                    mode = SetCoords;
                    break;
                case Kind.Weapon:
                    // Weapon objects: SET_ENTITY_ROTATION with the rotation (rot x, rot y, heading)
                    // right after CREATE_WEAPON_OBJECT, whatever it is (fm_lts_creator 1.73 at 98016).
                    loc = GTA.Offsets.Editor.Weapon.locx + index * GTA.Offsets.Editor.Weapon.NEXT;
                    head = GTA.Offsets.Editor.Weapon.head + index * GTA.Offsets.Editor.Weapon.NEXT;
                    if (loc == 0 || GTA.Offsets.Editor.Weapon.rotx == 0)
                        return null;
                    return new Pose
                    {
                        X = F(loc), Y = F(loc + 1), Z = F(loc + 2),
                        Rx = F(GTA.Offsets.Editor.Weapon.rotx + index * GTA.Offsets.Editor.Weapon.NEXT),
                        Ry = F(GTA.Offsets.Editor.Weapon.roty + index * GTA.Offsets.Editor.Weapon.NEXT),
                        Rz = F(head), Heading = F(head), Rotate = true, Mode = SetCoords,
                    };
                case Kind.Actor:
                    loc = GTA.Offsets.Editor.Actor.locx + index * GTA.Offsets.Editor.Actor.NEXT;
                    head = GTA.Offsets.Editor.Actor.head + index * GTA.Offsets.Editor.Actor.NEXT;
                    // CREATE_PED stands the ped on the job position; _NO_OFFSET sinks it to the hips.
                    mode = SetCoords;
                    break;
                case Kind.SpawnPoint:
                    loc = GTA.Offsets.Editor.SpawnPoints.loc + index * GTA.Offsets.Editor.SpawnPoints.NEXT;
                    head = GTA.Offsets.Editor.SpawnPoints.head + index * GTA.Offsets.Editor.SpawnPoints.NEXT;
                    mode = RemoveDecal;
                    break;
                default:
                    loc = GTA.Offsets.Editor.player_loc + team * GTA.Offsets.Editor.team_NEXT_settings + index * GTA.Offsets.Editor.next_settings;
                    head = GTA.Offsets.Editor.player_head + team * GTA.Offsets.Editor.team_NEXT_settings + index * GTA.Offsets.Editor.next_settings;
                    mode = RemoveDecal;
                    break;
            }
            if (loc == 0)
                return null;
            var p = new Pose { X = F(loc), Y = F(loc + 1), Z = F(loc + 2), Heading = head != 0 ? F(head) : 0, Mode = mode };
            if (rot != 0)
            {
                p.Rx = F(rot);
                p.Ry = F(rot + 1);
                p.Rz = F(rot + 2);
                // Unset rotations: 0,0,0, and for vehicles also 999,999,999 (func_1139).
                p.Rotate = !(p.Rx == 0 && p.Ry == 0 && p.Rz == 0) && !(p.Rx == 999 && p.Ry == 999 && p.Rz == 999);
            }
            return p;
        }

        private static long DataPage(string script)
        {
            if (script == "" || !ScriptVars.FeaturesOn || !ScriptSpace.Configured(script) || !ScrPatchesRunner.PageReady(script))
                return 0;
            return ScriptSpace.Get(script)?.Data ?? 0;
        }

        private static void Write(long at, byte[] bytes) => MainWindow.m.memory(at.ToString("X")).SetBytes(bytes);
        private static void WriteInt(long at, long value) => Write(at, BitConverter.GetBytes(value));
        private static void WriteFloat(long at, float value)
        {
            var slot = new byte[8];
            BitConverter.GetBytes(value).CopyTo(slot, 0);
            Write(at, slot);
        }

        private static (Kind, int, int)? _pending;
        private static bool _busy;

        /// <summary>
        /// Applies what the job data now says to the entity. False when it could not (no handle,
        /// payload not running, entity gone); the caller then rebuilds. While a move runs, a new
        /// request replaces any waiting one and runs right after it, so typing or dragging a
        /// slider sends only the latest pose.
        /// </summary>
        public static async Task<bool> MoveAsync(Kind kind, int index, int team = 0)
        {
            if (!MainWindow.m.IsProcOpen || index < 0)
                return false;
            if (_busy)
            {
                _pending = (kind, index, team);
                return true;
            }
            _busy = true;
            try
            {
                bool ok = await Send(kind, index, team).ConfigureAwait(true);
                while (_pending != null)
                {
                    var next = _pending.Value;
                    _pending = null;
                    ok = await Send(next.Item1, next.Item2, next.Item3).ConfigureAwait(true);
                }
                return ok;
            }
            finally
            {
                _busy = false;
            }
        }

        private static async Task<bool> Send(Kind kind, int index, int team)
        {
            await Gate.WaitAsync().ConfigureAwait(true);
            try
            {
                return await SendLocked(kind, index, team).ConfigureAwait(true);
            }
            finally
            {
                Gate.Release();
            }
        }

        private static async Task<bool> SendLocked(Kind kind, int index, int team)
        {
            string script = GTA.CurrentCreatorName();
            long page = DataPage(script);
            if (page == 0)
                return false;
            int handle = Handle(script, kind, team, index);
            var pose = Read(kind, team, index);
            if (handle == 0 || pose == null)
                return false;
            var p = pose.Value;
            WriteInt(page + ModeAt, p.Mode);
            WriteInt(page + HandleAt, (uint)handle);
            WriteFloat(page + PosAt, p.X);
            WriteFloat(page + PosAt + 8, p.Y);
            WriteFloat(page + PosAt + 16, p.Z);
            WriteFloat(page + RotAt, p.Rx);
            WriteFloat(page + RotAt + 8, p.Ry);
            WriteFloat(page + RotAt + 16, p.Rz);
            WriteInt(page + RotateAt, p.Rotate ? 1 : 0);
            WriteFloat(page + HeadingAt, p.Heading);
            int state = await Request(page, 20).ConfigureAwait(true);
            if (state == 3)
                Log.Debug($"entity mover: {script} {kind} {index}: handle {handle} is no entity", source: "EntityMover");
            return state == 2;
        }

        // Sets the request and waits for the payload (it runs once per frame): its answer, or 0
        // after `frames` polls, when the request is taken back.
        private static async Task<int> Request(long page, int frames)
        {
            WriteInt(page + StateAt, 1);
            for (int i = 0; i < frames; i++)
            {
                await Task.Delay(15).ConfigureAwait(true);
                int state = MainWindow.m.memory((page + StateAt).ToString("X")).Get<int>();
                if (state == 2 || state == 3)
                    return state;
            }
            WriteInt(page + StateAt, 0);
            return 0;
        }

        /// <summary>
        /// Brings the placed props first .. first+count-1 in GTA in line with the job data: missing
        /// entities are created (the creator's handle array gets them), the rest moved and turned.
        /// The entities of the next <paramref name="deleteAfter"/> props are deleted, for props
        /// the job no longer has. False when that is not possible here; the caller then rebuilds.
        /// </summary>
        public static async Task<bool> SyncPropsAsync(int first, int count, int deleteAfter)
        {
            if (!MainWindow.m.IsProcOpen || first < 0 || count < 0 || deleteAfter < 0)
                return false;
            await Gate.WaitAsync().ConfigureAwait(true);
            try
            {
                string script = GTA.CurrentCreatorName();
                long page = DataPage(script);
                int size = 0;
                long handles = page == 0 ? 0 : CheckedArray(script, Kind.Prop, 0, out size);
                if (handles == 0 || first + count + deleteAfter > size)
                    return false;
                long next = GTA.Offsets.Editor.Props.NEXT;
                long data = GTA.Offsets.Editor.Props.loc + first * next;
                long last = GTA.Offsets.Editor.Props.loc + (first + Math.Max(count, 1)) * next - 1;
                // Globals live in pages of 2^18 slots; the payload walks the block with one pointer.
                if (GTA.Offsets.Editor.Props.loc == 0 || (data >> 18) != (last >> 18))
                    return false;
                WriteInt(page + ModeAt, SyncBlock);
                WriteInt(page + BlockHandlesAt, LocalAddress(handles + first));
                WriteInt(page + BlockDataAt, new Global(data).GetAddress());
                WriteInt(page + BlockStrideAt, next);
                WriteInt(page + BlockCountAt, count);
                WriteInt(page + BlockDeleteAt, deleteAfter);
                WriteInt(page + BlockRotAt, GTA.Offsets.Editor.Props.vrot - GTA.Offsets.Editor.Props.loc);
                WriteInt(page + BlockModelAt, GTA.Offsets.Editor.Props.model - GTA.Offsets.Editor.Props.loc);
                // A model the game still streams keeps the request open for a few frames.
                return await Request(page, 120).ConfigureAwait(true) == 2;
            }
            finally
            {
                Gate.Release();
            }
        }

        /// <summary>
        /// Makes the Race Creator draw these checkpoints again with their current settings (size,
        /// type, heading, flags). It keeps per shown marker the position it made it at ("cps" in
        /// [ENTITYHANDLES]) and makes a marker again when that differs from the job data, so
        /// moving the kept position away is enough; no payload. The point before each one is
        /// drawn again too: its arrow points at the next. False when the list is not known.
        /// </summary>
        public static bool RefreshCheckpoints(ICollection<int> indices)
        {
            string script = GTA.CurrentCreatorName();
            if (script != "fm_race_creator" || indices.Count == 0 || !MainWindow.m.IsProcOpen)
                return false;
            var a = Array(script, "cps");
            if (a == null || ReadLocal(a.Value.First - 1) != a.Value.Size)
                return false;
            var hit = new HashSet<int>();
            foreach (int i in indices)
            {
                hit.Add(i);
                hit.Add(i + 1);
            }
            for (long e = 0; e < a.Value.Size; e++)
            {
                long entry = a.Value.First + e * a.Value.EntryStride;
                // Entry: checkpoint index, secondary, ?, marker, position made at (f_4).
                if (hit.Contains(ReadLocal(entry) + 1) && ReadLocal(entry + 3) != 0)
                {
                    long at = LocalAddress(entry + 4);
                    if (at != 0)
                        Write(at, BitConverter.GetBytes(99999f));
                }
            }
            return true;
        }
    }
}
