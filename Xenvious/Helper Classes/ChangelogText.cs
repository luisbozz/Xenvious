using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Xenvious
{
    /// <summary>
    /// Turns release notes and CHANGELOG.md (Markdown, as release-please writes it) into
    /// plain lines for the dialog: headings are marked bold, list items get a bullet,
    /// links keep their text, and commit/issue references are dropped.
    /// </summary>
    public static class ChangelogText
    {
        public struct Line
        {
            public string Text;
            public bool Heading;
        }

        private static readonly Regex Link = new Regex(@"\[([^\]]*)\]\([^)]*\)");
        private static readonly Regex CommitRef = new Regex(@"\s*\((?:\[?[0-9a-f]{7,40}\]?|#\d+)(?:,\s*(?:\[?[0-9a-f]{7,40}\]?|#\d+))*\)\s*$", RegexOptions.IgnoreCase);
        private static readonly Regex Emphasis = new Regex(@"(\*\*|__|`)");

        public static List<Line> Parse(string markdown)
        {
            var lines = new List<Line>();
            foreach (string raw in (markdown ?? "").Replace("\r\n", "\n").Split('\n'))
            {
                string text = raw.TrimEnd();
                if (text.StartsWith("<!--"))
                    continue;
                bool heading = false;
                if (text.StartsWith("#"))
                {
                    heading = true;
                    text = text.TrimStart('#').Trim();
                }
                else if (Regex.IsMatch(text, @"^\s*[*-]\s+"))
                {
                    text = "• " + Regex.Replace(text, @"^\s*[*-]\s+", "");
                }

                text = Link.Replace(text, "$1");
                text = CommitRef.Replace(text, "");
                text = Emphasis.Replace(text, "");

                // Keep one empty line between blocks, none at the start.
                if (text.Length == 0 && (lines.Count == 0 || lines[lines.Count - 1].Text.Length == 0))
                    continue;
                lines.Add(new Line { Text = text, Heading = heading });
            }
            while (lines.Count > 0 && lines[lines.Count - 1].Text.Length == 0)
                lines.RemoveAt(lines.Count - 1);
            return lines;
        }

        /// <summary>The CHANGELOG.md built into the exe, or "" when there is none.</summary>
        public static string Embedded()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Xenvious.CHANGELOG.md"))
            {
                if (stream == null)
                    return "";
                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// The part of the changelog for one version: from its "## [x.y.z]" heading to the
        /// next version heading. Empty when the version has no entry.
        /// </summary>
        public static string Section(string changelog, Version version)
        {
            string wanted = $"{version.Major}.{version.Minor}.{version.Build}";
            var result = new List<string>();
            bool inside = false;
            foreach (string line in (changelog ?? "").Replace("\r\n", "\n").Split('\n'))
            {
                var heading = Regex.Match(line, @"^##\s+\[?v?(\d+\.\d+\.\d+)");
                if (heading.Success)
                {
                    if (inside)
                        break;
                    inside = heading.Groups[1].Value == wanted;
                }
                if (inside)
                    result.Add(line);
            }
            return string.Join("\n", result);
        }
    }
}
