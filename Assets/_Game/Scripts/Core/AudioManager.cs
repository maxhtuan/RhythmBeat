using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PianoKeySound
{
    public string noteName;
    public AudioClip sound;
}

public class AudioManager : MonoBehaviour, IService
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Piano Key Sounds")]
    public PianoKeySound[] pianoKeySoundMappings = {
        new PianoKeySound { noteName = "C" },
        new PianoKeySound { noteName = "D" },
        new PianoKeySound { noteName = "E" },
        new PianoKeySound { noteName = "F" },
        new PianoKeySound { noteName = "G" },
        new PianoKeySound { noteName = "A" },
        new PianoKeySound { noteName = "B" }
    };

    [Header("Other Audio Clips")]
    public AudioClip hitSound;
    public AudioClip missSound;
    public AudioClip comboSound;

    [Header("Settings")]
    public float musicVolume = 0.7f;
    public float sfxVolume = 1f;

    private Dictionary<string, AudioClip> pianoKeySoundMap = new Dictionary<string, AudioClip>();
    private bool isInitialized = false;

    // Remove Start() method - initialization will be called from GameplayManager

    public void Initialize()
    {
        if (isInitialized) return;

        SetupAudioSources();
        SetupPianoKeySounds();
        isInitialized = true;

        Debug.Log("AudioManager: Initialized");
    }

    void SetupAudioSources()
    {
        // Setup music source if not assigned
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Setup SFX source if not assigned
        if (sfxSource == null)
        {
            GameObject sfxObject = new GameObject("SFXSource");
            sfxObject.transform.SetParent(transform);
            sfxSource = sfxObject.AddComponent<AudioSource>();
        }

        // Set initial volumes
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    void SetupPianoKeySounds()
    {
        pianoKeySoundMap.Clear();

        // Create mapping from inspector-assigned sounds
        if (pianoKeySoundMappings != null)
        {
            foreach (var mapping in pianoKeySoundMappings)
            {
                if (mapping != null && !string.IsNullOrEmpty(mapping.noteName) && mapping.sound != null)
                {
                    pianoKeySoundMap[mapping.noteName] = mapping.sound;
                    Debug.Log($"Mapped {mapping.noteName} to audio clip: {mapping.sound.name}");
                }
            }
        }

        Debug.Log($"AudioManager: Loaded {pianoKeySoundMap.Count} piano key sounds");
    }

    public void PlayPianoKeySound(string noteName)
    {
        Debug.Log($"PlayPianoKeySound called for note: {noteName}");



        if (pianoKeySoundMappings.Any(mapping => mapping.noteName == noteName))
        {
            var sound = pianoKeySoundMappings.First(mapping => mapping.noteName == noteName);

            if (sound == null) return;
            if (sound.sound == null) return;

            Debug.Log($"Found sound for {noteName}: {sound.sound.name}");

            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(sound.sound);
                Debug.Log($"Playing sound for {noteName}");
            }
            else
            {
                Debug.LogError("SFX AudioSource is null!");
            }
        }
        else
        {
            Debug.LogWarning(
                $"No sound found for note: {noteName}. Available sounds: {string.Join(", ", pianoKeySoundMappings.Select(mapping => mapping.noteName))}");
        }
    }

    public void PlayHitSound()
    {
        if (hitSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(hitSound);
        }
    }

    public void PlayMissSound()
    {
        if (missSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(missSound);
        }
    }

    public void PlayComboSound()
    {
        if (comboSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(comboSound);
        }
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    [ContextMenu("Test Play C Sound")]
    public void TestPlayCSound()
    {
        PlayPianoKeySound("C");
    }

    public void Cleanup()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
        if (sfxSource != null)
        {
            sfxSource.Stop();
        }
        pianoKeySoundMap.Clear();
        isInitialized = false;
        Debug.Log("AudioManager cleaned up");
    }
}

