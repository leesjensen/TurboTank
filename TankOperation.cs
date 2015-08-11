using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboTank
{
    public abstract class TankOperation
    {
        public abstract Action GetAction();
        public abstract EvalState GetScore(EvalState state, SignalWeights weights);
    }


    public class TankOperationLeft : TankOperation
    {
        public override Action GetAction() { return Action.Left; }

        public override EvalState GetScore(EvalState state, SignalWeights weights)
        {
            int score = 0;
            Position leftPosition = state.Grid.GetLeft();
            char item = state.Grid.GetItem(leftPosition);
            if (item == '_')
            {
                score = 10;
            }

            Position curPosition = state.Grid.Position;
            Position newPosition = new Position(curPosition.X, curPosition.Y, leftPosition.Orientation);
            return new EvalState(GetAction(), state, score, newPosition);
        }
    }

    public class TankOperationRight : TankOperation
    {
        public override Action GetAction() { return Action.Right; }

        public override EvalState GetScore(EvalState state, SignalWeights weights)
        {
            int score = 0;
            Position rightPosition = state.Grid.GetRight();
            char item = state.Grid.GetItem(rightPosition);
            if (item == '_')
            {
                score = 10;
            }

            Position curPosition = state.Grid.Position;
            Position newPosition = new Position(curPosition.X, curPosition.Y, rightPosition.Orientation);
            return new EvalState(GetAction(), state, score, newPosition);
        }
    }

    public class TankOperationFire : TankOperation
    {
        public override Action GetAction() { return Action.Fire; }

        public override EvalState GetScore(EvalState state, SignalWeights weights)
        {
            int distance = 1;
            int score = -1000;

            if (state.Grid.Energy > 0)
            {
                foreach (Position aheadPosition in state.Grid.LookAhead())
                {
                    char item = state.Grid.GetItem(aheadPosition);
                    if (item == 'W')
                    {
                        score = -100;
                        break;
                    }
                    else if ((item == 'L') && (state.Grid.Energy >= 10))
                    {
                        score = 100;
                        break;
                    }
                    else if (item == 'O')
                    {
                        score = (10 - distance) * 100;
                        break;
                    }
                    else if (item == 'B')
                    {
                        if (state.Grid.Health >= 300)
                        {
                            score = (15 - distance) * 10;
                        }
                        break;
                    }

                    distance++;
                }
            }

            EvalState fireState = new EvalState(GetAction(), state, score, state.Grid.Position);
            fireState.Grid.Energy -= 1;
            return fireState;
        }
    }

    public class TankOperationMove : TankOperation
    {
        public override Action GetAction() { return Action.Move; }

        public override EvalState GetScore(EvalState state, SignalWeights weights)
        {
            int distance = 1;
            int score = 0;
            int energyGained = 0;
            int healthGained = 0;
            foreach (Position aheadPosition in state.Grid.LookAhead())
            {
                char item = state.Grid.GetItem(aheadPosition);
                if (item == 'B')
                {
                    if (state.Grid.Health < 100)
                    {
                        score = 1000;
                    }
                    else
                    {
                        score = (15 - distance) * 50;
                    }

                    if (distance == 1)
                    {
                        energyGained = 5;
                        healthGained = 20;
                    }
                    break;
                }
                else if (item == 'L')
                {
                    score = -200;
                    break;
                }
                else if (item == 'W' && distance == 1)
                {
                    score = -1000;
                    break;
                }
                else if (item == '_')
                {
                    score = 15;
                }

                distance++;
            }

            EvalState moveState = new EvalState(GetAction(), state, score, state.Grid.GetAhead());
            moveState.Grid.Energy += energyGained;
            moveState.Grid.Health += healthGained;
            return moveState;
        }
    }

    public class TankOperationNoop : TankOperation
    {
        public override Action GetAction() { return Action.Noop; }

        public override EvalState GetScore(EvalState state, SignalWeights weights)
        {
            return new EvalState(GetAction(), state, 0, state.Grid.Position);
        }
    }
}
