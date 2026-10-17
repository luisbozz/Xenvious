using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Xenvious
{
    /// <summary>
    /// A confirmation dialog inside the Xenvious window instead of a Windows message box:
    /// the window behind is dimmed and blurred, Enter confirms, Esc cancels. One dialog shows
    /// at a time; the others wait in a queue ordered by <see cref="DialogLevel"/>.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Which waiting dialog comes first. An open dialog is never pushed away; when it closes,
        /// the waiting one with the highest level shows next, at the same level the earlier one.
        /// </summary>
        public enum DialogLevel { Low = 0, Normal = 1, High = 2, Critical = 3 }

        private sealed class DialogRequest
        {
            public DialogLevel Level;
            public long Order;
            public bool Busy;
            public System.Action Fill;
            public System.Func<bool> StillWanted;
            public TaskCompletionSource<DialogChoice> Result;
            public Task Handle;
        }

        private readonly List<DialogRequest> _dialogQueue = new List<DialogRequest>();
        private DialogRequest _dialogCurrent;
        private long _dialogOrder;

        // Automatic offers (start the creator, ...) wait until the start-up update check is
        // done, so an update is offered first.
        private bool _holdLowDialogs = true;

        private void ReleaseLowDialogs()
        {
            _holdLowDialogs = false;
            ShowNextDialog();
        }

        /// <summary>
        /// Shows the dialog and returns true when confirmed. <paramref name="cancelText"/>
        /// null shows only the confirm button. <paramref name="onShown"/> runs once the dialog is
        /// on screen (it may wait behind others); <paramref name="stillWanted"/> false at that
        /// moment skips it.
        /// </summary>
        public Task<bool> ConfirmAsync(string title, string message, string confirmText, string cancelText, bool danger = false,
            IList<ChangelogText.Line> details = null, DialogLevel level = DialogLevel.Normal,
            System.Action onShown = null, System.Func<bool> stillWanted = null)
        {
            var request = Enqueue(level, () => FillDialog(title, message, confirmText, cancelText, danger, details, null, onShown), stillWanted, false);
            var confirmed = request.Result.Task.ContinueWith(t => t.Result == DialogChoice.Confirm,
                System.Threading.CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            request.Handle = confirmed;
            ShowNextDialog();
            return confirmed;
        }

        private DialogRequest Enqueue(DialogLevel level, System.Action fill, System.Func<bool> stillWanted, bool busy)
        {
            var request = new DialogRequest
            {
                Level = level,
                Order = _dialogOrder++,
                Busy = busy,
                Fill = fill,
                StillWanted = stillWanted,
                Result = new TaskCompletionSource<DialogChoice>(TaskCreationOptions.RunContinuationsAsynchronously),
            };
            request.Handle = request.Result.Task;
            _dialogQueue.Add(request);
            return request;
        }

        private void ShowNextDialog()
        {
            if (_dialogCurrent != null)
                return;
            while (true)
            {
                DialogRequest next = null;
                foreach (var request in _dialogQueue)
                {
                    if (_holdLowDialogs && request.Level == DialogLevel.Low)
                        continue;
                    if (next == null || request.Level > next.Level || (request.Level == next.Level && request.Order < next.Order))
                        next = request;
                }
                if (next == null)
                {
                    DialogOverlay.Visibility = Visibility.Collapsed;
                    Main.Effect = null;
                    return;
                }
                _dialogQueue.Remove(next);
                if (next.StillWanted != null && !next.StillWanted())
                {
                    next.Result.TrySetResult(DialogChoice.Cancel);
                    continue;
                }
                _dialogCurrent = next;
                _dialogBusy = next.Busy;
                next.Fill();
                Main.Effect = new BlurEffect { Radius = 8, KernelType = KernelType.Gaussian };
                DialogOverlay.Visibility = Visibility.Visible;
                if (!next.Busy)
                {
                    DialogOverlay.Focus();
                    Keyboard.Focus(DialogOverlay);
                }
                return;
            }
        }

        private void FillDialog(string title, string message, string confirmText, string cancelText, bool danger,
            IList<ChangelogText.Line> details, string alternativeText, System.Action onShown)
        {
            DialogAltContainer.Visibility = alternativeText == null ? Visibility.Collapsed : Visibility.Visible;
            DialogAlt.Content = alternativeText ?? "";
            DialogButtons.Visibility = Visibility.Visible;
            DialogProgress.Visibility = Visibility.Collapsed;
            DialogImageBox.Visibility = Visibility.Collapsed;
            SetDialogConfirmEnabled(true);
            _dialogAltAction = null;
            _dialogPaste = null;
            _dialogDrop = null;
            SetDialogDetails(details);
            DialogContent.Content = null;
            DialogContent.Visibility = Visibility.Collapsed;
            DialogBox.MaxWidth = 560;
            DialogIcon.Visibility = Visibility.Collapsed;

            DialogTitle.Text = title ?? "";
            DialogText.Text = message ?? "";
            DialogText.Visibility = string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;
            DialogText.SetResourceReference(TextBlock.ForegroundProperty, "MutedTextBrush");
            DialogConfirm.Content = confirmText;
            _dialogConfirmBrush = ThemeBrush(danger ? "BadBrush" : "HighlightBrush");
            DialogConfirmContainer.Background = _dialogConfirmBrush;
            DialogConfirm.Foreground = danger ? Brushes.White : ThemeBrush("HighlightForeground");
            DialogCancel.Content = cancelText ?? "";
            DialogCancelContainer.Visibility = cancelText == null ? Visibility.Collapsed : Visibility.Visible;
            onShown?.Invoke();
        }

        /// <summary>
        /// Closes the dialog behind <paramref name="handle"/> (what the show call returned) as
        /// cancelled, or drops it while it still waits; other dialogs stay.
        /// </summary>
        public void CancelDialog(Task handle)
        {
            if (handle == null)
                return;
            if (_dialogCurrent != null && _dialogCurrent.Handle == handle)
            {
                CloseDialog(DialogChoice.Cancel);
                return;
            }
            var waiting = _dialogQueue.Find(r => r.Handle == handle);
            if (waiting != null)
            {
                _dialogQueue.Remove(waiting);
                waiting.Result.TrySetResult(DialogChoice.Cancel);
            }
        }

        private Brush _dialogConfirmBrush;

        // Hover and press colour the whole button: the confirm button a darker shade of its colour
        // (darker still while pressed), cancel and the alternative the theme's button colours.
        private void DialogButton_MouseEnter(object sender, MouseEventArgs e) => ShadeDialogButton((Border)sender, 0.85, "ButtonHoverBackgroundBrush");

        private void DialogButton_MouseDown(object sender, MouseButtonEventArgs e) => ShadeDialogButton((Border)sender, 0.7, "ButtonClickBackgroundBrush");

        private void ShadeDialogButton(Border box, double shade, string themeBrush)
        {
            if (box == DialogConfirmContainer)
            {
                if (_dialogConfirmBrush is SolidColorBrush b)
                    box.Background = new SolidColorBrush(Color.FromRgb((byte)(b.Color.R * shade), (byte)(b.Color.G * shade), (byte)(b.Color.B * shade)));
            }
            else
            {
                box.SetResourceReference(Border.BackgroundProperty, themeBrush);
            }
        }

        private void DialogButton_MouseLeave(object sender, MouseEventArgs e)
        {
            var box = (Border)sender;
            if (box == DialogConfirmContainer)
            {
                if (_dialogConfirmBrush != null)
                    box.Background = _dialogConfirmBrush;
            }
            else
            {
                box.SetResourceReference(Border.BackgroundProperty, "SectionBackgroundBrush");
            }
        }

        /// <summary>
        /// The dialog with a page's own controls under the text. The page keeps the confirm
        /// button's state up to date through <see cref="SetDialogConfirmEnabled"/>.
        /// </summary>
        public Task<bool> ShowContentAsync(string title, string message, UIElement content, string confirmText, string cancelText,
            double maxWidth = 640, DialogLevel level = DialogLevel.Normal, System.Action onShown = null)
        {
            // Filled when it is its turn: set right away, it would land in a dialog still open.
            return ConfirmAsync(title, message, confirmText, cancelText, level: level, onShown: () =>
            {
                DialogContent.Content = content;
                DialogContent.Visibility = Visibility.Visible;
                DialogBox.MaxWidth = maxWidth;
                onShown?.Invoke();
            });
        }

        public void SetDialogConfirmEnabled(bool enabled)
        {
            DialogConfirm.IsEnabled = enabled;
            DialogConfirmContainer.Opacity = enabled ? 1 : 0.4;
        }

        public enum DialogChoice { Cancel, Confirm, Alternative }

        /// <summary>
        /// The dialog with a third button between cancel and confirm, for a second way to
        /// go ahead (for example without a backup first).
        /// </summary>
        public Task<DialogChoice> ChooseAsync(string title, string message, string confirmText, string alternativeText,
            string cancelText, bool danger = false, DialogLevel level = DialogLevel.Normal, System.Action onShown = null)
        {
            var request = Enqueue(level, () => FillDialog(title, message, confirmText, cancelText, danger, null, alternativeText, onShown), null, false);
            ShowNextDialog();
            return request.Result.Task;
        }

        // Set by a caller whose third button acts inside the open dialog instead of closing it
        // (choosing a file for the job image), and whose dialog takes Ctrl+V.
        private System.Action _dialogAltAction;
        private System.Action _dialogPaste;
        private System.Action<IDataObject> _dialogDrop;

        private void DialogImageBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = _dialogDrop != null ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void DialogImageBox_Drop(object sender, DragEventArgs e)
        {
            _dialogDrop?.Invoke(e.Data);
            e.Handled = true;
        }

        private void DialogAlt_Click(object sender, RoutedEventArgs e)
        {
            if (_dialogAltAction != null)
            {
                _dialogAltAction();
                return;
            }
            CloseDialog(DialogChoice.Alternative);
        }

        private bool _dialogBusy;

        /// <summary>
        /// The dialog without buttons and with a progress bar, for work the player waits
        /// for (downloading an update). Esc does not close it; <see cref="CancelDialog"/> with
        /// the returned handle does.
        /// </summary>
        public Task ShowBusyDialog(string title, string message, DialogLevel level = DialogLevel.Critical)
        {
            var request = Enqueue(level, () =>
            {
                DialogTitle.Text = title ?? "";
                DialogText.Text = message ?? "";
                DialogText.Visibility = string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;
                SetDialogDetails(null);
                DialogImageBox.Visibility = Visibility.Collapsed;
                DialogButtons.Visibility = Visibility.Collapsed;
                DialogProgress.Value = 0;
                DialogProgress.IsIndeterminate = true;
                DialogProgress.Visibility = Visibility.Visible;
            }, null, true);
            ShowNextDialog();
            return request.Handle;
        }

        /// <summary>Progress from 0 to 1 for the busy dialog.</summary>
        public void SetDialogProgress(double value)
        {
            if (_dialogCurrent == null || !_dialogCurrent.Busy)
                return;
            DialogProgress.IsIndeterminate = false;
            DialogProgress.Value = value;
        }

        private void SetDialogDetails(IList<ChangelogText.Line> details)
        {
            DialogDetails.Inlines.Clear();
            if (details == null || details.Count == 0)
            {
                DialogDetailsScroll.Visibility = Visibility.Collapsed;
                return;
            }
            for (int i = 0; i < details.Count; i++)
            {
                if (i > 0)
                    DialogDetails.Inlines.Add(new LineBreak());
                var run = new Run(details[i].Text);
                if (details[i].Heading)
                    run.FontWeight = FontWeights.Bold;
                DialogDetails.Inlines.Add(run);
            }
            DialogDetailsScroll.ScrollToTop();
            DialogDetailsScroll.Visibility = Visibility.Visible;
        }

        private void CloseDialog(DialogChoice choice)
        {
            var closing = _dialogCurrent;
            _dialogCurrent = null;
            _dialogBusy = false;
            _dialogAltAction = null;
            _dialogPaste = null;
            _dialogDrop = null;
            ShowNextDialog();
            closing?.Result.TrySetResult(choice);
        }

        private void DialogConfirm_Click(object sender, RoutedEventArgs e) => CloseDialog(DialogChoice.Confirm);

        private void DialogCancel_Click(object sender, RoutedEventArgs e) => CloseDialog(DialogChoice.Cancel);

        private void DialogOverlay_KeyDown(object sender, KeyEventArgs e)
        {
            if (_dialogBusy)
                return;
            if (e.Key == Key.Escape)
            {
                CloseDialog(DialogChoice.Cancel);
                e.Handled = true;
            }
            else if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control && _dialogPaste != null)
            {
                _dialogPaste();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && DialogConfirm.IsEnabled)
            {
                CloseDialog(DialogChoice.Confirm);
                e.Handled = true;
            }
        }
    }
}
