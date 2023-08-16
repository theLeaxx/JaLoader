using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

            values = (int[])Enum.GetValues(typeof(KeyCode));
            keys = new bool[values.Length];
        }

        #endregion

        #region Declarations

        private readonly ModLoader modLoader = ModLoader.Instance;
        private readonly SettingsManager settingsManager = SettingsManager.Instance;

        public GameObject UIVersionCanvas {get; private set;}
        public GameObject modTemplatePrefab { get; private set; }
        public GameObject messageTemplatePrefab { get; private set; }
        public GameObject modConsole { get; private set; }
        private GameObject moreInfoPanelMods;
        private GameObject modSettingsScrollView;
        public GameObject modSettingsScrollViewContent { get; private set; }

        public GameObject modOptionsHolder { get; private set; }
        public GameObject modOptionsNameTemplate { get; private set; }
        public GameObject modOptionsHeaderTemplate { get; private set; }
        public GameObject modOptionsDropdownTemplate { get; private set; }
        public GameObject modOptionsToggleTemplate { get; private set; }
        public GameObject modOptionsSliderTemplate { get; private set; }

        private GameObject noticePanel;
        public GameObject modTemplateObject;

        public GameObject catalogueTemplate { get; private set; }
        public GameObject catalogueEntryTemplate { get; private set; }

        public GameObject modLoaderText { get; private set; }
        public GameObject modFolderText { get; private set; }

        private MainMenuBookC book;
        private GameObject exitConfirmButton;
        private GameObject newGameConfirmButton;

        private bool isOnOtherPage;
        private bool inOptions;
        private bool inModsOptions;
        private bool isObstructing;

        #region Settings Dropdown
        // Preferences tab
        private Dropdown consoleModeDropdown;
        private Dropdown consolePositionDropdown;
        private Dropdown showModsFolderDropdown;
        private Dropdown skipLanguageSelectionDropdown;
        private Dropdown discordRichPresenceDropdown;
        private Dropdown debugModeDropdown;

        // Tweaks tab
        private Dropdown menuMusicDropdown;
        private Slider menuMusicSlider;
        private Dropdown songsDropdown;
        private Dropdown uncleDropdown;
        private Dropdown enhancedMovementDropdown;
        private Dropdown changeLicensePlateTextDropdown;
        private InputField licensePlateTextField;
        #endregion

        private GameObject menuMusicPlayer;

        #endregion

        private bool IsBookClosed()
        {
            if (book == null)
                return true;

            return (bool)book.GetType().GetField("bookClosed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(book);
        }

        private void Update()
        {
            if (UIVersionCanvas == null)
                return;

            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                if (!isOnOtherPage && !IsBookClosed())
                {
                    UIVersionCanvas.transform.GetChild(0).Find("BookUI").gameObject.SetActive(true);
                }
                else
                {
                    UIVersionCanvas.transform.GetChild(0).Find("BookUI").gameObject.SetActive(false);
                }

                if (exitConfirmButton.activeSelf || newGameConfirmButton.activeSelf)
                    isOnOtherPage = true;
                else
                    isOnOtherPage = false;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (UIVersionCanvas.transform.GetChild(1).gameObject.activeSelf)
                    ToggleModMenu();

                if (UIVersionCanvas.transform.Find("JLSettingsPanel").Find("Main").gameObject.activeSelf)
                    ToggleModLoaderSettings_Main();

                RefreshUI();
            }

            //annoying fix for dropdowns only working once
            if (inOptions && Input.GetMouseButtonDown(0))
                if (consoleModeDropdown.transform.Find("Dropdown List") || consolePositionDropdown.transform.Find("Dropdown List") || showModsFolderDropdown.transform.Find("Dropdown List") || debugModeDropdown.transform.Find("Dropdown List") || menuMusicDropdown.transform.Find("Dropdown List") || uncleDropdown.transform.Find("Dropdown List") || songsDropdown.transform.Find("Dropdown List") || skipLanguageSelectionDropdown.transform.Find("Dropdown List") || discordRichPresenceDropdown.transform.Find("Dropdown List") || changeLicensePlateTextDropdown.transform.Find("Dropdown List") || enhancedMovementDropdown.transform.Find("Dropdown List"))
                    RefreshUI();

            if (inModsOptions && Input.GetMouseButtonDown(0))
                foreach (RectTransform item in modSettingsScrollViewContent.transform.GetComponentsInChildren<RectTransform>())
                    if (item.gameObject.name == "Dropdown List" && item.parent.gameObject.activeSelf)
                        RefreshUI();
        }

        private void OnMenuLoad()
        {
            if (UIVersionCanvas == null)
                StartCoroutine(LoadUIDelay());

            SetNewspaperText();

            menuMusicPlayer = GameObject.Find("RadioFreq");
            menuMusicPlayer.AddComponent<MenuVolumeChanger>();
            UpdateMenuMusic(!settingsManager.DisableMenuMusic, (float)settingsManager.MenuMusicVolume / 100);

            book = FindObjectOfType<MainMenuBookC>();

            exitConfirmButton = GameObject.Find("ExitPage").transform.GetChild(0).gameObject;
            newGameConfirmButton = GameObject.Find("New Game").transform.GetChild(1).gameObject;
            var skipConfirmButton = GameObject.Find("New Game").transform.GetChild(4).gameObject;

            newGameConfirmButton.GetComponent<UIButton>().onClick.Add(new EventDelegate(CustomObjectsManager.Instance, "DeleteData"));
            skipConfirmButton.GetComponent<UIButton>().onClick.Add(new EventDelegate(CustomObjectsManager.Instance, "DeleteData"));

            ToggleUIVisibility(true);

            var go = new GameObject();
            go.AddComponent<MenuCarRotate>();
        }

        private void OnLoadStart()
        {
            ToggleUIVisibility(false);
            Console.Instance.ToggleVisibility(false);

            if (inOptions)
            {
                inOptions = false;
                UIVersionCanvas.transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(false);
                UIVersionCanvas.transform.GetChild(2).transform.GetChild(1).gameObject.SetActive(false);
                UIVersionCanvas.transform.GetChild(2).transform.GetChild(2).gameObject.SetActive(false);
                UIVersionCanvas.transform.GetChild(2).transform.GetChild(3).gameObject.SetActive(false);
            }
        }

        public void ToggleUIVisibility(bool show)
        {
            if (UIVersionCanvas == null)
                return;

            RefreshUI();
            UIVersionCanvas.transform.GetChild(0).gameObject.SetActive(show);
        }

        private IEnumerator LoadUIDelay()
        {
            Debug.Log("Loading JaLoader UI...");

            yield return new WaitForSeconds(0.1f);

            var bundleLoadReq = AssetBundle.LoadFromFileAsync(Path.Combine(settingsManager.ModFolderLocation, @"Required\JaLoader_UI.unity3d"));

            yield return bundleLoadReq;

            AssetBundle ab = bundleLoadReq.assetBundle;

            if (ab == null)
            {
                StopAllCoroutines();

                modLoader.CreateImportantNotice("\n\nThe file 'JaLoader_UI.unity3d' was not found. You can try:", "Reinstalling JaLoader with JaPatcher\n\n\nCopying the file from JaPatcher's directory/Assets/Required to Mods/Required");
                Destroy(GameObject.Find("JaLoader Modding Helpers"));
                Destroy(gameObject);

                yield break;
            }

            var assetLoadRequest = ab.LoadAssetAsync<GameObject>("JLCanvas.prefab");

            yield return assetLoadRequest;

            var UIPrefab = assetLoadRequest.asset as GameObject;

            UIVersionCanvas = Instantiate(UIPrefab);
            DontDestroyOnLoad(UIVersionCanvas);

            modTemplatePrefab = UIVersionCanvas.transform.Find("JLModsPanel/Scroll View").GetChild(0).GetChild(0).GetChild(0).gameObject;
            modConsole = UIVersionCanvas.transform.Find("JLConsole/Console").gameObject;
            messageTemplatePrefab = modConsole.transform.Find("Scroll View/Viewport/Content").GetChild(0).gameObject;
            moreInfoPanelMods = UIVersionCanvas.transform.Find("JLModsPanel/MoreInfo").gameObject;
            noticePanel = UIVersionCanvas.transform.Find("JLNotice").gameObject;
            catalogueTemplate = UIVersionCanvas.transform.Find("JLCatalogue/MainTemplate").gameObject;
            catalogueEntryTemplate = catalogueTemplate.transform.Find("Viewport/Content/Template").gameObject;

            GameObject consoleObj = Instantiate(new GameObject());
            consoleObj.AddComponent<Console>();
            consoleObj.name = "ModConsole";
            DontDestroyOnLoad(consoleObj);

            modLoaderText = UIVersionCanvas.transform.GetChild(0).Find("JaLoader").gameObject;
            modFolderText = UIVersionCanvas.transform.GetChild(0).Find("ModsFolder").gameObject;

            if (settingsManager.HideModFolderLocation)
                modFolderText.SetActive(false);

            string version = ModHelper.Instance.GetLatestTagFromApiUrl("https://api.github.com/repos/theLeaxx/JaLoader/releases/latest");
            int versionInt = int.Parse(version.Replace(".", ""));

            if (versionInt > settingsManager.GetVersion())
                modLoaderText.GetComponent<Text>().text = $"JaLoader <color={(SettingsManager.IsPreReleaseVersion ? "red" : "yellow")}>{settingsManager.GetVersionString()}</color> loaded! (<color=lime>{version} available!</color>)";
            else
                modLoaderText.GetComponent<Text>().text = $"JaLoader <color={(SettingsManager.IsPreReleaseVersion ? "red" : "yellow")}>{settingsManager.GetVersionString()}</color> loaded!";

            modFolderText.GetComponent<Text>().text = $"Mods folder: <color=yellow>{settingsManager.ModFolderLocation}</color>";

            UIVersionCanvas.transform.Find("JLPanel/BookUI/ModsButton").GetComponent<Button>().onClick.AddListener(ToggleModMenu);
            UIVersionCanvas.transform.Find("JLModsPanel/ExitButton").GetComponent<Button>().onClick.AddListener(ToggleModMenu);

            UIVersionCanvas.transform.Find("JLPanel/BookUI/OptionsButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Main);
            UIVersionCanvas.transform.Find("JLSettingsPanel/Main/ExitButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Main);

            UIVersionCanvas.transform.Find("JLSettingsPanel/Main/VerticalButtonLayoutGroup/PreferencesButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Preferences);
            UIVersionCanvas.transform.Find("JLSettingsPanel/Main/VerticalButtonLayoutGroup/TweaksButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Tweaks);
            UIVersionCanvas.transform.Find("JLSettingsPanel/Main/VerticalButtonLayoutGroup/AccessibilityButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Accessibility);

            UIVersionCanvas.transform.Find("JLSettingsPanel/Preferences/BackButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Preferences);
            UIVersionCanvas.transform.Find("JLSettingsPanel/Tweaks/BackButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Tweaks);
            UIVersionCanvas.transform.Find("JLSettingsPanel/Accessibility/BackButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Accessibility);

            UIVersionCanvas.transform.Find("JLSettingsPanel/Preferences/SaveButton").GetComponent<Button>().onClick.AddListener(SaveValues);
            UIVersionCanvas.transform.Find("JLSettingsPanel/Tweaks/SaveButton").GetComponent<Button>().onClick.AddListener(SaveValues);
            UIVersionCanvas.transform.Find("JLSettingsPanel/Accessibility/SaveButton").GetComponent<Button>().onClick.AddListener(SaveValues);

            noticePanel.transform.Find("UnderstandButton").GetComponent<Button>().onClick.AddListener(CloseNotice);

            modSettingsScrollView = UIVersionCanvas.transform.Find("JLModsPanel/SettingsScrollView").gameObject;
            modSettingsScrollViewContent = modSettingsScrollView.transform.GetChild(0).GetChild(0).gameObject;

            modSettingsScrollView.transform.Find("SaveButton").GetComponent<Button>().onClick.AddListener(SaveModSettings);

            modOptionsHolder = modSettingsScrollViewContent.transform.Find("SettingsHolder").gameObject;
            modOptionsNameTemplate = modSettingsScrollViewContent.transform.Find("ModName").gameObject;
            modOptionsHeaderTemplate = modSettingsScrollViewContent.transform.Find("HeaderTemplate").gameObject;
            modOptionsDropdownTemplate = modSettingsScrollViewContent.transform.Find("DropdownTemplate").gameObject;
            modOptionsToggleTemplate = modSettingsScrollViewContent.transform.Find("ToggleTemplate").gameObject;
            modOptionsSliderTemplate = modSettingsScrollViewContent.transform.Find("SliderTemplate").gameObject;

            consoleModeDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Preferences/Scroll View/Viewport/Content/Row1/ConsoleMode").gameObject.GetComponent<Dropdown>();
            consolePositionDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Preferences/Scroll View/Viewport/Content/Row1/ConsolePosition").gameObject.GetComponent<Dropdown>();
            showModsFolderDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Preferences/Scroll View/Viewport/Content/Row1/ShowModsFolderLocation").gameObject.GetComponent<Dropdown>();
            skipLanguageSelectionDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Preferences/Scroll View/Viewport/Content/Row2/SkipLanguageSelectionScreen").gameObject.GetComponent<Dropdown>();
            discordRichPresenceDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Preferences/Scroll View/Viewport/Content/Row2/DiscordRichPresence").gameObject.GetComponent<Dropdown>();
            debugModeDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Preferences/Scroll View/Viewport/Content/Row3/DebugMode").gameObject.GetComponent<Dropdown>();

            menuMusicDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row1/MenuMusic").gameObject.GetComponent<Dropdown>();
            menuMusicSlider = UIVersionCanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row1/MenuMusicVolume").gameObject.GetComponent<Slider>();
            songsDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row2/CustomSongs").gameObject.GetComponent<Dropdown>();
            uncleDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row2/Uncle").gameObject.GetComponent<Dropdown>();
            enhancedMovementDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row2/UseEnhancedMovement").gameObject.GetComponent<Dropdown>();
            changeLicensePlateTextDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row3/ChangeLicensePlate").gameObject.GetComponent<Dropdown>();
            licensePlateTextField = UIVersionCanvas.transform.Find("JLSettingsPanel/Tweaks/Scroll View/Viewport/Content/Row3/LicensePlateText/InputField").gameObject.GetComponent<InputField>();

            UIVersionCanvas.transform.Find("JLSettingsPanel/Accessibility/VerticalLayoutGroup/TopRow/SwitchLanguage").gameObject.GetComponent<Button>().onClick.AddListener(SwitchLanguage);

            SetOptionsValues();

            Console.Instance.LogMessage("JaLoader", $"JaLoader {settingsManager.GetVersionString()} loaded successfully!");
            Debug.Log("Loaded JaLoader UI!");

            StartCoroutine(ReferencesLoader.Instance.LoadAssemblies());

            if (modLoader.IsCrackedVersion)
                ShowNotice("PIRATED GAME DETECTED", "You are using a pirated version of Jalopy.\r\n\r\nYou may encounter issues with certain mods, as well as more bugs in general.\r\n\r\nIf you encounter any game-breaking bugs, feel free to submit them to the official GitHub for JaLoader. Remember to mark them with the \"pirated\" tag!\r\n\r\nHave fun!");

            if (SettingsManager.IsPreReleaseVersion)
                ShowNotice("USING A PRE-RELEASE VERSION OF JALOADER", "You are using a pre-release version of JaLoader.\r\n\r\nThese versions are prone to bugs and may cause issues with certain mods.\r\n\r\nPlease report any bugs you encounter to the JaLoader GitHub page, marking them with the \"pre-release\" tag.\r\n\r\nHave fun!");

            AddWrench();

            ab.Unload(false);
        }

        private void AddWrench()
        {
            var toolBox = GameObject.Find("ToolBox");
            var wrench = toolBox.transform.Find("Cylinder_254");
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

        private void SaveModSettings()
        {
            for (int i = 0; i < modSettingsScrollViewContent.transform.childCount; i++)
            {
                if (modSettingsScrollViewContent.transform.GetChild(i).gameObject.activeSelf && Regex.Match(modSettingsScrollViewContent.transform.GetChild(i).gameObject.name, @"(.{15})\s*$").ToString() == "-SettingsHolder")
                {
                    string fullName = modSettingsScrollViewContent.transform.GetChild(i).gameObject.name;
                    string nameWithSpaces = Regex.Replace(fullName, "_", @" ");
                    nameWithSpaces = Regex.Replace(nameWithSpaces, Regex.Match(modSettingsScrollViewContent.transform.GetChild(i).gameObject.name, @"(.{15})\s*$").ToString(), @" ");

                    Regex r = new Regex(@"\s+([^\s]+)");
                    Match m = r.Match(nameWithSpaces);

                    string modAuthor = Regex.Match(nameWithSpaces, @"^([\w\-]+)").Value;
                    string modID = m.Groups[1].Value;
                    string modName = Regex.Replace(Regex.Replace(nameWithSpaces, modID, @" "), modAuthor, @" ").TrimStart().TrimEnd();

                    modLoader.FindMod(modAuthor, modID, modName).SaveModSettings();
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
            enhancedMovementDropdown.value = settingsManager.UseExperimentalCharacterController ? 1 : 0;
            changeLicensePlateTextDropdown.value = (int)settingsManager.ChangeLicensePlateText;
            licensePlateTextField.text = settingsManager.LicensePlateText;
        }

        public void UpdateMenuMusic(bool enabled, float volume)
        {
            menuMusicPlayer.SetActive(enabled);
            menuMusicPlayer.GetComponent<MenuVolumeChanger>().volume = volume;
        }

        private void SwitchLanguage()
        {
            SaveValues();

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
            isObstructing = false;
        }

        private void RefreshUI()
        {
            if (UIVersionCanvas == null)
                return;

            UIVersionCanvas.SetActive(false);
            UIVersionCanvas.SetActive(true);
        }

        public void ToggleMoreInfo(string name, string author, string version, string description)
        {
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
                moreInfoPanelMods.transform.Find("ModDescription").GetComponent<Text>().text = "You can enable or disable mods or see more information about them!";
            }
        }

        public void ToggleSettings(string objName)
        {
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

            UIVersionCanvas.transform.GetChild(1).gameObject.SetActive(!UIVersionCanvas.transform.GetChild(1).gameObject.activeSelf);
            ToggleObstructRay();
        }

        public void ToggleModLoaderSettings_Main()
        {
            if (!IsBookClosed())
                book.CloseBook();

            inOptions = !inOptions;
            UIVersionCanvas.transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(!UIVersionCanvas.transform.GetChild(2).transform.GetChild(0).gameObject.activeSelf);
            ToggleObstructRay();
        }

        public void ToggleModLoaderSettings_Preferences()
        {
            UIVersionCanvas.transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(!UIVersionCanvas.transform.GetChild(2).transform.GetChild(0).gameObject.activeSelf);
            UIVersionCanvas.transform.GetChild(2).transform.GetChild(1).gameObject.SetActive(!UIVersionCanvas.transform.GetChild(2).transform.GetChild(1).gameObject.activeSelf);
        }

        public void ToggleModLoaderSettings_Tweaks()
        {
            UIVersionCanvas.transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(!UIVersionCanvas.transform.GetChild(2).transform.GetChild(0).gameObject.activeSelf);
            UIVersionCanvas.transform.GetChild(2).transform.GetChild(2).gameObject.SetActive(!UIVersionCanvas.transform.GetChild(2).transform.GetChild(2).gameObject.activeSelf);
        }

        public void ToggleModLoaderSettings_Accessibility()
        {
            UIVersionCanvas.transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(!UIVersionCanvas.transform.GetChild(2).transform.GetChild(0).gameObject.activeSelf);
            UIVersionCanvas.transform.GetChild(2).transform.GetChild(3).gameObject.SetActive(!UIVersionCanvas.transform.GetChild(2).transform.GetChild(3).gameObject.activeSelf);
        }

        private void ToggleObstructRay()
        {
            isObstructing = !isObstructing;
            FindObjectOfType<MenuMouseInteractionsC>().restrictRay = isObstructing;
        }

        private void SetObstructRay(bool value)
        {
            isObstructing = value;
            FindObjectOfType<MenuMouseInteractionsC>().restrictRay = value;
        }

        private void SaveValues()
        {
            ConsoleModes consoleMode = (ConsoleModes)consoleModeDropdown.value;
            ConsolePositions consolePosition = (ConsolePositions)consolePositionDropdown.GetComponent<Dropdown>().value;

            settingsManager.ConsoleMode = consoleMode;
            settingsManager.ConsolePosition = consolePosition;
            settingsManager.HideModFolderLocation = Convert.ToBoolean(showModsFolderDropdown.value);
            settingsManager.DebugMode = !Convert.ToBoolean(debugModeDropdown.value);
            settingsManager.DisableMenuMusic = Convert.ToBoolean(menuMusicDropdown.value);
            settingsManager.MenuMusicVolume = (int)menuMusicSlider.value;
            settingsManager.DisableUncle = Convert.ToBoolean(uncleDropdown.value);
            settingsManager.UseCustomSongs = !Convert.ToBoolean(songsDropdown.value);
            settingsManager.UseExperimentalCharacterController = Convert.ToBoolean(enhancedMovementDropdown.value);
            settingsManager.SkipLanguage = !Convert.ToBoolean(skipLanguageSelectionDropdown.value);
            settingsManager.UseDiscordRichPresence = !Convert.ToBoolean(discordRichPresenceDropdown.value);
            settingsManager.ChangeLicensePlateText = (LicensePlateStyles)changeLicensePlateTextDropdown.value;
            settingsManager.LicensePlateText = licensePlateTextField.text;

            settingsManager.SaveSettings();

            if (consoleMode == ConsoleModes.Disabled)
                Console.Instance.ToggleVisibility(false);

            Console.Instance.SetPosition(consolePosition);
            modFolderText.SetActive(!settingsManager.HideModFolderLocation);

            UpdateMenuMusic(!Convert.ToBoolean(menuMusicDropdown.value), (float)menuMusicSlider.value / 100);

            UncleHelper.Instance.UncleEnabled = !settingsManager.DisableUncle;
        }

        private List<(string, string)> noticesToShow = new List<(string, string)>();
        private bool showingNotice;

        private IEnumerator ShowNoticeAfterLoad(string subtitle, string message)
        {
            while (noticePanel == null)
                yield return null;

            ShowNotice(subtitle, message);
        }

        private void ShowNotice(string subtitle, string message)
        {
            noticesToShow.Add((subtitle, message));

            noticePanel.SetActive(true);

            SetObstructRay(true);

            if (!showingNotice)
            {
                noticePanel.transform.Find("Subtitle").GetComponent<Text>().text = subtitle;
                noticePanel.transform.Find("Message").GetComponent<Text>().text = message;
            }

            showingNotice = true;
        }

        private void CloseNotice()
        {
            noticesToShow.RemoveAt(0);

            if(noticesToShow.Count == 0) 
            {
                 SetObstructRay(false);
                showingNotice = false;
                noticePanel.SetActive(false);
            }
            else
            {
                noticePanel.transform.Find("Subtitle").GetComponent<Text>().text = noticesToShow[0].Item1;
                noticePanel.transform.Find("Message").GetComponent<Text>().text = noticesToShow[0].Item2;
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
