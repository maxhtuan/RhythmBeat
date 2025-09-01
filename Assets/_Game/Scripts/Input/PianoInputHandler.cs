using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PianoInputHandler : MonoBehaviour, IService
{
    [Header("References")]
    public GameplayManager gameplayManager;

    [Header("Piano Key Components")]
    public PianoKey[] pianoKeyComponents; // Assign actual PianoKey components in inspector

    [Header("Input Settings")]
    public float hitWindow = 0.2f; // Time window for hitting notes (in seconds)

    // Private fields
    private Dictionary<PianoKey, bool> keyPressed = new Dictionary<PianoKey, bool>();
    private List<PianoKey> allPianoKeys = new List<PianoKey>();

    // Service references
    private NoteManager noteManager;
    private GameStateManager gameStateManager;
    private GameModeManager gameModeManager;
    private GameplayLogger gameplayLogger;
    [SerializeField] TargetBarController targetBarController;
    private AudioManager audioManager;

    // Input state tracking
    private bool isInputEnabled = true;
    private float currentTime = 0f;

    #region Service Implementation
    public void Initialize()
    {
        // Get all required services
        noteManager = ServiceLocator.Instance.GetService<NoteManager>();
        gameStateManager = ServiceLocator.Instance.GetService<GameStateManager>();
        gameModeManager = ServiceLocator.Instance.GetService<GameModeManager>();
        gameplayLogger = ServiceLocator.Instance.GetService<GameplayLogger>();
        audioManager = ServiceLocator.Instance.GetService<AudioManager>();

        // Get gameplay manager reference
        gameplayManager = ServiceLocator.Instance.GetService<GameplayManager>();

        // Get target bar controller from gameplay manager
        if (gameplayManager != null)
        {
            targetBarController = gameplayManager.TargetBarController;
        }

        InitializePianoKeys();
        Debug.Log("PianoInputHandler: Initialized");
    }

    public void Cleanup()
    {
        keyPressed.Clear();
        allPianoKeys.Clear();
        Debug.Log("PianoInputHandler: Cleaned up");
    }
    #endregion

    void Start()
    {
        // Fallback initialization if not called through service locator
        if (noteManager == null)
        {
            Initialize();
        }
    }

    void Update()
    {
        if (!isInputEnabled) return;

        UpdateCurrentTime();
    }

    #region Piano Key Management
    private void InitializePianoKeys()
    {
        // Add assigned piano key components
        if (pianoKeyComponents != null)
        {
            allPianoKeys.AddRange(pianoKeyComponents);
        }

        // Also find any additional piano keys in the scene
        var scenePianoKeys = GameObject.FindObjectsByType<PianoKey>(FindObjectsSortMode.None);
        foreach (var pianoKey in scenePianoKeys)
        {
            if (!allPianoKeys.Contains(pianoKey))
            {
                allPianoKeys.Add(pianoKey);
            }
        }

        // Initialize each piano key and set up input handling
        foreach (var pianoKey in allPianoKeys)
        {
            pianoKey.Initialize();
            keyPressed[pianoKey] = false;

            // Set up the piano key to call our input handler
            SetupPianoKeyInput(pianoKey);
        }

        Debug.Log($"PianoInputHandler: Initialized {allPianoKeys.Count} piano keys");
    }

    private void SetupPianoKeyInput(PianoKey pianoKey)
    {
        // Override or extend the piano key's input handling to call our methods
        // This depends on how PianoKey.cs is structured
        // For now, we'll assume PianoKey has events or methods we can hook into
    }

    public List<PianoKey> GetAllPianoKeys()
    {
        return allPianoKeys;
    }

    public PianoKey GetPianoKeyByNoteName(string noteName)
    {
        return allPianoKeys.FirstOrDefault(pk => pk.noteName == noteName);
    }
    #endregion

    #region Input Handling
    private void UpdateCurrentTime()
    {
        if (gameplayManager != null)
        {
            currentTime = gameplayManager.GetCurrentTime();
        }
    }

    #endregion

    #region Piano Key Input (Called by PianoKey components)
    public void OnPianoKeyPressed(PianoKey pianoKey)
    {
        // Find the closest note to hit for this piano key
        NoteData closestNote = FindClosestNote(pianoKey.noteName);
        if (closestNote != null)
        {
            float accuracy = CalculateHitAccuracy(closestNote);
            ProcessNoteHit(pianoKey, closestNote, accuracy);
        }
        else
        {
            // No note to hit - play sound and show feedback
            // OnNoteMiss(pianoKey, pianoKey.noteName);
            targetBarController?.OnHolding();
        }
    }

    public void OnPianoKeyReleased(PianoKey pianoKey, NoteData linkedNote)
    {
        if (!isInputEnabled) return;

        if (keyPressed.ContainsKey(pianoKey))
        {
            keyPressed[pianoKey] = false;
            ProcessNoteRelease(pianoKey, linkedNote);
        }
    }
    #endregion

    #region Note Processing
    private void ProcessNoteHit(PianoKey pianoKey, NoteData note, float accuracy)
    {
        if (gameplayManager == null) return;

        Debug.Log($"Note hit: {note.pitch} with accuracy: {accuracy:F2}");

        // Log the note hit
        LogNoteHit(note, accuracy);

        // Check if this is the first hit and we're waiting for it
        if (gameStateManager != null && gameStateManager.IsPreparing())
        {
            // Notify gameplay manager to start the game
            gameplayManager.StartGameFromFirstHit(note);
        }

        // Find the note GameObject using NoteManager
        GameObject noteObj = noteManager?.GetNoteGameObject(note);
        if (noteObj != null)
        {
            NoteController noteController = noteObj.GetComponent<NoteController>();
            if (noteController != null)
            {
                // Set the note as hit
                noteController.Hit();
                noteController.LinkWithPianoKey(pianoKey);
                targetBarController?.PlayOnHitEffect();

                // Trigger vibration on note hit (based on accuracy)
                TriggerVibrationByAccuracy(accuracy);

                // Play piano key sound
                audioManager?.PlayPianoKeySound(note.pitch);

                Debug.Log($"Note {note.pitch} marked as hit");
            }
        }
        else
        {
            targetBarController?.OnHolding();
            Debug.LogWarning($"Could not find GameObject for note: {note.pitch}");
        }

        // Notify GameModeManager
        if (gameModeManager != null)
        {
            gameModeManager.OnBeatHit();
        }
    }

    private void ProcessNoteRelease(PianoKey pianoKey, NoteData note)
    {
        if (gameplayManager == null) return;

        targetBarController?.OnReleaseHitEffect();

        if (note == null)
        {
            Debug.LogWarning("Note is null");
            return;
        }

        Debug.Log($"Note released: {note?.pitch}");

        // Log the note release
        LogNoteRelease(note);

        // Find the note GameObject using NoteManager
        GameObject noteObj = noteManager?.GetNoteGameObject(note);
        if (noteObj != null)
        {
            NoteController noteController = noteObj.GetComponent<NoteController>();
            if (noteController != null)
            {
                // Release the note
                noteController.Release();
                noteController.UnlinkFromPianoKey();
                Debug.Log($"Note {note.pitch} released");
            }
        }
        else
        {
            Debug.LogWarning($"Could not find GameObject for note: {note.pitch}");
        }
    }

    private void OnNoteMiss(PianoKey pianoKey, string noteName)
    {
        Debug.Log($"Pressed {noteName} but no note to hit");

        // Play miss sound
        audioManager?.PlayMissSound();

        // Show miss feedback
        targetBarController?.PlayOnMissEffect();
    }

    private void TryHitAnyAvailableNote()
    {
        // Try to find any note that can be hit
        NoteData closestNote = null;
        float closestTime = float.MaxValue;

        foreach (var note in noteManager.GetAllNotes())
        {
            if (note.isRest) continue;

            float noteArrivalTime = GetNoteArrivalTime(note);
            float timeDiff = Mathf.Abs(noteArrivalTime - currentTime);

            if (timeDiff <= hitWindow && timeDiff < closestTime)
            {
                closestNote = note;
                closestTime = timeDiff;
            }
        }

        if (closestNote != null)
        {
            // Find the piano key for this note
            PianoKey pianoKey = GetPianoKeyByNoteName(closestNote.pitch);
            if (pianoKey != null)
            {
                float accuracy = CalculateHitAccuracy(closestNote);
                ProcessNoteHit(pianoKey, closestNote, accuracy);
            }
        }
    }
    #endregion

    #region Note Finding and Accuracy
    private NoteData FindClosestNote(string noteName)
    {
        if (gameplayManager == null) return null;

        NoteData closestNote = null;
        float closestTime = float.MaxValue;

        foreach (var note in noteManager.GetAllNotes())
        {
            if (note.isRest) continue;

            // Use position-based timing instead of startTime
            float noteArrivalTime = GetNoteArrivalTime(note);
            float timeDiff = Mathf.Abs(noteArrivalTime - currentTime);

            if (timeDiff <= hitWindow && timeDiff < closestTime)
            {
                // Check if the note matches the piano key
                if (IsNoteMatch(note, noteName))
                {
                    closestNote = note;
                    closestTime = timeDiff;
                }
            }
        }

        return closestNote;
    }

    private float GetNoteArrivalTime(NoteData note)
    {
        // Get SongHandler to calculate position-based arrival time
        var songHandler = ServiceLocator.Instance.GetService<SongHandler>();
        if (songHandler != null)
        {
            // Calculate position-based arrival time
            float beatDuration = 60f / songHandler.GetCurrentBPM();
            return note.notePosition * beatDuration;
        }

        // Fallback to original startTime if SongHandler not available
        return note.startTime;
    }

    private bool IsNoteMatch(NoteData note, string noteName)
    {
        if (note.isRest) return false;

        // Simple pitch matching - check if the note pitch starts with the expected pitch
        return note.pitch.StartsWith(noteName);
    }

    private float CalculateHitAccuracy(NoteData note)
    {
        if (gameplayManager == null) return 0f;

        float noteArrivalTime = GetNoteArrivalTime(note);
        float timeDiff = Mathf.Abs(noteArrivalTime - currentTime);

        // Calculate accuracy based on timing
        float accuracy = 1f - (timeDiff / hitWindow);
        return Mathf.Clamp01(accuracy);
    }
    #endregion

    #region Logging
    private void LogNoteHit(NoteData note, float accuracy)
    {
        if (gameplayLogger != null && gameplayLogger.IsLogging())
        {
            gameplayLogger.LogNoteHit(note.pitch, note.notePosition, note.startTime, accuracy, currentTime, note.isRest);
        }
    }

    private void LogNoteRelease(NoteData note)
    {
        if (gameplayLogger != null && gameplayLogger.IsLogging())
        {
            gameplayLogger.LogNoteRelease(note.pitch, note.notePosition, note.startTime, note.isRest);
        }
    }

    public void LogNoteMiss(NoteData note)
    {
        if (gameplayLogger != null && gameplayLogger.IsLogging())
        {
            gameplayLogger.LogNoteMiss(note.pitch, note.notePosition, note.startTime, note.isRest);
        }
    }
    #endregion

    #region Vibration
    private void TriggerVibrationByAccuracy(float accuracy)
    {
        // Different vibration patterns based on accuracy
        if (accuracy >= 0.9f)
        {
            // Perfect hit - short, sharp vibration
            TriggerVibration(30);
        }
        else if (accuracy >= 0.7f)
        {
            // Good hit - medium vibration
            TriggerVibration(50);
        }
        else
        {
            // Poor hit - longer vibration
            TriggerVibration(80);
        }
    }

    private void TriggerVibration(int durationMs)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Android vibration with custom duration
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                vibrator.Call("vibrate", durationMs);
            }
        }
#elif UNITY_IOS && !UNITY_EDITOR
        // iOS vibration (fixed duration)
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Handheld.Vibrate();
        }
#elif UNITY_WEBGL && !UNITY_EDITOR
        // WebGL vibration with custom duration
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Application.ExternalEval($@"
                if (navigator.vibrate) {{
                    navigator.vibrate({durationMs});
                }}
            ");
        }
#else
        // Editor or other platforms
        if (SystemInfo.supportsVibration)
        {
            Handheld.Vibrate();
        }
#endif
    }
    #endregion

    #region Public Interface
    public void EnableInput(bool enable)
    {
        isInputEnabled = enable;
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }
    #endregion
}
