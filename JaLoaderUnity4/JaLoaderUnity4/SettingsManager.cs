using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    public class SettingsManager : MonoBehaviour
    {
        #region Singleton & ReadSettings on Awake
        public static SettingsManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            //ReadSettings();
            //SetVersionRegistryKey();
            //GetUpdateCheckRegistryKey();
        }

        #endregion

        public string ModFolderLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Jalopy\Mods");
    }
}
