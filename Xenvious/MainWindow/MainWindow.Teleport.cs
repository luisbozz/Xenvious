using System;
using System.ComponentModel;
using System.Windows;
using Newtonsoft.Json.Linq;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Teleport page.
    public partial class MainWindow
    {
        private void Btnteleportgetlocation_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var loc = Functions.Read.getlocation();

                tbteleportlocx.Text = loc[0];
                tbteleportlocy.Text = loc[1];
                tbteleportlocz.Text = loc[2];
            }
        }

        private void Btnteleportsetlocation_Click(object sender, RoutedEventArgs e)
        {
            GTA.Teleport(new XenVector3(Convert.ToSingle(tbteleportlocx.Text), Convert.ToSingle(tbteleportlocy.Text), Convert.ToSingle(tbteleportlocz.Text)));
        }

        private void Btnteleportgetplayerlocation_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var loc = GTA.GetLocation();

                tbteleportlocx.Text = loc[0].ToString();
                tbteleportlocy.Text = loc[1].ToString();
                tbteleportlocz.Text = loc[2].ToString();
            }
        }

        private void Btnteleportgetnextcplocation_Click(object sender, RoutedEventArgs e)
        {
            XenVector3 loc = new XenVector3(
                m.memory(GTA.Offsets.Editor.nextcp + 0 * 0x4).Get<float>(),
                m.memory(GTA.Offsets.Editor.nextcp + 1 * 0x4).Get<float>(),
                m.memory(GTA.Offsets.Editor.nextcp + 2 * 0x4).Get<float>());

            tbteleportlocx.Text = loc.X.ToString();
            tbteleportlocy.Text = loc.Y.ToString();
            tbteleportlocz.Text = loc.Z.ToString();
        }

        private void Btnteleportnextcplocation_Click(object sender, RoutedEventArgs e)
        {
            GTA.Teleport(new XenVector3(
                m.memory(GTA.Offsets.Editor.nextcp + 0 * 0x4).Get<float>(),
                m.memory(GTA.Offsets.Editor.nextcp + 1 * 0x4).Get<float>(),
                m.memory(GTA.Offsets.Editor.nextcp + 2 * 0x4).Get<float>() - 3));
        }
    }
}
