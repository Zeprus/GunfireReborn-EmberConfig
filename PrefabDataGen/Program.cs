namespace EmberConfig.PrefabDataGen;

using System;
using System.IO;

internal static class Program
{
    internal static int Main(string[] args)
    {
        if (args.Length is not 2)
        {
            Console.WriteLine("Usage: EmberConfig.PrefabDataGen <assetRipsPath> <outputPath>");
            return 1;
        }

        var assetRipsPath = Path.GetFullPath(args[0]);
        var outputPath = Path.GetFullPath(args[1]);

        if (!Directory.Exists(assetRipsPath))
        {
            Console.Error.WriteLine($"AssetRips path not found: {assetRipsPath}");
            return 2;
        }

        Directory.CreateDirectory(outputPath);

        try
        {
            var generator = new Generation.Generator(new Configuration.Paths(assetRipsPath, outputPath));
            generator.Run();
            Console.WriteLine($"Generated files written to {outputPath}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Generation failed: {ex}");
            return 3;
        }
    }
}
