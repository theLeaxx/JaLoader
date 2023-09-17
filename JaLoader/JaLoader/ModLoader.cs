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

        public List<Mod> modsInitInGame = new List<Mod>();
        private readonly List<Mod> modsInitInMenu = new List<Mod>();

        public List<Mod> disabledMods = new List<Mod>();
        private bool LoadedDisabledMods;
        private readonly Dictionary<Mod, Text> modStatusTextRef = new Dictionary<Mod, Text>();
   
        public bool InitializedInGameMods;
        private bool InitializedInMenuMods;
        
        public bool finishedLoadingMods;
        private bool skippedIntro;

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

            DontDestroyOnLoad(helperObj);

            settingsManager = gameObject.AddComponent<SettingsManager>();
            uiManager = gameObject.AddComponent<UIManager>();
            gameObject.AddComponent<CustomObjectsManager>();
            gameObject.AddComponent<DebugObjectSpawner>();
            gameObject.AddComponent<ReferencesLoader>();
            gameObject.AddComponent<CustomRadioController>();

            helperObj.AddComponent<ModHelper>();
            helperObj.AddComponent<UncleHelper>();
            helperObj.AddComponent<PartIconManager>();

            gameObject.AddComponent<DiscordController>();

            stopWatch = gameObject.AddComponent<Stopwatch>();

            Debug.Log("JaLoader initialized!");

            if (settingsManager.SkipLanguage && !skippedIntro)
            {
                skippedIntro = true;
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

        private void OnGameUnload() 
        {
            if (InitializedInGameMods)
            {
                foreach (Mod mod in modsInitInGame)
                    mod.gameObject.SetActive(false);

                InitializedInGameMods = false;
            }
        }

        private void Update()
        {
            if (modsNumber == 0)
                return;

            if (finishedLoadingMods)
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
                    Console.Instance.Log("JaLoader", $"{modsNumber} mods found! ({disabledMods.Count} disabled)");

                    foreach (Mod mod in modsInitInMenu)
                    {
                        if(!disabledMods.Contains(mod))
                            mod.gameObject.SetActive(true);
                    }

                    InitializedInMenuMods = true;
                }

                if (!InitializedInGameMods && SceneManager.GetActiveScene().buildIndex > 2)
                {
                    foreach (Mod mod in modsInitInGame)
                    {
                        if (!disabledMods.Contains(mod))
                            mod.gameObject.SetActive(true);
                    }

                    InitializedInGameMods = true;
                }
            }       
        }

        public IEnumerator LoadMods()
        {
            Debug.Log("Loading JaLoader mods...");
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

                    ModObject.name = mod.ModID;

                    mod.EventsDeclaration();

                    mod.SettingsDeclaration();

                    if (mod.UseAssets)
                    {
                        mod.AssetsPath = $@"{settingsManager.ModFolderLocation}\Assets\{mod.ModID}";

                        if (!Directory.Exists(mod.AssetsPath))
                            Directory.CreateDirectory(mod.AssetsPath);
                    }

                    mod.CustomObjectsRegistration();

                    string modVersionText = mod.ModVersion;
                    string modName = mod.ModName;

                    if (mod.GitHubLink != string.Empty && mod.GitHubLink != null)
                    {
                        string[] splitLink = mod.GitHubLink.Split('/');

                        string URL = $"https://api.github.com/repos/{splitLink[3]}/{splitLink[4]}/releases/latest";

                        string version = ModHelper.Instance.GetLatestTagFromApiUrl(URL, modName);
                        int versionInt = int.Parse(version.Replace(".", ""));
                        int currentVersion = int.Parse(mod.ModVersion.Replace(".", ""));

                        if(versionInt > currentVersion)
                        {
                            modVersionText = $"{mod.ModVersion} (Latest version: {version})";
                            modName = $"(Update Available!) {mod.ModName}";
                        }
                    }

                    uiManager.modTemplateObject.transform.Find("BasicInfo").Find("ModName").GetComponent<Text>().text = modName;
                    uiManager.modTemplateObject.transform.Find("BasicInfo").Find("ModAuthor").GetComponent<Text>().text = mod.ModAuthor;

                    uiManager.modTemplateObject.transform.Find("Buttons").Find("AboutButton").GetComponent<Button>().onClick.AddListener(delegate { uiManager.ToggleMoreInfo(mod.ModName, mod.ModAuthor, modVersionText, mod.ModDescription); });
                    uiManager.modTemplateObject.transform.Find("Buttons").Find("SettingsButton").GetComponent<Button>().onClick.AddListener(delegate { uiManager.ToggleSettings($"{mod.ModAuthor}_{mod.ModID}_{mod.ModName}-SettingsHolder"); });

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

                    if (mod.settingsIDS.Count > 0)
                        mod.LoadModSettings();

                    modsNumber++;
                }
                catch (Exception ex)
                {
                    uiManager.modTemplateObject.transform.Find("BasicInfo").Find("ModName").GetComponent<Text>().text = modFile.Name;
                    uiManager.modTemplateObject.transform.Find("BasicInfo").Find("ModAuthor").GetComponent<Text>().text = "Invalid mod";

                    uiManager.modTemplateObject.transform.Find("Buttons").Find("AboutButton").GetComponent<Button>().onClick.AddListener(delegate { uiManager.ToggleMoreInfo(modFile.Name, "Invalid mod", "", $"{modFile.Name} is not a valid mod, please remove it from the Mods folder!"); });

                    Console.Instance.LogError("/", ex);
                    Console.Instance.LogError("JaLoader", $"\"{modFile.Name}\" is not a valid mod! Please remove it from the Mods folder.");

                    validMods--;
                }

                uiManager.modTemplateObject = null;
            }

            if (validMods == modsNumber)
            {
                finishedLoadingMods = true;
            }

            if (modsNumber == 0)
            {
                Console.Instance.LogMessage("JaLoader", $"No mods found!");
                Console.Instance.ToggleVisibility(true);

                UIManager.Instance.modTemplatePrefab.transform.parent.parent.parent.parent.Find("NoMods").gameObject.SetActive(true);
            }

            gameObject.GetComponent<Stopwatch>().StopCounting();
            Debug.Log($"Loaded JaLoader mods! ({gameObject.GetComponent<Stopwatch>().timePassed}s)");

            Debug.Log($"JaLoader fully loaded! ({gameObject.GetComponent<Stopwatch>().totalTimePassed}s)");
            Destroy(gameObject.GetComponent<Stopwatch>());

            yield return null;
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

        private void StartUpdate()
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
