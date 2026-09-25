using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using Xenvious.Logging;

namespace Xenvious
{
    public static class GTA
    {
        public static void Teleport(XenVector3 vector3)
        {
            if (MainWindow.m.IsProcOpen)
            {
                long addr = MainWindow.m.memory(GTA.Offsets.Editor.WORLDversion, GTA.Offsets.Editor.OFFSET_player_loc).GetAddress();
                long addr2 = MainWindow.m.memory(GTA.Offsets.Editor.WORLDversion, GTA.Offsets.Editor.OFFSET_cam_loc).GetAddress();
                long addr3 = MainWindow.m.memory(GTA.Offsets.Editor.WORLDversion, GTA.Offsets.Editor.OFFSET_playerveh_loc).GetAddress();
                long addr4 = MainWindow.m.memory(GTA.Offsets.Editor.WORLDversion, GTA.Offsets.Editor.OFFSET_playervehcam_loc).GetAddress();
                long addr5 = MainWindow.m.memory(GTA.Offsets.Editor.creator_camptr, GTA.Offsets.Editor.OFFSET_creator_cam_loc).GetAddress();

                MainWindow.m.memory(addr.ToString("X")).SetFloat(vector3.X);
                MainWindow.m.memory((addr + 4).ToString("X")).SetFloat(vector3.Y);
                MainWindow.m.memory((addr + 8).ToString("X")).SetFloat(vector3.Z);
                MainWindow.m.memory(addr2.ToString("X")).SetFloat(vector3.X);
                MainWindow.m.memory((addr2 + 4).ToString("X")).SetFloat(vector3.Y);
                MainWindow.m.memory((addr2 + 8).ToString("X")).SetFloat(vector3.Z);
                MainWindow.m.memory(addr5.ToString("X")).SetFloat(vector3.X);
                MainWindow.m.memory((addr5 + 4).ToString("X")).SetFloat(vector3.Y);
                MainWindow.m.memory((addr5 + 8).ToString("X")).SetFloat(vector3.Z);
                if (IsPlayerInVehicle())
                {
                    MainWindow.m.memory(addr3.ToString("X")).SetFloat(vector3.X);
                    MainWindow.m.memory((addr3 + 4).ToString("X")).SetFloat(vector3.Y);
                    MainWindow.m.memory((addr3 + 8).ToString("X")).SetFloat(vector3.Z);
                    MainWindow.m.memory(addr4.ToString("X")).SetFloat(vector3.X);
                    MainWindow.m.memory((addr4 + 4).ToString("X")).SetFloat(vector3.Y);
                    MainWindow.m.memory((addr4 + 8).ToString("X")).SetFloat(vector3.Z);
                }
            }
        }

        public static XenVector3 GetLocation()
        {
            if (MainWindow.m.IsProcOpen)
            {
                long addr = MainWindow.m.memory(GTA.Offsets.Editor.WORLDversion, GTA.Offsets.Editor.OFFSET_player_loc).GetAddress();
                return new XenVector3(
                    MainWindow.m.memory(addr.ToString("X")).Get<float>(),
                    MainWindow.m.memory((addr + 4).ToString("X")).Get<float>(),
                    MainWindow.m.memory((addr + 8).ToString("X")).Get<float>());
            }
            return new XenVector3(0, 0, 0);
        }

        /// <summary>
        /// Whether an AOB pattern is usable for the build that is running.
        ///
        /// Byte patterns locate code, and the two GTA V builds are compiled
        /// separately, so a pattern found for one is meaningless for the other.
        /// Patterns that have not been derived for a build are shipped empty
        /// rather than carried over: an empty pattern is a state that can be
        /// reported, while a wrong one scans, finds nothing, and yields a
        /// pointer computed from address zero that reads unrelated memory.
        /// </summary>
        private static bool HasPattern(string pattern, string name)
        {
            if (!string.IsNullOrWhiteSpace(pattern))
                return true;

            Log.Warn(
                $"No {name} pattern for GTA {GameVariant.DisplayName(GameVariant.Current)}; " +
                "the features that need this pointer stay unavailable.",
                source: "GTA");
            return false;
        }

        /// <summary>
        /// Whether a pattern scan actually found its site.
        ///
        /// A pattern that exists but no longer matches is the dangerous case: the
        /// scan returns 0, and the caller then computes a pointer from address
        /// zero. That yields a plausible-looking but meaningless value instead of
        /// an obvious failure, and every feature downstream reads unrelated
        /// memory. Byte patterns locate machine code, so they break whenever the
        /// game executable is rebuilt -- which is exactly when nobody is looking.
        /// </summary>
        private static bool Found(ulong addy, string name)
        {
            if (addy != ulong.MinValue)
                return true;

            Log.Warn(
                $"{name} did not match GTA {GameVariant.DisplayName(GameVariant.Current)}; " +
                "the pattern needs to be re-derived for this game build.",
                source: "GTA");
            return false;
        }

        public static IntPtr getWorldPointer(byte[] buffer = null)
        {
            if (!HasPattern(GTA.Offsets.Editor.AOB_worldptr, "AOB_worldptr"))
                return IntPtr.Zero;

            ulong addy = ScanModule(GTA.Offsets.Editor.AOB_worldptr, buffer);

            if (!Found(addy, "AOB_worldptr"))
                return IntPtr.Zero;

            return (IntPtr)(MainWindow.m.rip(IntPtr.Add((IntPtr)addy, 3)).ToInt64() - (long)MainWindow.m.getBaseAddress());
        }

        public static IntPtr getGlobalPointer(byte[] buffer = null)
        {
            if (!HasPattern(GTA.Offsets.Editor.AOB_globalptr, "AOB_globalptr"))
                return IntPtr.Zero;

            ulong addy = ScanModule(GTA.Offsets.Editor.AOB_globalptr, buffer);

            if (!Found(addy, "AOB_globalptr"))
                return IntPtr.Zero;

            return (IntPtr)(MainWindow.m.rip(IntPtr.Add((IntPtr)addy, 3)).ToInt64() - (long)MainWindow.m.getBaseAddress());
        }

        public static IntPtr getLocalPointer(byte[] buffer = null)
        {
            if (!HasPattern(GTA.Offsets.Editor.AOB_localptr, "AOB_localptr"))
                return IntPtr.Zero;

            ulong addy = ScanModule(GTA.Offsets.Editor.AOB_localptr, buffer);

            if (!Found(addy, "AOB_localptr"))
                return IntPtr.Zero;

            return (IntPtr)(MainWindow.m.rip(IntPtr.Add((IntPtr)addy, 3)).ToInt64() - (long)MainWindow.m.getBaseAddress());
        }

        public static IntPtr getNEXTCPPointer(byte[] buffer = null)
        {
            if (!HasPattern(GTA.Offsets.Editor.AOB_nextcp_ptr, "AOB_nextcp_ptr"))
                return IntPtr.Zero;

            ulong addy = ScanModule(GTA.Offsets.Editor.AOB_nextcp_ptr, buffer);

            if (!Found(addy, "AOB_nextcp_ptr"))
                return IntPtr.Zero;

            return (IntPtr)(MainWindow.m.rip(IntPtr.Add((IntPtr)addy, 3)).ToInt64() - (long)MainWindow.m.getBaseAddress());
        }

        /// <summary>
        /// Every pointer getter scans through here. See AobCache: a hit is found once per
        /// process, and across starts it comes from the stored cache after its bytes are
        /// checked again. Without a buffer a scan reads the whole module, so this matters
        /// most for the getters that are called repeatedly.
        /// </summary>
        public static ulong ScanModule(string pattern, byte[] buffer)
        {
            ulong hit;
            if (AobCache.TryGet(pattern, out hit))
                return hit;
            hit = buffer == null
                ? MainWindow.m.AOBScanModule2(pattern, MainWindow.m.getMainModule())
                : MainWindow.m.AOBScanModule3(pattern, MainWindow.m.getMainModule(), buffer);
            AobCache.Put(pattern, hit);
            return hit;
        }

        /// <summary>
        /// True when every non-empty pattern can be served from the cache, so attaching
        /// needs no module read at all.
        /// </summary>
        public static bool AllPatternsCached()
        {
            string[] patterns =
            {
                GTA.Offsets.Editor.AOB_worldptr, GTA.Offsets.Editor.AOB_globalptr, GTA.Offsets.Editor.AOB_localptr,
                GTA.Offsets.Editor.AOB_nextcp_ptr, GTA.Offsets.Editor.AOB_session_ptr, GTA.Offsets.Editor.AOB_img_ptr,
                GTA.Offsets.Editor.AOB_cursor_ptr, GTA.Offsets.Editor.AOB_scrProgramptr, GTA.Offsets.Editor.AOB_devptr,
                GTA.Offsets.Editor.AOB_camptr, GTA.Offsets.Editor.AOB_versionptr, GTA.Offsets.Editor.AOB_creator_camptr,
            };
            ulong ignored;
            return patterns.Where(p => !string.IsNullOrWhiteSpace(p)).All(p => AobCache.TryGet(p, out ignored));
        }

        public static IntPtr getSessionPointer(byte[] buffer = null)
        {
            if (!HasPattern(GTA.Offsets.Editor.AOB_session_ptr, "AOB_session_ptr"))
                return IntPtr.Zero;

            ulong addy = ScanModule(GTA.Offsets.Editor.AOB_session_ptr, buffer);

            if (!Found(addy, "AOB_session_ptr"))
                return IntPtr.Zero;

            return (IntPtr)(MainWindow.m.rip(IntPtr.Add((IntPtr)addy, 3)).ToInt64() - (long)MainWindow.m.getBaseAddress());
        }

        public static IntPtr getIMGPointer(byte[] buffer = null)
        {
            if (!HasPattern(GTA.Offsets.Editor.AOB_img_ptr, "AOB_img_ptr"))
                return IntPtr.Zero;

            ulong addy = ScanModule(GTA.Offsets.Editor.AOB_img_ptr, buffer);

            if (!Found(addy, "AOB_img_ptr"))
                return IntPtr.Zero;

            return (IntPtr)(MainWindow.m.rip(IntPtr.Add((IntPtr)addy, 3)).ToInt64() - (long)MainWindow.m.getBaseAddress());
        }

        public static IntPtr getCursorPointer(byte[] buffer = null)
        {
            if (!HasPattern(GTA.Offsets.Editor.AOB_cursor_ptr, "AOB_cursor_ptr"))
                return IntPtr.Zero;

            ulong addy = ScanModule(GTA.Offsets.Editor.AOB_cursor_ptr, buffer);

            if (!Found(addy, "AOB_cursor_ptr"))
                return IntPtr.Zero;

            return (IntPtr)(MainWindow.m.rip(IntPtr.Add((IntPtr)addy, 3)).ToInt64() - (long)MainWindow.m.getBaseAddress());
        }

        public static IntPtr getscrProgramPointer(byte[] buffer = null)
        {
            if (!HasPattern(GTA.Offsets.Editor.AOB_scrProgramptr, "AOB_scrProgramptr"))
                return IntPtr.Zero;

            ulong addy = ScanModule(GTA.Offsets.Editor.AOB_scrProgramptr, buffer);

            if (!Found(addy, "AOB_scrProgramptr"))
                return IntPtr.Zero;

            return (IntPtr)(MainWindow.m.rip(IntPtr.Add((IntPtr)addy, 3)).ToInt64() - (long)MainWindow.m.getBaseAddress());
        }

        public static IntPtr getDEVPointer(byte[] buffer = null)
        {
            if (!HasPattern(GTA.Offsets.Editor.AOB_devptr, "AOB_devptr"))
                return IntPtr.Zero;

            ulong addy = ScanModule(GTA.Offsets.Editor.AOB_devptr, buffer);

            if (!Found(addy, "AOB_devptr"))
                return IntPtr.Zero;

            return (IntPtr)(addy - MainWindow.m.getBaseAddress());
        }


        public static IntPtr getCAMPointer(byte[] buffer = null)
        {
            if (!HasPattern(GTA.Offsets.Editor.AOB_camptr, "AOB_camptr"))
                return IntPtr.Zero;

            ulong addy = ScanModule(GTA.Offsets.Editor.AOB_camptr, buffer);

            if (!Found(addy, "AOB_camptr"))
                return IntPtr.Zero;

            return (IntPtr)(addy - MainWindow.m.getBaseAddress());
        }

        public static IntPtr getVersionPointer(byte[] buffer = null)
        {
            if (!HasPattern(GTA.Offsets.Editor.AOB_versionptr, "AOB_versionptr"))
                return IntPtr.Zero;

            ulong addy = ScanModule(GTA.Offsets.Editor.AOB_versionptr, buffer);

            if (!Found(addy, "AOB_versionptr"))
                return IntPtr.Zero;

            // This pattern matches three identical call sites and cannot be made
            // unique: past the first 31 bytes the surrounding code diverges
            // entirely, and everything inside that window is a RIP displacement.
            // The scan returns the lowest address, which is the site that holds
            // the online version string; the other two resolve to zeroed data.
            return (IntPtr)addy;
        }

        // Both strings sit near the site the version pattern matches, but the two
        // builds lay them out differently and the patterns match different
        // instructions, so the displacements cannot be shared.
        //
        // Legacy   "4C 8D 05 ..."  online at +3, build 165 bytes *before* the site
        // Enhanced "4C 8D 0D ..."  build at +3, online at +0x47
        //
        // Measured against 1.73-3889 and enhanced-1.73-1158: on Enhanced +3 reads
        // "1158.16" and +0x47 reads "1.73", while -165 lands outside the module.
        private static int VersionDelta(bool online) =>
            GameVariant.IsEnhanced ? (online ? 0x47 : 3) : (online ? 3 : -165);

        private static string VersionString(bool online, int length)
        {
            if (GTA.Offsets.Editor.versionptr == 0)
                return string.Empty;

            IntPtr site = IntPtr.Add((IntPtr)GTA.Offsets.Editor.versionptr, VersionDelta(online));
            IntPtr p = (IntPtr)(MainWindow.m.rip(site).ToInt64() - (long)MainWindow.m.getBaseAddress());
            return Encoding.UTF8.GetString(MainWindow.m.memory(p.ToInt64()).GetBytes(length));
        }

        public static string getOnlineVersion(byte[] buffer = null)
        {
            return VersionString(online: true, length: GameVariant.IsEnhanced ? 4 : 5);
        }

        public static string getBuildVersion(byte[] buffer = null)
        {
            return VersionString(online: false, length: GameVariant.IsEnhanced ? 7 : 8);
        }

        // Der Dev-Schalter ueberschreibt die ersten vier Bytes einer Funktion mit
        // "mov al,1; ret", damit sie immer true liefert. Zum Ausschalten muss der
        // urspruengliche Prolog zurueck -- und der ist pro Build ein anderer:
        //
        //   Legacy    48 89 5C 24   mov [rsp+8], rbx
        //   Enhanced  56 48 83 EC   push rsi ; sub rsp, 0x20
        //
        // Legacys Wert in Enhanced zurueckzuschreiben wuerde die Funktion
        // zerstoeren, weil dort ein voellig anderer Prolog steht.
        public const int DevPatched = 616759728;          // B0 01 C3 24

        public static int DevOriginal =>
            GameVariant.IsEnhanced ? unchecked((int)3968026710) : 610044232;

        public static IntPtr getCreatorCamPointer(byte[] buffer = null)
        {
            if (!HasPattern(GTA.Offsets.Editor.AOB_creator_camptr, "AOB_creator_camptr"))
                return IntPtr.Zero;

            ulong addy = ScanModule(GTA.Offsets.Editor.AOB_creator_camptr, buffer);

            if (!Found(addy, "AOB_creator_camptr"))
                return IntPtr.Zero;

            return (IntPtr)(IntPtr.Add((IntPtr)addy, 760).ToInt64() - (long)MainWindow.m.getBaseAddress());
        }

        public static bool IsPlayerInVehicle()
        {
            return MainWindow.m.memory(GTA.Offsets.Editor.WORLDversion, GTA.Offsets.Editor.OFFSET_playerinveh_loc).Get<int>() == 0;
        }

        public static readonly string[] CreatorScripts =
        {
            "fm_lts_creator", "fm_capture_creator", "fm_deathmatch_creator",
            "fm_race_creator", "fm_survival_creator", "fm_mission_creator"
        };

        /// <summary>
        /// Script name of the thread in slot <paramref name="slot"/> of the thread list,
        /// lower case, or "" when the slot holds nothing recognisable.
        ///
        /// Legacy threads carry their name inline (OFFSET_script_name). Enhanced threads
        /// do not -- the name lives in a separate {hash, name} registry -- but they carry
        /// the joaat hash of the script (OFFSET_script_hash), which is matched against the
        /// known creator names instead.
        /// </summary>
        public static string ReadScriptName(long listPtr, long slot)
        {
            try
            {
                if (GTA.Offsets.Editor.OFFSET_script_hash != 0)
                {
                    uint hash = MainWindow.m.memory(listPtr, new long[] { slot, GTA.Offsets.Editor.OFFSET_script_hash }).Get<uint>();
                    foreach (string name in CreatorScripts)
                        if (MainWindow.Joaat(name) == hash)
                            return name;
                    return "";
                }
                return MainWindow.m.memory(listPtr, new long[] { slot, GTA.Offsets.Editor.OFFSET_script_name }).GetString().ToLower();
            }
            catch
            {
                return "";
            }
        }

        /// <summary>Script name of the creator Xenvious is attached to, "" when none.</summary>
        public static string CurrentCreatorName()
        {
            long[] lp = GTA.Offsets.Editor.localptr;
            return lp == null || lp.Length < 2 ? "" : ReadScriptName(lp[0], lp[1]);
        }

        public static long[] getCurrentCreatorAddy()
        {
            long localaddy = getLocalPointer().ToInt64();

            foreach (long d in new long[] { 0x20, 0x78 })
                if (CreatorScripts.Contains(ReadScriptName(localaddy, d)))
                    return new long[] { localaddy, d };

            for (int d = 0; d < 0x1000; d += 0x8)
            {
                if (CreatorScripts.Contains(ReadScriptName(localaddy, d)))
                    return new long[] { localaddy, d };
            }
            return null;
        }

        public static long[] getLocalScriptAddy(string scriptname)
        {
            long localaddy = getLocalPointer().ToInt64();
            string wanted = (scriptname ?? "").ToLower();
            uint wantedHash = MainWindow.Joaat(wanted);
            // The thread list holds ~190 threads on Enhanced; 0x200 bytes only reached the
            // first 64 of them.
            for (int d = 0; d < 0x800; d += 0x8)
            {
                if (GTA.Offsets.Editor.OFFSET_script_hash != 0)
                {
                    // Beliebiges Script, nicht nur Creator: Hash direkt vergleichen.
                    uint hash = 0;
                    try { hash = MainWindow.m.memory(localaddy, new long[] { d, GTA.Offsets.Editor.OFFSET_script_hash }).Get<uint>(); } catch { }
                    if (hash == wantedHash)
                        return new long[] { localaddy, d };
                }
                else if (ReadScriptName(localaddy, d) == wanted)
                {
                    return new long[] { localaddy, d };
                }
            }
            return null;
        }

        public class Version
        {
            public static string Rockstar;
            public static string Steam;
            public static string Epic;
        }

        public class Offsets
        {
            public static long WorldPTR_steam = 0x0;
            public static long WorldPTR_rstar = 0x0;
            public static long WorldPTR_epic = 0x0;
            public static long GlobalPTR_steam = 0x0;
            public static long GlobalPTR_rstar = 0x0;
            public static long GlobalPTR_epic = 0x0;
            public static long version_steam = 0x0;
            public static long version_jobs_steam = 0x0;
            public static long version_rstar = 0x0;
            public static long version_jobs_rstar = 0x0;
            public static long version_epic = 0x0;
            public static long version_jobs_epic = 0x0;
            public static long pointer_steam = 0x0;
            public static long creator_menu_pointer = 0x0;
            public static long creator_menu_color = 0x0;
            public static long check_creator_steam = 0x0;
            public static long check_creator_rstar = 0x0;
            public static long check_creator_epic = 0x0;
            public static long preset_ptr_steam = 0x0;
            public static long preset_ptr_rstar = 0x0;
            public static long preset_ptr_epic = 0x0;

            public static class Editor
            {
                public class Image
                {
                    public static long img = 0x0;
                }

                public class Session
                {
                    public static long pointer = 0x0;
                    // Resolved from AOB session_ptr. Finds the struct the running binary
                    // actually uses, whichever launcher started it; 0 = pattern missing.
                    public static long patternPointer = 0x0;
                    public static long pointer_steam = 0x0;
                    public static long pointer_rstar = 0x0;
                    public static long username = 0x0;
                    public static long ticket = 0x0;
                    public static long sessionTicketOffset = 0x0;
                    public static long sessionKeyOffset = 0x0;
                    public static long milliSecsToExpiryOffset = 0x0;
                }

                public class Props
                {
                    public static long loc = 0;
                    public static long vrot = 0;
                    public static long head = 0;
                    public static long model = 0;
                    public static long asst = 0;
                    public static long asso = 0;
                    public static long asss = 0;
                    public static long pasc = 0;
                    public static long asst2 = 0;
                    public static long asso2 = 0;
                    public static long asss2 = 0;
                    public static long pasc2 = 0;
                    public static long asst3 = 0;
                    public static long asso3 = 0;
                    public static long asss3 = 0;
                    public static long pasc3 = 0;
                    public static long asst4 = 0;
                    public static long asso4 = 0;
                    public static long asss4 = 0;
                    public static long pasc4 = 0;
                    public static long bpbid = 0;
                    public static long bpbip = 0;
                    public static long bpbpt = 0;
                    public static long fcuat = 0;
                    public static long prpclcr = 0;
                    public static long prpclc = 0;
                    public static long aldel = 0;
                    public static long alsnd = 0;
                    public static long alteam = 0;
                    public static long flvfx = 0;
                    public static long flcl = 0;
                    public static long prpcl = 0;
                    public static long prpct = 0;
                    public static long prcra = 0;
                    public static long prpcr = 0;
                    public static long prpbs = 0;
                    public static long prpbs2 = 0;
                    public static long pprst = 0;
                    public static long prplod = 0;
                    public static long prpatn = 0;
                    public static long prpasn = 0;
                    public static long prpclr = 0;
                    public static long prptsp = 0;
                    public static long prptds = 0;
                    public static long prpsba = 0;
                    public static long sndid = 0;
                    public static long sndtri = 0;
                    public static long sndlmt = 0;
                    public static long ptfxtr = 0;
                    public static long prpsnpp = 0;
                    public static long ptfxst = 0;
                    public static long prrorc = 0;
                    public static long updatez = 0;
                    public static long updtime = 0;
                    public static long upddel = 0;
                    public static long ploddist = 0;
                    public static long trtact = 0;
                    public static long trppd = 0;
                    public static long prpdypil = 0;
                    public static long ttph = 0;
                    public static long prppi = 0;
                    public static long prpsbt = 0;
                    public static long prpsgg = 0;
                    public static long prpssg = 0;
                    public static long prpsdp = 0;
                    public static long pdip = 0;
                    public static long fwTPos = 0;
                    public static long fwTSize = 0;
                    public static long fwTeam = 0;
                    public static long prpscr0_ = 0;
                    public static long prpscr1_ = 0;
                    public static long prpscr2_ = 0;
                    public static long prpscr3_ = 0;
                    public static long pror_ = 0;
                    public static long number = 0;
                    public static long NEXT = 0;
                }

                public class DProps
                {
                    public static long loc = 0;
                    public static long vrot = 0;
                    public static long head = 0;
                    public static long model = 0;
                    public static long obref = 0;
                    public static long asst = 0;
                    public static long asso = 0;
                    public static long asss = 0;
                    public static long pasc = 0;
                    public static long asst2 = 0;
                    public static long asso2 = 0;
                    public static long asss2 = 0;
                    public static long pasc2 = 0;
                    public static long asst3 = 0;
                    public static long asso3 = 0;
                    public static long asss3 = 0;
                    public static long pasc3 = 0;
                    public static long asst4 = 0;
                    public static long asso4 = 0;
                    public static long asss4 = 0;
                    public static long pasc4 = 0;
                    public static long prpct = 0;
                    public static long prcra = 0;
                    public static long prpcr = 0;
                    public static long prpbs = 0;
                    public static long prpkt = 0;
                    public static long dprorc = 0;
                    public static long prpdclr = 0;
                    public static long dpLODd = 0;
                    public static long dptrpx = 0;
                    public static long dpsl = 0;
                    public static long dpcl = 0;
                    public static long dptrrs = 0;
                    public static long dyipho = 0;
                    public static long dyipbtt = 0;
                    public static long dcoid = 0;
                    public static long dynrpil = 0;
                    public static long dynblov = 0;
                    public static long dynblcl = 0;
                    public static long dynblrn = 0;
                    public static long dynblhd = 0;
                    public static long dynblsc = 0;
                    public static long dynblpr = 0;
                    public static long Dror_ = 0;
                    public static long number = 0;
                    public static long NEXT = 0;
                }

                public class Vehicle
                {
                    public static long model = 0;
                    public static long rule = 0;
                    public static long pri = 0;
                    public static long jtop = 0;
                    public static long jtof = 0;
                    public static long loc = 0;
                    public static long head = 0;
                    public static long vrr = 0;
                    public static long vrot = 0;
                    public static long col = 0;
                    public static long col2 = 0;
                    public static long col3 = 0;
                    public static long ncol = 0;
                    public static long colc = 0;
                    public static long liv = 0;
                    public static long drbs = 0;
                    public static long vbs2 = 0;
                    public static long vbs3 = 0;
                    public static long vbs4 = 0;
                    public static long vbs5 = 0;
                    public static long vbs6 = 0;
                    public static long vbs7 = 0;
                    public static long vbs8 = 0;
                    public static long vbs9 = 0;
                    public static long vbs10 = 0;
                    public static long vbs11 = 0;
                    public static long vebs = 0;
                    public static long enghp = 0;
                    public static long ptrhp = 0;
                    public static long bdyhp = 0;
                    public static long hlth = 0;
                    public static long arrowr = 0;
                    public static long arrowg = 0;
                    public static long arrowb = 0;
                    public static long vbvrr = 0;
                    public static long vehcr = 0;
                    public static long vehct = 0;
                    public static long vehbr = 0;
                    public static long vehbc = 0;
                    public static long vehbs = 0;
                    public static long vehbso = 0;
                    public static long vehwtci = 0;
                    public static long objt = 0;
                    public static long team = 0;
                    public static long spwn = 0;
                    public static long spsrc = 0;
                    public static long spasr = 0;
                    public static long objt2 = 0;
                    public static long team2 = 0;
                    public static long spwn2 = 0;
                    public static long spsrc2 = 0;
                    public static long spasr2 = 0;
                    public static long objt3 = 0;
                    public static long team3 = 0;
                    public static long spwn3 = 0;
                    public static long spsrc3 = 0;
                    public static long spasr3 = 0;
                    public static long objt4 = 0;
                    public static long team4 = 0;
                    public static long spwn4 = 0;
                    public static long spsrc4 = 0;
                    public static long spasr4 = 0;
                    public static long burst = 0;
                    public static long vclnrl = 0;
                    public static long vclnt = 0;
                    public static long vclnr = 0;
                    public static long rsp = 0;
                    public static long number = 0;
                    public static long NEXT = 0;
                }

                public class Objects
                {
                    public static long model = 0;
                    public static long loc = 0;
                    public static long head = 0;
                    public static long vrot = 0;
                    public static long bits1 = 0;
                    public static long bits2 = 0;
                    public static long bits3 = 0;
                    public static long bits4 = 0;
                    public static long rule = 0;
                    public static long pri = 0;
                    public static long jtop = 0;
                    public static long jtof = 0;
                    public static long team = 0;
                    public static long spwn = 0;
                    public static long objct = 0;
                    public static long objcr = 0;
                    public static long valu = 0;
                    public static long rsp = 0;
                    public static long mgbs = 0;
                    public static long cont = 0;
                    public static long nmpass = 0;
                    public static long nmfail = 0;
                    public static long hlt = 0;
                    public static long obb = 0;
                    public static long obbc = 0;
                    public static long obbs = 0;
                    public static long ped = 0;
                    public static long obint = 0;
                    public static long objLOD = 0;
                    public static long number = 0;
                    public static long NEXT = 0;
                }

                public class DHProp
                {
                    public static long number = 0;
                    public static long locx = 0;
                    public static long locy = 0;
                    public static long locz = 0;
                    public static long model = 0;
                    public static long bits = 0;
                    public static long mnswap = 0;
                    public static long wprad = 0;
                    public static long NEXT = 0;
                }

                public class Zones
                {
                    public static long zntp = 0;
                    public static long vtox = 0;
                    public static long vtoy = 0;
                    public static long vtoz = 0;
                    public static long vldx = 0;
                    public static long vldy = 0;
                    public static long vldz = 0;
                    public static long znwid = 0;
                    public static long znbs = 0;
                    public static long znbs2 = 0;
                    public static long znbs3 = 0;
                    public static long znatp = 0;
                    public static long znwd = 0;
                    public static long znwvd = 0;
                    public static long znepr = 0;
                    public static long znpr = 0;
                    public static long znhei = 0;
                    public static long zndel = 0;
                    public static long zngTe = 0;
                    public static long zngPo = 0;
                    public static long znscra1 = 0;
                    public static long znscra2 = 0;
                    public static long znscra3 = 0;
                    public static long zneilnk = 0;
                    public static long znetlnk = 0;
                    public static long znebc = 0;
                    public static long zneba = 0;
                    public static long number = 0;
                    public static long NEXT = 0;
                }

                public class Locations
                {
                    public static long rule = 0;
                    public static long pri = 0;
                    public static long locx = 0;
                    public static long locy = 0;
                    public static long locz = 0;
                    public static long locaa1x = 0;
                    public static long locaa1y = 0;
                    public static long locaa1z = 0;
                    public static long locaa2x = 0;
                    public static long locaa2y = 0;
                    public static long locaa2z = 0;
                    public static long locaaw = 0;
                    public static long locdir = 0;
                    public static long loctol = 0;
                    public static long sz = 0;
                    public static long lbs = 0;
                    public static long lcbs2 = 0;
                    public static long lcbs3 = 0;
                    public static long air = 0;
                    public static long dir = 0;
                    public static long cmp = 0;
                    public static long loc2rd = 0;
                    public static long locbc = 0;
                    public static long locti = 0;
                    public static long locgc = 0;
                    public static long gps = 0;
                    public static long wtm = 0;
                    public static long locart = 0;
                    public static long veh = 0;
                    public static long loclbr = 0;
                    public static long locstd = 0;
                    public static long number = 0;
                    public static long NEXT = 0;
                }

