using Xenvious.Helper_Classes;

namespace Xenvious
{
    /// <summary>A global in the freeze list.</summary>
    public class GlobalFreezeer : FreezeItem
    {
        public GlobalFreezeer(string name, string global, EditorKind kind, string text) : base(name, kind, text)
        {
            GlobalName = global;
            GlobalValue = GlobalPath.Evaluate(global);
        }

        public string GlobalName { get; }
        public long GlobalValue { get; }
        public override string Path => GlobalName;

        internal override long Resolve() => new Global(GlobalValue).GetAddress();

        public GlobalFreezeEntry ToEntry() => new GlobalFreezeEntry
        {
            Name = Name, GlobalName = GlobalName, Type = EditorValue.ConfigName(Kind), ValueString = Text
        };

        public static GlobalFreezeer FromEntry(GlobalFreezeEntry e) =>
            new GlobalFreezeer(e.Name, e.GlobalName, EditorValue.FromConfig(e.Type), FreezeEntryText.Of(e.Type, e.ValueString, e.ValueBase64));
    }
}
