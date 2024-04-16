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
    public GameObject startButton;
    public LeaderboardHandler leaderboardHandler;
    public SongSelectHandler songSelectHandler;
    public ScoreHandler scoreHandler;
    public GameState currentGameState = GameState.LOADING;
    private OsuMapVersion _selectedMapVersion;
    void Start()
    {
        SwitchGameState(GameState.MAIN_MENU);
    }
    public void OnMapSelected(OsuMapVersion mapVersion)
    {
        Debug.Log($"SELECTED MAP: {mapVersion!.Title}, SELECTED VERSION: {mapVersion!.FullTitle}");
        _selectedMapVersion = mapVersion;
        startButton.SetActive(true);
        leaderboardHandler.gameObject.SetActive(true);
        leaderboardHandler.ShowLeaderboard(mapVersion.FullTitle);
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

    public void OnMapFinished()
    {
        Debug.Log("GAME FINISHED!!!");
        var record = scoreHandler.GetCurrentRecord(_selectedMapVersion.FullTitle, _selectedMapVersion.HitObjects.Count);
        leaderboardHandler.AddScore(_selectedMapVersion.FullTitle, record);
        SwitchGameState(GameState.MAIN_MENU);
    }

    public void OnStartGame()
    {
        startButton.SetActive(false);
        leaderboardHandler.gameObject.SetActive(false);
        StartCoroutine(LoadMapAsync(_selectedMapVersion));    
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
