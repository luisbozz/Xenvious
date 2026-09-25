using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Xenvious
{
    /// <summary>
    /// Provides the runtime data that was previously fetched from the auth server
    /// (offsets and the various detail lists). The data is embedded into the
    /// executable as resources so the application stays a single self-contained EXE
    /// and works completely offline.
    ///
    /// Two kinds of data live here:
    ///
    /// * Game content -- props, vehicles, weapons, actors. The same in both GTA V
    ///   builds, so it is stored once at the root of OfflineData.
    /// * Build-specific data -- offsets and script patches. Legacy and Enhanced are
    ///   compiled separately, so every address and every byte pattern differs.
    ///   These live in a folder per build and are selected by <see cref="GameVariant"/>.
    ///
    /// Build-specific data is never substituted across builds. Loading Legacy
    /// offsets into an Enhanced process would resolve to unrelated memory and
    /// corrupt whatever happens to sit there, so a missing resource throws.
    /// </summary>
    public static class OfflineData
    {
        private static string LoadResource(string suffix)
        {
            Assembly asm = Assembly.GetExecutingAssembly();

            // Embedded resource names mirror the folder layout, e.g.
            // "Xenvious.OfflineData.legacy.offsets.ini". Match by suffix so the
            // code is resilient to namespace/folder changes above OfflineData.
            string resourceName = asm.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
                throw new FileNotFoundException("Embedded offline data resource not found: " + suffix);

            using (Stream stream = asm.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        /// <summary>Content shared by both builds.</summary>
        private static string LoadShared(string fileName) =>
            LoadResource("OfflineData." + fileName);

        /// <summary>
        /// Data for one build. The folder name is part of the resource name, so
        /// "OfflineData.legacy.offsets.ini" and "OfflineData.enhanced.offsets.ini"
        /// are distinct suffixes and neither can satisfy a request for the other.
        /// </summary>
        private static string LoadForEdition(GameEdition edition, string fileName) =>
            LoadResource("OfflineData." + GameVariant.DataFolder(edition) + "." + fileName);

        private static string LoadCurrent(string fileName) =>
            LoadForEdition(GameVariant.Current, fileName);

        /// <summary>Raw INI content that populates all GTA editor offsets.</summary>
        public static string Offsets => LoadCurrent("offsets.ini");

        /// <summary>Script patch definitions (JSON array).</summary>
        public static string ScrPatches => LoadCurrent("scrpatches.json");

        /// <summary>Developer script patch definitions (JSON array).</summary>
        public static string ScrPatchesDev => LoadCurrent("scrpatchesdev.json");

        /// <summary>Offsets for one specific build, regardless of what is detected.</summary>
        public static string OffsetsFor(GameEdition edition) =>
            LoadForEdition(edition, "offsets.ini");

        /// <summary>Prop detail list (JSON array).</summary>
        public static string Props => LoadShared("props.json");

        /// <summary>Vehicle detail list (JSON array).</summary>
        public static string Vehicles => LoadShared("vehicles.json");

        /// <summary>Weapon detail list (JSON array).</summary>
        public static string Weapons => LoadShared("weapons.json");

        /// <summary>Actor detail list (JSON array).</summary>
        public static string Actors => LoadShared("actors.json");

        /// <summary>Built-in advanced placement presets (JSON array).</summary>
        public static string PlacementPresets => LoadShared("placement_presets.json");
    }
}
