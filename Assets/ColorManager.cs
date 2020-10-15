using System.Collections.Generic;
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


        public Color currentColor;

        public Dictionary<Color, int> previousColors;

        public void FieldPainted(Color color)
        {
            if (previousColors.ContainsKey(color)) previousColors[color]++;
            else previousColors.Add(color, 1);
        }
    }
}