                public class Actor
                {
                    public static long model = 0;
                    public static long locx = 0;
                    public static long locy = 0;
                    public static long locz = 0;
                    public static long head = 0;
                    public static long actv_bs = 0;
                    public static long actvx = 0;
                    public static long actvy = 0;
                    public static long actvz = 0;
                    public static long achf = 0;
                    public static long awt = 0;
                    public static long awr = 0;
                    public static long awl = 0;
                    public static long awlr = 0;
                    public static long ags = 0;
                    public static long agrd = 0;
                    public static long agvr = 0;
                    public static long actv_NEXT = 0;
                    public static long veh = 0;
                    public static long pedbc = 0;
                    public static long pdbps = 0;
                    public static long gtds = 0;
                    public static long frr = 0;
                    public static long pspdl = 0;
                    public static long foll = 0;
                    public static long folr = 0;
                    public static long tmflw = 0;
                    public static long iaim = 0;
                    public static long whost = 0;
                    public static long cmsty = 0;
                    public static long rsp = 0;
                    public static long rel = 0;
                    public static long accu = 0;
                    public static long hlt = 0;
                    public static long weapon_model = 0;
                    public static long weapon = 0;
                    public static long group = 0;
                    public static long rule = 0;
                    public static long pri = 0;
                    public static long dmv = 0;
                    public static long jtop = 0;
                    public static long jtof = 0;
                    public static long pdcra = 0;
                    public static long cutsh = 0;
                    public static long pCwhT = 0;
                    public static long spawn = 0;
                    public static long spawn1 = 0;
                    public static long spawn2 = 0;
                    public static long spawn3 = 0;
                    public static long objt = 0;
                    public static long objt1 = 0;
                    public static long objt2 = 0;
                    public static long objt3 = 0;
                    public static long team = 0;
                    public static long team1 = 0;
                    public static long team2 = 0;
                    public static long team3 = 0;
                    public static long acts = 0;
                    public static long acts1 = 0;
                    public static long acts2 = 0;
                    public static long acts3 = 0;
                    public static long scrrq = 0;
                    public static long scrrq1 = 0;
                    public static long scrrq2 = 0;
                    public static long scrrq3 = 0;
                    public static long awysrl = 0;
                    public static long awysrl1 = 0;
                    public static long awysrl2 = 0;
                    public static long awysrl3 = 0;
                    public static long nmpass = 0;
                    public static long nmfail = 0;
                    public static long flee = 0;
                    public static long pedcr = 0;
                    public static long pedct = 0;
                    public static long pedbs = 0;
                    public static long pbstwo = 0;
                    public static long pbs3 = 0;
                    public static long pbs4 = 0;
                    public static long pbs5 = 0;
                    public static long pbs6 = 0;
                    public static long pbs7 = 0;
                    public static long pbs8 = 0;
                    public static long pbs9 = 0;
                    public static long pbs10 = 0;
                    public static long pbs11 = 0;
                    public static long pbs12 = 0;
                    public static long pbs13 = 0;
                    public static long pbs14 = 0;
                    public static long pbs15 = 0;
                    public static long pbs16 = 0;
                    public static long pbs17 = 0;
                    public static long pbs18 = 0;
                    public static long pbs19 = 0;
                    public static long pbs20 = 0;
                    public static long pbs21 = 0;
                    public static long pbs22 = 0;
                    public static long pbs23 = 0;
                    public static long psort = 0;
                    public static long pcash = 0;
                    public static long number = 0;
                    public static long NEXT = 0;
                }

                public class Weapon
                {
                    public static long model = 0;
                    public static long locx = 0;
                    public static long locy = 0;
                    public static long locz = 0;
                    public static long head = 0;
                    public static long rotx = 0;
                    public static long roty = 0;
                    public static long sub = 0;
                    public static long clip = 0;
                    public static long bits = 0;
                    public static long dmgmult = 0;
                    public static long brest = 0;
                    public static long vasst = 0;
                    public static long vasst2 = 0;
                    public static long vasst3 = 0;
                    public static long vasst4 = 0;
                    public static long vasso = 0;
                    public static long vasso2 = 0;
                    public static long vasso3 = 0;
                    public static long vasso4 = 0;
                    public static long vasss = 0;
                    public static long vasss2 = 0;
                    public static long vasss3 = 0;
                    public static long vasss4 = 0;
                    public static long rput = 0;
                    public static long vput = 0;
                    public static long vbmbm = 0;
                    public static long vclnt = 0;
                    public static long vclnr = 0;
                    public static long vclnrl = 0;
                    public static long iptnp = 0;
                    public static long wcpm = 0;
                    public static long number = 0;
                    public static long time = 0;
                    public static long pal = 0;
                    public static long blip = 0;
                    public static long randtyp = 0;
                    public static long NEXT = 0;
                }

                public class PlayArea
                {
                    public static long outbt = 0;
                    public static long outbx = 0;
                    public static long outby = 0;
                    public static long outbz = 0;
                    public static long outb1vx = 0;
                    public static long outb1vy = 0;
                    public static long outb1vz = 0;
                    public static long outb2vx = 0;
                    public static long outb2vy = 0;
                    public static long outb2vz = 0;
                    public static long outw = 0;
                    public static long outr = 0;
                    public static long outmm = 0;
                    public static long outbs = 0;
                    public static long outeid = 0;
                    public static long outety = 0;
                    public static long outilv = 0;
                    public static long outonfv = 0;
                    public static long outhc = 0;
                    public static long pribt = 0;
                    public static long bd2t = 0;
                    public static long bd2vx = 0;
                    public static long bd2vy = 0;
                    public static long bd2vz = 0;
                    public static long bd2v2x = 0;
                    public static long bd2v2y = 0;
                    public static long bd2v2z = 0;
                    public static long bd2v1x = 0;
                    public static long bd2v1y = 0;
                    public static long bd2v1z = 0;
                    public static long bd2r = 0;
                    public static long bd2w = 0;
                    public static long out2bs = 0;
                    public static long out2mm = 0;
                    public static long out2wg = 0;
                    public static long out2iv = 0;
                    public static long out2io = 0;
                    public static long out2fp = 0;
                    public static long out2sg = 0;
                    public static long out2bh = 0;
                    public static long out2ilv = 0;
                    public static long out2onfv = 0;
                    public static long out2hc = 0;
                    public static long out2et = 0;
                    public static long out2id = 0;
                    public static long NEXT = 0;
                }

                public class SMS
                {
                    public static long team = 0;
                    public static long smsdt = 0;
                    public static long rule = 0;
                    public static long time = 0;
                    public static long ptsreq = 0;
                    public static long delay = 0;
                    public static long txt = 0;
                    public static long sndall = 0;
                    public static long smsc = 0;
                    public static long smsei = 0;
                    public static long smstl = 0;
                    public static long smspwo = 0;
                    public static long NEXT = 0;
                }

                public class PTemp
                {
                    public static long number = 0;
                    public static long NEXT = 0;
                    public static long pto = 0;
                    public static long ptr = 0;
                    public static long ptm = 0;
                    public static long ptc = 0;
                }

                public class Race
                {
                    public static long aveh = 0;
                    public static long adlc = 0;
                    public static long adlc2 = 0;
                    public static long adlc3 = 0;
                    public static long adlc_NEXT = 0;
                    public class Checkpoints
                    {
                        public static long chs = 0;
                        public static long chs2 = 0;
                        public static long chh = 0;
                        public static long locx = 0;
                        public static long locy = 0;
                        public static long locz = 0;
                        public static long vspn = 0;
                        public static long vspns = 0;
                        public static long sndchk = 0;
                        public static long sndrsp = 0;
                        public static long cpado = 0;
                        public static long cpados = 0;
                        public static long chvs = 0;
                        public static long cpbs1 = 0;
                        public static long cpbs2 = 0;
                        public static long chstR = 0;
                        public static long chstRs = 0;
                        public static long cptfrm = 0;
                        public static long cptfrms = 0;
                        public static long chttu = 0;
                        public static long chttr = 0;
                        public static long chtd = 0;
                        public static long chtds = 0;
                        public static long cpwwt = 0;
                        public static long rdis = 0;
                        public static long trfmvm = 0;
                        public static long grid = 0;
                        public static long gridty = 0;
                        public static long ptp = 0;
                        public static long strtg = 0;
                        public static long rcdam = 0;
                        public static long sgdo = 0;
                        public static long lap = 0;
                        public static long type = 0;
                        public static long gtar = 0;
                        public static long head = 0;
                        public static long lrgs = 0;
                        public static long udgs = 0;
                        public static long gw = 0;
                        public static long gl = 0;
                        public static long lanes = 0;
                        public static long icv = 0;
                        public static long tri1 = 0;
                        public static long tri2 = 0;
                        public static long clbs = 0;
                        public static long cshr = 0;
                        public static long iprem = 0;
                        public static long bsted = 0;
                        public static long retl = 0;
                        public static long cemx = 0;
                        public static long cemn = 0;
                        public static long number = 0;
                        public static long NEXT = 0;
                        public static long NEXT_loc = 0;
                        public static long NEXT_respawn = 0;
                    }
                }

                public class Jobs
                {
                    public static long published_number = 0;
                    public static long published_link = 0;
                    public static long published_owner = 0;
                    public static long published_name = 0;
                    public static long published_year = 0;
                    public static long published_month = 0;
                    public static long published_day = 0;
                    public static long published_type = 0;
                    public static long published_subtype = 0;
                    public static long published_rating = 0;
                    public static long saved_number = 0;
                    public static long saved_link = 0;
                    public static long saved_owner = 0;
                    public static long saved_name = 0;
                    public static long saved_year = 0;
                    public static long saved_month = 0;
                    public static long saved_day = 0;
                    public static long saved_type = 0;
                    public static long saved_subtype = 0;
                    public static long saved_rating = 0;
                    public static long jobs_NEXT = 0;
                }

                public class Teleport_Marker
                {
                    public static long WAz = 0;
                    public static long WA = 0;
                    public static long WC = 0;
                    public static long WH = 0;
                    public static long WI = 0;
                    public static long WE = 0;
                    public static long WF = 0;
                    public static long WG = 0;
                    public static long WJ = 0;
                    public static long WK = 0;
                    public static long WL = 0;
                    public static long WM = 0;
                    public static long WN = 0;
                    public static long WO = 0;
                    public static long WX = 0;
                    public static long WP = 0;
                    public static long WQ = 0;
                    public static long WT = 0;
                    public static long WU = 0;
                    public static long WAb = 0;
                    public static long WAC = 0;
                    public static long WAe = 0;
                    public static long WAg = 0;
                    public static long WAj = 0;
                    public static long WAk = 0;
                    public static long alvpor = 0;
                    public static long NEXT = 0;
                }

                public class ddblip
                {
                    public static long pos = 0;
                    public static long rule = 0;
                    public static long team = 0;
                    public static long type = 0;
                    public static long size = 0;
                    public static long veh = 0;
                    public static long clr = 0;
                    public static long spri = 0;
                    public static long bits = 0;
                    public static long entt = 0;
                    public static long enti = 0;
                    public static long sbr = 0;
                    public static long sbhr = 0;
                    public static long hbr = 0;
                    public static long dbnm = 0;
                    public static long frul = 0;
                    public static long trul = 0;
                    public static long dumsg = 0;
                    public static long dumssg = 0;
                    public static long number = 0;
                    public static long NEXT = 0;
                    public static long NEXT_rul = 0;
                }

                public class Doors
                {
                    public static long loc = 0;
                    public static long fopen = 0;
                    public static long model = 0;
                    public static long _lock = 0;
                    public static long swing = 0;
                    public static long udrle = 0;
                    public static long udtem = 0;
                    public static long udrat = 0;
                    public static long swingu = 0;
                    public static long audst = 0;
                    public static long aurt = 0;
                    public static long uaudst = 0;
                    public static long uaurt = 0;
                    public static long lfp = 0;
                    public static long dtime = 0;
                    public static long mid = 0;
                    public static long dbs = 0;
                    public static long dcoid = 0;
                    public static long fcz = 0;
                    public static long foz = 0;
                    public static long org = 0;
                    public static long dle = 0;
                    public static long dird = 0;
                    public static long dirdv = 0;
                    public static long dirud = 0;
                    public static long dirduv = 0;
                    public static long ORNLO = 0;
                    public static long number = 0;
                    public static long NEXT = 0;
                }

                public class Survival
                {
                    public static long wave = 0;
                    public static long sbits = 0;
                    public static long sbndsr = 0;
                    public static long sbndsh = 0;
                    public static long sbndsbo = 0;
                    public static long sbndst = 0;
                }

                public class Kill
                {
                    public static long rule = 0;
                    public static long pri = 0;
                    public static long lim = 0;
                    public static long jtop = 0;
                    public static long jtof = 0;
                    public static long prbs = 0;
                    public static long mcf = 0;
                    public static long mcp = 0;
                    public static long number = 0;
                    public static long NEXT = 0;
                }

                public class otzone
                {
                    public static long number = 0;
                    public static long otvo = 0;
                    public static long otvt = 0;
                    public static long otbs = 0;
                    public static long otpg = 0;
                    public static long otpl = 0;
                    public static long otrw = 0;
                    public static long NEXT = 0;
                }

                public class PwrUp
                {
                    public static long pwrBS = 0;
                    public static long pwrMH = 0;
                    public static long pwrRC_ = 0;
                }

                public static long menubs = 0;
                public static long menubs2 = 0;
                public static long menubs3 = 0;
                public static long menubs4 = 0;
                public static long menubs5 = 0;
                public static long menubs6 = 0;
                public static long menubs7 = 0;
                public static long menubs8 = 0;
                public static long menubs9 = 0;
                public static long menubs10 = 0;
                public static long menubs11 = 0;
                public static long menubs12 = 0;
                public static long menubs13 = 0;
                public static long menubs14 = 0;
                public static long menubs15 = 0;
                public static long menubs16 = 0;
                public static long menubs17 = 0;
                public static long menubs18 = 0;
                public static long menubs19 = 0;
                public static long menubs20 = 0;
                public static long menubs21 = 0;
                public static long menubs22 = 0;
                public static long menubs23 = 0;
                public static long menubs24 = 0;
                public static long menubs25 = 0;
                public static long menubs26 = 0;
                public static long menubs27 = 0;
                public static long menubs28 = 0;
                public static long menubs29 = 0;
                public static long menubs30 = 0;
                public static long menubs31 = 0;
                public static long menubs32 = 0;

                public static long pnEMPd = 0;
                public static long pnEMPp = 0;

                public static long check_creator = 0x0;
                public static long hide_creator_menu = 0x0;
                public static long preset_version = 0x0;
                public static long WORLDversion = 0x0;
                public static long GlobalPTRversion = 0x0;
                public static long version = 0x0;
                public static long version2 = 0x0;
                public static long version_jobs = 0x0;
                public static long dev = 0x0;
                public static long nextcp = 0x0;
                public static long camptr = 0x0;
                public static long versionptr = 0x0;
                public static long creator_camptr = 0x0;
                public static long img_addy = 0x0;
                public static long cursor_addy = 0x0;
                public static long scrProgram_addy = 0x0;
                public static long[] localptr;

                public static long preset_offset_lts = 0x0;
                public static long preset_offset_dm = 0x0;
                public static long preset_offset_survival = 0x0;
                public static long preset_offset_race = 0x0;
                public static long preset_offset_check = 0x0;
                public static long preset_offset_pcategory = 0x0;
                public static long preset_offset_pcolor = 0x0;

                public static string AOB_worldptr = "";
                public static string AOB_globalptr = "";
                public static string AOB_localptr = "";
                public static string AOB_devptr = "";
                public static string AOB_camptr = "";
                public static string AOB_versionptr = "";
                public static string AOB_creator_camptr = "";
                public static string AOB_nextcp_ptr = "";
                public static string AOB_img_ptr = "";
                public static string AOB_session_ptr = "";
                public static string AOB_cursor_ptr = "";
                public static string AOB_scrProgramptr = "";

                public static string checksum_rstar = "";
                public static string checksum_steam = "";

                public static long skiptest = 0x0;
                public static long templatefix = 0x0;
                public static long cam_zoom = 0x0;
                public static long cam_mode = 0x0;
                public static long dev_steam = 0x0;
                public static long dev_rstar = 0x0;
                public static long dev_epic = 0x0;
                public static long nextcp_steam = 0x0;
                public static long nextcp_rstar = 0x0;
                public static long nextcp_epic = 0x0;
                public static long launch_creator_local_1 = 0x0;
                public static long launch_creator_local_2 = 0x0;
                public static long launch_creator_local_3 = 0x0;
                public static long launch_creator_local_4 = 0x0;
                public static long launch_creator_local_5 = 0x0;
                public static long transitionState = 0x0;
                public static int[] OFFSET_player_loc;
                public static int[] OFFSET_playerveh_loc;
                public static int[] OFFSET_playervehcam_loc;
                public static int[] OFFSET_playerinveh_loc;
                public static int[] OFFSET_cam_loc;
                public static long OFFSET_creator_cam_loc = 0x0;
                public static long steam_accname = 0x0;
                public static long rstar_accname = 0x0;
                public static long epic_accname = 0x0;
                public static long enable_murica = 0x0;
                public static long jobid = 0x0;
                public static long jobpublished = 0x0;
                public static long custom_check = 0x0;
                public static long custom_hovered_model = 0x0;
                public static long custom_dimension_model = 0x0;
                public static long custom_dimension_min = 0x0;
                public static long custom_dimension_max = 0x0;
                public static long templates = 0x0;
                public static long templates_count = 0x0;
                public static long OFFSET_script_name = 0x0;
                // Enhanced keeps no name in the thread, only the joaat hash of its script.
                // 0 means "match by name" (Legacy).
                public static long OFFSET_script_hash = 0x0;
                public static long OFFSET_script_local_start = 0x0;
                public static long OFFSET_current_creator_worker_survival = 0x0;
                public static long OFFSET_current_creator_worker_race = 0x0;
                public static long OFFSET_current_creator_worker_dm = 0x0;
                public static long OFFSET_current_creator_worker_capture = 0x0;
                public static long OFFSET_current_creator_worker_lts = 0x0;
                public static long OFFSET_current_creator_worker_offset_refresh = 0x0;
                public static long OFFSET_current_creator_worker_offset_editing_published = 0x0;
                public static long load_job_flag = 0;
                public static long load_job_id = 0;
                public static long creator_quit_flag = 0;
                public static long OFFSET_current_creator_worker_offset_menu = 0x0;
                public static long OFFSET_current_creator_worker_heading = 0x0;
                public static long OFFSET_current_creator_worker_pos = 0x0;
                public static long OFFSET_current_creator_cam_heading_survival = 0x0;
                public static long OFFSET_current_creator_cam_heading_race = 0x0;
                public static long OFFSET_current_creator_cam_heading_dm = 0x0;
                public static long OFFSET_current_creator_cam_heading_capture = 0x0;
                public static long OFFSET_current_creator_cam_heading_lts = 0x0;
                public static long OFFSET_current_creator_cam_heading_mission = 0x0;
                public static long OFFSET_current_creator_cam_heading_offset = 0x0;
                public static long OFFSET_current_creator_pre_survival = 0x0;
                public static long OFFSET_current_creator_pre_race = 0x0;
                public static long OFFSET_current_creator_pre_dm = 0x0;
                public static long OFFSET_current_creator_pre_capture = 0x0;
                public static long OFFSET_current_creator_pre_lts = 0x0;
                public static long OFFSET_current_creator_pre_menu_gm = 0x0;
                public static long OFFSET_current_creator_pre_test1 = 0x0;
                public static long OFFSET_current_creator_pre_test2 = 0x0;
                public static long OFFSET_current_creator_pre_color = 0x0;
                public static long OFFSET_current_creator_pre_category_num = 0x0;
                public static long OFFSET_current_creator_pre_alignment = 0x0;
                public static long OFFSET_current_creator_placement_race = 0x0;
                public static long OFFSET_current_creator_placement_lts = 0x0;
                public static long OFFSET_current_creator_placement_capture = 0x0;
                public static long OFFSET_current_creator_placement_dm = 0x0;
                public static long OFFSET_current_creator_placement_survival = 0x0;
                public static long OFFSET_current_creator_placement_display = 0x0;
                public static long OFFSET_current_creator_placement_template_objects = 0x0;
                public static long OFFSET_precise_template_category = 0x0;
                public static long OFFSET_precise_menu_advanced = 0x0;
                public static long OFFSET_precise_menu_position = 0x0;
                public static long OFFSET_precise_menu_rotation = 0x0;
                public static long OFFSET_current_creator_pre_prop_num = 0x0;
                public static long OFFSET_current_creator_pre_publish = 0x0;
                public static long OFFSET_current_creator_pre_previous_menu = 0x0;
                public static long OFFSET_current_creator_pre_current_menu = 0x0;
                public static long OFFSET_current_creator_pre_idk = 0x0;
                public static long OFFSET_current_creator_test_survival = 0x0;
                public static long OFFSET_current_creator_test_race = 0x0;
                public static long OFFSET_current_creator_test_dm = 0x0;
                public static long OFFSET_current_creator_test_capture = 0x0;
                public static long OFFSET_current_creator_test_lts = 0x0;
                public static long OFFSET_current_creator_refresh_lts = 0x0;
                public static long OFFSET_current_creator_refresh_mission = 0x0;
                public static long OFFSET_current_creator_refresh_capture = 0x0;
                public static long OFFSET_current_creator_refresh_race = 0x0;
                public static long OFFSET_current_creator_refresh_survival = 0x0;
                public static long OFFSET_current_creator_refresh_dm = 0x0;


                public static long todhr = 0;
                public static long todmn = 0;
                public static long testcomplete = 0;
                public static long dec = 0;
                public static long nm = 0;
                public static long sztag = 0;
                public static long start = 0;
                public static long cordmbs = 0;
                public static long turammo = 0;
                public static long turgudm = 0;
                public static long iplyli = 0;
                public static long teamvbs = 0;
                public static long irbs = 0;
                public static long irbs2 = 0;
                public static long irbs3 = 0;
                public static long irbs4 = 0;
                public static long irbs5 = 0;
                public static long irbs6 = 0;
                public static long irbs7 = 0;
                public static long irbs8 = 0;
                public static long irbs9 = 0;
                public static long irbs10 = 0;
                public static long irbs11 = 0;
                public static long irbs12 = 0;
                public static long irbs13 = 0;
                public static long irbs14 = 0;
                public static long irbs15 = 0;
                public static long irbs16 = 0;
                public static long irbs_NEXT = 0;
                public static long irfbs = 0;
                public static long tmbts = 0;
                public static long tmbt2 = 0;
                public static long tmbt3 = 0;
                public static long tmbt4 = 0;
                public static long scene = 0;
                public static long cam = 0;
                public static long camp = 0;
                public static long camh = 0;
                public static long camf = 0;
                public static long xpr = 0;
                public static long cshr = 0;
                public static long ltm = 0;
                public static long ctsc = 0;
                public static long mrd = 0;
                public static long type = 0;
                public static long subtype = 0;
                public static long ngjob = 0;
                public static long racetype = 0;
                public static long alttype = 0;
                public static long endtype = 0;
                public static long photo = 0;
                public static long phpo = 0;
                public static long adverm = 0;
                public static long twrst = 0;
                public static long iplop = 0;
                public static long iplop2 = 0;
                public static long intop = 0;
                public static long intop2 = 0;
                public static long intop3 = 0;
                public static long rloft = 0;
                public static long rloftv = 0;
                public static long teamv = 0;
                public static long tehlh = 0;
                public static long tehrn = 0;
                public static long tvmt = 0;
                public static long tvma = 0;
                public static long tvmac = 0;
                public static long tvBomb = 0;
                public static long tvmet = 0;
                public static long tvmspoil = 0;
                public static long tvpm = 0;
                public static long tvnc = 0;
                public static long csttn = 0;
                public static long tenms = 0;
                public static long mrtl = 0;
                public static long blmpmsg = 0;
                public static long tblpv1 = 0;
                public static long tblpv2 = 0;
                public static long tblpv3 = 0;
                public static long tblpv4 = 0;
                public static long gear = 0;
                public static long gear_NEXT = 0;
                public static long bla = 0;
                public static long sia = 0;
                public static long inv = 0;
                public static long inv2 = 0;
                public static long inv3 = 0;
                public static long inv4 = 0;
                public static long invsw = 0;
                public static long minv = 0;
                public static long minv2 = 0;
                public static long minv3 = 0;
                public static long minv4 = 0;
                public static long mpaumxscr = 0;
                public static long teambal = 0;
                public static long clrovr = 0;
                public static long nrl = 0;
                public static long plyl = 0;
                public static long plvrl = 0;
                public static long numpt = 0;
                public static long tnum = 0;
                public static long min = 0;
                public static long num = 0;
                public static long mnumpt = 0;
                public static long mtnum = 0;
                public static long vsbsout = 0;
                public static long vsclout = 0;
                public static long vsthout = 0;
                public static long vshwout = 0;
                public static long vsenout = 0;
                public static long vstgout = 0;
                public static long vsdfstc = 0;
                public static long charcon = 0;
                public static long rlopt = 0;
                public static long teamrvc = 0;
                public static long teamrvcs = 0;
                public static long boud = 0;
                public static long mts = 0;
                public static long mslr = 0;
                public static long mcvbs = 0;
                public static long patm = 0;
                public static long bla2 = 0;
                public static long bnd2 = 0;
                public static long vehrsp = 0;
                public static long fail = 0;
                public static long fail_txt = 0;
                public static long pcbd = 0;
                public static long trel = 0;
                public static long trrt = 0;
                public static long tmt = 0;
                public static long tms = 0;
                public static long numRounds = 0;
                public static long weth = 0;
                public static long tod = 0;
                public static long traf = 0;
                public static long trafpb = 0;
                public static long inumbnc = 0;
                public static long casCol = 0;
                public static long casPat = 0;
                public static long casArc = 0;
                public static long arnTh = 0;
                public static long arnTy = 0;
                public static long arnLi = 0;
                public static long anfMBS = 0;
                public static long newausc = 0;
                public static long ausc = 0;
                public static long musx = 0;
                public static long apeds = 0;
                public static long vdm = 0;
                public static long geard = 0;
                public static long ofovr = 0;
                public static long itsms = 0;
                public static long dtn = 0;
                public static long disar = 0;
                public static long cposr = 0;
                public static long cpopr = 0;
                public static long cpohr = 0;
                public static long cporv = 0;
                public static long vss = 0;
                public static long shdtxt = 0;
                public static long minspd = 0;
                public static long mspdlp = 0;
                public static long mspdsv = 0;
                public static long fsdtmr = 0;
                public static long frndf = 0;
                public static long mspdmx = 0;
                public static long vehdmro = 0;
                public static long vehdmri = 0;
                public static long teamrvp = 0;
                public static long inpts = 0;
                public static long player_number = 0;
                public static long player_loc = 0;
                public static long player_head = 0;
                public static long player_bit = 0;
                public static long player_veh = 0;
                public static long player_tars = 0;
                public static long player_vfrs = 0;
                public static long player_vfre = 0;
                public static long player_ty = 0;
                public static long player_as = 0;
                public static long player_qu = 0;
                public static long player_gg = 0;
                public static long player_ar = 0;
                public static long bmmxh = 0;
                public static long bmspm = 0;
                public static long bmsjd = 0;
                public static long bmstd = 0;
                public static long bmmph = 0;
                public static long bmhrgn = 0;
                public static long bmhok = 0;
                public static long bmmdm = 0;
                public static long beast_next = 0;
                public static long tmvhp = 0;
                public static long teamrvbh = 0;
                public static long tmvds = 0;
                public static long bdprt = 0;
                public static long bdpst = 0;
                public static long txt0 = 0;
                public static long txt_NEXT = 0;
                public static long NEXT_txt = 0;
                public static long tstrt = 0;
                public static long next_settings = 0;
                public static long team_NEXT_settings = 0;
                public static long team_NEXT = 0;
                public static long dtmp = 0;
                public static long dtmp2 = 0;
                public static long dtmp3 = 0;
                public static long ptint = 0;
                public static long pptint = 0;
                public static long tsc = 0;
                public static long mcry = 0;
                public static long mcstr = 0;
                public static long mcsrm = 0;
                public static long mcmp = 0;
                public static long ivm = 0;
                public static long wchg = 0;
                public static long fkwl = 0;
                public static long fiispr = 0;
                public static long itvsd = 0;
                public static long itved = 0;
                public static long dpos = 0;
                public static long dpos2 = 0;
                public static long dpost = 0;
                public static long dost = 0;
                public static long drpr = 0;
                public static long drph = 0;
                public static long dlcrel = 0;
                public static long abits = 0;
                public static long sdobs = 0;
                public static long edobs = 0;
                public static long dogps = 0;
                public static long bfm = 0;
                public static long rcvs = 0;
                public static long tmtsr = 0;
                public static long tmrph = 0;
                public static long spar = 0;
                public static long nrcid = 0;
                public static long nrmtt = 0;
                public static long chksfx = 0;
                public static long alrtLocal = 0;
                public static long dtspk = 0;
                public static long dtspP = 0;
                public static long entCont_Num = 0;
                public static long creator_end = 0;
                public static long armr = 0;
                public static long gbtp = 0;
                public static long gbtpm = 0;
                public static long gbtpi = 0;
                public static long gbtpp = 0;
                public static long gbtp_next = 0;
                public static long gbtp_size = 0;
                public static long gbnum = 0;
                public static long gbngn = 0;
                public static long gblgn = 0;
                public static long gbcol = 0;
                public static long gbdel = 0;
                public static long gbmax = 0;
                public static long gbngm = 0;
                public static long gbaie = 0;
                public static long gbfnr = 0;
                public static long gbvhl = 0;
                public static long gacc = 0;
                public static long gfld = 0;
                public static long gbat = 0;
                public static long gbv1 = 0;
                public static long gbv2 = 0;
                public static long gbaw = 0;
                public static long rsgmx = 0;
                public static long rsgng = 0;
                public static long rsgbs = 0;
                public static long otxsgo = 0;
                public static long ppk = 0;
                public static long current_team_test = 0;
                public static long eoid = 0;
                public static long eoet = 0;
                public static long eoir = 0;
                public static long eoep = 0;
                public static long trsrl = 0;
                public static long trstf = 0;
                public static long trcmn = 0;
                public static long trsth = 0;
                public static long trstp = 0;
                public static long mcpbs1 = 0;
                public static long mcpbs2 = 0;
                public static long mcpbs3 = 0;
                public static long mcobs = 0;
                public static long rpgbs1 = 0;
                public static long rpgbs2 = 0;
                public static long rpgbs3 = 0;
                public static long mcgbs1 = 0;
                public static long mcgbs2 = 0;
                public static long mcgbs3 = 0;
                public static long cspnm = 0;
                public static long csvnm = 0;
                public static long csonm = 0;
                public static long stpos = 0;
                public static long pol = 0;
            }
        }

