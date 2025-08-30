using UnityEngine;
using DG.Tweening;

public class NoteController : MonoBehaviour
{
    [Header("Note Properties")]
    public NoteData noteData;
    public float moveSpeed = 5f;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer, spriteRendererFake, spriteDecorRenderer, spriteDecorRendererFake;

    [Header("Effects")]
    [SerializeField] DOTweenAnimation onHitEffect, onReleaseEffect;

    public TMPro.TextMeshPro noteNameText;

    [Header("Hit Window Visual")]
    public SpriteRenderer hitWindowIndicator; // Assign a child sprite for the hit window
    public SpriteMask maskHitRemaining;
    public Color hitWindowColor = new Color(1f, 1f, 0f, 0.3f); // Semi-transparent yellow

    private Vector3 startPosition;
    private Vector3 endPosition;
    private float travelTime;
    private bool isHit = false;
    private bool isMissed = false;
    private float hitWindow = 0.2f; // This will be set from GameplayManager
    private GameplayManager gameplayManager;

    // Linking system
    private PianoKey linkedPianoKey = null;
    private bool isLinked = false;

    // Board passing detection
    private bool hasPassedBoard = false;

    public void Initialize(NoteData note, Vector3 start, Vector3 end, float travelTime = 3f, GameplayManager gm = null)
    {
        noteData = note;
        startPosition = start;
        endPosition = end;
        this.travelTime = travelTime;
        gameplayManager = gm;

        if (gameplayManager != null)
        {
            hitWindow = gameplayManager.hitWindow;
        }

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
        hitEffect.transform.DOKill();

        UpdateVisual();
        SetupHitWindowVisual();
        SetMaskHitRemaining();
    }

