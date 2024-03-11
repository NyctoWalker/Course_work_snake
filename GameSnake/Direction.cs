using System;
using System.Collections.Generic;
using System.Text;

namespace GameSnake
{
    public class Direction
    {
        public readonly static Direction Null = new Direction(0, 0);
        public readonly static Direction Right = new Direction(0, 1);
        public readonly static Direction Up = new Direction(-1, 0);
        public readonly static Direction Left = new Direction(0, -1);
        public readonly static Direction Down = new Direction(1, 0);

        public int HorMove { get; }
        public int VerMove { get; }

        public Direction(int verMove, int horMove)
        {
            HorMove = horMove;
            VerMove = verMove;
        }

        public override bool Equals(object obj)
        {
            return obj is Direction direction &&
                   HorMove == direction.HorMove &&
                   VerMove == direction.VerMove;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(HorMove, VerMove);
        }

        public static bool operator ==(Direction left, Direction right)
        {
            return EqualityComparer<Direction>.Default.Equals(left, right);
        }

        public static bool operator !=(Direction left, Direction right)
        {
            return !(left == right);
        }
    }
}
