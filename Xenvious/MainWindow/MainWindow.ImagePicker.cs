using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Xenvious.Logging;

namespace Xenvious
{
    // Part of MainWindow: the image dialog for presets (screenshot of the game, paste, drop) and the race test calls of the test vehicle page.
    public partial class MainWindow
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint { public int X, Y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect { public int Left, Top, Right, Bottom; }

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out NativeRect rect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref NativePoint point);

        /// <summary>
        /// The image dialog of the job image, for an optional picture: Ctrl+V or a dropped file,
        /// or the third button takes a screenshot of the game. Returns false when cancelled;
        /// the image is null when the user went on without one.
        /// </summary>
        public async Task<(bool Ok, BitmapSource Image)> PickImageAsync(string title, string message, string confirmText)
        {
            BitmapSource picked = null;
            void Show(BitmapSource image)
            {
                if (image == null)
                    return;
                picked = image;
                DialogImage.Source = image;
                DialogImageHint.Visibility = Visibility.Collapsed;
            }

            var closed = ChooseAsync(title, message, confirmText, TranslateOr("img_screenshot", "Screenshot from the game"),
                TranslateOr("dialog_cancel", "Cancel"));
            DialogImage.Source = null;
            DialogImageHint.Visibility = Visibility.Visible;
            DialogImageBox.Visibility = Visibility.Visible;
            DialogConfirm.IsEnabled = true;
            _dialogPaste = () => Show(ImageFrom(Clipboard.GetDataObject()));
            _dialogDrop = data => Show(ImageFrom(data));
            _dialogAltAction = async () => Show(await GameScreenshotAsync());

            if (await closed != DialogChoice.Confirm)
                return (false, null);
            return (true, picked);
        }

        /// <summary>
        /// The game window's picture. A DirectX window only shows its picture on screen, so the
        /// game comes to the front for a moment, then Xenvious again (the user asked for it here).
        /// </summary>
        private async Task<BitmapSource> GameScreenshotAsync()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName(GameVariant.ProcessName))
                {
                    IntPtr window = process.MainWindowHandle;
                    if (window == IntPtr.Zero || !GetClientRect(window, out var rect))
                        continue;
                    var origin = new NativePoint();
                    ClientToScreen(window, ref origin);
                    int width = rect.Right - rect.Left, height = rect.Bottom - rect.Top;
                    if (width <= 0 || height <= 0)
                        return null;
                    FocusGame();
                    // The game needs a few frames to draw again after it got the focus back.
                    await Task.Delay(600);
                    BitmapSource shot;
                    using (var bitmap = new System.Drawing.Bitmap(width, height))
                    {
                        using (var g = System.Drawing.Graphics.FromImage(bitmap))
                            g.CopyFromScreen(origin.X, origin.Y, 0, 0, new System.Drawing.Size(width, height));
                        IntPtr handle = bitmap.GetHbitmap();
                        try
                        {
                            shot = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            shot.Freeze();
                        }
                        finally
                        {
                            DeleteObject(handle);
                        }
                    }
                    Activate();
                    return shot;
                }
            }
            catch (Exception ex)
            {
                Log.Debug("screenshot: " + ex.Message, source: "image");
            }
            return null;
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr handle);

        /// <summary>Starts the race creator's test like its menu does (primary or secondary checkpoints).</summary>
        public void StartRaceTest(bool secondary)
        {
            if (!m.IsProcOpen)
                return;
            if (curcreatorscanneeded())
                GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy();
            var ptr = GTA.Offsets.Editor.localptr;
            if (ptr == null || GTA.ReadScriptName(ptr[0], ptr[1]) != "fm_race_creator")
                return;
            if (secondary)
                testJobRaceSecondary();
            else
                testJobRace();
        }

        public bool RaceTestActive => m.IsProcOpen && RaceTestRunning();

        public void EndRaceTestFromPage()
        {
            if (RaceTestActive)
                EndRaceTest();
        }
    }
}
