using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeGame
{
    public class Moving
    {
        public int GridWidth { get; }
        public int GridHeight { get; }

        public readonly static Moving Right = new Moving(1, 0);
        public readonly static Moving Up = new Moving(0, -1);
        public readonly static Moving Left = new Moving(-1, 0);
        public readonly static Moving Down = new Moving(0, 1);

        private Moving(int gridWidth, int gridHeght)
        {
            GridWidth = gridWidth;
            GridHeight = gridHeght;
        }
    }
}
