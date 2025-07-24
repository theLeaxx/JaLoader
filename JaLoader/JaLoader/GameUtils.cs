using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

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
    }
}
