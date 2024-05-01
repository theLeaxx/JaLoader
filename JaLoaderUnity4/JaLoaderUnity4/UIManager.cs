using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;
using Application = UnityEngine.Application;

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

            //EventsManager.Instance.OnMenuLoad += OnMenuLoad;
            //EventsManager.Instance.OnLoadStart += OnLoadStart;
            //EventsManager.Instance.OnGameLoad += OnGameLoad;

            values = (int[])Enum.GetValues(typeof(KeyCode));
            keys = new bool[values.Length];
        }

        #endregion

        #region Declarations

        private readonly ModLoader modLoader = ModLoader.Instance;
        //private readonly SettingsManager settingsManager = SettingsManager.Instance;

        public GameObject UICanvas { get; private set; }
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
        public GameObject modOptionsKeybindTemplate { get; private set; }

        public GameObject objectsList { get; private set; }
        public GameObject objectTemplate { get; private set; }
        //public Text currentlySelectedText { get; private set; }

        private GameObject noticePanel;
        public GameObject modTemplateObject;

        public GameObject catalogueTemplate { get; private set; }
        public GameObject catalogueEntryTemplate { get; private set; }

        public GameObject modLoaderText { get; private set; }
        public GameObject modFolderText { get; private set; }
        public GameObject fpsText { get; private set; }
        public GameObject debugText { get; private set; }

        private MainMenuBook book;
        private GameObject exitConfirmButton;
        private GameObject newGameConfirmButton;

        private bool isOnOtherPage;
        private bool inOptions;
        private bool inModsOptions;
        private bool isObstructing;

        #region Settings Dropdown
        // Preferences tab
        /*private Dropdown consoleModeDropdown;
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
        private Dropdown showFPSDropdown;*/
        #endregion

        #endregion

        private GameObject menuMusicPlayer;

        private Rect buttonRect;

        private bool buttonPressed;

        private void Start()
        {
            float halfScreenWidth = Screen.width / 2f;
            float halfScreenHeight = Screen.height / 2f;

            float buttonWidth = 250f;
            float buttonHeight = 25f;

            float buttonX = halfScreenWidth - buttonWidth / 2f;
            float buttonY = halfScreenHeight - buttonHeight / 2f - 300;

            buttonRect = new Rect(buttonX, buttonY, buttonWidth, buttonHeight);

        }

        private void OnGUI()
        {
            if (Application.loadedLevel != 1)
                return;

            if (GUI.Button(buttonRect, "JaLoader on Jalopy v1.0?"))
                buttonPressed = true;
            else if (Event.current.type == EventType.MouseUp)
                buttonPressed = false;

            if(buttonPressed)
                GUI.Label(new Rect(buttonRect.x + buttonRect.width / 2 - 40, buttonRect.y + buttonRect.height + 10, buttonRect.width, 20), "SoonTM!", new GUIStyle() { fontSize = 20, normal = { textColor = Color.white } });
        }
    }
}
