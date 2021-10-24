namespace HaloWarsTools
{
    class HWVisResource : HWXmlResource
    {
        public HWVisResource(string filename) : base(filename) { }

        public static new HWVisResource FromFile(string filename) {
            return GetOrCreateFromFile(filename) as HWVisResource;
        }
    }
}
