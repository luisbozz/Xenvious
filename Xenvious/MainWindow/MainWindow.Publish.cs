// Dashboard: the line under Publish (new job or update) and the choice before a publish that would make a new job.
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Xenvious
{
    public partial class MainWindow
    {
        private PublishMode.Mode _publishMode = PublishMode.Mode.Unknown;
        private string _publishBase;

        private void UpdatePublishMode(bool inCreator)
        {
            var (mode, baseId) = inCreator ? PublishMode.Current() : (PublishMode.Mode.Unknown, null);
            if (mode == _publishMode && baseId == _publishBase)
                return;
            _publishMode = mode;
            _publishBase = baseId;
            if (mode != PublishMode.Mode.Unknown)
                Logging.Log.Debug($"publish mode: {mode}{(baseId != null ? " " + baseId : "")}", source: "publish");
            DashPublishMode.Visibility = mode == PublishMode.Mode.Unknown ? Visibility.Collapsed : Visibility.Visible;
            DashPublishDot.SetResourceReference(Shape.FillProperty, mode == PublishMode.Mode.NewJob ? "WarnBrush" : "OkBrush");
            string shortId = baseId != null && baseId.Length > 12 ? baseId.Substring(0, 12) + "…" : baseId;
            switch (mode)
            {
                case PublishMode.Mode.NewJob:
                    DashPublishText.Text = TranslateOr("pub_mode_new", "New job");
                    break;
                case PublishMode.Mode.Update:
                    DashPublishText.Text = string.Format(CultureInfo.CurrentCulture, TranslateOr("pub_mode_update", "Updates {0}"), shortId);
                    break;
                case PublishMode.Mode.PublishSaved:
                    DashPublishText.Text = string.Format(CultureInfo.CurrentCulture, TranslateOr("pub_mode_saved", "Publishes saved job {0}"), shortId);
                    break;
            }
            DashPublishMode.ToolTip = baseId;
            if (mode == PublishMode.Mode.Update)
                BtnPublishMain.Content = TranslateOr("pub_update", "Update");
            else
                BtnPublishMain.SetBinding(ContentControl.ContentProperty, new System.Windows.Data.Binding("Translation[publish]") { FallbackValue = "Publish" });
        }

        private async void BtnPublishMain_Click(object sender, RoutedEventArgs e)
        {
            if (!m.IsProcOpen)
                return;
            // Only a publish that would make a new job asks; an update or a saved job goes on as before.
            if (PublishMode.Current().Mode != PublishMode.Mode.NewJob)
            {
                publishJob();
                return;
            }

            var optNew = PublishOption(TranslateOr("pub_opt_new", "New job"), TranslateOr("pub_opt_new_sub", "A second job next to the old one."), null);
            var idBox = new TextBox { Text = PublishMode.LastLoadedId ?? "", Height = 30, Margin = new Thickness(0, 8, 0, 0), VerticalContentAlignment = VerticalAlignment.Center };
            var optUpdate = PublishOption(TranslateOr("pub_opt_upd", "Update my published job"), TranslateOr("pub_opt_upd_sub", "Replaces that job instead of adding a second one."), idBox);
            var hint = new TextBlock { Text = TranslateOr("pub_hint", "Filled with the job you loaded last. Job id or link. Only works with your own jobs; for others the cloud refuses the update."),
                FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 10, 0, 0) };
            hint.SetResourceReference(TextBlock.ForegroundProperty, "FaintTextBrush");
            var content = new StackPanel();
            content.Children.Add(optNew);
            content.Children.Add(optUpdate);
            content.Children.Add(hint);

            string publishText = TranslateOr("publish", "Publish");
            string updateText = TranslateOr("pub_update", "Update");
            bool update = PublishMode.ParseId(idBox.Text) != null;
            var closed = ShowContentAsync(TranslateOr("pub_title", "Publish job"), TranslateOr("pub_text", "This creator would publish it as a new job."),
                content, update ? updateText : publishText, TranslateOr("dialog_cancel", "Cancel"), 520);
            void Refresh()
            {
                ((RadioButton)optNew.Tag).IsChecked = !update;
                ((RadioButton)optUpdate.Tag).IsChecked = update;
                idBox.IsEnabled = update;
                DialogConfirm.Content = update ? updateText : publishText;
                SetDialogConfirmEnabled(!update || PublishMode.ParseId(idBox.Text) != null);
            }
            ((RadioButton)optNew.Tag).Checked += (_, __) => { update = false; Refresh(); };
            ((RadioButton)optUpdate.Tag).Checked += (_, __) => { update = true; Refresh(); };
            idBox.TextChanged += (_, __) => Refresh();
            Refresh();

            bool go = await closed;
            SetDialogConfirmEnabled(true);
            if (!go || !m.IsProcOpen)
                return;
            if (update && !PublishMode.SetUpdate(PublishMode.ParseId(idBox.Text)))
            {
                displayScreenMessage(TranslateOr("pub_badid", "Not a valid job id or link."));
                return;
            }
            publishJob();
        }

        // A radio option with a bold title, a muted line under it and optional extra content; the RadioButton is in Tag.
        private Border PublishOption(string title, string sub, UIElement extra)
        {
            var radio = new RadioButton { GroupName = "PublishMode", VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 2, 10, 0) };
            var head = new TextBlock { Text = title, FontSize = 14, FontWeight = FontWeights.SemiBold };
            head.SetResourceReference(TextBlock.ForegroundProperty, "TextColor");
            var line = new TextBlock { Text = sub, FontSize = 12.5, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 0) };
            line.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            var text = new StackPanel();
            text.Children.Add(head);
            text.Children.Add(line);
            if (extra != null)
                text.Children.Add(extra);
            var row = new DockPanel();
            DockPanel.SetDock(radio, Dock.Left);
            row.Children.Add(radio);
            row.Children.Add(text);
            var border = new Border { BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6), Padding = new Thickness(12, 10, 12, 10), Margin = new Thickness(0, 0, 0, 8),
                Child = row, Tag = radio, Background = System.Windows.Media.Brushes.Transparent };
            border.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            // The whole box picks the option, not only the small circle.
            border.MouseLeftButtonUp += (_, __) => radio.IsChecked = true;
            return border;
        }
    }
}
