using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboTank
{
    public class EvalState
    {
        public Action Action;
        public int Score;
        public Grid Grid;

        private EvalState previousState;

        public bool Evaluated = false;

        public EvalState(Action action, Grid grid)
        {
            this.Action = action;
            this.Grid = grid;
        }

        public EvalState(Action action, EvalState copy, int score, Position position)
            : this(action, new Grid(copy.Grid, position))
        {
            if (copy.Evaluated)
            {
                this.previousState = copy;
            }

            this.Score = copy.Score + score;
        }

        public override string ToString()
        {
            string prev;
            if (previousState != null)
            {
                prev = previousState.ToString();
            }
            else
            {
                prev = string.Format("{0}", Action);
            }

            return string.Format("{0} -> {1} {2} {3}", prev, Action, Score, Grid);
        }

        public Action GetRootAction()
        {
            EvalState rootState = this;
            while (rootState.previousState != null)
            {
                rootState = rootState.previousState;
            }

            return rootState.Action;
        }
    }

}
