using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Dalek
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager instance;

        private int score = 0;
        [SerializeField] private TMP_Text scoreText;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AddScore(int points)
        {
            score += points;
            UpdateScoreUI();
        }

        void UpdateScoreUI()
        {
            if (scoreText != null)
            {
                scoreText.text = "Score: " + score;
            }
        }

        public int GetScore()
        {
            return score;
        }
    }
}