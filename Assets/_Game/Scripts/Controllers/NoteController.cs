using UnityEngine;

public class NoteController : MonoBehaviour
{
    [Header("Components")]
    public NoteMovement noteMovement; // Can be NoteMovement or NoteMovementPositionBased
    public NoteRenderer noteRenderer;

    [Header("Note Properties")]
    public NoteData noteData;

    [Header("Settings")]
    public NoteManager noteManager;
    public GameplayManager gameplayManager;
    public GameSettingsManager settingsManager;

    // Linking system
    private PianoKey linkedPianoKey = null;
    private bool isLinked = false;

    public void Initialize(NoteData note, Vector3 start, Vector3 end, float travelTime = 3f, NoteManager nm = null)
    {
        noteData = note;
        noteManager = nm;
        gameplayManager = ServiceLocator.Instance.GetService<GameplayManager>();
        settingsManager = ServiceLocator.Instance.GetService<GameSettingsManager>();

        // Initialize components
        if (noteMovement != null)
        {
            noteMovement.Initialize(note, start, end, travelTime, nm);
        }

        if (noteRenderer != null)
        {
            noteRenderer.Initialize(note, noteMovement);
        }

        isHitBefore = false;
    }

    // Link with a PianoKey
    public void LinkWithPianoKey(PianoKey pianoKey)
    {
        isHitBefore = true;
        linkedPianoKey = pianoKey;
        isLinked = true;

        if (noteRenderer != null)
        {
            noteRenderer.Hit();
        }

        Debug.Log($"Note {noteData.GetNoteName()} linked with PianoKey {pianoKey.noteName}");
    }

    // Unlink from PianoKey
    public void UnlinkFromPianoKey()
    {
        if (isLinked)
        {
            linkedPianoKey = null;
            isLinked = false;

            if (noteRenderer != null)
            {
                noteRenderer.Release();
            }

            Debug.Log($"Note {noteData.GetNoteName()} unlinked from PianoKey");
        }
    }

    // Check if note is linked to a specific PianoKey
    public bool IsLinkedTo(PianoKey pianoKey)
    {
        return isLinked && linkedPianoKey == pianoKey;
    }

    // Check if note is currently linked
    public bool IsLinked()
    {
        return isLinked;
    }

    bool isHitBefore = false;

    // Get the linked PianoKey
    public PianoKey GetLinkedPianoKey()
    {
        return linkedPianoKey;
    }

    // Check if the entire note (including tail) has passed the board
    public bool HasCompletelyPassedBoard()
    {
        return noteMovement != null ? noteMovement.HasCompletelyPassedBoard() : false;
    }

    public void UpdatePosition(float currentTime, float noteSpawnOffset = 3f, float noteArrivalOffset = 0f)
    {
        // Update movement
        if (noteMovement != null)
        {
            noteMovement.UpdatePosition(currentTime, noteSpawnOffset, noteArrivalOffset);
        }

        // Update visuals
        if (noteRenderer != null)
        {
            noteRenderer.UpdateVisuals(currentTime);
        }
    }

    public void Hit()
    {
        if (noteRenderer != null)
        {
            noteRenderer.Hit();
        }
    }

    public void Release()
    {
        if (noteRenderer != null)
        {
            noteRenderer.Release();
        }
    }

    public void Miss()
    {
        if (noteRenderer != null)
        {
            noteRenderer.Miss();
        }
        isHitBefore = false;
    }

    // Get note data
    public NoteData GetNoteData()
    {
        return noteData;
    }

    // Get hit state
    public bool IsHit()
    {
        return isHitBefore;
    }

    // Get missed state
    public bool IsMissed()
    {
        return noteRenderer != null ? noteRenderer.IsMissed() : false;
    }
}
