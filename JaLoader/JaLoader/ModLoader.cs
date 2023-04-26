using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Reflection;    
using System.IO;
using System.Collections;
using Newtonsoft.Json.Linq;

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

        public List<Mod> modsInitInGame = new List<Mod>();
        public List<Mod> modsInitInMenu = new List<Mod>();

        public int modsNumber;

        public List<Mod> disabledMods = new List<Mod>();
        Dictionary<Mod, Text> modStatusTextRef = new Dictionary<Mod, Text>();
   
        public bool InitializedInGameMods;
        private bool InitializedInMenuMods;
        
        private bool finishedLoadingMods;
        private bool skippedIntro;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            settingsManager = gameObject.AddComponent<SettingsManager>();
            uiManager = gameObject.AddComponent<UIManager>();
            gameObject.AddComponent<ObjectIDManager>();
        }

        private void Update()
        {
            if (settingsManager.SkipLanguage && !skippedIntro)
            {
                skippedIntro = true;
                SceneManager.LoadScene("MainMenu");
            }      

            if (modsNumber == 0)
                return;

            if (finishedLoadingMods)
            {
                if (disabledMods.Count == 0 && settingsManager.DisabledMods.Count != 0)
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
            GameObject modHelperObj = Instantiate(new GameObject());
            modHelperObj.name = "ModHelper";
            modHelperObj.AddComponent<ModReferences>();
            DontDestroyOnLoad(modHelperObj);

            DirectoryInfo d = new DirectoryInfo(settingsManager.ModFolderLocation);
            FileInfo[] mods = d.GetFiles("*.dll");

            int validMods = mods.Length;

            foreach (FileInfo modFile in mods)
            {
                uiManager.modTemplateObject = Instantiate(uiManager.modTemplatePrefab);
                uiManager.modTemplateObject.transform.parent = uiManager.UIVersionCanvas.transform.Find("JMLModsPanel").Find("Scroll View").GetChild(0).GetChild(0).transform;
                uiManager.modTemplateObject.SetActive(true);

                try
                {
                    Assembly modAssembly = Assembly.LoadFrom(modFile.FullName);

                    Type[] allModTypes = modAssembly.GetTypes();

                    string modTypeName = $"{Path.GetFileNameWithoutExtension(modFile.Name)}.{allModTypes[0].Name}";
                    Type modType = modAssembly.GetType(modTypeName);

                    GameObject ModObject = Instantiate(new GameObject());
                    ModObject.transform.parent = null;
                    ModObject.SetActive(false);
                    DontDestroyOnLoad(ModObject);

                    Component ModComponent = ModObject.AddComponent(modType);
                    Mod mod = ModObject.GetComponent<Mod>();
                    ModObject.name = mod.ModID;
                    mod.SettingsDeclaration();

                    if (mod.ModName == null || mod.ModAuthor == null || mod.ModVersion == null)
                    {
                        Console.Instance.LogError(modFile.Name, "Empty mod information.");
                        throw new Exception($"Mod {modFile.Name} contains no information related to its name, author or version.");
                    }

                    if (mod.UseAssets)
                    {
                        mod.AssetsPath = $@"{settingsManager.ModFolderLocation}\Assets\{mod.ModID}";
                        if (!Directory.Exists(mod.AssetsPath))
                        {
                            Console.Instance.LogError(mod.ModID, "Assets folder does not exist!");

                            uiManager.modTemplateObject.transform.Find("BasicInfo").Find("ModName").GetComponent<Text>().text = mod.ModName;
                            uiManager.modTemplateObject.transform.Find("BasicInfo").Find("ModAuthor").GetComponent<Text>().text = mod.ModAuthor;
                            Destroy(ModObject);

                            uiManager.modTemplateObject.transform.Find("Buttons").Find("AboutButton").GetComponent<Button>().onClick.AddListener(delegate { uiManager.ToggleMoreInfo(mod.ModName, mod.ModAuthor, mod.ModVersion, $"{mod.ModName} encountered an error while loading! Check the console for more info."); });

                            throw new Exception($"Mod {mod.ModName} uses custom assets, but its assets folder does not exist.");
                        }
                    }

                    mod.CustomObjectsRegistration();

                    uiManager.modTemplateObject.transform.Find("BasicInfo").Find("ModName").GetComponent<Text>().text = mod.ModName;
                    uiManager.modTemplateObject.transform.Find("BasicInfo").Find("ModAuthor").GetComponent<Text>().text = mod.ModAuthor;

                    uiManager.modTemplateObject.transform.Find("Buttons").Find("AboutButton").GetComponent<Button>().onClick.AddListener(delegate { uiManager.ToggleMoreInfo(mod.ModName, mod.ModAuthor, mod.ModVersion, mod.ModDescription); });
                    uiManager.modTemplateObject.transform.Find("Buttons").Find("SettingsButton").GetComponent<Button>().onClick.AddListener(delegate { uiManager.ToggleSettings($"{mod.ModAuthor}_{mod.ModID}_{mod.ModName}-SettingsHolder"); });

                    if (mod.WhenToInit == WhenToRun.InMenu)
                        modsInitInMenu.Add(mod);

                    if (mod.WhenToInit == WhenToRun.InGame)
                        modsInitInGame.Add(mod);

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
                    Console.Instance.LogError("JModLoader", $"\"{modFile.Name}\" is not a valid mod! Please remove it from the Mods folder.");

                    validMods--;
                }

                uiManager.modTemplateObject = null;
            }

            if (validMods == modsNumber)
            {
                finishedLoadingMods = true;
            }

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
    } 
}
