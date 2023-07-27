using Raylib_cs;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ChessChallenge.Application
{
    static class Program
    {
        const bool hideRaylibLogs = true;
        static Camera2D cam;

        public static void Main()
        {
            Vector2 loadedWindowSize = GetSavedWindowSize();
            int screenWidth = (int)loadedWindowSize.X;
            int screenHeight = (int)loadedWindowSize.Y;

            if (hideRaylibLogs)
            {
                unsafe
                {
                    Raylib.SetTraceLogCallback(&LogCustom);
                }
            }

            Raylib.InitWindow(screenWidth, screenHeight, "Chess Coding Challenge");
            Raylib.SetTargetFPS(60);
            Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);

            UpdateCamera(screenWidth, screenHeight);

            ChallengeController controller = new();

            while (!Raylib.WindowShouldClose())
            {
                UpdateWindowSize();
                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(22, 22, 22, 255));
                Raylib.BeginMode2D(cam);

                controller.Update();
                controller.Draw();

                Raylib.EndMode2D();

                controller.DrawOverlay();

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();

            controller.Release();
            UIHelper.Release();
        }

        static void UpdateWindowSize()
        {
            if (Raylib.IsWindowResized())
            {
                int width = Raylib.GetScreenWidth();
                int height = Raylib.GetScreenHeight();
                Vector2 size = new Vector2(width, height);
                SetWindowSize(size);
            }

            // Rest of update logic
        }

        public static void SetWindowSize(Vector2 size)
        {
            Raylib.SetWindowSize((int)size.X, (int)size.Y);
            UpdateCamera((int)size.X, (int)size.Y);
            SaveWindowSize();
        }

        public static Vector2 ScreenToWorldPos(Vector2 screenPos) =>
            Raylib.GetScreenToWorld2D(screenPos, cam);

        static void UpdateCamera(int screenWidth, int screenHeight)
        {
            cam = new Camera2D();
            cam.target = new Vector2(0, 15);
            cam.offset = new Vector2(screenWidth / 2f, screenHeight / 2f);
            int[] normalizedScreenDimensions = {(int)(Raylib.GetScreenHeight() * 1.777), Raylib.GetScreenWidth()};
            cam.zoom = normalizedScreenDimensions.Min() / 1280f * 0.7f;
        }

        [UnmanagedCallersOnly(
            CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) }
        )]
        private static unsafe void LogCustom(int logLevel, sbyte* text, sbyte* args) { }

        static Vector2 GetSavedWindowSize()
        {
            if (File.Exists(FileHelper.PrefsFilePath))
            {
                string prefs = File.ReadAllText(FileHelper.PrefsFilePath);
                if (!string.IsNullOrEmpty(prefs))
                {
                    string[] parts = prefs.Split('x');
                    if (parts.Length == 2)
                    {
                        int width = int.Parse(parts[0]);
                        int height = int.Parse(parts[1]);
                        return new Vector2(width, height);
                    }
                }
                // Default sizes
                if (prefs[0] == '0')
                {
                    return Settings.ScreenSizeSmall;
                }
                else if (prefs[0] == '1')
                {
                    return Settings.ScreenSizeBig;
                }
            }
            return Settings.ScreenSizeSmall;
        }

        static void SaveWindowSize()
        {
            int width = (int)Raylib.GetScreenWidth();
            int height = (int)Raylib.GetScreenHeight();

            string prefs = $"{width}x{height}";

            File.WriteAllText(FileHelper.PrefsFilePath, prefs);
        }

      

    }


}