using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboTank
{
    public class Grid
    {
        public const int Width = 24;
        public const int Height = 16;

        public int Health;
        public int Energy;
        public bool EnemyHit = false;
        public bool BatteryFound = false;
        public Position Position;
        public char[,] Cells = new char[Width, Height];

        public Grid()
        {
        }

        public Grid(Grid copy)
        {
            this.Position = copy.Position;
            this.Health = copy.Health - 1;
            this.Energy = copy.Energy;
            this.EnemyHit = copy.EnemyHit;
            this.BatteryFound = copy.BatteryFound;

            Array.Copy(copy.Cells, 0, Cells, 0, Cells.Length);
        }

        public override string ToString()
        {
            return Position.ToString();
        }

        public void Update(dynamic serialization)
        {
            EnemyHit = false;
            BatteryFound = false;

            Orientation orientation;
            Enum.TryParse(serialization.orientation, true, out orientation);

            Health = serialization.health;
            Energy = serialization.energy;

            string gridSerialization = serialization.grid;

            // Infer the health, battery and laser energy of our opponent based on how the board changes.
            int gridY = 0;
            foreach (string gridLine in gridSerialization.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                int gridX = 0;
                foreach (char gridChar in gridLine)
                {
                    Cells[gridX, gridY] = gridChar;

                    if (gridChar == 'X')
                    {
                        Position = new Position(gridX, gridY, orientation);
                    }

                    gridX++;
                }
                gridY++;
            }

        }

        public void MovePosition(Position newPosition)
        {
            Cells[Position.X, Position.Y] = '_';
            Cells[newPosition.X, newPosition.Y] = 'X';

            Position = newPosition;
        }

        public char GetItem(Position position)
        {
            return Cells[position.X, position.Y];
        }

        public void SetItem(Position position, char item)
        {
            Cells[position.X, position.Y] = item;
        }

        public Position GetLeft()
        {
            if (Position.Orientation == Orientation.North) return GetWestPosition(Position);
            else if (Position.Orientation == Orientation.South) return GetEastPosition(Position);
            else if (Position.Orientation == Orientation.East) return GetNorthPosition(Position);
            else if (Position.Orientation == Orientation.West) return GetSouthPosition(Position);
            else throw new Exception("Invalid orientation");
        }

        public Position GetRight()
        {
            if (Position.Orientation == Orientation.North) return GetEastPosition(Position);
            else if (Position.Orientation == Orientation.South) return GetWestPosition(Position);
            else if (Position.Orientation == Orientation.East) return GetSouthPosition(Position);
            else if (Position.Orientation == Orientation.West) return GetNorthPosition(Position);
            else throw new Exception("Invalid orientation");
        }

        public Position GetBehind(Position position)
        {
            if (position.Orientation == Orientation.North) return GetSouthPosition(Position);
            else if (position.Orientation == Orientation.South) return GetNorthPosition(Position);
            else if (position.Orientation == Orientation.East) return GetWestPosition(Position);
            else if (position.Orientation == Orientation.West) return GetEastPosition(Position);
            else throw new Exception("Invalid orientation");
        }


        public Position GetAhead()
        {
            return GetAhead(Position);
        }

        public Position GetAhead(Position position)
        {
            if (position.Orientation == Orientation.North) return GetNorthPosition(position);
            else if (position.Orientation == Orientation.South) return GetSouthPosition(position);
            else if (position.Orientation == Orientation.East) return GetEastPosition(position);
            else if (position.Orientation == Orientation.West) return GetWestPosition(position);
            else throw new Exception("Invalid orientation");
        }

        public bool ItemBehind(char item, int maxDistance)
        {
            int distance = 0;
            Position lookPosition = GetBehind(Position);
            while (distance < maxDistance)
            {
                if (GetItem(lookPosition) == item)
                {
                    return true;
                }

                lookPosition = GetAhead(lookPosition);
                distance++;
            }

            return false;
        }

        public IEnumerable<Position> LookAhead(int maxDistance)
        {
            Position lookPosition = GetAhead();
            while (maxDistance-- > 0 && (lookPosition.X != Position.X || lookPosition.Y != Position.Y))
            {
                yield return lookPosition;
                lookPosition = GetAhead(lookPosition);
            }
        }

        private static Position GetEastPosition(Position position)
        {
            int x, y;
            y = position.Y;
            if (position.X == (Width - 1))
            {
                x = 0;
            }
            else
            {
                x = position.X + 1;
            }

            return new Position(x, y, Orientation.East);
        }

        private static Position GetWestPosition(Position position)
        {
            int x, y;
            y = position.Y;
            if (position.X == 0)
            {
                x = Width - 1;
            }
            else
            {
                x = position.X - 1;
            }

            return new Position(x, y, Orientation.West);
        }

        private static Position GetSouthPosition(Position position)
        {
            int x, y;
            x = position.X;
            if (position.Y == (Height - 1))
            {
                y = 0;
            }
            else
            {
                y = position.Y + 1;
            }

            return new Position(x, y, Orientation.South);
        }

        private static Position GetNorthPosition(Position position)
        {
            int x, y;
            x = position.X;
            if (position.Y == 0)
            {
                y = Height - 1;
            }
            else
            {
                y = position.Y - 1;
            }

            return new Position(x, y, Orientation.North);
        }
    }
}
