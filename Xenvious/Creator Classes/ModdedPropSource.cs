using System;

namespace Xenvious
{
    public class ModdedPropSource
    {
        public ModdedPropSource(string scriptName, string displayName)
        {
            ScriptName = scriptName ?? throw new ArgumentNullException(nameof(scriptName));
            DisplayName = displayName ?? scriptName;
        }

        public string ScriptName { get; }

        public string DisplayName { get; }

        public ulong ScriptPointer { get; private set; }

        public ulong? DataRegion { get; set; }

        public bool RefreshScriptPointer()
        {
            ulong pointer = ScrProgramScanner.GetScrProgramByName(ScriptName);
            // A script loaded again sits somewhere else; the old table address now belongs to
            // unrelated memory, so it must be searched again.
            if (pointer != ScriptPointer)
                DataRegion = null;
            ScriptPointer = pointer;
            if (ScriptPointer == 0)
                return false;

            return true;
        }
    }
}
