using System;
using System.Diagnostics;
using UnityEngine;

namespace JaLoader
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
            Process.Start($@"{Application.dataPath}\..\JaUpdater.exe", $"{SettingsManager.ModFolderLocation} Jalopy");
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
            string latestVersion = GetLatestUpdateVersionAsString(URL, SettingsManager.GetVersion());
            latestVersionString = latestVersion;

            if (latestVersion == "-1")
            {
                Console.LogError("Couldn't check for updates!");
                return false;
            }

            int latestVersionInt = ConvertVersionStringToInt(latestVersion);

            if (latestVersionInt <= SettingsManager.GetVersion())
                return false;

            SettingsManager.updateAvailable = true;
            return true;
        }

        public static bool CheckForModUpdate(Mod mod, out string latestVersion)
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

            string latestVersion = ModHelper.Instance.GetLatestTagFromApiUrl(URL);
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

            switch (SettingsManager.UpdateCheckMode)
            {
                case UpdateCheckModes.Never:
                    break;

                case UpdateCheckModes.Hourly:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalHours >= 1)
                    {
                        SettingsManager.SetUpdateCheckRegistryKey();
                        canCheck = true;
                    }
                    break;

                case UpdateCheckModes.Daily:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalDays >= 1)
                    {
                        SettingsManager.SetUpdateCheckRegistryKey();
                        canCheck = true;
                    }
                    break;

                case UpdateCheckModes.Every3Days:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalDays >= 3)
                    {
                        SettingsManager.SetUpdateCheckRegistryKey();
                        canCheck = true;
                    }
                    break;

                case UpdateCheckModes.Weekly:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalDays >= 7)
                    {
                        SettingsManager.SetUpdateCheckRegistryKey();
                        canCheck = true;
                    }
                    break;
            }

            canCheckForUpdates = canCheck;
            return canCheck;
        }
    }
}
