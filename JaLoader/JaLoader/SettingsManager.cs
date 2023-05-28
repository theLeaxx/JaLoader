using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
        }

        #endregion

        [SerializeField] private Settings _settings = new Settings();
        [SerializeField] private ModsLocation _location = new ModsLocation();

        private static readonly string JaLoaderVersion = "1.0.1";
        public static readonly bool IsPreReleaseVersion = false;
        public string ModFolderLocation { get; private set; }

        public bool SkipLanguage;
        public bool DebugMode;
        public bool DisableUncle;
        public bool HideModFolderLocation;
        public bool DisableMenuMusic;
        public int MenuMusicVolume;
        public bool UseExperimentalCharacterController;
        public ConsolePositions ConsolePosition;
        public ConsoleModes ConsoleMode;

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

            else return JaLoaderVersion;
        }

        public bool IsNewerThan(string version)
        {
            var versionSpecified = int.Parse(version.Replace(".", ""));
            var currentVersion = int.Parse(GetVersionString().Replace(".", ""));

            if (currentVersion > versionSpecified) return true;
            else return false;
        }

        private void ReadSettings()
        {
            if (File.Exists(Path.Combine(Application.dataPath, @"..\ModsLocation.json")))
            {
                string json = File.ReadAllText(Path.Combine(Application.dataPath, @"..\ModsLocation.json"));
                _location = JsonUtility.FromJson<ModsLocation>(json);

                ModFolderLocation = _location.ModFolderLocation;
            }
            else
            {
                if (!Directory.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, @"..\Mods"))))
                {
                    Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Application.dataPath, @"..\Mods")));
                }

                ModFolderLocation = _location.ModFolderLocation = Path.GetFullPath(Path.Combine(Application.dataPath, @"..\Mods"));
            }

            if (File.Exists(Path.Combine(Application.persistentDataPath, @"JaConfig.json")))
            {
                string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, @"JaConfig.json"));
                _settings = JsonUtility.FromJson<Settings>(json);

                ConsolePosition = _settings.ConsolePosition;
                ConsoleMode = _settings.ConsoleMode;

                SkipLanguage = _settings.SkipLanguageSelector;
                DisableUncle = _settings.DisableUncle;
                DebugMode = _settings.DebugMode;
                HideModFolderLocation = _settings.HideModFolder;
                DisableMenuMusic = _settings.DisableMenuMusic;
                MenuMusicVolume = _settings.MenuMusicVolume;
                UseExperimentalCharacterController = _settings.UseExperimentalCharacterController;
                DisabledMods = _settings.DisabledMods;
            }
            else
            {
                ConsolePosition = _settings.ConsolePosition;
                ConsoleMode = _settings.ConsoleMode;

                SkipLanguage = _settings.SkipLanguageSelector;
                DisableUncle = _settings.DisableUncle;
                DebugMode = _settings.DebugMode;
                HideModFolderLocation = _settings.HideModFolder;
                DisableMenuMusic = _settings.DisableMenuMusic;
                MenuMusicVolume = _settings.MenuMusicVolume;
                UseExperimentalCharacterController = _settings.UseExperimentalCharacterController;
                DisabledMods = _settings.DisabledMods;

                File.WriteAllText(Path.Combine(Application.persistentDataPath, @"JaConfig.json"), JsonUtility.ToJson(_settings, true));
                return;
            }
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
            _settings.UseExperimentalCharacterController = UseExperimentalCharacterController;
            _settings.DisabledMods = DisabledMods;

            _settings.ConsolePosition = ConsolePosition;
            _settings.ConsoleMode = ConsoleMode;

            for (int i = 0; i < modLoaderReference.disabledMods.ToArray().Length; i++)
            {
                DisabledMods.Add($"{modLoaderReference.disabledMods.ToArray()[i].ModAuthor}_{modLoaderReference.disabledMods.ToArray()[i].ModID}_{modLoaderReference.disabledMods.ToArray()[i].ModName}");
            }

            _settings.DisabledMods = DisabledMods;

            File.WriteAllText(Path.Combine(Application.persistentDataPath, @"JaConfig.json"), JsonUtility.ToJson(_settings, true));
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
        [SerializeField] public bool UseExperimentalCharacterController = false;
    }

    [System.Serializable]
    public class ModsLocation
    {
        [SerializeField] public string ModFolderLocation = "";
    }
}
