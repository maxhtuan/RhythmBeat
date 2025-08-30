using UnityEngine;

public class PracticeMode : IGameMode
{
    public string ModeName => "Practice";

    private GameplayManager gameplayManager;

    public PracticeMode(GameplayManager gameplayManager)
    {
        this.gameplayManager = gameplayManager;
    }

    public void Initialize()
    {
        Debug.Log("Practice Mode: Initialized");
        // Set slower note speed for practice
        if (gameplayManager != null)
        {
            gameplayManager.noteTravelTime *= 1.5f; // 50% slower
        }
    }

    public void Start()
    {
        Debug.Log("Practice Mode: Started");
        // No background music in practice mode
    }

    public void Pause()
    {
        Debug.Log("Practice Mode: Paused");
    }

    public void Resume()
    {
        Debug.Log("Practice Mode: Resumed");
    }

    public void End()
    {
        Debug.Log("Practice Mode: Ended");
    }

    public void OnBeatHit()
    {
        // Practice mode doesn't need to do anything special on beat hit
    }
}
