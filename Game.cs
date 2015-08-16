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
    [Flags]
    public enum TankAction
    {
        Left = 1,
        Right = 2,
        Fire = 4,
        Move = 8,
        Noop = 16
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
        private int numberOfTurns = 0;

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
            ThreadPool.SetMinThreads(operations.Length, 4);

            this.client = client;
        }


        public void Run(SignalWeights weights)
        {
            dynamic joinResponse = client.Start();
            turnTimeout = (joinResponse.config.turn_timeout / 1000) - 5000;
            Grid grid = new Grid();

            Status status = ParseStatus(joinResponse, grid);

            while (status == Status.Running)
            {
                numberOfTurns++;

                TankAction action = GetBestAction(grid, weights);
                dynamic moveResponse = client.TakeAction(action);
                status = ParseStatus(moveResponse, grid);
            }
        }

        static TankOperation[] operations = new TankOperation[] { new TankOperationLeft(), new TankOperationRight(), new TankOperationMove(), new TankOperationFire() };

        public TankAction GetBestAction(Grid grid, SignalWeights weights)
        {
            long turnStartTime = Stopwatch.GetTimestamp();

            bool successfulActionFound = false;
            int depth = 1;
            List<Beam> beams = new List<Beam>();
            {
                foreach (var operation in operations)
                //TankOperation operation = operations[1];
                {
                    Beam beam = new Beam(operation, grid, weights);
                    beams.Add(beam);

                    successfulActionFound = (successfulActionFound || beam.WasSuccessful());
                }
            }

            int beamCompletedCount = 0;
            while (!successfulActionFound && (beamCompletedCount < beams.Count) && !IsTimeout(turnTimeout, turnStartTime))
            {
                depth++;
                Parallel.ForEach(beams, (beam) =>
                {
                    if (beam.StillSearching())
                    {
                        foreach (EvalState state in beam.Iterate())
                        {
                            state.Grid.Health--;
                            foreach (TankOperation operation in operations)
                            {
                                EvalState opState = operation.GetScore(state, weights);
                                beam.Add(opState);
                            }
                        }

                        beam.Evaluate();

                        beamCompletedCount += (beam.StillSearching() ? 0 : 1);

                        successfulActionFound = (successfulActionFound || beam.WasSuccessful());
                    }
                });
            }

            EvalState bestState = new EvalState(TankAction.Noop, grid);
            foreach (var beam in beams)
            {
                EvalState operationBest = beam.GetBest();
                Console.WriteLine("   Beam {0} ({3} pts) - depth: {1}, candidates: {2} - {4}", beam.StartAction, depth, beam.CandidateCount, operationBest.Score, operationBest);
                if (bestState.Score < operationBest.Score)
                {
                    bestState = operationBest;
                }
            }


            Console.WriteLine("TURN {0} - Best: ({1}) {2}", numberOfTurns, bestState.Score, bestState);

            return bestState.GetRootAction();
        }


        private static Status ParseStatus(dynamic moveResponse, Grid grid)
        {
            Status status;
            Enum.TryParse(moveResponse.status, true, out status);

            Console.WriteLine(moveResponse.grid.ToString());

            grid.Update(moveResponse);

            return status;
        }





        public static bool IsTimeout(long turnTimeout, long turnStartTime)
        {
            long ticksUsed = Stopwatch.GetTimestamp() - turnStartTime;

            return (ticksUsed > turnTimeout);
        }

    }

}
