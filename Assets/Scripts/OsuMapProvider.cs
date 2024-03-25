using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultNamespace.Models;
using UnityEngine;

namespace DefaultNamespace
{
    public static class OsuMapProvider
    {
        public static string OsuMapDirectory => Path.Join(Directory.GetCurrentDirectory(), "maps");
        private static Dictionary<string, OsuMap> _maps = new Dictionary<string, OsuMap>();

        public static List<OsuMap> GetAvailableMaps()
        {
            RefreshMapList();
            return _maps.Values.ToList();
        }
        
        private static void RefreshMapList()
        {
            if (!Directory.Exists(OsuMapDirectory))
            {
                Directory.CreateDirectory(OsuMapDirectory);
                return;
            }

            foreach (var mapDirectory in Directory.GetDirectories(OsuMapDirectory))
            {
                var title = Path.GetFileName(mapDirectory);
                if (_maps.ContainsKey(title)) continue;
                var files = Directory.GetFiles(mapDirectory);
                var osuFiles = files
                    .Where(f => f.EndsWith(".osu"))
                    .Select(OsuMapProcessor.ProcessMapVersion)
                    .Where(om => om.HitObjects.Count > 0);
                _maps.Add(title, new OsuMap()
                {
                    Versions = osuFiles.ToDictionary(of => of.FullTitle),
                    Title = title,
                    Location = mapDirectory
                });
                Debug.Log($"Added {title} map...");
            }
            Console.WriteLine();
        }
    }
}