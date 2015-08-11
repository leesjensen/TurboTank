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
            turnTimeout = (joinResponse.config.turn_timeout / 1000) - 5000;
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

//            TankOperation startingOperation = new TankOperationMove();
            Parallel.ForEach(operations, (startingOperation) =>
            {
                int depth = 0;
                Beam beam = new Beam(startingOperation, grid, weights);


                while (!IsTimeout(turnTimeout, turnStartTime))
                {
                    foreach (EvalState state in beam.Iterate())
                    {
                        state.Grid.Health--;
                        depth++;
                        foreach (TankOperation operation in operations)
                        {
                            Interlocked.Increment(ref evalCount);
                            beam.Add(operation.GetScore(state, weights));
                        }
                    }

                    beam.Evaluate();
                }

                lock (bestStateLock)
                {
                    EvalState operationBest = beam.GetBest();
                    Program.Log("   {0} depth, Best: ({1}) {2}", evalCount, operationBest.Score, operationBest);
                    if (bestState.Score < operationBest.Score)
                    {
                        bestState = operationBest;
                    }
                }
            });

            Program.Log("FINAL: {0} evaluated, Best: ({1}) {2}", evalCount, bestState.Score, bestState);

            return bestState.GetRootAction();
        }

        private void ParseStatus(dynamic moveResponse)
        {
            Enum.TryParse(moveResponse.status, true, out status);

            grid.Update(moveResponse);
        }





        public static bool IsTimeout(long turnTimeout, long turnStartTime)
        {
            long ticksUsed = Stopwatch.GetTimestamp() - turnStartTime;

            return (ticksUsed > turnTimeout);
        }

    }

}
