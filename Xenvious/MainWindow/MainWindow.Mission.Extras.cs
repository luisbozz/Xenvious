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
        // set here stays in the published job. Bit indices are 1-based like
        // writebinary. The bitset offsets are read when used, because OffsetLoader
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
                        (cbmissionextstartcover, () => GTA.Offsets.Editor.menubs2, 13),
                        (cbmissionextnopv, () => GTA.Offsets.Editor.menubs2, 19),
                        (cbmissionextbestweap, () => GTA.Offsets.Editor.menubs4, 5),
                        (cbmissionextnodropweap, () => GTA.Offsets.Editor.menubs4, 7),
                        (cbmissionextnodropammo, () => GTA.Offsets.Editor.menubs4, 28),
                        (cbmissionextteamheal, () => GTA.Offsets.Editor.menubs5, 23),
                        (cbmissionextteamvehexproof, () => GTA.Offsets.Editor.menubs6, 2),
                        (cbmissionextdelvehdeath, () => GTA.Offsets.Editor.menubs7, 27),
                        (cbmissionextnoarmour, () => GTA.Offsets.Editor.menubs8, 27),
                        (cbmissionexthidelives, () => GTA.Offsets.Editor.menubs12, 15),
                        (cbmissionexthidereticle, () => GTA.Offsets.Editor.menubs12, 22),
                        (cbmissionextruinernorockets, () => GTA.Offsets.Editor.menubs12, 32),
                        (cbmissionextnoregionvfx, () => GTA.Offsets.Editor.menubs13, 14),
                        (cbmissionextindoorsprint, () => GTA.Offsets.Editor.menubs13, 29),
                        (cbmissionextmuteambience, () => GTA.Offsets.Editor.menubs15, 7),
                        (cbmissionextconvroofdown, () => GTA.Offsets.Editor.menubs19, 17),
                        (cbmissionextnoheadlights, () => GTA.Offsets.Editor.menubs19, 21),
                        (cbmissionextnovehexpdmg, () => GTA.Offsets.Editor.menubs21, 13),
                        (cbmissionextnostuckdestroy, () => GTA.Offsets.Editor.menubs23, 16),
                        (cbmissionextnothrowweap, () => GTA.Offsets.Editor.menubs31, 26),
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
