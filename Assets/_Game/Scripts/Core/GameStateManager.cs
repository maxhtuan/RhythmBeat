using UnityEngine;
using System;

public class GameStateManager : MonoBehaviour, IService
{
    private GameState currentState = GameState.Preparing;
    private GameState previousState = GameState.Preparing;

    // Events
    public event Action<GameState> OnGameStateChanged;
    public event Action OnGameStarted;
    public event Action OnGameEnded;

    private GameplayManager gameplayManager;
    private GameplayLogger gameplayLogger;
    private GameModeManager gameModeManager;
    private GameUIManager gameUIManager;

    public void Initialize()
    {

        gameplayManager = ServiceLocator.Instance.GetService<GameplayManager>();
        gameplayLogger = ServiceLocator.Instance.GetService<GameplayLogger>();
        gameModeManager = ServiceLocator.Instance.GetService<GameModeManager>();
        gameUIManager = ServiceLocator.Instance.GetService<GameUIManager>();
        currentState = GameState.Preparing;
        previousState = GameState.Preparing;
        Debug.Log("GameStateManager initialized with Preparing state");
    }

    public void SetGameState(GameState newState)
    {
        if (currentState == newState) return;

        previousState = currentState;
        currentState = newState;

        // Trigger state-specific events
        switch (newState)
        {
            case GameState.Playing:
                OnGameStarted?.Invoke();
                break;

            case GameState.End:
                gameplayManager.OnEndGame();
                gameplayLogger.EndSession();
                gameModeManager.EndMode();
                gameUIManager.OnEndGame();
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

    public bool IsPreparing()
    {
        return currentState == GameState.Preparing;
    }

    public bool IsPlaying()
    {
        return currentState == GameState.Playing;
    }

    public bool IsEnded()
    {
        return currentState == GameState.End;
    }

    public void SubcribeToGameStateChanged(Action<GameState> action)
    {
        OnGameStateChanged += action;
    }

    public void Cleanup()
    {
        OnGameStateChanged = null;
        OnGameStarted = null;
        OnGameEnded = null;
        Debug.Log("GameStateManager cleaned up");
    }
}
