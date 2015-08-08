using Agilix.Shared;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public int X;
        public int Y;

        public Position(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }



    public class Game
    {
        public string GameId;
        public string PlayerId;

        private HttpClient client;

        private long turnTimeout;

        private Status status;
        private int health;
        private int energy;
        private Grid grid;


        public override string ToString()
        {
            return DynObject.FromPairs("gameId", GameId, "playerId", PlayerId, "status", status).ToString();
        }

        //

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


        public Game(string server, int port, string gameId)
        {
            this.GameId = gameId;
            this.client = new HttpClient(server, port);

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("X-Sm-Playermoniker", "Lee");

            dynamic joinResponse = client.GetJsonResponse("/game/" + gameId + ":screen/join", "POST", "", headers);
            turnTimeout = joinResponse.config.turn_timeout;
            grid = new Grid(turnTimeout);

            ParseStatus(joinResponse);

            PlayerId = headers["X-Sm-Playerid"];
        }

        private bool IsRunning()
        {
            return (status == Status.Running);
        }

        private void TakeAction(Action move)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("X-Sm-Playerid", PlayerId);

            dynamic moveResponse = client.GetJsonResponse("/game/" + GameId + ":screen/" + move.ToString().ToLower(), "POST", "", headers);

            ParseStatus(moveResponse);
        }

        public void Run(SignalWeights weights)
        {
            while (IsRunning())
            {
                TakeAction(TankOperation.ParallelGetNextAction(grid, weights));
            }
        }

        public class EvalState
        {
            public Position Position;
            public Orientation Orientation;
            public int Score;

            public EvalState(Position position, Orientation orientation)
            {
                this.Position = position;
                this.Orientation = orientation;
            }
        }



        public abstract class TankOperation
        {
            public abstract Action GetAction();
            public abstract int GetScore(Position position, Orientation orientation, Grid grid, SignalWeights weights);

            static TankOperation[] operations = new TankOperation[] { new TankOperationNoop(), new TankOperationLeft(), new TankOperationRight(), new TankOperationMove(), new TankOperationFire() };

            public static Action ParallelGetNextAction(Grid grid, SignalWeights weights)
            {
                int bestScore = 0;
                TankOperation bestOperation = operations[0];
                //Parallel.ForEach(operations, (operation) =>
                foreach (TankOperation operation in operations)
                {
                    int score = operation.GetScore(grid.MyPosition, grid.MyOrientation, grid, weights);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestOperation = operation;
                    }
                }
//                );

                return bestOperation.GetAction();
            }
        }


        public class TankOperationLeft : TankOperation
        {
            public override Action GetAction() { return Action.Left; }

            public override int GetScore(Position position, Orientation orientation, Grid grid, SignalWeights weights)
            {
                int score = 0;
                Position newPosition = grid.GetLeft(orientation, position);
                char item = grid.GetItem(newPosition);
                if (item == '_')
                {
                    score += 10;
                }

                return score;
            }
        }

        public class TankOperationRight : TankOperation
        {
            public override Action GetAction() { return Action.Right; }

            public override int GetScore(Position position, Orientation orientation, Grid grid, SignalWeights weights)
            {
                int score = 0;
                Position newPosition = grid.GetRight(orientation, position);
                char item = grid.GetItem(newPosition);
                if (item == '_')
                {
                    score += 10;
                }

                return score;
            }
        }

        public class TankOperationFire : TankOperation
        {
            public override Action GetAction() { return Action.Fire; }

            public override int GetScore(Position position, Orientation orientation, Grid grid, SignalWeights weights)
            {
                int score = 0;
                Position newPosition = position;
                char item = '_';
                while (item == '_')
                {
                    newPosition = grid.GetAhead(orientation, newPosition);
                    item = grid.GetItem(newPosition);
                    if (item == 'O')
                    {
                        score += 200;
                    }
                    else if (item == 'X')
                    {
                        score -= 1000;
                    }
                }

                return score;
            }
        }

        public class TankOperationMove : TankOperation
        {
            public override Action GetAction() { return Action.Move; }

            public override int GetScore(Position position, Orientation orientation, Grid grid, SignalWeights weights)
            {
                int score = 0;
                Position newPosition = position;
                char item = '_';
                while (item == '_')
                {
                    newPosition = grid.GetAhead(orientation, newPosition);
                    item = grid.GetItem(newPosition);
                    if (item == 'B')
                    {
                        score += 200;
                    }
                    else if (item == 'L') // And we actually have laser energy.
                    {
                        score -= 100;
                    }
                    else if (item == 'W')
                    {
                        score -= 10;
                    }
                }

                return score;
            }
        }

        public class TankOperationNoop : TankOperation
        {
            public override Action GetAction() { return Action.Noop; }

            public override int GetScore(Position position, Orientation orientation, Grid grid, SignalWeights weights)
            {
                return 0;
            }
        }

        private void ParseStatus(dynamic moveResponse)
        {
            Enum.TryParse(moveResponse.status, true, out status);
            Enum.TryParse(moveResponse.orientation, true, out grid.MyOrientation);
            health = moveResponse.health;
            energy = moveResponse.energy;
            grid.Update(moveResponse.grid);
        }
    }

}
