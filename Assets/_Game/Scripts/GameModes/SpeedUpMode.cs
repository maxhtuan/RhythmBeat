using UnityEngine;

public class SpeedUpMode : IGameMode
{
    public string ModeName => "Speed Up";

    private GameplayManager gameplayManager;
    private AudioSource musicSource;
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
    }

    public void Pause()
    {
        Debug.Log("Speed Up Mode: Paused");
    }

    public void Resume()
    {
        Debug.Log("Speed Up Mode: Resumed");
    }

    public void End()
    {
        Debug.Log("Speed Up Mode: Ended");
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

            // Every 3 beats, increase BPM by 10
            if (beatCount % 3 == 0)
            {
                currentBPM += 10f;
                Debug.Log($"Speed Up Mode: BPM increased to {currentBPM}");

                // Update gameplay speed using the new BPM system
                if (gameplayManager != null)
                {
                    gameplayManager.SetBPM(currentBPM);
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
