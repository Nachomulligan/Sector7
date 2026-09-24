using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerGameplay : MonoBehaviour
{
    [Header("Referencias de gameplay")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private MechaAbility playerAbility;

    [Header("UI - Vida")]
    [SerializeField] private TMP_Text healthText;

    [Header("UI - Habilidad")]
    [SerializeField] private Button abilityButton;

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
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (healthText != null)
        {
            healthText.text = $"{current} / {max}";
        }
    }

    private void HandleAbilityButtonPressed()
    {
        if (playerAbility != null)
        {
            playerAbility.TryActivate();
        }
    }
}
