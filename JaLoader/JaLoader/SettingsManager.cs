using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Microsoft.Win32;
using System;

namespace JaLoader
{
    class SettingsManager : MonoBehaviour
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
        }

        #endregion

        [SerializeField] private Settings _settings = new Settings();

        private static readonly string JaLoaderVersion = "1.1.4";
        public static readonly bool IsPreReleaseVersion = false;
        public string ModFolderLocation { get; private set; }

        public bool SkipLanguage;
        public bool DebugMode;
        public bool DisableUncle;
        public bool HideModFolderLocation;
        public bool DisableMenuMusic;
        public int MenuMusicVolume;
        public bool UseExperimentalCharacterController;
        public bool UseCustomSongs;
        public ConsolePositions ConsolePosition;
        public ConsoleModes ConsoleMode;
        public LicensePlateStyles ChangeLicensePlateText;
        public bool UseDiscordRichPresence;
        public string LicensePlateText;
        public bool ShowFPSCounter;
        public bool EnableJaDownloader;

        public bool updateAvailable;

        public bool loadedFirstTime;
        public bool selectedLanguage;

        public List<string> DisabledMods = new List<string>();

        private readonly ModLoader modLoaderReference = ModLoader.Instance;

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
            DisabledMods = _settings.DisabledMods;
            ChangeLicensePlateText = _settings.ChangeLicensePlateText;
            LicensePlateText = _settings.LicensePlateText;
            UseDiscordRichPresence = _settings.UseDiscordRichPresence;
            ShowFPSCounter = _settings.ShowFPSCounter;
            EnableJaDownloader = _settings.EnableJaDownloader;

            EventsManager.Instance.OnSettingsLoad();
        }

        public void SaveSettings()
        {
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

            _settings.ConsolePosition = ConsolePosition;
            _settings.ConsoleMode = ConsoleMode;
            _settings.ChangeLicensePlateText = ChangeLicensePlateText;
            _settings.LicensePlateText = LicensePlateText;
            _settings.UseDiscordRichPresence = UseDiscordRichPresence;
            _settings.ShowFPSCounter = ShowFPSCounter;
            _settings.EnableJaDownloader = EnableJaDownloader;

            for (int i = 0; i < modLoaderReference.disabledMods.ToArray().Length; i++)
            {
                DisabledMods.Add($"{modLoaderReference.disabledMods.ToArray()[i].ModAuthor}_{modLoaderReference.disabledMods.ToArray()[i].ModID}_{modLoaderReference.disabledMods.ToArray()[i].ModName}");
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
        [SerializeField] public bool UseEnhancedMovement = false;
        [SerializeField] public LicensePlateStyles ChangeLicensePlateText = LicensePlateStyles.None;
        [SerializeField] public string LicensePlateText = "";
        [SerializeField] public bool UseDiscordRichPresence = true;
        [SerializeField] public bool ShowFPSCounter = false;
        [SerializeField] public bool EnableJaDownloader = false;
    }
}
