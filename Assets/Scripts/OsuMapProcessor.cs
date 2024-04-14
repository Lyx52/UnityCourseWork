using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DefaultNamespace.Models;
using JetBrains.Annotations;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace DefaultNamespace
{
    public static class OsuMapProcessor
    {
        public static readonly Vector2 OsuScreenSize = new Vector2(640f, 480f);
        private const float MinCircleLifetime = 450;  // In ms
        private const float MaxCircleLifetime = 1250;  // In ms
        private const float MaxOverallDifficulty = 10.0f;
        private const float MinOverallDifficulty = 1.0f;
        private const float MinDistanceBetweenCircles = 1.1f;
        private const float MinZSpeed = 0.06f;
        private const float MaxZSpeed = 0.16f;
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
                        case "[Difficulty]":
                        {
                            ProcessDifficulty(sr, map);    
                        } break;
                    }
                }
            }
            
            return map.PostProcessHitObjects();
        }

        public static OsuMapVersion PostProcessHitObjects(this OsuMapVersion map)
        {
            if (map.HitObjects.Count <= 0) return map;
            map.ZSpeed = GetZSpeed(map.OverallDifficulty);
            var circleLifetime = GetCircleLifetime(map.OverallDifficulty);
            uint idx = 0;
            
            // Setup hit object
            map.HitObjects.ForEach((hitObject) =>
            {
                hitObject.EndTime = hitObject.StartTime + (uint)Math.Floor(circleLifetime);
                hitObject.Lifetime = (uint)Math.Floor(circleLifetime);
                hitObject.Id = ++idx;
                hitObject.X *= 24f;
                hitObject.Y *= 18f; 
            });
            
            // Remove duplicates
            var hitObjects = new List<HitObject>(map.HitObjects);
            for (int i = 0; i < hitObjects.Count; i++)
            {
                var first = hitObjects[i];
                foreach (var other in hitObjects.Skip(Math.Max(0, i - 5)).Take(10))
                {
                    if (other.Id == first.Id) continue;
                    var dist = Math.Abs(first.Distance(other));
                    var timeDiff = Math.Abs(first.StartTime - other.StartTime);
                    if (dist <= MinDistanceBetweenCircles && timeDiff < first.Lifetime)
                    {
                        Debug.Log($"Removed obj with dist {dist} & time {timeDiff}");
                        var removedObj = map.HitObjects.FirstOrDefault(obj => obj.Id == other.Id);
                        if (removedObj is not null) map.HitObjects.Remove(removedObj);
                    }
                }    
                
            }
            
            return map;
        }
        private static void ProcessDifficulty(StreamReader sr, OsuMapVersion map)
        {
            var line = sr.ReadLine();
            while (!sr.EndOfStream && line?.Length > 2)
            {
                var parts = line.Split(':');
                if (parts.Length < 2) break;
                switch (parts[0].Trim())
                {
                    case "OverallDifficulty": 
                        map.OverallDifficulty = float.TryParse(parts[1], NumberStyles.AllowDecimalPoint | NumberStyles.Any, CultureInfo.InvariantCulture, out var difficulty) ? difficulty : 1.0f;
                        break;
                }
                line = sr.ReadLine();
            }      
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
                if (obj is not null)
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
            if (!uint.TryParse(parts[2], out var startTime)) return null;
            if (!uint.TryParse(parts[3], out var objType)) return null;
            return new HitObject()
            {
                // From corner relative to center relative
                X = (float)(x - Math.Floor(OsuScreenSize.x / 2f)) / OsuScreenSize.x,
                Y = (float)(y - Math.Floor(OsuScreenSize.y / 2f)) / OsuScreenSize.y,
                StartTime = startTime,
                Type = objType
            };
        }

        private static float GetCircleLifetime(float difficulty) 
            => MinCircleLifetime + (1 - (difficulty / MaxOverallDifficulty) + (MinOverallDifficulty / MaxOverallDifficulty)) * (MaxCircleLifetime - MinCircleLifetime);

        private static float GetZSpeed(float difficulty) 
            => MinZSpeed + ((difficulty / MaxOverallDifficulty) + (MinOverallDifficulty / MaxOverallDifficulty)) * (MaxZSpeed - MinZSpeed);

        private static float Distance(this HitObject first, HitObject other)
        {
            return Vector2.Distance(first.Position, other.Position);
        }
    }
}