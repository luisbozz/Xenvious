namespace Xenvious
{
    /// <summary>
    /// How many entities of a kind the creators let you place, read from the decompiled creator
    /// scripts (1.73 Legacy 3889, Enhanced 1158). Where the script decides by tunable, the value
    /// is the one the tunable raises it to.
    /// </summary>
    public static class CreatorLimits
    {
        // The actor placement function gets 80 in the LTS and Capture creators. The survival
        // creator passes 60, or 15 when the survival tunable is off.
        public static int Actors(string creator) => creator == "fm_survival_creator" ? 60 : 80;

        // Size of the vehicle and weapon arrays; the placement code copies defaults into the
        // next slot only below it.
        public const int Vehicles = 32;
        public const int Weapons = 60;

        // 32 in every creator; 64 only while the mission creator or mission controller runs.
        public const int Zones = 32;

        // Legacy: always 40. Enhanced: 100 with the tunable Rockstar added (40 without).
        public static int Fixtures => GameVariant.IsEnhanced ? 100 : 40;
    }
}
