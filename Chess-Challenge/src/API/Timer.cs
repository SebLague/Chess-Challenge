using System;

namespace ChessChallenge.API
{
    public sealed class Timer
    {
        /// <summary>
        /// Amount of time left on clock for current player (in milliseconds)
        /// </summary>
        public int MillisecondsRemaining => Math.Max(0, initialMillisRemaining - (int)sw.ElapsedMilliseconds);
        /// <summary>
        /// Amount of time elapsed since current player started thinking (in milliseconds)
        /// </summary>
        public int MillisecondsElapsedThisTurn => (int)sw.ElapsedMilliseconds;

        System.Diagnostics.Stopwatch sw;
        readonly int initialMillisRemaining;

        public Timer(int millisRemaining)
        {
            initialMillisRemaining = millisRemaining;
            sw = System.Diagnostics.Stopwatch.StartNew();

        }
    }
}