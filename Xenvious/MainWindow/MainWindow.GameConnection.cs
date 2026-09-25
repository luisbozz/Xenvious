using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Xenvious.Logging;
using static mry.mem;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: attaching to the game process -- edition, offsets, pointers,
    // and the pointer readout on the Misc / Stuff page.
    public partial class MainWindow
    {
        int pid = 0;

        private async void Timercheckgta_Tick(object sender, EventArgs e)
        {
            // A user can close Legacy and start Enhanced without restarting the
            // editor. Offsets and patches are per-build, so the data has to
            // follow the process rather than stay at whatever was loaded first.
            if (GameVariant.DetectChanged())
            {
                Log.Info($"Reloading offline data for GTA {GameVariant.DisplayName(GameVariant.Current)}",
                    source: "timercheckgta");
                pid = 0;
                OffsetLoader.Load();
                BindLoadedData();
            }

            var p = Process.GetProcessesByName(GameVariant.ProcessName);
            if (p.Length > 0)
            {
                if (!m.IsProcOpen)
                {
                    try
                    {
                        m.OpenProcess(GameVariant.ProcessName);
                    }
                    catch (Exception) { }
                }
                if (pid != p[0].Id)
                {
                    if (pid == 0)
                    {
                        Log.Info("First Init", source: "timercheckgta");
                    }
                    OnlineEnable.isInjected();
                    var mainmodule = m.getMainModule();
                    // With every pattern cached for this game build there is nothing to scan,
                    // so skip reading the whole module; the getters answer from the cache.
                    bool fromCache = GTA.AllPatternsCached();
                    byte[] buff = fromCache ? null : m.memory(mainmodule.BaseAddress.ToInt64().ToString("X")).GetBytes(mainmodule.ModuleMemorySize);
                    Log.Info(fromCache ? "AOB: all patterns from cache, module not read" : "AOB: scanning module", source: "timercheckgta");
                    long globalptr = GTA.getGlobalPointer(buff).ToInt64();
                    if (!globalPtrSanityCheck(globalptr))
                    {
                        Log.Info($"Wait with init because too early! GlobalPTR={globalptr.ToString("X")}", source: "timercheckgta");
                        return;
                    }
                    if (buff == null || buff.Length > 0)
                    {
                        Action[] actions;

                        actions = new Action[]
                        {
                        () => GTA.Offsets.Editor.WORLDversion = GTA.getWorldPointer(buff).ToInt64(),
                        () => GTA.Offsets.Editor.GlobalPTRversion = globalptr,
                        () => GTA.Offsets.Editor.dev = GTA.getDEVPointer(buff).ToInt64(),
                        () => GTA.Offsets.Editor.camptr = GTA.getCAMPointer(buff).ToInt64(),
                        () => GTA.Offsets.Editor.versionptr = GTA.getVersionPointer(buff).ToInt64(),
                        () => GTA.Offsets.Editor.creator_camptr = GTA.getCreatorCamPointer(buff).ToInt64(),
                        () => GTA.Offsets.Editor.Session.patternPointer = GTA.getSessionPointer(buff).ToInt64(),
                        //() => GTA.Offsets.Editor.img_addy = m.memory((m.memory(GTA.getIMGPointer(buff).ToInt64()).GetAddress() + 0x18)).Get<long>(),
                        () => GTA.Offsets.Editor.cursor_addy = (m.memory(GTA.getCursorPointer(buff).ToInt64()).GetAddress() + 0x20),
                        () => GTA.Offsets.Editor.scrProgram_addy = GTA.getscrProgramPointer(buff).ToInt64(),
                        () => GTA.Offsets.Editor.nextcp = GTA.getNEXTCPPointer(buff).ToInt64(),
                        () => GTA.Offsets.Editor.localptr = GTA.getCurrentCreatorAddy()
                        };
                        // Parallel.Invoke rethrows as an AggregateException, and
                        // this handler is async void, so anything thrown here is
                        // lost: the tick stops half way, the window comes up
                        // without its pointers, and nothing says why. Enhanced
                        // has nine of these AOB patterns empty, so the resolvers
                        // hand back zero and whatever reads through that address
                        // is the one that throws.
                        try
                        {
                            Parallel.Invoke(actions);
                        }
                        catch (AggregateException agg)
                        {
                            foreach (Exception inner in agg.Flatten().InnerExceptions)
                            {
                                Log.Error("Resolving a pointer failed", inner, source: "timercheckgta");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Resolving pointers failed", ex, source: "timercheckgta");
                        }
                    }

                    GTA.Offsets.Editor.preset_version = IntPtr.Subtract((IntPtr)GTA.Offsets.Editor.GlobalPTRversion, 304).ToInt64();
                    GTA.Offsets.Editor.version = GTA.Offsets.Editor.GlobalPTRversion + 0x90;
                    GTA.Offsets.Editor.version2 = GTA.Offsets.Editor.GlobalPTRversion + 0x98;
                    GTA.Offsets.Editor.version_jobs = GTA.Offsets.Editor.GlobalPTRversion + 0xF8;

                    tbmodstuffgloablptr.Text = GTA.Offsets.Editor.GlobalPTRversion.ToString("X");
                    tbmodstuffworldptr.Text = GTA.Offsets.Editor.WORLDversion.ToString("X");
                    if (GTA.Offsets.Editor.localptr != null)
                    {
                        tbmodstuffccptr.Text = string.Join(", ", GTA.Offsets.Editor.localptr.Select(x => x.ToString("X")));
                        tbmodstuffccprace.Text = m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, GTA.Offsets.Editor.OFFSET_current_creator_pre_race * 8 }).GetAddress().ToString("X");
                        tbmodstuffccplts.Text = m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, GTA.Offsets.Editor.OFFSET_current_creator_pre_lts * 8 }).GetAddress().ToString("X");
                        tbmodstuffccpcapture.Text = m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, GTA.Offsets.Editor.OFFSET_current_creator_pre_capture * 8 }).GetAddress().ToString("X");
                        tbmodstuffccpdm.Text = m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, GTA.Offsets.Editor.OFFSET_current_creator_pre_dm * 8 }).GetAddress().ToString("X");
                        tbmodstuffccpsurvival.Text = m.memory(GTA.Offsets.Editor.localptr[0], new long[] { GTA.Offsets.Editor.localptr[1], GTA.Offsets.Editor.OFFSET_script_local_start, GTA.Offsets.Editor.OFFSET_current_creator_pre_survival * 8 }).GetAddress().ToString("X");
                        tbmodstuffpst.Text = m.memory((getCreatorScriptLocalWorkerBase() + GTA.Offsets.Editor.OFFSET_current_creator_worker_offset_menu * 8).ToString("X")).GetAddress().ToString("X");
                    }
                    tbmodstuffcs.Text = m.memory(GTA.Offsets.Editor.version, 0x0).GetAddress().ToString("X");
                    tbmodstuffc2s.Text = m.memory(GTA.Offsets.Editor.version2, 0x0).GetAddress().ToString("X");
                    tbmodstuffce.Text = new Global(GTA.Offsets.Editor.creator_end).GetAddress().ToString("X");
                    tbmodstuffjobs.Text = new Global(GTA.Offsets.Editor.Jobs.published_number).GetAddress().ToString("X");

                    if (Functions.Read.IsEpicVersion())
                    {
                        isrstar = false;
                        issteam = false;
                        isepic = true;
                        GTA.Offsets.Editor.Session.pointer = GTA.Offsets.Editor.Session.pointer_rstar;
                    }
                    else if (Functions.Read.IsSteamVersion())
                    {
                        isrstar = false;
                        isepic = false;
                        issteam = true;
                        GTA.Offsets.Editor.Session.pointer = GTA.Offsets.Editor.Session.pointer_steam;
                    }
                    else
                    {
                        isrstar = true;
                        isepic = false;
                        issteam = false;
                        GTA.Offsets.Editor.Session.pointer = GTA.Offsets.Editor.Session.pointer_rstar;
                    }

                    // The fixed RVAs above go stale with every game update. The pattern
                    // resolves the struct in whichever binary is running.
                    if (GTA.Offsets.Editor.Session.patternPointer != 0)
                        GTA.Offsets.Editor.Session.pointer = GTA.Offsets.Editor.Session.patternPointer;

                }

                pid = p[0].Id;

                if (!mWorker.IsBusy)
                {
                    mWorker.RunWorkerAsync();
                }

                if (SGTAMessage.Visibility == Visibility.Visible)
                {
                    SGTAMessage.Visibility = Visibility.Collapsed;
                }

            }
            else
            {

                if (mWorker.IsBusy)
                {
                    Log.Error("GTA Closed... kill worker", source: "timercheckgta");
                    mWorker.CancelAsync();
                }
                if (m.IsProcOpen)
                    m.CloseProcess();
            }
        }
    }
}
