using UnityEngine;
using System;

namespace ComBoom.Core
{
    public class ScoreManager : MonoBehaviour
    {
        private int currentScore;
        private int highScore;

        private const string HIGH_SCORE_KEY = "ComBoom_HighScore";

        public int CurrentScore => currentScore;
        public int HighScore => highScore;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnHighScoreChanged;

        private void Awake()
        {
            highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        }

        public void Initialize()
        {
            currentScore = 0;
            highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
            OnScoreChanged?.Invoke(currentScore);
            OnHighScoreChanged?.Invoke(highScore);
        }

        public void AddPlacementScore(int blockCount)
        {
            int points = blockCount * 10;
            AddScore(points);
        }

        public int AddLineClearScore(int lineCount, int comboMultiplier = 1)
        {
            if (lineCount <= 0) return 0;

            int score = 0;
            for (int i = 1; i <= lineCount; i++)
            {
                score += i * 100;
            }
            score *= comboMultiplier;
            AddScore(score);
            return score;
        }

        private void AddScore(int points)
        {
            currentScore += points;
            OnScoreChanged?.Invoke(currentScore);

            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
                PlayerPrefs.Save();
                OnHighScoreChanged?.Invoke(highScore);
            }
        }

        public void ResetScore()
        {
            currentScore = 0;
            OnScoreChanged?.Invoke(currentScore);
        }

        public void SetScore(int score)
        {
            currentScore = score;
            OnScoreChanged?.Invoke(currentScore);
        }
    }
}
