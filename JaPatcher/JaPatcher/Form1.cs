#pragma warning disable CS8601
#pragma warning disable CS8604
#pragma warning disable CS8600
#pragma warning disable CS8603

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;

namespace JalopyModLoader
{
    public partial class Form1 : Form
    {
        private Settings _settings = new();
        private Save _save = new();

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

            groupBox1.ForeColor = Color.White;
            groupBox2.ForeColor = Color.White;
            documentsButton.ForeColor = Color.White;
            installedTextValue.ForeColor = Color.White;
            gameFolderModsButton.ForeColor = Color.White;
            locateFolderButton.ForeColor = Color.White;
            installButton.ForeColor = Color.White;
        }
        #endregion

        private string gamePath = "";
        private string documentsModsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Jalopy\Mods");
        private string gameFolderModsPath = "";

        private string currentModPath = "";

        private bool installed = false;

        private string winhttpDLL = Path.Combine(Directory.GetCurrentDirectory(), @"Assets\winhttp.dll");
        private string doorstopConfig = Path.Combine(Directory.GetCurrentDirectory(), @"Assets\doorstop_config.ini");
        private string jsonDLL = Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Newtonsoft.Json.dll");
        private string jaLoaderDLL = Path.Combine(Directory.GetCurrentDirectory(), @"Assets\JaLoader.dll");
        private string jaPreLoaderDLL = Path.Combine(Directory.GetCurrentDirectory(), @"Assets\JaPreLoader.dll");
        private string assetBundle = Path.Combine(Directory.GetCurrentDirectory(), @"Assets\JaLoader_UI.unity3d");

