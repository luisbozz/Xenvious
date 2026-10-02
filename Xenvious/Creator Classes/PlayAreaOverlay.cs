using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Windows.Media;

namespace Xenvious
{
    /// <summary>
    /// Draws the play areas (area 1 and 2) and the gang chase areas of every team and rule in the
    /// creator, labelled with team and rules, through the injected drawer (ScriptDrawer). Works in
    /// every creator with a data page, the Mission Creator included; there it replaces customfuncs
    /// fn3/fn4, which draw only area 1 and only as boxes (WriteScriptFeatureBits leaves their bits
    /// off while the drawer runs). Follows the two switches on the "Show in game" card.
    /// </summary>
    public static class PlayAreaOverlay
    {
        // Same colours as TeamBrush1..4 in App.xaml.
        private static readonly Color[] TeamColours =
        {
            Color.FromRgb(0x2A, 0xA6, 0xB9), Color.FromRgb(0xFF, 0x85, 0x55),
            Color.FromRgb(0x39, 0x66, 0x39), Color.FromRgb(0x9C, 0x6E, 0xAF),
        };
        private static readonly Color GangColour = Color.FromRgb(0xE0, 0x30, 0x30);
        private static readonly Color MixedColour = Color.FromRgb(0xE6, 0xE6, 0xE6);
        private const int Slots = 36;

        // One shape the game data holds for several teams and rules (a play area copied to all
        // rules is drawn once).
        private class Group
        {
            public string Name;
            public ScriptDrawer.Shape Shape;
            // Null: the team's colour, or a neutral one for several teams.
            public Color? Colour;
            public byte Alpha;
            public SortedDictionary<int, SortedSet<int>> Rules = new SortedDictionary<int, SortedSet<int>>();
        }

        /// <summary>Called from the patch loop about twice a second.</summary>
        public static void Tick()
        {
            if (!MainWindow.m.IsProcOpen)
                return;
            string script = GTA.CurrentCreatorName();
            if (script == "")
                return;

            var groups = new Dictionary<string, Group>();
            // The drawer itself runs whenever its payload is in; the script features switch only
            // stops the dispatch bits, so the shapes go with it here.
            if (!ScriptVars.FeaturesOn)
            {
                ScriptDrawer.Show(script, groups.Values.Select(g => g.Shape).ToList());
                return;
            }
            if (VisibilityGroups.IsOn(VisibilityGroups.PlayAreaBit) && PlayAreas.Ready)
            {
                AddPlayAreas(groups, 1, Text("pa_area1", "Play area 1"), 60);
                AddPlayAreas(groups, 2, Text("pa_area2", "Play area 2"), 35);
            }
            if (VisibilityGroups.IsOn(VisibilityGroups.GangChaseBit))
                AddGangAreas(groups);

            var shapes = new List<ScriptDrawer.Shape>();
            foreach (var g in groups.Values)
            {
                var s = g.Shape;
                s.Colour = g.Colour ?? (g.Rules.Count == 1 ? TeamColours[g.Rules.Keys.First()] : MixedColour);
                s.FillAlpha = g.Alpha;
                // ASCII separators: the game font may lack a glyph for "·".
                s.Text = g.Name + " - " + Who(g.Rules);
                shapes.Add(s);
            }
            ScriptDrawer.Show(script, shapes);
        }

        private static void AddPlayAreas(Dictionary<string, Group> groups, int area, string name, byte alpha)
        {
            for (int team = 0; team < PlayAreas.Teams; team++)
            {
                for (int rule = 0; rule < PlayAreas.Rules; rule++)
                {
                    byte[] b = new Global(PlayAreas.Field(area, team, rule, 0)).GetBytes(Slots * 8);
                    if (b == null || b.Length < Slots * 8)
                        return;
                    float F(int slot) => BitConverter.ToSingle(b, slot * 8);
                    Vector3 V(int slot) => new Vector3(F(slot), F(slot + 1), F(slot + 2));
                    ScriptDrawer.Shape shape;
                    string key;
                    if (BitConverter.ToInt32(b, PlayAreas.Type * 8) == 0)
                    {
                        Vector3 centre = V(PlayAreas.Pos);
                        if (centre == Vector3.Zero)
                            continue;
                        shape = ScriptDrawer.Shape.Sphere(centre, F(PlayAreas.Radius));
                        key = Key("s", centre, centre, F(PlayAreas.Radius));
                    }
                    else
                    {
                        shape = ScriptDrawer.Shape.AngledArea(V(PlayAreas.Pos1), V(PlayAreas.Pos2), F(PlayAreas.Width));
                        key = Key("b", V(PlayAreas.Pos1), V(PlayAreas.Pos2), F(PlayAreas.Width));
                    }
                    Add(groups, "pa" + area + key, name, shape, null, alpha, team, rule);
                }
            }
        }

