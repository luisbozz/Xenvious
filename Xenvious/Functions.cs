using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Runtime.CompilerServices;
using System.Numerics;

namespace Xenvious
{
    [DebuggerStepThrough]
    public class Functions
    {
        /// <summary>
        /// Read Process Memory
        /// </summary>
        public class Read
        {
            public static bool IsSteamVersion()
            {
                ProcessModuleCollection p = Process.GetProcessesByName(GameVariant.ProcessName)[0].Modules;
                ProcessModule[] modules = new ProcessModule[p.Count];
                p.CopyTo(modules, 0);
                return modules.ToList().Where(x => x.ModuleName.ToLower().Equals("steam_api64.dll")).Any();
            }

            public static bool IsRockstarVersion()
            {
                ProcessModuleCollection p = Process.GetProcessesByName(GameVariant.ProcessName)[0].Modules;
                ProcessModule[] modules = new ProcessModule[p.Count];
                p.CopyTo(modules, 0);
                return modules.ToList().Where(x => x.ModuleName.ToLower().Equals("steam_api64.dll") || x.ModuleName.ToLower().Equals("EOSSDK-Win64-Shipping.dll".ToLower())).Any() ? false : true;
            }

            public static bool IsEpicVersion()
            {
                ProcessModuleCollection p = Process.GetProcessesByName(GameVariant.ProcessName)[0].Modules;
                ProcessModule[] modules = new ProcessModule[p.Count];
                p.CopyTo(modules, 0);
                return modules.ToList().Where(x => x.ModuleName.ToLower().Equals("EOSSDK-Win64-Shipping.dll".ToLower())).Any();
            }





            /// <summary>
            /// Get the current ingame Circle Cursor Location
            /// </summary>
            /// <param name="tbx">1st TextBox</param>
            /// <param name="tby">2nd TextBox</param>
            /// <param name="tbz">3rd TextBox</param>
            public static List<string> getlocation()
            {
                return new List<string>(new string[]
                {
                    MainWindow.m.memory((MainWindow.getCreatorScriptLocalWorkerBase() + (GTA.Offsets.Editor.OFFSET_current_creator_worker_pos + 0) * 8).ToString("X")).Get<float>().ToString(),
                    MainWindow.m.memory((MainWindow.getCreatorScriptLocalWorkerBase() + (GTA.Offsets.Editor.OFFSET_current_creator_worker_pos + 1) * 8).ToString("X")).Get<float>().ToString(),
                    MainWindow.m.memory((MainWindow.getCreatorScriptLocalWorkerBase() + (GTA.Offsets.Editor.OFFSET_current_creator_worker_pos + 2) * 8).ToString("X")).Get<float>().ToString()
                });
            }

            /// <summary>
            /// Get the current ingame Circle Cursor Location
            /// </summary>
            /// <param name="tbx">1st TextBox</param>
            /// <param name="tby">2nd TextBox</param>
            /// <param name="tbz">3rd TextBox</param>
            public static Vector3 GetLocationVec()
            {
                return new Vector3
                (
                    MainWindow.m.memory((MainWindow.getCreatorScriptLocalWorkerBase() + (GTA.Offsets.Editor.OFFSET_current_creator_worker_pos + 0) * 8).ToString("X")).Get<float>(),
                    MainWindow.m.memory((MainWindow.getCreatorScriptLocalWorkerBase() + (GTA.Offsets.Editor.OFFSET_current_creator_worker_pos + 1) * 8).ToString("X")).Get<float>(),
                    MainWindow.m.memory((MainWindow.getCreatorScriptLocalWorkerBase() + (GTA.Offsets.Editor.OFFSET_current_creator_worker_pos + 2) * 8).ToString("X")).Get<float>()
                );
            }

            /// <summary>
            /// Get the current ingame Heading
            /// </summary>
            /// <param name="heading">1st TextBox</param>
            public static float GetHeading()
            {
                return MainWindow.m.memory((MainWindow.getCreatorScriptLocalHeadingBase() + (GTA.Offsets.Editor.OFFSET_current_creator_cam_heading_offset) * 8).ToString("X")).Get<float>();
            }

