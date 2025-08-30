using UnityEngine;

public class SongHandler : MonoBehaviour, IService
{
    [Header("Song Timing")]
    public float originalBPM = 60f; // Original BPM from XML
    public float currentBPM = 60f; // Current BPM (can be modified by game modes)
    public float timeScale = 1f; // Time scaling factor (1.0 = normal speed)

    [Header("Note Timing")]
    public float noteSpawnOffset = 3f; // How many seconds before hit time to spawn notes
    public float noteTravelTime = 3f; // How long notes take to travel from spawn to target
    public float noteArrivalOffset = 0f; // How many seconds before hit time notes should arrive (negative = early, positive = late)

    // Store the original travel speed for consistent visual length
    private float originalTravelSpeed = 0f;

    // Reference to GameBoard for calculating travel speed
    private GameBoard gameBoard;

    public void SetBPM(float newBPM, float maxBPM = 0f)
    {
        // Check max BPM limit
        if (maxBPM > 0)
        {
            newBPM = Mathf.Min(newBPM, maxBPM);
        }

        currentBPM = newBPM;
        timeScale = 1;

        // Update note travel time to maintain visual consistency
        float speedMultiplier = currentBPM / originalBPM;
        noteTravelTime = 3f / speedMultiplier;

        // Calculate and store the original travel speed if not already done
        if (originalTravelSpeed == 0f && gameBoard != null)
        {
            Vector3 spawnPos = gameBoard.GetSpawnPosition("C");
            Vector3 targetPos = gameBoard.GetTargetPosition("C");
            float totalDistance = Vector3.Distance(spawnPos, targetPos);
            originalTravelSpeed = totalDistance / 3f; // Based on original noteTravelTime of 3f
        }

        Debug.Log($"BPM changed to {newBPM}, Time scale: {timeScale:F2}, Note travel time: {noteTravelTime:F2}");
    }

    public void ResetBPM()
    {
        currentBPM = originalBPM;
        timeScale = 1f;
        noteTravelTime = 3f;

        Debug.Log($"BPM reset to original: {originalBPM}");
    }

    public float GetSpeedUpMultiplier()
    {
        return currentBPM / originalBPM;
    }

    public void SetOriginalBPM(float bpm)
    {
        originalBPM = bpm;
        currentBPM = bpm;
        timeScale = 1f;
        Debug.Log($"Original BPM set to: {bpm}");
    }

    public float GetOriginalTravelSpeed()
    {
        // If originalTravelSpeed is still 0, calculate it now
        if (originalTravelSpeed == 0f && gameBoard != null)
        {
            InitializeOriginalTravelSpeed();
        }

        // Fallback if still 0
        if (originalTravelSpeed == 0f)
        {
            return 1f; // Return a safe default value
        }

        return originalTravelSpeed;
    }

    private void InitializeOriginalTravelSpeed()
    {
        if (originalTravelSpeed == 0f && gameBoard != null)
        {
            Vector3 spawnPos = gameBoard.GetSpawnPosition("C");
            Vector3 targetPos = gameBoard.GetTargetPosition("C");
            float totalDistance = Vector3.Distance(spawnPos, targetPos);
            originalTravelSpeed = totalDistance / noteTravelTime; // Use the current noteTravelTime
        }
    }

    // Getters for other components to access
    public float GetCurrentBPM() => currentBPM;
    public float GetOriginalBPM() => originalBPM;
    public float GetTimeScale() => timeScale;
    public float GetNoteSpawnOffset() => noteSpawnOffset;
    public float GetNoteTravelTime() => noteTravelTime;
    public float GetNoteArrivalOffset() => noteArrivalOffset;

    public void Initialize()
    {
        gameBoard = ServiceLocator.Instance.GetService<GameBoard>();
        Debug.Log("SongHandler: Initialized");
    }

    public void Cleanup()
    {
        Debug.Log("SongHandler: Cleaned up");
    }
}
