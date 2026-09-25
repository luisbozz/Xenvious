using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xenvious
{
    public partial class GlobalFreezeer : ObservableObject, IDisposable
    {
        public string Name { get; set; }
        public long GlobalValue { get; set; }
        public string GlobalName { get; set; }
        public long Address { get; set; }
        public byte[] Value { get; set; }
        public object val { get; set; }

        private bool _isChecked;
        private Type _type;
        private CancellationTokenSource? _cts;

        public IRelayCommand CheckedChanged { get; }
        public IRelayCommand<object?> ChangeValue { get; }

        public GlobalFreezeer(string name, string global, object value)
        {
            Name = name;
            GlobalName = global;
            GlobalValue = OffsetLoader.GetGlobalOffset(OffsetLoader.PrepareGlobal(global));
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
                _cts = new CancellationTokenSource();
                _ = Task.Run(() => FreezeGlobalAsync(_cts.Token));
            }
            else
            {
                _cts?.Cancel();
            }
        }

        private async Task FreezeGlobalAsync(CancellationToken ct)
        {
            if (MainWindow.m.IsProcOpen)
            {
                Address = new Global(GlobalValue).GetAddress();
            }

            // Endlosschleife kooperativ abbrechbar + CPU-schonend
            while (!ct.IsCancellationRequested)
            {
                MainWindow.m.memory(Address.ToString("X")).SetBytes(Value);
                await Task.Delay(1, ct); // minimaler Yield statt Busy-Loop
            }
        }

        private void ChangeValueFunc(object value)
        {
            if (string.IsNullOrEmpty(value?.ToString()))
                return;

            try
            {
                if (_type == typeof(byte[])) Value = (byte[])value;
                else if (_type == typeof(string)) Value = Encoding.UTF8.GetBytes(Convert.ToString(value)!);
                else if (_type == typeof(short)) Value = BitConverter.GetBytes(short.Parse(value.ToString()!));
                else if (_type == typeof(int)) Value = BitConverter.GetBytes(int.Parse(value.ToString()!));
                else if (_type == typeof(long)) Value = BitConverter.GetBytes(long.Parse(value.ToString()!));
                else if (_type == typeof(float)) Value = BitConverter.GetBytes(float.Parse(value.ToString()!));
                else if (_type == typeof(double)) Value = BitConverter.GetBytes(double.Parse(value.ToString()!));
            }
            catch { /* ggf. Logging */ }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
