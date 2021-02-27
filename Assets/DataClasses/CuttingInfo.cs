using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Classes;
using g3;

namespace Assets.Algorithms
{
    public class CuttingInfo
    {
        public DMesh3 mesh;
        public DMesh3 oldMesh;
        public CutSettingData data;
        public Dictionary<int, int> PointToPoint = new Dictionary<int, int>(); //first new vertex id, then old vertex id
    }
}