        public class Editor
        {
            public static List<string> csttn_values = new List<string>();
            public static List<int> tmt_values = new List<int>();
            public static List<string> prop_boostspd = new List<string>();
            public static List<string> prop_model_booster = new List<string>();
            public static List<string> prop_model_slowdown = new List<string>();
            public static List<string> prop_model_centitydef_whitelist = new List<string>();
            public static List<string> prop_model_stunt_with_color_option = new List<string>();
            public static List<string> prop_model_blacklisted = new List<string>();
            public static List<string> dprop_model_activationtimer = new List<string>();

            public static List<ScrPatches> ScrPatches = new List<ScrPatches>();
            public static List<ScrPatches> ScrPatchesDev = new List<ScrPatches>();

            public static List<Prop> PropList = new List<Prop>();
            public static List<string> PropCategories = new List<string>();
            public static List<int> PropListID = new List<int>();

            public static List<Vehicle> VehList = new List<Vehicle>();
            public static List<string> VehCategories = new List<string>();
            public static List<int> VehListID = new List<int>();

            public static List<Weapon> WeaponList = new List<Weapon>();
            public static List<string> WeaponCategories = new List<string>();
            public static List<int> WeaponListID = new List<int>();

            public static List<Actor> ActorList = new List<Actor>();

            public static readonly string[] Variation_Apocalypse = new[] { "Apocalypse 1", "Apocalypse 2", "Apocalypse 3", "Apocalypse 4", "Apocalypse 5", "Apocalypse 6", "Apocalypse 7", "Apocalypse 8", "Apocalypse 9", "Apocalypse 10", "Apocalypse 11", "Apocalypse 12" };
            public static readonly string[] Variation_Future_Shock = new[] { "Future Shock 1", "Future Shock 2", "Future Shock 3", "Future Shock 4", "Future Shock 5", "Future Shock 6", "Future Shock 7", "Future Shock 8", "Future Shock 9", "Future Shock 10" };
            public static readonly string[] Variation_Nightmare = new[] { "Nightmare 1", "Nightmare 2", "Nightmare 3", "Nightmare 4", "Nightmare 5", "Nightmare 6", "Nightmare 7", "Nightmare 8", "Nightmare 9", "Nightmare 10" };
            public static readonly string[] Arena_Variations = new[] { "Apocalypse", "Future Shock", "Nightmare" };
            public static readonly string[] Variation_Time = new[] { "Atlantis", "Evening", "Hell", "Midday", "Morning", "Night", "Saccharine", "Sandstorm", "Storm", "Toxic" };

            public static int TransformVehiclesCount = 12;
            public static IEnumerable<int> TransformVehicles = Enumerable.Range(1, TransformVehiclesCount);
            public static readonly ObservableCollection<string> Vehiclenames = new ObservableCollection<string>(new[] { "adder", "airbus", "airtug", "akula", "akuma", "aleutian", "alkonost", "alpha", "alphaz1", "ambulance", "annihilator", "annihilator2", "apc", "arbitergt", "ardent", "armytanker", "armytrailer", "armytrailer2", "asbo", "asea", "asea2", "asterope", "asterope2", "astron", "astron2", "autarch", "avarus", "avenger", "avenger2", "avenger3", "avenger4", "avisa", "bagger", "baletrailer", "baller", "baller2", "baller3", "baller4", "baller5", "baller6", "baller7", "baller8", "banshee", "banshee2", "barracks", "barracks2", "barracks3", "barrage", "bati", "bati2", "beast", "benson", "benson2", "besra", "bestiagts", "bf400", "bfinjection", "biff", "bifta", "bison", "bison2", "bison3", "bjxl", "blade", "blazer", "blazer2", "blazer3", "blazer4", "blazer5", "blimp", "blimp2", "blimp3", "blista", "blista2", "blista3", "bmx", "boattrailer", "bobcatxl", "bodhi2", "bombushka", "boor", "boxville", "boxville2", "boxville3", "boxville4", "boxville5", "boxville6", "brawler", "brickade", "brickade2", "brigham", "brioso", "brioso2", "brioso3", "broadway", "bruiser", "bruiser2", "bruiser3", "brutus", "brutus2", "brutus3", "btype", "btype2", "btype3", "buccaneer", "buccaneer2", "buffalo", "buffalo2", "buffalo3", "buffalo4", "buffalo5", "bulldozer", "bullet", "burrito", "burrito2", "burrito3", "burrito4", "burrito5", "bus", "buzzard", "buzzard2", "cablecar", "caddy", "caddy2", "caddy3", "calico", "camper", "caracara", "caracara2", "carbonizzare", "carbonrs", "cargobob", "cargobob2", "cargobob3", "cargobob4", "cargoplane", "cargoplane2", "casco", "cavalcade", "cavalcade2", "cavalcade3", "cerberus", "cerberus2", "cerberus3", "champion", "cheburek", "cheetah", "cheetah2", "chernobog", "chimera", "chino", "chino2", "cinquemila", "cliffhanger", "clique", "clique2", "club", "coach", "cog55", "cog552", "cogcabrio", "cognoscenti", "cognoscenti2", "comet2", "comet3", "comet4", "comet5", "comet6", "comet7", "conada", "conada2", "contender", "coquette", "coquette2", "coquette3", "coquette4", "corsita", "coureur", "cruiser", "crusader", "cuban800", "cutter", "cyclone", "cyclone2", "cypher", "daemon", "daemon2", "deathbike", "deathbike2", "deathbike3", "defiler", "deity", "deluxo", "deveste", "deviant", "diablous", "diablous2", "dilettante", "dilettante2", "dinghy", "dinghy2", "dinghy3", "dinghy4", "dinghy5", "dloader", "docktrailer", "docktug", "dodo", "dominator", "dominator2", "dominator3", "dominator4", "dominator5", "dominator6", "dominator7", "dominator8", "dominator9", "dorado", "double", "drafter", "draugur", "drifteuros", "driftfr36", "driftfuto", "driftjester", "driftremus", "drifttampa", "driftyosemite", "driftzr350", "dubsta", "dubsta2", "dubsta3", "dukes", "dukes2", "dukes3", "dump", "dune", "dune2", "dune3", "dune4", "dune5", "duster", "dynasty", "elegy", "elegy2", "ellie", "emerus", "emperor", "emperor2", "emperor3", "enduro", "entity2", "entity3", "entityxf", "esskey", "eudora", "euros", "everon", "everon2", "exemplar", "f620", "faction", "faction2", "faction3", "fagaloa", "faggio", "faggio2", "faggio3", "faggion", "fbi", "fbi2", "fcr", "fcr2", "felon", "felon2", "feltzer2", "feltzer3", "firetruk", "fixter", "flashgt", "flatbed", "fmj", "foot", "forklift", "formula", "formula2", "fq2", "fr36", "freecrawler", "freight", "freightcar", "freightcar2", "freightcont1", "freightcont2", "freightgrain", "freighttrailer", "frogger", "frogger2", "fugitive", "furia", "furoregt", "fusilade", "futo", "futo2", "gargoyle", "gauntlet", "gauntlet2", "gauntlet3", "gauntlet4", "gauntlet5", "gauntlet6", "gb200", "gburrito", "gburrito2", "glendale", "glendale2", "gp1", "graintrailer", "granger", "granger2", "greenwood", "gresley", "growler", "gt500", "guardian", "habanero", "hakuchou", "hakuchou2", "halftrack", "handler", "hauler", "hauler2", "havok", "hellion", "hermes", "hexer", "hotknife", "hotring", "howard", "hunter", "huntley", "hustler", "hydra", "ignus", "ignus2", "imorgon", "impaler", "impaler2", "impaler3", "impaler4", "impaler5", "impaler6", "imperator", "imperator2", "imperator3", "inductor", "inductor2", "infernus", "infernus2", "ingot", "innovation", "insurgent", "insurgent2", "insurgent3", "intruder", "issi2", "issi3", "issi4", "issi5", "issi6", "issi7", "issi8", "italigtb", "italigtb2", "italigto", "italirsx", "iwagen", "jackal", "jb700", "jb7002", "jester", "jester2", "jester3", "jester4", "jet", "jetmax", "journey", "journey2", "jubilee", "jugular", "kalahari", "kamacho", "kanjo", "kanjosj", "khamelion", "khanjali", "komoda", "kosatka", "krieger", "kuruma", "kuruma2", "l35", "landstalker", "landstalker2", "lazer", "le7b", "lectro", "lguard", "limo2", "lm87", "locust", "longfin", "lurcher", "luxor", "luxor2", "lynx", "mamba", "mammatus", "manana", "manana2", "manchez", "manchez2", "manchez3", "marquis", "marshall", "massacro", "massacro2", "maverick", "menacer", "mesa", "mesa2", "mesa3", "metrotrain", "michelli", "microlight", "miljet", "minitank", "minivan", "minivan2", "mixer", "mixer2", "mogul", "molotok", "monroe", "monster", "monster3", "monster4", "monster5", "monstrociti", "moonbeam", "moonbeam2", "mower", "mule", "mule2", "mule3", "mule4", "mule5", "nebula", "nemesis", "neo", "neon", "nero", "nero2", "nightblade", "nightshade", "nightshark", "nimbus", "ninef", "ninef2", "nokota", "novak", "omnis", "omnisegt", "openwheel1", "openwheel2", "oppressor", "oppressor2", "oracle", "oracle2", "osiris", "outlaw", "packer", "panthere", "panto", "paradise", "paragon", "paragon2", "pariah", "patriot", "patriot2", "patriot3", "patrolboat", "pbus", "pbus2", "pcj", "penetrator", "penumbra", "penumbra2", "peyote", "peyote2", "peyote3", "pfister811", "phantom", "phantom2", "phantom3", "phoenix", "picador", "pigalle", "polgauntlet", "police", "police2", "police3", "police4", "police5", "policeb", "policeold1", "policeold2", "policet", "polmav", "pony", "pony2", "postlude", "pounder", "pounder2", "powersurge", "prairie", "pranger", "predator", "premier", "previon", "primo", "primo2", "proptrailer", "prototipo", "pyro", "r300", "radi", "raiden", "raiju", "raketrailer", "rallytruck", "rancherxl", "rancherxl2", "rapidgt", "rapidgt2", "rapidgt3", "raptor", "ratbike", "ratel", "ratloader", "ratloader2", "rcbandito", "reaper", "rebel", "rebel2", "rebla", "reever", "regina", "remus", "rentalbus", "retinue", "retinue2", "revolter", "rhapsody", "rhinehart", "rhino", "riata", "riot", "riot2", "ripley", "rocoto", "rogue", "romero", "rrocket", "rt3000", "rubble", "ruffian", "ruiner", "ruiner2", "ruiner3", "ruiner4", "rumpo", "rumpo2", "rumpo3", "ruston", "s80", "s95", "sabregt", "sabregt2", "sadler", "sadler2", "sanchez", "sanchez2", "sanctus", "sandking", "sandking2", "savage", "savestra", "sc1", "scarab", "scarab2", "scarab3", "schafter2", "schafter3", "schafter4", "schafter5", "schafter6", "schlagen", "schwarzer", "scorcher", "scramjet", "scrap", "seabreeze", "seashark", "seashark2", "seashark3", "seasparrow", "seasparrow2", "seasparrow3", "seminole", "seminole2", "sentinel", "sentinel2", "sentinel3", "sentinel4", "serrano", "seven70", "shamal", "sheava", "sheriff", "sheriff2", "shinobi", "shotaro", "skylift", "slamtruck", "slamvan", "slamvan2", "slamvan3", "slamvan4", "slamvan5", "slamvan6", "sm722", "sovereign", "specter", "specter2", "speeder", "speeder2", "speedo", "speedo2", "speedo4", "speedo5", "squaddie", "squalo", "stafford", "stalion", "stalion2", "stanier", "starling", "stinger", "stingergt", "stingertt", "stockade", "stockade3", "stratum", "streamer216", "streiter", "stretch", "strikeforce", "stromberg", "stryder", "stunt", "submersible", "submersible2", "sugoi", "sultan", "sultan2", "sultan3", "sultanrs", "suntrap", "superd", "supervolito", "supervolito2", "surano", "surfer", "surfer2", "surfer3", "surge", "swift", "swift2", "swinger", "t20", "taco", "tahoma", "tailgater", "tailgater2", "taipan", "tampa", "tampa2", "tampa3", "tanker", "tanker2", "tankercar", "taxi", "technical", "technical2", "technical3", "tempesta", "tenf", "tenf2", "terbyte", "terminus", "tezeract", "thrax", "thrust", "thruster", "tigon", "tiptruck", "tiptruck2", "titan", "toreador", "torero", "torero2", "tornado", "tornado2", "tornado3", "tornado4", "tornado5", "tornado6", "toro", "toro2", "toros", "tourbus", "towtruck", "towtruck2", "tr2", "tr3", "tr4", "tractor", "tractor2", "tractor3", "trailerlarge", "trailerlogs", "trailers", "trailers2", "trailers3", "trailers4", "trailersmall", "trailersmall2", "trash", "trash2", "trflat", "tribike", "tribike2", "tribike3", "trophytruck", "trophytruck2", "tropic", "tropic2", "tropos", "tug", "tula", "tulip", "tulip2", "turismo2", "turismo3", "turismor", "tvtrailer", "tyrant", "tyrus", "utillitruck", "utillitruck2", "utillitruck3", "vacca", "vader", "vagner", "vagrant", "valkyrie", "valkyrie2", "vamos", "vectre", "velum", "velum2", "verlierer2", "verus", "vestra", "vetir", "veto", "veto2", "vigero", "vigero2", "vigero3", "vigilante", "vindicator", "virgo", "virgo2", "virgo3", "virtue", "viseris", "visione", "vivanite", "volatol", "volatus", "voltic", "voltic2", "voodoo", "voodoo2", "vortex", "vstr", "warrener", "warrener2", "washington", "wastelander", "weevil", "weevil2", "windsor", "windsor2", "winky", "wolfsbane", "xa21", "xls", "xls2", "yosemite", "yosemite2", "yosemite3", "youga", "youga2", "youga3", "youga4", "z190", "zeno", "zentorno", "zhaba", "zion", "zion2", "zion3", "zombiea", "zombieb", "zorrusso", "zr350", "zr380", "zr3802", "zr3803", "ztype" });

            public static readonly string[] PropColor = new[] { "Default", "Murica (America)", "Red", "Blue", "Purple", "Black", "White", "Gray", "Yellow", "Orange", "Green", "Pink", "Racing", "Black & Yellow", "Orange & Blue", "Green & Yellow", "Pink & Gray" };
            public static readonly string[] PropBoostSpd = new[] { "Weak", "Normal", "Strong", "Extra Strong", "Ultra Strong" };

            public static readonly string[] rvc = new[] { "None", "Black", "Black Poly", "Silver", "Gunmetal", "Matte Light Gray", "White", "Red", "Matt Dark Red", "Matte Orange", "Matte Yellow", "Dark Green", "Green", "Gasoline Green", "Blue Poly", "Dark Blue", "Blue", "Yellow", "Race Yellow", "Sandy Brown", "Gold", "Bronze", "Hot Pink", "Pfister Pink", "Salmon Pink", "Bright Purple", "Schafter Purple", "Lime Green", "Bright Green", "Ultra Blue", "Grace Red", "Cabernet Red", "Orange", "Bright Orange", "Matte Black", "Matte Gray", "Matte Ice White", "Matte Blue", "Matte Red", "Matte Green", "Matte Lime Green", "Matte Schafter Purple", "Matte Dark Earth", "Matte Desert Tan", "Brushed Steel", "Brushed Aluminum", "Brushed Gold", "Chrome" };
            public static readonly int[] window_value = new[] { -1, 0, 1, 2, 3 };
            public static readonly string[] window_name = new[] { "None", "Light Smoke", "Dark Smoke", "Limo", "Pure Black" };

            public static int[] iplopArray = { };
            public static string jobdata;
            public static bool vdm = false;


            public static bool MPropsInitialized = false;
            public static List<ModdedPropSource> ModdedPropSources { get; } = new List<ModdedPropSource>
            {
                new ModdedPropSource("fm_race_creator", "Race"),
                new ModdedPropSource("fm_lts_creator", "LTS"),
                new ModdedPropSource("fm_dm_creator", "Deathmatch"),
                new ModdedPropSource("fm_capture_creator", "Capture"),
                new ModdedPropSource("fm_survival_creator", "Survival")
            };

            public static readonly string moddedpropson = "58E264AC,8791883B,B467C540,4A03746E,0BD8BDA0,745F3383,4C70C83E,B39B99E2,F6FE2EF1,1A2FCEB6,20F32B15,8ACED9EC,DF9841D7,C42C019A,8F2D17AA,8973A868,B4978950,A2E165E4,14E3D6EB,97A58869,1076D97B,36393EA8,73268708,DB69770D,C2BC19CD,4DB7E514,8E8C7A5B,9C762726,5A1A1BD9,D3406E34,07121AC4,42C280E4,5853974F,0426F46E,7A6450F5,ED6065BC,B8D1885F,B6A56F76,9100EA55,D44295DD,FFD7D47D,C3F13FCC,B2CB4D2A,BAFBD223,CAF48C6C,D322A87F,6D9874A0,0C348391,C54C0CD2,26E15E59,28B0CD93,367E07F0,B92C6FA7,B1E5EDC9,3159696B,5376930C,A4D4093E,FAF5AA0C,7EA4A671,6D496CC1,F20B1BDD,662C9D2C,708D300F,9D536896,08DA1C0E,9058A753";
            public static readonly string moddedpropsoff = "2E7C9A23,7C3C9BEA,A085E47C,5687D081,3EA83D4D,306BE0C4,B34BC429,45DFEF67,295B365E,18C49531,B6CACC47,F7752D66,BB188579,4C30B8CF,F724026D,09DDA7E0,65DCD413,6D51EECB,1F319BE4,50C22184,B87E6DE1,E15CA04A,2DA13CC7,DE469BCF,ACF07F3A,CE14C182,7E86A267,B1A00899,7B9FAAA0,A105F56C,8FE85331,8F12D266,A56CFF1A,723E18BD,C7EDC41F,46A74190,D783C7C7,FF374A2B,A1E58F89,BB9B09AC,40D23ECE,7DA7C387,B5DD1656,C689B79B,05972FB1,4ED9C235,B9C69815,CC003C88,1649D11A,673AB38B,532B112B,6558B586,1FCA2A6A,7C9F3E0C,CA0958DF,D84B7563,575CF388,25A7101D,BF741865,09882DA0,2929EE13,2E4DF59F,F676077C,F79A0AF4,292C078E,4C0D000B";
            public static readonly string[] weaponactorArray = new[] { "1B06D571", "13532244", "2BE6766B", "BFEFFF6D", "AF113F99", "9D07F764", "7FD62962", "1D073A89", "7846A318", "05FC3C11", "0C472FE2", "A284510B", "B1CA77B1", "42BF8A85", "99B507EA", "958A4A8F", "A2719263", "DB1AA450", "93E220BD", "2C3731D9", "24B17070", "BA45E8B8", "83BF0278", "84BD7BFD", "4E875F73", "19044EE0", "C0A3098D", "7F229F94", "3AABBBAA", "9D61E50F", "E284C527", "EFE7E2DF", "0A3D4D34" };
            public static readonly string[] ActorWeaponNamesSorted = new[] { "ADVANCEDRIFLE", "ASSAULTRIFLE", "ASSAULTSHOTGUN", "ASSAULTSMG", "BAT", "BULLPUPRIFLE", "BULLPUPSHOTGUN", "CARBINERIFLE", "COMBATMG", "COMBATPDW", "CROWBAR", "GRENADE", "GRENADELAUNCHER", "HAMMER", "HEAVYSHOTGUN", "HEAVYSNIPER", "KNIFE", "MACHINEPISTOL", "MG", "MICROSMG", "MINIGUN", "MOLOTOV", "PIPEBOMB", "PISTOL", "PUMPSHOTGUN", "RPG", "SAWNOFFSHOTGUN", "SMG", "SNIPERRIFLE", "SPECIALCARBINE", "STICKYBOMB", "UNARMED", "WRENCH" };
            public static readonly int[] radioArray = new[] { -1, 1081038572, 749278879, -1205361198, -1364182385, 5712900, 1594321599, 1512393045, -945650630, -1869642782, -1933152170, 1583478223, 2079804834, 1084546764, 1429421331, 165889415, -1243779303, -99099081, 1905986299, -121722346, -145927566 };
            public static readonly string[] RadioNames = new[] { "Searching", "West Coast Talk", "Rebel Radio", "Soulwax FM", "East Los FM", "West Coast Classics", "Blue Ark", "Worldwide FM", "FlyLo FM", "The Lowdown 91.1", "The Lab", "Radio Mirror Park", "Space 103.2", "Vinewood Boulevard", "Blonded Los Santos 97.8 FM", "Los Santos Rock", "Non-Stop-Pop FM", "Radio Los Santos", "Channel X", "Los Santos Underground Radio", "Self Radio" };
            public static readonly List<string> WeaponRespawnTimeDisplay = new List<string> { "10 Seconds", "20 Seconds", "30 Seconds", "40 Seconds", "50 Seconds", "1 Minute", "02:40 Hours" };
            public static readonly int[] actorrule = new[] { 0, 1, 2, 3, 4, 5, 11, 12, 13, 15, 24 };

            //types
            public static string[] race = { };
            public static string[] deathmatch = { };
            public static string[] mission = { };

            //vehicles
            public static string[] vehicles_name = { };
            public static int[] vehicles_value = { };
            public static readonly uint[] vdm_veh = new uint[] { 4154065143, 788045382, 4108429845, 2166734073, 1032823388, 2046537925, 837858166, 788747387, 3013282534, 782665360, 970385471, 1938952078 };

            public static int[] color_ids = { };


            public static string[] country_codes = new[] { "en", "de", "es", "fr", "it", "ja", "ko", "pl", "ru", "zh", "zh-cn", "pt", "pt-pt", "en-gb", "es-mx" };

            //avehs
            public static readonly string[] classes = new[] { "'Compacts", "Sedans", "SUVs", "Coupes", "Muscle", "Sports Classics", "Sports", "Super", "Motorcycles", "Off-Road", "Industrial", "Utility", "Vans", "Cycles", "Custom", "Special", "Weaponized", "Arena Contender", "Open Wheel", "Go Kart", "Tuner'" };
            public static readonly string[] Compacts = new[] { "'Blista", "Dilettante", "Issi", "Prairie", "Rhapsody", "Panto", "Brioso R/A", "Issi Classic", "Blista Kanjo", "Asbo", "Club", "Brioso 300", "Weevil'" };
            public static readonly string[] Sedans = new[] { "'Asea", "Asterope", "Fugitive", "Premier", "Primo", "Schafter", "Stainer", "Super Diamond", "Surge", "Tailgater", "Washington", "Glendale", "Warrener", "Primo Custom", "Schafter V12", "Schafter V12 armored", "Schafter LWB", "Schafter LWB armored", "Cognoscenti 55", "Cognoscenti 55 armored", "Cognoscenti", "Cognoscenti armored", "Stafford", "Glendale Custom", "Warrener HKR", "Tailgator @", "Enus Deity", "Lampadati Cinquemila'" };
            public static readonly string[] SUVs = new[] { "'Baller", "Baller 2013", "BeeJay XL", "Cavalcade", "Crusader", "Dubsta", "Granger", "Gresley", "Landstalker", "Mesa", "Park Ranger", "Radius", "Seminole", "Serrano", "Dubsta 6X6", "Huntley S", "Baller LE", "Baller LE LWB", "Baller LE armored", "Baller LE LWB armored", "XLS", "XLS armored", "Contender", "Patriot", "FQ2", "Habanero", "Toros", "Novak", "Rebla GTS", "Landstalker XL", "Seminole Frontier", "Squaddie", "Pfister Astron", "Enus Jubilee'" };
            public static readonly string[] Coupes = new[] { "'Cognoscenti Cabrio", "Exemplar", "F620", "Felon GT", "Jackal", "Oracle", "Sentinel", "Windsor", "Windsor Cabrio", "Previon'" };
            public static readonly string[] Muscle = new[] { "'Buccaneer", "Dominator", "Gauntlet", "Phoenix", "Picador", "Ruiner", "Sabre Turbo", "Vigero", "Hotknife", "Blade", "Rat-Truck", "Slamvan", "Dukes", "Stallion", "Vigro", "Coquette Black Fin", "Chino", "Faction", "Faction Custom", "Moonbeam Custom", "Chino Custom", "Voodoo Custom", "Buccaneer Custom", "Nightshade", "Faction Custom Donk", "Slamvan Custom", "Virgo Classic", "Virgo Classic Custom", "Sabre Turbo Custom", "Pisswasser Dominator", "Redwood Gauntlet", "Burger Shot Stallion", "Duke ODeath", "Yosemite", "Hermes", "Hustler", "Ellie", "Dominator GTX", "Impaler", "Deviant", "Tulip", "Clique", "Gauntlet Classic", "Gauntlet Hellfire", "Peyote Gasser", "Drift Yosemite", "Beater Dukes", "Gauntlet Classic Custom", "Manana Custom", "Dominator ASP", "Dominator GTT", "Bravado Buffalo STX'" };
            public static readonly string[] Sports_Classics = new[] { "'JB 007", "Monroe", "Stinger", "Z-Type", "Roosebelt", "Pigalle", "Coquette Classic", "Casco", "Stirling GT", "Mamba", "Tornado Custom", "Tornado Rat Rod", "Invernus Classic", "Turismo Classic", "Cheetah Classic", "Torero", "Retinue", "Rapid GT Classic", "Savestra", "Viseris", "GT500", "190z", "Fagaloa", "Cheburek", "Michelli GT", "Jester Classic", "Swinger", "Zion Classic", "Dynasty", "Nebula Turbo", "Retinue Mk II", "Fränken Stange", "Peyote Custom", "Ardent", "JB 700W'" };
            public static readonly string[] Sports = new[] { "'9F Cabrio", "Banshee", "Carbonizzare", "Comet", "Coquette", "Feltzer", "Fusilade", "Futo", "Rapid GT", "Sultan", "Khamelion", "Alpha", "Jester", "Massacro", "Furore GT", "Jester Race", "Massacro Race", "Blista", "Kuruma", "Kuruma armored", "Verlierer", "Bestia GTS", "Seve-70", "Omnis", "Tropos Rallye", "Lynx", "Drift Tampa", "Sprunk Buffalo", "Raptor", "Elegy RH8", "Elegy Retro Custom", "Comet Retro Custom", "Specter", "Specter Custom", "Ruston", "Raiden", "Pariah", "Comet Safari", "Sentinel Classic", "Streiter", "Revolter", "Neon", "Comet SR", "Hotring Sabre", "GB 200", "Flasch GT", "Schlagen GT", "Itali GTO", "8F Drafter", "Issi Sport", "Neo", "Locust", "Jugular", "Paragon R", "Schwartzer", "Imorgon", "Sugoi", "V-STR", "Komoda", "Sultan Classic", "Penumbra FF", "Coquette D10", "Itali RSX", "Calico GTF", "Jester RR", "ZR350", "Remus", "Vectre", "Cypher", "Comet S2", "RT3000", "Sultan RS Classic", "Futo GTX", "Euros", "Growler", "UNKNOWN", "Comet S2 Cabrio", "Paragon R(armored)'" };
            public static readonly string[] Super = new[] { "'Adder", "Bullet", "Cheetah", "Entity XF", "Infernus", "Vacca", "Voltic", "Turismo R", "Zentorno", "Osiris", "T20", "Banshee 900R", "Sultan RS", "Reaperm", "FMJ", "X80", "Pfister 811", "RE-7B", "Tyrus", "ETR1", "Penetrator", "Tempesta", "Itali GTB", "Itali GTB Custom", "Nero", "Nero Custom", "GP1", "Vagner", "XA-21", "Visione", "Cyclone", "SC1", "Autarch", "Tapian", "Entity", "Tezeract", "Tyrant", "Deveste Eight", "Thrax", "Zorrusso", "Krieger", "Emerus", "S80RR", "Furia", "Tigon", "Pegassi Ignus", "UNKNOWN", "UNKNOWN", "Zeno", "Dewbauchee Champion'" };
            public static readonly string[] Motorcycles = new[] { "'Akuma", "Bagger", "Bati 801", "Bati 801R", "Blazer", "Deamon", "Duble-T", "Nemisis", "PCJ 600", "Ruffian", "Sanchez", "Sanchez Label", "Vader", "Carbon RS", "Thrust", "Soveregin", "Innovation", "Hakuchou", "Enduro", "Lectro", "Vindicator", "BF400", "Gargoyle", "Cliffhanger", "Hakuchou Drag", "Defiler", "Chimera", "Zombie Chopper", "Avarus", "Nightblade", "Zombie Bobber", "Wolfsbane", "Manchez", "Rat Bike", "Faggio Mod", "Faggio Sport", "Daemon Custom", "Vortex", "Shotaro", "Esskey", "Diablus", "Diablus Custom", "FRC 1000", "FRC 1000 Custom", "Sanctus", "Rampant Rocket", "Stryder", "Manchez Scout'" };
            public static readonly string[] Off_Road = new[] { "'Injection", "Baller", "Blazer", "Duneloader", "Dune Buggy", "Patriot", "Sanchez", "Sandking XL", "Bodhi", "Dubsta", "Mesa", "Rusty Rebell", "Sandking SWB", "Tornado", "Sanches label", "Dubsta 6X6", "Bifta", "Kalahari", "Paradiese", "The Liberator", "Marshal", "Insurgent", "Guardian", "Brawler", "Trophy Truck", "Desert Raid", "BF400", "Dune", "Street Blazer", "Riata", "Kamacho", "Freecrawler", "Menacer", "Hellion", "Caracara 4x4", "Rancher XL", "Outlaw", "Everon", "Zhaba", "Vagrant", "Yosemite Rancher", "Rebel", "Winky", "Verus", "Manchez Scout", "Nightshark", "Patriot Mil-Spec'" };
            public static readonly string[] Industrial = new[] { "'Dozer", "Bohrer", "Kipplaster", "Frachtlader", "Dock Handler", "Mixer'" };
            public static readonly string[] Utility = new[] { "'Airtug", "Caddie", "Faggio", "Fieldmaster", "Lawn Mower", "Slamtruck'" };
            public static readonly string[] Vans = new[] { "'Boxville", "Buritto", "Camper", "Clown", "Journey", "Pony", "Minivan", "Rumpo", "Surfer", "Taco", "Youga", "Mule", "Gang Buritto", "Minivan Custom", "Rumpo Custom", "Youga Classic", "Speedo Custom", "Mule Custom", "Bravado Bison", "Brute Boxville", "Declasse Burrito", "Declasse Bugstars Burrito", "Brute Pony'" };
            public static readonly string[] Cycles = new[] { "'BMX", "Cruiser", "Scorcher", "Whippet Race Bike", "Endurex Race Bike", "Tri-Cycles Race Bike'" };
            public static readonly string[] Custom = new[] { "''" };
            public static readonly string[] Special = new[] { "'Blazer Aqua", "Ruiner 2000", "Rocket Voltic", "Deluxo", "Stromberg", "Thruster", "Opressor", "Vigelante", "Oppressor Mk II", "Scramjet", "RC Bandito'" };
            public static readonly string[] Weaponized = new[] { "'Technical Aqua", "Technical Custom", "Technical", "Dune FAV", "Armored Boxville", "Turretted Limo", "Barrage", "Half-track", "APC", "Caracara", "Insurgend Pick-Up Custom", "Insurgend Pick-Up", "Menacer'" };
            public static readonly string[] Arena_Contender = new[] { "'Apocalypse Bruiser", "Apocalypse Brutus", "Apocalypse Cerberus", "Apocalypse Deathbike", "Apocalypse Dominator", "Apocalypse Impaler", "Apocalypse Imperator", "Apocalypse Issi", "Apocalypse Sasquatch", "Apocalypse Scarab", "Apocalypse Slamvan", "Apocalypse ZR380'" };
            public static readonly string[] Open_Wheel = new[] { "'PR4", "R88", "BR8", "DR1'" };
            public static readonly string[] Go_Kart = new[] { "'Dinka Veto Classic", "Dinka Veto Modern'" };
            public static readonly string[] Tuner = new[] { "'Calico GTF", "Jester RR", "ZR350", "Remus", "Vectre", "Cypher", "Dominator ASP", "Comet S2", "Warrener HKR", "RT3000", "Tailgator S", "Sultan RS Classic", "Futo GTX", "Dominator GTT", "Previon", "Euros", "Growler'" };

