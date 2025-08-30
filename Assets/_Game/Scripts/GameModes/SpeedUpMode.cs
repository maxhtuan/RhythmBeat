using UnityEngine;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Threading.Tasks;

public class SpeedUpMode : IGameMode
{
    public string ModeName => "Speed Up";

    private GameplayManager gameplayManager;
    private AudioSource musicSource;
    private MetronomeManager metronomeManager; // Add this
    private int beatCount = 0;
    private float originalBPM = 60f;
    private float currentBPM = 60f;

    // Beat tracking
    private float lastBeatTime = 0f;
    private float secondsPerBeat = 1f;
    private bool hasHitFirstNote = false;

    public SpeedUpMode(GameplayManager gameplayManager, AudioSource musicSource)
    {
        this.gameplayManager = gameplayManager;
        this.musicSource = musicSource;

        // Get metronome manager reference
        if (gameplayManager != null)
        {
            this.metronomeManager = gameplayManager.metronomeManager;
        }
    }

    public void Initialize()
    {
        Debug.Log("Speed Up Mode: Initialized");
        beatCount = 0;
        hasHitFirstNote = false;

        // Get the original BPM from GameplayManager
        if (gameplayManager != null)
        {
            originalBPM = gameplayManager.originalBPM;
            currentBPM = originalBPM;
            secondsPerBeat = 60f / originalBPM;
            gameplayManager.SetBPM(originalBPM); // Reset to original BPM
        }

        Debug.Log($"Speed Up Mode: Original BPM = {originalBPM}, Seconds per beat = {secondsPerBeat:F2}");
    }

    public void Start()
    {
        Debug.Log("Speed Up Mode: Started");
        // Don't play background music, only beat sounds
        if (musicSource != null)
        {
            musicSource.Stop();
        }

        // Start metronome for Speed Up mode
        if (metronomeManager != null)
        {
            // Set the GameplayManager reference
            metronomeManager.SetGameplayManager(gameplayManager);

            // Sync metronome BPM from GameplayManager (don't set it directly)
            metronomeManager.SyncBPMFromGameplayManager();
            metronomeManager.StartMetronome();

            // Sync metronome to current game time
            if (gameplayManager != null)
            {
                float currentTime = gameplayManager.GetCurrentTime();
                metronomeManager.SyncToGameTime(currentTime);
            }
        }
    }

    public void Pause()
    {
        Debug.Log("Speed Up Mode: Paused");
        // Pause metronome
        if (metronomeManager != null)
        {
            metronomeManager.StopMetronome();
        }
    }

    public void Resume()
    {
        Debug.Log("Speed Up Mode: Resumed");
        // Resume metronome
        if (metronomeManager != null)
        {
            metronomeManager.StartMetronome();
        }
    }

    public void End()
    {
        Debug.Log("Speed Up Mode: Ended");
        // Stop metronome
        if (metronomeManager != null)
        {
            metronomeManager.StopMetronome();
        }

        // Reset BPM to original
        if (gameplayManager != null)
        {
            gameplayManager.ResetBPM();
        }
        beatCount = 0;
        hasHitFirstNote = false;
    }

    public void OnBeatHit()
    {
        if (gameplayManager == null) return;

        float currentTime = gameplayManager.GetCurrentTime();

        // Only count beats if we've hit the first note and started the game
        if (!hasHitFirstNote)
        {
            hasHitFirstNote = true;
            lastBeatTime = currentTime;
            return;
        }

        // Check if enough time has passed since the last beat
        float timeSinceLastBeat = currentTime - lastBeatTime;
        float currentSecondsPerBeat = 60f / currentBPM;

        // Only count this as a beat if we're close to the expected beat time
        if (timeSinceLastBeat >= currentSecondsPerBeat * 0.8f) // Allow some tolerance
        {
            beatCount++;
            lastBeatTime = currentTime;

            Debug.Log($"Speed Up Mode: Beat {beatCount} hit at {currentTime:F2}s (BPM: {currentBPM})");

            // Get settings from GameplayManager
            float bpmIncrease = 0f; // Default to 0 since you set it to 0
            int beatsBeforeSpeedUp = 3; // Default

            if (gameplayManager.settingsManager != null)
            {
                bpmIncrease = gameplayManager.settingsManager.BPMIncreaseAmount;
                beatsBeforeSpeedUp = gameplayManager.settingsManager.BeatsBeforeSpeedUp;
            }

            // Every N beats, increase BPM by specified amount
            if (beatCount % beatsBeforeSpeedUp == 0 && bpmIncrease > 0)
            {
                currentBPM += bpmIncrease;
                Debug.Log($"Speed Up Mode: BPM increased to {currentBPM}");

                // Only update GameplayManager - metronome will sync from it
                if (gameplayManager != null)
                {
                    gameplayManager.SetBPM(currentBPM);

                    // Sync metronome BPM from GameplayManager
                    if (metronomeManager != null)
                    {
                        metronomeManager.SyncBPMFromGameplayManager();
                    }
                }
            }
        }
        else
        {
            // This is likely a note release or extra hit, not a beat
            Debug.Log($"Ignoring extra hit at {currentTime:F2}s (time since last beat: {timeSinceLastBeat:F2}s, expected: {currentSecondsPerBeat:F2}s)");
        }
    }
}
