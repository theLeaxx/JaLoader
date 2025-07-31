using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Reflection;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UIKeyBinding;
using static UnityEngine.EventSystems.EventTrigger;
using Debug = UnityEngine.Debug;

namespace JaLoader
{
    public class UIManager : MonoBehaviour
    {
        #region Singleton
        public static UIManager Instance;

        public int[] values;
        public bool[] keys;

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

            EventsManager.Instance.OnMenuLoad += OnMenuLoad;
            EventsManager.Instance.OnLoadStart += OnLoadStart;
            EventsManager.Instance.OnGameLoad += OnGameLoad;

            values = (int[])Enum.GetValues(typeof(KeyCode));
            keys = new bool[values.Length];

            gameObject.AddComponent<AudioSource>();
            audioSource = gameObject.GetComponent<AudioSource>();
        }

        #endregion

        #region Declarations

        internal GameObject JLCanvas;     

        internal Texture2D knobTexture;

        private MainMenuBookC book;

        private bool inOptions;
        private bool inModsOptions;
        private bool inModsList;
        private bool isObstructing;
        internal bool CanCloseMap = true;

        private Text JaLoaderText;
        private GameObject ModsLocationText;
        private GameObject JLBookUI;

        internal Transform ModsListContent;
        private Transform ModsList;
        internal Text ModsCountText;
        private InputField ModsSearchBar;
        private GameObject ModEntryTemplate;
        private GameObject ModsInstallButton;
        private GameObject ModsSettingsList;
        internal GameObject ModsSettingsContent;

        internal GameObject ConsoleMessageTemplate;
        internal GameObject CataloguePageTemplate;
        internal GameObject CatalogueEntryTemplate;
        internal GameObject ObjectsList;
        internal GameObject ObjectEntryTemplate;
        internal Text CurrentSelectedObjectText;

        public GameObject ModSettingsHolder;
        public GameObject ModSettingsNameTemplate;
        public GameObject ModSettingsHeaderTemplate;
        public GameObject ModSettingsDropdownTemplate;
        public GameObject ModSettingsToggleTemplate;
        public GameObject ModSettingsSliderTemplate;
        public GameObject ModSettingsKeybindTemplate;
        public GameObject ModSettingsInputTemplate;

        internal Dictionary<string, Dropdown> AllSettingsDropdowns = new Dictionary<string, Dropdown>();
        internal Dictionary<string, Slider> AllSettingsSliders = new Dictionary<string, Slider>();
        internal Dictionary<string, InputField> AllSettingsInputFields = new Dictionary<string, InputField>();

        #region Panels
        internal GameObject JLPanel;
        internal GameObject JLModsPanel;
        internal GameObject JLSettingsPanel;
        internal GameObject JLCatalogue;
        internal GameObject JLConsole;
        internal GameObject JLNoticePanel;
        internal GameObject JLObjectsList;
        internal GameObject JLFPSText;
        internal GameObject JLDebugText;

        private GameObject MainSettings;
        private GameObject TweaksSettings;
        private GameObject PreferencesSettings;
        private GameObject AccessibilitySettings;

        private GameObject MoreModInfoPanel;
        #endregion

        private AudioClip buttonClickSound;
        private AudioSource audioSource;
        private List<string> allNotices = new List<string>();

        internal Dictionary<GenericModData, GameObject> modEntries = new Dictionary<GenericModData, GameObject>();

        #endregion

        private bool IsBookClosed()
        {
            if (book == null || SceneManager.GetActiveScene().buildIndex != 1)
                return true;

            return (bool)book.GetType().GetField("bookClosed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(book);
        }

        public void ResetCanCloseMap()
        {
            CanCloseMap = true;
        }

        internal void ShowModLoaderBookUI()
        {
            JLCanvas.transform.GetChild(0).Find("BookUI").gameObject.SetActive(true);
        }

        internal void HideModLoaderBookUI()
        {
            JLCanvas.transform.GetChild(0).Find("BookUI").gameObject.SetActive(false);
        }

        private void Update()
        {
            if (JLCanvas == null)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (JLModsPanel.activeSelf)
                    ToggleModMenu();

                if (JLSettingsPanel.activeSelf)
                    ToggleModLoaderSettings_Main();

                RefreshUI();
            }

            if (inOptions && Input.GetMouseButtonDown(0))
                if (AnyDropdownClicked())
                    RefreshUI();

            if (inModsList && Input.GetMouseButtonDown(0))
                if (AllSettingsDropdowns["ShowDisabledMods"].Find("Dropdown List"))
                    RefreshUI();

            if (inModsOptions && Input.GetMouseButtonDown(0))
                if (AnyModDropdownClicked())
                    RefreshUI();
        }

        private bool AnyDropdownClicked()
        {
            foreach (var dropdown in AllSettingsDropdowns.Values)
            {
                if (dropdown.Find("Dropdown List") != null)
                    return true;
            }

            return false;
        }

        private bool AnyModDropdownClicked()
        {
            foreach (Transform item in ModsSettingsContent.transform)
            {
                if (item.Find("Dropdown List"))
                    return true;
            }
            return false;
        }

        private void OnMenuLoad()
        {
            if (!SettingsManager.loadedFirstTime)
            {
                var loadingScreenScript = gameObject.AddComponent<LoadingScreen>();
                loadingScreenScript.ShowLoadingScreen();
            }

            if (JLCanvas == null)
                StartCoroutine(LoadUIDelay());

            SetNewspaperText();

            book = FindObjectOfType<MainMenuBookC>();

            ToggleJaLoaderInfoStatus(true);

            AddObjectShortcuts();
        }

        internal void NoMods()
        {
            ModsCountText.text = "No mods installed";
            JLModsPanel.FindDeepChildObject("NoMods").SetActive(true);
        }

        private void OnLoadStart()
        {
            ToggleJaLoaderInfoStatus(false);
            Console.Instance.ToggleVisibility(false);

            if (inOptions)
                CloseAllSettings();
        }

        private void CloseAllSettings()
        {
            inOptions = false;
            MainSettings.SetActive(false);
            PreferencesSettings.SetActive(false);
            TweaksSettings.SetActive(false);
            AccessibilitySettings.SetActive(false);
        }

        private void OnGameLoad()
        {
            var uiRoot = GameObject.Find("UI Root");

            AddPauseButtons();

            var optionsSubMenu = uiRoot.transform.GetChild(5).Find("Restart Confirm/Accept").gameObject;
            optionsSubMenu.GetComponent<UIButton>().onClick.Clear();
            optionsSubMenu.GetComponent<UIButton>().onClick.Add(new EventDelegate(SaveDataThenReload));
            
            FixTranslations();

            if (gameObject.GetComponent<LoadingScreen>())
                gameObject.GetComponent<LoadingScreen>().DeleteLoadingScreen();
        }

        private void AddPauseButtons()
        {
            var uiRoot = GameObject.Find("UI Root");
            var optionsButton = uiRoot.transform.Find("Options").gameObject;

            var modsButton = Instantiate(optionsButton, uiRoot.transform, true);
            modsButton.SetActive(false);
            modsButton.name = "Mods";
            modsButton.transform.localPosition += new Vector3(0, 50, 0);
            modsButton.GetComponent<UIButton>().onClick.Clear();
            modsButton.GetComponent<UIButton>().onClick.Add(new EventDelegate(ToggleModMenu));
            modsButton.GetComponent<UILabel>().text = "Mods";
            modsButton.GetComponent<UILabel>().ProcessText();

            var settingsButton = Instantiate(optionsButton, uiRoot.transform, true);
            settingsButton.SetActive(false);
            settingsButton.name = "ModLoader Settings";
            settingsButton.transform.localPosition += new Vector3(0, 25, 0);
            settingsButton.GetComponent<UIButton>().onClick.Clear();
            settingsButton.GetComponent<UIButton>().onClick.Add(new EventDelegate(ToggleModLoaderSettings_Main));
            settingsButton.GetComponent<UILabel>().text = "ModLoader Options";
            settingsButton.GetComponent<UILabel>().ProcessText();
        }

        private void SaveDataThenReload()
        {
            MainMenuC.Global.SaveData(0);

            var loadingScreenScript = gameObject.AddComponent<LoadingScreen>();
            loadingScreenScript.useCircle = false;
            loadingScreenScript.dontDestroyOnLoad = true;
            loadingScreenScript.ShowLoadingScreen();

            SceneManager.LoadScene(2);
        }

        public void ToggleJaLoaderInfoStatus(bool show)
        {
            if (JLCanvas == null)
                return;

            RefreshUI();
            JLPanel.SetActive(show);
        }

