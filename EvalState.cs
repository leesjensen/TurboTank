using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboTank
{
    public class EvalState
    {
        public TankAction Action;
        public int Score;
        public Grid Grid;

        private EvalState previousState;

        public bool Evaluated = false;

        public EvalState(TankAction action, Grid grid)
        {
            this.Action = action;
            this.Grid = grid;
        }

        public EvalState(TankAction action, EvalState previousState, int score, Grid grid)
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
                string result = "";
                if (Grid.BatteryFound) result += "@";
                if (Grid.EnemyHit) result += "!";

                return string.Format("{0} -> {1}{2} {3} {4}", prev, Action, result, Score, Grid);
            }
        }

        public TankAction GetRootAction()
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
