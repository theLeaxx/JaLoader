using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;    
using System.IO;
using System.Collections;
using Application = UnityEngine.Application;
using System.Text.RegularExpressions;
using BepInEx;
using JaLoader.BepInExWrapper;
using JaLoader.Common;

namespace JaLoader
{
    public class ModLoader : MonoBehaviour
    {
        #region Singleton
        public static ModLoader Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        #endregion

        private static readonly Regex BannedCharacters = new Regex(@"[_|]", RegexOptions.Compiled);

        private void Start()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                Assembly loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName == args.Name);
                if (loadedAssembly != null)
                    return loadedAssembly;

                // try to redirect the bepinex assembly to this assembly
                if (args.Name.StartsWith("BepInEx"))
                    return Assembly.GetExecutingAssembly();

                if (args.Name.StartsWith("Harmony"))
                    return Assembly.LoadFrom(Path.Combine(Application.dataPath, @"Managed\0Harmony.dll"));

                // replaced Theraot.Core with MonoMod.Backports, so redirect it to that
                if (args.Name.StartsWith("Theraot"))
                    return Assembly.LoadFrom(Path.Combine(Application.dataPath, @"Managed\MonoMod.Backports.dll"));

                return null;
            };
        }

        internal bool CheckIfModIsAlreadyLoaded(string path)
        {
            AssemblyName assemblyName;

            try
            {
                assemblyName = AssemblyName.GetAssemblyName(path);
            }
            catch (Exception)
            {
                return true;
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.FullName == assemblyName.FullName)
                    return true;

            return false;
        }

        internal bool InitializeMod(out MonoBehaviour outMod, FileInfo modFile = null, string certainModFile = "", Type certainType = null, GameObject certainObject = null)
        {
            bool isBepInExMod = false;
            outMod = null;

            if (modFile == null && certainModFile == "" && certainType == null)
            {
                Console.LogError("JaLoader", "No mod file provided for initialization.");
                return false;
            }

            if (certainModFile != "")
            {
                var path = Path.Combine(SettingsManager.ModFolderLocation, certainModFile);

                if (CheckIfModIsAlreadyLoaded(path) == true)
                {
                    Console.LogError("JaLoader", $"Mod {certainModFile} is already installed and loaded!");
                    UIManager.Instance.ShowNotice("MOD INSTALLATION FAILED", "The mod installation failed. The mod is already installed and loaded.", ignoreObstructRayChange: true, enableDontShowAgain: false);

                    return false;
                }

                modFile = new FileInfo(Path.Combine(SettingsManager.ModFolderLocation, $"{certainModFile}"));
            }

            try
            {
                Assembly modAssembly = null;
                Type modType = null;
                Type[] allModTypes = null;

                if (certainType != null)
                {
                    modType = certainType;
                }
                else
                {
                    modAssembly = Assembly.LoadFrom(modFile.FullName);
                    allModTypes = modAssembly.GetTypes();
                    modType = allModTypes.FirstOrDefault(t => t.BaseType != null && t.BaseType.Name == "Mod");
                }

                Debug.Log($"Loading mod {(modFile == null ? modType.Name : modFile.Name)}...");

                #region BepInEx Loading
                if (modType == null)
                {
                    modType = allModTypes.FirstOrDefault(t => t.BaseType != null && t.BaseType.Name == "BaseUnityPlugin");

                    if (modType == null)
                    {
                        if (allModTypes.FirstOrDefault(t => t.BaseType != null && t.BaseType.Name == "ClassicMod") != null)
                        {
                            Console.LogWarning($"Mod {modFile.Name} is designed for an older version of Jalopy (1.0) and is not compatible with your current game version.");
                            throw new ModException($"Mod {modFile.Name} is built for 1.0 and is not compatible with this version of the game.", null, 101);
                        }
                        else
                        {
                            Console.LogError($"Mod {modFile.Name} does not contain any class derived from Mod, ClassicMod or BaseUnityPlugin.");
                            throw new ModException($"No valid mod class found for mod {modFile.Name}.", null, 102);
                        }

                    }

                    isBepInExMod = true;

                    GameObject bix_ModObject = Instantiate(new GameObject());
                    bix_ModObject.transform.parent = null;
                    bix_ModObject.SetActive(false);
                    DontDestroyOnLoad(bix_ModObject);

                    Component bix_ModComponent = bix_ModObject.AddComponent(modType);
                    BaseUnityPlugin bix_mod = bix_ModObject.GetComponent<BaseUnityPlugin>();

                    ModInfo modInfo = bix_ModObject.AddComponent<ModInfo>();
                    string ModID = "";
                    string ModName = "";
                    string ModVersion = "";
                    string ModDescription = "";

                    Type type = bix_mod.GetType();
                    Type pluginInfoType = modType.Assembly.GetType($"{modType.Namespace}.PluginInfo, {modType.Assembly.GetName().Name}");
                    if (pluginInfoType != null)
                    {
                        ModID = (string)pluginInfoType.GetField("PLUGIN_GUID").GetValue(null);
                        ModName = (string)pluginInfoType.GetField("PLUGIN_NAME").GetValue(null);
                        ModVersion = (string)pluginInfoType.GetField("PLUGIN_VERSION").GetValue(null);
                    }
                    else if (Attribute.IsDefined(type, typeof(BepInPlugin)))
                    {
                        BepInPlugin bepInPlugin = (BepInPlugin)Attribute.GetCustomAttribute(type, typeof(BepInPlugin));

                        ModID = bepInPlugin.GUID;
                        ModName = bepInPlugin.Name;
                        ModVersion = bepInPlugin.Version;
                        object[] attributes = modType.Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                        if (attributes.Length > 0)
                        {
                            AssemblyDescriptionAttribute descriptionAttribute = (AssemblyDescriptionAttribute)attributes[0];
                            ModDescription = descriptionAttribute.Description;
                        }
                        else
                            ModDescription = "This mod has no description!";
                    }
                    else
                    {
                        ModID = ModName = modAssembly.GetName().Name;
                        ModVersion = modAssembly.GetName().Version.ToString();
                    }

                    modInfo.GUID = ModID;
                    modInfo.Name = ModName;
                    modInfo.Version = ModVersion;

                    bix_ModObject.name = $"BepInEx_CompatLayer_{ModID}";

                    string bix_modVersionText = ModVersion;
                    string bix_modName = SettingsManager.DebugMode ? $"{ModID} - BepInEx" : ModName;

                    string pattern = @"^[a-zA-Z]+(\.[a-zA-Z]+)+$";
                    string authorName = "BepInEx";

                    if (Regex.IsMatch(modInfo.GUID, pattern))
                        authorName = modInfo.GUID.Split('.')[1];

                    var genericBIXModData = new GenericModData(ModID, ModName, ModVersion, ModDescription, authorName, null, bix_mod, isBIXMod: true);
                    var BIXtext = UIManager.Instance.CreateModEntryReturnText(genericBIXModData);
                    ModManager.AddMod(bix_mod, WhenToInit.InMenu, BIXtext, genericBIXModData);

                    Debug.Log($"Part 1/2 of initialization for BepInEx mod {ModName} completed");

                    if (certainModFile != "")
                        ModManager.FinishLoadingMod(bix_mod);

                    outMod = bix_mod;
                }
                #endregion

                #region JaLoader Loading
                GameObject ModObject;
                if (certainObject != null)
                    ModObject = certainObject;
                else
                {
                    ModObject = Instantiate(new GameObject());
                    ModObject.transform.parent = null;
                    ModObject.SetActive(false);
                    DontDestroyOnLoad(ModObject);
                }

                Component ModComponent = ModObject.AddComponent(modType);
                Mod mod = ModObject.GetComponent<Mod>();

                if (mod.ModID == null || mod.ModName == null || mod.ModAuthor == null || mod.ModVersion == null || mod.ModID == string.Empty || mod.ModName == string.Empty || mod.ModAuthor == string.Empty || mod.ModVersion == string.Empty)
                {
                    Console.LogError(modFile.Name, $"{modFile.Name} contains no information related to its ID, name, author or version.");
                    throw new ModException("Invalid ModID/ModName/ModAuthor/ModVersion!", null, 100);
                }

                if (BannedCharacters.IsMatch(mod.ModID) || BannedCharacters.IsMatch(mod.ModName) || BannedCharacters.IsMatch(mod.ModAuthor))
                {
                    Console.LogError(modFile.Name, $"{modFile.Name} contains invalid characters (_ or |) in its ID, name or author. Please remove them and try again.");
                    throw new ModException("Invalid characters in ModID/ModName/ModAuthor!", null, 103);
                }

                ModObject.name = $"{mod.ModID}_{mod.ModAuthor}_{mod.ModName}";

                if (mod.UseAssets)
                {
                    mod.AssetsPath = $@"{SettingsManager.ModFolderLocation}\Assets\{mod.ModID}";

                    if (!Directory.Exists(mod.AssetsPath))
                        Directory.CreateDirectory(mod.AssetsPath);
                }


                string modVersionText = mod.ModVersion;
                string modName = SettingsManager.DebugMode ? mod.ModID : mod.ModName;

                var genericModData = new GenericModData(mod.ModID, mod.ModName, mod.ModVersion, mod.ModDescription, mod.ModAuthor, mod.GitHubLink, mod);
                var text = UIManager.Instance.CreateModEntryReturnText(genericModData);
                ModManager.AddMod(mod, mod.WhenToInit, text, genericModData);

                Debug.Log($"Part 1/2 of initialization for mod {mod.ModName} completed");

                if (certainModFile != "")
                {
                    if (mod.WhenToInit == WhenToInit.InMenu)
                        ModManager.FinishLoadingMod(mod);
                    else if (mod.WhenToInit == WhenToInit.InGame && SceneManager.GetActiveScene().buildIndex == 3)
                        ModManager.FinishLoadingMod(mod);
                    else
                        ModManager.FinishLoadingMod(mod, false);
                }

                outMod = mod;
                #endregion
            }
            catch (Exception ex)
            {
                Debug.Log($"Failed to initialize mod {modFile.Name}");
                Console.LogError("JaLoader", $"An error occured while trying to initialize mod \"{modFile.Name}\": ");

                var obj = UIManager.Instance.CreateModEntryReturnEntry(new GenericModData(modFile.Name, modFile.Name, "Failed to load", $"{modFile.Name} experienced an issue during loading and couldn't be initialized. You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.", "Unknown", "", null));
                UIManager.Instance.AddWarningToMod(obj, "Failed to load mod!", true);

                switch (ex)
                {
                    case FileNotFoundException fileException:
                        string[] parts = fileException.FileName.Split(',');

                        if (parts.Length >= 2)
                        {
                            string dllName = parts[0].Trim();
                            string versionSection = parts[1].Trim();

                            int versionIndex = versionSection.IndexOf("Version=");
                            if (versionIndex != -1)
                            {
                                string versionNumber = versionSection.Substring(versionIndex + "Version=".Length).Trim();

                                UIManager.Instance.AddWarningToMod(obj, $"Missing assembly: {dllName}", true);

                                string errorMessage = $"\"{modFile.Name}\" requires the following DLL: {dllName}, version {versionNumber}";
                                Debug.Log(errorMessage);
                                Console.LogError("JaLoader", errorMessage);
                                Console.LogError("JaLoader", "You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.");
                            }
                        }
                        break;

                    case ModException modLoadException:
                        UIManager.Instance.AddWarningToMod(obj, $"Mod Load Exception: {modLoadException.Message}", true);
                        Debug.LogError($"Mod Load Exception: {modLoadException.Message}");
                        Console.LogError("JaLoader", $"Mod Load Exception: {modLoadException.Message}");
                        break;

                    default:
                        Console.LogError("/", ex);
                        Debug.Log(ex);
                        Debug.Log("You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.");
                        Console.LogError("JaLoader", "You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.");

                        if (isBepInExMod)
                            Console.LogWarning("JaLoader", "Please report this issue to the JaLoader GitHub page, making sure to upload your 'output_log.txt' file and applying the BepInEx label!");

                        break;
                }

                if (certainModFile != "")
                    UIManager.Instance.ShowNotice("MOD INSTALLATION FAILED", "The mod installation failed. Please make sure you have the correct URL and that your internet connection is stable.", ignoreObstructRayChange: true, enableDontShowAgain: false);

                return false;
            }

            if (certainModFile != "")
            {
                UIManager.Instance.ShowNotice("MOD INSTALLED", "The mod has been successfully installed. You can now enable it in the mods list.", ignoreObstructRayChange: true, enableDontShowAgain: false);
                UIManager.Instance.ModsCountText.text = $"{ModManager.Mods.Count} mods installed";
                Console.Log("JaLoader", $"Mod {certainModFile} has been successfully installed and loaded!");
            }

            return true;
        }

        public IEnumerator InitializeMods()
        {
            while (!ReferencesLoader.Instance.canLoadMods)
                yield return null;

            DebugUtils.SignalStartInit();

            DirectoryInfo d = new DirectoryInfo(SettingsManager.ModFolderLocation);
            FileInfo[] mods = d.GetFiles("*.dll");

            foreach (FileInfo modFile in mods)
                InitializeMod(out MonoBehaviour outMod, modFile: modFile);

            GetComponent<LoadingScreen>().DeleteLoadingScreen();

            DebugUtils.SignalFinishedInit();

            EventsManager.Instance.OnModsInit();

            yield return null;
        }

        internal IEnumerator ReloadAllMods()
        {
            Dictionary<GameObject, Type> modObjAndType = new Dictionary<GameObject, Type>();
            List<ModDataForManager> modsToRemove = new List<ModDataForManager>();
            List<Mod> reloadedMods = new List<Mod>();

            foreach (var script in ModManager.Mods)
                if (script.Value.InitTime == WhenToInit.InGame)
                {
                    var mod = (Mod)script.Key;

                    modsToRemove.Add(script.Value);
                    modObjAndType.Add(mod.gameObject, mod.GetType());

                    Destroy(mod);
                    if (UIManager.Instance.ModsSettingsContent.Find($"{mod.ModAuthor}_{mod.ModID}_{mod.ModName}-SettingsHolder"))
                        Destroy(UIManager.Instance.ModsSettingsContent.Find($"{mod.ModAuthor}_{mod.ModID}_{mod.ModName}-SettingsHolder").gameObject);

                    Destroy(UIManager.Instance.modEntries[script.Value.GenericModData]);
                    UIManager.Instance.modEntries.Remove(script.Value.GenericModData);

                    Console.Instance.RemoveCommandsFromMod(mod);
                }

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            foreach (var toRemove in modsToRemove)
            {
                foreach (var mod in ModManager.Mods)
                {
                    if (mod.Value.GenericModData == toRemove.GenericModData)
                    {
                        ModManager.Mods.Remove(mod.Key);
                        ModManager.loadOrderList.Remove(mod.Key);
                        break;
                    }
                }

                foreach (KeyValuePair<GameObject, Type> modObj in modObjAndType)
                {
                    InitializeMod(out MonoBehaviour mod, certainType: modObj.Value, certainObject: modObj.Key);
                    reloadedMods.Add((Mod)mod);
                }

                ModManager.LoadModOrder(false);
                ModManager.reloadGameModsRequired = false;
                ModManager.LoadDisabledStatus();

                CustomObjectsManager.Instance.ignoreAlreadyExists = true;
                foreach (var mod in reloadedMods)
                {
                    if (ModManager.Mods[mod].GenericModData.IsEnabled == true)
                    {
                        try
                        {
                            mod.OnReload();

                            ModManager.FinishLoadingMod(mod);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                CustomObjectsManager.Instance.ignoreAlreadyExists = false;

                yield break;
            }   
        }
    }
}
