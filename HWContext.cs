using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
