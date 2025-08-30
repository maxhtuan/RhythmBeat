using UnityEngine;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using DG.Tweening;

public class SpeedUpMode : IGameMode
{
    public string ModeName => "Speed Up";

    private GameplayManager gameplayManager;
    private AudioSource musicSource;
    private MetronomeManager metronomeManager; // Add this
    private GameSettingsManager settingsManager;
    private SongHandler songHandler;
    private int beatCount = 0;
    private float originalBPM = 60f;
    private float currentBPM = 60f;
    private int currentPatternStartPosition = 0; // Track which pattern we're in

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
        this.metronomeManager = ServiceLocator.Instance.GetService<MetronomeManager>();
        this.gameplayManager = ServiceLocator.Instance.GetService<GameplayManager>();
        this.settingsManager = ServiceLocator.Instance.GetService<GameSettingsManager>();
        this.songHandler = ServiceLocator.Instance.GetService<SongHandler>();


        beatCount = 0;
        hasHitFirstNote = false;
        currentPatternStartPosition = 0;

        // Get the original BPM from GameplayManager
        if (songHandler != null)
        {
            originalBPM = songHandler.originalBPM;
            currentBPM = originalBPM;
            secondsPerBeat = 60f / originalBPM;
            songHandler.SetBPM(originalBPM); // Reset to original BPM
        }

        Debug.Log($"Speed Up Mode: Original BPM = {originalBPM}, Seconds per beat = {secondsPerBeat:F2}");
    }

    public void TriggerThisMode()
    {
        isFirstHit = true;
        hitCount = 0;
        Debug.Log("Speed Up Mode: Started");
        // Don't play background music, only beat sounds
        if (musicSource != null)
        {
            musicSource.Stop();
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
        hitCount = 0;

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

        // Clear all notes from NoteManager
        var noteManager = ServiceLocator.Instance.GetService<NoteManager>();
        if (noteManager != null)
        {
            noteManager.ClearAllNotes();
            Debug.Log("Speed Up Mode: Cleared all notes");
        }

        beatCount = 0;
        hasHitFirstNote = false;
        currentPatternStartPosition = 0;
    }

    public async Task OnBeatHit()
    {
        if (gameplayManager == null) return;

        Debug.Log("Speed Up Mode: Beat Hit");

        if (isFirstHit)
        {
            isFirstHit = false;
            Debug.Log("Speed Up Mode: First Hit");
            return;
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
        if (hitCount >= 6)
        {
            await OnPatternComplete();
        }
    }

    bool isFirstHit = true;

    int hitCount = 0;

    // Method to reset pattern count when player misses notes
    public void OnNoteMissed()
    {
        // Reset hit count if player misses a note in the current pattern
        if (hitCount > 0)
        {
            Debug.Log("Speed Up Mode: Note Missed - Resetting Pattern Count");
            hitCount = 0;
        }
    }

    // Method to check if current pattern is complete
    public bool IsPatternComplete()
    {
        return hitCount >= 6;
    }

    // Method to handle pattern completion (called when all 6 notes are hit)
    public async Task OnPatternComplete()
    {
        Debug.Log("Speed Up Mode: Pattern Complete - Adding BPM");

        // Wait exactly 1 beat duration based on current BPM
        float beatDuration = 60f / songHandler.GetCurrentBPM();
        int delayMs = Mathf.RoundToInt(beatDuration * 1000f);
        await Task.Delay(delayMs);

        var gameUIManager = ServiceLocator.Instance.GetService<GameUIManager>();
        gameUIManager.OnSpeedUpTriggered();

        // Smooth BPM increase
        var curBMP = songHandler.GetCurrentBPM();
        var original = songHandler.GetCurrentBPM();
        DOTween.To(() => curBMP, x =>
        {
            songHandler.OnAddBPM(x - original);
            original = songHandler.GetCurrentBPM();
        }, curBMP + 5f, 1f).OnComplete(() =>
        {
        });

        // Reset hit count for next pattern
        hitCount = 0;
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
               notePosition == 16 ||
               notePosition == 28 ||
               notePosition == 40 ||
               notePosition == 52 ||
               notePosition == 64 ||
               notePosition == 76;
    }


}
