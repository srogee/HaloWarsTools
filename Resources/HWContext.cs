using System.IO;

namespace HaloWarsTools
{
    public class HWContext
    {
        public string Directory;
        public HWContext(string directory) {
            Directory = directory;
        }

        public string GetAbsolutePath(string relativePath) {
            return Path.Combine(Directory, relativePath);
        }

        public string GetRelativePath(string absolutePath) {
            return Path.GetRelativePath(Directory, absolutePath);
        }
    }
}
