using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [Header("References")]
    public GameplayManager gameplayManager;

    [Header("UI Panels")]
    public GameObject menuPanel;
    public GameObject gameplayPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    [Header("Gameplay UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI measureText;
    public TextMeshProUGUI beatText;
    public TextMeshProUGUI txtTitle;

    [Header("Title Settings")]
    public string gameTitle = "BeatEd";
    public string startInstruction = "Hit the first note to start!";
    [Header("Game Over UI")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI gradeText;
    public TextMeshProUGUI maxComboText;

    [Header("Buttons")]
    public Button startButton;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button quitButton;

    private bool isPlaying = false;
    private bool isPaused = false;

    void Start()
    {
        InitializeUI();
        SetupButtonListeners();
    }

    void Update()
    {
        UpdateGameplayUI();
    }

    private void InitializeUI()
    {
        ShowPanel(menuPanel);
        HidePanel(gameplayPanel);
        HidePanel(pausePanel);
        HidePanel(gameOverPanel);

        // Set up the title
        if (txtTitle != null)
        {
            txtTitle.text = $"{gameTitle}\n{startInstruction}";
        }
    }

    private void SetupButtonListeners()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseButtonClicked);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeButtonClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButtonClicked);
    }

    private void UpdateGameplayUI()
    {
        if (gameplayManager == null) return;

        // Update title visibility based on game state
        if (txtTitle != null)
        {
            if (gameplayManager.IsPlaying)
            {
                // Hide title when game is playing
                txtTitle.gameObject.SetActive(false);
            }
            else if (gameplayManager.IsSetupComplete && !gameplayManager.IsPlaying)
            {
                // Show title when setup is complete but game hasn't started yet
                txtTitle.gameObject.SetActive(true);
                txtTitle.text = $"{gameTitle}\n{startInstruction}";
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

        if (accuracyText != null)
        {
            accuracyText.text = "Accuracy: 0%";
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

    // Button event handlers
    private void OnStartButtonClicked()
    {
        if (gameplayManager != null)
        {
            gameplayManager.StartGame();
            isPlaying = true;
            isPaused = false;
            UpdateUIForState();
        }
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

    private void OnResumeButtonClicked()
    {
        if (gameplayManager != null)
        {
            gameplayManager.StartGame();
            isPlaying = true;
            isPaused = false;
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

    private void UpdateUIForState()
    {
        if (isPlaying && !isPaused)
        {
            // Playing state
            HidePanel(menuPanel);
            ShowPanel(gameplayPanel);
            HidePanel(pausePanel);
            HidePanel(gameOverPanel);
        }
        else if (isPaused)
        {
            // Paused state
            HidePanel(menuPanel);
            HidePanel(gameplayPanel);
            ShowPanel(pausePanel);
            HidePanel(gameOverPanel);
        }
        else
        {
            // Menu state
            ShowPanel(menuPanel);
            HidePanel(gameplayPanel);
            HidePanel(pausePanel);
            HidePanel(gameOverPanel);
        }
    }

    // Public method to be called when game starts from first hit
    public void OnGameStartedFromFirstHit()
    {
        // Hide the title when game starts
        if (txtTitle != null)
        {
            txtTitle.gameObject.SetActive(false);
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
        if (txtTitle != null)
        {
            txtTitle.gameObject.SetActive(true);
            txtTitle.text = $"{gameTitle}\n{startInstruction}";
        }

        // Update UI state
        isPlaying = false;
        isPaused = false;
        UpdateUIForState();
    }
}
