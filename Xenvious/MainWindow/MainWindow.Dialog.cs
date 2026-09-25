using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Xenvious
{
    /// <summary>
    /// A confirmation dialog inside the Xenvious window instead of a Windows message box:
    /// the window behind is dimmed and blurred, Enter confirms, Esc cancels.
    /// </summary>
    public partial class MainWindow
    {
        private TaskCompletionSource<bool> _dialogResult;

        /// <summary>
        /// Shows the dialog and returns true when confirmed. <paramref name="cancelText"/>
        /// null shows only the confirm button.
        /// </summary>
        public Task<bool> ConfirmAsync(string title, string message, string confirmText, string cancelText, bool danger = false,
            IList<ChangelogText.Line> details = null)
        {
            _dialogResult?.TrySetResult(false);
            _dialogResult = new TaskCompletionSource<bool>();
            _dialogBusy = false;
            _dialogAlternative = false;
            DialogAltContainer.Visibility = Visibility.Collapsed;
            DialogButtons.Visibility = Visibility.Visible;
            DialogProgress.Visibility = Visibility.Collapsed;
            SetDialogDetails(details);

            DialogTitle.Text = title ?? "";
            DialogText.Text = message ?? "";
            DialogText.Visibility = string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;
            DialogConfirm.Content = confirmText;
            DialogConfirmContainer.Background = danger
                ? new SolidColorBrush(Color.FromRgb(0xD9, 0x53, 0x4F))
                : new SolidColorBrush(Color.FromRgb(0xFA, 0xC8, 0x28));
            DialogConfirm.Foreground = danger ? Brushes.White : new SolidColorBrush(Color.FromRgb(0x20, 0x22, 0x25));
            DialogCancel.Content = cancelText ?? "";
            DialogCancelContainer.Visibility = cancelText == null ? Visibility.Collapsed : Visibility.Visible;

            Main.Effect = new BlurEffect { Radius = 8, KernelType = KernelType.Gaussian };
            DialogOverlay.Visibility = Visibility.Visible;
            DialogOverlay.Focus();
            Keyboard.Focus(DialogOverlay);
            return _dialogResult.Task;
        }

        public enum DialogChoice { Cancel, Confirm, Alternative }

        private bool _dialogAlternative;

        /// <summary>
        /// The dialog with a third button between cancel and confirm, for a second way to
        /// go ahead (for example without a backup first).
        /// </summary>
        public async Task<DialogChoice> ChooseAsync(string title, string message, string confirmText, string alternativeText,
            string cancelText, bool danger = false)
        {
            var closed = ConfirmAsync(title, message, confirmText, cancelText, danger);
            DialogAlt.Content = alternativeText;
            DialogAltContainer.Visibility = Visibility.Visible;
            bool confirmed = await closed;
            return _dialogAlternative ? DialogChoice.Alternative : confirmed ? DialogChoice.Confirm : DialogChoice.Cancel;
        }

        private void DialogAlt_Click(object sender, RoutedEventArgs e)
        {
            _dialogAlternative = true;
            CloseDialog(false);
        }

        private bool _dialogBusy;

        /// <summary>
        /// The dialog without buttons and with a progress bar, for work the player waits
        /// for (downloading an update). Esc does not close it; <see cref="CloseDialog"/> does.
        /// </summary>
        public void ShowBusyDialog(string title, string message)
        {
            _dialogResult?.TrySetResult(false);
            _dialogResult = null;
            _dialogBusy = true;
            DialogTitle.Text = title ?? "";
            DialogText.Text = message ?? "";
            DialogText.Visibility = string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;
            SetDialogDetails(null);
            DialogButtons.Visibility = Visibility.Collapsed;
            DialogProgress.Value = 0;
            DialogProgress.IsIndeterminate = true;
            DialogProgress.Visibility = Visibility.Visible;
            Main.Effect = new BlurEffect { Radius = 8, KernelType = KernelType.Gaussian };
            DialogOverlay.Visibility = Visibility.Visible;
        }

        /// <summary>Progress from 0 to 1 for the busy dialog.</summary>
        public void SetDialogProgress(double value)
        {
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

        private void CloseDialog(bool result)
        {
            _dialogBusy = false;
            DialogOverlay.Visibility = Visibility.Collapsed;
            Main.Effect = null;
            var pending = _dialogResult;
            _dialogResult = null;
            pending?.TrySetResult(result);
        }

        private void DialogConfirm_Click(object sender, RoutedEventArgs e) => CloseDialog(true);

        private void DialogCancel_Click(object sender, RoutedEventArgs e) => CloseDialog(false);

        private void DialogOverlay_KeyDown(object sender, KeyEventArgs e)
        {
            if (_dialogBusy)
                return;
            if (e.Key == Key.Escape)
            {
                CloseDialog(false);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                CloseDialog(true);
                e.Handled = true;
            }
        }
    }
}
