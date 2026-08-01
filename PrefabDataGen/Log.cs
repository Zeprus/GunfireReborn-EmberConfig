namespace EmberConfig.PrefabDataGen;

using System;

internal static class Log
{
    internal static bool IsVerbose { get; set; } = true;

    internal static void Info(string message)
    {
        if (IsVerbose)
            Console.WriteLine(message);
    }

    internal static void Error(string message) => Console.Error.WriteLine(message);
}
