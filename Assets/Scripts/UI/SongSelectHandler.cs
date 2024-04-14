using System.Collections.Generic;
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
    private Dictionary<string, Texture2D> _cachedTextures;
    public int currentlySelectedIdx = -1;
    void Start()
    {
        _buttons = new List<SelectButtonHandler>();
        _songList = OsuMapProvider.GetAvailableMaps();
        _cachedTextures = new Dictionary<string, Texture2D>();
        foreach (var song in _songList)
        {
            if (song.Value.TryLoadBackground(out var background))
            {
                AddMapButton(song.Value, song.Key, background);    
                _cachedTextures.Add(song.Key, background);
                continue;
            }
            
            AddMapButton(song.Value, song.Key, Texture2D.blackTexture);    
            _cachedTextures.Add(song.Key, Texture2D.blackTexture);
        }
    }

    private void AddMapButton(OsuMap map, string key, Texture2D background)
    {
        var button = Instantiate(songSelectButtonPrefab, buttonContentBox);
        var buttonHandler = button.GetComponent<SelectButtonHandler>();
        buttonHandler.Init(map.Title, key, background);
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

    public void Reset()
    {
        DeselectAll();
        gameObject.SetActive(true);
        difficultySelectHandler.HideMenu();
    }
    private void OnMapSelect(string mapKey, bool isSelected)
    {
        if (!_songList.TryGetValue(mapKey, out var map)) return;
        Debug.Log($"Map: {map.Title}, selected: {isSelected}");
        if (isSelected)
        {
            difficultySelectHandler.ShowMenu(map, _cachedTextures[mapKey]);
            OsuMapProvider.SetActiveMap(mapKey);
        }
        else
        {
            difficultySelectHandler.HideMenu();
        }
    }
}
