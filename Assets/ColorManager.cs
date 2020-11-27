using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            usedColors.Add(Color.white, 0);
        }

        public static ColorManager Instance
        {
            get
            {

                return instance;
            }
        }

        public event Action<List<Color>> OnColorsChanged;
        public event Action OnCurrentColorChanged = delegate {  };

        public Color currentColor
        {
            get => _currentColor;
            set
            {
                _currentColor = value;
                OnCurrentColorChanged.Invoke();
            }
        }

        public int? currentColorId => GetColorId(currentColor);

        private readonly Dictionary<Color, int> usedColors = new Dictionary<Color, int>();
        private Color _currentColor;

        public int? GetColorId(Color color)
        {
            if (usedColors.ContainsKey(color)) return usedColors[color];
            else return null;
        }

        public Color GetColorForId(int id)
        {
            if (usedColors.ContainsValue(id)) return usedColors.First(pair => pair.Value == id).Key;
            else return Color.white;

        }
        public int FieldPainted(Color color)
        {
            if (usedColors.ContainsKey(color))
            {
                return usedColors[color];
            }
            else
            {
                int number = usedColors.Count; //indices will start with 1 for now, as 0 is "not colored"
                usedColors.Add(color, number);
                OnColorsChanged?.Invoke(usedColors.Keys.ToList());
                return number;
            }
        }
    }
}