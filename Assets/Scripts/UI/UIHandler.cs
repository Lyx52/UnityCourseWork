using System;
using System.Collections;
using DefaultNamespace;
using DefaultNamespace.Models;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    public LoadingAnimator loadingAnimator;
    public GameObject gameUI;
    public CircleSpawner circleSpawner;
    public GameObject mainMenuUI;
    public GameObject pauseMenuUI;
    public SongSelectHandler songSelectHandler;
    public GameState currentGameState = GameState.LOADING;

    void Start()
    {
        SwitchGameState(GameState.MAIN_MENU);
    }
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

    public void OnPauseGame()
    {
        if (currentGameState is GameState.IN_GAME)
        {
            SwitchGameState(GameState.PAUSED);
        } else if (currentGameState is GameState.PAUSED)
        {
            SwitchGameState(GameState.IN_GAME);
        }
    }

    public void OnReturnToMenu()
    {
        SwitchGameState(GameState.MAIN_MENU);
    }
    public void SwitchGameState(GameState state)
    {
        switch (state)
        {
            case GameState.LOADING:
            {
                loadingAnimator.SetActive(false);
                pauseMenuUI.SetActive(false);
                gameUI.SetActive(false);
                mainMenuUI.SetActive(false);
                loadingAnimator.SetActive(true);
                
                currentGameState = state;
            } break;
            case GameState.IN_GAME:
            {
                if (currentGameState is not GameState.PAUSED)
                {
                    circleSpawner.Init();
                }
                circleSpawner.SetPause(false); 
                pauseMenuUI.SetActive(false);    
                loadingAnimator.SetActive(false);   
                gameObject.SetActive(false);
                gameUI.SetActive(true);
                
                currentGameState = state;
            } break;
            case GameState.MAIN_MENU:
            {
                loadingAnimator.SetActive(false);
                gameUI.SetActive(false);
                pauseMenuUI.SetActive(false);
                circleSpawner.Reset();
                gameObject.SetActive(true);
                mainMenuUI.SetActive(true);
                songSelectHandler.Reset();
                SkyboxHandler.ResetSkybox();
                currentGameState = state;
            } break;
            case GameState.PAUSED:
            {
                loadingAnimator.SetActive(false);
                circleSpawner.SetPause(true);
                gameUI.SetActive(false);
                pauseMenuUI.SetActive(true);
                gameObject.SetActive(true);
                currentGameState = state;
            } break;
        }
    }
}
