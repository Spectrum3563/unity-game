using TMPro;
using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private TMP_Text hpText;

    private void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealthUI: Player Health is not assigned.");
            return;
        }

        if (hpText == null)
        {
            Debug.LogError("PlayerHealthUI: HP Text is not assigned.");
            return;
        }

        playerHealth.OnHealthChanged += UpdateHealthText;

        UpdateHealthText(
            playerHealth.CurrentHealth,
            playerHealth.MaxHealth
        );
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthText;
        }
    }

    private void UpdateHealthText(int currentHealth, int maxHealth)
    {
        hpText.text = $"HP: {currentHealth}/{maxHealth}";
    }
}