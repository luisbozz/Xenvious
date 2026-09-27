using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using Xenvious.JSON;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Xenvious
{
    internal class OnlineEnable
    {
        [Flags]
        public enum Responses
        {
            SUCCESFULL = 1,
            FAILURE_INJECT = 2,
            FAILURE_DOWNLOAD = 4,
            FAILURE_OTHER = 8,
            ALREADY_INJECTED = 16
        }

        /// <summary>
        /// Switched off until the enabler works again: its DLL and checksum came from the
        /// xenvious.com backend, which is offline. The button stays hidden and init() does
        /// nothing while this is false.
        /// </summary>
        public const bool Available = false;

        static string path = "";
        public static bool isDLLInjected = false;
        public static bool initialized = false;

        public static async Task<Responses> init()
        {
            if (!Available)
                return Responses.FAILURE_OTHER;
            try
            {
                if (isInjected())
                    return Responses.ALREADY_INJECTED;

                // download file
                string folderpath = settings.getpath;
                path = folderpath + "\\online_enabler.dll";

                if (!File.Exists(path) || !await checkChecksum())
                {
                    if (!Directory.Exists(folderpath))
                        Directory.CreateDirectory(folderpath);

                    // Meta herunterladen, temporär abspeichern und ggf fehler anzeigen
                    if (!await download())
                        return Responses.FAILURE_DOWNLOAD;
                }

                if (inject())
                {
                    isDLLInjected = true;
                    return Responses.SUCCESFULL;
                }
                else
                {
                    isDLLInjected = false;
                    return Responses.FAILURE_INJECT;
                }
            }
            catch (Exception)
            {
                return Responses.FAILURE_OTHER;
            }
        }

        private static async Task<bool> download()
        {
            try
            {
                using (WebClient web = new WebClient())
                {
                    await web.DownloadFileTaskAsync(new Uri(settings.onlineenabler_download), path);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool isInjected()
        {
            try
            {
                bool injected =  Process.GetProcessesByName(GameVariant.ProcessName)[0].Modules.Cast<ProcessModule>()
                                                                              .Select(module => module.ModuleName)
                                                                              .ToList().Contains("online_enabler.dll");
                isDLLInjected = injected;
                return injected;
            }
            catch (Exception)
            {
                return false;
            }

        }

        private static async Task<bool> checkChecksum()
        {
            string result = await Functions.API.getonlineenablerchecksum();
            return File.Exists(path) ? result.Equals(Functions.md5(File.ReadAllText(path))) : false;
        }

        private static bool inject()
        {
            return File.Exists(path) ? MainWindow.m.InjectDLL(path) : false;
        }
    }
}
