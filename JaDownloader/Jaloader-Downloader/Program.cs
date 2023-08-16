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

namespace Jaloader_Downloader;
// This program is the program that is used for downloading mods from the internet. 
// Meb & Leaxx

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
            throw new Exception("The ModsLocation key could not be found. Please start the game at least once and/or reinstall JaLoader via JaPatcher.");
    }

    private static async Task ProcessRepositoriesAsync(string uri)
    {
        GitAsset asset;

        try
        {
            asset = JObject.Parse(await Client.GetStringAsync(uri)).ToObject<GitRepo>().assets[0];
        }
        catch (Exception)
        {
            Console.WriteLine("Invalid URL!");
            return;
            throw;
        }
        
        using var client = new WebClient();
        client.DownloadFile(asset.browser_download_url, _workingDirectory + @"\" + asset.name);
        if (!Directory.Exists(_workingDirectory + @"\" + _repo)) Directory.CreateDirectory(_workingDirectory + @"\" + _repo);
        if (asset.name.EndsWith(".zip"))
        {
            ZipFile.ExtractToDirectory(_workingDirectory + @"\" + asset.name, _workingDirectory + @"\" + _repo);
        }
        else if (asset.name.EndsWith(".dll"))
        {
            File.Move(_workingDirectory + @"\" + asset.name, _workingDirectory + @"\" + _repo + @"\" + asset.name);
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

        File.Delete(_workingDirectory + @"\" + asset.name);
        Directory.Delete(_workingDirectory + @"\" + _repo, true);
    }
}

public class GitRepo
{
    public List<GitAsset> assets { get; set; }
}

public class GitAsset
{
    public string browser_download_url { get; set;}
    public string name { get; set;}
}
// jaloader://install/jalopy-mods/DDR-PaperPlease