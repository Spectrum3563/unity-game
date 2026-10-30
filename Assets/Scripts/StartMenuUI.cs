using UnityEngine;

public class StartMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject startMenuPanel;

    private void Start()
    {
        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        startMenuPanel.SetActive(false);
    }
}