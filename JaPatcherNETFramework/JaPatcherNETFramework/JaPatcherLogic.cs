using Microsoft.Win32;
//using Newtonsoft.Json;
//using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.PerformanceData;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace JaPatcherNETFramework
{
    public class JaPatcherLogic
    {
        internal JaPatcherWindow Window;
        internal string Version = "1.0";

        internal string HelpLink = "https://github.com/theLeaxx/JaLoader/wiki/Installing-JaLoader-via-JaPatcher";
        internal string LatestRelease = "https://github.com/theLeaxx/JaLoader/releases/latest";

        internal string GamePath { get; private set; } = "";
        internal string DocumentsModsLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Jalopy\Mods";
        internal string GameFolderModsLocation = "";
        internal string CustomModsLocation = "";

        internal string CurrentDirectory;

        internal ModLocation SelectedModLocation = ModLocation.Documents;
        internal PatchedStatus PatchedStatus;

        internal bool IsLinux;

        internal readonly Dictionary<string, List<string>> requiredFiles = new Dictionary<string, List<string>>()
        {
            { "Managed", new List<string>()
            {
                "0Harmony.dll",
                "HarmonyXInterop.dll",
                "Mono.Cecil.dll",
                "Mono.Cecil.Mdb.dll",
                "Mono.Cecil.Pdb.dll",
                "Mono.Cecil.Rocks.dll",
                "ValueTupleBridge.dll",
                "MonoMod.RuntimeDetour.dll",
                "MonoMod.ILHelpers.dll",
                "MonoMod.Utils.dll",
                "JaLoader.dll",
                "JaPreLoader.dll",
                "JaLoaderClassic.dll",
                "JaLoader.Common.dll",
                "JaLoader.xml",
                "JaLoaderClassic.xml",
                "JaLoader.Common.xml",
                "System.Web.Extensions.dll",
                "NLayer.dll"
            }
            },
            { "Main", new List<string>()
            {
                "winhttp.dll",
                "doorstop_config.ini",
                "discord_game_sdk.dll",
                "Newtonsoft.Json.dll",
                "JaDownloader.exe",
                "JaDownloaderSetup.exe"
            }
            },
            { "Required", new List<string>()
            {
                "JaLoader_UI.unity3d",
                "JaLoader_AdjustmentsEditor.unity3d",
                "defaultPaintJobTexture.png",
                "defaultExtraTexture.png"
            }
            },
            { "Updater", new List<string>()
            {
                "JaUpdater.exe"
            }
            }
        };

        internal readonly string[] CommonGameLocations = new string[]
        {
            @"C:\Program Files (x86)\Steam\steamapps\common\Jalopy\Jalopy.exe",
            @"C:\Program Files\Steam\steamapps\common\Jalopy\Jalopy.exe",
            @"D:\Program Files (x86)\Steam\steamapps\common\Jalopy\Jalopy.exe",
            @"D:\Program Files\Steam\steamapps\common\Jalopy\Jalopy.exe",
            @"E:\Program Files (x86)\Steam\steamapps\common\Jalopy\Jalopy.exe",
            @"E:\Program Files\Steam\steamapps\common\Jalopy\Jalopy.exe",
            @"F:\Program Files (x86)\Steam\steamapps\common\Jalopy\Jalopy.exe",
            @"F:\Program Files\Steam\steamapps\common\Jalopy\Jalopy.exe",
            @"C:\SteamLibrary\steamapps\common\Jalopy\Jalopy.exe",
            @"D:\SteamLibrary\steamapps\common\Jalopy\Jalopy.exe",
            @"E:\SteamLibrary\steamapps\common\Jalopy\Jalopy.exe",
            @"F:\SteamLibrary\steamapps\common\Jalopy\Jalopy.exe"
        };

        internal string GetSubDirectory(string folder, bool inJalopyFolder)
        {
            if(inJalopyFolder && string.IsNullOrWhiteSpace(GamePath))
                return "";

            var basePath = inJalopyFolder ? GamePath : CurrentDirectory;
            switch (folder)
            {
                case "Managed":
                    return Path.Combine(basePath, inJalopyFolder ? @"Jalopy_Data\Managed" : @"Assets\Managed");
                case "Main":
                    return Path.Combine(basePath, inJalopyFolder ? @"" : @"Assets\Main");
                case "Required":
                    return Path.Combine(basePath, @"Assets\Required");
                case "Updater":
                    return basePath;

                default:
                    return basePath;
            }
        }

        internal bool CheckForMissingFiles(bool inJalopyFolder)
        {
            foreach (var pair in requiredFiles)
            {
                if (inJalopyFolder && pair.Key == "Required")
                    continue;

                var subDirectory = GetSubDirectory(pair.Key, inJalopyFolder);

                foreach (var file in pair.Value)
                {
                    if(!File.Exists(Path.Combine(subDirectory, file)))
                    {
                        if (!inJalopyFolder)
                            Window.WarnAboutMissingFiles(file, pair.Key);
                        else
                            CheckForIncompleteOrOutdatedInstall();
                        return true;
                    }
                }
            }

            return false;
        }

        internal void SelectDocumentsFolder()
        {
            SelectModFolder(ModLocation.Documents);
        }

        internal void SelectModFolder(ModLocation location, string customPath = "")
        {
            switch (location)
            {
                case ModLocation.Documents:
                    SelectedModLocation = ModLocation.Documents;
                    break;

                case ModLocation.GameFolder:
                    SelectedModLocation = ModLocation.GameFolder;
                    break;

                case ModLocation.Custom:
                    SelectedModLocation = ModLocation.Custom;
                    CustomModsLocation = GetValidDirectory(customPath);
                    break;

                default:
                    break;
            }

            if (GamePath != "")
                SaveOverallData(GamePath, SelectedModLocation, CustomModsLocation, GameFolderModsLocation);

            PrepareModFolder();
        }

        internal string ReturnPathOfSelectedModsFolder()
        {
            switch (SelectedModLocation)
            {
                case ModLocation.Documents:
                    return DocumentsModsLocation;
                case ModLocation.GameFolder:
                    return GameFolderModsLocation;
                case ModLocation.Custom:
                    return CustomModsLocation;

                default:
                    return DocumentsModsLocation;
            }
        }

        internal void SelectGameFolderModsFolder()
        {
            SelectModFolder(ModLocation.GameFolder);
        }

        internal void SelectCustomModsFolder(string customPath)
        {
            SelectModFolder(ModLocation.Custom, customPath);
        }

        internal void CheckForIncompleteOrOutdatedInstall()
        {
            var installedVersion = GetJaLoaderVersionAsInt(true);
            var currentVersion = GetJaLoaderVersionAsInt(false);

            if (installedVersion == currentVersion)
                PatchedStatus = PatchedStatus.PatchedIncomplete;
            else if (installedVersion < currentVersion && installedVersion != 0)
                PatchedStatus = PatchedStatus.PatchedOutdated;
            else if (installedVersion == 0)
                PatchedStatus = PatchedStatus.NotPatched;
        }

        internal void CreateDocumentsModsFolderIfMissing()
        {
            if (!Directory.Exists(DocumentsModsLocation))
                Directory.CreateDirectory(DocumentsModsLocation);
        }

        internal void StartupChecks()
        {
            CheckForMissingFiles(false);

            GetJaLoaderVersionFromDLL(false);

            IsLinux = LinuxUtils.IsLinux();

            if (IsLinux)
                DocumentsModsLocation = LinuxUtils.GetLinuxDocumentsModsFolder();
            else
                CreateDocumentsModsFolderIfMissing();

            SetLoadedData();
            if(!string.IsNullOrEmpty(GamePath))
                Window.CheckPatchedStatus(GamePath);
        }

        internal void SetLoadedData()
        {
            var data = ReadOverallData();
            if (data.Item1 == "")
                return;

            GamePath = data.Item1;
            SelectedModLocation = data.Item2;
            CustomModsLocation = data.Item3;
            GameFolderModsLocation = data.Item4;
        }

        internal bool IsOutdatedInstall()
        {
            var installedVersion = GetJaLoaderVersionAsInt(true);
            var currentVersion = GetJaLoaderVersionAsInt(false);

            if (installedVersion < currentVersion)
                return true;

            return false;
        }

        internal static (string, ModLocation, string, string) ReadOverallData()
        {
            var gamePath = "";
            var modsPath = ModLocation.Documents;
            var customModsPath = "";
            var gameModsPath = "";

            RegistryKey parentKey = Registry.CurrentUser;

            RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

            RegistryKey jalopyKey = softwareKey?.OpenSubKey("Jalopy", true);

            if (jalopyKey != null && jalopyKey.GetValue("JalopyPath") != null)
                gamePath = jalopyKey.GetValue("JalopyPath").ToString();

            if (jalopyKey != null && jalopyKey.GetValue("ModsLocation") != null)
                modsPath = (ModLocation)(int)jalopyKey.GetValue("ModsLocation");

            if (jalopyKey != null && jalopyKey.GetValue("CustomLocation") != null)
                customModsPath = jalopyKey.GetValue("CustomLocation").ToString();

            if (jalopyKey != null && jalopyKey.GetValue("GameLocation") != null)
                gameModsPath = jalopyKey.GetValue("GameLocation").ToString();

            return (gamePath, modsPath, customModsPath, gameModsPath);
        }

        private void SaveOverallData(string gamePath, ModLocation modsPath, string customModsPath, string gameModsPath)
        {
            RegistryKey parentKey = Registry.CurrentUser;

            RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

            RegistryKey jalopyKey = softwareKey?.CreateSubKey("Jalopy", true);

            jalopyKey?.SetValue("ModsLocation", (int)modsPath, RegistryValueKind.DWord);
            jalopyKey?.SetValue("JalopyPath", gamePath, RegistryValueKind.String);

            jalopyKey?.SetValue("GameLocation", gameModsPath, RegistryValueKind.String);
            jalopyKey?.SetValue("CustomLocation", customModsPath, RegistryValueKind.String);

            SaveDataAsJSONInGameFolder();
        }


        internal bool ReturnPatchedStatus(string path)
        {
            SetGamePath(path);

            if (CheckForMissingFiles(true) == false)
            {
                if (IsOutdatedInstall())
                    PatchedStatus = PatchedStatus.PatchedOutdated;
                else
                    PatchedStatus = PatchedStatus.Patched;
            }

            GameFolderModsLocation = Path.Combine(GamePath, @"Jalopy_Data\Mods");

            SaveOverallData(GamePath, SelectedModLocation, CustomModsLocation, GameFolderModsLocation);

            return true;
        }

        internal void InstallOrFixJaLoader()
        {
            foreach (var pair in requiredFiles)
            {
                if (pair.Key == "Required")
                    continue;

                var subDirectoryGame = GetSubDirectory(pair.Key, true);
                var subDirectoryLocal = GetSubDirectory(pair.Key, false);

                foreach (var file in pair.Value)
                    File.Copy(Path.Combine(subDirectoryLocal, file), Path.Combine(subDirectoryGame, file), true);
            }

            if(!Directory.Exists(Path.Combine(GamePath, "Songs")))
                Directory.CreateDirectory(Path.Combine(GamePath, "Songs"));

            PatchedStatus = PatchedStatus.Patched;
            Window.UpdateStatusTextAndInstallButton();
            PrepareModFolder();
            MessageBox.Show("JaLoader has been installed/updated successfully!", "JaPatcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal void PrepareModFolder()
        {
            var selectedPath = ReturnPathOfSelectedModsFolder();
            var requiredPath = Path.Combine(selectedPath, "Required");
            if (!Directory.Exists(requiredPath))
                Directory.CreateDirectory(requiredPath);

            if (!Directory.Exists(Path.Combine(selectedPath, "Assemblies")))
                Directory.CreateDirectory(Path.Combine(selectedPath, "Assemblies"));

            if (!Directory.Exists(Path.Combine(selectedPath, "Assets")))
                Directory.CreateDirectory(Path.Combine(selectedPath, "Assets"));

            if (!Directory.Exists(Path.Combine(selectedPath, "CachedImages")))
                Directory.CreateDirectory(Path.Combine(selectedPath, "CachedImages"));

            foreach (var pair in requiredFiles)
            {
                if(pair.Key != "Required")
                    continue;

                foreach(var file in pair.Value)
                {
                    var fullPath = Path.Combine(requiredPath, file);
                    File.Copy(Path.Combine(GetSubDirectory("Required", false), file), fullPath, true);
                }
            }
        }

        internal void UninstallJaLoader()
        {
            foreach (var pair in requiredFiles)
            {
                if(pair.Key == "Required")
                    continue;

                var subDirectoryGame = GetSubDirectory(pair.Key, true);

                foreach (var file in pair.Value)
                {
                    var fullPath = Path.Combine(subDirectoryGame, file);
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }
            }

            PatchedStatus = PatchedStatus.NotPatched;
            Window.UpdateStatusTextAndInstallButton();
        }

        internal void SetGamePath(string path)
        {
            GamePath = GetValidDirectory(path);
        }

        // Regular way through VS crashes on Wine
        internal static void LoadIcon(Form form)
        {
            try
            {
                var assembly = form.GetType().Assembly;
                using (var stream = assembly.GetManifestResourceStream("JaPatcherNETFramework.Resources.Icon.ico"))
                {
                    if (stream != null)
                    {
                        form.Icon = new Icon(stream);
                    }
                    else
                        MessageBox.Show("Failed to load application icon from resources.", "JaPatcher - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception)
            {

            }
        }

        internal void CheckForUpdates()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.Headers.Add("User-Agent", "JaPatcher-App");

                    string url = "https://api.github.com/repos/theLeaxx/JaLoader/releases/latest";
                    string json = client.DownloadString(url);

                    var serializer = new JavaScriptSerializer();
                    var release = serializer.Deserialize<dynamic>(json);

                    // 1. Get the TagName
                    string latest = release["tag_name"];
                    if (latest.Contains("Pre-Release")) return;

                    // 2. Get the Changelog (Body)
                    string changelog = release["body"];
                    bool installable = !changelog.Contains("This update REQUIRES re-downloading of JaPatcher");

                    int tagInt = int.Parse(latest.Replace(".", ""));
                    int currentInt = GetJaLoaderVersionAsInt(false);
                    if (tagInt > currentInt)
                    {
                        var result = MessageBox.Show($"A new version of JaLoader is available: {latest}\nYou are currently using version: {GetJaLoaderVersionFromDLL(false)}\n\nDo you want to update now?", "JaPatcher - Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            if (installable)
                            {
                                Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "JaUpdater.exe"), $"{Directory.GetCurrentDirectory()} Patcher");
                                System.Windows.Forms.Application.Exit();
                            }
                            else
                            {
                                MessageBox.Show("This update requires manual updating of JaPatcher. The JaLoader GitHub page will now open, to manually download the latest version of JaPatcher.", "JaPatcher - Update Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                Process.Start(LatestRelease);
                            }
                        }
                    }
                    else
                        MessageBox.Show("You are using the latest version!", "JaPatcher - No Update Available", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to check for updates. Please check your internet connection and try again.", "JaPatcher - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        internal string GetValidDirectory(string inputPath)
        {
            if (string.IsNullOrWhiteSpace(inputPath) || string.IsNullOrEmpty(inputPath)) return null;

            try
            {
                string trimmedPath = inputPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                if (Directory.Exists(trimmedPath))
                    return trimmedPath;

                if (File.Exists(trimmedPath))
                    return Path.GetDirectoryName(trimmedPath);

                if (Path.HasExtension(trimmedPath))
                    return Path.GetDirectoryName(trimmedPath);
            }
            catch (Exception)
            {
            }
            // /home/leaxx/.gmndsmdfs/jalopy
            return inputPath;
        }

        internal int GetJaLoaderVersionAsInt(bool gameDLL)
        {
            if(GetJaLoaderVersionFromDLL(gameDLL) == "Unknown")
                return 0;

            return int.Parse(GetJaLoaderVersionFromDLL(gameDLL).Replace(".", ""));
        }

        internal string GetJaLoaderVersionFromDLL(bool gameDLL)
        {
            string toReturn = "Unknown";

            var subDir = GetSubDirectory("Managed", gameDLL);

            if(gameDLL && string.IsNullOrWhiteSpace(GamePath))
                return toReturn;

            var path = Path.Combine(subDir, @"JaLoader.dll");

            if(!File.Exists(path))
                return toReturn;

            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(path);
            toReturn = versionInfo.FileVersion;

            return toReturn;
        }

        internal void OpenLink()
        {
            Process.Start(HelpLink);
        }

        internal void SaveDataAsJSONInGameFolder()
        {
            var settings = LoadDataFromJSONInGameFolder();
            settings.ModsLocation = ReturnPathOfSelectedModsFolder();

            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string json = serializer.Serialize(settings);

                string filePath = Path.Combine(GamePath, "JaSettings.json");
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings to JaSettings.json in the game folder.\nException: {ex.Message}\nPlease report this issue on the JaLoader GitHub page.", "JaPatcher - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        internal Settings LoadDataFromJSONInGameFolder()
        {
            var path = Path.Combine(GamePath, "JaSettings.json");
            if (!File.Exists(path))
                return new Settings();

            try
            {
                var json = File.ReadAllText(Path.Combine(GamePath, "JaSettings.json"));
                return new JavaScriptSerializer().Deserialize<Settings>(json) ?? new Settings();
            }
            catch (Exception)
            {
                return new Settings();
            }
        }
    }

    [Serializable]
    public class Settings
    {
        public string ModsLocation = "";
        public string JaLoaderVersion = "";
        public string LastUpdateCheck = "";
    }

    public enum ModLocation
    {
        Documents,
        GameFolder,
        Custom
    }

    public enum PatchedStatus
    {
        NotPatched,
        Patched,
        PatchedOutdated,
        PatchedIncomplete
    }
}
