using System;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// After an LTS creator test the creator goes on where the test ended: the fly camera above
    /// that point when the test was started from the camera, the player standing there when it
    /// was started on foot.
    ///
    /// fm_lts_creator 1.73, test end (Local_8883.f_565 case 7): bit 30 of the test local
    /// (OFFSET_current_creator_test_lts, Local_1709) keeps the mode the test was started from.
    /// The creator then sets bit 27 for the mode switch. Without bit 30 the camera is rebuilt
    /// from the camera struct (OFFSET_current_creator_cam_lts, Local_1757): at f_18 or f_15 when
    /// one is set, else at f_2 looking straight down. On foot the switch (func_4701) clears
    /// bit 27 once the player has control, without moving the player.
    /// </summary>
    public static class TestReturn
    {
        private const int BitOnFoot = 30;
        private const int BitSwitch = 27;
        // Height of the camera above the last position; the creator's own camera starts 40 m
        // above the ground when leaving the player.
        private const float CamHeight = 40f;

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
                    var pos = GTA.GetLocation();
                    if (pos.X != 0 || pos.Y != 0 || pos.Z != 0)
                    {
                        _last = pos;
                        _haveLast = true;
                        // Written all along: the camera is rebuilt in the frame after the test end.
                        PlaceCamera();
                    }
                    if (scan && !GTA.IsScriptRunning("fm_mission_controller"))
                    {
                        _phase = Phase.WaitTakeOver;
                        _since = DateTime.Now;
                        Log.Debug("LTS test ended", source: "TestReturn");
                    }
                    break;

                case Phase.WaitTakeOver:
                    if (Bit(BitSwitch))
                    {
                        if (Bit(BitOnFoot))
                        {
                            _phase = Phase.WaitSwitch;
                            _since = DateTime.Now;
                        }
                        else
                        {
                            Log.Debug("LTS test end: camera", source: "TestReturn");
                            _phase = Phase.Idle;
                        }
                    }
                    else if (DateTime.Now - _since > TimeSpan.FromSeconds(20))
                    {
                        Log.Debug("LTS test end: creator did not take over", source: "TestReturn");
                        _phase = Phase.Idle;
                    }
                    break;

                case Phase.WaitSwitch:
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

        private static bool Bit(int bit)
        {
            long addr = Local(GTA.Offsets.Editor.OFFSET_current_creator_test_lts);
            return addr != 0 && (MainWindow.m.memory(addr.ToString("X")).Get<int>() & (1 << bit)) != 0;
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
