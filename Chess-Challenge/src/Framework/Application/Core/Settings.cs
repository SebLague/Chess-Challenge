using System.Numerics;

namespace ChessChallenge.Application
{
    public static class Settings
    {
        public const int GameDurationMilliseconds = 60 * 1000;
        public const int MaxTokenCount = 1024;
        public static readonly bool RunBotsOnSeparateThread = true;

        public const LogType MessagesToLog = LogType.All;

        public static readonly Vector2 ScreenSizeSmall = new(1280, 720);
        public static readonly Vector2 ScreenSizeBig = new(1920, 1080);

        public enum LogType
        {
            None,
            ErrorOnly,
            All
        }
    }
}
