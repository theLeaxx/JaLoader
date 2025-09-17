using JaLoader.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JaLoader
{
    public class GameUtils : MonoBehaviour
    {
        public static bool IsCrackedGame;

        internal static bool CheckForCrack()
        {
            var mainGameFolder = $@"{Application.dataPath}\..";

            string[] commonCrackFiles = new string[]
            {
                "codex64.dll",
                "steam_api64.cdx",
                "steamclient64.dll",
                "steam_emu.ini",
                "SmartSteamEmu.dll",
                "SmartSteamEmu64.dll",
                "Launcher.exe",
                "Launcher_x64.exe",
            };

            foreach (var file in commonCrackFiles)
                if (File.Exists($@"{mainGameFolder}\{file}") || Directory.Exists($@"{mainGameFolder}\SmartSteamEmu"))
                {
                    IsCrackedGame = true;
                    return true;
                }

            return false;
        }

        internal static void SwitchLanguage()
        {
            JaLoaderSettings.SelectedLanguage = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }

        internal static void OpenModsFolder()
        {
            Application.OpenURL(JaLoaderSettings.ModFolderLocation);
        }

        internal static void OpenOutputLog()
        {
            string path = Path.Combine(Application.persistentDataPath, "output_log.txt");

            Process.Start(path);
        }

        internal static void OpenSavesFolder()
        {
            Application.OpenURL(Application.persistentDataPath);
        }
    }
}
