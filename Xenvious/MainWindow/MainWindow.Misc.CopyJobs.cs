using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xenvious.JSON;
using Xenvious.Logging;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Misc / CopyJobs page.
    public partial class MainWindow
    {
        public void setDescribtion(string describtion)
        {
            if (m.IsProcOpen)
            {
                if (Encoding.UTF8.GetBytes(describtion).Length <= 63)
                {
                    new Global(GTA.Offsets.Editor.dec + 16 * 0).SetBytes(Encoding.UTF8.GetBytes(describtion));
                    new Global(GTA.Offsets.Editor.dec + 16 * 0 + Encoding.UTF8.GetBytes(describtion).Length).SetBytes(new byte[] { 0 });
                }

                if (Encoding.UTF8.GetBytes(describtion).Length > 62 && Encoding.UTF8.GetBytes(describtion).Length < 127)
                {
                    new Global(GTA.Offsets.Editor.dec + 16 * 0).SetBytes(Encoding.UTF8.GetBytes(describtion).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 1).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 1 + Encoding.UTF8.GetBytes(describtion).Skip(63).ToArray().Length).SetBytes(new byte[] { 0 });
                }

                if (Encoding.UTF8.GetBytes(describtion).Length > 125 && Encoding.UTF8.GetBytes(describtion).Length < 190)
                {
                    new Global(GTA.Offsets.Editor.dec + 16 * 0).SetBytes(Encoding.UTF8.GetBytes(describtion).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 1).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(63).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 2).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(126).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 2 + Encoding.UTF8.GetBytes(describtion).Skip(126).ToArray().Length).SetBytes(new byte[] { 0 });
                }

                if (Encoding.UTF8.GetBytes(describtion).Length > 188 && Encoding.UTF8.GetBytes(describtion).Length < 253)
                {
                    new Global(GTA.Offsets.Editor.dec + 16 * 0).SetBytes(Encoding.UTF8.GetBytes(describtion).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 1).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(63).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 2).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(126).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 3).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(189).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 3 + Encoding.UTF8.GetBytes(describtion).Skip(189).ToArray().Length).SetBytes(new byte[] { 0 });
                }

                if (Encoding.UTF8.GetBytes(describtion).Length > 251 && Encoding.UTF8.GetBytes(describtion).Length < 316)
                {
                    new Global(GTA.Offsets.Editor.dec + 16 * 0).SetBytes(Encoding.UTF8.GetBytes(describtion).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 1).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(63).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 2).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(126).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 3).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(189).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 4).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(252).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 4 + Encoding.UTF8.GetBytes(describtion).Skip(252).ToArray().Length).SetBytes(new byte[] { 0 });
                }

                if (Encoding.UTF8.GetBytes(describtion).Length > 314 && Encoding.UTF8.GetBytes(describtion).Length < 379)
                {
                    new Global(GTA.Offsets.Editor.dec + 16 * 0).SetBytes(Encoding.UTF8.GetBytes(describtion).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 1).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(63).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 2).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(126).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 3).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(189).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 4).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(252).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 5).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(315).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 5 + Encoding.UTF8.GetBytes(describtion).Skip(315).ToArray().Length).SetBytes(new byte[] { 0 });
                }

                if (Encoding.UTF8.GetBytes(describtion).Length > 377 && Encoding.UTF8.GetBytes(describtion).Length < 442)
                {
                    new Global(GTA.Offsets.Editor.dec + 16 * 0).SetBytes(Encoding.UTF8.GetBytes(describtion).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 1).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(63).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 2).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(126).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 3).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(189).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 4).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(252).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 5).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(315).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 6).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(378).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 6 + Encoding.UTF8.GetBytes(describtion).Skip(378).ToArray().Length).SetBytes(new byte[] { 0 });
                }

                if (Encoding.UTF8.GetBytes(describtion).Length > 440 && Encoding.UTF8.GetBytes(describtion).Length < 505)
                {
                    new Global(GTA.Offsets.Editor.dec + 16 * 0).SetBytes(Encoding.UTF8.GetBytes(describtion).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 1).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(63).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 2).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(126).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 3).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(189).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 4).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(252).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 5).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(315).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 6).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(378).Take(63).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 7).SetBytes(Encoding.UTF8.GetBytes(describtion).Skip(441).ToArray());
                    new Global(GTA.Offsets.Editor.dec + 16 * 7 + Encoding.UTF8.GetBytes(describtion).Skip(441).ToArray().Length).SetBytes(new byte[] { 0 });
                }
            }
        }

        string jobdata = "";
        string copyJobContentId;
        JSON.Json jobjson = null;
        public async Task<bool> GetJobInfo()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            void Step(string what) => Log.Info($"copy job: {what} ({watch.ElapsedMilliseconds} ms)", source: "copyjob");
            try
            {
                string rootid;
                string details;
                string link = (tbCopyJobLink.Text ?? "").Trim();
                var directJson = Regex.Match(link, @"prod\.cloud\.rockstargames\.com\/ugc\/gta5mission\/\d{4}\/(.{22})\/\d+_\d+_[\w-]+\.json$");
                // A Social Club job link with or without https://, or the bare 22-character job id.
                var socialClub = Regex.Match(link, @"socialclub\.rockstargames\.com\/job\/gtav\/([\w-]{22})");
                if (!socialClub.Success)
                    socialClub = Regex.Match(link, @"^([\w-]{22})$");

                if (directJson.Success)
                {
                    if (!await UrlExistsAsync(link))
                    {
                        SetCopyStatus(TranslateOr("copy_err_link", "Kein gültiger Job-Link."), true);
                        return false;
                    }
                    rootid = directJson.Groups[1].Value;
                    full_link = link;
                    details = await Functions.API.jobdetails(rootid);
                }
                else if (socialClub.Success)
                {
                    rootid = socialClub.Groups[1].Value;
                    Step("details for " + rootid);
                    details = await Functions.API.jobdetails(rootid);
                    Step($"details received, {details?.Length ?? 0} chars");
                    var result = JsonConvert.DeserializeObject<JSON.META.Rootobject>(details);
                    if (result == null || !result.status || result.content == null)
                    {
                        SetCopyStatus(TranslateOr("copy_err_info", "Social Club hat keine Infos zu diesem Job geliefert."), true);
                        return false;
                    }
                    // The job file sits in the same folder as the job image.
                    string image = result.content.imgSrc ?? "";
                    string folder = image.Substring(0, image.LastIndexOf('/') + 1);
                    Step("searching job file in " + folder);
                    full_link = await FindJobJsonAsync(folder);
                    Step("job file " + (full_link ?? "not found"));
                    if (full_link == null)
                    {
                        SetCopyStatus(TranslateOr("copy_err_json", "Die Job-Daten wurden nicht gefunden."), true);
                        return false;
                    }
                }
                else
                {
                    SetCopyStatus(TranslateOr("copy_err_link", "Kein gültiger Job-Link."), true);
                    return false;
                }

                jobdata = await jobFileHttp.GetStringAsync(full_link);
                copyJobContentId = rootid;
                Step($"job file downloaded, {jobdata.Length} chars");

                jobjson = await Task.Run(() => JsonConvert.DeserializeObject<JSON.Json>(jobdata));
                Step("job file parsed");

                var img = JsonConvert.DeserializeObject<JSON.META.Rootobject>(details);

                byte[] bytes = Encoding.Default.GetBytes(jobjson.Mission.Gen.Nm);
                tbCopyName.Text = Encoding.UTF8.GetString(bytes);
                bytes = Encoding.Default.GetBytes(string.Join("", jobjson.Mission.Gen.Dec));
                tbCopyDesc.Text = Encoding.UTF8.GetString(bytes);

                if (!string.IsNullOrEmpty(img?.content?.imgSrc))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(img.content.imgSrc, UriKind.Absolute);
                    bitmap.EndInit();
                    imgCopyJobImage.Source = bitmap;
                }

                if (jobjson.Mission.Prop.No != null) tbCopyPropnum.Content = jobjson.Mission.Prop.No.ToString();
                if (jobjson.Mission.Dprop.No != null) tbCopyDPropnum.Content = jobjson.Mission.Dprop.No.ToString();
                if (jobjson.Mission.Weap.No != null) tbCopyWeapnum.Content = jobjson.Mission.Weap.No.ToString();
                if (jobjson.Mission.Veh.No != null) tbCopyVehnum.Content = jobjson.Mission.Veh.No.ToString();
                if (jobjson.Mission.Race != null) if (jobjson.Mission.Race.Chp != null) tbCopyCPnum.Content = jobjson.Mission.Race.Chp.ToString();
                if (jobjson.Mission.Ene.No != null) tbCopyActornum.Content = jobjson.Mission.Ene.No.ToString();

                //check if meta type is empty
                if (!String.IsNullOrWhiteSpace(img?.content?.type))
                {
                    tbCopyJobType.Content = img.content.type;
                }
                else //manuall job type validation
                {
                    if (int.Parse(jobjson.Mission.Gen.Type.ToString()) == 0 && int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 6)
                    {
                        tbCopyJobType.Content = "Capture";
                    }
                    else if (int.Parse(jobjson.Mission.Gen.Type.ToString()) == 0 && int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 5)
                    {
                        tbCopyJobType.Content = "Last Tean Standing";
                    }
                    else if (int.Parse(jobjson.Mission.Gen.Type.ToString()) == 0)
                    {
                        tbCopyJobType.Content = "Mission";
                    }
                    else if (int.Parse(jobjson.Mission.Gen.Type.ToString()) == 2 && (int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 0 || int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 1))
                    {
                        tbCopyJobType.Content = "Land Race";
                    }
                    else if (int.Parse(jobjson.Mission.Gen.Type.ToString()) == 2 && (int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 2 || int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 3))
                    {
                        tbCopyJobType.Content = "Water Race";
                    }
                    else if (int.Parse(jobjson.Mission.Gen.Type.ToString()) == 2 && (int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 4 || int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 5))
                    {
                        tbCopyJobType.Content = "Air Race";
                    }
                    else if (int.Parse(jobjson.Mission.Gen.Type.ToString()) == 2 && int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 0
                        && (int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 6 || int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 7))
                    {
                        tbCopyJobType.Content = "Stunt Race";
                    }
                    else if (int.Parse(jobjson.Mission.Gen.Type.ToString()) == 2 && int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 0
                        && (int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 8 || int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 9))
                    {
                        tbCopyJobType.Content = "Parachuting";
                    }
                    else if (int.Parse(jobjson.Mission.Gen.Type.ToString()) == 2 && int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 0
                        && (int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 10 || int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 11))
                    {
                        tbCopyJobType.Content = "Foot Race";
                    }
                    else if (int.Parse(jobjson.Mission.Gen.Type.ToString()) == 2 && (int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 18 || int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 19)
                        && (int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 0 || int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 1))
                    {
                        tbCopyJobType.Content = "Target Assault Race";
                    }
                    else if (int.Parse(jobjson.Mission.Gen.Type.ToString()) == 2 && (int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 6 || int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 7)
                        && (int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 20 || int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 21))
                    {
                        tbCopyJobType.Content = "Transform Race";
                    }
                    else if (int.Parse(jobjson.Mission.Gen.Type.ToString()) == 1 && int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 0)
                    {
                        tbCopyJobType.Content = "Deathmatch";
                    }
                    else if (int.Parse(jobjson.Mission.Gen.Type.ToString()) == 1 && int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 1)
                    {
                        tbCopyJobType.Content = "Team Deathmatch";
                    }
                    else if (int.Parse(jobjson.Mission.Gen.Type.ToString()) == 1 && int.Parse(jobjson.Mission.Gen.Subtype.ToString()) == 2)
                    {
                        tbCopyJobType.Content = "Vehicle Deathmatch";
                    }
                    else
                    {
                        tbCopyJobType.Content = "empty";
                    }
                }

                Array weth = new string[] { "Current", "EXTRASUNNY", "Rain", "Snow", "SMOG", "Halloween", "Halloween 2", "Clear", "Clouds", "Overcast", "Thunder", "Foggy" };
                Array tod = new string[] { "Current", "Morning", "Noon", "Night", "Really Dark" };

                tbCopyWeather.Content = weth.GetValue(int.Parse(jobjson.Mission.Rule.Weth.ToString())).ToString();
                tbCopyTOD.Content = tod.GetValue(int.Parse(jobjson.Mission.Rule.Tod.ToString())).ToString();

                Step("preview filled");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("copy job: loading failed", ex, "copyjob");
                SetCopyStatus(TranslateOr("copy_err_load", "Laden fehlgeschlagen:") + " " + ex.Message, true);
                return false;
            }
        }

        private void imgCopyJobImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            img.Source = imgCopyJobImage.Source;
            img.VerticalAlignment = VerticalAlignment.Stretch;
            img.HorizontalAlignment = HorizontalAlignment.Stretch;
            img.Style = (Style)FindResource("PopupImage");

            ScreenMessageContainer.Children.Add(img);
            //IMGBackgroundSource.Source = JobImage.Source;

            ScreenMessage.Visibility = Visibility.Visible;
        }

        bool gettingjobinfo = false;
        private async void tbCopyJobLink_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await LoadCopyJobAsync();
        }

        private async void BtnCopyJobLoad_Click(object sender, RoutedEventArgs e)
        {
            await LoadCopyJobAsync();
        }

        /// <summary>
        /// Fetches the job behind the link. The preview stays visible but greyed out
        /// until a job is loaded, so the page shows what it will offer.
        /// </summary>
        private async Task LoadCopyJobAsync()
        {
            if (gettingjobinfo)
                return;

            gettingjobinfo = true;
            BtnCopyJobLoad.IsEnabled = false;
            SetCopyPreviewEnabled(false);
            ResetCopyPreview();
            SetCopyStatus(null, false);
            CopyAnimation.IsActive = true;
            CopyAnimation.Visibility = Visibility.Visible;

            bool worked = await GetJobInfo();

            CopyAnimation.Visibility = Visibility.Collapsed;
            CopyAnimation.IsActive = false;
            SetCopyPreviewEnabled(worked);
            BtnCopyJobLoad.IsEnabled = true;
            gettingjobinfo = false;
        }

        private void SetCopyPreviewEnabled(bool enabled)
        {
            foreach (var part in new FrameworkElement[] { CopyInnerContent, CopyOptions })
            {
                part.IsEnabled = enabled;
                part.Opacity = enabled ? 1.0 : 0.35;
            }
        }

        private void ResetCopyPreview()
        {
            jobjson = null;
            tbCopyName.Text = "Job";
            tbCopyDesc.Text = "";
            imgCopyJobImage.Source = null;
            foreach (var label in new[] { tbCopyJobType, tbCopyPropnum, tbCopyDPropnum, tbCopyCPnum, tbCopyActornum, tbCopyVehnum, tbCopyWeapnum, tbCopyWeather, tbCopyTOD })
                label.Content = "–";
        }

        /// <summary>One line under the link box; errors in red, the rest in the accent colour.</summary>
        private void SetCopyStatus(string text, bool error)
        {
            tbCopyStatus.Text = text ?? "";
            if (error && !string.IsNullOrEmpty(text))
                Log.Warn("copy job: " + text, source: "copyjob");
            tbCopyStatus.Foreground = new SolidColorBrush(error ? Color.FromRgb(0xD9, 0x53, 0x4F) : Color.FromRgb(0xFA, 0xC8, 0x28));
            tbCopyStatus.Visibility = string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
        }

        private static readonly HttpClient copyHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private static readonly HttpClient jobFileHttp = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        private static async Task<bool> UrlExistsAsync(string url)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Head, url))
                using (var response = await copyHttp.SendAsync(request).ConfigureAwait(false))
                    return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// The job data sits next to the job image as &lt;part&gt;_&lt;version&gt;_&lt;language&gt;.json.
        /// Neither the version nor the language is in the job details, so every language is
        /// tried per version, and once one exists the later versions in that language as
        /// well: the highest one is the current job.
        /// </summary>
        private static async Task<string> FindJobJsonAsync(string folder)
        {
            ServicePointManager.FindServicePoint(new Uri(folder)).ConnectionLimit = 16;
            for (int version = 0; version < 200; version++)
            {
                string[] names = GTA.Editor.country_codes.Select(c => $"{folder}0_{version}_{c}.json").ToArray();
                bool[] exists = await Task.WhenAll(names.Select(UrlExistsAsync));
                int hit = Array.IndexOf(exists, true);
                if (hit < 0)
                    continue;

                string language = GTA.Editor.country_codes[hit];
                string latest = names[hit];
                for (int next = version + 1, misses = 0; misses < 3; next++)
                {
                    string candidate = $"{folder}0_{next}_{language}.json";
                    if (await UrlExistsAsync(candidate))
                    {
                        latest = candidate;
                        misses = 0;
                    }
                    else
                    {
                        misses++;
                    }
                }
                return latest;
            }
            return null;
        }

        /// <summary>Writes the loaded job's image into the creator as its photo.</summary>
        private void CopyJobImage()
        {
            try
            {
                List<byte> temp = ImageSourceToBytes(new JpegBitmapEncoder(), imgCopyJobImage.Source).ToList();

                long offset = GTA.Offsets.Editor.Image.img;
                long addy = m.memory((m.memory(GTA.getIMGPointer().ToInt64()).GetAddress() + 0x18)).Get<long>() + offset;

                byte[] temparray = temp.ToArray();

                m.memory((addy - 0x4).ToString("X")).SetInt(temparray.Count());

                m.memory(addy.ToString("X")).SetBytes(temparray);

                Functions.Write.writebinary(1, GTA.Offsets.Editor.photo, true);   // "photo": bit 0
                new Global(GTA.Offsets.Editor.phpo).SetFloat(jobjson.Mission.Gen.Phpo.X);
                new Global(GTA.Offsets.Editor.phpo + 1).SetFloat(jobjson.Mission.Gen.Phpo.Y);
                new Global(GTA.Offsets.Editor.phpo + 2).SetFloat(jobjson.Mission.Gen.Phpo.Z);
            }
            catch (Exception)
            {
                displayScreenMessage("there was an error trying to copy the image");
            }
        }

        private void BtnCopyJob_Click(object sender, RoutedEventArgs e)
        {
            if (!m.IsProcOpen || !IsCreatorRunning())
            {
                SetCopyStatus(TranslateOr("copy_need_creator", "Öffne zuerst einen Creator in GTA."), true);
                return;
            }
            if (CopyCompleteJob)
            {
                _ = CopyCompleteJobAsync();
                return;
            }
            if (jobjson != null)
            {
                if (cbCopyProps.IsChecked == true)
                {
                    if (jobjson.Mission.Prop != null)
                    {
                        new Global(GTA.Offsets.Editor.Props.number).SetInt((int)jobjson.Mission.Prop.No);
                        for (int i = 0; i < jobjson.Mission.Prop.No; i++)
                        {
                            if (jobjson.Mission.Prop.Model != null) new Global(GTA.Offsets.Editor.Props.model + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Model[i]);
                            if (jobjson.Mission.Prop.Loc != null) new Global(GTA.Offsets.Editor.Props.loc + 0 + (i * GTA.Offsets.Editor.Props.NEXT)).SetFloat((float)jobjson.Mission.Prop.Loc[i].X);
                            if (jobjson.Mission.Prop.Loc != null) new Global(GTA.Offsets.Editor.Props.loc + 1 + (i * GTA.Offsets.Editor.Props.NEXT)).SetFloat((float)jobjson.Mission.Prop.Loc[i].Y);
                            if (jobjson.Mission.Prop.Loc != null) new Global(GTA.Offsets.Editor.Props.loc + 2 + (i * GTA.Offsets.Editor.Props.NEXT)).SetFloat((float)jobjson.Mission.Prop.Loc[i].Z);
                            if (jobjson.Mission.Prop.VRot != null) new Global(GTA.Offsets.Editor.Props.vrot + 0 + (i * GTA.Offsets.Editor.Props.NEXT)).SetFloat((float)jobjson.Mission.Prop.VRot[i].X);
                            if (jobjson.Mission.Prop.VRot != null) new Global(GTA.Offsets.Editor.Props.vrot + 1 + (i * GTA.Offsets.Editor.Props.NEXT)).SetFloat((float)jobjson.Mission.Prop.VRot[i].Y);
                            if (jobjson.Mission.Prop.VRot != null) new Global(GTA.Offsets.Editor.Props.vrot + 2 + (i * GTA.Offsets.Editor.Props.NEXT)).SetFloat((float)jobjson.Mission.Prop.VRot[i].Z);
                            if (jobjson.Mission.Prop.Head != null) new Global(GTA.Offsets.Editor.Props.head + (i * GTA.Offsets.Editor.Props.NEXT)).SetFloat((float)jobjson.Mission.Prop.Head[i]);
                            if (jobjson.Mission.Prop.PLodDist != null) new Global(GTA.Offsets.Editor.Props.ploddist + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.PLodDist[i]);
                            if (jobjson.Mission.Prop.Prpbs != null) new Global(GTA.Offsets.Editor.Props.prpbs + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpbs[i]);
                            if (jobjson.Mission.Prop.Prpbs2 != null) new Global(GTA.Offsets.Editor.Props.prpbs2 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpbs2[i]);

                            if (jobjson.Mission.Prop.Aldel != null) new Global(GTA.Offsets.Editor.Props.aldel + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Aldel[i]);
                            if (jobjson.Mission.Prop.Alsnd != null) new Global(GTA.Offsets.Editor.Props.alsnd + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Alsnd[i]);
                            if (jobjson.Mission.Prop.Alteam != null) new Global(GTA.Offsets.Editor.Props.alteam + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Alteam[i]);
                            if (jobjson.Mission.Prop.Asso != null) new Global(GTA.Offsets.Editor.Props.asso + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Asso[i]);
                            if (jobjson.Mission.Prop.Asso2 != null) new Global(GTA.Offsets.Editor.Props.asso2 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Asso2[i]);
                            if (jobjson.Mission.Prop.Asso3 != null) new Global(GTA.Offsets.Editor.Props.asso3 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Asso3[i]);
                            if (jobjson.Mission.Prop.Asso4 != null) new Global(GTA.Offsets.Editor.Props.asso4 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Asso4[i]);
                            if (jobjson.Mission.Prop.Asss != null) new Global(GTA.Offsets.Editor.Props.asss + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Asss[i]);
                            if (jobjson.Mission.Prop.Asss2 != null) new Global(GTA.Offsets.Editor.Props.asss2 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Asss2[i]);
                            if (jobjson.Mission.Prop.Asss3 != null) new Global(GTA.Offsets.Editor.Props.asss3 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Asss3[i]);
                            if (jobjson.Mission.Prop.Asss4 != null) new Global(GTA.Offsets.Editor.Props.asss4 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Asss4[i]);
                            if (jobjson.Mission.Prop.Asst != null) new Global(GTA.Offsets.Editor.Props.asst + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Asst[i]);
                            if (jobjson.Mission.Prop.Asst2 != null) new Global(GTA.Offsets.Editor.Props.asst2 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Asst2[i]);
                            if (jobjson.Mission.Prop.Asst3 != null) new Global(GTA.Offsets.Editor.Props.asst3 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Asst3[i]);
                            if (jobjson.Mission.Prop.Asst4 != null) new Global(GTA.Offsets.Editor.Props.asst4 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Asst4[i]);
                            if (jobjson.Mission.Prop.Bpbpt != null) new Global(GTA.Offsets.Editor.Props.bpbpt + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Bpbpt[i]);
                            if (jobjson.Mission.Prop.Prpclr != null) new Global(GTA.Offsets.Editor.Props.prpclr + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpclr[i]);
                            if (jobjson.Mission.Prop.Fcuat != null) new Global(GTA.Offsets.Editor.Props.fcuat + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Fcuat[i]);
                            if (jobjson.Mission.Prop.Flcl != null) new Global(GTA.Offsets.Editor.Props.flcl + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Flcl[i]);
                            if (jobjson.Mission.Prop.Flvfx != null) new Global(GTA.Offsets.Editor.Props.flvfx + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Flvfx[i]);
                            if (jobjson.Mission.Prop.FwTeam != null) new Global(GTA.Offsets.Editor.Props.fwTeam + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.FwTeam[i]);
                            if (jobjson.Mission.Prop.FwTPos != null) new Global(GTA.Offsets.Editor.Props.fwTPos + 0 + (i * GTA.Offsets.Editor.Props.NEXT)).SetFloat((float)jobjson.Mission.Prop.FwTPos[i].X);
                            if (jobjson.Mission.Prop.FwTPos != null) new Global(GTA.Offsets.Editor.Props.fwTPos + 1 + (i * GTA.Offsets.Editor.Props.NEXT)).SetFloat((float)jobjson.Mission.Prop.FwTPos[i].Y);
                            if (jobjson.Mission.Prop.FwTPos != null) new Global(GTA.Offsets.Editor.Props.fwTPos + 2 + (i * GTA.Offsets.Editor.Props.NEXT)).SetFloat((float)jobjson.Mission.Prop.FwTPos[i].Z);
                            if (jobjson.Mission.Prop.FwTSize != null) new Global(GTA.Offsets.Editor.Props.fwTSize + (i * GTA.Offsets.Editor.Props.NEXT)).SetFloat((float)jobjson.Mission.Prop.FwTSize[i]);
                            if (jobjson.Mission.Prop.Pasc != null) new Global(GTA.Offsets.Editor.Props.pasc + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Pasc[i]);
                            if (jobjson.Mission.Prop.Pasc2 != null) new Global(GTA.Offsets.Editor.Props.pasc2 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Pasc2[i]);
                            if (jobjson.Mission.Prop.Pasc3 != null) new Global(GTA.Offsets.Editor.Props.pasc3 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Pasc3[i]);
                            if (jobjson.Mission.Prop.Pasc4 != null) new Global(GTA.Offsets.Editor.Props.pasc4 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Pasc4[i]);
                            if (jobjson.Mission.Prop.Pdip != null) new Global(GTA.Offsets.Editor.Props.pdip + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Pdip[i]);
                            if (jobjson.Mission.Prop.Pprst != null) new Global(GTA.Offsets.Editor.Props.pprst + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Pprst[i]);
                            if (jobjson.Mission.Prop.Prpasn != null) new Global(GTA.Offsets.Editor.Props.prpasn + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpasn[i]);
                            if (jobjson.Mission.Prop.Prpatn != null) new Global(GTA.Offsets.Editor.Props.prpatn + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpatn[i]);
                            if (jobjson.Mission.Prop.Prpcl != null) new Global(GTA.Offsets.Editor.Props.prpcl + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpcl[i]);
                            if (jobjson.Mission.Prop.Prpclc != null) new Global(GTA.Offsets.Editor.Props.prpclc + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpclc[i]);
                            if (jobjson.Mission.Prop.Prpclcr != null) new Global(GTA.Offsets.Editor.Props.prpclcr + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpclcr[i]);
                            if (jobjson.Mission.Prop.Prpcr != null) new Global(GTA.Offsets.Editor.Props.prpcr + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpcr[i]);
                            if (jobjson.Mission.Prop.Prpct != null) new Global(GTA.Offsets.Editor.Props.prpct + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpct[i]);
                            if (jobjson.Mission.Prop.Prpdypil != null) new Global(GTA.Offsets.Editor.Props.prpdypil + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpdypil[i]);
                            if (jobjson.Mission.Prop.Prplod != null) new Global(GTA.Offsets.Editor.Props.prplod + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prplod[i]);
                            if (jobjson.Mission.Prop.Prpsba != null) new Global(GTA.Offsets.Editor.Props.prpsba + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpsba[i]);
                            if (jobjson.Mission.Prop.Prpsdp0 != null) new Global(GTA.Offsets.Editor.Props.prpsdp + 0 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpsdp0[i]);
                            if (jobjson.Mission.Prop.Prpsdp1 != null) new Global(GTA.Offsets.Editor.Props.prpsdp + 1 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpsdp1[i]);
                            if (jobjson.Mission.Prop.Prpsdp2 != null) new Global(GTA.Offsets.Editor.Props.prpsdp + 2 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpsdp2[i]);
                            if (jobjson.Mission.Prop.Prpsdp3 != null) new Global(GTA.Offsets.Editor.Props.prpsdp + 3 + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpsdp3[i]);
                            if (jobjson.Mission.Prop.Prpsnpp != null) new Global(GTA.Offsets.Editor.Props.prpsnpp + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prpsnpp[i]);
                            //if (jobjson.Mission.Prop.Prrorc   != null) new Global(GTA.Offsets.Editor.Props.prrorc   + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Prrorc[i]);
                            if (jobjson.Mission.Prop.Ptfxst != null) new Global(GTA.Offsets.Editor.Props.ptfxst + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Ptfxst[i]);
                            if (jobjson.Mission.Prop.Ptfxtr != null) new Global(GTA.Offsets.Editor.Props.ptfxtr + (i * GTA.Offsets.Editor.Props.NEXT)).SetFloat((float)jobjson.Mission.Prop.Ptfxtr[i]);
                            if (jobjson.Mission.Prop.Sndid != null) new Global(GTA.Offsets.Editor.Props.sndid + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Sndid[i]);
                            if (jobjson.Mission.Prop.Sndlmt != null) new Global(GTA.Offsets.Editor.Props.sndlmt + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Sndlmt[i]);
                            if (jobjson.Mission.Prop.Sndtri != null) new Global(GTA.Offsets.Editor.Props.sndtri + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Sndtri[i]);
                            if (jobjson.Mission.Prop.TrPpd != null) new Global(GTA.Offsets.Editor.Props.trppd + (i * GTA.Offsets.Editor.Props.NEXT)).SetFloat((float)jobjson.Mission.Prop.TrPpd[i]);
                            if (jobjson.Mission.Prop.TrTAct != null) new Global(GTA.Offsets.Editor.Props.trtact + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.TrTAct[i]);
                            if (jobjson.Mission.Prop.Ttph != null) new Global(GTA.Offsets.Editor.Props.ttph + (i * GTA.Offsets.Editor.Props.NEXT)).SetFloat((float)jobjson.Mission.Prop.Ttph[i]);
                            if (jobjson.Mission.Prop.Updatez != null) new Global(GTA.Offsets.Editor.Props.updatez + (i * GTA.Offsets.Editor.Props.NEXT)).SetFloat((float)jobjson.Mission.Prop.Updatez[i]);
                            if (jobjson.Mission.Prop.Upddel != null) new Global(GTA.Offsets.Editor.Props.upddel + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Upddel[i]);
                            if (jobjson.Mission.Prop.Updtime != null) new Global(GTA.Offsets.Editor.Props.updtime + (i * GTA.Offsets.Editor.Props.NEXT)).SetInt((int)jobjson.Mission.Prop.Updtime[i]);

                        }
                    }
                    else
                    {
                        displayScreenMessage("there were no props to copy");
                    }
                }

                if (cbCopyDProps.IsChecked == true)
                {
                    if (jobjson.Mission.Dprop != null)
                    {
                        new Global(GTA.Offsets.Editor.DProps.number).SetInt((int)jobjson.Mission.Dprop.No);
                        for (int i = 0; i < jobjson.Mission.Dprop.No; i++)
                        {
                            if (jobjson.Mission.Dprop.Model != null) new Global(GTA.Offsets.Editor.DProps.model + (i * GTA.Offsets.Editor.DProps.NEXT)).SetInt((int)jobjson.Mission.Dprop.Model[i]);
                            if (jobjson.Mission.Dprop.Loc != null) new Global(GTA.Offsets.Editor.DProps.loc + 0 + (i * GTA.Offsets.Editor.DProps.NEXT)).SetFloat((float)jobjson.Mission.Dprop.Loc[i].X);
                            if (jobjson.Mission.Dprop.Loc != null) new Global(GTA.Offsets.Editor.DProps.loc + 1 + (i * GTA.Offsets.Editor.DProps.NEXT)).SetFloat((float)jobjson.Mission.Dprop.Loc[i].Y);
                            if (jobjson.Mission.Dprop.Loc != null) new Global(GTA.Offsets.Editor.DProps.loc + 2 + (i * GTA.Offsets.Editor.DProps.NEXT)).SetFloat((float)jobjson.Mission.Dprop.Loc[i].Z);
                            if (jobjson.Mission.Dprop.VRot != null) new Global(GTA.Offsets.Editor.DProps.vrot + 0 + (i * GTA.Offsets.Editor.DProps.NEXT)).SetFloat((float)jobjson.Mission.Dprop.VRot[i].X);
                            if (jobjson.Mission.Dprop.VRot != null) new Global(GTA.Offsets.Editor.DProps.vrot + 1 + (i * GTA.Offsets.Editor.DProps.NEXT)).SetFloat((float)jobjson.Mission.Dprop.VRot[i].Y);
                            if (jobjson.Mission.Dprop.VRot != null) new Global(GTA.Offsets.Editor.DProps.vrot + 2 + (i * GTA.Offsets.Editor.DProps.NEXT)).SetFloat((float)jobjson.Mission.Dprop.VRot[i].Z);
                            if (jobjson.Mission.Dprop.Head != null) new Global(GTA.Offsets.Editor.DProps.head + (i * GTA.Offsets.Editor.DProps.NEXT)).SetFloat((float)jobjson.Mission.Dprop.Head[i]);
                            if (jobjson.Mission.Dprop.Prpbs != null) new Global(GTA.Offsets.Editor.DProps.prpbs + (i * GTA.Offsets.Editor.DProps.NEXT)).SetInt((int)jobjson.Mission.Dprop.Prpbs[i]);
                            if (jobjson.Mission.Dprop.Dprorc != null) new Global(GTA.Offsets.Editor.DProps.dprorc + (i * GTA.Offsets.Editor.DProps.NEXT)).SetInt((int)jobjson.Mission.Dprop.Dprorc[i]);
                            if (jobjson.Mission.Dprop.Dptrpx != null) new Global(GTA.Offsets.Editor.DProps.dptrpx + (i * GTA.Offsets.Editor.DProps.NEXT)).SetFloat((float)jobjson.Mission.Dprop.Dptrpx[i]);
                            if (jobjson.Mission.Dprop.Obref != null) new Global(GTA.Offsets.Editor.DProps.obref + (i * GTA.Offsets.Editor.DProps.NEXT)).SetInt((int)jobjson.Mission.Dprop.Obref[i]);
                            if (jobjson.Mission.Dprop.Prcra != null) new Global(GTA.Offsets.Editor.DProps.prcra + (i * GTA.Offsets.Editor.DProps.NEXT)).SetInt((int)jobjson.Mission.Dprop.Prcra[i]);
                            if (jobjson.Mission.Dprop.Prpbs != null) new Global(GTA.Offsets.Editor.DProps.prpbs + (i * GTA.Offsets.Editor.DProps.NEXT)).SetInt((int)jobjson.Mission.Dprop.Prpbs[i]);
                            if (jobjson.Mission.Dprop.Prpcr != null) new Global(GTA.Offsets.Editor.DProps.prpcr + (i * GTA.Offsets.Editor.DProps.NEXT)).SetInt((int)jobjson.Mission.Dprop.Prpcr[i]);
                            if (jobjson.Mission.Dprop.Prpct != null) new Global(GTA.Offsets.Editor.DProps.prpct + (i * GTA.Offsets.Editor.DProps.NEXT)).SetInt((int)jobjson.Mission.Dprop.Prpct[i]);
                            if (jobjson.Mission.Dprop.Prpdclr != null) new Global(GTA.Offsets.Editor.DProps.prpdclr + (i * GTA.Offsets.Editor.DProps.NEXT)).SetInt((int)jobjson.Mission.Dprop.Prpdclr[i]);
                            if (jobjson.Mission.Dprop.Prpkt != null) new Global(GTA.Offsets.Editor.DProps.prpkt + (i * GTA.Offsets.Editor.DProps.NEXT)).SetInt((int)jobjson.Mission.Dprop.Prpkt[i]);
                        }
                    }
                    else
                    {
                        displayScreenMessage("there were no dynamic props to copy");
                    }
                }

                if (cbCopyImage.IsChecked == true)
                    CopyJobImage();

                if (cbCopyCP.IsChecked == true)
                {
                    //shift arrays to left

                    if (jobjson.Mission.Race != null)
                    {


                        List<XenVector3> temp = new List<XenVector3>();
                        List<XenVector3> temp1 = new List<XenVector3>();
                        List<XenVector3> temp2 = new List<XenVector3>();
                        List<XenVector3> temp3 = new List<XenVector3>();
                        List<XenVector3> temp4 = new List<XenVector3>();
                        List<XenVector3> temp5 = new List<XenVector3>();
                        List<XenVector3> temp6 = new List<XenVector3>();
                        List<XenVector3> temp7 = new List<XenVector3>();
                        List<XenVector3> temp8 = new List<XenVector3>();
                        List<XenVector3> temp9 = new List<XenVector3>();
                        List<double> temp10 = new List<double>();
                        List<double> temp11 = new List<double>();
                        List<double> temp12 = new List<double>();
                        List<double> temp13 = new List<double>();
                        List<double> temp14 = new List<double>();
                        List<long> temp15 = new List<long>();
                        List<long> temp16 = new List<long>();
                        List<long> temp17 = new List<long>();
                        List<long> temp18 = new List<long>();
                        List<long> temp19 = new List<long>();
                        List<long> temp20 = new List<long>();
                        List<long> temp21 = new List<long>();


                        if (jobjson.Mission.Race.Chl != null) temp.AddRange(jobjson.Mission.Race.Chl);
                        if (jobjson.Mission.Race.Cpado != null) temp1.AddRange(jobjson.Mission.Race.Cpado);
                        if (jobjson.Mission.Race.Cpados != null) temp2.AddRange(jobjson.Mission.Race.Cpados);
                        if (jobjson.Mission.Race.Sndchk != null) temp3.AddRange(jobjson.Mission.Race.Sndchk);
                        if (jobjson.Mission.Race.Vspn0 != null) temp4.AddRange(jobjson.Mission.Race.Vspn0);
                        if (jobjson.Mission.Race.Vspn1 != null) temp5.AddRange(jobjson.Mission.Race.Vspn1);
                        if (jobjson.Mission.Race.Vspn2 != null) temp6.AddRange(jobjson.Mission.Race.Vspn2);
                        if (jobjson.Mission.Race.Vspns0 != null) temp7.AddRange(jobjson.Mission.Race.Vspns0);
                        if (jobjson.Mission.Race.Vspns1 != null) temp8.AddRange(jobjson.Mission.Race.Vspns1);
                        if (jobjson.Mission.Race.Vspns2 != null) temp9.AddRange(jobjson.Mission.Race.Vspns2);
                        if (jobjson.Mission.Race.Chs != null) temp10.AddRange(jobjson.Mission.Race.Chs);
                        if (jobjson.Mission.Race.Chs2 != null) temp11.AddRange(jobjson.Mission.Race.Chs2);
                        if (jobjson.Mission.Race.Chh != null) temp12.AddRange(jobjson.Mission.Race.Chh);
                        if (jobjson.Mission.Race.Sndrsp != null) temp13.AddRange(jobjson.Mission.Race.Sndrsp);
                        if (jobjson.Mission.Race.Chvs != null) temp14.AddRange(jobjson.Mission.Race.Chvs);
                        if (jobjson.Mission.Race.Cpbs1 != null) temp15.AddRange(jobjson.Mission.Race.Cpbs1);
                        if (jobjson.Mission.Race.Cpbs2 != null) temp16.AddRange(jobjson.Mission.Race.Cpbs2);
                        if (jobjson.Mission.Race.Chttr != null) temp17.AddRange(jobjson.Mission.Race.Chttr);
                        if (jobjson.Mission.Race.Chttu != null) temp18.AddRange(jobjson.Mission.Race.Chttu);
                        if (jobjson.Mission.Race.Cpwwt != null) temp19.AddRange(jobjson.Mission.Race.Cpwwt);
                        if (jobjson.Mission.Race.Cptfrm != null) temp20.AddRange(jobjson.Mission.Race.Cptfrm);
                        if (jobjson.Mission.Race.Cptfrms != null) temp21.AddRange(jobjson.Mission.Race.Cptfrms);


                        if (jobjson.Mission.Race.Ptp == 0)
                        {
                            if (jobjson.Mission.Race.Chl != null) right(temp);
                            if (jobjson.Mission.Race.Cpado != null) right(temp1);
                            if (jobjson.Mission.Race.Cpados != null) right(temp2);
                            if (jobjson.Mission.Race.Sndchk != null) right(temp3);
                            if (jobjson.Mission.Race.Vspn0 != null) right(temp4);
                            if (jobjson.Mission.Race.Vspn1 != null) right(temp5);
                            if (jobjson.Mission.Race.Vspn2 != null) right(temp6);
                            if (jobjson.Mission.Race.Vspns0 != null) right(temp7);
                            if (jobjson.Mission.Race.Vspns1 != null) right(temp8);
                            if (jobjson.Mission.Race.Vspns2 != null) right(temp9);
                            if (jobjson.Mission.Race.Chs != null) right(temp10);
                            if (jobjson.Mission.Race.Chs2 != null) right(temp11);
                            if (jobjson.Mission.Race.Chh != null) right(temp12);
                            if (jobjson.Mission.Race.Sndrsp != null) right(temp13);
                            if (jobjson.Mission.Race.Chvs != null) right(temp14);
                            if (jobjson.Mission.Race.Cpbs1 != null) right(temp15);
                            if (jobjson.Mission.Race.Cpbs2 != null) right(temp16);
                            if (jobjson.Mission.Race.Chttr != null) right(temp17);
                            if (jobjson.Mission.Race.Chttu != null) right(temp18);
                            if (jobjson.Mission.Race.Cpwwt != null) right(temp19);
                            if (jobjson.Mission.Race.Cptfrm != null) right(temp20);
                            if (jobjson.Mission.Race.Cptfrms != null) right(temp21);
                        }


                        new Global(GTA.Offsets.Editor.Race.Checkpoints.number).SetInt((int)jobjson.Mission.Race.Chp);
                        for (int i = 0; i < jobjson.Mission.Race.Chp; i++)
                        {
                            // Checkpoint
                            if (jobjson.Mission.Race.Chl != null)
                            {
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.locx + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp[i].X);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.locy + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp[i].Y);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.locz + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp[i].Z);
                            }

                            if (jobjson.Mission.Race.Cpado != null)
                            {
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.cpado + 0 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp1[i].X);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.cpado + 1 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp1[i].Y);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.cpado + 2 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp1[i].Z);
                            }

                            if (jobjson.Mission.Race.Cpados != null)
                            {
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.cpados + 0 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp2[i].X);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.cpados + 1 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp2[i].Y);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.cpados + 2 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp2[i].Z);
                            }

                            // Secondary CP
                            if (jobjson.Mission.Race.Sndchk != null)
                            {
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.sndchk + 0 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp3[i].X);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.sndchk + 1 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp3[i].Y);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.sndchk + 2 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp3[i].Z);
                            }

                            // CP Respawn
                            if (jobjson.Mission.Race.Vspn0 != null)
                            {
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspn + 0 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp4[i].X);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspn + 1 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp4[i].Y);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspn + 2 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp4[i].Z);
                            }
                            if (jobjson.Mission.Race.Vspn1 != null)
                            {
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspn + 3 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp5[i].X);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspn + 4 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp5[i].Y);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspn + 5 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp5[i].Z);
                            }
                            if (jobjson.Mission.Race.Vspn2 != null)
                            {
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspn + 6 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp6[i].X);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspn + 7 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp6[i].Y);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspn + 8 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp6[i].Z);
                            }

                            // Secondary CP Respawn
                            if (jobjson.Mission.Race.Vspns0 != null)
                            {
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspns + 0 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp7[i].X);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspns + 1 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp7[i].Y);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspns + 2 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp7[i].Z);
                            }
                            if (jobjson.Mission.Race.Vspns1 != null)
                            {
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspns + 3 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp8[i].X);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspns + 4 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp8[i].Y);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspns + 5 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp8[i].Z);
                            }
                            if (jobjson.Mission.Race.Vspns2 != null)
                            {
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspns + 6 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp9[i].X);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspns + 7 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp9[i].Y);
                                new Global(GTA.Offsets.Editor.Race.Checkpoints.vspns + 8 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat(temp9[i].Z);
                            }



                            if (jobjson.Mission.Race.Chs != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.chs + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat((float)temp10[i]);
                            if (jobjson.Mission.Race.Chs2 != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.chs2 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat((float)temp11[i]);
                            if (jobjson.Mission.Race.Chh != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.chh + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat((float)temp12[i]);
                            if (jobjson.Mission.Race.Sndrsp != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.sndrsp + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat((float)temp13[i]);
                            if (jobjson.Mission.Race.Chvs != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.chvs + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetFloat((float)temp14[i]);
                            if (jobjson.Mission.Race.Cpbs1 != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.cpbs1 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetInt((int)temp15[i]);
                            if (jobjson.Mission.Race.Cpbs2 != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.cpbs2 + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetInt((int)temp16[i]);
                            if (jobjson.Mission.Race.Chttr != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.chttr + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetInt((int)temp17[i]);
                            if (jobjson.Mission.Race.Chttu != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.chttu + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetInt((int)temp18[i]);
                            if (jobjson.Mission.Race.Cpwwt != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.cpwwt + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetInt((int)temp19[i]);
                            if (jobjson.Mission.Race.Cptfrm != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.cptfrm + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetInt((int)temp20[i]);
                            if (jobjson.Mission.Race.Cptfrms != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.cptfrms + (i * GTA.Offsets.Editor.Race.Checkpoints.NEXT)).SetInt((int)temp21[i]);
                        }

                        temp = null;
                        temp1 = null;
                        temp2 = null;
                        temp3 = null;
                        temp4 = null;
                        temp5 = null;
                        temp6 = null;
                        temp7 = null;
                        temp8 = null;
                        temp9 = null;
                        temp10 = null;
                        temp11 = null;
                        temp12 = null;
                        temp13 = null;
                        temp14 = null;
                        temp15 = null;
                        temp16 = null;
                        temp17 = null;
                        temp18 = null;
                        temp19 = null;
                        temp20 = null;
                        temp21 = null;

                        #region Grid


                        new Global(GTA.Offsets.Editor.num).SetInt((int)jobjson.Mission.Gen.Num - (jobjson.Mission.Gen.Type == 2 ? 2 : 1));
                        new Global(GTA.Offsets.Editor.Vehicle.number).SetInt((int)jobjson.Mission.Veh.No);
                        for (int i = 0; i < jobjson.Mission.Veh.No; i++)
                        {
                            if (jobjson.Mission.Veh.Loc != null)
                            {
                                new Global(GTA.Offsets.Editor.Vehicle.loc + 0 + (i * GTA.Offsets.Editor.Vehicle.NEXT)).SetFloat(jobjson.Mission.Veh.Loc[i].X);
                                new Global(GTA.Offsets.Editor.Vehicle.loc + 1 + (i * GTA.Offsets.Editor.Vehicle.NEXT)).SetFloat(jobjson.Mission.Veh.Loc[i].Y);
                                new Global(GTA.Offsets.Editor.Vehicle.loc + 2 + (i * GTA.Offsets.Editor.Vehicle.NEXT)).SetFloat(jobjson.Mission.Veh.Loc[i].Z);
                                if (jobjson.Mission.Veh.Head != null)
                                    new Global(GTA.Offsets.Editor.Vehicle.head + (i * GTA.Offsets.Editor.Vehicle.NEXT)).SetFloat((float)jobjson.Mission.Veh.Head[i]);
                            }
                        }

                        #endregion

                        #region Available Vehicles

                        if (jobjson.Mission.Race.Aveh != null)
                        {
                            for (int i = 0; i < jobjson.Mission.Race.Aveh.Count(); i++)
                            {
                                new Global(GTA.Offsets.Editor.Race.aveh + i).SetInt((int)jobjson.Mission.Race.Aveh[i]);
                            }
                        }
                        if (jobjson.Mission.Race.Adlc != null)
                        {
                            for (int i = 0; i < jobjson.Mission.Race.Adlc.Count(); i++)
                            {
                                new Global(GTA.Offsets.Editor.Race.adlc + (i * GTA.Offsets.Editor.Race.adlc_NEXT)).SetInt((int)jobjson.Mission.Race.Adlc[i]);
                            }
                        }
                        if (jobjson.Mission.Race.Adlc2 != null)
                        {
                            for (int i = 0; i < jobjson.Mission.Race.Adlc2.Count(); i++)
                            {
                                new Global(GTA.Offsets.Editor.Race.adlc2 + (i * GTA.Offsets.Editor.Race.adlc_NEXT)).SetInt((int)jobjson.Mission.Race.Adlc2[i]);
                            }
                        }
                        if (jobjson.Mission.Race.Adlc3 != null)
                        {
                            for (int i = 0; i < jobjson.Mission.Race.Adlc3.Count(); i++)
                            {
                                new Global(GTA.Offsets.Editor.Race.adlc3 + (i * GTA.Offsets.Editor.Race.adlc_NEXT)).SetInt((int)jobjson.Mission.Race.Adlc3[i]);
                            }
                        }

                        if (jobjson.Mission.Race.Clbs != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.clbs).SetInt((int)jobjson.Mission.Race.Clbs);
                        if (jobjson.Mission.Race.Icv != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.icv).SetInt((int)jobjson.Mission.Race.Icv);

                        #endregion

                        #region Transform Vehicles

                        if (jobjson.Mission.Race.Trfmvm != null)
                        {
                            long offset = GTA.Offsets.Editor.Race.Checkpoints.trfmvm - 1;
                            for (int i = 0; i < jobjson.Mission.Race.Trfmvm.Count(); i++)
                            {
                                new Global(offset + i).SetInt((int)jobjson.Mission.Race.Trfmvm[i]);
                            }
                        }

                        #endregion

                        //trigger
                        var loc = jobjson.Mission.Gen.Start;

                        tbstartlocx.Text = loc.X.ToString();
                        tbstartlocy.Text = loc.Y.ToString();
                        tbstartlocz.Text = loc.Z.ToString();

                        if (jobjson.Mission.Race.Grid != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.grid + 0).SetFloat((float)jobjson.Mission.Race.Grid.X);
                        if (jobjson.Mission.Race.Grid != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.grid + 1).SetFloat((float)jobjson.Mission.Race.Grid.Y);
                        if (jobjson.Mission.Race.Grid != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.grid + 2).SetFloat((float)jobjson.Mission.Race.Grid.Z);

                        if (jobjson.Mission.Race.Scene != null) new Global(GTA.Offsets.Editor.scene + 0).SetFloat((float)jobjson.Mission.Race.Scene.X);
                        if (jobjson.Mission.Race.Scene != null) new Global(GTA.Offsets.Editor.scene + 1).SetFloat((float)jobjson.Mission.Race.Scene.Y);
                        if (jobjson.Mission.Race.Scene != null) new Global(GTA.Offsets.Editor.scene + 2).SetFloat((float)jobjson.Mission.Race.Scene.Z);

                        if (jobjson.Mission.Race.Gridty != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.gridty).SetInt((int)jobjson.Mission.Race.Gridty);
                        if (jobjson.Mission.Race.Gtar != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.gtar).SetInt((int)jobjson.Mission.Race.Gtar);
                        if (jobjson.Mission.Race.Lap != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.lap).SetInt((int)jobjson.Mission.Race.Lap);
                        if (jobjson.Mission.Race.Lrgs != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.lrgs).SetFloat((float)jobjson.Mission.Race.Lrgs);
                        if (jobjson.Mission.Race.Udgs != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.udgs).SetFloat((float)jobjson.Mission.Race.Udgs);
                        if (jobjson.Mission.Race.Gw != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.gw).SetFloat((float)jobjson.Mission.Race.Gw);
                        if (jobjson.Mission.Race.Gl != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.gl).SetFloat((float)jobjson.Mission.Race.Gl);
                        if (jobjson.Mission.Race.Lanes != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.lanes).SetInt((int)jobjson.Mission.Race.Lanes);
                        if (jobjson.Mission.Race.Ptp != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.ptp).SetInt((int)jobjson.Mission.Race.Ptp);
                        if (jobjson.Mission.Race.Strtg != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.strtg).SetInt((int)jobjson.Mission.Race.Strtg);
                        if (jobjson.Mission.Race.Sgdo != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.sgdo).SetInt((int)jobjson.Mission.Race.Sgdo);
                        if (jobjson.Mission.Race.Rdis != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.rdis).SetFloat((float)jobjson.Mission.Race.Rdis);
                        if (jobjson.Mission.Race.Tri1 != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.tri1).SetInt((int)jobjson.Mission.Race.Tri1);
                        if (jobjson.Mission.Race.Tri2 != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.tri2).SetInt((int)jobjson.Mission.Race.Tri2);
                        if (jobjson.Mission.Race.Type != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.type).SetInt((int)jobjson.Mission.Race.Type);
                        if (jobjson.Mission.Race.Head != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.head).SetFloat((float)jobjson.Mission.Race.Head);
                        if (jobjson.Mission.Race.Iprem != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.iprem).SetInt(jobjson.Mission.Race.Iprem == true ? 1 : 0);
                        if (jobjson.Mission.Race.Bsted != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.bsted).SetInt((int)jobjson.Mission.Race.Bsted);
                        if (jobjson.Mission.Race.Retl != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.retl).SetInt((int)jobjson.Mission.Race.Retl);
                        if (jobjson.Mission.Race.Cemn != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.cemn).SetInt((int)jobjson.Mission.Race.Cemn);
                        if (jobjson.Mission.Race.Cemx != null) new Global(GTA.Offsets.Editor.Race.Checkpoints.cemx).SetInt((int)jobjson.Mission.Race.Cemx);
                        if (jobjson.Mission.Race.Subtype != null) new Global(GTA.Offsets.Editor.racetype).SetInt((int)jobjson.Mission.Race.Subtype);
                        if (jobjson.Mission.Gen.Ivm != null) new Global(GTA.Offsets.Editor.ivm).SetInt((int)jobjson.Mission.Gen.Ivm);
                    }
                    else
                    {
                        displayScreenMessage("there were no checkpoints to copy");
                    }
                }

                if (cbCopyMenubs.IsChecked == true)
                {
                    if (jobjson.Mission.Gen.Menubs != null) new Global(GTA.Offsets.Editor.menubs).SetInt((int)jobjson.Mission.Gen.Menubs);
                    if (jobjson.Mission.Gen.Menubs2 != null) new Global(GTA.Offsets.Editor.menubs2).SetInt((int)jobjson.Mission.Gen.Menubs2);
                    if (jobjson.Mission.Gen.Menubs3 != null) new Global(GTA.Offsets.Editor.menubs3).SetInt((int)jobjson.Mission.Gen.Menubs3);
                    if (jobjson.Mission.Gen.Menubs4 != null) new Global(GTA.Offsets.Editor.menubs4).SetInt((int)jobjson.Mission.Gen.Menubs4);
                    if (jobjson.Mission.Gen.Menubs5 != null) new Global(GTA.Offsets.Editor.menubs5).SetInt((int)jobjson.Mission.Gen.Menubs5);
                    if (jobjson.Mission.Gen.Menubs6 != null) new Global(GTA.Offsets.Editor.menubs6).SetInt((int)jobjson.Mission.Gen.Menubs6);
                    if (jobjson.Mission.Gen.Menubs7 != null) new Global(GTA.Offsets.Editor.menubs7).SetInt((int)jobjson.Mission.Gen.Menubs7);
                    if (jobjson.Mission.Gen.Menubs8 != null) new Global(GTA.Offsets.Editor.menubs8).SetInt((int)jobjson.Mission.Gen.Menubs8);
                    if (jobjson.Mission.Gen.Menubs9 != null) new Global(GTA.Offsets.Editor.menubs9).SetInt((int)jobjson.Mission.Gen.Menubs9);
                    if (jobjson.Mission.Gen.Menubs10 != null) new Global(GTA.Offsets.Editor.menubs10).SetInt((int)jobjson.Mission.Gen.Menubs10);
                    if (jobjson.Mission.Gen.Menubs11 != null) new Global(GTA.Offsets.Editor.menubs11).SetInt((int)jobjson.Mission.Gen.Menubs11);
                    if (jobjson.Mission.Gen.Menubs12 != null) new Global(GTA.Offsets.Editor.menubs12).SetInt((int)jobjson.Mission.Gen.Menubs12);
                    if (jobjson.Mission.Gen.Menubs13 != null) new Global(GTA.Offsets.Editor.menubs13).SetInt((int)jobjson.Mission.Gen.Menubs13);
                    if (jobjson.Mission.Gen.Menubs14 != null) new Global(GTA.Offsets.Editor.menubs14).SetInt((int)jobjson.Mission.Gen.Menubs14);
                    if (jobjson.Mission.Gen.Menubs15 != null) new Global(GTA.Offsets.Editor.menubs15).SetInt((int)jobjson.Mission.Gen.Menubs15);
                    if (jobjson.Mission.Gen.Menubs16 != null) new Global(GTA.Offsets.Editor.menubs16).SetInt((int)jobjson.Mission.Gen.Menubs16);
                    if (jobjson.Mission.Gen.Menubs17 != null) new Global(GTA.Offsets.Editor.menubs17).SetInt((int)jobjson.Mission.Gen.Menubs17);
                    if (jobjson.Mission.Gen.Menubs18 != null) new Global(GTA.Offsets.Editor.menubs18).SetInt((int)jobjson.Mission.Gen.Menubs18);
                    if (jobjson.Mission.Gen.Menubs19 != null) new Global(GTA.Offsets.Editor.menubs19).SetInt((int)jobjson.Mission.Gen.Menubs19);
                    if (jobjson.Mission.Gen.Menubs20 != null) new Global(GTA.Offsets.Editor.menubs20).SetInt((int)jobjson.Mission.Gen.Menubs20);
                    if (jobjson.Mission.Gen.Menubs21 != null) new Global(GTA.Offsets.Editor.menubs21).SetInt((int)jobjson.Mission.Gen.Menubs21);
                    if (jobjson.Mission.Gen.Menubs22 != null) new Global(GTA.Offsets.Editor.menubs22).SetInt((int)jobjson.Mission.Gen.Menubs22);
                    if (jobjson.Mission.Gen.Menubs23 != null) new Global(GTA.Offsets.Editor.menubs23).SetInt((int)jobjson.Mission.Gen.Menubs23);
                    if (jobjson.Mission.Gen.Menubs24 != null) new Global(GTA.Offsets.Editor.menubs24).SetInt((int)jobjson.Mission.Gen.Menubs24);
                    if (jobjson.Mission.Gen.Menubs25 != null) new Global(GTA.Offsets.Editor.menubs25).SetInt((int)jobjson.Mission.Gen.Menubs25);
                    if (jobjson.Mission.Gen.Menubs26 != null) new Global(GTA.Offsets.Editor.menubs26).SetInt((int)jobjson.Mission.Gen.Menubs26);
                    if (jobjson.Mission.Gen.Menubs27 != null) new Global(GTA.Offsets.Editor.menubs27).SetInt((int)jobjson.Mission.Gen.Menubs27);
                    if (jobjson.Mission.Gen.Menubs28 != null) new Global(GTA.Offsets.Editor.menubs28).SetInt((int)jobjson.Mission.Gen.Menubs28);
                    if (jobjson.Mission.Gen.Menubs29 != null) new Global(GTA.Offsets.Editor.menubs29).SetInt((int)jobjson.Mission.Gen.Menubs29);
                }

                if (cbCopyTemplates.IsChecked == true)
                {
                    if (jobjson.Mission.Ptemp != null)
                    {
                        new Global(GTA.Offsets.Editor.PTemp.number).SetInt((int)jobjson.Mission.Ptemp.No);
                        for (int i = 0; i < jobjson.Mission.Ptemp.No; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    if (jobjson.Mission.Ptemp.Ptc0 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptc0.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptc + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptc0[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptm0 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptm0.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptm + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptm0[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Pto0 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto0.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto0[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto0[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto0[d].Z);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptr0 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto0.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr0[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr0[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr0[d].Z);
                                        }
                                    }
                                    break;
                                case 1:
                                    if (jobjson.Mission.Ptemp.Ptc1 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptc1.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptc + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptc1[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptm1 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptm1.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptm + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptm1[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Pto1 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto1.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto1[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto1[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto1[d].Z);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptr1 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto1.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr1[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr1[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr1[d].Z);
                                        }
                                    }
                                    break;
                                case 2:
                                    if (jobjson.Mission.Ptemp.Ptc2 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptc2.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptc + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptc2[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptm2 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptm2.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptm + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptm2[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Pto2 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto2.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto2[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto2[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto2[d].Z);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptr2 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto2.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr2[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr2[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr2[d].Z);
                                        }
                                    }
                                    break;
                                case 3:
                                    if (jobjson.Mission.Ptemp.Ptc3 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptc3.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptc + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptc3[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptm3 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptm3.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptm + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptm3[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Pto3 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto3.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto3[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto3[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto3[d].Z);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptr3 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto3.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr3[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr3[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr3[d].Z);
                                        }
                                    }
                                    break;
                                case 4:
                                    if (jobjson.Mission.Ptemp.Ptc4 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptc4.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptc + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptc4[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptm4 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptm4.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptm + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptm4[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Pto4 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto4.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto4[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto4[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto4[d].Z);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptr4 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto4.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr4[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr4[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr4[d].Z);
                                        }
                                    }
                                    break;
                                case 5:
                                    if (jobjson.Mission.Ptemp.Ptc5 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptc5.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptc + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptc5[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptm5 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptm5.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptm + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptm5[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Pto5 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto5.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto5[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto5[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto5[d].Z);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptr5 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto5.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr5[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr5[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr5[d].Z);
                                        }
                                    }
                                    break;
                                case 6:
                                    if (jobjson.Mission.Ptemp.Ptc6 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptc6.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptc + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptc6[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptm6 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptm6.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptm + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptm6[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Pto6 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto6.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto6[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto6[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto6[d].Z);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptr6 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto6.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr6[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr6[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr6[d].Z);
                                        }
                                    }
                                    break;
                                case 7:
                                    if (jobjson.Mission.Ptemp.Ptc7 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptc7.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptc + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptc7[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptm7 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptm7.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptm + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptm7[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Pto7 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto7.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto7[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto7[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto7[d].Z);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptr7 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto7.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr7[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr7[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr7[d].Z);
                                        }
                                    }
                                    break;
                                case 8:
                                    if (jobjson.Mission.Ptemp.Ptc8 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptc8.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptc + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptc8[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptm8 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptm8.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptm + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptm8[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Pto8 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto8.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto8[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto8[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto8[d].Z);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptr8 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto8.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr8[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr8[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr8[d].Z);
                                        }
                                    }
                                    break;
                                case 9:
                                    if (jobjson.Mission.Ptemp.Ptc9 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptc9.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptc + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptc9[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptm9 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Ptm9.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptm + d + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetInt((int)jobjson.Mission.Ptemp.Ptm9[d]);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Pto9 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto9.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto9[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto9[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.pto + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Pto9[d].Z);
                                        }
                                    }
                                    if (jobjson.Mission.Ptemp.Ptr9 != null)
                                    {
                                        for (int d = 0; d < jobjson.Mission.Ptemp.Pto9.Count; d++)
                                        {
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 0 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr9[d].X);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 1 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr9[d].Y);
                                            new Global((GTA.Offsets.Editor.PTemp.ptr + 2 + (d * 3) + (GTA.Offsets.Editor.PTemp.NEXT * i))).SetFloat(jobjson.Mission.Ptemp.Ptr9[d].Z);
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        displayScreenMessage("there were no templates to copy");
                    }
                }

                if (cbCopyDHProps.IsChecked == true)
                {
                    if (jobjson.Mission.Dhprop != null)
                    {
                        new Global(GTA.Offsets.Editor.DHProp.number).SetInt((int)jobjson.Mission.Dhprop.No);
                        for (int i = 0; i < jobjson.Mission.Dhprop.No; i++)
                        {
                            if (jobjson.Mission.Dhprop.Mn != null) new Global(GTA.Offsets.Editor.DHProp.model + (i * GTA.Offsets.Editor.DHProp.NEXT)).SetInt((int)jobjson.Mission.Dhprop.Mn[i]);
                            if (jobjson.Mission.Dhprop.Bits != null) new Global(GTA.Offsets.Editor.DHProp.bits + (i * GTA.Offsets.Editor.DHProp.NEXT)).SetFloat((int)jobjson.Mission.Dhprop.Bits[i]);
                            if (jobjson.Mission.Dhprop.Pos != null) new Global(GTA.Offsets.Editor.DHProp.locx + (i * GTA.Offsets.Editor.DHProp.NEXT)).SetFloat((float)jobjson.Mission.Dhprop.Pos[i].X);
                            if (jobjson.Mission.Dhprop.Pos != null) new Global(GTA.Offsets.Editor.DHProp.locy + (i * GTA.Offsets.Editor.DHProp.NEXT)).SetFloat((float)jobjson.Mission.Dhprop.Pos[i].Y);
                            if (jobjson.Mission.Dhprop.Pos != null) new Global(GTA.Offsets.Editor.DHProp.locz + (i * GTA.Offsets.Editor.DHProp.NEXT)).SetFloat((float)jobjson.Mission.Dhprop.Pos[i].Z);

                        }
                    }
                    else
                    {
                        displayScreenMessage("there were no fixtures to copy");
                    }
                }

                if (cbCopyEndcon.IsChecked == true)
                {
                    try
                    {
                        #region irbs

                        if (jobjson.Mission.Endcon.Irbs0 != null && jobjson.Mission.Endcon.Irbs0.Any()) new Global(GTA.Offsets.Editor.irbs + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs0[0]);
                        if (jobjson.Mission.Endcon.Irbs1 != null && jobjson.Mission.Endcon.Irbs1.Any()) new Global(GTA.Offsets.Editor.irbs + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs1[0]);
                        if (jobjson.Mission.Endcon.Irbs2 != null && jobjson.Mission.Endcon.Irbs2.Any()) new Global(GTA.Offsets.Editor.irbs + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs2[0]);
                        if (jobjson.Mission.Endcon.Irbs3 != null && jobjson.Mission.Endcon.Irbs3.Any()) new Global(GTA.Offsets.Editor.irbs + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs3[0]);

                        if (jobjson.Mission.Endcon.Irbs20 != null && jobjson.Mission.Endcon.Irbs20.Any()) new Global(GTA.Offsets.Editor.irbs2 + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs20[0]);
                        if (jobjson.Mission.Endcon.Irbs21 != null && jobjson.Mission.Endcon.Irbs21.Any()) new Global(GTA.Offsets.Editor.irbs2 + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs21[0]);
                        if (jobjson.Mission.Endcon.Irbs22 != null && jobjson.Mission.Endcon.Irbs22.Any()) new Global(GTA.Offsets.Editor.irbs2 + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs22[0]);
                        if (jobjson.Mission.Endcon.Irbs23 != null && jobjson.Mission.Endcon.Irbs23.Any()) new Global(GTA.Offsets.Editor.irbs2 + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs23[0]);

                        if (jobjson.Mission.Endcon.Irbs30 != null && jobjson.Mission.Endcon.Irbs30.Any()) new Global(GTA.Offsets.Editor.irbs3 + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs30[0]);
                        if (jobjson.Mission.Endcon.Irbs31 != null && jobjson.Mission.Endcon.Irbs31.Any()) new Global(GTA.Offsets.Editor.irbs3 + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs31[0]);
                        if (jobjson.Mission.Endcon.Irbs32 != null && jobjson.Mission.Endcon.Irbs32.Any()) new Global(GTA.Offsets.Editor.irbs3 + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs32[0]);
                        if (jobjson.Mission.Endcon.Irbs33 != null && jobjson.Mission.Endcon.Irbs33.Any()) new Global(GTA.Offsets.Editor.irbs3 + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs33[0]);

                        if (jobjson.Mission.Endcon.Irbs40 != null && jobjson.Mission.Endcon.Irbs40.Any()) new Global(GTA.Offsets.Editor.irbs4 + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs40[0]);
                        if (jobjson.Mission.Endcon.Irbs41 != null && jobjson.Mission.Endcon.Irbs41.Any()) new Global(GTA.Offsets.Editor.irbs4 + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs41[0]);
                        if (jobjson.Mission.Endcon.Irbs42 != null && jobjson.Mission.Endcon.Irbs42.Any()) new Global(GTA.Offsets.Editor.irbs4 + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs42[0]);
                        if (jobjson.Mission.Endcon.Irbs43 != null && jobjson.Mission.Endcon.Irbs43.Any()) new Global(GTA.Offsets.Editor.irbs4 + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs43[0]);

                        if (jobjson.Mission.Endcon.Irbs50 != null && jobjson.Mission.Endcon.Irbs50.Any()) new Global(GTA.Offsets.Editor.irbs5 + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs50[0]);
                        if (jobjson.Mission.Endcon.Irbs51 != null && jobjson.Mission.Endcon.Irbs51.Any()) new Global(GTA.Offsets.Editor.irbs5 + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs51[0]);
                        if (jobjson.Mission.Endcon.Irbs52 != null && jobjson.Mission.Endcon.Irbs52.Any()) new Global(GTA.Offsets.Editor.irbs5 + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs52[0]);
                        if (jobjson.Mission.Endcon.Irbs53 != null && jobjson.Mission.Endcon.Irbs53.Any()) new Global(GTA.Offsets.Editor.irbs5 + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs53[0]);

                        if (jobjson.Mission.Endcon.Irbs60 != null && jobjson.Mission.Endcon.Irbs60.Any()) new Global(GTA.Offsets.Editor.irbs6 + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs60[0]);
                        if (jobjson.Mission.Endcon.Irbs61 != null && jobjson.Mission.Endcon.Irbs61.Any()) new Global(GTA.Offsets.Editor.irbs6 + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs61[0]);
                        if (jobjson.Mission.Endcon.Irbs62 != null && jobjson.Mission.Endcon.Irbs62.Any()) new Global(GTA.Offsets.Editor.irbs6 + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs62[0]);
                        if (jobjson.Mission.Endcon.Irbs63 != null && jobjson.Mission.Endcon.Irbs63.Any()) new Global(GTA.Offsets.Editor.irbs6 + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs63[0]);

                        if (jobjson.Mission.Endcon.Irbs70 != null && jobjson.Mission.Endcon.Irbs70.Any()) new Global(GTA.Offsets.Editor.irbs7 + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs70[0]);
                        if (jobjson.Mission.Endcon.Irbs71 != null && jobjson.Mission.Endcon.Irbs71.Any()) new Global(GTA.Offsets.Editor.irbs7 + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs71[0]);
                        if (jobjson.Mission.Endcon.Irbs72 != null && jobjson.Mission.Endcon.Irbs72.Any()) new Global(GTA.Offsets.Editor.irbs7 + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs72[0]);
                        if (jobjson.Mission.Endcon.Irbs73 != null && jobjson.Mission.Endcon.Irbs73.Any()) new Global(GTA.Offsets.Editor.irbs7 + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs73[0]);

                        if (jobjson.Mission.Endcon.Irbs80 != null && jobjson.Mission.Endcon.Irbs80.Any()) new Global(GTA.Offsets.Editor.irbs8 + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs80[0]);
                        if (jobjson.Mission.Endcon.Irbs81 != null && jobjson.Mission.Endcon.Irbs81.Any()) new Global(GTA.Offsets.Editor.irbs8 + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs81[0]);
                        if (jobjson.Mission.Endcon.Irbs82 != null && jobjson.Mission.Endcon.Irbs82.Any()) new Global(GTA.Offsets.Editor.irbs8 + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs82[0]);
                        if (jobjson.Mission.Endcon.Irbs83 != null && jobjson.Mission.Endcon.Irbs83.Any()) new Global(GTA.Offsets.Editor.irbs8 + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs83[0]);

                        if (jobjson.Mission.Endcon.Irbs90 != null && jobjson.Mission.Endcon.Irbs90.Any()) new Global(GTA.Offsets.Editor.irbs9 + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs90[0]);
                        if (jobjson.Mission.Endcon.Irbs91 != null && jobjson.Mission.Endcon.Irbs91.Any()) new Global(GTA.Offsets.Editor.irbs9 + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs91[0]);
                        if (jobjson.Mission.Endcon.Irbs92 != null && jobjson.Mission.Endcon.Irbs92.Any()) new Global(GTA.Offsets.Editor.irbs9 + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs92[0]);
                        if (jobjson.Mission.Endcon.Irbs93 != null && jobjson.Mission.Endcon.Irbs93.Any()) new Global(GTA.Offsets.Editor.irbs9 + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs93[0]);

                        if (jobjson.Mission.Endcon.Irbs100 != null && jobjson.Mission.Endcon.Irbs100.Any()) new Global(GTA.Offsets.Editor.irbs10 + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs100[0]);
                        if (jobjson.Mission.Endcon.Irbs101 != null && jobjson.Mission.Endcon.Irbs101.Any()) new Global(GTA.Offsets.Editor.irbs10 + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs101[0]);
                        if (jobjson.Mission.Endcon.Irbs102 != null && jobjson.Mission.Endcon.Irbs102.Any()) new Global(GTA.Offsets.Editor.irbs10 + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs102[0]);
                        if (jobjson.Mission.Endcon.Irbs103 != null && jobjson.Mission.Endcon.Irbs103.Any()) new Global(GTA.Offsets.Editor.irbs10 + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs103[0]);

                        if (jobjson.Mission.Endcon.Irbs110 != null && jobjson.Mission.Endcon.Irbs110.Any()) new Global(GTA.Offsets.Editor.irbs11 + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs110[0]);
                        if (jobjson.Mission.Endcon.Irbs111 != null && jobjson.Mission.Endcon.Irbs111.Any()) new Global(GTA.Offsets.Editor.irbs11 + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs111[0]);
                        if (jobjson.Mission.Endcon.Irbs112 != null && jobjson.Mission.Endcon.Irbs112.Any()) new Global(GTA.Offsets.Editor.irbs11 + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs112[0]);
                        if (jobjson.Mission.Endcon.Irbs113 != null && jobjson.Mission.Endcon.Irbs113.Any()) new Global(GTA.Offsets.Editor.irbs11 + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs113[0]);

                        if (jobjson.Mission.Endcon.Irbs120 != null && jobjson.Mission.Endcon.Irbs120.Any()) new Global(GTA.Offsets.Editor.irbs12 + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs120[0]);
                        if (jobjson.Mission.Endcon.Irbs121 != null && jobjson.Mission.Endcon.Irbs121.Any()) new Global(GTA.Offsets.Editor.irbs12 + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs121[0]);
                        if (jobjson.Mission.Endcon.Irbs122 != null && jobjson.Mission.Endcon.Irbs122.Any()) new Global(GTA.Offsets.Editor.irbs12 + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs122[0]);
                        if (jobjson.Mission.Endcon.Irbs123 != null && jobjson.Mission.Endcon.Irbs123.Any()) new Global(GTA.Offsets.Editor.irbs12 + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs123[0]);

                        if (jobjson.Mission.Endcon.Irbs130 != null && jobjson.Mission.Endcon.Irbs130.Any()) new Global(GTA.Offsets.Editor.irbs13 + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs130[0]);
                        if (jobjson.Mission.Endcon.Irbs131 != null && jobjson.Mission.Endcon.Irbs131.Any()) new Global(GTA.Offsets.Editor.irbs13 + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs131[0]);
                        if (jobjson.Mission.Endcon.Irbs132 != null && jobjson.Mission.Endcon.Irbs132.Any()) new Global(GTA.Offsets.Editor.irbs13 + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs132[0]);
                        if (jobjson.Mission.Endcon.Irbs133 != null && jobjson.Mission.Endcon.Irbs133.Any()) new Global(GTA.Offsets.Editor.irbs13 + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs133[0]);

                        if (jobjson.Mission.Endcon.Irbs140 != null && jobjson.Mission.Endcon.Irbs140.Any()) new Global(GTA.Offsets.Editor.irbs14 + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs140[0]);
                        if (jobjson.Mission.Endcon.Irbs141 != null && jobjson.Mission.Endcon.Irbs141.Any()) new Global(GTA.Offsets.Editor.irbs14 + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs141[0]);
                        if (jobjson.Mission.Endcon.Irbs142 != null && jobjson.Mission.Endcon.Irbs142.Any()) new Global(GTA.Offsets.Editor.irbs14 + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs142[0]);
                        if (jobjson.Mission.Endcon.Irbs143 != null && jobjson.Mission.Endcon.Irbs143.Any()) new Global(GTA.Offsets.Editor.irbs14 + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Irbs143[0]);

                        #endregion


                        if (jobjson.Mission.Endcon.Minspd0 != null && jobjson.Mission.Endcon.Minspd0.Any()) new Global(GTA.Offsets.Editor.minspd + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Minspd0[0]);
                        if (jobjson.Mission.Endcon.Minspd1 != null && jobjson.Mission.Endcon.Minspd1.Any()) new Global(GTA.Offsets.Editor.minspd + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Minspd1[0]);
                        if (jobjson.Mission.Endcon.Minspd2 != null && jobjson.Mission.Endcon.Minspd2.Any()) new Global(GTA.Offsets.Editor.minspd + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Minspd2[0]);
                        if (jobjson.Mission.Endcon.Minspd3 != null && jobjson.Mission.Endcon.Minspd3.Any()) new Global(GTA.Offsets.Editor.minspd + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Minspd3[0]);

                        if (jobjson.Mission.Endcon.Rloft0 != null && jobjson.Mission.Endcon.Rloft0.Any()) new Global(GTA.Offsets.Editor.rloft + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Rloft0[0]);
                        if (jobjson.Mission.Endcon.Rloft1 != null && jobjson.Mission.Endcon.Rloft1.Any()) new Global(GTA.Offsets.Editor.rloft + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Rloft1[0]);
                        if (jobjson.Mission.Endcon.Rloft2 != null && jobjson.Mission.Endcon.Rloft2.Any()) new Global(GTA.Offsets.Editor.rloft + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Rloft2[0]);
                        if (jobjson.Mission.Endcon.Rloft3 != null && jobjson.Mission.Endcon.Rloft3.Any()) new Global(GTA.Offsets.Editor.rloft + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Rloft3[0]);

                        if (jobjson.Mission.Endcon.Rloftv0 != null && jobjson.Mission.Endcon.Rloftv0.Any()) new Global(GTA.Offsets.Editor.rloftv + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Rloftv0[0]);
                        if (jobjson.Mission.Endcon.Rloftv1 != null && jobjson.Mission.Endcon.Rloftv1.Any()) new Global(GTA.Offsets.Editor.rloftv + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Rloftv1[0]);
                        if (jobjson.Mission.Endcon.Rloftv2 != null && jobjson.Mission.Endcon.Rloftv2.Any()) new Global(GTA.Offsets.Editor.rloftv + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Rloftv2[0]);
                        if (jobjson.Mission.Endcon.Rloftv3 != null && jobjson.Mission.Endcon.Rloftv3.Any()) new Global(GTA.Offsets.Editor.rloftv + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Rloftv3[0]);

                        if (jobjson.Mission.Endcon.Shdtxt0 != null && jobjson.Mission.Endcon.Shdtxt0.Any()) new Global(GTA.Offsets.Editor.shdtxt + (0 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Shdtxt0[0]);
                        if (jobjson.Mission.Endcon.Shdtxt1 != null && jobjson.Mission.Endcon.Shdtxt1.Any()) new Global(GTA.Offsets.Editor.shdtxt + (1 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Shdtxt1[0]);
                        if (jobjson.Mission.Endcon.Shdtxt2 != null && jobjson.Mission.Endcon.Shdtxt2.Any()) new Global(GTA.Offsets.Editor.shdtxt + (2 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Shdtxt2[0]);
                        if (jobjson.Mission.Endcon.Shdtxt3 != null && jobjson.Mission.Endcon.Shdtxt3.Any()) new Global(GTA.Offsets.Editor.shdtxt + (3 * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Shdtxt3[0]);

                        //check if team num is bigger than 4 
                        int teamnumber = jobjson.Mission.Gen.Tnum > 4 ? 4 : (int)jobjson.Mission.Gen.Tnum;

                        for (int i = 0; i < teamnumber; i++)
                        {
                            try
                            {
                                if (jobjson.Mission.Endcon.Inv != null && jobjson.Mission.Endcon.Inv.Any()) new Global(GTA.Offsets.Editor.inv + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Inv[i]);
                                if (jobjson.Mission.Endcon.Inv2 != null && jobjson.Mission.Endcon.Inv2.Any()) new Global(GTA.Offsets.Editor.inv2 + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Inv2[i]);
                                if (jobjson.Mission.Endcon.Inv3 != null && jobjson.Mission.Endcon.Inv3.Any()) new Global(GTA.Offsets.Editor.inv3 + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Inv3[i]);
                                if (jobjson.Mission.Endcon.Inv4 != null && jobjson.Mission.Endcon.Inv4.Any()) new Global(GTA.Offsets.Editor.inv4 + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Inv4[i]);

                                if (jobjson.Mission.Endcon.Minv != null && jobjson.Mission.Endcon.Minv.Any()) new Global(GTA.Offsets.Editor.minv + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Minv[i]);
                                if (jobjson.Mission.Endcon.Minv2 != null && jobjson.Mission.Endcon.Minv2.Any()) new Global(GTA.Offsets.Editor.minv2 + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Minv2[i]);
                                if (jobjson.Mission.Endcon.Minv3 != null && jobjson.Mission.Endcon.Minv3.Any()) new Global(GTA.Offsets.Editor.minv3 + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Minv3[i]);
                                if (jobjson.Mission.Endcon.Minv4 != null && jobjson.Mission.Endcon.Minv4.Any()) new Global(GTA.Offsets.Editor.minv4 + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Minv4[i]);

                                if (jobjson.Mission.Endcon.Csttn != null && jobjson.Mission.Endcon.Csttn.Any()) new Global(GTA.Offsets.Editor.csttn + (i * 4)).SetString((string)jobjson.Mission.Endcon.Csttn[i]);
                                if (jobjson.Mission.Endcon.Bnd2 != null && jobjson.Mission.Endcon.Bnd2.Any()) new Global(GTA.Offsets.Editor.bnd2 + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Bnd2[i]);
                                if (jobjson.Mission.Endcon.Boud != null && jobjson.Mission.Endcon.Boud.Any()) new Global(GTA.Offsets.Editor.boud + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Boud[i]);
                                if (jobjson.Mission.Endcon.Patm != null && jobjson.Mission.Endcon.Patm.Any()) new Global(GTA.Offsets.Editor.patm + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Patm[i]);
                                if (jobjson.Mission.Endcon.Tenms != null && jobjson.Mission.Endcon.Tenms.Any()) new Global(GTA.Offsets.Editor.tenms + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Tenms[i]);
                                if (jobjson.Mission.Endcon.Tmbt2 != null && jobjson.Mission.Endcon.Tmbt2.Any()) new Global(GTA.Offsets.Editor.tmbt2 + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Tmbt2[i]);
                                if (jobjson.Mission.Endcon.Tmbt3 != null && jobjson.Mission.Endcon.Tmbt3.Any()) new Global(GTA.Offsets.Editor.tmbt3 + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Tmbt3[i]);
                                if (jobjson.Mission.Endcon.Tmbt4 != null && jobjson.Mission.Endcon.Tmbt4.Any()) new Global(GTA.Offsets.Editor.tmbt4 + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Tmbt4[i]);
                                if (jobjson.Mission.Endcon.Tmbts != null && jobjson.Mission.Endcon.Tmbts.Any()) new Global(GTA.Offsets.Editor.tmbts + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Tmbts[i]);
                                if (jobjson.Mission.Endcon.Vehrsp != null && jobjson.Mission.Endcon.Vehrsp.Any()) new Global(GTA.Offsets.Editor.vehrsp + (i * GTA.Offsets.Editor.team_NEXT)).SetInt((int)jobjson.Mission.Endcon.Vehrsp[i]);
                                if (jobjson.Mission.Endcon.Tehlh != null && jobjson.Mission.Endcon.Tehlh.Any()) new Global(GTA.Offsets.Editor.tehrn + (i * 1)).SetInt((int)jobjson.Mission.Endcon.Tehlh[i]);
                                if (jobjson.Mission.Endcon.Inpts != null && jobjson.Mission.Endcon.Inpts.Any()) new Global(GTA.Offsets.Editor.inpts + (i * 1)).SetInt((int)jobjson.Mission.Endcon.Inpts[i]);
                                if (jobjson.Mission.Endcon.Mnumpt != null && jobjson.Mission.Endcon.Mnumpt.Any()) new Global(GTA.Offsets.Editor.mnumpt + (i * 1)).SetInt((int)jobjson.Mission.Endcon.Mnumpt[i]);
                                if (jobjson.Mission.Endcon.Numpt != null && jobjson.Mission.Endcon.Numpt.Any()) new Global(GTA.Offsets.Editor.numpt + (i * 1)).SetInt((int)jobjson.Mission.Endcon.Numpt[i]);
                                if (jobjson.Mission.Endcon.Teamrvbh != null && jobjson.Mission.Endcon.Teamrvbh.Any()) new Global(GTA.Offsets.Editor.teamrvbh + (i * 1)).SetInt((int)jobjson.Mission.Endcon.Teamrvbh[i]);
                                if (jobjson.Mission.Endcon.Teamrvc != null && jobjson.Mission.Endcon.Teamrvc.Any()) new Global(GTA.Offsets.Editor.teamrvc + (i * 1)).SetInt((int)jobjson.Mission.Endcon.Teamrvc[i]);
                                if (jobjson.Mission.Endcon.Teamrvcs != null && jobjson.Mission.Endcon.Teamrvcs.Any()) new Global(GTA.Offsets.Editor.teamrvcs + (i * 1)).SetInt((int)jobjson.Mission.Endcon.Teamrvcs[i]);
                                if (jobjson.Mission.Endcon.Teamrvp != null && jobjson.Mission.Endcon.Teamrvp.Any()) new Global(GTA.Offsets.Editor.teamrvp + (i * 1)).SetInt((int)jobjson.Mission.Endcon.Teamrvp[i]);
                                if (jobjson.Mission.Endcon.Teamv != null && jobjson.Mission.Endcon.Teamv.Any()) new Global(GTA.Offsets.Editor.teamv + (i * 1)).SetInt((int)jobjson.Mission.Endcon.Teamv[i]);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }

                if (cbCopyGen.IsChecked == true)
                {
                    if (jobjson.Mission.Gen.Min != null) new Global(GTA.Offsets.Editor.tnum).SetInt((int)jobjson.Mission.Gen.Min);
                    if (jobjson.Mission.Gen.Num != null) new Global(GTA.Offsets.Editor.tnum).SetInt((int)jobjson.Mission.Gen.Num);
                    if (jobjson.Mission.Gen.Tnum != null) new Global(GTA.Offsets.Editor.tnum).SetInt((int)jobjson.Mission.Gen.Tnum);
                    if (jobjson.Mission.Gen.Adverm != null) new Global(GTA.Offsets.Editor.adverm).SetInt((int)jobjson.Mission.Gen.Adverm);
                    if (jobjson.Mission.Gen.Alttype != null) new Global(GTA.Offsets.Editor.alttype).SetInt((int)jobjson.Mission.Gen.Alttype);
                    if (jobjson.Mission.Gen.Ausc != null) new Global(GTA.Offsets.Editor.ausc).SetInt((int)jobjson.Mission.Gen.Ausc);
                    if (jobjson.Mission.Gen.Blmpmsg != null) new Global(GTA.Offsets.Editor.blmpmsg).SetString(jobjson.Mission.Gen.Blmpmsg);
                    if (jobjson.Mission.Gen.Cam != null) new Global(GTA.Offsets.Editor.cam + 0).SetFloat(jobjson.Mission.Gen.Cam.X);
                    if (jobjson.Mission.Gen.Cam != null) new Global(GTA.Offsets.Editor.cam + 1).SetFloat(jobjson.Mission.Gen.Cam.Y);
                    if (jobjson.Mission.Gen.Cam != null) new Global(GTA.Offsets.Editor.cam + 2).SetFloat(jobjson.Mission.Gen.Cam.Z);
                    if (jobjson.Mission.Gen.Camf != null) new Global(GTA.Offsets.Editor.camf + 0).SetFloat(jobjson.Mission.Gen.Camf.X);
                    if (jobjson.Mission.Gen.Camf != null) new Global(GTA.Offsets.Editor.camf + 1).SetFloat(jobjson.Mission.Gen.Camf.Y);
                    if (jobjson.Mission.Gen.Camf != null) new Global(GTA.Offsets.Editor.camf + 2).SetFloat(jobjson.Mission.Gen.Camf.Z);
                    if (jobjson.Mission.Gen.Endtype != null) new Global(GTA.Offsets.Editor.endtype).SetInt((int)jobjson.Mission.Gen.Endtype);
                    if (jobjson.Mission.Gen.Ltm != null) new Global(GTA.Offsets.Editor.ltm).SetInt((int)jobjson.Mission.Gen.Ltm);
                    if (jobjson.Mission.Gen.Mrd != null) new Global(GTA.Offsets.Editor.mrd).SetInt((int)jobjson.Mission.Gen.Mrd);
                    if (jobjson.Mission.Gen.NumRounds != null) new Global(GTA.Offsets.Editor.numRounds).SetInt((int)jobjson.Mission.Gen.NumRounds);
                    if (jobjson.Mission.Gen.Photo != null) Functions.Write.writebinary(1, GTA.Offsets.Editor.photo, jobjson.Mission.Gen.Photo == true);
                    if (jobjson.Mission.Gen.Phpo != null) new Global(GTA.Offsets.Editor.phpo + 0).SetFloat(jobjson.Mission.Gen.Phpo.X);
                    if (jobjson.Mission.Gen.Phpo != null) new Global(GTA.Offsets.Editor.phpo + 1).SetFloat(jobjson.Mission.Gen.Phpo.Y);
                    if (jobjson.Mission.Gen.Phpo != null) new Global(GTA.Offsets.Editor.phpo + 2).SetFloat(jobjson.Mission.Gen.Phpo.Z);
                    if (jobjson.Mission.Gen.Start != null) new Global(GTA.Offsets.Editor.start + 0).SetFloat(jobjson.Mission.Gen.Start.X);
                    if (jobjson.Mission.Gen.Start != null) new Global(GTA.Offsets.Editor.start + 1).SetFloat(jobjson.Mission.Gen.Start.Y);
                    if (jobjson.Mission.Gen.Start != null) new Global(GTA.Offsets.Editor.start + 2).SetFloat(jobjson.Mission.Gen.Start.Z);
                    if (jobjson.Mission.Gen.Type != null) new Global(GTA.Offsets.Editor.type).SetInt((int)jobjson.Mission.Gen.Type);
                    if (jobjson.Mission.Gen.Subtype != null) new Global(GTA.Offsets.Editor.subtype).SetInt((int)jobjson.Mission.Gen.Subtype);
                    if (jobjson.Mission.Gen.Todhr != null) new Global(GTA.Offsets.Editor.todhr).SetInt((int)jobjson.Mission.Gen.Todhr);
                    if (jobjson.Mission.Gen.Todmn != null) new Global(GTA.Offsets.Editor.todmn).SetInt((int)jobjson.Mission.Gen.Todmn);
                    if (jobjson.Mission.Gen.Nm != null) new Global(GTA.Offsets.Editor.nm).SetString(jobjson.Mission.Gen.Nm);
                    if (jobjson.Mission.Gen.Dec != null && jobjson.Mission.Gen.Dec.Any()) setDescribtion(string.Join("", jobjson.Mission.Gen.Dec));
                }

                if (cbCopyBasics.IsChecked == true)
                {
                    if (jobjson.Mission.Gen.Start != null) new Global(GTA.Offsets.Editor.start + 0).SetFloat(jobjson.Mission.Gen.Start.X);
                    if (jobjson.Mission.Gen.Start != null) new Global(GTA.Offsets.Editor.start + 1).SetFloat(jobjson.Mission.Gen.Start.Y);
                    if (jobjson.Mission.Gen.Start != null) new Global(GTA.Offsets.Editor.start + 2).SetFloat(jobjson.Mission.Gen.Start.Z);
                    if (jobjson.Mission.Gen.Type != null) new Global(GTA.Offsets.Editor.type).SetInt((int)jobjson.Mission.Gen.Type);
                    if (jobjson.Mission.Gen.Subtype != null) new Global(GTA.Offsets.Editor.subtype).SetInt((int)jobjson.Mission.Gen.Subtype);
                    if (jobjson.Mission.Gen.Todhr != null) new Global(GTA.Offsets.Editor.todhr).SetInt((int)jobjson.Mission.Gen.Todhr);
                    if (jobjson.Mission.Gen.Todmn != null) new Global(GTA.Offsets.Editor.todmn).SetInt((int)jobjson.Mission.Gen.Todmn);
                    if (jobjson.Mission.Gen.NumRounds != null) new Global(GTA.Offsets.Editor.numRounds).SetInt((int)jobjson.Mission.Gen.NumRounds);
                    if (jobjson.Mission.Gen.Alttype != null) new Global(GTA.Offsets.Editor.alttype).SetInt((int)jobjson.Mission.Gen.Alttype);
                    if (jobjson.Mission.Gen.Ausc != null) new Global(GTA.Offsets.Editor.ausc).SetInt((int)jobjson.Mission.Gen.Ausc);
                    if (jobjson.Mission.Gen.Newausc != null) new Global(GTA.Offsets.Editor.newausc).SetInt((int)jobjson.Mission.Gen.Newausc);
                    if (jobjson.Mission.Gen.Blmpmsg != null) new Global(GTA.Offsets.Editor.blmpmsg).SetString(jobjson.Mission.Gen.Blmpmsg);
                    if (jobjson.Mission.Gen.Cam != null) new Global(GTA.Offsets.Editor.cam + 0).SetFloat(jobjson.Mission.Gen.Cam.X);
                    if (jobjson.Mission.Gen.Cam != null) new Global(GTA.Offsets.Editor.cam + 1).SetFloat(jobjson.Mission.Gen.Cam.Y);
                    if (jobjson.Mission.Gen.Cam != null) new Global(GTA.Offsets.Editor.cam + 2).SetFloat(jobjson.Mission.Gen.Cam.Z);
                    if (jobjson.Mission.Gen.Camf != null) new Global(GTA.Offsets.Editor.camf + 0).SetFloat(jobjson.Mission.Gen.Camf.X);
                    if (jobjson.Mission.Gen.Camf != null) new Global(GTA.Offsets.Editor.camf + 1).SetFloat(jobjson.Mission.Gen.Camf.Y);
                    if (jobjson.Mission.Gen.Camf != null) new Global(GTA.Offsets.Editor.camf + 2).SetFloat(jobjson.Mission.Gen.Camf.Z);
                    if (jobjson.Mission.Gen.Endtype != null) new Global(GTA.Offsets.Editor.endtype).SetInt((int)jobjson.Mission.Gen.Endtype);
                    if (jobjson.Mission.Gen.Min != null) new Global(GTA.Offsets.Editor.min).SetInt((int)jobjson.Mission.Gen.Min);
                    if (jobjson.Mission.Gen.Num != null) new Global(GTA.Offsets.Editor.num).SetInt((int)jobjson.Mission.Gen.Num);
                    if (jobjson.Mission.Gen.Tnum != null) new Global(GTA.Offsets.Editor.tnum).SetInt((int)jobjson.Mission.Gen.Tnum);
                    if (jobjson.Mission.Rule.Weth != null) new Global(GTA.Offsets.Editor.weth).SetInt((int)jobjson.Mission.Rule.Weth);
                    if (jobjson.Mission.Rule.Tod != null) new Global(GTA.Offsets.Editor.tod).SetInt((int)jobjson.Mission.Rule.Tod);
                    if (jobjson.Mission.Rule.Traf != null) new Global(GTA.Offsets.Editor.traf).SetInt((int)jobjson.Mission.Rule.Traf);
                }
            }

            // Make the copied data visible, then save or publish if asked to.
            if (jobjson != null)
                _ = FinishCopyJobAsync();
        }
    }
}
