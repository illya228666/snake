using System;
using SnakeTimeKiller;

namespace SnakeTimeKiller.Tests
{
    internal static class Program
    {
        private static int _passed;
        private static int _failed;

        private static int Main()
        {
            Run("moves one cell in the current direction", MovesOneCellRight);
            Run("blocks 180 degree turns", BlocksReverseTurn);
            Run("grows and scores low cargo", () => CollectsCargo(CargoType.Low, 10));
            Run("grows and scores medium cargo", () => CollectsCargo(CargoType.Medium, 25));
            Run("grows and scores high cargo", () => CollectsCargo(CargoType.High, 50));
            Run("ends on wall collision", EndsOnWallCollision);
            Run("ends on self collision", EndsOnSelfCollision);
            Run("spawns cargo outside of the snake", SpawnsCargoOutsideSnake);

            Console.WriteLine();
            Console.WriteLine("Passed: " + _passed);
            Console.WriteLine("Failed: " + _failed);

            return _failed == 0 ? 0 : 1;
        }

        private static void MovesOneCellRight()
        {
            var engine = CreateEngine();
            engine.PlaceCargo(new SnakeCargo(CargoType.Low, new GridPosition(0, 0), 10));

            var head = engine.Snake[0];
            engine.Step();

            AssertEqual(head.Row, engine.Snake[0].Row, "Head row should not change.");
            AssertEqual(head.Column + 1, engine.Snake[0].Column, "Head column should advance by one.");
        }

        private static void BlocksReverseTurn()
        {
            var engine = CreateEngine();
            engine.PlaceCargo(new SnakeCargo(CargoType.Low, new GridPosition(0, 0), 10));

            var head = engine.Snake[0];
            engine.Turn(Direction.Left);
            engine.Step();

            AssertEqual(Direction.Right, engine.CurrentDirection, "Reverse turn should be ignored.");
            AssertEqual(head.Column + 1, engine.Snake[0].Column, "Snake should keep moving right.");
        }

        private static void CollectsCargo(CargoType type, int expectedScore)
        {
            var engine = CreateEngine();
            var oldLength = engine.Snake.Count;
            var head = engine.Snake[0];
            var cargoPosition = new GridPosition(head.Row, head.Column + 1);

            engine.PlaceCargo(new SnakeCargo(type, cargoPosition, expectedScore));
            var result = engine.Step();

            AssertTrue(result.CargoCollected, "Cargo should be collected.");
            AssertEqual(expectedScore, result.ScoreDelta, "Step result should contain the cargo score.");
            AssertEqual(expectedScore, engine.Score, "Engine score should increase by cargo value.");
            AssertEqual(oldLength + 1, engine.Snake.Count, "Snake should grow by one segment.");
        }

        private static void EndsOnWallCollision()
        {
            var engine = new SnakeGameEngine(6, 6, random: new Random(3));
            engine.PlaceCargo(new SnakeCargo(CargoType.Low, new GridPosition(0, 0), 10));

            for (var i = 0; i < 10 && !engine.IsGameOver; i++)
            {
                engine.Step();
            }

            AssertTrue(engine.IsGameOver, "Game should end after crossing the board edge.");
            AssertEqual(SnakeGameOverReason.HitWall, engine.GameOverReason.Value, "Game over reason should be wall hit.");
        }

        private static void EndsOnSelfCollision()
        {
            var engine = new SnakeGameEngine(10, 10, random: new Random(4));
            var head = engine.Snake[0];

            engine.PlaceCargo(new SnakeCargo(CargoType.High, new GridPosition(head.Row, head.Column + 1), 50));
            engine.Step();
            engine.PlaceCargo(new SnakeCargo(CargoType.Low, new GridPosition(0, 0), 10));

            engine.Turn(Direction.Up);
            engine.Step();
            engine.Turn(Direction.Left);
            engine.Step();
            engine.Turn(Direction.Down);
            engine.Step();

            AssertTrue(engine.IsGameOver, "Game should end when the head enters the body.");
            AssertEqual(SnakeGameOverReason.HitSelf, engine.GameOverReason.Value, "Game over reason should be self hit.");
        }

        private static void SpawnsCargoOutsideSnake()
        {
            var engine = CreateEngine();

            AssertTrue(engine.Cargo != null, "Cargo should exist after reset.");

            for (var i = 0; i < engine.Snake.Count; i++)
            {
                AssertTrue(
                    !engine.Snake[i].Equals(engine.Cargo.Position),
                    "Cargo should not be placed on a snake segment.");
            }
        }

        private static SnakeGameEngine CreateEngine()
        {
            return new SnakeGameEngine(16, 24, random: new Random(7));
        }

        private static void Run(string name, Action test)
        {
            try
            {
                test();
                _passed++;
                Console.WriteLine("[PASS] " + name);
            }
            catch (Exception ex)
            {
                _failed++;
                Console.Error.WriteLine("[FAIL] " + name);
                Console.Error.WriteLine(ex.Message);
            }
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void AssertEqual<T>(T expected, T actual, string message)
        {
            if (!Equals(expected, actual))
            {
                throw new InvalidOperationException(message + " Expected: " + expected + ". Actual: " + actual + ".");
            }
        }
    }
}
