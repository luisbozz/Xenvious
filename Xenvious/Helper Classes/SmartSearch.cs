using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Xenvious
{
    /// <summary>
    /// The side list search's matching: every word of the query has to be found somewhere in the
    /// hit (its title, the page path or its keywords), in any order. A word counts when a word of
    /// the text starts with it, contains it, or is it with one typo (two from 8 letters). Case,
    /// accents and umlauts do not matter ("gruen" finds "Grün"). The score ranks the hits: title
    /// before path, the whole query in the title first, word starts before inner matches.
    /// </summary>
    public static class SmartSearch
    {
        public static string Normalize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            var sb = new StringBuilder(text.Length);
            foreach (char c in text.ToLowerInvariant())
            {
                switch (c)
                {
                    case 'ä': sb.Append("ae"); continue;
                    case 'ö': sb.Append("oe"); continue;
                    case 'ü': sb.Append("ue"); continue;
                    case 'ß': sb.Append("ss"); continue;
                }
                string d = c.ToString().Normalize(NormalizationForm.FormD);
                foreach (char x in d)
                    if (CharUnicodeInfo.GetUnicodeCategory(x) != UnicodeCategory.NonSpacingMark)
                        sb.Append(char.IsLetterOrDigit(x) ? x : ' ');
            }
            return sb.ToString();
        }

        public static string[] Words(string text) =>
            Normalize(text).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>0: no hit. Higher is better.</summary>
        public static int Score(string query, string title, string path = null, string keywords = null)
        {
            var q = Words(query);
            if (q.Length == 0)
                return 0;
            var titleWords = Words(title);
            var otherWords = Words((path ?? "") + " " + (keywords ?? ""));
            int score = 0;
            foreach (string w in q)
            {
                int inTitle = Best(w, titleWords);
                int inOther = Best(w, otherWords);
                if (inTitle == 0 && inOther == 0)
                    return 0;
                score += Math.Max(inTitle * 3, inOther);
            }
            string nt = Normalize(title), nq = string.Join(" ", q);
            if (nt.Contains(nq))
                score += 40;
            if (nt.StartsWith(nq, StringComparison.Ordinal))
                score += 20;
            return score;
        }

        // How well one query word matches the best word of a text: 10 equal, 8 start, 5 inside, 3 typo.
        private static int Best(string word, string[] words)
        {
            int best = 0;
            foreach (string t in words)
            {
                int s = t == word ? 10
                    : t.StartsWith(word, StringComparison.Ordinal) ? 8
                    : word.Length >= 3 && t.Contains(word) ? 5
                    : word.Length >= 4 && Typos(word, t) <= (word.Length >= 8 ? 2 : 1) ? 3
                    : 0;
                if (s > best)
                    best = s;
            }
            return best;
        }

        // Typos between the word and the start of t (so "invinc" with one typo still finds
        // "invincible"): the edit distance against t cut to the word's length, one shorter or longer.
        private static int Typos(string word, string t)
        {
            int best = 99;
            for (int n = word.Length - 1; n <= word.Length + 1; n++)
                if (n > 0 && n <= t.Length)
                    best = Math.Min(best, Distance(word, t.Substring(0, n)));
            return best;
        }

        private static int Distance(string a, string b)
        {
            var prev = new int[b.Length + 1];
            var cur = new int[b.Length + 1];
            for (int j = 0; j <= b.Length; j++)
                prev[j] = j;
            for (int i = 1; i <= a.Length; i++)
            {
                cur[0] = i;
                for (int j = 1; j <= b.Length; j++)
                    cur[j] = Math.Min(Math.Min(prev[j] + 1, cur[j - 1] + 1), prev[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
                var swap = prev; prev = cur; cur = swap;
            }
            return prev[b.Length];
        }

        /// <summary>Spans (start, length) of the title that match query words exactly, for highlighting.</summary>
        public static List<(int Start, int Length)> Spans(string title, string query)
        {
            var spans = new List<(int, int)>();
            if (string.IsNullOrEmpty(title))
                return spans;
            foreach (string w in query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int at = title.IndexOf(w, StringComparison.CurrentCultureIgnoreCase);
                if (at >= 0)
                    spans.Add((at, w.Length));
            }
            spans.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            var merged = new List<(int, int)>();
            foreach (var s in spans)
            {
                if (merged.Count > 0 && s.Item1 <= merged[merged.Count - 1].Item1 + merged[merged.Count - 1].Item2)
                {
                    var last = merged[merged.Count - 1];
                    int end = Math.Max(last.Item1 + last.Item2, s.Item1 + s.Item2);
                    merged[merged.Count - 1] = (last.Item1, end - last.Item1);
                }
                else
                    merged.Add(s);
            }
            return merged;
        }
    }
}
