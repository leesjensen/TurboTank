using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboTank
{
    public abstract class TankOperation
    {
        public abstract TankAction GetAction();
        public abstract EvalState GetScore(EvalState state, SignalWeights weights);

        protected static double CalculateBatteryNeedMultiplier(Grid grid)
        {
            double needMultiplier = 1;
            if (grid.Health < 30 || grid.Energy < 2)
            {
                needMultiplier = 2;
            }
            else if (grid.Health > 280 && grid.Energy > 9)
            {
                needMultiplier = .8;
            }
            return needMultiplier;
        }
    }


    public class TankOperationLeft : TankOperation
    {
        public override TankAction GetAction() { return TankAction.Left; }

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
        public override TankAction GetAction() { return TankAction.Right; }

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
        public override TankAction GetAction() { return TankAction.Fire; }

        public override EvalState GetScore(EvalState state, SignalWeights weights)
        {
            Grid grid = new Grid(state.Grid);

            int score = -1000;

            if (grid.Energy > 0 && !grid.ItemBehind('L', 3) && !grid.ItemBehind('O', 8))
            {
                int maxDistance = 10;
                int distance = 1;
                foreach (Position aheadPosition in grid.LookAhead(maxDistance))
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
                        score = (maxDistance - distance) * 100;

                        grid.SetItem(aheadPosition, '_');
                        grid.EnemyHit = true;
                        break;
                    }
                    else if (item == 'B')
                    {
                        // TODO: if opponent is going to get the battery first then shoot it!
                        // TODO: One each evaluation move the lasers so they are in the right place.
                        // TODO: I should also influance going left or right based upon where the batteries and opponents are.
                        // TODO: Opponent behind what do we do? Shoot or turn around?
                        if (CalculateBatteryNeedMultiplier(grid) < 1)
                        {
                            score = (maxDistance - distance) * 10;
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
        public override TankAction GetAction() { return TankAction.Move; }

        public override EvalState GetScore(EvalState state, SignalWeights weights)
        {
            Grid grid = new Grid(state.Grid);

            int score = 0;

            if (grid.ItemBehind('L', 3) || grid.ItemBehind('O', 3))
            {
                score = -1000;
            }
            else
            {
                int maxDistance = 15;
                int distance = 1;
                foreach (Position aheadPosition in grid.LookAhead(maxDistance))
                {
                    char item = grid.GetItem(aheadPosition);
                    if (item == 'W')
                    {
                        if (distance == 1)
                        {
                            score = -1000;
                        }
                        break;
                    }
                    if (item == 'O')
                    {
                        if (grid.Energy == 0 && distance < 5)
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
                        int distanceMultiplier = (maxDistance - distance);
                        score = (int)(((double)45) * distanceMultiplier * CalculateBatteryNeedMultiplier(grid));

                        if (distance == 1)
                        {
                            grid.Energy += 5;
                            grid.Health += 20;
                            grid.BatteryFound = true;
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
            }

            grid.Position = grid.GetAhead();
            return new EvalState(GetAction(), state, score, grid);
        }
    }

    public class TankOperationNoop : TankOperation
    {
        public override TankAction GetAction() { return TankAction.Noop; }

        public override EvalState GetScore(EvalState state, SignalWeights weights)
        {
            return new EvalState(GetAction(), state, 0, state.Grid);
        }
    }
}
