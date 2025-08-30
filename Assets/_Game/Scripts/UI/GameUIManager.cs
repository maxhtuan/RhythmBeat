using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;

public class GameUIManager : MonoBehaviour, IService
{
    [Header("UI Panels")]
    public GameObject titlePanel;
    public GameObject gameplayPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    public Action onSpeedUpTriggered;


    [SerializeField] GameObject speedUpPanel; // the panel that shows when speed up is triggered

    [Header("Title Screen")]
    public Button startButton;
    public Button settingsButton;
    public Button quitButton;
    public TextMeshProUGUI titleText;

    [Header("Gameplay UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI measureText;
    public TextMeshProUGUI beatText;

    [Header("Pause Menu")]
    public Button resumeButton;
    public Button restartButton;
    public Button backToTitleButton;

    [Header("Game Over")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalAccuracyText;
    public TextMeshProUGUI gradeText;
    public TextMeshProUGUI maxComboText;
    public Button playAgainButton;
    public Button backToTitleFromGameOverButton;

    [Header("Settings")]
    public string gameTitle = "BeatEd";
    public string startInstruction = "Hit the first note to start!";

    private GameplayManager gameplayManager;
    private bool isInitialized = false;
    private bool isPlaying = false;
    private bool isPaused = false;

    // Remove Start() method - initialization will be called from GameplayManager

    public void Initialize()
    {
        if (isInitialized) return;

        // Find GameplayManager if not assigned
        if (gameplayManager == null)
        {
            gameplayManager = ServiceLocator.Instance.GetService<GameplayManager>();
        }

        onSpeedUpTriggered += OnSpeedUpTriggered;

        SetupUI();
        ShowTitleScreen();
        isInitialized = true;

        Debug.Log("GameUIManager: Initialized");
    }

    void SetupUI()
    {

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButtonClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);

        if (backToTitleButton != null)
            backToTitleButton.onClick.AddListener(OnBackToTitleClicked);

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);

        if (backToTitleFromGameOverButton != null)
            backToTitleFromGameOverButton.onClick.AddListener(OnBackToTitleClicked);
    }

    void Update()
    {
        if (isInitialized)
        {
            UpdateGameplayUI();
        }
    }

    private void UpdateGameplayUI()
    {
        if (gameplayManager == null) return;

        // Update title visibility based on game state
        if (titleText != null)
        {
            if (gameplayManager.IsPlaying)
            {
                // Hide title when game is playing
                titleText.gameObject.SetActive(false);
            }
            else if (gameplayManager.IsSetupComplete && !gameplayManager.IsPlaying)
            {
                // Show title when setup is complete but game hasn't started yet
                titleText.gameObject.SetActive(true);
                titleText.text = $"{gameTitle}\n{startInstruction}";
            }
        }

        // Update time (show game time when playing)
        if (timeText != null)
        {
            if (gameplayManager.IsPlaying)
            {
                float time = gameplayManager.GetCurrentTime();
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                timeText.text = $"Time: {minutes:00}:{seconds:00}";
            }
            else
            {
                timeText.text = "Time: 00:00";
            }
        }

        // Simple score (just show a placeholder)
        if (scoreText != null)
        {
            scoreText.text = "Score: 0";
        }

        // Simple combo (just show a placeholder)
        if (comboText != null)
        {
            comboText.text = "Combo: 0";
        }

        // Simple measure and beat (just show placeholders)
        if (measureText != null)
        {
            measureText.text = "Measure: 1";
        }

        if (beatText != null)
        {
            beatText.text = "Beat: 1";
        }
    }

    private void UpdateGameOverUI()
    {
        // Simple game over display
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: 0";
        }

        if (finalAccuracyText != null)
        {
            finalAccuracyText.text = "Accuracy: 0%";
        }

        if (gradeText != null)
        {
            gradeText.text = "Grade: F";
        }

        if (maxComboText != null)
        {
            maxComboText.text = "Max Combo: 0";
        }
    }

    private void ShowTitleScreen()
    {
        ShowPanel(titlePanel);
        HidePanel(gameplayPanel);
        HidePanel(pausePanel);
        HidePanel(gameOverPanel);

        if (titleText != null)
        {
            titleText.text = $"{gameTitle}\n{startInstruction}";
        }
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    private void HidePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }


    private void OnSettingsButtonClicked()
    {
        Debug.Log("Settings button clicked - implement settings menu");
    }

    private void OnPauseButtonClicked()
    {
        if (gameplayManager != null)
        {
            gameplayManager.PauseGame();
            isPaused = true;
            UpdateUIForState();
        }
    }


    private void OnRestartButtonClicked()
    {
        if (gameplayManager != null)
        {
            gameplayManager.RestartGame();
            isPlaying = false;
            isPaused = false;
            UpdateUIForState();
        }
    }

    private void OnQuitButtonClicked()
    {
        if (gameplayManager != null)
        {
            gameplayManager.RestartGame();
            isPlaying = false;
            isPaused = false;
            UpdateUIForState();
        }
    }

    private void OnBackToTitleClicked()
    {
        if (gameplayManager != null)
        {
            gameplayManager.RestartGame();
            isPlaying = false;
            isPaused = false;
            UpdateUIForState();
        }
    }

    private void OnPlayAgainClicked()
    {
        if (gameplayManager != null)
        {
            gameplayManager.RestartGame();
            isPlaying = false;
            isPaused = false;
            UpdateUIForState();
        }
    }

    private void UpdateUIForState()
    {
        if (isPlaying && !isPaused)
        {
            // Playing state
            HidePanel(titlePanel);
            ShowPanel(gameplayPanel);
            HidePanel(pausePanel);
            HidePanel(gameOverPanel);
        }
        else if (isPaused)
        {
            // Paused state
            HidePanel(titlePanel);
            HidePanel(gameplayPanel);
            ShowPanel(pausePanel);
            HidePanel(gameOverPanel);
        }
        else
        {
            // Menu state
            ShowTitleScreen();
        }
    }

    public async void OnSpeedUpTriggered()
    {
        ShowPanel(speedUpPanel);
        await Task.Delay(1500);
        HidePanel(speedUpPanel);
    }

    // Public method to be called when game starts from first hit
    public void OnGameStartedFromFirstHit()
    {
        // Hide the title when game starts
        if (titleText != null)
        {
            titleText.gameObject.SetActive(false);
        }

        // Update UI state
        isPlaying = true;
        isPaused = false;
        UpdateUIForState();
    }

    // Public method to be called when game is restarted
    public void OnGameRestarted()
    {
        // Show the title again when game is restarted
        if (titleText != null)
        {
            titleText.gameObject.SetActive(true);
            titleText.text = $"{gameTitle}\n{startInstruction}";
        }

        // Update UI state
        isPlaying = false;
        isPaused = false;
        UpdateUIForState();
    }

    // Public method to show game over screen
    public void ShowGameOver()
    {
        UpdateGameOverUI();
        ShowPanel(gameOverPanel);
        HidePanel(titlePanel);
        HidePanel(gameplayPanel);
        HidePanel(pausePanel);
    }

    public void Cleanup()
    {
        onSpeedUpTriggered = null;
    }
}
