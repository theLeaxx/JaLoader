﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

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
        //private readonly Dictionary<MonoBehaviour, Text> modStatusTextRef = new Dictionary<MonoBehaviour, Text>();

        public bool InitializedInGameMods;
        private bool InitializedInMenuMods;

        public bool finishedInitializingPartOneMods;
        public bool finishedInitializingPartTwoMods;
        private bool skippedIntro;
        private bool reloadMods;

        public bool IsCrackedVersion { get; private set; }

        //Stopwatch stopWatch;

        Color32 defaultWhiteColor = new Color32(255, 255, 255, 255);
        Color32 defaultGrayColor = new Color32(120, 120, 120, 255);

        private string[] requiredDLLs = new string[] {
            "0Harmony.dll",
            "HarmonyXInterop.dll",
            "NAudio.dll",
            "Mono.Cecil.dll",
            "Mono.Cecil.Mdb.dll",
            "Mono.Cecil.Pdb.dll",
            "Mono.Cecil.Rocks.dll",
            "MonoMod.Backports.dll",
            "MonoMod.RuntimeDetour.dll",
            "MonoMod.Utils.dll",
            "MonoMod.ILHelpers.dll"
        };

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            gameObject.name = "JaLoader";

            uiManager = gameObject.AddComponent<UIManager>();
            settingsManager = gameObject.AddComponent<SettingsManager>();
            gameObject.AddComponent<ReferencesLoader>();

            /*if (CheckForMissingDLLs() != "None")
            {
                CreateImportantNotice("\n\nOne or more required DLLs were not found. You can try:", "Reinstalling JaLoader with JaPatcher\n\n\nCopying the files from JaPatcher's directory/Assets/Managed to Jalopy_Data/Managed");
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

            GameObject consoleObj = Instantiate(new GameObject());
            consoleObj.AddComponent<Console>();
            consoleObj.name = "ModConsole";
            DontDestroyOnLoad(consoleObj);

            gameObject.AddComponent<CustomObjectsManager>();
            gameObject.AddComponent<DebugObjectSpawner>();
            gameObject.AddComponent<ReferencesLoader>();
            gameObject.AddComponent<CustomRadioController>();
            gameObject.AddComponent<ExtrasManager>();

            helperObj.AddComponent<ModHelper>();
            helperObj.AddComponent<UncleHelper>();
            helperObj.AddComponent<PartIconManager>();
            helperObj.AddComponent<HarmonyManager>();

            gameObject.AddComponent<DiscordController>();

            stopWatch = gameObject.AddComponent<Stopwatch>();

            Debug.Log("JaLoader initialized!");

            if (settingsManager.SkipLanguage && !skippedIntro)
            {
                skippedIntro = true;
                settingsManager.selectedLanguage = true;
                SceneManager.LoadScene("MainMenu");
            }*/
        }

        private void Update()
        {
            if (modsNumber == 0)
                return;

            if (finishedInitializingPartOneMods && !reloadMods)
            {
                if (!InitializedInMenuMods)
                {
                    foreach (MonoBehaviour monoBehaviour in modsInitInMenuIncludingBIX)
                    {
                        if (monoBehaviour is Mod mod)
                        {
                            //CheckForDependencies(mod);

                            try
                            {
                                mod.EventsDeclaration();

                                mod.SettingsDeclaration();

                                mod.CustomObjectsRegistration();

                                if (mod.settingsIDS.Count > 0)
                                {
                                    //mod.LoadModSettings();
                                    //mod.SaveModSettings();
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

                                //Console.LogError("JaLoader", $"An error occured while trying to load mod \"{mod.ModName}\"");

                                //modsToRemoveAfter.Add(mod);

                                continue;
                                throw;
                            }
                        }
                    }

                    InitializedInMenuMods = true;
                }
            }
        }

        public IEnumerator InitializeMods()
        {
            Debug.Log("Initializing JaLoader mods...");

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
                try
                {
                    Assembly modAssembly = Assembly.LoadFrom(modFile.FullName);

                    Type[] allModTypes = modAssembly.GetTypes();

                    Type modType = allModTypes.FirstOrDefault(t => t.BaseType != null && t.BaseType.Name == "Mod");

                    GameObject ModObject = (GameObject)Instantiate(new GameObject());
                    ModObject.transform.parent = null;
                    ModObject.SetActive(false);
                    DontDestroyOnLoad(ModObject);

                    Component ModComponent = ModObject.AddComponent(modType);
                    Mod mod = ModObject.GetComponent<Mod>();

                    //ModObject.name = $"{mod.ModID}_{mod.ModAuthor}_{mod.ModName}";

                    switch (mod.WhenToInit)
                    {
                        case WhenToInit.InMenu:
                            modsInitInMenuIncludingBIX.Add(mod);
                            break;

                        case WhenToInit.InGame:
                            modsInitInGame.Add(mod);
                            break;
                    }

                    Debug.Log($"Part 1/2 of initialization for mod {mod.ModName} completed");
                    modsNumber++;
                }
                catch (Exception ex)
                {
                    validMods--;

                    Debug.Log($"Failed to initialize mod {modFile.Name}");
                    Debug.Log(ex);
                    //Console.LogError("JaLoader", $"An error occured while trying to initialize mod \"{modFile.Name}\": ");
                }
            }

            finishedInitializingPartOneMods = true;

            yield return null;
        }
    }
}
