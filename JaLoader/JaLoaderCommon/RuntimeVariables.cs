using JaLoader.Common.Interfaces;

namespace JaLoader.Common
{
    public static class RuntimeVariables
    {
        public static string ApplicationDataPath;
        public static ILogger Logger;
        public static IModLoader ModLoader;
        public static IGitHubReleaseUtils GitHubReleaseUtils;
    }
}
