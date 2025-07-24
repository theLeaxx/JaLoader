using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Microsoft.Win32;
using System;
using BepInEx;
using JaLoader.BepInExWrapper;

namespace JaLoader
{
    public static class SettingsManager
    {
        internal static void Initialize()
        {
            ReadSettings();
            SetVersionRegistryKey();
            GetUpdateCheckRegistryKey();
        }

        [SerializeField] private static Settings _settings = new Settings();

        public const string JaLoaderVersion = "5.0.0";
        public static readonly bool IsPreReleaseVersion = false;
        public const string JaLoaderGitHubLink = "https://github.com/theLeaxx/JaLoader";
        public static string ModFolderLocation { get; private set; }

        public static bool SkipLanguage;
        public static bool DebugMode;
        public static bool DisableUncle;
        public static bool HideModFolderLocation;
        public static bool DisableMenuMusic;
        public static int MenuMusicVolume;
        public static bool UseExperimentalCharacterController;
        public static bool UseCustomSongs;
        public static CustomSongsBehaviour CustomSongsBehaviour;
        public static bool RadioAds;
        public static ConsolePositions ConsolePosition;
        public static ConsoleModes ConsoleMode;
        public static LicensePlateStyles ChangeLicensePlateText;
        public static bool UseDiscordRichPresence;
        public static string LicensePlateText;
        public static bool ShowFPSCounter;
        public static UpdateCheckModes UpdateCheckMode;
        public static bool EnableJaDownloader;
        public static bool AskedAboutJaDownloader;
        public static bool FixLaikaShopMusic;
        public static MirrorDistances MirrorDistances;
        public static CursorMode CursorMode;
        public static bool ShowDisabledMods;
        public static string AppliedPaintJobName;
        public static bool FixItemsFalilngBehindShop;
        public static bool FixBorderGuardsFlags;

        public static List<string> DontShowAgainNotices = new List<string>();

        public static DateTime lastUpdateCheck;
        private static bool shouldCheckForUpdates = true;

        public static bool updateAvailable;

        public static bool loadedFirstTime;
        public static bool selectedLanguage;

        public static List<string> DisabledMods = new List<string>();

        public static int GetLatestUpdateVersion(string URL, int version)
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

        public static string GetLatestUpdateVersionString(string URL, int version)
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

        public static int GetVersion()
        {
            return int.Parse(JaLoaderVersion.Replace(".", ""));
        }

        public static string GetVersionString()
        {
            if (IsPreReleaseVersion)
                return $"Pre-Release {JaLoaderVersion}";

            return JaLoaderVersion;
        }

        public static bool IsNewerThan(string version)
        {
            var versionSpecified = int.Parse(version.Replace(".", ""));
            var currentVersion = int.Parse(GetVersionString().Replace("Pre-Release ", "").Replace(".", ""));

            if (currentVersion > versionSpecified) return true;
            else return false;
        }

        private static void SetVersionRegistryKey()
        {
            RegistryKey parentKey = Registry.CurrentUser;

            RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

            RegistryKey jalopyKey = softwareKey?.OpenSubKey("Jalopy", true);

            jalopyKey?.SetValue("JaLoaderVersion", GetVersion().ToString(), RegistryValueKind.String);
        }

        private static void SetUpdateCheckRegistryKey()
        {
            RegistryKey parentKey = Registry.CurrentUser;

            RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

            RegistryKey jalopyKey = softwareKey?.OpenSubKey("Jalopy", true);

            jalopyKey?.SetValue("LastUpdateCheck", DateTime.Now.ToString(), RegistryValueKind.String);
        }

        private static void GetUpdateCheckRegistryKey()
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

        internal static void ReadSettings()
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

        private static void Load()
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

        public static void SaveSettings(bool includeDisabledMods = true)
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
                for (int i = 0; i < ModLoader.Instance.disabledMods.ToArray().Length; i++)
                {
                    var mod = ModLoader.Instance.disabledMods.ToArray()[i];

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
        //[SerializeField] public static bool FixItemsFalilngBehindShop = true;
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
