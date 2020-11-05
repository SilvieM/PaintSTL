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

        public Color currentColor; //int

        private readonly Dictionary<Color, int> usedColors = new Dictionary<Color, int>();

        public int? GetColorId(Color color)
        {
            if (usedColors.ContainsKey(color)) return usedColors[color];
            else return null;
        }
        public int FieldPainted(Color color)
        {
            if (usedColors.ContainsKey(color))
            {
                return usedColors[color];
            }
            else
            {
                int number = usedColors.Count+1; //indices will start with 1 for now, as 0 is "not colored"
                usedColors.Add(color, number);
                OnColorsChanged?.Invoke(usedColors.Keys.ToList());
                return number;
            }
        }
    }
}