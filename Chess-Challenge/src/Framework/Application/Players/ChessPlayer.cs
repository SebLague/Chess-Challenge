using ChessChallenge.API;
using System;

namespace ChessChallenge.Application
{
    public class ChessPlayer
    {
        // public event Action<Chess.Core.Move>? MoveChosen;

        public readonly ChallengeController.PlayerType PlayerType;
        public readonly IChessBot? Bot;
        public readonly HumanPlayer? Human;

        double secondsElapsed;
        int baseTimeMS;

        public ChessPlayer(object instance, ChallengeController.PlayerType type, int baseTimeMS = int.MaxValue)
        {
            this.PlayerType = type;
            Bot = instance as IChessBot;
            Human = instance as HumanPlayer;
            this.baseTimeMS = baseTimeMS;

        }

        public bool IsHuman => Human != null;
        public bool IsBot => Bot != null;

        public void Update()
        {
            if (Human != null)
            {
                Human.Update();
            }
        }

        public void UpdateClock(double dt)
        {
            secondsElapsed += dt;
        }

        public int TimeRemainingMs
        {
            get
            {
                if (baseTimeMS == int.MaxValue)
                {
                    return baseTimeMS;
                }
                return (int)Math.Ceiling(Math.Max(0, baseTimeMS - secondsElapsed * 1000.0));
            }
        }

        public void SubscribeToMoveChosenEventIfHuman(Action<ChessChallenge.Chess.Move> action)
        {
            if (Human != null)
            {
                Human.MoveChosen += action;
            }
        }


    }
}
