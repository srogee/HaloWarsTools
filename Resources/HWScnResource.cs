namespace HaloWarsTools
{
    public class HWScnResource : HWXmlResource
    {
        public HWScnResource(string filename) : base(filename) {
            Type = HWResourceType.Scn;
        }
    }
}
