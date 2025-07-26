using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;    
using System.IO;
using System.Collections;
using Application = UnityEngine.Application;
using Process = System.Diagnostics.Process;
using System.Text.RegularExpressions;
using BepInEx;
using JaLoader.BepInExWrapper;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using static UnityEngine.EventSystems.EventTrigger;
using System.CodeDom;
using System.IO.Pipes;

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

        private UIManager uiManager;
        private bool reloadMods;

        private static readonly Regex BannedCharacters = new Regex(@"[_|]", RegexOptions.Compiled);

        private void Start()
        {
            EventsManager.Instance.OnGameLoad += OnGameLoad;
            EventsManager.Instance.OnGameUnload += OnGameUnload;
            EventsManager.Instance.OnMenuLoad += OnMenuLoad;

            uiManager = UIManager.Instance;
        }

        private void OnGameLoad()
        {
            if (!reloadMods)
                return;

            StartCoroutine(WaitThenReload());
        }

        private void OnMenuLoad()
        {
            if (!reloadMods)
                return;

            StartCoroutine(WaitThenReload());
        }

        private IEnumerator WaitThenReload()
        {
            /*while (ModHelper.Instance.laika == null)
                yield return null;

            StartCoroutine(ReloadAllMods());*/

            yield break;
        }

        public void ReloadMods()
        {
            //StartCoroutine(ReloadAllMods());
        }

        private void OnGameUnload() 
        {
            /*reloadMods = true;

            if (InitializedInGameMods)
            {
                //finishedInitializingPartOneMods = false;
                
                foreach (Mod mod in modsInitInGame.ToList())
                {
                    mod.gameObject.SetActive(false);
                }

                InitializedInGameMods = false;
            }*/
        }

        private void Update()
        {
           /* if (modsNumber == 0)
                return;

            if (finishedInitializingPartOneMods && !reloadMods)
            {
                if (!LoadedDisabledMods && SettingsManager.DisabledMods.Count != 0)
                {
                    for (int i = 0; i < modsInitInGame.Count; i++)
                    {
                        string reference = $"{modsInitInGame.ToArray()[i].ModAuthor}_{modsInitInGame.ToArray()[i].ModID}_{modsInitInGame.ToArray()[i].ModName}";
                        if (SettingsManager.DisabledMods.Contains(reference))
                        {
                            disabledMods.Add(modsInitInGame.ToArray()[i]);
                        }
                    }

                    for (int i = 0; i < modsInitInMenuIncludingBIX.Count; i++)
                    {
                        if (modsInitInMenuIncludingBIX.ToArray()[i] is Mod mod)
                        {
                            string reference = $"{mod.ModAuthor}_{mod.ModID}_{mod.ModName}";
                            if (SettingsManager.DisabledMods.Contains(reference))
                            {
                                disabledMods.Add(mod);
                            }
                        }
                        else if (modsInitInMenuIncludingBIX.ToArray()[i] is BaseUnityPlugin bix_mod)
                        {
                            ModInfo modInfo = bix_mod.gameObject.GetComponent<ModInfo>();

                            string reference = $"BepInEx_CompatLayer_{modInfo.GUID}";
                            if (SettingsManager.DisabledMods.Contains(reference))
                            {
                                disabledMods.Add(bix_mod);
                            }
                        }
                    }

                    for (int i = 0; i < disabledMods.Count; i++)
                    {
                        if (modStatusTextRef.ContainsKey(disabledMods.ToArray()[i]))
                        {
                            modStatusTextRef[disabledMods.ToArray()[i]].text = "Enable";
                            modStatusTextRef[disabledMods.ToArray()[i]].transform.parent.parent.parent.Find("BasicInfo").Find("ModName").GetComponent<Text>().color = defaultGrayColor;
                            modStatusTextRef[disabledMods.ToArray()[i]].transform.parent.parent.parent.Find("BasicInfo").Find("ModAuthor").GetComponent<Text>().color = defaultGrayColor;
                        }
                    }

                    LoadedDisabledMods = true;
                }

                List<MonoBehaviour> modsToRemoveAfter = new List<MonoBehaviour>();

                if (!InitializedInMenuMods)
                {
                    string message = $"{modsNumber} mods found!";

                    if (disabledMods.Count > 0 || modsNeedUpdate > 0 || bepinexModsNumber > 0)
                    {
                        message += " (";

                        if (disabledMods.Count > 0)
                        {
                            message += $"{disabledMods.Count} disabled";
                        }

                        if (modsNeedUpdate > 0)
                        {
                            if (disabledMods.Count > 0)
                            {
                                message += ", ";
                            }
                            message += $"{modsNeedUpdate} updates available";
                        }

                        if (bepinexModsNumber > 0)
                        {
                            if (disabledMods.Count > 0 || modsNeedUpdate > 0)
                            {
                                message += ", ";
                            }
                            message += $"{bepinexModsNumber} BepInEx mods";
                        }

                        message += ")";
                    }

                    uiManager.modsCountText.text = $"{modsNumber} mods installed";

                    Console.Log("JaLoader", message);

                    foreach (MonoBehaviour monoBehaviour in modsInitInMenuIncludingBIX)
                    {
                        if (monoBehaviour is Mod mod)
                        {
                            CheckForDependencies(mod);

                            CheckForIncompatibilities(mod);

                            try
                            {
                                mod.EventsDeclaration();

                                mod.SettingsDeclaration();

                                mod.CustomObjectsRegistration();

                                if (mod.settingsIDS.Count > 0)
                                {
                                    mod.LoadModSettings();
                                    mod.SaveModSettings();
                                }

                                Debug.Log($"Part 2/2 of initialization for mod {mod.ModName} completed");

                                if (!disabledMods.Contains(mod))
                                    mod.gameObject.SetActive(true);

                                Debug.Log($"Loaded mod {mod.ModName}");
                            }
                            catch (Exception ex)
                            {
                                mod.gameObject.SetActive(false);

                                Debug.Log($"Part 2/2 of initialization for mod {mod.ModName} failed");
                                Debug.Log($"Failed to load mod {mod.ModName}. An error occoured while enabling the mod.");
                                Debug.Log(ex);

                                Console.LogError("JaLoader", $"An error occured while trying to load mod \"{mod.ModName}\"");

                                modsToRemoveAfter.Add(mod);

                                continue;
                                throw;
                            }             
                        }
                        else if(monoBehaviour is BaseUnityPlugin bix_mod)
                        {
                            ModInfo modInfo = bix_mod.gameObject.GetComponent<ModInfo>();

                            try
                            {
                                bix_mod.InstantiateBIXPluginSettings();

                                Debug.Log($"Part 2/2 of initialization for BepInEx mod {modInfo.Name} completed");

                                if (!disabledMods.Contains(bix_mod))
                                    bix_mod.gameObject.SetActive(true);

                                foreach (IConfigEntry entry in bix_mod.configEntries.Keys)
                                {
                                    if (entry is ConfigEntry<bool> boolEntry)
                                    {
                                        bool value = boolEntry._typedValue;
                                        bix_mod.AddBIXPluginToggle(bix_mod.configEntries[entry].Item1, bix_mod.configEntries[entry].Item2, value);
                                    }
                                    else if (entry is ConfigEntry<KeyboardShortcut> keyEntry)
                                    {
                                        KeyboardShortcut keyboardShortcut = keyEntry._typedValue;
                                        bix_mod.AddBIXPluginKeybind(bix_mod.configEntries[entry].Item1, bix_mod.configEntries[entry].Item2, keyboardShortcut.Key);
                                    }
                                }

                                if (bix_mod.configEntries.Count > 0)
                                    bix_mod.LoadBIXPluginSettings();

                                Debug.Log($"Loaded BepInEx mod {modInfo.Name}");

                            }
                            catch (Exception ex)
                            {
                                bix_mod.gameObject.SetActive(false);

                                Debug.Log($"Part 2/2 of initialization for BepInEx mod {modInfo.Name} failed");
                                Debug.Log($"Failed to load BepInEx mod {modInfo.Name}. An error occoured while enabling the mod.");
                                Debug.Log(ex);

                                Console.LogError("JaLoader", $"An error occured while trying to load BepInEx mod \"{modInfo.name}\"");

                                modsToRemoveAfter.Add(bix_mod);

                                continue;
                                throw;
                            }
                        }
                    }

                    foreach (Mod mod in modsInitInGame)
                    {
                        CheckForDependencies(mod);

                        CheckForIncompatibilities(mod);

                        try
                        {
                            mod.EventsDeclaration();

                            mod.SettingsDeclaration();

                            mod.CustomObjectsRegistration();

                            if (mod.settingsIDS.Count > 0)
                            {
                                mod.LoadModSettings();
                                mod.SaveModSettings();
                            }

                            Debug.Log($"Part 2/2 of initialization for mod {mod.ModName} completed");
                        }
                        catch (Exception ex)
                        {
                            mod.gameObject.SetActive(false);

                            Debug.Log($"Part 2/2 of initialization for mod {mod.ModName} failed");
                            Debug.Log($"Failed to load mod {mod.ModName}. An error occoured while enabling the mod.");
                            Debug.Log(ex);

                            Console.LogError("JaLoader", $"An error occured while trying to load mod \"{mod.ModName}\"");

                            modsToRemoveAfter.Add(mod);
                            continue;
                            throw;
                        }
                    }

                    foreach (MonoBehaviour mod in modsToRemoveAfter)
                    {
                        if(modsInitInGame.Contains((Mod)mod))
                            modsInitInGame.Remove((Mod)mod);
                        else if(modsInitInMenuIncludingBIX.Contains(mod))
                            modsInitInMenuIncludingBIX.Remove(mod);
                    }

                    Stopwatch.Instance.StopCounting();
                    Debug.Log($"Loaded JaLoader mods! ({Stopwatch.Instance.timePassed}s)");
                    Debug.Log($"JaLoader successfully loaded! ({Stopwatch.Instance.totalTimePassed}s)");
                    finishedInitializingPartTwoMods = true;
                    if(Stopwatch.Instance != null)
                        Destroy(Stopwatch.Instance);

                    InitializedInMenuMods = true;
                }

                if (!InitializedInGameMods && SceneManager.GetActiveScene().buildIndex > 2)
                {
                    foreach (Mod mod in modsInitInGame)
                    {
                        if (!disabledMods.Contains(mod))
                            mod.gameObject.SetActive(true);

                        Debug.Log($"Loaded mod {mod.ModName}");
                    }

                    InitializedInGameMods = true;
                }
            }*/       
        }

        private void CheckForIncompatibilities(Mod mod)
        {
            /*if (mod.Incompatibilities.Count > 0)
            {
                foreach (var incompatibility in mod.Incompatibilities)
                {
                    Mod incompatibleMod = FindMod(incompatibility.Item2, incompatibility.Item1);
                    if (incompatibleMod != null)
                    {
                        //version is of format x.y.z-a.b.c, representing a range of versions from x.y.z to a.b.c
                        // separate it into 2 ints, one for the lower bound and one for the upper bound

                        string[] version = incompatibility.Item3.Split('-');
                        int lowerBound = int.Parse(version[0].Replace(".", ""));
                        int upperBound = int.Parse(version[1].Replace(".", ""));
                        int modVersion = int.Parse(incompatibleMod.ModVersion.Replace(".", ""));

                        if (modVersion >= lowerBound && modVersion <= upperBound)
                        {
                            uiManager.ShowNotice("Incompatibility detected", $"The mod \"{mod.ModName}\" is incompatible with the mod \"{incompatibleMod.ModName}\". The mod may still load, but not function correctly.");
                            uiManager.AddWarningToMod(modStatusTextRef[mod].transform.parent.parent.parent.Find("WarningIcon").gameObject, $"Incompatible with {incompatibleMod.ModName}!");
                        }
                    }
                }
            }*/
        }

        private void CheckForDependencies(Mod mod)
        {
            /*if (mod.Dependencies.Count > 0)
            {
                foreach (var dependency in mod.Dependencies)
                {
                    int version = int.Parse(dependency.Item3.Replace(".", ""));

                    if (dependency.Item1 == "JaLoader" && dependency.Item2 == "Leaxx")
                    {
                        if(SettingsManager.GetVersion() < version)
                        {
                            uiManager.ShowNotice("Dependency required", $"The mod \"{mod.ModName}\" requires JaLoader version {dependency.Item3} or higher. You are currently using version {SettingsManager.GetVersionString()}. The mod may still load, but not function correctly.");

                            uiManager.AddWarningToMod(modStatusTextRef[mod].transform.parent.parent.parent.Find("WarningIcon").gameObject, $"Requires JaLoader >= {dependency.Item3}!");
                        }
                    }
                    else
                    {
                        Mod dependentMod = FindMod(dependency.Item2, dependency.Item1);

                        if (dependentMod == null)
                        {
                            uiManager.ShowNotice("Dependency required", $"The mod \"{mod.ModName}\" requires the mod \"{dependency.Item1}\" to be installed. The mod may still load, but not function correctly.");    
                        
                            uiManager.AddWarningToMod(modStatusTextRef[mod].transform.parent.parent.parent.Find("WarningIcon").gameObject, $"Requires {dependency.Item1}!");
                        }
                        else
                        {
                            if (dependentMod.ModVersion != dependency.Item3)
                            {
                                uiManager.ShowNotice("Dependency required", $"The mod \"{mod.ModName}\" requires the mod \"{dependency.Item1}\" to be version {dependency.Item3} or higher. You are currently using version {FindMod(dependency.Item2, dependency.Item1).ModVersion}. The mod may still load, but not function correctly.");

                                uiManager.AddWarningToMod(modStatusTextRef[mod].transform.parent.parent.parent.Find("WarningIcon").gameObject, $"Requires {dependency.Item1} >= {dependency.Item3}!");
                            }

                            if (modsInitInMenuIncludingBIX.IndexOf(dependentMod) > modsInitInMenuIncludingBIX.IndexOf(mod))
                            {
                                uiManager.ShowNotice("Dependency required", $"The mod \"{mod.ModName}\" requires the mod \"{dependency.Item1}\" to be loaded before it. Adjust its load order in the mods list.");

                                uiManager.AddWarningToMod(modStatusTextRef[mod].transform.parent.parent.parent.Find("WarningIcon").gameObject, $"Requires {dependency.Item1} to be loaded before it!");
                            }
                            else if(modsInitInGame.IndexOf(dependentMod) > modsInitInGame.IndexOf(mod))
                            {
                                uiManager.ShowNotice("Dependency required", $"The mod \"{mod.ModName}\" requires the mod \"{dependency.Item1}\" to be loaded before it. Adjust its load order in the mods list.");

                                uiManager.AddWarningToMod(modStatusTextRef[mod].transform.parent.parent.parent.Find("WarningIcon").gameObject, $"Requires {dependency.Item1} to be loaded before it!");
                            }
                        }
                    }
                }
            }*/
        }

        public IEnumerator InitializeMods(string certainModFile = "")
        {
            while (!ReferencesLoader.Instance.canLoadMods)
                yield return null;

            Debug.Log("Initializing JaLoader mods...");
            Stopwatch.Instance.StartCounting();

            DirectoryInfo d = new DirectoryInfo(SettingsManager.ModFolderLocation);
            FileInfo[] mods = d.GetFiles("*.dll");

            if(certainModFile != "")
                mods = new FileInfo[] { new FileInfo(Path.Combine(SettingsManager.ModFolderLocation, $"{certainModFile}")) };

            int validMods = mods.Length;
            bool errorOccured = false;

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                Assembly loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName == args.Name);
                if (loadedAssembly != null)
                {
                    return loadedAssembly;
                }

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

            foreach (FileInfo modFile in mods)
            {
                bool isBepInExMod = false;

                try
                {
                    Assembly modAssembly = Assembly.LoadFrom(modFile.FullName);
                    Type[] allModTypes = modAssembly.GetTypes();
                    Type modType = allModTypes.FirstOrDefault(t => t.BaseType != null && t.BaseType.Name == "Mod");

                    #region BepInEx Loading
                    if (modType == null)
                    {
                        modType = allModTypes.FirstOrDefault(t => t.BaseType != null && t.BaseType.Name == "BaseUnityPlugin");

                        if (modType == null)
                        {
                            if(allModTypes.FirstOrDefault(t => t.BaseType != null && t.BaseType.Name == "ClassicMod") != null) 
                            {
                                Console.LogWarning($"Mod {modFile.Name} is designed for an older version of Jalopy (1.0) and is not compatible with your current game version.");
                                throw new ModLoadException($"Mod {modFile.Name} is built for 1.0 and is not compatible with this version of the game.", null, 101);
                            }
                            else
                            {
                                Console.LogError($"Mod {modFile.Name} does not contain any class derived from Mod, ClassicMod or BaseUnityPlugin.");
                                throw new ModLoadException($"No valid mod class found for mod {modFile.Name}.", null, 102);
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

                        if(Regex.IsMatch(modInfo.GUID, pattern))
                            authorName = modInfo.GUID.Split('.')[1];

                        var genericBIXModData = new GenericModData(ModID, ModName, ModVersion, ModDescription, authorName, null, bix_mod, isBIXMod: true);
                        var BIXtext = uiManager.CreateModEntryReturnText(genericBIXModData);
                        ModManager.AddMod(bix_mod, WhenToInit.InMenu, BIXtext, genericBIXModData);

                        Debug.Log($"Part 1/2 of initialization for BepInEx mod {ModName} completed");
                        if (certainModFile != "")
                        {
                            try
                            {
                                bix_mod.InstantiateBIXPluginSettings();

                                Debug.Log($"Part 2/2 of initialization for BepInEx mod {modInfo.Name} completed");

                                ModDataForManager data;
                                if (ModManager.Mods.TryGetValue(bix_mod, out data) && data.IsEnabled == false)
                                    bix_mod.gameObject.SetActive(true);

                                foreach (IConfigEntry entry in bix_mod.configEntries.Keys)
                                {
                                    if (entry is ConfigEntry<bool> boolEntry)
                                    {
                                        bool value = boolEntry._typedValue;
                                        bix_mod.AddBIXPluginToggle(bix_mod.configEntries[entry].Item1, bix_mod.configEntries[entry].Item2, value);
                                    }
                                    else if (entry is ConfigEntry<KeyboardShortcut> keyEntry)
                                    {
                                        KeyboardShortcut keyboardShortcut = keyEntry._typedValue;
                                        bix_mod.AddBIXPluginKeybind(bix_mod.configEntries[entry].Item1, bix_mod.configEntries[entry].Item2, keyboardShortcut.Key);
                                    }
                                }

                                if (bix_mod.configEntries.Count > 0)
                                    bix_mod.LoadBIXPluginSettings();

                                Debug.Log($"Loaded BepInEx mod {modInfo.Name}");

                            }
                            catch (Exception ex)
                            {
                                bix_mod.gameObject.SetActive(false);

                                Debug.Log($"Part 2/2 of initialization for BepInEx mod {modInfo.Name} failed");
                                Debug.Log($"Failed to load BepInEx mod {modInfo.Name}. An error occoured while enabling the mod.");
                                Debug.Log(ex);

                                Console.LogError("JaLoader", $"An error occured while trying to load BepInEx mod \"{modInfo.name}\"");

                                ModManager.Mods.Remove(bix_mod);

                                continue;
                                throw;
                            }
                        }

                        continue;
                    }
                    #endregion

                    #region JaLoader Loading
                    GameObject ModObject = Instantiate(new GameObject());
                    ModObject.transform.parent = null;
                    ModObject.SetActive(false);
                    DontDestroyOnLoad(ModObject);

                    Component ModComponent = ModObject.AddComponent(modType);
                    Mod mod = ModObject.GetComponent<Mod>();
                    if (mod.ModID == null || mod.ModName == null || mod.ModAuthor == null || mod.ModVersion == null || mod.ModID == string.Empty || mod.ModName == string.Empty || mod.ModAuthor == string.Empty || mod.ModVersion == string.Empty)
                    {
                        Console.LogError(modFile.Name, $"{modFile.Name} contains no information related to its ID, name, author or version.");
                        throw new ModLoadException("Invalid ModID/ModName/ModAuthor/ModVersion!", null, 100);
                    }

                    if(BannedCharacters.IsMatch(mod.ModID) || BannedCharacters.IsMatch(mod.ModName) || BannedCharacters.IsMatch(mod.ModAuthor))
                    {
                        Console.LogError(modFile.Name, $"{modFile.Name} contains invalid characters (_ or |) in its ID, name or author. Please remove them and try again.");
                        throw new ModLoadException("Invalid characters in ModID/ModName/ModAuthor!", null, 103);
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
                    var text = uiManager.CreateModEntryReturnText(genericModData);
                    ModManager.AddMod(mod, mod.WhenToInit, text, genericModData);

                    Debug.Log($"Part 1/2 of initialization for mod {mod.ModName} completed");

                    if (certainModFile != "")
                    {
                        CheckForDependencies(mod);

                        CheckForIncompatibilities(mod);

                        try
                        {
                            mod.EventsDeclaration();

                            mod.SettingsDeclaration();

                            mod.CustomObjectsRegistration();

                            if (mod.settingsIDS.Count > 0)
                            {
                                mod.LoadModSettings();
                                mod.SaveModSettings();
                            }

                            Debug.Log($"Part 2/2 of initialization for mod {mod.ModName} completed");

                            if (mod.WhenToInit == WhenToInit.InMenu)
                            {
                                mod.gameObject.SetActive(true);
                                Debug.Log($"Loaded mod {mod.ModName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            mod.gameObject.SetActive(false);

                            Debug.Log($"Part 2/2 of initialization for mod {mod.ModName} failed");
                            Debug.Log($"Failed to load mod {mod.ModName}. An error occoured while enabling the mod.");
                            Debug.Log(ex);

                            Console.LogError("JaLoader", $"An error occured while trying to load mod \"{mod.ModName}\"");

                            ModManager.Mods.Remove(mod);

                            continue;
                            throw;
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    errorOccured = true;
                    validMods--;

                    Debug.Log($"Failed to initialize mod {modFile.Name}");
                    Console.LogError("JaLoader", $"An error occured while trying to initialize mod \"{modFile.Name}\": ");

                    var obj = uiManager.CreateModEntryReturnEntry(new GenericModData(modFile.Name, modFile.Name, "Failed to load", $"{modFile.Name} experienced an issue during loading and couldn't be initialized. You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.", "Unknown", "", null));
                    uiManager.AddWarningToMod(obj, "Failed to load mod!", true);

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

                                    uiManager.AddWarningToMod(obj, $"Missing assembly: {dllName}", true);

                                    string errorMessage = $"\"{modFile.Name}\" requires the following DLL: {dllName}, version {versionNumber}";
                                    Debug.Log(errorMessage);
                                    Console.LogError("JaLoader", errorMessage);
                                    Console.LogError("JaLoader", "You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.");
                                }
                            }
                            break;

                        case ModLoadException modLoadException:
                            uiManager.AddWarningToMod(obj, $"Mod Load Exception: {modLoadException.Message}", true);
                            Console.LogError("JaLoader", $"Mod Load Exception: {modLoadException.Message}");
                            break;

                        default:
                            Console.LogError("/", ex);
                            Debug.Log(ex);
                            Debug.Log("You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.");
                            Console.LogError("JaLoader", "You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.");

                            if (isBepInExMod)
                            {
                                Console.LogWarning("JaLoader", "Please report this issue to the JaLoader GitHub page, making sure to upload your 'output_log.txt' file and applying the BepInEx label!");
                            }

                            break;
                    }
                }
                finally
                {
                    if(errorOccured && certainModFile == "")
                        GetComponent<LoadingScreen>().DeleteLoadingScreen();
                }
            }

            if (!errorOccured && certainModFile == "")
                GetComponent<LoadingScreen>().DeleteLoadingScreen();

            EventsManager.Instance.OnModsInit();

            yield return null;


            /*if (validMods == modsNumber)
            {
                LoadModOrder();

                finishedInitializingPartOneMods = true;
            }

            if(!errorOccured && certainModFile == "")
                GetComponent<LoadingScreen>().DeleteLoadingScreen(); 

            EventsManager.Instance.OnModsInit();

            yield return null;
        }

        private bool waitedForEndOfFrame = false;

        private IEnumerator WaitForEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            waitedForEndOfFrame = true;
        }

        /*private IEnumerator ReloadAllMods()
        {
            Dictionary<GameObject, Type> modObjAndType = new Dictionary<GameObject, Type>();
            List<Mod> reloadedMods = new List<Mod>();

            foreach(Mod mod in modsInitInGame)
            {
                modObjAndType.Add(mod.gameObject, mod.GetType());

                Destroy(mod);
                if (uiManager.modSettingsScrollViewContent.transform.Find($"{mod.ModAuthor}_{mod.ModID}_{mod.ModName}-SettingsHolder"))
                    Destroy(uiManager.modSettingsScrollViewContent.transform.Find($"{mod.ModAuthor}_{mod.ModID}_{mod.ModName}-SettingsHolder").gameObject);
                if (uiManager.UICanvas.transform.Find("JLModsPanel/Scroll View").GetChild(0).GetChild(0).transform.Find($"{mod.ModID}_{mod.ModAuthor}_{mod.ModName}_Mod"))
                    Destroy(uiManager.UICanvas.transform.Find("JLModsPanel/Scroll View").GetChild(0).GetChild(0).transform.Find($"{mod.ModID}_{mod.ModAuthor}_{mod.ModName}_Mod").gameObject);

                modStatusTextRef.Remove(mod);
                Console.Instance.RemoveCommandsFromMod(mod);
            }

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            CustomObjectsManager.Instance.ignoreAlreadyExists = true;

            foreach (KeyValuePair<GameObject, Type> modObj in modObjAndType)
            {
                GameObject ModObject = modObj.Key;

                Type ModType = modObj.Value;

                Component ModComponent = ModObject.AddComponent(ModType);
                Mod mod = ModObject.GetComponent<Mod>();

                uiManager.modTemplateObject = Instantiate(uiManager.modTemplatePrefab);
                uiManager.modTemplateObject.transform.SetParent(uiManager.UICanvas.transform.Find("JLModsPanel/Scroll View").GetChild(0).GetChild(0).transform, false);
                uiManager.modTemplateObject.SetActive(true);

                uiManager.modTemplateObject.name = $"{mod.ModID}_{mod.ModAuthor}_{mod.ModName}_Mod";

                if (mod.UseAssets)
                    mod.AssetsPath = $@"{SettingsManager.ModFolderLocation}\Assets\{mod.ModID}";

                string modVersionText = mod.ModVersion;
                string modName = SettingsManager.DebugMode ? mod.ModID : mod.ModName;

                if (mod.GitHubLink != string.Empty && mod.GitHubLink != null)
                {
                    string[] splitLink = mod.GitHubLink.Split('/');

                    string URL = $"https://api.github.com/repos/{splitLink[3]}/{splitLink[4]}/releases/latest";

                    string version = ModHelper.Instance.GetLatestTagFromApiUrl(URL, modName);

                    int versionInt = int.Parse(version.Replace(".", ""));
                    int currentVersion = int.Parse(mod.ModVersion.Replace(".", ""));

                    if (versionInt > currentVersion)
                    {
                        modsNeedUpdate++;
                        modVersionText = $"{mod.ModVersion} <color=green>(Latest version: {version})</color>";
                        modName = $"<color=green>(Update Available!)</color> {modName}";
                    }
                }

                uiManager.modTemplateObject.transform.Find("BasicInfo").Find("ModName").GetComponent<Text>().text = modName;
                uiManager.modTemplateObject.transform.Find("BasicInfo").Find("ModAuthor").GetComponent<Text>().text = mod.ModAuthor;

                uiManager.modTemplateObject.transform.Find("Buttons").Find("AboutButton").GetComponent<Button>().onClick.AddListener(delegate { uiManager.ToggleMoreInfo(mod.ModName, mod.ModAuthor, modVersionText, mod.ModDescription); });
                uiManager.modTemplateObject.transform.Find("Buttons").Find("SettingsButton").GetComponent<Button>().onClick.AddListener(delegate { uiManager.ToggleSettings($"{mod.ModAuthor}_{mod.ModID}"); });

                var tempModObj = uiManager.modTemplateObject;
                uiManager.modTemplateObject.transform.Find("LoadOrderButtons").Find("MoveUpButton").GetComponent<Button>().onClick.AddListener(delegate { MoveModOrderUp(mod, tempModObj); });
                uiManager.modTemplateObject.transform.Find("LoadOrderButtons").Find("MoveDownButton").GetComponent<Button>().onClick.AddListener(delegate { MoveModOrderDown(mod, tempModObj); });
                uiManager.modTemplateObject.transform.Find("LoadOrderButtons").Find("MoveTopButton").GetComponent<Button>().onClick.AddListener(delegate { MoveModOrderTop(mod, tempModObj); });
                uiManager.modTemplateObject.transform.Find("LoadOrderButtons").Find("MoveBottomButton").GetComponent<Button>().onClick.AddListener(delegate { MoveModOrderBottom(mod, tempModObj); });

                GameObject tempObj = uiManager.modTemplateObject.transform.Find("Buttons").Find("ToggleButton").Find("Text").gameObject;
                uiManager.modTemplateObject.transform.Find("Buttons").Find("ToggleButton").GetComponent<Button>().onClick.AddListener(delegate { ToggleMod(mod, tempObj.GetComponent<Text>()); });

                modStatusTextRef.Add(mod, tempObj.GetComponent<Text>());

                uiManager.modTemplateObject = null;

                CheckForDependencies(mod);

                CheckForIncompatibilities(mod);

                mod.EventsDeclaration();

                mod.SettingsDeclaration();

                mod.CustomObjectsRegistration();

                if (mod.settingsIDS.Count > 0)
                    mod.LoadModSettings();

                reloadedMods.Add(mod);
            }

            CustomObjectsManager.Instance.ignoreAlreadyExists = false;

            modsInitInGame.Clear();
            modsInitInGame = reloadedMods;
            LoadModOrder();
            reloadMods = false;
            disabledMods.Clear();
            LoadedDisabledMods = false;

            yield return new WaitUntil(() => LoadedDisabledMods == true)
            { };

            foreach (var mod in reloadedMods)
                if (!disabledMods.Contains(mod))
                    mod.OnReload();

            yield return null;
        }*/

            /*private Mod ReloadMod(Mod modToReload)
            {
                uiManager.modTemplateObject = Instantiate(uiManager.modTemplatePrefab);
                uiManager.modTemplateObject.transform.SetParent(uiManager.UICanvas.transform.Find("JLModsPanel/Scroll View").GetChild(0).GetChild(0).transform, false);
                uiManager.modTemplateObject.SetActive(true);

                GameObject ModObject = modToReload.gameObject;

                Type ModType = ModObject.GetComponent<Mod>().GetType();

                DestroyImmediate(ModObject.GetComponent<Mod>());
                if (uiManager.modSettingsScrollViewContent.transform.Find($"{modToReload.ModAuthor}_{modToReload.ModID}_{modToReload.ModName}-SettingsHolder"))
                {
                    DestroyImmediate(uiManager.modSettingsScrollViewContent.transform.Find($"{modToReload.ModAuthor}_{modToReload.ModID}_{modToReload.ModName}-SettingsHolder").gameObject);
                }
                if (uiManager.UICanvas.transform.Find("JLModsPanel/Scroll View").GetChild(0).GetChild(0).transform.Find($"{modToReload.ModID}_{modToReload.ModAuthor}_{modToReload.ModName}_Mod"))
                {
                    DestroyImmediate(uiManager.UICanvas.transform.Find("JLModsPanel/Scroll View").GetChild(0).GetChild(0).transform.Find($"{modToReload.ModID}_{modToReload.ModAuthor}_{modToReload.ModName}_Mod").gameObject);
                }
                //modsInitInGame.Remove(modToReload);
                modStatusTextRef.Remove(modToReload);
                Console.Instance.RemoveCommandsFromMod(modToReload);

                bool started = false;
                while(!waitedForEndOfFrame)
                {
                    if(!started)
                    {
                        StartCoroutine(WaitForEndOfFrame());
                        started = true;
                    }
                }

                waitedForEndOfFrame = false;

                Component ModComponent = ModObject.AddComponent(ModType);
                Mod mod = ModObject.GetComponent<Mod>();

                uiManager.modTemplateObject.name = $"{mod.ModID}_{mod.ModAuthor}_{mod.ModName}_Mod";

                if (mod.UseAssets)
                {
                    mod.AssetsPath = $@"{SettingsManager.ModFolderLocation}\Assets\{mod.ModID}";
                }

                string modVersionText = mod.ModVersion;
                string modName = SettingsManager.DebugMode ? mod.ModID : mod.ModName;

                if (mod.GitHubLink != string.Empty && mod.GitHubLink != null)
                {
                    string[] splitLink = mod.GitHubLink.Split('/');

                    string URL = $"https://api.github.com/repos/{splitLink[3]}/{splitLink[4]}/releases/latest";

                    string version = ModHelper.Instance.GetLatestTagFromApiUrl(URL, modName);

                    int versionInt = int.Parse(version.Replace(".", ""));
                    int currentVersion = int.Parse(mod.ModVersion.Replace(".", ""));

                    if (versionInt > currentVersion)
                    {
                        modsNeedUpdate++;
                        modVersionText = $"{mod.ModVersion} <color=green>(Latest version: {version})</color>";
                        modName = $"<color=green>(Update Available!)</color> {modName}";
                    }
                }

                uiManager.modTemplateObject.transform.Find("BasicInfo").Find("ModName").GetComponent<Text>().text = modName;
                uiManager.modTemplateObject.transform.Find("BasicInfo").Find("ModAuthor").GetComponent<Text>().text = mod.ModAuthor;

                uiManager.modTemplateObject.transform.Find("Buttons").Find("AboutButton").GetComponent<Button>().onClick.AddListener(delegate { uiManager.ToggleMoreInfo(mod.ModName, mod.ModAuthor, modVersionText, mod.ModDescription); });
                uiManager.modTemplateObject.transform.Find("Buttons").Find("SettingsButton").GetComponent<Button>().onClick.AddListener(delegate { uiManager.ToggleSettings($"{mod.ModAuthor}_{mod.ModID}_{mod.ModName}-SettingsHolder"); });

                var tempModObj = uiManager.modTemplateObject;
                uiManager.modTemplateObject.transform.Find("LoadOrderButtons").Find("MoveUpButton").GetComponent<Button>().onClick.AddListener(delegate { MoveModOrderUp(mod, tempModObj); });
                uiManager.modTemplateObject.transform.Find("LoadOrderButtons").Find("MoveDownButton").GetComponent<Button>().onClick.AddListener(delegate { MoveModOrderDown(mod, tempModObj); });
                uiManager.modTemplateObject.transform.Find("LoadOrderButtons").Find("MoveTopButton").GetComponent<Button>().onClick.AddListener(delegate { MoveModOrderTop(mod, tempModObj); });
                uiManager.modTemplateObject.transform.Find("LoadOrderButtons").Find("MoveBottomButton").GetComponent<Button>().onClick.AddListener(delegate { MoveModOrderBottom(mod, tempModObj); });

                //modsInitInGame.Add(mod);

                GameObject tempObj = uiManager.modTemplateObject.transform.Find("Buttons").Find("ToggleButton").Find("Text").gameObject;
                uiManager.modTemplateObject.transform.Find("Buttons").Find("ToggleButton").GetComponent<Button>().onClick.AddListener(delegate { ToggleMod(mod, tempObj.GetComponent<Text>()); });

                modStatusTextRef.Add(mod, tempObj.GetComponent<Text>());

                uiManager.modTemplateObject = null;

                CheckForDependencies(mod);

                CheckForIncompatibilities(mod);

                mod.EventsDeclaration();

                mod.SettingsDeclaration();

                CustomObjectsManager.Instance.ignoreAlreadyExists = true;
                mod.CustomObjectsRegistration();
                CustomObjectsManager.Instance.ignoreAlreadyExists = false;

                if (mod.settingsIDS.Count > 0)
                    mod.LoadModSettings();

                return mod;*/
        }
    }
}
