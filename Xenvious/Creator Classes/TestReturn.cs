using System;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// After an LTS creator test the player stays on foot where the test ended, also when the
    /// test was started from the camera.
    ///
    /// fm_lts_creator 1.73, test end (Local_8883.f_565 case 7): bit 30 of the test local
    /// (OFFSET_current_creator_test_lts, Local_1709) decides on foot or camera. With bit 30 the
    /// creator unfreezes the player and fades in, then sets bit 27, and the mode switch
    /// (func_4701) clears bit 27 once the player has control, without moving the player.
    /// Without bit 30 it freezes the player and builds the fly camera. So bit 30 is held while
    /// fm_mission_controller runs and until the creator has taken over; then the player goes to
    /// the last position seen in the test.
    /// </summary>
    public static class TestReturn
    {
        private const int BitOnFoot = 30;
        private const int BitSwitch = 27;

        private enum Phase { Idle, InTest, WaitTakeOver, WaitSwitch }

        private static Phase _phase = Phase.Idle;
        private static long[] _creator;
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
                    if (scan && GTA.IsScriptRunning("fm_mission_controller"))
                    {
                        _creator = GTA.getLocalScriptAddy("fm_lts_creator");
                        if (_creator != null)
                        {
                            _haveLast = false;
                            _phase = Phase.InTest;
                            Log.Debug("LTS test started", source: "TestReturn");
                        }
                    }
                    break;

                case Phase.InTest:
                    HoldOnFoot();
                    var pos = GTA.GetLocation();
                    if (pos.X != 0 || pos.Y != 0 || pos.Z != 0)
                    {
                        _last = pos;
                        _haveLast = true;
                    }
                    if (scan && !GTA.IsScriptRunning("fm_mission_controller"))
                    {
                        _phase = Phase.WaitTakeOver;
                        _since = DateTime.Now;
                        Log.Debug("LTS test ended", source: "TestReturn");
                    }
                    break;

                case Phase.WaitTakeOver:
                    HoldOnFoot();
                    // The creator's test end sets bit 27 for the mode switch.
                    if (Bit(BitSwitch))
                    {
                        _phase = Phase.WaitSwitch;
                        _since = DateTime.Now;
                    }
                    else if (DateTime.Now - _since > TimeSpan.FromSeconds(20))
                    {
                        Log.Debug("LTS test end: creator did not take over", source: "TestReturn");
                        _phase = Phase.Idle;
                    }
                    break;

                case Phase.WaitSwitch:
                    HoldOnFoot();
                    if (!Bit(BitSwitch))
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

        private static long Address()
        {
            if (_creator == null)
                return 0;
            try
            {
                return MainWindow.m.memory(_creator[0], new long[] { _creator[1], GTA.Offsets.Editor.OFFSET_script_local_start, GTA.Offsets.Editor.OFFSET_current_creator_test_lts * 8 }).GetAddress();
            }
            catch
            {
                return 0;
            }
        }

        private static bool Bit(int bit)
        {
            long addr = Address();
            return addr != 0 && (MainWindow.m.memory(addr.ToString("X")).Get<int>() & (1 << bit)) != 0;
        }

        private static void HoldOnFoot()
        {
            long addr = Address();
            if (addr == 0)
                return;
            int v = MainWindow.m.memory(addr.ToString("X")).Get<int>();
            if ((v & (1 << BitOnFoot)) == 0)
                MainWindow.m.memory(addr.ToString("X")).SetInt(v | (1 << BitOnFoot));
        }
    }
}
