using System;
using System.Collections.Generic;

namespace Xenvious
{
    /// <summary>
    /// Where entities, doors and zones use a Mission Flag, as the Mission Creator's own flag
    /// overview lists it (public_mission_creator 1.73 func_4930 / func_4931, labels PMC_PRF_*).
    /// Single fields hold one flag (0-based, -1 = none, the default of every key: asspreq, clnval,
    /// pdHbP, pdPrOC, pdPrDK ...); a clean-up field counts only while its
    /// type (the field before) is 4 "on Mission Flag"; zones hold a bitset like the rules.
    /// Not here: interactables, cutscenes and gang settings.
    /// </summary>
    public static class MissionFlagUses
    {
        public sealed class Use
        {
            public string Entity;   // "Actor 3"
            public string Label;    // PMC_PRF_* label (the creator's overview line)
            public string Key;      // translation key of what the entity does with the flag
            public string Fallback;
            public bool Sets;       // the entity switches the flag on (on completion or death)
        }

        private sealed class Source
        {
            public string Kind, KindFallback, Label, Fallback;
            public Func<long> Field, Stride, Number;
            public bool CleanUp;    // the value is at Field + 1, used while Field is 4
            public bool Bitset;
        }

        private static readonly Source[] Sources =
        {
            Ped("PMC_PRF_SPWNPED", "spawns on it", () => GTA.Offsets.Editor.mf_ped_spawn),
            Ped("PMC_PRF_PED_CLN", "cleans up on it", () => GTA.Offsets.Editor.mf_ped_cln, cleanUp: true),
            Ped("PMC_PRF_PED_HBR", "health bar", () => GTA.Offsets.Editor.mf_ped_hbr),
            Ped("PMC_PRF_PEDO", "sets it when done", () => GTA.Offsets.Editor.mf_ped_o),
            Ped("PMC_PRF_PEDD", "sets it when killed", () => GTA.Offsets.Editor.mf_ped_d),
            Veh("PMC_PRF_SPWNVEH", "spawns on it", () => GTA.Offsets.Editor.mf_veh_spawn),
            Veh("PMC_PRF_VEH_CLN", "cleans up on it", () => GTA.Offsets.Editor.mf_veh_cln, cleanUp: true),
            Veh("PMC_PRF_VEH_HBR", "health bar", () => GTA.Offsets.Editor.mf_veh_hbr),
            Veh("PMC_PRF_VEHDRS", "doors", () => GTA.Offsets.Editor.mf_veh_drs),
            Veh("PMC_PRF_VEHO", "sets it when done", () => GTA.Offsets.Editor.mf_veh_o),
            Veh("PMC_PRF_VEHD", "sets it when destroyed", () => GTA.Offsets.Editor.mf_veh_d),
            Obj("PMC_PRF_SPWNOBJ", "spawns on it", () => GTA.Offsets.Editor.mf_obj_spawn),
            Obj("PMC_PRF_OBJ_CLN", "cleans up on it", () => GTA.Offsets.Editor.mf_obj_cln, cleanUp: true),
            Obj("PMC_PRF_OBJ_HBR", "health bar", () => GTA.Offsets.Editor.mf_obj_hbr),
            Obj("PMC_PRF_OBJ_DRS", "CCTV", () => GTA.Offsets.Editor.mf_obj_drs),
            Obj("PMC_PRF_OBJMF", "uses it", () => GTA.Offsets.Editor.mf_obj_mf),
            Obj("PMC_PRF_OBJO", "sets it when done", () => GTA.Offsets.Editor.mf_obj_o),
            Obj("PMC_PRF_OBJD", "sets it when destroyed", () => GTA.Offsets.Editor.mf_obj_d),
            new Source { Kind = "rl_k_prop", KindFallback = "Prop", Label = "PMC_PRF_SPWNPRO", Fallback = "spawns on it", Field = () => GTA.Offsets.Editor.mf_prop_spawn, Stride = () => GTA.Offsets.Editor.Props.NEXT, Number = () => GTA.Offsets.Editor.Props.number },
            new Source { Kind = "rl_k_prop", KindFallback = "Prop", Label = "PMC_PRF_SLIDPRQ", Fallback = "slides on it", Field = () => GTA.Offsets.Editor.mf_prop_slide, Stride = () => GTA.Offsets.Editor.Props.NEXT, Number = () => GTA.Offsets.Editor.Props.number },
            new Source { Kind = "rl_k_dprop", KindFallback = "Dynamic prop", Label = "PMC_PRF_SPWNDYN", Fallback = "spawns on it", Field = () => GTA.Offsets.Editor.mf_dyn_spawn, Stride = () => GTA.Offsets.Editor.DProps.NEXT, Number = () => GTA.Offsets.Editor.DProps.number },
            new Source { Kind = "rl_k_dprop", KindFallback = "Dynamic prop", Label = "PMC_PRF_DYN_CLN", Fallback = "cleans up on it", Field = () => GTA.Offsets.Editor.mf_dyn_cln, Stride = () => GTA.Offsets.Editor.DProps.NEXT, Number = () => GTA.Offsets.Editor.DProps.number, CleanUp = true },
            new Source { Kind = "rl_k_weap", KindFallback = "Pickup", Label = "PMC_PRF_SPWNWEP", Fallback = "spawns on it", Field = () => GTA.Offsets.Editor.mf_weap_spawn, Stride = () => GTA.Offsets.Editor.Weapon.NEXT, Number = () => GTA.Offsets.Editor.Weapon.number },
            new Source { Kind = "rl_k_door", KindFallback = "Door", Label = "PMC_PRF_DRMF", Fallback = "opens on it", Field = () => GTA.Offsets.Editor.mf_door, Stride = () => GTA.Offsets.Editor.Doors.NEXT, Number = () => GTA.Offsets.Editor.Doors.number },
            new Source { Kind = "rl_k_zone", KindFallback = "Zone", Label = "PMC_PRF_ZNE_AMF", Fallback = "turns on with it", Field = () => GTA.Offsets.Editor.mf_zone_on, Stride = () => GTA.Offsets.Editor.Zones.NEXT, Number = () => GTA.Offsets.Editor.Zones.number, Bitset = true },
            new Source { Kind = "rl_k_zone", KindFallback = "Zone", Label = "PMC_PRF_ZNE_DMF", Fallback = "turns off with it", Field = () => GTA.Offsets.Editor.mf_zone_off, Stride = () => GTA.Offsets.Editor.Zones.NEXT, Number = () => GTA.Offsets.Editor.Zones.number, Bitset = true },
        };

