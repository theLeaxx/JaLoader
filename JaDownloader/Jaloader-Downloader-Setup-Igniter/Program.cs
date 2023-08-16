using System;
using System.Diagnostics;

namespace Jalopy_Downloader_Setup_Igniter;
// This program is the program that is used for launching well the setup thing for the mod downloader.
// Meb

internal abstract class Program
{
    public static void Main(string[] args)
    {
        var p = new Process();
        p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        p.StartInfo.FileName = args[0];
        p.StartInfo.UseShellExecute = true;
        p.StartInfo.Arguments = args[1];
        if (Environment.OSVersion.Version.Major >= 6)
        {
            p.StartInfo.Verb = "runas";
        }

        p.Start();
        p.WaitForExit();
    }
}
