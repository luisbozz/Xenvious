using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Xenvious.Helper_Classes
{
    /// <summary>
    /// Mouse wheel and arrow keys change a numeric TextBox by <c>Step</c>;
    /// Shift for a tenth of it, Ctrl for ten times. The value goes straight to the
    /// binding, so the preview follows while you scroll.
    /// </summary>
    public static class NumericNudge
    {
        public static readonly DependencyProperty StepProperty = DependencyProperty.RegisterAttached(
            "Step", typeof(double), typeof(NumericNudge), new PropertyMetadata(0.0, OnStepChanged));

        public static double GetStep(DependencyObject o) => (double)o.GetValue(StepProperty);
        public static void SetStep(DependencyObject o, double value) => o.SetValue(StepProperty, value);

        private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBox box))
            {
                return;
            }
            box.PreviewMouseWheel -= OnWheel;
            box.PreviewKeyDown -= OnKey;
            if ((double)e.NewValue > 0)
            {
                box.PreviewMouseWheel += OnWheel;
                box.PreviewKeyDown += OnKey;
            }
        }

        private static void OnWheel(object sender, MouseWheelEventArgs e)
        {
            var box = (TextBox)sender;
            if (!box.IsKeyboardFocusWithin)
            {
                return; // scrolling the page must not change values it passes over
            }
            Nudge(box, e.Delta > 0 ? 1 : -1);
            e.Handled = true;
        }

        private static void OnKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                Nudge((TextBox)sender, e.Key == Key.Up ? 1 : -1);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                ((TextBox)sender).GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            }
        }

        private static void Nudge(TextBox box, int direction)
        {
            double step = GetStep(box);
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) step /= 10;
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0) step *= 10;

            string text = (box.Text ?? "").Trim().Replace(',', '.');
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                value = 0;
            }
            value = Math.Round(value + direction * step, 6);
            box.Text = value.ToString("0.######", CultureInfo.CurrentCulture);
            box.CaretIndex = box.Text.Length;
            box.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }
    }
}
