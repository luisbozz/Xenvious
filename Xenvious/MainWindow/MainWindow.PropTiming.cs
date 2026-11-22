// Props page: the "Hide Prop after" timing list, which edits the timing of many props at once.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    public partial class MainWindow
    {
        // The list shows only props whose timing is above 0. It is read when it opens, when the
        // selected prop or the prop count changes and after an edit, not on every worker tick:
        // with 200 props that froze the app. The ticked rows and the numbers typed into
        // "Select props" are kept by prop number, because a new ItemsSource drops the ListBox
        // selection.
        private List<PropHAS> _hasList = new List<PropHAS>();
        private HashSet<int> _hasTicked = new HashSet<int>();
        private HashSet<int> _hasTyped = new HashSet<int>();
        private bool _hasUpdating;
        private int _hasPropCount = -1;

        /// <summary>Reads the timing list again when it is open.</summary>
        public void RefreshPropHasList()
        {
            if (haslistcontainer.Visibility == Visibility.Visible)
                initializePropHasList();
        }

        public void initializePropHasList()
        {
            if (!m.IsProcOpen)
                return;
            try
            {
                int count = new Global(GTA.Offsets.Editor.Props.number).Get<int>();
                var raw = new List<(int Id, int Time, int Model)>();
                for (int i = 0; i < count; i++)
                {
                    int has = new Global(GTA.Offsets.Editor.Props.fcuat + i * GTA.Offsets.Editor.Props.NEXT).Get<int>();
                    if (has > 0)
                        raw.Add((i, has, new Global(GTA.Offsets.Editor.Props.model + i * GTA.Offsets.Editor.Props.NEXT).Get<int>()));
                }

                // Rows are only built (with their names) when something changed.
                bool same = raw.Count == _hasList.Count
                    && raw.Zip(_hasList, (a, b) => a.Id == b.Id && a.Time == b.Time && a.Model == b.Prop).All(x => x);
                var list = same ? _hasList : raw.Select(r => new PropHAS(r.Id, r.Time, r.Model)).ToList();
                if (!same)
                {
                    _hasList = list;
                    _hasUpdating = true;
                    PropHASList.ItemsSource = list;
                    foreach (var item in list.Where(x => _hasTicked.Contains(x.Id)))
                        PropHASList.SelectedItems.Add(item);
                    _hasUpdating = false;
                }

                Lblpropshasmax.Text = (list.Count > 0 ? list.Max(x => x.Time) : 0).ToString(CultureInfo.CurrentCulture) + " sec";
                Lblpropshasmin.Text = (list.Count > 0 ? list.Min(x => x.Time) : 0).ToString(CultureInfo.CurrentCulture) + " sec";
                Lblpropshascount.Text = list.Count.ToString(CultureInfo.CurrentCulture);
                UpdateHasSelection();
            }
            catch (Exception ex)
            {
                Logging.Log.Debug("prop timing list: " + ex.Message, source: "PropTiming");
            }
        }

        // "1-10", "1,2,5,7,123", "1-5, 123": prop numbers as the list shows them (from 1), returned
        // as prop indices (from 0).
        private static HashSet<int> ParsePropNumbers(string text)
        {
            var ids = new HashSet<int>();
            foreach (string part in (text ?? "").Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] r = part.Split('-');
                if (r.Length == 1 && int.TryParse(r[0], NumberStyles.None, CultureInfo.InvariantCulture, out int one))
                {
                    if (one > 0)
                        ids.Add(one - 1);
                }
                else if (r.Length == 2
                    && int.TryParse(r[0], NumberStyles.None, CultureInfo.InvariantCulture, out int from)
                    && int.TryParse(r[1], NumberStyles.None, CultureInfo.InvariantCulture, out int to))
                {
                    if (from > to)
                        (from, to) = (to, from);
                    for (int i = Math.Max(from, 1); i <= to && i - from < 5000; i++)
                        ids.Add(i - 1);
                }
            }
            return ids;
        }

        private void tbpropshassel_TextChanged(object sender, TextChangedEventArgs e)
        {
            _hasTyped = ParsePropNumbers(tbpropshassel.Text);
            SetHasTicked(_hasList.Where(x => _hasTyped.Contains(x.Id)).Select(x => x.Id));
        }

        private void SetHasTicked(IEnumerable<int> ids)
        {
            _hasTicked = new HashSet<int>(ids);
            _hasUpdating = true;
            PropHASList.SelectedItems.Clear();
            foreach (var item in _hasList.Where(x => _hasTicked.Contains(x.Id)))
                PropHASList.SelectedItems.Add(item);
            _hasUpdating = false;
            UpdateHasSelection();
        }

        private void PropHASList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_hasUpdating)
                return;
            _hasTicked = new HashSet<int>(PropHASList.SelectedItems.Cast<PropHAS>().Select(x => x.Id));
            UpdateHasSelection();
        }

        private void Btnpropshasall_Click(object sender, RoutedEventArgs e) => SetHasTicked(_hasList.Select(x => x.Id));

        private void cbpropshasall_Click(object sender, RoutedEventArgs e)
        {
            bool all = _hasList.Count > 0 && _hasList.All(x => _hasTicked.Contains(x.Id));
            SetHasTicked(all ? Enumerable.Empty<int>() : _hasList.Select(x => x.Id));
        }

        // Typed numbers that have no timing yet (not in the list) and exist in the job.
        private List<int> HasTypedWithoutTiming()
        {
            int count = m.IsProcOpen ? new Global(GTA.Offsets.Editor.Props.number).Get<int>() : 0;
            return _hasTyped.Where(id => id < count && !_hasList.Any(x => x.Id == id)).OrderBy(x => x).ToList();
        }

        private void UpdateHasSelection()
        {
            int ticked = _hasList.Count(x => _hasTicked.Contains(x.Id));
            cbpropshasall.IsChecked = ticked == 0 ? false : ticked == _hasList.Count ? true : (bool?)null;

            var extra = HasTypedWithoutTiming();
            string text = string.Format(CultureInfo.CurrentCulture, TranslateOr("phs_selected", "{0} selected"), ticked);
            if (extra.Count > 0)
                text += " · " + string.Format(CultureInfo.CurrentCulture, TranslateOr("phs_notiming", "{0} without timing yet ({1})"),
                    extra.Count, ShortList(extra));
            Lblpropshassel.Text = text;
        }

        private static string ShortList(List<int> ids)
        {
            string s = string.Join(", ", ids.Take(8).Select(x => (x + 1).ToString(CultureInfo.InvariantCulture)));
            return ids.Count > 8 ? s + ", …" : s;
        }

        // The ticked rows; with addTyped also the typed numbers without timing, which only
        // "+" and "Set to value" can give a timing.
        private List<int> HasTargets(bool addTyped)
        {
            var ids = _hasList.Where(x => _hasTicked.Contains(x.Id)).Select(x => x.Id).ToList();
            if (addTyped)
                ids.AddRange(HasTypedWithoutTiming());
            return ids;
        }

        private bool HasEditValue(out int value)
        {
            value = 0;
            if (!IsValidInt(tbpropshasinc.Text, false))
            {
                displayScreenMessage(TranslateOr("phs_badvalue", "Edit Value needs a whole number of seconds."));
                return false;
            }
            value = Functions.int_parse(tbpropshasinc.Text);
            return true;
        }

        private void WriteHasTimings(List<int> ids, Func<int, int> next)
        {
            if (!m.IsProcOpen)
                return;
            if (ids.Count == 0)
            {
                displayScreenMessage(TranslateOr("phs_nothing", "Select props first: tick rows or type their numbers."));
                return;
            }
            foreach (int id in ids)
            {
                var g = new Global(GTA.Offsets.Editor.Props.fcuat + id * GTA.Offsets.Editor.Props.NEXT);
                g.SetInt(Math.Max(0, next(g.Get<int>())));
            }
            initializePropHasList();
        }

        private void Btnpropshasplus_Click(object sender, RoutedEventArgs e)
        {
            if (HasEditValue(out int v))
                WriteHasTimings(HasTargets(true), t => t + v);
        }

        private void Btnpropshasminus_Click(object sender, RoutedEventArgs e)
        {
            if (HasEditValue(out int v))
                WriteHasTimings(HasTargets(false), t => t - v);
        }

        private void Btnpropshasset_Click(object sender, RoutedEventArgs e)
        {
            if (HasEditValue(out int v))
                WriteHasTimings(HasTargets(true), t => v);
        }

        private async void Btnpropshaszero_Click(object sender, RoutedEventArgs e)
        {
            var ids = HasTargets(false);
            if (ids.Count > 1 && !await ConfirmAsync(TranslateOr("phs_zero_title", "Clear prop timings"),
                    string.Format(CultureInfo.CurrentCulture, TranslateOr("phs_zero_ask", "Set the timing of {0} props to 0? They will no longer hide."), ids.Count),
                    TranslateOr("phs_zero", "Set to 0"), TranslateOr("dialog_cancel", "Cancel"), danger: true))
                return;
            WriteHasTimings(ids, t => 0);
        }
    }
}
