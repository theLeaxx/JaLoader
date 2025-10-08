using JaLoader.Common.Interfaces;
using System;
using System.Linq;

namespace JaLoader.Common
{
    public static class RuntimeVariables
    {
        public static string ApplicationDataPath;
        public static ILogger Logger;
        public static IModLoader ModLoader;
        public static IGitHubReleaseUtils GitHubReleaseUtils;
        public static bool NoModsFlag = Environment.GetCommandLineArgs().Length > 1 && Environment.GetCommandLineArgs().Any(arg => arg.Equals("-no-mods", StringComparison.OrdinalIgnoreCase));
    }
}
