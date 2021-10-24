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

            var xtt = HWXttResource.FromFile(Path.Combine(inputDirectory, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.xtt"));
            xtt.AlbedoTexture.Save(Path.Combine(outputDirectory, "blood_gulch_albedo.png"), ImageFormat.Png);

            var xtd = HWXtdResource.FromFile(Path.Combine(inputDirectory, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.xtd"));
            xtd.AmbientOcclusionTexture.Save(Path.Combine(outputDirectory, "blood_gulch_ao.png"), ImageFormat.Png);
            xtd.OpacityTexture.Save(Path.Combine(outputDirectory, "blood_gulch_opacity.png"), ImageFormat.Png);
            xtd.Mesh.Export(Path.Combine(outputDirectory, "blood_gulch_vismesh.obj"), GenericMeshExportFormat.Obj);

            var ugx = HWUgxResource.FromFile(Path.Combine(inputDirectory, "art\\covenant\\building\\builder_01\\builder_hologram_01.ugx"));
            ugx.Mesh.Export(Path.Combine(outputDirectory, "builder_hologram_01.obj"), GenericMeshExportFormat.Obj);

            var gls = HWGlsResource.FromFile(Path.Combine(inputDirectory, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.gls"));
            Console.WriteLine(gls.SunColor);

            var scn = HWScnResource.FromFile(Path.Combine(inputDirectory, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.scn"));
            var sc2 = HWSc2Resource.FromFile(Path.Combine(inputDirectory, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.sc2"));
            var sc3 = HWSc3Resource.FromFile(Path.Combine(inputDirectory, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.sc3"));
            var vis = HWVisResource.FromFile(Path.Combine(inputDirectory, "art\\covenant\\building\\builder_03\\builder_03.vis"));
        }
    }
}
