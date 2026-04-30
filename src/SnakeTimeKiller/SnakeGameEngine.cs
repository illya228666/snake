using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SnakeTimeKiller
{
    public sealed class SnakeGameEngine
    {
        private const int MinimumRows = 6;
        private const int MinimumColumns = 6;

        private readonly Random _random;
        private readonly List<GridPosition> _snake;
        private readonly ReadOnlyCollection<GridPosition> _snakeView;
        private Direction _currentDirection;
        private Direction _pendingDirection;

        public SnakeGameEngine(
            int rows,
            int columns,
            int lowCargoScore = 10,
            int mediumCargoScore = 25,
            int highCargoScore = 50,
            Random random = null)
        {
            if (rows < MinimumRows)
            {
                throw new ArgumentOutOfRangeException(nameof(rows), "Rows must be at least 6.");
            }

            if (columns < MinimumColumns)
            {
                throw new ArgumentOutOfRangeException(nameof(columns), "Columns must be at least 6.");
            }

            Rows = rows;
            Columns = columns;
            LowCargoScore = Math.Max(0, lowCargoScore);
            MediumCargoScore = Math.Max(0, mediumCargoScore);
            HighCargoScore = Math.Max(0, highCargoScore);
            _random = random ?? new Random();
            _snake = new List<GridPosition>();
            _snakeView = _snake.AsReadOnly();

            Reset();
        }

        public event EventHandler<SnakeScoreChangedEventArgs> ScoreChanged;

        public event EventHandler<SnakeCargoCollectedEventArgs> CargoCollected;

        public event EventHandler<SnakeGameOverEventArgs> GameOver;

        public int Rows { get; }

        public int Columns { get; }

        public int LowCargoScore { get; }

        public int MediumCargoScore { get; }

        public int HighCargoScore { get; }

        public int Score { get; private set; }

        public bool IsGameOver { get; private set; }

        public SnakeGameOverReason? GameOverReason { get; private set; }

        public Direction CurrentDirection => _currentDirection;

        public IReadOnlyList<GridPosition> Snake => _snakeView;

        public SnakeCargo Cargo { get; private set; }

        public void Reset()
        {
            _snake.Clear();

            var row = Rows / 2;
            var headColumn = Columns / 2;
            var startLength = Math.Min(4, headColumn + 1);

            for (var i = 0; i < startLength; i++)
            {
                _snake.Add(new GridPosition(row, headColumn - i));
            }

            _currentDirection = Direction.Right;
            _pendingDirection = Direction.Right;
            Score = 0;
            IsGameOver = false;
            GameOverReason = null;
            SpawnCargo();
        }

        public void Turn(Direction direction)
        {
            if (IsOpposite(_currentDirection, direction))
            {
                return;
            }

            _pendingDirection = direction;
        }

        public SnakeStepResult Step()
        {
            if (IsGameOver)
            {
                return new SnakeStepResult(false, null, 0, true, GameOverReason);
            }

            _currentDirection = _pendingDirection;
            var nextHead = GetNextHead();

            if (IsOutOfBounds(nextHead))
            {
                FinishGame(SnakeGameOverReason.HitWall);
                return new SnakeStepResult(false, null, 0, true, SnakeGameOverReason.HitWall);
            }

            var willCollect = Cargo != null && Cargo.Position.Equals(nextHead);
            if (HitsSelf(nextHead, willCollect))
            {
                FinishGame(SnakeGameOverReason.HitSelf);
                return new SnakeStepResult(false, null, 0, true, SnakeGameOverReason.HitSelf);
            }

            _snake.Insert(0, nextHead);

            SnakeCargo collectedCargo = null;
            var scoreDelta = 0;

            if (willCollect)
            {
                collectedCargo = Cargo;
                scoreDelta = collectedCargo.Value;
                AddScore(scoreDelta);
                OnCargoCollected(collectedCargo);
                SpawnCargo();
            }
            else
            {
                _snake.RemoveAt(_snake.Count - 1);
            }

            return new SnakeStepResult(
                willCollect,
                collectedCargo,
                scoreDelta,
                IsGameOver,
                GameOverReason);
        }

        public void PlaceCargo(SnakeCargo cargo)
        {
            if (cargo == null)
            {
                Cargo = null;
                return;
            }

            if (IsOutOfBounds(cargo.Position))
            {
                throw new ArgumentOutOfRangeException(nameof(cargo), "Cargo position is outside of the board.");
            }

            if (ContainsSnake(cargo.Position))
            {
                throw new ArgumentException("Cargo cannot be placed on the snake.", nameof(cargo));
            }

            Cargo = cargo;
        }

        private GridPosition GetNextHead()
        {
            var head = _snake[0];

            switch (_currentDirection)
            {
                case Direction.Up:
                    return new GridPosition(head.Row - 1, head.Column);
                case Direction.Right:
                    return new GridPosition(head.Row, head.Column + 1);
                case Direction.Down:
                    return new GridPosition(head.Row + 1, head.Column);
                case Direction.Left:
                    return new GridPosition(head.Row, head.Column - 1);
                default:
                    return head;
            }
        }

        private bool HitsSelf(GridPosition position, bool willCollect)
        {
            var checkedSegments = willCollect ? _snake.Count : _snake.Count - 1;

            for (var i = 0; i < checkedSegments; i++)
            {
                if (_snake[i].Equals(position))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainsSnake(GridPosition position)
        {
            for (var i = 0; i < _snake.Count; i++)
            {
                if (_snake[i].Equals(position))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsOutOfBounds(GridPosition position)
        {
            return position.Row < 0 ||
                   position.Column < 0 ||
                   position.Row >= Rows ||
                   position.Column >= Columns;
        }

        private void SpawnCargo()
        {
            var occupied = new HashSet<GridPosition>(_snake);
            var freeCount = (Rows * Columns) - occupied.Count;

            if (freeCount <= 0)
            {
                Cargo = null;
                FinishGame(SnakeGameOverReason.BoardFilled);
                return;
            }

            var targetIndex = _random.Next(freeCount);

            for (var row = 0; row < Rows; row++)
            {
                for (var column = 0; column < Columns; column++)
                {
                    var position = new GridPosition(row, column);
                    if (occupied.Contains(position))
                    {
                        continue;
                    }

                    if (targetIndex == 0)
                    {
                        var type = PickCargoType();
                        Cargo = new SnakeCargo(type, position, GetCargoScore(type));
                        return;
                    }

                    targetIndex--;
                }
            }
        }

        private CargoType PickCargoType()
        {
            var roll = _random.Next(100);

            if (roll < 55)
            {
                return CargoType.Low;
            }

            if (roll < 85)
            {
                return CargoType.Medium;
            }

            return CargoType.High;
        }

        private int GetCargoScore(CargoType type)
        {
            switch (type)
            {
                case CargoType.Low:
                    return LowCargoScore;
                case CargoType.Medium:
                    return MediumCargoScore;
                case CargoType.High:
                    return HighCargoScore;
                default:
                    return LowCargoScore;
            }
        }

        private static bool IsOpposite(Direction current, Direction requested)
        {
            return (current == Direction.Up && requested == Direction.Down) ||
                   (current == Direction.Down && requested == Direction.Up) ||
                   (current == Direction.Left && requested == Direction.Right) ||
                   (current == Direction.Right && requested == Direction.Left);
        }

        private void AddScore(int delta)
        {
            if (delta == 0)
            {
                return;
            }

            var oldScore = Score;
            Score += delta;
            ScoreChanged?.Invoke(this, new SnakeScoreChangedEventArgs(oldScore, Score, delta));
        }

        private void OnCargoCollected(SnakeCargo cargo)
        {
            CargoCollected?.Invoke(this, new SnakeCargoCollectedEventArgs(cargo, Score));
        }

        private void FinishGame(SnakeGameOverReason reason)
        {
            if (IsGameOver)
            {
                return;
            }

            IsGameOver = true;
            GameOverReason = reason;
            GameOver?.Invoke(this, new SnakeGameOverEventArgs(reason, Score));
        }
    }
}
