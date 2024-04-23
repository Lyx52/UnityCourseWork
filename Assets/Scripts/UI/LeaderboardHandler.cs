using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultNamespace.Models;
using JetBrains.Annotations;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardHandler : MonoBehaviour
{
    public GameObject leaderboardItemPrefab;
    public RectTransform leaderboardContentPanel;
    private List<GameObject> leaderboardItems;
    [CanBeNull] private static Dictionary<string, List<LeaderboardRecord>> _leaderboardRecords;
    private static string localFolder = Path.Join(Directory.GetCurrentDirectory(), "LocalState");
    private static string LeaderboardFileLocation => Path.Join(localFolder, "leaderboard.json");
    
    private static Dictionary<string, List<LeaderboardRecord>> LeaderboardRecords
    {
        get
        {
            if (_leaderboardRecords is not null) return _leaderboardRecords;
            if (!File.Exists(LeaderboardFileLocation))
            {
                _leaderboardRecords = new Dictionary<string, List<LeaderboardRecord>>();
                return _leaderboardRecords;
            }

            try
            {
                if (!Directory.Exists(localFolder)) Directory.CreateDirectory(localFolder);
                using var fs = File.OpenRead(LeaderboardFileLocation);
                using var reader = new StreamReader(fs);
                _leaderboardRecords = JsonConvert.DeserializeObject<Dictionary<string, List<LeaderboardRecord>>>(reader.ReadToEnd());
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize leaderboard json {e.Message}");
                _leaderboardRecords = new Dictionary<string, List<LeaderboardRecord>>();
            }
            return _leaderboardRecords;
        }
    }
    public void AddScore(string mapKey, LeaderboardRecord record)
    {
        if (LeaderboardRecords.TryGetValue(mapKey, out var recordList))
        {
            recordList.Add(record);
        }
        else
        {
            recordList = new List<LeaderboardRecord>()
            {
                record
            };
            LeaderboardRecords.Add(mapKey, recordList);
        }

        SaveLeaderboard();
    }

    public void ShowLeaderboard(string mapKey)
    {
        Cleanup();
        var records = GetMapLeaderboard(mapKey);
        leaderboardItems ??= new List<GameObject>();
        int idx = 0;
        foreach (var record in records.OrderByDescending(r => r.Score))
        {
            CreateRecordItem(record, ++idx);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(leaderboardContentPanel);
    }

    private void CreateRecordItem(LeaderboardRecord record, int place)
    {
        var item = Instantiate(leaderboardItemPrefab, leaderboardContentPanel);
        var textComponent = item.GetComponentInChildren<TextMeshProUGUI>();
        textComponent.text = $"{place}. {record.Score} - {record.Combo} - {record.ItemsHit}/{record.ItemsTotal}";
        
        leaderboardItems.Add(item);
    }

    public void Cleanup() => leaderboardItems?.ForEach(Destroy);
    public List<LeaderboardRecord> GetMapLeaderboard(string mapKey)
    {
        if (LeaderboardRecords.TryGetValue(mapKey, out var records)) return records;
        records = new List<LeaderboardRecord>();
        return records;
    }
    
    public void SaveLeaderboard()
    {
        if (_leaderboardRecords is null) return;
        
        using var fs = File.OpenWrite(LeaderboardFileLocation);
        using var writer = new StreamWriter(fs);
        var json = JsonConvert.SerializeObject(_leaderboardRecords, Formatting.Indented);
        writer.Write(json);
        writer.Flush();
        fs.Flush();
        writer.Close();
        fs.Close();
    }
}
