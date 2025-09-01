using UnityEngine;

public class GameBoardSetup : MonoBehaviour
{
    [Header("Setup")]
    public bool createGameBoardOnStart = true;
    public bool setupGameplayManager = true;

    void Start()
    {
        if (createGameBoardOnStart)
        {
            CreateGameBoard();
        }

        if (setupGameplayManager)
        {
            SetupGameplayManager();
        }
    }

    void CreateGameBoard()
    {
        // Check if GameBoard already exists
        GameBoardManager existingBoard = FindObjectOfType<GameBoardManager>();
        if (existingBoard != null)
        {
            Debug.Log("GameBoard already exists in scene!");
            return;
        }

        // Create GameBoard GameObject
        GameObject boardObj = new GameObject("GameBoard");
        boardObj.transform.position = Vector3.zero;

        // Add GameBoard component
        GameBoardManager gameBoard = boardObj.AddComponent<GameBoardManager>();

        Debug.Log("GameBoard created successfully!");
    }

    void SetupGameplayManager()
    {
        GameplayManager gameplayManager = FindObjectOfType<GameplayManager>();
        GameBoardManager gameBoard = FindObjectOfType<GameBoardManager>();

        if (gameplayManager != null && gameBoard != null)
        {
            // Use reflection to set the gameBoard field since it might not be compiled yet
            var field = typeof(GameplayManager).GetField("gameBoard",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(gameplayManager, gameBoard);
                Debug.Log("GameplayManager connected to GameBoard!");
            }
            else
            {
                Debug.LogWarning("Could not find gameBoard field in GameplayManager. Please assign manually in inspector.");
            }
        }
        else
        {
            Debug.LogWarning("GameplayManager or GameBoard not found. Please set up manually.");
        }
    }

    [ContextMenu("Create GameBoard")]
    void CreateGameBoardMenu()
    {
        CreateGameBoard();
    }

    [ContextMenu("Setup GameplayManager")]
    void SetupGameplayManagerMenu()
    {
        SetupGameplayManager();
    }
}