        public Form1()
        {
            InitializeComponent();

            if (IsSystemUsingDarkmode())
            {
                SetDarkMode();
            }

            if (!File.Exists(winhttpDLL) || !File.Exists(doorstopConfig) || !File.Exists(jaPreLoaderDLL) || !File.Exists(jaLoaderDLL) || !File.Exists(jsonDLL) || !File.Exists(assetBundle))
            {
                MessageBox.Show("Please extract all of the contents from the archive!", "DLLs not found", MessageBoxButtons.OK);
                Close();
            }

            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), @"save.json")))
            {
                _save.LastSelectedFolder = ReadString(Path.Combine(Directory.GetCurrentDirectory(), @"save.json"));

                CheckFolder(_save.LastSelectedFolder);
            }

            documentsModsText.Text = documentsModsPath;
        }

        public string ReadString(string path)
        {
            if (!File.Exists(path))
                return "";

            JObject o1 = JObject.Parse(File.ReadAllText(path));

            StreamReader file = File.OpenText(path);
            JsonTextReader reader = new JsonTextReader(file);

            JObject o2 = (JObject)JToken.ReadFrom(reader);
            string toReturn = (string)o2.First;

            reader.Close();
            file.Close();

            return toReturn;
        }

        public void ReadModsLocation()
        {
            _settings.ModFolderLocation = ReadString(Path.Combine(gamePath, @"ModsLocation.json"));

            if (_settings.ModFolderLocation == documentsModsPath)
            {
                documentsButton.Checked = true;
            }
            else if (_settings.ModFolderLocation == gameFolderModsPath)
            {
                gameFolderModsButton.Checked = true;
            }
        }

        public void Patch()
        {
            File.Copy(winhttpDLL, Path.Combine(gamePath, @"winhttp.dll"), true);
            File.Copy(doorstopConfig, Path.Combine(gamePath, @"doorstop_config.ini"), true);
            File.Copy(jaPreLoaderDLL, Path.Combine(gamePath, @"Jalopy_Data\Managed\JaPreLoader.dll"), true);
            File.Copy(jaLoaderDLL, Path.Combine(gamePath, @"Jalopy_Data\Managed\JaLoader.dll"), true);
            File.Copy(jsonDLL, Path.Combine(gamePath, @"Jalopy_Data\Managed\Newtonsoft.Json.dll"), true);

            if (!Directory.Exists(Path.Combine(currentModPath, "Required")))
            {
                Directory.CreateDirectory(Path.Combine(currentModPath, "Required"));
            }

            File.Copy(assetBundle, Path.Combine(currentModPath, @"Required\JaLoader_UI.unity3d"), true);

            _settings.ModFolderLocation = currentModPath;
            File.WriteAllText(Path.Combine(gamePath, @"ModsLocation.json"), JsonConvert.SerializeObject(_settings, Formatting.Indented));
        }

        public void Uninstall()
        {
            File.Delete(Path.Combine(gamePath, @"winhttp.dll"));
            File.Delete(Path.Combine(gamePath, @"doorstop_config.ini"));
            File.Delete(Path.Combine(gamePath, @"Jalopy_Data\Managed\JaPreLoader.dll"));
            File.Delete(Path.Combine(gamePath, @"Jalopy_Data\Managed\JaLoader.dll"));
            File.Delete(Path.Combine(gamePath, @"Jalopy_Data\Managed\Newtonsoft.Json.dll"));

            if (MessageBox.Show("Would you like to delete the configuration files too?", "JaPatcher", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                File.Delete(Path.Combine(gamePath, @"ModsLocation.json"));
                File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"..\LocalLow\MinskWorks\Jalopy\JMLConfig.json"));
            }
        }

        private void locateFolderButton_Click(object sender, EventArgs e)
        {
            if (locateFolderDialog.ShowDialog() == DialogResult.OK)
            {
                CheckFolder(locateFolderDialog.FileName);

                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), @"save.json"), JsonConvert.SerializeObject(_save, Formatting.Indented));
            }
        }

        private void CheckFolder(string folder)
        {
            _save.LastSelectedFolder = folder;

            groupBox2.Visible = true;
            installButton.Visible = true;

            documentsButton.Enabled = true;
            gameFolderModsButton.Enabled = true;

            gamePath = Path.GetDirectoryName(folder);
            gameFolderModsPath = Path.Combine(gamePath, @"Mods");

            if (File.Exists(Path.Combine(gamePath, @"Jalopy_Data\Managed\JaLoader.dll")) && File.Exists(Path.Combine(gamePath, @"Jalopy_Data\Managed\JaPreLoader.dll")) && File.Exists(Path.Combine(gamePath, @"Jalopy_Data\Managed\Newtonsoft.Json.dll")) && File.Exists(Path.Combine(gamePath, @"winhttp.dll")) && File.Exists(Path.Combine(gamePath, @"doorstop_config.ini")))
            {
                installed = true;
                installButton.Text = "Uninstall";
                installedTextValue.Text = "Yes";
                installedTextValue.ForeColor = Color.Green;
            }
            else
            {
                installed = false;
                installButton.Text = "Install";
                installedTextValue.Text = "No";
                installedTextValue.ForeColor = Color.Red;
            }

            if (!Directory.Exists(documentsModsPath))
            {
                Directory.CreateDirectory(documentsModsPath);
            }

            currentModPath = documentsModsPath;

            folderTextValue.Text = gamePath;
            gameFolderModsText.Text = gameFolderModsPath;

            if (File.Exists(Path.Combine(gamePath, @"ModsLocation.json")))
            {
                ReadModsLocation();
            }
        }

        private void installButton_Click(object sender, EventArgs e)
        {
            if (installed)
            {
                Uninstall();
                installed = false;
                installButton.Text = "Install";
                installedTextValue.Text = "No";
                installedTextValue.ForeColor = Color.Red;
            }
            else
            {
                Patch();
                installed = true;
                installButton.Text = "Uninstall";
                installedTextValue.Text = "Yes";
                installedTextValue.ForeColor = Color.Green;
            }
        }

        private void documentsButton_CheckedChanged(object sender, EventArgs e)
        {
            currentModPath = documentsModsPath;

            _settings.ModFolderLocation = currentModPath;

            if (!installed)
                return;


            if (!Directory.Exists(documentsModsPath))
            {
                Directory.CreateDirectory(documentsModsPath);
            }

            File.WriteAllText(Path.Combine(gamePath, @"ModsLocation.json"), JsonConvert.SerializeObject(_settings, Formatting.Indented));
        }

        private void gameFolderModsButton_CheckedChanged(object sender, EventArgs e)
        {
            currentModPath = gameFolderModsPath;

            _settings.ModFolderLocation = currentModPath;

            if (!installed)
                return;

            if (!Directory.Exists(gameFolderModsPath))
            {
                Directory.CreateDirectory(gameFolderModsPath);
            }

            File.WriteAllText(Path.Combine(gamePath, @"ModsLocation.json"), JsonConvert.SerializeObject(_settings, Formatting.Indented));
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