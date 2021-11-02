using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloWarsTools.Helpers
{
    class PhoenixUtils
    {
        public static bool UnpackEra(string eraPath, string outputPath) {
            if (IsEraUnpacked(eraPath, outputPath)) {
                return false;
            }

            var expander = new KSoft.Phoenix.Resource.EraFileExpander(eraPath);

            expander.Options.Set(KSoft.Phoenix.Resource.EraFileUtilOptions.x64);
            expander.Options.Set(KSoft.Phoenix.Resource.EraFileUtilOptions.SkipVerification);

            expander.ExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.Decrypt);
            expander.ExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.DontOverwriteExistingFiles);
            expander.ExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.DecompressUIFiles);
            expander.ExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions.TranslateGfxFiles);

            if (!expander.Read()) {
                return false;
            }

            if (!expander.ExpandTo(outputPath, Path.GetFileNameWithoutExtension(eraPath))) {
                return false;
            }

            return true;
        }

        public static bool IsEraUnpacked(string eraPath, string outputPath) {
            return File.Exists(Path.Combine(outputPath, Path.ChangeExtension(Path.GetFileName(eraPath), ".eradef")));
        }

        public static void ExpandAllEraFilesInDirectory(string directory, string outputPath) {
            var files = Directory.GetFiles(directory, "*.era");
            foreach (var eraFile in files) {
                UnpackEra(eraFile, outputPath);
            }
        }
    }
}
