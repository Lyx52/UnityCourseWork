using System;
using DefaultNamespace.Models;
using TMPro;
using UnityEngine;

public class ScoreHandler : MonoBehaviour
{
    private const long FullPoints = 10;
    private const long HalfPoints = 5;
    private long CurrentCombo { get; set; }
    private long CurrentPoints { get; set; }
    private long MaxCombo { get; set; }
    private long HitCircles { get; set; }
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI pointsText;
    
    public void UpdateDisplay(HitPointResult result)
    {
        switch (result)
        {
            case HitPointResult.MaxPoints:
            {
                CurrentCombo += 2;
                CurrentPoints += (FullPoints * CurrentCombo);
                HitCircles++;
            } break;
            case HitPointResult.HalfPoints:
            {
                CurrentCombo++;

                CurrentPoints += (HalfPoints * CurrentCombo);
                HitCircles++;
            } break;
            case HitPointResult.NoPoints:
            {
                MaxCombo = Math.Max(CurrentCombo, MaxCombo);
                CurrentCombo = 0;
            } break;
        }

        MaxCombo = Math.Max(CurrentCombo, MaxCombo);
        pointsText.text = $"{CurrentPoints} points";
        comboText.text = $"x{CurrentCombo}";
    }

    public LeaderboardRecord GetCurrentRecord(string mapKey, long totalItems)
    {
        return new LeaderboardRecord()
        {
            Combo = MaxCombo,
            Score = CurrentPoints,
            ItemsHit = HitCircles,
            ItemsTotal = totalItems,
            MapKey = mapKey
        };
    }
}
