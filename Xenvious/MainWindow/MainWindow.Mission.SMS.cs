using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xenvious.JSON;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Mission / SMS page.
    public partial class MainWindow
    {
        private void ddsmsno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetSMSValues(true);
            SelectActiveTextBox();
        }

        public void GetSMSValues(bool ignore_focus = false)
        {
            int index = ddsmsno.SelectedIndex;

            ddSMSteam.IsEnabled = index < 0 ? false : true;
            ddSMStrigger.IsEnabled = index < 0 ? false : true;
            tbSMSdelay.IsEnabled = index < 0 ? false : true;
            tbSMSrule.IsEnabled = index < 0 ? false : true;
            tbSMSpreq.IsEnabled = index < 0 ? false : true;
            tbSMSsmsei.IsEnabled = index < 0 ? false : true;
            tbSMSsmstl.IsEnabled = index < 0 ? false : true;
            tbSMSsmspwo.IsEnabled = index < 0 ? false : true;
            cbSMSsndall.IsEnabled = index < 0 ? false : true;
            tbSMSmsg.IsEnabled = index < 0 ? false : true;

            if (m.IsProcOpen && index > -1)
            {
                if (!tbSMSmsg.IsFocused || ignore_focus)
                {
                    string temptxt = new Global(GTA.Offsets.Editor.SMS.txt + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex).GetString();
                    temptxt += new Global(GTA.Offsets.Editor.SMS.txt + GTA.Offsets.Editor.txt_NEXT + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex).GetString();
                    tbSMSmsg.Text = temptxt;
                }
                if (!tbSMSdelay.IsFocused || ignore_focus) tbSMSdelay.Text = new Global((GTA.Offsets.Editor.SMS.delay + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).Get<int>().ToString();
                if (!tbSMSrule.IsFocused || ignore_focus) tbSMSrule.Text = new Global((GTA.Offsets.Editor.SMS.rule + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).Get<int>().ToString();
                if (!tbSMSpreq.IsFocused || ignore_focus) tbSMSpreq.Text = new Global((GTA.Offsets.Editor.SMS.ptsreq + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).Get<int>().ToString();
                if (!ddSMStrigger.IsFocused || ignore_focus) ddSMStrigger.SelectedIndex = new Global((GTA.Offsets.Editor.SMS.time + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).Get<int>();
                if (!ddSMSteam.IsFocused || ignore_focus) ddSMSteam.SelectedIndex = new Global((GTA.Offsets.Editor.SMS.team + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).Get<int>() + 1;
                if (!tbSMSsmsei.IsFocused || ignore_focus) tbSMSsmsei.Text = new Global((GTA.Offsets.Editor.SMS.ptsreq + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).Get<int>().ToString();
                if (!tbSMSsmstl.IsFocused || ignore_focus) tbSMSsmstl.Text = new Global((GTA.Offsets.Editor.SMS.smstl + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).Get<int>().ToString();
                if (!tbSMSsmspwo.IsFocused || ignore_focus) tbSMSsmspwo.Text = new Global((GTA.Offsets.Editor.SMS.smspwo + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).Get<int>().ToString();
                Functions.Read.checkbinary(0, GTA.Offsets.Editor.SMS.sndall + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex, cbSMSsndall);
            }
        }

        private void tbSMSsmsei_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbSMSsmsei.Text, false))
                new Global((GTA.Offsets.Editor.SMS.smsei + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).SetInt(tbSMSsmsei.Text);
        }

        private void tbSMSsmstl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbSMSsmstl.Text, false))
                new Global((GTA.Offsets.Editor.SMS.smstl + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).SetInt(tbSMSsmstl.Text);
        }

        private void tbSMSsmspwo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbSMSsmspwo.Text, false))
                new Global((GTA.Offsets.Editor.SMS.smspwo + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).SetInt(tbSMSsmspwo.Text);
        }

        private void cbSMSsndall_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(0, GTA.Offsets.Editor.SMS.sndall + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex, cbSMSsndall);
        }

        private void tbSMSmsg_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                if (m == null || !m.IsProcOpen)
                    return;

                const int ChunkSize = 63;      // Nutzbytes pro Segment
                const int ClearSize = ChunkSize + 1; // +1 für Terminator innerhalb Segment
                const int MaxSlots = 2;       // SMS hat in deinem Code 2 Segmente
                int slotStride = (int)GTA.Offsets.Editor.txt_NEXT;

                // Basisadresse der ausgewählten SMS
                var baseAddr = GTA.Offsets.Editor.SMS.txt
                             + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex;

                // Text -> UTF-8
                var bytes = Encoding.UTF8.GetBytes(tbSMSmsg.Text ?? string.Empty);

                // Auf die maximal nutzbare Länge begrenzen (2 * 63 = 126)
                int maxBytes = ChunkSize * MaxSlots;
                int total = Math.Min(bytes.Length, maxBytes);

                // 1) Alles vorab nullen (wichtig für Kürzen/Löschen)
                var zeros = new byte[ClearSize];
                for (int i = 0; i < MaxSlots; i++)
                    new Global(baseAddr + slotStride * i).SetBytes(zeros);

                // Leerer Text? Fertig.
                if (total == 0)
                    return;

                // 2) In 63-Byte-Chunks schreiben
                int slotsNeeded = (total + ChunkSize - 1) / ChunkSize; // ceil(total/63)
                for (int i = 0; i < slotsNeeded; i++)
                {
                    int start = i * ChunkSize;
                    int length = Math.Min(ChunkSize, total - start);
                    var chunk = new byte[length];
                    Buffer.BlockCopy(bytes, start, chunk, 0, length);

                    new Global(baseAddr + slotStride * i).SetBytes(chunk);
                }

                // 3) Null-Terminierung exakt hinter dem letzten Byte
                int lastSlot = slotsNeeded - 1;
                int lastLen = total - lastSlot * ChunkSize;
                new Global(baseAddr + slotStride * lastSlot + lastLen).SetBytes(new byte[] { 0 });
            }
        }

        private void tbSMSmsg_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Encoding.UTF8.GetBytes(tbSMSmsg.Text).Length > 125)
                e.Handled = true;
        }


        private void tbSMSdelay_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbSMSdelay.Text, false))
                new Global((GTA.Offsets.Editor.SMS.delay + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).SetInt(tbSMSdelay.Text);
        }

        private void tbSMSrule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbSMSrule.Text))
                new Global((GTA.Offsets.Editor.SMS.rule + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).SetInt(tbSMSrule.Text);
        }

        private void ddSMSteam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddSMSteam.SelectedIndex > -1)
            {
                new Global((GTA.Offsets.Editor.SMS.sndall + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).SetInt(ddSMSteam.SelectedIndex == 0 ? 1 : 0);
                new Global((GTA.Offsets.Editor.SMS.team + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).SetInt(ddSMSteam.SelectedIndex == 0 ? -1 : ddSMSteam.SelectedIndex - 1);
            }
        }

        private void ddSMStrigger_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.SMS.time + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).SetInt(ddSMStrigger.SelectedIndex > -1 ? ddSMStrigger.SelectedIndex : 0);
        }

        private void cbSMSuc_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(32, GTA.Offsets.Editor.menubs2, cbSMSuc);
        }

        private void cbSMSrt_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(20, GTA.Offsets.Editor.menubs19, cbSMSrt);
        }

        private void ddSMScontact_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void tbSMSpreq_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbSMSpreq.Text, false))
                new Global((GTA.Offsets.Editor.SMS.ptsreq + GTA.Offsets.Editor.SMS.NEXT * ddsmsno.SelectedIndex)).SetInt(tbSMSpreq.Text);
        }
    }
}
