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
    public GameplayManager gameplayManager;
    public DataHandler dataHandler;
    public SongHandler songHandler;
    public GameBoardManager gameBoard;
    public NoteManager noteManager;
    public GameUIManager gameUIManager;
    public FirebaseManager firebaseManager;
    public GameplayLogger gameplayLogger;
    public PianoInputHandler pianoInputHandler;
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

        if (gameplayManager != null)
        {
            ServiceLocator.Instance.RegisterService(gameplayManager);
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

        if (noteManager != null)
        {
            ServiceLocator.Instance.RegisterService(noteManager);
        }

        if (gameUIManager != null)
        {
            ServiceLocator.Instance.RegisterService(gameUIManager);
        }

        if (firebaseManager != null)
        {
            ServiceLocator.Instance.RegisterService(firebaseManager);
        }

        if (gameplayLogger != null)
        {
            ServiceLocator.Instance.RegisterService(gameplayLogger);
        }

        if (pianoInputHandler != null)
        {
            ServiceLocator.Instance.RegisterService(pianoInputHandler);
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
