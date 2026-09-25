using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Doors page.
    public partial class MainWindow
    {
        private void BtndoorsAdd_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int num = new Global(GTA.Offsets.Editor.Doors.number).Get<int>();
                if (num < 32 && num > -1)
                {
                    int new_index = num + 1;
                    new Global(GTA.Offsets.Editor.Doors.number).SetInt(new_index);
                    dddoorsno.SelectedIndex = new_index - 1;
                }
            }
        }

        private void BtndoorsDelete_Click(object sender, RoutedEventArgs e)
        {

            if (m.IsProcOpen)
            {
                int index = dddoorsno.SelectedIndex;
                int num = new Global(GTA.Offsets.Editor.Doors.number).Get<int>();

                if (index > -1)
                {
                    try
                    {

                        List<List<int>> valuesafterdeleteddoor = new List<List<int>>();

                        // get doors after deleted doors
                        for (int i = index; i < num - 1; i++)
                        {
                            List<int> temp = new List<int>();
                            for (int d = 0; d < GTA.Offsets.Editor.Doors.NEXT; d++)
                            {
                                int test = new Global((GTA.Offsets.Editor.Doors.loc + ((i + 1) * GTA.Offsets.Editor.Doors.NEXT) + d)).Get<int>();
                                temp.Add(test);
                            }

                            valuesafterdeleteddoor.Add(temp);
                        }

                        // lower doors number
                        if (num > 0)
                        {
                            new Global(GTA.Offsets.Editor.Doors.number).SetInt(num - 1);
                        }

                        for (int i = 0; i < valuesafterdeleteddoor.Count(); i++)
                        {
                            for (int d = 0; d < valuesafterdeleteddoor[i].Count(); d++)
                            {
                                long Doorsbase = GTA.Offsets.Editor.Doors.loc + ((index + i) * GTA.Offsets.Editor.Doors.NEXT);
                                new Global(Doorsbase + d).SetInt(valuesafterdeleteddoor[i][d]);
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        private void dddoorsno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetDoorsValues(true);
        }

        public void GetDoorsValues(bool ignore_focus = false)
        {
            int index = dddoorsno.SelectedIndex;

            tbdoorsmodel.IsEnabled = index < 0 ? false : true;
            tbdoorsbits.IsEnabled = index < 0 ? false : true;
            tbdoorsfopen.IsEnabled = index < 0 ? false : true;
            tbdoorsudrle.IsEnabled = index < 0 ? false : true;
            tbdoorsudtem.IsEnabled = index < 0 ? false : true;
            tbdoorsudrat.IsEnabled = index < 0 ? false : true;
            tbdoorslocx.IsEnabled = index < 0 ? false : true;
            tbdoorslocy.IsEnabled = index < 0 ? false : true;
            tbdoorslocz.IsEnabled = index < 0 ? false : true;
            Btndoorsgetloc.IsEnabled = index < 0 ? false : true;
            cbdoorsswing.IsEnabled = index < 0 ? false : true;
            cbdoorsswingu.IsEnabled = index < 0 ? false : true;
            cbdoorsmid.IsEnabled = index < 0 ? false : true;
            cbdoorslock.IsEnabled = index < 0 ? false : true;
            tbdoorsfcz.IsEnabled = index < 0 ? false : true;
            tbdoorsfoz.IsEnabled = index < 0 ? false : true;

            tbdoorsaurt.IsEnabled = index < 0 ? false : true;
            tbdoorsaudst.IsEnabled = index < 0 ? false : true;
            tbdoorsdird.IsEnabled = index < 0 ? false : true;
            tbdoorsdirdvx.IsEnabled = index < 0 ? false : true;
            tbdoorsdirdvy.IsEnabled = index < 0 ? false : true;
            tbdoorsdirdvz.IsEnabled = index < 0 ? false : true;
            Btndoorsdirdvgetloc.IsEnabled = index < 0 ? false : true;

            tbdoorsuaurt.IsEnabled = index < 0 ? false : true;
            tbdoorsuaudst.IsEnabled = index < 0 ? false : true;
            tbdoorsdirud.IsEnabled = index < 0 ? false : true;
            tbdoorsdirduvx.IsEnabled = index < 0 ? false : true;
            tbdoorsdirduvy.IsEnabled = index < 0 ? false : true;
            tbdoorsdirduvz.IsEnabled = index < 0 ? false : true;
            Btndoorsdirduvgetloc.IsEnabled = index < 0 ? false : true;

            tbdoorsorg.IsEnabled = index < 0 ? false : true;
            tbdoorsdcoid.IsEnabled = index < 0 ? false : true;
            tbdoorsdtime.IsEnabled = index < 0 ? false : true;
            cbdoorslfp.IsEnabled = index < 0 ? false : true;
            tbdoorsornlo.IsEnabled = index < 0 ? false : true;
            tbdoorsdle.IsEnabled = index < 0 ? false : true;

            if (m.IsProcOpen && index > -1)
            {
                if (!tbdoorsmodel.IsFocused || ignore_focus) tbdoorsmodel.Text = new Global(GTA.Offsets.Editor.Doors.model + GTA.Offsets.Editor.Doors.NEXT * index).Get<int>().ToString();
                if (!tbdoorsbits.IsFocused || ignore_focus) tbdoorsbits.Text = new Global(GTA.Offsets.Editor.Doors.dbs + GTA.Offsets.Editor.Doors.NEXT * index).Get<int>().ToString();
                if (!tbdoorsfopen.IsFocused || ignore_focus) tbdoorsfopen.Text = new Global(GTA.Offsets.Editor.Doors.fopen + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                if (!tbdoorsudrle.IsFocused || ignore_focus) tbdoorsudrle.Text = new Global(GTA.Offsets.Editor.Doors.udrle + GTA.Offsets.Editor.Doors.NEXT * index).Get<int>().ToString();
                if (!tbdoorsudtem.IsFocused || ignore_focus) tbdoorsudtem.Text = new Global(GTA.Offsets.Editor.Doors.udtem + GTA.Offsets.Editor.Doors.NEXT * index).Get<int>().ToString();
                if (!tbdoorsudrat.IsFocused || ignore_focus) tbdoorsudrat.Text = new Global(GTA.Offsets.Editor.Doors.udrat + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                if (!tbdoorslocx.IsFocused || ignore_focus) tbdoorslocx.Text = new Global(GTA.Offsets.Editor.Doors.loc + 0 + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                if (!tbdoorslocy.IsFocused || ignore_focus) tbdoorslocy.Text = new Global(GTA.Offsets.Editor.Doors.loc + 1 + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                if (!tbdoorslocz.IsFocused || ignore_focus) tbdoorslocz.Text = new Global(GTA.Offsets.Editor.Doors.loc + 2 + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                if (!tbdoorsfcz.IsFocused || ignore_focus) tbdoorsfcz.Text = new Global(GTA.Offsets.Editor.Doors.fcz + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                if (!tbdoorsfoz.IsFocused || ignore_focus) tbdoorsfoz.Text = new Global(GTA.Offsets.Editor.Doors.foz + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                Functions.Read.checkbinary(1, GTA.Offsets.Editor.Doors._lock + GTA.Offsets.Editor.Doors.NEXT * index, cbdoorslock);
                Functions.Read.checkbinary(1, GTA.Offsets.Editor.Doors.swing + GTA.Offsets.Editor.Doors.NEXT * index, cbdoorsswing);
                Functions.Read.checkbinary(1, GTA.Offsets.Editor.Doors.swingu + GTA.Offsets.Editor.Doors.NEXT * index, cbdoorsswingu);
                Functions.Read.checkbinary(1, GTA.Offsets.Editor.Doors.mid + GTA.Offsets.Editor.Doors.NEXT * index, cbdoorsmid);

                if (!tbdoorsaurt.IsFocused || ignore_focus) tbdoorsaurt.Text = new Global(GTA.Offsets.Editor.Doors.aurt + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                if (!tbdoorsaudst.IsFocused || ignore_focus) tbdoorsaudst.Text = new Global(GTA.Offsets.Editor.Doors.audst + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                if (!tbdoorsdird.IsFocused || ignore_focus) tbdoorsdird.Text = new Global(GTA.Offsets.Editor.Doors.dird + GTA.Offsets.Editor.Doors.NEXT * index).Get<int>().ToString();
                if (!tbdoorsdirdvx.IsFocused || ignore_focus) tbdoorsdirdvx.Text = new Global(GTA.Offsets.Editor.Doors.dirdv + 0 + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                if (!tbdoorsdirdvy.IsFocused || ignore_focus) tbdoorsdirdvy.Text = new Global(GTA.Offsets.Editor.Doors.dirdv + 1 + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                if (!tbdoorsdirdvz.IsFocused || ignore_focus) tbdoorsdirdvz.Text = new Global(GTA.Offsets.Editor.Doors.dirdv + 2 + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();

                if (!tbdoorsuaurt.IsFocused || ignore_focus) tbdoorsuaurt.Text = new Global(GTA.Offsets.Editor.Doors.uaurt + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                if (!tbdoorsuaudst.IsFocused || ignore_focus) tbdoorsuaudst.Text = new Global(GTA.Offsets.Editor.Doors.uaudst + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                if (!tbdoorsdirud.IsFocused || ignore_focus) tbdoorsdirud.Text = new Global(GTA.Offsets.Editor.Doors.dirud + GTA.Offsets.Editor.Doors.NEXT * index).Get<int>().ToString();
                if (!tbdoorsdirduvx.IsFocused || ignore_focus) tbdoorsdirduvx.Text = new Global(GTA.Offsets.Editor.Doors.dirduv + 0 + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                if (!tbdoorsdirduvy.IsFocused || ignore_focus) tbdoorsdirduvy.Text = new Global(GTA.Offsets.Editor.Doors.dirduv + 1 + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                if (!tbdoorsdirduvz.IsFocused || ignore_focus) tbdoorsdirduvz.Text = new Global(GTA.Offsets.Editor.Doors.dirduv + 2 + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();

                if (!tbdoorsorg.IsFocused || ignore_focus) tbdoorsorg.Text = new Global(GTA.Offsets.Editor.Doors.org + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                if (!tbdoorsdcoid.IsFocused || ignore_focus) tbdoorsdcoid.Text = new Global(GTA.Offsets.Editor.Doors.dcoid + GTA.Offsets.Editor.Doors.NEXT * index).Get<int>().ToString();
                if (!tbdoorsdtime.IsFocused || ignore_focus) tbdoorsdtime.Text = new Global(GTA.Offsets.Editor.Doors.dtime + GTA.Offsets.Editor.Doors.NEXT * index).Get<int>().ToString();
                Functions.Read.checkbinary(1, GTA.Offsets.Editor.Doors.lfp + GTA.Offsets.Editor.Doors.NEXT * index, cbdoorslfp);
                if (!tbdoorsornlo.IsFocused || ignore_focus) tbdoorsornlo.Text = new Global(GTA.Offsets.Editor.Doors.ORNLO + GTA.Offsets.Editor.Doors.NEXT * index).Get<float>().ToString();
                if (!tbdoorsdle.IsFocused || ignore_focus) tbdoorsdle.Text = new Global(GTA.Offsets.Editor.Doors.dle + GTA.Offsets.Editor.Doors.NEXT * index).Get<int>().ToString();
            }
        }

        private void tbdoorsmodel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbdoorsmodel.Text, true))
                new Global(GTA.Offsets.Editor.Doors.model + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetInt(tbdoorsmodel.Text);
        }

        private void tbdoorsfopen_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.fopen + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorsfopen.Text);
        }

        private void tbdoorsbits_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbdoorsbits.Text, true))
                new Global(GTA.Offsets.Editor.Doors.dbs + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetInt(tbdoorsbits.Text);
        }

        private void tbdoorslocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.loc + 0 + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorslocx.Text);
        }

        private void tbdoorslocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.loc + 1 + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorslocy.Text);
        }

        private void tbdoorslocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.loc + 2 + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorslocz.Text);
        }

        private void Btndoorsgetloc_Click(object sender, RoutedEventArgs e)
        {

            var loc = Functions.Read.getlocation();

            tbdoorslocx.Text = loc[0];
            tbdoorslocy.Text = loc[1];
            tbdoorslocz.Text = loc[2];
            creatorRefresh();
        }

        private void cbdoorsswing_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.Doors.swing + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex, cbdoorsswing);
        }

        private void cbdoorslock_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.Doors._lock + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex, cbdoorslock);
        }

        private void cbdoorsswingu_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.Doors.swingu + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex, cbdoorsswingu);
        }

        private void tbdoorsudrat_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.udrat + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorsudrat.Text);
        }

        private void tbdoorsudrle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbdoorsudrle.Text, true))
                new Global(GTA.Offsets.Editor.Doors.udrle + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetInt(tbdoorsudrle.Text);
        }

        private void tbdoorsudtem_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbdoorsudtem.Text, true))
                new Global(GTA.Offsets.Editor.Doors.udtem + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetInt(tbdoorsudtem.Text);
        }

        private void tbdoorsfcz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.fcz + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorsfcz.Text);
        }

        private void tbdoorsfoz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.foz + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorsfoz.Text);
        }

        private void cbdoorsmid_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.Doors.mid + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex, cbdoorsmid);
        }

        private void tbdoorsaurt_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.aurt + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorsaurt.Text);
        }

        private void tbdoorsaudst_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.audst + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorsaudst.Text);
        }

        private void tbdoorsdird_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbdoorsdird.Text, true))
                new Global(GTA.Offsets.Editor.Doors.dird + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetInt(tbdoorsdird.Text);
        }

        private void tbdoorsdirdvx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.dirdv + 0 + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorsdirdvx.Text);
        }

        private void tbdoorsdirdvy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.dirdv + 1 + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorsdirdvy.Text);
        }

        private void tbdoorsdirdvz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.dirdv + 2 + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorsdirdvz.Text);
        }

        private void Btndoorsdirdvgetloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.getlocation();

            tbdoorsdirdvx.Text = loc[0];
            tbdoorsdirdvy.Text = loc[1];
            tbdoorsdirdvz.Text = loc[2];
        }

        private void tbdoorsuaurt_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.uaurt + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorsuaurt.Text);
        }

        private void tbdoorsuaudst_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.uaudst + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorsuaudst.Text);
        }

        private void tbdoorsdirud_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbdoorsdirud.Text, true))
                new Global(GTA.Offsets.Editor.Doors.dirud + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetInt(tbdoorsdirud.Text);
        }

        private void tbdoorsdirduvx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.dirduv + 0 + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorsdirduvx.Text);
        }

        private void tbdoorsdirduvy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.dirduv + 1 + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorsdirduvy.Text);
        }

        private void tbdoorsdirduvz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.dirduv + 2 + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorsdirduvz.Text);
        }

        private void Btndoorsdirduvgetloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.getlocation();

            tbdoorsdirduvx.Text = loc[0];
            tbdoorsdirduvy.Text = loc[1];
            tbdoorsdirduvz.Text = loc[2];
        }

        private void tbdoorsdle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbdoorsdle.Text, true))
                new Global(GTA.Offsets.Editor.Doors.dle + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetInt(tbdoorsdle.Text);
        }

        private void tbdoorsornlo_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.ORNLO + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorsornlo.Text);
        }

        private void tbdoorsdtime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbdoorsdtime.Text, true))
                new Global(GTA.Offsets.Editor.Doors.dtime + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetInt(tbdoorsdtime.Text);
        }

        private void tbdoorsdcoid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbdoorsdcoid.Text, true))
                new Global(GTA.Offsets.Editor.Doors.dcoid + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetInt(tbdoorsdcoid.Text);
        }

        private void tbdoorsorg_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Doors.org + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex).SetFloat(tbdoorsorg.Text);
        }

        private void cbdoorslfp_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.Doors.lfp + GTA.Offsets.Editor.Doors.NEXT * dddoorsno.SelectedIndex, cbdoorslfp);
        }
    }
}
