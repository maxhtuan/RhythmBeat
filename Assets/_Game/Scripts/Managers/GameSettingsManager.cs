using UnityEngine;

public class GameSettingsManager : MonoBehaviour, IService
{
    [Header("Settings")]
    public GameSettings gameSettings;

    private GameplayManager gameplayManager;
    private bool isInitialized = false;

    // Remove Start() method - initialization will be called from GameplayManager

    public void Initialize()
    {
        if (isInitialized) return;

        // Find GameplayManager if not assigned
        if (gameplayManager == null)
        {
            gameplayManager = FindObjectOfType<GameplayManager>();
        }

        // Load settings if not assigned
        if (gameSettings == null)
        {
            gameSettings = Resources.Load<GameSettings>("GameSettings");
            if (gameSettings == null)
            {
                Debug.LogWarning("GameSettings not found in Resources, creating default settings");
                gameSettings = ScriptableObject.CreateInstance<GameSettings>();
            }
        }

        // Apply settings after loading
        ApplyGameplaySettings();
        ApplyAudioSettings();

        isInitialized = true;
        Debug.Log("GameSettingsManager: Initialized");
    }

    public void ApplyGameplaySettings()
    {
        if (gameplayManager == null) return;

        // Apply settings to GameplayManager
        gameplayManager.noteTravelTime = gameSettings.defaultNoteTravelTime;
        gameplayManager.noteSpawnOffset = gameSettings.noteSpawnOffset;
        gameplayManager.noteArrivalOffset = gameSettings.noteArrivalOffset;
        gameplayManager.hitWindow = gameSettings.hitWindow;

        Debug.Log("Gameplay settings applied");
    }

    public void ApplyAudioSettings()
    {
        if (gameSettings == null) return;

        // Apply audio settings to AudioSources
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            if (source.CompareTag("Music"))
            {
                source.volume = gameSettings.musicVolume * gameSettings.masterVolume;
            }
            else if (source.CompareTag("SFX"))
            {
                source.volume = gameSettings.sfxVolume * gameSettings.masterVolume;
            }
            else
            {
                source.volume = gameSettings.masterVolume;
            }
        }

        Debug.Log("Audio settings applied");
    }

    public void UpdateWindowSize(int width, int height)
    {
        if (gameSettings == null || !gameSettings.allowWindowSizeChanges) return;

        // Validate against min/max constraints
        width = Mathf.Clamp(width, gameSettings.minWindowWidth,
            gameSettings.maxWindowWidth > 0 ? gameSettings.maxWindowWidth : width);
        height = Mathf.Clamp(height, gameSettings.minWindowHeight,
            gameSettings.maxWindowHeight > 0 ? gameSettings.maxWindowHeight : height);

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
        if (gameSettings == null) return;
        gameSettings.masterVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
    }

    public void SetMusicVolume(float volume)
    {
        if (gameSettings == null) return;
        gameSettings.musicVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
    }

    public void SetSFXVolume(float volume)
    {
        if (gameSettings == null) return;
        gameSettings.sfxVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
    }

    // Getter methods for other scripts to access settings
    public bool AllowWindowSizeChanges => gameSettings?.allowWindowSizeChanges ?? false;
    public float DefaultNoteTravelTime => gameSettings?.defaultNoteTravelTime ?? 3f;
    public float HitWindow => gameSettings?.hitWindow ?? 0.2f;
    public float NoteLengthMultiplier => gameSettings?.noteLengthMultiplier ?? 1f;
    public bool EnableDebugLogs => gameSettings?.enableDebugLogs ?? false;
    public float BPMIncreaseAmount => gameSettings?.bpmIncreaseAmount ?? 10f;
    public int BeatsBeforeSpeedUp => gameSettings?.beatsBeforeSpeedUp ?? 3;
    public float MaxBPM => gameSettings?.maxBPM ?? 200f;

    public void Cleanup()
    {
        isInitialized = false;
        Debug.Log("GameSettingsManager cleaned up");
    }
}
