using UnityEngine;
using System;

public class GameStateManager : MonoBehaviour, IService
{
    private GameState currentState = GameState.Menu;
    private GameState previousState = GameState.Menu;

    // Events
    public event Action<GameState> OnGameStateChanged;
    public event Action OnGameStarted;
    public event Action OnGamePaused;
    public event Action OnGameResumed;
    public event Action OnGameEnded;

    public void SetGameState(GameState newState)
    {
        if (currentState == newState) return;

        previousState = currentState;
        currentState = newState;

        // Trigger state-specific events
        switch (newState)
        {
            case GameState.Playing:
                if (previousState == GameState.Menu)
                {
                    OnGameStarted?.Invoke();
                }
                else if (previousState == GameState.Paused)
                {
                    OnGameResumed?.Invoke();
                }
                break;

            case GameState.Paused:
                OnGamePaused?.Invoke();
                break;

            case GameState.GameOver:
                OnGameEnded?.Invoke();
                break;
        }

        // Trigger general state change event
        OnGameStateChanged?.Invoke(newState);

        Debug.Log($"Game state changed from {previousState} to {newState}");
    }

    public GameState GetCurrentState()
    {
        return currentState;
    }

    public GameState GetPreviousState()
    {
        return previousState;
    }

    public bool IsInState(GameState state)
    {
        return currentState == state;
    }

    public bool IsPlaying()
    {
        return currentState == GameState.Playing;
    }

    public bool IsPaused()
    {
        return currentState == GameState.Paused;
    }

    public bool IsGameOver()
    {
        return currentState == GameState.GameOver;
    }

    public bool IsInMenu()
    {
        return currentState == GameState.Menu;
    }

    public void Initialize()
    {
        currentState = GameState.Menu;
        previousState = GameState.Menu;
        Debug.Log("GameStateManager initialized");
    }

    public void Cleanup()
    {
        OnGameStateChanged = null;
        OnGameStarted = null;
        OnGamePaused = null;
        OnGameResumed = null;
        OnGameEnded = null;
        Debug.Log("GameStateManager cleaned up");
    }
}
