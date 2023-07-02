using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace Jaloader_Downloader;
// This program is the program that is used for downloading mods from the internet. 

internal abstract class Program
{
    private static readonly HttpClient Client = new();
        
    private static string _param;
    private static string _author;
    private static string _repo;
    private static string _workingDirectory;
    public static async Task Main(string[] args)
    {
        _param = args[0].Split('\u002F')[2];
        _author = args[0].Split('\u002F')[3];
        _repo = args[0].Split('\u002F')[4];
        _workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Jalopy\.cache"; 
        
        if (_workingDirectory != null && !Directory.Exists(_workingDirectory)) Directory.CreateDirectory(_workingDirectory);
        if (_param != "install") return;
            
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        Client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
        
        Console.WriteLine($"Got link -> https://api.github.com/repos/{_author}/{_repo}/releases/latest");
        await ProcessRepositoriesAsync($"https://api.github.com/repos/{_author}/{_repo}/releases/latest");
        Console.ReadKey();
    }

    private static async Task ProcessRepositoriesAsync(string uri)
    {
        // await Client.GetStringAsync(uri)
        var asset = JObject.Parse(await Client.GetStringAsync(uri)).ToObject<GitRepo>().assets[0];
        using var client = new WebClient();
        client.DownloadFile(asset.browser_download_url, _workingDirectory + @"\" + asset.name);
        ZipFile.ExtractToDirectory(_workingDirectory + @"\" + asset.name, _workingDirectory + @"\" + _repo);
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class GitRepo
{
    public List<GitAsset> assets { get; set; }
}


[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class GitAsset
{
    public string browser_download_url { get; set;}
    public string name { get; set;}
}
// jaloader://install/jalopy-mods/CameraPlacements