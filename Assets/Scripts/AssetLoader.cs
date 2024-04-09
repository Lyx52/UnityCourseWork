using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using DefaultNamespace.Models;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace DefaultNamespace
{
    public static class AssetLoader
    {
        private static ConcurrentDictionary<string, Texture2D> _textureCache = new ConcurrentDictionary<string, Texture2D>();
        private static ConcurrentDictionary<string, AudioClip> _clipCache = new ConcurrentDictionary<string, AudioClip>();
        
        public static bool TryLoadBackground(this OsuMapVersion mapVersion, [CanBeNull] out Texture2D background)
            => mapVersion.BackgroundImage.TryLoadTexture(out background);
        
        public static bool TryLoadBackground(this OsuMap map, [CanBeNull] out Texture2D background)
            => map.BackgroundImage.TryLoadTexture(out background);
        
        private static bool TryLoadTexture([CanBeNull] this string textureLocation, [CanBeNull] out Texture2D background)
        {
            background = new Texture2D(2, 2);
            if (textureLocation is null) return false;
            if (_textureCache.TryGetValue(textureLocation, out background)) return true;
            background = new Texture2D(2, 2);
            var imageData = File.ReadAllBytes(textureLocation);
            background.LoadImage(imageData);
            background.filterMode = FilterMode.Trilinear;
            return _textureCache.TryAdd(textureLocation, background);
        }
        
        public static bool TryGetAudioClip(this OsuMapVersion mapVersion, [CanBeNull] out AudioClip audioClip)
            => _clipCache.TryGetValue(mapVersion.AudioFile, out audioClip);
        public static IEnumerator LoadAudioClip(string location)
        {
            if (_clipCache.ContainsKey(location)) yield break;
            using UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(location, AudioType.UNKNOWN);
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError) {
                Debug.LogError($"{uwr.error}");
            } else {
                var clip = DownloadHandlerAudioClip.GetContent(uwr);
                _clipCache.TryAdd(location, clip);
            }
        }
    }
}