        private void SetReferences()
        {
            JLPanel = JLCanvas.FindObject("JLPanel");
            JLModsPanel = JLCanvas.FindDeepChildObject("JLModsPanel");
            JLSettingsPanel = JLCanvas.FindDeepChildObject("JLSettingsPanel");
            JLConsole = JLCanvas.FindDeepChildObject("Console");
            JLNoticePanel = JLCanvas.FindDeepChildObject("JLNotice");
            JLCatalogue = JLCanvas.FindDeepChildObject("JLCatalogue");
            JLObjectsList = JLCanvas.FindDeepChildObject("JLObjectsList");

            JaLoaderText = JLPanel.FindObject("JaLoaderText").GetComponent<Text>();
            ModsLocationText = JLPanel.FindObject("ModsLocationText");
            JLBookUI = JLPanel.FindDeepChildObject("BookUI");

            ModsList = JLModsPanel.Find("ModsList");
            ModsListContent = ModsList.FindDeepChild("Content");
            ModEntryTemplate = ModsListContent.FindDeepChildObject("ModTemplate");
            ModsCountText = JLModsPanel.FindObject("ModsCount").GetComponent<Text>();
            ModsSearchBar = JLModsPanel.FindInputField("SearchBar");
            ModsSettingsList = JLModsPanel.FindDeepChildObject("SettingsList");
            ModsSettingsContent = ModsSettingsList.FindDeepChildObject("Content");
            ModsInstallButton = JLModsPanel.FindDeepChildObject("InstallButton");

            ConsoleMessageTemplate = JLConsole.FindDeepChildObject("MessageTemplate");
            MoreModInfoPanel = JLModsPanel.FindObject("MoreModInfo");
            CataloguePageTemplate = JLCatalogue.FindDeepChildObject("MainTemplate");
            CatalogueEntryTemplate = CataloguePageTemplate.FindDeepChildObject("Template");
            ObjectsList = JLObjectsList.FindObject("ObjectsList");
            ObjectEntryTemplate = ObjectsList.FindDeepChildObject("ItemTemplate");
            CurrentSelectedObjectText = ObjectsList.FindObject("CurrentlySelectedText").GetComponent<Text>();

            MainSettings = JLSettingsPanel.FindDeepChildObject("Main");
            TweaksSettings = JLSettingsPanel.FindDeepChildObject("Tweaks");
            PreferencesSettings = JLSettingsPanel.FindDeepChildObject("Preferences");
            AccessibilitySettings = JLSettingsPanel.FindDeepChildObject("Accessibility");

            JLFPSText = JLCanvas.FindObject("FPSCounter");
            JLDebugText = JLCanvas.FindObject("DebugInfo");

            ModsSettingsList.FindButton("SaveButton").onClick.AddListener(SaveModSettings);
            ModsSettingsList.FindButton("ResetButton").onClick.AddListener(ResetModSettings);

            ModSettingsHolder = ModsSettingsContent.Find("SettingsHolder").gameObject;
            ModSettingsNameTemplate = ModsSettingsContent.Find("ModName").gameObject;
            ModSettingsHeaderTemplate = ModsSettingsContent.Find("HeaderTemplate").gameObject;
            ModSettingsDropdownTemplate = ModsSettingsContent.Find("DropdownTemplate").gameObject;
            ModSettingsToggleTemplate = ModsSettingsContent.Find("ToggleTemplate").gameObject;
            ModSettingsSliderTemplate = ModsSettingsContent.Find("SliderTemplate").gameObject;
            ModSettingsKeybindTemplate = ModsSettingsContent.Find("KeybindTemplate").gameObject;
            ModSettingsInputTemplate = ModsSettingsContent.Find("InputTemplate").gameObject;

            var clickers = GameObject.Find("UI Root").Find("Options/OptionsGameplay/Back").GetComponent<MainMenuClickersC>();
            buttonClickSound = clickers.audioClip;
        }

        private void AddListenerEvents()
        {
            JLBookUI.FindButton("ModsButton").onClick.AddListener(ToggleModMenu);
            JLBookUI.FindButton("OptionsButton").onClick.AddListener(ToggleModLoaderSettings_Main);

            JLModsPanel.FindButton("ExitButton").onClick.AddListener(ToggleModMenu);

            MainSettings.FindButton("ExitButton").onClick.AddListener(ToggleModLoaderSettings_Main);
            MainSettings.FindButton("Buttons/PreferencesButton").onClick.AddListener(ToggleModLoaderSettings_Preferences);
            MainSettings.FindButton("Buttons/TweaksButton").onClick.AddListener(ToggleModLoaderSettings_Tweaks);
            MainSettings.FindButton("Buttons/AccessibilityButton").onClick.AddListener(ToggleModLoaderSettings_Accessibility);

            PreferencesSettings.FindButton("BackButton").onClick.AddListener(ToggleModLoaderSettings_Preferences);
            TweaksSettings.FindButton("BackButton").onClick.AddListener(ToggleModLoaderSettings_Tweaks);
            AccessibilitySettings.FindButton("BackButton").onClick.AddListener(ToggleModLoaderSettings_Accessibility);

            PreferencesSettings.FindButton("SaveButton").onClick.AddListener(SaveAndApplyValues);
            TweaksSettings.FindButton("SaveButton").onClick.AddListener(SaveAndApplyValues);
            AccessibilitySettings.FindButton("SaveButton").onClick.AddListener(SaveAndApplyValues);

            JLNoticePanel.FindButton("UnderstandButton").onClick.AddListener(delegate { CloseNotice(); });
            JLNoticePanel.FindButton("DontShowAgainButton").onClick.AddListener(delegate { CloseNotice(true); });

            ModsSearchBar.onValueChanged.AddListener(delegate { OnInputValueChanged_ModsList(); });
            JLModsPanel.FindDeepButton("InstallButton").onClick.AddListener(InstallMod);

            AllSettingsDropdowns["ShowDisabledMods"].onValueChanged.AddListener(delegate { ShowDisabledMods(); });

            AllSettingsDropdowns["MirrorDistance"].onValueChanged.AddListener(delegate { GameTweaks.Instance.UpdateMirrors((MirrorDistances)AllSettingsDropdowns["MirrorDistance"].value); });
            AllSettingsDropdowns["CursorMode"].onValueChanged.AddListener(delegate { GameTweaks.Instance.ChangeCursor((CursorMode)AllSettingsDropdowns["CursorMode"].value); });
        }

        private void AddAllSettingsToDictionaries()
        {
            AllSettingsDropdowns.Add("ShowDisabledMods", JLModsPanel.FindObject("ToggleShowDisabledMods").GetComponentInChildren<Dropdown>());

            AllSettingsDropdowns.Add("ConsoleMode", PreferencesSettings.FindDeepDropdown("ConsoleMode"));
            AllSettingsDropdowns.Add("ConsolePosition", PreferencesSettings.FindDeepDropdown("ConsolePosition"));
            AllSettingsDropdowns.Add("ShowModsFolderLocation", PreferencesSettings.FindDeepDropdown("ShowModsFolderLocation"));
            AllSettingsDropdowns.Add("EnableJaDownloader", PreferencesSettings.FindDeepDropdown("EnableJaDownloader"));
            AllSettingsDropdowns.Add("UpdateCheckFrequency", PreferencesSettings.FindDeepDropdown("UpdateCheckFrequency"));
            AllSettingsDropdowns.Add("SkipLanguageSelectionScreen", PreferencesSettings.FindDeepDropdown("SkipLanguageSelectionScreen"));
            AllSettingsDropdowns.Add("DiscordRichPresence", PreferencesSettings.FindDeepDropdown("DiscordRichPresence"));
            AllSettingsDropdowns.Add("DebugMode", PreferencesSettings.FindDeepDropdown("DebugMode"));

            AllSettingsDropdowns.Add("MenuMusic", TweaksSettings.FindDeepDropdown("MenuMusic"));
            AllSettingsDropdowns.Add("MenuMusicVolume", TweaksSettings.FindDeepSlider("MenuMusicVolume").GetComponent<Dropdown>());
            AllSettingsDropdowns.Add("CustomSongs", TweaksSettings.FindDeepDropdown("CustomSongs"));
            AllSettingsDropdowns.Add("CustomSongsBehaviour", TweaksSettings.FindDeepDropdown("CustomSongsBehaviour"));
            AllSettingsDropdowns.Add("RadioAds", TweaksSettings.FindDeepDropdown("RadioAds"));
            AllSettingsDropdowns.Add("Uncle", TweaksSettings.FindDeepDropdown("Uncle"));
            AllSettingsDropdowns.Add("UseEnhancedMovement", TweaksSettings.FindDeepDropdown("UseEnhancedMovement"));
            AllSettingsDropdowns.Add("ChangeLicensePlate", TweaksSettings.FindDeepDropdown("ChangeLicensePlate"));
            AllSettingsDropdowns.Add("ShowFPSCounter", TweaksSettings.FindDeepDropdown("ShowFPSCounter"));
            AllSettingsDropdowns.Add("FixLaikaShopMusic", TweaksSettings.FindDeepDropdown("FixLaikaShopMusic"));
            AllSettingsDropdowns.Add("Replace0WithBanned", TweaksSettings.FindDeepDropdown("Replace0WithBanned"));
            AllSettingsDropdowns.Add("MirrorDistance", TweaksSettings.FindDeepDropdown("MirrorDistance"));
            AllSettingsDropdowns.Add("CursorMode", TweaksSettings.FindDeepDropdown("CursorMode"));
            AllSettingsDropdowns.Add("FixItemsFallingBehindShop", TweaksSettings.FindDeepDropdown("FixItemsFallingBehindShop"));
            AllSettingsDropdowns.Add("FixGuardsFlags", TweaksSettings.FindDeepDropdown("FixGuardsFlags"));

            AllSettingsSliders.Add("MenuMusicVolume", TweaksSettings.FindDeepSlider("MenuMusicVolume"));
            AllSettingsInputFields.Add("LicensePlateText", TweaksSettings.FindDeepInputField("InputField"));

            AccessibilitySettings.FindDeepButton("SwitchLanguage").onClick.AddListener(SwitchLanguage);
            AccessibilitySettings.FindDeepButton("OpenModsFolder").onClick.AddListener(OpenModsFolder);
            AccessibilitySettings.FindDeepButton("OpenSavesFolder").onClick.AddListener(OpenSavesFolder);
            AccessibilitySettings.FindDeepButton("OpenOutputLog").onClick.AddListener(OpenOutputLog);
        }

