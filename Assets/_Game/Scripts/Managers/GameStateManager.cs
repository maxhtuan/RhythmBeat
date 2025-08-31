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

                var gameplayManager = ServiceLocator.Instance.GetService<GameplayManager>();
                gameplayManager.OnEndGame();

                var gameUIManager = ServiceLocator.Instance.GetService<GameUIManager>();
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

    public void Initialize()
    {
        currentState = GameState.Preparing;
        previousState = GameState.Preparing;
        Debug.Log("GameStateManager initialized with Preparing state");
    }

    public void Cleanup()
    {
        OnGameStateChanged = null;
        OnGameStarted = null;
        OnGameEnded = null;
        Debug.Log("GameStateManager cleaned up");
    }
}
