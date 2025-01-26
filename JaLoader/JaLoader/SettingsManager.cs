﻿using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Microsoft.Win32;
using System;
using BepInEx;
using JaLoader.BepInExWrapper;

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

            ReadSettings();
            SetVersionRegistryKey();
            GetUpdateCheckRegistryKey();
        }

        #endregion

        [SerializeField] private Settings _settings = new Settings();

        public const string JaLoaderVersion = "4.0.0";
        public static readonly bool IsPreReleaseVersion = false;
        public const string JaLoaderGitHubLink = "https://github.com/theLeaxx/JaLoader";
        public string ModFolderLocation { get; private set; }

        public bool SkipLanguage;
        public bool DebugMode;
        public bool DisableUncle;
        public bool HideModFolderLocation;
        public bool DisableMenuMusic;
        public int MenuMusicVolume;
        public bool UseExperimentalCharacterController;
        public bool UseCustomSongs;
        public CustomSongsBehaviour CustomSongsBehaviour;
        public bool RadioAds;
        public ConsolePositions ConsolePosition;
        public ConsoleModes ConsoleMode;
        public LicensePlateStyles ChangeLicensePlateText;
        public bool UseDiscordRichPresence;
        public string LicensePlateText;
        public bool ShowFPSCounter;
        public UpdateCheckModes UpdateCheckMode;
        public bool EnableJaDownloader;
        public bool AskedAboutJaDownloader;
        public bool FixLaikaShopMusic;
        public MirrorDistances MirrorDistances;
        public CursorMode CursorMode;
        public bool ShowDisabledMods;
        public string AppliedPaintJobName;
        public bool FixItemsFalilngBehindShop;
        public bool FixBorderGuardsFlags;

        public List<string> DontShowAgainNotices = new List<string>();

        public DateTime lastUpdateCheck;
        private bool shouldCheckForUpdates = true;

        public bool updateAvailable;

        public bool loadedFirstTime;
        public bool selectedLanguage;

        public List<string> DisabledMods = new List<string>();

        private readonly ModLoader modLoaderReference = ModLoader.Instance;

        public int GetLatestUpdateVersion(string URL, int version)
        {
            if(!shouldCheckForUpdates)
            {
                return 0;
            }

            bool shouldContinueChecking = false;

            switch (UpdateCheckMode)
            {
                case UpdateCheckModes.Never:
                    break;

                case UpdateCheckModes.Hourly:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalHours >= 1)
                    {
                        SetUpdateCheckRegistryKey();
                        shouldContinueChecking = true;
                    }
                    break;

                case UpdateCheckModes.Daily:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalDays >= 1)
                    {
                        SetUpdateCheckRegistryKey();
                        shouldContinueChecking = true;
                    }
                    break;

                case UpdateCheckModes.Every3Days:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalDays >= 3)
                    {
                        SetUpdateCheckRegistryKey();
                        shouldContinueChecking = true;
                    }
                    break;

                case UpdateCheckModes.Weekly:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalDays >= 7)
                    {
                        SetUpdateCheckRegistryKey();
                        shouldContinueChecking = true;
                    }
                    break;
            }

            shouldCheckForUpdates = shouldContinueChecking;

            if (!shouldContinueChecking)
                return 0;
            else
            {
                string latestVersion = ModHelper.Instance.GetLatestTagFromApiUrl(URL);
                int latestVersionInt = int.Parse(latestVersion.Replace(".", ""));

                if (latestVersion == "-1")
                    return -1;
                else if (latestVersionInt > version)
                {
                    return latestVersionInt;
                }
            }

            return 0;
        }

        public string GetLatestUpdateVersionString(string URL, int version)
        {
            if (!shouldCheckForUpdates)
            {
                return "0";
            }

            bool shouldContinueChecking = false;

            switch (UpdateCheckMode)
            {
                case UpdateCheckModes.Never:
                    break;

                case UpdateCheckModes.Hourly:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalHours >= 1)
                    {
                        SetUpdateCheckRegistryKey();
                        shouldContinueChecking = true;
                    }
                    break;

                case UpdateCheckModes.Daily:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalDays >= 1)
                    {
                        SetUpdateCheckRegistryKey();
                        shouldContinueChecking = true;
                    }
                    break;

                case UpdateCheckModes.Every3Days:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalDays >= 3)
                    {
                        SetUpdateCheckRegistryKey();
                        shouldContinueChecking = true;
                    }
                    break;

                case UpdateCheckModes.Weekly:
                    if (DateTime.Now.Subtract(lastUpdateCheck).TotalDays >= 7)
                    {
                        SetUpdateCheckRegistryKey();
                        shouldContinueChecking = true;
                    }
                    break;
            }

            shouldCheckForUpdates = shouldContinueChecking;

            if (!shouldContinueChecking)
                return "0";
            else
            {
                string latestVersion = ModHelper.Instance.GetLatestTagFromApiUrl(URL);
                int latestVersionInt = int.Parse(latestVersion.Replace(".", ""));

                if (latestVersion == "-1")
                    return "-1";
                else if (latestVersionInt > version)
                {
                    return latestVersion;
                }
            }

            return "0";
        }

        public int GetVersion()
        {
            return int.Parse(JaLoaderVersion.Replace(".", ""));
        }

        public string GetVersionString()
        {
            if (IsPreReleaseVersion)
                return $"Pre-Release {JaLoaderVersion}";

            return JaLoaderVersion;
        }

        public bool IsNewerThan(string version)
        {
            var versionSpecified = int.Parse(version.Replace(".", ""));
            var currentVersion = int.Parse(GetVersionString().Replace("Pre-Release ", "").Replace(".", ""));

            if (currentVersion > versionSpecified) return true;
            else return false;
        }

        private void SetVersionRegistryKey()
        {
            RegistryKey parentKey = Registry.CurrentUser;

            RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

            RegistryKey jalopyKey = softwareKey?.OpenSubKey("Jalopy", true);

            jalopyKey?.SetValue("JaLoaderVersion", GetVersion().ToString(), RegistryValueKind.String);
        }

        private void SetUpdateCheckRegistryKey()
        {
            RegistryKey parentKey = Registry.CurrentUser;

            RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

            RegistryKey jalopyKey = softwareKey?.OpenSubKey("Jalopy", true);

            jalopyKey?.SetValue("LastUpdateCheck", DateTime.Now.ToString(), RegistryValueKind.String);
        }

        private void GetUpdateCheckRegistryKey()
        {
            RegistryKey parentKey = Registry.CurrentUser;

            RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

            RegistryKey jalopyKey = softwareKey?.OpenSubKey("Jalopy", true);

            if (jalopyKey != null && jalopyKey.GetValue("LastUpdateCheck") != null)
            {
                lastUpdateCheck = DateTime.Parse(jalopyKey.GetValue("LastUpdateCheck").ToString());
            }
            else
            {
                SetUpdateCheckRegistryKey();
                lastUpdateCheck = DateTime.Now;
            }
        }

        private void ReadSettings()
        {
            RegistryKey parentKey = Registry.CurrentUser;

            RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

            RegistryKey jalopyKey = softwareKey?.OpenSubKey("Jalopy", true);

            if (jalopyKey != null && jalopyKey.GetValue("ModsLocation") != null)
            {
                ModFolderLocation = jalopyKey.GetValue("ModsLocation").ToString();
            }
            else
            {
                ModFolderLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Jalopy\Mods");

                jalopyKey?.SetValue("ModsLocation", ModFolderLocation, RegistryValueKind.String);
            }

            if (File.Exists(Path.Combine(Application.persistentDataPath, @"JaConfig.json")))
            {
                string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, @"JaConfig.json"));
                _settings = JsonUtility.FromJson<Settings>(json);

                Load();
            }
            else
            {
                Load();

                File.WriteAllText(Path.Combine(Application.persistentDataPath, @"JaConfig.json"), JsonUtility.ToJson(_settings, true));
                return;
            }
        }

        private void Load()
        {
            ConsolePosition = _settings.ConsolePosition;
            ConsoleMode = _settings.ConsoleMode;

            SkipLanguage = _settings.SkipLanguageSelector;
            DisableUncle = _settings.DisableUncle;
            DebugMode = _settings.DebugMode;
            HideModFolderLocation = _settings.HideModFolder;
            DisableMenuMusic = _settings.DisableMenuMusic;
            MenuMusicVolume = _settings.MenuMusicVolume;
            UseExperimentalCharacterController = _settings.UseEnhancedMovement;
            UseCustomSongs = _settings.UseCustomSongs;
            CustomSongsBehaviour = _settings.CustomSongsBehaviour;
            RadioAds = _settings.RadioAds;
            DisabledMods = _settings.DisabledMods;
            ChangeLicensePlateText = _settings.ChangeLicensePlateText;
            LicensePlateText = _settings.LicensePlateText;
            UseDiscordRichPresence = _settings.UseDiscordRichPresence;
            ShowFPSCounter = _settings.ShowFPSCounter;
            EnableJaDownloader = _settings.EnableJaDownloader;
            AskedAboutJaDownloader = _settings.AskedAboutJaDownloader;
            UpdateCheckMode = _settings.UpdateCheckMode;
            FixLaikaShopMusic = _settings.FixLaikaShopMusic;
            MirrorDistances = _settings.MirrorDistances;
            CursorMode = _settings.CursorMode;
            ShowDisabledMods = _settings.ShowDisabledMods;
            AppliedPaintJobName = _settings.AppliedPaintJobName;
            //FixItemsFalilngBehindShop = _settings.FixItemsFalilngBehindShop;
            FixBorderGuardsFlags = _settings.FixBorderGuardsFlags;

            DontShowAgainNotices = _settings.DontShowAgainNotices;

            EventsManager.Instance.OnSettingsLoad();
        }

        public void SaveSettings(bool includeDisabledMods = true)
        {
            if (includeDisabledMods)
                DisabledMods.Clear();

            _settings.SkipLanguageSelector = SkipLanguage;
            _settings.DisableUncle = DisableUncle;
            _settings.DebugMode = DebugMode;
            _settings.HideModFolder = HideModFolderLocation;
            _settings.DisableMenuMusic = DisableMenuMusic;
            _settings.MenuMusicVolume = MenuMusicVolume;
            _settings.UseEnhancedMovement = UseExperimentalCharacterController;
            _settings.DisabledMods = DisabledMods;
            _settings.UseCustomSongs = UseCustomSongs;
            _settings.CustomSongsBehaviour = CustomSongsBehaviour;
            _settings.RadioAds = RadioAds;
            _settings.ConsolePosition = ConsolePosition;
            _settings.ConsoleMode = ConsoleMode;
            _settings.ChangeLicensePlateText = ChangeLicensePlateText;
            _settings.LicensePlateText = LicensePlateText;
            _settings.UseDiscordRichPresence = UseDiscordRichPresence;
            _settings.ShowFPSCounter = ShowFPSCounter;
            _settings.EnableJaDownloader = EnableJaDownloader;
            _settings.AskedAboutJaDownloader = AskedAboutJaDownloader;
            _settings.UpdateCheckMode = UpdateCheckMode;

            _settings.FixLaikaShopMusic = FixLaikaShopMusic;
            _settings.MirrorDistances = MirrorDistances;
            _settings.CursorMode = CursorMode;
            _settings.ShowDisabledMods = ShowDisabledMods;

            _settings.AppliedPaintJobName = AppliedPaintJobName;
            //_settings.FixItemsFalilngBehindShop = FixItemsFalilngBehindShop;
            _settings.FixBorderGuardsFlags = FixBorderGuardsFlags;

            _settings.DontShowAgainNotices = DontShowAgainNotices;

            if (includeDisabledMods)
            {
                for (int i = 0; i < modLoaderReference.disabledMods.ToArray().Length; i++)
                {
                    var mod = modLoaderReference.disabledMods.ToArray()[i];

                    if (mod is Mod)
                    {
                        var modReference = mod as Mod;
                        DisabledMods.Add($"{modReference.ModAuthor}_{modReference.ModID}_{modReference.ModName}");
                    }
                    else if (mod is BaseUnityPlugin)
                    {
                        var modReference = mod as BaseUnityPlugin;

                        ModInfo modInfo = modReference.gameObject.GetComponent<ModInfo>();

                        DisabledMods.Add($"BepInEx_CompatLayer_{modInfo.GUID}");
                    }
                }
            }

            _settings.DisabledMods = DisabledMods;

            File.WriteAllText(Path.Combine(Application.persistentDataPath, @"JaConfig.json"), JsonUtility.ToJson(_settings, true));

            EventsManager.Instance.OnSettingsSave();
        }

        private void Update()
        {
            if (!DebugMode) return;

            if (Input.GetKeyDown(KeyCode.F5))
                ReadSettings();
        }
    }

    [System.Serializable]
    public class Settings
    {
        [SerializeField] public bool SkipLanguageSelector = true;
        [SerializeField] public bool DebugMode = false;
        [SerializeField] public ConsolePositions ConsolePosition = ConsolePositions.BottomLeft;
        [SerializeField] public ConsoleModes ConsoleMode = ConsoleModes.Default;
        [SerializeField] public List<string> DisabledMods = new List<string>();

        [SerializeField] public bool DisableMenuMusic = false;
        [SerializeField] public int MenuMusicVolume = 50;
        [SerializeField] public bool HideModFolder = false;
        [SerializeField] public bool DisableUncle = false;
        [SerializeField] public bool UseCustomSongs = true;
        [SerializeField] public CustomSongsBehaviour CustomSongsBehaviour = CustomSongsBehaviour.Replace;
        [SerializeField] public bool RadioAds = true;
        [SerializeField] public bool UseEnhancedMovement = false;
        [SerializeField] public LicensePlateStyles ChangeLicensePlateText = LicensePlateStyles.None;
        [SerializeField] public string LicensePlateText = "";
        [SerializeField] public bool UseDiscordRichPresence = true;
        [SerializeField] public bool ShowFPSCounter = false;
        [SerializeField] public bool EnableJaDownloader = false;
        [SerializeField] public bool AskedAboutJaDownloader = false;
        [SerializeField] public UpdateCheckModes UpdateCheckMode = UpdateCheckModes.Daily;
        [SerializeField] public bool FixLaikaShopMusic = true;
        [SerializeField] public MirrorDistances MirrorDistances = MirrorDistances.m1000;
        [SerializeField] public CursorMode CursorMode = CursorMode.Default;
        [SerializeField] public bool ShowDisabledMods = true;
        //[SerializeField] public bool FixItemsFalilngBehindShop = true;
        [SerializeField] public bool FixBorderGuardsFlags = true;

        [SerializeField] public string AppliedPaintJobName = "";

        [SerializeField] public List<string> DontShowAgainNotices = new List<string>();
    }

    public enum UpdateCheckModes
    {
        Never,
        Hourly,
        Daily,
        Every3Days,
        Weekly
    }

    public enum CustomSongsBehaviour
    {
        Add,
        Replace
    }

    public enum MirrorDistances
    {
        m250,
        m500,
        m750,
        m1000,
        m1500,
        m2000,
        m2500,
        m3000,
        m3500,
        m4000,
        m5000
    }

    public enum CursorMode
    {
        Default,
        Circle,
        Hidden
    }
}
