using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultNamespace.Models;
using JetBrains.Annotations;
using UnityEngine;

namespace DefaultNamespace
{
    public static class OsuMapProvider
    {
        public static string OsuMapDirectory => Path.Join(Directory.GetCurrentDirectory(), "maps");
        private static Dictionary<string, OsuMap> _maps = new Dictionary<string, OsuMap>();
        [CanBeNull] public static OsuMap ActiveMap { get; set; }
        [CanBeNull] public static OsuMapVersion ActiveMapVersion  { get; set; }

        public static void SetActiveMap(string mapKey)
        {
            if (!_maps.ContainsKey(mapKey))
                throw new UnityException($"Map with key {mapKey} does not exist");
            ActiveMap = _maps[mapKey];
            ActiveMapVersion = null;
        } 

        public static void SetActiveMapVersion(string mapVersionKey)
        {
            if (ActiveMap is null) 
                throw new UnityException("Set ActiveMap before Map version");
            
            if (!ActiveMap.Versions.ContainsKey(mapVersionKey))
                throw new UnityException($"Map {ActiveMap.Title} version with key {mapVersionKey} does not exist");
            ActiveMapVersion = ActiveMap!.Versions[mapVersionKey]!;
        }
        public static Dictionary<string, OsuMap> GetAvailableMaps()
        {
            RefreshMapList();
            return _maps;
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