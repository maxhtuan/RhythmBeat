using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;

[System.Serializable]
public class GameplaySession
{
    public string sessionId;
    public string startTime;
    public string endTime;
    public float totalDuration;
    public float totalHit;
    public float totalMissed;
    public string gameMode;
    public float initialBPM;
    public float finalBPM;
    public int totalNotes;
    public int notesHit;
    public int notesMissed;
    public float averageAccuracy;
    public List<NoteEvent> noteEvents;
    public List<BPMChange> bpmChanges;
    public GameplayStats stats;
}

[System.Serializable]
public class NoteEvent
{
    public string timestamp;
    public string notePitch;
    public int notePosition;
    public float noteStartTime;
    public string eventType; // "hit", "miss", "release"
    public float accuracy;
    public float hitTime;
    public bool isRest;
}

[System.Serializable]
public class BPMChange
{
    public string timestamp;
    public float oldBPM;
    public float newBPM;
    public string reason; // "manual", "speed_up", "pattern_complete"
}

[System.Serializable]
public class GameplayStats
{
    public float bestAccuracy;
    public float worstAccuracy;
    public int consecutiveHits;
    public int maxConsecutiveHits;
    public int consecutiveMisses;
    public int maxConsecutiveMisses;
    public float totalPlayTime;
    public int patternsCompleted;
}

public class GameplayLogger : MonoBehaviour, IService
{
    private GameplaySession currentSession;
    private bool isLogging = false;
    private float sessionStartTime;
    private int consecutiveHits = 0;
    private int consecutiveMisses = 0;
    private int maxConsecutiveHits = 0;
    private int maxConsecutiveMisses = 0;
    private float bestAccuracy = 0f;
    private float worstAccuracy = 1f;
    private int patternsCompleted = 0;

    public void Initialize()
    {
        Debug.Log("GameplayLogger: Initialized");
    }

    public void StartSession(string gameMode, float initialBPM, int totalNotes)
    {
        currentSession = new GameplaySession
        {
            sessionId = Guid.NewGuid().ToString(),
            startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            gameMode = gameMode,
            initialBPM = initialBPM,
            finalBPM = initialBPM,
            totalNotes = totalNotes,
            notesHit = 0,
            notesMissed = 0,
            averageAccuracy = 0f,
            noteEvents = new List<NoteEvent>(),
            bpmChanges = new List<BPMChange>(),
            stats = new GameplayStats()
        };

        sessionStartTime = Time.time;
        isLogging = true;

        // Reset stats
        consecutiveHits = 0;
        consecutiveMisses = 0;
        maxConsecutiveHits = 0;
        maxConsecutiveMisses = 0;
        bestAccuracy = 0f;
        worstAccuracy = 1f;
        patternsCompleted = 0;

        Debug.Log($"GameplayLogger: Session started - ID: {currentSession.sessionId}, Mode: {gameMode}, BPM: {initialBPM}");
    }

    public void LogNoteHit(string notePitch, int notePosition, float noteStartTime, float accuracy, float hitTime, bool isRest)
    {
        if (!isLogging || currentSession == null) return;

        var noteEvent = new NoteEvent
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            notePitch = notePitch,
            notePosition = notePosition,
            noteStartTime = noteStartTime,
            eventType = "hit",
            accuracy = accuracy,
            hitTime = hitTime,
            isRest = isRest
        };

        currentSession.noteEvents.Add(noteEvent);
        currentSession.notesHit++;

        // Update consecutive hits
        consecutiveHits++;
        consecutiveMisses = 0;
        if (consecutiveHits > maxConsecutiveHits)
        {
            maxConsecutiveHits = consecutiveHits;
        }

        // Update accuracy stats
        if (accuracy > bestAccuracy) bestAccuracy = accuracy;
        if (accuracy < worstAccuracy) worstAccuracy = accuracy;

