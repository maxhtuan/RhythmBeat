using UnityEngine;

public class NoteMovement : MonoBehaviour
{
    [Header("Movement Properties")]
    public NoteData noteData;
    public float moveSpeed = 5f;

    protected Vector3 startPosition;
    protected Vector3 endPosition;
    protected float travelTime;
    protected float hitWindow = 0.2f;

    // References
    private NoteManager noteManager;
    private GameplayManager gameplayManager;
    protected SongHandler songHandler;
    private GameSettingsManager gameSettingsManager;
    // Board passing detection
    protected bool hasPassedBoard = false;

    public void Initialize(NoteData note, Vector3 start, Vector3 end, float travelTime = 3f, NoteManager nm = null)
    {
        noteData = note;
        startPosition = start;
        endPosition = end;
        this.travelTime = travelTime;
        noteManager = nm;

        // Get services
        gameplayManager = ServiceLocator.Instance.GetService<GameplayManager>();
        songHandler = ServiceLocator.Instance.GetService<SongHandler>();
        gameSettingsManager = ServiceLocator.Instance.GetService<GameSettingsManager>();
        if (gameplayManager != null)
        {
            hitWindow = gameSettingsManager.GetHitWindow();
        }
    }

    public virtual void UpdatePosition(float currentTime, float noteSpawnOffset = 3f, float noteArrivalOffset = 0f)
    {
        // Check if the note has completely passed the board
        if (!hasPassedBoard && CheckIfPassedBoard(currentTime))
        {
            hasPassedBoard = true;
        }

        // Calculate the total travel distance
        float totalDistance = Vector3.Distance(startPosition, endPosition);

        // Get current travel speed
        float currentTravelSpeed = songHandler != null ?
            (totalDistance / songHandler.GetNoteTravelTime()) :
            (totalDistance / travelTime);

        // Use original XML timing
        float noteArrivalTime = noteData.startTime + noteArrivalOffset;

        // Calculate when this note should start its journey
        float noteStartTime = noteArrivalTime - (totalDistance / currentTravelSpeed);

        // Calculate how much time has passed since this note should have started
        float timeSinceNoteShouldStart = currentTime - noteStartTime;

        // Calculate distance traveled
        float distanceTraveled = timeSinceNoteShouldStart * currentTravelSpeed;

        // Position the note
        Vector3 direction = (endPosition - startPosition).normalized;
        transform.position = startPosition + (direction * distanceTraveled);
    }

    // Calculate if the note has completely passed the board
    protected virtual bool CheckIfPassedBoard(float currentTime)
    {
        // Use original XML timing
        float noteArrivalTime = noteData.startTime;

        // Calculate the total time the note needs to completely pass the board
        // This includes the note's duration (tail length)
        float totalNoteTime = noteArrivalTime + noteData.duration;

        // Check if current time is past the total note time
        return currentTime > totalNoteTime;
    }

    // Check if the entire note (including tail) has passed the board
    public bool HasCompletelyPassedBoard()
    {
        return hasPassedBoard;
    }

    // Get the total travel distance
    public float GetTotalDistance()
    {
        return Vector3.Distance(startPosition, endPosition);
    }

    // Get current travel speed
    public float GetCurrentTravelSpeed()
    {
        float totalDistance = GetTotalDistance();
        return songHandler != null ?
            (totalDistance / songHandler.GetNoteTravelTime()) :
            (totalDistance / travelTime);
    }

    // Get original travel speed (not affected by BPM changes)
    public float GetOriginalTravelSpeed()
    {
        float totalDistance = GetTotalDistance();
        return gameplayManager != null ? gameplayManager.GetOriginalTravelSpeed() : (totalDistance / travelTime);
    }

    // Get note arrival time
    public virtual float GetNoteArrivalTime()
    {
        return noteData.startTime;
    }

    // Get hit window
    public float GetHitWindow()
    {
        return hitWindow;
    }

    // Get start position
    public Vector3 GetStartPosition()
    {
        return startPosition;
    }

    // Get end position
    public Vector3 GetEndPosition()
    {
        return endPosition;
    }
}
