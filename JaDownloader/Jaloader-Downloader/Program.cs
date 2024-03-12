using Timer = System.Timers.Timer;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System;
using System.IO;
using Microsoft.Win32;
using System.IO.Compression;
using System.Net;
using System.Diagnostics;
using System.Windows.Forms;

namespace JaDownloader;
// This program is used for downloading mods from the internet. 
// Main code -- Meb
// Edits -- Leaxx

internal abstract class Program
{
    private static readonly HttpClient Client = new();
        
    private static string _param;
    private static string _author;
    private static string _repo;
    private static string _workingDirectory;

    private static string _modsLocation;

    public static async Task Main(string[] args)
    {
        _param = args[0].Split('\u002F')[2];
        _author = args[0].Split('\u002F')[3];
        _repo = args[0].Split('\u002F')[4];
        _modsLocation = GetModsLocation();
        _workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Jalopy\.cache";

        if (_workingDirectory != null && !Directory.Exists(_workingDirectory)) Directory.CreateDirectory(_workingDirectory);
        if (_param != "install") return;
        
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12; // For backwards compatibility with older OS's (such as Win7/8/8.1)

        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        Client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

        Console.WriteLine($"Got link -> https://api.github.com/repos/{_author}/{_repo}/releases/latest");
        await ProcessRepositoriesAsync($"https://api.github.com/repos/{_author}/{_repo}/releases/latest");
        Console.WriteLine("Done! Exiting in 5 seconds...");

        Timer timer = new()
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

    private static string GetModsLocation()
    {
        RegistryKey parentKey = Registry.CurrentUser;

        RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

        RegistryKey jalopyKey = softwareKey?.OpenSubKey("Jalopy");

        if (jalopyKey != null && jalopyKey.GetValue("ModsLocation") != null)
            return jalopyKey.GetValue("ModsLocation").ToString();
        else
        {
            MessageBox.Show("The ModsLocation key could not be found. Please start the game at least once and/or reinstall JaLoader via JaPatcher.", "JaDownloader", MessageBoxButtons.OK, MessageBoxIcon.Error);
            throw new Exception("The ModsLocation key could not be found. Please start the game at least once and/or reinstall JaLoader via JaPatcher.");
        }
    }

    private static async Task ProcessRepositoriesAsync(string uri)
    {
        GitAsset asset;

        try
        {
            asset = JObject.Parse(await Client.GetStringAsync(uri)).ToObject<GitRepo>().Assets[0];
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine("Invalid URL!");
            return;
            throw;
        }
        
        using var client = new WebClient();
        client.DownloadFile(asset.Browser_Download_URL, _workingDirectory + @"\" + asset.Name);
        if (!Directory.Exists(_workingDirectory + @"\" + _repo))
        {
            Directory.CreateDirectory(_workingDirectory + @"\" + _repo);
        }
        else
        {
            Directory.Delete(_workingDirectory + @"\" + _repo, true);
        }
        if (asset.Name.EndsWith(".zip"))
        {
            ZipFile.ExtractToDirectory(_workingDirectory + @"\" + asset.Name, _workingDirectory + @"\" + _repo);
        }
        else if (asset.Name.EndsWith(".dll"))
        {
            File.Move(_workingDirectory + @"\" + asset.Name, _workingDirectory + @"\" + _repo + @"\" + asset.Name);
        }
        else
        {
            Console.WriteLine("This mod cannot be installed via JaDownloader! Please install manually.");
            return;
        }

        Console.WriteLine("Download finished successfully! Installing mod...");

        var assetsFolder = _modsLocation + @"\Assets";
        if(!Directory.Exists(assetsFolder)) Directory.CreateDirectory(assetsFolder);

        var assembliesFolder = _modsLocation + @"\Assemblies";
        if(!Directory.Exists(assembliesFolder)) Directory.CreateDirectory(assembliesFolder);

        DirectoryInfo d = new(_workingDirectory + @"\" + _repo);
        FileInfo mainDLL = d.GetFiles("*.dll")[0];

        var dirs = d.GetDirectories();

        if (dirs.Length > 0)
        {
            foreach (var dir in dirs)
            {
                if (dir.Name == "Assets")
                {
                    DirectoryInfo inside = new(dir.FullName);
                    var folder = inside.GetDirectories()[0];
                    var folderName = folder.Name;
                    DirectoryInfo contents = new(folder.FullName);
                    FileInfo[] files = contents.GetFiles();

                    Directory.CreateDirectory(assetsFolder + @$"\{folderName}");

                    foreach (var file in files)
                    {
                        File.Copy(file.FullName, assetsFolder + @$"\{folderName}\{file.Name}", true);
                    }
                }

                if (dir.Name == "Assemblies")
                {
                    DirectoryInfo contents = new(dir.FullName);
                    FileInfo[] files = contents.GetFiles();

                    foreach (var file in files)
                    {
                        File.Copy(file.FullName, assembliesFolder + @$"\{file.Name}", true);
                    }
                }
            }
        }

        File.Copy(mainDLL.FullName, _modsLocation + $@"\{mainDLL.Name}", true);

        Console.WriteLine("Mod successfully installed! Cleaning up...");

        File.Delete(_workingDirectory + @"\" + asset.Name);
        Directory.Delete(_workingDirectory + @"\" + _repo, true);
    }
}

public class GitRepo
{
    public List<GitAsset> Assets { get; set; }
}

public class GitAsset
{
    public string Browser_Download_URL { get; set;}
    public string Name { get; set;}
}