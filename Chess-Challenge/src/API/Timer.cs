using System;

namespace ChessChallenge.API
{
    public sealed class Timer
    {
        /// <summary>
        /// The amount of time (in milliseconds) that each player started the game with
        /// </summary>
        public readonly int GameStartTimeMilliseconds;

        /// <summary>
        /// Amount of time elapsed since the current player started thinking (in milliseconds)
        /// </summary>
        public int MillisecondsElapsedThisTurn => (int)sw.ElapsedMilliseconds;

        /// <summary>
        /// Amount of time left on the clock for the current player (in milliseconds)
        /// </summary>
        public int MillisecondsRemaining => Math.Max(0, millisRemainingAtStartOfTurn - MillisecondsElapsedThisTurn);

        /// <summary>
        /// Amount of time left on the clock for the other player (in milliseconds)
        /// </summary>
        public readonly int OpponentMillisecondsRemaining;
        
        readonly System.Diagnostics.Stopwatch sw;
        readonly int millisRemainingAtStartOfTurn;

        public Timer(int millisRemaining)
        {
            millisRemainingAtStartOfTurn = millisRemaining;
            sw = System.Diagnostics.Stopwatch.StartNew();
        }

        public Timer(int millisRemaining, int opponentMillisRemaining, int startingTimeMillis)
        {
            millisRemainingAtStartOfTurn = millisRemaining;
            sw = System.Diagnostics.Stopwatch.StartNew();
            GameStartTimeMilliseconds = startingTimeMillis;
            OpponentMillisecondsRemaining = opponentMillisRemaining;
        }

        public override string ToString()
        {
            return $"Game start time: {GameStartTimeMilliseconds} ms. Turn elapsed time: {MillisecondsElapsedThisTurn} ms. My time remaining: {MillisecondsRemaining} Opponent time remaining: {OpponentMillisecondsRemaining} ms.";
        }
    }
}