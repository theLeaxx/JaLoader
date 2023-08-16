using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Jalopy_Downloader_Setup;
// This program is the program that is used for setting up the mod downloader.
// Meb

internal static class Setup
{
    public static void Main(string[] args)
    {
        using var key = Registry.ClassesRoot.OpenSubKey(@"jaloader\shell\open\command", false);
        if (key?.GetValue("") == null)
        {
            MessageBox.Show("Mod Downloader already well setup!");
        }
        else
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                var key0 = Registry.ClassesRoot.CreateSubKey(@"jaloader\shell\open\command");
                using var key1 = Registry.ClassesRoot.OpenSubKey(@"jaloader", true);
                key0?.SetValue("", $"\"{args[0]}\" \"%1\"");
                key1?.SetValue("URL Protocol", "");
                MessageBox.Show("Mod Downloader setup successfully!");
            }
            else MessageBox.Show("Please run this program as administrator!");
        }
    }
}