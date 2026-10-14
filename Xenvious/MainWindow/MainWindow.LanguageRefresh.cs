using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Xenvious.Translation;

namespace Xenvious
{
    // Part of MainWindow: puts texts that code wrote once (TranslateOr / T at build time) into the new language.
    public partial class MainWindow
    {
        private bool _refreshingThemeNames;

        /// <summary>
        /// XAML bindings on Translation[key] follow a language change by themselves, but many pages are
        /// built in code (TranslateOr / T) and hold plain strings. This walks the window and swaps every
        /// string that is a text of any language for the same key's text in the new one, so a text left
        /// over from an earlier switch is caught too. Texts with numbers or other parts mixed in are
        /// left; the pages that show them write them again on their next refresh.
        /// </summary>
        private void RetranslateCodeTexts(Translator<string, string> from, Translator<string, string> to)
        {
            if (from == null || to == null || ReferenceEquals(from, to))
                return;

            // Every text of every language -> the keys that have it.
            var keysOf = new Dictionary<string, HashSet<string>>();
            foreach (var language in _Language.Languages.Values)
                foreach (var pair in language)
                {
                    if (string.IsNullOrEmpty(pair.Value))
                        continue;
                    if (!keysOf.TryGetValue(pair.Value, out var keys))
                        keysOf[pair.Value] = keys = new HashSet<string>();
                    keys.Add(pair.Key);
                }

            // Text -> new text. Keys of the old language win; a text whose keys still translate
            // differently ("Start" as a button and as a race point) is left alone.
            var map = new Dictionary<string, string>();
            foreach (var entry in keysOf)
            {
                var keys = entry.Value.Where(k => from.TryGetValue(k, out string old) && old == entry.Key).ToList();
                if (keys.Count == 0)
                    keys = entry.Value.ToList();
                var texts = keys.Select(k => to.TryGetValue(k, out string text) ? text : null).Where(t => t != null).Distinct().ToList();
                if (texts.Count == 1 && texts[0] != entry.Key)
                    map[entry.Key] = texts[0];
            }

            int themeIndex = ddcolor?.SelectedIndex ?? -1;
            var seen = new HashSet<DependencyObject>();
            var combos = new List<ComboBox>();
            Retranslate(this, map, seen, combos);

            // A combo box shows a copy of its selected item's content; it only copies it again on a selection change.
            var update = typeof(ComboBox).GetMethod("UpdateSelectionBoxItem", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var combo in combos)
                update?.Invoke(combo, null);

            RefreshThemeNames(themeIndex);
        }

        // Only properties that show text. Others with string values (Tag, SelectedValue, paths) carry logic.
        private static readonly HashSet<string> TextProperties = new HashSet<string>
        {
            "Text", "Content", "Header", "ToolTip", "Title", "Label", "Hint", "Description", "Caption", "Watermark", "Placeholder",
        };

        private static void Retranslate(DependencyObject node, Dictionary<string, string> map, HashSet<DependencyObject> seen, List<ComboBox> combos)
        {
            if (node == null || !seen.Add(node))
                return;
            // What the user typed and what the log shows are data, not UI texts. A text box keeps its
            // watermark in Tag (style Watermark) and may have a tool tip.
            bool input = node is TextBoxBase || node is PasswordBox || (node is ComboBox box && box.IsEditable);
            if (node is Logging.LogView)
                return;
            if (node is ComboBox combo)
                combos.Add(combo);

            var changes = new List<(DependencyProperty, string)>();
            var children = new List<DependencyObject>();
            var local = node.GetLocalValueEnumerator();
            while (local.MoveNext())
            {
                var entry = local.Current;
                // A bound property holds its binding expression here, not a string, and is skipped.
                bool shown;
                if (node is ComboBox && entry.Property == ComboBox.TextProperty)
                    shown = false; // setting it looks for an item with that text and drops the selection
                else if (input)
                    shown = entry.Property == FrameworkElement.TagProperty || entry.Property == FrameworkElement.ToolTipProperty;
                else
                    shown = TextProperties.Contains(entry.Property.Name);
                if (entry.Value is string text && shown && !entry.Property.ReadOnly && map.TryGetValue(text, out string translated))
                    changes.Add((entry.Property, translated));
                else if (entry.Value is DependencyObject child)
                    children.Add(child); // tool tips, context menus, popups
            }
            foreach (var (property, text) in changes)
                node.SetValue(property, text);
            if (input)
            {
                foreach (var child in children)
                    Retranslate(child, map, seen, combos);
                return;
            }

            foreach (var child in LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>())
                children.Add(child);
            if (node is Visual || node is System.Windows.Media.Media3D.Visual3D)
            {
                int count = VisualTreeHelper.GetChildrenCount(node);
                for (int i = 0; i < count; i++)
                    children.Add(VisualTreeHelper.GetChild(node, i));
            }
            foreach (var child in children)
                Retranslate(child, map, seen, combos);
        }

        /// <summary>The colour dropdown lists plain strings; fill it again without applying the theme again.</summary>
        private void RefreshThemeNames(int index)
        {
            if (ddcolor == null)
                return;
            _refreshingThemeNames = true;
            ddcolor.ItemsSource = Themes.All.Select(t => TranslateOr(t.NameKey, t.Fallback)).ToList();
            ddcolor.SelectedIndex = index;
            _refreshingThemeNames = false;
        }
    }
}
