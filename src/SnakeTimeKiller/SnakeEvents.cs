using System;

namespace SnakeTimeKiller
{
    public sealed class SnakeScoreChangedEventArgs : EventArgs
    {
        public SnakeScoreChangedEventArgs(int oldScore, int newScore, int delta)
        {
            OldScore = oldScore;
            NewScore = newScore;
            Delta = delta;
        }

        public int OldScore { get; }

        public int NewScore { get; }

        public int Delta { get; }
    }

    public sealed class SnakeCargoCollectedEventArgs : EventArgs
    {
        public SnakeCargoCollectedEventArgs(SnakeCargo cargo, int score)
        {
            Cargo = cargo;
            Score = score;
        }

        public SnakeCargo Cargo { get; }

        public int Score { get; }
    }

    public sealed class SnakeGameOverEventArgs : EventArgs
    {
        public SnakeGameOverEventArgs(SnakeGameOverReason reason, int score)
        {
            Reason = reason;
            Score = score;
        }

        public SnakeGameOverReason Reason { get; }

        public int Score { get; }
    }
}