        private void SetConsolePosition(ConsolePositions pos)
        {
            var modLoaderTextRT = JaLoaderText.GetComponent<RectTransform>();
            var consoleRectTransform = JLConsole.GetComponent<RectTransform>();

            var modsFolderTextRT = ModsLocationText.GetComponent<RectTransform>();
            var modsFolderText = ModsLocationText.GetComponent<Text>();

            switch (pos)
            {
                case ConsolePositions.TopLeft:
                    consoleRectTransform.anchorMin = new Vector2(0, 1);
                    consoleRectTransform.anchorMax = new Vector2(0, 1);
                    consoleRectTransform.pivot = new Vector2(0, 1);
                    consoleRectTransform.position = new Vector2(5, Screen.height - 5);

                    modLoaderTextRT.anchorMin = new Vector2(1, 1);
                    modLoaderTextRT.anchorMax = new Vector2(1, 1);
                    modLoaderTextRT.pivot = new Vector2(1, 1);
                    modLoaderTextRT.position = new Vector2(Screen.width - 10, Screen.height - 5);
                    JaLoaderText.alignment = TextAnchor.MiddleRight;

                    modsFolderTextRT.anchorMin = new Vector2(1, 1);
                    modsFolderTextRT.anchorMax = new Vector2(1, 1);
                    modsFolderTextRT.pivot = new Vector2(1, 1);
                    modsFolderTextRT.position = new Vector2(Screen.width - 10, Screen.height - 30);
                    JaLoaderText.alignment = TextAnchor.MiddleRight;
                    break;

                case ConsolePositions.TopRight:
                    consoleRectTransform.anchorMin = new Vector2(1, 1);
                    consoleRectTransform.anchorMax = new Vector2(1, 1);
                    consoleRectTransform.pivot = new Vector2(1, 1);
                    consoleRectTransform.position = new Vector2(Screen.width - 5, Screen.height - 5);

                    modLoaderTextRT.anchorMin = new Vector2(0, 1);
                    modLoaderTextRT.anchorMax = new Vector2(0, 1);
                    modLoaderTextRT.pivot = new Vector2(0, 1);
                    modLoaderTextRT.position = new Vector2(10, Screen.height - 5);
                    JaLoaderText.alignment = TextAnchor.MiddleLeft;

                    modsFolderTextRT.anchorMin = new Vector2(0, 1);
                    modsFolderTextRT.anchorMax = new Vector2(0, 1);
                    modsFolderTextRT.pivot = new Vector2(0, 1);
                    modsFolderTextRT.position = new Vector2(10, Screen.height - 30);
                    JaLoaderText.alignment = TextAnchor.MiddleLeft;
                    break;

                case ConsolePositions.BottomLeft:
                    consoleRectTransform.anchorMin = new Vector2(0, 0);
                    consoleRectTransform.anchorMax = new Vector2(0, 0);
                    consoleRectTransform.pivot = new Vector2(0, 0);
                    consoleRectTransform.position = new Vector2(5, 5);

                    modLoaderTextRT.anchorMin = new Vector2(0, 1);
                    modLoaderTextRT.anchorMax = new Vector2(0, 1);
                    modLoaderTextRT.pivot = new Vector2(0, 1);
                    modLoaderTextRT.position = new Vector2(10, Screen.height - 5);
                    JaLoaderText.alignment = TextAnchor.MiddleLeft;

                    modsFolderTextRT.anchorMin = new Vector2(0, 1);
                    modsFolderTextRT.anchorMax = new Vector2(0, 1);
                    modsFolderTextRT.pivot = new Vector2(0, 1);
                    modsFolderTextRT.position = new Vector2(10, Screen.height - 30);
                    JaLoaderText.alignment = TextAnchor.MiddleLeft;
                    break;

                case ConsolePositions.BottomRight:
                    consoleRectTransform.anchorMin = new Vector2(1, 0);
                    consoleRectTransform.anchorMax = new Vector2(1, 0);
                    consoleRectTransform.pivot = new Vector2(1, 0);
                    consoleRectTransform.position = new Vector2(Screen.width - 5, 5);

                    modLoaderTextRT.anchorMin = new Vector2(0, 1);
                    modLoaderTextRT.anchorMax = new Vector2(0, 1);
                    modLoaderTextRT.pivot = new Vector2(0, 1);
                    modLoaderTextRT.position = new Vector2(10, Screen.height - 5);
                    JaLoaderText.alignment = TextAnchor.MiddleLeft;

                    modsFolderTextRT.anchorMin = new Vector2(0, 1);
                    modsFolderTextRT.anchorMax = new Vector2(0, 1);
                    modsFolderTextRT.pivot = new Vector2(0, 1);
                    modsFolderTextRT.position = new Vector2(10, Screen.height - 30);
                    JaLoaderText.alignment = TextAnchor.MiddleLeft;
                    break;
            }
        }

        private void SetVersionAndLocationText()
        {
            if (UpdateUtils.JaLoaderUpdateAvailable(out string latestVersion))
            {
                SetObstructRay(true);

                JaLoaderText.text = $"JaLoader <color={(SettingsManager.IsPreReleaseVersion ? "red" : "yellow")}>{SettingsManager.GetVersionString()}</color> loaded! (<color=lime>{latestVersion} available!</color>)";

                var dialog = JLCanvas.FindObject("JLUpdateDialog");
                dialog.FindButton("YesButton").onClick.AddListener(() => UpdateUtils.StartJaLoaderUpdate());
                dialog.Find("Subtitle").GetComponent<Text>().text = $"{SettingsManager.GetVersionString()} ➔ {latestVersion}";
                dialog.FindButton("NoButton").onClick.AddListener(delegate { dialog.SetActive(false); SetObstructRay(false); });
                dialog.FindButton("OpenGitHubButton").onClick.AddListener(() => Application.OpenURL($"{SettingsManager.JaLoaderGitHubLink}/releases/latest"));
                dialog.SetActive(true);

                MakeWrenchGreen();
            }
            else
                JaLoaderText.text = $"JaLoader <color={(SettingsManager.IsPreReleaseVersion ? "red" : "yellow")}>{SettingsManager.GetVersionString()}</color> loaded!";

            ModsLocationText.GetComponent<Text>().text = $"Mods folder: <color=yellow>{SettingsManager.ModFolderLocation}</color>";
        }

