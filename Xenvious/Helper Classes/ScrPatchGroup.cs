using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Xenvious
{
    /// <summary>
    /// One row of the Scr Patches page: every entry of scrpatches.json that shares a patch
    /// name. Most patches exist once per creator script; they are one feature, so they are
    /// switched on and off together.
    /// </summary>
    public sealed class ScrPatchGroup : INotifyPropertyChanged
    {
        private static readonly string[] Creators =
        {
            "fm_race_creator", "fm_lts_creator", "fm_capture_creator", "fm_deathmatch_creator", "fm_survival_creator"
        };

        private readonly Action<GTA.ScrPatches, bool> _onToggle;

        public ScrPatchGroup(string name, IReadOnlyList<GTA.ScrPatches> patches, string description,
            Func<string, string, string> translate, Action<GTA.ScrPatches, bool> onToggle)
        {
            Name = name;
            Patches = patches;
            Description = description;
            _onToggle = onToggle;

            var scripts = patches.Select(p => p.script_name).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
            var tags = new List<string>();
            if (Creators.All(scripts.Contains))
            {
                tags.Add(translate("scrpatch_tag_all", "Alle Creator"));
                scripts = scripts.Except(Creators).ToList();
            }
            tags.AddRange(scripts.Select(ShortScriptName));
            if (patches.Any(p => p.dev))
                tags.Add(translate("scrpatch_tag_dev", "Script-Funktionen"));
            if (patches.Any(p => p.trigger == "templates"))
                tags.Add(translate("scrpatch_tag_templates", "nur bei Templates"));
            Tags = tags;
        }

        public string Name { get; }
        public string Description { get; }
        public IReadOnlyList<GTA.ScrPatches> Patches { get; }
        public IReadOnlyList<string> Tags { get; }
        public bool HasDescription => !string.IsNullOrEmpty(Description);

        public bool Enabled
        {
            get => Patches.All(p => p.enabled);
            set
            {
                if (value == Enabled)
                    return;
                foreach (var patch in Patches)
                {
                    if (patch.enabled == value)
                        continue;
                    patch.enabled = value;
                    _onToggle?.Invoke(patch, value);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enabled)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static string ShortScriptName(string script)
        {
            switch (script)
            {
                case "fm_race_creator": return "Race";
                case "fm_lts_creator": return "LTS";
                case "fm_capture_creator": return "Capture";
                case "fm_deathmatch_creator": return "Deathmatch";
                case "fm_survival_creator": return "Survival";
                default: return script;
            }
        }
    }
}
