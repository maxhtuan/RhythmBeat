using UnityEngine;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Threading.Tasks;
using System;

public class GameplayManager : MonoBehaviour, IService
{
    [Header("Basic Settings")]
    public GameBoard gameBoard; // Assign your GameBoard component here

    [Header("Note Visual")]
    public float noteLengthMultiplier = 2f; // Multiply note visual length by this value
    [SerializeField] TargetBarController targetBarController;

    // Public property to access target bar controller
    public TargetBarController TargetBarController => targetBarController;

    [Header("Audio")]
    public AudioSource musicSource;

    [Header("Game Mode")]
    [SerializeField] GameModeManager gameModeManager;

    [Header("Timeline")]
    [SerializeField] TimelineManager timelineManager;

    [Header("Time Manager")]
    [SerializeField] TimeManager timeManager;

    GameUIManager gameUIManager;

    [Header("Controls")]
    // Removed keyboard controls - using UI buttons instead

    // Game state - now managed by GameStateManager
    private float currentTime = 0f;

    // Note management is now handled by NoteManager
    private NoteManager noteManager;




    // Setup state
    private bool isSetupComplete = false;

    private GameSettingsManager settingsManager;
    private MetronomeManager metronomeManager;
    private GameStateManager gameStateManager;
    private DataHandler dataHandler;
    private SongHandler songHandler;
    private FirebaseManager firebaseManager;
    private GameplayLogger gameplayLogger;


    // Make it public so SpeedUpMode can access it



    async void Start()
    {
        Debug.Log("Starting async setup...");
        await SetupGameAsync();
        Debug.Log("Setup complete!");
    }

    public async Task SetupGameAsync()
    {
        // Step 1: Initialize all managers first
        await InitializeAllManagers();

        gameStateManager.SetGameState(GameState.None);

        Debug.Log("All managers initialized");

        // Step 2: Load notes from XML
        LoadNotes();
        Debug.Log($"Loaded notes from XML");

        // Step 3: Pre-spawn initial notes
        if (noteManager != null)
        {
            noteManager.PreSpawnInitialNotes();
        }

        isSetupComplete = true;

        gameStateManager.SetGameState(GameState.Preparing);
    }

    private async Task InitializeAllManagers()
    {
        gameModeManager = ServiceLocator.Instance.GetService<GameModeManager>();
        settingsManager = ServiceLocator.Instance.GetService<GameSettingsManager>();
        gameStateManager = ServiceLocator.Instance.GetService<GameStateManager>();
        dataHandler = ServiceLocator.Instance.GetService<DataHandler>();
        songHandler = ServiceLocator.Instance.GetService<SongHandler>();
        gameBoard = ServiceLocator.Instance.GetService<GameBoard>();
        noteManager = ServiceLocator.Instance.GetService<NoteManager>();
        gameUIManager = ServiceLocator.Instance.GetService<GameUIManager>();
        firebaseManager = ServiceLocator.Instance.GetService<FirebaseManager>();
        gameplayLogger = ServiceLocator.Instance.GetService<GameplayLogger>();
        Debug.Log("Starting manager initialization...");

        if (gameUIManager != null)
        {
            gameUIManager.Initialize();
        }
        Debug.Log("Manager initialization complete!");
    }


    void Update()
    {
        if (gameStateManager != null && gameStateManager.IsPlaying())
        {
            // Use real time for currentTime (not scaled time)
            currentTime += Time.deltaTime;

            // Update notes using NoteManager
            if (noteManager != null)
            {
                noteManager.UpdateNotes(currentTime);
            }

            // Update TimeManager
            if (timeManager != null)
            {
                timeManager.UpdateCurrentTime(currentTime);
            }
        }
        else if (gameStateManager != null && gameStateManager.IsPreparing())
        {
            // Update note positions even when waiting for first hit
            if (noteManager != null)
            {
                noteManager.UpdateNotePositions(currentTime);
            }
        }
    }

