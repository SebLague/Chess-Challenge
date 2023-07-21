using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System;

namespace ChessChallenge.Application
{
    public static class FileHelper
    {

        public static string AppDataPath
        {
            get
            {
                string dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(dir, "ChessCodingChallenge");
            }
        }

        public static string SavedGamesPath => Path.Combine(AppDataPath, "Games");
        public static string PrefsFilePath => Path.Combine(AppDataPath, "prefs.txt");

        public static string GetUniqueFileName(string path, string fileName, string fileExtension)
        {
            if (fileExtension[0] != '.')
            {
                fileExtension = "." + fileExtension;
            }

            string uniqueName = fileName;
            int index = 0;

            while (File.Exists(Path.Combine(path, uniqueName + fileExtension)))
            {
                index++;
                uniqueName = fileName + index;
            }
            return uniqueName + fileExtension;
        }

        public static string GetResourcePath(params string[] localPath)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "resources", Path.Combine(localPath));
        }

        public static string ReadResourceFile(string localPath)
        {
            return File.ReadAllText(GetResourcePath(localPath));
        }

        // Thanks to https://github.com/dotnet/runtime/issues/17938
        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

    }
}
