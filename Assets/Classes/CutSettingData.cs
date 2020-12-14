﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Classes
{
    public class CutSettingData
    {
        public int ColorNum;
        public Algorithm.AlgorithmType algo;
        public double depth;

        public CutSettingData(int colorNum, Algorithm.AlgorithmType algo, double depth)
        {
            ColorNum = colorNum;
            this.algo = algo;
            this.depth = depth;
        }
    }
}