            /// <summary>
            /// Check if a specific Bit is set
            /// </summary>
            /// <param name="index">Bit Index</param>
            /// <param name="offset">Offsets</param>
            /// <param name="control">CheckBox that will get checked or unchecked depending on the Bit state.</param>
            /// <returns>Returns bool.</returns>
            public static bool checkbinary(int index, long offset, CheckBox control)
            {
                if (MainWindow.m.IsProcOpen)
                {
                    int value = new Global(offset).Get<int>();


                    if (control != null)
                    {

                        if ((value & (1 << index - 1)) != 0)
                        {
                            control.IsChecked = true;
                            return true;
                        }
                        else
                        {
                            control.IsChecked = false;
                            return false;
                        }
                    }
                    else
                    {
                        if ((value & (1 << index - 1)) == 1)
                            return true;
                        else
                            return false;
                    }
                }
                return false;
            }
            /// <summary>
            /// Check if a specific Bit is set
            /// </summary>
            /// <param name="index">Bit Index</param>
            /// <param name="offset">Offsets</param>
            /// <returns>Returns bool.</returns>
            public static bool checkbinary(int index, long offset)
            {
                if (MainWindow.m.IsProcOpen)
                {
                    int value = new Global(offset).Get<int>();

                    if ((value & (1 << index - 1)) != 0)
                        return true;
                    else
                        return false;
                }
                return false;
            }

            public static bool isBoosterProp(int model)
            {
                return GTA.Editor.prop_model_booster.Contains(toHex(model));
            }

            public static bool isSlowDownProp(int model)
            {
                return GTA.Editor.prop_model_slowdown.Contains(toHex(model));
            }

            public static bool isCEntityDefProp(int model)
            {
                return GTA.Editor.prop_model_centitydef_whitelist.Contains(toHex(model));
            }

            public static bool isStuntPropWithColorOption(int model)
            {
                return GTA.Editor.prop_model_stunt_with_color_option.Contains(toHex(model));
            }

            public static bool isPropBlacklisted(int model)
            {
                return GTA.Editor.prop_model_blacklisted.Contains(model.ToString());
            }

            public static bool isDPropActivationTimer(int model)
            {
                return GTA.Editor.dprop_model_activationtimer.Contains(model.ToString());
            }

            public static bool isFlareProp(int model)
            {
                return -2071229766 == int_parse(model.ToString());
            }

            public static bool isMission()
            {
                if (MainWindow.m.IsProcOpen)
                {
                    if (new Global(GTA.Offsets.Editor.check_creator).Get<int>() == 1)
                        return new Global(GTA.Offsets.Editor.type).Get<int>() == 0;
                    else
                        return false;
                }
                else
                {
                    throw new Exception("Please Open the Process.");
                }
            }

            public static bool isLTS()
            {
                if (MainWindow.m.IsProcOpen)
                {
                    if (new Global(GTA.Offsets.Editor.check_creator).Get<int>() == 1)
                        return new Global(GTA.Offsets.Editor.type).Get<int>() == 0 && new Global(GTA.Offsets.Editor.subtype).Get<int>() == 5;
                    else
                        return false;
                }
                else
                {
                    throw new Exception("Please Open the Process.");
                }
            }

            public static bool isCapture()
            {
                if (MainWindow.m.IsProcOpen)
                {
                    if (new Global(GTA.Offsets.Editor.check_creator).Get<int>() == 1)
                        return new Global(GTA.Offsets.Editor.type).Get<int>() == 0 && new Global(GTA.Offsets.Editor.subtype).Get<int>() == 6;
                    else
                        return false;
                }
                else
                {
                    throw new Exception("Please Open the Process.");
                }
            }

            public static bool isSurvival()
            {
                if (MainWindow.m.IsProcOpen)
                {
                    return new Global(GTA.Offsets.Editor.type).Get<int>() == 3;
                }
                else
                {
                    throw new Exception("Please Open the Process.");
                }
            }

            public static bool isRace()
            {
                if (MainWindow.m.IsProcOpen)
                {
                    return (new Global(GTA.Offsets.Editor.type).Get<int>() == 2);
                }
                else
                {
                    throw new Exception("Please Open the Process.");
                }
            }

