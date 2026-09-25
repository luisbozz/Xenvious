using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Xenvious.Helper_Classes
{
    /// <summary>
    /// True when the bound value (enum, int, bool) equals the parameter, compared by
    /// text. Two-way: checking a toggle writes the parameter back, which turns a set
    /// of ToggleButtons into a radio group.
    /// </summary>
    public sealed class EqualsToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Equals(value?.ToString(), parameter?.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool b) || !b || parameter == null)
            {
                return Binding.DoNothing;
            }
            Type type = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (type.IsEnum)
            {
                return Enum.Parse(type, parameter.ToString());
            }
            return System.Convert.ChangeType(parameter.ToString(), type, CultureInfo.InvariantCulture);
        }
    }

    /// <summary>Visible when the bound value equals one of the space-separated parameters.</summary>
    public sealed class EqualsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value?.ToString() ?? "";
            foreach (string p in (parameter?.ToString() ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (string.Equals(text, p, StringComparison.OrdinalIgnoreCase))
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    /// <summary>Bool to Visibility; parameter "invert" flips it.</summary>
    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = value is bool v && v;
            if (string.Equals(parameter?.ToString(), "invert", StringComparison.OrdinalIgnoreCase))
            {
                b = !b;
            }
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}

namespace Xenvious.Helper_Classes
{
    /// <summary>True when the bound int is at least the parameter (step dots: done or current).</summary>
    public sealed class AtLeastToBoolConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is int v && int.TryParse(parameter?.ToString(), out int min) && v >= min;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) => System.Windows.Data.Binding.DoNothing;
    }
}
