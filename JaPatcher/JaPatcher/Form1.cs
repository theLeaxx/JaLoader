#pragma warning disable CS8601
#pragma warning disable CS8604
#pragma warning disable CS8600
#pragma warning disable CS8603

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using Octokit;
using System.Diagnostics;
using Microsoft.Win32;
using Application = System.Windows.Forms.Application;
using System.Drawing.Text;

namespace JalopyModLoader
{
    public partial class Form1 : Form
    {
        private readonly Settings _settings = new();
        private readonly Save _save = new();

        #region Dark Mode
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
        private static extern bool IsSystemUsingDarkmode();

        private static bool IsWindows10OrGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }

        private static bool UseDarkTitlebar(IntPtr handle, bool enabled)
        {
            if (IsWindows10OrGreater(17763))
            {
                var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }

            return false;
        }

        private void SetDarkMode()
        {
            Color dark = Color.FromArgb(255, 30, 30, 30);

            UseDarkTitlebar(Handle, true);

            BackColor = dark;
            groupBox1.BackColor = dark;
            groupBox2.BackColor = dark;
            locateFolderButton.BackColor = dark;
            installButton.BackColor = dark;
            launchButton.BackColor = dark;
            updateButton.BackColor = dark;

            groupBox1.ForeColor = Color.White;
            groupBox2.ForeColor = Color.White;
            documentsButton.ForeColor = Color.White;
            installedTextValue.ForeColor = Color.White;
            gameFolderModsButton.ForeColor = Color.White;
            customModsLocationButton.ForeColor = Color.White;
            locateFolderButton.ForeColor = Color.White;
            installButton.ForeColor = Color.White;
            launchButton.ForeColor = Color.White;
            updateButton.ForeColor = Color.White;
        }
        #endregion

        private string gamePath = "";
        private readonly string documentsModsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Jalopy\Mods");
        private string gameFolderModsPath = "";
        private string customModsPath = "";

        private string currentModPath = "";

        private bool installed = false;
        private bool updateRequired = false;

        private readonly string[] requiredManagedFiles = new string[] {
            "0Harmony.dll",
            "HarmonyXInterop.dll",
            "NAudio.dll",
            "Mono.Cecil.dll",
            "Mono.Cecil.Mdb.dll",
            "Mono.Cecil.Pdb.dll",
            "Mono.Cecil.Rocks.dll",
            "MonoMod.Backports.dll",
            "MonoMod.RuntimeDetour.dll",
            "MonoMod.Utils.dll",
            "JaLoader.dll",
            "JaPreLoader.dll",
            "JaLoader.xml",
            "MonoMod.ILHelpers.dll"
        };

        private readonly string winhttpDLL = Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Main\winhttp.dll");
        private readonly string doorstopConfig = Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Main\doorstop_config.ini");
        private readonly string discordDLL = Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Main\discord_game_sdk.dll");

        private readonly string jsonDLL = Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Main\Newtonsoft.Json.dll");
        private readonly string jaDownloader = Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Main\JaDownloader.exe");
        private readonly string jaDownloaderSetup = Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Main\JaDownloaderSetup.exe");

        private readonly string assetBundle = Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Required\JaLoader_UI.unity3d");

        private readonly string updater = Path.Combine(Directory.GetCurrentDirectory(), "JaUpdater.exe");
        private readonly string version = "3.5.3";

        private bool canClickCustom = false;

