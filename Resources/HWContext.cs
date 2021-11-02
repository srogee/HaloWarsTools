using System.IO;

namespace HaloWarsTools
{
    public class HWContext
    {
        public string GameInstallDirectory;
        public string ScratchDirectory;

        public HWContext(string gameInstallDirectory, string scratchDirectory) {
            GameInstallDirectory = gameInstallDirectory;
            ScratchDirectory = scratchDirectory;
        }

        public string GetAbsoluteGamePath(string relativePath) {
            return Path.Combine(GameInstallDirectory, relativePath);
        }

        public string GetRelativeGamePath(string absolutePath) {
            return Path.GetRelativePath(GameInstallDirectory, absolutePath);
        }

        public string GetAbsoluteScratchPath(string relativePath) {
            return Path.Combine(ScratchDirectory, relativePath);
        }

        public string GetRelativeScratchPath(string absolutePath) {
            return Path.GetRelativePath(ScratchDirectory, absolutePath);
        }
    }
}
