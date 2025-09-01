using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "BeatEd/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Window Settings")]
    [Tooltip("Enable to allow window size changes during gameplay")]
    public bool allowWindowSizeChanges = true;

    [Tooltip("Minimum window width in pixels")]
    public int minWindowWidth = 800;

    [Tooltip("Minimum window height in pixels")]
    public int minWindowHeight = 600;

    [Tooltip("Maximum window width in pixels (0 = no limit)")]
    public int maxWindowWidth = 0;

    [Tooltip("Maximum window height in pixels (0 = no limit)")]
    public int maxWindowHeight = 0;

    [Header("Gameplay Settings")]
    [Tooltip("Default note travel time in seconds")]
    public float defaultNoteTravelTime = 3f;

    [Tooltip("Note spawn offset in seconds")]
    public float noteSpawnOffset = 3f;

    [Tooltip("Note arrival offset in seconds")]
    public float noteArrivalOffset = 0f;

    [Tooltip("Hit window size in seconds")]
    public float hitWindow = 0.2f;

    [Tooltip("Note length multiplier for visual display")]
    public float noteLengthMultiplier = 1f;

    [Header("Audio Settings")]
    [Tooltip("Master volume (0-1)")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Tooltip("Music volume (0-1)")]
    [Range(0f, 1f)]
    public float musicVolume = 0.8f;

    [Tooltip("SFX volume (0-1)")]
    [Range(0f, 1f)]
    public float sfxVolume = 0.9f;

    [Header("Visual Settings")]
    [Tooltip("Enable particle effects")]
    public bool enableParticleEffects = true;

    [Tooltip("Enable hit effects")]
    public bool enableHitEffects = true;

    [Tooltip("Enable miss effects")]
    public bool enableMissEffects = true;

    [Header("Performance Settings")]
    [Tooltip("Maximum number of active notes")]
    public int maxActiveNotes = 50;

    [Tooltip("Note cleanup delay in seconds")]
    public float noteCleanupDelay = 4f;


    [Tooltip("Base BPM Speed Up mode")]
    public float baseBPMSpeedUpMode = 60f;

    [Header("Speed Up Mode Settings")]
    [Tooltip("BPM increase amount per speed up")]
    public float bpmIncreaseAmount = 10f;

    [Tooltip("Number of beats before speed up")]
    public int beatsBeforeSpeedUp = 3;

    [Tooltip("Maximum BPM limit (0 = no limit)")]
    public float maxBPM = 0f;

    [Header("Debug Settings")]
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogs = true;

    [Tooltip("Show FPS counter")]
    public bool showFPSCounter = false;

    [Tooltip("Show timing debug info")]
    public bool showTimingDebug = false;

    // Singleton instance
    private static GameSettings _instance;
    public static GameSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameSettings>("GameSettings");
                if (_instance == null)
                {
                    Debug.LogWarning("GameSettings not found in Resources folder. Creating default settings.");
                    _instance = CreateInstance<GameSettings>();
                }
            }
            return _instance;
        }
    }

    // Apply window size settings
    public void ApplyWindowSettings()
    {
        if (!allowWindowSizeChanges) return;

        // Set minimum window size
        Screen.SetResolution(
            Mathf.Max(Screen.width, minWindowWidth),
            Mathf.Max(Screen.height, minWindowHeight),
            Screen.fullScreen
        );

        // Set maximum window size if specified
        if (maxWindowWidth > 0 || maxWindowHeight > 0)
        {
            int maxWidth = maxWindowWidth > 0 ? maxWindowWidth : Screen.currentResolution.width;
            int maxHeight = maxWindowHeight > 0 ? maxWindowHeight : Screen.currentResolution.height;

            if (Screen.width > maxWidth || Screen.height > maxHeight)
            {
                Screen.SetResolution(
                    Mathf.Min(Screen.width, maxWidth),
                    Mathf.Min(Screen.height, maxHeight),
                    Screen.fullScreen
                );
            }
        }
    }

    // Validate settings
    public void ValidateSettings()
    {
        defaultNoteTravelTime = Mathf.Max(0.1f, defaultNoteTravelTime);
        noteSpawnOffset = Mathf.Max(0f, noteSpawnOffset);
        hitWindow = Mathf.Max(0.05f, hitWindow);
        noteLengthMultiplier = Mathf.Max(0.1f, noteLengthMultiplier);
        masterVolume = Mathf.Clamp01(masterVolume);
        musicVolume = Mathf.Clamp01(musicVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);
        maxActiveNotes = Mathf.Max(1, maxActiveNotes);
        noteCleanupDelay = Mathf.Max(0.1f, noteCleanupDelay);
        bpmIncreaseAmount = Mathf.Max(0, bpmIncreaseAmount);
        beatsBeforeSpeedUp = Mathf.Max(1, beatsBeforeSpeedUp);
    }
}