        private IEnumerator LoadUIDelay()
        {
            DebugUtils.SignalStartUI();

            yield return new WaitForEndOfFrame();

            var bundleLoadReq = AssetBundle.LoadFromFileAsync(Path.Combine(SettingsManager.ModFolderLocation, @"Required\JaLoader_UI.unity3d"));

            yield return bundleLoadReq;
            yield return new WaitForEndOfFrame();

            AssetBundle ab = bundleLoadReq.assetBundle;

            if (ab == null)
            {
                StopAllCoroutines();

                JaLoaderCore.Instance.CreateErrorMessage("\n\nThe file 'JaLoader_UI.unity3d' was not found. You can try:", "Reinstalling JaLoader with JaPatcher\n\n\nCopying the file from JaPatcher's directory/Assets/Required to Mods/Required");
                JaLoaderCore.Instance.DestroyJaLoader();

                yield break;
            }

            var assetLoadRequest = ab.LoadAssetAsync<GameObject>("JLCanvas.prefab");

            yield return assetLoadRequest;

            var UIPrefab = assetLoadRequest.asset as GameObject;
            yield return new WaitForEndOfFrame();

            JLCanvas = Instantiate(UIPrefab);
            DontDestroyOnLoad(JLCanvas);
            JLCanvas.name = "JLCanvas";

            yield return new WaitForEndOfFrame();

            try
            {
                SetReferences();
                knobTexture = ab.LoadAsset<Texture2D>("knob.png");

                AddButtonsToBookLogic();

                AddAllSettingsToDictionaries();

                SetVersionAndLocationText();

                AddListenerEvents();

                Console.Instance.Init();

                if (SettingsManager.HideModFolderLocation)
                    ModsLocationText.SetActive(false);

                SetOptionsValues();
                
                SetConsolePosition(SettingsManager.ConsolePosition);

                Console.LogMessage("JaLoader", $"JaLoader {SettingsManager.GetVersionString()} loaded successfully!");
                DebugUtils.SignalFinishedUI();
            }
            catch (Exception e)
            {
                Console.LogMessage("JaLoader", $"JaLoader {SettingsManager.GetVersionString()} failed to load!");
                DebugUtils.StopCounting();
                Debug.Log($"Failed to load JaLoader UI!");

                Debug.Log($"Exception: {e}");

                FindObjectOfType<LoadingScreen>().DeleteLoadingScreen();

                ShowNotice("JaLoader failed to load!", "JaLoader failed to fully load the UI. This is likely due to an outdated JaLoader_UI.unity3d file. Please try reinstalling JaLoader with JaPatcher.");

                if (!SettingsManager.updateAvailable)
                    JaLoaderText.text = $"JaLoader <color=red>{SettingsManager.GetVersionString()}</color> failed to load!";
                else
                    JaLoaderText.text = $"JaLoader <color=red>{SettingsManager.GetVersionString()}</color> failed to load! (<color=lime>Update available!</color>)";
                throw;
            }
            finally
            {
                if (GameUtils.IsCrackedGame)
                    ShowNotice("PIRATED GAME DETECTED", "You are using a pirated version of Jalopy.\r\n\r\nYou may encounter issues with certain mods, as well as more bugs in general.\r\n\r\nIf you encounter any game-breaking bugs, feel free to submit them to the official GitHub for JaLoader. Remember to mark them with the \"pirated\" tag!\r\n\r\nHave fun!");

                if (SettingsManager.IsPreReleaseVersion)
                    ShowNotice("USING A PRE-RELEASE VERSION OF JALOADER", "You are using a pre-release version of JaLoader.\r\n\r\nThese versions are prone to bugs and may cause issues with certain mods.\r\n\r\nPlease report any bugs you encounter to the JaLoader GitHub page, marking them with the \"pre-release\" tag.\r\n\r\nHave fun!");

                if (!SettingsManager.AskedAboutJaDownloader && !SettingsManager.EnableJaDownloader)
                    ShowJaDownloaderNotice();

                EventsManager.Instance.OnUILoadFinish();

                ab.Unload(false);
            }
        }

        private void AddButtonsToBookLogic()
        {
            var contentsAsList = book.frontPageContents.ToList();
            contentsAsList.Add(JLCanvas.transform.Find("JLPanel/BookUI").gameObject);
            book.frontPageContents = contentsAsList.ToArray();
        }

        internal void WriteConsoleStartMessage()
        {
            string message = $"{ModManager.Mods.Count} mods found!";

            if (ModManager.GetDisabledModsCount() > 0 || ModManager.GetBepinExModsCount() > 0)
            {
                message += " (";

                if (ModManager.GetDisabledModsCount() > 0)
                    message += $"{ModManager.GetDisabledModsCount()} disabled";

                if (ModManager.GetBepinExModsCount() > 0)
                {
                    if (ModManager.GetBepinExModsCount() > 0)
                        message += ", ";
                    
                    message += $"{ModManager.GetBepinExModsCount()} BepInEx mods";
                }

                message += ")";
            }

            Console.Log("JaLoader", message);
        }

        internal void ShowUpdateAvailableForMod(GenericModData data, string latestVersion)
        {
            modEntries[data].FindDeepButton("AboutButton").onClick.RemoveAllListeners();
            modEntries[data].FindDeepButton("AboutButton").onClick.AddListener(delegate { ToggleMoreInfo(data.ModName, data.ModAuthor, $"{data.ModVersion} <color=green>(Latest version: {latestVersion})</color>", data.ModDescription); });

            modEntries[data].FindDeepChildObject("ModName").GetComponent<Text>().text = $"<color=green>(Update Available!)</color> {(SettingsManager.DebugMode == false ? data.ModName : data.ModID)}";

            if(!isNutGreen)
                MakeNutGreen();
        }

        internal Text CreateModEntryReturnText(GenericModData modData)
        {   
            var newEntry = Instantiate(ModEntryTemplate);
            newEntry.transform.SetParent(ModsListContent, false);
            newEntry.SetActive(true);

            if (modData.IsBepInExMod)
            {
                modData.ModDescription += "\n\nThis mod was made using BepInEx. Some features might not work as intended.";
                newEntry.name = $"BepInEx_CompatLayer_{modData.ModID}_Mod";
            }
            else
                newEntry.name = $"{modData.ModID}_{modData.ModAuthor}_{modData.ModName}_Mod";

            if(modData.GitHubLink != null && modData.GitHubLink != string.Empty)
            {
                newEntry.FindDeepButton("GitHubButton").onClick.AddListener(() => Application.OpenURL(modData.GitHubLink));
                newEntry.FindDeepButton("GitHubButton").gameObject.SetActive(true);
            }

            newEntry.FindDeepChildObject("ModName").GetComponent<Text>().text = SettingsManager.DebugMode == false ? modData.ModName : modData.ModID;
            newEntry.FindDeepChildObject("ModAuthor").GetComponent<Text>().text = modData.ModAuthor;

            newEntry.FindDeepButton("AboutButton").onClick.AddListener(delegate { ToggleMoreInfo(modData.ModName, modData.ModAuthor, modData.ModVersion, modData.ModDescription); });

            if(modData.IsBepInExMod)
                newEntry.FindDeepButton("SettingsButton").onClick.AddListener(delegate { ToggleSettings($"BepInEx_CompatLayer_{modData.ModID}-SettingsHolder"); });
            else
                newEntry.FindDeepButton("SettingsButton").onClick.AddListener(delegate { ToggleSettings($"{modData.ModAuthor}_{modData.ModID}_{modData.ModName}-SettingsHolder"); });

            Text text = newEntry.FindDeepButton("ToggleButton").GetComponentInChildren<Text>();

            if (modData.Mod == null)
                return text;

            newEntry.FindDeepButton("MoveUpButton").onClick.AddListener(delegate { ModManager.MoveModOrderUp(modData.Mod, newEntry); });
            newEntry.FindDeepButton("MoveDownButton").onClick.AddListener(delegate { ModManager.MoveModOrderDown(modData.Mod, newEntry); });
            newEntry.FindDeepButton("MoveTopButton").onClick.AddListener(delegate { ModManager.MoveModOrderTop(modData.Mod, newEntry); });
            newEntry.FindDeepButton("MoveBottomButton").onClick.AddListener(delegate { ModManager.MoveModOrderBottom(modData.Mod, newEntry); });

            newEntry.FindDeepButton("ToggleButton").onClick.AddListener(delegate { ModManager.ToggleMod(modData); });

            modEntries.Add(modData, newEntry);

            return text;
        }

        internal GameObject CreateModEntryReturnEntry(GenericModData modData)
        {
            var text = CreateModEntryReturnText(modData);

            return text.transform.parent.parent.parent.gameObject;
        }

        private void AddObjectShortcuts()
        {
            var toolBox = GameObject.Find("ToolBox");
            var wrench = toolBox.transform.Find("Cylinder_254");
            var nut = toolBox.transform.GetChild(toolBox.transform.childCount - 1);
            MainMenuToolboxC orgComp = toolBox.GetComponent<MainMenuToolboxC>();

            wrench.localPosition = new Vector3(24.21f, -3.55f, 8.96f); // The wrench in the original toolbox clips through the casing

            var JLWrench = Instantiate(wrench.gameObject);
            JLWrench.name = "JLWrench";
            Destroy(JLWrench.transform.GetChild(0).gameObject);
            JLWrench.transform.position = new Vector3(-51.88f, 38.9877f, -73.8051f);
            JLWrench.transform.eulerAngles = new Vector3(-68.094f, 111.995f, -94.47f);
            JLWrench.transform.localScale = new Vector3(2, 2, 2);

            JLWrench.layer = 11;
            JLWrench.tag = "Pickup";

            BoxCollider col = JLWrench.AddComponent<BoxCollider>();
            col.isTrigger = true;
            MenuWrench comp = JLWrench.AddComponent<MenuWrench>();
            comp.book = orgComp.book;
            comp.renderTarget = JLWrench;

            Material mat = new Material(Shader.Find("Legacy Shaders/Diffuse"))
            {
                color = new Color32(255, 255, 107, 255)
            };

            Material glowMat = new Material(Shader.Find("Toony Gooch/Toony Gooch RimLight"))
            {
                color = mat.color
            };

            JLWrench.GetComponent<MeshRenderer>().material = mat;

            comp.startMaterial = mat;
            comp.glowMaterial = glowMat;

            var JLNut = Instantiate(nut.gameObject);
            JLNut.name = "JLNut";
            JLNut.transform.position = new Vector3(-47.894f, 39.164f, -70.268f);
            JLNut.transform.eulerAngles = new Vector3(17.309f, 8.457f -9.101f);
            JLNut.transform.localScale = new Vector3(10, 10, 10);

            JLNut.layer = 11;
            JLNut.tag = "Pickup";

            BoxCollider nutCol = JLNut.AddComponent<BoxCollider>();
            nutCol.isTrigger = true;
            MenuNut nutComp = JLNut.AddComponent<MenuNut>();
            nutComp.book = orgComp.book;
            nutComp.renderTarget = JLNut;

            JLNut.GetComponent<MeshRenderer>().material = mat;

            nutComp.startMaterial = mat;
            nutComp.glowMaterial = glowMat;
        }

