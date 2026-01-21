using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float timeSeconds = 300f; // 5 minutes

    private bool running = true;

    private void Update()
    {
        if (!running) return;

        timeSeconds -= Time.deltaTime;

        if (timeSeconds <= 0f)
        {
            timeSeconds = 0f;
            running = false;
            EndGame();
        }

        UpdateText();
    }

    private void UpdateText()
    {
        int minutes = Mathf.FloorToInt(timeSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeSeconds % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void EndGame()
    {
        // Option 1: stop time
        Time.timeScale = 0f;

        // Option 2: load game over scene
        // SceneManager.LoadScene("GameOver");

        // Option 3: quit game
        // Application.Quit();
    }
}

