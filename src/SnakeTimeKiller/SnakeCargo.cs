namespace SnakeTimeKiller
{
    public sealed class SnakeCargo
    {
        public SnakeCargo(CargoType type, GridPosition position, int value)
        {
            Type = type;
            Position = position;
            Value = value;
        }

        public CargoType Type { get; }

        public GridPosition Position { get; }

        public int Value { get; }
    }
}
