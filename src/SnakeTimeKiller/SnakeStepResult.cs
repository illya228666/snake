namespace SnakeTimeKiller
{
    public sealed class SnakeStepResult
    {
        public SnakeStepResult(
            bool cargoCollected,
            SnakeCargo collectedCargo,
            int scoreDelta,
            bool gameOver,
            SnakeGameOverReason? gameOverReason)
        {
            CargoCollected = cargoCollected;
            CollectedCargo = collectedCargo;
            ScoreDelta = scoreDelta;
            GameOver = gameOver;
            GameOverReason = gameOverReason;
        }

        public bool CargoCollected { get; }

        public SnakeCargo CollectedCargo { get; }

        public int ScoreDelta { get; }

        public bool GameOver { get; }

        public SnakeGameOverReason? GameOverReason { get; }
    }
}
