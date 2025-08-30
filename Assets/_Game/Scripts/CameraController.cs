using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public SpriteRenderer targetSprite;

    [Header("Padding")]
    public float paddingTop = 1f;
    public float paddingBottom = 1f;
    public float paddingLeft = 1f;
    public float paddingRight = 1f;

    [Header("Camera Settings")]
    public bool autoAdjust = true;
    public float minOrthographicSize = 1f;
    public float maxOrthographicSize = 10f;

    [Header("View Mode")]
    public ViewMode viewMode = ViewMode.FitAll;

    public enum ViewMode
    {
        FitAll,     // Fits the entire sprite with padding
        Square,     // Forces a square view (uses the larger dimension)
        Custom      // Uses custom orthographic size
    }

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        if (autoAdjust)
        {
            AdjustCameraToFitSprite();
        }
    }

    void Update()
    {
        if (autoAdjust && targetSprite != null)
        {
            AdjustCameraToFitSprite();
        }
    }

    [ContextMenu("Adjust Camera to Fit Sprite")]
    public void AdjustCameraToFitSprite()
    {
        if (targetSprite == null || cam == null) return;

        // Get the sprite bounds in world space
        Bounds spriteBounds = targetSprite.bounds;

        // Calculate the required orthographic size to fit the sprite with custom padding
        float spriteHeight = spriteBounds.size.y;
        float spriteWidth = spriteBounds.size.x;

        // Calculate orthographic size needed for height (top + bottom padding)
        float heightSize = (spriteHeight + paddingTop + paddingBottom) / 2f;

        // Calculate orthographic size needed for width (left + right padding, considering aspect ratio)
        float widthSize = (spriteWidth + paddingLeft + paddingRight) / (2f * cam.aspect);

        float requiredSize;

        switch (viewMode)
        {
            case ViewMode.FitAll:
                // Use the larger size to ensure the sprite fits completely
                requiredSize = Mathf.Max(heightSize, widthSize);
                break;

            case ViewMode.Square:
                // Force square view - use the larger dimension to ensure square aspect
                float maxDimension = Mathf.Max(spriteHeight + paddingTop + paddingBottom,
                                              spriteWidth + paddingLeft + paddingRight);
                requiredSize = maxDimension / 2f;
                break;

            case ViewMode.Custom:
                // Use current size or a custom value
                requiredSize = cam.orthographicSize;
                break;

            default:
                requiredSize = Mathf.Max(heightSize, widthSize);
                break;
        }

        // Clamp the size to min/max values
        requiredSize = Mathf.Clamp(requiredSize, minOrthographicSize, maxOrthographicSize);

        // Set the camera's orthographic size
        cam.orthographicSize = requiredSize;

        // Center the camera on the sprite with offset based on padding differences
        Vector3 targetPosition = spriteBounds.center;

        // Apply horizontal offset if left/right padding is different
        float horizontalOffset = (paddingRight - paddingLeft) / 2f;
        targetPosition.x += horizontalOffset;

        // Apply vertical offset if top/bottom padding is different
        float verticalOffset = (paddingTop - paddingBottom) / 2f;
        targetPosition.y += verticalOffset;

        targetPosition.z = transform.position.z; // Keep the camera's current Z position
        transform.position = targetPosition;

        // Debug info
        Debug.Log($"Camera adjusted - Height size: {heightSize:F2}, Width size: {widthSize:F2}, " +
                  $"Final size: {requiredSize:F2}, View mode: {viewMode}");
    }

    // Method to set a new target sprite
    public void SetTarget(SpriteRenderer newTarget)
    {
        targetSprite = newTarget;
        if (autoAdjust)
        {
            AdjustCameraToFitSprite();
        }
    }

    // Method to set all padding values at once
    public void SetPadding(float top, float bottom, float left, float right)
    {
        paddingTop = top;
        paddingBottom = bottom;
        paddingLeft = left;
        paddingRight = right;

        if (autoAdjust)
        {
            AdjustCameraToFitSprite();
        }
    }

    // Method to set uniform padding
    public void SetUniformPadding(float padding)
    {
        paddingTop = padding;
        paddingBottom = padding;
        paddingLeft = padding;
        paddingRight = padding;

        if (autoAdjust)
        {
            AdjustCameraToFitSprite();
        }
    }

    // Method to set view mode
    public void SetViewMode(ViewMode mode)
    {
        viewMode = mode;
        if (autoAdjust)
        {
            AdjustCameraToFitSprite();
        }
    }

    // Individual padding setters
    public void SetPaddingTop(float padding) { paddingTop = padding; if (autoAdjust) AdjustCameraToFitSprite(); }
    public void SetPaddingBottom(float padding) { paddingBottom = padding; if (autoAdjust) AdjustCameraToFitSprite(); }
    public void SetPaddingLeft(float padding) { paddingLeft = padding; if (autoAdjust) AdjustCameraToFitSprite(); }
    public void SetPaddingRight(float padding) { paddingRight = padding; if (autoAdjust) AdjustCameraToFitSprite(); }
}