        private static void AddGangAreas(Dictionary<string, Group> groups)
        {
            long v1 = GTA.Offsets.Editor.gbv1, v2 = GTA.Offsets.Editor.gbv2, w = GTA.Offsets.Editor.gbaw;
            if (v1 == 0 || v2 == 0 || w == 0 || GTA.Offsets.Editor.team_NEXT == 0)
                return;
            string name = Text("drawer_gangchase", "Gang chase");
            for (int team = 0; team < PlayAreas.Teams; team++)
            {
                long t = team * GTA.Offsets.Editor.team_NEXT;
                byte[] p1 = new Global(v1 + t).GetBytes(PlayAreas.Rules * 3 * 8);
                byte[] p2 = new Global(v2 + t).GetBytes(PlayAreas.Rules * 3 * 8);
                byte[] width = new Global(w + t).GetBytes(PlayAreas.Rules * 8);
                if (p1 == null || p2 == null || width == null)
                    return;
                for (int rule = 0; rule < PlayAreas.Rules; rule++)
                {
                    Vector3 V(byte[] b) => new Vector3(BitConverter.ToSingle(b, rule * 24), BitConverter.ToSingle(b, rule * 24 + 8), BitConverter.ToSingle(b, rule * 24 + 16));
                    float wd = BitConverter.ToSingle(width, rule * 8);
                    var shape = ScriptDrawer.Shape.AngledArea(V(p1), V(p2), wd);
                    Add(groups, "gb" + Key("b", V(p1), V(p2), wd), name, shape, GangColour, 45, team, rule);
                }
            }
        }

        private static void Add(Dictionary<string, Group> groups, string key, string name, ScriptDrawer.Shape shape, Color? colour, byte alpha, int team, int rule)
        {
            if (shape == null)
                return;
            if (!groups.TryGetValue(key, out var g))
                groups[key] = g = new Group { Name = name, Shape = shape, Colour = colour, Alpha = alpha };
            if (!g.Rules.TryGetValue(team, out var rules))
                g.Rules[team] = rules = new SortedSet<int>();
            rules.Add(rule);
        }

        private static string Key(string kind, Vector3 a, Vector3 b, float w)
            => string.Join(",", new[] { a.X, a.Y, a.Z, b.X, b.Y, b.Z, w }.Select(f => f.ToString("0.00", CultureInfo.InvariantCulture))) + kind;

        // "T1-2 R1-17", or per team when the teams differ: "T1 R1-3/5, T3 R5".
        private static string Who(SortedDictionary<int, SortedSet<int>> rules)
        {
            var parts = new List<string>();
            foreach (var same in rules.GroupBy(kv => string.Join(",", kv.Value)))
            {
                string teams = Ranges(same.Select(kv => kv.Key + 1));
                string ruleList = Ranges(same.First().Value.Select(r => r + 1));
                parts.Add(Format("drawer_team_short", "T{0}", teams) + " " + Format("drawer_rule_short", "R{0}", ruleList));
            }
            return string.Join(", ", parts);
        }

        private static string Ranges(IEnumerable<int> numbers)
        {
            var list = numbers.ToList();
            var parts = new List<string>();
            for (int i = 0; i < list.Count;)
            {
                int j = i;
                while (j + 1 < list.Count && list[j + 1] == list[j] + 1)
                    j++;
                parts.Add(j > i ? list[i] + "-" + list[j] : list[i].ToString(CultureInfo.InvariantCulture));
                i = j + 1;
            }
            return string.Join("/", parts);
        }

        private static string Text(string key, string fallback)
        {
            string t = MainWindow.Instance?.Translation?[key];
            return string.IsNullOrEmpty(t) || t == "missing translation" ? fallback : t;
        }

        private static string Format(string key, string fallback, string value)
        {
            string t = Text(key, fallback);
            return t.Contains("{0}") ? t.Replace("{0}", value) : t + value;
        }
    }
}
