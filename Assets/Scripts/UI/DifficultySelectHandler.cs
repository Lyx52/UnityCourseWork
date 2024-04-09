using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using DefaultNamespace.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DifficultySelectHandler : MonoBehaviour
{
    private Dictionary<string, List<SelectButtonHandler>> _buttonGroups;
    public GameObject selectButtonPrefab;
    public RectTransform buttonContentBox;
    private OsuMap _currentMap;
    public UnityEvent<OsuMapVersion> onMapSelected;

    private void Start()
    {
        
    }

    public void ShowMenu(OsuMap map, Texture2D background)
    {
        Cleanup();
        _currentMap = map;
        _buttonGroups ??= new Dictionary<string, List<SelectButtonHandler>>();
        
        if (!_buttonGroups.ContainsKey(map.Title))
        {
            var buttons = map.Versions
                .Select(kvp => BuildDifficultyButton(kvp.Value, kvp.Key, background))
                .ToList();
            _buttonGroups.Add(map.Title, buttons);
        }
        
        ActivateButtons(map.Title);
        gameObject.SetActive(true);
    }

    public void HideMenu() => gameObject.SetActive(false);

    private void ActivateButtons(string mapKey)
    {
        foreach (var button in _buttonGroups[mapKey])
        {
            button.gameObject.SetActive(true);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonContentBox);
    }
    private void Cleanup()
    {
        _currentMap = null;
        if (_buttonGroups is not null)
        {
            foreach (var button in _buttonGroups.SelectMany(group => group.Value))
                button.gameObject.SetActive(false);
        }
    }
    
    private SelectButtonHandler BuildDifficultyButton(OsuMapVersion version, string key, Texture2D background)
    {
        var button = Instantiate(selectButtonPrefab, buttonContentBox);
        var buttonHandler = button.GetComponent<SelectButtonHandler>();
        buttonHandler.Init(version.FullTitle, key, background);
        buttonHandler.onSelected += OnDifficultySelect;
        return buttonHandler;
    }

    private void OnDifficultySelect(string mapVersionKey, bool isSelected)
    {
        OsuMapProvider.SetActiveMapVersion(mapVersionKey);
        onMapSelected.Invoke(OsuMapProvider.ActiveMapVersion);
    }
}
