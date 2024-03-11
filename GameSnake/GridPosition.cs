using System;
using System.Collections.Generic;
using System.Text;

namespace GameSnake
{
    public class GridPosition
    {
        public int Row { get; }
        public int Col { get; }

        public GridPosition(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public GridPosition NewPosition(Direction direction)
        {
            return new GridPosition(Row + direction.VerMove, Col + direction.HorMove);
        }

        public override bool Equals(object obj)
        {
            return obj is GridPosition position &&
                   Row == position.Row &&
                   Col == position.Col;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Col);
        }

        public static bool operator ==(GridPosition left, GridPosition right)
        {
            return EqualityComparer<GridPosition>.Default.Equals(left, right);
        }

        public static bool operator !=(GridPosition left, GridPosition right)
        {
            return !(left == right);
        }
    }
}
