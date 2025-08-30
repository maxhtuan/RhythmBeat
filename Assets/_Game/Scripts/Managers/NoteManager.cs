using System.Collections.Generic;
using UnityEngine;

public class NoteManager
{
    private SongData currentSong;
    private Transform spawnPoint;
    private Transform targetPoint;
    private GameObject notePrefab;
    private List<NoteData> activeNotes = new List<NoteData>();
    private List<GameObject> spawnedNoteObjects = new List<GameObject>();

    public void Initialize(SongData song, Transform spawn, Transform target, GameObject prefab)
    {
        currentSong = song;
        spawnPoint = spawn;
        targetPoint = target;
        notePrefab = prefab;
        activeNotes.Clear();
        spawnedNoteObjects.Clear();
    }

    public void UpdateNotes(float currentTime, float hitWindow)
    {
        // Spawn new notes that should appear
        SpawnNewNotes(currentTime);

        // Update existing note positions
        UpdateNotePositions(currentTime);

        // Check for missed notes
        CheckMissedNotes(currentTime, hitWindow);
    }

    private void SpawnNewNotes(float currentTime)
    {
        if (currentSong == null) return;

        foreach (var note in currentSong.allNotes)
        {
            if (note.startTime <= currentTime + 3f && // Spawn 3 seconds ahead
                note.startTime > currentTime - 0.1f && // Don't spawn if already past
                !note.isHit &&
                !note.isMissed &&
                !activeNotes.Contains(note))
            {
                SpawnNote(note);
            }
        }
    }

    private void SpawnNote(NoteData note)
    {
        if (notePrefab == null || spawnPoint == null) return;

        GameObject noteObject = GameObject.Instantiate(notePrefab, spawnPoint.position, spawnPoint.rotation);
        noteObject.name = $"Note_{note.pitch}_{note.startTime}";

        // Set note properties
        NoteController noteController = noteObject.GetComponent<NoteController>();
        if (noteController != null)
        {
            noteController.Initialize(note, spawnPoint.position, targetPoint.position);
        }

        // Store references
        note.noteObject = noteObject;
        activeNotes.Add(note);
        spawnedNoteObjects.Add(noteObject);
    }

    private void UpdateNotePositions(float currentTime)
    {
        foreach (var note in activeNotes)
        {
            if (note.noteObject != null)
            {
                NoteController controller = note.noteObject.GetComponent<NoteController>();
                if (controller != null)
                {
                    controller.UpdatePosition(currentTime);
                }
            }
        }
    }

    private void CheckMissedNotes(float currentTime, float hitWindow)
    {
        List<NoteData> notesToRemove = new List<NoteData>();

        foreach (var note in activeNotes)
        {
            if (currentTime > note.startTime + hitWindow && !note.isHit)
            {
                note.isMissed = true;
                notesToRemove.Add(note);

                // Trigger miss event
                OnNoteMissed(note);
            }
        }

        foreach (var note in notesToRemove)
        {
            RemoveNote(note);
        }
    }

    public void RemoveNote(NoteData note)
    {
        if (activeNotes.Contains(note))
        {
            activeNotes.Remove(note);
        }

        if (note.noteObject != null)
        {
            spawnedNoteObjects.Remove(note.noteObject);
            GameObject.Destroy(note.noteObject);
            note.noteObject = null;
        }
    }

    public void ClearAllNotes()
    {
        foreach (var note in activeNotes)
        {
            if (note.noteObject != null)
            {
                GameObject.Destroy(note.noteObject);
            }
        }

        activeNotes.Clear();
        spawnedNoteObjects.Clear();
    }

    public List<NoteData> GetActiveNotes()
    {
        return new List<NoteData>(activeNotes);
    }

    public NoteData GetNoteAtPosition(Vector3 position, float hitWindow)
    {
        foreach (var note in activeNotes)
        {
            if (note.noteObject != null)
            {
                float distance = Vector3.Distance(note.noteObject.transform.position, position);
                if (distance <= hitWindow)
                {
                    return note;
                }
            }
        }
        return null;
    }

    // Events
    private void OnNoteMissed(NoteData note)
    {
        // This will be called when a note is missed
        // You can add visual/audio feedback here
        Debug.Log($"Note missed: {note.pitch} at time {note.startTime}");
    }
}
