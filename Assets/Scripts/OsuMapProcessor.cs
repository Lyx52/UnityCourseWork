using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DefaultNamespace.Models;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace DefaultNamespace
{
    public static class OsuMapProcessor
    {
        public static readonly Vector2 OsuScreenSize = new Vector2(640f, 480f);

        private static uint[] _allowedTypes =
        {
            1, 2
        };
        private static ConcurrentDictionary<string, AudioClip> _loadedClips =
            new ConcurrentDictionary<string, AudioClip>();
        
        public static OsuMapVersion ProcessMapVersion(string filePath)
        {
            var map = new OsuMapVersion();
            map.MetadataFile = filePath;
            map.Location = Path.GetDirectoryName(filePath);
            using (var sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    switch (line)
                    {
                        case "[HitObjects]":
                        {
                            ProcessHitObjects(sr, map);
                        } break;
                        case "[General]":
                        {
                            ProcessGeneralInfo(sr, map);
                        } break;
                        case "[Metadata]":
                        {
                            ProcessMetadata(sr, map);
                        } break;
                        case "[Events]":
                        {
                            ProcessEvents(sr, map);    
                        } break;
                    }
                }
            }
            
            return map.PostProcessHitObjects();
        }

        public static OsuMapVersion PostProcessHitObjects(this OsuMapVersion map)
        {
            if (map.HitObjects.Count <= 0) return map;
            var hitObjects = new List<HitObject> { map.HitObjects.First() };
            const float minDist = 0.01f;
            const long minTimeDiff = 25;
            for (int i = 0; i < map.HitObjects.Count - 1; i++)
            {
                var first = map.HitObjects[i];
                var second = map.HitObjects[i + 1];

                var diffX = Math.Abs(first.X - second.X);
                var diffY = Math.Abs(first.Y - second.Y);
                var diffTime = Math.Abs(first.endedAt - second.endedAt);
                if (diffX > minDist && diffY > minDist && diffTime > minTimeDiff) hitObjects.Add(second);
            }

            map.HitObjects = hitObjects;
            return map;
        }
        private static void ProcessGeneralInfo(StreamReader sr, OsuMapVersion map)
        {
            var line = sr.ReadLine();
            while (!sr.EndOfStream && line?.Length > 2)
            {
                var parts = line.Split(':');
                if (parts.Length < 2) break;
                switch (parts[0].Trim())
                {
                    case "AudioFilename": 
                        map.AudioFile = $"{map.Location}\\{parts[1].Trim()}";
                        break;
                }
                line = sr.ReadLine();
            }      
        }
        
        private static void ProcessEvents(StreamReader sr, OsuMapVersion map)
        {
            var line = sr.ReadLine();
            var currentEventType = string.Empty;
            while (!sr.EndOfStream && !string.IsNullOrEmpty(line))
            {
                if (string.IsNullOrEmpty(line))
                {
                    line = sr.ReadLine();
                    continue;
                }

                if (line!.StartsWith("//"))
                {
                    currentEventType = line[2..].Trim();
                    line = sr.ReadLine();
                    continue;
                }

                if (string.IsNullOrEmpty(currentEventType))
                {
                    line = sr.ReadLine();
                    continue;
                }
                
                switch (currentEventType)
                {
                    case "Background and Video events":
                    {
                        var eventParts = line.Split(',');

                        if (eventParts.Length < 3) break;
                        var fileName = eventParts[2].Replace("\"", string.Empty);
                        if (fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || 
                            fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        {
                            map.BackgroundImage = Path.Join(map.Location, fileName);
                        }
                    } break;
                }
                line = sr.ReadLine();
            }      
        }
        
        private static void ProcessMetadata(StreamReader sr, OsuMapVersion map)
        {
            var line = sr.ReadLine();
            while (!sr.EndOfStream && line?.Length > 2)
            {
                var parts = line.Split(':');
                if (parts.Length < 2) break;
                switch (parts[0].Trim())
                {
                    case "Title": 
                        map.Title = parts[1].Trim();
                        break;
                    case "Version": 
                        map.Version = parts[1].Trim();
                        break;
                    case "Artist": 
                        map.Artist = parts[1].Trim();
                        break;
                }
                line = sr.ReadLine();
            }      
        }
        private static void ProcessHitObjects(StreamReader sr, OsuMapVersion map)
        {
            var line = sr.ReadLine();
            while (!sr.EndOfStream && line?.Length > 2)
            {
                var obj = ParseHitObject(line);
                if (obj is not null && _allowedTypes.Contains(obj.Type))
                {
                    map.HitObjects.Add(obj);    
                }
                
                line = sr.ReadLine();
            }    
        }

        [CanBeNull]
        private static HitObject ParseHitObject(string line)
        {
            var parts = line.Split(',');
            if (parts.Length < 2) return null;
            if (!float.TryParse(parts[0], out var x)) return null;
            if (!float.TryParse(parts[1], out var y)) return null;
            if (!uint.TryParse(parts[2], out var playbackOffset)) return null;
            if (!uint.TryParse(parts[3], out var objType)) return null;
            return new HitObject()
            {
                // From corner relative to center relative
                X = (float)(x - Math.Floor(OsuScreenSize.x / 2f)) / OsuScreenSize.x,
                Y = (float)(y - Math.Floor(OsuScreenSize.y / 2f)) / OsuScreenSize.y,
                endedAt = playbackOffset,
                Type = objType
            };
        }
        
        public static bool TryLoadBackground(this OsuMapVersion mapVersion, [CanBeNull] out Texture2D background)
        {
            background = new Texture2D(2, 2);
            if (mapVersion.BackgroundImage is null) return false;
            var imageData = File.ReadAllBytes(mapVersion.BackgroundImage);
            background.LoadImage(imageData);
            background.filterMode = FilterMode.Trilinear;
            return true;
        }
        public static bool TryGetAudioClip(this OsuMapVersion mapVersion, [CanBeNull] out AudioClip audioClip)
            => _loadedClips.TryGetValue(mapVersion.AudioFile, out audioClip);
        public static IEnumerator LoadClip(this OsuMapVersion mapVersion)
        {
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(mapVersion.AudioFile, AudioType.UNKNOWN))
            {
                uwr.SendWebRequest();
                while (!uwr.isDone) yield return new WaitForSeconds(1);
                try
                {
                    if (uwr.result == UnityWebRequest.Result.ConnectionError) Debug.Log($"{uwr.error}");
                    else
                    {
                        var clip = DownloadHandlerAudioClip.GetContent(uwr);
                        if (!_loadedClips.TryAdd(mapVersion.AudioFile, clip))
                            throw new ApplicationException("Audio clip is already loaded!");
                    }
                }
                catch (Exception err)
                {
                    Debug.Log($"{err.Message}, {err.StackTrace}");
                }
            }
        }
    }
}