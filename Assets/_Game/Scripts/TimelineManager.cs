using UnityEngine;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

public class TimelineManager : MonoBehaviour
{
    [Header("References")]
    public GameplayManager gameplayManager;
    public GameBoard gameBoard;
    public GameObject linePrefab;

    [Header("Timeline Settings")]
    public float bpm = 60f;
    public float timelineWidth = 10f;
    public float timelineHeight = 2f;
    public int linesPerBeat = 1;
    public int beatsPerMeasure = 4; // Add missing field
    public Color timelineLineColor = Color.white; // Add missing field
    public float timelineLineAlpha = 0.3f; // Add missing field

    private List<GameObject> timelineLines = new List<GameObject>();
    private bool isInitialized = false;
    private float songDuration = 0f;
    private SongHandler songHandler;
    // Remove Start() method - initialization will be called from GameplayManager

    void Update()
    {
        if (isInitialized && gameplayManager != null && gameplayManager.IsPlaying)
        {
            UpdateTimelineLines();
        }
    }
    public NoteManager noteManager;
    public void Initialize()
    {
        if (isInitialized) return;

        // Get references if not assigned
        if (gameplayManager == null)
            gameplayManager = ServiceLocator.Instance.GetService<GameplayManager>();
        if (songHandler == null)
            songHandler = ServiceLocator.Instance.GetService<SongHandler>();
        if (gameBoard == null)
            gameBoard = ServiceLocator.Instance.GetService<GameBoard>();
        if (noteManager == null)
            noteManager = ServiceLocator.Instance.GetService<NoteManager>();
        InitializeTimeline();
        isInitialized = true;

        Debug.Log("TimelineManager: Initialized");
    }

    public void InitializeTimeline()
    {
        if (gameplayManager == null || gameBoard == null || linePrefab == null)
        {
            Debug.LogError("TimelineManager: Missing required references!");
            return;
        }

        // Get BPM from GameplayManager's loaded notes
        LoadBPMFromNotes();

        // Calculate song duration from notes
        CalculateSongDuration();

        // Create timeline lines
        CreateTimelineLines();

        Debug.Log($"TimelineManager initialized: BPM={bpm}, Duration={songDuration:F2}s");
    }

    void LoadBPMFromNotes()
    {
        // Load BPM from the XML file, just like GameplayManager does
        TextAsset xmlFile = Resources.Load<TextAsset>("song");
        if (xmlFile == null)
        {
            Debug.LogError("TimelineManager: Could not load song.xml!");
            return;
        }

        // Parse XML to get BPM
        var doc = XDocument.Parse(xmlFile.text);
        float xmlBpm = 60f;

        // Get BPM from metronome element
        var metronome = doc.Descendants("metronome").FirstOrDefault();
        if (metronome != null)
        {
            var perMinute = metronome.Element("per-minute");
            if (perMinute != null)
            {
                xmlBpm = float.Parse(perMinute.Value);
                // Update the BPM setting to match the XML file
                bpm = xmlBpm;
                Debug.Log($"TimelineManager: Loaded BPM from XML: {xmlBpm}");
            }
        }
        else
        {
            Debug.LogWarning("TimelineManager: No metronome found in XML, using default BPM");
        }
    }


    List<NoteData> notes => noteManager.GetAllNotes();

    void CalculateSongDuration()
    {
        if (notes.Count > 0)
        {
            NoteData firstNote = notes[0];
            NoteData lastNote = notes[notes.Count - 1];

            // Calculate song duration from first note to last note
            float firstBeatTime = firstNote.startTime;
            float lastBeatTime = lastNote.startTime + lastNote.duration;
            songDuration = lastBeatTime - firstBeatTime;

            Debug.Log($"Song duration: {firstBeatTime:F2}s to {lastBeatTime:F2}s (duration: {songDuration:F2}s)");
        }
        else
        {
            songDuration = 60f; // Default duration
        }
    }

