using Raylib_cs;

namespace ChessChallenge.Application
{
    public static class BotBrainCapacityUI
    {
        static readonly Color green = new(17, 212, 73, 255);
        static readonly Color yellow = new(219, 161, 24, 255);
        static readonly Color orange = new(219, 96, 24, 255);
        static readonly Color red = new(219, 9, 9, 255);
        static readonly Color background = new Color(40, 40, 40, 255);

        public static void Draw(int totalTokenCount, int debugTokenCount, int tokenLimit)
        {
            int activeTokenCount = totalTokenCount - debugTokenCount;

            int screenWidth = Raylib.GetScreenWidth();
            int screenHeight = Raylib.GetScreenHeight();
            int height = UIHelper.ScaleInt(48);
            int fontSize = UIHelper.ScaleInt(35);
            // Bg
            Raylib.DrawRectangle(0, screenHeight - height, screenWidth, height, background);
            // Bar
            double t = (double)activeTokenCount / tokenLimit;

            Color col;
            if (t <= 0.7)
                col = green;
            else if (t <= 0.85)
                col = yellow;
            else if (t <= 1)
                col = orange;
            else
                col = red;
            Raylib.DrawRectangle(0, screenHeight - height, (int)(screenWidth * t), height, col);

            var textPos = new System.Numerics.Vector2(screenWidth / 2, screenHeight - height / 2);
            string text = $"Bot Brain Capacity: {activeTokenCount}/{tokenLimit}";
            if (activeTokenCount > tokenLimit)
            {
                text += " [LIMIT EXCEEDED]";
            }
            else if (debugTokenCount != 0)
            {
                text += $"    ({totalTokenCount} with Debugs included)";
            }
            UIHelper.DrawText(text, textPos, fontSize, 1, Color.WHITE, UIHelper.AlignH.Centre);
        }
    }
}