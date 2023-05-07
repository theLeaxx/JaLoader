using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JaLoader
{
    class UIManager : MonoBehaviour
    {
        #region Singleton & ToggleUI event on scene change
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

            SceneManager.activeSceneChanged += ToggleUI;

            values = (int[])Enum.GetValues(typeof(KeyCode));
            keys = new bool[values.Length];
        }

        #endregion

        private ModLoader modLoader = ModLoader.Instance;
        private SettingsManager settingsManager = SettingsManager.Instance;

        public GameObject UIVersionCanvas;
        public GameObject modTemplatePrefab;
        public GameObject messageTemplatePrefab;
        public GameObject modConsole;
        private GameObject moreInfoPanelMods;
        private GameObject modSettingsScrollView;
        public GameObject modSettingsScrollViewContent;

        public GameObject modOptionsHolder;
        public GameObject modOptionsNameTemplate;
        public GameObject modOptionsHeaderTemplate;
        public GameObject modOptionsDropdownTemplate;
        public GameObject modOptionsToggleTemplate;
        public GameObject modOptionsSliderTemplate;

        public GameObject modTemplateObject;

        public GameObject modLoaderText;
        public GameObject modFolderText;

        private MainMenuBookC book;
        private GameObject exitConfirmButton;

        private bool isOnExitPage;
        private bool inOptions;
        private bool inModsOptions;
        private bool isObstructing;

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

        private void Update()
        {
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                if (!book.bookClosed && !isOnExitPage)
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

                if (UIVersionCanvas.transform.Find("JMLSettingsPanel").Find("Main").gameObject.activeSelf)
                    ToggleModLoaderSettings_Main();

                RefreshUI();
            }

            //annoying fix for dropdowns only working once
            if (inOptions && Input.GetMouseButtonDown(0))
            {
                if(consoleModeDropdown.transform.Find("Dropdown List") || consolePositionDropdown.transform.Find("Dropdown List") || showModsFolderDropdown.transform.Find("Dropdown List") ||debugModeDropdown.transform.Find("Dropdown List") || menuMusicDropdown.transform.Find("Dropdown List") || uncleDropdown.transform.Find("Dropdown List") || experimentalCCDropdown.transform.Find("Dropdown List"))
                    RefreshUI();
            }

            if (inModsOptions && Input.GetMouseButtonDown(0))
            {
                foreach (RectTransform item in modSettingsScrollViewContent.transform.GetComponentsInChildren<RectTransform>())
                {
                    if(item.gameObject.name == "Dropdown List" && item.parent.gameObject.activeSelf)
                        RefreshUI();
                }
            }
        }

        public void ToggleUI(Scene current, Scene next)
        {
            if (UIVersionCanvas == null)
                StartCoroutine(LoadUIDelay());

            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                SetNewspaperText();

                menuMusicPlayer = GameObject.Find("RadioFreq");
                menuMusicPlayer.AddComponent<RadioVolumeChanger>();
                UpdateMenuMusic(!settingsManager.DisableMenuMusic, (float)settingsManager.MenuMusicVolume / 100);

                book = FindObjectOfType<MainMenuBookC>();
                exitConfirmButton = GameObject.Find("ExitPage").transform.GetChild(0).gameObject;
                isOnExitPage = exitConfirmButton.activeSelf;
            }

            if (SceneManager.GetActiveScene().buildIndex > 1 || SceneManager.GetActiveScene().buildIndex == 0)
            {
                Console.Instance.ToggleVisibility(false);

                if (inOptions)
                {
                    inOptions = false;
                    UIVersionCanvas.transform.GetChild(2).transform.GetChild(0).gameObject.SetActive(false);
                    UIVersionCanvas.transform.GetChild(2).transform.GetChild(1).gameObject.SetActive(false);
                    UIVersionCanvas.transform.GetChild(2).transform.GetChild(2).gameObject.SetActive(false);
                    UIVersionCanvas.transform.GetChild(2).transform.GetChild(3).gameObject.SetActive(false);
                }

                ToggleUIVisibility(false);
            }
            else
                ToggleUIVisibility(true);
        }

        public void ToggleUIVisibility(bool show)
        {
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
                FindObjectOfType<MenuMouseInteractionsC>().restrictRay = true;
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
                notice.transform.GetChild(2).GetComponent<UILabel>().text = "\n\nThe file 'JaLoader_UI.unity3d' was not found. You can try:";
                notice.transform.GetChild(2).GetComponent<UILabel>().height = 550;
                notice.transform.GetChild(2).GetComponent<UILabel>().ProcessText();
                notice.transform.GetChild(3).GetComponent<UILabel>().text = "\nWHAT WENT WRONG";
                notice.transform.GetChild(3).GetComponent<UILabel>().ProcessText();
                notice.transform.GetChild(4).GetComponent<UILabel>().text = "Reinstalling JaLoader with JaPatcher\n\n\n Copying the file from JaPatcher's directory/Assets to Mods/Required";
                notice.transform.GetChild(4).GetComponent<UILabel>().fontSize = 24;
                notice.transform.GetChild(4).GetComponent<UILabel>().ProcessText();

                yield break;
            }

            var assetLoadRequest = ab.LoadAssetAsync<GameObject>("JMLCanvas.prefab");
            yield return assetLoadRequest;

            var UIPrefab = assetLoadRequest.asset as GameObject;

            UIVersionCanvas = Instantiate(UIPrefab);
            DontDestroyOnLoad(UIVersionCanvas);

            modTemplatePrefab = UIVersionCanvas.transform.Find("JMLModsPanel").Find("Scroll View").GetChild(0).GetChild(0).GetChild(0).gameObject;
            modConsole = UIVersionCanvas.transform.Find("JMLConsole").Find("Console").gameObject;
            messageTemplatePrefab = modConsole.transform.Find("Scroll View").Find("Viewport").Find("Content").GetChild(0).gameObject;
            moreInfoPanelMods = UIVersionCanvas.transform.Find("JMLModsPanel").Find("MoreInfo").gameObject;

            GameObject consoleObj = Instantiate(new GameObject());
            consoleObj.AddComponent<Console>();
            consoleObj.name = "ModConsole";
            DontDestroyOnLoad(consoleObj);

            modLoaderText = UIVersionCanvas.transform.GetChild(0).Find("JModLoader").gameObject;
            modFolderText = UIVersionCanvas.transform.GetChild(0).Find("ModsFolder").gameObject;

            if (settingsManager.HideModFolderLocation)
                modFolderText.SetActive(false);

            modLoaderText.GetComponent<Text>().text = $"JaLoader <color=yellow>a_{settingsManager.Version}</color> loaded!";
            modFolderText.GetComponent<Text>().text = $"Mods folder: <color=yellow>{settingsManager.ModFolderLocation}</color>";

            UIVersionCanvas.transform.Find("JMLPanel/BookUI/ModsButton").GetComponent<Button>().onClick.AddListener(ToggleModMenu);
            UIVersionCanvas.transform.Find("JMLModsPanel/ExitButton").GetComponent<Button>().onClick.AddListener(ToggleModMenu);

            UIVersionCanvas.transform.Find("JMLPanel/BookUI/OptionsButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Main);
            UIVersionCanvas.transform.Find("JMLSettingsPanel/Main/ExitButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Main);

            UIVersionCanvas.transform.Find("JMLSettingsPanel/Main/VerticalButtonLayoutGroup/PreferencesButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Preferences);
            UIVersionCanvas.transform.Find("JMLSettingsPanel/Main/VerticalButtonLayoutGroup/TweaksButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Tweaks);
            UIVersionCanvas.transform.Find("JMLSettingsPanel/Main/VerticalButtonLayoutGroup/AccessibilityButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Accessibility);

            UIVersionCanvas.transform.Find("JMLSettingsPanel/Preferences/BackButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Preferences);
            UIVersionCanvas.transform.Find("JMLSettingsPanel/Tweaks/BackButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Tweaks);
            UIVersionCanvas.transform.Find("JMLSettingsPanel/Accessibility/BackButton").GetComponent<Button>().onClick.AddListener(ToggleModLoaderSettings_Accessibility);

            UIVersionCanvas.transform.Find("JMLSettingsPanel/Preferences/SaveButton").GetComponent<Button>().onClick.AddListener(SaveValues);
            UIVersionCanvas.transform.Find("JMLSettingsPanel/Tweaks/SaveButton").GetComponent<Button>().onClick.AddListener(SaveValues);
            UIVersionCanvas.transform.Find("JMLSettingsPanel/Accessibility/SaveButton").GetComponent<Button>().onClick.AddListener(SaveValues);

            modSettingsScrollView = UIVersionCanvas.transform.Find("JMLModsPanel/SettingsScrollView").gameObject;
            modSettingsScrollViewContent = modSettingsScrollView.transform.GetChild(0).GetChild(0).gameObject;

            modSettingsScrollView.transform.Find("SaveButton").GetComponent<Button>().onClick.AddListener(SaveModSettings);

            modOptionsHolder = modSettingsScrollViewContent.transform.Find("SettingsHolder").gameObject;
            modOptionsNameTemplate = modSettingsScrollViewContent.transform.Find("ModName").gameObject;
            modOptionsHeaderTemplate = modSettingsScrollViewContent.transform.Find("HeaderTemplate").gameObject;
            modOptionsDropdownTemplate = modSettingsScrollViewContent.transform.Find("DropdownTemplate").gameObject;
            modOptionsToggleTemplate = modSettingsScrollViewContent.transform.Find("ToggleTemplate").gameObject;
            modOptionsSliderTemplate = modSettingsScrollViewContent.transform.Find("SliderTemplate").gameObject;

            consoleModeDropdown = UIVersionCanvas.transform.Find("JMLSettingsPanel/Preferences/VerticalLayoutGroup/TopRow/ConsoleMode").gameObject.GetComponent<Dropdown>();
            consolePositionDropdown = UIVersionCanvas.transform.Find("JMLSettingsPanel/Preferences/VerticalLayoutGroup/TopRow/ConsolePosition").gameObject.GetComponent<Dropdown>();
            showModsFolderDropdown = UIVersionCanvas.transform.Find("JMLSettingsPanel/Preferences/VerticalLayoutGroup/TopRow/ShowModsFolderLocation").gameObject.GetComponent<Dropdown>();
            debugModeDropdown = UIVersionCanvas.transform.Find("JMLSettingsPanel/Preferences/VerticalLayoutGroup/MiddleRow/DebugMode").gameObject.GetComponent<Dropdown>();

            menuMusicDropdown = UIVersionCanvas.transform.Find("JMLSettingsPanel").Find("Tweaks").Find("VerticalLayoutGroup").Find("TopRow").Find("MenuMusic").gameObject.GetComponent<Dropdown>();
            menuMusicSlider = UIVersionCanvas.transform.Find("JMLSettingsPanel").Find("Tweaks").Find("VerticalLayoutGroup").Find("TopRow").Find("MenuMusicVolume").gameObject.GetComponent<Slider>();
            uncleDropdown = UIVersionCanvas.transform.Find("JMLSettingsPanel").Find("Tweaks").Find("VerticalLayoutGroup").Find("TopRow").Find("Uncle").gameObject.GetComponent<Dropdown>();
            experimentalCCDropdown = UIVersionCanvas.transform.Find("JMLSettingsPanel").Find("Tweaks").Find("VerticalLayoutGroup").Find("BottomRow").Find("ExperimentalCC").gameObject.GetComponent<Dropdown>();

            UIVersionCanvas.transform.Find("JMLSettingsPanel").Find("Accessibility").Find("VerticalLayoutGroup").Find("TopRow").Find("SwitchLanguage").gameObject.GetComponent<Button>().onClick.AddListener(SwitchLanguage);

            SetOptionsValues();

            Console.Instance.LogMessage("JaLoader", $"JaLoader a_{settingsManager.Version} loaded successfully!");

            StartCoroutine(modLoader.LoadMods());

            ab.Unload(false);
        }

        private void SetNewspaperText()
        {
            if (Language.CurrentLanguage().Equals(LanguageCode.EN))
            {
                Component component = GameObject.Find("Newspaper").transform.Find("TextMeshPro").GetComponents<Component>()[3];
                string versionText = (string)component.GetType().GetProperty("text").GetValue(component, null);
                versionText = System.Text.RegularExpressions.Regex.Replace(versionText, @"JALOPY", "");
                versionText = System.Text.RegularExpressions.Regex.Replace(versionText, @"\s", "");
                component.GetType().GetProperty("text").SetValue(component, $"JALOPY {versionText}|JALOADER A_{settingsManager.Version}", null);
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
                {
                    moreInfoPanelMods.transform.Find("ModDescription").GetComponent<Text>().text = description;
                }
                else
                {
                    moreInfoPanelMods.transform.Find("ModDescription").GetComponent<Text>().text = "This mod does not have a description.";
                }
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
                {
                    item.gameObject.SetActive(false);
                }

                modSettingsScrollViewContent.transform.Find(objName).gameObject.SetActive(true);
            }
            else
            {
                foreach (Transform item in modSettingsScrollViewContent.transform)
                {
                    item.gameObject.SetActive(false);
                }

                modSettingsScrollViewContent.transform.Find("NoSettings").gameObject.SetActive(true);
            }
        }

        public void ToggleModMenu()
        {
            if (!book.bookClosed)
            {
                book.CloseBook();
            }

            UIVersionCanvas.transform.GetChild(1).gameObject.SetActive(!UIVersionCanvas.transform.GetChild(1).gameObject.activeSelf);
            ToggleObstructRay();
        }

        public void ToggleModLoaderSettings_Main()
        {
            if (!book.bookClosed)
            {
                book.CloseBook();
            }

            inOptions = !inOptions;
            //UIVersionCanvas.transform.Find("JMLSettingsPanel").gameObject.SetActive(!UIVersionCanvas.transform.Find("JMLSettingsPanel").gameObject.activeSelf);
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
            {
                Console.Instance.ToggleVisibility(false);
            }

            Console.Instance.SetPosition(consolePosition);
            modFolderText.SetActive(!settingsManager.HideModFolderLocation);

            UpdateMenuMusic(!Convert.ToBoolean(menuMusicDropdown.value), (float)menuMusicSlider.value / 100);
        }
    }
}
