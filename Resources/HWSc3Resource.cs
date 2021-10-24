namespace HaloWarsTools
{
    public class HWSc3Resource : HWScnResource
    {
        public HWSc3Resource(string filename) : base(filename) { }

        public static new HWSc3Resource FromFile(string filename) {
            return GetOrCreateFromFile(filename) as HWSc3Resource;
        }
    }
}