    void CreateTimelineLines()
    {
        // Clear existing lines
        ClearTimelineLines();

        if (notes.Count == 0) return;

        float secondsPerBeat = 60f / bpm;
        float secondsPerMeasure = secondsPerBeat * beatsPerMeasure;

        // Get the first note time to align timeline with music
        float firstNoteTime = notes[0].startTime;

        // Calculate how many measures we need
        int totalMeasures = Mathf.CeilToInt(songDuration / secondsPerMeasure);

        Debug.Log($"Creating timeline: First note at {firstNoteTime:F2}s, Duration = {songDuration:F2}s, BPM = {bpm}, Measures = {totalMeasures}");

        // Create timeline lines
        for (int i = 0; i < totalMeasures; i++)
        {
            GameObject line = Instantiate(linePrefab);
            line.name = $"TimelineLine_{i}";

            // Set line properties
            SpriteRenderer lineRenderer = line.GetComponent<SpriteRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.color = new Color(timelineLineColor.r, timelineLineColor.g, timelineLineColor.b, timelineLineAlpha);
            }

            // Set line height to match navigation bar height
            if (gameplayManager != null && gameplayManager.TargetBarController != null)
            {
                // Get the navigation bar's Y scale (height)
                float navBarHeight = gameplayManager.TargetBarController.transform.localScale.y;
                // line.transform.localScale = new Vector3(0.05f, navBarHeight, 1f);
            }

            // Position line at the correct time (aligned with first beat)
            float lineTime = firstNoteTime + (i * secondsPerMeasure);
            Vector3 linePosition = CalculateTimelinePosition(lineTime);
            line.transform.position = linePosition;

            timelineLines.Add(line);
        }

        Debug.Log($"Created {timelineLines.Count} timeline lines starting from first beat at {firstNoteTime:F2}s");
    }

    Vector3 CalculateTimelinePosition(float time)
    {
        if (gameBoard == null) return Vector3.zero;

        // Calculate how far the line should be based on time
        float totalDistance = Vector3.Distance(gameBoard.GetSpawnPosition("C"), gameBoard.GetTargetPosition("C"));
        float travelSpeed = totalDistance / songHandler.noteTravelTime;
        float distanceTraveled = time * travelSpeed;

        // Position line along the travel path
        Vector3 startPos = gameBoard.GetSpawnPosition("C");
        Vector3 endPos = gameBoard.GetTargetPosition("C");
        Vector3 direction = (endPos - startPos).normalized;

        // Calculate the position along the travel path
        Vector3 position = startPos + (direction * distanceTraveled);

        // Ensure the line is at the same height as the game board
        // Use the game board's center Y position
        position.y = gameBoard.transform.position.y;

        return position;
    }

    void UpdateTimelineLines()
    {
        float currentTime = gameplayManager.GetCurrentTime();

        if (notes.Count == 0) return;

        // Get the first note time to align timeline with music
        float firstNoteTime = notes[0].startTime;

        for (int i = 0; i < timelineLines.Count; i++)
        {
            if (timelineLines[i] == null) continue;

            // Calculate line time (aligned with first beat)
            float secondsPerBeat = 60f / bpm;
            float secondsPerMeasure = secondsPerBeat * beatsPerMeasure;
            float lineTime = firstNoteTime + (i * secondsPerMeasure);

            // Calculate time until this line should arrive at target
            float timeUntilArrival = lineTime - currentTime;

            // For lines moving from right to left:
            // - Positive timeUntilArrival = line is in the future (should be at spawn)
            // - Zero timeUntilArrival = line should be at target
            // - Negative timeUntilArrival = line has passed target

            // Update position - lines move from spawn to target
            // We need to invert the time so lines move in the correct direction
            Vector3 newPosition = CalculateTimelinePosition(-timeUntilArrival);
            timelineLines[i].transform.position = newPosition;

            // Show/hide line based on whether it should be visible
            bool shouldBeVisible = timeUntilArrival > -songHandler.noteTravelTime && timeUntilArrival < songHandler.noteSpawnOffset;
            timelineLines[i].SetActive(shouldBeVisible);

            // Debug first line to see what's happening
            if (i == 0 && currentTime % 2f < 0.1f) // Log every 2 seconds
            {
                Debug.Log($"Timeline line {i}: lineTime={lineTime:F2}, timeUntilArrival={timeUntilArrival:F2}, pos={newPosition}, visible={shouldBeVisible}");
            }
        }
    }

    public void StartTimeline()
    {
        Debug.Log("Timeline started");
        // Timeline automatically updates when game is playing
    }

    public void PauseTimeline()
    {
        Debug.Log("Timeline paused");
        // Timeline automatically stops updating when game is paused
    }

    public void RestartTimeline()
    {
        Debug.Log("Timeline restarted");
        InitializeTimeline();
    }

    void ClearTimelineLines()
    {
        foreach (var line in timelineLines)
        {
            if (line != null)
            {
                Destroy(line);
            }
        }
        timelineLines.Clear();
    }

    void OnDestroy()
    {
        ClearTimelineLines();
    }
}