    void SetMaskHitRemaining()
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
                maskHitRemaining.transform.localScale = new Vector3(spriteWidth, 1f, 1f); // Start at full scale
                maskHitRemaining.transform.localPosition = new Vector3(spriteWidth, 0, 0);
            }
        }
    }

    // Link with a PianoKey
    public void LinkWithPianoKey(PianoKey pianoKey)
    {
        linkedPianoKey = pianoKey;
        isLinked = true;
        isHit = true;
        Debug.Log($"Note {noteData.GetNoteName()} linked with PianoKey {pianoKey.noteName}");
    }

    // Unlink from PianoKey
    public void UnlinkFromPianoKey()
    {
        if (isLinked)
        {
            linkedPianoKey = null;
            isLinked = false;
            isHit = false;
            Debug.Log($"Note {noteData.GetNoteName()} unlinked from PianoKey");
        }
    }

    // Check if note is linked to a specific PianoKey
    public bool IsLinkedTo(PianoKey pianoKey)
    {
        return isLinked && linkedPianoKey == pianoKey;
    }

    // Check if note is currently linked
    public bool IsLinked()
    {
        return isLinked;
    }

    // Get the linked PianoKey
    public PianoKey GetLinkedPianoKey()
    {
        return linkedPianoKey;
    }

    // Check if the entire note (including tail) has passed the board
    public bool HasCompletelyPassedBoard()
    {
        return hasPassedBoard;
    }

    // Calculate if the note has completely passed the board
    private bool CheckIfPassedBoard(float currentTime)
    {
        // Calculate when the note should arrive at target
        float noteArrivalTime = noteData.startTime;

        // Calculate the total time the note needs to completely pass the board
        // This includes the note's duration (tail length)
        float totalNoteTime = noteArrivalTime + noteData.duration;

        // Check if current time is past the total note time
        return currentTime > totalNoteTime;
    }

    // Update the mask to show remaining time
    private void UpdateMaskLength(float currentTime)
    {
        if (maskHitRemaining == null) return;

        // Calculate when the note should arrive at target
        float noteArrivalTime = noteData.startTime;

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

        // Update the mask scale (SpriteMask uses transform.localScale)
        maskHitRemaining.transform.localScale = new Vector3(newMaskScale, 1f, 1f);

        // Show/hide mask based on hit state
        maskHitRemaining.enabled = isHit && progress > 0f;
    }

    private void SetupHitWindowVisual()
    {
        if (hitWindowIndicator == null) return;

        // Set the hit window color
        hitWindowIndicator.color = hitWindowColor;

        // Calculate the visual size of the hit window based on travel speed and GameplayManager's hit window
        float totalDistance = Vector3.Distance(startPosition, endPosition);
        float travelSpeed = totalDistance / travelTime;
        float hitWindowDistance = hitWindow * travelSpeed; // Use GameplayManager's hit window value

        // Make the hit window a small rectangle at the front of the note
        hitWindowIndicator.transform.localScale = new Vector3(hitWindowDistance, 1f, 1f);

        // Position it at the left/front of the note
        hitWindowIndicator.transform.localPosition = Vector3.zero;
    }

    public void UpdatePosition(float currentTime, float noteSpawnOffset = 3f, float noteArrivalOffset = 0f)
    {
        // Check if the note has completely passed the board
        if (!hasPassedBoard && CheckIfPassedBoard(currentTime))
        {
            hasPassedBoard = true;
            Debug.Log($"Note {noteData.GetNoteName()} has completely passed the board");

            // You can add additional logic here when the note passes the board
            // For example, mark as missed if not hit, or trigger cleanup
        }

        // if (isHit || isMissed) return;

        // Calculate when this note should arrive at target (with arrival offset)
        float noteArrivalTime = noteData.startTime + noteArrivalOffset;

        // Calculate how much time has passed since this note should have started
        float timeSinceNoteShouldStart = currentTime - (noteArrivalTime - travelTime);

        // Calculate distance traveled
        float totalDistance = Vector3.Distance(startPosition, endPosition);
        float travelSpeed = totalDistance / travelTime;
        float distanceTraveled = timeSinceNoteShouldStart * travelSpeed;

        // Allow notes to continue flowing past the target (no clamping)
        Vector3 direction = (endPosition - startPosition).normalized;
        transform.position = startPosition + (direction * distanceTraveled);

        // Update visual length based on duration
        UpdateNoteVisualLength(travelSpeed);

        // Update hit window visual position
        UpdateHitWindowVisual();

        SetMaskHitRemaining();

        // Update mask length to show remaining time
        UpdateMaskLength(currentTime);

        // Update the mask length to show remaining time
    }

    private void UpdateNoteVisualLength(float travelSpeed)
    {
        if (spriteRenderer == null) return;

        // Calculate how long the note should be visually (in distance units)
        float noteLengthMultiplier = 1f; // You can make this configurable
        float noteLengthInDistance = noteData.duration * travelSpeed * noteLengthMultiplier;

        // Scale the sprite to represent the note length
        float scaleX = Mathf.Max(0.5f, noteLengthInDistance); // Minimum scale of 0.5 for better visibility
        // spriteRenderer.transform.localScale = new Vector3(scaleX, 1f, 1f);
        //change the spriteRenderer.drawMode.size
        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        spriteRenderer.size = new Vector2(scaleX, 1f);

        spriteRendererFake.drawMode = SpriteDrawMode.Sliced;
        spriteRendererFake.size = new Vector2(scaleX, 1f);

        spriteDecorRenderer.drawMode = SpriteDrawMode.Sliced;
        spriteDecorRenderer.size = new Vector2(scaleX, 1f);

        spriteDecorRendererFake.drawMode = SpriteDrawMode.Sliced;
        spriteDecorRendererFake.size = new Vector2(scaleX, 1f);
    }

    public void Hit()
    {
        if (isHit || isMissed) return;

        isHit = true;

        // Add hit effect
        AddHitEffect();

        // Destroy after a short delay
        // Destroy(gameObject, 0.5f);
    }

    public void Release()
    {
        onReleaseEffect.DORestartById("ScaleDown");
        // hitEffect.transform.DOKill();
        // hitEffect.transform.DOScale(0, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
        // {
        // onReleaseEffect.DOPauseAllById("ScaleDown");
        // onReleaseEffect.DOPauseAllById("Shake");
        isHit = false;

        hitWindowIndicator.transform.localPosition = Vector3.zero;
        hitEffect.Stop();
        hitEffect.gameObject.SetActive(false);
        hitWindowIndicator.gameObject.SetActive(false);
        // });
    }

    public void Miss()
    {
        if (isHit || isMissed) return;

        isMissed = true;

        // Add miss effect
        AddMissEffect();

        // Destroy after a short delay
        // Destroy(gameObject, 0.5f);
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

                // Set particle color using the Main module (proper way)
                var main = childEffect.main;
                main.startColor = noteColor;
                // childEffect.main = main;
                // hitEffect.GetComponentInChildren<ParticleSystem>().main = main;

            }

            // Scale based on note duration
            float scale = 1f;
            if (noteData.duration > 0)
            {
                scale = Mathf.Max(0.5f, noteData.duration * 2f); // Scale based on duration
            }

            // spriteRenderer.transform.localScale = Vector3.one * scale;
            spriteRenderer.size = new Vector2(scale, 1f);
            spriteDecorRenderer.size = new Vector2(scale, 1f);

            spriteRendererFake.size = new Vector2(scale, 1f);
            spriteDecorRendererFake.size = new Vector2(scale, 1f);
        }
    }

    public ParticleSystem hitEffect;
    public ParticleSystem childEffect;
    private void AddHitEffect()
    {
        // Add particle effect or animation for hit
        // You can implement this based on your visual needs
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.3f);
        spriteDecorRenderer.color = new Color(spriteDecorRenderer.color.r, spriteDecorRenderer.color.g, spriteDecorRenderer.color.b, 0.3f);
        noteNameText.color = new Color(noteNameText.color.r, noteNameText.color.g, noteNameText.color.b, 0.3f);

        if (hitEffect != null)
        {
            hitEffectPosition = hitWindowIndicator.transform.position;
            hitEffect.transform.position = hitEffectPosition;
            hitEffect.gameObject.SetActive(true);
            hitEffect.Play();
        }
        onHitEffect.DORestartById("Shake");

    }

    Vector3 hitEffectPosition;

    private void AddMissEffect()
    {
        // Add particle effect or animation for miss
        // You can implement this based on your visual needs
    }

    private void UpdateHitWindowVisual()
    {
        if (hitWindowIndicator == null) return;

        // Get window color from GameConfigs using proper Unity color parsing
        string noteName = noteData.GetNoteName();
        string hexColor = GameConfigs.GetWindowColor(noteName);
        Color windowColor;
        if (ColorUtility.TryParseHtmlString(hexColor, out windowColor))
        {
            hitWindowIndicator.color = windowColor;
        }

        // Calculate when the note should be at the target
        float noteArrivalTime = noteData.startTime;
        float totalDistance = Vector3.Distance(startPosition, endPosition);
        float travelSpeed = totalDistance / travelTime;

        // Calculate current time relative to note arrival
        float timeUntilHit = noteArrivalTime - Time.time;

        // Show window if in hit range OR if note has been hit
        bool inHitWindow = Mathf.Abs(timeUntilHit) <= hitWindow;
        bool shouldShowWindow = isHit;
        hitWindowIndicator.enabled = !shouldShowWindow;

        // Position the hit window
        if (shouldShowWindow)
        {
            Vector3 notePosition = transform.position;
            Vector3 direction = (endPosition - startPosition).normalized;
            float hitWindowDistance = hitWindow * travelSpeed;

            if (isHit)
            {
                // When hit: position window at the left side of the board (same X position for all hits)
                Vector3 boardLeftPosition = new Vector3(endPosition.x, notePosition.y, notePosition.z);
                hitWindowIndicator.transform.position = boardLeftPosition;
            }
            else
            {
                // Before hit: normal positioning at left side of note
                hitWindowIndicator.transform.localPosition = Vector3.zero;
            }

            // Scale the window based on hit window time
            hitWindowIndicator.transform.localScale = new Vector3(hitWindowDistance, 1f, 1f);
        }
    }
}
