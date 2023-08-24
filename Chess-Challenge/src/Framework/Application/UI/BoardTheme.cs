using Raylib_cs;

namespace ChessChallenge.Application
{
    public class BoardTheme
    {
        public Color LightCol = new Color(238, 238, 210, 255);
        public Color DarkCol = new Color(118, 150, 86, 255);

        public Color selectedLight = new Color(236, 197, 123, 255);
        public Color selectedDark = new Color(200, 158, 80, 255);

        public Color MoveFromLight = new Color(247, 247, 105, 255);
        public Color MoveFromDark = new Color(187, 203, 43, 255);

        public Color MoveToLight = new Color(247, 247, 105, 255);
        public Color MoveToDark = new Color(187, 203, 43, 255);

        public Color LegalLight = new Color(238, 139, 127, 255);
        public Color LegalDark = new Color(181, 97, 67, 255);

        public Color CheckLight = new Color(238, 238, 210, 255);
        public Color CheckDark = new Color(118, 150, 86, 255);

        public Color BorderCol = new Color(44, 44, 44, 255);

        public Color LightCoordCol = new Color(238, 238, 210, 255);
        public Color DarkCoordCol = new Color(118, 150, 86, 255);
    }
}
