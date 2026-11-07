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
        playerHealth.OnScoreChanged += UpdateScoreText;

        UpdateStatsText();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthText;
            playerHealth.OnScoreChanged -= UpdateScoreText;
        }
    }

    private void UpdateHealthText(int currentHealth, int maxHealth)
    {
        UpdateStatsText(currentHealth, maxHealth, playerHealth.Score);
    }

    private void UpdateScoreText(int score)
    {
        UpdateStatsText(
            playerHealth.CurrentHealth,
            playerHealth.MaxHealth,
            score
        );
    }

    private void UpdateStatsText()
    {
        UpdateStatsText(
            playerHealth.CurrentHealth,
            playerHealth.MaxHealth,
            playerHealth.Score
        );
    }

    private void UpdateStatsText(int currentHealth, int maxHealth, int score)
    {
        hpText.text = $"HP: {currentHealth}/{maxHealth}\nScore: {score}";
    }
}