    void LoadNotes()
    {
        // Get DataHandler from Service Locator
        var dataHandler = ServiceLocator.Instance.GetService<DataHandler>();
        if (dataHandler == null)
        {
            Debug.LogError("DataHandler not found in Service Locator!");
            return;
        }

        // Load notes using DataHandler
        var loadedNotes = dataHandler.LoadNotesFromXML();

        // Pass notes to NoteManager
        if (noteManager != null)
        {
            noteManager.LoadNotes(loadedNotes);
        }

        // Debug: Print all loaded notes with their positions
        Debug.Log($"Loaded {loadedNotes.Count} notes:");
        for (int i = 0; i < Mathf.Min(loadedNotes.Count, 10); i++) // Show first 10 notes
        {
            var note = loadedNotes[i];
            Debug.Log($"  Note {i}: position={note.notePosition}, pitch={note.pitch}, isRest={note.isRest}");
        }

        // Get BPM from DataHandler and set it in SongHandler
        float xmlBpm = dataHandler.GetBPMFromXML();

        bool isSpeedUpMode = gameModeManager.IsSpeedUpMode();

        if (songHandler != null)
        {
            songHandler.SetOriginalBPM(xmlBpm);
            if (isSpeedUpMode)
            {
                songHandler.SetBPM(settingsManager.BaseBPMSpeedUpMode);
            }
        }
    }

    void StartGameFromFirstHit(NoteData hitNote)
    {
        if (gameStateManager != null)
        {
            gameStateManager.SetGameState(GameState.Playing);
        }

        // Sync timing to the note that was actually hit
        currentTime = hitNote.startTime;
        Debug.Log($"First note hit! Game started! Syncing to note '{hitNote.pitch}' at {currentTime:F2}s");

        // Start gameplay logging session
        StartGameplayLogging(hitNote);

        // Start game mode (SpeedUpMode will handle metronome)
        if (gameModeManager != null)
        {
            gameModeManager.StartMode();
        }

        // Start timeline
        if (timelineManager != null)
        {
            timelineManager.StartTimeline();
        }

        // Reset TimeManager to sync with game start
        if (timeManager != null)
        {
            timeManager.Reset();
        }

        // Notify UI Manager that game started from first hit
        if (gameUIManager != null)
        {
            gameUIManager.OnGameStartedFromFirstHit();
        }
    }

    private void StartGameplayLogging(NoteData firstNote)
    {
        if (gameplayLogger != null)
        {
            string gameMode = gameModeManager.IsSpeedUpMode() ? "SpeedUp" : "Perform";
            float initialBPM = songHandler.GetCurrentBPM();
            int totalNotes = noteManager.GetActiveNotesCount();

            gameplayLogger.StartSession(gameMode, initialBPM, totalNotes);
            Debug.Log($"GameplayLogger: Started logging session for {gameMode} mode, BPM: {initialBPM}, Total Notes: {totalNotes}");
        }
    }

    public void PauseGame()
    {
        // Note: We don't have a Paused state anymore, so this could either:
        // 1. Go back to Preparing state, or
        // 2. Stay in Playing but pause the game mode
        // For now, let's pause the game mode but keep the state as Playing

        // Pause game mode
        if (gameModeManager != null)
        {
            gameModeManager.PauseMode();
        }

        Debug.Log("Game paused! Press SPACE to resume.");
    }

    public void RestartGame()
    {
        if (gameStateManager != null)
        {
            gameStateManager.SetGameState(GameState.Preparing);
        }
        currentTime = 0f;

        // End and reinitialize current game mode
        if (gameModeManager != null)
        {
            gameModeManager.ReinitializeMode();
        }

        // Pause timeline
        if (timelineManager != null)
        {
            timelineManager.PauseTimeline();
        }

        // Reset TimeManager
        if (timeManager != null)
        {
            timeManager.Reset();
        }

        // Clear all existing notes
        ClearAllNotes();

        // Pre-spawn notes for the first 5 seconds again
        if (noteManager != null)
        {
            noteManager.PreSpawnInitialNotes();
        }

        // Notify UI Manager to show title again
        if (gameUIManager != null)
        {
            gameUIManager.OnGameRestarted();
        }

        // Stop metronome
        if (metronomeManager != null)
        {
            metronomeManager.StopMetronome();
        }

        Debug.Log("Game restarted! Pre-spawned notes for first 5 seconds. Hit the first note to start!");
    }

