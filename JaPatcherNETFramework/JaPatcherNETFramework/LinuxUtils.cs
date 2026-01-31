using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JaPatcherNETFramework
{
    internal static class LinuxUtils
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public static bool IsLinux()
        {
            IntPtr hModule = GetModuleHandle("ntdll.dll");
            if (hModule == IntPtr.Zero) return false;

            IntPtr ptr = GetProcAddress(hModule, "wine_get_version");

            return ptr != IntPtr.Zero;
        }

        public static string ToWinePath(string linuxPath)
        {
            if(string.IsNullOrEmpty(linuxPath))
                return string.Empty;

            return "Z:" + linuxPath.Replace('/', '\\');
        }

        public static string GetLinuxDocumentsModsFolder()
        {
            string wineHome = Environment.GetEnvironmentVariable("WINEHOMEDIR");
            string root;

            if (!string.IsNullOrEmpty(wineHome))
                // Z:\home\user\Jalopy\Mods
                root = wineHome.Replace(@"\??\", "").TrimEnd('\\') + @"\Jalopy\Mods";
            else
                // C:\Users\Name\Jalopy\Mods
                root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"Jalopy\Mods");

            if (!Directory.Exists(root))
            {
                try
                {
                    Directory.CreateDirectory(root);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error creating folders: " + ex.Message);
                }
            }

            return root;
        }
    }
}