        private void SetNewspaperText()
        {
            if (Language.CurrentLanguage().Equals(LanguageCode.EN))
            {
                string versionText = GameObject.Find("Newspaper").transform.Find("TextMeshPro").GetComponent<TextMeshPro>().text;
                versionText = Regex.Replace(versionText, @"JALOPY", "");
                versionText = Regex.Replace(versionText, @"\s", "");
                GameObject.Find("Newspaper").transform.Find("TextMeshPro").GetComponent<TextMeshPro>().text = $"JALOPY {versionText}|JALOADER {(SettingsManager.IsPreReleaseVersion ? $"{SettingsManager.GetVersionString().Replace("Pre-Release", "PR")}" : SettingsManager.GetVersionString())}";

                if (int.Parse(versionText.Replace(".", "")) < 1105)
                    StartCoroutine(ShowNoticeAfterLoad("OUTDATED GAME DETECTED", "You are using an outdated version of Jalopy.\r\n\r\nYou may encounter issues with JaLoader and certain mods, as well as more bugs in general.\r\n\r\nIf you encounter bugs, please make sure to ask or check if they exist in newer versions as well before reporting them.\r\n\r\nHave fun!"));
            }
        }
        bool isNutGreen = false;

        public void MakeNutGreen()
        {
            var nut = FindObjectOfType<MenuNut>();

            var greenMat = new Material(Shader.Find("Legacy Shaders/Diffuse"))
            {
                color = new Color32(120, 255, 85, 255)
            };

            nut.startMaterial = greenMat;

            var glowMat = new Material(Shader.Find("Toony Gooch/Toony Gooch RimLight"))
            {
                color = greenMat.color
            };

            nut.glowMaterial = glowMat;

            nut.GetComponent<MeshRenderer>().material = greenMat;

            isNutGreen = true;
        }

        public void MakeWrenchGreen()
        {
            var wrench = FindObjectOfType<MenuWrench>();

            var greenMat = new Material(Shader.Find("Legacy Shaders/Diffuse"))
            {
                color = new Color32(120, 255, 85, 255)
            };

            wrench.startMaterial = greenMat;

            var glowMat = new Material(Shader.Find("Toony Gooch/Toony Gooch RimLight"))
            {
                color = greenMat.color
            };

            wrench.glowMaterial = glowMat;

            wrench.GetComponent<MeshRenderer>().material = greenMat;
        }

        private void SaveModSettings()
        {
            for (int i = 0; i < ModsSettingsContent.transform.childCount; i++)
            {
                if (ModsSettingsContent.transform.GetChild(i).gameObject.activeSelf && Regex.Match(ModsSettingsContent.transform.GetChild(i).gameObject.name, @"(.{15})\s*$").ToString() == "-SettingsHolder")
                {
                    string fullName = ModsSettingsContent.transform.GetChild(i).gameObject.name;

                    string modAuthor = "";
                    string modID = "";
                    string modName = "";

                    string[] parts = fullName.Split('_');

                    modAuthor = parts[0];
                    modID = parts[1];

                    modName = parts[2];
                    modName = modName.Remove(modName.Length - 15);

                    if (modName == string.Empty)
                        modName = modID;

                    var mod = ModManager.FindMod(modAuthor, modID, modName);

                    if(mod != null && mod is Mod)
                    {
                        var modClass = mod as Mod;
                        modClass.SaveModSettings();
                    }
                    else if (mod != null && mod is BaseUnityPlugin)
                    {
                        var modClass = mod as BaseUnityPlugin;
                        modClass.SaveBIXPluginSettings();
                    }
                }
            }
        }

        private void ResetModSettings()
        {
            for (int i = 0; i < ModsSettingsContent.transform.childCount; i++)
            {
                if (ModsSettingsContent.transform.GetChild(i).gameObject.activeSelf && Regex.Match(ModsSettingsContent.transform.GetChild(i).gameObject.name, @"(.{15})\s*$").ToString() == "-SettingsHolder")
                {
                    string fullName = ModsSettingsContent.transform.GetChild(i).gameObject.name;

                    string modAuthor = "";
                    string modID = "";
                    string modName = "";

                    string[] parts = fullName.Split('_');

                    modAuthor = parts[0];
                    modID = parts[1];

                    modName = parts[2];
                    modName = modName.Remove(modName.Length - 15);

                    if (modName == string.Empty)
                        modName = modID;

                    var mod = ModManager.FindMod(modAuthor, modID, modName);

                    if (mod != null && mod is Mod)
                    {
                        var modClass = mod as Mod;
                        modClass.ResetModSettings();
                    }
                    else if (mod != null && mod is BaseUnityPlugin)
                    {
                        var modClass = mod as BaseUnityPlugin;
                        modClass.SaveBIXPluginSettings();
                    }
                }
            }
        }

        public void SetOptionsValues()
        {
            AllSettingsDropdowns["ConsoleMode"].value = (int)SettingsManager.ConsoleMode;
            AllSettingsDropdowns["ConsolePosition"].value = (int)SettingsManager.ConsolePosition;
            AllSettingsDropdowns["ShowModsFolderLocation"].value = SettingsManager.HideModFolderLocation ? 1 : 0;
            AllSettingsDropdowns["EnableJaDownloader"].value = SettingsManager.EnableJaDownloader ? 0 : 1;
            AllSettingsDropdowns["UpdateCheckFrequency"].value = (int)SettingsManager.UpdateCheckMode;
            AllSettingsDropdowns["SkipLanguageSelectionScreen"].value = SettingsManager.SkipLanguage ? 0 : 1;
            AllSettingsDropdowns["DiscordRichPresence"].value = SettingsManager.UseDiscordRichPresence ? 0 : 1;
            AllSettingsDropdowns["DebugMode"].value = SettingsManager.DebugMode ? 0 : 1;
            AllSettingsDropdowns["MenuMusic"].value = SettingsManager.DisableMenuMusic ? 1 : 0;
            AllSettingsSliders["MenuMusicVolume"].value = SettingsManager.MenuMusicVolume;
            AllSettingsDropdowns["Uncle"].value = SettingsManager.DisableUncle ? 1 : 0;
            AllSettingsDropdowns["CustomSongs"].value = SettingsManager.UseCustomSongs ? 0 : 1;
            AllSettingsDropdowns["CustomSongsBehaviour"].value = (int)SettingsManager.CustomSongsBehaviour;
            AllSettingsDropdowns["RadioAds"].value = SettingsManager.RadioAds ? 0 : 1;
            AllSettingsDropdowns["UseEnhancedMovement"].value = SettingsManager.UseExperimentalCharacterController ? 1 : 0;
            AllSettingsDropdowns["ChangeLicensePlate"].value = (int)SettingsManager.ChangeLicensePlateText;
            AllSettingsInputFields["LicensePlateText"].text = SettingsManager.LicensePlateText;
            AllSettingsDropdowns["ShowFPSCounter"].value = SettingsManager.ShowFPSCounter ? 1 : 0;
            AllSettingsDropdowns["FixLaikaShopMusic"].value = SettingsManager.FixLaikaShopMusic ? 0 : 1;
            // AllSettingsDropdowns["Replace0WithBanned"].value = SettingsManager.Replace0WithBanned ? 0 : 1;
            AllSettingsDropdowns["MirrorDistance"].value = (int)SettingsManager.MirrorDistances;
            AllSettingsDropdowns["CursorMode"].value = (int)SettingsManager.CursorMode;
            //AllSettingsDropdowns["FixItemsFallingBehindShop"].value = SettingsManager.FixItemsFallingBehindShop ? 0 : 1;
            AllSettingsDropdowns["FixGuardsFlags"].value = SettingsManager.FixBorderGuardsFlags ? 0 : 1;
            AllSettingsDropdowns["ShowDisabledMods"].value = SettingsManager.ShowDisabledMods ? 0 : 1;

            JLFPSText.SetActive(SettingsManager.ShowFPSCounter);
            JLDebugText.SetActive(SettingsManager.DebugMode);

            ShowDisabledMods();
        }

