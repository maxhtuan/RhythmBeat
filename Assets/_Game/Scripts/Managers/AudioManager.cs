using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [Header("Piano Key Sounds")]
    [SerializeField] AudioClip c4Sound;
    [SerializeField] AudioClip d4Sound;
    [SerializeField] AudioClip e4Sound;
    [SerializeField] AudioClip f4Sound;
    [SerializeField] AudioClip g4Sound;

    [Header("Audio Settings")]
    [SerializeField] float volume = 0.8f;
    [SerializeField] float pitch = 1.0f;
    [SerializeField] bool enableSound = true;

    // Dictionary to map note names to audio clips
    private Dictionary<string, AudioClip> noteToSoundMap = new Dictionary<string, AudioClip>();

    // Audio source for playing sounds
    private AudioSource audioSource;

    void Start()
    {
        // Get or create AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
        audioSource.pitch = pitch;

        // Initialize the note-to-sound mapping
        InitializeNoteSoundMap();

        Debug.Log("AudioManager initialized with piano key sounds");
    }

    void InitializeNoteSoundMap()
    {
        // Clear existing mappings
        noteToSoundMap.Clear();

        // Map note names to their corresponding audio clips
        noteToSoundMap["C4"] = c4Sound;
        noteToSoundMap["D4"] = d4Sound;
        noteToSoundMap["E4"] = e4Sound;
        noteToSoundMap["F4"] = f4Sound;
        noteToSoundMap["G4"] = g4Sound;

        // Also map without octave for flexibility
        noteToSoundMap["C"] = c4Sound;
        noteToSoundMap["D"] = d4Sound;
        noteToSoundMap["E"] = e4Sound;
        noteToSoundMap["F"] = f4Sound;
        noteToSoundMap["G"] = g4Sound;

        // Log which sounds are available
        Debug.Log("Note sound mappings:");
        foreach (var kvp in noteToSoundMap)
        {
            if (kvp.Value != null)
            {
                Debug.Log($"  {kvp.Key} -> {kvp.Value.name}");
            }
            else
            {
                Debug.LogWarning($"  {kvp.Key} -> NULL (no audio clip assigned)");
            }
        }
    }

    // Play sound for a specific note
    public void PlayNoteSound(string noteName)
    {
        if (!enableSound)
        {
            Debug.Log($"Sound disabled, skipping note: {noteName}");
            return;
        }

        // Try to find the audio clip for this note
        if (noteToSoundMap.TryGetValue(noteName, out AudioClip clip))
        {
            if (clip != null)
            {
                // Play the sound
                audioSource.PlayOneShot(clip, volume);
                Debug.Log($"Playing sound for note: {noteName}");
            }
            else
            {
                Debug.LogWarning($"Audio clip for note {noteName} is null!");
            }
        }
        else
        {
            Debug.LogWarning($"No audio clip found for note: {noteName}");
        }
    }

    // Play sound for a piano key (called from PianoKey script)
    public void PlayPianoKeySound(string keyName)
    {
        PlayNoteSound(keyName);
    }

    // Play sound for a note data (called from GameplayManager)
    public void PlayNoteDataSound(NoteData noteData)
    {
        if (noteData != null && !noteData.isRest)
        {
            PlayNoteSound(noteData.pitch);
        }
    }

    // Stop all sounds
    public void StopAllSounds()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    // Set volume
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    // Set pitch
    public void SetPitch(float newPitch)
    {
        pitch = Mathf.Clamp(newPitch, 0.1f, 3.0f);
        if (audioSource != null)
        {
            audioSource.pitch = pitch;
        }
    }

    // Enable/disable sound
    public void SetSoundEnabled(bool enabled)
    {
        enableSound = enabled;
        Debug.Log($"Sound {(enabled ? "enabled" : "disabled")}");
    }

    // Get current volume
    public float GetVolume()
    {
        return volume;
    }

    // Get current pitch
    public float GetPitch()
    {
        return pitch;
    }

    // Check if sound is enabled
    public bool IsSoundEnabled()
    {
        return enableSound;
    }

    // Check if a note has a sound assigned
    public bool HasNoteSound(string noteName)
    {
        return noteToSoundMap.ContainsKey(noteName) && noteToSoundMap[noteName] != null;
    }

    // Get all available note names
    public List<string> GetAvailableNotes()
    {
        List<string> availableNotes = new List<string>();
        foreach (var kvp in noteToSoundMap)
        {
            if (kvp.Value != null)
            {
                availableNotes.Add(kvp.Key);
            }
        }
        return availableNotes;
    }

    // Debug method to test all sounds
    [ContextMenu("Test All Sounds")]
    public void TestAllSounds()
    {
        Debug.Log("Testing all piano key sounds...");
        foreach (var kvp in noteToSoundMap)
        {
            if (kvp.Value != null)
            {
                Debug.Log($"Testing sound for {kvp.Key}...");
                audioSource.PlayOneShot(kvp.Value, volume);
                // Wait a bit between sounds
                System.Threading.Thread.Sleep(500);
            }
        }
    }
}

