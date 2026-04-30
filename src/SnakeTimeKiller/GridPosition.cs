using System;

namespace SnakeTimeKiller
{
    public struct GridPosition : IEquatable<GridPosition>
    {
        public GridPosition(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public int Row { get; }

        public int Column { get; }

        public bool Equals(GridPosition other)
        {
            return Row == other.Row && Column == other.Column;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPosition && Equals((GridPosition)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Row * 397) ^ Column;
            }
        }

        public override string ToString()
        {
            return Row + "," + Column;
        }
    }
}