        public Form1()
        {
            InitializeComponent();

            if (IsSystemUsingDarkmode())
            {
                SetDarkMode();
            }

            bool anyMissing = false;
            foreach(string file in requiredManagedFiles)
            {
                if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Managed", file)))
                {
                    anyMissing = true;
                    break;
                }
            }

            if (anyMissing || !File.Exists(winhttpDLL) || !File.Exists(doorstopConfig) || !File.Exists(assetBundle) || !File.Exists(discordDLL) || !File.Exists(jaDownloader) || !File.Exists(jaDownloaderSetup) || !File.Exists(jsonDLL))
            {
                MessageBox.Show("Please extract all of the contents from the archive!", "DLLs not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            CheckForUpdates();

            if (ReadSave() != "")
            {
                //_save.LastSelectedFolder = ReadString(Path.Combine(Directory.GetCurrentDirectory(), @"save.json"));

                CheckFolder(ReadSave());
            }

            documentsModsText.Text = documentsModsPath;
        }

        public async void CheckForUpdates()
        {
            var client = new GitHubClient(new ProductHeaderValue("JaLoader-JaPatcher"));

            var releases = await client.Repository.Release.GetLatest("theLeaxx", "JaLoader");

            var latest = releases.TagName;

            if (latest.Contains("Pre-Release")) return;

            int tagInt = int.Parse(latest.Replace(".", ""));
            int currentInt = int.Parse(version.Replace(".", ""));

            if (tagInt > currentInt)
            {
                updateButton.Visible = true;
                installedTextValue.ForeColor = Color.Orange;
                updateRequired = true;

                if (installed)
                    installedTextValue.Text = "Yes, Update Available";
                else
                {
                    installedTextValue.ForeColor = Color.OrangeRed;
                    installedTextValue.Text = "No, Update Available";
                }

                TellAboutUpdate(latest);
            }
        }

        private static void TellAboutUpdate(string tagName)
        {
            var message = MessageBox.Show($"A new version of JaPatcher is available ({tagName})! Would you like to update now?", "JaPatcher", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (message == DialogResult.Yes)
            {
                UpdateFiles();
                //Process.Start("explorer", "https://github.com/theLeaxx/JaLoader/releases/latest");
            }
        }

        public static string ReadString(string path)
        {
            if (!File.Exists(path))
                return "";

            StreamReader file = File.OpenText(path);
            JsonTextReader reader = new(file);

            JObject o2 = (JObject)JToken.ReadFrom(reader);
            string toReturn = (string)o2.First;

            reader.Close();
            file.Close();

            return toReturn;
        }

        private static void AddRegistryKeys(string modsLocation)
        {
            RegistryKey parentKey = Registry.CurrentUser;

            RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

            RegistryKey jalopyKey = softwareKey?.CreateSubKey("Jalopy", true);

            jalopyKey?.SetValue("ModsLocation", modsLocation, RegistryValueKind.String);
        }

        private static void AddJalopyPathKey(string path)
        {
            RegistryKey parentKey = Registry.CurrentUser;

            RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

            RegistryKey jalopyKey = softwareKey?.CreateSubKey("Jalopy", true);

            jalopyKey?.SetValue("JalopyPath", path, RegistryValueKind.String);
        }

        private static string ReadSave()
        {
            RegistryKey parentKey = Registry.CurrentUser;

            RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

            RegistryKey jalopyKey = softwareKey?.OpenSubKey("Jalopy", true);

            if (jalopyKey != null && jalopyKey.GetValue("JalopyPath") != null)
                return jalopyKey.GetValue("JalopyPath").ToString();
            else
                return "";
        }

        private static string GetModsPath()
        {
            RegistryKey parentKey = Registry.CurrentUser;

            RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

            RegistryKey jalopyKey = softwareKey?.OpenSubKey("Jalopy", true);

            if (jalopyKey != null && jalopyKey.GetValue("ModsLocation") != null)
                return jalopyKey.GetValue("ModsLocation").ToString();
            else
                return "";
        }

        public void ReadModsLocation()
        {
            _settings.ModFolderLocation = GetModsPath();//ReadString(Path.Combine(gamePath, @"ModsLocation.json"));

            if (_settings.ModFolderLocation == documentsModsPath)
            {
                documentsButton.Checked = true;
            }
            else if (_settings.ModFolderLocation == gameFolderModsPath)
            {
                gameFolderModsButton.Checked = true;
            }
            else
            {
                customFolderModsText.Text = _settings.ModFolderLocation;
                customModsLocationButton.Checked = true;
            }

            canClickCustom = true;
        }

        public void Patch()
        {
            File.Copy(winhttpDLL, Path.Combine(gamePath, @"winhttp.dll"), true);
            File.Copy(doorstopConfig, Path.Combine(gamePath, @"doorstop_config.ini"), true);
            File.Copy(discordDLL, Path.Combine(gamePath, @"discord_game_sdk.dll"), true);
            File.Copy(jsonDLL, Path.Combine(gamePath, @"Newtonsoft.Json.dll"), true);
            File.Copy(jaDownloader, Path.Combine(gamePath, @"JaDownloader.exe"), true);
            File.Copy(jaDownloaderSetup, Path.Combine(gamePath, @"JaDownloaderSetup.exe"), true);

            foreach(string file in requiredManagedFiles)
            {
                File.Copy(Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Managed", file), Path.Combine(gamePath, @"Jalopy_Data\Managed", file), true);
            }

            File.Copy(updater, Path.Combine(gamePath, @"JaUpdater.exe"), true);

            if (!Directory.Exists(Path.Combine(currentModPath, "Required")))
                Directory.CreateDirectory(Path.Combine(currentModPath, "Required"));

            if (!Directory.Exists(Path.Combine(currentModPath, "Assemblies")))
                Directory.CreateDirectory(Path.Combine(currentModPath, "Assemblies"));

            if (!Directory.Exists(Path.Combine(currentModPath, "Assets")))
                Directory.CreateDirectory(Path.Combine(currentModPath, "Assets"));

            if (!Directory.Exists(Path.Combine(currentModPath, "CachedImages")))
                Directory.CreateDirectory(Path.Combine(currentModPath, "CachedImages"));

            if (!Directory.Exists(Path.Combine(gamePath, "Songs")))
                Directory.CreateDirectory(Path.Combine(gamePath, "Songs"));

            File.Copy(assetBundle, Path.Combine(currentModPath, @"Required\JaLoader_UI.unity3d"), true);

            _settings.ModFolderLocation = currentModPath;
            //File.WriteAllText(Path.Combine(gamePath, @"ModsLocation.json"), JsonConvert.SerializeObject(_settings, Formatting.Indented));
            AddRegistryKeys(_settings.ModFolderLocation);

            MessageBox.Show("JaLoader successfully installed!", "JaPatcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void Uninstall()
        {
            File.Delete(Path.Combine(gamePath, @"winhttp.dll"));
            File.Delete(Path.Combine(gamePath, @"doorstop_config.ini"));
            File.Delete(Path.Combine(gamePath, @"discord_game_sdk.dll"));
            File.Delete(Path.Combine(gamePath, @"JaUpdater.exe"));
            File.Delete(Path.Combine(gamePath, @"Newtonsoft.Json.dll"));
            File.Delete(Path.Combine(gamePath, @"JaDownloader.exe"));
            File.Delete(Path.Combine(gamePath, @"JaDownloaderSetup.exe"));

            foreach (string file in requiredManagedFiles)
            {
                File.Delete(Path.Combine(gamePath, @"Jalopy_Data\Managed", file));
            }

            if (MessageBox.Show("Would you like to delete the configuration files too?", "JaPatcher", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                //File.Delete(Path.Combine(gamePath, @"ModsLocation.json"));
                //TODO: delete registry keys
                File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"..\LocalLow\MinskWorks\Jalopy\JaConfig.json"));
            }
        }

        private void locateFolderButton_Click(object sender, EventArgs e)
        {
            if (locateFolderDialog.ShowDialog() == DialogResult.OK)
            {
                CheckFolder(locateFolderDialog.FileName);
            }
        }

        private void CheckFolder(string folder)
        {
            _save.LastSelectedFolder = folder;

            groupBox2.Visible = true;
            installButton.Visible = true;

            documentsButton.Enabled = true;
            gameFolderModsButton.Enabled = true;
            customModsLocationButton.Enabled = true;

            gamePath = Path.GetDirectoryName(folder);
            gameFolderModsPath = Path.Combine(gamePath, @"Mods");

            bool allExist = true;
            foreach (string file in requiredManagedFiles)
            {
                if (!File.Exists(Path.Combine(gamePath, @"Jalopy_Data\Managed", file)))
                {
                    allExist = false;
                    break;
                }
            }

            if (allExist && File.Exists(Path.Combine(gamePath, @"winhttp.dll")) && File.Exists(Path.Combine(gamePath, @"doorstop_config.ini")) && File.Exists(Path.Combine(gamePath, @"discord_game_sdk.dll")))
            {
                installed = true;

                if (updateRequired)
                {
                    updateButton.Visible = true;
                    installedTextValue.Text = "Yes, Update Available";
                    installedTextValue.ForeColor = Color.Orange;
                }
                else
                {
                    installButton.Text = "Uninstall";
                    installedTextValue.Text = "Yes";
                    installedTextValue.ForeColor = Color.Green;
                }
            }
            else
            {
                installed = false;

                if (updateRequired)
                {
                    updateButton.Visible = true;
                    installButton.Text = "Install";
                    installedTextValue.Text = "No, Update Available";
                    installedTextValue.ForeColor = Color.OrangeRed;
                }
                else
                {
                    installButton.Text = "Install";
                    installedTextValue.Text = "No";
                    installedTextValue.ForeColor = Color.Red;
                }
            }

            if (!Directory.Exists(documentsModsPath))
            {
                Directory.CreateDirectory(documentsModsPath);
            }

            currentModPath = documentsModsPath;

            folderTextValue.Text = gamePath;
            gameFolderModsText.Text = gameFolderModsPath;

            if (GetModsPath() != "")
            {
                ReadModsLocation();
            }
            else
            {
                canClickCustom = true;
                _settings.ModFolderLocation = currentModPath;
            }

            /*if (File.Exists(Path.Combine(gamePath, @"ModsLocation.json")))
            {
                ReadModsLocation();
            }
            else
            {
                _settings.ModFolderLocation = currentModPath == "" ? documentsModsPath : currentModPath;
                File.WriteAllText(Path.Combine(gamePath, @"ModsLocation.json"), JsonConvert.SerializeObject(_settings, Formatting.Indented));
            }*/

            AddRegistryKeys(_settings.ModFolderLocation);
            AddJalopyPathKey(folder);

            launchButton.Visible = true;
        }

        private void installButton_Click(object sender, EventArgs e)
        {
            if (installed)
            {
                Uninstall();
                installed = false;
                installButton.Text = "Install";

                if (updateRequired)
                {
                    updateButton.Visible = true;
                    installedTextValue.Text = "No, Update Available";
                    installedTextValue.ForeColor = Color.OrangeRed;
                }
                else
                {
                    updateButton.Visible = false;
                    installedTextValue.Text = "No";
                    installedTextValue.ForeColor = Color.Red;
                }
            }
            else
            {
                Patch();
                installed = true;

                if (!updateRequired)
                {
                    installButton.Text = "Uninstall";
                    installedTextValue.Text = "Yes";
                    installedTextValue.ForeColor = Color.Green;
                }
                else
                {
                    updateRequired = true;
                    updateButton.Visible = true;
                    installedTextValue.Text = "Yes, Update Available";
                    installedTextValue.ForeColor = Color.Orange;
                }
            }
        }

        private void documentsButton_CheckedChanged(object sender, EventArgs e)
        {
            currentModPath = documentsModsPath;

            _settings.ModFolderLocation = currentModPath;
            AddRegistryKeys(_settings.ModFolderLocation);

            if (!installed)
                return;

            if (!Directory.Exists(documentsModsPath))
            {
                Directory.CreateDirectory(documentsModsPath);
            }

            if (!Directory.Exists($@"{documentsModsPath}\Required"))
            {
                Directory.CreateDirectory($@"{documentsModsPath}\Required");
            }

            File.Copy(assetBundle, $@"{documentsModsPath}\Required\JaLoader_UI.unity3d", true);

            //File.WriteAllText(Path.Combine(gamePath, @"ModsLocation.json"), JsonConvert.SerializeObject(_settings, Formatting.Indented));
        }

        private void gameFolderModsButton_CheckedChanged(object sender, EventArgs e)
        {
            currentModPath = gameFolderModsPath;

            _settings.ModFolderLocation = currentModPath;
            AddRegistryKeys(_settings.ModFolderLocation);

            if (!installed)
                return;

            if (!Directory.Exists(gameFolderModsPath))
            {
                Directory.CreateDirectory(gameFolderModsPath);
            }

            if (!Directory.Exists($@"{gameFolderModsPath}\Required"))
            {
                Directory.CreateDirectory($@"{gameFolderModsPath}\Required");
            }

            File.Copy(assetBundle, $@"{gameFolderModsPath}\Required\JaLoader_UI.unity3d", true);

            //File.WriteAllText(Path.Combine(gamePath, @"ModsLocation.json"), JsonConvert.SerializeObject(_settings, Formatting.Indented));
        }

        private void launchButton_Click(object sender, EventArgs e)
        {
            Process.Start($@"{gamePath}\Jalopy.exe");
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            if (!updateRequired)
            {
                MessageBox.Show("There are no updates available!", "JaPatcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            UpdateFiles();
        }

        private static void UpdateFiles()
        {
            Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "JaUpdater.exe"), $"{Directory.GetCurrentDirectory()} Both");
            Application.Exit();
        }

        private void customModsLocationButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!customModsLocationButton.Checked || !canClickCustom)
                return;

            if (customModsDialog.ShowDialog() == DialogResult.OK)
            {
                currentModPath = customModsDialog.SelectedPath;

                customFolderModsText.Text = currentModPath;

                _settings.ModFolderLocation = currentModPath;
                AddRegistryKeys(_settings.ModFolderLocation);

                if (!installed)
                    return;

                if (!Directory.Exists($@"{currentModPath}\Mods"))
                {
                    Directory.CreateDirectory($@"{currentModPath}\Mods");
                }

                if (!Directory.Exists($@"{currentModPath}\Required"))
                {
                    Directory.CreateDirectory($@"{currentModPath}\Required");
                }

                File.Copy(assetBundle, $@"{currentModPath}\Required\JaLoader_UI.unity3d", true);
            }
        }
    }

    class Settings
    {
        public string ModFolderLocation = "";
    }

    class Save
    {
        public string LastSelectedFolder = "";
    }
}