    // Public properties for other scripts to access
    public bool IsPlaying => gameStateManager != null && gameStateManager.IsPlaying();
    public bool IsSetupComplete => isSetupComplete;
    public bool IsWaitingForFirstHit => gameStateManager != null && gameStateManager.IsPreparing();
    public float hitWindow = 0.2f; // Time window for hitting notes (in seconds)

    // Method for piano input to call when notes are hit
    public void OnNoteHit(PianoKey pianoKey, NoteData note, float accuracy)
    {
        if (!isSetupComplete)
        {
            Debug.LogWarning("Game not fully set up yet, ignoring note hit");
            return;
        }

        Debug.Log($"Note hit: {note.pitch} with accuracy: {accuracy:F2}");

        // Log the note hit
        LogNoteHit(note, accuracy);

        // Check if this is the first hit and we're waiting for it
        if (gameStateManager != null && gameStateManager.IsPreparing())
        {
            StartGameFromFirstHit(note);
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


                Debug.Log($"Note {note.pitch} marked as hit");
            }
        }
        else
        {
            Debug.LogWarning($"Could not find GameObject for note: {note.pitch}");
        }

        // Notify GameModeManager
        if (gameModeManager != null)
        {
            gameModeManager.OnBeatHit();
        }
    }

    // Method for piano input to call when notes are released
    public void OnNoteRelease(PianoKey pianoKey, NoteData note)
    {
        targetBarController?.OnReleaseHitEffect();

        if (pianoKey == null)
        {
            Debug.LogWarning("PianoKey is null");
            return;
        }



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

    public void OnEndGame()
    {
        ClearAllNotes();
        gameplayLogger.EndSession();
        gameModeManager.EndMode();
    }

    public void OnNoteHolding(PianoKey pianoKey)
    {
        Debug.Log($"Note holding: {pianoKey.noteName}");
        targetBarController?.OnHolding();
    }

    // Method to get current time for input handling
    public float GetCurrentTime()
    {
        return currentTime;
    }

    // Method to get active NoteControllers for PianoKey linking
    public List<NoteController> GetActiveNoteControllers()
    {
        return noteManager?.GetActiveNoteControllers() ?? new List<NoteController>();
    }

    // Note-related methods are now handled by NoteManager
    void SpawnInitialNotes()
    {
        if (noteManager != null)
        {
            noteManager.SpawnInitialNotes();
        }
    }

    void ClearAllNotes()
    {
        if (noteManager != null)
        {
            noteManager.ClearAllNotes();
        }
    }

    public void ResetBPM()
    {
        if (songHandler != null)
        {
            songHandler.ResetBPM();
        }
    }

    public float GetOriginalTravelSpeed()
    {
        return songHandler != null ? songHandler.GetOriginalTravelSpeed() : 1f;
    }

    public void Initialize()
    {
        Debug.Log("GameplayManager: Initialized");
    }

    public void Cleanup()
    {
        // End gameplay logging session
        if (gameplayLogger != null && gameplayLogger.IsLogging())
        {
            gameplayLogger.EndSession();
        }

        Debug.Log("GameplayManager: Cleaned up");
    }

    // Gameplay logging methods
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

    public void LogBPMChange(float oldBPM, float newBPM, string reason)
    {
        if (gameplayLogger != null && gameplayLogger.IsLogging())
        {
            gameplayLogger.LogBPMChange(oldBPM, newBPM, reason);
        }
    }

    public void LogPatternComplete()
    {
        if (gameplayLogger != null && gameplayLogger.IsLogging())
        {
            gameplayLogger.LogPatternComplete();
        }
    }

    // Vibration methods
    private void TriggerVibration()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Android vibration
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                vibrator.Call("vibrate", 50); // 50ms vibration
            }
        }
#elif UNITY_IOS && !UNITY_EDITOR
        // iOS vibration
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Handheld.Vibrate();
        }
#elif UNITY_WEBGL && !UNITY_EDITOR
        // WebGL vibration (if supported by browser)
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // Use WebGL vibration API
            Application.ExternalEval(@"
                if (navigator.vibrate) {
                    navigator.vibrate(50);
                }
            ");
        }
#else
        // Editor or other platforms - use Unity's built-in vibration
        if (SystemInfo.supportsVibration)
        {
            Handheld.Vibrate();
        }
#endif
    }

    // Vibration with custom duration
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

    // Vibration based on accuracy
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
}
