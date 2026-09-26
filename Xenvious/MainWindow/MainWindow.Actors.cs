using System;
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
    // Part of MainWindow: Actors page.
    public partial class MainWindow
    {
        private void Btnactorgetloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.getlocation();

            tbactorlocx.Text = loc[0];
            tbactorlocy.Text = loc[1];
            tbactorlocz.Text = loc[2];
            creatorRefresh();
        }

        private void tbactorlocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Actor.locx + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetFloat(tbactorlocx.Text);
        }

        private void tbactorlocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Actor.locy + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetFloat(tbactorlocy.Text);
        }

        private void tbactorlocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Actor.locz + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetFloat(tbactorlocz.Text);
        }

        private void ddactorno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetActorValues(true, true);
            SelectActiveTextBox();
        }

        public void GetActorValues(bool skipcategorymodel = false, bool ignore_focus = false)
        {
            int index = ddactorno.SelectedIndex;

            ddactormodelswicther.IsEnabled = index < 0 ? false : true;
            Btnactorgetloc.IsEnabled = index < 0 ? false : true;
            tbactormodel.IsEnabled = index < 0 ? false : true;
            tbactorlocx.IsEnabled = index < 0 ? false : true;
            tbactorlocy.IsEnabled = index < 0 ? false : true;
            tbactorlocz.IsEnabled = index < 0 ? false : true;
            actorModelList.IsEnabled = index < 0 ? false : true;
            tbactorhead.IsEnabled = index < 0 ? false : true;
            tbactorpcash.IsEnabled = index < 0 ? false : true;
            tbactorblipsize.IsEnabled = index < 0 ? false : true;
            tbactordmv.IsEnabled = index < 0 ? false : true;
            tbactoractvvehspeed.IsEnabled = index < 0 ? false : true;
            tbactorrr.IsEnabled = index < 0 ? false : true;
            tbactorpspdl.IsEnabled = index < 0 ? false : true;
            ddActorpbs.IsEnabled = index < 0 ? false : true;
            tbactorpbs.IsEnabled = index < 0 ? false : true;
            ddActorweap.IsEnabled = index < 0 ? false : true;
            cbactortacticlelight.IsEnabled = index < 0 ? false : true;
            ddActoridle.IsEnabled = index < 0 ? false : true;
            ddActorrsp.IsEnabled = index < 0 ? false : true;
            ddActoraccu.IsEnabled = index < 0 ? false : true;
            ddActorhealth.IsEnabled = index < 0 ? false : true;
            ddActorcombat.IsEnabled = index < 0 ? false : true;
            ddActorcar.IsEnabled = index < 0 ? false : true;
            ddActorteam.IsEnabled = index < 0 ? false : true;
            ddActorrel.IsEnabled = index < 0 ? false : true;
            cbactorantifall.IsEnabled = index < 0 ? false : true;
            cbactorddb.IsEnabled = index < 0 ? false : true;
            cbactordiw.IsEnabled = index < 0 ? false : true;
            cbactords.IsEnabled = index < 0 ? false : true;
            cbactorfgf.IsEnabled = index < 0 ? false : true;
            cbactorfmdc.IsEnabled = index < 0 ? false : true;
            cbactorie.IsEnabled = index < 0 ? false : true;
            cbactorroav.IsEnabled = index < 0 ? false : true;
            cbactorstationary.IsEnabled = index < 0 ? false : true;
            cbactorwh.IsEnabled = index < 0 ? false : true;
            cbactorfightunarmed.IsEnabled = index < 0 ? false : true;
            cbactorfoll.IsEnabled = index < 0 ? false : true;
            ddActorgoto.IsEnabled = index < 0 ? false : true;
            cbactorcantleaveveh.IsEnabled = index < 0 ? false : true;
            cbactorcanttarget.IsEnabled = index < 0 ? false : true;
            cbactordiswd.IsEnabled = index < 0 ? false : true;
            cbactorremarmor.IsEnabled = index < 0 ? false : true;
            cbactorspd.IsEnabled = index < 0 ? false : true;
            cbactorivc.IsEnabled = index < 0 ? false : true;
            cbactorspwnrlivc.IsEnabled = index < 0 ? false : true;
            cbactorrespawnrlivc.IsEnabled = index < 0 ? false : true;
            cbactoractvhadest.IsEnabled = index < 0 ? false : true;
            cbactoractvradest.IsEnabled = index < 0 ? false : true;
            cbactoractvrpadest.IsEnabled = index < 0 ? false : true;
            cbactorbulletproof.IsEnabled = index < 0 ? false : true;
            cbactorfireproof.IsEnabled = index < 0 ? false : true;
            cbactorexplosionproof.IsEnabled = index < 0 ? false : true;
            cbactorcollisionproof.IsEnabled = index < 0 ? false : true;
            cbactormeeleproof.IsEnabled = index < 0 ? false : true;
            cbactorsteamproof.IsEnabled = index < 0 ? false : true;
            cbactordrowningproof.IsEnabled = index < 0 ? false : true;
            ddactorteamrlprio.IsEnabled = index < 0 ? false : true;
            tbactorrule.IsEnabled = index < 0 ? false : true;
            tbactorpriority.IsEnabled = index < 0 ? false : true;
            tbactorjtop.IsEnabled = index < 0 ? false : true;
            tbactorjtof.IsEnabled = index < 0 ? false : true;
            tbactorobjt.IsEnabled = index < 0 ? false : true;
            tbactorteam.IsEnabled = index < 0 ? false : true;
            tbactorspwn.IsEnabled = index < 0 ? false : true;
            tbactoracts.IsEnabled = index < 0 ? false : true;
            tbactorscrrq.IsEnabled = index < 0 ? false : true;
            tbactorawysrl.IsEnabled = index < 0 ? false : true;
            tbactorpedcr.IsEnabled = index < 0 ? false : true;
            tbactorpedct.IsEnabled = index < 0 ? false : true;
            tbactorclearrule.IsEnabled = index < 0 ? false : true;
            ddactorteamclear.IsEnabled = index < 0 ? false : true;
            ddactorspawnon.IsEnabled = index < 0 ? false : true;
            ddactoractionon.IsEnabled = index < 0 ? false : true;



            if (m.IsProcOpen && index > -1)
            {
                int model = new Global(GTA.Offsets.Editor.Actor.model + GTA.Offsets.Editor.Actor.NEXT * index).Get<int>();

                if (!skipcategorymodel)
                {
                    if (ddactormodelswicther.SelectedIndex == 1)
                    {
                        try
                        {
                            ddactormodelswicther.SelectedIndex = 1;
                            GTA.Actor[] temp = new GTA.Actor[actorModelList.Items.Count];
                            actorModelList.Items.CopyTo(temp, 0);
                            actorModelList.SelectedIndex = actorModelList.Items.IndexOf(temp.ToList().Where(x => x.Int32 == model).First());
                            actorModelList.ScrollIntoView(actorModelList.SelectedItem);
                        }
                        catch (Exception)
                        {
                            ddactormodelswicther.SelectedIndex = 0;
                        }
                    }
                }

                // enable when unarmed
                if (model == -1569615261)
                    cbactorfightunarmed.IsEnabled = true;


                if (!tbactormodel.IsFocused || ignore_focus) tbactormodel.Text = model.ToString();
                ActorModelCard.SetModel(unchecked((uint)model));
                if (!tbactorlocx.IsFocused || ignore_focus) tbactorlocx.Text = new Global((GTA.Offsets.Editor.Actor.locx + GTA.Offsets.Editor.Actor.NEXT * index)).Get<float>().ToString();
                if (!tbactorlocy.IsFocused || ignore_focus) tbactorlocy.Text = new Global((GTA.Offsets.Editor.Actor.locy + GTA.Offsets.Editor.Actor.NEXT * index)).Get<float>().ToString();
                if (!tbactorlocz.IsFocused || ignore_focus) tbactorlocz.Text = new Global((GTA.Offsets.Editor.Actor.locz + GTA.Offsets.Editor.Actor.NEXT * index)).Get<float>().ToString();

                if (!tbactorhead.IsFocused || ignore_focus) tbactorhead.Text = new Global((GTA.Offsets.Editor.Actor.head + GTA.Offsets.Editor.Actor.NEXT * index)).Get<float>().ToString();
                if (!tbactorrr.IsFocused || ignore_focus) tbactorrr.Text = new Global((GTA.Offsets.Editor.Actor.frr + GTA.Offsets.Editor.Actor.NEXT * index)).Get<float>().ToString();
                if (!tbactorpspdl.IsFocused || ignore_focus) tbactorpspdl.Text = new Global((GTA.Offsets.Editor.Actor.pspdl + GTA.Offsets.Editor.Actor.NEXT * index)).Get<int>().ToString();
                if (!tbactorblipsize.IsFocused || ignore_focus) tbactorblipsize.Text = new Global((GTA.Offsets.Editor.Actor.pdbps + GTA.Offsets.Editor.Actor.NEXT * index)).Get<float>().ToString();
                if (!tbactordmv.IsFocused || ignore_focus) tbactordmv.Text = new Global((GTA.Offsets.Editor.Actor.dmv + GTA.Offsets.Editor.Actor.NEXT * index)).Get<int>().ToString();
                if (!tbactoractvvehspeed.IsFocused || ignore_focus) tbactoractvvehspeed.Text = new Global((GTA.Offsets.Editor.Actor.gtds + GTA.Offsets.Editor.Actor.NEXT * index)).Get<int>().ToString();
                if (!tbactorpcash.IsFocused || ignore_focus) tbactorpcash.Text = new Global((GTA.Offsets.Editor.Actor.pcash + GTA.Offsets.Editor.Actor.NEXT * index)).Get<int>().ToString();


                if (ddActorpbs.SelectedIndex > -1)
                {
                    if (!tbactorpbs.IsFocused || ignore_focus) tbactorpbs.Text = new Global(GTA.Offsets.Editor.Actor.pedbs + ddActorpbs.SelectedIndex + (GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();
                }

                try
                {
                    ddActorweap.SelectedIndex = Array.IndexOf(GTA.Editor.weaponactorArray, new Global((GTA.Offsets.Editor.Actor.weapon_model + GTA.Offsets.Editor.Actor.NEXT * index)).Get<int>().ToString("X8"));
                }
                catch (Exception)
                {
                    ddActorweap.SelectedIndex = -1;
                }
                Functions.Read.checkbinary(13, GTA.Offsets.Editor.Actor.pbs6 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactortacticlelight);
                ddActoridle.SelectedIndex = new Global((GTA.Offsets.Editor.Actor.iaim + GTA.Offsets.Editor.Actor.NEXT * index)).Get<int>();
                ddActorrsp.SelectedIndex = new Global((GTA.Offsets.Editor.Actor.rsp + GTA.Offsets.Editor.Actor.NEXT * index)).Get<int>();
                ddActoraccu.SelectedIndex = SwitchActorAccuracy2();
                ddActorhealth.SelectedIndex = SwitchActorHealth2();

                ddActorcombat.SelectedIndex = new Global((GTA.Offsets.Editor.Actor.cmsty + GTA.Offsets.Editor.Actor.NEXT * index)).Get<int>();
                if (ddActorteam.SelectedIndex != -1)
                    ddActorrel.SelectedIndex = new Global(GTA.Offsets.Editor.Actor.group + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorteam.SelectedIndex).Get<int>();

                ddActorcar.SelectedIndex = new Global((GTA.Offsets.Editor.Actor.veh + GTA.Offsets.Editor.Actor.NEXT * index)).Get<int>() + 1;

                cbactorfoll.IsChecked = new Global((GTA.Offsets.Editor.Actor.foll + GTA.Offsets.Editor.Actor.NEXT * index)).Get<int>() == 1;

                if (cbactorfoll.IsChecked == true)
                {
                    tbactorfolr.Text = new Global((GTA.Offsets.Editor.Actor.folr + GTA.Offsets.Editor.Actor.NEXT * index)).Get<int>().ToString();

                    for (int i = 0; i < 4; i++)
                    {
                        int num = new Global((GTA.Offsets.Editor.Actor.tmflw + i + GTA.Offsets.Editor.Actor.NEXT * index)).Get<int>() == 1 ? 1 : 0;
                        if (num == 1)
                        {
                            try
                            {
                                ddActorfollteam.SelectedIndex = i;
                            }
                            catch (Exception)
                            {

                            }
                            break;
                        }
                    }
                }

                Functions.Read.checkbinary(21, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactordiw);
                Functions.Read.checkbinary(1, GTA.Offsets.Editor.Actor.whost + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorwh);
                Functions.Read.checkbinary(4, GTA.Offsets.Editor.Actor.pbs5 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorie);
                Functions.Read.checkbinary(7, GTA.Offsets.Editor.Actor.pbs5 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorddb);
                Functions.Read.checkbinary(17, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactords);
                Functions.Read.checkbinary(32, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorfgf);
                Functions.Read.checkbinary(1, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorstationary);
                Functions.Read.checkbinary(2, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorfmdc);
                Functions.Read.checkbinary(3, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorroav);
                Functions.Read.checkbinary(28, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorantifall);
                Functions.Read.checkbinary(13, GTA.Offsets.Editor.Actor.pbs5 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorcantleaveveh);
                Functions.Read.checkbinary(14, GTA.Offsets.Editor.Actor.pbs14 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorcanttarget);
                Functions.Read.checkbinary(32, GTA.Offsets.Editor.Actor.pbs8 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactordiswd);
                Functions.Read.checkbinary(23, GTA.Offsets.Editor.Actor.pbs8 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorremarmor);
                Functions.Read.checkbinary(7, GTA.Offsets.Editor.Actor.pbs15 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorspd);
                Functions.Read.checkbinary(13, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorivc);
                Functions.Read.checkbinary(12, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorspwnrlivc);
                Functions.Read.checkbinary(27, GTA.Offsets.Editor.Actor.pbs8 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorrespawnrlivc);


                if (new Global((GTA.Offsets.Editor.Actor.weapon_model + GTA.Offsets.Editor.Actor.NEXT * index)).Get<int>() == -1569615261)
                {
                    int cmsty = new Global(GTA.Offsets.Editor.Actor.cmsty + GTA.Offsets.Editor.Actor.NEXT * index).Get<int>();
                    if (cmsty == 0 || cmsty == 2)
                    {
                        cbactorfightunarmed.IsChecked = true;
                    }
                    else
                    {
                        cbactorfightunarmed.IsChecked = false;
                    }
                }

                Functions.Read.checkbinary(16, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactoractvloop);
                Functions.Read.checkbinary(14, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactoractvhadest);
                Functions.Read.checkbinary(15, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactoractvradest);
                Functions.Read.checkbinary(10, GTA.Offsets.Editor.Actor.pbs13 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactoractvrpadest);
                Functions.Read.checkbinary(24, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorbulletproof);
                Functions.Read.checkbinary(25, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorfireproof);
                Functions.Read.checkbinary(26, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorexplosionproof);
                Functions.Read.checkbinary(27, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorcollisionproof);
                Functions.Read.checkbinary(28, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactormeeleproof);
                Functions.Read.checkbinary(29, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorsteamproof);
                Functions.Read.checkbinary(30, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactordrowningproof);

                if (ddactorteamrlprio.SelectedIndex > -1)
                {
                    if (!tbactorrule.IsFocused || ignore_focus) tbactorrule.Text = new Global((GTA.Offsets.Editor.Actor.rule + ddactorteamrlprio.SelectedIndex + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();
                    if (!tbactorpriority.IsFocused || ignore_focus) tbactorpriority.Text = new Global((GTA.Offsets.Editor.Actor.pri + ddactorteamrlprio.SelectedIndex + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();
                    if (!tbactorjtop.IsFocused || ignore_focus) tbactorjtop.Text = new Global((GTA.Offsets.Editor.Actor.jtop + ddactorteamrlprio.SelectedIndex + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();
                    if (!tbactorjtof.IsFocused || ignore_focus) tbactorjtof.Text = new Global((GTA.Offsets.Editor.Actor.jtof + ddactorteamrlprio.SelectedIndex + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();
                    GetActorSpecialValues(ignore_focus);
                }

                int clrteam = new Global((GTA.Offsets.Editor.Actor.pedct + GTA.Offsets.Editor.Actor.NEXT * index)).Get<int>();
                int clrrule = new Global((GTA.Offsets.Editor.Actor.pedcr + GTA.Offsets.Editor.Actor.NEXT * index)).Get<int>();

                if (!tbactorpedcr.IsFocused || ignore_focus) tbactorpedcr.Text = clrrule.ToString();
                if (!tbactorpedct.IsFocused || ignore_focus) tbactorpedct.Text = clrteam.ToString();

                if (!tbactorclearrule.IsFocused || ignore_focus) tbactorclearrule.Text = clrrule.ToString();
                if (!ddactorteamclear.IsFocused || ignore_focus) ddactorteamclear.SelectedIndex = clrteam + 1;


            }
        }

        private void BtnActorAdd_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int actornum = new Global(GTA.Offsets.Editor.Actor.number).Get<int>();
                if (actornum < 80 && actornum > -1)
                {
                    int new_index = actornum + 1;

                    new Global(GTA.Offsets.Editor.Actor.number).SetInt(new_index);
                    //new Global(GTA.Offsets.Editor.Actor.cutsh + GTA.Offsets.Editor.Actor.NEXT * new_index).SetInt(-1);
                    //new Global(GTA.Offsets.Editor.Actor.pCwhT + GTA.Offsets.Editor.Actor.NEXT * new_index).SetInt(0);
                    ddactorno.SelectedIndex = new_index - 1;

                    if (new Global((GTA.Offsets.Editor.Actor.model + GTA.Offsets.Editor.Actor.NEXT * new_index)).Get<int>() != 0)
                    {
                        creatorRefresh();
                    }
                }
            }
        }

        private void ddactormodelswicther_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (actorModelList != null)
                {
                    if (ddactormodelswicther.SelectedIndex == 0)
                    {
                        actorModelList.Visibility = Visibility.Collapsed;
                        tbactormodel.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        actorModelList.Visibility = Visibility.Visible;
                        tbactormodel.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void tbactormodel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbactormodel.Text))
            {
                new Global((GTA.Offsets.Editor.Actor.model + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactormodel.Text);
            }
        }

        private void actorModelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (actorModelList.SelectedIndex > -1 && m.IsProcOpen)
            {
                int model = (actorModelList.SelectedItem as GTA.Actor).Int32;
                new Global(GTA.Offsets.Editor.Actor.model + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT).SetInt(model);
            }
        }

        private void tbactorhead_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Actor.head + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetFloat(tbactorhead.Text);
        }

        private void tbactorpcash_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbactorpcash.Text, true))
                new Global((GTA.Offsets.Editor.Actor.pcash + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorpcash.Text);
        }

        private void tbactorrr_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Actor.frr + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetFloat(tbactorrr.Text);
        }

        private void tbactorpspdl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbactorpspdl.Text, false))
                new Global((GTA.Offsets.Editor.Actor.pspdl + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorpspdl.Text);
        }

        private void tbactorblipsize_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Actor.pdbps + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetFloat(tbactorblipsize.Text);
        }

        private void ddActorWeap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddActorweap.SelectedIndex > -1 && m.IsProcOpen)
            {
                new Global((GTA.Offsets.Editor.Actor.weapon_model + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(Functions.int_parse(GTA.Editor.weaponactorArray.GetValue(ddActorweap.SelectedIndex).ToString()));
                new Global((GTA.Offsets.Editor.Actor.weapon + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(ddActorweap.SelectedIndex);

                if (new Global((GTA.Offsets.Editor.Actor.weapon_model + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>() == -1569615261)
                    cbactorfightunarmed.IsEnabled = true;
                else
                    cbactorfightunarmed.IsEnabled = false;

            }
        }

        private void ddActoridle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddActoridle.SelectedIndex > -1 && m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Actor.iaim + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(ddActoridle.SelectedIndex);
        }

        private void ddActorrsp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddActorrsp.SelectedIndex > -1 && m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Actor.rsp + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(ddActorrsp.SelectedIndex);
        }

        private void ddActoraccu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddActoraccu.SelectedIndex > -1 && m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Actor.accu + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(SwitchActorAccuracy1());
        }

        public int SwitchActorAccuracy1()
        {
            switch (ddActoraccu.SelectedIndex)
            {
                case 0:
                    return 3;
                case 1:
                    return 2;
                case 2:
                    return 0;
                case 3:
                    return 1;
                case 4:
                    return 4;
                default:
                    return -1;
            }
        }

        public int SwitchActorAccuracy2()
        {
            switch (new Global((GTA.Offsets.Editor.Actor.accu + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>())
            {
                case 3:
                    return 0;
                case 2:
                    return 1;
                case 0:
                    return 2;
                case 1:
                    return 3;
                case 4:
                    return 4;
                default:
                    return -1;
            }
        }

        public int SwitchActorHealth1()
        {
            int health = ddActorhealth.SelectedIndex;
            switch (health)
            {
                case 0:
                    return 6;
                case 1:
                    return 7;
                case 2:
                    return 8;
                case 3:
                    return 0;
                case 4:
                    return 1;
                case 5:
                    return 2;
                case 6:
                    return 14;
                case 7:
                    return 3;
                case 8:
                    return 15;
                case 9:
                    return 4;
                case 10:
                    return 16;
                case 11:
                    return 5;
                case 12:
                    return 9;
                case 13:
                    return 10;
                case 14:
                    return 11;
                case 15:
                    return 12;
                case 16:
                    return 13;
                default:
                    return health;
            }
        }
        public int SwitchActorHealth2()
        {
            int health = new Global((GTA.Offsets.Editor.Actor.hlt + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>();
            switch (health)
            {
                case 6:
                    return 0;
                case 7:
                    return 1;
                case 8:
                    return 2;
                case 0:
                    return 3;
                case 1:
                    return 4;
                case 2:
                    return 5;
                case 14:
                    return 6;
                case 3:
                    return 7;
                case 15:
                    return 8;
                case 4:
                    return 9;
                case 16:
                    return 10;
                case 5:
                    return 11;
                case 9:
                    return 12;
                case 10:
                    return 13;
                case 11:
                    return 14;
                case 12:
                    return 15;
                case 13:
                    return 16;
                default:
                    return health;
            }
        }

        private void ddActorhealth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddActorhealth.SelectedIndex > -1 && m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Actor.hlt + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(SwitchActorHealth1());
        }

        private void ddActorcar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddActorcar.SelectedIndex > -1 && m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Actor.veh + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(ddActorcar.SelectedIndex - 1);
        }

        private void ddActorcombat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddActorcombat.SelectedIndex > -1 && m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Actor.cmsty + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(ddActorcombat.SelectedIndex);
        }

        private void ddActorrel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddActorteam.SelectedIndex > -1 && m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Actor.group + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorteam.SelectedIndex)).SetInt(ddActorrel.SelectedIndex);
        }

        private void ddActorteam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddActorteam.SelectedIndex != -1 && m.IsProcOpen)
            {
                ddActorrel.SelectedIndex = new Global(GTA.Offsets.Editor.Actor.group + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorteam.SelectedIndex).Get<int>();

                SelectActiveTextBox();
            }
        }

        private void cbactorantifall_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(28, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorantifall);
        }

        private void cbactorddb_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(7, GTA.Offsets.Editor.Actor.pbs5 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorddb);
        }

        private void cbactordiw_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(21, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactordiw);
        }

        private void cbactords_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(17, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactords);
        }

        private void cbactorfgf_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(32, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorfgf);
        }

        private void cbactorie_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(19, GTA.Offsets.Editor.Actor.pbs5 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorie);
        }

        private void cbactorfmdc_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(2, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorfmdc);
        }

        private void cbactorroav_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(3, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorroav);
        }

        private void cbactorstationary_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorstationary);
        }

        private void cbactorwh_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.Actor.whost + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorwh);
        }

        private void Btnactorgetlocgoto_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.getlocation();

            tbactoractvx.Text = loc[0];
            tbactoractvy.Text = loc[1];
            tbactoractvz.Text = loc[2];
        }

        private void tbactoractvx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && ddactorno.SelectedIndex > -1)
            {
                new Global(GTA.Offsets.Editor.Actor.actvx + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).SetFloat(tbactoractvx.Text);
            }
        }

        private void tbactoractvy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && ddactorno.SelectedIndex > -1)
            {
                new Global(GTA.Offsets.Editor.Actor.actvy + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).SetFloat(tbactoractvy.Text);
            }
        }

        private void tbactoractvz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && ddactorno.SelectedIndex > -1)
            {
                new Global(GTA.Offsets.Editor.Actor.actvz + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).SetFloat(tbactoractvz.Text);
            }
        }

        private void ddActorgoto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetActorACTVValues();
            SelectActiveTextBox();
        }

        public void GetActorACTVValues(bool ignore_focus = true)
        {
            int index = ddActorgoto.SelectedIndex;
            tbactoractvx.IsEnabled = index < 0 ? false : true;
            tbactoractvy.IsEnabled = index < 0 ? false : true;
            tbactoractvz.IsEnabled = index < 0 ? false : true;
            Btnactorgetlocgoto.IsEnabled = index < 0 ? false : true;
            Btnactorgetlocgotoawl.IsEnabled = index < 0 ? false : true;
            tbactoractvsize.IsEnabled = index < 0 ? false : true;
            tbactoractvspeed.IsEnabled = index < 0 ? false : true;
            tbactoractvbs.IsEnabled = index < 0 ? false : true;
            tbactoractvachf.IsEnabled = index < 0 ? false : true;
            tbactoractvawt.IsEnabled = index < 0 ? false : true;
            tbactoractvawr.IsEnabled = index < 0 ? false : true;
            tbactoractvawlx.IsEnabled = index < 0 ? false : true;
            tbactoractvawly.IsEnabled = index < 0 ? false : true;
            tbactoractvawlz.IsEnabled = index < 0 ? false : true;
            tbactoractvawlr.IsEnabled = index < 0 ? false : true;
            tbactoractvags.IsEnabled = index < 0 ? false : true;
            cbactoractvloop.IsEnabled = index < 0 ? false : true;

            if (m.IsProcOpen && ddActorgoto.SelectedIndex > -1)
            {
                if (ddactorno.SelectedIndex > -1)
                {
                    if (!tbactoractvx.IsFocused || ignore_focus) tbactoractvx.Text = new Global(GTA.Offsets.Editor.Actor.actvx + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).Get<float>().ToString();
                    if (!tbactoractvy.IsFocused || ignore_focus) tbactoractvy.Text = new Global(GTA.Offsets.Editor.Actor.actvy + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).Get<float>().ToString();
                    if (!tbactoractvz.IsFocused || ignore_focus) tbactoractvz.Text = new Global(GTA.Offsets.Editor.Actor.actvz + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).Get<float>().ToString();
                    if (!tbactoractvsize.IsFocused || ignore_focus) tbactoractvsize.Text = new Global(GTA.Offsets.Editor.Actor.agrd + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).Get<float>().ToString();
                    if (!tbactoractvspeed.IsFocused || ignore_focus) tbactoractvspeed.Text = new Global(GTA.Offsets.Editor.Actor.agvr + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).Get<float>().ToString();
                    if (!tbactoractvbs.IsFocused || ignore_focus) tbactoractvbs.Text = new Global(GTA.Offsets.Editor.Actor.actv_bs + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).Get<int>().ToString();
                    if (!tbactoractvachf.IsFocused || ignore_focus) tbactoractvachf.Text = new Global(GTA.Offsets.Editor.Actor.achf + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).Get<float>().ToString();
                    if (!tbactoractvawt.IsFocused || ignore_focus) tbactoractvawt.Text = new Global(GTA.Offsets.Editor.Actor.awt + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).Get<int>().ToString();
                    if (!tbactoractvawr.IsFocused || ignore_focus) tbactoractvawr.Text = new Global(GTA.Offsets.Editor.Actor.awr + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).Get<int>().ToString();
                    if (!tbactoractvawlx.IsFocused || ignore_focus) tbactoractvawlx.Text = new Global(GTA.Offsets.Editor.Actor.awl + 0 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).Get<float>().ToString();
                    if (!tbactoractvawly.IsFocused || ignore_focus) tbactoractvawly.Text = new Global(GTA.Offsets.Editor.Actor.awl + 1 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).Get<float>().ToString();
                    if (!tbactoractvawlz.IsFocused || ignore_focus) tbactoractvawlz.Text = new Global(GTA.Offsets.Editor.Actor.awl + 2 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).Get<float>().ToString();
                    if (!tbactoractvawlr.IsFocused || ignore_focus) tbactoractvawlr.Text = new Global(GTA.Offsets.Editor.Actor.awlr + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).Get<int>().ToString();
                    if (!tbactoractvags.IsFocused || ignore_focus) tbactoractvags.Text = new Global(GTA.Offsets.Editor.Actor.ags + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).Get<int>().ToString();
                }
            }
        }

        private void tbactoractvsize_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Actor.agrd + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).SetFloat(tbactoractvsize.Text);
        }

        private void tbactoractvspeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Actor.agvr + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).SetFloat(tbactoractvspeed.Text);
        }

        private void tbactoractvawt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbactoractvawt.Text))
            {
                new Global(GTA.Offsets.Editor.Actor.awt + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).SetInt(tbactoractvawt.Text);
            }
        }

        private void tbactoractvawr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbactoractvawr.Text))
            {
                new Global(GTA.Offsets.Editor.Actor.awr + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).SetInt(tbactoractvawr.Text);
            }
        }

        private void tbactoractvawlx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Actor.awl + 0 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).SetFloat(tbactoractvawlx.Text);
        }

        private void tbactoractvawly_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Actor.awl + 1 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).SetFloat(tbactoractvawly.Text);
        }

        private void tbactoractvawlz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Actor.awl + 2 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).SetFloat(tbactoractvawlz.Text);
        }

        private void tbactoractvawlr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbactoractvawlr.Text))
            {
                new Global(GTA.Offsets.Editor.Actor.awlr + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).SetInt(tbactoractvawlr.Text);
            }
        }

        private void tbactoractvags_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbactoractvags.Text))
            {
                new Global(GTA.Offsets.Editor.Actor.ags + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).SetInt(tbactoractvags.Text);
            }
        }

        private void tbactoractvbs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbactoractvbs.Text))
            {
                new Global(GTA.Offsets.Editor.Actor.actv_bs + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).SetInt(tbactoractvbs.Text);
            }
        }

        private void tbactoractvachf_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Actor.achf + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex + ddActorgoto.SelectedIndex * GTA.Offsets.Editor.Actor.actv_NEXT).SetFloat(tbactoractvachf.Text);
        }

        private void Btnactorgetlocgotoawl_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.getlocation();

            tbactoractvawlx.Text = loc[0];
            tbactoractvawly.Text = loc[1];
            tbactoractvawlz.Text = loc[2];
        }

        private void cbactorfoll_Checked(object sender, RoutedEventArgs e)
        {
            bool enabled = cbactorfoll.IsChecked == true;

            tbactorfolr.IsEnabled = enabled;
            ddActorfollteam.IsEnabled = enabled;

            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Actor.foll + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(enabled ? 1 : 0);
        }

        private void tbactorfolr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbactorfolr.Text, true))
                new Global((GTA.Offsets.Editor.Actor.folr + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorfolr.Text);
        }

        private void ddActorfollteam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ddActorfollteam.SelectedIndex;
            if (cbactorfoll.IsChecked == true && m.IsProcOpen && ddActorfollteam.SelectedIndex > -1)
            {
                for (int i = 0; i < 4; i++)
                {
                    new Global((GTA.Offsets.Editor.Actor.tmflw + i + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt((i == index) ? 1 : 0);
                }
            }
        }

        private void ChangeActorLocationXPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Actor.number), GTA.Offsets.Editor.Actor.locx, GTA.Offsets.Editor.Actor.NEXT);
        }

        private void ChangeActorLocationYPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Actor.number), GTA.Offsets.Editor.Actor.locy, GTA.Offsets.Editor.Actor.NEXT);
        }

        private void ChangeActorLocationZPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Actor.number), GTA.Offsets.Editor.Actor.locz, GTA.Offsets.Editor.Actor.NEXT);
        }

        private void cbactorfightunarmed_Checked(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                new Global(GTA.Offsets.Editor.Actor.cmsty + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex).SetInt(cbactorfightunarmed.IsChecked == true ? 2 : 1);
            }
        }

        private void cbactorcentleaveveh_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(13, GTA.Offsets.Editor.Actor.pbs5 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorcantleaveveh);
        }

        private void cbactorbulletproof_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(24, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorbulletproof);
        }

        private void cbactorfireproof_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(25, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorfireproof);
        }

        private void cbactorexplosionproof_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(26, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorexplosionproof);
        }

        private void cbactorcollisionproof_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(27, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorcollisionproof);
        }

        private void cbactormeeleproof_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(28, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactormeeleproof);
        }

        private void cbactorsteamproof_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(29, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorsteamproof);
        }

        private void cbactordrowningproof_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(30, GTA.Offsets.Editor.Actor.pbs4 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactordrowningproof);
        }

        private void cbactorcanttarget_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(14, GTA.Offsets.Editor.Actor.pbs14 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorcanttarget);
        }

        private void cbactortacticlelight_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(13, GTA.Offsets.Editor.Actor.pbs6 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactortacticlelight);
        }


        private void ddactorteamrlprio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen && ddactorno.SelectedIndex > -1 && ddactorteamrlprio.SelectedIndex > -1)
            {
                tbactorrule.Text = new Global((GTA.Offsets.Editor.Actor.rule + ddactorteamrlprio.SelectedIndex + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();
                tbactorpriority.Text = new Global((GTA.Offsets.Editor.Actor.pri + ddactorteamrlprio.SelectedIndex + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();
                tbactorjtop.Text = new Global((GTA.Offsets.Editor.Actor.jtop + ddactorteamrlprio.SelectedIndex + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();
                tbactorjtof.Text = new Global((GTA.Offsets.Editor.Actor.jtof + ddactorteamrlprio.SelectedIndex + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();
                GetActorSpecialValues();
                SelectActiveTextBox();
            }
        }

        private void tbactorrule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbactorrule.Text))
                new Global((GTA.Offsets.Editor.Actor.rule + ddactorteamrlprio.SelectedIndex + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorrule.Text);
        }

        private void tbactorpriority_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbactorpriority.Text))
                new Global((GTA.Offsets.Editor.Actor.pri + ddactorteamrlprio.SelectedIndex + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorpriority.Text);
        }

        private void ddActorpbs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddActorpbs == null)
                return;
            if (m.IsProcOpen)
            {
                tbactorpbs.Text = new Global(GTA.Offsets.Editor.Actor.pedbs + ddActorpbs.SelectedIndex + (GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();

                SelectActiveTextBox();
            }
        }

        private void tbactorpbs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbactorpbs.Text))
            {
                new Global(GTA.Offsets.Editor.Actor.pedbs + ddActorpbs.SelectedIndex + (GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorpbs.Text);
            }
        }


        private void cbactoractvloop_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(16, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactoractvloop);
        }

        private void tbactorjtof_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbactorjtof.Text))
                new Global((GTA.Offsets.Editor.Actor.jtof + ddactorteamrlprio.SelectedIndex + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorjtof.Text);
        }

        private void tbactorjtop_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbactorjtop.Text))
                new Global((GTA.Offsets.Editor.Actor.jtop + ddactorteamrlprio.SelectedIndex + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorjtop.Text);

        }

        private void tbactorobjt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbactorobjt.Text))
            {
                if (ddactorteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.Actor.objt + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorobjt.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.Actor.objt1 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorobjt.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.Actor.objt2 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorobjt.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.Actor.objt3 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorobjt.Text);
                }
            }
        }

        private void tbactorteam_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbactorteam.Text))
            {
                if (ddactorteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.Actor.team + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorteam.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.Actor.team1 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorteam.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.Actor.team2 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorteam.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.Actor.team3 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorteam.Text);
                }
            }
        }

        private void tbactorspwn_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbactorspwn.Text))
            {
                if (ddactorteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.Actor.spawn + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorspwn.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.Actor.spawn1 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorspwn.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.Actor.spawn2 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorspwn.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.Actor.spawn3 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorspwn.Text);
                }
            }
        }

        public void GetActorSpecialValues(bool ignore_focus = true)
        {
            if (ddactorteamrlprio != null)
            {
                if (m.IsProcOpen && ddactorteamrlprio.SelectedIndex > -1)
                {
                    int index = ddactorteamrlprio.SelectedIndex;
                    long team, spwn, objt, acts, scrrq, awysrl;

                    if (index == 1)
                    {
                        team = GTA.Offsets.Editor.Actor.team1;
                        spwn = GTA.Offsets.Editor.Actor.spawn1;
                        objt = GTA.Offsets.Editor.Actor.objt1;
                        acts = GTA.Offsets.Editor.Actor.acts1;
                        scrrq = GTA.Offsets.Editor.Actor.scrrq1;
                        awysrl = GTA.Offsets.Editor.Actor.awysrl1;
                    }
                    else if (index == 2)
                    {
                        team = GTA.Offsets.Editor.Actor.team2;
                        spwn = GTA.Offsets.Editor.Actor.spawn2;
                        objt = GTA.Offsets.Editor.Actor.objt2;
                        acts = GTA.Offsets.Editor.Actor.acts2;
                        scrrq = GTA.Offsets.Editor.Actor.scrrq2;
                        awysrl = GTA.Offsets.Editor.Actor.awysrl2;
                    }
                    else if (index == 3)
                    {
                        team = GTA.Offsets.Editor.Actor.team3;
                        spwn = GTA.Offsets.Editor.Actor.spawn3;
                        objt = GTA.Offsets.Editor.Actor.objt3;
                        acts = GTA.Offsets.Editor.Actor.acts3;
                        scrrq = GTA.Offsets.Editor.Actor.scrrq3;
                        awysrl = GTA.Offsets.Editor.Actor.awysrl3;
                    }
                    else
                    {
                        team = GTA.Offsets.Editor.Actor.team;
                        spwn = GTA.Offsets.Editor.Actor.spawn;
                        objt = GTA.Offsets.Editor.Actor.objt;
                        acts = GTA.Offsets.Editor.Actor.acts;
                        scrrq = GTA.Offsets.Editor.Actor.scrrq;
                        awysrl = GTA.Offsets.Editor.Actor.awysrl;
                    }


                    if (!tbactorobjt.IsFocused || ignore_focus) tbactorobjt.Text = new Global((objt + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();
                    if (!tbactorspawnrule.IsFocused || ignore_focus) tbactorspawnrule.Text = new Global((objt + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();
                    if (!tbactorteam.IsFocused || ignore_focus) tbactorteam.Text = new Global((team + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();
                    if (!ddactorspawnteam.IsFocused || ignore_focus) ddactorspawnteam.SelectedIndex = new Global((team + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>() + 1;
                    if (!tbactorspwn.IsFocused || ignore_focus) tbactorspwn.Text = new Global((spwn + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();
                    if (!ddactorspawnon.IsFocused || ignore_focus) ddactorspawnon.SelectedIndex = new Global((spwn + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>();
                    if (!tbactoracts.IsFocused || ignore_focus) tbactoracts.Text = new Global((acts + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();
                    if (!ddactoractionon.IsFocused || ignore_focus) ddactoractionon.SelectedIndex = new Global((acts + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>();
                    if (!tbactorscrrq.IsFocused || ignore_focus) tbactorscrrq.Text = new Global((scrrq + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();
                    if (!tbactorawysrl.IsFocused || ignore_focus) tbactorawysrl.Text = new Global((awysrl + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).Get<int>().ToString();


                    if (ddactorspawnon.SelectedIndex == 0 && ddactoractionon.SelectedIndex == 0)
                    {
                        ddactorspawnteam.IsEnabled = false;
                        tbactorspawnrule.IsEnabled = false;
                    }
                    else
                    {
                        ddactorspawnteam.IsEnabled = true;
                        tbactorspawnrule.IsEnabled = true;
                    }

                }
            }
        }

        private void tbactoracts_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbactoracts.Text))
            {
                if (ddactorteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.Actor.acts + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactoracts.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.Actor.acts1 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactoracts.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.Actor.acts2 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactoracts.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.Actor.acts3 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactoracts.Text);
                }
            }
        }

        private void tbactorscrrq_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbactorscrrq.Text))
            {
                if (ddactorteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.Actor.scrrq + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorscrrq.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.Actor.scrrq1 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorscrrq.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.Actor.scrrq2 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorscrrq.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.Actor.scrrq3 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorscrrq.Text);
                }
            }
        }

        private void tbactorawysrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && IsValidInt(tbactorawysrl.Text))
            {
                if (ddactorteamrlprio.SelectedIndex == 0)
                {
                    new Global((GTA.Offsets.Editor.Actor.awysrl + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorawysrl.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 1)
                {
                    new Global((GTA.Offsets.Editor.Actor.awysrl1 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorawysrl.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 2)
                {
                    new Global((GTA.Offsets.Editor.Actor.awysrl2 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorawysrl.Text);
                }
                else if (ddactorteamrlprio.SelectedIndex == 3)
                {
                    new Global((GTA.Offsets.Editor.Actor.awysrl3 + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorawysrl.Text);
                }
            }
        }

        private void cbactordiswd_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(32, GTA.Offsets.Editor.Actor.pbs8 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactordiswd);
        }

        private void cbactorremarmor_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(23, GTA.Offsets.Editor.Actor.pbs8 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorremarmor);
        }

        private void cbactorspd_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(7, GTA.Offsets.Editor.Actor.pbs15 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorspd);
        }

        private void tbactorpedcr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbactorpedcr.Text))
            {
                new Global((GTA.Offsets.Editor.Actor.pedcr + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorpedcr.Text);
            }
        }

        private void tbactorpedct_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbactorpedct.Text))
            {
                new Global((GTA.Offsets.Editor.Actor.pedct + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorpedct.Text);
            }
        }

        private void cbactorivc_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(13, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorivc);
        }

        private void cbactoractvhadest_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(14, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactoractvhadest);
        }

        private void cbactoractvradest_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(15, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactoractvradest);
        }

        private void cbactoractvrpadest_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(10, GTA.Offsets.Editor.Actor.pbs13 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactoractvrpadest);
        }

        private void tbactoractvvehspeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbactoractvvehspeed.Text))
            {
                new Global((GTA.Offsets.Editor.Actor.gtds + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactoractvvehspeed.Text);
            }
        }

        private void tbactordmv_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Actor.dmv + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactordmv.Text);
        }

        private void cbactorrespawnrlivc_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(27, GTA.Offsets.Editor.Actor.pbs8 + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorrespawnrlivc);
        }

        private void cbactorspwnrlivc_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(12, GTA.Offsets.Editor.Actor.pedbs + ddactorno.SelectedIndex * GTA.Offsets.Editor.Actor.NEXT, cbactorspwnrlivc);
        }

        private void tbactorspawnrule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbactorspawnrule.Text))
            {
                long objt;

                if (ddactorteamrlprio.SelectedIndex == 1)
                {
                    objt = GTA.Offsets.Editor.Actor.objt1;
                }
                else if (ddactorteamrlprio.SelectedIndex == 2)
                {
                    objt = GTA.Offsets.Editor.Actor.objt2;
                }
                else if (ddactorteamrlprio.SelectedIndex == 3)
                {
                    objt = GTA.Offsets.Editor.Actor.objt3;
                }
                else
                {
                    objt = GTA.Offsets.Editor.Actor.objt;
                }

                new Global(objt + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex).SetInt(tbactorspawnrule.Text);
            }
        }

        private void ddactorspawnteam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            long team;

            if (ddactorteamrlprio.SelectedIndex == 1)
            {
                team = GTA.Offsets.Editor.Actor.team1;
            }
            else if (ddactorteamrlprio.SelectedIndex == 2)
            {
                team = GTA.Offsets.Editor.Actor.team2;
            }
            else if (ddactorteamrlprio.SelectedIndex == 3)
            {
                team = GTA.Offsets.Editor.Actor.team3;
            }
            else
            {
                team = GTA.Offsets.Editor.Actor.team;
            }

            new Global(team + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex).SetInt(ddactorspawnteam.SelectedIndex - 1);
        }

        private void ddactoractionon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            long acts;

            if (ddactorteamrlprio.SelectedIndex == 1)
            {
                acts = GTA.Offsets.Editor.Actor.acts1;
            }
            else if (ddactorteamrlprio.SelectedIndex == 2)
            {
                acts = GTA.Offsets.Editor.Actor.acts2;
            }
            else if (ddactorteamrlprio.SelectedIndex == 3)
            {
                acts = GTA.Offsets.Editor.Actor.acts3;
            }
            else
            {
                acts = GTA.Offsets.Editor.Actor.acts;
            }

            new Global(acts + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex).SetInt(ddactoractionon.SelectedIndex);
        }

        private void ddactorspawnon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            long spwn;

            if (ddactorteamrlprio.SelectedIndex == 1)
            {
                spwn = GTA.Offsets.Editor.Actor.spawn1;
            }
            else if (ddactorteamrlprio.SelectedIndex == 2)
            {
                spwn = GTA.Offsets.Editor.Actor.spawn2;
            }
            else if (ddactorteamrlprio.SelectedIndex == 3)
            {
                spwn = GTA.Offsets.Editor.Actor.spawn3;
            }
            else
            {
                spwn = GTA.Offsets.Editor.Actor.spawn;
            }

            new Global(spwn + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex).SetInt(ddactorspawnon.SelectedIndex);
        }

        private void tbactorclearrule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbactorclearrule.Text))
            {
                new Global((GTA.Offsets.Editor.Actor.pedcr + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(tbactorclearrule.Text);
            }
        }

        private void ddactorteamclear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new Global((GTA.Offsets.Editor.Actor.pedct + GTA.Offsets.Editor.Actor.NEXT * ddactorno.SelectedIndex)).SetInt(ddactorteamclear.SelectedIndex - 1);
        }
    }
}