        private void SwitchLanguage()
        {
            SettingsManager.selectedLanguage = false;

            SaveAndApplyValues();

            ToggleModLoaderSettings_Accessibility();
            ToggleModLoaderSettings_Main();
            Console.Instance.ToggleVisibility(false);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
            isObstructing = false;
        }

        private void OpenModsFolder()
        {
            PlayClickSound();

            Application.OpenURL(SettingsManager.ModFolderLocation);
        }

        private void OpenOutputLog()
        {
            PlayClickSound();

            string path = Path.Combine(Application.persistentDataPath, "output_log.txt");

            Process.Start(path);
        }

        private void PlayClickSound()
        {
            audioSource.PlayOneShot(buttonClickSound);
        }

        private void OpenSavesFolder()
        {
            PlayClickSound();

            Application.OpenURL(Application.persistentDataPath);
        }

        private void RefreshUI()
        {
            if (JLCanvas == null)
                return;

            JLCanvas.SetActive(false);
            JLCanvas.SetActive(true);
        }

        public void ShowTooltip(string text)
        {
            JLCanvas.Find("JLTooltip/Text").GetComponent<Text>().text = text;
            JLCanvas.Find("JLTooltip").gameObject.SetActive(true);
        }

        public void HideTooltip()
        {
            JLCanvas.Find("JLTooltip/Text").GetComponent<Text>().text = "";
            JLCanvas.Find("JLTooltip").gameObject.SetActive(false);
        }

        public void ToggleMoreInfo(string name, string author, string version, string description)
        {
            PlayClickSound();

            ModsSettingsList.SetActive(false);
            inModsOptions = false;

            MoreModInfoPanel.SetActive(true);

            if (MoreModInfoPanel.Find("ModName").GetComponent<Text>().text != name)
            {
                MoreModInfoPanel.Find("ModName").GetComponent<Text>().text = name;
                MoreModInfoPanel.Find("ModAuthor").GetComponent<Text>().text = author;
                MoreModInfoPanel.Find("ModVersion").GetComponent<Text>().text = version;

                if (description != null)
                    MoreModInfoPanel.Find("ModDescription").GetComponent<Text>().text = description;
                else
                    MoreModInfoPanel.Find("ModDescription").GetComponent<Text>().text = "This mod does not have a description.";
            }
            else
            {
                MoreModInfoPanel.Find("ModName").GetComponent<Text>().text = "Welcome to the mods list!";
                MoreModInfoPanel.Find("ModAuthor").GetComponent<Text>().text = "";
                MoreModInfoPanel.Find("ModVersion").GetComponent<Text>().text = "";
                MoreModInfoPanel.Find("ModDescription").GetComponent<Text>().text = "You can enable/disable mods, view more information about them, adjust their settings, and arrange them in a desired load order using the provided directional arrows!";
            }
        }

        public void ToggleSettings(string objName)
        {
            PlayClickSound();

            ModsSettingsList.SetActive(true);
            inModsOptions = true;

            MoreModInfoPanel.SetActive(false);

            if (ModsSettingsContent.Find(objName) && ModsSettingsContent.Find(objName).childCount != 0)
            {
                foreach (Transform item in ModsSettingsContent.transform)
                    item.gameObject.SetActive(false);

                ModsSettingsContent.Find(objName).gameObject.SetActive(true);
            }
            else
            {
                foreach (Transform item in ModsSettingsContent.transform)
                    item.gameObject.SetActive(false);

                ModsSettingsContent.Find("NoSettings").gameObject.SetActive(true);
            }
        }

        public void ToggleModMenu()
        {
            if (!IsBookClosed())
                book.CloseBook();

            ModsSearchBar.Select();
            ModsSearchBar.text = "";
            JLModsPanel.SetActive(!JLModsPanel.gameObject.activeSelf);
            inModsList = !inModsList;

            if (SceneManager.GetActiveScene().buildIndex == 3)
                TogglePauseMenu(JLModsPanel.gameObject.activeSelf);

            ToggleObstructRay();

        }

        public void ToggleModLoaderSettings_Main()
        {
            if (!IsBookClosed())
                book.CloseBook();

            inOptions = !inOptions;
            JLSettingsPanel.SetActive(inOptions);
            JLSettingsPanel.transform.GetChild(1).gameObject.SetActive(!JLSettingsPanel.transform.GetChild(1).gameObject.activeSelf);

            if (SceneManager.GetActiveScene().buildIndex == 3)
                TogglePauseMenu(JLSettingsPanel.gameObject.activeSelf);

            ToggleObstructRay();
        }

        public void ToggleModLoaderSettings_Preferences()
        {
            PlayClickSound();

            JLSettingsPanel.transform.GetChild(1).gameObject.SetActive(!JLSettingsPanel.transform.GetChild(0).gameObject.activeSelf);
            JLSettingsPanel.transform.GetChild(2).gameObject.SetActive(!JLSettingsPanel.transform.GetChild(1).gameObject.activeSelf);
        }

        public void ToggleModLoaderSettings_Tweaks()
        {
            PlayClickSound();

            JLSettingsPanel.transform.GetChild(1).gameObject.SetActive(! JLSettingsPanel.transform.GetChild(0).gameObject.activeSelf);
            JLSettingsPanel.transform.GetChild(3).gameObject.SetActive(!JLSettingsPanel.transform.GetChild(2).gameObject.activeSelf);
        }

        public void ToggleModLoaderSettings_Accessibility()
        {
            PlayClickSound();

            JLSettingsPanel.transform.GetChild(1).gameObject.SetActive(!JLSettingsPanel.transform.GetChild(0).gameObject.activeSelf);
            JLSettingsPanel.transform.GetChild(4).gameObject.SetActive(! JLSettingsPanel.transform.GetChild(3).gameObject.activeSelf);
        }

        private void TogglePauseMenu(bool value)
        {
            var script = MainMenuC.Global;
            var pauseUI = script.pauseUI;
            if (value)
            {
                TweenAlpha.Begin(pauseUI[0], 0.8f, 0f);
                TweenAlpha.Begin(pauseUI[1], 0.8f, 0f);
                TweenAlpha.Begin(pauseUI[16], 0.8f, 0f);
                TweenAlpha.Begin(pauseUI[2], 0.8f, 0f);
                TweenAlpha.Begin(pauseUI[3], 0.8f, 0f);
                TweenAlpha.Begin(pauseUI[8], 0.8f, 0f);
                TweenAlpha.Begin(pauseUI[10], 0.8f, 0f);
                pauseUI[1].GetComponent<Collider>().enabled = false;
                pauseUI[2].GetComponent<Collider>().enabled = false;
                pauseUI[3].GetComponent<Collider>().enabled = false;
                pauseUI[8].GetComponent<Collider>().enabled = false;
                pauseUI[10].GetComponent<Collider>().enabled = false;

                var uiRoot = GameObject.Find("UI Root");

                var modsButton = uiRoot.transform.Find("Mods").gameObject;
                TweenAlpha.Begin(modsButton, 0.8f, 0f);
                modsButton.GetComponent<Collider>().enabled = false;

                var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
                TweenAlpha.Begin(optionsButton, 0.8f, 0f);
                optionsButton.GetComponent<Collider>().enabled = false;
            }
            else
            {
                TweenAlpha.Begin(pauseUI[16], 0.8f, 1f);
                TweenAlpha.Begin(pauseUI[0], 0.8f, 1f);
                TweenAlpha.Begin(pauseUI[1], 0.8f, 1f);
                TweenAlpha.Begin(pauseUI[2], 0.8f, 1f);
                TweenAlpha.Begin(pauseUI[3], 0.8f, 1f);
                TweenAlpha.Begin(pauseUI[8], 0.8f, 1f);
                TweenAlpha.Begin(pauseUI[10], 0.8f, 1f);
                pauseUI[1].GetComponent<Collider>().enabled = true;
                pauseUI[2].GetComponent<Collider>().enabled = true;
                pauseUI[3].GetComponent<Collider>().enabled = true;
                pauseUI[8].GetComponent<Collider>().enabled = true;
                pauseUI[10].GetComponent<Collider>().enabled = true;
                pauseUI[16].GetComponent<Collider>().enabled = true;

                var uiRoot = GameObject.Find("UI Root");

                var modsButton = uiRoot.transform.Find("Mods").gameObject;
                TweenAlpha.Begin(modsButton, 0.8f, 1f);
                modsButton.GetComponent<Collider>().enabled = true;

                var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
                TweenAlpha.Begin(optionsButton, 0.8f, 1f);
                optionsButton.GetComponent<Collider>().enabled = true;
            }
        }

