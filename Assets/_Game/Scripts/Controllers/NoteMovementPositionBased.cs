using UnityEngine;

public class NoteMovementPositionBased : NoteMovement
{
    [Header("Position-Based Settings")]
    public float positionSpacing = 1f; // Distance between note positions
    public float baseSpeed = 2f; // Base movement speed

    private int currentPosition = 0;
    private float lastPositionTime = 0f;

    public override void UpdatePosition(float currentTime, float noteSpawnOffset = 3f, float noteArrivalOffset = 0f)
    {
        // Check if the note has completely passed the board
        if (!hasPassedBoard && CheckIfPassedBoardPositionBased(currentTime))
        {
            hasPassedBoard = true;
        }

        // Calculate position-based movement using note's sequence position
        float totalDistance = Vector3.Distance(startPosition, endPosition);
        float currentTravelSpeed = GetCurrentTravelSpeed();

        // Use the note's position in the sequence (noteData.notePosition)
        // Each position represents a beat or time unit
        float notePositionInSequence = noteData.notePosition;

        // Calculate when this note should arrive based on its position
        float beatDuration = 60f / songHandler.GetCurrentBPM(); // Duration of one beat
        float noteArrivalTime = notePositionInSequence * beatDuration;

        // Calculate when this note should start its journey
        float noteStartTime = noteArrivalTime - (totalDistance / currentTravelSpeed);

        // Calculate how much time has passed since this note should have started
        float timeSinceNoteShouldStart = currentTime - noteStartTime;

        // Calculate distance traveled
        float distanceTraveled = timeSinceNoteShouldStart * currentTravelSpeed;

        // Allow the note to continue moving past the target (no clamping)

        // Position the note
        Vector3 direction = (endPosition - startPosition).normalized;
        transform.position = startPosition + (direction * distanceTraveled);
    }

    // Calculate if the note has completely passed the board (position-based)
    private bool CheckIfPassedBoardPositionBased(float currentTime)
    {
        // Use the note's position in the sequence
        float notePositionInSequence = noteData.notePosition;
        float beatDuration = 60f / songHandler.GetCurrentBPM();
        float noteArrivalTime = notePositionInSequence * beatDuration;

        // Calculate the total time the note needs to completely pass the board
        float totalNoteTime = noteArrivalTime + noteData.duration;

        // Check if current time is past the total note time
        return currentTime > totalNoteTime;
    }

    // Set the starting position (smooth float)
    public void SetStartingPosition(float position)
    {
        currentPosition = Mathf.RoundToInt(position);
        lastPositionTime = Time.time;
    }

    // Get current position (note's sequence position)
    public float GetCurrentPosition()
    {
        return noteData.notePosition;
    }

    // Get the total number of positions to the target
    public int GetTotalPositions()
    {
        float totalDistance = Vector3.Distance(startPosition, endPosition);
        return Mathf.CeilToInt(totalDistance / positionSpacing);
    }

    // Get position-based arrival time
    public float GetPositionBasedArrivalTime()
    {
        float notePositionInSequence = noteData.notePosition;
        float beatDuration = 60f / songHandler.GetCurrentBPM();
        return notePositionInSequence * beatDuration;
    }

    // Override to use position-based timing
    public override float GetNoteArrivalTime()
    {
        return GetPositionBasedArrivalTime();
    }

    // Initialize with position-based settings
    public void InitializePositionBased(NoteData note, Vector3 start, Vector3 end, float travelTime = 3f, NoteManager nm = null, float spacing = 1f)
    {
        Initialize(note, start, end, travelTime, nm);
        positionSpacing = spacing;
        currentPosition = 0;
        lastPositionTime = Time.time;
    }
}
