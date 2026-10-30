using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }

    public GameState currentState = GameState.Menu;
    public RectTransform titlePanel;
    public GameObject startButton;
    public Camera mainCamera;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    public void StartGame()
    {
        currentState = GameState.Playing;
        if (titlePanel != null)
        {
            titlePanel.gameObject.SetActive(false);
        }

        if (startButton != null)
        {
            startButton.SetActive(false);
        }
    }
}
