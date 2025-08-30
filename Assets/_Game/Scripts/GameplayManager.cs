using UnityEngine;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Threading.Tasks;

public class GameplayManager : MonoBehaviour
{
    [Header("Basic Settings")]
    public GameBoard gameBoard; // Assign your GameBoard component here
    public GameObject notePrefab; // Assign your note prefab here

    [Header("Note Timing")]
    public float noteSpawnOffset = 3f; // How many seconds before hit time to spawn notes
    public float noteTravelTime = 3f; // How long notes take to travel from spawn to target
    public float noteArrivalOffset = 0f; // How many seconds before hit time notes should arrive (negative = early, positive = late)
    public float noteLengthMultiplier = 2f; // Multiply note visual length by this value
    [SerializeField] TargetBarController targetBarController;

    // Add BPM and time scaling properties
    [Header("BPM and Timing")]
    public float originalBPM = 60f; // Original BPM from XML
    public float currentBPM = 60f; // Current BPM (can be modified by game modes)
    public float timeScale = 1f; // Time scaling factor (1.0 = normal speed)

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

    // Game state
    private bool isPlaying = false;
    private bool isWaitingForFirstHit = true;
    private float currentTime = 0f;
    public List<NoteData> notes = new List<NoteData>();
    private List<GameObject> activeNotes = new List<GameObject>();

    // Add this field at the top of GameplayManager class
    private Dictionary<NoteData, GameObject> noteToGameObjectMap = new Dictionary<NoteData, GameObject>();

    // Setup state
    private bool isSetupComplete = false;

    async void Start()
    {
        Debug.Log("Starting async setup...");
        await SetupGameAsync();
        Debug.Log("Setup complete!");
    }

    async Task SetupGameAsync()
    {
        // Step 1: Load notes from XML
        LoadNotes();
        Debug.Log($"Loaded {notes.Count} notes from XML");

        // Step 2: Wait for GameBoard to be initialized
        await WaitForGameBoardInitialization();
        Debug.Log("GameBoard initialized");

        // Step 3: Wait for other managers to be ready
        // await WaitForManagersReady();
        // Debug.Log("All managers ready");

        // Step 4: Pre-spawn initial notes
        PreSpawnInitialNotes();
        Debug.Log("Pre-spawned notes for first 5 seconds. Hit the first note to start!");

        isSetupComplete = true;
    }

    async Task WaitForGameBoardInitialization()
    {
        if (gameBoard == null)
        {
            Debug.LogError("GameBoard not assigned!");
            return;
        }

        // Wait until GameBoard is initialized
        while (!gameBoard.IsInitialized())
        {
            await Task.Yield();
        }

        // Debug distance information
        Bounds boardBounds = gameBoard.GetBoardBounds();
        Debug.Log($"GameBoard bounds: {boardBounds}");
        Debug.Log($"Board center: {gameBoard.transform.position}");
    }

    async Task WaitForManagersReady()
    {
        // Wait for GameModeManager if assigned
        if (gameModeManager != null)
        {
            // Add a small delay to ensure GameModeManager is ready
            await Task.Delay(100);
        }

        // Wait for TimelineManager if assigned
        // if (timelineManager != null)
        // {
        //     // Add a small delay to ensure TimelineManager is ready
        //     await Task.Delay(100);
        // }
    }



    void Update()
    {
        // Only run update logic if setup is complete
        if (!isSetupComplete) return;

        if (isPlaying)
        {
            // Apply time scaling to current time increment
            currentTime += Time.deltaTime * timeScale;
            UpdateNotes();

            // Update TimeManager
            if (timeManager != null)
            {
                timeManager.UpdateCurrentTime(currentTime);
            }
        }
        else if (isWaitingForFirstHit)
        {
            // Update note positions even when waiting for first hit
            UpdateNotePositions();
        }
    }

