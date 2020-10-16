using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Assets
{
    public sealed class ColorManager
    {
        private static readonly ColorManager instance = new ColorManager();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ColorManager()
        {
        }

        private ColorManager()
        {
        }

        public static ColorManager Instance
        {
            get
            {
                return instance;
            }
        }

        public event Action<List<Color>> OnColorsChanged;

        public Color currentColor;

        private readonly Dictionary<Color, int> previousColors = new Dictionary<Color, int>();

        public void FieldPainted(Color color)
        {
            Debug.Log($"Field painted {color.ToString()}");
            if (previousColors.ContainsKey(color)) previousColors[color]++;
            else
            {
                previousColors.Add(color, 1);
                OnColorsChanged?.Invoke(previousColors.Keys.ToList());
            }
        }
    }
}