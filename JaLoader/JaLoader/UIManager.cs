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

        private bool isOnExitPage;
        private bool inOptions;
        private bool inModsOptions;
        private bool isObstructing;
        private bool forceRestrictRay;

        #region Settings Dropdown
        private Dropdown consoleModeDropdown;
        private Dropdown consolePositionDropdown;
        private Dropdown debugModeDropdown;
        private Dropdown menuMusicDropdown;
        private Slider menuMusicSlider;
        private Dropdown uncleDropdown;
        private Dropdown experimentalCCDropdown;
        private Dropdown showModsFolderDropdown;
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
            if (forceRestrictRay)
                FindObjectOfType<MenuMouseInteractionsC>().restrictRay = true;

            if (UIVersionCanvas == null)
                return;

            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                if (!isOnExitPage && !IsBookClosed())
                {
                    UIVersionCanvas.transform.GetChild(0).Find("BookUI").gameObject.SetActive(true);
                }
                else
                {
                    UIVersionCanvas.transform.GetChild(0).Find("BookUI").gameObject.SetActive(false);
                }
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
                if (consoleModeDropdown.transform.Find("Dropdown List") || consolePositionDropdown.transform.Find("Dropdown List") || showModsFolderDropdown.transform.Find("Dropdown List") || debugModeDropdown.transform.Find("Dropdown List") || menuMusicDropdown.transform.Find("Dropdown List") || uncleDropdown.transform.Find("Dropdown List") || experimentalCCDropdown.transform.Find("Dropdown List"))
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
            menuMusicPlayer.AddComponent<RadioVolumeChanger>();
            UpdateMenuMusic(!settingsManager.DisableMenuMusic, (float)settingsManager.MenuMusicVolume / 100);

            book = FindObjectOfType<MainMenuBookC>();

            exitConfirmButton = GameObject.Find("ExitPage").transform.GetChild(0).gameObject;
            isOnExitPage = exitConfirmButton.activeSelf;

            ToggleUIVisibility(true);
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

        IEnumerator LoadUIDelay()
        {
            yield return new WaitForSeconds(0.1f);

            var bundleLoadReq = AssetBundle.LoadFromFileAsync(Path.Combine(settingsManager.ModFolderLocation, @"Required\JaLoader_UI.unity3d"));

            yield return bundleLoadReq;

            AssetBundle ab = bundleLoadReq.assetBundle;

            if (ab == null)
            {
                StopAllCoroutines();

                GameObject notice = Instantiate(GameObject.Find("UI Root").transform.Find("Notice").gameObject);
                forceRestrictRay = true;
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
                notice.transform.GetChild(2).GetComponent<UILabel>().text = "\n\nThe file 'JaLoader_UI.unity3d' was not found. You can try:";
                notice.transform.GetChild(2).GetComponent<UILabel>().height = 550;
                notice.transform.GetChild(2).GetComponent<UILabel>().ProcessText();
                notice.transform.GetChild(4).GetComponent<UILabel>().text = "Reinstalling JaLoader with JaPatcher\n\n\n Copying the file from JaPatcher's directory/Assets to Mods/Required";
                notice.transform.GetChild(4).GetComponent<UILabel>().fontSize = 24;
                notice.transform.GetChild(4).GetComponent<UILabel>().ProcessText();

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

            consoleModeDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Preferences/VerticalLayoutGroup/TopRow/ConsoleMode").gameObject.GetComponent<Dropdown>();
            consolePositionDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Preferences/VerticalLayoutGroup/TopRow/ConsolePosition").gameObject.GetComponent<Dropdown>();
            showModsFolderDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Preferences/VerticalLayoutGroup/TopRow/ShowModsFolderLocation").gameObject.GetComponent<Dropdown>();
            debugModeDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Preferences/VerticalLayoutGroup/MiddleRow/DebugMode").gameObject.GetComponent<Dropdown>();

            menuMusicDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Tweaks/VerticalLayoutGroup/TopRow/MenuMusic").gameObject.GetComponent<Dropdown>();
            menuMusicSlider = UIVersionCanvas.transform.Find("JLSettingsPanel/Tweaks/VerticalLayoutGroup/TopRow/MenuMusicVolume").gameObject.GetComponent<Slider>();
            uncleDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Tweaks/VerticalLayoutGroup/TopRow/Uncle").gameObject.GetComponent<Dropdown>();
            experimentalCCDropdown = UIVersionCanvas.transform.Find("JLSettingsPanel/Tweaks/VerticalLayoutGroup/BottomRow/ExperimentalCC").gameObject.GetComponent<Dropdown>();

            UIVersionCanvas.transform.Find("JLSettingsPanel/Accessibility/VerticalLayoutGroup/TopRow/SwitchLanguage").gameObject.GetComponent<Button>().onClick.AddListener(SwitchLanguage);

            SetOptionsValues();

            Console.Instance.LogMessage("JaLoader", $"JaLoader {settingsManager.GetVersionString()} loaded successfully!");

            StartCoroutine(ReferencesLoader.Instance.LoadAssemblies());

            if (modLoader.IsCrackedVersion)
                ShowNotice("PIRATED GAME DETECTED", "You are using a pirated version of Jalopy.\r\n\r\nYou may encounter issues with certain mods, as well as more bugs in general.\r\n\r\nIf you encounter any game-breaking bugs, feel free to submit them to the official GitHub for JaLoader. Remember to mark them with the \"pirated\" tag!\r\n\r\nHave fun!");

            if (SettingsManager.IsPreReleaseVersion)
                ShowNotice("USING A PRE-RELEASE VERSION OF JALOADER", "You are using a pre-release version of JaLoader.\r\n\r\nThese versions are prone to bugs and may cause issues with certain mods.\r\n\r\nPlease report any bugs you encounter to the JaLoader GitHub page, marking them with the \"pre-release\" tag.\r\n\r\nHave fun!");

            ab.Unload(false);
        }

        private void SetNewspaperText()
        {
            if (Language.CurrentLanguage().Equals(LanguageCode.EN))
            {
                string versionText = GameObject.Find("Newspaper").transform.Find("TextMeshPro").GetComponent<TextMeshPro>().text;
                versionText = Regex.Replace(versionText, @"JALOPY", "");
                versionText = Regex.Replace(versionText, @"\s", "");
                GameObject.Find("Newspaper").transform.Find("TextMeshPro").GetComponent<TextMeshPro>().text = $"JALOPY {versionText}|JALOADER {(SettingsManager.IsPreReleaseVersion ? "PR 0.1" : settingsManager.GetVersionString())}";

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
            debugModeDropdown.value = settingsManager.DebugMode ? 1 : 0;

            menuMusicDropdown.value = settingsManager.DisableMenuMusic ? 1 : 0;
            menuMusicSlider.value = settingsManager.MenuMusicVolume;
            uncleDropdown.value = settingsManager.DisableUncle ? 1 : 0;
            experimentalCCDropdown.value = settingsManager.UseExperimentalCharacterController ? 1 : 0;
        }

        public void UpdateMenuMusic(bool enabled, float volume)
        {
            menuMusicPlayer.SetActive(enabled);
            menuMusicPlayer.GetComponent<RadioVolumeChanger>().volume = volume;
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
            settingsManager.DebugMode = Convert.ToBoolean(debugModeDropdown.value);
            settingsManager.DisableMenuMusic = Convert.ToBoolean(menuMusicDropdown.value);
            settingsManager.MenuMusicVolume = (int)menuMusicSlider.value;
            settingsManager.DisableUncle = Convert.ToBoolean(uncleDropdown.value);
            settingsManager.UseExperimentalCharacterController = Convert.ToBoolean(experimentalCCDropdown.value);

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
}
