// Error card for cloud refusals (save, publish, update, job download): title, reason, facts, hint.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xenvious
{
    public partial class MainWindow
    {
        /// <summary>
        /// Shows an error card in the in-app dialog: a red mark and the title, one line why,
        /// a box with the facts (id, creator, code) and a grey hint. "Copy details" puts title,
        /// reason and facts on the clipboard without closing the card. Waits while another
        /// dialog is open instead of replacing it. Call on the UI thread.
        /// </summary>
        public async void ShowErrorCard(string title, string reason, IList<(string Label, string Value)> facts, string hint)
        {
            var factsGrid = new Grid();
            factsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            factsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            for (int i = 0; i < facts.Count; i++)
            {
                factsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var label = new TextBlock { Text = facts[i].Label, FontSize = 12, Margin = new Thickness(0, 2, 14, 2) };
                label.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
                var value = new TextBlock { Text = facts[i].Value, FontSize = 12, Margin = new Thickness(0, 2, 0, 2), TextWrapping = TextWrapping.Wrap };
                value.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
                Grid.SetRow(label, i);
                Grid.SetRow(value, i);
                Grid.SetColumn(value, 1);
                factsGrid.Children.Add(label);
                factsGrid.Children.Add(value);
            }
            var box = new Border { CornerRadius = new CornerRadius(5), Padding = new Thickness(10, 6, 10, 6), Child = factsGrid };
            box.SetResourceReference(Border.BackgroundProperty, "TextBoxBackground");

            var content = new StackPanel();
            if (facts.Count > 0)
                content.Children.Add(box);
            if (!string.IsNullOrEmpty(hint))
            {
                var tip = new TextBlock { Text = hint, FontSize = 12.5, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, facts.Count > 0 ? 10 : 0, 0, 0) };
                tip.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
                content.Children.Add(tip);
            }

            string details = title + Environment.NewLine + reason + Environment.NewLine
                + string.Join(Environment.NewLine, facts.Select(f => f.Label + ": " + f.Value));
            // The mark, the darker reason and "Copy details" go in when the card is shown: it
            // waits behind an open dialog instead of replacing it.
            await ShowContentAsync(title, reason, content, "OK", null, 560, onShown: () =>
            {
                var bad = ThemeBrush("BadBrush") as SolidColorBrush;
                DialogIcon.Background = bad != null ? new SolidColorBrush(Color.FromArgb(0x29, bad.Color.R, bad.Color.G, bad.Color.B)) : Brushes.Transparent;
                DialogIconText.Foreground = bad ?? Brushes.Red;
                DialogIcon.Visibility = Visibility.Visible;
                DialogText.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
                DialogAlt.Content = TranslateOr("err_copy", "Copy details");
                DialogAltContainer.Visibility = Visibility.Visible;
                _dialogAltAction = () =>
                {
                    try
                    {
                        Clipboard.SetText(details);
                        DialogAlt.Content = TranslateOr("err_copied", "Copied");
                    }
                    catch (Exception ex)
                    {
                        Logging.Log.Debug("copy details: " + ex.Message, source: "ErrorCard");
                    }
                };
            });
        }
    }
}
