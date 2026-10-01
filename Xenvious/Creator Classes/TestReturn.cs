using System;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// After an LTS creator test the creator goes on where the test ended: the fly camera above
    /// that point when the test was started from the camera, the player standing there when it
    /// was started on foot.
    ///
    /// fm_lts_creator 1.73: its Test entry sets bits 18 and 25 of the test local
    /// (OFFSET_current_creator_test_lts, Local_1709), and the test start switches to the player,
    /// which sets bit 30. At the test end (Local_8883.f_565 case 7) the creator clears bits 18
    /// and 25, and bit 30 decides: set, the player is unfrozen on foot; clear, the fly camera is
    /// rebuilt from the camera struct (OFFSET_current_creator_cam_lts, Local_1757) at f_18 or
    /// f_15 when one is set, else at f_2 looking straight down. The creator does not remember
    /// that the test came from the camera, so Xenvious notes bit 30 before the test and clears
    /// it again as soon as fm_mission_controller has ended.
    /// </summary>
    public static class TestReturn
    {
        private const int BitOnFoot = 30;
        private const int BitSwitch = 27;
        private const int BitTestEntry = 18;
        private const int BitTestStart = 25;
        // Height of the camera above the last position; the creator's own camera starts 40 m
        // above the ground when leaving the player.
        private const float CamHeight = 40f;

        private enum Phase { Idle, InTest, WaitTakeOver, WaitSwitch }

        private static Phase _phase = Phase.Idle;
        private static long[] _creator;
        private static long[] _controller;
        private static bool _fromCamera;
        private static XenVector3 _last;
        private static bool _haveLast;
        private static DateTime _since;
        private static int _tick;

        /// <summary>Called every 50 ms from the patch thread.</summary>
        public static void Tick()
        {
            if (MainWindow.m == null || !MainWindow.m.IsProcOpen || GTA.Offsets.Editor.OFFSET_current_creator_test_lts == 0)
            {
                _phase = Phase.Idle;
                return;
            }
            // Looking through the thread list costs a few hundred reads, so twice a second.
            bool scan = _tick++ % 10 == 0;

            switch (_phase)
            {
                case Phase.Idle:
                    if (scan)
                        _creator = GTA.getLocalScriptAddy("fm_lts_creator");
                    if (_creator == null)
                        break;
                    // The mode before the test, as long as no Test entry is under way.
                    int v = Value();
                    if ((v & (1 << BitTestEntry)) == 0 && (v & (1 << BitTestStart)) == 0)
                        _fromCamera = (v & (1 << BitOnFoot)) == 0;
                    if (scan)
                    {
                        _controller = GTA.getLocalScriptAddy("fm_mission_controller");
                        if (_controller != null)
                        {
                            _haveLast = false;
                            _phase = Phase.InTest;
                            Log.Debug("LTS test started from the " + (_fromCamera ? "camera" : "player"), source: "TestReturn");
                        }
                    }
                    break;

                case Phase.InTest:
                    var pos = GTA.GetLocation();
                    if (pos.X != 0 || pos.Y != 0 || pos.Z != 0)
                    {
                        _last = pos;
                        _haveLast = true;
                        if (_fromCamera)
                            PlaceCamera();
                    }
                    // Every tick: bit 30 has to be cleared before the creator's test end runs. The
                    // controller can move to another thread slot while it starts, so an empty slot
                    // only counts once the whole list has no controller either.
                    if (!Alive(_controller, "fm_mission_controller")
                        && (_controller = GTA.getLocalScriptAddy("fm_mission_controller")) == null)
                    {
                        _phase = Phase.WaitTakeOver;
                        _since = DateTime.Now;
                        Log.Debug("LTS test ended", source: "TestReturn");
                        goto case Phase.WaitTakeOver;
                    }
                    break;

                case Phase.WaitTakeOver:
                    if (_fromCamera)
                        SetBit(BitOnFoot, false);
                    if (!Bit(BitTestEntry) && !Bit(BitTestStart))
                    {
                        if (_fromCamera)
                        {
                            Log.Debug("LTS test end: back to the camera", source: "TestReturn");
                            _phase = Phase.Idle;
                        }
                        else
                        {
                            _phase = Phase.WaitSwitch;
                            _since = DateTime.Now;
                        }
                    }
                    else if (DateTime.Now - _since > TimeSpan.FromSeconds(20))
                    {
                        Log.Debug("LTS test end: creator did not take over", source: "TestReturn");
                        _phase = Phase.Idle;
                    }
                    break;

                case Phase.WaitSwitch:
                    // Half a second for the creator to fade in and hand over control.
                    if (!Bit(BitSwitch) && DateTime.Now - _since > TimeSpan.FromMilliseconds(500))
                    {
                        if (_haveLast)
                        {
                            GTA.Teleport(_last);
                            Log.Debug($"LTS test end: player back to {_last.X:0.0} {_last.Y:0.0} {_last.Z:0.0}", source: "TestReturn");
                        }
                        _phase = Phase.Idle;
                    }
                    else if (DateTime.Now - _since > TimeSpan.FromSeconds(10))
                    {
                        _phase = Phase.Idle;
                    }
                    break;
            }
        }

        // One thread slot instead of the whole list: still live and still the same script.
        private static bool Alive(long[] slot, string script)
        {
            if (slot == null || !GTA.IsLiveThread(slot[0], slot[1]))
                return false;
            try
            {
                if (GTA.Offsets.Editor.OFFSET_script_hash != 0)
                    return MainWindow.m.memory(slot[0], new long[] { slot[1], GTA.Offsets.Editor.OFFSET_script_hash }).Get<uint>() == MainWindow.Joaat(script);
                return MainWindow.m.memory(slot[0], new long[] { slot[1], GTA.Offsets.Editor.OFFSET_script_name }).GetString().ToLower() == script;
            }
            catch
            {
                return false;
            }
        }

        private static long Local(long index)
        {
            if (_creator == null || index == 0)
                return 0;
            try
            {
                return MainWindow.m.memory(_creator[0], new long[] { _creator[1], GTA.Offsets.Editor.OFFSET_script_local_start, index * 8 }).GetAddress();
            }
            catch
            {
                return 0;
            }
        }

        private static int Value()
        {
            long addr = Local(GTA.Offsets.Editor.OFFSET_current_creator_test_lts);
            return addr == 0 ? 0 : MainWindow.m.memory(addr.ToString("X")).Get<int>();
        }

        private static bool Bit(int bit) => (Value() & (1 << bit)) != 0;

        private static void SetBit(int bit, bool on)
        {
            long addr = Local(GTA.Offsets.Editor.OFFSET_current_creator_test_lts);
            if (addr == 0)
                return;
            int v = MainWindow.m.memory(addr.ToString("X")).Get<int>();
            int n = on ? v | (1 << bit) : v & ~(1 << bit);
            if (n != v)
                MainWindow.m.memory(addr.ToString("X")).SetInt(n);
        }

        // The camera struct's start points: f_2 always, f_15 / f_18 only when the creator set them.
        private static void PlaceCamera()
        {
            long cam = Local(GTA.Offsets.Editor.OFFSET_current_creator_cam_lts);
            if (cam == 0)
                return;
            foreach (int field in new[] { 2, 15, 18 })
            {
                long at = cam + field * 8;
                if (field != 2 && IsZero(at))
                    continue;
                MainWindow.m.memory(at.ToString("X")).SetFloat(_last.X);
                MainWindow.m.memory((at + 8).ToString("X")).SetFloat(_last.Y);
                MainWindow.m.memory((at + 16).ToString("X")).SetFloat(_last.Z + CamHeight);
            }
        }

        private static bool IsZero(long at)
        {
            for (int i = 0; i < 3; i++)
                if (MainWindow.m.memory((at + i * 8).ToString("X")).Get<float>() != 0f)
                    return false;
            return true;
        }
    }
}
