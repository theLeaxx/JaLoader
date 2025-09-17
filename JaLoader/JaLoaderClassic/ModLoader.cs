using JaLoader.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace JaLoaderClassic
{
    public class ModLoader : MonoBehaviour, IModLoader
    {
        private void Start()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                Console.InternalLogDebug("JaLoader", $"Resolving assembly: {args.Name}");

                Assembly loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName == args.Name);
                if (loadedAssembly != null)
                    return loadedAssembly;

                // try to redirect the bepinex assembly to this assembly
                if (args.Name.StartsWith("BepInEx"))
                    return Assembly.GetExecutingAssembly();

                if (args.Name.StartsWith("Harmony"))
                    return Assembly.LoadFrom(Path.Combine(Application.dataPath, @"Managed\0Harmony.dll"));

                // replaced Theraot.Core with something else, so redirect it to that
                if (args.Name.StartsWith("Theraot"))
                    return Assembly.LoadFrom(Path.Combine(Application.dataPath, @"Managed\ValueTupleBridge.dll"));

                // redirect MonoMod.Backports to ValueTupleBridge.dll
                if (args.Name.StartsWith("MonoMod.Back"))
                    return Assembly.LoadFrom(Path.Combine(Application.dataPath, @"Managed\ValueTupleBridge.dll"));

                return null;
            };
        }

        public void StartInitializeMods()
        {
            StartCoroutine(InitializeMods());
        }

        internal bool InitializeMod(out MonoBehaviour outMod, FileInfo modFile = null, string certainModFile = "", Type certainType = null, GameObject certainObject = null)
        {
            bool isBepInExMod = false;
            outMod = null;

            if (modFile == null && certainModFile == "" && certainType == null)
            {
                Console.InternalLogError("JaLoader", "No mod file provided for initialization.");
                return false;
            }

            if (certainModFile != "")
            {
                /*var path = Path.Combine(JaLoaderSettings.ModFolderLocation, certainModFile);

                if (CheckIfModIsAlreadyLoaded(path) == true)
                {
                    Console.InternalLogError("JaLoader", $"Mod {certainModFile} is already installed and loaded!");
                    UIManager.Instance.ShowNotice("MOD INSTALLATION FAILED", "The mod installation failed. The mod is already installed and loaded.", ignoreObstructRayChange: true, enableDontShowAgain: false);

                    return false;
                }

                modFile = new FileInfo(Path.Combine(JaLoaderSettings.ModFolderLocation, $"{certainModFile}"));*/
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
                    modType = allModTypes.FirstOrDefault(t => t.BaseType != null && t.BaseType.Name == "ModClassic");
                }

                Debug.Log($"Loading mod {(modFile == null ? modType.Name : modFile.Name)}...");

                #region BepInEx Loading
                if (modType == null)
                {
                    modType = allModTypes.FirstOrDefault(t => t.BaseType != null && t.BaseType.Name == "BaseUnityPlugin");

                    if (modType == null)
                    {
                        if (allModTypes.FirstOrDefault(t => t.BaseType != null && t.BaseType.Name == "Mod") != null)
                        {
                            Console.InternalLogWarning("JaLoader", $"Mod {modFile.Name} is designed for a newer version of Jalopy (1.105) and is not compatible with your current game version.");
                            throw new ModException($"Mod {modFile.Name} is built for 1.105 and is not compatible with this version of the game.", null, 101);
                        }
                        else
                        {
                            Console.InternalLogError("JaLoader", $"Mod {modFile.Name} does not contain any class derived from Mod, ModClassic or BaseUnityPlugin.");
                            throw new ModException($"No valid mod class found for mod {modFile.Name}.", null, 102);
                        }
                    }
                }
                #endregion

                #region JaLoader Loading
                GameObject ModObject;
                if (certainObject != null)
                    ModObject = certainObject;
                else
                {
                    ModObject = (GameObject)Instantiate(new GameObject());
                    ModObject.transform.parent = null;
                    ModObject.SetActive(false);
                    DontDestroyOnLoad(ModObject);
                }

                Component ModComponent = ModObject.AddComponent(modType);
                ModClassic mod = ModObject.GetComponent<ModClassic>();

                if (mod.ModID == null || mod.ModName == null || mod.ModAuthor == null || mod.ModVersion == null || mod.ModID == string.Empty || mod.ModName == string.Empty || mod.ModAuthor == string.Empty || mod.ModVersion == string.Empty)
                {
                    Console.InternalLogError(modFile.Name, $"{modFile.Name} contains no information related to its ID, name, author or version.");
                    throw new ModException("Invalid ModID/ModName/ModAuthor/ModVersion!", null, 100);
                }

                if (CoreUtils.BannedCharacters.IsMatch(mod.ModID) || CoreUtils.BannedCharacters.IsMatch(mod.ModName) || CoreUtils.BannedCharacters.IsMatch(mod.ModAuthor))
                {
                    Console.InternalLogError(modFile.Name, $"{modFile.Name} contains invalid characters (_ or |) in its ID, name or author. Please remove them and try again.");
                    throw new ModException("Invalid characters in ModID/ModName/ModAuthor!", null, 103);
                }

                ModObject.name = $"{mod.ModID}_{mod.ModAuthor}_{mod.ModName}";

                if (mod.UseAssets)
                {
                    mod._assetsPath = $@"{JaLoaderSettings.ModFolderLocation}\Assets\{mod.ModID}";

                    if (!Directory.Exists(mod.AssetsPath))
                        Directory.CreateDirectory(mod.AssetsPath);
                }

                string modVersionText = mod.ModVersion;
                string modName = JaLoaderSettings.DebugMode ? mod.ModID : mod.ModName;

                var genericModData = new GenericModData(mod.ModID, mod.ModName, mod.ModVersion, mod.ModDescription, mod.ModAuthor, mod.GitHubLink, mod);
                //var text = UIManager.Instance.CreateModEntryReturnText(genericModData);
                ModManager.AddMod(mod, mod.WhenToInitMod, /*text,*/ genericModData);

                Debug.Log($"Part 1/2 of initialization for mod {mod.ModName} completed");

                if (certainModFile != "")
                {
                    if (mod.WhenToInitMod == WhenToInit.InMenu)
                        ModManager.FinishLoadingMod(mod);
                    else if (mod.WhenToInitMod == WhenToInit.InGame && Application.loadedLevel == 3)
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
                Console.InternalLogError("JaLoader", $"An error occured while trying to initialize mod \"{modFile.Name}\": ");

                //var obj = UIManager.Instance.CreateModEntryReturnEntry(new GenericModData(modFile.Name, modFile.Name, "Failed to load", $"{modFile.Name} experienced an issue during loading and couldn't be initialized. You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.", "Unknown", "", null));
                //UIManager.Instance.AddWarningToMod(obj, "Failed to load mod!", true);

                string errorMessage;

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

                                //UIManager.Instance.AddWarningToMod(obj, $"Missing assembly: {dllName}", true);

                                errorMessage = $"\"{modFile.Name}\" requires the following DLL: {dllName}, version {versionNumber}";
                                Debug.Log(errorMessage);
                                Console.InternalLogError("JaLoader", errorMessage);
                                Console.InternalLogError("JaLoader", "You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.");
                            }
                        }
                        break;

                    case ReflectionTypeLoadException typeLoadException:
                        errorMessage = $"{modFile.Name} has a type load exception. This may be caused by a missing reference. Contact the mod author.";
                        //UIManager.Instance.AddWarningToMod(obj, errorMessage, true);
                        Debug.LogError(errorMessage);
                        Debug.LogError(typeLoadException);
                        Console.InternalLogError("JaLoader", errorMessage);
                        Console.InternalLogError("JaLoader", "You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.");
                        break;

                    case ModException modLoadException:
                        //UIManager.Instance.AddWarningToMod(obj, $"Mod Load Exception: {modLoadException.Message}", true);
                        Debug.LogError($"Mod Load Exception: {modLoadException.Message}");
                        Console.InternalLogError("JaLoader", $"Mod Load Exception: {modLoadException.Message}");
                        break;

                    default:
                        Console.InternalLogError("JaLoader", ex);
                        Debug.Log(ex);
                        Debug.Log("You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.");
                        Console.InternalLogError("JaLoader", "You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.");

                        if (isBepInExMod)
                            Console.InternalLogWarning("JaLoader", "Please report this issue to the JaLoader GitHub page, making sure to upload your 'output_log.txt' file and applying the BepInEx label!");

                        break;
                }

                //if (certainModFile != "")
                //    UIManager.Instance.ShowNotice("MOD INSTALLATION FAILED", "The mod installation failed. Please make sure you have the correct URL and that your internet connection is stable.", ignoreObstructRayChange: true, enableDontShowAgain: false);

                ModManager.ModFilesCount++;

                return false;
            }

            if (certainModFile != "")
            {
                //UIManager.Instance.ShowNotice("MOD INSTALLED", "The mod has been successfully installed. You can now enable it in the mods list.", ignoreObstructRayChange: true, enableDontShowAgain: false);
                //UIManager.Instance.ModsCountText.text = $"{ModManager.Mods.Count} mods installed";
                Console.InternalLog("JaLoader", $"Mod {certainModFile} has been successfully installed and loaded!");
            }

            return true;
        }


        public IEnumerator InitializeMods()
        {
            while (!ReferencesLoader.CanLoadMods)
                yield return null;

            DebugUtils.SignalStartInit();

            DirectoryInfo d = new DirectoryInfo(JaLoaderSettings.ModFolderLocation);
            FileInfo[] mods = d.GetFiles("*.dll");
            foreach (FileInfo modFile in mods)
                InitializeMod(out _, modFile: modFile);

            //GetComponent<LoadingScreen>().DeleteLoadingScreen();

            DebugUtils.SignalFinishedInit();
            ModManager.LoadMods();

            //EventsManager.Instance.OnModsInit();

            yield return null;
        }

    }
}
