using System.IO.Compression;
using Timer = System.Timers.Timer;
using Microsoft.Win32;

// this will only work with releases 1.1.0 and above, as the zip file contents have been rearranged
Console.WriteLine("Downloading update...");
if (args.Length == 2)
{
    var file = "https://github.com/theLeaxx/JaLoader/releases/latest/download/JaPatcher.zip";
    var currentDir = Directory.GetCurrentDirectory();
    var filesPath = $@"{currentDir}\Files";

    var extractLocation = "";
    if (args[0].Contains("\"")) extractLocation = args[0].Replace("\"", "");
    var type = args[1];

    using var client = new HttpClient();
    using var s = client.GetStreamAsync(file);
    using var fs = new FileStream($@"{currentDir}\JaPatcher.zip", FileMode.OpenOrCreate);
    
    if (s.IsCanceled || s.IsFaulted)
    {
        Console.WriteLine("Failed to download update! Are you connected to the internet?");
        return;
    }

    s.Result.CopyTo(fs);
    fs.Dispose();
    s.Dispose();
    client.Dispose();

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

#pragma warning disable CA1416
#pragma warning disable CS8602
#pragma warning disable CS8600

    RegistryKey parentKey = Registry.CurrentUser;

    RegistryKey softwareKey = parentKey.OpenSubKey("Software", true);

    RegistryKey jalopyKey = softwareKey?.CreateSubKey("Jalopy", true);

    var jalopyPath = jalopyKey?.GetValue("JalopyPath").ToString();
    var modsPath = jalopyKey?.GetValue("ModsLocation").ToString();

#pragma warning restore CA1416
#pragma warning restore CS8602
#pragma warning restore CS8600

    switch (type)
    {
        case "Jalopy":
            foreach (var updateFile in mainFiles)
            {
                File.Copy(updateFile.FullName, $@"{currentDir}\{updateFile.Name}", true);
            }

            foreach (var updateFile in managedFiles)
            {
                File.Copy(updateFile.FullName, $@"{currentDir}\Jalopy_Data\Managed\{updateFile.Name}", true);
            }

            foreach (var updateFile in requiredFiles)
            {
                File.Copy(updateFile.FullName, $@"{extractLocation}\{updateFile.Name}", true);
            }
            break;

        case "Patcher":
            foreach (var updateFile in mainFiles)
            {
                File.Copy(updateFile.FullName, $@"{extractLocation}\Assets\Main\{updateFile.Name}", true);
            }

            foreach (var updateFile in managedFiles)
            {
                File.Copy(updateFile.FullName, $@"{extractLocation}\Assets\Managed\{updateFile.Name}", true);
            }

            foreach (var updateFile in requiredFiles)
            {
                File.Copy(updateFile.FullName, $@"{extractLocation}\Assets\Required\{updateFile.Name}", true);
            }

            File.Copy($@"{filesPath}\JaPatcher.exe", $@"{extractLocation}\JaPatcher.exe", true);
            break;

        case "Both":
            if (!string.IsNullOrEmpty(jalopyPath) && !string.IsNullOrEmpty(modsPath))
            {
                foreach (var updateFile in mainFiles)
                {
                    File.Copy(updateFile.FullName, $@"{jalopyPath}\..\{updateFile.Name}", true);
                }

                foreach (var updateFile in managedFiles)
                {
                    File.Copy(updateFile.FullName, $@"{jalopyPath}\..\Jalopy_Data\Managed\{updateFile.Name}", true);
                }

                foreach (var updateFile in requiredFiles)
                {
                    File.Copy(updateFile.FullName, $@"{modsPath}\Required\{updateFile.Name}", true);
                }
            }

            foreach (var updateFile in mainFiles)
            {
                File.Copy(updateFile.FullName, $@"{extractLocation}\Assets\Main\{updateFile.Name}", true);
            }

            foreach (var updateFile in managedFiles)
            {
                File.Copy(updateFile.FullName, $@"{extractLocation}\Assets\Managed\{updateFile.Name}", true);
            }

            foreach (var updateFile in requiredFiles)
            {
                File.Copy(updateFile.FullName, $@"{extractLocation}\Assets\Required\{updateFile.Name}", true);
            }

            File.Copy($@"{filesPath}\JaPatcher.exe", $@"{extractLocation}\JaPatcher.exe", true);
            break;
    }

    File.Delete($@"{currentDir}\JaPatcher.zip");
    Directory.Delete(filesPath, true);
}

Console.WriteLine("Update applied successfully! Closing in 5 seconds...");

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

