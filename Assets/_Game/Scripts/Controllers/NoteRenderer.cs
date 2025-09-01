using UnityEngine;
using DG.Tweening;

public class NoteRenderer : MonoBehaviour
{
    public SpriteRenderer spriteRenderer, spriteRendererFake, spriteDecorRenderer, spriteDecorRendererFake;
    public TMPro.TextMeshPro noteNameText;

    [Header("Hit Window Visual")]
    public SpriteRenderer hitWindowIndicator;
    public SpriteMask maskHitRemaining;
    public Color hitWindowColor = new Color(1f, 1f, 0f, 0.3f);

    [Header("Effects")]
    [SerializeField] DOTweenAnimation onHitEffect, onReleaseEffect;
    public ParticleSystem hitEffect;
    public ParticleSystem childEffect;

    [Header("Settings")]
    public GameSettingsManager settingsManager;

    // State
    private bool isHit = false;
    private bool isMissed = false;
    private Vector3 hitEffectPosition;

    // References
    private NoteMovement noteMovement;
    private NoteData noteData;

    public void Initialize(NoteData note, NoteMovement movement)
    {
        noteData = note;
        noteMovement = movement;
        settingsManager = ServiceLocator.Instance.GetService<GameSettingsManager>();

        // Initialize hit effect
        if (hitEffect != null)
        {
            hitEffect.Stop();
            hitEffect.gameObject.SetActive(false);
        }

        if (noteNameText != null)
        {
            noteNameText.text = noteData.GetNoteName();
        }

        if (hitEffect != null)
        {
            hitEffect.transform.DOKill();
        }

        UpdateVisual();
        SetupHitWindowVisual();
        SetMaskHitRemaining();
    }

    public void UpdateVisuals(float currentTime)
    {
        if (noteMovement == null) return;

        // Update note visual length
        UpdateNoteVisualLength(noteMovement.GetOriginalTravelSpeed());

        // Update hit window visual
        UpdateHitWindowVisual(currentTime);

        // Update mask length
        UpdateMaskLength(currentTime);
    }

    private void UpdateVisual()
    {
        if (spriteRenderer != null)
        {
            // Set color based on note type
            if (noteData.isRest)
            {
                spriteRenderer.color = Color.gray;
                spriteRendererFake.color = Color.gray;
                spriteDecorRenderer.color = Color.gray;
                spriteDecorRendererFake.color = Color.gray;
            }
            else
            {
                string noteName = noteData.GetNoteName();
                string hexColor = GameConfigs.GetNoteBaseColor(noteName);
                Color noteColor;
                if (ColorUtility.TryParseHtmlString(hexColor, out noteColor))
                {
                    spriteRenderer.color = noteColor;
                    spriteRendererFake.color = noteColor;
                }

                string hexColor2 = GameConfigs.GetNote2ndColor(noteName);
                Color noteColor2;
                if (ColorUtility.TryParseHtmlString(hexColor2, out noteColor2))
                {
                    spriteDecorRenderer.color = noteColor2;
                    spriteDecorRendererFake.color = noteColor2;
                }

                // Set particle color
                if (childEffect != null)
                {
                    var main = childEffect.main;
                    main.startColor = noteColor;
                }
            }

            // Scale based on note duration
            float scale = 1f;
            if (noteData.duration > 0)
            {
                scale = Mathf.Max(0.5f, noteData.duration * 2f);
            }

            spriteRenderer.size = new Vector2(scale, 1f);
            spriteDecorRenderer.size = new Vector2(scale, 1f);
            spriteRendererFake.size = new Vector2(scale, 1f);
            spriteDecorRendererFake.size = new Vector2(scale, 1f);
        }
    }

