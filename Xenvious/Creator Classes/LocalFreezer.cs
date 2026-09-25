using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xenvious
{
    public partial class LocalFreezeer : ObservableObject, IDisposable
    {
        public string Name { get; set; }
        public long LocalValue { get; set; }
        public string LocalName { get; set; }
        public string Script { get; set; }
        public long Address { get; set; }
        public byte[] Value { get; set; } = Array.Empty<byte>();
        public object val { get; set; }

        private bool _isChecked;
        private readonly Type _type;
        private CancellationTokenSource? _cts;

        public IRelayCommand CheckedChanged { get; }
        public IRelayCommand<object?> ChangeValue { get; }

        public LocalFreezeer(string name, string script, string local, object value)
        {
            Name = name;
            Script = script;
            LocalName = local;

            // Falls dein Local-Offset anders ermittelt wird, hier ggf. anpassen:
            LocalValue = OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(local));

            val = value;
            _type = value.GetType();
            ChangeValueFunc(value);

            CheckedChanged = new RelayCommand(ToggleFreeze);
            ChangeValue = new RelayCommand<object?>(s => ChangeValueFunc(s!));
        }

        private void ToggleFreeze()
        {
            _isChecked = !_isChecked;

            if (_isChecked)
            {
                if (!MainWindow.m.IsProcOpen)
                {
                    _isChecked = false;
                    return;
                }

                long[]? scriptptr = GTA.getLocalScriptAddy(Script);
                if (scriptptr == null || scriptptr.Length < 2)
                {
                    _isChecked = false;
                    return;
                }

                // Adresse einmalig berechnen
                Address = MainWindow.m
                    .memory(scriptptr[0], new long[] {
                        scriptptr[1],
                        GTA.Offsets.Editor.OFFSET_script_local_start,
                        LocalValue * 8
                    })
                    .GetAddress();

                _cts = new CancellationTokenSource();
                _ = Task.Run(() => FreezeLocalAsync(_cts.Token));
            }
            else
            {
                _cts?.Cancel();
            }
        }

        private async Task FreezeLocalAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                MainWindow.m.memory(Address.ToString("X")).SetBytes(Value);
                // Minimales Yield, vermeidet 100% CPU
                await Task.Delay(1, ct);
            }
        }

        private void ChangeValueFunc(object value)
        {
            var s = value?.ToString();
            if (string.IsNullOrEmpty(s))
                return;

            try
            {
                // Für Float/Double: InvariantCulture -> '.' statt ',' erzwingen
                var inv = CultureInfo.InvariantCulture;

                if (_type == typeof(byte[]))
                {
                    Value = (byte[])value;
                }
                else if (_type == typeof(string))
                {
                    Value = Encoding.UTF8.GetBytes(Convert.ToString(value)!);
                }
                else if (_type == typeof(short))
                {
                    if (short.TryParse(s, NumberStyles.Integer, inv, out var si))
                        Value = BitConverter.GetBytes(si);
                    else
                        Value = BitConverter.GetBytes(ushort.Parse(s, inv));
                }
                else if (_type == typeof(int))
                {
                    if (int.TryParse(s, NumberStyles.Integer, inv, out var ii))
                        Value = BitConverter.GetBytes(ii);
                    else
                        Value = BitConverter.GetBytes(uint.Parse(s, inv));
                }
                else if (_type == typeof(long))
                {
                    if (long.TryParse(s, NumberStyles.Integer, inv, out var li))
                        Value = BitConverter.GetBytes(li);
                    else
                        Value = BitConverter.GetBytes(ulong.Parse(s, inv));
                }
                else if (_type == typeof(float))
                {
                    Value = BitConverter.GetBytes(float.Parse(s, inv));
                }
                else if (_type == typeof(double))
                {
                    Value = BitConverter.GetBytes(double.Parse(s, inv));
                }
            }
            catch
            {
                // optional: Logging
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
