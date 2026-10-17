using System;
using Xenvious.Helper_Classes;

namespace Xenvious
{
    /// <summary>A local of a running script in the freeze list.</summary>
    public class LocalFreezeer : FreezeItem
    {
        public LocalFreezeer(string name, string script, string local, EditorKind kind, string text) : base(name, kind, text)
        {
            Script = script;
            LocalName = local;
            LocalValue = GlobalPath.Evaluate(local);
        }

        public string Script { get; }
        public string LocalName { get; }
        public long LocalValue { get; }
        public override string Path => Script + " · " + LocalName;

        internal override long Resolve()
        {
            long[] thread = GTA.getLocalScriptAddy(Script);
            if (thread == null)
                return 0;
            return MainWindow.m.memory(thread[0], new long[] { thread[1], GTA.Offsets.Editor.OFFSET_script_local_start, LocalValue * 8 }).GetAddress();
        }

        public LocalFreezeEntry ToEntry() => new LocalFreezeEntry
        {
            Name = Name, Script = Script, LocalName = LocalName, Type = EditorValue.ConfigName(Kind), ValueString = Text
        };

        public static LocalFreezeer FromEntry(LocalFreezeEntry e) =>
            new LocalFreezeer(e.Name, e.Script, e.LocalName, EditorValue.FromConfig(e.Type), FreezeEntryText.Of(e.Type, e.ValueString, e.ValueBase64));
    }

    internal static class FreezeEntryText
    {
        // Older versions saved a byte as Base64 bytes; its first byte is the value.
        public static string Of(string type, string value, string base64)
        {
            if (type == "bytes" && string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(base64))
            {
                byte[] b = Convert.FromBase64String(base64);
                return b.Length > 0 ? b[0].ToString(System.Globalization.CultureInfo.InvariantCulture) : "0";
            }
            return value ?? "";
        }
    }
}
