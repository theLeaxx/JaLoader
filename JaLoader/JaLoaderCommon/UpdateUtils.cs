using JaLoader.Common;
using System;
using System.Data;
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

        internal static bool JaLoaderUpdateAvailable(bool force = false)
        {
            return JaLoaderUpdateAvailable(out _, out _, force);
        }

        internal static bool JaLoaderUpdateAvailable(out string latestVersionString, out string changelog, bool force = false)
        {
            latestVersionString = changelog = null;
            if (!canCheckForUpdates && !force)
                return false;

            string URL = "https://api.github.com/repos/theLeaxx/JaLoader/releases/latest";
            Release latestRelease = GetLatestUpdateAsRelease(URL, JaLoaderSettings.GetVersion(), force);
            latestVersionString = latestRelease.tag_name;
            changelog = latestRelease.body;

            if (latestRelease.tag_name == "-1")
            {
                RuntimeVariables.Logger.ILogError("Couldn't check for updates!");
                return false;
            }

            int latestVersionInt = ConvertVersionStringToInt(latestRelease.tag_name);

            if (latestVersionInt <= JaLoaderSettings.GetVersion())
                return false;

            JaLoaderSettings.UpdateAvailable = true;
            return true;
        }

        public static bool CheckForModUpdate(IMod mod, out string latestVersion, bool force = false)
        {
            if ((!canCheckForUpdates && !force)|| string.IsNullOrEmpty(mod.GitHubLink))
            {
                latestVersion = null;
                return false;
            }

            string[] splitLink = mod.GitHubLink.Split('/');

            string URL = $"https://api.github.com/repos/{splitLink[3]}/{splitLink[4]}/releases/latest";

            int currentVersion = int.Parse(mod.ModVersion.Replace(".", ""));

            latestVersion = GetLatestUpdateVersionAsString(URL, currentVersion, force);

            int intLatestVersion = GetLatestUpdateVersionAsInt(URL, currentVersion, force);

            if (intLatestVersion > currentVersion)
                return true;

            return false;
        }

        public static int GetLatestUpdateVersionAsInt(string URL, int version, bool force = false)
        {
            return int.Parse(GetLatestUpdateVersionAsString(URL, version, force).Replace(".", ""));
        }

        public static string GetLatestUpdateVersionAsString(string URL, int version, bool force = false)
        {
            return GetLatestUpdateAsRelease(URL, version, force).tag_name;
        }

        public static Release GetLatestUpdateAsRelease(string URL, int version, bool force = false)
        {
            if (!CanCheckForUpdatesInternal() && !force)
                return new Release()
                {
                    tag_name = "0"
                };

            Release latestRelease = RuntimeVariables.GitHubReleaseUtils.GetLatestTagFromAPIURL(URL);
            int latestVersionInt = int.Parse(latestRelease.tag_name.Replace(".", ""));

            if (latestRelease.tag_name == "-1")
                return new Release()
                {
                    tag_name = "-1"
                };

            if (latestVersionInt > version)
                return latestRelease;

            return new Release()
            {
                tag_name = "0"
            }; ;
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
                        JaLoaderSettings.SetUpdateCheckInJSON();
                        canCheck = true;
                    }
                    break;

                case UpdateCheckModes.Daily:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalDays >= 1)
                    {
                        JaLoaderSettings.SetUpdateCheckInJSON();
                        canCheck = true;
                    }
                    break;

                case UpdateCheckModes.Every3Days:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalDays >= 3)
                    {
                        JaLoaderSettings.SetUpdateCheckInJSON();
                        canCheck = true;
                    }
                    break;

                case UpdateCheckModes.Weekly:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalDays >= 7)
                    {
                        JaLoaderSettings.SetUpdateCheckInJSON();
                        canCheck = true;
                    }
                    break;
            }

            canCheckForUpdates = canCheck;
            return canCheck;
        }
    }

    [Serializable]
    public class Release
    {
        public string tag_name = "";
        public string body = "";
    }
}
