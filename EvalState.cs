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

        public EvalState(Action action, EvalState previousState, int score, Grid grid)
            : this(action, grid)
        {

            if (previousState.Evaluated)
            {
                this.previousState = previousState;
            }

            this.Score = previousState.Score + score;
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
                prev = "Start";
            }

            if (prev.Length > 1000)
            {
                return prev;
            }
            else
            {
                return string.Format("{0} -> {1} {2} {3}", prev, Action, Score, Grid);
            }
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
