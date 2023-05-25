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

        private void Start()
        {
            DontDestroyOnLoad(gameObject);

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

            helperObj.AddComponent<ModHelper>();
            helperObj.AddComponent<UncleHelper>();
            helperObj.AddComponent<PartIconManager>();

            if (settingsManager.SkipLanguage && !skippedIntro)
            {
                skippedIntro = true;
                SceneManager.LoadScene("MainMenu");
            }
        }

        private void OnGameLoad()
        {
            if (settingsManager.UseExperimentalCharacterController)
                GameObject.Find("First Person Controller").AddComponent<ExperimentalCharacterController>();
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
            DirectoryInfo d = new DirectoryInfo(settingsManager.ModFolderLocation);
            FileInfo[] mods = d.GetFiles("*.dll");

            int validMods = mods.Length;

            foreach (FileInfo modFile in mods)
            {
                uiManager.modTemplateObject = Instantiate(uiManager.modTemplatePrefab);
                uiManager.modTemplateObject.transform.parent = uiManager.UIVersionCanvas.transform.Find("JLModsPanel/Scroll View").GetChild(0).GetChild(0).transform;
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

                        string version = GetLatestTagFromApiUrl(URL);
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

            if(modsNumber == 0)
                Console.Instance.LogMessage("JaLoader", $"No mods found!");

            yield return null;
        }

        private string GetLatestTagFromApiUrl(string URL)
        {
            UnityWebRequest request = UnityWebRequest.Get(URL);
            request.SetRequestHeader("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)");
            request.SetRequestHeader("Accept", "application/vnd.github.v3+json");

            request.SendWebRequest();

            while (!request.isDone)
            {
                // wait for the request to complete
            }

            string tagName = null;

            if (!request.isNetworkError && !request.isHttpError)
            {
                string json = request.downloadHandler.text;
                Release release = JsonUtility.FromJson<Release>(json);
                tagName = release.tag_name;
            }
            else
            {
                Console.Instance.LogError($"Error getting response for URL \"{URL}\": {request.error}");
            }

            return tagName;
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
    }

    [Serializable]
    class Release
    {
        public string tag_name = "";
    }
}
