using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class PerformMode : IGameMode
{
    public string ModeName => "Perform";

    private GameplayManager gameplayManager;
    private AudioSource musicSource;
    private MetronomeManager metronomeManager; // Add this
    private GameSettingsManager settingsManager;
    private SongHandler songHandler;

    public PerformMode(GameplayManager gameplayManager, AudioSource musicSource)
    {
        this.gameplayManager = gameplayManager;
        this.musicSource = musicSource;
    }
    bool isFirstHit = true;

    public void Initialize()
    {
        this.metronomeManager = ServiceLocator.Instance.GetService<MetronomeManager>();
        this.gameplayManager = ServiceLocator.Instance.GetService<GameplayManager>();
        this.settingsManager = ServiceLocator.Instance.GetService<GameSettingsManager>();
        this.songHandler = ServiceLocator.Instance.GetService<SongHandler>();
        isFirstHit = true;


        gameplayManager.SetupGameAsync();

        Debug.Log("Perform Mode: Initialized");
    }

    public void TriggerThisMode()
    {
        isFirstHit = true;
        Debug.Log("Perform Mode: Started");
        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.Play();
        }


        // Start metronome for Speed Up mode
        if (metronomeManager != null)
        {
            // Sync metronome BPM from GameplayManager (don't set it directly)
            metronomeManager.SyncBPMFromGameplayManager();
            metronomeManager.StartMetronome();

            // Sync metronome to current game time
            if (gameplayManager != null)
            {
                float currentTime = gameplayManager.GetCurrentTime();
                metronomeManager.SyncToGameTime(currentTime);
            }
            metronomeManager.MuteMetronome();
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

    private bool hasHitFirstNote = false;
    public void End()
    {
        hitCount = 0;
        hasHitFirstNote = false;
        currentPatternStartPosition = 0;
        Debug.Log("Perform Mode: Ended");
        if (musicSource != null)
        {
            musicSource.Stop();
        }
        metronomeManager.MuteMetronome();

    }

    public Task OnBeatHit()
    {
        if (gameplayManager == null) return Task.CompletedTask;

        Debug.Log("Speed Up Mode: Beat Hit");

        if (isFirstHit)
        {
            isFirstHit = false;
            Debug.Log("Speed Up Mode: First Hit");
            return Task.CompletedTask;
        }

        // Get current note position from NoteManager
        var noteManager = ServiceLocator.Instance.GetService<NoteManager>();
        if (noteManager != null)
        {
            var currentNote = GetCurrentNoteFromPosition();
            if (currentNote != null)
            {
                // Check if this is the start of a new pattern
                if (IsPatternStart(currentNote.notePosition))
                {
                    Debug.Log($"Speed Up Mode: New Pattern Started at position {currentNote.notePosition}");
                    currentPatternStartPosition = currentNote.notePosition;
                    hitCount = 0; // Reset hit count for new pattern
                }


            }
        }

        // Count this note hit
        hitCount++;
        Debug.Log("Speed Up Mode: Hit Count: " + hitCount);

        // Check if we've completed a full pattern (6 notes)
        if (hitCount >= 3)
        {
            Debug.Log($"Speed Up Mode: Pattern complete detected! Starting coroutine. Hit count: {hitCount}");
            gameplayManager.StartCoroutine(OnPatternComplete());
        }
        return Task.CompletedTask;
    }

    IEnumerator OnPatternComplete()
    {
        Debug.Log("Perform Mode: OnPatternComplete");
        // Wait exactly 1 beat duration based on current BPM
        float beatDuration = 60f / songHandler.GetCurrentBPM();
        int delayMs = Mathf.RoundToInt(beatDuration);
        delayMs = Mathf.Max(delayMs, 1);
        yield return new WaitForSeconds(delayMs);
        Debug.Log("Speed Up Mode: After delay");


        metronomeManager.UnmuteMetronome();
    }

    private int currentPatternStartPosition = 0; // Track which pattern we're in
    int hitCount = 0;

    public void OnNoteMissed()
    {

        Debug.Log("Perform Mode: OnNoteMissed");
        metronomeManager.MuteMetronome();

        // Perform mode doesn't need to do anything special on note missed
    }
    // Get the current note based on position
    private NoteData GetCurrentNoteFromPosition()
    {
        var noteManager = ServiceLocator.Instance.GetService<NoteManager>();
        if (noteManager != null)
        {
            var notes = noteManager.GetAllNotes();
            var gameplayManager = ServiceLocator.Instance.GetService<GameplayManager>();
            if (gameplayManager != null)
            {
                float currentTime = gameplayManager.GetCurrentTime();
                float beatDuration = 60f / songHandler.GetCurrentBPM();
                float currentPosition = currentTime / beatDuration;

                // Find the note closest to current position
                foreach (var note in notes)
                {
                    if (Mathf.Abs(note.notePosition - currentPosition) < 0.5f)
                    {
                        return note;
                    }
                }
            }
        }
        return null;
    }

    // Check if this position is the start of a pattern
    private bool IsPatternStart(int notePosition)
    {
        // Pattern starts at positions: 1, 5, 13, 21, 29, 37, 45, 53, 61, 69
        // (Each pattern is 8 positions: 4 initial + 8 repeated + 4 gap)
        return
               notePosition == 4 ||
               notePosition == 8 ||
               notePosition == 16 ||
               notePosition == 24 ||
                notePosition == 32 ||
                notePosition == 40 ||
                notePosition == 48 ||
                notePosition == 56 ||
               notePosition == 64 ||
               notePosition == 72 ||
               notePosition == 80;
    }

}
