namespace HaloWarsTools
{
    class HWVisResource : HWXmlResource
    {
        public HWVisResource(string filename) : base(filename) {
            Type = HWResourceType.Vis;
        }
    }
}
