using System;
using System.Text;

namespace Xenvious
{
    /// <summary>
    /// The JSON values Xenvious keeps in config.ini (kill values, menu switcher presets,
    /// global and local freeze lists). The ini goes through the ANSI profile API, which
    /// would mangle non-ASCII text such as preset names, so the JSON is stored as Base64
    /// of its UTF-8 bytes behind a "b64:" prefix.
    ///
    /// Versions up to 2.71 stored these values AES-encrypted with a key in the code; that
    /// hid nothing, since the key ships with the exe. They are still read, and written
    /// in the new form on the next save.
    /// </summary>
    public static class ConfigText
    {
        private const string Prefix = "b64:";

        public static string Encode(string json) =>
            Prefix + Convert.ToBase64String(Encoding.UTF8.GetBytes(json ?? ""));

        public static string Decode(string stored)
        {
            if (string.IsNullOrWhiteSpace(stored))
                return stored;
            if (stored.StartsWith(Prefix, StringComparison.Ordinal))
                return Encoding.UTF8.GetString(Convert.FromBase64String(stored.Substring(Prefix.Length)));
            return AES.Decrypt(stored);
        }
    }
}
