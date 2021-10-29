using System;
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
        Vis  // Visual Representation
    }

    public class HWResourceTypeDefinition
    {
        public HWResourceType Type;
        public Type Class;

        public HWResourceTypeDefinition(HWResourceType type, Type resourceClass) {
            Type = type;
            Class = resourceClass;
        }
    }
 
    public class HWResource {
        private static LazyValueCache ResourceCache = new LazyValueCache();
        private static Dictionary<string, HWResourceTypeDefinition> TypeDefinitions = new Dictionary<string, HWResourceTypeDefinition>() {
            { ".xtt", new HWResourceTypeDefinition(HWResourceType.Xtt, typeof(HWXttResource)) },
            { ".xtd", new HWResourceTypeDefinition(HWResourceType.Xtd, typeof(HWXtdResource)) },
            { ".scn", new HWResourceTypeDefinition(HWResourceType.Scn, typeof(HWScnResource)) },
            { ".sc2", new HWResourceTypeDefinition(HWResourceType.Sc2, typeof(HWSc2Resource)) },
            { ".sc3", new HWResourceTypeDefinition(HWResourceType.Sc3, typeof(HWSc3Resource)) },
            { ".gls", new HWResourceTypeDefinition(HWResourceType.Gls, typeof(HWGlsResource)) },
            { ".ugx", new HWResourceTypeDefinition(HWResourceType.Ugx, typeof(HWUgxResource)) },
            { ".vis", new HWResourceTypeDefinition(HWResourceType.Vis, typeof(HWVisResource)) },
        };

        protected LazyValueCache ValueCache;
        protected HWContext Context;
        protected string RelativePath;

        public string AbsolutePath => Context.GetAbsolutePath(RelativePath);
        public HWResourceType Type = HWResourceType.None;

        public override string ToString() {
            return $"{Type.ToString().ToUpperInvariant()} {Path.ChangeExtension(RelativePath, null)}";
        }

        protected HWResource() {
            ValueCache = new LazyValueCache();
        }

        public static HWResource FromFile(HWContext context, string filename) {
            return GetOrCreateFromFile(context, filename);
        }

        protected static HWResource GetOrCreateFromFile(HWContext context, string filename) {
            return ResourceCache.Get(() => CreateResource(context, filename), filename);
        }

        private static HWResource CreateResource(HWContext context, string filename) {
            string extension = Path.GetExtension(filename).ToLowerInvariant();
            
            if (TypeDefinitions.TryGetValue(extension, out HWResourceTypeDefinition definition)) {
                if (Activator.CreateInstance(definition.Class) is HWResource resource) {
                    resource.Type = definition.Type;
                    resource.Context = context;
                    resource.RelativePath = filename;
                    return resource;
                }
            }

            return null;
        }
    }
}
