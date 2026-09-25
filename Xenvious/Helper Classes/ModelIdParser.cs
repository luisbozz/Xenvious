using System;
using System.Globalization;
using System.Numerics;

namespace Xenvious
{
    /// <summary>
    /// Erkennt und parst Modell-IDs in folgenden Formen:
    /// - Dezimal SIGNED (int)          → z.B. "-1", "12345"
    /// - Dezimal UNSIGNED (uint)       → z.B. "4294967295" (→ unchecked int = -1)
    /// - Hex (1..8 Stellen)            → "DEADBEEF", "0xDEADBEEF", "#DEAD_BEEF", "$DEADBEEF", "DEAD_BEEFh"
    /// - Native-Name (JOAAT)           → z.B. "prop_container_01", "joaat(prop_container_01)", "hash(prop...)"
    ///
    /// Ausgabe ist immer die 32-bit-ID als uint (0..0xFFFFFFFF).
    /// Für APIs, die int erwarten, nutze unchecked((int)id).
    /// </summary>
    public static class ModelIdParser
    {
        public enum ModelIdKind { Empty, IntDec, UIntDec, Hex, Native, Invalid }

        /// <summary>
        /// Versucht, eine Eingabe als Modell-ID zu interpretieren.
        /// </summary>
        public static bool TryParseModelId(string? input, out uint id, out ModelIdKind kind)
        {
            id = 0;
            kind = ModelIdKind.Invalid;

            if (string.IsNullOrWhiteSpace(input))
            {
                kind = ModelIdKind.Empty;
                return false;
            }

            // Normalize: trim, Quotes raus, underscores entfernen (für Hex-Varianten)
            var raw = input.Trim().Trim('\"', '\'');
            if (raw.Length == 0) { kind = ModelIdKind.Empty; return false; }

            // joaat(...) / hash(...) Helper
            if (TryParseJoaatCall(raw, out var joaatFromCall))
            {
                id = joaatFromCall;
                kind = ModelIdKind.Native;
                return true;
            }

            // Hex-Pfad?
            if (LooksLikeHex(raw))
            {
                if (TryParseHex32(raw, out id))
                {
                    kind = ModelIdKind.Hex;
                    return true;
                }
                return false;
            }

            // Dezimal?
            if (LooksLikeDecimal(raw))
            {
                if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var asLong))
                {
                    // signed int?
                    if (asLong >= int.MinValue && asLong <= int.MaxValue)
                    {
                        kind = ModelIdKind.IntDec;
                        id = unchecked((uint)(int)asLong);
                        return true;
                    }

                    // unsigned uint Range (0 .. 4294967295)
                    if (asLong >= 0 && asLong <= uint.MaxValue)
                    {
                        kind = ModelIdKind.UIntDec;
                        id = (uint)asLong;
                        return true;
                    }
                }
                return false;
            }

