using JaLoader.Common;
using System;
using System.Diagnostics;

namespace JaLoader.Common
{
    public static class UpdateUtils
    {
        internal static DateTime lastUpdateCheck;
        private static bool canCheckForUpdates = true;

        internal static bool CanCheckForUpdates()
        {
            return canCheckForUpdates;
        }

        public static void StartJaLoaderUpdate()
        {
            Process.Start($@"{RuntimeVariables.ApplicationDataPath}\..\JaUpdater.exe", $"{JaLoaderSettings.ModFolderLocation} Jalopy");
            Process.GetCurrentProcess().Kill();
        }

        internal static bool JaLoaderUpdateAvailable()
        {
            return JaLoaderUpdateAvailable(out _);
        }

        internal static bool JaLoaderUpdateAvailable(out string latestVersionString)
        {
            latestVersionString = null;
            if (!canCheckForUpdates)
                return false;

            string URL = "https://api.github.com/repos/JaJalopy/Jalopy/releases/latest";
            string latestVersion = GetLatestUpdateVersionAsString(URL, JaLoaderSettings.GetVersion());
            latestVersionString = latestVersion;

            if (latestVersion == "-1")
            {
                RuntimeVariables.Logger.ILogError("Couldn't check for updates!");
                return false;
            }

            int latestVersionInt = ConvertVersionStringToInt(latestVersion);

            if (latestVersionInt <= JaLoaderSettings.GetVersion())
                return false;

            JaLoaderSettings.UpdateAvailable = true;
            return true;
        }

        public static bool CheckForModUpdate(IMod mod, out string latestVersion)
        {
            if (!canCheckForUpdates || string.IsNullOrEmpty(mod.GitHubLink))
            {
                latestVersion = null;
                return false;
            }

            string[] splitLink = mod.GitHubLink.Split('/');

            string URL = $"https://api.github.com/repos/{splitLink[3]}/{splitLink[4]}/releases/latest";

            int currentVersion = int.Parse(mod.ModVersion.Replace(".", ""));

            latestVersion = GetLatestUpdateVersionAsString(URL, currentVersion);

            int intLatestVersion = GetLatestUpdateVersionAsInt(URL, currentVersion);

            if (intLatestVersion > currentVersion)
                return true;

            return false;
        }

        public static int GetLatestUpdateVersionAsInt(string URL, int version)
        {
            return int.Parse(GetLatestUpdateVersionAsString(URL, version).Replace(".", ""));
        }

        public static string GetLatestUpdateVersionAsString(string URL, int version)
        {
            if (!CanCheckForUpdatesInternal())
                return "0";

            string latestVersion = RuntimeVariables.GitHubReleaseUtils.GetLatestTagFromAPIURL(URL);
            int latestVersionInt = int.Parse(latestVersion.Replace(".", ""));

            if (latestVersion == "-1")
                return "-1";

            if (latestVersionInt > version)
                return latestVersion;

            return "0";
        }

        public static int ConvertVersionStringToInt(string version)
        {
            return int.Parse(version.Replace(".", ""));
        }

        private static bool CanCheckForUpdatesInternal()
        {
            bool canCheck = false;

            switch (JaLoaderSettings.UpdateCheckMode)
            {
                case UpdateCheckModes.Never:
                    break;

                case UpdateCheckModes.Hourly:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalHours >= 1)
                    {
                        JaLoaderSettings.SetUpdateCheckRegistryKey();
                        canCheck = true;
                    }
                    break;

                case UpdateCheckModes.Daily:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalDays >= 1)
                    {
                        JaLoaderSettings.SetUpdateCheckRegistryKey();
                        canCheck = true;
                    }
                    break;

                case UpdateCheckModes.Every3Days:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalDays >= 3)
                    {
                        JaLoaderSettings.SetUpdateCheckRegistryKey();
                        canCheck = true;
                    }
                    break;

                case UpdateCheckModes.Weekly:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalDays >= 7)
                    {
                        JaLoaderSettings.SetUpdateCheckRegistryKey();
                        canCheck = true;
                    }
                    break;
            }

            canCheckForUpdates = canCheck;
            return canCheck;
        }
    }
}
