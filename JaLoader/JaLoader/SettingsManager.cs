using System.IO;
using UnityEngine;
using BepInEx;
using JaLoader.BepInExWrapper;
using JaLoader.Common;
using System.Diagnostics;

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

        public static string GetJaLoaderVersion()
        {
            return JaLoaderSettings.JaLoaderVersion;
        }

        public static bool IsPreReleaseVersion()
        {
            return JaLoaderSettings.IsPreReleaseVersion;
        }

        public static object GetSettingValue(string settingID)
        {
            switch (settingID)
            {
                case "SkipLanguage": 
                    return JaLoaderSettings.SkipLanguage;

                case "DebugMode":
                    return JaLoaderSettings.DebugMode;

                case "DisableUncle":
                    return JaLoaderSettings.DisableUncle;

                case "HideModFolderLocation":
                    return JaLoaderSettings.HideModFolderLocation;

                case "DisableMenuMusic":
                    return JaLoaderSettings.DisableMenuMusic;

                case "MenuMusicVolume":
                    return JaLoaderSettings.MenuMusicVolume;

                case "UseExperimentalCharacterController":
                    return JaLoaderSettings.UseExperimentalCharacterController;

                case "UseCustomSongs":
                    return JaLoaderSettings.UseCustomSongs;

                case "CustomSongsBehaviour":
                    return JaLoaderSettings.CustomSongsBehaviour;

                case "RadioAds":
                    return JaLoaderSettings.RadioAds;

                case "ConsolePosition":
                    return JaLoaderSettings.ConsolePosition;

                case "ConsoleMode":
                    return JaLoaderSettings.ConsoleMode;

                case "ChangeLicensePlateText":
                    return JaLoaderSettings.ChangeLicensePlateText;

                case "UseDiscordRichPresence":
                    return JaLoaderSettings.UseDiscordRichPresence;

                case "LicensePlateText":
                    return JaLoaderSettings.LicensePlateText;

                case "ShowFPSCounter":
                    return JaLoaderSettings.ShowFPSCounter;

                case "UpdateCheckMode":
                    return JaLoaderSettings.UpdateCheckMode;

                case "EnableJaDownloader":
                    return JaLoaderSettings.EnableJaDownloader;

                case "FixLaikaShopMusic":
                    return JaLoaderSettings.FixLaikaShopMusic;

                case "MirrorDistances":
                    return JaLoaderSettings.MirrorDistances;

                case "CursorMode":
                    return JaLoaderSettings.CursorMode;

                case "AppliedPaintJobName":
                    return JaLoaderSettings.AppliedPaintJobName;

                case "FixItemsFallingBehindShop":
                    return JaLoaderSettings.FixItemsFallingBehindShop;

                case "FixBorderGuardsFlags":
                    return JaLoaderSettings.FixBorderGuardsFlags;

                default:
                    Console.LogError("JaLoader", "Invalid setting ID provided to GetSettingValue.");
                    return null;
            }
        }

        public static void SetSettingValue(string settingID, object value)
        {
            Console.LogDebug("JaLoader", $"Overriding setting {settingID} to {value}");
            switch (settingID)
            {
                case "SkipLanguage":
                    JaLoaderSettings.SkipLanguage = (bool)value;
                    break;
                case "DebugMode":
                    JaLoaderSettings.DebugMode = (bool)value;
                    break;
                case "DisableUncle":
                    JaLoaderSettings.DisableUncle = (bool)value;
                    break;
                case "HideModFolderLocation":
                    JaLoaderSettings.HideModFolderLocation = (bool)value;
                    break;
                case "DisableMenuMusic":
                    JaLoaderSettings.DisableMenuMusic = (bool)value;
                    break;
                case "MenuMusicVolume":
                    JaLoaderSettings.MenuMusicVolume = (int)value;
                    break;
                case "UseExperimentalCharacterController":
                    JaLoaderSettings.UseExperimentalCharacterController = (bool)value;
                    break;
                case "UseCustomSongs":
                    JaLoaderSettings.UseCustomSongs = (bool)value;
                    break;
                case "CustomSongsBehaviour":
                    JaLoaderSettings.CustomSongsBehaviour = (CustomSongsBehaviour)(int)value;
                    break;
                case "RadioAds":
                    JaLoaderSettings.RadioAds = (bool)value;
                    break;
                case "ConsolePosition":
                    JaLoaderSettings.ConsolePosition = (ConsolePositions)value;
                    break;
                case "ConsoleMode":
                    JaLoaderSettings.ConsoleMode = (ConsoleModes)value;
                    break;
                case "ChangeLicensePlateText":
                    JaLoaderSettings.ChangeLicensePlateText = (LicensePlateStyles)value;
                    break;
                case "UseDiscordRichPresence":
                    JaLoaderSettings.UseDiscordRichPresence = (bool)value;
                    break;
                case "LicensePlateText":
                    JaLoaderSettings.LicensePlateText = (string)value;
                    break;
                case "ShowFPSCounter":
                    JaLoaderSettings.ShowFPSCounter = (bool)value;
                    break;
                case "UpdateCheckMode":
                    JaLoaderSettings.UpdateCheckMode = (UpdateCheckModes)value;
                    break;
                case "EnableJaDownloader":
                    JaLoaderSettings.EnableJaDownloader = (bool)value;
                    break;
                case "FixLaikaShopMusic":
                    JaLoaderSettings.FixLaikaShopMusic = (bool)value;
                    break;
                case "MirrorDistances":
                    JaLoaderSettings.MirrorDistances = (MirrorDistances)value;
                    break;
                case "CursorMode":
                    JaLoaderSettings.CursorMode = (Common.CursorMode)(int)value;
                    break;
                case "AppliedPaintJobName":
                    JaLoaderSettings.AppliedPaintJobName = (string)value;
                    break;

                case "FixItemsFallingBehindShop":
                    JaLoaderSettings.FixItemsFallingBehindShop = (bool)value;
                    break;

                case "FixBorderGuardsFlags":
                    JaLoaderSettings.FixBorderGuardsFlags = (bool)value;
                    break;

                default:
                    Console.LogError("JaLoader", "Invalid setting ID provided to SetSettingValue.");
                    break;
            }

            SaveSettings(false);
        }

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
