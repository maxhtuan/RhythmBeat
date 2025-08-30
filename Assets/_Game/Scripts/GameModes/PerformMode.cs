using UnityEngine;

public class PerformMode : IGameMode
{
    public string ModeName => "Perform";

    private GameplayManager gameplayManager;
    private AudioSource musicSource;

    public PerformMode(GameplayManager gameplayManager, AudioSource musicSource)
    {
        this.gameplayManager = gameplayManager;
        this.musicSource = musicSource;
    }

    public void Initialize()
    {
        Debug.Log("Perform Mode: Initialized");
    }

    public void Start()
    {
        Debug.Log("Perform Mode: Started");
        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.Play();
        }
    }

    public void Pause()
    {
        Debug.Log("Perform Mode: Paused");
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }

    public void Resume()
    {
        Debug.Log("Perform Mode: Resumed");
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    public void End()
    {
        Debug.Log("Perform Mode: Ended");
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void OnBeatHit()
    {
        // Perform mode doesn't need to do anything special on beat hit
    }
}
