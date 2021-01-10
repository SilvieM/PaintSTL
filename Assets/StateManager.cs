using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{
    public sealed class StateManager
    {
        private static readonly StateManager instance = new StateManager();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static StateManager()
        {
        }

        private StateManager()
        {
        }

        public static StateManager Instance
        {
            get
            {

                return instance;
            }
        }

        public List<PaintAction> actions = new List<PaintAction>();

        public void SaveAction(PaintAction action)
        {
            actions.Add(action);
        }

        public PaintAction PeekLastAction()
        {
            if (actions.Count > 0)
                return actions.Last();
            else return null;
        }

        public void CommitLastAction()
        {
            actions.RemoveAt(actions.Count-1);
        }

        public class PaintAction
        {
            /// <summary>
            /// TriId = Key, previous Color Id = Value
            /// </summary>
            public Dictionary<int, int> painted;

            public PaintAction(Dictionary<int, int> painted)
            {
                this.painted = painted;
            }
        }
    }
}
