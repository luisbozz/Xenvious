using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Xenvious
{
    /// <summary>
    /// A row of the Global Editor's freeze list: a global or a script local that, while frozen,
    /// keeps getting the value written. The value is kept as text (shown and saved) and as the
    /// bytes it encodes to (written).
    /// </summary>
    public abstract class FreezeItem : ObservableObject
    {
        protected FreezeItem(string name, EditorKind kind, string text)
        {
            Name = name;
            Kind = kind;
            Text = text;
        }

        public string Name { get; }
        public EditorKind Kind { get; }
        public string TypeName => Kind.ToString();

        /// <summary>Where the value goes, shown under the name.</summary>
        public abstract string Path { get; }

        private string _text = "";
        private byte[] _bytes;
        private bool _invalid;

        public string Text
        {
            get => _text;
            set
            {
                if (!SetProperty(ref _text, value ?? ""))
                    return;
                try
                {
                    _bytes = EditorValue.Encode(Kind, _text);
                    Invalid = false;
                }
                catch (Exception e) when (e is FormatException || e is OverflowException)
                {
                    // The last value that fit stays frozen until the text fits again.
                    Invalid = true;
                }
            }
        }

        /// <summary>The text does not fit the type; the row shows it.</summary>
        public bool Invalid
        {
            get => _invalid;
            private set => SetProperty(ref _invalid, value);
        }

        public byte[] Bytes => _bytes;

        private bool _frozen;

        public bool IsFrozen
        {
            get => _frozen;
            set
            {
                if (value && (MainWindow.m == null || !MainWindow.m.IsProcOpen))
                    value = false;
                if (SetProperty(ref _frozen, value))
                    Freezer.Changed(this);
                else
                    OnPropertyChanged(nameof(IsFrozen));
            }
        }

        /// <summary>The value's address in the game, 0 when it cannot be found now.</summary>
        internal abstract long Resolve();
    }

    /// <summary>
    /// Writes every frozen value on one background thread. Before, each entry ran its own task
    /// and parsed its address from a string on every write.
    /// Addresses are looked up again every second: a local moves when its script restarts.
    /// </summary>
    public static class Freezer
    {
        private static readonly List<FreezeItem> _items = new List<FreezeItem>();
        private static Thread _thread;

        /// <summary>Raised on the thread that froze or thawed an entry.</summary>
        public static event Action CountChanged;

        public static int Count
        {
            get { lock (_items) return _items.Count; }
        }

        internal static void Changed(FreezeItem item)
        {
            lock (_items)
            {
                if (item.IsFrozen && !_items.Contains(item))
                    _items.Add(item);
                else if (!item.IsFrozen)
                    _items.Remove(item);
                if (_items.Count > 0 && _thread == null)
                {
                    _thread = new Thread(Run) { IsBackground = true, Name = "Freezer" };
                    _thread.Start();
                }
            }
            CountChanged?.Invoke();
        }

        private static void Run()
        {
            var addresses = new Dictionary<FreezeItem, (string Hex, DateTime At)>();
            while (true)
            {
                FreezeItem[] items;
                lock (_items)
                {
                    if (_items.Count == 0)
                    {
                        _thread = null;
                        return;
                    }
                    items = _items.ToArray();
                }
                if (!MainWindow.m.IsProcOpen)
                {
                    addresses.Clear();
                    Thread.Sleep(250);
                    continue;
                }
                var now = DateTime.UtcNow;
                foreach (var item in items)
                {
                    try
                    {
                        if (!addresses.TryGetValue(item, out var a) || (now - a.At).TotalSeconds >= 1)
                        {
                            long address = item.Resolve();
                            a = (address == 0 ? null : address.ToString("X", CultureInfo.InvariantCulture), now);
                            addresses[item] = a;
                        }
                        if (a.Hex != null && item.Bytes != null)
                            MainWindow.m.memory(a.Hex).SetBytes(item.Bytes);
                    }
                    catch
                    {
                        addresses.Remove(item);
                    }
                }
                Thread.Sleep(5);
            }
        }
    }
}
