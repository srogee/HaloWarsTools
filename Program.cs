using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using HaloWarsTools.Helpers;

namespace HaloWarsTools
{
    class Program
    {
        static void Main(string[] args) {
            string gameDirectory = "Z:\\SteamLibrary\\steamapps\\common\\HaloWarsDE";
            string scratchDirectory = "C:\\Users\\rid3r\\Documents\\HaloWarsTools\\scratch";
            string outputDirectory = "C:\\Users\\rid3r\\Documents\\HaloWarsTools\\output";

            var context = new HWContext(gameDirectory, scratchDirectory);
            PhoenixUtils.ExpandAllEraFilesInDirectory(context.GameInstallDirectory, context.ScratchDirectory);

            var xtt = HWXttResource.FromFile(context, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.xtt");
            xtt.AlbedoTexture.Save(Path.Combine(outputDirectory, "blood_gulch_albedo.png"), ImageFormat.Png);
            Console.WriteLine($"Processed {xtt}");

            var xtd = HWXtdResource.FromFile(context, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.xtd");
            xtd.AmbientOcclusionTexture.Save(Path.Combine(outputDirectory, "blood_gulch_ao.png"), ImageFormat.Png);
            xtd.OpacityTexture.Save(Path.Combine(outputDirectory, "blood_gulch_opacity.png"), ImageFormat.Png);
            xtd.Mesh.Export(Path.Combine(outputDirectory, "blood_gulch_vismesh.obj"), GenericMeshExportFormat.Obj);
            Console.WriteLine($"Processed {xtd}");

            var gls = HWGlsResource.FromFile(context, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.gls");
            Console.WriteLine($"Processed {gls}");

            var scn = HWScnResource.FromFile(context, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.scn");
            PrintScenarioObjects(scn);
            Console.WriteLine($"Processed {scn}");

            var sc2 = HWSc2Resource.FromFile(context, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.sc2");
            PrintScenarioObjects(sc2);
            Console.WriteLine($"Processed {sc2}");

            var sc3 = HWSc3Resource.FromFile(context, "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.sc3");
            PrintScenarioObjects(sc3);
            Console.WriteLine($"Processed {sc3}");

            var vis = HWVisResource.FromFile(context, "art\\covenant\\building\\builder_03\\builder_03.vis");
            Console.WriteLine($"Processed {vis}");

            foreach (var model in vis.Models) {
                model.Resource.Mesh.Export(Path.Combine(outputDirectory, Path.GetFileName(model.Resource.AbsolutePath)), GenericMeshExportFormat.Obj);
                Console.WriteLine($"Processed {model.Resource}");
            }
        }

        static void PrintScenarioObjects(HWScnResource scenario) {
            foreach (var obj in scenario.Objects) {
                Console.WriteLine($"\t{obj}");
            }
        }
    }
}
