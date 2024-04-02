using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using DefaultNamespace.Models;
using UnityEngine;
using UnityEngine.UI;

public class SongSelectHandler : MonoBehaviour
{
    private List<SelectButtonHandler> _buttons;
    public GameObject songSelectButtonPrefab;
    public RectTransform buttonContentBox;
    public DifficultySelectHandler difficultySelectHandler;
    public Dictionary<string, OsuMap> _songList;
    public int currentlySelectedIdx = -1;
    void Start()
    {
        _buttons = new List<SelectButtonHandler>();
        _songList = OsuMapProvider.GetAvailableMaps();
        foreach (var song in _songList)
        {
            AddMapButton(song.Value, song.Key);    
        }
    }

    private void AddMapButton(OsuMap map, string key)
    {
        var button = Instantiate(songSelectButtonPrefab, buttonContentBox);
        var buttonHandler = button.GetComponent<SelectButtonHandler>();
        buttonHandler.Init(map.Title, map.BackgroundImage, key);
        buttonHandler.onSelected += (key, active) =>
        {
            DeselectAll();
            OnMapSelect(key, active);
            buttonHandler.SetBorderActive(active);
        };
        _buttons.Add(buttonHandler);
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonContentBox);
    }

    private void DeselectAll() => _buttons.ForEach(df => df.SetSelected(false));
    private void OnMapSelect(string mapKey, bool isSelected)
    {
        if (!_songList.TryGetValue(mapKey, out var map)) return;
        Debug.Log($"Map: {map.Title}, selected: {isSelected}");
        if (isSelected)
        {
            difficultySelectHandler.ShowMenu(map);
            OsuMapProvider.SetActiveMap(mapKey);
        }
        else
        {
            difficultySelectHandler.HideMenu();
        }
    }
}
