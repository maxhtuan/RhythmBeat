using UnityEngine;
using System.Collections.Generic;

public enum GameModeType
{
    Perform,
    Practice,
    SpeedUp
}

public class GameModeManager : MonoBehaviour
{
    [Header("References")]
    public GameplayManager gameplayManager;
    public AudioSource musicSource;

    [Header("Current Mode")]
    public GameModeType currentMode = GameModeType.Perform;

    private IGameMode activeMode;
    private Dictionary<GameModeType, IGameMode> availableModes = new Dictionary<GameModeType, IGameMode>();

    void Start()
    {
        InitializeModes();
        SetMode(currentMode);
    }

    void InitializeModes()
    {
        // Create mode instances
        availableModes[GameModeType.Perform] = new PerformMode(gameplayManager, musicSource);
        availableModes[GameModeType.Practice] = new PracticeMode(gameplayManager);
        availableModes[GameModeType.SpeedUp] = new SpeedUpMode(gameplayManager, musicSource);

        Debug.Log($"GameModeManager: Initialized {availableModes.Count} modes");
    }

    public void SetMode(GameModeType modeType)
    {
        if (!availableModes.ContainsKey(modeType))
        {
            Debug.LogError($"Mode {modeType} not found!");
            return;
        }

        // End current mode
        activeMode?.End();

        // Set new mode
        currentMode = modeType;
        activeMode = availableModes[modeType];

        // Initialize new mode
        activeMode.Initialize();

        Debug.Log($"Switched to {activeMode.ModeName} mode");
    }

    public void StartMode()
    {
        activeMode?.Start();
    }

    public void PauseMode()
    {
        activeMode?.Pause();
    }

    public void ResumeMode()
    {
        activeMode?.Resume();
    }

    public void EndMode()
    {
        activeMode?.End();
    }

    // Add this new method
    public void ReinitializeMode()
    {
        if (activeMode != null)
        {
            activeMode.End();
            activeMode.Initialize();
            Debug.Log($"Reinitialized {activeMode.ModeName} mode");
        }
    }

    public void OnBeatHit()
    {
        activeMode?.OnBeatHit();
    }

    // Context menu for testing
    [ContextMenu("Switch to Perform")]
    public void SwitchToPerform()
    {
        SetMode(GameModeType.Perform);
    }

    [ContextMenu("Switch to Practice")]
    public void SwitchToPractice()
    {
        SetMode(GameModeType.Practice);
    }

    [ContextMenu("Switch to Speed Up")]
    public void SwitchToSpeedUp()
    {
        SetMode(GameModeType.SpeedUp);
    }
}
