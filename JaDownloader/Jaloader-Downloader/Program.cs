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
using System.Reflection;
using System.Text.Json;
using Newtonsoft.Json;
using System.Linq;

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

    private static string version = "1.2.0";

    private static bool isGitHub = true;

    public static async Task Main(string[] args)
    {
        Console.WriteLine($"JaDownloader v{version}\n");

        if (args.Length <= 0)
        {
            Console.WriteLine("No arguments provided!");
            return;
        }

        _param = args[0].Split('\u002F')[2];
        _author = args[0].Split('\u002F')[3];
        _repo = args[0].Split('\u002F')[4];
        //Console.WriteLine("Please enter the mod ID (available values currently are 2, 4, 5 and 6):");
        //_repo = Console.ReadLine();
        //_param = "install";
        //_author = "nexus";
        _modsLocation = GetModsLocation();
        _workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Jalopy\.cache";

        if (_workingDirectory != null && !Directory.Exists(_workingDirectory)) Directory.CreateDirectory(_workingDirectory);
        if (_param != "install" && _param != "installingame") return;
        
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12; // For backwards compatibility with older OS's (such as Win7/8/8.1)

        if (_author == "nexus")
            isGitHub = false;

        if (isGitHub)
        {
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            Client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            Console.WriteLine($"Got link -> https://api.github.com/repos/{_author}/{_repo}/releases/latest");
            await ProcessRepositoriesAsync($"https://api.github.com/repos/{_author}/{_repo}/releases/latest");
            Console.WriteLine("Done! Exiting in 5 seconds...");
        }
        else
        {
            string apikey;

            Console.WriteLine("Please enter your NexusMods API key:");
            apikey = Console.ReadLine();

            var gameId = "jalopy";

            string url = await GetLatestFileDownloadUrlAsync(gameId, int.Parse(_repo), apikey);

            Console.WriteLine(url);
        }

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
        catch (Exception)
        {
            Console.WriteLine("Invalid URL!");
            return;
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
                        try
                        {
                            File.Copy(file.FullName, assetsFolder + @$"\{folderName}\{file.Name}", true);
                        }
                        catch (IOException ex)
                        {
                            if(ex.Message.Contains("The requested operation cannot be performed on a file with a user-mapped section open."))
                                Console.WriteLine($"Failed to copy file `{file.Name}`, since the game is using the file already.");
                        }
                }

                if (dir.Name == "Assemblies")
                {
                    DirectoryInfo contents = new(dir.FullName);
                    FileInfo[] files = contents.GetFiles();

                    foreach (var file in files)
                        try
                        {
                            File.Copy(file.FullName, assembliesFolder + @$"\{file.Name}", true);
                        }
                        catch (IOException ex)
                        {
                            if (ex.Message.Contains("The requested operation cannot be performed on a file with a user-mapped section open."))
                                Console.WriteLine($"Failed to copy file `{file.Name}`, since the game is using the file already.");
                        }
                }
            }
        }

        try
        {
            File.Copy(mainDLL.FullName, _modsLocation + $@"\{mainDLL.Name}", true);
        }
        catch (IOException ex)
        {
            if (ex.Message.Contains("The requested operation cannot be performed on a file with a user-mapped section open."))
                Console.WriteLine($"Failed to copy file `{mainDLL.Name}`, since the game is using the file already.");
        }

        Console.WriteLine("Mod successfully installed! Cleaning up...");

        File.Delete(_workingDirectory + @"\" + asset.Name);
        Directory.Delete(_workingDirectory + @"\" + _repo, true);

        if (_param == "installingame")
            File.WriteAllText(_modsLocation + @$"\{_author}_{_repo}_Installed.txt", mainDLL.Name);
    }

    public static async Task<string> GetLatestFileDownloadUrlAsync(string gameId, int modId, string apiKey)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("apikey", apiKey);

        var fileEndpoint = $"v1/games/{gameId}/mods/{modId}/files.json";
        var fileResponse = await client.GetAsync($"https://api.nexusmods.com/{fileEndpoint}");
        fileResponse.EnsureSuccessStatusCode();
        var fileContent = await fileResponse.Content.ReadAsStringAsync();

        var file = JsonConvert.DeserializeObject<NexusFile>(fileContent);

        Console.WriteLine($"Mod ID is valid, latest file name is {file.Files[file.Files.Count - 1].name}");
        Console.WriteLine("Attempting to obtain download link.");

        var downloadEndpoint = $"v1/games/{gameId}/mods/{modId}/files/{file.Files[file.Files.Count - 1].file_id}/download_link.json";
        var downloadResponse = await client.GetAsync($"https://api.nexusmods.com/{downloadEndpoint}");
        try
        {
            downloadResponse.EnsureSuccessStatusCode();
            var downloadContent = await downloadResponse.Content.ReadAsStringAsync();
            return downloadContent;
        }
        catch (Exception)
        {
            Console.WriteLine("Getting the download link is only available with Premium.");
            return "";
        }
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

public class NexusFile
{
    public List<InnerFile> Files { get; set; }
}

public class InnerFile
{
    public List<int> id { get; set; }
    public long uid { get; set; }
    public int file_id { get; set; }
    public string name { get; set; }
    public string version { get; set; }
    public int? category_id { get; set; }
    public string category_name { get; set; }
    public bool is_primary { get; set; }
    public long? size { get; set; }
    public string file_name { get; set; }
    public long? uploaded_timestamp { get; set; }
    public string uploaded_time { get; set; }
    public string mod_version { get; set; }
    public string external_virus_scan_url { get; set; }
    public string description { get; set; }
    public long? size_kb { get; set; }
    public long? size_in_bytes { get; set; }
    public object changelog_html { get; set; }
    public string content_preview_link { get; set; }
}