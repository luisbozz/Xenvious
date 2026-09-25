using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Weapons page.
    public partial class MainWindow
    {
        private void BtnWeapAdd_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int weapnum = new Global(GTA.Offsets.Editor.Weapon.number).Get<int>();
                if (weapnum < 60 && weapnum > -1)
                {
                    int new_index = weapnum + 1;

                    new Global(GTA.Offsets.Editor.Weapon.number).SetInt(new_index);

                    ddweapno.SelectedIndex = new_index - 1;
                }
            }
        }

        public void GetWeapons(bool skipcategorymodel = false, bool ignore_focus = false)
        {
            int index = ddweapno.SelectedIndex;

            ddweapmodelswicther.IsEnabled = index < 0 ? false : true;
            ddweapcategory.IsEnabled = index < 0 ? false : true;
            weapModelList.IsEnabled = index < 0 ? false : true;
            Btnweapgetloc.IsEnabled = index < 0 ? false : true;
            tbweapmodel.IsEnabled = index < 0 ? false : true;
            tbweaplocx.IsEnabled = index < 0 ? false : true;
            tbweaplocy.IsEnabled = index < 0 ? false : true;
            tbweaplocz.IsEnabled = index < 0 ? false : true;
            tbweapheading.IsEnabled = index < 0 ? false : true;
            tbweapdmgmlt.IsEnabled = index < 0 ? false : true;
            tbweapbits.IsEnabled = index < 0 ? false : true;
            tbweapvput.IsEnabled = index < 0 ? false : true;
            tbweaprput.IsEnabled = index < 0 ? false : true;
            tbweapvclnrl.IsEnabled = index < 0 ? false : true;
            tbweapvclnr.IsEnabled = index < 0 ? false : true;
            tbweapclip.IsEnabled = index < 0 ? false : true;
            tbweapsub.IsEnabled = index < 0 ? false : true;
            tbweapvclnt.IsEnabled = index < 0 ? false : true;
            tbweapiptnp.IsEnabled = index < 0 ? false : true;
            tbweapwcpm.IsEnabled = index < 0 ? false : true;
            ddweaponteam.IsEnabled = index < 0 ? false : true;
            tbweapvasss.IsEnabled = index < 0 ? false : true;
            tbweapvasso.IsEnabled = index < 0 ? false : true;
            tbweapvasst.IsEnabled = index < 0 ? false : true;
            cb_weap_invisible.IsEnabled = index < 0 ? false : true;
            cb_weap_civ.IsEnabled = index < 0 ? false : true;
            cb_weap_brest1.IsEnabled = index < 0 ? false : true;
            cb_weap_brest2.IsEnabled = index < 0 ? false : true;
            cb_weap_brest3.IsEnabled = index < 0 ? false : true;
            cb_weap_brest4.IsEnabled = index < 0 ? false : true;
            cbweapenablerot.IsEnabled = index < 0 ? false : true;
            ddweapspawnon.IsEnabled = index < 0 ? false : true;
            tbweapclearrule.IsEnabled = index < 0 ? false : true;
            ddweapteamclear.IsEnabled = index < 0 ? false : true;

            if (m.IsProcOpen && index > -1)
            {
                int model = new Global(GTA.Offsets.Editor.Weapon.model + GTA.Offsets.Editor.Weapon.NEXT * index).Get<int>();

                if (!skipcategorymodel)
                {

                    try
                    {
                        int modelindex = GTA.Editor.WeaponListID.IndexOf(model);
                        if (modelindex > -1)
                        {
                            int categoryindex = GTA.Editor.WeaponCategories.IndexOf(GTA.Editor.WeaponList[modelindex].Category);
                            if (categoryindex > -1)
                            {
                                ddweapcategory.IsEnabled = true;
                                ddweapcategory.SelectedIndex = categoryindex;
                                ddweapmodelswicther.SelectedIndex = 1;
                                GTA.Weapon[] temp = new GTA.Weapon[weapModelList.Items.Count];
                                weapModelList.Items.CopyTo(temp, 0);
                                weapModelList.SelectedIndex = weapModelList.Items.IndexOf(temp.ToList().Where(x => x.Int32 == model).First());
                                weapModelList.ScrollIntoView(weapModelList.SelectedItem);
                            }
                            else
                            {
                                ddweapcategory.IsEnabled = false;
                                ddweapmodelswicther.SelectedIndex = 0;
                            }
                        }
                        else
                        {
                            ddweapcategory.IsEnabled = false;
                            ddweapmodelswicther.SelectedIndex = 0;
                        }
                    }
                    catch (Exception)
                    {
                        ddweapcategory.IsEnabled = false;
                        ddweapmodelswicther.SelectedIndex = 0;
                    }

                }

                if (ddweapmodelswicther.SelectedIndex == 0)
                {
                    ddweapcategory.IsEnabled = false;
                }

                if (!tbweapmodel.IsFocused || ignore_focus) tbweapmodel.Text = model.ToString();
                if (!tbweaplocx.IsFocused || ignore_focus) tbweaplocx.Text = new Global((GTA.Offsets.Editor.Weapon.locx + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<float>().ToString();
                if (!tbweaplocy.IsFocused || ignore_focus) tbweaplocy.Text = new Global((GTA.Offsets.Editor.Weapon.locy + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<float>().ToString();
                if (!tbweaplocz.IsFocused || ignore_focus) tbweaplocz.Text = new Global((GTA.Offsets.Editor.Weapon.locz + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<float>().ToString();
                if (!tbweapheading.IsFocused || ignore_focus) tbweapheading.Text = new Global((GTA.Offsets.Editor.Weapon.head + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<float>().ToString();

                if (!tbweapdmgmlt.IsFocused || ignore_focus) tbweapdmgmlt.Text = new Global((GTA.Offsets.Editor.Weapon.dmgmult + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<float>().ToString();
                Functions.Read.checkbinary(7, GTA.Offsets.Editor.Weapon.bits + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT, cb_weap_invisible);
                Functions.Read.checkbinary(11, GTA.Offsets.Editor.Weapon.bits + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT, cb_weap_civ);
                Functions.Read.checkbinary(1, GTA.Offsets.Editor.Weapon.brest + 0 + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT, cb_weap_brest1);
                Functions.Read.checkbinary(1, GTA.Offsets.Editor.Weapon.brest + 1 + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT, cb_weap_brest2);
                Functions.Read.checkbinary(1, GTA.Offsets.Editor.Weapon.brest + 2 + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT, cb_weap_brest3);
                Functions.Read.checkbinary(1, GTA.Offsets.Editor.Weapon.brest + 3 + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT, cb_weap_brest4);
                if (Functions.Read.checkbinary(10, GTA.Offsets.Editor.Weapon.bits + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT, cbweapenablerot))
                {
                    if (!tbweaprotx.IsFocused || ignore_focus) tbweaprotx.Text = new Global((GTA.Offsets.Editor.Weapon.rotx + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<float>().ToString();
                    if (!tbweaproty.IsFocused || ignore_focus) tbweaproty.Text = new Global((GTA.Offsets.Editor.Weapon.roty + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<float>().ToString();
                }
                if (!tbweapbits.IsFocused || ignore_focus) tbweapbits.Text = new Global((GTA.Offsets.Editor.Weapon.bits + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<int>().ToString();
                if (!tbweapvput.IsFocused || ignore_focus) tbweapvput.Text = new Global((GTA.Offsets.Editor.Weapon.vput + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<int>().ToString();
                if (!tbweaprput.IsFocused || ignore_focus) tbweaprput.Text = new Global((GTA.Offsets.Editor.Weapon.rput + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<int>().ToString();
                if (!tbweapclip.IsFocused || ignore_focus) tbweapclip.Text = new Global((GTA.Offsets.Editor.Weapon.clip + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<int>().ToString();
                if (!tbweapsub.IsFocused || ignore_focus) tbweapsub.Text = new Global((GTA.Offsets.Editor.Weapon.sub + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<int>().ToString();
                if (!tbweapiptnp.IsFocused || ignore_focus) tbweapiptnp.Text = new Global((GTA.Offsets.Editor.Weapon.iptnp + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<int>().ToString();
                if (!tbweapwcpm.IsFocused || ignore_focus) tbweapwcpm.Text = new Global((GTA.Offsets.Editor.Weapon.wcpm + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<int>().ToString();
                if (!tbweapvclnrl.IsFocused || ignore_focus) tbweapvclnrl.Text = new Global((GTA.Offsets.Editor.Weapon.vclnrl + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<int>().ToString();
                if (!tbweapvclnr.IsFocused || ignore_focus) tbweapvclnr.Text = new Global((GTA.Offsets.Editor.Weapon.vclnr + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<int>().ToString();
                if (!tbweapvclnt.IsFocused || ignore_focus) tbweapvclnt.Text = new Global((GTA.Offsets.Editor.Weapon.vclnt + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<int>().ToString();
                if (!tbweapclearrule.IsFocused || ignore_focus) tbweapclearrule.Text = new Global((GTA.Offsets.Editor.Weapon.vclnrl + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<int>().ToString();
                if (!ddweapteamclear.IsFocused || ignore_focus) ddweapteamclear.SelectedIndex = new Global((GTA.Offsets.Editor.Weapon.vclnt + GTA.Offsets.Editor.Weapon.NEXT * index)).Get<int>() + 1;

                loadweapvass(ignore_focus);
            }
        }

        private void ddweapno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetWeapons(false, true);

            SelectActiveTextBox();
        }

        private void ddweapmodelswicther_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ddweapcategory != null)
                {
                    if (ddweapmodelswicther.SelectedIndex == 0)
                    {
                        ddweapcategory.IsEnabled = false;
                        weapModelList.Visibility = Visibility.Collapsed;
                        tbweapmodel.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ddweapcategory.IsEnabled = true;
                        weapModelList.Visibility = Visibility.Visible;
                        tbweapmodel.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void ddweapcategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            weapModelList.DataContext = null;

            ObservableCollection<GTA.Weapon> weapon = new ObservableCollection<GTA.Weapon>();
            GTA.Editor.WeaponList.Where(x => x.Category == GTA.Editor.WeaponCategories[ddweapcategory.SelectedIndex]).ToList().ForEach(x => weapon.Add(x));

            weapModelList.DataContext = weapon;
        }

        private void weapModelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (weapModelList.SelectedIndex > -1 && m.IsProcOpen)
                new Global(GTA.Offsets.Editor.Weapon.model + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT).SetInt((weapModelList.SelectedItem as GTA.Weapon).Int32);
        }

        private void tbweapmodel_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Weapon.model + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetInt(Functions.int_parse(tbweapmodel.Text));
        }

        private void Btnweapgetloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.getlocation();

            tbweaplocx.Text = loc[0];
            tbweaplocy.Text = loc[1];
            tbweaplocz.Text = loc[2];
            creatorRefresh();
        }

        private void tbweaplocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Weapon.locz + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetFloat(tbweaplocz.Text);
        }

        private void tbweaplocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Weapon.locy + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetFloat(tbweaplocy.Text);
        }

        private void tbweaplocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Weapon.locx + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetFloat(tbweaplocx.Text);
        }

        private void tbweapheading_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Weapon.head + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetFloat(tbweapheading.Text);
        }

        private void tbweapdmgmlt_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Weapon.dmgmult + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetFloat(tbweapdmgmlt.Text);
        }

        private void cb_weap_invisible_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(7, GTA.Offsets.Editor.Weapon.bits + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT, cb_weap_invisible);
        }
        private void cb_weap_civ_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(11, GTA.Offsets.Editor.Weapon.bits + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT, cb_weap_civ);
        }

        private void tbweapvput_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (IsValidInt(tbweapvput.Text))
                new Global((GTA.Offsets.Editor.Weapon.vput + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetInt(tbweapvput.Text);
        }

        private void tbweapbits_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbweapbits.Text))
            {
                new Global((GTA.Offsets.Editor.Weapon.bits + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetInt(tbweapbits.Text);
            }
        }

        private void ChangeWeapLocationXPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Weapon.number), GTA.Offsets.Editor.Weapon.locx, GTA.Offsets.Editor.Weapon.NEXT);
        }

        private void ChangeWeapLocationYPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Weapon.number), GTA.Offsets.Editor.Weapon.locy, GTA.Offsets.Editor.Weapon.NEXT);
        }

        private void ChangeWeapLocationZPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Weapon.number), GTA.Offsets.Editor.Weapon.locz, GTA.Offsets.Editor.Weapon.NEXT);
        }

        private void tbweaprput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbweaprput.Text))
                new Global((GTA.Offsets.Editor.Weapon.rput + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetInt(tbweaprput.Text);
        }

        private void ddweaponteam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadweapvass(true);
            SelectActiveTextBox();
        }

        private void tbweapvclnrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbweapvclnrl.Text))
                new Global((GTA.Offsets.Editor.Weapon.vclnrl + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetInt(tbweapvclnrl.Text);
        }

        private void tbweapvclnr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbweapvclnr.Text))
                new Global((GTA.Offsets.Editor.Weapon.vclnr + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetInt(tbweapvclnr.Text);
        }

        private void tbweapvclnt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbweapvclnt.Text))
                new Global((GTA.Offsets.Editor.Weapon.vclnt + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetInt(tbweapvclnt.Text);
        }

        private void tbweapclip_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbweapclip.Text))
                new Global((GTA.Offsets.Editor.Weapon.clip + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetInt(tbweapclip.Text);
        }

        private void tbweapsub_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbweapsub.Text))
                new Global((GTA.Offsets.Editor.Weapon.sub + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetInt(tbweapsub.Text);
        }

        private void tbweapvasso_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (IsValidInt(tbweapvasso.Text))
            {
                switch (ddweaponteam.SelectedIndex)
                {
                    case 0:
                        new Global(GTA.Offsets.Editor.Weapon.vasso + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex).SetInt(tbweapvasso.Text);
                        break;
                    case 1:
                        new Global(GTA.Offsets.Editor.Weapon.vasso2 + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex).SetInt(tbweapvasso.Text);
                        break;
                    case 2:
                        new Global(GTA.Offsets.Editor.Weapon.vasso3 + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex).SetInt(tbweapvasso.Text);
                        break;
                    case 3:
                        new Global(GTA.Offsets.Editor.Weapon.vasso4 + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex).SetInt(tbweapvasso.Text);
                        break;
                    default:
                        break;
                }
            }
        }

        private void tbweapvasss_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (IsValidInt(tbweapvasss.Text))
            {
                switch (ddweaponteam.SelectedIndex)
                {
                    case 0:
                        new Global(GTA.Offsets.Editor.Weapon.vasss + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex).SetInt(tbweapvasss.Text);
                        break;
                    case 1:
                        new Global(GTA.Offsets.Editor.Weapon.vasss2 + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex).SetInt(tbweapvasss.Text);
                        break;
                    case 2:
                        new Global(GTA.Offsets.Editor.Weapon.vasss3 + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex).SetInt(tbweapvasss.Text);
                        break;
                    case 3:
                        new Global(GTA.Offsets.Editor.Weapon.vasss4 + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex).SetInt(tbweapvasss.Text);
                        break;
                    default:
                        break;
                }
            }
        }

        private void tbweapvasst_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (IsValidInt(tbweapvasst.Text))
            {
                switch (ddweaponteam.SelectedIndex)
                {
                    case 0:
                        new Global(GTA.Offsets.Editor.Weapon.vasst + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex).SetInt(tbweapvasst.Text);
                        break;
                    case 1:
                        new Global(GTA.Offsets.Editor.Weapon.vasst2 + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex).SetInt(tbweapvasst.Text);
                        break;
                    case 2:
                        new Global(GTA.Offsets.Editor.Weapon.vasst3 + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex).SetInt(tbweapvasst.Text);
                        break;
                    case 3:
                        new Global(GTA.Offsets.Editor.Weapon.vasst4 + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex).SetInt(tbweapvasst.Text);
                        break;
                    default:
                        break;
                }
            }
        }

        private void tbweapiptnp_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbweapiptnp.Text, false))
                new Global((GTA.Offsets.Editor.Weapon.iptnp + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetInt(tbweapiptnp.Text);
        }

        private void tbweapwcpm_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbweapwcpm.Text, false))
                new Global((GTA.Offsets.Editor.Weapon.wcpm + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetInt(tbweapwcpm.Text);
        }

        public void loadweapvass(bool ignore_focus = false)
        {
            if (m.IsProcOpen)
            {
                if (ddweaponteam != null)
                {
                    int index = ddweaponteam.SelectedIndex;

                    long vasso, vasss, vasst;

                    if (index == 1)
                    {
                        vasso = GTA.Offsets.Editor.Weapon.vasso2;
                        vasss = GTA.Offsets.Editor.Weapon.vasss2;
                        vasst = GTA.Offsets.Editor.Weapon.vasst2;
                    }
                    else if (index == 2)
                    {
                        vasso = GTA.Offsets.Editor.Weapon.vasso3;
                        vasss = GTA.Offsets.Editor.Weapon.vasss3;
                        vasst = GTA.Offsets.Editor.Weapon.vasst3;
                    }
                    else if (index == 3)
                    {
                        vasso = GTA.Offsets.Editor.Weapon.vasso4;
                        vasss = GTA.Offsets.Editor.Weapon.vasss4;
                        vasst = GTA.Offsets.Editor.Weapon.vasst4;
                    }
                    else
                    {
                        vasso = GTA.Offsets.Editor.Weapon.vasso;
                        vasss = GTA.Offsets.Editor.Weapon.vasss;
                        vasst = GTA.Offsets.Editor.Weapon.vasst;
                    }


                    if (!tbweapvasso.IsFocused || ignore_focus) tbweapvasso.Text = new Global((vasso + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).Get<int>().ToString();
                    if (!tbweapvasst.IsFocused || ignore_focus) tbweapvasst.Text = new Global((vasst + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).Get<int>().ToString();
                    if (!tbweapvasss.IsFocused || ignore_focus) tbweapvasss.Text = new Global((vasss + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).Get<int>().ToString();
                    if (!tbweapspawnrule.IsFocused || ignore_focus) tbweapspawnrule.Text = new Global((vasso + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).Get<int>().ToString();
                    if (!ddweapspawnteam.IsFocused || ignore_focus) ddweapspawnteam.SelectedIndex = new Global((vasst + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).Get<int>() + 1;
                    if (!ddweapspawnon.IsFocused || ignore_focus) ddweapspawnon.SelectedIndex = new Global((vasss + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).Get<int>();


                    if (ddweapspawnon.SelectedIndex == 0)
                    {
                        ddweapspawnteam.IsEnabled = false;
                        tbweapspawnrule.IsEnabled = false;
                    }
                    else
                    {
                        ddweapspawnteam.IsEnabled = true;
                        tbweapspawnrule.IsEnabled = true;
                    }
                }
            }
        }

        private void tbweaprotx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Weapon.rotx + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT).SetFloat(tbweaprotx.Text);
        }

        private void tbweaproty_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Weapon.roty + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT).SetFloat(tbweaproty.Text);
        }

        private void cbweapenablerot_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(10, GTA.Offsets.Editor.Weapon.bits + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT, cbweapenablerot);
        }

        private void ddweapteamclear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Weapon.vclnt + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetInt(ddweapteamclear.SelectedIndex - 1);
        }

        private void tbweapclearrule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbweapclearrule.Text))
                new Global((GTA.Offsets.Editor.Weapon.vclnrl + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex)).SetInt(tbweapclearrule.Text);
        }

        private void ddweapspawnon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            long vasss;

            if (ddweaponteam.SelectedIndex == 1)
            {
                vasss = GTA.Offsets.Editor.Weapon.vasss2;
            }
            else if (ddweaponteam.SelectedIndex == 2)
            {
                vasss = GTA.Offsets.Editor.Weapon.vasss3;
            }
            else if (ddweaponteam.SelectedIndex == 3)
            {
                vasss = GTA.Offsets.Editor.Weapon.vasss4;
            }
            else
            {
                vasss = GTA.Offsets.Editor.Weapon.vasss;
            }

            new Global(vasss + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex).SetInt(ddweapspawnon.SelectedIndex);
        }

        private void ddweapspawnteam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            long vasst;

            if (ddweaponteam.SelectedIndex == 1)
            {
                vasst = GTA.Offsets.Editor.Weapon.vasst2;
            }
            else if (ddweaponteam.SelectedIndex == 2)
            {
                vasst = GTA.Offsets.Editor.Weapon.vasst3;
            }
            else if (ddweaponteam.SelectedIndex == 3)
            {
                vasst = GTA.Offsets.Editor.Weapon.vasst4;
            }
            else
            {
                vasst = GTA.Offsets.Editor.Weapon.vasst;
            }

            new Global(vasst + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex).SetInt(ddweapspawnteam.SelectedIndex - 1);
        }

        private void tbweapspawnrule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbweapspawnrule.Text))
            {
                long vasso;

                if (ddweaponteam.SelectedIndex == 1)
                {
                    vasso = GTA.Offsets.Editor.Weapon.vasso2;
                }
                else if (ddweaponteam.SelectedIndex == 2)
                {
                    vasso = GTA.Offsets.Editor.Weapon.vasso3;
                }
                else if (ddweaponteam.SelectedIndex == 3)
                {
                    vasso = GTA.Offsets.Editor.Weapon.vasso4;
                }
                else
                {
                    vasso = GTA.Offsets.Editor.Weapon.vasso;
                }
                new Global(vasso + GTA.Offsets.Editor.Weapon.NEXT * ddweapno.SelectedIndex).SetInt(tbweapspawnrule.Text);
            }
        }

        private void cb_weap_brest1_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.Weapon.brest + 0 + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT, cb_weap_brest1);
        }

        private void cb_weap_brest2_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.Weapon.brest + 1 + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT, cb_weap_brest2);
        }

        private void cb_weap_brest3_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.Weapon.brest + 2 + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT, cb_weap_brest3);
        }

        private void cb_weap_brest4_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.Weapon.brest + 3 + ddweapno.SelectedIndex * GTA.Offsets.Editor.Weapon.NEXT, cb_weap_brest4);
        }
    }
}
