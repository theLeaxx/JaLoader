using BepInEx;
using JaLoader.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CursorMode = JaLoader.Common.CursorMode;
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
            EventsManager.Instance.OnSettingsSaved += SetOptionsValues;

            values = (int[])Enum.GetValues(typeof(KeyCode));
            keys = new bool[values.Length];

            gameObject.AddComponent<AudioSource>();
            audioSource = gameObject.GetComponent<AudioSource>();
        }

        #endregion

        #region Declarations  

        internal Texture2D knobTexture;
        private AudioClip buttonClickSound;
        private AudioSource audioSource;
        private Material GreenToolMaterial;
        private Material GlowGreenToolMaterial;
        private MainMenuBookC book;

        private bool isObstructing;
        private bool isNutGreen;
        internal bool CanCloseMap = true;
        private bool showingNotice;

        public GameObject JLCanvas { get; private set; }
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
        internal ScrollRect ConsoleScrollRect;
        internal Transform ConsoleList;

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
        internal Dictionary<GenericModData, GameObject> modEntries = new Dictionary<GenericModData, GameObject>();
        private readonly List<(string, string, bool)> noticesToShow = new List<(string, string, bool)>();
        private readonly List<string> allNotices = new List<string>();

        public static Font DefaultJaLoaderFont { get; private set; }

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
            }
        }

        private void OnMenuLoad()
        {
            if (!JaLoaderSettings.LoadedFirstTime)
            {
                var loadingScreenScript = gameObject.AddComponent<LoadingScreen>();
                loadingScreenScript.ShowLoadingScreen();

                GreenToolMaterial = new Material(Shader.Find("Legacy Shaders/Diffuse"))
                {
                    color = CommonColors.MenuGreenToolColor.ToColor()
                };

                GlowGreenToolMaterial = new Material(Shader.Find("Toony Gooch/Toony Gooch RimLight"))
                {
                    color = CommonColors.MenuGreenToolColor.ToColor()
                };
            }

            book = FindObjectOfType<MainMenuBookC>();
            
            if (JLCanvas == null)
                StartCoroutine(LoadUIDelay());
            else
                AddButtonsToBookLogic();

            SetNewspaperText();

            ToggleJaLoaderInfoStatus(true);

            AddObjectShortcuts();

            FixDropdowns();
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
        }

        private void CloseAllSettings()
        {
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

            JLPanel.SetActive(show);
        }

        private void SetReferences()
        {
            JLPanel = JLCanvas.FindObject("JLPanel");
            JLModsPanel = JLCanvas.FindDeepChildObject("JLModsPanel");
            JLSettingsPanel = JLCanvas.FindDeepChildObject("JLSettingsPanel");
            JLConsole = JLCanvas.FindDeepChildObject("Console");
            ConsoleList = JLConsole.GetChild(1).GetChild(0).GetChild(0).transform;
            ConsoleScrollRect = JLConsole.GetChild(1).GetComponent<ScrollRect>();
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
            MoreModInfoPanel = JLModsPanel.FindObject("MoreModInfo");

            ConsoleMessageTemplate = JLConsole.FindDeepChildObject("MessageTemplate");
            
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

            ModsSettingsList.FindButton("SaveButton").onClick.AddListener(ModManager.SaveModSettings);
            ModsSettingsList.FindButton("ResetButton").onClick.AddListener(ModManager.ResetModSettings);

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

            DefaultJaLoaderFont = JLCanvas.FindObject("JLUpdateDialog").Find("Subtitle").GetComponent<Text>().font;
        }

        private void FixDropdowns()
        {
            GameObject.Find("Canvas").Find("Image").GetComponent<Image>().raycastTarget = false;
        }

        private void AddListenerEvents()
        {
            JLBookUI.FindButton("ModsButton").onClick.AddListener(ToggleModMenu);
            JLBookUI.FindButton("OptionsButton").onClick.AddListener(ToggleModLoaderSettings_Main);

            JLModsPanel.FindButton("ExitButton").onClick.AddListener(ToggleModMenu);
            ModsSearchBar.onValueChanged.AddListener(delegate { OnInputValueChanged_ModsList(); });
            JLModsPanel.FindDeepButton("InstallButton").onClick.AddListener(InstallMod);

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

            AllSettingsDropdowns["ShowDisabledMods"].onValueChanged.AddListener(delegate { ShowDisabledMods(); });
            AllSettingsDropdowns["MirrorDistance"].onValueChanged.AddListener(delegate { GameTweaks.Instance.UpdateMirrors((MirrorDistances)AllSettingsDropdowns["MirrorDistance"].value); });
            AllSettingsDropdowns["CursorMode"].onValueChanged.AddListener(delegate { GameTweaks.Instance.ChangeCursor((CursorMode)AllSettingsDropdowns["CursorMode"].value); });

            JLNoticePanel.FindButton("UnderstandButton").onClick.AddListener(delegate { CloseNotice(); });
            JLNoticePanel.FindButton("DontShowAgainButton").onClick.AddListener(delegate { CloseNotice(true); });
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
            AllSettingsDropdowns.Add("MultipleTypesInBoxes", TweaksSettings.FindDeepDropdown("MultipleTypesInBoxes"));
            AllSettingsDropdowns.Add("RemoveSmugglingPunishments", TweaksSettings.FindDeepDropdown("RemoveSmugglingPunishments"));

            AllSettingsSliders.Add("MenuMusicVolume", TweaksSettings.FindDeepSlider("MenuMusicVolume"));
            AllSettingsInputFields.Add("LicensePlateText", TweaksSettings.FindDeepInputField("InputField"));

            AccessibilitySettings.FindDeepButton("SwitchLanguage").onClick.AddListener(delegate {
                SaveAndApplyValues();

                ToggleModLoaderSettings_Accessibility();
                ToggleModLoaderSettings_Main();
                Console.Instance.ToggleVisibility(false);

                GameUtils.SwitchLanguage();
                isObstructing = false;
            });
            AccessibilitySettings.FindDeepButton("OpenModsFolder").onClick.AddListener(delegate
            {
                PlayClickSound();
                GameUtils.OpenModsFolder();
            });
            AccessibilitySettings.FindDeepButton("OpenSavesFolder").onClick.AddListener(delegate
            {
                PlayClickSound();
                GameUtils.OpenSavesFolder();
            });
            AccessibilitySettings.FindDeepButton("OpenOutputLog").onClick.AddListener(delegate
            {
                PlayClickSound();
                GameUtils.OpenOutputLog();
            });
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
                    modsFolderText.alignment = TextAnchor.MiddleRight;
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
                    modsFolderText.alignment = TextAnchor.MiddleLeft;
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
                    modsFolderText.alignment = TextAnchor.MiddleLeft;
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
                    modsFolderText.alignment = TextAnchor.MiddleLeft;
                    break;
            }
        }

        private void SetVersionAndLocationText()
        {
            if (CheckAndCreateUpdateDialogIfNeeded(out string latestVersion) == true)
                JaLoaderText.text = $"JaLoader <color={(JaLoaderSettings.IsPreReleaseVersion ? "red" : "yellow")}>{JaLoaderSettings.GetVersionString()}</color> loaded! (<color=lime>{latestVersion} available!</color>)";
            else
                JaLoaderText.text = $"JaLoader <color={(JaLoaderSettings.IsPreReleaseVersion ? "red" : "yellow")}>{JaLoaderSettings.GetVersionString()}</color> loaded!";

            ModsLocationText.GetComponent<Text>().text = $"Mods folder: <color=yellow>{JaLoaderSettings.ModFolderLocation}</color>";
        }

        internal bool CheckAndCreateUpdateDialogIfNeeded(out string _latestVersion, bool force = false)
        {
            if (UpdateUtils.JaLoaderUpdateAvailable(out string latestVersion, force))
            {
                _latestVersion = latestVersion;
                SetObstructRay(true);

                var dialog = JLCanvas.FindObject("JLUpdateDialog");
                dialog.FindButton("YesButton").onClick.AddListener(() => UpdateUtils.StartJaLoaderUpdate());
                dialog.Find("Subtitle").GetComponent<Text>().text = $"{JaLoaderSettings.GetVersionString()} ➔ {latestVersion}";
                dialog.FindButton("NoButton").onClick.AddListener(delegate { dialog.SetActive(false); SetObstructRay(false); });
                dialog.FindButton("OpenGitHubButton").onClick.AddListener(() => Application.OpenURL($"{JaLoaderSettings.JaLoaderGitHubLink}/releases/latest"));
                dialog.SetActive(true);

                MakeWrenchGreen();

                return true;
            }

            _latestVersion = null;

            return false;
        }

        private IEnumerator LoadUIDelay()
        {
            DebugUtils.SignalStartUI();

            yield return new WaitForEndOfFrame();

            var bundleLoadReq = AssetBundle.LoadFromFileAsync(Path.Combine(JaLoaderSettings.ModFolderLocation, @"Required\JaLoader_UI.unity3d"));

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

                if (JaLoaderSettings.HideModFolderLocation)
                    ModsLocationText.SetActive(false);

                SetOptionsValues();
                
                SetConsolePosition(JaLoaderSettings.ConsolePosition);

                Console.LogMessage("JaLoader", $"JaLoader {JaLoaderSettings.GetVersionString()} loaded successfully!");
                DebugUtils.SignalFinishedUI();
            }
            catch (Exception e)
            {
                Console.LogMessage("JaLoader", $"JaLoader {JaLoaderSettings.GetVersionString()} failed to load!");
                DebugUtils.StopCounting();
                Console.LogError("JaLoader", $"Failed to load JaLoader UI!");

                Console.LogError("JaLoader", $"Exception: {e}");

                FindObjectOfType<LoadingScreen>().DeleteLoadingScreen();

                ShowNotice("JaLoader failed to load!", "JaLoader failed to fully load the UI. This is likely due to an outdated JaLoader_UI.unity3d file. Please try reinstalling JaLoader with JaPatcher.");

                if (!JaLoaderSettings.UpdateAvailable)
                    JaLoaderText.text = $"JaLoader <color=red>{JaLoaderSettings.GetVersionString()}</color> failed to load!";
                else
                    JaLoaderText.text = $"JaLoader <color=red>{JaLoaderSettings.GetVersionString()}</color> failed to load! (<color=lime>Update available!</color>)";
                throw;
            }
            finally
            {
                if (GameUtils.IsCrackedGame)
                    ShowNotice("PIRATED GAME DETECTED", "You are using a pirated version of Jalopy.\r\n\r\nYou may encounter issues with certain mods, as well as more bugs in general.\r\n\r\nIf you encounter any game-breaking bugs, feel free to submit them to the official GitHub for JaLoader. Remember to mark them with the \"pirated\" tag!\r\n\r\nHave fun!");

                if (JaLoaderSettings.IsPreReleaseVersion)
                    ShowNotice("USING A PRE-RELEASE VERSION OF JALOADER", "You are using a pre-release version of JaLoader.\r\n\r\nThese versions are prone to bugs and may cause issues with certain mods.\r\n\r\nPlease report any bugs you encounter to the JaLoader GitHub page, marking them with the \"pre-release\" tag.\r\n\r\nHave fun!");

                if (!JaLoaderSettings.AskedAboutJaDownloader && !JaLoaderSettings.EnableJaDownloader)
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
                    if (ModManager.GetDisabledModsCount() > 0)
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

            modEntries[data].FindDeepChildObject("ModName").GetComponent<Text>().text = $"<color=green>(Update Available!)</color> {(JaLoaderSettings.DebugMode == false ? data.ModName : data.ModID)}";

            if(!isNutGreen)
                MakeNutGreen();
        }

        internal void CreateModListEntry(SerializableModListEntry entry)
        {
            var newEntry = Instantiate(ModEntryTemplate);
            newEntry.transform.SetParent(ModsListContent, false);
            newEntry.SetActive(true);

            newEntry.FindDeepChildObject("ModName").GetComponent<Text>().color = CommonColors.DisabledModColor.ToColor();
            newEntry.FindDeepChildObject("ModAuthor").GetComponent<Text>().color = CommonColors.DisabledModColor.ToColor();

            newEntry.FindDeepChildObject("ModName").GetComponent<Text>().text = $"Imported Mod List: {entry.Name}";
            newEntry.FindDeepChildObject("ModAuthor").GetComponent<Text>().text = entry.Author;

            newEntry.FindDeepButton("AboutButton").interactable = false;
            newEntry.FindDeepButton("SettingsButton").interactable = false;
            newEntry.FindDeepButton("ToggleButton").interactable = false;
            newEntry.FindDeepButton("MoveUpButton").interactable = false;
            newEntry.FindDeepButton("MoveDownButton").interactable = false;
            newEntry.FindDeepButton("MoveTopButton").interactable = false;
            newEntry.FindDeepButton("MoveBottomButton").interactable = false;

            newEntry.FindDeepButton("GitHubButton").GetComponentInChildren<Text>().text = "Install";
            if (JaLoaderSettings.EnableJaDownloader)
            {
                if (entry.GitHubLink != null && entry.GitHubLink != string.Empty)
                {
                    newEntry.FindDeepButton("GitHubButton").interactable = true;
                    newEntry.FindDeepButton("GitHubButton").onClick.AddListener(delegate { InstallMod(entry.GitHubLink, entry.Name); Destroy(newEntry); });
                }
                else
                {
                    AddWarningToMod(newEntry, "This mod does not have a GitHub or NexusMods link! Mod cannot be installed in-game", true);
                }
            }
            else
                AddWarningToMod(newEntry, "You do not have JaDownloader enabled! Mod cannot be installed in-game", true);
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
                newEntry.FindDeepButton("GitHubButton").interactable = true;
                var script = newEntry.FindDeepButton("GitHubButton").gameObject.AddComponent<OnRightClickUIElement>();
                script.onRightClick.AddListener(() => GUIUtility.systemCopyBuffer = modData.GitHubLink);
            }

            newEntry.FindDeepChildObject("ModName").GetComponent<Text>().text = JaLoaderSettings.DebugMode == false ? modData.ModName : modData.ModID;
            newEntry.FindDeepChildObject("ModAuthor").GetComponent<Text>().text = modData.ModAuthor;

            newEntry.FindDeepButton("AboutButton").onClick.AddListener(delegate { ToggleMoreInfo(modData.ModName, modData.ModAuthor, modData.ModVersion, modData.ModDescription); });

            if(modData.IsBepInExMod)
                newEntry.FindDeepButton("SettingsButton").onClick.AddListener(delegate { ToggleSettings($"BepInEx_CompatLayer_{modData.ModID}-SettingsHolder"); });
            else
                newEntry.FindDeepButton("SettingsButton").onClick.AddListener(delegate { ToggleSettings($"{modData.ModAuthor}_{modData.ModID}_{modData.ModName}-SettingsHolder"); });

            Text text = newEntry.FindDeepButton("ToggleButton").GetComponentInChildren<Text>();

            if (modData.Mod == null)
                return text;

            newEntry.FindDeepButton("MoveUpButton").onClick.AddListener(delegate { ModManager.MoveModOrderUp(newEntry); });
            newEntry.FindDeepButton("MoveDownButton").onClick.AddListener(delegate { ModManager.MoveModOrderDown(newEntry); });
            newEntry.FindDeepButton("MoveTopButton").onClick.AddListener(delegate { ModManager.MoveModOrderTop(newEntry); });
            newEntry.FindDeepButton("MoveBottomButton").onClick.AddListener(delegate { ModManager.MoveModOrderBottom(newEntry); });

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

            wrench.localPosition = new Vector3(24.21f, -3.55f, 8.96f); // The wrench in the original toolbox clips through the casing, so why not fix it here

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
                GameObject.Find("Newspaper").transform.Find("TextMeshPro").GetComponent<TextMeshPro>().text = $"JALOPY {versionText}|JALOADER {(JaLoaderSettings.IsPreReleaseVersion ? $"{JaLoaderSettings.GetVersionString().Replace("Pre-Release", "PR")}" : JaLoaderSettings.GetVersionString())}";

                if (int.Parse(versionText.Replace(".", "")) < 1105)
                    StartCoroutine(ShowNoticeAfterLoad("OUTDATED GAME DETECTED", "You are using an outdated version of Jalopy.\r\n\r\nYou may encounter issues with JaLoader and certain mods, as well as more bugs in general.\r\n\r\nIf you encounter bugs, please make sure to ask or check if they exist in newer versions as well before reporting them.\r\n\r\nHave fun!"));
            }
        }

        public void MakeNutGreen()
        {
            var nut = FindObjectOfType<MenuNut>();

            nut.startMaterial = GreenToolMaterial;
            nut.glowMaterial = GlowGreenToolMaterial;
            nut.GetComponent<MeshRenderer>().material = GreenToolMaterial;

            isNutGreen = true;
        }

        public void MakeWrenchGreen()
        {
            var wrench = FindObjectOfType<MenuWrench>();

            wrench.startMaterial = GreenToolMaterial;

            wrench.glowMaterial = GlowGreenToolMaterial;

            wrench.GetComponent<MeshRenderer>().material = GreenToolMaterial;
        }

        public void SetOptionsValues()
        {
            SetOptionsValues(false);
        }
        
        public void SetOptionsValues(bool showDisabledMods = false)
        {
            AllSettingsDropdowns["ConsoleMode"].value = (int)JaLoaderSettings.ConsoleMode;
            AllSettingsDropdowns["ConsolePosition"].value = (int)JaLoaderSettings.ConsolePosition;
            AllSettingsDropdowns["ShowModsFolderLocation"].value = JaLoaderSettings.HideModFolderLocation ? 1 : 0;
            AllSettingsDropdowns["EnableJaDownloader"].value = JaLoaderSettings.EnableJaDownloader ? 0 : 1;
            AllSettingsDropdowns["UpdateCheckFrequency"].value = (int)JaLoaderSettings.UpdateCheckMode;
            AllSettingsDropdowns["SkipLanguageSelectionScreen"].value = JaLoaderSettings.SkipLanguage ? 0 : 1;
            AllSettingsDropdowns["DiscordRichPresence"].value = JaLoaderSettings.UseDiscordRichPresence ? 0 : 1;
            AllSettingsDropdowns["DebugMode"].value = JaLoaderSettings.DebugMode ? 0 : 1;
            AllSettingsDropdowns["MenuMusic"].value = JaLoaderSettings.DisableMenuMusic ? 1 : 0;
            AllSettingsSliders["MenuMusicVolume"].value = JaLoaderSettings.MenuMusicVolume;
            AllSettingsDropdowns["Uncle"].value = JaLoaderSettings.DisableUncle ? 1 : 0;
            AllSettingsDropdowns["CustomSongs"].value = JaLoaderSettings.UseCustomSongs ? 0 : 1;
            AllSettingsDropdowns["CustomSongsBehaviour"].value = (int)JaLoaderSettings.CustomSongsBehaviour;
            AllSettingsDropdowns["RadioAds"].value = JaLoaderSettings.RadioAds ? 0 : 1;
            AllSettingsDropdowns["UseEnhancedMovement"].value = JaLoaderSettings.UseExperimentalCharacterController ? 1 : 0;
            AllSettingsDropdowns["ChangeLicensePlate"].value = (int)JaLoaderSettings.ChangeLicensePlateText;
            AllSettingsInputFields["LicensePlateText"].text = JaLoaderSettings.LicensePlateText;
            AllSettingsDropdowns["ShowFPSCounter"].value = JaLoaderSettings.ShowFPSCounter ? 1 : 0;
            AllSettingsDropdowns["FixLaikaShopMusic"].value = JaLoaderSettings.FixLaikaShopMusic ? 0 : 1;
            // AllSettingsDropdowns["Replace0WithBanned"].value = JaLoaderSettings.Replace0WithBanned ? 0 : 1;
            AllSettingsDropdowns["MirrorDistance"].value = (int)JaLoaderSettings.MirrorDistances;
            AllSettingsDropdowns["CursorMode"].value = (int)JaLoaderSettings.CursorMode;
            //AllSettingsDropdowns["FixItemsFallingBehindShop"].value = JaLoaderSettings.FixItemsFallingBehindShop ? 0 : 1;
            AllSettingsDropdowns["FixGuardsFlags"].value = JaLoaderSettings.FixBorderGuardsFlags ? 0 : 1;
            AllSettingsDropdowns["ShowDisabledMods"].value = JaLoaderSettings.ShowDisabledMods ? 0 : 1;
            AllSettingsDropdowns["MultipleTypesInBoxes"].value = JaLoaderSettings.MultipleTypesInBoxes ? 0 : 1;
            AllSettingsDropdowns["RemoveSmugglingPunishments"].value = JaLoaderSettings.RemoveSmugglingPunishments ? 0 : 1;

            JLFPSText.SetActive(JaLoaderSettings.ShowFPSCounter);
            JLDebugText.SetActive(JaLoaderSettings.DebugMode);

            if(showDisabledMods)
                ShowDisabledMods();
        }

        private void PlayClickSound()
        {
            audioSource.PlayOneShot(buttonClickSound);
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

            if (SceneManager.GetActiveScene().buildIndex == 3)
                TogglePauseMenu(JLModsPanel.gameObject.activeSelf);

            ToggleObstructRay();

        }

        public void ToggleModLoaderSettings_Main()
        {
            if (!IsBookClosed())
                book.CloseBook();

            JLSettingsPanel.SetActive(!JLSettingsPanel.activeSelf);
            MainSettings.SetActive(!MainSettings.activeSelf);

            if (SceneManager.GetActiveScene().buildIndex == 3)
                TogglePauseMenu(JLSettingsPanel.gameObject.activeSelf);

            ToggleObstructRay();
        }

        public void ToggleModLoaderSettings_Preferences()
        {
            PlayClickSound();

            MainSettings.SetActive(!MainSettings.activeSelf);
            PreferencesSettings.SetActive(!PreferencesSettings.activeSelf);
        }

        public void ToggleModLoaderSettings_Tweaks()
        {
            PlayClickSound();

            MainSettings.SetActive(!MainSettings.activeSelf);
            TweaksSettings.SetActive(!TweaksSettings.activeSelf);
        }

        public void ToggleModLoaderSettings_Accessibility()
        {
            PlayClickSound();

            MainSettings.SetActive(!MainSettings.activeSelf);
            AccessibilitySettings.SetActive(!AccessibilitySettings.activeSelf);
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
            entry.transform.Find("BasicInfo/ModName").GetComponent<Text>().color = CommonColors.ErrorRed.ToColor();

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
            InstallMod("");
        }

        private void InstallMod(string setModURL = "", string modName = "")
        {
            var modURL = ModsSearchBar.text;
            modURL = modURL.Replace("jaloader://install/", "jaloader://installingame/");

            if (setModURL != "")
                modURL = $"jaloader://installingame/{setModURL}";

            Process.Start(modURL);

            var author = modURL.Split('/')[3];
            var repo = modURL.Split('/')[4];

            StartCoroutine(ModLoader.Instance.CheckIfModInstalled(author, repo, modName));
        }

        private void OnInputValueChanged_ModsList()
        {
            if (ModsSearchBar.text.StartsWith("jaloader://install/") && JaLoaderSettings.EnableJaDownloader)
                ModsInstallButton.SetActive(true);
            else
                ModsInstallButton.SetActive(false);

            foreach (Transform child in ModsListContent)
            {
                if (child.name == "ModTemplate") continue;

                string textToCompare, searchBarTextTrimmed = ModsSearchBar.text.TrimStart();

                if (ModsSearchBar.text.StartsWith("a:"))
                {
                    textToCompare = child.GetChild(2).GetChild(1).GetComponent<Text>().text;
                    searchBarTextTrimmed = searchBarTextTrimmed.Substring(2).TrimStart();
                }
                else
                    textToCompare = child.GetChild(2).GetChild(0).GetComponent<Text>().text;

                if (textToCompare.ToLower().Contains(searchBarTextTrimmed.ToLower()))
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

            if (JaLoaderSettings.UseDiscordRichPresence != !Convert.ToBoolean(AllSettingsDropdowns["DiscordRichPresence"].value))
            {
                ToggleModLoaderSettings_Preferences();
                ToggleModLoaderSettings_Main();
                ShowNotice("RESTART REQUIRED", "Changing the Discord Rich Presence setting requires a game restart for the changes to apply.");
            }

            var wasJaDownChanged = false;

            if (JaLoaderSettings.EnableJaDownloader != !Convert.ToBoolean(AllSettingsDropdowns["EnableJaDownloader"].value))
                wasJaDownChanged = true;

            JaLoaderSettings.ConsoleMode = (ConsoleModes)AllSettingsDropdowns["ConsoleMode"].value;
            JaLoaderSettings.ConsolePosition = (ConsolePositions)AllSettingsDropdowns["ConsolePosition"].value;
            JaLoaderSettings.HideModFolderLocation = Convert.ToBoolean(AllSettingsDropdowns["ShowModsFolderLocation"].value);
            JaLoaderSettings.EnableJaDownloader = !Convert.ToBoolean(AllSettingsDropdowns["EnableJaDownloader"].value);
            JaLoaderSettings.UpdateCheckMode = (UpdateCheckModes)AllSettingsDropdowns["UpdateCheckFrequency"].value;
            JaLoaderSettings.SkipLanguage = !Convert.ToBoolean(AllSettingsDropdowns["SkipLanguageSelectionScreen"].value);
            JaLoaderSettings.UseDiscordRichPresence = !Convert.ToBoolean(AllSettingsDropdowns["DiscordRichPresence"].value);
            JaLoaderSettings.DebugMode = !Convert.ToBoolean(AllSettingsDropdowns["DebugMode"].value);
            JaLoaderSettings.DisableMenuMusic = Convert.ToBoolean(AllSettingsDropdowns["MenuMusic"].value);
            JaLoaderSettings.MenuMusicVolume = (int)AllSettingsSliders["MenuMusicVolume"].value;
            JaLoaderSettings.DisableUncle = Convert.ToBoolean(AllSettingsDropdowns["Uncle"].value);
            JaLoaderSettings.UseCustomSongs = !Convert.ToBoolean(AllSettingsDropdowns["CustomSongs"].value);
            JaLoaderSettings.CustomSongsBehaviour = (CustomSongsBehaviour)AllSettingsDropdowns["CustomSongsBehaviour"].value;
            JaLoaderSettings.RadioAds = !Convert.ToBoolean(AllSettingsDropdowns["RadioAds"].value);
            JaLoaderSettings.UseExperimentalCharacterController = Convert.ToBoolean(AllSettingsDropdowns["UseEnhancedMovement"].value);
            JaLoaderSettings.ChangeLicensePlateText = (LicensePlateStyles)AllSettingsDropdowns["ChangeLicensePlate"].value;
            JaLoaderSettings.LicensePlateText = AllSettingsInputFields["LicensePlateText"].text;
            JaLoaderSettings.ShowFPSCounter = Convert.ToBoolean(AllSettingsDropdowns["ShowFPSCounter"].value);
            JaLoaderSettings.FixLaikaShopMusic = !Convert.ToBoolean(AllSettingsDropdowns["FixLaikaShopMusic"].value);
            //JaLoaderSettings.Replace0WithBanned = !Convert.ToBoolean(AllSettingsDropdowns["Replace0WithBanned"].value);
            JaLoaderSettings.MirrorDistances = (MirrorDistances)AllSettingsDropdowns["MirrorDistance"].value;
            JaLoaderSettings.CursorMode = (CursorMode)AllSettingsDropdowns["CursorMode"].value;
            //JaLoaderSettings.FixItemsFallingBehindShop = !Convert.ToBoolean(AllSettingsDropdowns["FixItemsFallingBehindShop"].value);
            JaLoaderSettings.FixBorderGuardsFlags = !Convert.ToBoolean(AllSettingsDropdowns["FixGuardsFlags"].value);
            JaLoaderSettings.ShowDisabledMods = !Convert.ToBoolean(AllSettingsDropdowns["ShowDisabledMods"].value);
            JaLoaderSettings.MultipleTypesInBoxes = !Convert.ToBoolean(AllSettingsDropdowns["MultipleTypesInBoxes"].value);
            JaLoaderSettings.RemoveSmugglingPunishments = !Convert.ToBoolean(AllSettingsDropdowns["RemoveSmugglingPunishments"].value);

            JLFPSText.SetActive(JaLoaderSettings.ShowFPSCounter);
            JLDebugText.gameObject.SetActive(JaLoaderSettings.DebugMode);

            SettingsManager.SaveSettings(false);

            if ((ConsoleModes)AllSettingsDropdowns["ConsoleMode"].value == ConsoleModes.Disabled)
                Console.Instance.ToggleVisibility(false);

            SetConsolePosition((ConsolePositions)AllSettingsDropdowns["ConsolePosition"].value);
            ModsLocationText.SetActive(!JaLoaderSettings.HideModFolderLocation);

            CustomRadioController.Instance.UpdateMenuMusic(!Convert.ToBoolean(AllSettingsDropdowns["MenuMusic"].value), (float)AllSettingsSliders["MenuMusicVolume"].value / 100);

            UncleHelper.Instance.UncleEnabled = !JaLoaderSettings.DisableUncle;

            if (wasJaDownChanged)
            {
                if (JaLoaderSettings.EnableJaDownloader)
                {
                    var path = Path.GetFullPath(Path.Combine(Path.Combine(Application.dataPath, "."), "JaDownloader.exe"));
                    try
                    {
                        Process.Start($@"{Application.dataPath}\..\JaDownloaderSetup.exe", $"\"{path}\"");
                    }
                    catch (Exception)
                    {
                        // cancelled
                    }
                }
                else
                {
                    try
                    {
                        Process.Start($@"{Application.dataPath}\..\JaDownloaderSetup.exe", "Uninstall");
                    }
                    catch (Exception)
                    {
                        // cancelled
                    }
                }
            }
        }

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
            dialog.FindDeepChild("Title").GetComponent<Text>().text = "Enable JaDownloader";
            dialog.Find("Message").GetComponent<Text>().text = "JaDownloader is a tool that allows you to install most mods automatically. \r\n Would you like to enable it now? (you can find this setting in Modloader Settings/Preferences)";
            dialog.FindButton("YesButton").onClick.AddListener(delegate {
                var path = Path.GetFullPath(Path.Combine(Path.Combine(Application.dataPath, "."), "JaDownloader.exe"));
                Process.Start($@"{Application.dataPath}\.\JaDownloaderSetup.exe", $"\"{path}\""); JaLoaderSettings.EnableJaDownloader = true; SetObstructRay(false); Destroy(dialog);
            });
            dialog.FindButton("NoButton").onClick.AddListener(delegate {SetObstructRay(false); Destroy(dialog); });
            dialog.SetActive(true);

            JaLoaderSettings.AskedAboutJaDownloader = true;
            SettingsManager.SaveSettings(false);
        }

        public void ShowNotice(string subtitle, string message, bool enableDontShowAgain = true, bool ignoreObstructRayChange = false)
        {
            allNotices.Add(message);
            noticesToShow.Add((subtitle, message, ignoreObstructRayChange));

            if (!JaLoaderSettings.DontShowAgainNotices.Contains(message))
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

            if (JaLoaderSettings.DontShowAgainNotices.Contains(noticesToShow[0].Item2) && noticesToShow.Count > 1)
            {
                var _ignoreIfLastOne = noticesToShow[0].Item3;
                noticesToShow.RemoveAt(0);

                for (int i = 0; i < noticesToShow.Count; i++)
                {
                    if(JaLoaderSettings.DontShowAgainNotices.Contains(noticesToShow[i].Item2))
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

            if (dontShowAgain && !JaLoaderSettings.DontShowAgainNotices.Contains(JLNoticePanel.transform.Find("Message").GetComponent<Text>().text))
            {
                JaLoaderSettings.DontShowAgainNotices.Add(JLNoticePanel.transform.Find("Message").GetComponent<Text>().text);
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
            if (allNotices.Count <= JaLoaderSettings.DontShowAgainNotices.Count)
            {
                var toRemove = JaLoaderSettings.DontShowAgainNotices.Except(allNotices).ToList();

                foreach (var item in toRemove)
                    JaLoaderSettings.DontShowAgainNotices.Remove(item);

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

    public class OnRightClickUIElement : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent onRightClick = new UnityEvent();

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                onRightClick.Invoke();

                eventData.Use();
            }
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