        Debug.Log($"GameplayLogger: Note hit - {notePitch} (pos: {notePosition}), accuracy: {accuracy:F3}");
    }

    public void LogNoteMiss(string notePitch, int notePosition, float noteStartTime, bool isRest)
    {
        if (!isLogging || currentSession == null) return;

        var noteEvent = new NoteEvent
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            notePitch = notePitch,
            notePosition = notePosition,
            noteStartTime = noteStartTime,
            eventType = "miss",
            accuracy = 0f,
            hitTime = 0f,
            isRest = isRest
        };

        currentSession.noteEvents.Add(noteEvent);
        currentSession.notesMissed++;

        // Update consecutive misses
        consecutiveMisses++;
        consecutiveHits = 0;
        if (consecutiveMisses > maxConsecutiveMisses)
        {
            maxConsecutiveMisses = consecutiveMisses;
        }

        Debug.Log($"GameplayLogger: Note missed - {notePitch} (pos: {notePosition})");
    }

    public void LogNoteRelease(string notePitch, int notePosition, float noteStartTime, bool isRest)
    {
        if (!isLogging || currentSession == null) return;

        var noteEvent = new NoteEvent
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            notePitch = notePitch,
            notePosition = notePosition,
            noteStartTime = noteStartTime,
            eventType = "release",
            accuracy = 0f,
            hitTime = 0f,
            isRest = isRest
        };

        currentSession.noteEvents.Add(noteEvent);
        Debug.Log($"GameplayLogger: Note released - {notePitch} (pos: {notePosition})");
    }

    public void LogBPMChange(float oldBPM, float newBPM, string reason)
    {
        if (!isLogging || currentSession == null) return;

        var bpmChange = new BPMChange
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            oldBPM = oldBPM,
            newBPM = newBPM,
            reason = reason
        };

        currentSession.bpmChanges.Add(bpmChange);
        currentSession.finalBPM = newBPM;

        Debug.Log($"GameplayLogger: BPM changed - {oldBPM} → {newBPM} ({reason})");
    }

    public void LogPatternComplete()
    {
        if (!isLogging || currentSession == null) return;

        patternsCompleted++;
        Debug.Log($"GameplayLogger: Pattern completed - Total: {patternsCompleted}");
    }

    public void EndSession()
    {
        if (!isLogging || currentSession == null) return;

        float totalDuration = Time.time - sessionStartTime;
        currentSession.endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        currentSession.totalDuration = totalDuration;
        currentSession.totalHit = currentSession.notesHit;
        currentSession.totalMissed = currentSession.notesMissed;
        isLogging = false;

        // Output the complete session data as JSON
        string jsonData = JsonUtility.ToJson(currentSession, true);
        Debug.Log("=== GAMEPLAY SESSION DATA ===");
        Debug.Log(jsonData);
        var firebaseManager = ServiceLocator.Instance.GetService<FirebaseManager>();
        firebaseManager.SendMessageToReact("TotalHit", currentSession.notesHit.ToString());
        firebaseManager.SendMessageToReact("TotalMissed", currentSession.notesMissed.ToString());
        firebaseManager.SendMessageToReact("TotalDuration", currentSession.totalDuration.ToString());
        Debug.Log("=== END SESSION DATA ===");

        // Also log a summary
        LogSessionSummary();
    }

    private void LogSessionSummary()
    {
        if (currentSession == null) return;

        Debug.Log($"=== SESSION SUMMARY ===");
        Debug.Log($"Session ID: {currentSession.sessionId}");
        Debug.Log($"Duration: {currentSession.totalDuration:F2}s");
        Debug.Log($"Game Mode: {currentSession.gameMode}");
        Debug.Log($"BPM: {currentSession.initialBPM} → {currentSession.finalBPM}");
        Debug.Log($"Notes: {currentSession.notesHit} hit, {currentSession.notesMissed} missed");
        Debug.Log($"Accuracy: {currentSession.averageAccuracy:P1}");
        Debug.Log($"Best Streak: {maxConsecutiveHits} hits");
        Debug.Log($"Patterns Completed: {patternsCompleted}");
        Debug.Log($"Total Events: {currentSession.noteEvents.Count}");
        Debug.Log($"BPM Changes: {currentSession.bpmChanges.Count}");
        Debug.Log($"=== END SUMMARY ===");
    }

    public GameplaySession GetCurrentSession()
    {
        return currentSession;
    }

    public bool IsLogging()
    {
        return isLogging;
    }

    // Get current totals during gameplay
    public int GetTotalHits()
    {
        return currentSession?.notesHit ?? 0;
    }

    public int GetTotalMisses()
    {
        return currentSession?.notesMissed ?? 0;
    }

    public int GetTotalNotes()
    {
        return (currentSession?.notesHit ?? 0) + (currentSession?.notesMissed ?? 0);
    }

    public float GetHitRate()
    {
        int total = GetTotalNotes();
        if (total == 0) return 0f;
        return (float)GetTotalHits() / total;
    }

    // Get current accuracy stats
    public float GetCurrentAccuracy()
    {
        return currentSession?.averageAccuracy ?? 0f;
    }

    public int GetMaxConsecutiveHits()
    {
        return maxConsecutiveHits;
    }

    public int GetMaxConsecutiveMisses()
    {
        return maxConsecutiveMisses;
    }

    // Get real-time summary during gameplay
    public string GetRealTimeSummary()
    {
        if (currentSession == null) return "No active session";

        int hits = GetTotalHits();
        int misses = GetTotalMisses();
        int total = GetTotalNotes();
        float hitRate = GetHitRate();
        float accuracy = GetCurrentAccuracy();

        return $"Hits: {hits} | Misses: {misses} | Total: {total} | Hit Rate: {hitRate:P1} | Accuracy: {accuracy:P1} | Best Streak: {maxConsecutiveHits}";
    }

    // Log current totals to console
    public void LogCurrentTotals()
    {
        if (currentSession == null) return;

        Debug.Log($"=== CURRENT TOTALS ===");
        Debug.Log($"Total Hits: {GetTotalHits()}");
        Debug.Log($"Total Misses: {GetTotalMisses()}");
        Debug.Log($"Total Notes: {GetTotalNotes()}");
        Debug.Log($"Hit Rate: {GetHitRate():P1}");
        Debug.Log($"Current Accuracy: {GetCurrentAccuracy():P1}");
        Debug.Log($"Best Streak: {maxConsecutiveHits} hits");
        Debug.Log($"Worst Streak: {maxConsecutiveMisses} misses");
        Debug.Log($"Patterns Completed: {patternsCompleted}");
        Debug.Log($"=== END TOTALS ===");
    }

    public void Cleanup()
    {
        if (isLogging)
        {
            EndSession();
        }
        Debug.Log("GameplayLogger: Cleaned up");
    }
}
