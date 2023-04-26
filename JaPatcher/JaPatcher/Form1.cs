using Newtonsoft.Json;

namespace JalopyModLoader
{

    public partial class Form1 : Form
    {
        Settings _settings = new Settings();

        private string gamePath = "";
        private string documentsModsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Jalopy\Mods");
        private string gameFolderModsPath = "";

        private string currentModPath = "";

        private bool installed = false;
        private bool selectedPath = false;

        private string winhttpDLL = Path.Combine(Directory.GetCurrentDirectory(), @"winhttp.dll");
        private string doorstopConfig = Path.Combine(Directory.GetCurrentDirectory(), @"doorstop_config.ini");
        private string jsonDLL = Path.Combine(Directory.GetCurrentDirectory(), @"Newtonsoft.Json.dll");
        private string modLoaderInjectDLL = Path.Combine(Directory.GetCurrentDirectory(), @"ModLoaderInject.dll");
        private string modLoaderDLL = Path.Combine(Directory.GetCurrentDirectory(), @"ModLoader.dll");

        public Form1()
        {
            InitializeComponent();

            if (!File.Exists(winhttpDLL) || !File.Exists(doorstopConfig) || !File.Exists(modLoaderDLL) || !File.Exists(modLoaderInjectDLL) || !File.Exists(jsonDLL))
            {
                MessageBox.Show("Please extract all of the contents from the archive!", "DLLs not found", MessageBoxButtons.OK);
                Close();
            }
        }

        public void Patch()
        {
            File.Copy(winhttpDLL, Path.Combine(gamePath, @"winhttp.dll"), true);
            File.Copy(doorstopConfig, Path.Combine(gamePath, @"doorstop_config.ini"), true);
            File.Copy(modLoaderDLL, Path.Combine(gamePath, @"Jalopy_Data\Managed\ModLoader.dll"), true);
            File.Copy(modLoaderInjectDLL, Path.Combine(gamePath, @"Jalopy_Data\Managed\ModLoaderInject.dll"), true);
            File.Copy(jsonDLL, Path.Combine(gamePath, @"Jalopy_Data\Managed\Newtonsoft.Json.dll"), true);

            _settings.ModFolderLocation = currentModPath;

            File.WriteAllText(Path.Combine(gamePath, @"ModsLocation.json"), JsonConvert.SerializeObject(_settings, Formatting.Indented));
        }

        public void Uninstall()
        {
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void folderTextValue_Click(object sender, EventArgs e)
        {

        }

#pragma warning disable CS8601
#pragma warning disable CS8604
        private void locateFolderButton_Click(object sender, EventArgs e)
        {
            if (locateFolderDialog.ShowDialog() == DialogResult.OK)
            {
                selectedPath = true;

                documentsButton.Enabled = true;
                gameFolderModsButton.Enabled = true;

                gamePath = Path.GetDirectoryName(locateFolderDialog.FileName);
                gameFolderModsPath = Path.Combine(gamePath, @"Mods");

                if (File.Exists(Path.Combine(gamePath, @"Jalopy_Data\Managed\ModLoaderInject.dll")) && File.Exists(Path.Combine(gamePath, @"Jalopy_Data\Managed\ModLoader.dll")) && File.Exists(Path.Combine(gamePath, @"Jalopy_Data\Managed\Newtonsoft.Json.dll")) && File.Exists(Path.Combine(gamePath, @"winhttp.dll")) && File.Exists(Path.Combine(gamePath, @"doorstop_config.ini")))
                {
                    installed = true;
                    installedTextValue.Text = "Yes";
                }

                if (installed)
                    uninstallButton.Enabled = true;
                else
                    installButton.Enabled = true;

                if (!Directory.Exists(gameFolderModsPath))
                {
                    Directory.CreateDirectory(gameFolderModsPath);
                }

                if (!Directory.Exists(documentsModsPath))
                {
                    Directory.CreateDirectory(documentsModsPath);
                }

                currentModPath = documentsModsPath;

                folderTextValue.Text = gamePath;
                gameFolderModsText.Text = gameFolderModsPath;
                documentsModsText.Text = documentsModsPath;
            }
        }
#pragma warning restore CS8601
#pragma warning restore CS8604

        private void uninstallButton_Click(object sender, EventArgs e)
        {
            installed = false;
            installButton.Enabled = true;
            uninstallButton.Enabled = false;
            installedTextValue.Text = "No";
        }

        private void installButton_Click(object sender, EventArgs e)
        {
            Patch();
            installed = true;
            uninstallButton.Enabled = true;
            installButton.Enabled = false;
            installedTextValue.Text = "Yes";
        }

        private void modsTab_Click(object sender, EventArgs e)
        {

        }

        private void settingsTab_Click(object sender, EventArgs e)
        {

        }

        private void locateFolderDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void gameFolderText_Click(object sender, EventArgs e)
        {

        }

        private void documentsButton_CheckedChanged(object sender, EventArgs e)
        {
            currentModPath = documentsModsPath;

            _settings.ModFolderLocation = currentModPath;

            File.WriteAllText(Path.Combine(gamePath, @"ModsLocation.json"), JsonConvert.SerializeObject(_settings, Formatting.Indented));
        }

        private void gameFolderModsButton_CheckedChanged(object sender, EventArgs e)
        {
            currentModPath = gameFolderModsPath;

            _settings.ModFolderLocation = currentModPath;

            File.WriteAllText(Path.Combine(gamePath, @"ModsLocation.json"), JsonConvert.SerializeObject(_settings, Formatting.Indented));
        }

        private void documentsModsText_Click(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

    }

    class Settings
    {
        public string ModFolderLocation = "";
    }
}