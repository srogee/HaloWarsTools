namespace HaloWarsTools
{
    public class HWSc2Resource : HWScnResource
    {
        public HWSc2Resource(string filename) : base(filename) { }

        public static new HWSc2Resource FromFile(string filename) {
            return GetOrCreateFromFile(filename) as HWSc2Resource;
        }
    }
}
