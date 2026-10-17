using System;
using System.Windows;
using System.Windows.Controls;

namespace Xenvious
{
    // Part of MainWindow: option bits the creator never shows (marked * on Mission / General).
    public partial class MainWindow
    {
        // Bits in the job's option bitsets that fm_mission_controller acts on but the
        // LTS and Capture creators never offer. The General page shows them next to
        // the related creator options. The creator saves each bitset as a whole, so a bit
        // set here stays in the published job. Bit indices are 0-based like
        // the scripts. The bitset offsets are read when used, because OffsetLoader
        // can load them again for the other edition.
        private (CheckBox Box, Func<long> Bitset, int Bit)[] missionExtraBits;

        private (CheckBox Box, Func<long> Bitset, int Bit)[] MissionExtraBits
        {
            get
            {
                if (missionExtraBits == null)
                {
                    missionExtraBits = new (CheckBox, Func<long>, int)[]
                    {
                        (cbmissionextstartcover, () => GTA.Offsets.Editor.menubs2, 12),
                        (cbmissionextnopv, () => GTA.Offsets.Editor.menubs2, 18),
                        (cbmissionextbestweap, () => GTA.Offsets.Editor.menubs4, 4),
                        (cbmissionextnodropweap, () => GTA.Offsets.Editor.menubs4, 6),
                        (cbmissionextnodropammo, () => GTA.Offsets.Editor.menubs4, 27),
                        (cbmissionextteamheal, () => GTA.Offsets.Editor.menubs5, 22),
                        (cbmissionextteamvehexproof, () => GTA.Offsets.Editor.menubs6, 1),
                        (cbmissionextdelvehdeath, () => GTA.Offsets.Editor.menubs7, 26),
                        (cbmissionextnoarmour, () => GTA.Offsets.Editor.menubs8, 26),
                        (cbmissionexthidelives, () => GTA.Offsets.Editor.menubs12, 14),
                        (cbmissionexthidereticle, () => GTA.Offsets.Editor.menubs12, 21),
                        (cbmissionextruinernorockets, () => GTA.Offsets.Editor.menubs12, 31),
                        (cbmissionextnoregionvfx, () => GTA.Offsets.Editor.menubs13, 13),
                        (cbmissionextindoorsprint, () => GTA.Offsets.Editor.menubs13, 28),
                        (cbmissionextmuteambience, () => GTA.Offsets.Editor.menubs15, 6),
                        (cbmissionextconvroofdown, () => GTA.Offsets.Editor.menubs19, 16),
                        (cbmissionextnoheadlights, () => GTA.Offsets.Editor.menubs19, 20),
                        (cbmissionextnovehexpdmg, () => GTA.Offsets.Editor.menubs21, 12),
                        (cbmissionextnostuckdestroy, () => GTA.Offsets.Editor.menubs23, 15),
                        // Mission Creator "Hint Cams" (PMCT_OGF_HCAM): set = off. Only the 2020 and v3 controllers read it;
                        // the per-rule Hint Cam (irbs13 bit 0) works in all three.
                        (cbmissionextnohintcam, () => GTA.Offsets.Editor.menubs27, 15),
                        (cbmissionextnothrowweap, () => GTA.Offsets.Editor.menubs31, 25),
                        // The same two world options on the dashboard.
                        (cbdashmuteambience, () => GTA.Offsets.Editor.menubs15, 6),
                        (cbdashnoregionvfx, () => GTA.Offsets.Editor.menubs13, 13),
                    };
                }
                return missionExtraBits;
            }
        }

        private void cbMissionExtra_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var extra in MissionExtraBits)
            {
                if (!ReferenceEquals(extra.Box, sender))
                    continue;

                // A key missing from offsets.ini loads as 0, which is not a bitset.
                long bitset = extra.Bitset();
                if (bitset != 0)
                    Functions.Write.writebinary(extra.Bit, bitset, extra.Box);
                return;
            }
        }

        private void GetMissionExtraValues()
        {
            foreach (var extra in MissionExtraBits)
            {
                long bitset = extra.Bitset();
                if (bitset != 0)
                    Functions.Read.checkbinary(extra.Bit, bitset, extra.Box);
            }
        }
    }
}
