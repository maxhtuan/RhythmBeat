using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NoteManager : MonoBehaviour, IService
{
    [Header("Note Settings")]
    public GameObject notePrefab; // Assign your note prefab here

    // Note tracking
    [SerializeField]
    private List<NoteData> notes = new List<NoteData>();
    private List<NoteController> activeNotes = new List<NoteController>();
    private Dictionary<NoteData, GameObject> noteToGameObjectMap = new Dictionary<NoteData, GameObject>();
    public List<NoteData> GetAllNotes()
    {
        return notes;
    }
    // References
    private GameBoardManager gameBoard;
    private SongHandler songHandler;
    private GameStateManager gameStateManager;
    private GameModeManager gameModeManager;


    public void Initialize()
    {
        gameBoard = ServiceLocator.Instance.GetService<GameBoardManager>();
        songHandler = ServiceLocator.Instance.GetService<SongHandler>();
        gameStateManager = ServiceLocator.Instance.GetService<GameStateManager>();
        gameModeManager = ServiceLocator.Instance.GetService<GameModeManager>();
        Debug.Log("NoteManager: Initialized");
    }

    public void Cleanup()
    {
        ClearAllNotes();
        Debug.Log("NoteManager: Cleaned up");
    }

    // Load notes from DataHandler
    public void LoadNotes(List<NoteData> loadedNotes)
    {
        notes = loadedNotes;

        var isSpeedUpMode = gameModeManager.IsSpeedUpMode();
        if (isSpeedUpMode)
        {
            // Generate speed-up pattern: E4 - Rest - Rest - Rest, then repeat E4 - D4 - C4 - Rest - G4 - F4 - E4 - Rest (5 times)
            GenerateSpeedUpPattern();
        }



        Debug.Log($"NoteManager: Loaded {notes.Count} notes");
    }

    // Spawn notes based on position (not time)
    public void UpdateNotes(float currentTime)
    {
        // Calculate current position based on time and BPM
        float beatDuration = 60f / (songHandler != null ? songHandler.GetCurrentBPM() : 60f);
        float currentPosition = currentTime / beatDuration;

        // Spawn notes 5 positions ahead
        float spawnPositionOffset = 5f;
        var speedUpMode = gameModeManager.IsSpeedUpMode();
        if (speedUpMode)
        {
            spawnPositionOffset = 10f;
        }
        float spawnPosition = currentPosition + spawnPositionOffset;

        foreach (var note in notes)
        {
            if (!note.isRest &&
                note.notePosition <= spawnPosition && // Spawn up to 5 positions ahead
                note.notePosition > currentPosition && // Don't spawn notes that have already passed
                !IsNoteAlreadySpawned(note))
            {
                SpawnNote(note);
            }
        }

        UpdateNotePositions(currentTime);
        CleanupOldNotes(currentTime);
    }

    // Update note positions
    public void UpdateNotePositions(float currentTime)
    {
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

    // Pre-spawn initial notes
    public void PreSpawnInitialNotes()
    {
        foreach (var note in notes)
        {
            if (!note.isRest &&
                note.startTime <= 5f && // Pre-spawn notes for first 5 seconds
                !IsNoteAlreadySpawned(note))
            {
                SpawnNote(note);
            }
        }
        Debug.Log($"NoteManager: Pre-spawned notes for first 5 seconds");
    }

    // Spawn initial notes
    public void SpawnInitialNotes()
    {
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

    // Check if note is already spawned
    private bool IsNoteAlreadySpawned(NoteData note)
    {
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

    // Spawn a single note
    private void SpawnNote(NoteData note)
    {
        if (gameBoard == null)
        {
            Debug.LogError("NoteManager: GameBoard is null, cannot spawn note!");
            return;
        }

        if (!gameBoard.IsInitialized())
        {
            Debug.LogError("NoteManager: GameBoard is not initialized, cannot spawn note!");
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

        // Initialize with NoteManager reference
        if (noteController != null)
        {
            float noteTravelTime = songHandler != null ? songHandler.GetNoteTravelTime() : 3f;
            noteController.Initialize(note, spawnPos, targetPos, noteTravelTime, this);
        }

        // Add to active notes list
        activeNotes.Add(noteController);

        // Add to dictionary for easy lookup
        noteToGameObjectMap[note] = noteObj;

        Debug.Log($"NoteManager: Spawned note {note.pitch} at {note.startTime:F2}s");
    }

    // Cleanup old notes
    private void CleanupOldNotes(float currentTime)
    {
        List<NoteController> toRemove = new List<NoteController>();
        List<NoteData> notesToRemove = new List<NoteData>();

        foreach (var noteController in activeNotes)
        {
            if (noteController == null)
            {
                toRemove.Add(noteController);
            }
            else
            {
                if (noteController != null && noteController.noteData != null)
                {
                    if (noteController.HasCompletelyPassedBoard())
                    {
                        if (!noteController.IsHit())
                        {
                            gameModeManager.OnNoteMissed();
                        }
                        noteController.Release();
                        noteController.UnlinkFromPianoKey();

                    }

                    // Remove notes that have passed their start time by more than 4 seconds
                    if (currentTime > noteController.noteData.startTime + 4f)
                    {
                        if (!noteController.IsHit())
                        {
                            gameModeManager.OnNoteMissed();
                        }
                        toRemove.Add(noteController);
                        notesToRemove.Add(noteController.noteData);
                    }
                }
            }
        }

        foreach (var noteController in toRemove)
        {
            if (noteController != null)
            {
                Destroy(noteController.gameObject);
            }
            activeNotes.Remove(noteController);
        }

        // Clean up dictionaries
        foreach (var note in notesToRemove)
        {
            noteToGameObjectMap.Remove(note);
        }
    }

    // Clear all notes
    public void ClearAllNotes()
    {
        foreach (var noteObj in activeNotes)
        {
            if (noteObj != null)
            {
                Destroy(noteObj.gameObject);
            }
        }
        activeNotes.Clear();
        noteToGameObjectMap.Clear();
    }

    // Get note GameObject by NoteData
    public GameObject GetNoteGameObject(NoteData note)
    {
        noteToGameObjectMap.TryGetValue(note, out GameObject noteObj);
        return noteObj;
    }

    // Get all active NoteControllers
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

    // Get all notes
    public List<NoteData> GetNotes()
    {
        return notes;
    }

    // Get active notes count
    public int GetActiveNotesCount()
    {
        return activeNotes.Count;
    }

    // Generate speed-up pattern
    private void GenerateSpeedUpPattern()
    {
        // Clear existing notes
        notes.Clear();

        int notePosition = 0;
        float currentTime = 0f;

        // First pattern: E4 - Rest - Rest - Rest
        notes.Add(new NoteData { pitch = "E4", startTime = currentTime, duration = 1f, isRest = false, notePosition = notePosition++ });
        currentTime += 1f;
        notes.Add(new NoteData { pitch = "", startTime = currentTime, duration = 1f, isRest = true, notePosition = notePosition++ });
        currentTime += 1f;
        notes.Add(new NoteData { pitch = "", startTime = currentTime, duration = 1f, isRest = true, notePosition = notePosition++ });
        currentTime += 1f;
        notes.Add(new NoteData { pitch = "", startTime = currentTime, duration = 1f, isRest = true, notePosition = notePosition++ });
        currentTime += 1f;

        // Repeat pattern 5 times: E4 - D4 - C4 - Rest - G4 - F4 - E4 - Rest
        for (int repeat = 0; repeat < 5; repeat++)
        {
            // Pattern: E4 - D4 - C4 - Rest - G4 - F4 - E4 - Rest
            notes.Add(new NoteData { pitch = "E4", startTime = currentTime, duration = 1f, isRest = false, notePosition = notePosition++ });
            currentTime += 1f;
            notes.Add(new NoteData { pitch = "D4", startTime = currentTime, duration = 1f, isRest = false, notePosition = notePosition++ });
            currentTime += 1f;
            notes.Add(new NoteData { pitch = "C4", startTime = currentTime, duration = 1f, isRest = false, notePosition = notePosition++ });
            currentTime += 1f;
            notes.Add(new NoteData { pitch = "", startTime = currentTime, duration = 1f, isRest = true, notePosition = notePosition++ });
            currentTime += 1f;
            notes.Add(new NoteData { pitch = "G4", startTime = currentTime, duration = 1f, isRest = false, notePosition = notePosition++ });
            currentTime += 1f;
            notes.Add(new NoteData { pitch = "F4", startTime = currentTime, duration = 1f, isRest = false, notePosition = notePosition++ });
            currentTime += 1f;
            notes.Add(new NoteData { pitch = "E4", startTime = currentTime, duration = 1f, isRest = false, notePosition = notePosition++ });
            currentTime += 1f;
            notes.Add(new NoteData { pitch = "", startTime = currentTime, duration = 1f, isRest = true, notePosition = notePosition++ });
            currentTime += 1f;

            // Add 4 beats of rest (gap) between patterns (except after the last pattern)
            if (repeat < 4) // Don't add gap after the last pattern
            {
                for (int rest = 0; rest < 4; rest++)
                {
                    notes.Add(new NoteData { pitch = "", startTime = currentTime, duration = 1f, isRest = true, notePosition = notePosition++ });
                    currentTime += 1f;
                }
            }
        }

        Debug.Log($"Generated speed-up pattern with {notes.Count} notes");
    }
}
