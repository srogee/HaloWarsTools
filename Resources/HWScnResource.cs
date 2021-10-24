namespace HaloWarsTools
{
    public class HWScnResource : HWXmlResource
    {
        public HWScnResource(string filename) : base(filename) { }

        public static new HWScnResource FromFile(string filename) {
            return GetOrCreateFromFile(filename) as HWScnResource;
        }
    }
}
