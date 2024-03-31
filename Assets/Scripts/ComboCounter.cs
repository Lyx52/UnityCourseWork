using System;
using System.Threading;
using DefaultNamespace.Models;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class ComboCounter : MonoBehaviour
    {
        public const long FullPoints = 10;
        public const long HalfPoints = 5;
        public long CurrentCombo { get; set; }
        public long CurrentPoints { get; set; }
        public long MaxCombo { get; set; }

        private TextMeshProUGUI _textMesh;
        private void Start()
        {
            _textMesh = GetComponent<TextMeshProUGUI>();
        }

        public void UpdateDisplay(HitPointResult result)
        {
            switch (result)
            {
                case HitPointResult.MaxPoints:
                {
                    CurrentCombo += 2;
                    CurrentPoints += (FullPoints * CurrentCombo);
                } break;
                case HitPointResult.HalfPoints:
                {
                    CurrentCombo++;

                    CurrentPoints += (HalfPoints * CurrentCombo);
                } break;
                case HitPointResult.NoPoints:
                {
                    MaxCombo = Math.Max(CurrentCombo, MaxCombo);
                    CurrentCombo = 0;
                } break;
            }

            MaxCombo = Math.Max(CurrentCombo, MaxCombo);
            UpdateText($"x{CurrentCombo}\n{CurrentPoints}");
        }

        private void UpdateText(string text) => _textMesh.text = text;
    }
}