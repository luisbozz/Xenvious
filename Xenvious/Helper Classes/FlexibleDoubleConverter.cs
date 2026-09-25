using System;
using System.Globalization;
using System.Windows.Data;

namespace Xenvious.Helper_Classes
{
    /// <summary>
    /// Converts between double values and strings, accepting both comma and dot as decimal separators.
    /// </summary>
    public sealed class FlexibleDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ConverterParameter = number of decimals to show ("2" -> 63.59).
            if (value is double d)
            {
                if (parameter != null && int.TryParse(parameter.ToString(), out int decimals) && decimals >= 0)
                {
                    return d.ToString(decimals == 0 ? "0" : "0." + new string('#', decimals), culture);
                }
                return d.ToString("G", culture);
            }

            if (value is float f)
            {
                return ((double)f).ToString("G", culture);
            }

            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return Binding.DoNothing;
                }

                if (double.TryParse(text, NumberStyles.Float, culture, out var result))
                {
                    return result;
                }

                if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                {
                    return result;
                }

                var normalized = text.Replace(',', '.');
                if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                {
                    return result;
                }
            }

            return Binding.DoNothing;
        }
    }
}
