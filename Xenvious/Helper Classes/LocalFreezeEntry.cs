using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenvious.Helper_Classes
{
    public sealed class LocalFreezeEntry
    {
        public string Name { get; set; } = "";
        public string Script { get; set; } = "";
        public string LocalName { get; set; } = "";
        public string Type { get; set; } = "";
        public string? ValueString { get; set; }
        public string? ValueBase64 { get; set; }
    }
}