            public static bool isDeathmatch()
            {
                if (MainWindow.m.IsProcOpen)
                {
                    return (new Global(GTA.Offsets.Editor.type).Get<int>() == 1);
                }
                else
                {
                    throw new Exception("Please Open the Process.");
                }
            }
        }

        /// <summary>
        /// Write Process Memory
        /// </summary>
        public class Write
        {
            #region Write

            /// <summary>
            /// Write a Value, in the current Open Process Memory
            /// </summary>
            /// <param name="offset">Offset</param>
            /// <param name="value">Value that will get written to the desired Memory Address</param>
            public static void writemem(long offset, string value)
            {
                if (MainWindow.m.IsProcOpen)
                {
                    new Global(offset).SetString(value);
                }
            }

            #endregion

            public static bool is_bit_set(int value, int bit)
            {
                bit = bit - 1;
                return (value & (1 << bit)) != 0;
            }

            private static int clear_bit(int value, int bit)
            {
                bit = bit - 1;
                return value & ~(1 << bit);
            }

            private static int set_bit(int value, int bit)
            {
                bit = bit - 1;
                return value | 1 << bit;
            }

            /// <summary>
            /// Sets Bit on desired Memory Address
            /// </summary>
            /// <param name="index">Bit that will get set</param>
            /// <param name="offset">Offset</param>
            public static void writebinary(int index, long offset)
            {
                if (MainWindow.m.IsProcOpen)
                {
                    int value = new Global(offset).Get<int>();

                    if (!is_bit_set(value, index))
                        new Global(offset).SetInt(set_bit(value, index));
                }
            }           

            /// <summary>
            /// Sets Bit on desired Memory Address
            /// </summary>
            /// <param name="index">Bit that will get set</param>
            /// <param name="offset">Offset</param>
            /// <param name="control">Will use the CheckBox Checked State to set or clear</param>
            public static void writebinary(int index, long offset, CheckBox control)
            {
                if (MainWindow.m.IsProcOpen)
                {
                    int value = new Global(offset).Get<int>();

                    if (control.IsChecked ?? true)
                    {
                        if (!is_bit_set(value, index))
                            new Global(offset).SetInt(set_bit(value, index));
                    }
                    else
                    {
                        if (is_bit_set(value, index))
                            new Global(offset).SetInt(clear_bit(value, index));
                    }
                }
            }
            /// <summary>
            /// Sets Bit on desired Memory Address
            /// </summary>
            /// <param name="index">Bit that will get set</param>
            /// <param name="offset">Offset</param>
            /// <param name="enable">Set or Clear the desired Bit</param>
            public static void writebinary(int index, long offset, bool enable)
            {
                if (MainWindow.m.IsProcOpen)
                {
                    int value = new Global(offset).Get<int>();

                    if (enable)
                    {
                        if (!is_bit_set(value, index))
                            new Global(offset).SetInt(set_bit(value, index));
                    }
                    else
                    {
                        if (is_bit_set(value, index))
                            new Global(offset).SetInt(clear_bit(value, index));
                    }
                }
            }

            /// <summary>
            /// Sets Bit on desired Memory Address
            /// </summary>
            /// <param name="index">Bit that will get set</param>
            /// <param name="offset">Offset</param>
            /// <param name="amount">Amount of jumps to other Memory Addresses</param>
            /// <param name="offset_next">Offset to the next Memory Address</param>
            public static void writebinary(int index, long offset, int amount, long offset_next)
            {
                if (MainWindow.m.IsProcOpen)
                {
                    int value = new Global(offset).Get<int>();

                    if (!is_bit_set(value, index))
                    {
                        for (int i = 0; i < amount; i++)
                        {
                            new Global((offset + i * offset_next)).SetInt(set_bit(value, index));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < amount; i++)
                        {
                            new Global((offset + i * offset_next)).SetInt(clear_bit(value, index));
                        }
                    }
                }
            }