            public static ObservableCollection<Outfit> Outfits
            {
                get { return JToken.Parse(Properties.Resources.Outfits).ToObject<ObservableCollection<Outfit>>(); }
            }

            public static ObservableCollection<Gang> GangTypes
            {
                get { return JToken.Parse(Properties.Resources.Gangtypes).ToObject<ObservableCollection<Gang>>(); }
            }

            public static ObservableCollection<JobTime> JobTime
            {
                get { return JToken.Parse(Properties.Resources.Jobtime).ToObject<ObservableCollection<JobTime>>(); }
            }

            public static ObservableCollection<Teamname> TeamNames
            {
                get { return JToken.Parse(Properties.Resources.Teamnames).ToObject<ObservableCollection<Teamname>>(); }
            }

            public static ObservableCollection<VehicleColors> VehicleColors
            {
                get
                {
                    var v = JToken.Parse(Properties.Resources.VehicleColors).ToObject<ObservableCollection<VehicleColors>>();
                    return new ObservableCollection<VehicleColors>(v.OrderBy(x => x.Name));
                }
            }

            public static List<GTA.Prop> ProplistNew
            {
                get { return JToken.Parse(Properties.Resources.ProplistNew).ToObject<List<GTA.Prop>>(); }
            }

            public static List<GTA.Weapon> CreateStartWeaponsList()
            {
                var items = new List<string> { "weapon_pistol", "weapon_combatpistol", "weapon_appistol", "weapon_pistol50", "weapon_heavypistol", "weapon_snspistol", "weapon_vintagepistol", "weapon_flaregun", "weapon_marksmanpistol", "weapon_pistol_mk2", "weapon_revolver_mk2", "weapon_snspistol_mk2", "weapon_revolver", "weapon_raypistol", "weapon_gadgetpistol", "weapon_microsmg", "weapon_smg", "weapon_assaultsmg", "weapon_combatpdw", "weapon_minismg", "weapon_machinepistol", "weapon_smg_mk2", "weapon_assaultrifle", "weapon_carbinerifle", "weapon_advancedrifle", "weapon_bullpuprifle", "weapon_marksmanrifle", "weapon_compactrifle", "weapon_gusenberg", "weapon_musket", "weapon_raycarbine", "weapon_specialcarbine", "weapon_assaultrifle_mk2", "weapon_carbinerifle_mk2", "weapon_combatmg_mk2", "weapon_bullpuprifle_mk2", "weapon_marksmanrifle_mk2", "weapon_specialcarbine_mk2", "weapon_militaryrifle", "weapon_mg", "weapon_combatmg", "weapon_pumpshotgun", "weapon_sawnoffshotgun", "weapon_assaultshotgun", "weapon_bullpupshotgun", "weapon_heavyshotgun", "weapon_dbshotgun", "weapon_autoshotgun", "weapon_doubleaction", "weapon_pumpshotgun_mk2", "weapon_combatshotgun", "weapon_sniperrifle", "weapon_heavysniper", "weapon_heavysniper_mk2", "weapon_grenadelauncher", "weapon_rpg", "weapon_minigun", "weapon_rayminigun", "weapon_firework", "weapon_hominglauncher", "weapon_railgun", "weapon_compactlauncher", "weapon_grenade", "weapon_smokegrenade", "weapon_stickybomb", "weapon_molotov", "weapon_proxmine", "weapon_pipebomb", "weapon_stungun_mp", "weapon_petrolcan", "weapon_knife", "weapon_nightstick", "weapon_hammer", "weapon_bat", "weapon_crowbar", "weapon_golfclub", "weapon_bottle", "weapon_dagger", "weapon_knuckle", "weapon_hatchet", "weapon_machete", "weapon_flashlight", "weapon_switchblade", "weapon_battleaxe", "weapon_poolcue", "weapon_wrench", "weapon_stone_hatchet", "weapon_unarmed", "weapon_fertilizercan", "weapon_emplaunchercase", "weapon_tacticalrifle", "weapon_precisionrifle", "weapon_heavyrifle" };
                List<GTA.Weapon> wplist = new List<GTA.Weapon>();
                foreach (var item in items)
                {
                    int var1;
                    if (item.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        var1 = int.Parse(item.Substring(2), NumberStyles.HexNumber);
                    else
                        var1 = int.Parse(item, CultureInfo.InvariantCulture);
                    uint var2 = unchecked((uint)var1);
                    wplist.Add(new GTA.Weapon(item, "Start Weapon", var1.ToString("X"), var1, var2));
                }
                return wplist;
            }

            public enum JobTypes : int
            {
                Mission = 0,
                Deathmatch = 1,
                Race = 2,
                Survival = 3
            }

            public enum RaceRoundSubtypes : int
            {
                Landrace = 0,
                Seerace = 2,
                Airrace = 4,
                Stuntrace = 6,
                Race = 8,
                Footrace = 10,
                Bikerace = 12,
                TargetAssault = 18,
                Transform = 20,
                SpecialVehicle = 21,
                ArenaWar = 20
            }
            public enum RaceP2PSubtypes : int
            {
                Landrace = 1,
                Seerace = 3,
                Airrace = 5,
                Stuntrace = 7,
                Race = 9,
                Footrace = 11,
                Bikerace = 13,
                TargetAssault = 19,
                Transform = 20,
                SpecialVehicle = 21,
                ArenaWar = 21
            }

            public enum MissionSubtypes : int
            {
                Heist = 1,
                Mission = 2,
                Adversary = 4,
                LastTeamStanding = 5,
                Capture = 6,
                Heist_Setup = 7
            }

            public enum DeathmatchSubtypes : int
            {
                NormalDeathmatch = 0,
                TeamDeathmatch = 1,
                VehicleDeathmatch = 2,
                NormalKOTH = 3,
                TeamKOTH = 4
            }

            public enum SpecialPropCategorys : int
            {
                Special = 16,
                LandingPlaces = 34,
                Targets = 35,
                Drugs = 37,
                Gunrunning = 38,
                Hidden = 15,
                Hidden2 = 39,
                Hidden3 = 49,
                Hidden4 = 50,
                Hidden5 = 51,
                Custom = 52,
                Templates = 58
            }
        }

        public class IPLEntry
        {
            public class IPLEntryBit
            {
                public int bit;
                public long offset;

                public IPLEntryBit(int bit, long offset)
                {
                    this.bit = bit;
                    this.offset = offset;
                }
            }

            public string name;
            public XenVector3 vector3;
            public List<IPLEntryBit> bits;

            public IPLEntry(string name, XenVector3 vector3, List<IPLEntryBit> bits)
            {
                this.name = name;
                this.vector3 = vector3;
                this.bits = bits;
            }
        }

