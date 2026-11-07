using UnityEngine;

public class StartMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject startMenuPanel;

    private static int gameStartedFrame;

    public static bool CanPlaceTowers =>
        Time.timeScale > 0f && Time.frameCount > gameStartedFrame;

    private void Awake()
    {
        gameStartedFrame = int.MaxValue;
    }

    private void Start()
    {
        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        gameStartedFrame = Time.frameCount;
        startMenuPanel.SetActive(false);
    }
}