using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Xenvious
{
    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }

    public class IsStringNotEmpty : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsStringNotEmpty can only be used OneWay.");
        }
    }

    public class IsCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool)value;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsCheckedConverter can only be used OneWay.");
        }
    }

    public class FreezeValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int || value is float || value is long || value is double || value is short)
            {
                return value.ToString();
            }
            else if (value is byte[])
            {
                return string.Join(", ", (value as byte[]).Select(x => x.ToString("X")));
            }
            else if (value is string)
            {
                return value;
            }
            return "empty";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class ColorToShadowColorConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Only touch the shadow color if it's a solid color, do not mess up other fancy effects
            if (value is SolidColorBrush)
            {
                Color color = ((SolidColorBrush)value).Color;
                var r = Transform(color.R);
                var g = Transform(color.G);
                var b = Transform(color.B);

                // return with Color and not SolidColorBrush, otherwise it will not work
                // This means that most likely the Color -> SolidBrushColor conversion does the RBG -> sRBG conversion somewhere...
                return Color.FromArgb(color.A, r, g, b);
            }

            return value;
        }

        private byte Transform(byte source)
        {
            // see http://en.wikipedia.org/wiki/SRGB
            return (byte)(Math.Pow(source / 255d, 1 / 2.2d) * 255);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("ColorToShadowColorConverter is a OneWay converter.");
        }

        #endregion
    }
    public class TrimmedTextBlockVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;

            FrameworkElement textBlock = (FrameworkElement)value;

            textBlock.Measure(new System.Windows.Size(Double.PositiveInfinity, Double.PositiveInfinity));

            if (((FrameworkElement)value).ActualWidth < ((FrameworkElement)value).DesiredSize.Width)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class DoubleMultiplierConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!TryGetDouble(value, out var input) || double.IsNaN(input) || input <= 0d)
            {
                return double.PositiveInfinity;
            }

            var factor = 1d;
            if (parameter != null)
            {
                var parameterString = parameter as string ?? parameter.ToString();
                if (!double.TryParse(parameterString, NumberStyles.Float, culture, out factor) &&
                    !double.TryParse(parameterString, NumberStyles.Float, CultureInfo.InvariantCulture, out factor))
                {
                    factor = 1d;
                }
            }

            if (double.IsNaN(factor) || double.IsInfinity(factor))
            {
                factor = 1d;
            }

            var result = input * factor;

            if (double.IsNaN(result) || double.IsInfinity(result) || result <= 0d)
            {
                result = double.PositiveInfinity;
            }

            if (targetType == typeof(GridLength))
            {
                return new GridLength(result);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("DoubleMultiplierConverter is a OneWay converter.");
        }

        private static bool TryGetDouble(object value, out double result)
        {
            switch (value)
            {
                case double d:
                    result = d;
                    return true;
                case float f:
                    result = f;
                    return true;
                case int i:
                    result = i;
                    return true;
                case long l:
                    result = l;
                    return true;
                case string s when double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed):
                    result = parsed;
                    return true;
                default:
                    result = 0d;
                    return false;
            }
        }
    }
}
