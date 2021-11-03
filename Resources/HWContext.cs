using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace HaloWarsTools
{
    public class HWContext
    {
        public string GameInstallDirectory;
        public string ScratchDirectory;

        private LazyValueCache ValueCache;

        public Dictionary<string, HWObjectDefinition> ObjectDefinitions => ValueCache.Get(LoadObjectDefinitions);
        public LazyValueCache ResourceCache;

        private Dictionary<string, HWObjectDefinition> LoadObjectDefinitions() {
            var manifest = new Dictionary<string, HWObjectDefinition>();

            var source = "data\\objects.xml";

            var objects = XElement.Load(GetAbsoluteScratchPath(source)).Descendants("Object");
            foreach (var obj in objects) {
                if (obj.Attribute("name") != null) {
                    var def = new HWObjectDefinition {
                        Name = obj.Attribute("name").Value
                    };
                    var vis = obj.Descendants().FirstOrDefault(xmlElement => xmlElement.Name == "Visual");
                    if (vis != null) {
                        def.Visual = HWVisResource.FromFile(this, Path.Combine("art", vis.Value));
                    }
                    manifest.Add(def.Name, def);
                }
            }

            return manifest;
        }

        public HWContext(string gameInstallDirectory, string scratchDirectory) {
            GameInstallDirectory = gameInstallDirectory;
            ScratchDirectory = scratchDirectory;
            ValueCache = new LazyValueCache();
            ResourceCache = new LazyValueCache();
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
