using System;
using System.Drawing.Imaging;
using System.IO;

namespace HaloWarsTools
{
    class Program
    {
        static void Main(string[] args) {
            string inputDirectory = "C:\\Users\\rid3r\\Desktop\\PhoenixTools\\extract";
            string outputDirectory = "C:\\Users\\rid3r\\Desktop\\HaloWarsTools";

            var xtt = HWResource.GetOrCreateResource(Path.Combine(inputDirectory, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.xtt")) as HWXttResource;
            var xtd = HWResource.GetOrCreateResource(Path.Combine(inputDirectory, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.xtd")) as HWXtdResource;
            var gls = HWResource.GetOrCreateResource(Path.Combine(inputDirectory, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.gls")) as HWGlsResource;

            xtt.AlbedoTexture.Save(Path.Combine(outputDirectory, "albedo.png"), ImageFormat.Png);
            xtd.AmbientOcclusionTexture.Save(Path.Combine(outputDirectory, "ao.png"), ImageFormat.Png);
            xtd.OpacityTexture.Save(Path.Combine(outputDirectory, "opacity.png"), ImageFormat.Png);

            Console.WriteLine(gls.SunColor);
        }
    }
}
