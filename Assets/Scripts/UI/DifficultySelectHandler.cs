using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using DefaultNamespace.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Management;

public class DifficultySelectHandler : MonoBehaviour
{
    private List<SelectButtonHandler> _buttons;
    public GameObject selectButtonPrefab;
    public RectTransform buttonContentBox;
    public GameObject menuXRSetup;
    private Dictionary<string, OsuMapVersion> _mapVersions;
    public void ShowMenu(OsuMap map)
    {
        Cleanup();
        _mapVersions = new Dictionary<string, OsuMapVersion>(map.Versions);
        _buttons = new List<SelectButtonHandler>();
        foreach (var mapVersion in _mapVersions)
        {
            AddDifficultyButton(mapVersion.Value, mapVersion.Key);
        }
        gameObject.SetActive(true);
    }
    
    public void HideMenu()
    {
        // TODO: Cleanup
        gameObject.SetActive(false);
        Cleanup();
    }

    private void Cleanup()
    {
        if (_buttons is not null)
        {
            foreach(var button in _buttons) 
                Destroy(button.gameObject);   
            _buttons.Clear();
        }
        _mapVersions?.Clear();    
    }
    
    private void AddDifficultyButton(OsuMapVersion version, string key)
    {
        var button = Instantiate(selectButtonPrefab, buttonContentBox);
        var buttonHandler = button.GetComponent<SelectButtonHandler>();
        buttonHandler.Init(version.FullTitle, version.BackgroundImage, key);
        buttonHandler.onSelected += OnDifficultySelect;
        _buttons.Add(buttonHandler);
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonContentBox);
    }

    private void OnDifficultySelect(string mapVersionKey, bool isSelected)
    {
        if (!_mapVersions.TryGetValue(mapVersionKey, out var version)) return;
        Debug.Log($"Osu map version: {version.Title}, selected: {isSelected}");
        OsuMapProvider.SetActiveMapVersion(mapVersionKey);
        Debug.Log($"SELECTED MAP: {OsuMapProvider.ActiveMap!.Title}, SELECTED VERSION: {OsuMapProvider.ActiveMapVersion!.FullTitle}");
        StartCoroutine(LoadMainSceneAsync());
    }
    private IEnumerator LoadMainSceneAsync()
    {
        yield return SceneManager.LoadSceneAsync("Scenes/MainScene", LoadSceneMode.Single);
    }
}
