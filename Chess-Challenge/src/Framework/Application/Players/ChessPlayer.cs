using ChessChallenge.API;
using System;

namespace ChessChallenge.Application
{
    public class ChessPlayer
    {
        // public event Action<Chess.Core.Move>? MoveChosen;

        public readonly IChessBot? Bot;

        double secondsElapsed;
        int incrementAddedMs;
        int baseTimeMs;

        public ChessPlayer(object instance, int baseTimeMs = int.MaxValue)
        {
            Bot = instance as IChessBot;
            this.baseTimeMs = baseTimeMs;

        }

        public void Update()
        {
        }

        public void UpdateClock(double dt)
        {
            secondsElapsed += dt;
        }

        public void AddIncrement(int incrementMs)
        {
            incrementAddedMs += incrementMs;
        }

        public int TimeRemainingMs
        {
            get
            {
                if (baseTimeMs == int.MaxValue)
                {
                    return baseTimeMs;
                }
                return (int)Math.Ceiling(Math.Max(0, baseTimeMs - secondsElapsed * 1000.0 + incrementAddedMs));
            }
        }

        public void SubscribeToMoveChosenEventIfHuman(Action<Chess.Move> action)
        {

        }


    }
}
