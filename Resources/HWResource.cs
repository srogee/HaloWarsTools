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

    public class HWResource {
        private static LazyValueCache ResourceCache = new LazyValueCache();
        protected LazyValueCache ValueCache;

        public string Filename;
        public HWResourceType Type = HWResourceType.None;

        protected HWResource(string filename) {
            if (ResourceCache.Contains(filename)) {
                // Prevent consumers from just calling new HW***Resource(), which has to be public so we can call it in CreateResource
                throw new Exception("Resources must be instantiated via a FromFile call");
            }

            Filename = filename;
            ValueCache = new LazyValueCache();
        }

        public static HWResource FromFile(string filename) {
            return GetOrCreateFromFile(filename);
        }

        protected static HWResource GetOrCreateFromFile(string filename) {
            return ResourceCache.Get(() => CreateResource(filename), filename);
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
                ".vis" => new HWVisResource(filename),
                _ => null
            };
        }
    }
}
