using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml.Linq;
using System.Linq;

public class TimeManager : MonoBehaviour
{
    [Header("UI References")]
    public Slider progressSlider;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI totalTimeText;

    [Header("Settings")]
    public bool showTimeText = true;
    public bool showTotalTimeText = true;

    private float songDuration = 0f;
    private float currentTime = 0f;
    private bool isInitialized = false;

    void Start()
    {
        InitializeTimeManager();
    }

    void Update()
    {
        if (isInitialized)
        {
            UpdateProgress();
        }
    }

    void InitializeTimeManager()
    {
        // Calculate song duration from XML
        CalculateSongDuration();

        // Initialize UI
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = songDuration;
            progressSlider.value = 0f;
        }

        if (totalTimeText != null && showTotalTimeText)
        {
            totalTimeText.text = FormatTime(songDuration);
        }

        isInitialized = true;
        Debug.Log($"TimeManager initialized. Song duration: {songDuration:F2}s");
    }

    void CalculateSongDuration()
    {
        // Load XML file
        TextAsset xmlFile = Resources.Load<TextAsset>("song");
        if (xmlFile == null)
        {
            Debug.LogError("Could not load song.xml for duration calculation!");
            songDuration = 60f; // Default fallback
            return;
        }

        // Parse XML
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

        // Get divisions
        var divisions = doc.Descendants("divisions").FirstOrDefault();
        int divisionsPerQuarter = divisions != null ? int.Parse(divisions.Value) : 8;
        float secondsPerTick = 60f / (xmlBpm * divisionsPerQuarter);

        // Parse notes from Learner part only (P1)
        var learnerPart = doc.Descendants("part").FirstOrDefault(p => p.Attribute("id")?.Value == "P1");
        if (learnerPart == null)
        {
            Debug.LogError("Could not find Learner part (P1) in XML for duration calculation!");
            songDuration = 60f; // Default fallback
            return;
        }

        // Calculate total duration
        foreach (var noteElement in learnerPart.Descendants("note"))
        {
            var durationElement = noteElement.Element("duration");
            if (durationElement != null)
            {
                int durationTicks = int.Parse(durationElement.Value);
                float noteDuration = durationTicks * secondsPerTick;
                currentTime += noteDuration;
            }
        }

        songDuration = currentTime;
    }

    public void UpdateCurrentTime(float time)
    {
        currentTime = Mathf.Clamp(time, 0f, songDuration);
        UpdateProgress();
    }

    void UpdateProgress()
    {
        // Update slider
        if (progressSlider != null)
        {
            progressSlider.value = currentTime;
        }

        // Update time text
        if (timeText != null && showTimeText)
        {
            timeText.text = FormatTime(currentTime);
        }
    }

    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public float GetSongDuration()
    {
        return songDuration;
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public float GetProgressPercentage()
    {
        if (songDuration <= 0f) return 0f;
        return (currentTime / songDuration) * 100f;
    }

    public bool IsSongFinished()
    {
        return currentTime >= songDuration;
    }

    // Method to reset the time manager (useful for restarting)
    public void Reset()
    {
        currentTime = 0f;
        UpdateProgress();
    }
}
