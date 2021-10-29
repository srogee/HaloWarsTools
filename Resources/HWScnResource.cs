namespace HaloWarsTools
{
    public class HWScnResource : HWXmlResource
    {
        public static new HWScnResource FromFile(HWContext context, string filename) {
            return GetOrCreateFromFile(context, filename, HWResourceType.Scn) as HWScnResource;
        }
    }
}
