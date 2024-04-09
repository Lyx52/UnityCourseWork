using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.Models;
using UnityEngine;

namespace DefaultNamespace
{
    public static class SkyboxHandler
    {
        private static Texture2D _defaultTexture = new Texture2D(1, 1);
        public static void UpdateSkyboxTexture(Texture2D background)
        {
            RenderSettings.skybox.SetTexture("_FrontTex", background);    
            RenderSettings.skybox.SetTexture("_BackTex", _defaultTexture);    
            RenderSettings.skybox.SetTexture("_LeftTex", _defaultTexture);   
            RenderSettings.skybox.SetTexture("_RightTex", _defaultTexture);  
            RenderSettings.skybox.SetTexture("_UpTex", _defaultTexture);  
            RenderSettings.skybox.SetTexture("_DownTex", _defaultTexture);
        }
        public static IEnumerator UpdateBackgroundColor(Texture2D texture, int pixelSkip = 8)
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

            int maxCount = 0;
            Color mostCommonColor = Color.black;

            foreach (var entry in colorHistogram)
            {
                if (entry.Value >= maxCount) continue;
                maxCount = entry.Value;
                mostCommonColor = entry.Key;
                yield return null;
            }

            _defaultTexture.SetPixel(0, 0, mostCommonColor);
            _defaultTexture.Apply();
            yield return null;
        }
    }
}