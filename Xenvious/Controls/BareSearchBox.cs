using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Xenvious
{
    /// <summary>
    /// The search line used across the redesign: no box, a grey hint until something is typed
    /// and a thin line under it (same look as the drop-down search in SearchableCombo).
    /// </summary>
    public class BareSearchBox : Grid
    {
        public TextBox Box { get; } = new TextBox { Background = Brushes.Transparent, BorderThickness = new Thickness(0), Padding = new Thickness(2, 6, 2, 6), FontSize = 14 };
        private readonly TextBlock _hint = new TextBlock { Margin = new Thickness(5, 6, 2, 6), FontSize = 14, IsHitTestVisible = false };

        public event Action<string> Changed;

        public string Text => Box.Text;

        public BareSearchBox(string hint)
        {
            _hint.Text = hint;
            Box.SetResourceReference(Control.ForegroundProperty, "TextColor");
            Box.SetResourceReference(TextBoxBase.CaretBrushProperty, "TextColor");
            _hint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            var line = new Border { Height = 1, VerticalAlignment = VerticalAlignment.Bottom };
            line.SetResourceReference(Border.BackgroundProperty, "LineBrush");
            Children.Add(Box);
            Children.Add(_hint);
            Children.Add(line);
            Box.TextChanged += (_, __) =>
            {
                _hint.Visibility = Box.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
                Changed?.Invoke(Box.Text);
            };
        }
    }
}
