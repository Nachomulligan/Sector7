using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManagerGameplay : MonoBehaviour
{
    [Header("Gameplay References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private MechaAbility playerAbility;

    [Header("Health")]
    [SerializeField] private TMP_Text healthText;

    [Header("Ability")]
    [SerializeField] private Button abilityButton;
    [SerializeField] private Image abilityIconImage;
    [SerializeField] private Image abilityCooldownFillImage;

    [Header("Score")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Rounds")]
    [SerializeField] private TMP_Text waveText;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button reviveButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Pause")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button pauseMainMenuButton;

    private ScoreManager scoreManager;
    private GameManager gameManager;
    private bool hasStarted;

    private void Start()
    {
        hasStarted = true;
        BindGameplayServices();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        if (abilityButton != null)
        {
            abilityButton.onClick.AddListener(HandleAbilityButtonPressed);
        }

        if (playerAbility != null)
        {
            playerAbility.OnCooldownChanged += HandleCooldownChanged;
            playerAbility.OnAbilityChanged += HandleAbilityChanged;

            HandleAbilityChanged(playerAbility.CurrentAbility);
            HandleCooldownChanged(playerAbility.IsReady ? 0f : 1f);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(HandlePauseButtonPressed);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(HandleContinueButtonPressed);
        }

        if (pauseMainMenuButton != null)
        {
            pauseMainMenuButton.onClick.AddListener(HandleMainMenuButtonPressed);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (hasStarted) BindGameplayServices();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (reviveButton != null)
        {
            reviveButton.onClick.AddListener(HandleReviveButtonPressed);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(HandleMainMenuButtonPressed);
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
        }

        if (abilityButton != null)
        {
            abilityButton.onClick.RemoveListener(HandleAbilityButtonPressed);
        }

        if (playerAbility != null)
        {
            playerAbility.OnCooldownChanged -= HandleCooldownChanged;
            playerAbility.OnAbilityChanged -= HandleAbilityChanged;
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(HandlePauseButtonPressed);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(HandleContinueButtonPressed);
        }

        if (pauseMainMenuButton != null)
        {
            pauseMainMenuButton.onClick.RemoveListener(HandleMainMenuButtonPressed);
        }

        UnbindGameplayServices();

        if (reviveButton != null)
        {
            reviveButton.onClick.RemoveListener(HandleReviveButtonPressed);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(HandleMainMenuButtonPressed);
        }
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (healthText != null)
        {
            healthText.text = $"{current} / {max}";
        }
    }

    private void HandleScoreChanged(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"{newScore}";
        }
    }

    private void HandleWaveChanged(int waveNumber)
    {
        if (waveText != null)
        {
            waveText.text = waveNumber.ToString();
        }
    }

    private void HandleGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    private void HandleReviveButtonPressed()
    {
        if (gameManager != null)
        {
            gameManager.Revive();
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void HandleMainMenuButtonPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    private void HandleAbilityButtonPressed()
    {
        if (playerAbility != null)
        {
            playerAbility.TryActivate();
        }
    }

    private void HandleAbilityChanged(SkillStrategySO newAbility)
    {
        Debug.Log($"[UIManagerGameplay] HandleAbilityChanged: newAbility={(newAbility != null ? newAbility.name : "null")}, " +
                  $"icon={(newAbility != null && newAbility.Icon != null ? newAbility.Icon.name : "null")}, " +
                  $"abilityIconImage asignado={(abilityIconImage != null)}");

        if (abilityIconImage != null)
        {
            abilityIconImage.sprite = newAbility != null ? newAbility.Icon : null;
            abilityIconImage.enabled = newAbility != null && newAbility.Icon != null;
        }
    }

    private void HandleCooldownChanged(float normalized)
    {
        if (abilityCooldownFillImage != null)
        {
            abilityCooldownFillImage.fillAmount = Mathf.Clamp01(normalized);
        }

        if (abilityButton != null)
        {
            abilityButton.interactable = normalized <= 0f;
        }
    }

    private void HandlePauseButtonPressed()
    {
        if (gameManager != null)
        {
            gameManager.TogglePause();
        }
    }

    private void HandlePauseChanged(bool paused)
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(paused);
        }
    }

    private void HandleContinueButtonPressed()
    {
        if (gameManager != null)
        {
            gameManager.Resume();
        }
    }

    private void BindGameplayServices()
    {
        UnbindGameplayServices();

        if (ServiceLocator.Instance.TryGet(out scoreManager))
        {
            scoreManager.OnScoreChanged += HandleScoreChanged;
            HandleScoreChanged(scoreManager.CurrentScore);
        }

        if (ServiceLocator.Instance.TryGet(out gameManager))
        {
            gameManager.OnWaveChanged += HandleWaveChanged;
            gameManager.OnGameOver += HandleGameOver;
            gameManager.OnPauseChanged += HandlePauseChanged;

            if (gameManager.CurrentWaveNumber > 0)
                HandleWaveChanged(gameManager.CurrentWaveNumber);
        }
    }

    private void UnbindGameplayServices()
    {
        if (scoreManager != null)
            scoreManager.OnScoreChanged -= HandleScoreChanged;

        if (gameManager != null)
        {
            gameManager.OnWaveChanged -= HandleWaveChanged;
            gameManager.OnGameOver -= HandleGameOver;
            gameManager.OnPauseChanged -= HandlePauseChanged;
        }

        scoreManager = null;
        gameManager = null;
    }
}