        private static Source Ped(string label, string fallback, Func<long> field, bool cleanUp = false) =>
            new Source { Kind = "eo_k_ped", KindFallback = "Actor", Label = label, Fallback = fallback, Field = field, Stride = () => GTA.Offsets.Editor.Actor.NEXT, Number = () => GTA.Offsets.Editor.Actor.number, CleanUp = cleanUp };

        private static Source Veh(string label, string fallback, Func<long> field, bool cleanUp = false) =>
            new Source { Kind = "eo_k_veh", KindFallback = "Vehicle", Label = label, Fallback = fallback, Field = field, Stride = () => GTA.Offsets.Editor.Vehicle.NEXT, Number = () => GTA.Offsets.Editor.Vehicle.number, CleanUp = cleanUp };

        private static Source Obj(string label, string fallback, Func<long> field, bool cleanUp = false) =>
            new Source { Kind = "eo_k_obj", KindFallback = "Object", Label = label, Fallback = fallback, Field = field, Stride = () => GTA.Offsets.Editor.Objects.NEXT, Number = () => GTA.Offsets.Editor.Objects.number, CleanUp = cleanUp };

        private static string Slug(string text) => text.Replace(' ', '_');

        /// <summary>flag (0-based) -> where it is used. Empty when the game is not open.</summary>
        public static Dictionary<int, List<Use>> All(Func<string, string, string> translate)
        {
            var result = new Dictionary<int, List<Use>>();
            if (MainWindow.m == null || !MainWindow.m.IsProcOpen)
                return result;
            void Add(int flag, Source s, int index)
            {
                if (flag < 0 || flag >= RuleOptions.FlagCount)
                    return;
                if (!result.TryGetValue(flag, out var list))
                    result[flag] = list = new List<Use>();
                list.Add(new Use { Entity = translate(s.Kind, s.KindFallback) + " " + (index + 1), Label = s.Label, Key = "mfu_" + Slug(s.Fallback), Fallback = s.Fallback, Sets = s.Fallback.StartsWith("sets", StringComparison.Ordinal) });
            }
            foreach (var s in Sources)
            {
                long field = s.Field(), stride = s.Stride(), number = s.Number();
                if (field == 0 || stride == 0 || number == 0)
                    continue;
                int count = Math.Max(0, Math.Min(new Global(number).Get<int>(), 300));
                for (int i = 0; i < count; i++)
                {
                    long at = field + i * stride;
                    if (s.Bitset)
                    {
                        for (int w = 0; w < (RuleOptions.FlagCount + 31) / 32; w++)
                        {
                            int v = new Global(at + 1 + w).Get<int>();
                            for (int b = 0; b < 32; b++)
                                if ((v & (1 << b)) != 0)
                                    Add(w * 32 + b, s, i);
                        }
                    }
                    else if (s.CleanUp)
                    {
                        if (new Global(at).Get<int>() == 4)
                            Add(new Global(at + 1).Get<int>(), s, i);
                    }
                    else
                    {
                        Add(new Global(at).Get<int>(), s, i);
                    }
                }
            }
            return result;
        }
    }
}