        public void AddWarningToMod(GameObject entry, string warningText, bool blockLoadOrder = false)
        {
            entry.transform.Find("BasicInfo/ModName").GetComponent<Text>().color = CommonColors.ErrorRed;

            var warningIcon = entry.transform.Find("WarningIcon").gameObject;

            warningIcon.SetActive(true);

            var template = warningIcon.transform.GetChild(0).GetChild(0).GetChild(0);

            var warning = Instantiate(template.gameObject, warningIcon.transform.GetChild(0).GetChild(0));
            warning.GetComponent<Text>().text = warningText;
            warning.SetActive(true);

            warningIcon.transform.GetChild(0).GetComponent<VerticalLayoutGroup>().enabled = false;
            warningIcon.transform.GetChild(0).GetComponent<VerticalLayoutGroup>().enabled = true;

            if(!warning.GetComponent<WarningOnHover>())
                warningIcon.AddComponent<WarningOnHover>();

            if (blockLoadOrder)
            {
                entry.FindDeepButton("MoveUpButton").interactable = false;
                entry.FindDeepButton("MoveDownButton").interactable = false;
                entry.FindDeepButton("MoveTopButton").interactable = false;
                entry.FindDeepButton("MoveBottomButton").interactable = false;
            }
        }

        private void InstallMod()
        {
            var modURL = ModsSearchBar.text;

            modURL = modURL.Replace("jaloader://install/", "jaloader://installingame/");

            Process.Start(modURL);

            var author = modURL.Split('/')[3];
            var repo = modURL.Split('/')[4];

            StartCoroutine(CheckIfModInstalled(author, repo));
        }

        private IEnumerator CheckIfModInstalled(string author, string repo)
        {
            var maximumTime = 60;
            var currentTime = 0;

            author = author.Replace("\n", "").Replace("\r", "");
            repo = repo.Replace("\n", "").Replace("\r", "");

            while (!File.Exists(Path.Combine(SettingsManager.ModFolderLocation, $"{author}_{repo}_Installed.txt")))
            {
                if(maximumTime == currentTime)
                {
                    ShowNotice("MOD INSTALLATION FAILED", "The mod installation failed. Please make sure you have the correct URL and that your internet connection is stable.", ignoreObstructRayChange: true);
                    yield break;
                }

                currentTime++;
                yield return new WaitForSeconds(1);
            }

            var dllName = File.ReadAllText(Path.Combine(SettingsManager.ModFolderLocation, $"{author}_{repo}_Installed.txt"));
            File.Delete(Path.Combine(SettingsManager.ModFolderLocation, $"{author}_{repo}_Installed.txt"));

            ReferencesLoader.Instance.StartCoroutine(ReferencesLoader.Instance.LoadAssemblies());

            ModLoader.Instance.InitializeMod(out MonoBehaviour mod, certainModFile: dllName);
            yield return null;
        }

        private void OnInputValueChanged_ModsList()
        {
            if (ModsSearchBar.text.StartsWith("jaloader://install/") && SettingsManager.EnableJaDownloader)
                ModsInstallButton.SetActive(true);
            else
                ModsInstallButton.SetActive(false);

            foreach (Transform child in ModsListContent)
            {
                if (child.name == "ModTemplate") continue;

                if (child.GetChild(2).GetChild(0).GetComponent<Text>().text.ToLower().Contains(ModsSearchBar.text.ToLower()))
                {
                    if (IsModEntryDisabled(child.gameObject) && AllSettingsDropdowns["ShowDisabledMods"].value == 1)
                        child.gameObject.SetActive(false);
                    else
                        child.gameObject.SetActive(true);
                }
                else
                    child.gameObject.SetActive(false);
            }
        }

        private void ShowDisabledMods()
        {
            foreach (Transform child in ModsListContent)
            {
                if (child.name == "ModTemplate") continue;

                if(IsModEntryDisabled(child.gameObject) && AllSettingsDropdowns["ShowDisabledMods"].value == 1)
                    child.gameObject.SetActive(false);
                else
                    child.gameObject.SetActive(true);
            }

            OnInputValueChanged_ModsList();

            SaveAndApplyValues();
        }

        private bool IsModEntryDisabled(GameObject obj)
        {
            var text = obj.transform.Find("Buttons").Find("ToggleButton").Find("Text").GetComponent<Text>().text;

            if (text == "Enable")
                return true;

            return false;
        }

        private void ToggleObstructRay()
        {
            if (SceneManager.GetActiveScene().buildIndex != 1)
                return;

            isObstructing = !isObstructing;
            FindObjectOfType<MenuMouseInteractionsC>().restrictRay = isObstructing;
        }

        private void SetObstructRay(bool value)
        {
            if (SceneManager.GetActiveScene().buildIndex != 1)
                return;

            isObstructing = value;
            FindObjectOfType<MenuMouseInteractionsC>().restrictRay = value;
        }

        private void FixTranslations()
        {
            if (Language.CurrentLanguage() == LanguageCode.FR)
            {
                FieldInfo fieldInfo = typeof(Language).GetField("currentEntrySheets", BindingFlags.Static | BindingFlags.NonPublic);
                if (fieldInfo != null)
                {
                    var fieldValue = fieldInfo.GetValue(null) as Dictionary<string, Dictionary<string, string>>;

                    fieldValue["Inspector_UI"]["ui_obj_repkit_03"] = "Utilisé pour réparer les pièces du moteur de la Laika 601 Deluxe.";
                    fieldValue["Inspector_UI"]["tooltip_03_X"] = "Pour changer l'objet en main utilisez LB ou RB";
                    fieldValue["Inspector_UI"]["tooltip_05_X"] = "Pour Installer une amélioration, utilisez la sur le véhicule en maintenant le bouton A";

                    fieldInfo.SetValue(null, fieldValue);
                }              
            }
        }

        private void SaveAndApplyValues()
        {
            PlayClickSound();

            if (SettingsManager.UseDiscordRichPresence != !Convert.ToBoolean(AllSettingsDropdowns["DiscordRichPresence"].value))
            {
                ToggleModLoaderSettings_Preferences();
                ToggleModLoaderSettings_Main();
                ShowNotice("RESTART REQUIRED", "Changing the Discord Rich Presence setting requires a game restart for the changes to apply.");
            }

            var wasJaDownChanged = false;

            if (SettingsManager.EnableJaDownloader != !Convert.ToBoolean(AllSettingsDropdowns["EnableJaDownloader"].value))
            {
                wasJaDownChanged = true;
            }

            SettingsManager.ConsoleMode = (ConsoleModes)AllSettingsDropdowns["ConsoleMode"].value;
            SettingsManager.ConsolePosition = (ConsolePositions)AllSettingsDropdowns["ConsolePosition"].value;
            SettingsManager.HideModFolderLocation = Convert.ToBoolean(AllSettingsDropdowns["ShowModsFolderLocation"].value);
            SettingsManager.EnableJaDownloader = !Convert.ToBoolean(AllSettingsDropdowns["EnableJaDownloader"].value);
            SettingsManager.UpdateCheckMode = (UpdateCheckModes)AllSettingsDropdowns["UpdateCheckFrequency"].value;
            SettingsManager.SkipLanguage = !Convert.ToBoolean(AllSettingsDropdowns["SkipLanguageSelectionScreen"].value);
            SettingsManager.UseDiscordRichPresence = !Convert.ToBoolean(AllSettingsDropdowns["DiscordRichPresence"].value);
            SettingsManager.DebugMode = !Convert.ToBoolean(AllSettingsDropdowns["DebugMode"].value);
            SettingsManager.DisableMenuMusic = Convert.ToBoolean(AllSettingsDropdowns["MenuMusic"].value);
            SettingsManager.MenuMusicVolume = (int)AllSettingsSliders["MenuMusicVolume"].value;
            SettingsManager.DisableUncle = Convert.ToBoolean(AllSettingsDropdowns["Uncle"].value);
            SettingsManager.UseCustomSongs = !Convert.ToBoolean(AllSettingsDropdowns["CustomSongs"].value);
            SettingsManager.CustomSongsBehaviour = (CustomSongsBehaviour)AllSettingsDropdowns["CustomSongsBehaviour"].value;
            SettingsManager.RadioAds = !Convert.ToBoolean(AllSettingsDropdowns["RadioAds"].value);
            SettingsManager.UseExperimentalCharacterController = Convert.ToBoolean(AllSettingsDropdowns["UseEnhancedMovement"].value);
            SettingsManager.ChangeLicensePlateText = (LicensePlateStyles)AllSettingsDropdowns["ChangeLicensePlate"].value;
            SettingsManager.LicensePlateText = AllSettingsInputFields["LicensePlateText"].text;
            SettingsManager.ShowFPSCounter = Convert.ToBoolean(AllSettingsDropdowns["ShowFPSCounter"].value);
            SettingsManager.FixLaikaShopMusic = !Convert.ToBoolean(AllSettingsDropdowns["FixLaikaShopMusic"].value);
            // SettingsManager.Replace0WithBanned = !Convert.ToBoolean(AllSettingsDropdowns["Replace0WithBanned"].value);
            SettingsManager.MirrorDistances = (MirrorDistances)AllSettingsDropdowns["MirrorDistance"].value;
            SettingsManager.CursorMode = (CursorMode)AllSettingsDropdowns["CursorMode"].value;
            //SettingsManager.FixItemsFallingBehindShop = !Convert.ToBoolean(AllSettingsDropdowns["FixItemsFallingBehindShop"].value);
            SettingsManager.FixBorderGuardsFlags = !Convert.ToBoolean(AllSettingsDropdowns["FixGuardsFlags"].value);
            SettingsManager.ShowDisabledMods = !Convert.ToBoolean(AllSettingsDropdowns["ShowDisabledMods"].value);

            JLFPSText.SetActive(SettingsManager.ShowFPSCounter);
            JLDebugText.gameObject.SetActive(SettingsManager.DebugMode);

            SettingsManager.SaveSettings(false);

            if ((ConsoleModes)AllSettingsDropdowns["ConsoleMode"].value == ConsoleModes.Disabled)
                Console.Instance.ToggleVisibility(false);

            SetConsolePosition((ConsolePositions)AllSettingsDropdowns["ConsolePosition"].value);
            ModsLocationText.SetActive(!SettingsManager.HideModFolderLocation);

            CustomRadioController.Instance.UpdateMenuMusic(!Convert.ToBoolean(AllSettingsDropdowns["MenuMusic"].value), (float)AllSettingsSliders["MenuMusicVolume"].value / 100);

            UncleHelper.Instance.UncleEnabled = !SettingsManager.DisableUncle;

            if (wasJaDownChanged)
            {
                if (SettingsManager.EnableJaDownloader)
                {
                    var path = Path.GetFullPath(Path.Combine(Path.Combine(Application.dataPath, "."), "JaDownloader.exe"));
                    Process.Start($@"{Application.dataPath}\.\JaDownloaderSetup.exe", $"\"{path}\"");
                }
                else
                {
                    Process.Start($@"{Application.dataPath}\.\JaDownloaderSetup.exe", "Uninstall");
                }
            }
        }

