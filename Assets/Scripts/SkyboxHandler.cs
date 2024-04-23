using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace.Models;
using UnityEngine;

namespace DefaultNamespace
{
    public static class SkyboxHandler
    {
        private static Texture2D _defaultTexture = new Texture2D(1, 1);
        public static void ResetSkybox() => UpdateSkyboxTexture(Texture2D.blackTexture);
        public static void UpdateSkyboxTexture(Texture2D background)
        {
            RenderSettings.skybox.SetTexture("_FrontTex", background);    
            RenderSettings.skybox.SetTexture("_BackTex", _defaultTexture);    
            RenderSettings.skybox.SetTexture("_LeftTex", _defaultTexture);   
            RenderSettings.skybox.SetTexture("_RightTex", _defaultTexture);  
            RenderSettings.skybox.SetTexture("_UpTex", _defaultTexture);  
            RenderSettings.skybox.SetTexture("_DownTex", _defaultTexture);
        }
        public static IEnumerator UpdateBackgroundColor(Texture2D texture, int pixelSkip = 4)
        {
            var colorHistogram = new Dictionary<Color, int>();

            for (int x = 0; x < texture.width; x += pixelSkip)
            {
                for (int y = 0; y < texture.height; y += pixelSkip)
                {
                    Color color = texture.GetPixel(x, y);

                    if (!colorHistogram.TryAdd(color, 1))
                    {
                        colorHistogram[color]++;
                    }
                }

                yield return null;
            }

            int maxCount = colorHistogram.Values.Max();
            var mostCommon = colorHistogram.FirstOrDefault(kvp => kvp.Value == maxCount);
    
            _defaultTexture.SetPixel(0, 0, mostCommon.Key);
            _defaultTexture.Apply();
            yield return null;
        }
    }
}