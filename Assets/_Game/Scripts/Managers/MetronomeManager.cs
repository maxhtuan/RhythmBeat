using UnityEngine;
using System.Collections;

public class MetronomeManager : MonoBehaviour, IService
{
    [Header("Metronome Settings")]
    public AudioSource metronomeAudioSource;
    public AudioClip beatSound;

    [Header("Timing")]
    public float bpm = 60f;
    public bool isPlaying = false;

    [Header("Visual Feedback")]
    public bool enableVisualFeedback = true;
    public GameObject metronomeVisual;

    private float secondsPerBeat;
    private float nextBeatTime;
    private int beatCount = 0;
    private GameplayManager gameplayManager;
    private bool isInitialized = false;

    // Keep the original Start() method but make it call Initialize()
    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (isInitialized) return;
        gameplayManager = ServiceLocator.Instance.GetService<GameplayManager>();

        // Calculate seconds per beat
        secondsPerBeat = 60f / bpm;
        nextBeatTime = 0f;

        // Setup audio source if not assigned
        if (metronomeAudioSource == null)
        {
            metronomeAudioSource = GetComponent<AudioSource>();
            if (metronomeAudioSource == null)
            {
                metronomeAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Set audio source properties for metronome
        metronomeAudioSource.clip = beatSound;
        metronomeAudioSource.playOnAwake = false;
        metronomeAudioSource.loop = false;
        metronomeAudioSource.volume = 0.5f;

        isInitialized = true;
        Debug.Log("MetronomeManager: Initialized");
    }

    void Update()
    {
        if (!isPlaying) return;

        float currentGameTime = gameplayManager != null ? gameplayManager.GetCurrentTime() : 0f;

        // Check if it's time for the next beat
        if (currentGameTime >= nextBeatTime)
        {
            PlayBeat();
            beatCount++;
            nextBeatTime += secondsPerBeat; // Always add exactly one beat interval

            Debug.Log($"Metronome beat {beatCount} scheduled for {nextBeatTime:F2}s");
        }
    }

    public void StartMetronome()
    {
        if (isPlaying) return;

        isPlaying = true;
        beatCount = 0;

        float currentGameTime = gameplayManager != null ? gameplayManager.GetCurrentTime() : 0f;

        // Start the first beat exactly at the next whole second
        float timeToNextWholeSecond = 1f - (currentGameTime % 1f);
        nextBeatTime = currentGameTime + timeToNextWholeSecond;

        Debug.Log($"Metronome started at {bpm} BPM. First beat at {nextBeatTime:F2}s (in {timeToNextWholeSecond:F2}s)");
    }

    public void StopMetronome()
    {
        isPlaying = false;
        Debug.Log("Metronome stopped");
    }

    public void SetBPM(float newBPM)
    {
        // Don't set BPM directly - get it from GameplayManager
        if (gameplayManager != null)
        {
            float oldBPM = bpm;
            bpm = gameplayManager.currentBPM;
            float oldSecondsPerBeat = secondsPerBeat;
            secondsPerBeat = 60f / bpm;

            // If the metronome is playing and BPM changed, we need to adjust the timing
            if (isPlaying && Mathf.Abs(oldBPM - bpm) > 0.1f) // Only if BPM actually changed
            {
                float currentGameTime = gameplayManager.GetCurrentTime();

                // Calculate how many beats have elapsed since the start
                float totalBeatsElapsed = currentGameTime / oldSecondsPerBeat;
                int currentBeatNumber = Mathf.FloorToInt(totalBeatsElapsed);

                // Calculate when the next beat should happen using the new BPM
                float timeSinceLastBeat = currentGameTime % oldSecondsPerBeat;
                float newTimeSinceLastBeat = timeSinceLastBeat * (oldSecondsPerBeat / secondsPerBeat);

                // Set next beat time based on the new BPM
                nextBeatTime = currentGameTime + (secondsPerBeat - newTimeSinceLastBeat);

                Debug.Log($"Metronome BPM changed from {oldBPM} to {bpm}. Adjusted timing: next beat at {nextBeatTime:F2}s (interval: {secondsPerBeat:F2}s)");
            }
            else
            {
                Debug.Log($"Metronome BPM updated to {bpm} from GameplayManager (beat every {secondsPerBeat:F2} seconds)");
            }
        }
    }

    // Add this method to sync with GameplayManager's BPM
    public void SyncBPMFromGameplayManager()
    {
        if (gameplayManager != null)
        {
            float oldBPM = bpm;
            bpm = gameplayManager.currentBPM;
            secondsPerBeat = 60f / bpm;

            // If the metronome is playing, recalculate timing
            if (isPlaying)
            {
                float currentGameTime = gameplayManager.GetCurrentTime();

                // Recalculate next beat time based on current time and new BPM
                float timeSinceLastBeat = currentGameTime % secondsPerBeat;
                nextBeatTime = currentGameTime + (secondsPerBeat - timeSinceLastBeat);

                Debug.Log($"Metronome synced BPM to {bpm} from GameplayManager. Next beat at {nextBeatTime:F2}s");
            }
            else
            {
                Debug.Log($"Metronome synced BPM to {bpm} from GameplayManager");
            }
        }
    }

    private void PlayBeat()
    {
        if (metronomeAudioSource != null && beatSound != null)
        {
            metronomeAudioSource.PlayOneShot(beatSound);
        }

        if (enableVisualFeedback && metronomeVisual != null)
        {
            StartCoroutine(VisualBeat());
        }

        float currentGameTime = gameplayManager != null ? gameplayManager.GetCurrentTime() : 0f;
        Debug.Log($"Metronome beat {beatCount} played at {currentGameTime:F2}s");
    }

    private IEnumerator VisualBeat()
    {
        Vector3 originalScale = metronomeVisual.transform.localScale;
        metronomeVisual.transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(0.1f);
        metronomeVisual.transform.localScale = originalScale;
    }

    public void SyncToGameTime(float gameTime)
    {
        if (!isPlaying) return;

        float beatsElapsed = gameTime / secondsPerBeat;
        beatCount = Mathf.FloorToInt(beatsElapsed);
        nextBeatTime = gameTime + (secondsPerBeat - (gameTime % secondsPerBeat));

        Debug.Log($"Metronome synced to game time {gameTime:F2}s, beat {beatCount}, next beat at {nextBeatTime:F2}s");
    }

    public int GetCurrentBeat()
    {
        return beatCount;
    }

    public bool IsNearBeat(float tolerance = 0.1f)
    {
        float currentGameTime = gameplayManager != null ? gameplayManager.GetCurrentTime() : 0f;
        float timeToNextBeat = nextBeatTime - currentGameTime;
        return timeToNextBeat <= tolerance;
    }

    public void Cleanup()
    {
        isPlaying = false;
        beatCount = 0;
        nextBeatTime = 0f;
        isInitialized = false;
        Debug.Log("MetronomeManager cleaned up");
    }
}
