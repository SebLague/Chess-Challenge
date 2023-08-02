using System;
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

        public static void DrawTokenUsage(int totalTokenCount, int debugTokenCount, int tokenLimit)
        {
            int activeTokenCount = totalTokenCount - debugTokenCount;

            int startPos = Raylib.GetScreenWidth() / 2;
            double t = (double)activeTokenCount / tokenLimit;
            string text = $"Bot Brain Capacity: {activeTokenCount}/{tokenLimit}";
            if (activeTokenCount > tokenLimit)
            {
                text += " [LIMIT EXCEEDED]";
            }
            else if (debugTokenCount != 0)
            {
                text += $"    ({totalTokenCount} with Debugs included)";
            }
            Draw(text, t, startPos);
        }
        
        public static void DrawMemoryUsage(long memorySize, long memoryLimit)
        {
            int startPos = 0;
            double t = (double)memorySize / memoryLimit;
            string text = $"Bot Memory Capacity: {FriendlySize(memorySize)} / {FriendlySize(memoryLimit)}";
            if (memorySize > memoryLimit)
            {
                text += " [LIMIT EXCEEDED]";
            }
            Draw(text, t, startPos);
        }
        
        static void Draw(string text, double t, int startPos)
        {
            int barWidth = Raylib.GetScreenWidth() / 2;
            int screenHeight = Raylib.GetScreenHeight();
            int barHeight = UIHelper.ScaleInt(48);
            int fontSize = UIHelper.ScaleInt(35);
            // Bg
            Raylib.DrawRectangle(startPos, screenHeight - barHeight, startPos + barWidth, barHeight, background);
            // Bar
            Color col;
            if (t <= 0.7)
                col = green;
            else if (t <= 0.85)
                col = yellow;
            else if (t <= 1)
                col = orange;
            else
                col = red;
            
            Raylib.DrawRectangle(startPos, screenHeight - barHeight, startPos + (int)(barWidth * Math.Min(t, 1)), barHeight, col);

            var textPos = new System.Numerics.Vector2(startPos + barWidth / 2, screenHeight - barHeight / 2);
            UIHelper.DrawText(text, textPos, fontSize, 1, Color.WHITE, UIHelper.AlignH.Centre);
        }
        
        static readonly string[] sizes = { "B", "KB", "MB" };
        static string FriendlySize(long size)
        {
            if (size == 0)
            {
                return "--";
            }
            double friendlySize = size;
            int order = 0;
            while (friendlySize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                friendlySize /= 1024;
            }
            return $"{friendlySize:0.##} {sizes[order]}";
        }
    }
}