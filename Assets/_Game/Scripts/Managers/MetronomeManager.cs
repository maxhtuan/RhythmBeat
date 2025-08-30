using UnityEngine;
using System.Collections;

public class MetronomeManager : MonoBehaviour, IService
{
    [Header("Metronome Settings")]
    public AudioSource metronomeAudioSource;
    public AudioClip beatSound;
    private SongHandler songHandler;

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
        songHandler = ServiceLocator.Instance.GetService<SongHandler>();
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

            // Calculate the next beat time based on the current BPM
            // Don't just add secondsPerBeat, recalculate based on current time
            float beatsElapsed = currentGameTime / secondsPerBeat;
            int nextBeatNumber = Mathf.FloorToInt(beatsElapsed) + 1;
            nextBeatTime = nextBeatNumber * secondsPerBeat;

            Debug.Log($"Metronome beat {beatCount} scheduled for {nextBeatTime:F2}s (BPM: {bpm}, interval: {secondsPerBeat:F2}s)");
        }
    }

    public void StartMetronome()
    {
        if (isPlaying) return;

        isPlaying = true;
        beatCount = 1;

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
            float oldSecondsPerBeat = secondsPerBeat;

            bpm = songHandler.currentBPM;
            secondsPerBeat = 60f / bpm;

            // If the metronome is playing and BPM changed, we need to adjust the timing
            if (isPlaying && Mathf.Abs(oldBPM - bpm) > 0.1f) // Only if BPM actually changed
            {
                float currentGameTime = gameplayManager.GetCurrentTime();

                // Calculate how many beats have elapsed since the start
                float totalBeatsElapsed = currentGameTime / oldSecondsPerBeat;
                int currentBeatNumber = Mathf.FloorToInt(totalBeatsElapsed);

                // Calculate when the next beat should happen using the new BPM
                // Find the time of the last beat that should have happened
                float lastBeatTime = currentBeatNumber * oldSecondsPerBeat;

                // Calculate the next beat time using the new BPM
                nextBeatTime = lastBeatTime + secondsPerBeat;

                // If the next beat time is in the past, advance to the next beat
                while (nextBeatTime <= currentGameTime)
                {
                    nextBeatTime += secondsPerBeat;
                }

                Debug.Log($"Metronome BPM changed from {oldBPM} to {bpm}. Current time: {currentGameTime:F2}s, next beat at {nextBeatTime:F2}s (interval: {secondsPerBeat:F2}s)");
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
            float oldSecondsPerBeat = secondsPerBeat;

            bpm = songHandler.currentBPM;
            secondsPerBeat = 60f / bpm;

            // If the metronome is playing, recalculate timing
            if (isPlaying)
            {
                float currentGameTime = gameplayManager.GetCurrentTime();

                // More aggressive sync: calculate next beat based on current time and new BPM
                // Find how many beats should have happened by now with the new BPM
                float beatsElapsedWithNewBPM = currentGameTime / secondsPerBeat;
                int currentBeatNumber = Mathf.FloorToInt(beatsElapsedWithNewBPM);

                // Calculate the next beat time using the new BPM
                nextBeatTime = (currentBeatNumber + 1) * secondsPerBeat;

                // If we're very close to the next beat (within 0.1 seconds), just advance to it
                float timeToNextBeat = nextBeatTime - currentGameTime;
                if (timeToNextBeat <= 0.1f && timeToNextBeat > 0f)
                {
                    nextBeatTime = currentGameTime + 0.05f; // Schedule beat very soon
                    Debug.Log($"Metronome: Close to beat, advancing. Current: {currentGameTime:F2}s, Next beat at: {nextBeatTime:F2}s");
                }

                Debug.Log($"Metronome synced BPM from {oldBPM} to {bpm}. Current time: {currentGameTime:F2}s, next beat at {nextBeatTime:F2}s (interval: {secondsPerBeat:F2}s)");
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
