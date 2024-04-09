using System;
using System.Collections;
using DefaultNamespace;
using DefaultNamespace.Models;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    public LoadingAnimator loadingAnimator;
    public GameObject gameUI;
    public GameObject circleSpawner;
    public GameObject mainMenuUI;
    public GameState currentGameState = GameState.LOADING;
    
    public void OnMapSelected(OsuMapVersion mapVersion)
    {
        Debug.Log($"SELECTED MAP: {mapVersion!.Title}, SELECTED VERSION: {mapVersion!.FullTitle}");
        StartCoroutine(LoadMapAsync(mapVersion));
    }

    private IEnumerator LoadMapAsync(OsuMapVersion mapVersion)
    {
        SwitchGameState(GameState.LOADING);
        yield return StartCoroutine(AssetLoader.LoadAudioClip(mapVersion.AudioFile));
        Texture2D background;
        while (!mapVersion.TryLoadBackground(out background))
        {
            yield return null;
        }
        yield return StartCoroutine(SkyboxHandler.UpdateBackgroundColor(background));
        SkyboxHandler.UpdateSkyboxTexture(background);
        SwitchGameState(GameState.IN_GAME);
    }
    public void SwitchGameState(GameState state)
    {
        currentGameState = state;
        switch (state)
        {
            case GameState.LOADING:
            {
                loadingAnimator.SetActive(false);
                gameUI.SetActive(false);
                mainMenuUI.SetActive(false);
                loadingAnimator.SetActive(true);
            } break;
            case GameState.IN_GAME:
            {
                loadingAnimator.SetActive(false);   
                gameObject.SetActive(false);
                gameUI.SetActive(true);
                circleSpawner.SetActive(true);
            } break;
            case GameState.MAIN_MENU:
            {
                loadingAnimator.SetActive(false);
                gameUI.SetActive(false);
                circleSpawner.SetActive(false);
                gameObject.SetActive(true);
            } break;
        }
    }
}
