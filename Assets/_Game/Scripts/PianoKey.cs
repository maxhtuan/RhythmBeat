using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic; // Added for List

public class PianoKey : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Piano Key Settings")]
    public string noteName = "C"; // C, D, E, F, G, A, B
    public Color keyColor = Color.white; // Get color from GameConfigs


    [Header("References")]
    public Image backSpriteRenderer;
    public TMPro.TextMeshProUGUI text;
    public Button button;
    public GameplayManager gameplayManager;

    [Header("Audio")]
    public AudioManager audioManager;

    private Color originalColor;
    private bool isPressed = false;
    private NoteData linkedNote = null;

    void Start()
    {
        // Get color from GameConfigs using proper Unity color parsing
        if (GameConfigs.PianoNoteColors.ContainsKey(noteName))
        {
            string hexColor = GameConfigs.PianoNoteColors[noteName];
            ColorUtility.TryParseHtmlString(hexColor, out keyColor);
        }

        if (button == null)
            button = GetComponent<Button>();

        // Find GameplayManager if not assigned
        if (gameplayManager == null)
        {
            gameplayManager = FindObjectOfType<GameplayManager>();
        }

        // Find AudioManager if not assigned
        if (audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
        }

        // Set the original color
        originalColor = keyColor;

        // Set the text to show the note name
        if (text != null)
        {
            text.text = noteName;
        }

        // Apply colors based on note
        ApplyNoteColors();
    }

    private void ApplyNoteColors()
    {
        // Set back sprite color
        if (backSpriteRenderer != null)
        {
            backSpriteRenderer.color = originalColor;
        }

        // Set text color
        if (text != null)
        {
            text.color = originalColor;
        }
    }

    // This is called when pointer (mouse/touch) goes down on the button
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isPressed)
        {
            isPressed = true;
            OnButtonPressed();
        }
    }

    // This is called when pointer (mouse/touch) goes up on the button
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPressed)
        {
            isPressed = false;
            OnButtonReleased();
        }
    }

    private void OnButtonPressed()
    {
        Debug.Log($"Started pressing {noteName} key");

        // Play piano key sound
        if (audioManager != null)
        {
            audioManager.PlayPianoKeySound(noteName);
        }

        if (gameplayManager != null && (gameplayManager.IsPlaying || gameplayManager.IsSetupComplete))
        {
            // Find the closest note to hit
            NoteData closestNote = FindClosestNote();
            if (closestNote != null)
            {
                float accuracy = CalculateHitAccuracy(closestNote);
                gameplayManager.OnNoteHit(this, closestNote, accuracy);
                linkedNote = closestNote; // Track the linked note
                Debug.Log($"Hit {noteName} with accuracy: {accuracy:F2}");
                return;
            }
            else
            {
                // gameplayManager.OnNoteHolding(this);

                Debug.Log($"Pressed {noteName} but no note to hit");
            }
        }
        gameplayManager.OnNoteHolding(this);
    }

    private void OnButtonReleased()
    {
        Debug.Log($"Released {noteName} key");

        // Release the linked note
        if (linkedNote != null && gameplayManager != null)
        {
            gameplayManager.OnNoteRelease(this, linkedNote);
            linkedNote = null;
        }
        else
        {
            // //if holding
            gameplayManager.OnNoteRelease(this, null);
        }
    }

    private NoteData FindClosestNote()
    {
        if (gameplayManager == null) return null;

        float currentTime = gameplayManager.GetCurrentTime();
        float hitWindow = gameplayManager.hitWindow;

        NoteData closestNote = null;
        float closestTime = float.MaxValue;

        foreach (var note in gameplayManager.notes)
        {
            if (note.isRest) continue;

            float timeDiff = Mathf.Abs(note.startTime - currentTime);
            if (timeDiff <= hitWindow && timeDiff < closestTime)
            {
                // Check if the note matches this key
                if (IsNoteMatch(note))
                {
                    closestNote = note;
                    closestTime = timeDiff;
                }
            }
        }

        return closestNote;
    }

    private bool IsNoteMatch(NoteData note)
    {
        if (note.isRest) return false;

        // Check if the note pitch starts with this key's note name
        return note.pitch.StartsWith(noteName);
    }

    private float CalculateHitAccuracy(NoteData note)
    {
        if (gameplayManager == null) return 0f;

        float currentTime = gameplayManager.GetCurrentTime();
        float timeDiff = Mathf.Abs(note.startTime - currentTime);
        float hitWindow = gameplayManager.hitWindow;

        // Calculate accuracy based on timing
        float accuracy = 1f - (timeDiff / hitWindow);
        return Mathf.Clamp01(accuracy);
    }
}
