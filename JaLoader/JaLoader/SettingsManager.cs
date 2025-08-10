using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Microsoft.Win32;
using System;
using BepInEx;
using JaLoader.BepInExWrapper;
using System.Reflection;
using JaLoader.Common;
using CursorMode = JaLoader.Common.CursorMode;

namespace JaLoader
{
    public static class SettingsManager
    {
        internal static void Initialize()
        {
            JaLoaderSettings.JaLoaderVersion = "5.0.0";
            JaLoaderSettings.IsPreReleaseVersion = false;

            ReadSettings();
            JaLoaderSettings.SetVersionRegistryKey();
            JaLoaderSettings.GetUpdateCheckRegistryKey();

            JaLoaderSettings.CompareAssemblyVersion();
        }

        [SerializeField] private static SerializableJaLoaderSettings _settings = new SerializableJaLoaderSettings();

        internal static void ReadSettings()
        {
            JaLoaderSettings.ReadSettings();

            if (File.Exists(Path.Combine(Application.persistentDataPath, @"JaConfig.json")))
            {
                string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, @"JaConfig.json"));
                _settings = JsonUtility.FromJson<SerializableJaLoaderSettings>(json);

                Load();
            }
            else
            {
                Load();

                File.WriteAllText(Path.Combine(Application.persistentDataPath, @"JaConfig.json"), JsonUtility.ToJson(_settings, true));
                return;
            }
        }

        private static void Load()
        {
            JaLoaderSettings.LoadSettings(_settings);

            EventsManager.Instance.OnSettingsLoad();
        }

        public static void SaveSettings(bool includeDisabledMods = true)
        {
            if (includeDisabledMods)
                JaLoaderSettings.DisabledMods.Clear();

            JaLoaderSettings.SaveSettings(_settings);

            if (includeDisabledMods)
            {
                foreach (var mod in ModManager.Mods)
                {
                    if (!mod.Value.IsEnabled)
                    {
                        if (mod.Key is Mod)
                        {
                            var modReference = mod.Key as Mod;
                            JaLoaderSettings.DisabledMods.Add($"{modReference.ModAuthor}_{modReference.ModID}_{modReference.ModName}");
                        }
                        else if (mod.Key is BaseUnityPlugin)
                        {
                            var modReference = mod.Key as BaseUnityPlugin;
                            ModInfo modInfo = modReference.gameObject.GetComponent<ModInfo>();
                            JaLoaderSettings.DisabledMods.Add($"BepInEx_CompatLayer_{modInfo.GUID}");
                        }
                    }
                }
            }

            _settings.DisabledMods = JaLoaderSettings.DisabledMods;

            File.WriteAllText(Path.Combine(Application.persistentDataPath, @"JaConfig.json"), JsonUtility.ToJson(_settings, true));

            EventsManager.Instance.OnSettingsSave();
        }
    }
}
