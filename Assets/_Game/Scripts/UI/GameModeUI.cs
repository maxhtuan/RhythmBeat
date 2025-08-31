using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameModeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [Header("Mode Names")]
    [SerializeField] private string performModeName = "Perform";
    [SerializeField] private string speedUpModeName = "Speed Up";

    private GameModeManager gameModeManager;
    private GameModeType[] availableModes = { GameModeType.Perform, GameModeType.SpeedUp };
    private int currentModeIndex = 0;

    void Start()
    {
        InitializeUI();
    }

    void InitializeUI()
    {
        // Get GameModeManager from ServiceLocator
        gameModeManager = ServiceLocator.Instance.GetService<GameModeManager>();

        if (gameModeManager == null)
        {
            Debug.LogError("GameModeUI: GameModeManager not found!");
            return;
        }

        // Set up button listeners
        if (leftButton != null)
        {
            leftButton.onClick.AddListener(OnLeftButtonClicked);
        }

        if (rightButton != null)
        {
            rightButton.onClick.AddListener(OnRightButtonClicked);
        }

        // Set initial mode index based on current mode
        SetCurrentModeIndex(gameModeManager.currentMode);

        // Update UI
        UpdateModeDisplay();

        Debug.Log("GameModeUI: Initialized");
    }

    void OnLeftButtonClicked()
    {
        SwitchToPreviousMode();
    }

    void OnRightButtonClicked()
    {
        SwitchToNextMode();
    }

    public void SwitchToNextMode()
    {
        currentModeIndex = (currentModeIndex + 1) % availableModes.Length;
        SwitchToMode(availableModes[currentModeIndex]);
    }

    public void SwitchToPreviousMode()
    {
        currentModeIndex = (currentModeIndex - 1 + availableModes.Length) % availableModes.Length;
        SwitchToMode(availableModes[currentModeIndex]);
    }

    void SwitchToMode(GameModeType modeType)
    {
        if (gameModeManager != null)
        {
            gameModeManager.SetMode(modeType);
            var gameplayManager = ServiceLocator.Instance.GetService<GameplayManager>();
            if (gameplayManager != null)
            {
                gameplayManager.RestartGame();
            }

            UpdateModeDisplay();
            Debug.Log($"GameModeUI: Switched to {GetModeDisplayName(modeType)}");
        }
    }

    void SetCurrentModeIndex(GameModeType modeType)
    {
        for (int i = 0; i < availableModes.Length; i++)
        {
            if (availableModes[i] == modeType)
            {
                currentModeIndex = i;
                break;
            }
        }
    }

    void UpdateModeDisplay()
    {
        if (modeText != null && gameModeManager != null)
        {
            string displayName = GetModeDisplayName(gameModeManager.currentMode);
            modeText.text = displayName;
        }
    }

    string GetModeDisplayName(GameModeType modeType)
    {
        switch (modeType)
        {
            case GameModeType.Perform:
                return performModeName;
            case GameModeType.SpeedUp:
                return speedUpModeName;
            default:
                return "Unknown";
        }
    }

    // Public methods for external access
    public void SwitchToPerformMode()
    {
        SwitchToMode(GameModeType.Perform);
    }

    public void SwitchToSpeedUpMode()
    {
        SwitchToMode(GameModeType.SpeedUp);
    }

    // Get current mode name
    public string GetCurrentModeName()
    {
        return gameModeManager != null ? GetModeDisplayName(gameModeManager.currentMode) : "Unknown";
    }
}
