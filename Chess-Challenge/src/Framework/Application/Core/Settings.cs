using System.Numerics;

namespace ChessChallenge.Application
{
    public static class Settings
    {
        public const string Version = "1.16";

        // Game settings
        public const int GameDurationMilliseconds = 60 * 1000;
        public const float MinMoveDelay = 0;
        public static bool RunBotsOnSeparateThread = true; // IF NOT IN FAST FORWARD, TURN THIS ON - It's no longer readonly

        // Display settings
        public const bool DisplayBoardCoordinates = true;
        public static readonly Vector2 ScreenSizeSmall = new(1280, 720);
        public static readonly Vector2 ScreenSizeBig = new(1920, 1080);
        public static readonly Vector2 ScreenSizeXS = new (400, 200);

        // Other settings
        public const int MaxTokenCount = 1024;
        public const LogType MessagesToLog = LogType.All;

        public enum LogType
        {
            None,
            ErrorOnly,
            All
        }
    }
}