    private void UpdateNoteVisualLength(float travelSpeed)
    {
        if (spriteRenderer == null) return;

        // Calculate how long the note should be visually (in distance units)
        float noteLengthMultiplier = 1f;

        // Use the original travel speed for visual length calculation (not affected by BPM changes)
        float originalTravelSpeed = travelSpeed;
        float noteLengthInDistance = noteData.duration * originalTravelSpeed * noteLengthMultiplier;

        // Scale the sprite to represent the note length
        float scaleX = Mathf.Max(0.5f, noteLengthInDistance);

        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        spriteRenderer.size = new Vector2(scaleX, 1f);

        spriteRendererFake.drawMode = SpriteDrawMode.Sliced;
        spriteRendererFake.size = new Vector2(scaleX, 1f);

        spriteDecorRenderer.drawMode = SpriteDrawMode.Sliced;
        spriteDecorRenderer.size = new Vector2(scaleX, 1f);

        spriteDecorRendererFake.drawMode = SpriteDrawMode.Sliced;
        spriteDecorRendererFake.size = new Vector2(scaleX, 1f);
    }

    private void SetupHitWindowVisual()
    {
        if (hitWindowIndicator == null) return;

        // Set the hit window color
        hitWindowIndicator.color = hitWindowColor;

        // Calculate the visual size of the hit window
        float totalDistance = noteMovement.GetTotalDistance();
        float currentTravelSpeed = noteMovement.GetCurrentTravelSpeed();
        float hitWindowDistance = noteMovement.GetHitWindow() * currentTravelSpeed;

        // Make the hit window a small rectangle at the front of the note
        if (IsWindowSizeChangeEnabled())
            hitWindowIndicator.transform.localScale = new Vector3(hitWindowDistance, 1f, 1f);

        // Position it at the left/front of the note
        hitWindowIndicator.transform.localPosition = Vector3.zero;
    }

    private void UpdateHitWindowVisual(float currentTime)
    {
        if (hitWindowIndicator == null) return;

        // Get window color from GameConfigs
        string noteName = noteData.GetNoteName();
        string hexColor = GameConfigs.GetWindowColor(noteName);
        Color windowColor;
        if (ColorUtility.TryParseHtmlString(hexColor, out windowColor))
        {
            hitWindowIndicator.color = windowColor;
        }

        // Use original XML timing
        float noteArrivalTime = noteMovement.GetNoteArrivalTime();

        float totalDistance = noteMovement.GetTotalDistance();
        float currentTravelSpeed = noteMovement.GetCurrentTravelSpeed();

        // Calculate current time relative to note arrival
        float timeUntilHit = noteArrivalTime - currentTime;

        // Show window if in hit range OR if note has been hit
        bool inHitWindow = Mathf.Abs(timeUntilHit) <= noteMovement.GetHitWindow();
        bool shouldShowWindow = isHit;
        hitWindowIndicator.enabled = !shouldShowWindow;

        // Position the hit window
        if (shouldShowWindow)
        {
            Vector3 notePosition = transform.position;
            Vector3 direction = (noteMovement.GetEndPosition() - noteMovement.GetStartPosition()).normalized;
            float hitWindowDistance = noteMovement.GetHitWindow() * currentTravelSpeed;

            if (isHit)
            {
                // When hit: position window at the left side of the board
                Vector3 boardLeftPosition = new Vector3(noteMovement.GetEndPosition().x, notePosition.y, notePosition.z);
                hitWindowIndicator.transform.position = boardLeftPosition;
            }
            else
            {
                // Before hit: normal positioning at left side of note
                hitWindowIndicator.transform.localPosition = Vector3.zero;
            }

            // Scale the window based on hit window time
            if (IsWindowSizeChangeEnabled())
                hitWindowIndicator.transform.localScale = new Vector3(hitWindowDistance, 1f, 1f);
        }
    }

