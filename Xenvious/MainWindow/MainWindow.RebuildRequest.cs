using System;
using System.IO;
using System.Windows;
using Xenvious.Logging;

namespace Xenvious
{
    // Part of MainWindow: asks to close Xenvious when a rebuild is waiting (the build locks on a running exe).
    public partial class MainWindow
    {
        // Written by the WSL build script (xrebuild); "close" or "later" is the answer it waits for.
        private static readonly string RebuildRequestFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Xenvious", "rebuild.request");
        private static readonly string RebuildAnswerFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Xenvious", "rebuild.answer");

        private FileSystemWatcher _rebuildWatcher;
        private bool _rebuildAsking;

        private void WatchRebuildRequests()
        {
            try
            {
                string folder = Path.GetDirectoryName(RebuildRequestFile);
                Directory.CreateDirectory(folder);
                File.Delete(RebuildRequestFile);
                _rebuildWatcher = new FileSystemWatcher(folder, Path.GetFileName(RebuildRequestFile))
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                    EnableRaisingEvents = true,
                };
                _rebuildWatcher.Created += (_, __) => Dispatcher.BeginInvoke(new Action(AskForRebuild));
                _rebuildWatcher.Changed += (_, __) => Dispatcher.BeginInvoke(new Action(AskForRebuild));
            }
            catch (Exception ex)
            {
                Log.Debug($"rebuild watcher: {ex.Message}", source: "startup");
            }
        }

        private async void AskForRebuild()
        {
            if (_rebuildAsking || !File.Exists(RebuildRequestFile))
                return;
            _rebuildAsking = true;
            try
            {
                // The user asked for this one exception to "never steal focus": a rebuild waits on
                // the answer, so Xenvious comes to the front.
                if (WindowState == WindowState.Minimized)
                    WindowState = WindowState.Normal;
                Topmost = true;
                Activate();
                Topmost = false;
                bool close = await ConfirmAsync(TranslateOr("rebuild_title", "New build"),
                    TranslateOr("rebuild_msg", "A rebuild is waiting. Close Xenvious now? Start the new version afterwards."),
                    TranslateOr("rebuild_close", "Close"), TranslateOr("rebuild_later", "Later"));
                try
                {
                    File.Delete(RebuildRequestFile);
                    File.WriteAllText(RebuildAnswerFile, close ? "close" : "later");
                }
                catch (IOException)
                {
                    // The script times out on its own if the answer cannot be written.
                }
                if (close)
                    Close();
            }
            finally
            {
                _rebuildAsking = false;
            }
        }
    }
}
