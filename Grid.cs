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

        public Position MyPosition;
        public char[,] Cells = new char[Width, Height];
        public Orientation MyOrientation;

        private long turnTimeout;
        private long turnStartTime;

        public Grid(long turnTimeout)
        {
            this.turnTimeout = turnTimeout * 1000;
        }

        public void Update(string gridSerialization)
        {
            turnStartTime = Stopwatch.GetTimestamp();

            // Infer the health, battery and laser energy of our opponent based on how the board changes.
            int gridY = 0;
            foreach (string gridLine in gridSerialization.Split('\n'))
            {
                int gridX = 0;
                foreach (char gridChar in gridLine)
                {
                    Cells[gridX, gridY] = gridChar;

                    if (gridChar == 'X')
                    {
                        MyPosition = new Position(gridX, gridY);
                    }

                    gridX++;
                }
                gridY++;
            }

        }

        public bool IsTimeout()
        {
            long ticksUsed = Stopwatch.GetTimestamp() - turnStartTime;

            return (ticksUsed > turnTimeout);
        }

        public char GetItem(Position position)
        {
            return Cells[position.X, position.Y];
        }

        public Position GetLeft(Orientation orientation, Position position)
        {
            int x = 0, y = 0;
            if (orientation == Orientation.North) GetWestPosition(position, ref x, ref y);
            else if (orientation == Orientation.South) GetEastPosition(position, ref x, ref y);
            else if (orientation == Orientation.East) GetNorthPosition(position, ref x, ref y);
            else if (orientation == Orientation.West) GetSouthPosition(position, ref x, ref y);

            return new Position(x, y);
        }

        public Position GetRight(Orientation orientation, Position position)
        {
            int x = 0, y = 0;
            if (orientation == Orientation.North) GetEastPosition(position, ref x, ref y);
            else if (orientation == Orientation.South) GetWestPosition(position, ref x, ref y);
            else if (orientation == Orientation.East) GetSouthPosition(position, ref x, ref y);
            else if (orientation == Orientation.West) GetNorthPosition(position, ref x, ref y);

            return new Position(x, y);
        }


        public Position GetAhead(Orientation orientation, Position position)
        {
            int x = 0, y = 0;
            if (orientation == Orientation.North) GetNorthPosition(position, ref x, ref y);
            else if (orientation == Orientation.South) GetSouthPosition(position, ref x, ref y);
            else if (orientation == Orientation.East) GetEastPosition(position, ref x, ref y);
            else if (orientation == Orientation.West) GetWestPosition(position, ref x, ref y);

            return new Position(x, y);
        }

        private static void GetEastPosition(Position position, ref int x, ref int y)
        {
            y = position.Y;
            if (position.X == (Width - 1))
            {
                x = 0;
            }
            else
            {
                x = position.X + 1;
            }
        }

        private static void GetWestPosition(Position position, ref int x, ref int y)
        {
            y = position.Y;
            if (position.X == 0)
            {
                x = Width - 1;
            }
            else
            {
                x = position.X - 1;
            }
        }

        private static void GetSouthPosition(Position position, ref int x, ref int y)
        {
            x = position.X;
            if (position.Y == (Height - 1))
            {
                y = 0;
            }
            else
            {
                y = position.Y + 1;
            }
        }

        private static void GetNorthPosition(Position position, ref int x, ref int y)
        {
            x = position.X;
            if (position.Y == 0)
            {
                y = Height - 1;
            }
            else
            {
                y = position.Y - 1;
            }
        }
    }
}