    private void SetMaskHitRemaining()
    {
        // Initialize mask
        if (maskHitRemaining != null)
        {
            maskHitRemaining.enabled = false; // Start hidden until note is hit

            // Set mask to same size as sprite renderer
            if (spriteRenderer != null)
            {
                // Set the mask scale to match the sprite size
                float spriteWidth = spriteRenderer.size.x;
                maskHitRemaining.transform.localScale = new Vector3(spriteWidth, 1f, 1f);
                maskHitRemaining.transform.localPosition = new Vector3(spriteWidth * 2, 0, 0);
            }
        }
    }

    private void UpdateMaskLength(float currentTime)
    {
        if (maskHitRemaining == null) return;

        // Use original XML timing
        float noteArrivalTime = noteMovement.GetNoteArrivalTime();

        // Calculate how much time has passed since the note arrived
        float timeSinceArrival = currentTime - noteArrivalTime;

        // Calculate remaining time (clamped to note duration)
        float remainingTime = Mathf.Max(0f, noteData.duration - timeSinceArrival);

        // Calculate progress (0 = no time left, 1 = full time remaining)
        float progress = remainingTime / noteData.duration;

        // Get the original size of the sprite
        float originalSize = spriteRenderer.size.x;

        // Calculate new mask scale based on remaining time and original size
        float newMaskScale = originalSize * progress;

        // Update the mask scale
        maskHitRemaining.transform.localScale = new Vector3(newMaskScale, 1f, 1f);

        // Show/hide mask based on hit state
        maskHitRemaining.enabled = isHit && progress > 0f;
    }

    public void Hit()
    {
        if (isHit || isMissed) return;

        isHit = true;

        // Add hit effect
        AddHitEffect();
    }

    public void Release()
    {
        if (onReleaseEffect != null)
        {
            onReleaseEffect.DORestartById("ScaleDown");
        }

        isHit = false;

        if (hitWindowIndicator != null)
        {
            hitWindowIndicator.transform.localPosition = Vector3.zero;
        }

        if (hitEffect != null)
        {
            hitEffect.Stop();
            hitEffect.gameObject.SetActive(false);
        }

        if (hitWindowIndicator != null)
        {
            hitWindowIndicator.gameObject.SetActive(false);
        }
    }

    public void Miss()
    {
        if (isHit || isMissed) return;

        isMissed = true;

        // Add miss effect
        AddMissEffect();
    }

    private void AddHitEffect()
    {
        // Add particle effect or animation for hit
        float alpha = 0.15f;
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
        spriteDecorRenderer.color = new Color(spriteDecorRenderer.color.r, spriteDecorRenderer.color.g, spriteDecorRenderer.color.b, alpha);
        noteNameText.color = new Color(noteNameText.color.r, noteNameText.color.g, noteNameText.color.b, alpha);

        if (hitEffect != null)
        {
            hitEffect.transform.localPosition = Vector3.zero;
            hitEffect.gameObject.SetActive(true);
            hitEffect.Play();
        }

        if (onHitEffect != null)
        {
            onHitEffect.transform.DOKill();
            // onHitEffect.DOPlayById("Shake");
            // onHitEffect.DORestartById("Shake");
            // onHitEffect.DOPlay();
            hitEffect.transform.
            DOPunchPosition(new Vector3(0.09f, 0, 0), 0.2f, 50, 1).SetLoops(-1, LoopType.Restart)
            ;
        }
    }

    private void AddMissEffect()
    {
        // Add particle effect or animation for miss
        // You can implement this based on your visual needs
    }

    public bool IsWindowSizeChangeEnabled()
    {
        if (settingsManager != null)
        {
            return settingsManager.AllowWindowSizeChanges;
        }
        return true; // Default to true if settings not available
    }

    public bool AllowWindowSizeChanges =>
        settingsManager?.AllowWindowSizeChanges ?? true;

    // Get note data
    public NoteData GetNoteData()
    {
        return noteData;
    }

    // Get hit state
    public bool IsHit()
    {
        return isHit;
    }

    // Get missed state
    public bool IsMissed()
    {
        return isMissed;
    }
}