            /// <summary>
            /// Sets Bit on desired Memory Address
            /// </summary>
            /// <param name="index">Bit that will get set</param>
            /// <param name="offset">Offset</param>
            /// <param name="amount">Amount of jumps to other Memory Addresses</param>
            /// <param name="offset_next">Offset to the next Memory Address</param>
            public static void writebinary(int index, long offset, int amount, int offset_next)
            {
                if (MainWindow.m.IsProcOpen)
                {
                    int value = new Global(offset).Get<int>();

                    if (!is_bit_set(value, index))
                    {
                        for (int i = 0; i < amount; i++)
                        {
                            new Global((offset + i * offset_next)).SetInt(set_bit(value, index));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < amount; i++)
                        {
                            new Global((offset + i * offset_next)).SetInt(clear_bit(value, index));
                        }
                    }
                }
            }


            /// <summary>
            /// Sets Bit on desired Memory Address
            /// </summary>
            /// <param name="index">Bit that will get set</param>
            /// <param name="offset">Offset</param>
            /// <param name="amount">Amount of jumps to other Memory Addresses</param>
            /// <param name="offset_next">Offset to the next Memory Address</param>
            /// <param name="control">Will use the CheckBox Checked State to set or clear the Bits</param>
            public static void writebinary(int index, long offset, int amount, int offset_next, CheckBox control)
            {
                if (MainWindow.m.IsProcOpen)
                {
                    int value = new Global(offset).Get<int>();

                    if (control.IsChecked ?? true)
                    {
                        if (!is_bit_set(value, index))
                        {
                            for (int i = 0; i < amount; i++)
                            {
                                new Global((offset + i * offset_next)).SetInt(set_bit(value, index));
                            }
                        }
                    }
                    else
                    {
                        if (is_bit_set(value, index))
                        {
                            for (int i = 0; i < amount; i++)
                            {
                                new Global((offset + i * offset_next)).SetInt(clear_bit(value, index));
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Sets Bit on desired Memory Address
            /// </summary>
            /// <param name="index">Bit that will get set</param>
            /// <param name="offset">Offset</param>
            /// <param name="amount">Amount of jumps to other Memory Addresses</param>
            /// <param name="offset_next">Offset to the next Memory Address</param>
            /// <param name="control">Will use the CheckBox Checked State to set or clear the Bits</param>
            public static void writebinary(int index, long offset, int amount, long offset_next, CheckBox control)
            {
                if (MainWindow.m.IsProcOpen)
                {
                    int value = new Global(offset).Get<int>();

                    if (control.IsChecked ?? true)
                    {
                        if (!is_bit_set(value, index))
                        {
                            for (int i = 0; i < amount; i++)
                            {
                                new Global((offset + i * offset_next)).SetInt(set_bit(value, index));
                            }
                        }
                    }
                    else
                    {
                        if (is_bit_set(value, index))
                        {
                            for (int i = 0; i < amount; i++)
                            {
                                new Global((offset + i * offset_next)).SetInt(clear_bit(value, index));
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Sets Bit on desired Memory Address
            /// </summary>
            /// <param name="index">Bit that will get set</param>
            /// <param name="offset">Offset</param>
            /// <param name="amount">Amount of jumps to other Memory Addresses</param>
            /// <param name="offset_next">Offset to the next Memory Address</param>
            /// <param name="enable">Set or Clear the desired Bits</param>
            public static void writebinary(int index, long offset, int amount, int offset_next, bool enable)
            {
                if (MainWindow.m.IsProcOpen)
                {
                    int value = new Global(offset).Get<int>();

                    if (enable)
                    {
                        if (!is_bit_set(value, index))
                        {
                            for (int i = 0; i < amount; i++)
                            {
                                new Global((offset + i * offset_next)).SetInt(set_bit(value, index));
                            }
                        }
                    }
                    else
                    {
                        if (is_bit_set(value, index))
                        {
                            for (int i = 0; i < amount; i++)
                            {
                                new Global((offset + i * offset_next)).SetInt(clear_bit(value, index));
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Sets Bit on desired Memory Address
            /// </summary>
            /// <param name="index">Bit that will get set</param>
            /// <param name="offset">Offset</param>
            /// <param name="amount">Amount of jumps to other Memory Addresses</param>
            /// <param name="offset_next">Offset to the next Memory Address</param>
            /// <param name="enable">Set or Clear the desired Bits</param>
            public static void writebinary(int index, long offset, int amount, long offset_next, bool enable)
            {
                if (MainWindow.m.IsProcOpen)
                {
                    int value = new Global(offset).Get<int>();

                    if (enable)
                    {
                        if (!is_bit_set(value, index))
                        {
                            for (int i = 0; i < amount; i++)
                            {
                                new Global((offset + i * offset_next)).SetInt(set_bit(value, index));
                            }
                        }
                    }
                    else
                    {
                        if (is_bit_set(value, index))
                        {
                            for (int i = 0; i < amount; i++)
                            {
                                new Global((offset + i * offset_next)).SetInt(clear_bit(value, index));
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Sets Bit on desired Memory Address
            /// </summary>
            /// <param name="index">Bit that will get set</param>
            /// <param name="offset">Offset</param>
            public static void writebinarytoaddy(int index, long address)
            {
                if (MainWindow.m.IsProcOpen)
                {
                    int value = MainWindow.m.memory(address.ToString("X")).Get<int>();

                    if (!is_bit_set(value, index))
                        MainWindow.m.memory(address.ToString("X")).SetInt(set_bit(value, index));
                }
            }

        }

        public static int int_parse(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return 0;
            try
            {
                if (Convert.ToInt64(value) > int.MaxValue)
                    return Convert.ToInt32((uint.Parse(value) - 4294967296));
            }
            catch (Exception) { }
            try
            {
                if (int.Parse(value) >= int.MinValue && int.Parse(value) <= int.MaxValue)
                    return int.Parse(value);
            }
            catch (Exception) { }
            try
            {
                if (value.ToList().Where(x => char.IsLetter(x)).Count() > 0 && ((value.Length == 8 && !isHex(value.ToList())) || value.Length != 8))
                    return unchecked((int)joaat(value));
            }
            catch (Exception) { }
            try
            {
                if (isHex(value.ToList()) && value.Length == 8)
                    return int.Parse(value, NumberStyles.HexNumber);
            }
            catch (Exception) { }
            
            return 0;
        }

        public static uint joaat(string Native)
        {
            Native = Native.ToLower();
            uint num1 = 0;

            foreach (uint num2 in Encoding.UTF8.GetBytes(Native))
            {
                uint num3 = num1 + num2;
                uint num4 = num3 + (num3 << 10);
                num1 = num4 ^ num4 >> 6;
            }
            uint num5 = num1 + (num1 << 3);
            uint num6 = num5 ^ num5 >> 11;
            return num6 + (num6 << 15);
        }

        public static bool isHex(string hex)
        {
            return Regex.IsMatch(hex, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        public static bool isHex(IEnumerable<char> chars)
        {
            bool isHex;
            foreach (var c in chars)
            {
                isHex = ((c >= '0' && c <= '9') ||
                         (c >= 'a' && c <= 'f') ||
                         (c >= 'A' && c <= 'F'));

                if (!isHex)
                    return false;
            }
            return true;
        }

        public static string toHex(int model)
        {
            return model.ToString("X");
        }


        /// <summary>
        /// Draw Elipse around Object
        /// </summary>
        /// <param name="nLeftRect">x-coordinate of upper-left corner</param>
        /// <param name="nTopRect">y-coordinate of upper-left corner</param>
        /// <param name="nRightRect">x-coordinate of lower-right corner</param>
        /// <param name="nBottomRect">y-coordinate of lower-right corner</param>
        /// <param name="nWdithEllipse">height of ellipse</param>
        /// <param name="nHeightEllipse">width of ellipse</param>
        /// <returns></returns>
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWdithEllipse,
            int nHeightEllipse
        );



        //    if (!Directory.Exists(configpath))
        //        Directory.CreateDirectory(configpath);


        //    return Path.GetFullPath(settings.getpath + @"\config.ini");
        //}

        public static string sha1(string randomString)
        {
            var crypt = new SHA1Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(randomString));
            foreach (byte theByte in crypto)
            {
                hash += theByte.ToString("x2");
            }
            return hash;
        }

        public static string md5(string randomString)
        {
            using (MD5 hash = MD5.Create())
            {
                string result = String.Join
                (
                    "",
                    from ba in hash.ComputeHash
                    (
                        Encoding.UTF8.GetBytes(randomString)
                    )
                    select ba.ToString("x2")
                );
                return result;
            }
        }

        public static bool getNetwork()
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return true;
            }
            return false;
        }


        public class API
        {
            private static readonly System.Net.Http.HttpClient backendHttp = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(30) };

            /// <summary>
            /// The Online Enabler's checksum as plain text. The old backend sent it
            /// AES-encrypted; a new source has to serve it in the clear. Does not throw on
            /// an HTTP error or a failed connection; it passes on whatever body came back.
            /// </summary>
            public static async Task<string> getonlineenablerchecksum()
            {
                string content = null;
                try
                {
                    using (var response = await backendHttp.GetAsync("https://xenvious.com/backendauth/api/onlineenablerchecksum"))
                        content = await response.Content.ReadAsStringAsync();
                }
                catch (System.Net.Http.HttpRequestException)
                {
                }
                catch (TaskCanceledException)
                {
                }
                return content?.Trim();
            }

            private static readonly System.Net.Http.HttpClient socialClubHttp = new System.Net.Http.HttpClient(
                new System.Net.Http.HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate })
            { Timeout = TimeSpan.FromSeconds(15) };

            /// <summary>
            /// Job metadata (name, image, type, creator) straight from the Social Club API.
            /// It needs no login; without the browser-like headers the request is rejected.
            /// </summary>
            public static async Task<string> jobdetails(string id)
            {
                string url = "https://scapi.rockstargames.com/ugc/mission/details?title=gtav&contentId=" + Uri.EscapeDataString(id) + "&autoGetLatest=true";
                using (var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "None");
                    request.Headers.TryAddWithoutValidation("Origin", "https://socialclub.rockstargames.com");
                    request.Headers.TryAddWithoutValidation("Referer", "https://socialclub.rockstargames.com/job/gtav/" + id);
                    request.Headers.TryAddWithoutValidation("Accept", "*/*");
                    request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
                    request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36");
                    request.Headers.TryAddWithoutValidation("X-Amc", "true");
                    request.Headers.TryAddWithoutValidation("X-Cache-Ver", "0");
                    request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                    request.Headers.TryAddWithoutValidation("X-Lang", "en-US");
                    try
                    {
                        using (var response = await socialClubHttp.SendAsync(request))
                        {
                            string body = await response.Content.ReadAsStringAsync();
                            if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(body))
                                return body;
                            Xenvious.Logging.Log.Warn($"job details {id}: HTTP {(int)response.StatusCode} {body}", source: "copyjob");
                        }
                    }
                    catch (Exception ex)
                    {
                        Xenvious.Logging.Log.Error($"job details {id}", ex, "copyjob");
                    }
                }
                return "{\"status\":false}";
            }

            public static string newLine(string value)
            {
                return DateAndTime() + ": " + value + "\n";
            }
        }

        [DllImport("gdi32.dll", EntryPoint = "AddFontResourceW", SetLastError = true)]
        public static extern int AddFontResource([In][MarshalAs(UnmanagedType.LPWStr)]
                                         string lpFileName);
        [DllImport("gdi32.dll", EntryPoint = "RemoveFontResourceW", SetLastError = true)]
        public static extern int RemoveFontResource([In][MarshalAs(UnmanagedType.LPWStr)]
                                            string lpFileName);

        public static void newThread(System.Threading.ThreadStart func, System.Threading.ApartmentState state = System.Threading.ApartmentState.STA, System.Threading.ThreadPriority priority = System.Threading.ThreadPriority.Normal, string culture = "de-DE")
        {
            System.Threading.Thread th = new System.Threading.Thread(func);
            th.SetApartmentState(System.Threading.ApartmentState.STA);
            th.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(culture);
            th.Priority = System.Threading.ThreadPriority.Normal;
            th.Start();
        }

        public static void StartGTA()
        {
            string path = string.Empty;

            string InstallPath = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Rockstar Games\GTAV", "InstallFolderSteam", null);
            if (InstallPath != null)
            {
                path = InstallPath + "Launcher.exe";
            }
            else
            {
                InstallPath = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Rockstar Games\Grand Theft Auto V", "InstallFolder", null);
                path = InstallPath + "\\PlayGTAV.exe";
            }

            if (File.Exists(path))
            {
                Process process = new Process();
                process.StartInfo.FileName = path;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.ErrorDialog = true;
                process.Start();
            }
            else
            {
                MessageBox.Show("Couldnt find GTA. Please Start it manually.", "Xenvious",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Create Local AppData Path
        /// </summary>
        public static string create_LocalAppData()
        {
            string path = Path.GetFullPath(Path.GetTempPath()) + "Xenvious\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        public static string getRoamingConfigFilePath()
        {
            string path = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)) + "\\Xenvious\\";
            //check if directory exists
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            //check if file exists
            if (!File.Exists(path + "config.ini"))
                File.Create(path + "config.ini");
            //return path
            return path + "config.ini";
        }


        /// <summary>
        /// Get Date and Time
        /// </summary>
        /// <param name="replace">Replace ':'</param>
        /// <returns>Date and Time</returns>
        public static string DateAndTime(char replace = ':')
        {
            return DateTime.UtcNow.ToShortDateString().Replace(':', replace) + " " + DateTime.UtcNow.ToLongTimeString().Replace(':', replace);
        }

        internal static class date
        {
            /// <summary>
            /// Short Date
            /// </summary>
            /// <returns>Get Short Date</returns>
            public static string FormatedDate()
            {
                return DateTime.UtcNow.ToLongDateString();
            }
            /// <summary>
            /// Short Date
            /// </summary>
            /// <param name="replace">Replace ':'</param>
            /// <returns>Get Short Date</returns>
            public static string FormatedDate(char replace = ':')
            {
                return DateTime.UtcNow.ToShortDateString().Replace(':', replace);
            }

        }

        internal static class time
        {
            /// <summary>
            /// Get Current Time with Seconds
            /// </summary>
            /// <param name="replace"></param>
            /// <returns></returns>
            public static string TimewithSeconds(char replace = '?')
            {
                if (replace == '?')
                    return DateTime.UtcNow.ToLongTimeString();
                else
                    return DateTime.UtcNow.ToLongTimeString().Replace(':', replace);

            }
            public static string Time(char replace = '?')
            {
                if (replace == '?')
                    return DateTime.UtcNow.ToShortTimeString();
                else
                    return DateTime.UtcNow.ToShortTimeString().Replace(':', replace);
            }
        }
    }

    internal static class cmd
    {
        private static Process proc;
        /// <summary>
        /// Execute CMD with custom Commands as a background Script.
        /// </summary>
        /// <param name="CMD">Enter a CMD command here.</param>
        internal static void EXECUTECMD(string CMD)
        {
            cmd.proc = new Process();
            cmd.proc.StartInfo.FileName = "popershell.exe";
            cmd.proc.StartInfo.Arguments = CMD;
            cmd.proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.proc.Start();
            cmd.proc.WaitForExit();
        }
    }

    internal static class Extensions
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        public static string GetMainModuleFileName(this Process process, int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ?
                fileNameBuilder.ToString() :
                null;
        }
    }

    internal static class settings
    {
        public static string getpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Xenvious";
        public static string onlineenabler_download = "https://xenvious.com/downloads/xenvious/online_enabler.dll";
        public static string link_sc = "https://de.socialclub.rockstargames.com/member/luisbozz_rsg";
        public static string link_discord = "https://discordapp.com/invite/f2Uwzwr";
        public static string link_youtube = "https://www.youtube.com/channel/UCGsCGsOpaw8D4NU25CuOzLQ";
        public static string link_twitter = "https://twitter.com/luis___g";

        // Where updates come from: the latest release of this GitHub repository.
        // A fork changes these two to publish its own releases.
        public const string UpdateOwner = "luisbozz";
        public const string UpdateRepo = "Xenvious";

        public static string props = "";
        public static string propswpic = "";
        public static string vehicles = "";
        public static string weapons = "";
        public static string peds = "";
        public static string colorcodes = "";

        public class Rootobject
        {
            public bool prescan { get; set; }
            public bool reconnect { get; set; }
            public bool keeptemp { get; set; }
            public int zoom { get; set; }
            public int lastpid { get; set; }
            public long lastmprops { get; set; }
            public long lastpresets { get; set; }
        }
    }
}