        public class Location
        {
            public static IPLEntry YachtH = new IPLEntry("Dignity Heist Yacht", GTA.Location.Other.YACHT_H, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(13, GTA.Offsets.Editor.iplop)
            });
            public static IPLEntry YachtP = new IPLEntry("Dignity Party Yacht", GTA.Location.Other.YACHT_P, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(3, GTA.Offsets.Editor.iplop)
            });
            public static IPLEntry Carrier = new IPLEntry("Aircraft Carrier", GTA.Location.Other.CARRIER, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(2, GTA.Offsets.Editor.iplop)
            });
            public static IPLEntry Morgue = new IPLEntry("Morgue", GTA.Location.Other.MORGUE, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(1, GTA.Offsets.Editor.intop),
                new IPLEntry.IPLEntryBit(5, GTA.Offsets.Editor.intop)
            });
            public static IPLEntry Franklin = new IPLEntry("Franklins House", GTA.Location.Other.FRANKLIN, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(7, GTA.Offsets.Editor.intop)
            });
            public static IPLEntry Michael = new IPLEntry("Michaels House", GTA.Location.Other.MICHEAL, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(6, GTA.Offsets.Editor.intop)
            });
            public static IPLEntry RockClub = new IPLEntry("Rockclub", GTA.Location.Other.ROCKCLUB, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(9, GTA.Offsets.Editor.iplop)
            });
            public static IPLEntry StripClub = new IPLEntry("Stripclub", GTA.Location.Other.STRIPCLUB, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(10, GTA.Offsets.Editor.iplop)
            });
            public static IPLEntry ONeil = new IPLEntry("O-Neil Farm", GTA.Location.Other.ONEIL, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(4, GTA.Offsets.Editor.intop)
            });
            public static IPLEntry Liveinvader = new IPLEntry("Liveinvader Office", GTA.Location.Other.LIVE_INVADER_OFFICE, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(10, GTA.Offsets.Editor.intop)
            });
            public static IPLEntry Slaughter = new IPLEntry("Race Slaughterhouse", GTA.Location.Other.SLAUGHTER, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(3, GTA.Offsets.Editor.intop)
            });
            public static IPLEntry vrecycle = new IPLEntry("vrecycle", GTA.Location.Other.RECYCLE, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(2, GTA.Offsets.Editor.intop)
            });
            public static IPLEntry Foundry = new IPLEntry("Foundry", GTA.Location.Other.FOUNDRY, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(11, GTA.Offsets.Editor.intop)
            });
            public static IPLEntry ServerFarm = new IPLEntry("Server Farm", GTA.Location.Other.FARM, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(15, GTA.Offsets.Editor.intop)
            });
            public static IPLEntry IAA = new IPLEntry("IAA Base", GTA.Location.Other.IAA_OFFICE, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(15, GTA.Offsets.Editor.iplop),
                new IPLEntry.IPLEntryBit(12, GTA.Offsets.Editor.intop),
                new IPLEntry.IPLEntryBit(9, GTA.Offsets.Editor.intop)
            });
            public static IPLEntry FIB = new IPLEntry("FIB", GTA.Location.Other.FIB_Floor_Top, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(31, GTA.Offsets.Editor.intop),
                new IPLEntry.IPLEntryBit(32, GTA.Offsets.Editor.intop)
            });
            public static IPLEntry CayoPerico = new IPLEntry("Cayo Perico", GTA.Location.Other.CAYOPREICO, new List<IPLEntry.IPLEntryBit>
            {
                new IPLEntry.IPLEntryBit(20, GTA.Offsets.Editor.iplop)
            });


            public class CEO
            {
                public class ARCADIUS_BUSINESS_CENTRE
                {
                    private static XenVector3 loc_OFFICE = new XenVector3(-138.002F, -629.739F, 170.285F);
                    private static XenVector3 loc_GARAGE_1 = new XenVector3(-186.568F, -576.462F, 135.000F);
                    private static XenVector3 loc_GARAGE_2 = new XenVector3(-113.886F, -564.386F, 135.000F);
                    private static XenVector3 loc_GARAGE_3 = new XenVector3(-134.656F, -635.177F, 135.000F);
                    private static XenVector3 loc_MOD_SHOP = new XenVector3(-146.616F, -596.630F, 166.000F);
                    public static XenVector3 OFFICE
                    {
                        get { return loc_OFFICE; }
                    }
                    public static XenVector3 GARAGE_1
                    {
                        get { return loc_GARAGE_1; }
                    }
                    public static XenVector3 GARAGE_2
                    {
                        get { return loc_GARAGE_2; }
                    }
                    public static XenVector3 GARAGE_3
                    {
                        get { return loc_GARAGE_3; }
                    }
                    public static XenVector3 MOD_SHOP
                    {
                        get { return loc_MOD_SHOP; }
                    }
                }

                public class MAZE_BANK_TOWER
                {
                    private static XenVector3 loc_OFFICE = new XenVector3(-74.889F, -817.688F, 244.850F);
                    private static XenVector3 loc_GARAGE_1 = new XenVector3(-79.047F, -822.639F, 221.000F);
                    private static XenVector3 loc_GARAGE_2 = new XenVector3(-70.308F, -819.578F, 221.000F);
                    private static XenVector3 loc_GARAGE_3 = new XenVector3(-79.986F, -818.425F, 221.000F);
                    private static XenVector3 loc_MOD_SHOP = new XenVector3(-73.904F, -821.620F, 284.000F);

                    public static XenVector3 OFFICE
                    {
                        get { return loc_OFFICE; }
                    }
                    public static XenVector3 GARAGE_1
                    {
                        get { return loc_GARAGE_1; }
                    }
                    public static XenVector3 GARAGE_2
                    {
                        get { return loc_GARAGE_2; }
                    }
                    public static XenVector3 GARAGE_3
                    {
                        get { return loc_GARAGE_3; }
                    }
                    public static XenVector3 MOD_SHOP
                    {
                        get { return loc_MOD_SHOP; }
                    }
                }

                public class LOM_BANK
                {
                    private static XenVector3 loc_OFFICE = new XenVector3(-1572.187F, -570.831F, 109.987F);
                    private static XenVector3 loc_GARAGE_1 = new XenVector3(-1576.571F, -569.759F, 85.500F);
                    private static XenVector3 loc_GARAGE_2 = new XenVector3(-1571.254F, -566.586F, 85.500F);
                    private static XenVector3 loc_GARAGE_3 = new XenVector3(-1568.098F, -571.917F, 85.500F);
                    private static XenVector3 loc_MOD_SHOP = new XenVector3(-1578.022F, -576.425F, 104.200F);
                    public static XenVector3 OFFICE
                    {
                        get { return loc_OFFICE; }
                    }
                    public static XenVector3 GARAGE_1
                    {
                        get { return loc_GARAGE_1; }
                    }
                    public static XenVector3 GARAGE_2
                    {
                        get { return loc_GARAGE_2; }
                    }
                    public static XenVector3 GARAGE_3
                    {
                        get { return loc_GARAGE_3; }
                    }
                    public static XenVector3 MOD_SHOP
                    {
                        get { return loc_MOD_SHOP; }
                    }
                }
                public class MAZE_BANK_WEST
                {
                    private static XenVector3 loc_OFFICE = new XenVector3(-1383.954F, -476.711F, 73.507F);
                    private static XenVector3 loc_GARAGE_1 = new XenVector3(-1384.518F, -475.865F, 56.100F);
                    private static XenVector3 loc_GARAGE_2 = new XenVector3(-1384.538F, -475.882F, 48.100F);
                    private static XenVector3 loc_GARAGE_3 = new XenVector3(-1378.994F, -477.248F, 56.100F);
                    private static XenVector3 loc_MOD_SHOP = new XenVector3(-1391.245F, -473.963F, 77.200F);
                    public static XenVector3 OFFICE
                    {
                        get { return loc_OFFICE; }
                    }
                    public static XenVector3 GARAGE_1
                    {
                        get { return loc_GARAGE_1; }
                    }
                    public static XenVector3 GARAGE_2
                    {
                        get { return loc_GARAGE_2; }
                    }
                    public static XenVector3 GARAGE_3
                    {
                        get { return loc_GARAGE_3; }
                    }
                    public static XenVector3 MOD_SHOP
                    {
                        get { return loc_MOD_SHOP; }
                    }
                }
                public class WAREHOUSE
                {
                    private static XenVector3 loc_Small = new XenVector3(1094.988F, -3101.776F, -39.003F);
                    private static XenVector3 loc_Medium = new XenVector3(1056.486F, -3105.724F, -39.003F);
                    private static XenVector3 loc_Large = new XenVector3(1006.967F, -3102.079F, -39.003F);
                    private static XenVector3 loc_special = new XenVector3(994.5925F, -3002.594F, -39.646F);

                    public static XenVector3 Small
                    {
                        get { return loc_Small; }
                    }
                    public static XenVector3 Medium
                    {
                        get { return loc_Medium; }
                    }
                    public static XenVector3 Large
                    {
                        get { return loc_Large; }
                    }
                    public static XenVector3 Special
                    {
                        get { return loc_special; }
                    }
                }
            }
            public class Other
            {
                private static XenVector3 loc_FIB_burnt = new XenVector3(159.553f, -738.851f, 246.152f);
                private static XenVector3 loc_FIB_Floor_Top = new XenVector3(135.733f, -749.216f, 258.152f);
                private static XenVector3 loc_FIB_Floor_47 = new XenVector3(134.573F, -766.486F, 234.152F);
                private static XenVector3 loc_FIB_Floor_49 = new XenVector3(134.635F, -765.831F, 242.152F);
                private static XenVector3 loc_FIB_LOBBY = new XenVector3(110.400F, -744.200F, 45.749F);
                private static XenVector3 loc_CHARACTER_CREATION_ROOM = new XenVector3(-402.516F, -1002.847F, -99.258F);
                private static XenVector3 loc_MOTEL = new XenVector3(-152.260F, -1004.471F, -98.999F);
                private static XenVector3 loc_MANDRAZOS_RANCH = new XenVector3(-1401.210F, 1146.954F, 114.333F);
                private static XenVector3 loc_LIVE_INVADER_OFFICE = new XenVector3(-1044.193F, -236.953F, 37.964F);
                private static XenVector3 loc_LESTERS_HOUSE = new XenVector3(1274.934F, -1714.726F, 53.7715F);
                private static XenVector3 loc_IAA_OFFICE = new XenVector3(2047f, 2942f, -61.9f);
                private static XenVector3 loc_YACHT_P = new XenVector3(-2017.98F, -1040F, 2.45F);
                private static XenVector3 loc_YACHT_H = new XenVector3(-1363.43F, 6734.07F, 2.45F);
                private static XenVector3 loc_CARRIER = new XenVector3(3080.9F, -4705.28F, 15.26F);
                private static XenVector3 loc_MORGUE = new XenVector3(245.2593f, -1372.178f, 39.534f);
                private static XenVector3 loc_FRANKLIN = new XenVector3(7.025f, 537.307f, 175.028f);
                private static XenVector3 loc_MICHEAL = new XenVector3(-811.267f, 179.334f, 75.740f);
                private static XenVector3 loc_ROCKCLUB = new XenVector3(-556.508f, 286.318f, 81.176f);
                private static XenVector3 loc_STRIPCLUB = new XenVector3(119.6f, -1286.6f, 29.3f);
                private static XenVector3 loc_ONEIL = new XenVector3(-1601.424f, 2808.213f, 16.259f);
                private static XenVector3 loc_SLAUGHTER = new XenVector3(982.233f, -2160.382f, 28.476f);
                private static XenVector3 loc_FOUNDRY = new XenVector3(1087.195f, -1988.445f, 28.649f);
                private static XenVector3 loc_RECYCLE = new XenVector3(-598.637f, -1608.399f, 26.010f);
                private static XenVector3 loc_FARM = new XenVector3(2483f, -404.2f, -85.800f);
                private static XenVector3 loc_BUNKER = new XenVector3(938.307f, -3196.112f, -98f);
                private static XenVector3 loc_CAYOPREICO = new XenVector3(4971.15f, -5704.03f, 20.09f);

                public static XenVector3 FIB_burnt
                {
                    get { return loc_FIB_burnt; }
                }
                public static XenVector3 FIB_Floor_Top
                {
                    get { return loc_FIB_Floor_Top; }
                }
                public static XenVector3 FIB_Floor_47
                {
                    get { return loc_FIB_Floor_47; }
                }
                public static XenVector3 FIB_Floor_49
                {
                    get { return loc_FIB_Floor_49; }
                }
                public static XenVector3 FIB_LOBBY
                {
                    get { return loc_FIB_LOBBY; }
                }
                public static XenVector3 CHARACTER_CREATION_ROOM
                {
                    get { return loc_CHARACTER_CREATION_ROOM; }
                }
                public static XenVector3 MOTEL
                {
                    get { return loc_MOTEL; }
                }
                public static XenVector3 MANDRAZOS_RANCH
                {
                    get { return loc_MANDRAZOS_RANCH; }
                }
                public static XenVector3 LIVE_INVADER_OFFICE
                {
                    get { return loc_LIVE_INVADER_OFFICE; }
                }
                public static XenVector3 LESTERS_HOUSE
                {
                    get { return loc_LESTERS_HOUSE; }
                }
                public static XenVector3 IAA_OFFICE
                {
                    get { return loc_IAA_OFFICE; }
                }
                public static XenVector3 YACHT_P
                {
                    get { return loc_YACHT_P; }
                }
                public static XenVector3 YACHT_H
                {
                    get { return loc_YACHT_H; }
                }
                public static XenVector3 CARRIER
                {
                    get { return loc_CARRIER; }
                }
                public static XenVector3 MORGUE
                {
                    get { return loc_MORGUE; }
                }
                public static XenVector3 FRANKLIN
                {
                    get { return loc_FRANKLIN; }
                }
                public static XenVector3 MICHEAL
                {
                    get { return loc_MICHEAL; }
                }
                public static XenVector3 ROCKCLUB
                {
                    get { return loc_ROCKCLUB; }
                }
                public static XenVector3 STRIPCLUB
                {
                    get { return loc_STRIPCLUB; }
                }
                public static XenVector3 ONEIL
                {
                    get { return loc_ONEIL; }
                }
                public static XenVector3 SLAUGHTER
                {
                    get { return loc_SLAUGHTER; }
                }
                public static XenVector3 FOUNDRY
                {
                    get { return loc_FOUNDRY; }
                }
                public static XenVector3 RECYCLE
                {
                    get { return loc_RECYCLE; }
                }
                public static XenVector3 FARM
                {
                    get { return loc_FARM; }
                }
                public static XenVector3 BUNKER
                {
                    get { return loc_BUNKER; }
                }
                public static XenVector3 CAYOPREICO
                {
                    get { return loc_CAYOPREICO; }
                }
            }
            public class Garage
            {
                private static XenVector3 loc_10car = new XenVector3(228.605F, -992.053F, -99.999F);
                private static XenVector3 loc_6car = new XenVector3(199.971F, -999.667F, -99.999F);
                private static XenVector3 loc_4car = new XenVector3(199.971F, -1018.954F, -99.999F);
                private static XenVector3 loc_2car = new XenVector3(173.117F, -1003.279F, -99.999F);

                public static XenVector3 _10_Car_Garage
                {
                    get { return loc_10car; }
                }
                public static XenVector3 _6_Car_Garage
                {
                    get { return loc_6car; }
                }
                public static XenVector3 _4_Car_Garage
                {
                    get { return loc_4car; }
                }
                public static XenVector3 _2_Car_Garage
                {
                    get { return loc_2car; }
                }
            }
        }

        public class Defaults
        {
            private static List<int> prop = new List<int>(new int[] { -998079770, -987196513, 1095638230, 0, 0, 1116604317, 1116604318, 779917859, -1, -1, 0, 0, 0, 0, -1, 0, -1, -1, -1, -1, 0, 0, 524288, -1, -1, 0, 0, 1, 0, 5, -1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 2, 1092616192, 0, -1, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, -1, -1082130432, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 1, 0, -1, 0, 0, 0, 0, -1, -1, 0, 0, -1, 0, 0, 0, -1, 0, 0, 0, 1068289229, -1, -1, -1, -1, -1, -1, -1, 3, 0, 0, 0, -1, -1, -1, -1, -1, 0, -1, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, 0 });
            private static List<int> dprop = new List<int>(new int[] { -998013687, -987205004, 1096029030, 0, 0, 1116604317, 1116604317, -757971088, -1, 0, 0, 0, -1, -1, 0, 0, -1, -1, 0, 0, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 1073741824, 0, 0, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 255, 1067030938, 0, 0, 0, 0, 0, -1, 4, 0, -1, -1, -1, 0, 0, -1, -1, -1, 0, 0, -1, -1, -1, 0, 0, -1, -1, -1, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 2, 0, 0, 2, 0, 0, 0, 75, 1120403456, 1148846080, 0, 0, 0, -1, 0, 0, 0, 1065353216, 1077936128, 1082130432, 1077936128, -1, -1, -1, 0, 0, 1065353216, 1082130432, 0, 0, 1, 0, 0, 0, 2, 0, 3, 0, 4, 0, 1084227584, 0, 0, 30, 0, 0, 1, 0, 0, 70, 3000, 250, 1, 3, 0, 0, 0, -1, -1, -1, -1, -1, 0, -1, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, -1, -1, -1, 1, 0, 0, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1 });
            private static List<int> actor = new List<int>(new int[] { -998139478, -987180962, 1095528244, 0, 0, 1112014848, 1123024896, 1106247680, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 1, 1581098148, 453432689, 0, 4, 0, 0, 0, 0, 4, 2, 2, 2, 2, 4, 99999, 99999, 99999, 99999, 0, 0, 0, -1, 0, 100, 0, -1, -1, -1, 1065353216, 1065353216, 0, -1, -1, 0, 0, 0, -1, -1, 0, -1, -1, 1128857600, 0, -1082130432, -1082130432, -1, -1, -1, 1065353216, -1, 1065353216, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, 0, 0, -1, 0, 0, 0, 0, 0, 0, -1, -1, 453432689, -1, -1, -1, 0, -1, -1, -1, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, -1, -1, 1, 0, -1, 0, 0, -1, -1, -1, -1, 12, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, -1, -1, 0, -1, -1, -1, 0, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, -1, 1065353216, -1, -1, 11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, -1, -1, 0, 0, -1, -1, 1109393408, -1, 0, 0, -1, 4, -1, -1, -1, -1, 4, -1, -1, -1, -1, -1, 0, 0, 0, 250, 0, -1, 0, -1, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1, 0, 0, 20, 0, 0, 0, 0, 0, 0, 1065353216, -1, 0, -1, -1, -1, 0, -1, 0, -1, -1, 4, 0, 0, 0, 0, -1, 0, 4, 0, 0, 0, 0, -3, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, 0, 0, 0, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 4, -1, 4, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 10, 0, -1, 0, -1, 0, 0, 0, 0, -1, -1, 10, -1, -1, 0, 1, -101, -1, 0, 0, 0, 0, 0, 0, -1, -1, -1, 0, -1, -1082130432, -1, -1, 0, -1, -1, -1, -1, -1, -1, -1, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, -1, -1, -1, 0, 0, 1107296256, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 255, 1065353216, 0, 0, 0, 0, 0, -1, 4, 0, -1, -1, -1, 0, 0, -1, -1, -1, 0, 0, -1, -1, -1, 0, 0, -1, -1, -1, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 2, 0, 0, 2, 0, 0, 0, 75, 1120403456, 1148846080, 0, 0, 17, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, 3, 0, 0, 0, -1, 0, 0, -1, -1, 1101004800, 1110704128, 1114636288, 1117126656, 1084227584, 1101004800, 1114636288, 1117126656, 5, 1101004800, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, 1092616192, 1112014848, 0, 1, 0, -1, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, 1, 0, 0, -1, 0, 0, -1, -1, 0, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, 0, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, 0, 4, 0, 0, 0, 0, -1, -1, 0, -1, 0, -1, -1, 3, 0, 0, -2, -1, -1, 0, 0, -2, -1, -1, 0, 0, -2, -1, -1, -1, 0, 0, 0 });
            private static List<int> vehicle = new List<int>(new int[] { -998181938, -987162779, 1095552076, 0, 1120403456, 0, 0, -1, 0, 0, 0, 0, -1130810103, 4, 0, 0, 0, 0, 4, 99999, 99999, 99999, 99999, -1, -1, -1, -1, -1, -1, -1, -1082130432, 0, 100, 1148862464, 0, -1, -1, 0, 0, 0, -1, 1148862464, 1148862464, 1148862464, 1148862464, 3, 0, 0, 0, 1069547520, 0, 5, 0, 3, 0, 0, 0, -1, -1, 0, 0, -1, -1, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 536870912, 0, 0, 0, 0, 0, 0, -1, 10, 17, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, 1, 0, 0, 0, 0, 255, 1060320051, 0, 0, 0, 0, 0, -1, 4, 0, -1, -1, -1, 0, 0, -1, -1, -1, 0, 0, -1, -1, -1, 0, 0, -1, -1, -1, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 2, 0, 0, 2, 0, 0, 0, 75, 1120403456, 1148846080, 0, -1, -1, 0, -1, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, -1, -1, 0, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, -1, -1, 0, -1, -1, 0, -1, -1, -1, -1, 1, 4, 0, 0, 0, 0, 4, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, 0, -1, -1, 4, 0, 0, 0, 0, 1120403456, -1, -1, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1082130432, 10, 0, 0, -1, 0, 0, 0, 0, 0, 0, -1, -1, 3, 0, 0, 0, -1, 0, 4, 0, 0, 0, 0, 2, -1, -1, -1, -1, -1, 0, -1, 3, 3, 3000, -1, 0, 1065353216, 1065353216, 1065353216, 0, -1082130432, 0, -1, -1, 0, -1, -1, 10, -1, -1, 18, 0, -1, 0, 0, -1, -1, -1, -1, 1, 0, 0, 60, 1083179008, 0, 0, -1, 0, 0, -1, 0, -1088646911, 1052049852, -1105001265, 0, -1, -1, 0, 0, -1, -1, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, -1, -1, 0, -1, -1, 0, 0, -1, 0, -1, -1, -1, -1, 0, 0, 0, 1, 0, 2, 0, 0, -1, -1, -1, 0, -1, -1, 0, -1, -1, -1082130432, 0, -1, -1, -1, -1, -1, -1, 1065353216, 0, 0, 0, 0, 0, -1, -1, -1, -1, 1, 0, 0, -1, -1, -1, -1, 0, 0, 0, -1, 0, 0, -1, -1, -1, -1, -1, -1, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, -1, 0, -1, -1, 0, 1065353216 });
            private static List<int> weapon = new List<int>(new int[] { -998176860, -987210773, 1096107872, 0, 0, 0, 0, 0, 0, 1065353216, 4, 0, 0, 0, 0, -105925489, 6, 0, 0, 0, 0, 0, 0, -1, 0, -1, 0, -1, -1, 0, -1, -1, 0, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 3, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 21, 0, 76, 76, 76, 76, 76, 76, 76, 76, 76, 76, 76, 76, 76, 76, 76, 76, 76, 76, 76, 76, -1, 256, 0, 0, 0, 0, 255, 1065353216, 0, 0, 0, 0, 0, -1, 4, 0, -1, -1, -1, 0, 0, -1, -1, -1, 0, 0, -1, -1, -1, 0, 0, -1, -1, -1, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 2, 0, 0, 2, 0, 0, 0, 75, 1120403456, 1148846080, 0, -1, -1, -1, -1, 0, 1, 0, 0, -1 });
            private static List<int> obj = new List<int>(new int[] { -998137169, -987166431, 1096479222, 0, 0, -2147483648, -2147483648, 1112014848, 0, 0, -1, 0, 0, 0, 0, -1249748547, 4, 1, 1, 1, 1, 4, 0, 0, 0, 0, 13, 5, 0, 0, -1, 0, 1, 0, 1, 0, 0, 0, 255, 1065353216, 0, 0, 0, 0, 0, -1, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 2, 0, 0, 2, 0, 0, 0, 75, 1120403456, 1148846080, 0, -1, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 60, 0, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, -1, 0, -1, -1, -1, 0, 0, 0, 0, -1, 0, -1, -1, -1, -1, 0, 0, 0, 0, -1, -1, -1, -1, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 4, 0, 0, 0, 0, 4, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, 1112014848, -1, -1, -1, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 4, 0, 0, 0, 0, 1000, 0, 0, 0, -1, 0, 0, 0, 1065353216, 1077936128, 1082130432, 1077936128, -1, -1, -1, 0, 0, 1065353216, 1082130432, 0, 0, 1, 0, 0, 0, 2, 0, 3, 0, 4, 0, 1084227584, 0, 0, 30, 0, 0, 1, 0, 0, 70, 3000, 250, 1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 0, 1065353216, 1082130432, 0, 0, 1, 0, 0, 0, 2, 0, 3, 0, 4, 0, 1084227584, 0, 0, 30, 0, 0, 1, 0, 0, 70, 3000, 250, 1, -1, -1, -1, -1, -1, -1, -1, 0, -1, 1084227584, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1, 0, 0, 0, -1, -1, 3, 0, 0, 0, -1, 0, 0, 4, 0, 0, 0, 0, 0, 0, -1, 0, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, -1, -1, -1, -1, 0, 0, 0, -1, -1, 4, 0, 0, 0, 0, -1, 1, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 2, 0, 0, -1, -1, -1, -1, 0, 0, 0, -1, -1, -1, 0, 1, 0, 0, -1, -1, -1, -1, 1, 0, 0, -1, 0, 0, -1, -1, -1, -1, 18, -1, 40, -1, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, -1, 0, 0, -1, -1, -1, -1, 0, -1, -1, -1, 4, 0, 0, 0, 0, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, 0, 0, 0, 0, 1065353216, -1, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, -1, 0, -1, -1 });
            private static List<int> zone = new List<int>(new int[] { -997972930, -987217489, 1095565952, -998107001, -987300034, 1105101120, 1084227584, 1084227584, -1073741824, 0, 0, 4, -1, -1, -1, -1, 4, 99999, 99999, 99999, 99999, 256, 0, 0, 0, 0, 0, 0, 0, 2, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, -1, 0, 0, 0, 0, -1, 0, 0, -1, -1, 1, 120, 0, -1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1101004800, 1119092736, 1, 0, 100, 0, -1, -1, 1073741824, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, 0, -1, -1, 2 });
            private static List<int> location = new List<int>(new int[] { 0, 4, -998145122, -987172749, 1095553088, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 1073741824, 1073741824, 1073741824, 1073741824, 0, 0, 0, 0, 0, 0, 255, 1065353216, 0, 0, 0, 0, 0, -1, 4, 0, -1, -1, -1, 0, 0, -1, -1, -1, 0, 0, -1, -1, -1, 0, 0, -1, -1, -1, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 2, 0, 0, 2, 0, 0, 0, 75, 1120403456, 1148846080, 0, 0, 0, 0, 0, 0, 0, 1065353216, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, 0, 0, 4, 0, 0, 0, 0, -1, 4, 4, 4, 4, 4, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, 0, 1093140480, 0, 1065353216, 0, -1, 0, 0, -1, -1082130432, 0, 0, -1, 0, -1, -1, -1, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 4, 0, 0, 0, 0, -1, -1, -1, -1, 0, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, 0, 1065353216, 0, 0, 0, 0, 0, 0, 1132396544, -1, -1, 0, -1, 1, 0, 0, -1, 0, -1, -1, -1, -1, 0 });

            public static List<int> Prop { get => prop; }
            public static List<int> DProp { get => dprop; }
            public static List<int> Actor { get => actor; }
            public static List<int> Vehicle { get => vehicle; }
            public static List<int> Weapon { get => weapon; }
            public static List<int> Object { get => obj; }
            public static List<int> Zone { get => zone; }
            public static List<int> Location { get => location; }

            private static List<string> mpropsdefaultsrace = new List<string>(new string[] { "2E7C9A23,7C3C9BEA,A085E47C,5687D081,3EA83D4D,306BE0C4,B34BC429,45DFEF67,295B365E,18C49531,B6CACC47,F7752D66,BB188579,E56A5A1C,F724026D,9DDA7E0,65DCD413,6D51EECB,1F319BE4,50C22184,B87E6DE1,E15CA04A,2DA13CC7,DE469BCF,ACF07F3A,CE14C182,7E86A267,B1A00899,7B9FAAA0,A105F56C,8FE85331,8F12D266,A56CFF1A,723E18BD,C7EDC41F,46A74190,D783C7C7,FF374A2B,A1E58F89,BB9B09AC,40D23ECE,7DA7C387,B5DD1656,C689B79B,5972FB1,4ED9C235,B9C69815,CC003C88,1649D11A,673AB38B,532B112B,6558B586,1FCA2A6A,7C9F3E0C,CA0958DF,D84B7563,575CF388,25A7101D,BF741865,9882DA0,2929EE13,2E4DF59F,F676077C,F79A0AF4,292C078E,4C0D000B,FF3FCB5,A77C9A44,E40962FD,C53D2685,2CC1641D,A0133A76,2761E158,38BF0080,0B0332DF,72E2F577,7F02DF82",
                                                                                             "9EC80810,FBBE41FB,6BA514AC,E7ED1A59,9CD81E9F,3AA93E76",
                                                                                             "1B276762,2BE688E0,4653780,1D0FB6F4",
                                                                                             "E51F88D9,BEC44B8F,BE862050,A1ED363,F9B71F35,F872EFF,122C438C,3C5EBE3C,EE80FD5A",
                                                                                             "71325391,8BB2A762,4B444DBC,DE962965,9303E1A6,8FD48CB3,AF650A95,27C67B62,127150B8,8333C3C,517E8858,B94857EA,6EF2433F,D3D69366,6608DC0F,74F6B8BE,3C4ECDB",
                                                                                             "E40A0F8E,7D02B479,BC4649E5,342160A2,AEB63C4B,74E9F5BB,C7C649FF,5F5E76C9,C44ECA22,527818A3",
                                                                                             "B8465008,9910206D,51B12338,45709EF7,876CBCD2,B131133A,E44D5CEC,F2BD35BB,357CBA6D,2575D371,2EFBB698,5FF8D96F,47595E26,7C24C0B8,FFC4E948,91868CCD,8467C8D0,11E9FD7B,E5FECF61,5EEEF81F,CC23D613",
                                                                                             "74A3557,683475EE,F3AE2877,28B2940F,CFFB6B0,27BAEB1A,FC8394AC,5FB619D7",
                                                                                             "808B5D53,1D6F7B34,8C195886,B0833E3A,6C7C6A45,C44DD309,5A9789A0,69E9413E,7FB36CD2,E0A6FEFB,6F849B90,5748690C,FC96F411,A2023E64,3454C0D,DA1A2626",
                                                                                             "AEF01947,B01315B4,C0B9BCDA,4CEBD53C,17236AA7,E1497820,BF8918DE,EC0725C8,E0171018",
                                                                                             "5D011F16,BE9BB86F,E5257DB,D44163BA,B08A1C4C,A94763AB,A3C1D8A0,D0DD10A,1FC47677,9D8DF1FC,2F4B9579,7EE762D5",
                                                                                             "EFC4165A,4AF2CCB6,E0545565,44AEA99C,F88282E4",
                                                                                             "F9B8B7A0,376CB307,3FC3D20B,5580FD85,CA88E79F,97CB0224,42CE3C8A,206F9EB5,233DFD6A,C972C9D5,EFB6165B,1338DD60,9A382361,7125B4CE,AE8D2FA8,7C964BAF",
                                                                                             "B5AE3861,9A515D3F,9D47AFF9,3EBFCA03,E2AA93CA,E2048E2C,29E51C94,B83FBAA3,55908EC0,379FB809,79C0A750,2B3C88AE,174B35D6,FB9D3051,B3B836B0,DCA5159A,34E04379,B1367227,A37FD6BA,3F7E8EB1,29C26335,1C14C7DA",
                                                                                             "D2D24770,C4932AF2,AFDD8CBB,89F828EB,3C1B83BA,C2339364,E0264F5D,A6C23161,51400793,36AF4BB5,418F055A,C25DE433,E4DB3322,5AE0A333,7F8C93D0,9C7F3F08,FD389C44,D647866B,4A46E08E,70B0E25A,FDC8BCF7,ADA71CB1,5B385FFF,6277EC7E,BF1B2B36,8C9FC63C,FD3D779F,FF69E270,EAEED302,2EDD9003,4BBBAC6E,C2A63045,8DA442A2,4AF4BD44,7F7927AC,D7F1D89C,333332ED,893BA3A0,5779131A,53724BB0,0286F5F0,C00C3530,DB2C3E38,8DEB227B,0C0CF205,FE4E5688,F0873AFA,4A1BAE33,6881B256,A09BA29D,9BA61A22,81ED04F0,52DC99B6,E2FFAB8E,3DEFCE4D,C04ED1FD,6BEC23AD,CBE2A89C,BF77D87C,153F040D,23A8A0E0,4D306507,B2B841A3,97D0969C",
                                                                                             "AC7EC6AE,5981477A,0499AA60,9109DFBC,C08841A0,D1DDE44B,A7378EFF,B6602D50,C676CD85,D0FFE297,9A91F5AC,B48A29AC,7906B296,7C1AABEC,25054043,FE18F26F,FA10E36E,63F9CEA3,5DD2F986,9C1D6A5B,3CA58296,CB4D298C,39D75A93,B9319CBB,EF650831,E7F10145,1A76E4F0,CDE820A1,E2C51D69,45D81E73,A11AD1D7,A6E54878,249D1342,1CB21205,357CBA6D,F2BD35BB,2575D371,7C24C0B8,873F5C24,B56E3881,CC8478D8,C7116D1E,9C391ADE,276886A1,CA6ACA41,8A68CE9D,74503C56,2DE41B51,392D62AA,A7CF17C4,EDD0E658,918D2BAE,6AB3B57B,7A845307,01457D66,4014C50C,5CD2E1EA,3EA37E15,27BAEB1A,DCA9A809,74219CCB,417EC5DE,FCDB5E71,2DA13CC7,B17EAD7D,4F3FA437,A24CE57E,3794ACC9,A420E7B0,350F045E,E27289B5,3F0E8CE3,9D4F3537,6C38D8FA,534ACC12,B7DD5FD1,8538A366,158C9081,6B795EBC,B892B90D,E56BA797,876DEB81,D02ABBB2,22751A56,3E6FF91D,30699A93,C18B8FA3,B2614DD1,8CA6EAD9,7C041BDB",
                                                                                             "522CE28E,F7752D66,5DB600C9,848B8ABA,11FFB28C,40E01BC1,889E3E33,2DE41B51,8C4D43C4,6204EF38,49344B5A,FB120943,CAB94BE5,8E58F6AE,4EFF1313,671C5C38,3F2EC2B6,88D6CD61,7A9EBC92,4AF9D1D9,72BFC423,9FED6275,B22E8314,8C842A43,33D1F786,F5D9C598,B46EE154,9026C985,D593F420,33A12F6E,2652CBD9,CA6ACA41,070DBA50,276886A1,71C6E744,7CE1C05F,DF435615,E210FECF,66C0304D,25967365,1DA1220D,2F61C58E,4293EBF2,55411B30,677FBFAD,82900FDB,05993A0E,1A97B9A2,3C21B172,88476B1F,C46EFE43,E62441AD,861070B1,5778A9B1,389E11B5,DFF7CC31,34DD0DC9,DA96560A",
                                                                                             "5A5C0189,5ABD03F2,4259D185,ADFAAA90,748EB5EE,42EFD478,0A4723A5,3C314954,6D8066A2,BE577E09,A857DD30,218EC9A2,BF18A65C,285A9FE7,B1CA32D4,8810DF5E,6373EEF4,162A902D,7FDB66E9,E40743D1,392ED991,F475E4AE,2A783C24,0F8B1AD8,F9A05949,F821EFA6,087E7705,01C802F2,96CA139E,C8821067,99CBDED8,F4BD03EC,84B09BF2,DF7F5979,76138C75,EB8754FB,2EF52FC7,FE3FE726,3CB2CB42,58CFB07D,946BDDF0,785F56EE,2B862DA0,A8EBDD48,38B8C805,7F0A0985,BC835FE5,BDBD6259,04CA7DF2",
                                                                                             "27F13BA6,DA6EE30A,15BB173A,AFDE0DE9,4B2E021F,BC342695,8057D1CD,9792ED2B,5D7D8C19,A523884C,4E8CEE2C,79263052,56E458B3,20E736B0,A1736DDC,129E1A1E,82B3305C,4574FFCB,112321D8,7394BF68,26654C5C,8554E2E8,EF675E51,96300452,BB5B8306,E772C534,21E1D015,393268B6,4F762B3D,CBCA0DE3,06901972,1E7EB34F,38D1FDF5,292BC8A9,71A7F702,AD2857C2,CCEAAF2D,CF457E16,56833FF6",
                                                                                             "D7ADE0B8,69702115,8978ACE1,F590C75E,549E6D6B,289AB86D,4D02ED6A,5F2D2615,974101E9,6DD3C362,CC526C0B,70F9805A,EFA2443E,34A9E824,D04E18F0,6C6CD789,C618FE56,231CC3D2,204BE438,469E8AD5,74721347,A8B59980,EF3AA68D,229D5A2D,BA8C3D2D,474E1975,0189CB2B,6F5FF065,197F0168,AFFE67A4,6CA1E917,42561AA1,827092E6,F97807C6,F1FD3CFF,3EDD1DC3,3898738E,7A5F8915,F275DF37,D46BC199,62DD7CBE",
                                                                                             "C3FABAEE,FC0D2F15,E8D688A8,F2740D0C,6D3F1D16,554EED06,51E7E4F5,41EE5902,3BA14C68,2BEDCF90,BD511D34,DF7B1B6E,FDE81647,2BF6BBCF,989B0462,9BB260EF",
                                                                                             "C3F00C20,237861B3,C78E90F8,55F4B55D,6EF7C1E5,80B96568,B96AD9F2,C745F5A8",
                                                                                             "A11DB377,493F5F6F,56F7FAE0,5A990226,6A20A135,7FDECCB1,8D8D680E,B1EEB0DC,912C6F58,D7627BC3,2AFF21A7,64C7153A,8E8068AC,C69958DD,784E3C48,35C41C32,983947B2,1A78B62B,08BE12B6,E405C946,C15B03F1,D25BA5F2,BD95FC67,AC7E5A38,9AC036BC,763DEDB8,3DF8913E,F6A38295,0D56AFFB,A93DE7CB,770C0368,53ACBCA6,875C14A6,CAE6B5E9,666CEAA3,96A6B33B,F2B6EB52,E5054FEF,88AA173A,4095E943,B34EE0EC,E7904742,3D1B24C8,B4621358,9DE6F54D,68E40B48,9637D7B4,4366B213,39BC9EBF,67877A54,5661D809,FCA1249D,D7F1DB3B,7DAB26AB,389F3C9D,844BD3E9,4D64E628,282F9BBE,43B4D2BC,563777C1,342CBDB4,400F041D,55C92F91,5BBF8F23,CA96ECD0,7F4D563E,6A1B2BDA,4EDBF564,1D769048,264AD095,380C7418,5F5642AB,71ABE756,A72625EF,878D66CA,719B3AE6,2B682E7D,150B01C3,95A52A74,F0BDDF93,FE8B7B2E,F08418BA,641C7FE9,16B8E523,78E4ACB9,A45B47E0,0BF69604,17B9AD8A,5C9B374C,9AE5ED7F,3F32B616,55A82AC6,67F5CF61,A78D86DA,DF8F76DD,93EE5A7E,4E829BBB,B7126C6D,C8630F0E,D29D2382,5DE7B9A4,1C2A7F48,159E12C2,AC9D8713,4D720F4E,60B435D2,B0EF5647,8A0BBF9D,E2717BD5",
                                                                                             "1173612A,577BA63C,07214C86,7322DD8A,14E631A8,4E6FED66",
                                                                                             "F1337DE5,D7AB5EDB,808A8D98,B6B7AB45,D3E08775,F2099C08,15F3B86C,F8E728AB,5AEB062C,0BEBE82F,471CC681,55D2619B,DBBC6FBE,9EE25FAF,6393E913,62F370D4,23C8E97A,123C23E9,89696AD0,31BA3B53,32C96A52,A5196261,370C2079,5A666558,E5CFF3D4,7BD571D9,ADBA2D46,3C2C4BF0,CD5E5D8D,AF2D6CB2,2D80FC8A,3C04C081,B0AFD3E2,BE7FCC0F,11105A79,AB0391C1,060C1123,E794E24D,6BB55C74,F5DDA35D,06924B81,14A157DE,58A16E84,CC87476A,B856A8FD,92A53EF6,D27DC540,84DFAC51,C917B3E8",
                                                                                             "AD8C4C69,DF90308C,2A667310,D17A9461,3DA676E8,161AA7D5,00161AA7,8035DC01,2E5F6892,87A67D61,C779FFF6,7E1928FC,644BCB4C,628549ED,4BE9928B,31FF22CC,6FC3E5E0,89855E6F,8A85AF58,44952A09,3C7A3036,A325F1CF,C90AE498,80620730,EF1D64A5,27055474,7B5AB510,188FEF7C",
                                                                                             "85111220,8CD63A06,273CF578,78A811AA,073061F3,09FCBDE7,44AE05DE,CDD1A3EB,C43E9C6D,29DD35B8,D1A401A3,55778682,19A7DA0D,F3541068,049FFB40,B611EB07,6C51261A,B0B986C4,1B3287F3,C5755176,C114CE95,B4212798,B98797F6,BD9E98C9,8875B882,9A5C431D,421E102A,8AA61FD1,B15BA2D4,05F74459,7565C0E7,0739AE23,3C3649B8,C7BA5FD9,0DF3D3A7,F2515F80,91B91F7D,097099E6,9358DD56,97FD512A,5CC3CE68",
                                                                                             "0E2A823E,DBA04CB1,EEB1A195,34554245,D98C3759,49979437,3B40315A,F0E7834D,F6B52431,555D81D8,7CCA0A5E,F765DC2F,E7C48647,BBDC4F1A,CB9421B9,3BDD92D4,38543636,7661B973,D2822EE0,14917E66,B6687031,6A077455,793BC737,D048B4F5,F734D17A,18D5C385,776B8A11",
                                                                                             "431B529E,FDB8A439,42FFA24E,FD3A1616,7C17D532,3F3BCE2F,0E26794D,D0A67102,E07C9DFA,C3CCD74F,D187A7B4,A6DC4761,4FBEE77F,5B6835E7,7F71FA19,083DF517,CF989620,5F911F1D,05213D35,8BB5D808,88525AE3,21A0465B,0A21DE74,2A55D7E6,9CE203F6,5B7C3A32,B65E519A,3551D083,3D2730D1,4F120520,4F8DE627,AF7E2606,A2490B9C,14006F09,F6B83479,C41B8039,2E72D572,27391A04,BA3C2825,56217F90,A18079A5",
                                                                                             "651ED9A0,C2D7F2C7,3BBD0FF6,817F0D0E,4B6E534B,E143B9B2,202056B3,E0C0CEAD,85389651,26E644F4,1EEB7E4A,A26133FA,8E9E5130,1BFE545D,2AB8F1D2,C8A18A81,33E46105,3D3C9C28,2B1E3BFB",
                                                                                             "39D75A93,B9319CBB,EF650831,E7F10145,1A76E4F0,CDE820A1,E2C51D69,45D81E73,A11AD1D7,A6E54878,CC8478D8,C7116D1E,9C391ADE,0208A347,389B4DA3,9292F5F3,A0639194,B56E3881,873F5C24,0A312D63,F86E89DE,66E26804,350F045E,0EE23BD0,9868B672,C5387D57,45DA9996,2B45E599,F1C07967,DECE46FF,85049A3D,D2CB36ED",
                                                                                             "7EC70E5A,9D440B24,77A1B032,87BCE2A7,3EA497DD,4640E9FC,F02A7163,BC92BEA4,2B9FEA8C,87289B3B,88402278,70F2B3DE,757CAAD4,96880EDD,52B6E0D1,436DBEA2,85E7B733,9BC77D46,A5A35D61,D7753887,94B3BC1F,8B486959,C29CC3DB,17FF8EBA,408EBB61,A7E00235,9EAFE72B,444754AF,6B838CE6,E43C4EAD,A0E2D595,28D625D6,CB8B54A0,02015F32,B0569C0B,A2A3F651,6D410DBA,E85100D0,052B3F13,E525CA93,E6CE6B97,0D78A2A6,553B5E42,A516AADA,046F4313,4125B7B6,E8320096,C4463793,D16ED66F,ADBD3F8B",
                                                                                             "EE9D9F22,63C62308,65D5D133,F902FAC4,FC6C3ABF,A2049F84,DF869348,DEBCC638,D083E2EB,CF537A21,F1D637E7,6359CF70,CDFDE752,E067AB34,32A5CFB3,034470ED,0614768D,97E99A39,D721F01A,ADFC9DD0",
                                                                                             "14A07210,DB6BFF98,24FD56C0,5750BB66,0516376C,F3289391,1CD6B125,F57FE274,377D9206,669B7041,64024CC0,13142AE5,2A3FDC0C,38ABFBC0,B4DBE72A,90379A82,CC742196,B25DE10E,5A7DE93C,16F063CB,08ADC746,06FBDEE8,A7C9A19D",
                                                                                             "14791163,7FCCC104,7756B8BD,0AB4DF9F,9B25007D,D077AE75,03096DC5,CCF7E186,D3C5CBF8,7CB7364D,5A73A10E,74193851,A474F821,70EF8CD7,CA10FD05,B71CEEEF,FB1BC2EB,6EA72DF5,F2B19E6C,182743D3,753B7DFE,77C48318,BACB21DB,FB3C2945,A0860CA4,4933EA45,F6078ECB,3F966352,EF51DAC0,C04B3C88,DA63243F,E8D3ED7C,FD0415E0,0E53387E,EA87F0ED,889B632B,7C5B1ACC,F5411BB5,43CFF024,79C30A9D,23118A7E",
                                                                                             "8D49232F,F500A753,8B316741,DA1F051B,17507F7D,B25D5BBF,52AD0B0B,3787C289,5CC31D0E,592F0B89,1C816A7C",
                                                                                             "A6CDE189,D40F3C0B,06961D74,14A1B98B,9C4AE98B,9EC3D01D,AB836F80",
                                                                                             "0488BBD7,519A49D8,F6FE94AA,357BEFA7,667B7E0E,8DB64C83,A2FEECEF,90894804,7F2B2550,DCAD4137,C8DE4156,DF176DC8,61AD1E25,647023AB,57CF0A75,1930DA61,28CE1BAC,97C21DE3,F5A7FCEA,D81D9B1C,27B9AD13,4B7A2059,511100D2,F40C5722,522DAFE9,EB3ECDE7,8B2812EC",
                                                                                             "EAE5AB7C,9F5710E8,E74A21F5,73D22E7F,31829521,A935AD92,5BD391EF,2A88C695",
                                                                                             "77C2A9F1,5C6CE824,7C46A803,2B3FF42A,197D50A5,4FABBD01,61CE6146,92E29019,7FFBC1E2,5B386B9A,BDA3605C,392D7653,09E13C63,14445129,B1048AAB,C2C12E24,109A860F,4A03746E,ECE8DC86,B467C540,D03B3780,5DF38783,6BD1233E,C60F5AC3,10951EEC,22E5438C,60DABFAA,720CE20E,77210061,7B5026A9,2730DD95,9E30CD97,0834E3B2,D6C700D7,A8231F27,75A8446C,442F28A7,225BB56B,77C6E168,1458E1D7,6077632D,B1078EFD,7F65E2C7,5AE7BFF9,3DC4C741,427BADC1,813C7637,AA9E9321,CEA2D48E,935DE6A4,34713C62,09FB0974,28655EA8,5774CDAD,C853667B,CC6DEB6F,75AEE460,7376DFF0,1A59ADB7,9CA884DD,CB4D298C,7E6CAA3B,633FC452,0B3502AA,39655F0A,91000CC3,BFB6EA30,E56BA797,F5E87E89,C33719A0,F66F2146,5144D666,757C28D,BABEA183,64544401,5CCC56BA,A5F93E5E,624E4227,FE89EE4A,8CA6EAD9,A4EF297F,38088E4C,42EAF9E3,8A0D7B1A,8CEEDDAC,D2463525,09DA3EB9,6EFF2315,B01B091D,54F96425",
                                                                                             "56EA110C,4755F0BC,1EC69582,B86147A1,BDE0764E,FA98958C,3E675324,8C6CD70C,B4F4281A,4DAD98E3,35DC769D,50534AD4,F00BEC6F,6180E18B,F37D111D,C75C27F0,CF33387E,0239246D,141252D1,D8344CA2",
                                                                                             "FD5601DD,C33233FC,A53214DB,549C9517,976207EA,04D8CE01,2F685727,D7F7B22F,D1B5EE46,E86AF833,0827E835,F1D55C6B,44F95782,7842AD45,D28BE7B5,D028B5E7,B177A5DD,746FBD5D,3AC67077",
                                                                                             "42F59AF7,BC66A271,A55107A9,4D96D6DC,3153C97F,A004C97E,24CB3D69,65A13FE4,3A915583,639B5E46,BD10D1EF,5758A1F8,7372B010,A1DD84AE,C672CB17,A3888C97,17CF71B3,0E1050E4,6E15D391",
                                                                                             "84A4A2D1,85D60DE1,545DB0AD,071D1295,63FB0227,43DCC0BB,97BEF533,DC8F4A09,6B676D83,5CC32DE7,AB104A84,732F5ABF,7A03865F,DC799A5A,CDE2FD2D,B794D091,379C509E,2952B40B,69AA5BA5,A44AB305,E75B5849,310A5999,AA30C7AB,2426622A,76EB3D6A,07E9F2EE,B131A564,A4D194D1,1DA179C0,5AADE199,A24DAE3A,66DA3567,C2DD62CD,A9780F53,B6E4AA2C,B2BE6261,702F82C3,81EC263C,2AC8F7FF,F3E48A37,5CD56F3C,271E03CE,3A358389,5554B9C7,62C9D4B1,A68C4885,B8B4ECD6,884F0C03,489F4F55,28108D50,B582FDE3,1B614A86,9C421940,81BFE434,991613C8,8B9BF8CC,F274C684,E0BB2311,95B3ED4D,8743D06D,798934F8,D9A059C4,CB66BD51,C1615682,5169C3D3,24B78AEE,C5B42D86,18A6E847",
                                                                                             "8A635002,DF6B1251,81ACAD01,311535BA,B0F1C510,ED8EDDE3,CF22ED6E,1D1FEC83,B5380B02,77DD0621,EDD6E52B,706EAEB2,58C04660,B664C1F7,1810CDFF,AF08F987,0A34A69E,7DB0927C,0863DBC2,6A0CED95,F89041E9,F88C016E,3910FFE1",
                                                                                             "AF500B3C,135A0165,68ADBF92,711C2F18,7272BFAE,D4F96B44,BE49C6B2,89B3BE30,328A1773,8ECE318C,933DAD77,46D50E48,AB33F7A4,E29885C5,B276E13C,30E04D69,0D348D76,B4979EA4,F4D00A10,CFABD85D,AD6499D7,8050C367,FAF5970E,4EAB4556,A9C2CA39,3CD6346A,711E2239,F0577203,63BC2835,E5C3E6F1",
                                                                                             "5B6970E0,DC696305,3E370A61,434FEF5D,3817A594,F21EA8EF,2DDCBF5A,BD96243C,C6969C9A,1C63B932,26272411,AFAEDD0C,7B7D6DE1,A397C20A,803D96B0,3456126C,971772A1,4C2A22F5,B3F8198E,65DC08FD,F34136D4,66C45C6B,EE7F6CC3,03A51292",
                                                                                             "431869C5,AD14A264,425D5157,5DEA9F07",
                                                                                             "4DE126A4,B67CFFA2,F140D36C,C3F4FCDB,922F1950,F5AD127C",
                                                                                             "B33F4E61,56160347,D85F50FA,6BF39D6E,80DD7FA0,98D55E94,FDC63019,AE472B97,2AE88985,22AFCA27,9E49F1D8,27E85255,5A0E36A0,55CE2E24,40080298,714BE51F,6B7CD981,BC725D51,8B6B1656,0EFC5D6F,E5111098,D8FA917A,356FB097,380ABA31,EBE08E71,4C60C225",
                                                                                             "13C3630D,3B1FD771,6AA4331B,F1ED9D32,8E925153,B8F04FB2,8172DF65,EC8D70CD,3B93FF26,E73DF637,80053E1A,9B1A6442,CBC704A4,5D259626,AE9FBC19,DEE640A5,5541612F,C3ED980F,6384F654,CA4D116E,9F1AA33A,90018062,B6F51E6D,B7A70CBE,625852A7" });

            private static List<string> mpropsdefaultssurvival = new List<string>(new string[] { "2E7C9A23,7C3C9BEA,A085E47C,5687D081,3EA83D4D,306BE0C4,B34BC429,45DFEF67,295B365E,18C49531,B6CACC47,F7752D66,BB188579,E56A5A1C,F724026D,9B5FAA3B,65DCD413,6D51EECB,1F319BE4,50C22184,B87E6DE1,E15CA04A,2DA13CC7,DE469BCF,ACF07F3A,CE14C182,7E86A267,B1A00899,7B9FAAA0,A105F56C,8FE85331,8F12D266,A56CFF1A,723E18BD,C7EDC41F,46A74190,D783C7C7,FF374A2B,A1E58F89,BB9B09AC,40D23ECE,7DA7C387,B5DD1656,C689B79B,51AB960C,4ED9C235,B9C69815,CC003C88,1649D11A,673AB38B,532B112B,6558B586,1FCA2A6A,7C9F3E0C,CA0958DF,D84B7563,575CF388,25A7101D,BF741865,8169CBE1,2929EE13,2E4DF59F,F676077C,F79A0AF4,292C078E,4C0D000B,F893E9B0,A77C9A44,E40962FD,C53D2685,2CC1641D,A0133A76,2761E158,38BF0080,0B0332DF,72E2F577,7F02DF82",
                                                                                                 "9EC80810,FBBE41FB,6BA514AC,E7ED1A59,9CD81E9F,3AA93E76",
                                                                                                 "1B276762,2BE688E0,004702D4,1D0FB6F4",
                                                                                                 "E51F88D9,BEC44B8F,BE862050,FDBC7BB9,F9B71F35,0892B679,122C438C,3C5EBE3C,EE80FD5A",
                                                                                                 "044056CF,8BB2A762,4B444DBC,DE962965,9303E1A6,8FD48CB3,AF650A95,27C67B62,127150B8,E979601E,517E8858,B94857EA,6EF2433F,D3D69366,6608DC0F,74F6B8BE,A3402550",
                                                                                                 "E40A0F8E,7D02B479,BC4649E5,342160A2,AEB63C4B,74E9F5BB,C7C649FF,5F5E76C9,C44ECA22,527818A3",
                                                                                                 "B8465008,9910206D,51B12338,45709EF7,876CBCD2,B131133A,E44D5CEC,F2BD35BB,357CBA6D,2575D371,2EFBB698,5FF8D96F,47595E26,7C24C0B8,FFC4E948,91868CCD,8467C8D0,11E9FD7B,E5FECF61,5EEEF81F,CC23D613",
                                                                                                 "995BA588,683475EE,F3AE2877,28B2940F,2D756082,27BAEB1A,FC8394AC,5FB619D7",
                                                                                                 "808B5D53,1D6F7B34,8C195886,B0833E3A,6C7C6A45,C44DD309,5A9789A0,69E9413E,7FB36CD2,E0A6FEFB,6F849B90,5748690C,FC96F411,A2023E64,2AE8B60D,DA1A2626",
                                                                                                 "AEF01947,B01315B4,C0B9BCDA,4CEBD53C,17236AA7,E1497820,BF8918DE,EC0725C8,E0171018",
                                                                                                 "5D011F16,BE9BB86F,DAEEEAB6,D44163BA,B08A1C4C,A94763AB,A3C1D8A0,5251EAD9,1FC47677,9D8DF1FC,2F4B9579,7EE762D5",
                                                                                                 "EFC4165A,4AF2CCB6,E0545565,44AEA99C,F88282E4",
                                                                                                 "F9B8B7A0,376CB307,3FC3D20B,5580FD85,CA88E79F,97CB0224,42CE3C8A,206F9EB5,233DFD6A,C972C9D5,EFB6165B,1338DD60,9A382361,7125B4CE,AE8D2FA8,7C964BAF",
                                                                                                 "B5AE3861,9A515D3F,9D47AFF9,3EBFCA03,E2AA93CA,E2048E2C,29E51C94,B83FBAA3,55908EC0,379FB809,79C0A750,2B3C88AE,174B35D6,FB9D3051,B3B836B0,DCA5159A,34E04379,B1367227,A37FD6BA,3F7E8EB1,29C26335,1C14C7DA",
                                                                                                 "D2D24770,C4932AF2,AFDD8CBB,89F828EB,3C1B83BA,C2339364,E0264F5D,A6C23161,51400793,36AF4BB5,418F055A,C25DE433,E4DB3322,5AE0A333,7F8C93D0,9C7F3F08,FD389C44,D647866B,4A46E08E,70B0E25A,FDC8BCF7,ADA71CB1,5B385FFF,6277EC7E,BF1B2B36,8C9FC63C,FD3D779F,FF69E270,EAEED302,2EDD9003,4BBBAC6E,C2A63045,8DA442A2,4AF4BD44,7F7927AC,D7F1D89C,333332ED,893BA3A0,5779131A,53724BB0,0286F5F0,C00C3530,DB2C3E38,8DEB227B,0C0CF205,FE4E5688,F0873AFA,4A1BAE33,6881B256,A09BA29D,9BA61A22,81ED04F0,52DC99B6,E2FFAB8E,3DEFCE4D,C04ED1FD,6BEC23AD,CBE2A89C,BF77D87C,153F040D,23A8A0E0,4D306507,B2B841A3,97D0969C",
                                                                                                 "AC7EC6AE,5981477A,0499AA60,9109DFBC,C08841A0,D1DDE44B,A7378EFF,B6602D50,C676CD85,D0FFE297,9A91F5AC,B48A29AC,7906B296,7C1AABEC,25054043,FE18F26F,FA10E36E,63F9CEA3,5DD2F986,9C1D6A5B,3CA58296,CB4D298C,39D75A93,B9319CBB,EF650831,E7F10145,1A76E4F0,CDE820A1,E2C51D69,45D81E73,A11AD1D7,A6E54878,249D1342,1CB21205,357CBA6D,F2BD35BB,2575D371,7C24C0B8,873F5C24,B56E3881,CC8478D8,C7116D1E,9C391ADE,276886A1,CA6ACA41,8A68CE9D,74503C56,2DE41B51,392D62AA,A7CF17C4,EDD0E658,918D2BAE,6AB3B57B,7A845307,01457D66,4014C50C,5CD2E1EA,3EA37E15,27BAEB1A,DCA9A809,74219CCB,417EC5DE,FCDB5E71,2DA13CC7,B17EAD7D,4F3FA437,A24CE57E,3794ACC9,A420E7B0,350F045E,E27289B5,3F0E8CE3,9D4F3537,6C38D8FA,534ACC12,B7DD5FD1,8538A366,158C9081,6B795EBC,B892B90D,E56BA797,876DEB81,D02ABBB2,22751A56,3E6FF91D,30699A93,C18B8FA3,B2614DD1,8CA6EAD9,7C041BDB",
                                                                                                 "522CE28E,F7752D66,5DB600C9,848B8ABA,11FFB28C,40E01BC1,889E3E33,2DE41B51,8C4D43C4,6204EF38,49344B5A,FB120943,CAB94BE5,8E58F6AE,4EFF1313,671C5C38,3F2EC2B6,88D6CD61,7A9EBC92,4AF9D1D9,72BFC423,9FED6275,B22E8314,8C842A43,33D1F786,F5D9C598,B46EE154,9026C985,D593F420,33A12F6E,2652CBD9,CA6ACA41,070DBA50,276886A1,71C6E744,7CE1C05F,DF435615,E210FECF,66C0304D,25967365,1DA1220D,2F61C58E,4293EBF2,55411B30,677FBFAD,82900FDB,05993A0E,1A97B9A2,3C21B172,88476B1F,C46EFE43,E62441AD,861070B1,5778A9B1,389E11B5,DFF7CC31,34DD0DC9,DA96560A",
                                                                                                 "5A5C0189,5ABD03F2,4259D185,ADFAAA90,748EB5EE,42EFD478,0A4723A5,3C314954,6D8066A2,BE577E09,A857DD30,218EC9A2,BF18A65C,285A9FE7,B1CA32D4,8810DF5E,6373EEF4,162A902D,7FDB66E9,E40743D1,392ED991,F475E4AE,2A783C24,0F8B1AD8,F9A05949,F821EFA6,087E7705,01C802F2,96CA139E,C8821067,99CBDED8,F4BD03EC,84B09BF2,DF7F5979,76138C75,EB8754FB,2EF52FC7,FE3FE726,3CB2CB42,58CFB07D,946BDDF0,785F56EE,2B862DA0,A8EBDD48,38B8C805,7F0A0985,BC835FE5,BDBD6259,04CA7DF2",
                                                                                                 "27F13BA6,DA6EE30A,15BB173A,AFDE0DE9,4B2E021F,BC342695,8057D1CD,9792ED2B,5D7D8C19,A523884C,4E8CEE2C,79263052,56E458B3,20E736B0,A1736DDC,129E1A1E,82B3305C,4574FFCB,112321D8,7394BF68,26654C5C,8554E2E8,EF675E51,96300452,BB5B8306,E772C534,21E1D015,393268B6,4F762B3D,CBCA0DE3,06901972,1E7EB34F,38D1FDF5,292BC8A9,71A7F702,AD2857C2,CCEAAF2D,CF457E16,56833FF6",
                                                                                                 "D7ADE0B8,69702115,8978ACE1,F590C75E,549E6D6B,289AB86D,4D02ED6A,5F2D2615,974101E9,6DD3C362,CC526C0B,70F9805A,EFA2443E,34A9E824,D04E18F0,6C6CD789,C618FE56,231CC3D2,204BE438,469E8AD5,74721347,A8B59980,EF3AA68D,229D5A2D,BA8C3D2D,474E1975,0189CB2B,6F5FF065,197F0168,AFFE67A4,6CA1E917,42561AA1,827092E6,F97807C6,F1FD3CFF,3EDD1DC3,3898738E,7A5F8915,F275DF37,D46BC199,62DD7CBE",
                                                                                                 "C3FABAEE,FC0D2F15,E8D688A8,F2740D0C,6D3F1D16,554EED06,51E7E4F5,41EE5902,3BA14C68,2BEDCF90,BD511D34,DF7B1B6E,FDE81647,2BF6BBCF,989B0462,9BB260EF",
                                                                                                 "C3F00C20,237861B3,C78E90F8,55F4B55D,6EF7C1E5,80B96568,B96AD9F2,C745F5A8",
                                                                                                 "A11DB377,493F5F6F,56F7FAE0,5A990226,6A20A135,7FDECCB1,8D8D680E,B1EEB0DC,912C6F58,D7627BC3,2AFF21A7,64C7153A,8E8068AC,C69958DD,784E3C48,35C41C32,983947B2,1A78B62B,08BE12B6,E405C946,C15B03F1,D25BA5F2,BD95FC67,AC7E5A38,9AC036BC,763DEDB8,3DF8913E,F6A38295,0D56AFFB,A93DE7CB,770C0368,53ACBCA6,875C14A6,CAE6B5E9,666CEAA3,96A6B33B,F2B6EB52,E5054FEF,88AA173A,4095E943,B34EE0EC,E7904742,3D1B24C8,B4621358,9DE6F54D,68E40B48,9637D7B4,4366B213,39BC9EBF,67877A54,5661D809,FCA1249D,D7F1DB3B,7DAB26AB,389F3C9D,844BD3E9,4D64E628,282F9BBE,43B4D2BC,563777C1,342CBDB4,400F041D,55C92F91,5BBF8F23,CA96ECD0,7F4D563E,6A1B2BDA,4EDBF564,1D769048,264AD095,380C7418,5F5642AB,71ABE756,A72625EF,878D66CA,719B3AE6,2B682E7D,150B01C3,95A52A74,F0BDDF93,FE8B7B2E,F08418BA,641C7FE9,16B8E523,78E4ACB9,A45B47E0,0BF69604,17B9AD8A,5C9B374C,9AE5ED7F,3F32B616,55A82AC6,67F5CF61,A78D86DA,DF8F76DD,93EE5A7E,4E829BBB,B7126C6D,C8630F0E,D29D2382,5DE7B9A4,1C2A7F48,159E12C2,AC9D8713,4D720F4E,60B435D2,B0EF5647,8A0BBF9D,E2717BD5",
                                                                                                 "1173612A,577BA63C,07214C86,7322DD8A,14E631A8,4E6FED66",
                                                                                                 "F1337DE5,D7AB5EDB,808A8D98,B6B7AB45,D3E08775,F2099C08,15F3B86C,F8E728AB,5AEB062C,0BEBE82F,471CC681,55D2619B,DBBC6FBE,9EE25FAF,6393E913,62F370D4,23C8E97A,123C23E9,89696AD0,31BA3B53,32C96A52,A5196261,370C2079,5A666558,E5CFF3D4,7BD571D9,ADBA2D46,3C2C4BF0,CD5E5D8D,AF2D6CB2,2D80FC8A,3C04C081,B0AFD3E2,BE7FCC0F,11105A79,AB0391C1,060C1123,E794E24D,6BB55C74,F5DDA35D,06924B81,14A157DE,58A16E84,CC87476A,B856A8FD,92A53EF6,D27DC540,84DFAC51,C917B3E8",
                                                                                                 "AD8C4C69,DF90308C,2A667310,D17A9461,3DA676E8,161AA7D5,8035DC01,2E5F6892,87A67D61,C779FFF6,7E1928FC,644BCB4C,628549ED,4BE9928B,31FF22CC,6FC3E5E0,89855E6F,8A85AF58,44952A09,3C7A3036,A325F1CF,C90AE498,80620730,EF1D64A5,27055474,7B5AB510,188FEF7C",
                                                                                                 "85111220,8CD63A06,273CF578,78A811AA,073061F3,09FCBDE7,44AE05DE,CDD1A3EB,C43E9C6D,29DD35B8,D1A401A3,55778682,19A7DA0D,F3541068,049FFB40,00049FFB,B611EB07,6C51261A,B0B986C4,1B3287F3,C5755176,C114CE95,B4212798,B98797F6,BD9E98C9,8875B882,9A5C431D,421E102A,8AA61FD1,B15BA2D4,05F74459,7565C0E7,0739AE23,3C3649B8,C7BA5FD9,0DF3D3A7,F2515F80,91B91F7D,097099E6,9358DD56,97FD512A,5CC3CE68",
                                                                                                 "0E2A823E,DBA04CB1,EEB1A195,34554245,D98C3759,49979437,3B40315A,F0E7834D,F6B52431,555D81D8,7CCA0A5E,F765DC2F,E7C48647,BBDC4F1A,CB9421B9,3BDD92D4,38543636,7661B973,D2822EE0,14917E66,B6687031,6A077455,793BC737,D048B4F5,F734D17A,18D5C385,776B8A11",
                                                                                                 "431B529E,FDB8A439,42FFA24E,FD3A1616,7C17D532,3F3BCE2F,0E26794D,D0A67102,E07C9DFA,C3CCD74F,D187A7B4,A6DC4761,4FBEE77F,5B6835E7,7F71FA19,083DF517,CF989620,5F911F1D,05213D35,8BB5D808,88525AE3,21A0465B,0A21DE74,2A55D7E6,9CE203F6,5B7C3A32,B65E519A,3551D083,3D2730D1,4F120520,4F8DE627,AF7E2606,A2490B9C,14006F09,F6B83479,C41B8039,2E72D572,27391A04,BA3C2825,56217F90,A18079A5",
                                                                                                 "651ED9A0,C2D7F2C7,3BBD0FF6,817F0D0E,4B6E534B,E143B9B2,202056B3,E0C0CEAD,85389651,26E644F4,1EEB7E4A,A26133FA,8E9E5130,1BFE545D,2AB8F1D2,C8A18A81,33E46105,3D3C9C28,2B1E3BFB",
                                                                                                 "39D75A93,B9319CBB,EF650831,E7F10145,1A76E4F0,CDE820A1,E2C51D69,45D81E73,A11AD1D7,A6E54878,CC8478D8,C7116D1E,9C391ADE,0208A347,389B4DA3,9292F5F3,A0639194,B56E3881,873F5C24,0A312D63,F86E89DE,66E26804,350F045E,0EE23BD0,9868B672,C5387D57,45DA9996,2B45E599,F1C07967,DECE46FF,85049A3D,D2CB36ED",
                                                                                                 "7EC70E5A,9D440B24,77A1B032,87BCE2A7,3EA497DD,4640E9FC,F02A7163,BC92BEA4,2B9FEA8C,87289B3B,88402278,70F2B3DE,757CAAD4,96880EDD,52B6E0D1,436DBEA2,85E7B733,9BC77D46,A5A35D61,D7753887,94B3BC1F,8B486959,C29CC3DB,17FF8EBA,408EBB61,A7E00235,9EAFE72B,444754AF,6B838CE6,E43C4EAD,A0E2D595,28D625D6,CB8B54A0,02015F32,B0569C0B,A2A3F651,6D410DBA,E85100D0,052B3F13,E525CA93,E6CE6B97,0D78A2A6,553B5E42,A516AADA,046F4313,4125B7B6,E8320096,C4463793,D16ED66F,ADBD3F8B",
                                                                                                 "EE9D9F22,63C62308,65D5D133,F902FAC4,FC6C3ABF,A2049F84,DF869348,DEBCC638,D083E2EB,CF537A21,F1D637E7,6359CF70,CDFDE752,E067AB34,32A5CFB3,034470ED,0614768D,97E99A39,D721F01A,ADFC9DD0",
                                                                                                 "14A07210,DB6BFF98,24FD56C0,5750BB66,0516376C,F3289391,1CD6B125,F57FE274,377D9206,669B7041,64024CC0,13142AE5,2A3FDC0C,38ABFBC0,B4DBE72A,90379A82,CC742196,B25DE10E,5A7DE93C,16F063CB,08ADC746,06FBDEE8,A7C9A19D",
                                                                                                 "14791163,7FCCC104,7756B8BD,0AB4DF9F,9B25007D,D077AE75,03096DC5,CCF7E186,D3C5CBF8,7CB7364D,5A73A10E,74193851,A474F821,70EF8CD7,CA10FD05,B71CEEEF,FB1BC2EB,6EA72DF5,F2B19E6C,182743D3,753B7DFE,77C48318,BACB21DB,FB3C2945,A0860CA4,4933EA45,F6078ECB,3F966352,EF51DAC0,C04B3C88,DA63243F,E8D3ED7C,FD0415E0,0E53387E,EA87F0ED,889B632B,7C5B1ACC,F5411BB5,43CFF024,79C30A9D,23118A7E",
                                                                                                 "8D49232F,F500A753,8B316741,DA1F051B,17507F7D,B25D5BBF,52AD0B0B,3787C289,5CC31D0E,592F0B89,1C816A7C",
                                                                                                 "A6CDE189,D40F3C0B,06961D74,14A1B98B,9C4AE98B,9EC3D01D,AB836F80",
                                                                                                 "0488BBD7,519A49D8,F6FE94AA,357BEFA7,667B7E0E,8DB64C83,A2FEECEF,90894804,7F2B2550,DCAD4137,C8DE4156,DF176DC8,61AD1E25,647023AB,57CF0A75,1930DA61,28CE1BAC,97C21DE3,F5A7FCEA,D81D9B1C,27B9AD13,4B7A2059,511100D2,F40C5722,522DAFE9,EB3ECDE7,8B2812EC",
                                                                                                 "EAE5AB7C,9F5710E8,E74A21F5,73D22E7F,31829521,A935AD92,5BD391EF,2A88C695",
                                                                                                 "77C2A9F1,5C6CE824,7C46A803,2B3FF42A,197D50A5,4FABBD01,61CE6146,92E29019,7FFBC1E2,5B386B9A,BDA3605C,392D7653,09E13C63,14445129,B1048AAB,C2C12E24,109A860F,4A03746E,ECE8DC86,B467C540,D03B3780,5DF38783,6BD1233E,C60F5AC3,10951EEC,22E5438C,60DABFAA,720CE20E,77210061,7B5026A9,2730DD95,9E30CD97,0834E3B2,D6C700D7,A8231F27,75A8446C,442F28A7,225BB56B,77C6E168,1458E1D7,6077632D,B1078EFD,7F65E2C7,5AE7BFF9,3DC4C741,427BADC1,813C7637,AA9E9321,CEA2D48E,935DE6A4,34713C62,09FB0974,28655EA8,5774CDAD,C853667B,CC6DEB6F,75AEE460,7376DFF0,1A59ADB7,9CA884DD,CB4D298C,7E6CAA3B,633FC452,0B3502AA,39655F0A,91000CC3,BFB6EA30,E56BA797,F5E87E89,C33719A0,F66F2146,5144D666,757C28D,BABEA183,64544401,5CCC56BA,A5F93E5E,624E4227,FE89EE4A,8CA6EAD9,A4EF297F,38088E4C,42EAF9E3,8A0D7B1A,8CEEDDAC,D2463525,09DA3EB9,6EFF2315,B01B091D,54F96425",
                                                                                                 "56EA110C,4755F0BC,1EC69582,B86147A1,BDE0764E,FA98958C,3E675324,8C6CD70C,B4F4281A,4DAD98E3,35DC769D,50534AD4,F00BEC6F,6180E18B,F37D111D,C75C27F0,CF33387E,0239246D,141252D1,D8344CA2",
                                                                                                 "FD5601DD,C33233FC,A53214DB,549C9517,976207EA,04D8CE01,2F685727,D7F7B22F,D1B5EE46,E86AF833,0827E835,F1D55C6B,44F95782,7842AD45,D28BE7B5,D028B5E7,B177A5DD,746FBD5D,3AC67077",
                                                                                                 "42F59AF7,BC66A271,A55107A9,4D96D6DC,3153C97F,A004C97E,24CB3D69,65A13FE4,3A915583,639B5E46,BD10D1EF,5758A1F8,7372B010,A1DD84AE,C672CB17,A3888C97,17CF71B3,0E1050E4,6E15D391",
                                                                                                 "84A4A2D1,85D60DE1,545DB0AD,071D1295,63FB0227,43DCC0BB,97BEF533,DC8F4A09,6B676D83,5CC32DE7,AB104A84,732F5ABF,7A03865F,DC799A5A,CDE2FD2D,B794D091,379C509E,2952B40B,69AA5BA5,A44AB305,E75B5849,310A5999,AA30C7AB,2426622A,76EB3D6A,07E9F2EE,B131A564,A4D194D1,1DA179C0,5AADE199,A24DAE3A,66DA3567,C2DD62CD,A9780F53,B6E4AA2C,B2BE6261,702F82C3,81EC263C,2AC8F7FF,F3E48A37,5CD56F3C,271E03CE,3A358389,5554B9C7,62C9D4B1,A68C4885,B8B4ECD6,884F0C03,489F4F55,28108D50,B582FDE3,1B614A86,9C421940,81BFE434,991613C8,8B9BF8CC,F274C684,E0BB2311,95B3ED4D,8743D06D,798934F8,D9A059C4,CB66BD51,C1615682,5169C3D3,24B78AEE,C5B42D86,18A6E847",
                                                                                                 "8A635002,DF6B1251,81ACAD01,311535BA,B0F1C510,ED8EDDE3,CF22ED6E,1D1FEC83,B5380B02,77DD0621,EDD6E52B,706EAEB2,58C04660,B664C1F7,1810CDFF,AF08F987,0A34A69E,7DB0927C,0863DBC2,6A0CED95,F89041E9,F88C016E,3910FFE1",
                                                                                                 "AF500B3C,135A0165,68ADBF92,711C2F18,7272BFAE,D4F96B44,BE49C6B2,89B3BE30,328A1773,8ECE318C,933DAD77,46D50E48,AB33F7A4,E29885C5,B276E13C,30E04D69,0D348D76,B4979EA4,F4D00A10,CFABD85D,AD6499D7,8050C367,FAF5970E,4EAB4556,A9C2CA39,3CD6346A,711E2239,F0577203,63BC2835,E5C3E6F1",
                                                                                                 "5B6970E0,DC696305,3E370A61,434FEF5D,3817A594,F21EA8EF,2DDCBF5A,BD96243C,C6969C9A,1C63B932,26272411,AFAEDD0C,7B7D6DE1,A397C20A,803D96B0,3456126C,971772A1,4C2A22F5,B3F8198E,65DC08FD,F34136D4,66C45C6B,EE7F6CC3,03A51292",
                                                                                                 "431869C5,AD14A264,425D5157,5DEA9F07",
                                                                                                 "4DE126A4,B67CFFA2,F140D36C,C3F4FCDB,922F1950,F5AD127C",
                                                                                                 "B33F4E61,56160347,D85F50FA,6BF39D6E,80DD7FA0,98D55E94,FDC63019,AE472B97,2AE88985,22AFCA27,9E49F1D8,27E85255,5A0E36A0,55CE2E24,40080298,714BE51F,6B7CD981,BC725D51,8B6B1656,0EFC5D6F,E5111098,D8FA917A,356FB097,380ABA31,EBE08E71,4C60C225",
                                                                                                 "13C3630D,3B1FD771,6AA4331B,F1ED9D32,8E925153,B8F04FB2,8172DF65,EC8D70CD,3B93FF26,E73DF637,80053E1A,9B1A6442,CBC704A4,5D259626,AE9FBC19,DEE640A5,5541612F,C3ED980F,6384F654,CA4D116E,9F1AA33A,90018062,B6F51E6D,B7A70CBE,625852A7",
                                                                                                 "6E003455,A2719263,CDC174B0,08D4BE52,2024F4E8,2024F4E8,F6773201",
                                                                                                 "1B06D571,5EF9FEC4,22D8FE39,13532244,7846A318,EFE7E2DF,99AEEB3B,93E220BD,FDBC8A50,2C3731D9,24B17070,AB564B93,6E01022E,A2719263,A2719263,97EA20B8,3813FC08,184140A1",
                                                                                                 "4BFB42D1,8F707C18,2C014CA6,6773257D,098D79EF,A5B8CAA9",
                                                                                                 "A54AE7B7,D0AACEF7,CC8B3905,B86AEE5B,2E071B5A,68605A36,D3A39366,A717F898,2C804FE3,65A7D8E9,84D676D4,098D79EF,A5B8CAA9"});

            private static List<string> mpropsdefaultslts = new List<string>(new string[] { "2E7C9A23,7C3C9BEA,A085E47C,5687D081,3EA83D4D,306BE0C4,B34BC429,45DFEF67,295B365E,18C49531,B6CACC47,F7752D66,BB188579,E56A5A1C,F724026D,9DDA7E0,65DCD413,6D51EECB,1F319BE4,50C22184,B87E6DE1,E15CA04A,2DA13CC7,DE469BCF,ACF07F3A,CE14C182,7E86A267,B1A00899,7B9FAAA0,A105F56C,8FE85331,8F12D266,A56CFF1A,723E18BD,C7EDC41F,46A74190,D783C7C7,FF374A2B,A1E58F89,BB9B09AC,40D23ECE,7DA7C387,B5DD1656,C689B79B,5972FB1,4ED9C235,B9C69815,CC003C88,1649D11A,673AB38B,532B112B,6558B586,1FCA2A6A,7C9F3E0C,CA0958DF,D84B7563,575CF388,25A7101D,BF741865,9882DA0,2929EE13,2E4DF59F,F676077C,F79A0AF4,292C078E,4C0D000B,FF3FCB5,A77C9A44,E40962FD,C53D2685,2CC1641D,A0133A76,2761E158,38BF0080,0B0332DF,72E2F577,7F02DF82",
                                                                                            "9EC80810,FBBE41FB,6BA514AC,E7ED1A59,9CD81E9F,3AA93E76",
                                                                                            "1B276762,2BE688E0,4653780,1D0FB6F4",
                                                                                            "E51F88D9,BEC44B8F,BE862050,A1ED363,F9B71F35,F872EFF,122C438C,3C5EBE3C,EE80FD5A",
                                                                                            "71325391,8BB2A762,4B444DBC,DE962965,9303E1A6,8FD48CB3,AF650A95,27C67B62,127150B8,8333C3C,517E8858,B94857EA,6EF2433F,D3D69366,6608DC0F,74F6B8BE,3C4ECDB",
                                                                                            "E40A0F8E,7D02B479,BC4649E5,342160A2,AEB63C4B,74E9F5BB,C7C649FF,5F5E76C9,C44ECA22,527818A3",
                                                                                            "B8465008,9910206D,51B12338,45709EF7,876CBCD2,B131133A,E44D5CEC,F2BD35BB,357CBA6D,2575D371,2EFBB698,5FF8D96F,47595E26,7C24C0B8,FFC4E948,91868CCD,8467C8D0,11E9FD7B,E5FECF61,5EEEF81F,CC23D613",
                                                                                            "74A3557,683475EE,F3AE2877,28B2940F,CFFB6B0,27BAEB1A,FC8394AC,5FB619D7",
                                                                                            "808B5D53,1D6F7B34,8C195886,B0833E3A,6C7C6A45,C44DD309,5A9789A0,69E9413E,7FB36CD2,E0A6FEFB,6F849B90,5748690C,FC96F411,A2023E64,3454C0D,DA1A2626",
                                                                                            "AEF01947,B01315B4,C0B9BCDA,4CEBD53C,17236AA7,E1497820,BF8918DE,EC0725C8,E0171018",
                                                                                            "5D011F16,BE9BB86F,E5257DB,D44163BA,B08A1C4C,A94763AB,A3C1D8A0,D0DD10A,1FC47677,9D8DF1FC,2F4B9579,7EE762D5",
                                                                                            "EFC4165A,4AF2CCB6,E0545565,44AEA99C,F88282E4",
                                                                                            "F9B8B7A0,376CB307,3FC3D20B,5580FD85,CA88E79F,97CB0224,42CE3C8A,206F9EB5,233DFD6A,C972C9D5,EFB6165B,1338DD60,9A382361,7125B4CE,AE8D2FA8,7C964BAF",
                                                                                            "B5AE3861,9A515D3F,9D47AFF9,3EBFCA03,E2AA93CA,E2048E2C,29E51C94,B83FBAA3,55908EC0,379FB809,79C0A750,2B3C88AE,174B35D6,FB9D3051,B3B836B0,DCA5159A,34E04379,B1367227,A37FD6BA,3F7E8EB1,29C26335,1C14C7DA",
                                                                                            "D2D24770,C4932AF2,AFDD8CBB,89F828EB,3C1B83BA,C2339364,E0264F5D,A6C23161,51400793,36AF4BB5,418F055A,C25DE433,E4DB3322,5AE0A333,7F8C93D0,9C7F3F08,FD389C44,D647866B,4A46E08E,70B0E25A,FDC8BCF7,ADA71CB1,5B385FFF,6277EC7E,BF1B2B36,8C9FC63C,FD3D779F,FF69E270,EAEED302,2EDD9003,4BBBAC6E,C2A63045,8DA442A2,4AF4BD44,7F7927AC,D7F1D89C,333332ED,893BA3A0,5779131A,53724BB0,0286F5F0,C00C3530,DB2C3E38,8DEB227B,0C0CF205,FE4E5688,F0873AFA,4A1BAE33,6881B256,A09BA29D,9BA61A22,81ED04F0,52DC99B6,E2FFAB8E,3DEFCE4D,C04ED1FD,6BEC23AD,CBE2A89C,BF77D87C,153F040D,23A8A0E0,4D306507,B2B841A3,97D0969C",
                                                                                            "AC7EC6AE,5981477A,0499AA60,9109DFBC,C08841A0,D1DDE44B,A7378EFF,B6602D50,C676CD85,D0FFE297,9A91F5AC,B48A29AC,7906B296,7C1AABEC,25054043,FE18F26F,FA10E36E,63F9CEA3,5DD2F986,9C1D6A5B,3CA58296,CB4D298C,39D75A93,B9319CBB,EF650831,E7F10145,1A76E4F0,CDE820A1,E2C51D69,45D81E73,A11AD1D7,A6E54878,249D1342,1CB21205,357CBA6D,F2BD35BB,2575D371,7C24C0B8,873F5C24,B56E3881,CC8478D8,C7116D1E,9C391ADE,276886A1,CA6ACA41,8A68CE9D,74503C56,2DE41B51,392D62AA,A7CF17C4,EDD0E658,918D2BAE,6AB3B57B,7A845307,01457D66,4014C50C,5CD2E1EA,3EA37E15,27BAEB1A,DCA9A809,74219CCB,417EC5DE,FCDB5E71,2DA13CC7,B17EAD7D,4F3FA437,A24CE57E,3794ACC9,A420E7B0,350F045E,E27289B5,3F0E8CE3,9D4F3537,6C38D8FA,534ACC12,B7DD5FD1,8538A366,158C9081,6B795EBC,B892B90D,E56BA797,876DEB81,D02ABBB2,22751A56,3E6FF91D,30699A93,C18B8FA3,B2614DD1,8CA6EAD9,7C041BDB",
                                                                                            "522CE28E,F7752D66,5DB600C9,848B8ABA,11FFB28C,40E01BC1,889E3E33,2DE41B51,8C4D43C4,6204EF38,49344B5A,FB120943,CAB94BE5,8E58F6AE,4EFF1313,671C5C38,3F2EC2B6,88D6CD61,7A9EBC92,4AF9D1D9,72BFC423,9FED6275,B22E8314,8C842A43,33D1F786,F5D9C598,B46EE154,9026C985,D593F420,33A12F6E,2652CBD9,CA6ACA41,070DBA50,276886A1,71C6E744,7CE1C05F,DF435615,E210FECF,66C0304D,25967365,1DA1220D,2F61C58E,4293EBF2,55411B30,677FBFAD,82900FDB,05993A0E,1A97B9A2,3C21B172,88476B1F,C46EFE43,E62441AD,861070B1,5778A9B1,389E11B5,DFF7CC31,34DD0DC9,DA96560A",
                                                                                            "5A5C0189,5ABD03F2,4259D185,ADFAAA90,748EB5EE,42EFD478,0A4723A5,3C314954,6D8066A2,BE577E09,A857DD30,218EC9A2,BF18A65C,285A9FE7,B1CA32D4,8810DF5E,6373EEF4,162A902D,7FDB66E9,E40743D1,392ED991,F475E4AE,2A783C24,0F8B1AD8,F9A05949,F821EFA6,087E7705,01C802F2,96CA139E,C8821067,99CBDED8,F4BD03EC,84B09BF2,DF7F5979,76138C75,EB8754FB,2EF52FC7,FE3FE726,3CB2CB42,58CFB07D,946BDDF0,785F56EE,2B862DA0,A8EBDD48,38B8C805,7F0A0985,BC835FE5,BDBD6259,04CA7DF2",
                                                                                            "27F13BA6,DA6EE30A,15BB173A,AFDE0DE9,4B2E021F,BC342695,8057D1CD,9792ED2B,5D7D8C19,A523884C,4E8CEE2C,79263052,56E458B3,20E736B0,A1736DDC,129E1A1E,82B3305C,4574FFCB,112321D8,7394BF68,26654C5C,8554E2E8,EF675E51,96300452,BB5B8306,E772C534,21E1D015,393268B6,4F762B3D,CBCA0DE3,06901972,1E7EB34F,38D1FDF5,292BC8A9,71A7F702,AD2857C2,CCEAAF2D,CF457E16,56833FF6",
                                                                                            "D7ADE0B8,69702115,8978ACE1,F590C75E,549E6D6B,289AB86D,4D02ED6A,5F2D2615,974101E9,6DD3C362,CC526C0B,70F9805A,EFA2443E,34A9E824,D04E18F0,6C6CD789,C618FE56,231CC3D2,204BE438,469E8AD5,74721347,A8B59980,EF3AA68D,229D5A2D,BA8C3D2D,474E1975,0189CB2B,6F5FF065,197F0168,AFFE67A4,6CA1E917,42561AA1,827092E6,F97807C6,F1FD3CFF,3EDD1DC3,3898738E,7A5F8915,F275DF37,D46BC199,62DD7CBE",
                                                                                            "C3FABAEE,FC0D2F15,E8D688A8,F2740D0C,6D3F1D16,554EED06,51E7E4F5,41EE5902,3BA14C68,2BEDCF90,BD511D34,DF7B1B6E,FDE81647,2BF6BBCF,989B0462,9BB260EF",
                                                                                            "C3F00C20,237861B3,C78E90F8,55F4B55D,6EF7C1E5,80B96568,B96AD9F2,C745F5A8",
                                                                                            "A11DB377,493F5F6F,56F7FAE0,5A990226,6A20A135,7FDECCB1,8D8D680E,B1EEB0DC,912C6F58,D7627BC3,2AFF21A7,64C7153A,8E8068AC,C69958DD,784E3C48,35C41C32,983947B2,1A78B62B,08BE12B6,E405C946,C15B03F1,D25BA5F2,BD95FC67,AC7E5A38,9AC036BC,763DEDB8,3DF8913E,F6A38295,0D56AFFB,A93DE7CB,770C0368,53ACBCA6,875C14A6,CAE6B5E9,666CEAA3,96A6B33B,F2B6EB52,E5054FEF,88AA173A,4095E943,B34EE0EC,E7904742,3D1B24C8,B4621358,9DE6F54D,68E40B48,9637D7B4,4366B213,39BC9EBF,67877A54,5661D809,FCA1249D,D7F1DB3B,7DAB26AB,389F3C9D,844BD3E9,4D64E628,282F9BBE,43B4D2BC,563777C1,342CBDB4,400F041D,55C92F91,5BBF8F23,CA96ECD0,7F4D563E,6A1B2BDA,4EDBF564,1D769048,264AD095,380C7418,5F5642AB,71ABE756,A72625EF,878D66CA,719B3AE6,2B682E7D,150B01C3,95A52A74,F0BDDF93,FE8B7B2E,F08418BA,641C7FE9,16B8E523,78E4ACB9,A45B47E0,0BF69604,17B9AD8A,5C9B374C,9AE5ED7F,3F32B616,55A82AC6,67F5CF61,A78D86DA,DF8F76DD,93EE5A7E,4E829BBB,B7126C6D,C8630F0E,D29D2382,5DE7B9A4,1C2A7F48,159E12C2,AC9D8713,4D720F4E,60B435D2,B0EF5647,8A0BBF9D,E2717BD5",
                                                                                            "1173612A,577BA63C,07214C86,7322DD8A,14E631A8,4E6FED66",
                                                                                            "F1337DE5,D7AB5EDB,808A8D98,B6B7AB45,D3E08775,F2099C08,15F3B86C,F8E728AB,5AEB062C,0BEBE82F,471CC681,55D2619B,DBBC6FBE,9EE25FAF,6393E913,62F370D4,23C8E97A,123C23E9,89696AD0,31BA3B53,32C96A52,A5196261,370C2079,5A666558,E5CFF3D4,7BD571D9,ADBA2D46,3C2C4BF0,CD5E5D8D,AF2D6CB2,2D80FC8A,3C04C081,B0AFD3E2,BE7FCC0F,11105A79,AB0391C1,060C1123,E794E24D,6BB55C74,F5DDA35D,06924B81,14A157DE,58A16E84,CC87476A,B856A8FD,92A53EF6,D27DC540,84DFAC51,C917B3E8",
                                                                                            "AD8C4C69,DF90308C,2A667310,D17A9461,3DA676E8,161AA7D5,8035DC01,2E5F6892,87A67D61,C779FFF6,7E1928FC,644BCB4C,628549ED,4BE9928B,31FF22CC,6FC3E5E0,89855E6F,8A85AF58,44952A09,3C7A3036,A325F1CF,C90AE498,80620730,EF1D64A5,27055474,7B5AB510,188FEF7C",
                                                                                            "85111220,8CD63A06,273CF578,78A811AA,073061F3,09FCBDE7,44AE05DE,CDD1A3EB,C43E9C6D,29DD35B8,D1A401A3,55778682,19A7DA0D,F3541068,049FFB40,B611EB07,6C51261A,B0B986C4,1B3287F3,C5755176,C114CE95,B4212798,B98797F6,BD9E98C9,8875B882,9A5C431D,421E102A,8AA61FD1,B15BA2D4,05F74459,7565C0E7,0739AE23,3C3649B8,C7BA5FD9,0DF3D3A7,F2515F80,91B91F7D,097099E6,9358DD56,97FD512A,5CC3CE68",
                                                                                            "0E2A823E,DBA04CB1,EEB1A195,34554245,D98C3759,49979437,3B40315A,F0E7834D,F6B52431,555D81D8,7CCA0A5E,F765DC2F,E7C48647,BBDC4F1A,CB9421B9,3BDD92D4,38543636,7661B973,D2822EE0,14917E66,B6687031,6A077455,793BC737,D048B4F5,F734D17A,18D5C385,776B8A11",
                                                                                            "431B529E,FDB8A439,42FFA24E,FD3A1616,7C17D532,3F3BCE2F,0E26794D,D0A67102,E07C9DFA,C3CCD74F,D187A7B4,A6DC4761,4FBEE77F,5B6835E7,7F71FA19,083DF517,CF989620,5F911F1D,05213D35,8BB5D808,88525AE3,21A0465B,0A21DE74,2A55D7E6,9CE203F6,5B7C3A32,B65E519A,3551D083,3D2730D1,4F120520,4F8DE627,AF7E2606,A2490B9C,14006F09,F6B83479,C41B8039,2E72D572,27391A04,BA3C2825,56217F90,A18079A5",
                                                                                            "651ED9A0,C2D7F2C7,3BBD0FF6,817F0D0E,4B6E534B,E143B9B2,202056B3,E0C0CEAD,85389651,26E644F4,1EEB7E4A,A26133FA,8E9E5130,1BFE545D,2AB8F1D2,C8A18A81,33E46105,3D3C9C28,2B1E3BFB",
                                                                                            "39D75A93,B9319CBB,EF650831,E7F10145,1A76E4F0,CDE820A1,E2C51D69,45D81E73,A11AD1D7,A6E54878,CC8478D8,C7116D1E,9C391ADE,0208A347,389B4DA3,9292F5F3,A0639194,B56E3881,873F5C24,0A312D63,F86E89DE,66E26804,350F045E,0EE23BD0,9868B672,C5387D57,45DA9996,2B45E599,F1C07967,DECE46FF,85049A3D,D2CB36ED",
                                                                                            "7EC70E5A,9D440B24,77A1B032,87BCE2A7,3EA497DD,4640E9FC,F02A7163,BC92BEA4,2B9FEA8C,87289B3B,88402278,70F2B3DE,757CAAD4,96880EDD,52B6E0D1,436DBEA2,85E7B733,9BC77D46,A5A35D61,D7753887,94B3BC1F,8B486959,C29CC3DB,17FF8EBA,408EBB61,A7E00235,9EAFE72B,444754AF,6B838CE6,E43C4EAD,A0E2D595,28D625D6,CB8B54A0,02015F32,B0569C0B,A2A3F651,6D410DBA,E85100D0,052B3F13,E525CA93,E6CE6B97,0D78A2A6,553B5E42,A516AADA,046F4313,4125B7B6,E8320096,C4463793,D16ED66F,ADBD3F8B",
                                                                                            "EE9D9F22,63C62308,65D5D133,F902FAC4,FC6C3ABF,A2049F84,DF869348,DEBCC638,D083E2EB,CF537A21,F1D637E7,6359CF70,CDFDE752,E067AB34,32A5CFB3,034470ED,0614768D,97E99A39,D721F01A,ADFC9DD0",
                                                                                            "14A07210,DB6BFF98,24FD56C0,5750BB66,0516376C,F3289391,1CD6B125,F57FE274,377D9206,669B7041,64024CC0,13142AE5,2A3FDC0C,38ABFBC0,B4DBE72A,90379A82,CC742196,B25DE10E,5A7DE93C,16F063CB,08ADC746,06FBDEE8,A7C9A19D",
                                                                                            "14791163,7FCCC104,7756B8BD,0AB4DF9F,9B25007D,D077AE75,03096DC5,CCF7E186,D3C5CBF8,7CB7364D,5A73A10E,74193851,A474F821,70EF8CD7,CA10FD05,B71CEEEF,FB1BC2EB,6EA72DF5,F2B19E6C,182743D3,753B7DFE,77C48318,BACB21DB,FB3C2945,A0860CA4,4933EA45,F6078ECB,3F966352,EF51DAC0,C04B3C88,DA63243F,E8D3ED7C,FD0415E0,0E53387E,EA87F0ED,889B632B,7C5B1ACC,F5411BB5,43CFF024,79C30A9D,23118A7E",
                                                                                            "8D49232F,F500A753,8B316741,DA1F051B,17507F7D,B25D5BBF,52AD0B0B,3787C289,5CC31D0E,592F0B89,1C816A7C",
                                                                                            "A6CDE189,D40F3C0B,06961D74,14A1B98B,9C4AE98B,9EC3D01D,AB836F80",
                                                                                            "0488BBD7,519A49D8,F6FE94AA,357BEFA7,667B7E0E,8DB64C83,A2FEECEF,90894804,7F2B2550,DCAD4137,C8DE4156,DF176DC8,61AD1E25,647023AB,57CF0A75,1930DA61,28CE1BAC,97C21DE3,F5A7FCEA,D81D9B1C,27B9AD13,4B7A2059,511100D2,F40C5722,522DAFE9,EB3ECDE7,8B2812EC",
                                                                                            "EAE5AB7C,9F5710E8,E74A21F5,73D22E7F,31829521,A935AD92,5BD391EF,2A88C695",
                                                                                            "77C2A9F1,5C6CE824,7C46A803,2B3FF42A,197D50A5,4FABBD01,61CE6146,92E29019,7FFBC1E2,5B386B9A,BDA3605C,392D7653,09E13C63,14445129,B1048AAB,C2C12E24,109A860F,4A03746E,ECE8DC86,B467C540,D03B3780,5DF38783,6BD1233E,C60F5AC3,10951EEC,22E5438C,60DABFAA,720CE20E,77210061,7B5026A9,2730DD95,9E30CD97,0834E3B2,D6C700D7,A8231F27,75A8446C,442F28A7,225BB56B,77C6E168,1458E1D7,6077632D,B1078EFD,7F65E2C7,5AE7BFF9,3DC4C741,427BADC1,813C7637,AA9E9321,CEA2D48E,935DE6A4,34713C62,09FB0974,28655EA8,5774CDAD,C853667B,CC6DEB6F,75AEE460,7376DFF0,1A59ADB7,9CA884DD,CB4D298C,7E6CAA3B,633FC452,0B3502AA,39655F0A,91000CC3,BFB6EA30,E56BA797,F5E87E89,C33719A0,F66F2146,5144D666,757C28D,BABEA183,64544401,5CCC56BA,A5F93E5E,624E4227,FE89EE4A,8CA6EAD9,A4EF297F,38088E4C,42EAF9E3,8A0D7B1A,8CEEDDAC,D2463525,09DA3EB9,6EFF2315,B01B091D,54F96425",
                                                                                            "56EA110C,4755F0BC,1EC69582,B86147A1,BDE0764E,FA98958C,3E675324,8C6CD70C,B4F4281A,4DAD98E3,35DC769D,50534AD4,F00BEC6F,6180E18B,F37D111D,C75C27F0,CF33387E,0239246D,141252D1,D8344CA2",
                                                                                            "FD5601DD,C33233FC,A53214DB,549C9517,976207EA,04D8CE01,2F685727,D7F7B22F,D1B5EE46,E86AF833,0827E835,F1D55C6B,44F95782,7842AD45,D28BE7B5,D028B5E7,B177A5DD,746FBD5D,3AC67077",
                                                                                            "42F59AF7,BC66A271,A55107A9,4D96D6DC,3153C97F,A004C97E,24CB3D69,65A13FE4,3A915583,639B5E46,BD10D1EF,5758A1F8,7372B010,A1DD84AE,C672CB17,A3888C97,17CF71B3,0E1050E4,6E15D391",
                                                                                            "84A4A2D1,85D60DE1,545DB0AD,071D1295,63FB0227,43DCC0BB,97BEF533,DC8F4A09,6B676D83,5CC32DE7,AB104A84,732F5ABF,7A03865F,DC799A5A,CDE2FD2D,B794D091,379C509E,2952B40B,69AA5BA5,A44AB305,E75B5849,310A5999,AA30C7AB,2426622A,76EB3D6A,07E9F2EE,B131A564,A4D194D1,1DA179C0,5AADE199,A24DAE3A,66DA3567,C2DD62CD,A9780F53,B6E4AA2C,B2BE6261,702F82C3,81EC263C,2AC8F7FF,F3E48A37,5CD56F3C,271E03CE,3A358389,5554B9C7,62C9D4B1,A68C4885,B8B4ECD6,884F0C03,489F4F55,28108D50,B582FDE3,1B614A86,9C421940,81BFE434,991613C8,8B9BF8CC,F274C684,E0BB2311,95B3ED4D,8743D06D,798934F8,D9A059C4,CB66BD51,C1615682,5169C3D3,24B78AEE,C5B42D86,18A6E847",
                                                                                            "8A635002,DF6B1251,81ACAD01,311535BA,B0F1C510,ED8EDDE3,CF22ED6E,1D1FEC83,B5380B02,77DD0621,EDD6E52B,706EAEB2,58C04660,B664C1F7,1810CDFF,AF08F987,0A34A69E,7DB0927C,0863DBC2,6A0CED95,F89041E9,F88C016E,3910FFE1",
                                                                                            "AF500B3C,135A0165,68ADBF92,711C2F18,7272BFAE,D4F96B44,BE49C6B2,89B3BE30,328A1773,8ECE318C,933DAD77,46D50E48,AB33F7A4,E29885C5,B276E13C,30E04D69,0D348D76,B4979EA4,F4D00A10,CFABD85D,AD6499D7,8050C367,FAF5970E,4EAB4556,A9C2CA39,3CD6346A,711E2239,F0577203,63BC2835,E5C3E6F1",
                                                                                            "5B6970E0,DC696305,3E370A61,434FEF5D,3817A594,F21EA8EF,2DDCBF5A,BD96243C,C6969C9A,1C63B932,26272411,AFAEDD0C,7B7D6DE1,A397C20A,803D96B0,3456126C,971772A1,4C2A22F5,B3F8198E,65DC08FD,F34136D4,66C45C6B,EE7F6CC3,03A51292",
                                                                                            "431869C5,AD14A264,425D5157,5DEA9F07",
                                                                                            "4DE126A4,B67CFFA2,F140D36C,C3F4FCDB,922F1950,F5AD127C",
                                                                                            "B33F4E61,56160347,D85F50FA,6BF39D6E,80DD7FA0,98D55E94,FDC63019,AE472B97,2AE88985,22AFCA27,9E49F1D8,27E85255,5A0E36A0,55CE2E24,40080298,714BE51F,6B7CD981,BC725D51,8B6B1656,0EFC5D6F,E5111098,D8FA917A,356FB097,380ABA31,EBE08E71,4C60C225",
                                                                                            "13C3630D,3B1FD771,6AA4331B,F1ED9D32,8E925153,B8F04FB2,8172DF65,EC8D70CD,3B93FF26,E73DF637,80053E1A,9B1A6442,CBC704A4,5D259626,AE9FBC19,DEE640A5,5541612F,C3ED980F,6384F654,CA4D116E,9F1AA33A,90018062,B6F51E6D,B7A70CBE,625852A7",
                                                                                            "6E003455,A2719263,CDC174B0,08D4BE52,2024F4E8,2024F4E8,F6773201",
                                                                                            "1B06D571,5EF9FEC4,22D8FE39,13532244,7846A318,EFE7E2DF,99AEEB3B,93E220BD,FDBC8A50,2C3731D9,24B17070,AB564B93,6E01022E,A2719263,A2719263,97EA20B8,3813FC08,184140A1",
                                                                                            "4BFB42D1,8F707C18,2C014CA6,6773257D,098D79EF,A5B8CAA9",
                                                                                            "A54AE7B7,D0AACEF7,CC8B3905,B86AEE5B,2E071B5A,68605A36,D3A39366,A717F898,2C804FE3,65A7D8E9,84D676D4,098D79EF,A5B8CAA9"});

            private static List<string> mpropsdefaultscapture = new List<string>(new string[] { "2E7C9A23,7C3C9BEA,A085E47C,5687D081,3EA83D4D,306BE0C4,B34BC429,45DFEF67,295B365E,18C49531,B6CACC47,F7752D66,BB188579,E56A5A1C,F724026D,9DDA7E0,65DCD413,6D51EECB,1F319BE4,50C22184,B87E6DE1,E15CA04A,2DA13CC7,DE469BCF,ACF07F3A,CE14C182,7E86A267,B1A00899,7B9FAAA0,A105F56C,8FE85331,8F12D266,A56CFF1A,723E18BD,C7EDC41F,46A74190,D783C7C7,FF374A2B,A1E58F89,BB9B09AC,40D23ECE,7DA7C387,B5DD1656,C689B79B,5972FB1,4ED9C235,B9C69815,CC003C88,1649D11A,673AB38B,532B112B,6558B586,1FCA2A6A,7C9F3E0C,CA0958DF,D84B7563,575CF388,25A7101D,BF741865,9882DA0,2929EE13,2E4DF59F,F676077C,F79A0AF4,292C078E,4C0D000B,FF3FCB5,A77C9A44,E40962FD,C53D2685,2CC1641D,A0133A76,2761E158,38BF0080,0B0332DF,72E2F577,7F02DF82",
                                                                                                "9EC80810,FBBE41FB,6BA514AC,E7ED1A59,9CD81E9F,3AA93E76",
                                                                                                "1B276762,2BE688E0,4653780,1D0FB6F4",
                                                                                                "E51F88D9,BEC44B8F,BE862050,A1ED363,F9B71F35,F872EFF,122C438C,3C5EBE3C,EE80FD5A",
                                                                                                "71325391,8BB2A762,4B444DBC,DE962965,9303E1A6,8FD48CB3,AF650A95,27C67B62,127150B8,8333C3C,517E8858,B94857EA,6EF2433F,D3D69366,6608DC0F,74F6B8BE,3C4ECDB",
                                                                                                "E40A0F8E,7D02B479,BC4649E5,342160A2,AEB63C4B,74E9F5BB,C7C649FF,5F5E76C9,C44ECA22,527818A3",
                                                                                                "B8465008,9910206D,51B12338,45709EF7,876CBCD2,B131133A,E44D5CEC,F2BD35BB,357CBA6D,2575D371,2EFBB698,5FF8D96F,47595E26,7C24C0B8,FFC4E948,91868CCD,8467C8D0,11E9FD7B,E5FECF61,5EEEF81F,CC23D613",
                                                                                                "74A3557,683475EE,F3AE2877,28B2940F,CFFB6B0,27BAEB1A,FC8394AC,5FB619D7",
                                                                                                "808B5D53,1D6F7B34,8C195886,B0833E3A,6C7C6A45,C44DD309,5A9789A0,69E9413E,7FB36CD2,E0A6FEFB,6F849B90,5748690C,FC96F411,A2023E64,3454C0D,DA1A2626",
                                                                                                "AEF01947,B01315B4,C0B9BCDA,4CEBD53C,17236AA7,E1497820,BF8918DE,EC0725C8,E0171018",
                                                                                                "5D011F16,BE9BB86F,E5257DB,D44163BA,B08A1C4C,A94763AB,A3C1D8A0,D0DD10A,1FC47677,9D8DF1FC,2F4B9579,7EE762D5",
                                                                                                "EFC4165A,4AF2CCB6,E0545565,44AEA99C,F88282E4",
                                                                                                "F9B8B7A0,376CB307,3FC3D20B,5580FD85,CA88E79F,97CB0224,42CE3C8A,206F9EB5"});

            private static List<string> mpropsdefaultsdm = new List<string>(new string[] {"2E7C9A23,7C3C9BEA,A085E47C,5687D081,3EA83D4D,306BE0C4,B34BC429,45DFEF67,295B365E,18C49531,B6CACC47,F7752D66,BB188579,E56A5A1C,F724026D,9DDA7E0,65DCD413,6D51EECB,1F319BE4,50C22184,B87E6DE1,E15CA04A,2DA13CC7,DE469BCF,ACF07F3A,CE14C182,7E86A267,B1A00899,7B9FAAA0,A105F56C,8FE85331,8F12D266,A56CFF1A,723E18BD,C7EDC41F,46A74190,D783C7C7,FF374A2B,A1E58F89,BB9B09AC,40D23ECE,7DA7C387,B5DD1656,C689B79B,5972FB1,4ED9C235,B9C69815,CC003C88,1649D11A,673AB38B,532B112B,6558B586,1FCA2A6A,7C9F3E0C,CA0958DF,D84B7563,575CF388,25A7101D,BF741865,9882DA0,2929EE13,2E4DF59F,F676077C,F79A0AF4,292C078E,4C0D000B,FF3FCB5,A77C9A44,E40962FD,C53D2685,2CC1641D,A0133A76,2761E158,38BF0080,0B0332DF,72E2F577,7F02DF82",
                                                                                          "9EC80810,FBBE41FB,6BA514AC,E7ED1A59,9CD81E9F,3AA93E76",
                                                                                          "1B276762,2BE688E0,4653780,1D0FB6F4",
                                                                                          "E51F88D9,BEC44B8F,BE862050,A1ED363,F9B71F35,F872EFF,122C438C,3C5EBE3C,EE80FD5A",
                                                                                          "71325391,8BB2A762,4B444DBC,DE962965,9303E1A6,8FD48CB3,AF650A95,27C67B62,127150B8,8333C3C,517E8858,B94857EA,6EF2433F,D3D69366,6608DC0F,74F6B8BE,3C4ECDB",
                                                                                          "E40A0F8E,7D02B479,BC4649E5,342160A2,AEB63C4B,74E9F5BB,C7C649FF,5F5E76C9,C44ECA22,527818A3",
                                                                                          "B8465008,9910206D,51B12338,45709EF7,876CBCD2,B131133A,E44D5CEC,F2BD35BB,357CBA6D,2575D371,2EFBB698,5FF8D96F,47595E26,7C24C0B8,FFC4E948,91868CCD,8467C8D0,11E9FD7B,E5FECF61,5EEEF81F,CC23D613",
                                                                                          "74A3557,683475EE,F3AE2877,28B2940F,CFFB6B0,27BAEB1A,FC8394AC,5FB619D7",
                                                                                          "808B5D53,1D6F7B34,8C195886,B0833E3A,6C7C6A45,C44DD309,5A9789A0,69E9413E,7FB36CD2,E0A6FEFB,6F849B90,5748690C,FC96F411,A2023E64,3454C0D,DA1A2626",
                                                                                          "AEF01947,B01315B4,C0B9BCDA,4CEBD53C,17236AA7,E1497820,BF8918DE,EC0725C8,E0171018",
                                                                                          "5D011F16,BE9BB86F,E5257DB,D44163BA,B08A1C4C,A94763AB,A3C1D8A0,D0DD10A,1FC47677,9D8DF1FC,2F4B9579,7EE762D5",
                                                                                          "EFC4165A,4AF2CCB6,E0545565,44AEA99C,F88282E4",
                                                                                          "F9B8B7A0,376CB307,3FC3D20B,5580FD85,CA88E79F,97CB0224,42CE3C8A,206F9EB5,233DFD6A,C972C9D5,EFB6165B,1338DD60,9A382361,7125B4CE,AE8D2FA8,7C964BAF",
                                                                                          "B5AE3861,9A515D3F,9D47AFF9,3EBFCA03,E2AA93CA,E2048E2C,29E51C94,B83FBAA3,55908EC0,379FB809,79C0A750,2B3C88AE,174B35D6,FB9D3051,B3B836B0,DCA5159A,34E04379,B1367227,A37FD6BA,3F7E8EB1,29C26335,1C14C7DA",
                                                                                          "D2D24770,C4932AF2,AFDD8CBB,89F828EB,3C1B83BA,C2339364,E0264F5D,A6C23161,51400793,36AF4BB5,418F055A,C25DE433,E4DB3322,5AE0A333,7F8C93D0,9C7F3F08,FD389C44,D647866B,4A46E08E,70B0E25A,FDC8BCF7,ADA71CB1,5B385FFF,6277EC7E,BF1B2B36,8C9FC63C,FD3D779F,FF69E270,EAEED302,2EDD9003,4BBBAC6E,C2A63045,8DA442A2,4AF4BD44,7F7927AC,D7F1D89C,333332ED,893BA3A0,5779131A,53724BB0,0286F5F0,C00C3530,DB2C3E38,8DEB227B,0C0CF205,FE4E5688,F0873AFA,4A1BAE33,6881B256,A09BA29D,9BA61A22,81ED04F0,52DC99B6,E2FFAB8E,3DEFCE4D,C04ED1FD,6BEC23AD,CBE2A89C,BF77D87C,153F040D,23A8A0E0,4D306507,B2B841A3,97D0969C",
                                                                                          "AC7EC6AE,5981477A,0499AA60,9109DFBC,C08841A0,D1DDE44B,A7378EFF,B6602D50,C676CD85,D0FFE297,9A91F5AC,B48A29AC,7906B296,7C1AABEC,25054043,FE18F26F,FA10E36E,63F9CEA3,5DD2F986,9C1D6A5B,3CA58296,CB4D298C,39D75A93,B9319CBB,EF650831,E7F10145,1A76E4F0,CDE820A1,E2C51D69,45D81E73,A11AD1D7,A6E54878,249D1342,1CB21205,357CBA6D,F2BD35BB,2575D371,7C24C0B8,873F5C24,B56E3881,CC8478D8,C7116D1E,9C391ADE,276886A1,CA6ACA41,8A68CE9D,74503C56,2DE41B51,392D62AA,A7CF17C4,EDD0E658,918D2BAE,6AB3B57B,7A845307,01457D66,4014C50C,5CD2E1EA,3EA37E15,27BAEB1A,DCA9A809,74219CCB,417EC5DE,FCDB5E71,2DA13CC7,B17EAD7D,4F3FA437,A24CE57E,3794ACC9,A420E7B0,350F045E,E27289B5,3F0E8CE3,9D4F3537,6C38D8FA,534ACC12,B7DD5FD1,8538A366,158C9081,6B795EBC,B892B90D,E56BA797,876DEB81,D02ABBB2,22751A56,3E6FF91D,30699A93,C18B8FA3,B2614DD1,8CA6EAD9,7C041BDB",
                                                                                          "522CE28E,F7752D66,5DB600C9,848B8ABA,11FFB28C,40E01BC1,889E3E33,2DE41B51,8C4D43C4,6204EF38,49344B5A,FB120943,CAB94BE5,8E58F6AE,4EFF1313,671C5C38,3F2EC2B6,88D6CD61,7A9EBC92,4AF9D1D9,72BFC423,9FED6275,B22E8314,8C842A43,33D1F786,F5D9C598,B46EE154,9026C985,D593F420,33A12F6E,2652CBD9,CA6ACA41,070DBA50,276886A1,71C6E744,7CE1C05F,DF435615,E210FECF,66C0304D,25967365,1DA1220D,2F61C58E,4293EBF2,55411B30,677FBFAD,82900FDB,05993A0E,1A97B9A2,3C21B172,88476B1F,C46EFE43,E62441AD,861070B1,5778A9B1,389E11B5,DFF7CC31,34DD0DC9,DA96560A",
                                                                                          "5A5C0189,5ABD03F2,4259D185,ADFAAA90,748EB5EE,42EFD478,0A4723A5,3C314954,6D8066A2,BE577E09,A857DD30,218EC9A2,BF18A65C,285A9FE7,B1CA32D4,8810DF5E,6373EEF4,162A902D,7FDB66E9,E40743D1,392ED991,F475E4AE,2A783C24,0F8B1AD8,F9A05949,F821EFA6,087E7705,01C802F2,96CA139E,C8821067,99CBDED8,F4BD03EC,84B09BF2,DF7F5979,76138C75,EB8754FB,2EF52FC7,FE3FE726,3CB2CB42,58CFB07D,946BDDF0,785F56EE,2B862DA0,A8EBDD48,38B8C805,7F0A0985,BC835FE5,BDBD6259,04CA7DF2",
                                                                                          "27F13BA6,DA6EE30A,15BB173A,AFDE0DE9,4B2E021F,BC342695,8057D1CD,9792ED2B,5D7D8C19,A523884C,4E8CEE2C,79263052,56E458B3,20E736B0,A1736DDC,129E1A1E,82B3305C,4574FFCB,112321D8,7394BF68,26654C5C,8554E2E8,EF675E51,96300452,BB5B8306,E772C534,21E1D015,393268B6,4F762B3D,CBCA0DE3,06901972,1E7EB34F,38D1FDF5,292BC8A9,71A7F702,AD2857C2,CCEAAF2D,CF457E16,56833FF6",
                                                                                          "D7ADE0B8,69702115,8978ACE1,F590C75E,549E6D6B,289AB86D,4D02ED6A,5F2D2615,974101E9,6DD3C362,CC526C0B,70F9805A,EFA2443E,34A9E824,D04E18F0,6C6CD789,C618FE56,231CC3D2,204BE438,469E8AD5,74721347,A8B59980,EF3AA68D,229D5A2D,BA8C3D2D,474E1975,0189CB2B,6F5FF065,197F0168,AFFE67A4,6CA1E917,42561AA1,827092E6,F97807C6,F1FD3CFF,3EDD1DC3,3898738E,7A5F8915,F275DF37,D46BC199,62DD7CBE",
                                                                                          "C3FABAEE,FC0D2F15,E8D688A8,F2740D0C,6D3F1D16,554EED06,51E7E4F5,41EE5902,3BA14C68,2BEDCF90,BD511D34,DF7B1B6E,FDE81647,2BF6BBCF,989B0462,9BB260EF",
                                                                                          "C3F00C20,237861B3,C78E90F8,55F4B55D,6EF7C1E5,80B96568,B96AD9F2,C745F5A8",
                                                                                          "A11DB377,493F5F6F,56F7FAE0,5A990226,6A20A135,7FDECCB1,8D8D680E,B1EEB0DC,912C6F58,D7627BC3,2AFF21A7,64C7153A,8E8068AC,C69958DD,784E3C48,35C41C32,983947B2,1A78B62B,08BE12B6,E405C946,C15B03F1,D25BA5F2,BD95FC67,AC7E5A38,9AC036BC,763DEDB8,3DF8913E,F6A38295,0D56AFFB,A93DE7CB,770C0368,53ACBCA6,875C14A6,CAE6B5E9,666CEAA3,96A6B33B,F2B6EB52,E5054FEF,88AA173A,4095E943,B34EE0EC,E7904742,3D1B24C8,B4621358,9DE6F54D,68E40B48,9637D7B4,4366B213,39BC9EBF,67877A54,5661D809,FCA1249D,D7F1DB3B,7DAB26AB,389F3C9D,844BD3E9,4D64E628,282F9BBE,43B4D2BC,563777C1,342CBDB4,400F041D,55C92F91,5BBF8F23,CA96ECD0,7F4D563E,6A1B2BDA,4EDBF564,1D769048,264AD095,380C7418,5F5642AB,71ABE756,A72625EF,878D66CA,719B3AE6,2B682E7D,150B01C3,95A52A74,F0BDDF93,FE8B7B2E,F08418BA,641C7FE9,16B8E523,78E4ACB9,A45B47E0,0BF69604,17B9AD8A,5C9B374C,9AE5ED7F,3F32B616,55A82AC6,67F5CF61,A78D86DA,DF8F76DD,93EE5A7E,4E829BBB,B7126C6D,C8630F0E,D29D2382,5DE7B9A4,1C2A7F48,159E12C2,AC9D8713,4D720F4E,60B435D2,B0EF5647,8A0BBF9D,E2717BD5",
                                                                                          "1173612A,577BA63C,07214C86,7322DD8A,14E631A8,4E6FED66",
                                                                                          "F1337DE5,D7AB5EDB,808A8D98,B6B7AB45,D3E08775,F2099C08,15F3B86C,F8E728AB,5AEB062C,0BEBE82F,471CC681,55D2619B,DBBC6FBE,9EE25FAF,6393E913,62F370D4,23C8E97A,123C23E9,89696AD0,31BA3B53,32C96A52,A5196261,370C2079,5A666558,E5CFF3D4,7BD571D9,ADBA2D46,3C2C4BF0,CD5E5D8D,AF2D6CB2,2D80FC8A,3C04C081,B0AFD3E2,BE7FCC0F,11105A79,AB0391C1,060C1123,E794E24D,6BB55C74,F5DDA35D,06924B81,14A157DE,58A16E84,CC87476A,B856A8FD,92A53EF6,D27DC540,84DFAC51,C917B3E8",
                                                                                          "AD8C4C69,DF90308C,2A667310,D17A9461,3DA676E8,161AA7D5,8035DC01,2E5F6892,87A67D61,C779FFF6,7E1928FC,644BCB4C,628549ED,4BE9928B,31FF22CC,6FC3E5E0,89855E6F,8A85AF58,44952A09,3C7A3036,A325F1CF,C90AE498,80620730,EF1D64A5,27055474,7B5AB510,188FEF7C",
                                                                                          "85111220,8CD63A06,273CF578,78A811AA,073061F3,09FCBDE7,44AE05DE,CDD1A3EB,C43E9C6D,29DD35B8,D1A401A3,55778682,19A7DA0D,F3541068,049FFB40,B611EB07,6C51261A,B0B986C4,1B3287F3,C5755176,C114CE95,B4212798,B98797F6,BD9E98C9,8875B882,9A5C431D,421E102A,8AA61FD1,B15BA2D4,05F74459,7565C0E7,0739AE23,3C3649B8,C7BA5FD9,0DF3D3A7,F2515F80,91B91F7D,097099E6,9358DD56,97FD512A,5CC3CE68",
                                                                                          "0E2A823E,DBA04CB1,EEB1A195,34554245,D98C3759,49979437,3B40315A,F0E7834D,F6B52431,555D81D8,7CCA0A5E,F765DC2F,E7C48647,BBDC4F1A,CB9421B9,3BDD92D4,38543636,7661B973,D2822EE0,14917E66,B6687031,6A077455,793BC737,D048B4F5,F734D17A,18D5C385,776B8A11",
                                                                                          "431B529E,FDB8A439,42FFA24E,FD3A1616,7C17D532,3F3BCE2F,0E26794D,D0A67102,E07C9DFA,C3CCD74F,D187A7B4,A6DC4761,4FBEE77F,5B6835E7,7F71FA19,083DF517,CF989620,5F911F1D,05213D35,8BB5D808,88525AE3,21A0465B,0A21DE74,2A55D7E6,9CE203F6,5B7C3A32,B65E519A,3551D083,3D2730D1,4F120520,4F8DE627,AF7E2606,A2490B9C,14006F09,F6B83479,C41B8039,2E72D572,27391A04,BA3C2825,56217F90,A18079A5",
                                                                                          "651ED9A0,C2D7F2C7,3BBD0FF6,817F0D0E,4B6E534B,E143B9B2,202056B3,E0C0CEAD,85389651,26E644F4,1EEB7E4A,A26133FA,8E9E5130,1BFE545D,2AB8F1D2,C8A18A81,33E46105,3D3C9C28,2B1E3BFB",
                                                                                          "39D75A93,B9319CBB,EF650831,E7F10145,1A76E4F0,CDE820A1,E2C51D69,45D81E73,A11AD1D7,A6E54878,CC8478D8,C7116D1E,9C391ADE,0208A347,389B4DA3,9292F5F3,A0639194,B56E3881,873F5C24,0A312D63,F86E89DE,66E26804,350F045E,0EE23BD0,9868B672,C5387D57,45DA9996,2B45E599,F1C07967,DECE46FF,85049A3D,D2CB36ED",
                                                                                          "7EC70E5A,9D440B24,77A1B032,87BCE2A7,3EA497DD,4640E9FC,F02A7163,BC92BEA4,2B9FEA8C,87289B3B,88402278,70F2B3DE,757CAAD4,96880EDD,52B6E0D1,436DBEA2,85E7B733,9BC77D46,A5A35D61,D7753887,94B3BC1F,8B486959,C29CC3DB,17FF8EBA,408EBB61,A7E00235,9EAFE72B,444754AF,6B838CE6,E43C4EAD,A0E2D595,28D625D6,CB8B54A0,02015F32,B0569C0B,A2A3F651,6D410DBA,E85100D0,052B3F13,E525CA93,E6CE6B97,0D78A2A6,553B5E42,A516AADA,046F4313,4125B7B6,E8320096,C4463793,D16ED66F,ADBD3F8B",
                                                                                          "EE9D9F22,63C62308,65D5D133,F902FAC4,FC6C3ABF,A2049F84,DF869348,DEBCC638,D083E2EB,CF537A21,F1D637E7,6359CF70,CDFDE752,E067AB34,32A5CFB3,034470ED,0614768D,97E99A39,D721F01A,ADFC9DD0",
                                                                                          "14A07210,DB6BFF98,24FD56C0,5750BB66,0516376C,F3289391,1CD6B125,F57FE274,377D9206,669B7041,64024CC0,13142AE5,2A3FDC0C,38ABFBC0,B4DBE72A,90379A82,CC742196,B25DE10E,5A7DE93C,16F063CB,08ADC746,06FBDEE8,A7C9A19D",
                                                                                          "14791163,7FCCC104,7756B8BD,0AB4DF9F,9B25007D,D077AE75,03096DC5,CCF7E186,D3C5CBF8,7CB7364D,5A73A10E,74193851,A474F821,70EF8CD7,CA10FD05,B71CEEEF,FB1BC2EB,6EA72DF5,F2B19E6C,182743D3,753B7DFE,77C48318,BACB21DB,FB3C2945,A0860CA4,4933EA45,F6078ECB,3F966352,EF51DAC0,C04B3C88,DA63243F,E8D3ED7C,FD0415E0,0E53387E,EA87F0ED,889B632B,7C5B1ACC,F5411BB5,43CFF024,79C30A9D,23118A7E",
                                                                                          "8D49232F,F500A753,8B316741,DA1F051B,17507F7D,B25D5BBF,52AD0B0B,3787C289,5CC31D0E,592F0B89,1C816A7C",
                                                                                          "A6CDE189,D40F3C0B,06961D74,14A1B98B,9C4AE98B,9EC3D01D,AB836F80",
                                                                                          "0488BBD7,519A49D8,F6FE94AA,357BEFA7,667B7E0E,8DB64C83,A2FEECEF,90894804,7F2B2550,DCAD4137,C8DE4156,DF176DC8,61AD1E25,647023AB,57CF0A75,1930DA61,28CE1BAC,97C21DE3,F5A7FCEA,D81D9B1C,27B9AD13,4B7A2059,511100D2,F40C5722,522DAFE9,EB3ECDE7,8B2812EC",
                                                                                          "EAE5AB7C,9F5710E8,E74A21F5,73D22E7F,31829521,A935AD92,5BD391EF,2A88C695",
                                                                                          "77C2A9F1,5C6CE824,7C46A803,2B3FF42A,197D50A5,4FABBD01,61CE6146,92E29019,7FFBC1E2,5B386B9A,BDA3605C,392D7653,09E13C63,14445129,B1048AAB,C2C12E24,109A860F,4A03746E,ECE8DC86,B467C540,D03B3780,5DF38783,6BD1233E,C60F5AC3,10951EEC,22E5438C,60DABFAA,720CE20E,77210061,7B5026A9,2730DD95,9E30CD97,0834E3B2,D6C700D7,A8231F27,75A8446C,442F28A7,225BB56B,77C6E168,1458E1D7,6077632D,B1078EFD,7F65E2C7,5AE7BFF9,3DC4C741,427BADC1,813C7637,AA9E9321,CEA2D48E,935DE6A4,34713C62,09FB0974,28655EA8,5774CDAD,C853667B,CC6DEB6F,75AEE460,7376DFF0,1A59ADB7,9CA884DD,CB4D298C,7E6CAA3B,633FC452,0B3502AA,39655F0A,91000CC3,BFB6EA30,E56BA797,F5E87E89,C33719A0,F66F2146,5144D666,757C28D,BABEA183,64544401,5CCC56BA,A5F93E5E,624E4227,FE89EE4A,8CA6EAD9,A4EF297F,38088E4C,42EAF9E3,8A0D7B1A,8CEEDDAC,D2463525,09DA3EB9,6EFF2315,B01B091D,54F96425",
                                                                                          "56EA110C,4755F0BC,1EC69582,B86147A1,BDE0764E,FA98958C,3E675324,8C6CD70C,B4F4281A,4DAD98E3,35DC769D,50534AD4,F00BEC6F,6180E18B,F37D111D,C75C27F0,CF33387E,0239246D,141252D1,D8344CA2",
                                                                                          "FD5601DD,C33233FC,A53214DB,549C9517,976207EA,04D8CE01,2F685727,D7F7B22F,D1B5EE46,E86AF833,0827E835,F1D55C6B,44F95782,7842AD45,D28BE7B5,D028B5E7,B177A5DD,746FBD5D,3AC67077",
                                                                                          "42F59AF7,BC66A271,A55107A9,4D96D6DC,3153C97F,A004C97E,24CB3D69,65A13FE4,3A915583,639B5E46,BD10D1EF,5758A1F8,7372B010,A1DD84AE,C672CB17,A3888C97,17CF71B3,0E1050E4,6E15D391",
                                                                                          "84A4A2D1,85D60DE1,545DB0AD,071D1295,63FB0227,43DCC0BB,97BEF533,DC8F4A09,6B676D83,5CC32DE7,AB104A84,732F5ABF,7A03865F,DC799A5A,CDE2FD2D,B794D091,379C509E,2952B40B,69AA5BA5,A44AB305,E75B5849,310A5999,AA30C7AB,2426622A,76EB3D6A,07E9F2EE,B131A564,A4D194D1,1DA179C0,5AADE199,A24DAE3A,66DA3567,C2DD62CD,A9780F53,B6E4AA2C,B2BE6261,702F82C3,81EC263C,2AC8F7FF,F3E48A37,5CD56F3C,271E03CE,3A358389,5554B9C7,62C9D4B1,A68C4885,B8B4ECD6,884F0C03,489F4F55,28108D50,B582FDE3,1B614A86,9C421940,81BFE434,991613C8,8B9BF8CC,F274C684,E0BB2311,95B3ED4D,8743D06D,798934F8,D9A059C4,CB66BD51,C1615682,5169C3D3,24B78AEE,C5B42D86,18A6E847",
                                                                                          "8A635002,DF6B1251,81ACAD01,311535BA,B0F1C510,ED8EDDE3,CF22ED6E,1D1FEC83,B5380B02,77DD0621,EDD6E52B,706EAEB2,58C04660,B664C1F7,1810CDFF,AF08F987,0A34A69E,7DB0927C,0863DBC2,6A0CED95,F89041E9,F88C016E,3910FFE1",
                                                                                          "AF500B3C,135A0165,68ADBF92,711C2F18,7272BFAE,D4F96B44,BE49C6B2,89B3BE30,328A1773,8ECE318C,933DAD77,46D50E48,AB33F7A4,E29885C5,B276E13C,30E04D69,0D348D76,B4979EA4,F4D00A10,CFABD85D,AD6499D7,8050C367,FAF5970E,4EAB4556,A9C2CA39,3CD6346A,711E2239,F0577203,63BC2835,E5C3E6F1",
                                                                                          "5B6970E0,DC696305,3E370A61,434FEF5D,3817A594,F21EA8EF,2DDCBF5A,BD96243C,C6969C9A,1C63B932,26272411,AFAEDD0C,7B7D6DE1,A397C20A,803D96B0,3456126C,971772A1,4C2A22F5,B3F8198E,65DC08FD,F34136D4,66C45C6B,EE7F6CC3,03A51292",
                                                                                          "431869C5,AD14A264,425D5157,5DEA9F07",
                                                                                          "4DE126A4,B67CFFA2,F140D36C,C3F4FCDB,922F1950,F5AD127C",
                                                                                          "B33F4E61,56160347,D85F50FA,6BF39D6E,80DD7FA0,98D55E94,FDC63019,AE472B97,2AE88985,22AFCA27,9E49F1D8,27E85255,5A0E36A0,55CE2E24,40080298,714BE51F,6B7CD981,BC725D51,8B6B1656,0EFC5D6F,E5111098,D8FA917A,356FB097,380ABA31,EBE08E71,4C60C225",
                                                                                          "13C3630D,3B1FD771,6AA4331B,F1ED9D32,8E925153,B8F04FB2,8172DF65,EC8D70CD,3B93FF26,E73DF637,80053E1A,9B1A6442,CBC704A4,5D259626,AE9FBC19,DEE640A5,5541612F,C3ED980F,6384F654,CA4D116E,9F1AA33A,90018062,B6F51E6D,B7A70CBE,625852A7",
                                                                                          "6E003455,A2719263,CDC174B0,08D4BE52,2024F4E8,2024F4E8,F6773201",
                                                                                          "1B06D571,5EF9FEC4,22D8FE39,13532244,7846A318,EFE7E2DF,99AEEB3B,93E220BD,FDBC8A50,2C3731D9,24B17070,AB564B93,6E01022E,A2719263,A2719263,97EA20B8,3813FC08,184140A1",
                                                                                          "4BFB42D1,8F707C18,2C014CA6,6773257D,098D79EF,A5B8CAA9",
                                                                                          "A54AE7B7,D0AACEF7,CC8B3905,B86AEE5B,2E071B5A,68605A36,D3A39366,A717F898,2C804FE3,65A7D8E9,84D676D4,098D79EF,A5B8CAA9"});

            public static List<string> MPropsDefaultsRace { get => mpropsdefaultsrace; }
            public static List<string> MPropsDefaultsSurvival { get => mpropsdefaultssurvival; }
            public static List<string> MPropsDefaultsLTS { get => mpropsdefaultslts; }
            public static List<string> MPropsDefaultsCapture { get => mpropsdefaultscapture; }
            public static List<string> MPropsDefaultsDM { get => mpropsdefaultsdm; }
        }

        public class ScrPatchValue
        {
            public int id { get; set; }
            public string pattern { get; set; }
            public int offset { get; set; }
            public int bytes_to_read { get; set; }
        }

        public class ScrPatches
        {
            public string patch_name { get; set; }
            public string script_name { get; set; }
            public string pattern { get; set; }
            public int offset { get; set; }
            public string bytes_to_patch { get; set; }
            public byte[] original_bytes { get; set; }
            public List<ScrPatchValue> values { get; set; }
            public bool dev { get; set; }

            // Patches that only belong in the script while a feature needs them; the
            // runner leaves them out unless that feature is active ("templates":
            // PreciseTemplates.Active) and takes them back out afterwards.
            public string trigger { get; set; }

            // Defaults to true so every existing entry keeps working: the field
            // is absent from most of scrpatches.json, and an absent bool would
            // otherwise deserialize to false and silently disable the patch.
            // Set it to false to park a patch whose pattern broke on a game
            // update, without losing the pattern itself.
            public bool enabled { get; set; } = true;
        }

        public class Prop
        {
            public string Native;
            public string Name;
            public string Category;


            public int Integer
            {
                get => unchecked((int)joaat(Native));
            }
            public uint UInt
            {
                get => joaat(Native);
            }
            public string Hex
            {
                get => unchecked((int)joaat(Native)).ToString("X");
            }

            public Prop(string native, string name, string category)
            {
                this.Native = native;
                this.Name = name;
                this.Category = category;
            }

            private uint joaat(string Native)
            {
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

            public override string ToString()
            {
                return this.Name;
            }
        }

        public class Weapon
        {
            public string Native;
            public string Category;
            public string Hex;
            public int Int32;
            public uint UInt32;

            public Weapon(string native, string category, string hex, int int32, uint uInt32)
            {
                Native = native;
                Category = category;
                Hex = hex;
                Int32 = int32;
                UInt32 = uInt32;
            }

            public override string ToString()
            {
                return this.Native;
            }

        }

        public class Actor
        {
            public string Name;
            public uint UInt32;

            public Actor(string name, uint uInt32)
            {
                Name = name;
                UInt32 = uInt32;
            }

            public int Int32
            {
                get => unchecked((int)UInt32);
            }

            public string Hex
            {
                get => UInt32.ToString("X");
            }

            public override string ToString()
            {
                return this.Name;
            }

        }

        public class Vehicle
        {
            public string Native;
            public string Category;
            public string Hex;
            public int Int32;
            public uint Uint32;

            public Vehicle(string native, string hex, uint uint32, int int32, string category)
            {
                Native = native;
                Hex = hex;
                Uint32 = uint32;
                Int32 = int32;
                Category = category;
            }

            public override string ToString()
            {
                return this.Native;
            }
        }

        public class Arena
        {
            public string Theme;
            public string Variation;

            public Arena(string Theme, string Variation)
            {
                this.Theme = Theme;
                this.Variation = Variation;
            }
        }

        public class VehClass : UIElement
        {
            
            public string Name
            {
                get 
                { 
                    return (string)GetValue(NameProperty); 
                }
                set 
                { 
                    SetValue(NameProperty, value);
                }
            }
            public int Index;

            public VehClass(string name, int index)
            {
                Name = name;
                Index = index;
            }

            public static readonly DependencyProperty NameProperty =
                DependencyProperty.Register("Name", typeof(string), typeof(VehClass));

            public override string ToString()
            {
                return this.Name;
            }
        }

        public class MPEntry : INotifyPropertyChanged
        {
            private string name;
            private List<string> address;
            private string displayName;
            private int integerValue;
            private string hexValue;

            public string Name
            {
                get => name;
                set
                {
                    value = (value ?? string.Empty).Trim();
                    if (name == value)
                        return;

                    name = value;
                    UpdateDisplayName();
                    OnPropertyChanged();
                }
            }

            public List<string> Address
            {
                get => address;
                set
                {
                    address = value ?? new List<string>();
                    OnPropertyChanged();
                }
            }

            public string DisplayName => displayName;

            public int IntegerValue
            {
                get => integerValue;
                set
                {
                    if (integerValue == value)
                        return;

                    integerValue = value;
                    HexValue = integerValue.ToString("X8");
                    UpdateDisplayName();
                    OnPropertyChanged();
                }
            }

            public string HexValue
            {
                get => hexValue;
                private set
                {
                    value = (value ?? string.Empty).ToUpperInvariant();
                    if (hexValue == value)
                        return;

                    hexValue = value;
                    OnPropertyChanged();
                }
            }

            public MPEntry(string name, List<string> address)
                : this(name, address, 0)
            {
            }

            public MPEntry(string name, List<string> address, int integerValue)
                : this(name, address, integerValue, integerValue.ToString("X8"))
            {
            }

            public MPEntry(string name, List<string> address, int integerValue, string hexValue)
            {
                this.address = address ?? new List<string>();
                this.integerValue = integerValue;
                this.hexValue = string.IsNullOrEmpty(hexValue) ? integerValue.ToString("X8") : hexValue.ToUpperInvariant();
                this.name = (name ?? string.Empty).Trim();

                UpdateDisplayName();
            }

            private void UpdateDisplayName()
            {
                var label = IsUnknownName() ? "unknown native" : name;
                var composed = $"{label} \u00B7 {integerValue} \u00B7 {HexValue}";
                if (!string.Equals(displayName, composed, StringComparison.Ordinal))
                {
                    displayName = composed;
                    OnPropertyChanged(nameof(DisplayName));
                }
            }

            private bool IsUnknownName()
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return true;
                }

                return int.TryParse(name, out var parsedValue) && parsedValue == integerValue;
            }

            public override string ToString()
            {
                return DisplayName;
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }



    }
}
