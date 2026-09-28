using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Xenvious.Logging;

namespace Xenvious
{
    /// <summary>
    /// Favourites and recently used models of the catalog, per kind ("prop", "actor"), kept in
    /// config.ini as hash lists. Props start with the hand-picked Special props as favourites
    /// (the UFOs and lights that used to be a premium category) until the list is changed.
    /// </summary>
    public static class ModelCatalogStore
    {
        private const string Section = "MODELCATALOG";
        private const int RecentMax = 12;

        private static readonly Dictionary<string, HashSet<uint>> Favorites = new Dictionary<string, HashSet<uint>>();
        private static readonly Dictionary<string, List<uint>> Recent = new Dictionary<string, List<uint>>();

        public static bool IsFavorite(string kind, uint hash) => FavoritesOf(kind).Contains(hash);

        public static void SetFavorite(string kind, uint hash, bool favorite)
        {
            var set = FavoritesOf(kind);
            if (favorite ? set.Add(hash) : set.Remove(hash))
                Write(kind + "_favorites", set);
        }

        public static IReadOnlyCollection<uint> FavoriteHashes(string kind) => FavoritesOf(kind);

        public static IReadOnlyList<uint> RecentHashes(string kind) => RecentOf(kind);

        public static void AddRecent(string kind, uint hash)
        {
            var list = RecentOf(kind);
            list.Remove(hash);
            list.Insert(0, hash);
            if (list.Count > RecentMax)
                list.RemoveRange(RecentMax, list.Count - RecentMax);
            Write(kind + "_recent", list);
        }

        private static HashSet<uint> FavoritesOf(string kind)
        {
            if (!Favorites.TryGetValue(kind, out var set))
            {
                var stored = Read(kind + "_favorites");
                set = stored != null
                    ? new HashSet<uint>(stored)
                    : kind == "prop" ? new HashSet<uint>(StaticData.SpecialProps.Select(p => p.UInt)) : new HashSet<uint>();
                Favorites[kind] = set;
            }
            return set;
        }

        private static List<uint> RecentOf(string kind)
        {
            if (!Recent.TryGetValue(kind, out var list))
                Recent[kind] = list = Read(kind + "_recent") ?? new List<uint>();
            return list;
        }

        // null when the key was never written (so the defaults apply), an empty list when it was cleared.
        private static List<uint> Read(string key)
        {
            try
            {
                string path = Functions.getRoamingConfigFilePath();
                if (!File.Exists(path))
                    return null;
                string value = new ini_reader(path).ReadString(Section, key);
                if (string.IsNullOrEmpty(value))
                    return null;
                if (value == "-")
                    return new List<uint>();
                return value.Split(',')
                    .Select(v => uint.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var hash) ? hash : 0u)
                    .Where(h => h != 0).ToList();
            }
            catch (Exception ex)
            {
                Log.Debug("catalog store: " + ex.Message, source: "catalog");
                return null;
            }
        }

        private static void Write(string key, IEnumerable<uint> hashes)
        {
            try
            {
                string value = string.Join(",", hashes.Select(h => h.ToString(CultureInfo.InvariantCulture)));
                new ini_reader(Functions.getRoamingConfigFilePath()).Write(Section, key, value.Length == 0 ? "-" : value);
            }
            catch (Exception ex)
            {
                Log.Warn("catalog store: not saved: " + ex.Message);
            }
        }
    }
}
