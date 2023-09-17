using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;

namespace JaDownloaderSetup;
// This program is used for setting up the mod downloader.
// Main code -- Meb
// Edits -- Leaxx

internal static class Setup
{
    public static void Main(string[] args)
    {
        if (args.Length <= 0)
        {
            MessageBox.Show("No arguments provided! Please use the in-game option to enable JaLoader, or use JaPatcher.");
            return;
        }

        using var key = Registry.ClassesRoot.OpenSubKey(@"jaloader\shell\open\command", false);
        if (key != null)
        {
            if (args.Length == 1 && args[0] == "Uninstall")
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    Registry.ClassesRoot.DeleteSubKeyTree(@"jaloader");
                    MessageBox.Show("JaDownloader successfully uninstalled!");
                }
                return;
            }

            MessageBox.Show("JaDownloader is already setup!");
            return;
        }
        else
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                var key0 = Registry.ClassesRoot.CreateSubKey(@"jaloader\shell\open\command");
                using var key1 = Registry.ClassesRoot.OpenSubKey(@"jaloader", true);
                if (args[0].Contains("\"")) args[0].Replace("\"", "");
                key0?.SetValue("", $"\"{args[0]}\" \"%1\"");
                key1?.SetValue("URL Protocol", "");
                MessageBox.Show("JaDownloader successfully setup!");
            }
            else MessageBox.Show("Please run this program as administrator!");
        }
    }
}