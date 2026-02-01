using JaLoader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoaderClassic
{
    public static class SettingsManager
    {
        public static void Initialize()
        {
            JaLoaderSettings.JaLoaderVersion = "1.0.0";
            JaLoaderSettings.IsPreReleaseVersion = false;

            ReadSettings();
            JaLoaderSettings.SetJaLoaderVersionInJSON();
            JaLoaderSettings.GetUpdateCheckFromJSON();

            //JaLoaderSettings.CompareAssemblyVersion();
        }

        [SerializeField] private static SerializableJaLoaderSettings _settings = new SerializableJaLoaderSettings();

        internal static void ReadSettings()
        {
            JaLoaderSettings.ReadEssentialSettings();

            /*if (File.Exists(Path.Combine(Application.persistentDataPath, @"JaConfig.json")))
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
            }*/
        }

        private static void Load()
        {
            JaLoaderSettings.LoadSettings(_settings);

            Console.InternalLog(_settings.LicensePlateText);

            //EventsManager.Instance.OnSettingsLoad();
        }

        public static void SaveSettings(bool includeDisabledMods = true)
        {
            if (includeDisabledMods)
                JaLoaderSettings.DisabledMods.Clear();

            JaLoaderSettings.SaveSettings(_settings);

            /*if (includeDisabledMods)
            {
                foreach (var mod in ModManager.Mods)
                {
                    if (!mod.Value.IsEnabled)
                    {
                        if (mod.Key is ModClassic)
                        {
                            var modReference = mod.Key as ModClassic;
                            JaLoaderSettings.DisabledMods.Add($"{modReference.ModAuthor}_{modReference.ModID}_{modReference.ModName}");
                        }
                        /*else if (mod.Key is BaseUnityPlugin)
                        {
                            var modReference = mod.Key as BaseUnityPlugin;
                            ModInfo modInfo = modReference.gameObject.GetComponent<ModInfo>();
                            JaLoaderSettings.DisabledMods.Add($"BepInEx_CompatLayer_{modInfo.GUID}");
                        }
                    }
                }
            }

            _settings.DisabledMods = JaLoaderSettings.DisabledMods;*/

            //File.WriteAllText(Path.Combine(Application.persistentDataPath, @"JaConfig.json"), JsonUtility.ToJson(_settings, true));

            //EventsManager.Instance.OnSettingsSave();
        }
        }
    }
