using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace JaLoader.Common
{
    public enum WhenToInit
    {
        InMenu,
        InGame
    }

    public static class Constants
    {
        public const string JaLoaderGitHubLink = "https://github.com/theLeaxx/JaLoader";
    }

    public static class JaLoaderSettings
    {
        public static string JaLoaderVersion { get; internal set; }
        public static bool IsPreReleaseVersion { get; internal set; }
        public const string JaLoaderGitHubLink = "https://github.com/theLeaxx/JaLoader";
        public static string ModFolderLocation { get; internal set; }

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
        public static bool FixItemsFallingBehindShop;
        public static bool FixBorderGuardsFlags;
        public static bool UpdateAvailable;
        public static bool LoadedFirstTime;
        public static bool SelectedLanguage;

        public static List<string> DisabledMods = new List<string>();
        public static List<string> DontShowAgainNotices = new List<string>();

        internal static void CompareAssemblyVersion()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var targetAssembly = assemblies.FirstOrDefault(a => a.GetName().Name == "JaLoader" || a.GetName().Name == "JaLoaderClassic");

            Version version = targetAssembly.GetName().Version;

            string dllVersion = $"{version.Major}.{version.Minor}.{version.Build}";

            if (dllVersion != JaLoaderVersion)
                RuntimeVariables.Logger.LogWarning($"JaLoader version mismatch! Expected: {JaLoaderVersion}, Found: {dllVersion}");
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
        }

        internal static void LoadSettings(SerializableJaLoaderSettings _settings)
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
            FixBorderGuardsFlags = _settings.FixBorderGuardsFlags;
            DontShowAgainNotices = _settings.DontShowAgainNotices;
        }

        internal static void SaveSettings(SerializableJaLoaderSettings _settings)
        {
            _settings.ConsolePosition = ConsolePosition;
            _settings.ConsoleMode = ConsoleMode;
            _settings.SkipLanguageSelector = SkipLanguage;
            _settings.DisableUncle = DisableUncle;
            _settings.DebugMode = DebugMode;
            _settings.HideModFolder = HideModFolderLocation;
            _settings.DisableMenuMusic = DisableMenuMusic;
            _settings.MenuMusicVolume = MenuMusicVolume;
            _settings.UseEnhancedMovement = UseExperimentalCharacterController;
            _settings.UseCustomSongs = UseCustomSongs;
            _settings.CustomSongsBehaviour = CustomSongsBehaviour;
            _settings.RadioAds = RadioAds;
            _settings.DisabledMods = DisabledMods;
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
            _settings.FixBorderGuardsFlags = FixBorderGuardsFlags;
            _settings.DontShowAgainNotices = DontShowAgainNotices;  
        }

        public static bool IsNewerThan(string version)
        {
            var versionSpecified = int.Parse(version.Replace(".", ""));
            var currentVersion = int.Parse(GetVersionString().Replace("Pre-Release ", "").Replace(".", ""));

            if (currentVersion > versionSpecified) return true;
            else return false;
        }

        internal static void SetVersionRegistryKey()
        {
            RegistryKey parentKey = Registry.CurrentUser;

            RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

            RegistryKey jalopyKey = softwareKey?.OpenSubKey("Jalopy", true);

            jalopyKey?.SetValue("JaLoaderVersion", GetVersion().ToString(), RegistryValueKind.String);
        }

        internal static void SetUpdateCheckRegistryKey()
        {
            RegistryKey parentKey = Registry.CurrentUser;

            RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

            RegistryKey jalopyKey = softwareKey?.OpenSubKey("Jalopy", true);

            jalopyKey?.SetValue("LastUpdateCheck", DateTime.Now.ToString(), RegistryValueKind.String);
        }

        internal static void GetUpdateCheckRegistryKey()
        {
            RegistryKey parentKey = Registry.CurrentUser;

            RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

            RegistryKey jalopyKey = softwareKey?.OpenSubKey("Jalopy", true);

            if (jalopyKey != null && jalopyKey.GetValue("LastUpdateCheck") != null)
            {
                UpdateUtils.lastUpdateCheck = DateTime.Parse(jalopyKey.GetValue("LastUpdateCheck").ToString());
            }
            else
            {
                SetUpdateCheckRegistryKey();
                UpdateUtils.lastUpdateCheck = DateTime.Now;
            }
        }
    }

    [Serializable]
    public class SerializableJaLoaderSettings
    {
        public bool SkipLanguageSelector = true;
        public bool DebugMode = false;
        public ConsolePositions ConsolePosition = ConsolePositions.BottomLeft;
        public ConsoleModes ConsoleMode = ConsoleModes.Default;
        public bool DisableMenuMusic = false;
        public int MenuMusicVolume = 50;
        public bool HideModFolder = false;
        public bool DisableUncle = false;
        public bool UseCustomSongs = true;
        public CustomSongsBehaviour CustomSongsBehaviour = CustomSongsBehaviour.Replace;
        public bool RadioAds = true;
        public bool UseEnhancedMovement = false;
        public LicensePlateStyles ChangeLicensePlateText = LicensePlateStyles.None;
        public string LicensePlateText = "";
        public bool UseDiscordRichPresence = true;
        public bool ShowFPSCounter = false;
        public bool EnableJaDownloader = false;
        public bool AskedAboutJaDownloader = false;
        public UpdateCheckModes UpdateCheckMode = UpdateCheckModes.Daily;
        public bool FixLaikaShopMusic = true;
        public MirrorDistances MirrorDistances = MirrorDistances.m1000;
        public CursorMode CursorMode = CursorMode.Default;
        public bool ShowDisabledMods = true;
        public bool FixBorderGuardsFlags = true;
        public string AppliedPaintJobName = "";

        public List<string> DisabledMods = new List<string>();
        public List<string> DontShowAgainNotices = new List<string>();
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

    public enum ConsolePositions
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public enum ConsoleModes
    {
        Default,
        ErrorsWarnings,
        Errors,
        Disabled
    }

    public enum LicensePlateStyles
    {
        None,
        Default,
        DiplomaticRed,
        DiplomaticBlue,
        WhiteOnBlack
    }
}
