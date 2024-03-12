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
using NAudio.Midi;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using BepInEx;
using JaLoader.BepInExWrapper;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using static UnityEngine.EventSystems.EventTrigger;
using Mono.Cecil;

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

        private SettingsManager settingsManager;
        private UIManager uiManager;

        public int modsNumber;
        private int bepinexModsNumber;
        private int modsNeedUpdate;

        public List<Mod> modsInitInGame = new List<Mod>();
        private readonly List<MonoBehaviour> modsInitInMenuIncludingBIX = new List<MonoBehaviour>();
        //private readonly List<Mod> modsInitInMenu = new List<Mod>();

        public List<MonoBehaviour> disabledMods = new List<MonoBehaviour>();
        private bool LoadedDisabledMods;
        private readonly Dictionary<MonoBehaviour, Text> modStatusTextRef = new Dictionary<MonoBehaviour, Text>();
   
        public bool InitializedInGameMods;
        private bool InitializedInMenuMods;
        
        public bool finishedInitializingPartOneMods;
        public bool finishedInitializingPartTwoMods;
        private bool skippedIntro;
        private bool reloadMods;

        public bool IsCrackedVersion { get; private set; }

        Stopwatch stopWatch;

        Color32 defaultWhiteColor = new Color32(255, 255, 255, 255);
        Color32 defaultGrayColor = new Color32(120, 120, 120, 255);

        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            switch (CheckForMissingDLLs())
            {
                case "Theraot.Core.dll":
                    CreateImportantNotice("\n\nThe file 'Theraot.Core.dll' was not found. You can try:", "Reinstalling JaLoader with JaPatcher\n\n\nCopying the file from JaPatcher's directory/Assets to Jalopy_Data/Managed");
                    SceneManager.LoadScene("MainMenu");
                    return;

                case "NAudio.dll":
                    CreateImportantNotice("\n\nThe file 'NAudio.dll' was not found. You can try:", "Reinstalling JaLoader with JaPatcher\n\n\nCopying the file from JaPatcher's directory/Assets to Jalopy_Data/Managed");
                    SceneManager.LoadScene("MainMenu");
                    return;
            }

            CheckForCrack();

            GameObject helperObj = Instantiate(new GameObject());
            helperObj.name = "JaLoader Modding Helpers";
            helperObj.AddComponent<EventsManager>();

            EventsManager.Instance.OnGameLoad += OnGameLoad;
            EventsManager.Instance.OnGameUnload += OnGameUnload;
            EventsManager.Instance.OnMenuLoad += OnMenuLoad;

            DontDestroyOnLoad(helperObj);

            settingsManager = gameObject.AddComponent<SettingsManager>();
            uiManager = gameObject.AddComponent<UIManager>();
            gameObject.AddComponent<CustomObjectsManager>();
            gameObject.AddComponent<DebugObjectSpawner>();
            gameObject.AddComponent<ReferencesLoader>();
            gameObject.AddComponent<CustomRadioController>();
            gameObject.AddComponent<ExtrasManager>();

            helperObj.AddComponent<ModHelper>();
            helperObj.AddComponent<UncleHelper>();
            helperObj.AddComponent<PartIconManager>();

            gameObject.AddComponent<DiscordController>();

            stopWatch = gameObject.AddComponent<Stopwatch>();

            Debug.Log("JaLoader initialized!");

            if (settingsManager.SkipLanguage && !skippedIntro)
            {
                skippedIntro = true;
                settingsManager.selectedLanguage = true;
                SceneManager.LoadScene("MainMenu");
            }
        }

        private string CheckForMissingDLLs()
        {
            var path = $@"{Application.dataPath}\Managed";

            if (!File.Exists($@"{path}\Theraot.Core.dll"))
                return "Theraot.Core.dll";

            if (!File.Exists($@"{path}\NAudio.dll"))
                return "NAudio.dll";

            return "None";
        }

        private void OnGameLoad()
        {
            if (settingsManager.UseExperimentalCharacterController)
                GameObject.Find("First Person Controller").AddComponent<EnhancedMovement>();

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
            while (ModHelper.Instance.laika == null)
                yield return null;

            var list = new List<Mod>();

            foreach(Mod mod in modsInitInGame.ToList())
            {
                var modToAdd = ReloadMod(mod);
                list.Add(modToAdd);
            }

            modsInitInGame.Clear();
            modsInitInGame = list;
            LoadModOrder();
            reloadMods = false;
            disabledMods.Clear();
            LoadedDisabledMods = false;
        }

        private void OnGameUnload() 
        {
            reloadMods = true;

            if (InitializedInGameMods)
            {
                //finishedInitializingPartOneMods = false;
                
                foreach (Mod mod in modsInitInGame.ToList())
                {
                    mod.gameObject.SetActive(false);
                }

                InitializedInGameMods = false;
            }
        }

        private void Update()
        {
            if (modsNumber == 0)
                return;

            if (finishedInitializingPartOneMods && !reloadMods)
            {
                if (!LoadedDisabledMods && settingsManager.DisabledMods.Count != 0)
                {
                    for (int i = 0; i < modsInitInGame.Count; i++)
                    {
                        string reference = $"{modsInitInGame.ToArray()[i].ModAuthor}_{modsInitInGame.ToArray()[i].ModID}_{modsInitInGame.ToArray()[i].ModName}";
                        if (settingsManager.DisabledMods.Contains(reference))
                        {
                            disabledMods.Add(modsInitInGame.ToArray()[i]);
                        }
                    }

                    for (int i = 0; i < modsInitInMenuIncludingBIX.Count; i++)
                    {
                        if (modsInitInMenuIncludingBIX.ToArray()[i] is Mod mod)
                        {
                            string reference = $"{mod.ModAuthor}_{mod.ModID}_{mod.ModName}";
                            if (settingsManager.DisabledMods.Contains(reference))
                            {
                                disabledMods.Add(mod);
                            }
                        }
                        else if (modsInitInMenuIncludingBIX.ToArray()[i] is BaseUnityPlugin bix_mod)
                        {
                            ModInfo modInfo = bix_mod.gameObject.GetComponent<ModInfo>();

                            string reference = $"BepInEx_CompatLayer_{modInfo.GUID}";
                            if (settingsManager.DisabledMods.Contains(reference))
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

                    Console.Instance.Log("JaLoader", message);
                    if (settingsManager.UseCustomSongs)
                        Console.Instance.Log("JaLoader", $"{CustomRadioController.Instance.loadedSongs.Count} custom songs loaded!");

                    foreach (MonoBehaviour monoBehaviour in modsInitInMenuIncludingBIX)
                    {
                        if (monoBehaviour is Mod mod)
                        {
                            CheckForDependencies(mod);

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
                            catch (Exception)
                            {
                                mod.gameObject.SetActive(false);

                                Debug.Log($"Part 2/2 of initialization for mod {mod.ModName} failed");
                                Debug.Log($"Failed to load mod {mod.ModName}. An error occoured while enabling the mod.");

                                Console.Instance.LogError("JaLoader", $"An error occured while trying to load mod \"{mod.ModName}\"");

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
                            catch (Exception)
                            {
                                bix_mod.gameObject.SetActive(false);

                                Debug.Log($"Part 2/2 of initialization for BepInEx mod {modInfo.Name} failed");
                                Debug.Log($"Failed to load BepInEx mod {modInfo.Name}. An error occoured while enabling the mod.");

                                Console.Instance.LogError("JaLoader", $"An error occured while trying to load BepInEx mod \"{modInfo.name}\"");

                                modsToRemoveAfter.Add(bix_mod);

                                continue;
                                throw;
                            }
                        }
                    }

                    foreach (Mod mod in modsInitInGame)
                    {
                        CheckForDependencies(mod);

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
                        catch (Exception)
                        {
                            mod.gameObject.SetActive(false);

                            Debug.Log($"Part 2/2 of initialization for mod {mod.ModName} failed");
                            Debug.Log($"Failed to load mod {mod.ModName}. An error occoured while enabling the mod.");

                            Console.Instance.LogError("JaLoader", $"An error occured while trying to load mod \"{mod.ModName}\"");

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

                    stopWatch.StopCounting();
                    Debug.Log($"Loaded JaLoader mods! ({stopWatch.timePassed}s)");
                    Debug.Log($"JaLoader successfully loaded! ({stopWatch.totalTimePassed}s)");
                    finishedInitializingPartTwoMods = true;
                    Destroy(stopWatch);

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
            }       
        }

        private void CheckForDependencies(Mod mod)
        {
            if (mod.Dependencies.Count > 0)
            {
                foreach (var dependency in mod.Dependencies)
                {
                    int version = int.Parse(dependency.Item3.Replace(".", ""));

                    if (dependency.Item1 == "JaLoader" && dependency.Item2 == "Leaxx")
                    {
                        if(settingsManager.GetVersion() < version)
                        {
                            uiManager.ShowNotice("Dependency required", $"The mod \"{mod.ModName}\" requires JaLoader version {dependency.Item3} or higher. You are currently using version {settingsManager.GetVersionString()}. The mod may still load, but not function correctly.");
                        }
                    }
                    else
                    {
                        Mod dependentMod = FindMod(dependency.Item2, dependency.Item1);

                        if (dependentMod == null)
                        {
                            uiManager.ShowNotice("Dependency required", $"The mod \"{mod.ModName}\" requires the mod \"{dependency.Item1}\" to be installed. The mod may still load, but not function correctly.");    
                        }
                        else
                        {
                            if (dependentMod.ModVersion != dependency.Item3)
                            {
                                uiManager.ShowNotice("Dependency required", $"The mod \"{mod.ModName}\" requires the mod \"{dependency.Item1}\" to be version {dependency.Item3} or higher. You are currently using version {FindMod(dependency.Item2, dependency.Item1).ModVersion}. The mod may still load, but not function correctly.");
                            }

                            if (modsInitInMenuIncludingBIX.IndexOf(dependentMod) > modsInitInMenuIncludingBIX.IndexOf(mod))
                            {
                                uiManager.ShowNotice("Dependency required", $"The mod \"{mod.ModName}\" requires the mod \"{dependency.Item1}\" to be loaded before it. Adjust its load order in the mods list.");
                            }
                            else if(modsInitInGame.IndexOf(dependentMod) > modsInitInGame.IndexOf(mod))
                            {
                                uiManager.ShowNotice("Dependency required", $"The mod \"{mod.ModName}\" requires the mod \"{dependency.Item1}\" to be loaded before it. Adjust its load order in the mods list.");
                            }
                        }
                    }
                }
            }
        }

        public IEnumerator InitializeMods()
        {
            Debug.Log("Initializing JaLoader mods...");
            gameObject.GetComponent<Stopwatch>().StartCounting();

            DirectoryInfo d = new DirectoryInfo(settingsManager.ModFolderLocation);
            FileInfo[] mods = d.GetFiles("*.dll");

            int validMods = mods.Length;

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                Assembly loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName == args.Name);
                if (loadedAssembly != null)
                {
                    return loadedAssembly;
                }

                // try to redirect the bepinex assembly to this assembly
                if (args.Name.StartsWith("BepInEx"))
                {
                    return Assembly.GetExecutingAssembly();
                }

                if (args.Name.StartsWith("Harmony"))
                {
                    return Assembly.LoadFrom(Path.Combine(Application.dataPath, @"Managed\0Harmony.dll"));
                }

                return null;
            };

            foreach (FileInfo modFile in mods)
            {
                bool isBepInExMod = false;

                uiManager.modTemplateObject = Instantiate(uiManager.modTemplatePrefab);
                uiManager.modTemplateObject.transform.SetParent(uiManager.UICanvas.transform.Find("JLModsPanel/Scroll View").GetChild(0).GetChild(0).transform, false);
                uiManager.modTemplateObject.SetActive(true);

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
                            Console.Instance.LogError($"Mod {modFile.Name} does not contain any class derived from Mod or BaseUnityPlugin.");
                            throw new Exception("No valid mod class found.");
                        }

                        isBepInExMod = true;

                        // the mod is made with BepInEx, try to load it
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
                            ModDescription = modType.Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
                        }
                        else
                        {
                            // somehow there is no attribute or it failed to load

                            ModID = ModName = modAssembly.GetName().Name;
                            ModVersion = modAssembly.GetName().Version.ToString();
                        }

                        modInfo.GUID = ModID;
                        modInfo.Name = ModName;
                        modInfo.Version = ModVersion;

                        bix_ModObject.name = $"BepInEx_CompatLayer_{ModID}";
                        uiManager.modTemplateObject.name = $"BepInEx_CompatLayer_{ModID}_Mod";

                        string bix_modVersionText = ModVersion;
                        string bix_modName = settingsManager.DebugMode ? $"{ModID} - BepInEx" : ModName;

                        string pattern = @"^[a-zA-Z]+(\.[a-zA-Z]+)+$";
                        string authorName = "BepInEx";

                        if(Regex.IsMatch(modInfo.GUID, pattern))
                        {
                            authorName = modInfo.GUID.Split('.')[1];
                        }

                        uiManager.modTemplateObject.transform.Find("BasicInfo").Find("ModName").GetComponent<Text>().text = bix_modName;
                        uiManager.modTemplateObject.transform.Find("BasicInfo").Find("ModAuthor").GetComponent<Text>().text = authorName;

                        uiManager.modTemplateObject.transform.Find("Buttons").Find("AboutButton").GetComponent<Button>().onClick.AddListener(delegate { uiManager.ToggleMoreInfo(ModID, authorName, bix_modVersionText, $"{ModDescription}\n\nThis mod was made using BepInEx. Some features might not work as intended."); });
                        uiManager.modTemplateObject.transform.Find("Buttons").Find("SettingsButton").GetComponent<Button>().onClick.AddListener(delegate { uiManager.ToggleSettings($"BepInEx_CompatLayer_{ModID}-SettingsHolder"); });

                        var bix_tempModObj = uiManager.modTemplateObject;

                        uiManager.modTemplateObject.transform.Find("LoadOrderButtons").Find("MoveUpButton").GetComponent<Button>().onClick.AddListener(delegate { MoveModOrderUp(bix_mod, bix_tempModObj); });
                        uiManager.modTemplateObject.transform.Find("LoadOrderButtons").Find("MoveDownButton").GetComponent<Button>().onClick.AddListener(delegate { MoveModOrderDown(bix_mod, bix_tempModObj); });
                        uiManager.modTemplateObject.transform.Find("LoadOrderButtons").Find("MoveTopButton").GetComponent<Button>().onClick.AddListener(delegate { MoveModOrderTop(bix_mod, bix_tempModObj); });
                        uiManager.modTemplateObject.transform.Find("LoadOrderButtons").Find("MoveBottomButton").GetComponent<Button>().onClick.AddListener(delegate { MoveModOrderBottom(bix_mod, bix_tempModObj); });

                        modsInitInMenuIncludingBIX.Add(bix_mod);

                        GameObject bix_tempObj = uiManager.modTemplateObject.transform.Find("Buttons").Find("ToggleButton").Find("Text").gameObject;
                        uiManager.modTemplateObject.transform.Find("Buttons").Find("ToggleButton").GetComponent<Button>().onClick.AddListener(delegate { ToggleMod(bix_mod, bix_tempObj.GetComponent<Text>()); });

                        modStatusTextRef.Add(bix_mod, bix_tempObj.GetComponent<Text>());

                        Debug.Log($"Part 1/2 of initialization for BepInEx mod {ModName} completed");              

                        modsNumber++;
                        bepinexModsNumber++;

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
                        Console.Instance.LogError(modFile.Name, $"{modFile.Name} contains no information related to its ID, name, author or version.");
                        throw new Exception();
                    }

                    ModObject.name = $"{mod.ModID}_{mod.ModAuthor}_{mod.ModName}";
                    uiManager.modTemplateObject.name = $"{mod.ModID}_{mod.ModAuthor}_{mod.ModName}_Mod";

                    if (mod.UseAssets)
                    {
                        mod.AssetsPath = $@"{settingsManager.ModFolderLocation}\Assets\{mod.ModID}";

                        if (!Directory.Exists(mod.AssetsPath))
                            Directory.CreateDirectory(mod.AssetsPath);
                    }

                    string modVersionText = mod.ModVersion;
                    string modName = settingsManager.DebugMode ? mod.ModID : mod.ModName;

                    if (mod.GitHubLink != string.Empty && mod.GitHubLink != null)
                    {
                        string[] splitLink = mod.GitHubLink.Split('/');

                        string URL = $"https://api.github.com/repos/{splitLink[3]}/{splitLink[4]}/releases/latest";

                        int currentVersion = int.Parse(mod.ModVersion.Replace(".", ""));

                        string latestVersion = settingsManager.GetLatestUpdateVersionString(URL, currentVersion);

                        if (int.Parse(latestVersion.Replace(".", "")) > currentVersion)
                        {
                            modsNeedUpdate++;
                            modVersionText = $"{mod.ModVersion} <color=green>(Latest version: {latestVersion})</color>";
                            modName = $"<color=green>(Update Available!)</color> {modName}";
                        }

                        uiManager.modTemplateObject.transform.Find("Buttons").Find("GitHubButton").GetComponent<Button>().interactable = true;
                        uiManager.modTemplateObject.transform.Find("Buttons").Find("GitHubButton").GetComponent<Button>().onClick.AddListener(delegate { ModHelper.Instance.OpenURL(mod.GitHubLink); });
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

                    switch (mod.WhenToInit)
                    {
                        case WhenToInit.InMenu:
                            modsInitInMenuIncludingBIX.Add(mod);
                            break;

                        case WhenToInit.InGame:
                            modsInitInGame.Add(mod);
                            break;
                    }

                    GameObject tempObj = uiManager.modTemplateObject.transform.Find("Buttons").Find("ToggleButton").Find("Text").gameObject;
                    uiManager.modTemplateObject.transform.Find("Buttons").Find("ToggleButton").GetComponent<Button>().onClick.AddListener(delegate { ToggleMod(mod, tempObj.GetComponent<Text>()); });

                    modStatusTextRef.Add(mod, tempObj.GetComponent<Text>());

                    Debug.Log($"Part 1/2 of initialization for mod {mod.ModName} completed");

                    modsNumber++;
                    #endregion
                }
                catch (Exception ex)
                {
                    validMods--;

                    Debug.Log($"Failed to initialize mod {modFile.Name}");
                    Console.Instance.LogError("JaLoader", $"An error occured while trying to initialize mod \"{modFile.Name}\": ");

                    uiManager.modTemplateObject.transform.Find("BasicInfo").Find("ModName").GetComponent<Text>().text = modFile.Name;
                    uiManager.modTemplateObject.transform.Find("BasicInfo").Find("ModAuthor").GetComponent<Text>().text = "Failed to load mod";

                    uiManager.modTemplateObject.transform.Find("Buttons").Find("AboutButton").GetComponent<Button>().onClick.AddListener(delegate { uiManager.ToggleMoreInfo(modFile.Name, "", "", $"{modFile.Name} experienced an issue during loading and couldn't be initialized. You can check the \"JaLoader_log.log\" file, located in the main game folder for more details."); });

                    if (ex is FileNotFoundException fileNotFoundException)
                    {
                        string[] parts = fileNotFoundException.FileName.Split(',');

                        if (parts.Length >= 2)
                        {
                            string dllName = parts[0].Trim();
                            string versionSection = parts[1].Trim();

                            int versionIndex = versionSection.IndexOf("Version=");
                            if (versionIndex != -1)
                            {
                                string versionNumber = versionSection.Substring(versionIndex + "Version=".Length).Trim();

                                string errorMessage = $"\"{modFile.Name}\" requires the following DLL: {dllName}, version {versionNumber}";
                                Debug.Log(errorMessage);
                                Console.Instance.LogError("JaLoader", errorMessage);
                                Console.Instance.LogError("JaLoader", "You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.");
                            }
                        }
                    }
                    else
                    {
                        Console.Instance.LogError("/", ex);
                        Debug.Log(ex);
                        Debug.Log("You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.");
                        Console.Instance.LogError("JaLoader", "You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.");

                        if (isBepInExMod)
                        {
                            Console.Instance.LogWarning("JaLoader", "Please report this issue to the JaLoader GitHub page, making sure to upload your 'output_log.txt' file and applying the BepInEx label!");
                        }
                    }

                    //Console.Instance.LogError("JaLoader", $"\"{modFile.Name}\" is not a valid mod! Please remove it from the Mods folder.");
                }
                finally
                {
                    uiManager.modTemplateObject = null;
                }
            }


            if (validMods == modsNumber)
            {
                LoadModOrder();

                finishedInitializingPartOneMods = true;
            }

            if (modsNumber == 0)
            {
                Console.Instance.LogMessage("JaLoader", $"No mods found!");
                Console.Instance.ToggleVisibility(true);

                UIManager.Instance.modTemplatePrefab.transform.parent.parent.parent.parent.Find("NoMods").gameObject.SetActive(true);
            }

            GetComponent<LoadingScreen>().DeleteLoadingScreen();

            yield return null;
        }

        private void MoveModOrderUp(MonoBehaviour mod, GameObject modListObj)
        {
            if(modListObj.transform.GetSiblingIndex() > 1)
                modListObj.transform.SetSiblingIndex(modListObj.transform.GetSiblingIndex() - 1);

            SaveModsOrder();
        }

        private void MoveModOrderDown(MonoBehaviour mod, GameObject modListObj)
        {
            if(modListObj.transform.GetSiblingIndex() < modListObj.transform.parent.childCount - 1)
                modListObj.transform.SetSiblingIndex(modListObj.transform.GetSiblingIndex() + 1);

            SaveModsOrder();
        }

        private void MoveModOrderTop(MonoBehaviour mod, GameObject modListObj)
        {
            modListObj.transform.SetSiblingIndex(1);

            SaveModsOrder();
        }

        private void MoveModOrderBottom(MonoBehaviour mod, GameObject modListObj)
        {
            modListObj.transform.SetSiblingIndex(modListObj.transform.parent.childCount - 1);

            SaveModsOrder();
        }

        private void SaveModsOrder()
        {
            string orderFilePath = Path.Combine(Application.persistentDataPath, "ModsOrder.txt");

            using (StreamWriter writer = new StreamWriter(orderFilePath))
            {
                for (int i = 1; i < uiManager.UICanvas.transform.Find("JLModsPanel/Scroll View").GetChild(0).GetChild(0).childCount; i++)
                {
                    GameObject modObj = uiManager.UICanvas.transform.Find("JLModsPanel/Scroll View").GetChild(0).GetChild(0).GetChild(i).gameObject;
                    string[] modInfo = modObj.name.Split('_');
                    if (modInfo.Length < 3) return;

                    writer.WriteLine($"{modInfo[0]}_{modInfo[1]}_{modInfo[2]}_{i}");
                }
            }

            Debug.Log("Saved mods order");
        }

        private void LoadModOrder()
        {
            string orderFilePath = Path.Combine(Application.persistentDataPath, "ModsOrder.txt");

            if (File.Exists(orderFilePath))
            {
                string[] lines = File.ReadAllLines(orderFilePath);

                foreach (string line in lines)
                {
                    string[] modInfo = line.Split('_');

                    string modID = modInfo[0];
                    string modAuthor = modInfo[1];
                    string modName = modInfo[2];
                    int loadOrder = int.Parse(modInfo[3]);

                    MonoBehaviour mod = FindMod(modAuthor, modID, modName);

                    if (mod != null)
                    {
                        GameObject modObj = null;

                        if (mod is Mod)
                        {
                            modObj = uiManager.UICanvas.transform.Find("JLModsPanel/Scroll View").GetChild(0).GetChild(0).Find($"{modID}_{modAuthor}_{modName}_Mod").gameObject;
                        }
                        else if(mod is BaseUnityPlugin)
                        {
                            ModInfo pluginInfo = mod.gameObject.GetComponent<ModInfo>();

                            modObj = uiManager.UICanvas.transform.Find("JLModsPanel/Scroll View").GetChild(0).GetChild(0).Find($"BepInEx_CompatLayer_{pluginInfo.GUID}_Mod").gameObject;
                        }

                        modObj.transform.SetSiblingIndex(loadOrder);

                        if (modsInitInMenuIncludingBIX.Contains(mod))
                        {
                            modsInitInMenuIncludingBIX.Remove(mod);

                            if (loadOrder <= modsInitInMenuIncludingBIX.Count)
                            {
                                modsInitInMenuIncludingBIX.Insert(loadOrder - 1, mod);
                            }
                            else
                            {
                                modsInitInMenuIncludingBIX.Add(mod);
                            }
                        }

                        if (modsInitInGame.Contains(mod as Mod))
                        {
                            modsInitInGame.Remove(mod as Mod);

                            if (loadOrder <= modsInitInGame.Count)
                            {
                                modsInitInGame.Insert(loadOrder - 1, mod as Mod);
                            }
                            else
                            {
                                modsInitInGame.Add(mod as Mod);
                            }
                        }
                    }
                }

                SaveModsOrder();
            }
            else
            {
                SaveModsOrder();
            }

            Debug.Log("Loaded mods order");
        }

        private Mod ReloadMod(Mod modToReload)
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

            Component ModComponent = ModObject.AddComponent(ModType);
            Mod mod = ModObject.GetComponent<Mod>();

            uiManager.modTemplateObject.name = $"{mod.ModID}_{mod.ModAuthor}_{mod.ModName}_Mod";

            if (mod.UseAssets)
            {
                mod.AssetsPath = $@"{settingsManager.ModFolderLocation}\Assets\{mod.ModID}";
            }

            string modVersionText = mod.ModVersion;
            string modName = settingsManager.DebugMode ? mod.ModID : mod.ModName;

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

            mod.EventsDeclaration();

            mod.SettingsDeclaration();

            CustomObjectsManager.Instance.ignoreAlreadyExists = true;
            mod.CustomObjectsRegistration();
            CustomObjectsManager.Instance.ignoreAlreadyExists = false;

            if (mod.settingsIDS.Count > 0)
                mod.LoadModSettings();

            return mod;
        }

        public void ToggleMod(MonoBehaviour mod, Text toggleBtn)
        {
            if (modsInitInMenuIncludingBIX.Contains(mod))
            {
                if (!disabledMods.Contains(mod))
                {
                    disabledMods.Add(mod);
                    toggleBtn.text = "Enable";
                    toggleBtn.transform.parent.parent.parent.Find("BasicInfo").Find("ModName").GetComponent<Text>().color = defaultGrayColor;
                    toggleBtn.transform.parent.parent.parent.Find("BasicInfo").Find("ModAuthor").GetComponent<Text>().color = defaultGrayColor;
                    mod.gameObject.SetActive(false);
                }
                else
                {
                    disabledMods.Remove(mod);
                    disabledMods.Remove(mod); // bug "fix"
                    toggleBtn.text = "Disable";
                    toggleBtn.transform.parent.parent.parent.Find("BasicInfo").Find("ModName").GetComponent<Text>().color = defaultWhiteColor;
                    toggleBtn.transform.parent.parent.parent.Find("BasicInfo").Find("ModAuthor").GetComponent<Text>().color = defaultWhiteColor;
                    mod.gameObject.SetActive(true);
                }
            }

            if (modsInitInGame.Contains(mod as Mod))
            {
                if (!disabledMods.Contains(mod))
                {
                    disabledMods.Add(mod);
                    toggleBtn.text = "Enable";
                    toggleBtn.transform.parent.parent.parent.Find("BasicInfo").Find("ModName").GetComponent<Text>().color = defaultGrayColor;
                    toggleBtn.transform.parent.parent.parent.Find("BasicInfo").Find("ModAuthor").GetComponent<Text>().color = defaultGrayColor;
                }
                else
                {
                    disabledMods.Remove(mod);
                    toggleBtn.text = "Disable";
                    toggleBtn.transform.parent.parent.parent.Find("BasicInfo").Find("ModName").GetComponent<Text>().color = defaultWhiteColor;
                    toggleBtn.transform.parent.parent.parent.Find("BasicInfo").Find("ModAuthor").GetComponent<Text>().color = defaultWhiteColor;
                }
            }

            settingsManager.SaveSettings();
        }

        /// <summary>
        /// Searches for a mod with the specified ID, name and author and returns it if found, otherwise returns null.
        /// </summary>
        /// <param name="author">The mod's author</param>
        /// <param name="ID">The mod's ID</param>
        /// <param name="name">The mod's name</param>
        /// <returns>The searched Mod if found, otherwise null</returns>
        public MonoBehaviour FindMod(string author, string ID, string name)
        {
            if (modsInitInGame.Find(i => i.ModID == ID && i.ModName == name && i.ModAuthor == author))
            {
                return modsInitInGame.Find(i => i.ModID == ID && i.ModName == name && i.ModAuthor == author);
            }

            foreach (MonoBehaviour monoBehaviour in modsInitInMenuIncludingBIX)
            {
                if (monoBehaviour is Mod mod)
                {
                    if (mod.ModID == ID && mod.ModAuthor == author)
                    {
                        return mod;
                    }
                }
                else if (monoBehaviour is BaseUnityPlugin bix_mod)
                {
                    ModInfo modInfo = bix_mod.gameObject.GetComponent<ModInfo>();

                    if (modInfo.GUID == name)
                    {
                        return bix_mod;
                    }
                }
            }

            return null;
        }

        public Type GetTypeFromMod(string author, string ID, string name, string typeName)
        {
            MonoBehaviour mod = FindMod(author, ID, name);

            if (mod != null)
                return Type.GetType($"{mod.GetType().Namespace}.{typeName}, {mod.GetType().Assembly.GetName().Name}");

            return null;
        }   

        /// <summary>
        /// Searches for a mod with the specified ID and author and returns it if found, otherwise returns null.
        /// </summary>
        /// <param name="author">The mod's author</param>
        /// <param name="ID">The mod's ID</param>
        /// <returns>The searched Mod if found, otherwise null</returns>
        public Mod FindMod(string author, string ID)
        {
            Mod inGameMod = modsInitInGame.Find(i => i.ModID == ID && i.ModAuthor == author);
            Mod inMenuMod = null;

            foreach (MonoBehaviour monoBehaviour in modsInitInMenuIncludingBIX)
            {
                if (monoBehaviour is Mod mod)
                {
                    if (mod.ModID == ID && mod.ModAuthor == author)
                    {
                        inMenuMod = mod;
                        break;
                    }
                }
            }

            return inGameMod ?? inMenuMod;
        }

        private void CheckForCrack()
        {
            var mainGameFolder = $@"{Application.dataPath}\..";

            List<string> commonCrackFiles = new List<string>()
            {
                "codex64.dll",
                "steam_api64.cdx",
                "steamclient64.dll",
                "steam_emu.ini",
                "SmartSteamEmu.dll",
                "SmartSteamEmu64.dll",
                "Launcher.exe",
                "Launcher_x64.exe",
            };

            foreach (var file in commonCrackFiles)
            {
                if (File.Exists($@"{mainGameFolder}\{file}") || Directory.Exists($@"{mainGameFolder}\SmartSteamEmu"))
                {
                    IsCrackedVersion = true;
                    break;
                }
            }
        }

        public void CreateImportantNotice(string issue, string possibleFixes)
        {
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                Debug.Log("JaLoader encounted an error!");
                Debug.Log($"JaLoader: {issue} {possibleFixes}");

                FindObjectOfType<MenuMouseInteractionsC>().enabled = false;

                GameObject notice = Instantiate(GameObject.Find("UI Root").transform.Find("Notice").gameObject);
                notice.name = "Error";
                notice.transform.parent = GameObject.Find("UI Root").transform;
                notice.transform.localPosition = Vector3.zero;
                notice.transform.position = new Vector3(notice.transform.position.x, notice.transform.position.y - 0.15f, notice.transform.position.z);
                notice.transform.localRotation = Quaternion.identity;
                notice.transform.localScale = Vector3.one;
                notice.SetActive(true);

                notice.transform.GetChild(5).gameObject.SetActive(false);
                notice.transform.GetChild(1).GetComponent<UITexture>().height = 600;
                notice.transform.GetChild(1).position = new Vector3(notice.transform.GetChild(1).position.x, notice.transform.GetChild(1).position.y + 0.2f, notice.transform.GetChild(1).position.z);
                notice.transform.GetChild(0).GetComponent<UILabel>().text = "JaLoader encountered an error!";
                notice.transform.GetChild(0).GetComponent<UILabel>().ProcessText();
                notice.transform.GetChild(3).GetComponent<UILabel>().text = "\nWHAT WENT WRONG";
                notice.transform.GetChild(3).GetComponent<UILabel>().ProcessText();
                notice.transform.GetChild(2).GetComponent<UILabel>().text = issue;
                notice.transform.GetChild(2).GetComponent<UILabel>().height = 550;
                notice.transform.GetChild(2).GetComponent<UILabel>().ProcessText();
                notice.transform.GetChild(4).GetComponent<UILabel>().text = possibleFixes;
                notice.transform.GetChild(4).GetComponent<UILabel>().fontSize = 24;
                notice.transform.GetChild(4).GetComponent<UILabel>().ProcessText();
                return;
            }

            StartCoroutine(WaitUntilMenuNotice(issue, possibleFixes));
        }

        public void StartUpdate()
        {
            Process.Start($@"{Application.dataPath}\..\JaUpdater.exe", $"{settingsManager.ModFolderLocation} Jalopy");
            Process.GetCurrentProcess().Kill();
        }

        private IEnumerator WaitUntilMenuNotice(string issue, string possibleFixes)
        {
            while (SceneManager.GetActiveScene().buildIndex != 1)
                yield return null;

            CreateImportantNotice(issue, possibleFixes);

            yield return null;
        }
    }
}
