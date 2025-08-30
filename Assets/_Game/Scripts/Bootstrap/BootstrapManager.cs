using UnityEngine;

public class BootstrapManager : MonoBehaviour
{
    [Header("Manager References")]
    public GameStateManager gameStateManager;
    public AudioManager audioManager;
    public TimeManager timeManager;
    public GameModeManager gameModeManager;
    public GameSettingsManager gameSettingsManager;
    public MetronomeManager metronomeManager;
    public ScoreManager scoreManager;
    public GameplayManager gameplayManager;
    public PianoKeyManager pianoKeyManager;
    public DataHandler dataHandler;
    public SongHandler songHandler;
    public GameBoard gameBoard;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InitializeServices();
    }

    private void InitializeServices()
    {
        Debug.Log("Starting service initialization...");

        // Register all managers as services
        if (gameStateManager != null)
        {
            ServiceLocator.Instance.RegisterService(gameStateManager);
        }

        if (audioManager != null)
        {
            ServiceLocator.Instance.RegisterService(audioManager);
        }

        if (timeManager != null)
        {
            ServiceLocator.Instance.RegisterService(timeManager);
        }

        if (gameModeManager != null)
        {
            ServiceLocator.Instance.RegisterService(gameModeManager);
        }

        if (gameSettingsManager != null)
        {
            ServiceLocator.Instance.RegisterService(gameSettingsManager);
        }

        if (metronomeManager != null)
        {
            ServiceLocator.Instance.RegisterService(metronomeManager);
        }

        if (scoreManager != null)
        {
            ServiceLocator.Instance.RegisterService(scoreManager);
        }

        if (gameplayManager != null)
        {
            ServiceLocator.Instance.RegisterService(gameplayManager);
        }

        if (pianoKeyManager != null)
        {
            ServiceLocator.Instance.RegisterService(pianoKeyManager);
        }

        if (dataHandler != null)
        {
            ServiceLocator.Instance.RegisterService(dataHandler);
        }

        if (songHandler != null)
        {
            ServiceLocator.Instance.RegisterService(songHandler);
        }

        if (gameBoard != null)
        {
            ServiceLocator.Instance.RegisterService(gameBoard);
        }

        // Initialize all services
        ServiceLocator.Instance.InitializeAllServices();

        Debug.Log("Service initialization complete!");
    }

    private void OnDestroy()
    {
        ServiceLocator.Instance.CleanupAllServices();
    }
}
