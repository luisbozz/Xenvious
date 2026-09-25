using System.ComponentModel;
using System.Windows.Controls;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Misc / JobLinks page.
    public partial class MainWindow
    {
        private void tbjoblink1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((1 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink1.Text);
        }
        private void tbjoblink2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((2 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink2.Text);
        }
        private void tbjoblink3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((3 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink3.Text);
        }
        private void tbjoblink4_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((4 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink4.Text);
        }
        private void tbjoblink5_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((5 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink5.Text);
        }
        private void tbjoblink6_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((6 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink6.Text);
        }
        private void tbjoblink7_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((7 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink7.Text);
        }
        private void tbjoblink8_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((8 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink8.Text);
        }
        private void tbjoblink9_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((9 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink9.Text);
        }
        private void tbjoblink10_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((10 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink10.Text);
        }
        private void tbjoblink11_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((11 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink11.Text);
        }
        private void tbjoblink12_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((12 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink12.Text);
        }
        private void tbjoblink13_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((13 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink13.Text);
        }
        private void tbjoblink14_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((14 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink14.Text);
        }
        private void tbjoblink15_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((15 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink15.Text);
        }
        private void tbjoblink16_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((16 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink16.Text);
        }
        private void tbjoblink17_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((17 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink17.Text);
        }
        private void tbjoblink18_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((18 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink18.Text);
        }
        private void tbjoblink19_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((19 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink19.Text);
        }
        private void tbjoblink20_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((20 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink20.Text);
        }
        private void tbjoblink21_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((21 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink21.Text);
        }
        private void tbjoblink22_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((22 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink22.Text);
        }
        private void tbjoblink23_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((23 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink23.Text);
        }
        private void tbjoblink24_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((24 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink24.Text);
        }
        private void tbjoblink25_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((25 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink25.Text);
        }
        private void tbjoblink26_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((26 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink26.Text);
        }
        private void tbjoblink27_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((27 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink27.Text);
        }
        private void tbjoblink28_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((28 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink28.Text);
        }
        private void tbjoblink29_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((29 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink29.Text);
        }
        private void tbjoblink30_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((30 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).SetString(tbjoblink30.Text);
        }

        private void ddjoblinks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                getjoblinks();
            }
        }

        public void getjoblinks()
        {
            //get job link and set text
            tbjoblink1.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((1 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink2.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((2 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink3.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((3 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink4.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((4 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink5.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((5 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink6.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((6 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink7.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((7 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink8.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((8 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink9.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((9 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink10.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((10 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink11.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((11 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink12.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((12 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink13.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((13 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink14.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((14 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink15.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((15 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink16.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((16 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink17.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((17 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink18.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((18 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink19.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((19 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink20.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((20 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink21.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((21 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink22.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((22 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink23.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((23 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink24.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((24 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink25.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((25 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink26.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((26 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink27.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((27 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink28.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((28 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink29.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((29 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);
            tbjoblink30.Text = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_link : GTA.Offsets.Editor.Jobs.saved_link) + ((30 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(22);

            //get job name and set tooltip
            tbjoblink1.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((1 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink2.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((2 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink3.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((3 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink4.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((4 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink5.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((5 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink6.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((6 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink7.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((7 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink8.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((8 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink9.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((9 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink10.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((10 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink11.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((11 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink12.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((12 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink13.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((13 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink14.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((14 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink15.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((15 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink16.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((16 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink17.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((17 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink18.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((18 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink19.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((19 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink20.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((20 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink21.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((21 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink22.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((22 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink23.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((23 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink24.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((24 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink25.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((25 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink26.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((26 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink27.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((27 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink28.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((28 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink29.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((29 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
            tbjoblink30.ToolTip = new Global((ddjoblinks.SelectedIndex == 0 ? GTA.Offsets.Editor.Jobs.published_name : GTA.Offsets.Editor.Jobs.saved_name) + ((30 - 1) * GTA.Offsets.Editor.Jobs.jobs_NEXT)).GetString(80);
        }
    }
}
