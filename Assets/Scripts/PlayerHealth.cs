using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 10;

    private int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    public event Action<int, int> OnHealthChanged;
    public event Action OnPlayerDied;
    private GameOverUI gameOverUI;

    private void Awake()
    {
        currentHealth = maxHealth;

        gameOverUI = FindFirstObjectByType<GameOverUI>();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        if (currentHealth <= 0)
            return;

        currentHealth = Mathf.Max(currentHealth - amount, 0);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            OnPlayerDied?.Invoke();

            if (gameOverUI != null)
            {
                gameOverUI.ShowGameOver();
            }
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}