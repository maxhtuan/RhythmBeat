using UnityEngine;
using System.Collections.Generic;

public class PianoInputHandler : MonoBehaviour
{
    [Header("References")]
    public GameplayManager gameplayManager;

    [Header("Piano Keys")]
    public KeyCode[] pianoKeys = {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7
    };

    [Header("Key Labels")]
    public string[] keyLabels = { "C", "D", "E", "F", "G", "A", "B" };

    private Dictionary<KeyCode, int> keyToNoteMap = new Dictionary<KeyCode, int>();
    private Dictionary<KeyCode, bool> keyPressed = new Dictionary<KeyCode, bool>();
    public NoteManager noteManager;
    void Start()
    {
        if (noteManager == null)
        {
            noteManager = ServiceLocator.Instance.GetService<NoteManager>();
        }

        InitializeKeyMappings();
    }

    void Update()
    {
        HandlePianoInput();
    }

    private void InitializeKeyMappings()
    {
        for (int i = 0; i < pianoKeys.Length; i++)
        {
            keyToNoteMap[pianoKeys[i]] = i;
            keyPressed[pianoKeys[i]] = false;
        }
    }

    private void HandlePianoInput()
    {
        if (gameplayManager == null || !gameplayManager.IsPlaying) return;

        foreach (var key in pianoKeys)
        {
            if (UnityEngine.Input.GetKeyDown(key) && !keyPressed[key])
            {
                OnKeyPressed(key);
                keyPressed[key] = true;
            }
            else if (UnityEngine.Input.GetKeyUp(key))
            {
                OnKeyReleased(key);
                keyPressed[key] = false;
            }
        }
    }

    private void OnKeyPressed(KeyCode key)
    {
        if (keyToNoteMap.ContainsKey(key))
        {
            int noteIndex = keyToNoteMap[key];
            string noteName = keyLabels[noteIndex];

            // Find the closest note to hit
            NoteData closestNote = FindClosestNote(noteIndex);
            if (closestNote != null)
            {
                float accuracy = CalculateHitAccuracy(closestNote);
                // gameplayManager.OnNoteHit(closestNote, accuracy);

                Debug.Log($"Hit {noteName} with accuracy: {accuracy:F2}");
            }
            else
            {
                Debug.Log($"Pressed {noteName} but no note to hit");
            }
        }
    }

    private void OnKeyReleased(KeyCode key)
    {
        // Handle key release if needed
        // For now, we only care about key presses
    }

    private NoteData FindClosestNote(int noteIndex)
    {
        if (gameplayManager == null) return null;

        float currentTime = gameplayManager.GetCurrentTime();
        float hitWindow = gameplayManager.hitWindow;

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
                // Check if the note matches the pressed key
                if (IsNoteMatch(note, noteIndex))
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

    private bool IsNoteMatch(NoteData note, int noteIndex)
    {
        if (note.isRest) return false;

        // Convert note index to expected pitch
        string expectedPitch = keyLabels[noteIndex % 12];

        // Simple pitch matching - check if the note pitch starts with the expected pitch
        return note.pitch.StartsWith(expectedPitch);
    }

    private float CalculateHitAccuracy(NoteData note)
    {
        if (gameplayManager == null) return 0f;

        float currentTime = gameplayManager.GetCurrentTime();
        float noteArrivalTime = GetNoteArrivalTime(note);
        float timeDiff = Mathf.Abs(noteArrivalTime - currentTime);
        float hitWindow = gameplayManager.hitWindow;

        // Calculate accuracy based on timing
        float accuracy = 1f - (timeDiff / hitWindow);
        return Mathf.Clamp01(accuracy);
    }

    // Public methods for UI
    public string GetKeyLabel(KeyCode key)
    {
        if (keyToNoteMap.ContainsKey(key))
        {
            return keyLabels[keyToNoteMap[key]];
        }
        return "";
    }

    public bool IsKeyPressed(KeyCode key)
    {
        return keyPressed.ContainsKey(key) && keyPressed[key];
    }
}
