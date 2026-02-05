using UnityEngine;
using System;

namespace ComBoom.Core
{
    public class LevelManager : MonoBehaviour
    {
        private const string TOTAL_XP_KEY = "ComBoom_TotalXP";

        private int totalXP;
        private int currentLevel;

        public int CurrentLevel => currentLevel;
        public int TotalXP => totalXP;

        public event Action<int> OnLevelChanged;

        public void Initialize()
        {
            totalXP = PlayerPrefs.GetInt(TOTAL_XP_KEY, 0);
            currentLevel = CalculateLevel(totalXP);
        }

        public void AddGameScore(int gameScore)
        {
            if (gameScore <= 0) return;
            totalXP += gameScore;
            PlayerPrefs.SetInt(TOTAL_XP_KEY, totalXP);
            PlayerPrefs.Save();

            int newLevel = CalculateLevel(totalXP);
            if (newLevel > currentLevel)
            {
                currentLevel = newLevel;
                OnLevelChanged?.Invoke(currentLevel);
            }
        }

        public int XPForLevel(int level)
        {
            return level * level * 500;
        }

        public int CalculateLevel(int xp)
        {
            int level = 1;
            while (XPForLevel(level + 1) <= xp)
                level++;
            return level;
        }

        public float GetLevelProgress()
        {
            int currentThreshold = XPForLevel(currentLevel);
            int nextThreshold = XPForLevel(currentLevel + 1);
            int range = nextThreshold - currentThreshold;
            if (range <= 0) return 0f;
            return Mathf.Clamp01((float)(totalXP - currentThreshold) / range);
        }
    }
}
