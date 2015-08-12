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
            Grid grid = new Grid(state.Grid);

            int score = 0;
            Position leftPosition = grid.GetLeft();
            char item = grid.GetItem(leftPosition);
            if (item == '_')
            {
                score = 10;
            }

            grid.Position = new Position(grid.Position.X, grid.Position.Y, leftPosition.Orientation);
            return new EvalState(GetAction(), state, score, grid);
        }
    }

    public class TankOperationRight : TankOperation
    {
        public override Action GetAction() { return Action.Right; }

        public override EvalState GetScore(EvalState state, SignalWeights weights)
        {
            Grid grid = new Grid(state.Grid);

            int score = 0;
            Position rightPosition = grid.GetRight();
            char item = grid.GetItem(rightPosition);
            if (item == '_')
            {
                score = 10;
            }

            grid.Position = new Position(grid.Position.X, grid.Position.Y, rightPosition.Orientation);
            return new EvalState(GetAction(), state, score, grid);
        }
    }

    public class TankOperationFire : TankOperation
    {
        public override Action GetAction() { return Action.Fire; }

        public override EvalState GetScore(EvalState state, SignalWeights weights)
        {
            Grid grid = new Grid(state.Grid);

            int distance = 1;
            int score = -1000;

            if (grid.Energy > 0)
            {
                foreach (Position aheadPosition in grid.LookAhead())
                {
                    char item = grid.GetItem(aheadPosition);
                    if (item == 'W')
                    {
                        score = -100;
                        break;
                    }
                    else if ((item == 'L') && (grid.Energy >= 10))
                    {
                        score = 100;
                        grid.SetItem(aheadPosition, '_');
                        break;
                    }
                    else if (item == 'O')
                    {
                        score = (10 - distance) * 100;

                        grid.SetItem(aheadPosition, '_');
                        grid.EnemyHit = true;
                        break;
                    }
                    else if (item == 'B')
                    {
                        // TODO: Or if here is going to get there first then shoot it!
                        // TODO: Move if lasers are coming up from behind.
                        // TODO: One each evaluation move the lasers so they are in the right place.
                        // TODO: I should also influance going left or right based upon where the batteries and opponents are.
                        if (grid.Health >= 280 && grid.Energy >= 10)
                        {
                            score = (15 - distance) * 10;
                        }
                        grid.SetItem(aheadPosition, '_');
                        break;
                    }

                    distance++;
                }
            }

            grid.Energy -= 1;
            EvalState fireState = new EvalState(GetAction(), state, score, grid);
            return fireState;
        }
    }

    public class TankOperationMove : TankOperation
    {
        public override Action GetAction() { return Action.Move; }

        public override EvalState GetScore(EvalState state, SignalWeights weights)
        {
            Grid grid = new Grid(state.Grid);

            int distance = 1;
            int score = 0;
            foreach (Position aheadPosition in grid.LookAhead())
            {
                char item = grid.GetItem(aheadPosition);
                if ((item == 'W') || (item == 'O'))
                {
                    if (distance == 1)
                    {
                        score = -1000;
                    }
                    break;
                }
                else if (item == 'L')
                {
                    score = -200;
                    break;
                }
                else if (item == 'B')
                {
                    if (grid.Health < 100 || grid.Energy < 5)
                    {
                        score = 1000;
                    }
                    else
                    {
                        score = (15 - distance) * 50;
                    }

                    if (distance == 1)
                    {
                        grid.Energy += 5;
                        grid.Health += 20;
                    }

                    grid.SetItem(aheadPosition, '_');
                    break;
                }
                else if (item == '_')
                {
                    score = 15;
                }

                distance++;
            }

            grid.Position = grid.GetAhead();
            return new EvalState(GetAction(), state, score, grid);
        }
    }

    public class TankOperationNoop : TankOperation
    {
        public override Action GetAction() { return Action.Noop; }

        public override EvalState GetScore(EvalState state, SignalWeights weights)
        {
            return new EvalState(GetAction(), state, 0, state.Grid);
        }
    }
}
