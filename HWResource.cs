using System.Collections.Generic;
using System.IO;

namespace HaloWarsTools
{
    public enum HWResourceType
    {
        None,
        Xtt, // Terrain Mesh/Albedo
        Xtd, // Terrain Opacity/AO
        Scn, // Scenario
        Sc2, // Scenario
        Sc3, // Scenario
        Gls, // Scenario Lighting
        Ugx, // Mesh
    }

    public class HWResource {
        private static Dictionary<string, HWResource> ResourcesByFilename = new Dictionary<string, HWResource>();
        private static List<HWResourceAuditEntry> AuditLog = new List<HWResourceAuditEntry>();

        protected LazyValueCache ValueCache;

        private static void AuditResource(HWResource resource, HWResourceAuditEntryType type) {
            AuditLog.Add(new HWResourceAuditEntry(resource, type));
        }

        public string Filename;
        public HWResourceType Type = HWResourceType.None;
        public bool IsLoaded = false;

        public string UserFriendlyName
        {
            get
            {
                return Filename;
            }
        }

        protected HWResource(string filename) {
            Filename = filename;
            ValueCache = new LazyValueCache();
        }

        public static HWResource GetOrCreateResource(string filename) {
            if (!ResourcesByFilename.ContainsKey(filename)) {
                ResourcesByFilename.Add(filename, CreateResource(filename));
                AuditResource(ResourcesByFilename[filename], HWResourceAuditEntryType.Created);
            } else {
                AuditResource(ResourcesByFilename[filename], HWResourceAuditEntryType.Accessed);
            }

            return ResourcesByFilename[filename];
        }

        private static HWResource CreateResource(string filename) {
            string extension = Path.GetExtension(filename).ToLowerInvariant();
            return extension switch {
                ".xtt" => new HWXttResource(filename),
                ".xtd" => new HWXtdResource(filename),
                ".scn" => new HWScnResource(filename),
                ".sc2" => new HWSc2Resource(filename),
                ".sc3" => new HWSc3Resource(filename),
                ".gls" => new HWGlsResource(filename),
                ".ugx" => new HWUgxResource(filename),
                _ => null
            };
        }
    }
}
