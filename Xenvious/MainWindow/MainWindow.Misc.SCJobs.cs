using System;
using System.Windows;
using System.Windows.Input;

namespace Xenvious
{
    // Part of MainWindow: Misc / SCJobs page.
    public partial class MainWindow
    {
        private void tbCopyJobLink_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(full_link))
                Clipboard.SetText(full_link);
        }

        bool gettingscuserjobsinfo = false;
        private void tbModSCJobsUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (gettingscuserjobsinfo)
                    return;


            }
        }
    }
}
