using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System.IO;

namespace JaLoader
{
    [Serializable]
    public enum ConsolePositions
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    [Serializable]
    public enum ConsoleModes
    {
        Default,
        ErrorsWarnings,
        Errors,
        Disabled
    }

    public class Console : MonoBehaviour
    {
        public static Console Instance { get; private set; }

        private InputField inputField;
        private SettingsManager settingsManager = SettingsManager.Instance;
        private RectTransform consoleRectTransform;

        private List<string> log = new List<string>();

        public bool Visible
        {
            get { return UIManager.Instance.modConsole.activeSelf; }
            private set { }
        }

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

        private void Start()
        {
            inputField = UIManager.Instance.modConsole.transform.GetChild(2).GetComponent<InputField>();
            consoleRectTransform = UIManager.Instance.modConsole.GetComponent<RectTransform>();

            ToggleVisibility(false);

            SetPosition(settingsManager.ConsolePosition);

            if(ModLoader.Instance.modsNumber == 1)
                LogMessage("JModLoader", "1 mod found & loaded!");
            else
                LogMessage("JModLoader", $"{ModLoader.Instance.modsNumber} mods found & loaded!");
        }

        public void SetPosition(ConsolePositions pos)
        {
            switch (pos)
            {
                case ConsolePositions.TopLeft:
                    consoleRectTransform.anchorMin = new Vector2(0, 1);
                    consoleRectTransform.anchorMax = new Vector2(0, 1);
                    consoleRectTransform.pivot = new Vector2(0, 1);
                    consoleRectTransform.position = new Vector2(5, Screen.height - 5);

                    UIManager.Instance.modLoaderText.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
                    UIManager.Instance.modLoaderText.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
                    UIManager.Instance.modLoaderText.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
                    UIManager.Instance.modLoaderText.GetComponent<RectTransform>().position = new Vector2(Screen.width - 10, Screen.height - 5);
                    UIManager.Instance.modLoaderText.GetComponent<Text>().alignment = TextAnchor.MiddleRight;

                    UIManager.Instance.modFolderText.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
                    UIManager.Instance.modFolderText.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
                    UIManager.Instance.modFolderText.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
                    UIManager.Instance.modFolderText.GetComponent<RectTransform>().position = new Vector2(Screen.width - 10, Screen.height - 30);
                    UIManager.Instance.modFolderText.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
                    break;

                case ConsolePositions.TopRight:
                    consoleRectTransform.anchorMin = new Vector2(1, 1);
                    consoleRectTransform.anchorMax = new Vector2(1, 1);
                    consoleRectTransform.pivot = new Vector2(1, 1);
                    consoleRectTransform.position = new Vector2(Screen.width -5, Screen.height - 5);

                    UIManager.Instance.modLoaderText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                    UIManager.Instance.modLoaderText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                    UIManager.Instance.modLoaderText.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                    UIManager.Instance.modLoaderText.GetComponent<RectTransform>().position = new Vector2(10, Screen.height - 5);
                    UIManager.Instance.modLoaderText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

                    UIManager.Instance.modFolderText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                    UIManager.Instance.modFolderText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                    UIManager.Instance.modFolderText.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                    UIManager.Instance.modFolderText.GetComponent<RectTransform>().position = new Vector2(10, Screen.height - 30);
                    UIManager.Instance.modFolderText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                    break;

                case ConsolePositions.BottomLeft:
                    consoleRectTransform.anchorMin = new Vector2(0, 0);
                    consoleRectTransform.anchorMax = new Vector2(0, 0);
                    consoleRectTransform.pivot = new Vector2(0, 0);
                    consoleRectTransform.position = new Vector2(5, 5);

                    UIManager.Instance.modLoaderText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                    UIManager.Instance.modLoaderText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                    UIManager.Instance.modLoaderText.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                    UIManager.Instance.modLoaderText.GetComponent<RectTransform>().position = new Vector2(10, Screen.height - 5);
                    UIManager.Instance.modLoaderText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

                    UIManager.Instance.modFolderText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                    UIManager.Instance.modFolderText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                    UIManager.Instance.modFolderText.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                    UIManager.Instance.modFolderText.GetComponent<RectTransform>().position = new Vector2(10, Screen.height - 30);
                    UIManager.Instance.modFolderText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                    break;

                case ConsolePositions.BottomRight:
                    consoleRectTransform.anchorMin = new Vector2(1, 0);
                    consoleRectTransform.anchorMax = new Vector2(1, 0);
                    consoleRectTransform.pivot = new Vector2(1, 0);
                    consoleRectTransform.position = new Vector2(Screen.width -5, 5);

                    UIManager.Instance.modLoaderText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                    UIManager.Instance.modLoaderText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                    UIManager.Instance.modLoaderText.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                    UIManager.Instance.modLoaderText.GetComponent<RectTransform>().position = new Vector2(10, Screen.height - 5);
                    UIManager.Instance.modLoaderText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

                    UIManager.Instance.modFolderText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                    UIManager.Instance.modFolderText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                    UIManager.Instance.modFolderText.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                    UIManager.Instance.modFolderText.GetComponent<RectTransform>().position = new Vector2(10, Screen.height - 30);
                    UIManager.Instance.modFolderText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                    break;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleVisibility(!Visible);
            }

            if (inputField.text.Length > 0)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    log.Add($">> {inputField.text.ToLower()}");
                    WriteLog();
                    CheckInput(inputField);
                }
            }
        }

        public void UpdateConsole(float value)
        {
            StartCoroutine(ApplyScrollPosition(UIManager.Instance.modConsole.transform.GetChild(1).GetComponent<ScrollRect>(), value));
        }

        public void ToggleVisibility(bool visible)
        {
            if (settingsManager.ConsoleMode == ConsoleModes.Disabled)
            {
                UIManager.Instance.modConsole.SetActive(false);
                return;
            }

            UIManager.Instance.modConsole.SetActive(visible);
        }

        IEnumerator ApplyScrollPosition(ScrollRect sr, float verticalPos)
        {
            yield return new WaitForEndOfFrame();
            sr.verticalNormalizedPosition = verticalPos;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)sr.transform);
        }

        /// <summary>
        /// Same as LogMessage().
        /// </summary>
        /// <param name="sender">The message sender (use ModID or ModName)</param>
        /// <param name="message">The message to be sent</param>
        public void Log(object sender, object message)
        {
            LogMessage(sender, message);
        }

        public void Log(object message)
        {
            LogMessage(message);
        }

        public void LogMessage(object message)
        {
            if (settingsManager.ConsoleMode != ConsoleModes.Default)
                return;

            ToggleVisibility(true);

            float _value = UIManager.Instance.modConsole.transform.GetChild(1).GetComponent<ScrollRect>().verticalNormalizedPosition;

            GameObject _msg = Instantiate(UIManager.Instance.messageTemplatePrefab);
            _msg.transform.parent = UIManager.Instance.modConsole.transform.GetChild(1).GetChild(0).GetChild(0);
            _msg.transform.Find("Text").GetComponent<Text>().text = $"<color=aqua>/</color>: {message}";
            log.Add($"{DateTime.Now} > '/' '{message}'");
            _msg.SetActive(true);

            UpdateConsole(_value);
            WriteLog();
        }

        public void LogMessage(object sender, object message)
        {
            if (settingsManager.ConsoleMode != ConsoleModes.Default)
                return;

            ToggleVisibility(true);

            float _value = UIManager.Instance.modConsole.transform.GetChild(1).GetComponent<ScrollRect>().verticalNormalizedPosition;

            GameObject _msg = Instantiate(UIManager.Instance.messageTemplatePrefab);
            _msg.transform.parent = UIManager.Instance.modConsole.transform.GetChild(1).GetChild(0).GetChild(0);
            _msg.transform.Find("Text").GetComponent<Text>().text = $"<color=aqua>{sender}</color>: {message}";
            log.Add($"{DateTime.Now} > '{sender}' '{message}'");
            _msg.SetActive(true);

            UpdateConsole(_value);
            WriteLog();
        }

        public void LogWarning(object sender, object message)
        {
            if (settingsManager.ConsoleMode == ConsoleModes.Disabled || settingsManager.ConsoleMode == ConsoleModes.Errors)
                return;

            ToggleVisibility(true);

            float _value = UIManager.Instance.modConsole.transform.GetChild(1).GetComponent<ScrollRect>().verticalNormalizedPosition;

            GameObject _msg = Instantiate(UIManager.Instance.messageTemplatePrefab);
            _msg.transform.parent = UIManager.Instance.modConsole.transform.GetChild(1).GetChild(0).GetChild(0);
            _msg.transform.Find("Text").GetComponent<Text>().text = $"<color=aqua>{sender}</color>: <color=yellow>{message}</color>";
            log.Add($"{DateTime.Now} >! '{sender}' '{message}'");
            _msg.SetActive(true);

            UpdateConsole(_value);
            WriteLog();
        }
        public void LogWarning(object message)
        {
            if (settingsManager.ConsoleMode == ConsoleModes.Disabled || settingsManager.ConsoleMode == ConsoleModes.Errors)
                return;

            ToggleVisibility(true);

            float _value = UIManager.Instance.modConsole.transform.GetChild(1).GetComponent<ScrollRect>().verticalNormalizedPosition;

            GameObject _msg = Instantiate(UIManager.Instance.messageTemplatePrefab);
            _msg.transform.parent = UIManager.Instance.modConsole.transform.GetChild(1).GetChild(0).GetChild(0);
            _msg.transform.Find("Text").GetComponent<Text>().text = $"<color=aqua>/</color>: <color=yellow>{message}</color>";
            log.Add($"{DateTime.Now} >! '/' '{message}'");
            _msg.SetActive(true);

            UpdateConsole(_value);
            WriteLog();
        }

        public void LogError(object sender, object message)
        {
            if (settingsManager.ConsoleMode == ConsoleModes.Disabled)
                return;

            ToggleVisibility(true);

            float _value = UIManager.Instance.modConsole.transform.GetChild(1).GetComponent<ScrollRect>().verticalNormalizedPosition;

            GameObject _msg = Instantiate(UIManager.Instance.messageTemplatePrefab);
            _msg.transform.parent = UIManager.Instance.modConsole.transform.GetChild(1).GetChild(0).GetChild(0);
            _msg.transform.Find("Text").GetComponent<Text>().text = $"<color=aqua>{sender}</color>: <color=red>{message}</color>";
            log.Add($"{DateTime.Now} >!! '{sender}' '{message}'");
            _msg.SetActive(true);

            UpdateConsole(_value);
            WriteLog();
        }

        public void LogError(object message)
        {
            if (settingsManager.ConsoleMode == ConsoleModes.Disabled)
                return;

            ToggleVisibility(true);

            float _value = UIManager.Instance.modConsole.transform.GetChild(1).GetComponent<ScrollRect>().verticalNormalizedPosition;

            GameObject _msg = Instantiate(UIManager.Instance.messageTemplatePrefab);
            _msg.transform.parent = UIManager.Instance.modConsole.transform.GetChild(1).GetChild(0).GetChild(0);
            _msg.transform.Find("Text").GetComponent<Text>().text = $"<color=aqua>/</color>: <color=red>{message}</color>";
            log.Add($"{DateTime.Now} >!! '/' '{message}'");
            _msg.SetActive(true);

            UpdateConsole(_value);
            WriteLog();
        }

        public void LogDebug(object sender, object message)
        {
            if (settingsManager.ConsoleMode == ConsoleModes.Disabled || !settingsManager.DebugMode)
                return;

            ToggleVisibility(true);

            float _value = UIManager.Instance.modConsole.transform.GetChild(1).GetComponent<ScrollRect>().verticalNormalizedPosition;

            GameObject _msg = Instantiate(UIManager.Instance.messageTemplatePrefab);
            _msg.transform.parent = UIManager.Instance.modConsole.transform.GetChild(1).GetChild(0).GetChild(0);
            _msg.transform.Find("Text").GetComponent<Text>().text = $"<color=grey>{sender}: {message}</color>";
            log.Add($"{DateTime.Now} * '{sender}' '{message}'");

            _msg.SetActive(true);

            UpdateConsole(_value);
            WriteLog();
        }

        public void LogDebug(object message)
        {
            if (settingsManager.ConsoleMode == ConsoleModes.Disabled || !settingsManager.DebugMode)
                return;

            ToggleVisibility(true);

            float _value = UIManager.Instance.modConsole.transform.GetChild(1).GetComponent<ScrollRect>().verticalNormalizedPosition;

            GameObject _msg = Instantiate(UIManager.Instance.messageTemplatePrefab);
            _msg.transform.parent = UIManager.Instance.modConsole.transform.GetChild(1).GetChild(0).GetChild(0);
            _msg.transform.Find("Text").GetComponent<Text>().text = $"<color=grey>/: {message}</color>";
            log.Add($"{DateTime.Now} * '/' '{message}'");

            _msg.SetActive(true);

            UpdateConsole(_value);
            WriteLog();
        }

        public void WriteLog()
        {
            if (!settingsManager.DebugMode)
                return;

            File.WriteAllLines(Path.Combine(Application.dataPath, @"..\JML_log.log"), log.ToArray());
        }

        public void CheckInput(InputField inputField)
        {
            string input = inputField.text.ToLower();
            string[] inputWords = input.Split(' ');
            inputField.text = "";

            if (inputWords.Length == 1)
            {
                switch (input)
                {
                    case "help":
                        ShowHelp();
                        break;

                    case "clear":
                        Clear();
                        break;

                    case "mods":
                        ToggleModList();
                        break;

                    case "path":
                        SendPath();
                        break;

                    case "version":
                        SendVersion();
                        break;

                    case "debug":
                        ToggleDebug();
                        break;

                    case "repairkit":
                        if (SceneManager.GetActiveScene().buildIndex != 3)
                        {
                            LogError("/", "This command only works in-game!");
                            break;
                        }
                        SpawnRepairKit();
                        break;

                    case "repairall":
                        if (SceneManager.GetActiveScene().buildIndex != 3)
                        {
                            LogError("/", "This command only works in-game!");
                            break;
                        }
                        RepairAll();
                        break;

                    case "laika":
                        if (SceneManager.GetActiveScene().buildIndex != 3)
                        {
                            LogError("/", "This command only works in-game!");
                            break;
                        }
                        TeleportLaika();
                        break;

                    case "money":
                        if (SceneManager.GetActiveScene().buildIndex != 3)
                        {
                            LogError("/", "This command only works in-game!");
                            break;
                        }

                        LogError("/", "Invalid command syntax. Usage: 'money add/set/remove {value}'");
                        break;

                    case "time":
                        if (SceneManager.GetActiveScene().buildIndex != 3)
                        {
                            LogError("/", "This command only works in-game!");
                            break;
                        }

                        LogError("/", "Invalid command syntax. Usage: 'time set {value}'");
                        break;

                    default:
                        LogError("/", "Invalid command; type 'help' for a list of commands");
                        break;
                }
            }
            else if (inputWords.Length > 1)
            {
                int value;

                switch (inputWords[0])
                {
                    case "time":
                        if (SceneManager.GetActiveScene().buildIndex != 3)
                        {
                            LogError("/", "This command only works in-game!");
                            break;
                        }

                        if (inputWords[1] == "set")
                        {
                            if (!int.TryParse(inputWords[2], out value))
                            {
                                LogError("/", "Invalid value. Enter a valid integer value, between 0 and 24.");

                                return;
                            }

                            if (value >= 0 && value < 24)
                            {
                                SetTime(value);
                            }
                            else
                            {
                                LogError("/", "Invalid value. Enter a valid integer value, between 0 and 24.");
                            }
                        }
                        else
                        {
                            LogError("/", "Invalid command syntax. Usage: 'time set {value}'");
                        }
                        break;

                    case "money":
                        if (SceneManager.GetActiveScene().buildIndex != 3)
                        {
                            LogError("/", "This command only works in-game!");
                            break;
                        }

                        if (!int.TryParse(inputWords[2], out value) && inputWords.Length == 3)
                        {
                            LogError("/", "Invalid value. Enter a valid integer value.");
                            break;
                        }

                        switch (inputWords[1])
                        {
                            case "add":
                                AddMoney(value);
                                break;

                            case "set":
                                SetMoney(value);
                                break;

                            case "remove":
                                RemoveMoney(value);
                                break;

                            default:
                                LogError("/", "Invalid command syntax. Usage: 'money add/set/remove {value}'");
                                break;
                        }
                        break;

                    default:
                        LogError("/", "Invalid command; type 'help' for a list of commands");
                        break;
                }
            }
        }

        public void ShowHelp()
        {
            LogMessage("/", "'help' - Shows this");
            LogMessage("/", "'clear' - Clears the console");
            LogMessage("/", "'mods' - Toggles the mod list");
            LogMessage("/", "'path' - Prints the mods path");
            LogMessage("/", "'version' - Prints the modloader's version");
            LogMessage("/", "'debug' - Toggle debug messages");
            LogMessage("Cheat commands (only work in-game)", "");
            LogMessage("/", "'money add/set/remove {value}' - Add, set or remove money from your wallet");
            LogMessage("/", "'time set {value}' - Sets the time to the specified hour");
            LogMessage("/", "'repairkit' - Spawns a repair kit near you");
            LogMessage("/", "'repairall' - Restores every installed part to full condition");
            LogMessage("/", "'laika' - Teleports the Laika near you");
        }

        public void SpawnRepairKit()
        {
            Log("/", $"Spawned a repair kit!");
        }

        public void RepairAll()
        {
            Log("/", $"Restored every installed part to full condition!");
        }

        public void TeleportLaika()
        {
            Log("/", $"Teleported Laika near you!");
        }

        public void SetTime(int hour)
        {
            Log("/", $"Set the time to {hour}!");
            FindObjectOfType<DNC_DayNight>()._timeInSeconds = hour * 3600;
        }

        public void SetMoney(int value)
        {
            FindObjectOfType<WalletC>().TotalWealth = value;
            FindObjectOfType<WalletC>().UpdateWealth();
            Log("/", $"Set money to {value}!");
        }

        public void AddMoney(int value)
        {
            FindObjectOfType<WalletC>().TotalWealth += value;
            FindObjectOfType<WalletC>().UpdateWealth();
            Log("/", $"Added {value} money!");
        }

        public void RemoveMoney(int value)
        {
            FindObjectOfType<WalletC>().TotalWealth -= value;
            FindObjectOfType<WalletC>().UpdateWealth();
            Log("/", $"Removed {value} money!");
        }

        public void ToggleDebug()
        {
            settingsManager.DebugMode = !settingsManager.DebugMode;
            settingsManager.SaveSettings();
            UIManager.Instance.SetOptionsValues();

            switch (settingsManager.DebugMode)
            {
                case true:
                    LogMessage("/", "Any future debug messages sent from now on will be <i>shown.</i>");
                    break;

                case false:
                    LogMessage("/", "Any future debug messages sent from now on will be <i>hidden.</i>");
                    break;
            }
        }

        public void ToggleModList()
        {
            UIManager.Instance.ToggleModMenu();
        }

        public void SendPath()
        {
            LogMessage("/", settingsManager.ModFolderLocation);
        }

        public void SendVersion()
        {
            LogMessage("/", $"a_{settingsManager.Version}");
        }

        public void Clear()
        {
            foreach (Transform message in UIManager.Instance.modConsole.transform.GetChild(1).GetChild(0).GetChild(0))
            {
                if (message.name != "MessageTemplate")
                    Destroy(message.gameObject);
            }
        }
    }
}
