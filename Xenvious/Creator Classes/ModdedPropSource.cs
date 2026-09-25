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
            ScriptPointer = ScrProgramScanner.GetScrProgramByName(ScriptName);
            if (ScriptPointer == 0)
            {
                DataRegion = null;
                return false;
            }

            return true;
        }
    }
}