    void LoadNotes()
    {
        // Load XML file
        TextAsset xmlFile = Resources.Load<TextAsset>("song");
        if (xmlFile == null)
        {
            Debug.LogError("Could not load song.xml!");
            return;
        }

        // Parse XML (simplified)
        var doc = XDocument.Parse(xmlFile.text);
        float currentTime = 0f;
        float xmlBpm = 60f;

        // Get BPM
        var metronome = doc.Descendants("metronome").FirstOrDefault();
        if (metronome != null)
        {
            var perMinute = metronome.Element("per-minute");
            if (perMinute != null)
            {
                xmlBpm = float.Parse(perMinute.Value);
            }
        }

        // Store the original BPM
        originalBPM = xmlBpm;
        currentBPM = xmlBpm;

        // Get divisions
        var divisions = doc.Descendants("divisions").FirstOrDefault();
        int divisionsPerQuarter = divisions != null ? int.Parse(divisions.Value) : 8;
        float secondsPerTick = 60f / (xmlBpm * divisionsPerQuarter);

        // Parse notes from Learner part only (P1)
        var learnerPart = doc.Descendants("part").FirstOrDefault(p => p.Attribute("id")?.Value == "P1");
        if (learnerPart == null)
        {
            Debug.LogError("Could not find Learner part (P1) in XML!");
            return;
        }

        Debug.Log("Found Learner part (P1), parsing notes...");

        // Parse notes from the Learner part only
        foreach (var noteElement in learnerPart.Descendants("note"))
        {
            NoteData note = new NoteData();

            // Get duration
            var durationElement = noteElement.Element("duration");
            if (durationElement != null)
            {
                int durationTicks = int.Parse(durationElement.Value);
                note.duration = durationTicks * secondsPerTick;
            }

            // Check if rest
            var restElement = noteElement.Element("rest");
            if (restElement != null)
            {
                note.isRest = true;
                note.pitch = "REST";
            }
            else
            {
                note.isRest = false;
                var pitchElement = noteElement.Element("pitch");
                if (pitchElement != null)
                {
                    var step = pitchElement.Element("step");
                    var octave = pitchElement.Element("octave");
                    if (step != null && octave != null)
                    {
                        note.pitch = step.Value + octave.Value;
                    }
                }
            }

            note.startTime = currentTime;
            notes.Add(note);
            currentTime += note.duration;
        }

        Debug.Log($"Loaded {notes.Count} notes, BPM: {xmlBpm}");
    }

    public void StartGame()
    {
        isPlaying = true;
        currentTime = 0f;

        // Spawn notes that should have already been spawned before game started
        SpawnInitialNotes();

        // Start game mode
        if (gameModeManager != null)
        {
            gameModeManager.StartMode();
        }

        Debug.Log("Game started! Press P to pause, R to restart.");
    }

    void StartGameFromFirstHit(NoteData hitNote)
    {
        isWaitingForFirstHit = false;
        isPlaying = true;

        // Sync timing to the note that was actually hit
        currentTime = hitNote.startTime;
        Debug.Log($"First note hit! Game started! Syncing to note '{hitNote.pitch}' at {currentTime:F2}s");

        // Start game mode
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
        isPlaying = false;

        // Pause game mode
        if (gameModeManager != null)
        {
            gameModeManager.PauseMode();
        }

        Debug.Log("Game paused! Press SPACE to resume.");
    }

    public void RestartGame()
    {
        isPlaying = false;
        isWaitingForFirstHit = true;
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

        Debug.Log("Game restarted! Pre-spawned notes for first 5 seconds. Hit the first note to start!");
    }

    // Public properties for other scripts to access
    public bool IsPlaying => isPlaying;
    public bool IsSetupComplete => isSetupComplete;
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
        if (isWaitingForFirstHit)
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
        // Spawn new notes based on offset before their hit time
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
                    noteController.UpdatePosition(currentTime, noteSpawnOffset, noteArrivalOffset);
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
        foreach (var noteObj in activeNotes)
        {
            if (noteObj != null && noteObj.name.Contains(note.pitch) &&
                noteObj.name.Contains(note.startTime.ToString("F1")))
            {
                return true;
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
            // Fallback: create from scratch
            noteObj = new GameObject($"Note_{note.pitch}_{note.startTime:F1}_{note.duration:F1}");
            noteController = noteObj.AddComponent<NoteController>();

            // Get the existing SpriteRenderer from the NoteController or add one if it doesn't exist
            SpriteRenderer sr = noteController.spriteRenderer;
            if (sr == null)
            {
                sr = noteObj.GetComponent<SpriteRenderer>();
                if (sr == null)
                {
                    sr = noteObj.AddComponent<SpriteRenderer>();
                }
                noteController.spriteRenderer = sr;
            }

            // Set the visual properties using board colors
            sr.color = gameBoard.GetLaneColor(note.pitch);
            if (sr.sprite == null)
            {
                sr.sprite = CreateSquareSprite();
            }
        }

        // Initialize with GameplayManager reference
        if (noteController != null)
        {
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
                    if (noteController.HasCompletelyPassedBoard()
                    )
                    {
                        // targetBarController?.OnReleaseHitEffect();
                        noteController.Release();
                        noteController.UnlinkFromPianoKey();

                    }
                    float timeSinceSpawn = currentTime - noteController.noteData.startTime;
                    if (timeSinceSpawn > 4f) // Remove after 4 seconds
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

        // Clean up dictionary
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
    }



    Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }

    // Add method to set BPM and time scale
    public void SetBPM(float newBPM)
    {
        currentBPM = newBPM;
        timeScale = newBPM / originalBPM;

        // Update note travel time to maintain visual consistency
        float speedMultiplier = currentBPM / originalBPM;
        noteTravelTime = 3f / speedMultiplier;

        Debug.Log($"BPM changed to {newBPM}, Time scale: {timeScale:F2}, Note travel time: {noteTravelTime:F2}");
    }

    // Add method to reset BPM to original
    public void ResetBPM()
    {
        currentBPM = originalBPM;
        timeScale = 1f;
        noteTravelTime = 3f;

        Debug.Log($"BPM reset to original: {originalBPM}");
    }
}
