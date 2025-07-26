using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    public static class UpdateUtils
    {
        public static void StartJaLoaderUpdate()
        {
            Process.Start($@"{Application.dataPath}\..\JaUpdater.exe", $"{SettingsManager.ModFolderLocation} Jalopy");
            Process.GetCurrentProcess().Kill();
        }

        public static bool CheckForModUpdate(Mod mod)
        {
            string[] splitLink = mod.GitHubLink.Split('/');

            string URL = $"https://api.github.com/repos/{splitLink[3]}/{splitLink[4]}/releases/latest";

            int currentVersion = int.Parse(mod.ModVersion.Replace(".", ""));

            string latestVersion = SettingsManager.GetLatestUpdateVersionString(URL, currentVersion);

            if (int.Parse(latestVersion.Replace(".", "")) > currentVersion)
            {
               /* modsNeedUpdate++;
                modVersionText = $"{mod.ModVersion} <color=green>(Latest version: {latestVersion})</color>";
                modName = $"<color=green>(Update Available!)</color> {mod.ModName}";

                uiManager.MakeNutGreen();*/
            }

            return false;
        }
    }
}
