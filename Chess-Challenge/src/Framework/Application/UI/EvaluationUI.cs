using System;
using Raylib_cs;

namespace ChessChallenge.Application
{
    public static class EvaluationUI
    {
        static readonly Color black = new(50,50,50, 255);
        static readonly Color white = new(255,255,255, 255);


        public static void Draw(double evaluation, int scale = 10)
        {
            int screenWidth = Raylib.GetScreenWidth();
            int screenHeight = Raylib.GetScreenHeight();
            int height = UIHelper.ScaleInt(48);
            int fontSize = UIHelper.ScaleInt(35);
            
            int middle = (int)(screenWidth/2);
            double percent = Math.Min(evaluation,scale)/scale;

            Raylib.DrawRectangle(0, screenHeight - height, screenWidth, height, white);
            Raylib.DrawRectangle(0, screenHeight - height, (int)(middle + middle*percent), height, black);

            var textPos = new System.Numerics.Vector2(screenWidth / 2, screenHeight - height / 2);

            // format text to show 1 decimal place
            string text = $"{evaluation:0.0}";
            UIHelper.DrawText(text, textPos, fontSize, 1, Color.RED, UIHelper.AlignH.Centre);
        }
    }
}
