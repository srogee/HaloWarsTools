using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HaloWarsTools
{
    public class HWVisResource : HWXmlResource
    {
        public HWModel[] Models => ValueCache.Get(ImportModels);

        public static new HWVisResource FromFile(HWContext context, string filename) {
            return GetOrCreateFromFile(context, filename, HWResourceType.Vis) as HWVisResource;
        }

        private HWModel[] ImportModels() {
            var models = new List<HWModel>();

            foreach (var model in XmlData.Descendants("model")) {
                var components = model.Descendants("component");
                foreach (var component in components) {
                    var assets = component.Descendants("asset");
                    foreach (var asset in assets) {
                        var file = Path.Combine("art", asset.Descendants("file").First().Value);
                        var resource = HWUgxResource.FromFile(Context, file);
                        if (resource != null) {
                            models.Add(new HWModel(model.Attribute("name").Value, resource));
                        }
                    }
                }
            }

            return models.ToArray();
        }
    }
}
