using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    [Header("References")]
    public GameplayManager gameplayManager;

    [SerializeField]
    private GameSettings settings;

    void Awake()
    {
        // Load settings
        settings.ValidateSettings();

        // Apply window settings
        settings.ApplyWindowSettings();

        // Apply audio settings
        ApplyAudioSettings();
    }

    void Start()
    {
        // Apply gameplay settings to GameplayManager
        if (gameplayManager != null)
        {
            ApplyGameplaySettings();
        }
    }

    public void ApplyGameplaySettings()
    {
        if (gameplayManager == null) return;

        // Apply settings to GameplayManager
        gameplayManager.noteTravelTime = settings.defaultNoteTravelTime;
        gameplayManager.noteSpawnOffset = settings.noteSpawnOffset;
        gameplayManager.noteArrivalOffset = settings.noteArrivalOffset;
        gameplayManager.hitWindow = settings.hitWindow;

        Debug.Log("Gameplay settings applied");
    }

    public void ApplyAudioSettings()
    {
        // Apply audio settings to AudioSources
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            if (source.CompareTag("Music"))
            {
                source.volume = settings.musicVolume * settings.masterVolume;
            }
            else if (source.CompareTag("SFX"))
            {
                source.volume = settings.sfxVolume * settings.masterVolume;
            }
            else
            {
                source.volume = settings.masterVolume;
            }
        }

        Debug.Log("Audio settings applied");
    }

    public void UpdateWindowSize(int width, int height)
    {
        if (!settings.allowWindowSizeChanges) return;

        // Validate against min/max constraints
        width = Mathf.Clamp(width, settings.minWindowWidth,
            settings.maxWindowWidth > 0 ? settings.maxWindowWidth : width);
        height = Mathf.Clamp(height, settings.minWindowHeight,
            settings.maxWindowHeight > 0 ? settings.maxWindowHeight : height);

        Screen.SetResolution(width, height, Screen.fullScreen);
        Debug.Log($"Window size changed to {width}x{height}");
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        Debug.Log($"Fullscreen toggled: {Screen.fullScreen}");
    }

    public void SetMasterVolume(float volume)
    {
        settings.masterVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
    }

    public void SetMusicVolume(float volume)
    {
        settings.musicVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
    }

    public void SetSFXVolume(float volume)
    {
        settings.sfxVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
    }

    // Getter methods for other scripts to access settings
    public bool AllowWindowSizeChanges => settings.allowWindowSizeChanges;
    public float DefaultNoteTravelTime => settings.defaultNoteTravelTime;
    public float HitWindow => settings.hitWindow;
    public float NoteLengthMultiplier => settings.noteLengthMultiplier;
    public bool EnableDebugLogs => settings.enableDebugLogs;
    public float BPMIncreaseAmount => settings.bpmIncreaseAmount;
    public int BeatsBeforeSpeedUp => settings.beatsBeforeSpeedUp;
    public float MaxBPM => settings.maxBPM;
}