            // Alles andere → Native-Name → JOAAT
            id = Joaat(raw);
            kind = ModelIdKind.Native;
            return true;
        }

        /// <summary>
        /// Praktischer Wrapper, der direkt ein int liefert (bit-identisch zu uint via unchecked Cast).
        /// </summary>
        public static bool TryParseModelIdInt(string? input, out int modelId, out ModelIdKind kind)
        {
            if (TryParseModelId(input, out var u, out kind))
            {
                modelId = unchecked((int)u);
                return true;
            }
            modelId = 0;
            return false;
        }

        // ---------------- Internals ----------------

        private static bool TryParseJoaatCall(string raw, out uint id)
        {
            id = 0;
            // Formen: joaat(name), hash(name) – tolerant bzgl. Spaces
            // Kleinschreibung auf invariant reduzieren
            var s = raw.Trim();
            var lower = s.ToLowerInvariant();
            if ((lower.StartsWith("joaat(") || lower.StartsWith("hash(")) && lower.EndsWith(")"))
            {
                var inner = s.Substring(s.IndexOf('(') + 1, s.Length - s.IndexOf('(') - 2).Trim();
                if (inner.Length == 0) return false;
                id = Joaat(inner);
                return true;
            }
            return false;
        }

        private static bool LooksLikeDecimal(string s)
        {
            // erlaubt: +/-, Ziffern; keine Punkte, kein 'e' (keine Exponenten)
            // Schnellpfad: erstes Zeichen +/- oder Ziffer
            var c0 = s[0];
            if (!(c0 == '+' || c0 == '-' || (c0 >= '0' && c0 <= '9'))) return false;

            for (int i = 1; i < s.Length; i++)
            {
                var c = s[i];
                if (!(c >= '0' && c <= '9'))
                    return false;
            }
            return true;
        }

        private static bool LooksLikeHex(string s)
        {
            // Erlaubte Präfixe/Suffixe:
            //  - 0xFFFF, 0XFFFF
            //  - #FFFF, $FFFF
            //  - FFFFh / FFFFH
            //  - mit Unterstrichen: DEAD_BEEF
            //  - reine Hex ohne Präfix (wenn mind. 1 Buchstabe a-f/A-F enthalten ist oder Länge<=8 Ziffern gemischt)

            var trimmed = s.Trim();

            // suffix h?
            bool suffixH = false;
            if (trimmed.EndsWith("h", StringComparison.OrdinalIgnoreCase))
            {
                suffixH = true;
                trimmed = trimmed.Substring(0, trimmed.Length - 1);
            }

            // Präfixe
            if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("#") ||
                trimmed.StartsWith("$"))
            {
                trimmed = trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? trimmed.Substring(2) : trimmed.Substring(1);
            }

            // underscores entfernen
            trimmed = trimmed.Replace("_", "");

            if (trimmed.Length == 0) return false;

            // Check: max 8 Stellen (32-bit). Wir erlauben >8, nehmen aber die letzten 8 als LSB (tolerant).
            // Erkennung: nur 0-9 a-f A-F
            int hexCount = 0;
            foreach (var ch in trimmed)
            {
                bool isHex = (ch >= '0' && ch <= '9') ||
                             (ch >= 'a' && ch <= 'f') ||
                             (ch >= 'A' && ch <= 'F');
                if (!isHex) return false;
                hexCount++;
            }

            if (hexCount == 0) return false;

            // Wenn suffixH gesetzt war → sicher Hex
            if (suffixH) return true;

            // Kein Dezimal-only (z.B. "12345678") als Hex zurückgeben, außer mit Präfix/Suffix.
            // Heuristik: Wenn nur Ziffern und KEIN Präfix/Suffix, behandeln wir es als Dezimal → false.
            bool hasAlpha = false;
            foreach (var ch in trimmed)
            {
                if ((ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F')) { hasAlpha = true; break; }
            }
            if (!hasAlpha && !suffixH && !s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && !s.StartsWith("#") && !s.StartsWith("$"))
                return false;

            return true;
        }

        private static bool TryParseHex32(string s, out uint value)
        {
            value = 0;

            var trimmed = s.Trim();
            bool hadSuffix = false;

            if (trimmed.EndsWith("h", StringComparison.OrdinalIgnoreCase))
            {
                hadSuffix = true;
                trimmed = trimmed.Substring(0, trimmed.Length - 1);
            }

            if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                trimmed = trimmed.Substring(2);
            else if (trimmed.StartsWith("#") || trimmed.StartsWith("$"))
                trimmed = trimmed.Substring(1);

            trimmed = trimmed.Replace("_", "");
            if (trimmed.Length == 0) return false;

            // Bei >8 Stellen: least significant 8 nehmen (tolerant)
            if (trimmed.Length > 8)
                trimmed = trimmed.Substring(trimmed.Length - 8);

            // Jetzt sicher echte Hex-Zeichen?
            for (int i = 0; i < trimmed.Length; i++)
            {
                var c = trimmed[i];
                bool isHex = (c >= '0' && c <= '9') ||
                             (c >= 'a' && c <= 'f') ||
                             (c >= 'A' && c <= 'F');
                if (!isHex) return false;
            }

            if (uint.TryParse(trimmed, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
                return true;

            return false;
        }

        /// <summary>
        /// Jenkins one-at-a-time hash (JOAAT), lowercased UTF8 – wie bei GTA.
        /// </summary>
        public static uint Joaat(string native)
        {
            if (string.IsNullOrEmpty(native)) return 0u;

            var lower = native.ToLowerInvariant();
            uint hash = 0u;

            // UTF8 bytes iterieren
            var bytes = System.Text.Encoding.UTF8.GetBytes(lower);
            for (int i = 0; i < bytes.Length; i++)
            {
                unchecked
                {
                    hash += bytes[i];
                    hash += (hash << 10);
                    hash ^= (hash >> 6);
                }
            }
            unchecked
            {
                hash += (hash << 3);
                hash ^= (hash >> 11);
                hash += (hash << 15);
            }
            return hash;
        }

        public static string ToHex(uint value, bool withPrefix = false)
            => withPrefix ? $"0x{value:X8}" : $"{value:X8}";
    }
}
