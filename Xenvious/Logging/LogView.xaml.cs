using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Xenvious.Logging;

namespace Xenvious.Logging
{
    public partial class LogView : UserControl
    {
        public LogView()
        {
            InitializeComponent();
            DataContext = this;
            _view = (ListCollectionView)CollectionViewSource.GetDefaultView(Entries);
            _view.Filter = Filter;
            List.ItemsSource = Entries;
        }

        private readonly ListCollectionView _view;
        public ObservableCollection<LogEntry> Entries { get; } = new();

        public static readonly DependencyProperty LoggerProperty = DependencyProperty.Register(
            nameof(Logger), typeof(Logger), typeof(LogView), new PropertyMetadata(null, OnLoggerChanged));

        public Logger? Logger
        {
            get => (Logger?)GetValue(LoggerProperty);
            set => SetValue(LoggerProperty, value);
        }

        private static void OnLoggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LogView)d;
            var oldLog = e.OldValue as Logger;
            if (oldLog != null)
                oldLog.EntryAdded -= control.OnEntryAdded;

            var newLog = e.NewValue as Logger;
            if (newLog != null)
                newLog.EntryAdded += control.OnEntryAdded;
        }


        private void OnEntryAdded(object sender, LogEntry e)
        {
            // Marshal to the UI thread, but never wait for it. Dispatcher.Invoke
            // blocks the caller until the UI thread runs the callback, and a log
            // call can come from anywhere -- including a worker the UI thread is
            // itself waiting on. Timercheckgta_Tick does exactly that: it resolves
            // ten pointers through Parallel.Invoke, and on GTA Enhanced seven of
            // those patterns are empty, so seven workers warn at once while the UI
            // thread sits inside Parallel.Invoke waiting for them. Everything
            // stops. A log line is not worth a deadlock, so post it and move on.
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => OnEntryAdded(sender, e)));
                return;
            }

            Entries.Add(e);

            // Enforce capacity if provided by logger
            if (Logger != null && Logger.MaxEntries > 0 && Entries.Count > Logger.MaxEntries)
            {
                var removeCount = Entries.Count - Logger.MaxEntries;
                for (int i = 0; i < removeCount; i++)
                    Entries.RemoveAt(0);
            }

            if (AutoScrollCheck.IsChecked == true && List.Items.Count > 0)
            {
                List.ScrollIntoView(List.Items[List.Items.Count - 1]);
            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e) => Entries.Clear();

        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = List.SelectedItems.Cast<LogEntry>().ToList();
            if (!selected.Any()) return;
            Clipboard.SetText(string.Join(Environment.NewLine, selected.Select(x => x.ToString())));
        }

        private void CopyAllBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(string.Join(Environment.NewLine, Entries.Select(x => x.ToString())));
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _view.Refresh();
        }

        private bool Filter(object obj)
        {
            var e = obj as LogEntry; if (e == null) return false;
            var text = SearchBox.Text?.Trim();
            if (string.IsNullOrEmpty(text)) return true;
            return (e.Message?.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                || (e.Source?.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                || e.Level.ToString().IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}