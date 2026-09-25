using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Xenvious.Helper_Classes
{
    public sealed class MenuSwitcherPreset : INotifyPropertyChanged
    {
        private string name = string.Empty;
        private int menuId;
        private int returnMenuId;

        public MenuSwitcherPreset()
        {
        }

        public MenuSwitcherPreset(string name, int menuId, int returnMenuId)
        {
            Name = name;
            MenuId = menuId;
            ReturnMenuId = returnMenuId;
        }

        public string Name
        {
            get => name;
            set => SetField(ref name, value);
        }

        public int MenuId
        {
            get => menuId;
            set
            {
                if (SetField(ref menuId, value))
                {
                    OnPropertyChanged(nameof(ToolTip));
                }
            }
        }

        public int ReturnMenuId
        {
            get => returnMenuId;
            set
            {
                if (SetField(ref returnMenuId, value))
                {
                    OnPropertyChanged(nameof(ToolTip));
                }
            }
        }

        [JsonIgnore]
        public string ToolTip => $"Menu ID: {MenuId}\nReturn ID: {ReturnMenuId}";

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
