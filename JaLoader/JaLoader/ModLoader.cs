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
        private int modsNeedUpdate;

        public List<Mod> modsInitInGame = new List<Mod>();
        private readonly List<Mod> modsInitInMenu = new List<Mod>();

        public List<Mod> disabledMods = new List<Mod>();
        private bool LoadedDisabledMods;
        private readonly Dictionary<Mod, Text> modStatusTextRef = new Dictionary<Mod, Text>();
   
        public bool InitializedInGameMods;
        private bool InitializedInMenuMods;
        
        public bool finishedInitializingPartOneMods;
        public bool finishedInitializingPartTwoMods;
        private bool skippedIntro;
        private bool reloadMods;

        public bool IsCrackedVersion { get; private set; }

        Stopwatch stopWatch;

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
        }

        private void OnMenuLoad()
        {
            if (!reloadMods)
                return;

            StartCoroutine(WaitThenReload());
        }

        IEnumerator WaitThenReload()
        {
            while (ModHelper.Instance.laika == null)
                yield return null;

            var list = new List<Mod>();

            foreach(Mod mod in modsInitInGame.ToList())
            {
                Console.Instance.Log($"reloading mod {mod.ModName}");
                var modToAdd = ReloadMod(mod);
                list.Add(modToAdd);
                Console.Instance.Log($"added mod {mod.ModName}");
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

                    for (int i = 0; i < modsInitInMenu.Count; i++)
                    {
                        string reference = $"{modsInitInMenu.ToArray()[i].ModAuthor}_{modsInitInMenu.ToArray()[i].ModID}_{modsInitInMenu.ToArray()[i].ModName}";
                        if (settingsManager.DisabledMods.Contains(reference))
                        {
                            disabledMods.Add(modsInitInMenu.ToArray()[i]);
                        }
                    }

                    for (int i = 0; i < disabledMods.Count; i++)
                    {
                        if (modStatusTextRef.ContainsKey(disabledMods.ToArray()[i]))
                        {
                            modStatusTextRef[disabledMods.ToArray()[i]].text = "Enable";
                        }
                    }

                    LoadedDisabledMods = true;
                }

                if (!InitializedInMenuMods)
                {
                    Console.Instance.Log("JaLoader", $"{modsNumber} mods found! ({disabledMods.Count} disabled, {modsNeedUpdate} updates available)");

                    foreach (Mod mod in modsInitInMenu)
                    {
                        CheckForDependencies(mod);
                        
                        mod.EventsDeclaration();

                        mod.SettingsDeclaration();

                        mod.CustomObjectsRegistration();

                        if (mod.settingsIDS.Count > 0)
                            mod.LoadModSettings();

                        Debug.Log($"Part 2/2 of initialization for mod {mod.ModName} completed");

                        if (!disabledMods.Contains(mod))
                            mod.gameObject.SetActive(true);

                        Debug.Log($"Loaded mod {mod.ModName}");
                    }

                    foreach (Mod mod in modsInitInGame)
                    {
                        CheckForDependencies(mod);
                        
                        mod.EventsDeclaration();

                        mod.SettingsDeclaration();

                        mod.CustomObjectsRegistration();

                        if (mod.settingsIDS.Count > 0)
                            mod.LoadModSettings();

                        Debug.Log($"Part 2/2 of initialization for mod {mod.ModName} completed");
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

                            if (modsInitInMenu.IndexOf(dependentMod) > modsInitInMenu.IndexOf(mod))
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

            foreach (FileInfo modFile in mods)
            {
                uiManager.modTemplateObject = Instantiate(uiManager.modTemplatePrefab);
                uiManager.modTemplateObject.transform.SetParent(uiManager.UICanvas.transform.Find("JLModsPanel/Scroll View").GetChild(0).GetChild(0).transform, false);
                uiManager.modTemplateObject.SetActive(true);

                try
                {
                    Assembly modAssembly = Assembly.LoadFrom(modFile.FullName);

                    Type[] allModTypes = modAssembly.GetTypes();

                    Type modType = allModTypes.First(t => t.BaseType.Name == "Mod");

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

                        string version = ModHelper.Instance.GetLatestTagFromApiUrl(URL, modName);

                        int versionInt = int.Parse(version.Replace(".", ""));
                        int currentVersion = int.Parse(mod.ModVersion.Replace(".", ""));

                        if(versionInt > currentVersion)
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

                    switch (mod.WhenToInit)
                    {
                        case WhenToInit.InMenu:
                            modsInitInMenu.Add(mod);
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
                }
                catch (Exception ex)
                {
                    validMods--;

                    Debug.Log($"Failed to initialize mod {modFile.Name}");
                    Console.Instance.LogError("JaLoader", $"An error occured while trying to load mod \"{modFile.Name}\": ");

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

                        continue;
                    }

                    Console.Instance.LogError("/", ex);
                    Debug.Log(ex);
                    Debug.Log("You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.");
                    Console.Instance.LogError("JaLoader", "You can check the \"JaLoader_log.log\" file, located in the main game folder for more details.");
                    //Console.Instance.LogError("JaLoader", $"\"{modFile.Name}\" is not a valid mod! Please remove it from the Mods folder.");
                }

                uiManager.modTemplateObject = null;
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

        private void MoveModOrderUp(Mod mod, GameObject modListObj)
        {
            //Console.Instance.Log($"Moved mod {mod.ModName} up");
            //Console.Instance.Log($"Object is {modListObj.name}");

            if(modListObj.transform.GetSiblingIndex() > 1)
                modListObj.transform.SetSiblingIndex(modListObj.transform.GetSiblingIndex() - 1);

            SaveModsOrder();
        }

        private void MoveModOrderDown(Mod mod, GameObject modListObj)
        {
            if(modListObj.transform.GetSiblingIndex() < modListObj.transform.parent.childCount - 1)
                modListObj.transform.SetSiblingIndex(modListObj.transform.GetSiblingIndex() + 1);

            SaveModsOrder();
        }

        private void MoveModOrderTop(Mod mod, GameObject modListObj)
        {
            modListObj.transform.SetSiblingIndex(1);

            SaveModsOrder();
        }

        private void MoveModOrderBottom(Mod mod, GameObject modListObj)
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

                    Mod mod = FindMod(modAuthor, modID, modName);

                    if (mod != null)
                    {
                        GameObject modObj = uiManager.UICanvas.transform.Find("JLModsPanel/Scroll View").GetChild(0).GetChild(0).Find($"{modID}_{modAuthor}_{modName}_Mod").gameObject;
                        modObj.transform.SetSiblingIndex(loadOrder);

                        if (modsInitInMenu.Contains(mod))
                        {
                            modsInitInMenu.Remove(mod);

                            if (loadOrder <= modsInitInMenu.Count)
                            {
                                modsInitInMenu.Insert(loadOrder - 1, mod);
                            }
                            else
                            {
                                modsInitInMenu.Add(mod);
                            }
                        }

                        if (modsInitInGame.Contains(mod))
                        {
                            modsInitInGame.Remove(mod);

                            if (loadOrder <= modsInitInGame.Count)
                            {
                                modsInitInGame.Insert(loadOrder - 1, mod);
                            }
                            else
                            {
                                modsInitInGame.Add(mod);
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

        public void ToggleMod(Mod mod, Text toggleBtn)
        {
            if (modsInitInMenu.Contains(mod))
            {
                if (!disabledMods.Contains(mod))
                {
                    disabledMods.Add(mod);
                    toggleBtn.text = "Enable";
                    mod.gameObject.SetActive(false);
                }
                else
                {
                    disabledMods.Remove(mod);
                    disabledMods.Remove(mod); // bug "fix"
                    toggleBtn.text = "Disable";
                    mod.gameObject.SetActive(true);
                }
            }

            if (modsInitInGame.Contains(mod))
            {
                if (!disabledMods.Contains(mod))
                {
                    disabledMods.Add(mod);
                    toggleBtn.text = "Enable";
                }
                else
                {
                    disabledMods.Remove(mod);
                    toggleBtn.text = "Disable";
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
        public Mod FindMod(string author, string ID, string name)
        {
            if (modsInitInGame.Find(i => i.ModID == ID && i.ModName == name && i.ModAuthor == author))
            {
                return modsInitInGame.Find(i => i.ModID == ID && i.ModName == name && i.ModAuthor == author);
            }
            else if (modsInitInMenu.Find(i => i.ModID == ID && i.ModName == name && i.ModAuthor == author))
            {
                return modsInitInMenu.Find(i => i.ModID == ID && i.ModName == name && i.ModAuthor == author);
            }

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
            Mod inMenuMod = modsInitInMenu.Find(i => i.ModID == ID && i.ModAuthor == author);

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
