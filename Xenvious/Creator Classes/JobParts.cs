using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xenvious
{
    /// <summary>
    /// Parts of a job file (props, dynamic props, checkpoints, templates) on their own: read
    /// from a job file, saved as a map backup, and put into another job file, either in place
    /// of what is there or behind it. Works on the JSON of <see cref="JobDatafile"/>, so a
    /// backup fits both editions and every build.
    ///
    /// The creators keep a list as parallel arrays with a count ("no"): loc[i], model[i],
    /// prpclr[i] ... belong to entry i. Appending adds to every array and raises the count, so
    /// the entries already in the job keep their numbers and whatever refers to them stays
    /// right. The loaders read most arrays only when entry i has the right type and take a
    /// default otherwise, but some without that check; a short array is filled up with the
    /// loader's defaults so every array reaches the count.
    /// </summary>
    public static class JobParts
    {
        public const string FileFormat = "xenvious-parts";
        public const string FileExtension = ".json";

        public enum Mode { Replace, Append }

        public sealed class Part
        {
            public string Key;          // in the backup file and the UI
            public string Section;      // the dict under "mission"
            public string CountKey;
            /// <summary>Most entries the creator loads or lets you place.</summary>
            public int Limit;
            public Func<string, bool> AppliesTo;
        }

        // Limits from the creator scripts: the loaders cut props at 200, the creators place up
        // to 32 dynamic props and 10 templates, and stop placing checkpoints at 101 (the race
        // loader does not cut "chp", so more would run past the array).
        public static readonly Part[] All =
        {
            new Part { Key = "prop", Section = "prop", CountKey = "no", Limit = 200, AppliesTo = c => true },
            new Part { Key = "dprop", Section = "dprop", CountKey = "no", Limit = 32, AppliesTo = c => true },
            new Part { Key = "cp", Section = "race", CountKey = "chp", Limit = 101, AppliesTo = c => c == "fm_race_creator" },
            new Part { Key = "tpl", Section = "ptemp", CountKey = "no", Limit = 10, AppliesTo = c => true },
        };

        public static Part Get(string key) => All.First(p => p.Key == key);

        // Arrays in "race" that are race settings, not one entry per checkpoint: vehicle classes
        // and the vehicle lists per class (adlc, adlc2, ...). Besides these, an array only counts
        // as checkpoints when it has one entry per checkpoint.
        private static readonly Regex RaceSettingArray = new Regex(@"^(aveh|vta|trfmvm|adlc\d*)$", RegexOptions.IgnoreCase);

        // The list of custom prop models; not one entry per prop. The capture loader writes it
        // into an array of 31.
        private const string CustomModels = "cusprpMn";
        private const int CustomModelsLimit = 31;

        // A template's own prop lists: pto0, ptr0, ptm0, ptc0 belong to template 0.
        private static readonly Regex TemplateKey = new Regex(@"^(pto|ptr|ptm|ptc)(\d+)$", RegexOptions.IgnoreCase);

        // ---- reading ------------------------------------------------------------------------

        private static JObject SectionOf(JObject job, Part part) => job?["mission"]?[part.Section] as JObject;

        private static int CountIn(JObject section, Part part) =>
            section?[part.CountKey]?.Type == JTokenType.Integer ? Math.Max(0, (int)section[part.CountKey]) : 0;

        /// <summary>Entries of the part in a job file.</summary>
        public static int Count(JObject job, string key)
        {
            var part = Get(key);
            return CountIn(SectionOf(job, part), part);
        }

        private static bool IsEntryArray(Part part, string name, JObject section)
        {
            if (part.Key == "cp")
                return !RaceSettingArray.IsMatch(name)
                    && section?[name] is JArray values && values.Count > 0 && values.Count == CountIn(section, part);
            if (part.Key == "tpl")
                return !TemplateKey.IsMatch(name);
            return !string.Equals(name, CustomModels, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>The part out of a job file, or null when the job has none.</summary>
        public static JObject Extract(JObject job, string key)
        {
            var part = Get(key);
            var section = SectionOf(job, part);
            if (section == null || CountIn(section, part) == 0)
                return null;
            if (part.Key != "cp")
                return (JObject)section.DeepClone();
            // Checkpoints share "race" with the race's settings; only the count and the
            // checkpoint arrays go with them.
            var result = new JObject { [part.CountKey] = section[part.CountKey].DeepClone() };
            foreach (var property in section.Properties().Where(p => p.Value is JArray && IsEntryArray(part, p.Name, section)))
                result[property.Name] = property.Value.DeepClone();
            return result;
        }

        // ---- merging ------------------------------------------------------------------------

        /// <summary>
        /// Puts the part into the job file: in place of the job's own entries, or behind them.
        /// <paramref name="resetLinks"/> clears what the new entries know about teams, rules and
        /// objects, which belong to the job they came from.
        /// </summary>
        public static void Merge(JObject job, string key, JObject source, Mode mode, bool resetLinks)
        {
            var part = Get(key);
            if (!(job["mission"] is JObject mission) || source == null)
                return;
            if (!(mission[part.Section] is JObject target))
            {
                target = new JObject { [part.CountKey] = 0 };
                mission[part.Section] = target;
            }
            int had = mode == Mode.Append ? CountIn(target, part) : 0;
            int adds = CountIn(source, part);

            if (mode == Mode.Replace)
            {
                if (part.Key == "cp")
                {
                    foreach (var name in target.Properties().Where(p => p.Value is JArray && IsEntryArray(part, p.Name, target)).Select(p => p.Name).ToList())
                        target.Remove(name);
                    // Backups saved before the length check may hold race settings too.
                    foreach (var property in source.Properties().Where(p => p.Name == part.CountKey || IsEntryArray(part, p.Name, source)))
                        target[property.Name] = property.Value.DeepClone();
                }
                else
                {
                    target = (JObject)source.DeepClone();
                    mission[part.Section] = target;
                }
            }
            else
            {
                AppendEntries(part, target, source, had, adds);
                if (part.Key == "tpl")
                    AppendTemplates(target, source, had, adds);
                if (part.Key == "prop")
                    MergeCustomModels(target, source);
            }

            if (resetLinks)
                ResetLinks(part, target, had, had + adds);
        }

        private static void AppendEntries(Part part, JObject target, JObject source, int had, int adds)
        {
            var names = target.Properties().Select(p => p.Name)
                .Concat(source.Properties().Select(p => p.Name))
                .Distinct(StringComparer.Ordinal)
                .Where(n => (target[n] is JArray || source[n] is JArray) && (IsEntryArray(part, n, target) || IsEntryArray(part, n, source)))
                .ToList();
            foreach (string name in names)
            {
                var mine = target[name] as JArray ?? new JArray();
                var theirs = source[name] as JArray ?? new JArray();
                if (mine.Count == 0 && theirs.Count == 0)
                    continue;
                JToken sample = theirs.FirstOrDefault(t => t.Type != JTokenType.Null) ?? mine.FirstOrDefault(t => t.Type != JTokenType.Null);
                var merged = new JArray();
                for (int i = 0; i < had; i++)
                    merged.Add(i < mine.Count ? mine[i].DeepClone() : Default(part, name, sample));
                for (int i = 0; i < adds; i++)
                    merged.Add(i < theirs.Count ? theirs[i].DeepClone() : Default(part, name, sample));
                target[name] = merged;
            }
            target[part.CountKey] = had + adds;
        }

        /// <summary>Templates of the backup get the numbers after the job's own.</summary>
        private static void AppendTemplates(JObject target, JObject source, int had, int adds)
        {
            foreach (var property in source.Properties())
            {
                var match = TemplateKey.Match(property.Name);
                if (!match.Success || !int.TryParse(match.Groups[2].Value, out int index) || index >= adds)
                    continue;
                target[match.Groups[1].Value + (had + index)] = property.Value.DeepClone();
            }
        }

        private static void MergeCustomModels(JObject target, JObject source)
        {
            if (!(source[CustomModels] is JArray theirs) || theirs.Count == 0)
                return;
            var mine = target[CustomModels] as JArray ?? new JArray();
            var known = new HashSet<long>(mine.Where(t => t.Type == JTokenType.Integer).Select(t => (long)t));
            foreach (var model in theirs.Where(t => t.Type == JTokenType.Integer))
            {
                if (mine.Count >= CustomModelsLimit)
                    break;
                if (known.Add((long)model))
                    mine.Add(model.DeepClone());
            }
            target[CustomModels] = mine;
        }

        // Fields that tie an entry to the job's teams, rules and objects, with the value the
        // loader takes when the field is missing (no link).
        private static readonly Dictionary<string, long> PropLinks = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            ["asst"] = 0, ["asso"] = -1, ["asss"] = 0, ["pasc"] = 0,
            ["asst2"] = 0, ["asso2"] = -1, ["asss2"] = 0, ["pasc2"] = 0,
            ["asst3"] = 0, ["asso3"] = -1, ["asss3"] = 0, ["pasc3"] = 0,
            ["asst4"] = 0, ["asso4"] = -1, ["asss4"] = 0, ["pasc4"] = 0,
            ["alteam"] = 0, ["prpasn"] = 0, ["prpatn"] = 0, ["fwTeam"] = 0,
        };

        private static void ResetLinks(Part part, JObject target, int from, int to)
        {
            if (part.Key != "prop" && part.Key != "dprop")
                return;
            foreach (var link in PropLinks)
            {
                if (!(target[link.Key] is JArray values))
                    continue;
                for (int i = from; i < to && i < values.Count; i++)
                    values[i] = new JValue(link.Value);
            }
        }

        // Defaults of the loaders (capture creator, the same function in every creator) for
        // fields whose default is not 0.
        private static readonly Dictionary<string, JToken> PropDefaults = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase)
        {
            ["prpclc"] = -1, ["prplod"] = -1, ["prpclr"] = 1, ["prpcr"] = -1, ["prpct"] = -1, ["prcra"] = -1,
            ["prptds"] = 0.4, ["prpsba"] = 2, ["updatez"] = -1.0, ["TrTAct"] = -1, ["prpdypil"] = -1, ["prppi"] = -1,
            ["asso"] = -1, ["asso2"] = -1, ["asso3"] = -1, ["asso4"] = -1,
            ["prpsdp0"] = -1, ["prpsdp1"] = -1, ["prpsdp2"] = -1, ["prpsdp3"] = -1, ["prpsdp4"] = -1,
            ["prpscr0_"] = -1, ["prpscr1_"] = -1, ["prpscr2_"] = -1, ["prpscr3_"] = -1,
        };

        private static readonly Dictionary<string, JToken> DPropDefaults = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase)
        {
            ["asso"] = -1, ["asso2"] = -1, ["asso3"] = -1, ["asso4"] = -1,
            ["prpcr"] = -1, ["prpct"] = -1, ["prcra"] = -1, ["prpdclr"] = 1, ["dptrpx"] = 2.0, ["dptrRS"] = -1, ["dynrpil"] = -1,
        };

        /// <summary>A value for an entry the array is missing, of the type the array holds.</summary>
        private static JToken Default(Part part, string name, JToken sample)
        {
            var table = part.Key == "prop" ? PropDefaults : part.Key == "dprop" ? DPropDefaults : null;
            if (table != null && table.TryGetValue(name, out JToken known))
                return known.DeepClone();
            switch (sample?.Type)
            {
                case JTokenType.Float: return new JValue(0.0);
                case JTokenType.Boolean: return new JValue(false);
                case JTokenType.String: return new JValue("");
                case JTokenType.Object: return new JObject { ["x"] = 0.0, ["y"] = 0.0, ["z"] = 0.0 };
                default: return new JValue(0);
            }
        }

        // ---- backup files -------------------------------------------------------------------

        /// <summary>A map backup: the parts of one job and where they came from.</summary>
        public static JObject CreateFile(JObject job, string name, string creator)
        {
            var parts = new JObject();
            var counts = new JObject();
            foreach (var part in All.Where(p => p.AppliesTo(creator)))
            {
                var data = Extract(job, part.Key);
                if (data == null)
                    continue;
                parts[part.Key] = data;
                counts[part.Key] = CountIn(data, part);
            }
            return new JObject
            {
                ["format"] = FileFormat,
                ["version"] = 1,
                ["name"] = name,
                ["creator"] = creator,
                ["edition"] = GameVariant.IsEnhanced ? "Enhanced" : "Legacy",
                ["build"] = GTA.GameVersion(),
                ["savedAt"] = DateTime.Now.ToString("o"),
                ["counts"] = counts,
                ["parts"] = parts,
            };
        }

        /// <summary>Reads a map backup; null when the file is none.</summary>
        public static JObject ParseFile(string text)
        {
            var settings = new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None,
                FloatParseHandling = FloatParseHandling.Double
            };
            var file = JsonConvert.DeserializeObject<JToken>(text, settings) as JObject;
            return (string)file?["format"] == FileFormat && file["parts"] is JObject ? file : null;
        }

        /// <summary>The part out of a map backup, or null.</summary>
        public static JObject PartOf(JObject file, string key) => file?["parts"]?[key] as JObject;

        public static void Save(JObject file, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, file.ToString(Formatting.Indented));
        }
    }
}
