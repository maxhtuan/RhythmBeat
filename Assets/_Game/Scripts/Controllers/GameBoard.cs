using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [Header("Board Settings")]
    public float boardWidth = 10f;
    public float boardHeight = 6f;
    public int numberOfLanes = 7; // C, D, E, F, G, A, B

    [Header("Lane Colors")]
    public Color[] laneColors = {
        Color.red,    // C
        Color.orange, // D
        Color.yellow, // E
        Color.green,  // F
        Color.blue,   // G
        Color.magenta, // A
        Color.cyan    // B
    };

    [Header("Note Names")]
    public string[] noteNames = { "C", "D", "E", "F", "G", "A", "B" };

    [Header("Visual")]
    public SpriteRenderer boardRenderer;
    public Color boardColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    private float laneHeight;
    private Vector3[] lanePositions;
    private bool isInitialized = false;

    // Keep the original Start() method but make it call Initialize()
    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (isInitialized) return;

        InitializeBoard();
        isInitialized = true;

        Debug.Log("GameBoard: Initialized");
    }

    void InitializeBoard()
    {
        // Calculate lane height
        laneHeight = boardHeight / numberOfLanes;

        // Calculate lane positions (Y coordinates for horizontal movement)
        // C at bottom, B at top
        lanePositions = new Vector3[numberOfLanes];
        float startY = transform.position.y - (boardHeight / 2f) + (laneHeight / 2f);

        for (int i = 0; i < numberOfLanes; i++)
        {
            float yPos = startY + (i * laneHeight);
            lanePositions[i] = new Vector3(transform.position.x, yPos, transform.position.z);
        }

        // Create visual board if needed
        if (boardRenderer == null)
        {
            CreateBoardVisual();
        }

        Debug.Log($"GameBoard initialized with {numberOfLanes} lanes, lane height: {laneHeight:F2}");
    }

    void CreateBoardVisual()
    {
        // Create board GameObject
        GameObject boardObj = new GameObject("BoardVisual");
        boardObj.transform.SetParent(transform);
        boardObj.transform.localPosition = Vector3.zero;

        // Add SpriteRenderer
        boardRenderer = boardObj.AddComponent<SpriteRenderer>();
        boardRenderer.sprite = CreateRectangleSprite(boardWidth, boardHeight);
        boardRenderer.color = boardColor;
        boardRenderer.sortingOrder = -1; // Behind notes

    }


    Sprite CreateRectangleSprite(float width, float height)
    {
        int textureWidth = Mathf.RoundToInt(width * 10);
        int textureHeight = Mathf.RoundToInt(height * 10);

        Texture2D texture = new Texture2D(textureWidth, textureHeight);
        Color[] pixels = new Color[textureWidth * textureHeight];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f));
    }

    // Get lane position for a specific note
    public Vector3 GetLanePosition(string noteName)
    {
        // Ensure board is initialized
        if (!isInitialized || lanePositions == null)
        {
            Debug.LogWarning($"GameBoard not initialized when GetLanePosition called for {noteName}. Initializing now...");
            Initialize();
        }

        int laneIndex = GetLaneIndex(noteName);
        if (laneIndex >= 0 && laneIndex < lanePositions.Length)
        {
            return lanePositions[laneIndex];
        }
        return transform.position; // Fallback to center
    }

    // Get lane index for a note
    public int GetLaneIndex(string noteName)
    {
        if (string.IsNullOrEmpty(noteName)) return 0;

        // Extract the note letter (C, D, E, F, G, A, B)
        string noteLetter = noteName.Substring(0, 1).ToUpper();

        for (int i = 0; i < noteNames.Length; i++)
        {
            if (noteNames[i] == noteLetter)
            {
                return i;
            }
        }

        return 0; // Default to first lane
    }

    // Get lane color for a note
    public Color GetLaneColor(string noteName)
    {
        int laneIndex = GetLaneIndex(noteName);
        if (laneIndex >= 0 && laneIndex < laneColors.Length)
        {
            return laneColors[laneIndex];
        }
        return Color.white; // Default color
    }

    // Get spawn position for a note (right side of the board)
    public Vector3 GetSpawnPosition(string noteName)
    {
        Vector3 lanePos = GetLanePosition(noteName);
        return new Vector3(transform.position.x + (boardWidth / 2f), lanePos.y, lanePos.z);
    }

    // Get target position for a note (left side of the board)
    public Vector3 GetTargetPosition(string noteName)
    {
        Vector3 lanePos = GetLanePosition(noteName);
        return new Vector3(transform.position.x - (boardWidth / 2f), lanePos.y, lanePos.z);
    }

    // Check if board is initialized
    public bool IsInitialized()
    {
        return isInitialized && lanePositions != null && lanePositions.Length > 0;
    }

    // Get board bounds
    public Bounds GetBoardBounds()
    {
        Vector3 center = transform.position;
        Vector3 size = new Vector3(boardWidth, boardHeight, 1f);
        return new Bounds(center, size);
    }

    void OnDrawGizmos()
    {
        // Draw board outline
        Gizmos.color = Color.gray;
        Vector3 center = transform.position;
        Vector3 size = new Vector3(boardWidth, boardHeight, 0.1f);
        Gizmos.DrawWireCube(center, size);

        // Draw lane positions
        if (lanePositions != null)
        {
            for (int i = 0; i < lanePositions.Length; i++)
            {
                Gizmos.color = laneColors[i % laneColors.Length];
                Gizmos.DrawLine(
                    new Vector3(center.x - (boardWidth / 2f), lanePositions[i].y, center.z),
                    new Vector3(center.x + (boardWidth / 2f), lanePositions[i].y, center.z)
                );
            }
        }
    }
}
