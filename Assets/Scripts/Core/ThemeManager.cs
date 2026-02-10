using UnityEngine;
using System;

namespace ComBoom.Core
{
    /// <summary>
    /// Simple static color provider for the game.
    /// Provides consistent colors without theme switching.
    /// </summary>
    public class ThemeManager : MonoBehaviour
    {
        public static ThemeManager Instance { get; private set; }

        // Static colors used throughout the game
        public static readonly Color Primary = new Color(0.545f, 0.361f, 0.965f, 1f);      // #8B5CF6 Purple
        public static readonly Color Secondary = new Color(0.231f, 0.510f, 0.965f, 1f);    // #3B82F6 Blue
        public static readonly Color Background = new Color(0.039f, 0.059f, 0.118f, 1f);   // #0A0F1E
        public static readonly Color BackgroundAlt = new Color(0.059f, 0.086f, 0.161f, 1f);// #0F1629
        public static readonly Color TextPrimary = Color.white;
        public static readonly Color TextSecondary = new Color(0.580f, 0.639f, 0.722f, 1f);// #94A3B8
        public static readonly Color TextMuted = new Color(0.392f, 0.455f, 0.545f, 1f);    // #64748B
        public static readonly Color Success = new Color(0.063f, 0.725f, 0.506f, 1f);      // #10B981
        public static readonly Color Warning = new Color(0.961f, 0.620f, 0.043f, 1f);      // #F59E0B
        public static readonly Color Error = new Color(0.937f, 0.267f, 0.267f, 1f);        // #EF4444
        public static readonly Color GridCell = new Color(0.118f, 0.161f, 0.231f, 0.25f);  // slate-800/25

        // Block colors for game pieces
        public static readonly Color[] BlockColors = new Color[]
        {
            new Color(0.545f, 0.361f, 0.965f, 1f),  // Purple  #8B5CF6
            new Color(0.231f, 0.510f, 0.965f, 1f),  // Blue    #3B82F6
            new Color(0.063f, 0.725f, 0.506f, 1f),  // Green   #10B981
            new Color(0.961f, 0.620f, 0.043f, 1f),  // Gold    #F59E0B
            new Color(0.925f, 0.282f, 0.600f, 1f),  // Pink    #EC4899
            new Color(0.937f, 0.267f, 0.267f, 1f),  // Red     #EF4444
            new Color(0.024f, 0.714f, 0.831f, 1f),  // Cyan    #06B6D4
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public static Color GetBlockColor(int index)
        {
            return BlockColors[index % BlockColors.Length];
        }
    }
}
