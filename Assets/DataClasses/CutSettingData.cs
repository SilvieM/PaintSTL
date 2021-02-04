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
        public Modifier modifier;
        public double minDepth;
        public int mainColorId;
        public CutSettingData(int colorNum, Algorithm.AlgorithmType algo, double depth, Modifier modifier, double minDepth)
        {
            ColorNum = colorNum;
            this.algo = algo;
            this.depth = depth;
            this.modifier = modifier;
            this.minDepth = minDepth;
        }

        public enum Modifier { None, DepthDependant, Compute, StraightNormals }
    }
}
