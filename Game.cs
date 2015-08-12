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
                Action action = GetBestAction(grid, weights);
                dynamic moveResponse = client.TakeAction(action);
                status = ParseStatus(moveResponse, grid);
            }
        }

        static TankOperation[] operations = new TankOperation[] { new TankOperationLeft(), new TankOperationRight(), new TankOperationMove(), new TankOperationFire() };

        public Action GetBestAction(Grid grid, SignalWeights weights)
        {
            long turnStartTime = Stopwatch.GetTimestamp();

            bool enemyHit = false;
            int depth = 1;
            List<Beam> beams = new List<Beam>();
            foreach (var operation in operations)
            {
                Beam beam = new Beam(operation, grid, weights);
                beams.Add(beam);

                enemyHit = (enemyHit || beam.GetBest().Grid.EnemyHit);
            }

            while (!enemyHit && !IsTimeout(turnTimeout, turnStartTime))
            {
                depth++;
                Parallel.ForEach(beams, (beam) =>
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

                    enemyHit = (enemyHit || beam.GetBest().Grid.EnemyHit);
                });
            }

            EvalState bestState = new EvalState(Action.Noop, grid);
            foreach (var beam in beams)
            {
                EvalState operationBest = beam.GetBest();
                Console.WriteLine("   Beam {0} - hit: {1}, depth: {2}, candidates: {3}, ({4}) {5}", beam.StartAction, operationBest.Grid.EnemyHit, depth, beam.CandidateCount, operationBest.Score, operationBest);
                if (bestState.Score < operationBest.Score)
                {
                    bestState = operationBest;
                }
            }


            Console.WriteLine("FINAL - hit: {0} Best: ({1}) {2}", enemyHit, bestState.Score, bestState);

            return bestState.GetRootAction();
        }


        private static Status ParseStatus(dynamic moveResponse, Grid grid)
        {
            Status status;
            Enum.TryParse(moveResponse.status, true, out status);

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
