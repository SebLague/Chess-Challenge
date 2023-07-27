using Raylib_cs;
using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace ChessChallenge.Application
{
    public static class UIHelper
    {
        static readonly bool SDF_Enabled = true;
        const string fontName = "OPENSANS-SEMIBOLD.TTF";
        const int referenceResolution = 1920;

        static Font font;
        static Font fontSdf;
        static Shader shader;

        public enum AlignH
        {
            Left,
            Centre,
            Right
        }
        public enum AlignV
        {
            Top,
            Centre,
            Bottom
        }

        static UIHelper()
        {
            if (SDF_Enabled)
            {
                unsafe
                {
                    const int baseSize = 64;
                    uint fileSize = 0;
                    var fileData = Raylib.LoadFileData(GetResourcePath("Fonts", fontName), ref fileSize);
                    Font fontSdf = default;
                    fontSdf.baseSize = baseSize;
                    fontSdf.glyphCount = 95;
                    fontSdf.glyphs = Raylib.LoadFontData(fileData, (int)fileSize, baseSize, null, 0, FontType.FONT_SDF);

                    Image atlas = Raylib.GenImageFontAtlas(fontSdf.glyphs, &fontSdf.recs, 95, baseSize, 0, 1);
                    fontSdf.texture = Raylib.LoadTextureFromImage(atlas);
                    Raylib.UnloadImage(atlas);
                    Raylib.UnloadFileData(fileData);

                    Raylib.SetTextureFilter(fontSdf.texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
                    UIHelper.fontSdf = fontSdf;

                }
                shader = Raylib.LoadShader("", GetResourcePath("Fonts", "sdf.fs"));
            }
            font = Raylib.LoadFontEx(GetResourcePath("Fonts", fontName), 128, null, 0);

        }

        public static void DrawText(string text, Vector2 pos, int size, int spacing, Color col, AlignH alignH = AlignH.Left, AlignV alignV = AlignV.Centre)
        {
            Vector2 boundSize = Raylib.MeasureTextEx(font, text, size, spacing);
            float offsetX = alignH == AlignH.Left ? 0 : (alignH == AlignH.Centre ? -boundSize.X / 2 : -boundSize.X);
            float offsetY = alignV == AlignV.Top ? 0 : (alignV == AlignV.Centre ? -boundSize.Y / 2 : -boundSize.Y);
            Vector2 offset = new(offsetX, offsetY);

            if (SDF_Enabled)
            {
                Raylib.BeginShaderMode(shader);
                Raylib.DrawTextEx(fontSdf, text, pos + offset, size, spacing, col);
                Raylib.EndShaderMode();
            }
            else
            {
                Raylib.DrawTextEx(font, text, pos + offset, size, spacing, col);
            }
        }

        public static bool Button(string text, Vector2 centre, Vector2 size)
        {
            Rectangle rec = new(centre.X - size.X / 2, centre.Y - size.Y / 2, size.X, size.Y);

            Color normalCol = new(40, 40, 40, 255);
            Color hoverCol = new(3, 173, 252, 255);
            Color pressCol = new(2, 119, 173, 255);

            bool mouseOver = MouseInRect(rec);
            bool pressed = mouseOver && Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT);
            bool pressedThisFrame = pressed && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT);
            Color col = mouseOver ? (pressed ? pressCol : hoverCol) : normalCol;

            Raylib.DrawRectangleRec(rec, col);
            Color textCol = mouseOver ? Color.WHITE : new Color(180, 180, 180, 255);
            int fontSize = ScaleInt(32);

            DrawText(text, centre, fontSize, 1, textCol, AlignH.Centre);

            return pressedThisFrame;
        }

        public static T? Dropdown<T>(IEnumerable<T> values, T selected, Vector2 centre, Vector2 size, ref bool persist)
        {
            Rectangle rec = new(centre.X - size.X / 2, centre.Y - size.Y / 2, size.X, size.Y);

            Color normalCol = new(40, 40, 40, 255);
            Color hoverCol = new(3, 173, 252, 255);
            Color pressCol = new(2, 119, 173, 255);

            var height = size.Y * (values.Count());
            Rectangle optionsRec = new(centre.X + size.X / 2, centre.Y - size.Y / 2, size.X, height);

            bool mouseOver = !MouseInRect(rec) ? (persist ? MouseInRect(optionsRec) : false) : true;
            bool pressed = mouseOver && Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT);
            bool pressedThisFrame = pressed && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT);
            int fontSize = ScaleInt(32);

            var col = mouseOver ? hoverCol : normalCol;
            Color textCol = mouseOver ? Color.WHITE : new Color(180, 180, 180, 255);
            Raylib.DrawRectangleRec(rec, col);

            DrawText(selected.ToString()!, centre, fontSize, 1, textCol, AlignH.Centre);

            if (mouseOver)
            {
                Raylib.DrawRectangleRec(optionsRec, normalCol);

                if (MouseInRect(optionsRec))
                {
                    var selectionIndex = (int)Math.Floor((Raylib.GetMouseY() - optionsRec.y) / size.Y);
                    selectionIndex = Math.Max(0, selectionIndex);
                    selectionIndex = Math.Min(selectionIndex, values.Count() - 1);

                    col = pressed ? pressCol : hoverCol;
                    Rectangle selectionRec = new(optionsRec.x, optionsRec.y + selectionIndex * size.Y, size.X, size.Y);
                    Raylib.DrawRectangleRec(selectionRec, col);

                    if (pressedThisFrame)
                    {
                        persist = false;
                        return values.ElementAt(selectionIndex);
                    }
                }

                var offset = 0;
                foreach (var option in values)
                {
                    var hOffset = offset++ * size.Y;
                    DrawText(option.ToString()!, centre + new Vector2(size.X, hOffset), fontSize, 1, textCol, AlignH.Centre);
                }
            }

            persist = mouseOver;
            return default(T);
        }

        static bool MouseInRect(Rectangle rec)
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            return mousePos.X >= rec.x && mousePos.Y >= rec.y && mousePos.X <= rec.x + rec.width && mousePos.Y <= rec.y + rec.height;
        }

        public static string GetResourcePath(params string[] localPath)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "resources", Path.Combine(localPath));
        }

        public static float Scale(float val, int referenceResolution = referenceResolution)
        {
            return Raylib.GetScreenWidth() / (float)referenceResolution * val;
        }

        public static int ScaleInt(int val, int referenceResolution = referenceResolution)
        {
            return (int)Math.Round(Raylib.GetScreenWidth() / (float)referenceResolution * val);
        }

        public static Vector2 Scale(Vector2 vec, int referenceResolution = referenceResolution)
        {
            float x = Scale(vec.X, referenceResolution);
            float y = Scale(vec.Y, referenceResolution);
            return new Vector2(x, y);
        }

        public static void Release()
        {
            Raylib.UnloadFont(font);
            if (SDF_Enabled)
            {
                Raylib.UnloadFont(fontSdf);
                Raylib.UnloadShader(shader);
            }
        }
    }
}
