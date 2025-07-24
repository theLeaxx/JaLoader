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
    }
}
