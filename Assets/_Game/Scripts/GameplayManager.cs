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
    public GameObject notePrefab; // Assign your note prefab here

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

    [Header("UI Manager")]
    [SerializeField] GameUIManager gameUIManager;

    [Header("Controls")]
    // Removed keyboard controls - using UI buttons instead

    // Game state - now managed by GameStateManager
    private float currentTime = 0f;
    public List<NoteData> notes = new List<NoteData>();
    private List<GameObject> activeNotes = new List<GameObject>();

    // Add this field at the top of GameplayManager class
    private Dictionary<NoteData, GameObject> noteToGameObjectMap = new Dictionary<NoteData, GameObject>();




    // Setup state
    private bool isSetupComplete = false;

    private GameSettingsManager settingsManager;
    private MetronomeManager metronomeManager;
    private GameStateManager gameStateManager;
    private DataHandler dataHandler;
    private SongHandler songHandler;


    // Make it public so SpeedUpMode can access it



    async void Start()
    {
        Debug.Log("Starting async setup...");
        await SetupGameAsync();
        Debug.Log("Setup complete!");
    }

    async Task SetupGameAsync()
    {
        // Step 1: Initialize all managers first
        InitializeAllManagers();

        gameStateManager.SetGameState(GameState.None);

        Debug.Log("All managers initialized");

        // Step 2: Load notes from XML
        LoadNotes();
        Debug.Log($"Loaded {notes.Count} notes from XML");

        // Step 3: Pre-spawn initial notes
        PreSpawnInitialNotes();

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
            UpdateNotes();

            // Update TimeManager
            if (timeManager != null)
            {
                timeManager.UpdateCurrentTime(currentTime);
            }
        }
        else if (gameStateManager != null && gameStateManager.IsPreparing())
        {
            // Update note positions even when waiting for first hit
            UpdateNotePositions();
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
        notes = dataHandler.LoadNotesFromXML();

        // Debug: Print all loaded notes with their positions
        Debug.Log($"Loaded {notes.Count} notes:");
        for (int i = 0; i < Mathf.Min(notes.Count, 10); i++) // Show first 10 notes
        {
            var note = notes[i];
            Debug.Log($"  Note {i}: position={note.notePosition}, pitch={note.pitch}, isRest={note.isRest}");
        }

        // Get BPM from DataHandler and set it in SongHandler
        float xmlBpm = dataHandler.GetBPMFromXML();
        if (songHandler != null)
        {
            songHandler.SetOriginalBPM(xmlBpm);
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
        PreSpawnInitialNotes();

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

        // Check if this is the first hit and we're waiting for it
        if (gameStateManager != null && gameStateManager.IsPreparing())
        {
            StartGameFromFirstHit(note);
        }

        // Find the note GameObject using the dictionary
        if (noteToGameObjectMap.TryGetValue(note, out GameObject noteObj))
        {
            NoteController noteController = noteObj.GetComponent<NoteController>();
            if (noteController != null)
            {
                // Set the note as hit
                noteController.Hit();
                noteController.LinkWithPianoKey(pianoKey);
                targetBarController?.PlayOnHitEffect();
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

        // Find the note GameObject using the dictionary
        if (noteToGameObjectMap.TryGetValue(note, out GameObject noteObj))
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

        // Notify GameModeManager
        if (gameModeManager != null)
        {
            gameModeManager.OnBeatHit();
        }
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
        List<NoteController> controllers = new List<NoteController>();
        foreach (var noteObj in activeNotes)
        {
            if (noteObj != null)
            {
                NoteController controller = noteObj.GetComponent<NoteController>();
                if (controller != null)
                {
                    controllers.Add(controller);
                }
            }
        }
        return controllers;
    }

    void UpdateNotes()
    {
        // Spawn notes from XML data based on time
        float noteSpawnOffset = songHandler != null ? songHandler.GetNoteSpawnOffset() : 3f;
        foreach (var note in notes)
        {
            if (!note.isRest &&
                note.startTime <= currentTime + noteSpawnOffset && // Spawn up to offset seconds before hit
                note.startTime > currentTime && // Don't spawn notes that have already passed
                !IsNoteAlreadySpawned(note))
            {
                SpawnNote(note);
            }
        }

        UpdateNotePositions();
        CleanupOldNotes();
    }

    void UpdateNotePositions()
    {
        // Update note positions using NoteController
        foreach (var noteObj in activeNotes)
        {
            if (noteObj != null)
            {
                NoteController noteController = noteObj.GetComponent<NoteController>();
                if (noteController != null)
                {
                    // During preparation, we need to position notes based on their actual start time
                    // rather than the current game time (which is 0)
                    float timeToUse = gameStateManager != null && gameStateManager.IsPreparing()
                        ? 0f  // Use 0 for preparation so notes show their correct positions
                        : currentTime;

                    float noteSpawnOffset = songHandler != null ? songHandler.GetNoteSpawnOffset() : 3f;
                    float noteArrivalOffset = songHandler != null ? songHandler.GetNoteArrivalOffset() : 0f;
                    noteController.UpdatePosition(timeToUse, noteSpawnOffset, noteArrivalOffset);
                }
            }
        }
    }

    void PreSpawnInitialNotes()
    {
        // Pre-spawn all notes for the first 5 seconds
        foreach (var note in notes)
        {
            if (!note.isRest &&
                note.startTime <= 5f && // Pre-spawn notes for first 5 seconds
                !IsNoteAlreadySpawned(note))
            {
                SpawnNote(note);
            }
        }
        Debug.Log($"Pre-spawned notes for first 5 seconds");
    }

    void SpawnInitialNotes()
    {
        // Spawn notes that should have already been spawned before the game started
        float noteSpawnOffset = songHandler != null ? songHandler.GetNoteSpawnOffset() : 3f;
        foreach (var note in notes)
        {
            if (!note.isRest &&
                note.startTime <= noteSpawnOffset && // Notes that should have been spawned before game start
                !IsNoteAlreadySpawned(note))
            {
                SpawnNote(note);
            }
        }
    }

    bool IsNoteAlreadySpawned(NoteData note)
    {
        // Check if a note with this pitch and start time has already been spawned
        foreach (var noteObj in activeNotes)
        {
            if (noteObj != null)
            {
                NoteController noteController = noteObj.GetComponent<NoteController>();
                if (noteController != null && noteController.noteData != null)
                {
                    if (noteController.noteData.pitch == note.pitch &&
                        Mathf.Abs(noteController.noteData.startTime - note.startTime) < 0.1f)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void SpawnNote(NoteData note)
    {
        if (gameBoard == null)
        {
            Debug.LogError("GameBoard is null, cannot spawn note!");
            return;
        }

        if (!gameBoard.IsInitialized())
        {
            Debug.LogError("GameBoard is not initialized, cannot spawn note!");
            return;
        }

        GameObject noteObj;
        NoteController noteController;

        // Get spawn and target positions from the board
        Vector3 spawnPos = gameBoard.GetSpawnPosition(note.pitch);
        Vector3 targetPos = gameBoard.GetTargetPosition(note.pitch);

        if (notePrefab != null)
        {
            // Use the prefab
            noteObj = Instantiate(notePrefab, spawnPos, Quaternion.identity);
            noteObj.name = $"Note_{note.pitch}_{note.startTime:F1}_{note.duration:F1}";
            noteController = noteObj.GetComponent<NoteController>();
        }
        else
        {
            noteObj = null;
            noteController = null;
            return;
        }

        // Initialize with GameplayManager reference
        if (noteController != null)
        {
            float noteTravelTime = songHandler != null ? songHandler.GetNoteTravelTime() : 3f;
            noteController.Initialize(note, spawnPos, targetPos, noteTravelTime, this);
        }

        // Add to active notes list
        activeNotes.Add(noteObj);

        // Add to dictionary for easy lookup
        noteToGameObjectMap[note] = noteObj;

        Debug.Log($"Spawned note: {note.pitch} at {note.startTime:F2}s");
    }

    void CleanupOldNotes()
    {
        List<GameObject> toRemove = new List<GameObject>();
        List<NoteData> notesToRemove = new List<NoteData>();

        foreach (var noteObj in activeNotes)
        {
            if (noteObj == null)
            {
                toRemove.Add(noteObj);
            }
            else
            {
                NoteController noteController = noteObj.GetComponent<NoteController>();
                if (noteController != null && noteController.noteData != null)
                {
                    if (noteController.HasCompletelyPassedBoard())
                    {
                        noteController.Release();
                        noteController.UnlinkFromPianoKey();
                    }

                    // Remove notes that have passed their start time by more than 4 seconds
                    if (currentTime > noteController.noteData.startTime + 4f)
                    {
                        toRemove.Add(noteObj);
                        notesToRemove.Add(noteController.noteData);
                    }
                }
            }
        }

        foreach (var noteObj in toRemove)
        {
            if (noteObj != null)
            {
                Destroy(noteObj);
            }
            activeNotes.Remove(noteObj);
        }

        // Clean up dictionaries
        foreach (var note in notesToRemove)
        {
            noteToGameObjectMap.Remove(note);
        }
    }

    void ClearAllNotes()
    {
        foreach (var noteObj in activeNotes)
        {
            if (noteObj != null)
            {
                Destroy(noteObj);
            }
        }
        activeNotes.Clear();
        noteToGameObjectMap.Clear();
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
        Debug.Log("GameplayManager: Cleaned up");
    }
}
