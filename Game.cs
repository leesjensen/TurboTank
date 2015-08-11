using Agilix.Shared;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TurboTank
{

    public enum Action
    {
        Left,
        Right,
        Fire,
        Move,
        Noop
    }

    public enum Orientation
    {
        North,
        East,
        South,
        West
    }

    public enum Status
    {
        Running,
        Won,
        Lost,
        Draw
    }

    public class Position
    {
        public Orientation Orientation;
        public int X;
        public int Y;

        public Position(int x, int y, Orientation orientation)
        {
            this.Orientation = orientation;
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return string.Format("{0} {1},{2}", Orientation, X, Y);
        }
    }



    public class Game
    {
        private TankClient client;
        private long turnTimeout;

        private Status status;
        private Grid grid;


        public class GameConstants
        {
            //max_energy;
            //laser_distance
            //health_loss
            //battery_power
            //battery_health
            //laser_energy
            //connect_back_timeout
            //max_health
            //laser_damage
            //turn_timeout
        }



        public Game(TankClient client)
        {
            this.client = client;
            dynamic joinResponse = client.Start();
            turnTimeout = (joinResponse.config.turn_timeout / 1000) - 3000;
            grid = new Grid();

            ParseStatus(joinResponse);
        }

        private bool IsRunning()
        {
            return (status == Status.Running);
        }

        private void TakeAction(Action move)
        {
            dynamic moveResponse = client.TakeAction(move);
            ParseStatus(moveResponse);
        }

        public void Run(SignalWeights weights)
        {
            while (IsRunning())
            {
                TakeAction(GetBestAction(weights));
            }
        }


        static TankOperation[] operations = new TankOperation[] { new TankOperationLeft(), new TankOperationRight(), new TankOperationMove(), new TankOperationFire(), new TankOperationNoop() };

        public Action GetBestAction(SignalWeights weights)
        {
            long turnStartTime = Stopwatch.GetTimestamp();

            int evalCount = 0;
            object bestStateLock = new object();
            EvalState bestState = new EvalState(Action.Noop, grid);
            Parallel.ForEach(operations, (startingOperation) =>
            {
                int depth = 0;
                Beam beam = new Beam(startingOperation, grid, weights);
                foreach (EvalState state in beam.Iterate())
                {
                    depth++;
                    foreach (TankOperation operation in operations)
                    {
                        Interlocked.Increment(ref evalCount);
                        beam.Add(operation.GetScore(state, weights));
                    }

                    beam.Evaluate();

                    if (IsTimeout(turnTimeout, turnStartTime))
                    {
                        break;
                    }
                }

                lock (bestStateLock)
                {
                    Program.Log("   {0} depth, Best: {1}", evalCount, beam.GetBest());
                    if (bestState.Score < beam.GetBest().Score)
                    {
                        bestState = beam.GetBest();
                    }
                }
            });

            Program.Log("FINAL: {0} evaluated, Best: {1}", evalCount, bestState);

            return bestState.StartAction;
        }

        private void ParseStatus(dynamic moveResponse)
        {
            Enum.TryParse(moveResponse.status, true, out status);

            grid.Update(moveResponse);
        }

        public class EvalState
        {
            public Action Action;
            public Action StartAction;
            public int Score;
            public Grid Grid;

            public bool Evaluated = false;

            public EvalState(Action action, Grid grid)
            {
                this.Action = action;
                this.StartAction = action;
                this.Grid = grid;
            }

            public EvalState(Action action, EvalState copy, int score, Position position)
                : this(copy.StartAction, new Grid(copy.Grid, position))
            {
                this.Action = action;
                this.Score = copy.Score + score;
            }

            public override string ToString()
            {
                return string.Format("{0} {1} ({2}) {3}", Score, Action, StartAction, Grid);
            }
        }


        public class Beam
        {
            private const int MaxSize = 10;
            EvalState[] bestStates = new EvalState[MaxSize];
            List<EvalState> candidates = new List<EvalState>();

            public Beam(TankOperation operation, Grid grid, SignalWeights weights)
            {
                EvalState startingState = new EvalState(operation.GetAction(), grid);
                bestStates[0] = operation.GetScore(startingState, weights);
            }

            public void Add(EvalState candidateState)
            {
                candidates.Add(candidateState);
            }

            public void Evaluate()
            {
                foreach (var candidateState in candidates)
                {
                    if (bestStates[MaxSize - 1] == null || candidateState.Score > bestStates[MaxSize - 1].Score)
                    {
                        for (int pos = 0; pos < MaxSize; pos++)
                        {
                            if (bestStates[pos] == null)
                            {
                                bestStates[pos] = candidateState;
                                break;
                            }
                            else if (candidateState.Score > bestStates[pos].Score)
                            {
                                for (int movePos = MaxSize - 1; movePos > pos; movePos--)
                                {
                                    bestStates[movePos] = bestStates[movePos - 1];
                                }

                                bestStates[pos] = candidateState;
                                break;
                            }
                        }
                    }
                }

                candidates.Clear();
            }

            public IEnumerable<EvalState> Iterate()
            {
                for (int pos = 0; pos < MaxSize; pos++)
                {
                    if (!bestStates[pos].Evaluated)
                    {
                        bestStates[pos].Evaluated = true;

                        yield return bestStates[pos];
                    }
                }
            }

            public EvalState GetBest()
            {
                return bestStates[0];
            }

            public override string ToString()
            {
                return bestStates[0].ToString();
            }
        }


        public abstract class TankOperation
        {
            public abstract Action GetAction();
            public abstract EvalState GetScore(EvalState state, SignalWeights weights);
        }


        public static bool IsTimeout(long turnTimeout, long turnStartTime)
        {
            long ticksUsed = Stopwatch.GetTimestamp() - turnStartTime;

            return (ticksUsed > turnTimeout);
        }


        public class TankOperationLeft : TankOperation
        {
            public override Action GetAction() { return Action.Left; }

            public override EvalState GetScore(EvalState state, SignalWeights weights)
            {
                int score = 0;
                Position newPosition = state.Grid.GetLeft();
                char item = state.Grid.GetItem(newPosition);
                if (item == '_')
                {
                    score += 10;
                }

                return new EvalState(GetAction(), state, score, newPosition);
            }
        }

        public class TankOperationRight : TankOperation
        {
            public override Action GetAction() { return Action.Right; }

            public override EvalState GetScore(EvalState state, SignalWeights weights)
            {
                int score = 0;
                Position newPosition = state.Grid.GetRight();
                char item = state.Grid.GetItem(newPosition);
                if (item == '_')
                {
                    score += 10;
                }

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
                        if (item == 'O')
                        {
                            score = (10 - distance) * 100;
                            break;
                        }

                        distance++;
                    }
                }

                return new EvalState(GetAction(), state, score, state.Grid.Position);
            }
        }

        public class TankOperationMove : TankOperation
        {
            public override Action GetAction() { return Action.Move; }

            public override EvalState GetScore(EvalState state, SignalWeights weights)
            {
                int distance = 1;
                int score = 0;
                foreach (Position aheadPosition in state.Grid.LookAhead())
                {
                    char item = state.Grid.GetItem(aheadPosition);
                    if (item == 'B')
                    {
                        score = (15 - distance) * 50;
                        if (state.Grid.Health < 100)
                        {
                            score += 1000;
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
                        score = -10;
                        break;
                    }
                }

                return new EvalState(GetAction(), state, score, state.Grid.Position);
            }
        }

        public class TankOperationNoop : TankOperation
        {
            public override Action GetAction() { return Action.Noop; }

            public override EvalState GetScore(EvalState state, SignalWeights weights)
            {
                return state;
            }
        }
    }

}
