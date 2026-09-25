using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Capture page.
    public partial class MainWindow
    {
        public static List<string[]> capturetext = new List<string[]>
        {
            new string[]{ "","","" },
            new string[]{ "","","" },
            new string[]{ "","","" },
            new string[]{ "","","" },
            new string[]{ "","","" },
            new string[]{ "","","" }
        };



        //    throw new InvalidKeyException("Unsupported PEM format...");
        //}



        public static Thread force_settext = new Thread(new ThreadStart(force_settextfunc));
        bool objrotchainactive = false;

        private void ChangeOBJLocationXPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Objects.number), GTA.Offsets.Editor.Objects.loc + 0, GTA.Offsets.Editor.Objects.NEXT);
        }

        private void ChangeOBJLocationYPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Objects.number), GTA.Offsets.Editor.Objects.loc + 1, GTA.Offsets.Editor.Objects.NEXT);
        }

        private void ChangeOBJLocationZPlusMinus(object sender, KeyEventArgs e)
        {
            ChangeValuePlusMinus(sender, e, new Global(GTA.Offsets.Editor.Objects.number), GTA.Offsets.Editor.Objects.loc + 2, GTA.Offsets.Editor.Objects.NEXT);
        }

        private void ddcapturetexts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddcapturetexts.SelectedIndex != -1 && ddcapturetexts != null && m.IsProcOpen)
            {
                if (ddcaptureteamno.SelectedIndex == 0)
                {
                    tbcapturetext.Text = new Global(GTA.Offsets.Editor.txt0 + ddcapturetexts.SelectedIndex * GTA.Offsets.Editor.NEXT_txt + ddcaptureteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddcaptureno.SelectedIndex * GTA.Offsets.Editor.txt_NEXT).GetString();
                }
                else
                {
                    if (ddcaptureno.SelectedIndex == 0)
                    {
                        tbcapturetext.Text = capturetext[ddcapturetexts.SelectedIndex][ddcaptureteamno.SelectedIndex - 1];
                    }
                    else
                    {
                        tbcapturetext.Text = new Global(GTA.Offsets.Editor.txt0 + ddcapturetexts.SelectedIndex * GTA.Offsets.Editor.NEXT_txt + ddcaptureteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddcaptureno.SelectedIndex * GTA.Offsets.Editor.txt_NEXT).GetString();
                    }
                }

                IInputElement focusedControl = FocusManager.GetFocusedElement(this);

                if (focusedControl is TextBox)
                {
                    if ((focusedControl as TextBox).Name == "tbcapturetext")
                    {
                        tbcapturetext.SelectAll();
                    }
                }
            }
        }

        private void tbcapturetext_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ddcapturetexts != null)
            {
                if (ddcapturetexts.SelectedIndex > -1 && ddcaptureno.SelectedIndex > -1)
                {
                    if (ddcaptureteamno.SelectedIndex == 0)
                    {
                        new Global(GTA.Offsets.Editor.txt0 + ddcapturetexts.SelectedIndex * GTA.Offsets.Editor.NEXT_txt + ddcaptureteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddcaptureno.SelectedIndex * GTA.Offsets.Editor.txt_NEXT).SetString(tbcapturetext.Text);
                    }
                    else
                    {
                        if (ddcaptureno.SelectedIndex == 0)
                        {
                            capturetext[ddcapturetexts.SelectedIndex][ddcaptureteamno.SelectedIndex - 1] = tbcapturetext.Text;
                        }
                        else
                        {
                            new Global(GTA.Offsets.Editor.txt0 + ddcapturetexts.SelectedIndex * GTA.Offsets.Editor.NEXT_txt + ddcaptureteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddcaptureno.SelectedIndex * GTA.Offsets.Editor.txt_NEXT).SetString(tbcapturetext.Text);
                        }
                    }
                }
            }
        }

        private void BtnCaptureGeneral_Click(object sender, RoutedEventArgs e)
        {
            PageInnerCapture.SelectedItem = PageInnerCaptureGeneral;
        }

        private void BtnCaptureObjects_Click(object sender, RoutedEventArgs e)
        {
            PageInnerCapture.SelectedItem = PageInnerCaptureObjects;
        }

        private void PageInnerCapture_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnCaptureGeneral.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnCaptureObjects.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];
            BtnCaptureDelivery.Background = (SolidColorBrush)Resources["SeactionHeaderBackgroundBrush"];

            if (PageInnerCapture.SelectedItem == PageInnerCaptureGeneral)
            {
                BtnCaptureGeneral.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (PageInnerCapture.SelectedItem == PageInnerCaptureObjects)
            {
                BtnCaptureObjects.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
            else if (PageInnerCapture.SelectedItem == PageInnerCaptureDelivery)
            {
                BtnCaptureDelivery.Background = (SolidColorBrush)Resources["ButtonHoverBackgroundBrush"];
            }
        }

        private void ddcaptureteamno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckCaptureGeneralSection();
        }

        public void CheckCaptureGeneralSection()
        {
            if (m.IsProcOpen)
            {
                if (ddcapturetexts != null)
                {
                    if (ddcaptureteamno.SelectedIndex == 0)
                    {
                        tbcapturetext.Text = new Global(GTA.Offsets.Editor.txt0 + ddcapturetexts.SelectedIndex * GTA.Offsets.Editor.NEXT_txt + ddcaptureteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddcaptureno.SelectedIndex * GTA.Offsets.Editor.txt_NEXT).GetString();
                    }
                    else
                    {
                        if (ddcaptureno.SelectedIndex == 0)
                        {
                            tbcapturetext.Text = capturetext[ddcapturetexts.SelectedIndex][ddcaptureteamno.SelectedIndex - 1];
                        }
                        else
                        {
                            tbcapturetext.Text = new Global(GTA.Offsets.Editor.txt0 + ddcapturetexts.SelectedIndex * GTA.Offsets.Editor.NEXT_txt + ddcaptureteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddcaptureno.SelectedIndex * GTA.Offsets.Editor.txt_NEXT).GetString();
                        }
                    }

                }

                tbcapturetsc.Text = new Global(GTA.Offsets.Editor.tsc + ddcaptureteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddcaptureno.SelectedIndex).Get<int>().ToString();
                tbcapturemcry.Text = new Global(GTA.Offsets.Editor.mcry + ddcaptureteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddcaptureno.SelectedIndex).Get<int>().ToString();

                SelectActiveTextBox();
            }
        }

        private void tbcapturetsc_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.tsc + ddcaptureteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddcaptureno.SelectedIndex).SetInt(tbcapturetsc.Text);
        }

        private void tbcapturemcry_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.mcry + ddcaptureteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT + ddcaptureno.SelectedIndex).SetInt(tbcapturemcry.Text);
        }

        private void ddcaptureno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckCaptureGeneralSection();
        }

        private void BtnobjDelete_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int index = ddobjno.SelectedIndex;
                int num = new Global(GTA.Offsets.Editor.Objects.number).Get<int>();

                if (index > -1)
                {
                    try
                    {

                        List<List<int>> valuesafterdeletedobj = new List<List<int>>();

                        // get props after deleted obj
                        for (int i = index; i < num - 1; i++)
                        {
                            List<int> temp = new List<int>();
                            for (int d = 0; d < GTA.Offsets.Editor.Objects.NEXT; d++)
                            {
                                int test = new Global((GTA.Offsets.Editor.Objects.loc + ((i + 1) * GTA.Offsets.Editor.Objects.NEXT) + d)).Get<int>();
                                temp.Add(test);
                            }

                            valuesafterdeletedobj.Add(temp);
                        }

                        // clear deleted obj values

                        for (int i = 0; i < GTA.Offsets.Editor.Objects.NEXT; i++)
                        {
                            long deletedobjbase = GTA.Offsets.Editor.Objects.loc + (index * GTA.Offsets.Editor.Objects.NEXT);
                            new Global(deletedobjbase + i).SetInt(GTA.Defaults.Object[i]);
                        }

                        // lower obj number
                        if (num > 0)
                        {
                            new Global(GTA.Offsets.Editor.Objects.number).SetInt(num - 1);
                        }

                        for (int i = 0; i < valuesafterdeletedobj.Count(); i++)
                        {
                            for (int d = 0; d < valuesafterdeletedobj[i].Count(); d++)
                            {
                                long objbase = GTA.Offsets.Editor.Objects.loc + ((index + i) * GTA.Offsets.Editor.Objects.NEXT);
                                new Global(objbase + d).SetInt(valuesafterdeletedobj[i][d]);
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        private void BtnobjAdd_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                int objnum = new Global(GTA.Offsets.Editor.Objects.number).Get<int>();
                if (objnum < 20 && objnum > -1)
                {
                    int new_index = objnum + 1;

                    new Global(GTA.Offsets.Editor.Objects.number).SetInt(new_index);
                    if (objrotchainactive)
                    {
                        new Global((GTA.Offsets.Editor.Objects.vrot + 0 + GTA.Offsets.Editor.Objects.NEXT * (ddobjno.SelectedIndex + 1))).SetFloat(tbobjrotx.Text);
                        new Global((GTA.Offsets.Editor.Objects.vrot + 1 + GTA.Offsets.Editor.Objects.NEXT * (ddobjno.SelectedIndex + 1))).SetFloat(tbobjroty.Text);
                        new Global((GTA.Offsets.Editor.Objects.vrot + 2 + GTA.Offsets.Editor.Objects.NEXT * (ddobjno.SelectedIndex + 1))).SetFloat(tbobjrotz.Text);
                    }
                    else
                    {
                        tbobjrotx.Text = "";
                        tbobjroty.Text = "";
                        tbobjrotz.Text = "";
                    }

                    Global locx = new Global(GTA.Offsets.Editor.Objects.loc + 0 + GTA.Offsets.Editor.Objects.NEXT * (ddobjno.SelectedIndex + 1));
                    Global locy = new Global(GTA.Offsets.Editor.Objects.loc + 1 + GTA.Offsets.Editor.Objects.NEXT * (ddobjno.SelectedIndex + 1));
                    Global locz = new Global(GTA.Offsets.Editor.Objects.loc + 2 + GTA.Offsets.Editor.Objects.NEXT * (ddobjno.SelectedIndex + 1));

                    var loc = Functions.Read.getlocation();

                    if (locx.Get<float>() == 0)
                        locx.SetFloat(loc[0]);
                    if (locy.Get<float>() == 0)
                        locy.SetFloat(loc[1]);
                    if (locz.Get<float>() == 0)
                        locz.SetFloat(loc[2]);

                    ddobjno.SelectedIndex = new_index - 1;


                    if (new Global((GTA.Offsets.Editor.Objects.model + GTA.Offsets.Editor.Objects.NEXT * (ddobjno.SelectedIndex + 1))).Get<int>() != 0)
                    {
                        creatorRefresh();
                    }
                }
            }
        }

        private void ddobjno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetCaptureObj(true);
        }

        private void tbobjlocx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Objects.loc + 0 + GTA.Offsets.Editor.Objects.NEXT * ddobjno.SelectedIndex)).SetFloat(tbobjlocx.Text);
        }

        private void tbobjrotx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Objects.vrot + 0 + GTA.Offsets.Editor.Objects.NEXT * ddobjno.SelectedIndex)).SetFloat(tbobjrotx.Text);
        }

        private void Btnobjgetloc_Click(object sender, RoutedEventArgs e)
        {
            var loc = Functions.Read.GetLocationVec();

            tbobjlocx.Text = loc.X.ToString();
            tbobjlocy.Text = loc.Y.ToString();
            tbobjlocz.Text = loc.Z.ToString();
        }

        public void GetCaptureObj(bool ignore_focus = false)
        {
            int index = ddobjno.SelectedIndex;

            Btnobjgetloc.IsEnabled = index < 0 ? false : true;
            Btnobjrotchain.IsEnabled = index < 0 ? false : true;
            tbobjmodel.IsEnabled = index < 0 ? false : true;
            tbobjlocx.IsEnabled = index < 0 ? false : true;
            tbobjlocy.IsEnabled = index < 0 ? false : true;
            tbobjlocz.IsEnabled = index < 0 ? false : true;
            tbobjrotx.IsEnabled = index < 0 ? false : true;
            tbobjroty.IsEnabled = index < 0 ? false : true;
            tbobjrotz.IsEnabled = index < 0 ? false : true;
            tbobjhead.IsEnabled = index < 0 ? false : true;
            tbobjrender.IsEnabled = index < 0 ? false : true;
            tbobjrule.IsEnabled = index < 0 ? false : true;
            tbobjpriority.IsEnabled = index < 0 ? false : true;
            cb_obj_arrow.IsEnabled = index < 0 ? false : true;
            cb_obj_invisible.IsEnabled = index < 0 ? false : true;
            tbobjobjct.IsEnabled = index < 0 ? false : true;
            ddobjteam.IsEnabled = index < 0 ? false : true;
            tbobjspwn.IsEnabled = index < 0 ? false : true;
            tbobjjtop.IsEnabled = index < 0 ? false : true;
            tbobjjtof.IsEnabled = index < 0 ? false : true;
            tbobjbits.IsEnabled = index < 0 ? false : true;
            tbobjbits2.IsEnabled = index < 0 ? false : true;
            tbobjbits3.IsEnabled = index < 0 ? false : true;
            tbobjbits4.IsEnabled = index < 0 ? false : true;
            tbobjvalu.IsEnabled = index < 0 ? false : true;
            tbobjnmfail.IsEnabled = index < 0 ? false : true;
            tbobjnmpass.IsEnabled = index < 0 ? false : true;
            tbobjcont.IsEnabled = index < 0 ? false : true;
            tbobjhlt.IsEnabled = index < 0 ? false : true;
            tbobjmgbs.IsEnabled = index < 0 ? false : true;
            tbobjped.IsEnabled = index < 0 ? false : true;
            tbobjrsp.IsEnabled = index < 0 ? false : true;
            tbobjobint.IsEnabled = index < 0 ? false : true;
            tbobjobb.IsEnabled = index < 0 ? false : true;
            tbobjobbc.IsEnabled = index < 0 ? false : true;
            tbobjobbs.IsEnabled = index < 0 ? false : true;
            tbobjjtop.IsEnabled = index < 0 ? false : true;
            tbobjjtof.IsEnabled = index < 0 ? false : true;
            tbobjteam.IsEnabled = index < 0 ? false : true;
            tbobjspwn.IsEnabled = index < 0 ? false : true;
            tbobjobjct.IsEnabled = index < 0 ? false : true;

            if (m.IsProcOpen && index > -1)
            {
                int model = new Global(GTA.Offsets.Editor.Objects.model + GTA.Offsets.Editor.Objects.NEXT * index).Get<int>();

                if (!tbobjmodel.IsFocused || ignore_focus) tbobjmodel.Text = model.ToString();
                if (!tbobjlocx.IsFocused || ignore_focus) tbobjlocx.Text = new Global((GTA.Offsets.Editor.Objects.loc + 0 + GTA.Offsets.Editor.Objects.NEXT * index)).Get<float>().ToString();
                if (!tbobjlocy.IsFocused || ignore_focus) tbobjlocy.Text = new Global((GTA.Offsets.Editor.Objects.loc + 1 + GTA.Offsets.Editor.Objects.NEXT * index)).Get<float>().ToString();
                if (!tbobjlocz.IsFocused || ignore_focus) tbobjlocz.Text = new Global((GTA.Offsets.Editor.Objects.loc + 2 + GTA.Offsets.Editor.Objects.NEXT * index)).Get<float>().ToString();
                if (!tbobjrotx.IsFocused || ignore_focus) tbobjrotx.Text = new Global((GTA.Offsets.Editor.Objects.vrot + 0 + GTA.Offsets.Editor.Objects.NEXT * index)).Get<float>().ToString();
                if (!tbobjroty.IsFocused || ignore_focus) tbobjroty.Text = new Global((GTA.Offsets.Editor.Objects.vrot + 1 + GTA.Offsets.Editor.Objects.NEXT * index)).Get<float>().ToString();
                if (!tbobjrotz.IsFocused || ignore_focus) tbobjrotz.Text = new Global((GTA.Offsets.Editor.Objects.vrot + 2 + GTA.Offsets.Editor.Objects.NEXT * index)).Get<float>().ToString();
                if (!tbobjhead.IsFocused || ignore_focus) tbobjhead.Text = new Global((GTA.Offsets.Editor.Objects.head + GTA.Offsets.Editor.Objects.NEXT * index)).Get<float>().ToString();

                if (!tbobjrender.IsFocused || ignore_focus) tbobjrender.Text = new Global((GTA.Offsets.Editor.Objects.objLOD + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                Functions.Read.checkbinary(28, GTA.Offsets.Editor.Objects.bits1 + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT, cb_obj_arrow);
                Functions.Read.checkbinary(1, GTA.Offsets.Editor.Objects.bits1 + 1 + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT, cb_obj_invisible);

                if (!tbobjrule.IsFocused || ignore_focus) tbobjrule.Text = new Global((GTA.Offsets.Editor.Objects.rule + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjpriority.IsFocused || ignore_focus) tbobjpriority.Text = new Global((GTA.Offsets.Editor.Objects.pri + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjjtop.IsFocused || ignore_focus) tbobjjtop.Text = new Global((GTA.Offsets.Editor.Objects.jtop + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjjtof.IsFocused || ignore_focus) tbobjjtof.Text = new Global((GTA.Offsets.Editor.Objects.jtof + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjteam.IsFocused || ignore_focus) tbobjteam.Text = new Global((GTA.Offsets.Editor.Objects.team + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjspwn.IsFocused || ignore_focus) tbobjspwn.Text = new Global((GTA.Offsets.Editor.Objects.spwn + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjobjct.IsFocused || ignore_focus) tbobjobjct.Text = new Global((GTA.Offsets.Editor.Objects.objct + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();

                if (!tbobjbits.IsFocused || ignore_focus) tbobjbits.Text = new Global((GTA.Offsets.Editor.Objects.bits1 + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjbits2.IsFocused || ignore_focus) tbobjbits2.Text = new Global((GTA.Offsets.Editor.Objects.bits2 + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjbits3.IsFocused || ignore_focus) tbobjbits3.Text = new Global((GTA.Offsets.Editor.Objects.bits3 + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjbits4.IsFocused || ignore_focus) tbobjbits4.Text = new Global((GTA.Offsets.Editor.Objects.bits4 + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjvalu.IsFocused || ignore_focus) tbobjvalu.Text = new Global((GTA.Offsets.Editor.Objects.valu + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjnmfail.IsFocused || ignore_focus) tbobjnmfail.Text = new Global((GTA.Offsets.Editor.Objects.nmfail + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjnmpass.IsFocused || ignore_focus) tbobjnmpass.Text = new Global((GTA.Offsets.Editor.Objects.nmpass + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjcont.IsFocused || ignore_focus) tbobjcont.Text = new Global((GTA.Offsets.Editor.Objects.cont + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjhlt.IsFocused || ignore_focus) tbobjhlt.Text = new Global((GTA.Offsets.Editor.Objects.hlt + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjmgbs.IsFocused || ignore_focus) tbobjmgbs.Text = new Global((GTA.Offsets.Editor.Objects.mgbs + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjped.IsFocused || ignore_focus) tbobjped.Text = new Global((GTA.Offsets.Editor.Objects.ped + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjrsp.IsFocused || ignore_focus) tbobjrsp.Text = new Global((GTA.Offsets.Editor.Objects.rsp + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjobint.IsFocused || ignore_focus) tbobjobint.Text = new Global((GTA.Offsets.Editor.Objects.obint + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjobb.IsFocused || ignore_focus) tbobjobb.Text = new Global((GTA.Offsets.Editor.Objects.obb + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjobbc.IsFocused || ignore_focus) tbobjobbc.Text = new Global((GTA.Offsets.Editor.Objects.obbc + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                if (!tbobjobbs.IsFocused || ignore_focus) tbobjobbs.Text = new Global((GTA.Offsets.Editor.Objects.obbs + GTA.Offsets.Editor.Objects.NEXT * index)).Get<float>().ToString();

            }
        }

        private void tbobjhead_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Objects.head + GTA.Offsets.Editor.Objects.NEXT * ddobjno.SelectedIndex)).SetFloat(tbobjhead.Text);
        }

        private void tbobjlocy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Objects.loc + 1 + GTA.Offsets.Editor.Objects.NEXT * ddobjno.SelectedIndex)).SetFloat(tbobjlocy.Text);
        }

        private void tbobjlocz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Objects.loc + 2 + GTA.Offsets.Editor.Objects.NEXT * ddobjno.SelectedIndex)).SetFloat(tbobjlocz.Text);
        }

        private void tbobjroty_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Objects.vrot + 1 + GTA.Offsets.Editor.Objects.NEXT * ddobjno.SelectedIndex)).SetFloat(tbobjroty.Text);
        }

        private void tbobjrotz_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen)
                new Global((GTA.Offsets.Editor.Objects.vrot + 2 + GTA.Offsets.Editor.Objects.NEXT * ddobjno.SelectedIndex)).SetFloat(tbobjrotz.Text);
        }

        private void Btnobjrotchain_Click(object sender, RoutedEventArgs e)
        {
            Binding b = new Binding();
            if (objrotchainactive)
            {
                objrotchain.Source = (BitmapImage)FindResource("link2");
                b.Path = new PropertyPath("Translation[fixedrotn]");
            }
            else
            {
                objrotchain.Source = (BitmapImage)FindResource("link1");
                b.Path = new PropertyPath("Translation[fixedroty]");
            }
            Btnobjrotchain.SetBinding(Button.ToolTipProperty, b);
            objrotchainactive = !objrotchainactive;
        }

        private void tbobjmodel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjmodel.Text))
                new Global(GTA.Offsets.Editor.Objects.model + GTA.Offsets.Editor.Objects.NEXT * ddobjno.SelectedIndex).SetInt(Functions.int_parse(tbobjmodel.Text));
        }

        private void tbobjrule_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && ddobjno.SelectedIndex > -1 && ddobjteam.SelectedIndex > -1 && IsValidInt(tbobjrule.Text))
                new Global((GTA.Offsets.Editor.Objects.rule + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * ddobjno.SelectedIndex)).SetInt(tbobjrule.Text);
        }

        private void tbobjpriority_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m.IsProcOpen && ddobjno.SelectedIndex > -1 && ddobjteam.SelectedIndex > -1 && IsValidInt(tbobjpriority.Text))
                new Global((GTA.Offsets.Editor.Objects.pri + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * ddobjno.SelectedIndex)).SetInt(tbobjpriority.Text);
        }

        private void cb_obj_arrow_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(28, GTA.Offsets.Editor.Objects.bits1 + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT, cb_obj_arrow);
        }

        private void tbobjrender_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Objects.objLOD + GTA.Offsets.Editor.Objects.NEXT * ddobjno.SelectedIndex).SetInt(tbobjrender.Text);
        }

        private void ddobjteam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m.IsProcOpen && ddobjno.SelectedIndex > -1 && ddobjteam.SelectedIndex > -1)
            {
                int index = ddobjno.SelectedIndex;
                tbobjrule.Text = new Global((GTA.Offsets.Editor.Objects.rule + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                tbobjpriority.Text = new Global((GTA.Offsets.Editor.Objects.pri + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                tbobjjtop.Text = new Global((GTA.Offsets.Editor.Objects.jtop + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                tbobjjtof.Text = new Global((GTA.Offsets.Editor.Objects.jtof + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                tbobjteam.Text = new Global((GTA.Offsets.Editor.Objects.team + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                tbobjspwn.Text = new Global((GTA.Offsets.Editor.Objects.spwn + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                tbobjobjct.Text = new Global((GTA.Offsets.Editor.Objects.objct + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * index)).Get<int>().ToString();
                SelectActiveTextBox();
            }
        }



        public static void force_settextfunc()
        {

            while (true)
            {
                new Global(GTA.Offsets.Editor.txt0 + 0 * GTA.Offsets.Editor.NEXT_txt + 1 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[0][0]);
                new Global(GTA.Offsets.Editor.txt0 + 1 * GTA.Offsets.Editor.NEXT_txt + 1 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[1][0]);
                new Global(GTA.Offsets.Editor.txt0 + 2 * GTA.Offsets.Editor.NEXT_txt + 1 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[2][0]);
                new Global(GTA.Offsets.Editor.txt0 + 3 * GTA.Offsets.Editor.NEXT_txt + 1 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[3][0]);
                new Global(GTA.Offsets.Editor.txt0 + 4 * GTA.Offsets.Editor.NEXT_txt + 1 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[4][0]);
                new Global(GTA.Offsets.Editor.txt0 + 5 * GTA.Offsets.Editor.NEXT_txt + 1 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[5][0]);
                new Global(GTA.Offsets.Editor.txt0 + 0 * GTA.Offsets.Editor.NEXT_txt + 2 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[0][1]);
                new Global(GTA.Offsets.Editor.txt0 + 1 * GTA.Offsets.Editor.NEXT_txt + 2 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[1][1]);
                new Global(GTA.Offsets.Editor.txt0 + 2 * GTA.Offsets.Editor.NEXT_txt + 2 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[2][1]);
                new Global(GTA.Offsets.Editor.txt0 + 3 * GTA.Offsets.Editor.NEXT_txt + 2 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[3][1]);
                new Global(GTA.Offsets.Editor.txt0 + 4 * GTA.Offsets.Editor.NEXT_txt + 2 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[4][1]);
                new Global(GTA.Offsets.Editor.txt0 + 5 * GTA.Offsets.Editor.NEXT_txt + 2 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[5][1]);
                new Global(GTA.Offsets.Editor.txt0 + 0 * GTA.Offsets.Editor.NEXT_txt + 3 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[0][2]);
                new Global(GTA.Offsets.Editor.txt0 + 1 * GTA.Offsets.Editor.NEXT_txt + 3 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[1][2]);
                new Global(GTA.Offsets.Editor.txt0 + 2 * GTA.Offsets.Editor.NEXT_txt + 3 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[2][2]);
                new Global(GTA.Offsets.Editor.txt0 + 3 * GTA.Offsets.Editor.NEXT_txt + 3 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[3][2]);
                new Global(GTA.Offsets.Editor.txt0 + 4 * GTA.Offsets.Editor.NEXT_txt + 3 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[4][2]);
                new Global(GTA.Offsets.Editor.txt0 + 5 * GTA.Offsets.Editor.NEXT_txt + 3 * GTA.Offsets.Editor.team_NEXT).SetString(capturetext[5][2]);
                //Functions.Write.writebinarytoaddy(4, addr);
            }
        }

        private void BtnCaptureDelivery_Click(object sender, RoutedEventArgs e)
        {
            PageInnerCapture.SelectedItem = PageInnerCaptureDelivery;
            if (m.IsProcOpen && dddzno.SelectedIndex == -1)
                dddzno.SelectedIndex = 0;
        }

        private void dddzno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetDZValues();
        }

        public void GetDZValues()
        {
            if (dddzno == null || dddzteamno == null)
                return;
            int index = dddzno.SelectedIndex;

            Btndzgetstart.IsEnabled = index < 0 ? false : true;
            Btndzgetend.IsEnabled = index < 0 ? false : true;
            tbdzwidth.IsEnabled = index < 0 ? false : true;
            tbdzheight.IsEnabled = index < 0 ? false : true;
            dddzvariation.IsEnabled = index < 0 ? false : true;
            tbdzstartx.IsEnabled = index < 0 ? false : true;
            tbdzstarty.IsEnabled = index < 0 ? false : true;
            tbdzstartz.IsEnabled = index < 0 ? false : true;
            tbdzendx.IsEnabled = index < 0 ? false : true;
            tbdzendy.IsEnabled = index < 0 ? false : true;
            tbdzendz.IsEnabled = index < 0 ? false : true;

            if (m.IsProcOpen && index > -1)
            {
                tbdzstartx.Text = new Global(GTA.Offsets.Editor.dpos + 0 + 3 * index + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                tbdzstarty.Text = new Global(GTA.Offsets.Editor.dpos + 1 + 3 * index + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                tbdzstartz.Text = new Global(GTA.Offsets.Editor.dpos + 2 + 3 * index + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                tbdzendx.Text = new Global(GTA.Offsets.Editor.dpos2 + 0 + 3 * index + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                tbdzendy.Text = new Global(GTA.Offsets.Editor.dpos2 + 1 + 3 * index + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                tbdzendz.Text = new Global(GTA.Offsets.Editor.dpos2 + 2 + 3 * index + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                tbdzwidth.Text = new Global(GTA.Offsets.Editor.drpr + 1 * index + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                tbdzheight.Text = new Global(GTA.Offsets.Editor.drph + 1 * index + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<float>().ToString();
                int dpost = new Global(GTA.Offsets.Editor.dpost + 1 * index + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).Get<int>();
                if (dpost == 1)
                {
                    dddzvariation.SelectedIndex = 0;
                }
                else if (dpost == 6)
                {
                    dddzvariation.SelectedIndex = 1;
                }

                SelectActiveTextBox();
            }
        }

        private void tbdzstartx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.dpos + 0 + dddzno.SelectedIndex * 3 + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbdzstartx.Text);
        }

        private void tbdzstarty_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.dpos + 1 + dddzno.SelectedIndex * 3 + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbdzstarty.Text);
        }

        private void tbdzstartz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.dpos + 2 + dddzno.SelectedIndex * 3 + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbdzstartz.Text);
        }

        private void tbdzendx_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.dpos2 + 0 + dddzno.SelectedIndex * 3 + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbdzendx.Text);
        }

        private void tbdzendy_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.dpos2 + 1 + dddzno.SelectedIndex * 3 + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbdzendy.Text);
        }

        private void tbdzendz_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.dpos2 + 2 + dddzno.SelectedIndex * 3 + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbdzendz.Text);
        }

        private void Btndzgetend_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var loc = Functions.Read.getlocation();

                tbdzendx.Text = loc[0];
                tbdzendy.Text = loc[1];
                tbdzendz.Text = loc[2];
            }
        }

        private void Btndzgetstart_Click(object sender, RoutedEventArgs e)
        {
            if (m.IsProcOpen)
            {
                var loc = Functions.Read.getlocation();

                tbdzstartx.Text = loc[0];
                tbdzstarty.Text = loc[1];
                tbdzstartz.Text = loc[2];
            }
        }

        private void tbdzwidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.drpr + dddzno.SelectedIndex + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbdzwidth.Text);
        }

        private void tbdzheight_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.drph + dddzno.SelectedIndex + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetFloat(tbdzheight.Text);
        }

        private void dddzvariation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.dpost + dddzno.SelectedIndex + dddzteamno.SelectedIndex * GTA.Offsets.Editor.team_NEXT).SetInt(dddzvariation.SelectedIndex == 0 ? 1 : 6);
        }

        private void cb_obj_invisible_Checked(object sender, RoutedEventArgs e)
        {
            Functions.Write.writebinary(1, GTA.Offsets.Editor.Objects.bits1 + 1 + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT, cb_obj_invisible);
        }

        private void cbcapturetextforce_Checked(object sender, RoutedEventArgs e)
        {
            bool ischecked = cbcapturetextforce.IsChecked ?? true;
            if (ischecked)
            {
                for (int i = 0; i < 6; i++)
                {
                    for (int d = 0; d < 3; d++)
                    {
                        capturetext[i][d] = new Global(GTA.Offsets.Editor.txt0 + i * GTA.Offsets.Editor.NEXT_txt + (d + 1) * GTA.Offsets.Editor.team_NEXT).GetString();
                    }
                }
                force_settext = new Thread(new ThreadStart(force_settextfunc));
                force_settext.Priority = ThreadPriority.Highest;
                force_settext.IsBackground = true;
                force_settext.Start();
            }
            else
            {
                capturetext = new List<string[]>
                                    {
                                        new string[]{ "","","" },
                                        new string[]{ "","","" },
                                        new string[]{ "","","" },
                                        new string[]{ "","","" },
                                        new string[]{ "","","" },
                                        new string[]{ "","","" }
                                    };
                force_settext.Abort();
            }
        }

        private void tbobjteam_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjteam.Text))
                new Global(GTA.Offsets.Editor.Objects.team + ddobjteam.SelectedIndex + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjteam.Text);
        }

        private void tbobjspwn_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjspwn.Text))
                new Global(GTA.Offsets.Editor.Objects.spwn + ddobjteam.SelectedIndex + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjspwn.Text);
        }

        private void tbobjobjcr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjobjcr.Text))
                new Global(GTA.Offsets.Editor.Objects.objcr + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjobjcr.Text);
        }

        private void tbobjobjct_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjobjct.Text))
                new Global(GTA.Offsets.Editor.Objects.objct + ddobjteam.SelectedIndex + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjobjct.Text);
        }

        private void tbobjbits_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjbits.Text))
                new Global(GTA.Offsets.Editor.Objects.bits1 + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjbits.Text);
        }

        private void tbobjvalu_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjvalu.Text))
                new Global(GTA.Offsets.Editor.Objects.valu + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjvalu.Text);
        }

        private void tbobjrsp_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjrsp.Text))
                new Global(GTA.Offsets.Editor.Objects.rsp + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjrsp.Text);
        }

        private void tbobjped_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjped.Text))
                new Global(GTA.Offsets.Editor.Objects.ped + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjped.Text);
        }

        private void tbobjnmfail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjnmfail.Text))
                new Global(GTA.Offsets.Editor.Objects.nmfail + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjnmfail.Text);
        }

        private void tbobjnmpass_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjnmpass.Text))
                new Global(GTA.Offsets.Editor.Objects.nmpass + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjnmpass.Text);
        }

        private void tbobjmgbs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjmgbs.Text))
                new Global(GTA.Offsets.Editor.Objects.mgbs + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjmgbs.Text);
        }

        private void tbobjhlt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjhlt.Text))
                new Global(GTA.Offsets.Editor.Objects.hlt + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjhlt.Text);
        }

        private void tbobjcont_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjcont.Text))
                new Global(GTA.Offsets.Editor.Objects.cont + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjcont.Text);
        }

        private void tbobjbits2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjbits2.Text))
                new Global(GTA.Offsets.Editor.Objects.bits2 + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjbits2.Text);
        }

        private void tbobjbits3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjbits3.Text))
                new Global(GTA.Offsets.Editor.Objects.bits3 + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjbits3.Text);
        }

        private void tbobjbits4_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjbits4.Text))
                new Global(GTA.Offsets.Editor.Objects.bits4 + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjbits4.Text);
        }

        private void tbobjobb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjobb.Text))
                new Global(GTA.Offsets.Editor.Objects.obb + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjobb.Text);
        }

        private void tbobjobbc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjobbc.Text))
                new Global(GTA.Offsets.Editor.Objects.obbc + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjobbc.Text);
        }

        private void tbobjobbs_TextChanged(object sender, TextChangedEventArgs e)
        {
            new Global(GTA.Offsets.Editor.Objects.obbs + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetFloat(tbobjobbs.Text);
        }

        private void tbobjobint_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjobint.Text))
                new Global(GTA.Offsets.Editor.Objects.obint + ddobjno.SelectedIndex * GTA.Offsets.Editor.Objects.NEXT).SetInt(tbobjobint.Text);
        }

        private void tbobjjtop_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjjtop.Text))
                new Global((GTA.Offsets.Editor.Objects.jtop + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * ddobjno.SelectedIndex)).SetInt(tbobjjtop.Text);
        }

        private void tbobjjtof_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidInt(tbobjjtof.Text))
                new Global((GTA.Offsets.Editor.Objects.jtof + ddobjteam.SelectedIndex + GTA.Offsets.Editor.Objects.NEXT * ddobjno.SelectedIndex)).SetInt(tbobjjtof.Text);
        }

        private void dddzteamno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetDZValues();
        }
    }
}
