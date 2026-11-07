using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;

    [Header("Difficulty (optional)")]
    [Tooltip("Shows the difficulty of the run that just ended, e.g. 'Difficulty: Hard'.")]
    [SerializeField] private TMP_Text difficultyText;

    private void Start()
    {
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (difficultyText != null)
            difficultyText.text = $"Difficulty: {DifficultySettings.Current}";

        gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // Restart on the same difficulty, going straight into the game.
    public void Retry()
    {
        StartMenuUI.AutoStartOnLoad = true;
        ReloadScene();
    }

    // Go back to the start menu to pick a different difficulty.
    public void ChangeDifficulty()
    {
        StartMenuUI.AutoStartOnLoad = false;
        ReloadScene();
    }

    private void ReloadScene()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}