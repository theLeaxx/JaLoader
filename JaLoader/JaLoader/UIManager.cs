using BepInEx;
using HarmonyLib;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text.RegularExpressions;
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
        public static UIManager Instance { get; private set; }

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

        private readonly ModLoader modLoader = ModLoader.Instance;
        private readonly SettingsManager settingsManager = SettingsManager.Instance;
        public GameObject UICanvas {get; private set;}
        public GameObject modTemplatePrefab { get; private set; }
        public GameObject messageTemplatePrefab { get; private set; }
        public GameObject modConsole { get; private set; }
        private GameObject moreInfoPanelMods;
        private GameObject modSettingsScrollView;
        public GameObject modSettingsScrollViewContent { get; private set; }

        private InputField modsInputField;
        private Transform modsPanelScrollView;
        
        public Text modsCountText { get; private set; }

        public GameObject modOptionsHolder { get; private set; }
        public GameObject modOptionsNameTemplate { get; private set; }
        public GameObject modOptionsHeaderTemplate { get; private set; }
        public GameObject modOptionsDropdownTemplate { get; private set; }
        public GameObject modOptionsToggleTemplate { get; private set; }
        public GameObject modOptionsSliderTemplate { get; private set; }
        public GameObject modOptionsKeybindTemplate { get; private set; }
        public GameObject modOptionsInputTemplate { get; private set; }

        public GameObject objectsList { get; private set; }
        public GameObject objectTemplate { get; private set; }
        public Text currentlySelectedText { get; private set; }
        
        private GameObject noticePanel;
        public GameObject modTemplateObject;

        public GameObject catalogueTemplate { get; private set; }
        public GameObject catalogueEntryTemplate { get; private set; }

        public GameObject modLoaderText { get; private set; }
        public GameObject modFolderText { get; private set; }
        public GameObject fpsText { get; private set; }
        public GameObject debugText { get; private set; }

        public GameObject tooltipText { get; private set; }
        public Text tooltipTextText { get; private set; }

        private GameObject installButton;

        private Dropdown toggleShowDisabledMods;

        public Texture2D knobTexture { get; private set; }

        private MainMenuBookC book;
        private GameObject exitConfirmButton;
        private GameObject newGameConfirmButton;

        private bool isOnOtherPage;
        private bool inOptions;
        private bool inModsOptions;
        private bool inModsList;
        private bool isObstructing;
        public bool CanCloseMap = true;

        #region Settings Dropdown
        // Preferences tab
        private Dropdown consoleModeDropdown;
        private Dropdown consolePositionDropdown;
        private Dropdown showModsFolderDropdown;
        private Dropdown enableJaDownloaderDropdown;
        private Dropdown updateCheckFreqDropdown;
        private Dropdown skipLanguageSelectionDropdown;
        private Dropdown discordRichPresenceDropdown;
        private Dropdown debugModeDropdown;

        // Tweaks tab
        private Dropdown menuMusicDropdown;
        private Slider menuMusicSlider;
        private Dropdown songsDropdown;
        private Dropdown songsBehaviourDropdown;
        private Dropdown radioAdsDropdown;
        private Dropdown uncleDropdown;
        private Dropdown enhancedMovementDropdown;
        private Dropdown changeLicensePlateTextDropdown;
        private InputField licensePlateTextField;
        private Dropdown showFPSDropdown;
        private Dropdown fixLaikaShopMusic;
        private Dropdown replace0WithBanned;
        private Dropdown mirrorDistance;
        private Dropdown cursorMode;
        private Dropdown fixItemsFallingBehindShop;
        private Dropdown fixBorderGuardsFlags;
        #endregion

        private GameObject menuMusicPlayer;

        private AudioClip buttonClickSound;
        private AudioSource audioSource;
        private List<string> allNotices = new List<string>();

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

        private void Update()
        {
            if (UICanvas == null)
                return;

            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                if (!isOnOtherPage && !IsBookClosed())
                {
                    UICanvas.transform.GetChild(0).Find("BookUI").gameObject.SetActive(true);
                }
                else
                {
                    UICanvas.transform.GetChild(0).Find("BookUI").gameObject.SetActive(false);
                }

                if (exitConfirmButton.activeSelf || newGameConfirmButton.activeSelf)
                    isOnOtherPage = true;
                else
                    isOnOtherPage = false;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (UICanvas.transform.GetChild(1).gameObject.activeSelf)
                    ToggleModMenu();

                if (UICanvas.transform.Find("JLSettingsPanel").Find("Main").gameObject.activeSelf)
                    ToggleModLoaderSettings_Main();

                RefreshUI();
            }

            /*if (Input.GetKeyDown(KeyCode.Delete))
                FixTranslations();*/

            //annoying fix for dropdowns only working once
            if (inOptions && Input.GetMouseButtonDown(0))
                if (consoleModeDropdown.transform.Find("Dropdown List") || fixLaikaShopMusic.transform.Find("Dropdown List") || replace0WithBanned.transform.Find("Dropdown List") || mirrorDistance.transform.Find("Dropdown List") || cursorMode.transform.Find("Dropdown List") || fixItemsFallingBehindShop.transform.Find("Dropdown List") || fixBorderGuardsFlags.transform.Find("Dropdown List") || songsBehaviourDropdown.transform.Find("Dropdown List") || radioAdsDropdown.transform.Find("Dropdown List") || consolePositionDropdown.transform.Find("Dropdown List") || showModsFolderDropdown.transform.Find("Dropdown List") || debugModeDropdown.transform.Find("Dropdown List") || menuMusicDropdown.transform.Find("Dropdown List") || uncleDropdown.transform.Find("Dropdown List") || songsDropdown.transform.Find("Dropdown List") || skipLanguageSelectionDropdown.transform.Find("Dropdown List") || discordRichPresenceDropdown.transform.Find("Dropdown List") || changeLicensePlateTextDropdown.transform.Find("Dropdown List") || enhancedMovementDropdown.transform.Find("Dropdown List") || showFPSDropdown.transform.Find("Dropdown List") || enableJaDownloaderDropdown.transform.Find("Dropdown List") || updateCheckFreqDropdown.transform.Find("Dropdown List"))
                    RefreshUI();

            if (inModsList && Input.GetMouseButtonDown(0))
                if (toggleShowDisabledMods.transform.Find("Dropdown List"))
                    RefreshUI();

            if (inModsOptions && Input.GetMouseButtonDown(0))
                foreach (RectTransform item in modSettingsScrollViewContent.transform.GetComponentsInChildren<RectTransform>())
                    if (item.gameObject.name == "Dropdown List" && item.parent.gameObject.activeSelf)
                        RefreshUI();  
        }

        private void OnMenuLoad()
        {
            if (!settingsManager.loadedFirstTime)
            {
                var loadingScreenScript = gameObject.AddComponent<LoadingScreen>();
                loadingScreenScript.ShowLoadingScreen();
            }

            if (UICanvas == null)
                StartCoroutine(LoadUIDelay());

            SetNewspaperText();

            menuMusicPlayer = GameObject.Find("RadioFreq");
            menuMusicPlayer.AddComponent<MenuVolumeChanger>();
            UpdateMenuMusic(!settingsManager.DisableMenuMusic, (float)settingsManager.MenuMusicVolume / 100);

            book = FindObjectOfType<MainMenuBookC>();

            exitConfirmButton = GameObject.Find("ExitPage").transform.GetChild(0).gameObject;
            newGameConfirmButton = GameObject.Find("New Game").transform.GetChild(1).gameObject;
            var skipConfirmButton = GameObject.Find("New Game").transform.GetChild(4).gameObject;

            newGameConfirmButton.GetComponent<UIButton>().onClick.Add(new EventDelegate(EventsManager.Instance, "OnNewGameStart"));
            skipConfirmButton.GetComponent<UIButton>().onClick.Add(new EventDelegate(EventsManager.Instance, "OnNewGameStart"));

            ToggleUIVisibility(true);

            var go = new GameObject();
            go.AddComponent<MenuCarRotate>();

            AddObjectShortcuts();
        }

        private void OnLoadStart()
        {
            ToggleUIVisibility(false);
            Console.Instance.ToggleVisibility(false);

            if (inOptions)
            {
                inOptions = false;
                UICanvas.transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(false);
                UICanvas.transform.GetChild(2).transform.GetChild(1).gameObject.SetActive(false);
                UICanvas.transform.GetChild(2).transform.GetChild(2).gameObject.SetActive(false);
                UICanvas.transform.GetChild(2).transform.GetChild(3).gameObject.SetActive(false);
            }
        }

        private void OnGameLoad()
        {
            var uiRoot = GameObject.Find("UI Root");

            AddPauseButtons();

            var optionsSubMenu = uiRoot.transform.GetChild(5).Find("Restart Confirm/Accept").gameObject;
            optionsSubMenu.GetComponent<UIButton>().onClick.Clear();
            optionsSubMenu.GetComponent<UIButton>().onClick.Add(new EventDelegate(SaveDataThenReload));
            
            FixTranslations();

            GameObject.Find("UI Root").transform.Find("UncleStuff").gameObject.AddComponent<EnableCursorOnEnable>();

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

        public void ToggleUIVisibility(bool show)
        {
            if (UICanvas == null)
                return;

            RefreshUI();
            UICanvas.transform.GetChild(0).gameObject.SetActive(show);
        }

        private IEnumerator LoadUIDelay()
        {
            Debug.Log("Loading JaLoader UI...");

            gameObject.GetComponent<Stopwatch>().StartCounting();

            yield return new WaitForSeconds(0.1f);

            var bundleLoadReq = AssetBundle.LoadFromFileAsync(Path.Combine(settingsManager.ModFolderLocation, @"Required\JaLoader_UI.unity3d"));

            yield return bundleLoadReq;

            AssetBundle ab = bundleLoadReq.assetBundle;

            if (ab == null)
            {
                Destroy(GameObject.Find("JaLoader Loading Screen Canvas"));
                StopAllCoroutines();

                modLoader.CreateImportantNotice("\n\nThe file 'JaLoader_UI.unity3d' was not found. You can try:", "Reinstalling JaLoader with JaPatcher\n\n\nCopying the file from JaPatcher's directory/Assets/Required to Mods/Required");
                Destroy(GameObject.Find("JaLoader Modding Helpers"));
                Destroy(gameObject);

                yield break;
            }

            var assetLoadRequest = ab.LoadAssetAsync<GameObject>("JLCanvas.prefab");

            yield return assetLoadRequest;

            var UIPrefab = assetLoadRequest.asset as GameObject;

            UICanvas = Instantiate(UIPrefab);
            DontDestroyOnLoad(UICanvas);

            try
            {
                modTemplatePrefab = UICanvas.transform.Find("JLModsPanel/Scroll View").GetChild(0).GetChild(0).GetChild(0).gameObject;
                modConsole = UICanvas.transform.Find("JLConsole/Console").gameObject;
                messageTemplatePrefab = modConsole.transform.Find("Scroll View/Viewport/Content").GetChild(0).gameObject;
                moreInfoPanelMods = UICanvas.transform.Find("JLModsPanel/MoreInfo").gameObject;
                noticePanel = UICanvas.transform.Find("JLNotice").gameObject;
                catalogueTemplate = UICanvas.transform.Find("JLCatalogue/MainTemplate").gameObject;
                catalogueEntryTemplate = catalogueTemplate.transform.Find("Viewport/Content/Template").gameObject;
                objectsList = UICanvas.transform.Find("JLObjectsList/ObjectsList").gameObject;
                objectTemplate = objectsList.transform.Find("Scroll View/Viewport/Content").GetChild(0).gameObject;
                currentlySelectedText = objectsList.transform.Find("CurrentlySelectedText").GetComponent<Text>();

                modLoaderText = UICanvas.transform.GetChild(0).Find("JaLoader").gameObject;
                modFolderText = UICanvas.transform.GetChild(0).Find("ModsFolder").gameObject;

                Console.Instance.Init();

                if (settingsManager.HideModFolderLocation)
                    modFolderText.SetActive(false);

                string latestVersion = settingsManager.GetLatestUpdateVersionString("https://api.github.com/repos/theLeaxx/JaLoader/releases/latest", settingsManager.GetVersion());

                if (latestVersion == "-1")
                {
                    //couldn't check for updates

                    Console.LogError("Couldn't check for updates!");

                    modLoaderText.GetComponent<Text>().text = $"JaLoader <color={(SettingsManager.IsPreReleaseVersion ? "red" : "yellow")}>{settingsManager.GetVersionString()}</color> loaded!";
                }
                else if (int.Parse(latestVersion.Replace(".", "")) > settingsManager.GetVersion())
                {
                    SetObstructRay(true);

                    modLoaderText.GetComponent<Text>().text = $"JaLoader <color={(SettingsManager.IsPreReleaseVersion ? "red" : "yellow")}>{settingsManager.GetVersionString()}</color> loaded! (<color=lime>{latestVersion} available!</color>)";

                    var dialog = UICanvas.transform.Find("JLUpdateDialog").gameObject;
                    dialog.transform.Find("YesButton").GetComponent<Button>().onClick.AddListener(() => modLoader.StartUpdate());
                    dialog.transform.Find("Subtitle").GetComponent<Text>().text = $"{settingsManager.GetVersionString()} ➔ {latestVersion}";
                    dialog.transform.Find("NoButton").GetComponent<Button>().onClick.AddListener(delegate { dialog.SetActive(false); SetObstructRay(false); });
                    dialog.transform.Find("OpenGitHubButton").GetComponent<Button>().onClick.AddListener(() => Application.OpenURL($"{SettingsManager.JaLoaderGitHubLink}/releases/latest"));
                    dialog.SetActive(true);

                    settingsManager.updateAvailable = true;

                    MakeWrenchGreen();
                }
                else
                    modLoaderText.GetComponent<Text>().text = $"JaLoader <color={(SettingsManager.IsPreReleaseVersion ? "red" : "yellow")}>{settingsManager.GetVersionString()}</color> loaded!";

                modFolderText.GetComponent<Text>().text = $"Mods folder: <color=yellow>{settingsManager.ModFolderLocation}</color>";

                UICanvas.transform.Find("JLPanel/BookUI/ModsButton").GetComponent<Button>().onClick.AddListener(ToggleModMenu);
                UICanvas.transform.Find("JLModsPanel/ExitButton").GetComponent<Button>().onClick.AddListener(ToggleModMenu);

                UICanvas.transform.Find("JLPanel/BookUI/OptionsButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Main);
                UICanvas.transform.Find("JLSettingsPanel/Main/ExitButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Main);

                UICanvas.transform.Find("JLSettingsPanel/Main/VerticalButtonLayoutGroup/PreferencesButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Preferences);
                UICanvas.transform.Find("JLSettingsPanel/Main/VerticalButtonLayoutGroup/TweaksButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Tweaks);
                UICanvas.transform.Find("JLSettingsPanel/Main/VerticalButtonLayoutGroup/AccessibilityButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Accessibility);

                UICanvas.transform.Find("JLSettingsPanel/Preferences/BackButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Preferences);
                UICanvas.transform.Find("JLSettingsPanel/Tweaks/BackButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Tweaks);
                UICanvas.transform.Find("JLSettingsPanel/Accessibility/BackButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Accessibility);

                UICanvas.transform.Find("JLSettingsPanel/Preferences/SaveButton").GetComponent<Button>().onClick.AddListener(SaveValues);
                UICanvas.transform.Find("JLSettingsPanel/Tweaks/SaveButton").GetComponent<Button>().onClick.AddListener(SaveValues);
                UICanvas.transform.Find("JLSettingsPanel/Accessibility/SaveButton").GetComponent<Button>().onClick.AddListener(SaveValues);

                noticePanel.transform.Find("UnderstandButton").GetComponent<Button>().onClick.AddListener(delegate { CloseNotice(); });
                noticePanel.transform.Find("DontShowAgainButton").GetComponent<Button>().onClick.AddListener(delegate { CloseNotice(true); });

                modSettingsScrollView = UICanvas.transform.Find("JLModsPanel/SettingsScrollView").gameObject;
                modSettingsScrollViewContent = modSettingsScrollView.transform.GetChild(0).GetChild(0).gameObject;

                modsCountText = UICanvas.transform.Find("JLModsPanel/ModsCount").GetComponent<Text>();

                modsPanelScrollView = UICanvas.transform.Find("JLModsPanel/Scroll View/Viewport/Content");
                modsInputField = UICanvas.transform.Find("JLModsPanel/SearchBar").GetComponent<InputField>();
                modsInputField.onValueChanged.AddListener(delegate { OnInputValueChanged_ModsList(); });

                modSettingsScrollView.transform.Find("SaveButton").GetComponent<Button>().onClick.AddListener(SaveModSettings);
                modSettingsScrollView.transform.Find("ResetButton").GetComponent<Button>().onClick.AddListener(ResetModSettings);

                modOptionsHolder = modSettingsScrollViewContent.transform.Find("SettingsHolder").gameObject;
                modOptionsNameTemplate = modSettingsScrollViewContent.transform.Find("ModName").gameObject;
                modOptionsHeaderTemplate = modSettingsScrollViewContent.transform.Find("HeaderTemplate").gameObject;
                modOptionsDropdownTemplate = modSettingsScrollViewContent.transform.Find("DropdownTemplate").gameObject;
                modOptionsToggleTemplate = modSettingsScrollViewContent.transform.Find("ToggleTemplate").gameObject;
                modOptionsSliderTemplate = modSettingsScrollViewContent.transform.Find("SliderTemplate").gameObject;
                modOptionsKeybindTemplate = modSettingsScrollViewContent.transform.Find("KeybindTemplate").gameObject;
                modOptionsInputTemplate = modSettingsScrollViewContent.transform.Find("InputTemplate").gameObject;

                consoleModeDropdown = UICanvas.transform.Find("JLSettingsPanel/Preferences/Scroll View/Viewport/Content/Row1/ConsoleMode").gameObject.GetComponent<Dropdown>();
                consolePositionDropdown = UICanvas.transform.Find("JLSettingsPanel/Preferences/Scroll View/Viewport/Content/Row1/ConsolePosition").gameObject.GetComponent<Dropdown>();
                showModsFolderDropdown = UICanvas.transform.Find("JLSettingsPanel/Preferences/Scroll View/Viewport/Content/Row1/ShowModsFolderLocation").gameObject.GetComponent<Dropdown>();
                enableJaDownloaderDropdown = UICanvas.transform.Find("JLSettingsPanel/Preferences/Scroll View/Viewport/Content/Row1/EnableJaDownloader").gameObject.GetComponent<Dropdown>();
                updateCheckFreqDropdown = UICanvas.transform.Find("JLSettingsPanel/Preferences/Scroll View/Viewport/Content/Row1/UpdateCheckFrequency").gameObject.GetComponent<Dropdown>();
                skipLanguageSelectionDropdown = UICanvas.transform.Find("JLSettingsPanel/Preferences/Scroll View/Viewport/Content/Row2/SkipLanguageSelectionScreen").gameObject.GetComponent<Dropdown>();
                discordRichPresenceDropdown = UICanvas.transform.Find("JLSettingsPanel/Preferences/Scroll View/Viewport/Content/Row2/DiscordRichPresence").gameObject.GetComponent<Dropdown>();
                debugModeDropdown = UICanvas.transform.Find("JLSettingsPanel/Preferences/Scroll View/Viewport/Content/Row3/DebugMode").gameObject.GetComponent<Dropdown>();

                menuMusicDropdown = UICanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row1/MenuMusic").gameObject.GetComponent<Dropdown>();
                menuMusicSlider = UICanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row1/MenuMusicVolume").gameObject.GetComponent<Slider>();
                songsDropdown = UICanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row2/CustomSongs").gameObject.GetComponent<Dropdown>();
                songsBehaviourDropdown = UICanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row2/CustomSongsBehaviour").gameObject.GetComponent<Dropdown>();
                radioAdsDropdown = UICanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row2/RadioAds").gameObject.GetComponent<Dropdown>();
                uncleDropdown = UICanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row2/Uncle").gameObject.GetComponent<Dropdown>();
                enhancedMovementDropdown = UICanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row2/UseEnhancedMovement").gameObject.GetComponent<Dropdown>();
                changeLicensePlateTextDropdown = UICanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row3/ChangeLicensePlate").gameObject.GetComponent<Dropdown>();
                licensePlateTextField = UICanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row3/LicensePlateText/InputField").gameObject.GetComponent<InputField>();
                showFPSDropdown = UICanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row4/ShowFPSCounter").gameObject.GetComponent<Dropdown>();
                cursorMode = UICanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row4/CursorMode").gameObject.GetComponent<Dropdown>();
                fixLaikaShopMusic = UICanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row5/FixLaikaShopMusic").gameObject.GetComponent<Dropdown>();
                replace0WithBanned = UICanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row5/Replace0WithBanned").gameObject.GetComponent<Dropdown>();
                mirrorDistance = UICanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row5/MirrorDistance").gameObject.GetComponent<Dropdown>();
                fixItemsFallingBehindShop = UICanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row5/FixItemsFallingBehindShop").gameObject.GetComponent<Dropdown>();
                fixBorderGuardsFlags = UICanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row5/FixGuardsFlags").gameObject.GetComponent<Dropdown>();

                installButton = UICanvas.transform.Find("JLModsPanel/InstallButton").gameObject;
                installButton.GetComponent<Button>().onClick.AddListener(InstallMod);
                toggleShowDisabledMods = UICanvas.transform.Find("JLModsPanel/ToggleShowDisabledMods/Dropdown").gameObject.GetComponent<Dropdown>();

                toggleShowDisabledMods.onValueChanged.AddListener(delegate { ShowDisabledMods(); });

                UICanvas.transform.Find("JLSettingsPanel/Accessibility/VerticalLayoutGroup/TopRow/SwitchLanguage").gameObject.GetComponent<Button>().onClick.AddListener(SwitchLanguage);
                UICanvas.transform.Find("JLSettingsPanel/Accessibility/VerticalLayoutGroup/TopRow/OpenModsFolder").gameObject.GetComponent<Button>().onClick.AddListener(OpenModsFolder);
                UICanvas.transform.Find("JLSettingsPanel/Accessibility/VerticalLayoutGroup/TopRow/OpenSavesFolder").gameObject.GetComponent<Button>().onClick.AddListener(OpenSavesFolder);
                UICanvas.transform.Find("JLSettingsPanel/Accessibility/VerticalLayoutGroup/TopRow/OpenOutputLog").gameObject.GetComponent<Button>().onClick.AddListener(OpenOutputLog);

                fpsText = UICanvas.transform.Find("FPSCounter").gameObject;
                fpsText.AddComponent<FPSCounter>();

                debugText = UICanvas.transform.Find("DebugInfo").gameObject;
                debugText.AddComponent<DebugInfo>();

                SetOptionsValues();

                mirrorDistance.onValueChanged.AddListener(delegate { GameTweaks.Instance.UpdateMirrors((MirrorDistances)mirrorDistance.value); });
                cursorMode.onValueChanged.AddListener(delegate { GameTweaks.Instance.ChangeCursor((CursorMode)cursorMode.value); });

                Console.LogMessage("JaLoader", $"JaLoader {settingsManager.GetVersionString()} loaded successfully!");
                gameObject.GetComponent<Stopwatch>().StopCounting();
                Debug.Log($"Loaded JaLoader UI! ({gameObject.GetComponent<Stopwatch>().timePassed}s)");
            }
            catch (Exception e)
            {
                Console.LogMessage("JaLoader", $"JaLoader {settingsManager.GetVersionString()} failed to load successfully!");
                gameObject.GetComponent<Stopwatch>().StopCounting();
                Debug.Log($"Failed to load JaLoader UI!");

                Debug.Log($"Exception: {e}");

                ShowNotice("JaLoader failed to load!", "JaLoader failed to fully load the UI. This is likely due to an outdated JaLoader_UI.unity3d file. Please try reinstalling JaLoader with JaPatcher.");

                if (!settingsManager.updateAvailable)
                    modLoaderText.GetComponent<Text>().text = $"JaLoader <color=red>{settingsManager.GetVersionString()}</color> failed to load!";
                else
                    modLoaderText.GetComponent<Text>().text = $"JaLoader <color=red>{settingsManager.GetVersionString()}</color> failed to load! (<color=lime>Update available!</color>)";

                throw;
            }
            finally
            {
                StartCoroutine(ReferencesLoader.Instance.LoadAssemblies());

                if (modLoader.IsCrackedVersion)
                    ShowNotice("PIRATED GAME DETECTED", "You are using a pirated version of Jalopy.\r\n\r\nYou may encounter issues with certain mods, as well as more bugs in general.\r\n\r\nIf you encounter any game-breaking bugs, feel free to submit them to the official GitHub for JaLoader. Remember to mark them with the \"pirated\" tag!\r\n\r\nHave fun!");

                if (SettingsManager.IsPreReleaseVersion)
                    ShowNotice("USING A PRE-RELEASE VERSION OF JALOADER", "You are using a pre-release version of JaLoader.\r\n\r\nThese versions are prone to bugs and may cause issues with certain mods.\r\n\r\nPlease report any bugs you encounter to the JaLoader GitHub page, marking them with the \"pre-release\" tag.\r\n\r\nHave fun!");

                if (!settingsManager.AskedAboutJaDownloader && !settingsManager.EnableJaDownloader)
                    ShowJaDownloaderNotice();

                //var knob = ab.LoadAsset<Sprite>("knob.png");
                //var texture = new Texture2D((int)knob.rect.width, (int)knob.rect.height, knob.texture.format, true);
                //Graphics.CopyTexture(knob.texture, texture);

                var texture = ab.LoadAsset<Texture2D>("knob.png");

                knobTexture = texture;

                EventsManager.Instance.OnUILoadFinish();

                ab.Unload(false);
            }

            var clickers = GameObject.Find("UI Root").transform.Find("Options/OptionsGameplay/Back").GetComponent<MainMenuClickersC>();
            buttonClickSound = clickers.audioClip;
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
                GameObject.Find("Newspaper").transform.Find("TextMeshPro").GetComponent<TextMeshPro>().text = $"JALOPY {versionText}|JALOADER {(SettingsManager.IsPreReleaseVersion ? $"{settingsManager.GetVersionString().Replace("Pre-Release", "PR")}" : settingsManager.GetVersionString())}";

                if (int.Parse(versionText.Replace(".", "")) < 1105)
                    StartCoroutine(ShowNoticeAfterLoad("OUTDATED GAME DETECTED", "You are using an outdated version of Jalopy.\r\n\r\nYou may encounter issues with JaLoader and certain mods, as well as more bugs in general.\r\n\r\nIf you encounter bugs, please make sure to ask or check if they exist in newer versions as well before reporting them.\r\n\r\nHave fun!"));
            }
        }

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
            for (int i = 0; i < modSettingsScrollViewContent.transform.childCount; i++)
            {
                if (modSettingsScrollViewContent.transform.GetChild(i).gameObject.activeSelf && Regex.Match(modSettingsScrollViewContent.transform.GetChild(i).gameObject.name, @"(.{15})\s*$").ToString() == "-SettingsHolder")
                {
                    string fullName = modSettingsScrollViewContent.transform.GetChild(i).gameObject.name;

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

                    var mod = modLoader.FindMod(modAuthor, modID, modName);

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
            for (int i = 0; i < modSettingsScrollViewContent.transform.childCount; i++)
            {
                if (modSettingsScrollViewContent.transform.GetChild(i).gameObject.activeSelf && Regex.Match(modSettingsScrollViewContent.transform.GetChild(i).gameObject.name, @"(.{15})\s*$").ToString() == "-SettingsHolder")
                {
                    string fullName = modSettingsScrollViewContent.transform.GetChild(i).gameObject.name;

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

                    var mod = modLoader.FindMod(modAuthor, modID, modName);

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
            consoleModeDropdown.value = (int)settingsManager.ConsoleMode;
            consolePositionDropdown.value = (int)settingsManager.ConsolePosition;
            showModsFolderDropdown.value = settingsManager.HideModFolderLocation ? 1 : 0;
            skipLanguageSelectionDropdown.value = settingsManager.SkipLanguage ? 0 : 1;
            discordRichPresenceDropdown.value = settingsManager.UseDiscordRichPresence ? 0 : 1;
            debugModeDropdown.value = settingsManager.DebugMode ? 0 : 1;

            menuMusicDropdown.value = settingsManager.DisableMenuMusic ? 1 : 0;
            menuMusicSlider.value = settingsManager.MenuMusicVolume;
            uncleDropdown.value = settingsManager.DisableUncle ? 1 : 0;
            songsDropdown.value = settingsManager.UseCustomSongs ? 0 : 1;
            songsBehaviourDropdown.value = (int)settingsManager.CustomSongsBehaviour;
            radioAdsDropdown.value = settingsManager.RadioAds ? 0 : 1;
            enhancedMovementDropdown.value = settingsManager.UseExperimentalCharacterController ? 1 : 0;
            changeLicensePlateTextDropdown.value = (int)settingsManager.ChangeLicensePlateText;
            licensePlateTextField.text = settingsManager.LicensePlateText;
            showFPSDropdown.value = settingsManager.ShowFPSCounter ? 1 : 0;
            enableJaDownloaderDropdown.value = settingsManager.EnableJaDownloader ? 0 : 1;
            updateCheckFreqDropdown.value = (int)settingsManager.UpdateCheckMode;

            fixLaikaShopMusic.value = settingsManager.FixLaikaShopMusic ? 0 : 1;
            mirrorDistance.value = (int)settingsManager.MirrorDistances;
            cursorMode.value = (int)settingsManager.CursorMode;

            fixItemsFallingBehindShop.value = settingsManager.FixItemsFalilngBehindShop ? 0 : 1;
            fixBorderGuardsFlags.value = settingsManager.FixBorderGuardsFlags ? 0 : 1;

            toggleShowDisabledMods.value = settingsManager.ShowDisabledMods ? 0 : 1;

            fpsText.SetActive(settingsManager.ShowFPSCounter);
            debugText.SetActive(settingsManager.DebugMode);

            ShowDisabledMods();
        }

        public void UpdateMenuMusic(bool enabled, float volume)
        {
            if (SceneManager.GetActiveScene().buildIndex != 1)
                return;

            menuMusicPlayer.SetActive(enabled);
            menuMusicPlayer.GetComponent<MenuVolumeChanger>().volume = volume;
        }

        private void SwitchLanguage()
        {
            settingsManager.selectedLanguage = false;

            SaveValues();

            ToggleModLoaderSettings_Accessibility();
            ToggleModLoaderSettings_Main();
            Console.Instance.ToggleVisibility(false);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
            isObstructing = false;
        }

        private void OpenModsFolder()
        {
            PlayClickSound();

            Application.OpenURL(settingsManager.ModFolderLocation);
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
            if (UICanvas == null)
                return;

            UICanvas.SetActive(false);
            UICanvas.SetActive(true);
        }

        public void ShowTooltip(string text)
        {
            UICanvas.transform.Find("JLTooltip/Text").GetComponent<Text>().text = text;
            UICanvas.transform.Find("JLTooltip").gameObject.SetActive(true);
        }

        public void HideTooltip()
        {
            UICanvas.transform.Find("JLTooltip/Text").GetComponent<Text>().text = "";
            UICanvas.transform.Find("JLTooltip").gameObject.SetActive(false);
        }

        public void ToggleMoreInfo(string name, string author, string version, string description)
        {
            PlayClickSound();

            modSettingsScrollView.SetActive(false);
            inModsOptions = false;

            moreInfoPanelMods.SetActive(true);

            if (moreInfoPanelMods.transform.Find("ModName").GetComponent<Text>().text != name)
            {
                moreInfoPanelMods.transform.Find("ModName").GetComponent<Text>().text = name;
                moreInfoPanelMods.transform.Find("ModAuthor").GetComponent<Text>().text = author;
                moreInfoPanelMods.transform.Find("ModVersion").GetComponent<Text>().text = version;

                if (description != null)
                    moreInfoPanelMods.transform.Find("ModDescription").GetComponent<Text>().text = description;
                else
                    moreInfoPanelMods.transform.Find("ModDescription").GetComponent<Text>().text = "This mod does not have a description.";
            }
            else
            {
                moreInfoPanelMods.transform.Find("ModName").GetComponent<Text>().text = "Welcome to the mods list!";
                moreInfoPanelMods.transform.Find("ModAuthor").GetComponent<Text>().text = "";
                moreInfoPanelMods.transform.Find("ModVersion").GetComponent<Text>().text = "";
                moreInfoPanelMods.transform.Find("ModDescription").GetComponent<Text>().text = "You can enable/disable mods, view more information about them, adjust their settings, and arrange them in a desired load order using the provided directional arrows!";
            }
        }

        public void ToggleSettings(string objName)
        {
            PlayClickSound();

            modSettingsScrollView.SetActive(true);
            inModsOptions = true;

            moreInfoPanelMods.SetActive(false);

            if (modSettingsScrollViewContent.transform.Find(objName) && modSettingsScrollViewContent.transform.Find(objName).childCount != 0)
            {
                foreach (Transform item in modSettingsScrollViewContent.transform)
                    item.gameObject.SetActive(false);

                modSettingsScrollViewContent.transform.Find(objName).gameObject.SetActive(true);
            }
            else
            {
                foreach (Transform item in modSettingsScrollViewContent.transform)
                    item.gameObject.SetActive(false);

                modSettingsScrollViewContent.transform.Find("NoSettings").gameObject.SetActive(true);
            }
        }

        public void ToggleModMenu()
        {
            if (!IsBookClosed())
                book.CloseBook();

            modsInputField.Select();
            modsInputField.text = "";
            UICanvas.transform.GetChild(1).gameObject.SetActive(!UICanvas.transform.GetChild(1).gameObject.activeSelf);
            inModsList = !inModsList;

            if (SceneManager.GetActiveScene().buildIndex == 3)
                TogglePauseMenu(UICanvas.transform.GetChild(1).gameObject.activeSelf);

            ToggleObstructRay();

        }

        public void ToggleModLoaderSettings_Main()
        {
            if (!IsBookClosed())
                book.CloseBook();

            inOptions = !inOptions;
            UICanvas.transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(!UICanvas.transform.GetChild(2).transform.GetChild(0).gameObject.activeSelf);

            if (SceneManager.GetActiveScene().buildIndex == 3)
                TogglePauseMenu(UICanvas.transform.GetChild(2).transform.GetChild(0).gameObject.activeSelf);

            ToggleObstructRay();
        }

        public void ToggleModLoaderSettings_Preferences()
        {
            PlayClickSound();

            UICanvas.transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(!UICanvas.transform.GetChild(2).transform.GetChild(0).gameObject.activeSelf);
            UICanvas.transform.GetChild(2).transform.GetChild(1).gameObject.SetActive(!UICanvas.transform.GetChild(2).transform.GetChild(1).gameObject.activeSelf);
        }

        public void ToggleModLoaderSettings_Tweaks()
        {
            PlayClickSound();

            UICanvas.transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(!UICanvas.transform.GetChild(2).transform.GetChild(0).gameObject.activeSelf);
            UICanvas.transform.GetChild(2).transform.GetChild(2).gameObject.SetActive(!UICanvas.transform.GetChild(2).transform.GetChild(2).gameObject.activeSelf);
        }

        public void ToggleModLoaderSettings_Accessibility()
        {
            PlayClickSound();

            UICanvas.transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(!UICanvas.transform.GetChild(2).transform.GetChild(0).gameObject.activeSelf);
            UICanvas.transform.GetChild(2).transform.GetChild(3).gameObject.SetActive(!UICanvas.transform.GetChild(2).transform.GetChild(3).gameObject.activeSelf);
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

        public void AddWarningToMod(GameObject warningIcon, string warningText)
        {
            warningIcon.SetActive(true);

            var template = warningIcon.transform.GetChild(0).GetChild(0).GetChild(0);

            var warning = Instantiate(template.gameObject, warningIcon.transform.GetChild(0).GetChild(0));
            warning.GetComponent<Text>().text = warningText;
            warning.SetActive(true);

            warningIcon.transform.GetChild(0).GetComponent<VerticalLayoutGroup>().enabled = false;
            warningIcon.transform.GetChild(0).GetComponent<VerticalLayoutGroup>().enabled = true;

            if(!warning.GetComponent<WarningOnHover>())
                warningIcon.AddComponent<WarningOnHover>();
        }

        private void InstallMod()
        {
            var modURL = modsInputField.text;

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

            while (!File.Exists(Path.Combine(settingsManager.ModFolderLocation, $"{author}_{repo}_Installed.txt")))
            {
                if(maximumTime == currentTime)
                {
                    ShowNotice("MOD INSTALLATION FAILED", "The mod installation failed. Please make sure you have the correct URL and that your internet connection is stable.", ignoreObstructRayChange: true);
                    yield break;
                }

                currentTime++;
                yield return new WaitForSeconds(1);
            }

            var dllName = File.ReadAllText(Path.Combine(settingsManager.ModFolderLocation, $"{author}_{repo}_Installed.txt"));
            File.Delete(Path.Combine(settingsManager.ModFolderLocation, $"{author}_{repo}_Installed.txt"));

            ShowNotice("MOD INSTALLED", "The mod has been successfully installed. You can now enable it in the mods list.", ignoreObstructRayChange: true);

            ReferencesLoader.Instance.StartCoroutine(ReferencesLoader.Instance.LoadAssemblies());
            
            modLoader.StartCoroutine(modLoader.InitializeMods(dllName));

            yield return null;
        }

        private void OnInputValueChanged_ModsList()
        {
            if (modsInputField.text.StartsWith("jaloader://install/") && settingsManager.EnableJaDownloader && SceneManager.GetActiveScene().buildIndex == 3)
                installButton.SetActive(true);
            else
                installButton.SetActive(false);

            foreach (Transform child in modsPanelScrollView)
            {
                if (child.name == "ModTemplate") continue;

                if (child.GetChild(2).GetChild(0).GetComponent<Text>().text.ToLower().Contains(modsInputField.text.ToLower()))
                {
                    if (IsModEntryDisabled(child.gameObject) && toggleShowDisabledMods.value == 1)
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
            foreach (Transform child in modsPanelScrollView)
            {
                if (child.name == "ModTemplate") continue;

                if(IsModEntryDisabled(child.gameObject) && toggleShowDisabledMods.value == 1)
                    child.gameObject.SetActive(false);
                else
                    child.gameObject.SetActive(true);
            }

            OnInputValueChanged_ModsList();

            SaveValues();
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

        private void SaveValues()
        {
            PlayClickSound();

            ConsoleModes consoleMode = (ConsoleModes)consoleModeDropdown.value;
            ConsolePositions consolePosition = (ConsolePositions)consolePositionDropdown.GetComponent<Dropdown>().value;

            if (settingsManager.UseDiscordRichPresence != !Convert.ToBoolean(discordRichPresenceDropdown.value))
            {
                ToggleModLoaderSettings_Preferences();
                ToggleModLoaderSettings_Main();
                ShowNotice("RESTART REQUIRED", "Changing the Discord Rich Presence setting requires a game restart for the changes to apply.");
            }

            var wasJaDownChanged = false;

            if (settingsManager.EnableJaDownloader != !Convert.ToBoolean(enableJaDownloaderDropdown.value))
            {
                wasJaDownChanged = true;
            }

            settingsManager.ConsoleMode = consoleMode;
            settingsManager.ConsolePosition = consolePosition;
            settingsManager.HideModFolderLocation = Convert.ToBoolean(showModsFolderDropdown.value);
            settingsManager.DebugMode = !Convert.ToBoolean(debugModeDropdown.value);
            settingsManager.DisableMenuMusic = Convert.ToBoolean(menuMusicDropdown.value);
            settingsManager.MenuMusicVolume = (int)menuMusicSlider.value;
            settingsManager.DisableUncle = Convert.ToBoolean(uncleDropdown.value);
            settingsManager.UseCustomSongs = !Convert.ToBoolean(songsDropdown.value);
            settingsManager.CustomSongsBehaviour = (CustomSongsBehaviour)songsBehaviourDropdown.value;
            settingsManager.RadioAds = !Convert.ToBoolean(radioAdsDropdown.value);
            settingsManager.UseExperimentalCharacterController = Convert.ToBoolean(enhancedMovementDropdown.value);
            settingsManager.SkipLanguage = !Convert.ToBoolean(skipLanguageSelectionDropdown.value);
            settingsManager.UseDiscordRichPresence = !Convert.ToBoolean(discordRichPresenceDropdown.value);
            settingsManager.ChangeLicensePlateText = (LicensePlateStyles)changeLicensePlateTextDropdown.value;
            settingsManager.LicensePlateText = licensePlateTextField.text;
            settingsManager.ShowFPSCounter = Convert.ToBoolean(showFPSDropdown.value);
            settingsManager.EnableJaDownloader = !Convert.ToBoolean(enableJaDownloaderDropdown.value);
            settingsManager.UpdateCheckMode = (UpdateCheckModes)updateCheckFreqDropdown.value;

            settingsManager.FixLaikaShopMusic = !Convert.ToBoolean(fixLaikaShopMusic.value);
            settingsManager.MirrorDistances = (MirrorDistances)mirrorDistance.value;
            settingsManager.CursorMode = (CursorMode)cursorMode.value;

            settingsManager.FixItemsFalilngBehindShop = !Convert.ToBoolean(fixItemsFallingBehindShop.value);
            settingsManager.FixBorderGuardsFlags = !Convert.ToBoolean(fixBorderGuardsFlags.value);

            settingsManager.ShowDisabledMods = !Convert.ToBoolean(toggleShowDisabledMods.value);

            fpsText.SetActive(settingsManager.ShowFPSCounter);
            debugText.gameObject.SetActive(settingsManager.DebugMode);

            settingsManager.SaveSettings(false);

            if (consoleMode == ConsoleModes.Disabled)
                Console.Instance.ToggleVisibility(false);

            Console.Instance.SetPosition(consolePosition);
            modFolderText.SetActive(!settingsManager.HideModFolderLocation);

            UpdateMenuMusic(!Convert.ToBoolean(menuMusicDropdown.value), (float)menuMusicSlider.value / 100);

            UncleHelper.Instance.UncleEnabled = !settingsManager.DisableUncle;

            if (wasJaDownChanged)
            {
                if (settingsManager.EnableJaDownloader)
                {
                    var path = Path.GetFullPath(Path.Combine(Path.Combine(Application.dataPath, ".."), "JaDownloader.exe"));
                    Process.Start($@"{Application.dataPath}\..\JaDownloaderSetup.exe", $"\"{path}\"");
                }
                else
                {
                    Process.Start($@"{Application.dataPath}\..\JaDownloaderSetup.exe", "Uninstall");
                }
            }
        }

        private List<(string, string, bool)> noticesToShow = new List<(string, string, bool)>();
        private bool showingNotice;

        private IEnumerator ShowNoticeAfterLoad(string subtitle, string message)
        {
            while (noticePanel == null)
                yield return null;

            ShowNotice(subtitle, message);
        }

        private void ShowJaDownloaderNotice()
        {
            SetObstructRay(true);
            var dialog = Instantiate(UICanvas.transform.Find("JLUpdateDialog").gameObject, UICanvas.transform.Find("JLUpdateDialog").parent);
            dialog.name = "JLDownloaderNotice";
            dialog.SetActive(false);
            dialog.transform.Find("Subtitle").gameObject.SetActive(false);
            dialog.transform.Find("OpenGitHubButton").gameObject.SetActive(false);
            dialog.transform.Find("Title").GetComponent<Text>().text = "Enable JaDownloader";
            dialog.transform.Find("Message").GetComponent<Text>().text = "JaDownloader is a tool that allows you to install most mods automatically. \r\n Would you like to enable it now? (you can find this setting in Modloader Settings/Preferences)";
            dialog.transform.Find("YesButton").GetComponent<Button>().onClick.AddListener(delegate {
                var path = Path.GetFullPath(Path.Combine(Path.Combine(Application.dataPath, ".."), "JaDownloader.exe"));
                Process.Start($@"{Application.dataPath}\..\JaDownloaderSetup.exe", $"\"{path}\""); settingsManager.EnableJaDownloader = true; SetObstructRay(false); Destroy(dialog);
            });
            dialog.transform.Find("NoButton").GetComponent<Button>().onClick.AddListener(delegate {SetObstructRay(false); Destroy(dialog); });
            dialog.SetActive(true);

            settingsManager.AskedAboutJaDownloader = true;
            settingsManager.SaveSettings(false);
        }

        public void ShowNotice(string subtitle, string message, bool enableDontShowAgain = true, bool ignoreObstructRayChange = false)
        {
            allNotices.Add(message);
            noticesToShow.Add((subtitle, message, ignoreObstructRayChange));

            if (!settingsManager.DontShowAgainNotices.Contains(message))
            {
                noticePanel.SetActive(true);

                SetObstructRay(true);

                if (!showingNotice)
                {
                    noticePanel.transform.Find("Subtitle").GetComponent<Text>().text = subtitle;
                    noticePanel.transform.Find("Message").GetComponent<Text>().text = message;
                }

                showingNotice = true;

                if (!enableDontShowAgain)
                    noticePanel.transform.Find("DontShowAgainButton").gameObject.SetActive(false);
                else
                    noticePanel.transform.Find("DontShowAgainButton").gameObject.SetActive(true);
            }
        }

        private void CloseNotice(bool dontShowAgain = false)
        {
            PlayClickSound();

            if (settingsManager.DontShowAgainNotices.Contains(noticesToShow[0].Item2) && noticesToShow.Count > 1)
            {
                var _ignoreIfLastOne = noticesToShow[0].Item3;
                noticesToShow.RemoveAt(0);

                for (int i = 0; i < noticesToShow.Count; i++)
                {
                    if(settingsManager.DontShowAgainNotices.Contains(noticesToShow[i].Item2))
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
                    noticePanel.SetActive(false);

                    RemoveExcessDontShowAgainNotices();
                }
            }

            var ignoreIfLastOne = noticesToShow[0].Item3;
            noticesToShow.RemoveAt(0);

            if (dontShowAgain && !settingsManager.DontShowAgainNotices.Contains(noticePanel.transform.Find("Message").GetComponent<Text>().text))
            {
                settingsManager.DontShowAgainNotices.Add(noticePanel.transform.Find("Message").GetComponent<Text>().text);
                settingsManager.SaveSettings(false);
            }

            if (noticesToShow.Count == 0) 
            {
                if(!ignoreIfLastOne)
                    SetObstructRay(false);

                showingNotice = false;
                noticePanel.SetActive(false);

                RemoveExcessDontShowAgainNotices();
            }
            else
            {
                noticePanel.transform.Find("Subtitle").GetComponent<Text>().text = noticesToShow[0].Item1;
                noticePanel.transform.Find("Message").GetComponent<Text>().text = noticesToShow[0].Item2;
            }
        }

        private void RemoveExcessDontShowAgainNotices()
        {
            if (allNotices.Count <= settingsManager.DontShowAgainNotices.Count)
            {
                var toRemove = settingsManager.DontShowAgainNotices.Except(allNotices).ToList();

                foreach (var item in toRemove)
                    settingsManager.DontShowAgainNotices.Remove(item);

                settingsManager.SaveSettings(false);
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
