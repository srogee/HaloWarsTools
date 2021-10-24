using System.Numerics;
using System.Xml.Linq;

namespace HaloWarsTools
{
    public class HWXmlResource : HWResource
    {
        protected HWXmlResource(string filename) : base(filename) { }

        protected XElement XmlData => ValueCache.Get(() => XElement.Load(Filename));

        protected static Vector3 DeserializeVector3(string value) {
            string[] components = value.Split(",");
            return new Vector3(float.Parse(components[0]), float.Parse(components[1]), float.Parse(components[2]));
        }
    }
}
