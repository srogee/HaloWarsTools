namespace HaloWarsTools
{
    class HWVisResource : HWXmlResource
    {
        public static new HWVisResource FromFile(HWContext context, string filename) {
            return GetOrCreateFromFile(context, filename) as HWVisResource;
        }
    }
}
