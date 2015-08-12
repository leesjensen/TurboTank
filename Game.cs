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
            ThreadPool.SetMinThreads(operations.Length, 4);

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

        static TankOperation[] operations = new TankOperation[] { new TankOperationLeft(), new TankOperationRight(), new TankOperationMove(), new TankOperationFire() };

        public Action GetBestAction(SignalWeights weights)
        {
            long turnStartTime = Stopwatch.GetTimestamp();

            List<Beam> beams = new List<Beam>();
            foreach (var operation in operations)
            {
                beams.Add(new Beam(operation, grid, weights));
            }

            int depth = 0;
            while (!IsTimeout(turnTimeout, turnStartTime))
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
                                    Program.Log("         ({0}) {1}", beam.StartAction, opState);
                                    beam.Add(opState);
                                }
                            }

                            beam.Evaluate();
                    });
            }

            EvalState bestState = new EvalState(Action.Noop, grid);
            foreach (var beam in beams)
            {
                EvalState operationBest = beam.GetBest();
                Program.Log("   Beam {0}: {1} depth, {2} candidates, ({3}) {4}", beam.StartAction, depth, beam.CandidateCount, operationBest.Score, operationBest);
                if (bestState.Score < operationBest.Score)
                {
                    bestState = operationBest;
                }
            }


            Program.Log("FINAL: Best: ({0}) {1}", bestState.Score, bestState);

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
