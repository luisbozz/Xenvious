using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Xenvious
{
    public enum CreatorType
    {
        fm_lts_creator,
        fm_capture_creator,
        fm_race_creator,
        fm_survival_creator,
        fm_deathmatch_creator
    }

    public enum FunctionCategory
    {
        Root,
        Loop,
        Helper
    }

    public class FunctionTemplate
    {
        public string Name { get; set; }
        public FunctionCategory Category { get; set; }
        public string Template { get; set; }
    }

    public static class CustomCreatorFuncsBuilder
    {
        // Werte pro Creator/Kategorie
        private static readonly Dictionary<CreatorType, Dictionary<string, string>> NativeValues =
            new()
            {
                [CreatorType.fm_lts_creator] = new()
                {
                    ["DOES_ENTITY_EXIST"] = "2C 05 03 DB",
                    ["IS_ENTITY_AN_OBJECT"] = "2C 05 02 F9",
                    ["GET_ENTITY_MODEL"] = "2C 05 04 90",
                    ["GET_HUD_COLOUR"] = "2C 14 00 C8",
                    ["IS_MODEL_VALID"] = "2C 05 01 86",
                    ["GET_MODEL_DIMENSIONS"] = "2C 0C 02 1F"
                },
                [CreatorType.fm_race_creator] = new()
                {
                    ["DOES_ENTITY_EXIST"] = "2C 05 01 13",
                    ["IS_ENTITY_AN_OBJECT"] = "2C 05 06 F7",
                    ["GET_ENTITY_MODEL"] = "2C 05 03 B6",
                    ["GET_HUD_COLOUR"] = "2C 14 00 6F",
                    ["IS_MODEL_VALID"] = "2C 05 03 7F",
                    ["GET_MODEL_DIMENSIONS"] = "2C 0C 02 63"
                },
                [CreatorType.fm_capture_creator] = new()
                {
                    ["DOES_ENTITY_EXIST"] = "2C 05 00 B4",
                    ["IS_ENTITY_AN_OBJECT"] = "2C 05 04 4A",
                    ["GET_ENTITY_MODEL"] = "2C 05 00 46",
                    ["GET_HUD_COLOUR"] = "2C 14 01 34",
                    ["IS_MODEL_VALID"] = "2C 05 02 99",
                    ["GET_MODEL_DIMENSIONS"] = "2C 0C 00 0E"
                }
                // weitere Creator hier…
            };

        private static readonly Dictionary<CreatorType, Dictionary<string, string>> LocalValues =
            new()
            {
                [CreatorType.fm_lts_creator] = new()
                {
                    ["WORKER"] = "4F EC 21",
                    ["WORKER_HOVERED_MODEL"] = "41 0C"
                },
                [CreatorType.fm_race_creator] = new()
                {
                    ["WORKER"] = "4F 55 C5",
                    ["WORKER_HOVERED_MODEL"] = "41 0C",
                    ["PRE"] = "4F 35 A7",
                    ["PRE_MENU"] = "47 10 01",
                    ["PLACEMENT"] = "4F B5 A2",
                    ["LOAD_PLACEMENT_PROP"] = "41 EF",
                    ["STORE_PLACEMENT_PROP"] = "42 EF",
                    ["PLACEMENT_PTEMP"] = "40 F0"
                },
                [CreatorType.fm_capture_creator] = new()
                {
                    ["WORKER"] = "4F 81 20",
                    ["WORKER_HOVERED_MODEL"] = "41 0C"
                }
                // weitere Creator hier…
            };

        // 👉 Neue Function-Werte
        private static readonly Dictionary<CreatorType, Dictionary<string, string>> FunctionValues = new()
        {
            [CreatorType.fm_lts_creator] = new()
            {
                ["DRAW_POLY"] = "5D B9 FE 22",
                ["MIDPOINT"] = "5D AB FB 11",
                ["DRAW_STRING"] = "5D DE FC 22"
            },
            [CreatorType.fm_race_creator] = new()
            {
                ["DRAW_POLY"] = "5D E4 A5 3D",
                ["MIDPOINT"] = "5D 0E A5 39",
                ["DRAW_STRING"] = "5D 2F A4 3D"
            },
            [CreatorType.fm_capture_creator] = new()
            {
                ["DRAW_POLY"] = "5D 96 0A 23",
                ["MIDPOINT"] = "5D CA EF 11",
                ["DRAW_STRING"] = "5D BB 08 23"
            }
            // Für andere Creator analog ergänzen
        };

        // pro Creator-Type
        public static readonly Dictionary<string, FunctionTemplate> FunctionCatalog = 
            new()
            {
                ["root_main"] = new FunctionTemplate
                {
                    Name = "root_main",
                    Category = FunctionCategory.Root,
                    Template = "2D 00 03 00 00 {{ LOOP_POINTERS }} 2E 00 00"
                },
                ["get model dimension"] = new FunctionTemplate
                {
                    Name = "get model dimension",
                    Category = FunctionCategory.Loop,
                    Template = "2D 00 02 00 00 62 6A E0 1B {{ NATIVE:IS_MODEL_VALID }} 06 56 03 00 2E 00 00 62 6A E0 1B 61 6B E0 1B 61 6E E0 1B {{ NATIVE:GET_MODEL_DIMENSIONS }} 2E 00 00"
                },
                ["colclrovr"] = new FunctionTemplate
                {
                    Name = "colclrovr",
                    Category = FunctionCategory.Helper,
                    Template = "2D 02 03 00 00 38 01 70 58 38 00 38 01 71 57 08 00 25 0F 2E 02 01 55 5F 00 38 01 72 57 08 00 25 15 2E 02 01 55 51 00 38 01 73 57 08 00 25 18 2E 02 01 55 43 00 38 01 74 57 05 00 25 12 2E 02 01 55 35 00 38 00 71 57 08 00 25 34 2E 02 01 55 27 00 38 00 72 57 08 00 25 0F 2E 02 01 55 19 00 38 00 73 57 08 00 25 14 2E 02 01 55 0B 00 38 00 74 57 05 00 25 1D 2E 02 01 77 2E 02 01"
                },
                ["draw rec pa"] = new FunctionTemplate
                {
                    Name = "draw rec pa",
                    Category = FunctionCategory.Loop,
                    Template = "2D 00 08 00 00 71 39 02 38 02 74 5C C3 00 38 02 38 02 61 00 00 48 64 D5 EB 01 3F 35 01 {{ HELPERFUNC:colclrovr }} 37 04 37 05 37 06 37 07 {{ NATIVE:GET_HUD_COLOUR }} 71 39 03 38 03 25 10 5C 91 00 74 38 03 38 02 61 00 00 48 46 15 0E 49 45 69 46 50 0C 34 24 40 10 32 74 38 03 38 02 61 00 00 48 46 15 0E 49 45 69 46 50 0C 34 24 40 13 32 38 03 38 02 61 00 00 48 46 15 0E 49 45 69 46 50 0C 34 24 41 17 38 04 38 05 38 06 25 62 72 72 {{ FUNC:DRAW_POLY }} 74 74 38 03 38 02 61 00 00 48 46 15 0E 49 45 69 46 50 0C 34 24 40 10 32 74 38 03 38 02 61 00 00 48 46 15 0E 49 45 69 46 50 0C 34 24 40 13 32 {{ FUNC:MIDPOINT }} {{ FUNC:DRAW_STRING }} 38 03 3D 01 39 03 55 68 FF 38 02 3D 01 39 02 55 37 FF 2E 00 00"
                },
                ["draw gang chase"] = new FunctionTemplate
                {
                    Name = "draw gang chase",
                    Category = FunctionCategory.Loop,
                    Template = "2D 00 08 00 00 71 39 02 38 02 74 5C 9A 00 71 39 03 38 03 25 10 5C 87 00 74 38 03 38 02 61 00 00 48 46 15 0E 49 45 69 46 85 1A 34 03 32 74 38 03 38 02 61 00 00 48 46 15 0E 49 45 69 46 B9 1A 34 03 32 38 03 38 02 61 00 00 48 46 15 0E 49 45 69 46 ED 1A 35 01 25 FF 25 00 25 00 25 62 72 72 {{ FUNC:DRAW_POLY }} 72 74 38 03 38 02 61 00 00 48 46 15 0E 49 45 69 46 85 1A 34 03 32 74 38 03 38 02 61 00 00 48 46 15 0E 49 45 69 46 B9 1A 34 03 32 {{ FUNC:MIDPOINT }} {{ FUNC:DRAW_STRING }} 38 03 3D 01 39 03 55 72 FF 38 02 3D 01 39 02 55 60 FF 2E 00 00"
                },
                ["get hovered model"] = new FunctionTemplate
                {
                    Name = "get hovered model",
                    Category = FunctionCategory.Loop,
                    Template = "2D 00 02 00 00 {{ LOCAL:WORKER }} {{ LOCAL:WORKER_HOVERED_MODEL }} {{ NATIVE:DOES_ENTITY_EXIST }} 2A 56 0B 00 {{ LOCAL:WORKER }} {{ LOCAL:WORKER_HOVERED_MODEL }} {{ NATIVE:IS_ENTITY_AN_OBJECT }} 06 1F 56 03 00 2E 00 00 {{ LOCAL:WORKER }} {{ LOCAL:WORKER_HOVERED_MODEL }} {{ NATIVE:GET_ENTITY_MODEL }} 63 69 E0 1B 2E 00 00"
                },
                ["creator refresh"] = new FunctionTemplate
                {
                    Name = "creator refresh",
                    Category = FunctionCategory.Loop,
                    Template = "2D 00 02 00 00 {{ PATTERN:CREATOR_REFRESH }} 2E 00 00"
                }
                //["set f_240[0] to f_239"] = new FunctionTemplate
                //{
                //    Name = "set f_240[0] to f_239",
                //    Category = FunctionCategory.Loop,
                //    Template = "2D 00 02 00 00 {{ LOCAL:PRE }} {{ LOCAL:PRE_MENU }} 43 85 03 08 2A 06 56 0C 00 {{ LOCAL:PLACEMENT }} {{ LOCAL:PLACEMENT_PTEMP }} 35 01 {{ NATIVE:DOES_ENTITY_EXIST }} 1F 56 18 00 {{ LOCAL:PLACEMENT }} {{ LOCAL:LOAD_PLACEMENT_PROP }} {{ NATIVE:DOES_ENTITY_EXIST }} 56 0C 00 {{ LOCAL:PLACEMENT }} {{ LOCAL:PLACEMENT_PTEMP }} 35 01 {{ LOCAL:PLACEMENT }} {{ LOCAL:STORE_PLACEMENT_PROP }} 2E 00 00"
                //}
            };

        public static readonly Dictionary<CreatorType, string> RootStartHex = new()
        {
            [CreatorType.fm_lts_creator] = "5D 1E 90 23",
            [CreatorType.fm_race_creator] = "5D 98 4A 3F",
            [CreatorType.fm_capture_creator] = "5D EF 9B 23"
        };



        public class PatternDefinition
        {
            public string Name { get; set; } = string.Empty;
            public List<CreatorType> Creators { get; set; } = new();
            public string Pattern { get; set; } = string.Empty;
            public int Offset { get; set; }
            public int Length { get; set; }
        }

        public static readonly Dictionary<string, List<PatternDefinition>> PatternValues = new()
        {
            {
                "CREATOR_REFRESH", new List<PatternDefinition>
                {
                    new PatternDefinition
                    {
                        Creators = new List<CreatorType>
                        {
                            CreatorType.fm_lts_creator,
                            CreatorType.fm_capture_creator
                        },
                        Pattern = "71 51 ? ? 4F ? ? 47 ? ? 72 57 0A 00 72 2C ? ? ? 72 2C ? ? ? 72 2C ? ? ? 4F ? ? 25 12",
                        Offset = -7,
                        Length = 4
                    },
                    new PatternDefinition
                    {
                        Creators = new List<CreatorType>
                        {
                            CreatorType.fm_race_creator
                        },
                        Pattern = "2C ? ? ? 06 56 0A 00 72 63 ? ? ?",
                        Offset = -7,
                        Length = 4
                    },
                    new PatternDefinition
                    {
                        Creators = new List<CreatorType>
                        {
                            CreatorType.fm_survival_creator
                        },
                        Pattern = "71 5D ? ? ? 72 2C ? ? ? 72 71",
                        Offset = -7,
                        Length = 4
                    },
                    new PatternDefinition
                    {
                        Creators = new List<CreatorType>
                        {
                            CreatorType.fm_survival_creator
                        },
                        Pattern = "71 51 ? ? 72 2C ? ? ?",
                        Offset = -7,
                        Length = 4
                    }
                }
            }
        };

        private static int ParseStartValue(string hex4Bytes)
        {
            var bytes = hex4Bytes.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);
            // 5D always ignored -> little-endian order (?? abhängig vom Spiel)
            // wir nehmen hier [3]=23, [2]=9B, [1]=EF -> 0x23 9B EF
            return Convert.ToInt32(bytes[3] + bytes[2] + bytes[1], 16);
        }

        private static int HexLength(string hex)
        {
            return hex
                .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Length;
        }

        public static string BuildAllFunctionsAndRoot(CreatorType creator)
        {
            // 1️ Alle Nicht-Root-Funktionen vorab bauen, um Länge zu kennen
            var built = new Dictionary<string, string>();
            foreach (var kv in FunctionCatalog)
            {
                if (kv.Value.Category != FunctionCategory.Root)
                {
                    built[kv.Key] = Build(kv.Value.Template, creator);
                }
            }

            // 2️ Root-Länge ohne Pointer
            string rootTemplate = FunctionCatalog["root_main"].Template;
            int baseRootLength = HexLength(
                Build(rootTemplate.Replace("{{ LOOP_POINTERS }}", ""), creator));

            // Größe eines einzelnen Loop-Check-Blocks (konstant pro Loop)
            // 62 68 E0 1B 25 00 82 56 04 00 5D 00 00 00 = 15
            // 62 68 E0 1B 71 00 82 56 04 00 5D 00 00 00 = 15
            int perLoopBlockTokens = 14;

            int loopCount = FunctionCatalog.Values.Count(f => f.Category == FunctionCategory.Loop);
            int totalRootLength = baseRootLength + loopCount * perLoopBlockTokens;

            int startValue = ParseStartValue(RootStartHex[creator]);

            // 4️ Pointer-Berechnungen
            var loopPointers = new List<string>();
            var helperPointers = new Dictionary<string, string>();
            int offset = totalRootLength;

            foreach (var kv in built)
            {
                var cat = FunctionCatalog[kv.Key].Category;
                int addr = startValue + offset;

                string b1 = (addr & 0xFF).ToString("X2");
                string b2 = ((addr >> 8) & 0xFF).ToString("X2");
                string b3 = ((addr >> 16) & 0xFF).ToString("X2");
                string pointerHex = $"5D {b1} {b2} {b3}";

                if (cat == FunctionCategory.Loop)
                {
                    loopPointers.Add(pointerHex);
                }
                else if (cat == FunctionCategory.Helper)
                {
                    helperPointers[kv.Key] = pointerHex;
                }

                // HELPERFUNC Platzhalter temporär durch 4 Token ersetzen
                string normalized = Regex.Replace(
                    kv.Value,
                    @"\{\{\s*HELPERFUNC:[A-Za-z0-9_]+\s*\}\}",
                    "5D 00 00 00");

                offset += HexLength(normalized);
            }

            // 6 Root-Funktion mit allen Loop-Pointern
            var loopCodeSb = new StringBuilder();
            for (int i = 0; i < loopPointers.Count; i++)
            {
                string ptr = loopPointers[i];
                // 25 {i:X2} gibt z.B. 25 00, 25 01, 25 02 ...
                if (i < 8)
                    loopCodeSb.Append($"62 68 E0 1B 7{i + 1} 00 82 56 04 00 {ptr} ");
                else
                    loopCodeSb.Append($"62 68 E0 1B 25 {i:X2} 82 56 04 00 {ptr} ");
            }
            string loopCode = loopCodeSb.ToString().Trim();

            // 8) Root mit den generierten Loop-Checks bauen
            string rootHex = Build(rootTemplate.Replace("{{ LOOP_POINTERS }}", loopCode), creator);

            var sb = new StringBuilder();
            sb.Append(rootHex);

            foreach (var kv in FunctionCatalog)
            {
                if (kv.Value.Category == FunctionCategory.Root)
                    continue;

                string funcHex = built[kv.Key];

                // HELPERFUNC-Platzhalter direkt beim Schreiben ersetzen
                if (kv.Value.Category == FunctionCategory.Loop)
                {
                    funcHex = Regex.Replace(
                        funcHex,
                        @"\{\{\s*HELPERFUNC:([A-Za-z0-9_]+)\s*\}\}",
                        m =>
                        {
                            var name = m.Groups[1].Value;
                            if (!helperPointers.TryGetValue(name, out var ptr))
                                throw new InvalidOperationException(
                                    $"Helper '{name}' not found for pointer replacement.");
                            return ptr;
                        });
                }

                sb.Append(" " + funcHex);
            }

            return (sb + " 00 00").ToString();
        }

        /// <summary>
        /// Ersetzt {{ NATIVE:… }}, {{ LOCAL:… }} und {{ FUNC:… }} im Template.
        /// </summary>
        public static string Build(string template, CreatorType creator)
        {
            return Regex.Replace(
                template,
                @"\{\{\s*(NATIVE|LOCAL|FUNC|PATTERN):([A-Za-z0-9_]+)\s*\}\}",
                m =>
                {
                    string type = m.Groups[1].Value;
                    string key = m.Groups[2].Value;

                    return type switch
                    {
                        "NATIVE" => GetValue(NativeValues, creator, key, type),
                        "LOCAL" => GetValue(LocalValues, creator, key, type),
                        "FUNC" => GetValue(FunctionValues, creator, key, type),
                        "PATTERN" => GetPatternValue(PatternValues, creator, key),
                        _ => m.Value
                    };
                });
        }

        private static string GetValue(
            Dictionary<CreatorType, Dictionary<string, string>> dict,
            CreatorType creator, string key, string type)
        {
            if (dict.TryGetValue(creator, out var inner) && inner.TryGetValue(key, out var val))
                return val;
            throw new KeyNotFoundException($"Kein {type} '{key}' für {creator}");
        }

        private static string GetPatternValue(
            Dictionary<string, List<PatternDefinition>> dict,
            CreatorType creator,
            string key)
        {
            if (dict.TryGetValue(key, out var defs))
            {
                var def = defs.FirstOrDefault(d => d.Creators.Contains(creator));
                if (def == null)
                    throw new KeyNotFoundException($"Kein PATTERN '{key}' für {creator}");

                // scrProgram Pointer holen
                ulong scrProgramPtr = ScrProgramScanner.GetScrProgramByName(creator.ToString());
                if (scrProgramPtr == 0)
                    return null;

                // Pattern-Scan durchführen
                var foundAddrs = ScrProgramScanner.ScanScrProgramForPattern(scrProgramPtr, def.Pattern);
                if (foundAddrs == null || foundAddrs.Count == 0)
                    return null;

                var bytes = MainWindow.m.memory((foundAddrs[0] + (ulong)def.Offset).ToString("X")).GetBytes(4);
                return string.Join(" ", bytes.Select(b => b.ToString("X2")));
            }

            throw new KeyNotFoundException($"Kein PATTERN '{key}' definiert");
        }
    }
}