        private List<(string, string, bool)> noticesToShow = new List<(string, string, bool)>();
        private bool showingNotice;

        private IEnumerator ShowNoticeAfterLoad(string subtitle, string message)
        {
            while (JLNoticePanel == null)
                yield return null;

            ShowNotice(subtitle, message);
        }

        private void ShowJaDownloaderNotice()
        {
            SetObstructRay(true);
            var dialog = Instantiate(JLCanvas.transform.Find("JLUpdateDialog").gameObject, JLCanvas.transform.Find("JLUpdateDialog").parent);
            dialog.name = "JLDownloaderNotice";
            dialog.SetActive(false);
            dialog.FindObject("Subtitle").SetActive(false);
            dialog.FindButton("OpenGitHubButton").gameObject.SetActive(false);
            dialog.Find("Title").GetComponent<Text>().text = "Enable JaDownloader";
            dialog.Find("Message").GetComponent<Text>().text = "JaDownloader is a tool that allows you to install most mods automatically. \r\n Would you like to enable it now? (you can find this setting in Modloader Settings/Preferences)";
            dialog.FindButton("YesButton").onClick.AddListener(delegate {
                var path = Path.GetFullPath(Path.Combine(Path.Combine(Application.dataPath, "."), "JaDownloader.exe"));
                Process.Start($@"{Application.dataPath}\.\JaDownloaderSetup.exe", $"\"{path}\""); SettingsManager.EnableJaDownloader = true; SetObstructRay(false); Destroy(dialog);
            });
            dialog.FindButton("NoButton").onClick.AddListener(delegate {SetObstructRay(false); Destroy(dialog); });
            dialog.SetActive(true);

            SettingsManager.AskedAboutJaDownloader = true;
            SettingsManager.SaveSettings(false);
        }

        public void ShowNotice(string subtitle, string message, bool enableDontShowAgain = true, bool ignoreObstructRayChange = false)
        {
            allNotices.Add(message);
            noticesToShow.Add((subtitle, message, ignoreObstructRayChange));

            if (!SettingsManager.DontShowAgainNotices.Contains(message))
            {
                JLNoticePanel.SetActive(true);

                SetObstructRay(true);

                if (!showingNotice)
                {
                    JLNoticePanel.Find("Subtitle").GetComponent<Text>().text = subtitle;
                    JLNoticePanel.Find("Message").GetComponent<Text>().text = message;
                }

                showingNotice = true;

                if (!enableDontShowAgain)
                    JLNoticePanel.FindObject("DontShowAgainButton").SetActive(false);
                else
                    JLNoticePanel.FindObject("DontShowAgainButton").SetActive(true);
            }
        }

        private void CloseNotice(bool dontShowAgain = false)
        {
            PlayClickSound();

            if (SettingsManager.DontShowAgainNotices.Contains(noticesToShow[0].Item2) && noticesToShow.Count > 1)
            {
                var _ignoreIfLastOne = noticesToShow[0].Item3;
                noticesToShow.RemoveAt(0);

                for (int i = 0; i < noticesToShow.Count; i++)
                {
                    if(SettingsManager.DontShowAgainNotices.Contains(noticesToShow[i].Item2))
                    {
                        if(noticesToShow.Count == 1)
                            _ignoreIfLastOne = noticesToShow[0].Item3;

                        noticesToShow.RemoveAt(i);
                        i--;
                    }
                }

                if (noticesToShow.Count == 0)
                {
                    if (!_ignoreIfLastOne)
                        SetObstructRay(false);

                    showingNotice = false;
                    JLNoticePanel.SetActive(false);

                    RemoveExcessDontShowAgainNotices();
                }
            }

            var ignoreIfLastOne = noticesToShow[0].Item3;
            noticesToShow.RemoveAt(0);

            if (dontShowAgain && !SettingsManager.DontShowAgainNotices.Contains(JLNoticePanel.transform.Find("Message").GetComponent<Text>().text))
            {
                SettingsManager.DontShowAgainNotices.Add(JLNoticePanel.transform.Find("Message").GetComponent<Text>().text);
                SettingsManager.SaveSettings(false);
            }

            if (noticesToShow.Count == 0) 
            {
                if(!ignoreIfLastOne)
                    SetObstructRay(false);

                showingNotice = false;
                JLNoticePanel.SetActive(false);

                RemoveExcessDontShowAgainNotices();
            }
            else
            {
                JLNoticePanel.transform.Find("Subtitle").GetComponent<Text>().text = noticesToShow[0].Item1;
                JLNoticePanel.transform.Find("Message").GetComponent<Text>().text = noticesToShow[0].Item2;
            }
        }

        private void RemoveExcessDontShowAgainNotices()
        {
            if (allNotices.Count <= SettingsManager.DontShowAgainNotices.Count)
            {
                var toRemove = SettingsManager.DontShowAgainNotices.Except(allNotices).ToList();

                foreach (var item in toRemove)
                    SettingsManager.DontShowAgainNotices.Remove(item);

                SettingsManager.SaveSettings(false);
            }
        }
    }

    public class WarningOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public class TooltipOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Slider slider;
        private bool visible;
        private Text text;

        private void Awake()
        {
            text = gameObject.transform.Find("Tooltip/InfoTemplate").GetComponent<Text>();
        }

        private void Update()
        {
            if (visible)
                text.text = slider.value.ToString();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            visible = true;
            gameObject.transform.Find("Tooltip").gameObject.SetActive(visible);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            visible = false;
            gameObject.transform.Find("Tooltip").gameObject.SetActive(visible);
        }
    }

    public class MenuNut : MonoBehaviour
    {
        public bool isGlowing;

        public Material startMaterial;

        public Material glowMaterial;

        public GameObject renderTarget;

        public GameObject book;

        public void Action()
        {
            book.SendMessage("CloseBookNoParticle");
            UIManager.Instance.ToggleModMenu();
        }

        private void Update()
        {
            if (isGlowing)
            {
                float value = Mathf.PingPong(Time.time, 0.75f) + 1.25f;
                renderTarget.GetComponent<Renderer>().material.SetFloat("_RimPower", value);
            }
        }

        public void RaycastEnter()
        {
            isGlowing = true;
            renderTarget.GetComponent<Renderer>().material = glowMaterial;
        }

        public void RaycastExit()
        {
            isGlowing = false;
            renderTarget.GetComponent<Renderer>().material = startMaterial;
        }
    }

    public class MenuWrench : MonoBehaviour
    {
        public bool isGlowing;

        public Material startMaterial;

        public Material glowMaterial;

        public GameObject renderTarget;

        public GameObject book;

        public void Action()
        {
            book.SendMessage("CloseBookNoParticle");
            UIManager.Instance.ToggleModLoaderSettings_Main();
        }

        private void Update()
        {
            if (isGlowing)
            {
                float value = Mathf.PingPong(Time.time, 0.75f) + 1.25f;
                renderTarget.GetComponent<Renderer>().material.SetFloat("_RimPower", value);
            }
        }

        public void RaycastEnter()
        {
            isGlowing = true;
            renderTarget.GetComponent<Renderer>().material = glowMaterial;
        }

        public void RaycastExit()
        {
            isGlowing = false;
            renderTarget.GetComponent<Renderer>().material = startMaterial;
        }
    }
}
