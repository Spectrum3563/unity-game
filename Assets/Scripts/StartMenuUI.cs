using TMPro;
using UnityEngine;

public class StartMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject startMenuPanel;

    [Header("Difficulty (optional)")]
    [Tooltip("Shows the selected difficulty, e.g. 'Difficulty: Hard'.")]
    [SerializeField] private TMP_Text difficultyText;

    private static int gameStartedFrame;

    // Set by GameOverUI.Retry so the scene reloads straight into the game,
    // skipping the start menu and keeping the current difficulty.
    public static bool AutoStartOnLoad;

    public static bool CanPlaceTowers =>
        Time.timeScale > 0f && Time.frameCount > gameStartedFrame;

    private void Awake()
    {
        gameStartedFrame = int.MaxValue;
    }

    private void Start()
    {
        if (AutoStartOnLoad)
        {
            AutoStartOnLoad = false;
            StartGame();
            return;
        }

        Time.timeScale = 0f;
        UpdateDifficultyText();
    }

    // Hook the Easy / Normal / Hard buttons to this method in their OnClick
    // list, typing 0, 1 or 2 as the number (0 = Easy, 1 = Normal, 2 = Hard).
    public void SetDifficulty(int level)
    {
        DifficultySettings.Current = (DifficultyLevel)Mathf.Clamp(level, 0, 2);
        UpdateDifficultyText();
    }

    // Hook the Easy / Normal / Hard buttons to this method instead of
    // SetDifficulty to pick the difficulty AND start the game in one click.
    // Type 0, 1 or 2 as the number (0 = Easy, 1 = Normal, 2 = Hard).
    public void StartGameWithDifficulty(int level)
    {
        SetDifficulty(level);
        StartGame();
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        gameStartedFrame = Time.frameCount;
        startMenuPanel.SetActive(false);
    }

    private void UpdateDifficultyText()
    {
        if (difficultyText != null)
            difficultyText.text = $"Difficulty: {DifficultySettings.Current}";
    }
}