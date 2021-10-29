using System;
using System.Drawing.Imaging;
using System.IO;

namespace HaloWarsTools
{
    class Program
    {
        static void Main(string[] args) {
            // TODO
            // Make sure all meshes (terrain + ugx) use the same coordinate system
            string inputDirectory = "C:\\Users\\rid3r\\Desktop\\PhoenixTools\\extract";
            string outputDirectory = "C:\\Users\\rid3r\\Desktop\\HaloWarsTools";

            var context = new HWContext(inputDirectory);

            var xtt = HWXttResource.FromFile(context, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.xtt");
            xtt.AlbedoTexture.Save(Path.Combine(outputDirectory, "blood_gulch_albedo.png"), ImageFormat.Png);
            Console.WriteLine($"Processed {xtt}");

            var xtd = HWXtdResource.FromFile(context, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.xtd");
            xtd.AmbientOcclusionTexture.Save(Path.Combine(outputDirectory, "blood_gulch_ao.png"), ImageFormat.Png);
            xtd.OpacityTexture.Save(Path.Combine(outputDirectory, "blood_gulch_opacity.png"), ImageFormat.Png);
            xtd.Mesh.Export(Path.Combine(outputDirectory, "blood_gulch_vismesh.obj"), GenericMeshExportFormat.Obj);
            Console.WriteLine($"Processed {xtd}");

            var ugx = HWUgxResource.FromFile(context, "art\\covenant\\building\\builder_01\\builder_hologram_01.ugx");
            ugx.Mesh.Export(Path.Combine(outputDirectory, "builder_hologram_01.obj"), GenericMeshExportFormat.Obj);
            Console.WriteLine($"Processed {ugx}");

            var gls = HWGlsResource.FromFile(context, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.gls");
            Console.WriteLine($"Processed {gls}");

            var scn = HWScnResource.FromFile(context, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.scn");
            Console.WriteLine($"Processed {scn}");

            var sc2 = HWSc2Resource.FromFile(context, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.sc2");
            Console.WriteLine($"Processed {sc2}");

            var sc3 = HWSc3Resource.FromFile(context, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.sc3");
            Console.WriteLine($"Processed {sc3}");

            var vis = HWVisResource.FromFile(context, "art\\covenant\\building\\builder_03\\builder_03.vis");
            Console.WriteLine($"Processed {vis}");
        }
    }
}
