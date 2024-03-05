using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace DefaultNamespace
{
    public class HitObject
    {
        public float X { get; set; }
        public float Y { get; set; }
        public uint endedAt { get; set; }
    }
    public class OsuMapProcessor
    {
        private string filePath { get; set; }
        private string beatmapDirectory { get; set; }
        // Idk if this is true...
        private readonly Vector2 osuScreenSize = new Vector2(640f, 480f);
        public Queue<HitObject> _hitObjects;
        private string audioFilePath { get; set; }
        public AudioClip audioClip { get; private set; }
        public OsuMapProcessor(string mapPath, string mapName)
        {
            this.filePath = $"{mapPath}\\{mapName}";
            this.beatmapDirectory = mapPath;
            this._hitObjects = new Queue<HitObject>();
        }

        public void Process()
        {
            using (var sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    switch (line)
                    {
                        case "[HitObjects]":
                        {
                            ProcessHitObjects(sr);
                        } break;
                        case "[General]":
                        {
                            ProcessGeneralInfo(sr);
                        } break;
                    }
                }
            }
        }

        private void ProcessGeneralInfo(StreamReader sr)
        {
            var line = sr.ReadLine();
            while (!sr.EndOfStream && line?.Length > 2)
            {
                var parts = line.Split(':');
                if (parts.Length < 2) break;
                switch (parts[0].Trim())
                {
                    case "AudioFilename": 
                        audioFilePath = $"{beatmapDirectory}\\{parts[1].Trim()}";
                        break;
                }
                line = sr.ReadLine();
            }      
        }
        private void ProcessHitObjects(StreamReader sr)
        {
            var line = sr.ReadLine();
            while (!sr.EndOfStream && line?.Length > 2)
            {
                var obj = ParseHitObject(line);
                if (obj is not null)
                {
                    _hitObjects.Enqueue(obj);    
                }
                
                line = sr.ReadLine();
            }    
        }

        [CanBeNull]
        private HitObject ParseHitObject(string line)
        {
            var parts = line.Split(',');
            if (parts.Length < 2) return null;
            if (!float.TryParse(parts[0], out var x)) return null;
            if (!float.TryParse(parts[1], out var y)) return null;
            if (!uint.TryParse(parts[2], out var playbackOffset)) return null;
            return new HitObject()
            {
                // From corner relative to center relative
                X = (float)(x - Math.Floor(osuScreenSize.x / 2f)) / osuScreenSize.x,
                Y = (float)(y - Math.Floor(osuScreenSize.y / 2f)) / osuScreenSize.y,
                endedAt = playbackOffset
            };
        }
        public IEnumerator LoadClip()
        {
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(audioFilePath, AudioType.UNKNOWN))
            {
                uwr.SendWebRequest();
                while (!uwr.isDone) yield return new WaitForSeconds(5);
                try
                {
                    if (uwr.result == UnityWebRequest.Result.ConnectionError) Debug.Log($"{uwr.error}");
                    else
                    {
                        audioClip = DownloadHandlerAudioClip.GetContent(uwr);
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