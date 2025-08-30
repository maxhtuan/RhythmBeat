using UnityEngine;

public class ScoreManager : IService
{
    private int currentScore = 0;
    private int currentCombo = 0;
    private int maxCombo = 0;
    private int totalNotes = 0;
    private int hitNotes = 0;
    private int missedNotes = 0;

    // Scoring multipliers
    private const int PERFECT_SCORE = 100;
    private const int GREAT_SCORE = 80;
    private const int GOOD_SCORE = 60;
    private const int OK_SCORE = 40;
    private const int MISS_SCORE = 0;

    // Accuracy thresholds
    private const float PERFECT_THRESHOLD = 0.95f;
    private const float GREAT_THRESHOLD = 0.85f;
    private const float GOOD_THRESHOLD = 0.70f;
    private const float OK_THRESHOLD = 0.50f;

    public void AddScore(float accuracy)
    {
        hitNotes++;
        totalNotes++;

        int scoreToAdd = 0;
        string accuracyRating = "";

        if (accuracy >= PERFECT_THRESHOLD)
        {
            scoreToAdd = PERFECT_SCORE;
            accuracyRating = "Perfect";
        }
        else if (accuracy >= GREAT_THRESHOLD)
        {
            scoreToAdd = GREAT_SCORE;
            accuracyRating = "Great";
        }
        else if (accuracy >= GOOD_THRESHOLD)
        {
            scoreToAdd = GOOD_SCORE;
            accuracyRating = "Good";
        }
        else if (accuracy >= OK_THRESHOLD)
        {
            scoreToAdd = OK_SCORE;
            accuracyRating = "OK";
        }
        else
        {
            scoreToAdd = MISS_SCORE;
            accuracyRating = "Miss";
        }

        // Apply combo multiplier
        scoreToAdd *= (1 + currentCombo / 10); // Every 10 combo adds 1x multiplier

        currentScore += scoreToAdd;
        currentCombo++;

        if (currentCombo > maxCombo)
        {
            maxCombo = currentCombo;
        }

        Debug.Log($"{accuracyRating} hit! Accuracy: {accuracy:F2}, Score: +{scoreToAdd}, Combo: {currentCombo}");
    }

    public void AddMiss()
    {
        missedNotes++;
        totalNotes++;
        currentCombo = 0;

        Debug.Log($"Miss! Combo broken. Total misses: {missedNotes}");
    }

    public void Reset()
    {
        currentScore = 0;
        currentCombo = 0;
        maxCombo = 0;
        totalNotes = 0;
        hitNotes = 0;
        missedNotes = 0;
    }

    // Getters
    public int GetScore() => currentScore;
    public int GetCombo() => currentCombo;
    public int GetMaxCombo() => maxCombo;
    public int GetTotalNotes() => totalNotes;
    public int GetHitNotes() => hitNotes;
    public int GetMissedNotes() => missedNotes;

    public float GetAccuracy()
    {
        if (totalNotes == 0) return 0f;
        return (float)hitNotes / totalNotes * 100f;
    }

    public string GetGrade()
    {
        float accuracy = GetAccuracy();

        if (accuracy >= 95f) return "S";
        if (accuracy >= 90f) return "A";
        if (accuracy >= 80f) return "B";
        if (accuracy >= 70f) return "C";
        if (accuracy >= 60f) return "D";
        return "F";
    }

    public ScoreData GetScoreData()
    {
        return new ScoreData
        {
            score = currentScore,
            combo = currentCombo,
            maxCombo = maxCombo,
            totalNotes = totalNotes,
            hitNotes = hitNotes,
            missedNotes = missedNotes,
            accuracy = GetAccuracy(),
            grade = GetGrade()
        };
    }

    public void Initialize()
    {
        Reset();
        Debug.Log("ScoreManager initialized");
    }

    public void Cleanup()
    {
        Reset();
        Debug.Log("ScoreManager cleaned up");
    }
}

[System.Serializable]
public class ScoreData
{
    public int score;
    public int combo;
    public int maxCombo;
    public int totalNotes;
    public int hitNotes;
    public int missedNotes;
    public float accuracy;
    public string grade;
}
