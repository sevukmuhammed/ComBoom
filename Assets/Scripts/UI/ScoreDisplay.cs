using UnityEngine;
using TMPro;

namespace ComBoom.UI
{
    public class ScoreDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;

        public void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = score.ToString("N0");
        }

        public void UpdateHighScore(int highScore)
        {
            if (highScoreText != null)
                highScoreText.text = highScore.ToString("N0");
        }
    }
}
