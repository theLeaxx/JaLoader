using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JaUpdaterNETFramework
{
    internal class Program
    {
        static string currentDir = Directory.GetCurrentDirectory();
        static string file = "https://github.com/theLeaxx/JaLoader/releases/latest/download/JaPatcher.zip";
        static string extractLocation = "";

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                DownloadJaPatcherFunction();
                return;
            }

            Console.WriteLine("Downloading update...");
            if (args.Length == 2)
            {
                var filesPath = $@"{currentDir}\Files";

                if (args[0].Contains("\"")) extractLocation = args[0].Replace("\"", "");
                else extractLocation = args[0];
                var type = args[1];

                DownloadFile(file, currentDir);

                Console.WriteLine("Update downloaded successfully!");

                if (!Directory.Exists(filesPath))
                    Directory.CreateDirectory(filesPath);

                Console.WriteLine("Extracting files...");

                ZipFile.ExtractToDirectory($@"{currentDir}\JaPatcher.zip", filesPath);

                Console.WriteLine("Files extracted successfully!");

                var info = new DirectoryInfo($@"{filesPath}\Assets");
                FileInfo[] files = info.GetFiles();

                var mainFolder = new DirectoryInfo($@"{filesPath}\Assets\Main");
                var managedFolder = new DirectoryInfo($@"{filesPath}\Assets\Managed");
                var requiredFolder = new DirectoryInfo($@"{filesPath}\Assets\Required");

                FileInfo[] mainFiles = mainFolder.GetFiles();
                FileInfo[] managedFiles = managedFolder.GetFiles();
                FileInfo[] requiredFiles = requiredFolder.GetFiles();

                Console.WriteLine("Replacing files...");

                switch (type)
                {
                    case "Jalopy":

                        Thread.Sleep(2500);

                        foreach (var updateFile in requiredFiles)
                            File.Copy(updateFile.FullName, $@"{extractLocation}\Required\{updateFile.Name}", true);

                        foreach (var updateFile in managedFiles)
                            File.Copy(updateFile.FullName, $@"{currentDir}\Jalopy_Data\Managed\{updateFile.Name}", true);

                        foreach (var updateFile in mainFiles)
                        {
                            if (updateFile.Name == "winhttp.dll") continue;
                            File.Copy(updateFile.FullName, $@"{currentDir}\{updateFile.Name}", true);
                        }

                        break;

                    case "Patcher":
                        foreach (var updateFile in mainFiles)
                            File.Copy(updateFile.FullName, $@"{extractLocation}\Assets\Main\{updateFile.Name}", true);

                        foreach (var updateFile in managedFiles)
                            File.Copy(updateFile.FullName, $@"{extractLocation}\Assets\Managed\{updateFile.Name}", true);

                        foreach (var updateFile in requiredFiles)
                            File.Copy(updateFile.FullName, $@"{extractLocation}\Assets\Required\{updateFile.Name}", true);

                        File.Copy($@"{filesPath}\JaPatcher.exe", $@"{extractLocation}\JaPatcher.exe", true);

                        break;
                }

                File.Delete($@"{currentDir}\JaPatcher.zip");
                Directory.Delete(filesPath, true);
            }

            Console.WriteLine("Update applied successfully! Closing in 5 seconds...");

            System.Timers.Timer timer = new System.Timers.Timer()
            {
                Interval = 5000,
                Enabled = true
            };
            timer.Elapsed += (s, e) =>
            {
                Environment.Exit(0);
            };

            Console.ReadLine();
        }

        static void DownloadJaPatcherFunction()
        {
            Console.WriteLine("Running this program will download JaPatcher and extract it to a directory here. Continue? (Y/N)");
            var input = Console.ReadLine();
            if (input == null || input.ToLower() != "y")
            {
                Console.WriteLine("Exiting...");
                Environment.Exit(0);
                return;
            }

            var dir = Directory.CreateDirectory($@"{currentDir}\JaPatcher");
            DownloadFile(file, dir.FullName);

            Console.WriteLine("JaPatcher downloaded successfully! You can now run JaPatcher.exe in the zip file inside the JaPatcher folder to install JaLoader.");
            Console.ReadLine();
        }

        static void DownloadFile(string url, string destination)
        {
            var client = new HttpClient();
            var s = client.GetStreamAsync(file);
            var fs = new FileStream($@"{destination}\JaPatcher.zip", FileMode.OpenOrCreate);

            if (s.IsCanceled || s.IsFaulted)
            {
                Console.WriteLine("Failed to download update! Are you connected to the internet?");
                return;
            }

            s.Result.CopyTo(fs);
            fs.Dispose();
            s.Dispose();
            client.Dispose();
        }
    }
}
