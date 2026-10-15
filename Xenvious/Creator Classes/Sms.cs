using System.Collections.Generic;
using System.Text;

namespace Xenvious
{
    /// <summary>
    /// The job's ten text messages (Global_4718592.f_112650[i /*44*/], JSON keys sms*). What each
    /// field does, from the controllers (fm_mission_controller func_12446 area, _2020 func_20614)
    /// and the Mission Creator's SMS menu (public_mission_creator 1.73, labels FMMC_SMS*):
    /// - f_0 team: the team whose rule sends it; -1 = the slot is unused (nothing is sent).
    /// - f_1 dialogue trigger: sent when that dialogue trigger fires, besides the rule (-1 = off).
    /// - f_2 rule, f_3 0 = at the rule's start, 1 = at its mid point, f_5 delay in seconds.
    /// - f_4 points: only once the team has at least this many points; -1 = off (any negative
    ///   value behaves the same: the check is "score &gt;= value").
    /// - f_6 the text, two text labels of 64 bytes.
    /// - f_39 1 = every team gets it (the rule is still the team's).
    /// - f_40 sender: -1 = the job's contact, 0..24 another character.
    /// - f_42 bit 0: the text is a game text label, not a custom text; bit 1 FMMC_SMS_BD (a flag
    ///   of the message itself); bit 2: when the phone cannot take it right now, send it later.
    ///   f_41 1 adds the date as a second part (only with bit 0, only the 2020 controller).
    /// - f_43 a mission flag that is on while the message is open on the phone (2020 controller).
    /// </summary>
    public static class Sms
    {
        public const int Count = 10;

        public static bool Ready => MainWindow.m != null && MainWindow.m.IsProcOpen && GTA.Offsets.Editor.SMS.NEXT != 0;

        private static long At(long field, int i) => field + GTA.Offsets.Editor.SMS.NEXT * i;

        public static int Get(long field, int i) => field == 0 ? 0 : new Global(At(field, i)).Get<int>();

        public static void Set(long field, int i, int value)
        {
            if (Ready && field != 0)
                new Global(At(field, i)).SetInt(value);
        }

        public static bool Bit(long field, int i, int bit) => (Get(field, i) & (1 << bit)) != 0;

        public static void SetBit(long field, int i, int bit, bool on)
        {
            int v = Get(field, i);
            Set(field, i, on ? v | (1 << bit) : v & ~(1 << bit));
        }

        public static int Team(int i) => Get(GTA.Offsets.Editor.SMS.team, i);
        public static int Rule(int i) => Get(GTA.Offsets.Editor.SMS.rule, i);
        public static bool AtMidPoint(int i) => Get(GTA.Offsets.Editor.SMS.time, i) == 1;
        public static int Delay(int i) => Get(GTA.Offsets.Editor.SMS.delay, i);
        public static bool ToAllTeams(int i) => Get(GTA.Offsets.Editor.SMS.sndall, i) == 1;

        public static string Text(int i)
        {
            if (!Ready || GTA.Offsets.Editor.SMS.txt == 0)
                return "";
            // Two 64 byte text labels; each read up to its own NUL.
            var sb = new StringBuilder();
            for (int part = 0; part < 2; part++)
            {
                byte[] b = new Global(At(GTA.Offsets.Editor.SMS.txt, i) + GTA.Offsets.Editor.txt_NEXT * part).GetBytes(64);
                if (b == null)
                    break;
                int n = System.Array.IndexOf(b, (byte)0);
                sb.Append(Encoding.UTF8.GetString(b, 0, n < 0 ? 63 : System.Math.Min(n, 63)));
            }
            return sb.ToString();
        }

        /// <summary>The messages a team's rule sends, in slot order.</summary>
        public static List<int> ForRule(int team, int rule)
        {
            var list = new List<int>();
            if (!Ready)
                return list;
            for (int i = 0; i < Count; i++)
                if (Team(i) == team && Rule(i) == rule)
                    list.Add(i);
            return list;
        }

        /// <summary>The first unused slot (team -1 and no text), -1 when all ten are used.</summary>
        public static int FreeSlot()
        {
            for (int i = 0; i < Count; i++)
                if (Team(i) < 0 && Text(i).Length == 0)
                    return i;
            return -1;
        }

        /// <summary>A new message for the team's rule at its start, with the creator's defaults for the rest.</summary>
        public static int Add(int team, int rule)
        {
            int i = FreeSlot();
            if (i < 0)
                return -1;
            Set(GTA.Offsets.Editor.SMS.team, i, team);
            Set(GTA.Offsets.Editor.SMS.rule, i, rule);
            Set(GTA.Offsets.Editor.SMS.time, i, 0);
            Set(GTA.Offsets.Editor.SMS.delay, i, 0);
            Set(GTA.Offsets.Editor.SMS.smsdt, i, -1);
            Set(GTA.Offsets.Editor.SMS.ptsreq, i, -1);
            Set(GTA.Offsets.Editor.SMS.sndall, i, 0);
            Set(GTA.Offsets.Editor.SMS.smsc, i, -1);
            Set(GTA.Offsets.Editor.SMS.smsei, i, 0);
            Set(GTA.Offsets.Editor.SMS.smstl, i, 0);
            Set(GTA.Offsets.Editor.SMS.smspwo, i, -1);
            return i;
        }
    }
}
