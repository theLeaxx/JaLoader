﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Linq;
using UnityEngine.EventSystems;

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
        #region Singleton
        public static Console Instance { get; private set; }

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

            settingsManager = SettingsManager.Instance;
            uiManager = UIManager.Instance;
            Application.logMessageReceived += HandleLog;
        }

        private void HandleLog(string message, string stackTrace, LogType type)
        {
            if (!LogEverythingFromDebug)
                return;

            switch (type)
            {
                case LogType.Error:
                    LogError("Unity", message);
                    break;

                case LogType.Assert:
                    LogError("Unity", message);
                    break;

                case LogType.Warning:
                    LogWarning("Unity", message);
                    break;

                case LogType.Log:
                    LogMessage("Unity", message);
                    break;

                case LogType.Exception:
                    LogError("Unity", message);
                    break;
            }
        }
        #endregion

        private InputField inputField;
        private SettingsManager settingsManager;
        private UIManager uiManager;
        private RectTransform consoleRectTransform;

        private readonly List<string> log = new List<string>();
        private readonly List<string> enteredCommands = new List<string>();
        private int currentInList = 0;

        private readonly Dictionary<(string, string, string), Mod> customCommands = new Dictionary<(string, string, string), Mod>();

        private bool init = false;

        private readonly List<(string, string, string)> queuedLogs = new List<(string, string, string)>();

        public bool LogEverythingFromDebug = false;
        
        public bool Visible
        {
            get { return uiManager.modConsole.activeSelf; }
            private set { }
        }
        
        public void Init()
        {
            inputField = uiManager.modConsole.transform.GetChild(2).GetComponent<InputField>();
            consoleRectTransform = uiManager.modConsole.GetComponent<RectTransform>();

            SetPosition(settingsManager.ConsolePosition);

            init = true;

            LogAllQueuedMessages();
        }

        public void SetPosition(ConsolePositions pos)
        {
            var modLoaderTextRT = uiManager.modLoaderText.GetComponent<RectTransform>();
            var modLoaderTextT = uiManager.modLoaderText.GetComponent<Text>();

            var modsFolderTextRT = uiManager.modFolderText.GetComponent<RectTransform>();
            var modsFolderTextT = uiManager.modFolderText.GetComponent<Text>();

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
                    modLoaderTextT.alignment = TextAnchor.MiddleRight;

                    modsFolderTextRT.anchorMin = new Vector2(1, 1);
                    modsFolderTextRT.anchorMax = new Vector2(1, 1);
                    modsFolderTextRT.pivot = new Vector2(1, 1);
                    modsFolderTextRT.position = new Vector2(Screen.width - 10, Screen.height - 30);
                    modsFolderTextT.alignment = TextAnchor.MiddleRight;
                    break;

                case ConsolePositions.TopRight:
                    consoleRectTransform.anchorMin = new Vector2(1, 1);
                    consoleRectTransform.anchorMax = new Vector2(1, 1);
                    consoleRectTransform.pivot = new Vector2(1, 1);
                    consoleRectTransform.position = new Vector2(Screen.width -5, Screen.height - 5);

                    modLoaderTextRT.anchorMin = new Vector2(0, 1);
                    modLoaderTextRT.anchorMax = new Vector2(0, 1);
                    modLoaderTextRT.pivot = new Vector2(0, 1);
                    modLoaderTextRT.position = new Vector2(10, Screen.height - 5);
                    modLoaderTextT.alignment = TextAnchor.MiddleLeft;

                    modsFolderTextRT.anchorMin = new Vector2(0, 1);
                    modsFolderTextRT.anchorMax = new Vector2(0, 1);
                    modsFolderTextRT.pivot = new Vector2(0, 1);
                    modsFolderTextRT.position = new Vector2(10, Screen.height - 30);
                    modsFolderTextT.alignment = TextAnchor.MiddleLeft;
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
                    modLoaderTextT.alignment = TextAnchor.MiddleLeft;

                    modsFolderTextRT.anchorMin = new Vector2(0, 1);
                    modsFolderTextRT.anchorMax = new Vector2(0, 1);
                    modsFolderTextRT.pivot = new Vector2(0, 1);
                    modsFolderTextRT.position = new Vector2(10, Screen.height - 30);
                    modsFolderTextT.alignment = TextAnchor.MiddleLeft;
                    break;

                case ConsolePositions.BottomRight:
                    consoleRectTransform.anchorMin = new Vector2(1, 0);
                    consoleRectTransform.anchorMax = new Vector2(1, 0);
                    consoleRectTransform.pivot = new Vector2(1, 0);
                    consoleRectTransform.position = new Vector2(Screen.width -5, 5);

                    modLoaderTextRT.anchorMin = new Vector2(0, 1);
                    modLoaderTextRT.anchorMax = new Vector2(0, 1);
                    modLoaderTextRT.pivot = new Vector2(0, 1);
                    modLoaderTextRT.position = new Vector2(10, Screen.height - 5);
                    modLoaderTextT.alignment = TextAnchor.MiddleLeft;

                    modsFolderTextRT.anchorMin = new Vector2(0, 1);
                    modsFolderTextRT.anchorMax = new Vector2(0, 1);
                    modsFolderTextRT.pivot = new Vector2(0, 1);
                    modsFolderTextRT.position = new Vector2(10, Screen.height - 30);
                    modsFolderTextT.alignment = TextAnchor.MiddleLeft;
                    break;
            }
        }

        public void AddCommand(string commandName, string description, string methodName, Mod mod)
        {
            foreach ((string, string, string) pair in customCommands.Keys)
            {
                if (pair.Item1 == commandName)
                {
                    LogError(mod.ModID, $"A command with the name {commandName} already exists!");
                    return;
                }
            }

            customCommands.Add((commandName, description ,methodName), mod);
        }

        public void RemoveCommandsFromMod(Mod mod)
        {
            var keysToRemove = new List<(string, string, string)>();

            foreach (var kv in customCommands)
            {
                if (kv.Value == mod)
                {
                    keysToRemove.Add(kv.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                customCommands.Remove(key);
            }
        }

        private void Update()
        {
            if (!init)
                return;

            if (Input.GetKeyDown(KeyCode.Tab))
                ToggleVisibility(!Visible);

            if (inputField.text.Length > 0)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    log.Add($">> {inputField.text.ToLower()}");
                    enteredCommands.Add(inputField.text);
                    currentInList = enteredCommands.Count;
                    WriteLog();

                    CheckInput(inputField);
                }
            }

            if (Visible)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (currentInList > 0)
                    {
                        currentInList--;
                        inputField.text = enteredCommands[currentInList];

                        inputField.Select();

                        inputField.caretPosition = inputField.text.Length;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (currentInList < enteredCommands.Count)
                    {
                        currentInList++;
                        inputField.text = enteredCommands[currentInList];

                        inputField.Select();

                        inputField.caretPosition = inputField.text.Length;
                    }
                }
            }
        }

        public void UpdateConsole(float value)
        {
            if (uiManager.modConsole.transform.GetChild(1).GetChild(0).GetChild(0).childCount > 100)
                Destroy(uiManager.modConsole.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(1).gameObject);

            StartCoroutine(ApplyScrollPosition(uiManager.modConsole.transform.GetChild(1).GetComponent<ScrollRect>(), value));
        }

        public void ToggleVisibility(bool visible)
        {
            if (settingsManager.ConsoleMode == ConsoleModes.Disabled)
            {
                uiManager.modConsole.SetActive(false);
                return;
            }

            uiManager.modConsole.SetActive(visible);
        }

        IEnumerator ApplyScrollPosition(ScrollRect sr, float verticalPos)
        {
            yield return new WaitForEndOfFrame();
            sr.verticalNormalizedPosition = verticalPos;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)sr.transform);
        }

        #region Logging
        /// <summary>
        /// Same as LogMessage().
        /// </summary>
        /// <param name="sender">The message sender (use ModID or ModName)</param>
        /// <param name="message">The message to be sent</param>
        public static void Log(object sender, object message)
        {
            LogMessage(sender, message);
        }

        /// <summary>
        /// Same as LogMessage().
        /// </summary>
        /// <param name="message">The message to be sent</param>
        public static void Log(object message)
        {
            LogMessage(message);
        }

        /// <summary>
        /// Logs a normal message to the in-game console.
        /// </summary>
        /// <param name="message">The message to be sent</param>
        public static void LogMessage(object message)
        {
            if (SettingsManager.Instance.ConsoleMode != ConsoleModes.Default)
                return;

            Instance.LogMessage("/", message, "aqua");
        }

        /// <summary>
        /// Logs a normal message to the in-game console.
        /// </summary>
        /// <param name="sender">The message sender (use ModID or ModName)</param>
        /// <param name="message">The message to be sent</param>
        public static void LogMessage(object sender, object message)
        {
            if (SettingsManager.Instance.ConsoleMode != ConsoleModes.Default)
                return;

            Instance.LogMessage(sender, message, "aqua");
        }

        /// <summary>
        /// Logs a normal message to the JaLoader_log.log file, does not show in-game.
        /// </summary>
        /// <param name="message"></param>
        public static void LogOnlyToFile(object message)
        {
            Instance.log.Add($"{DateTime.Now} - {message}");
            Instance.WriteLog();
        }

        private void LogAllQueuedMessages()
        {
            foreach ((string, string, string) log in queuedLogs)
            {
                LogMessage(log.Item1, log.Item2, log.Item3);
            }

            queuedLogs.Clear();
        }

        private void LogMessage(object sender, object message, string color)
        {
            if (!init)
            {
                queuedLogs.Add((sender.ToString(), message.ToString(), color));

                switch (color)
                {
                    case "grey":
                        log.Add($"{DateTime.Now} * '{sender}': '{message}'");
                        break;

                    case "red":
                        log.Add($"{DateTime.Now} >!! '{sender}': '{message}'");
                        break;

                    case "yellow":
                        log.Add($"{DateTime.Now} >! '{sender}': '{message}'");
                        break;

                    case "aqua":
                        log.Add($"{DateTime.Now} > '/': '{message}'");
                        break;
                }
                return;
            }

            ToggleVisibility(true);

            float _value = UIManager.Instance.modConsole.transform.GetChild(1).GetComponent<ScrollRect>().verticalNormalizedPosition;

            GameObject _msg = Instantiate(uiManager.messageTemplatePrefab);
            _msg.transform.SetParent(uiManager.modConsole.transform.GetChild(1).GetChild(0).GetChild(0), false);
            switch (color)
            {
                case "grey":
                    _msg.GetComponent<InputField>().text = $"<color=grey>{sender}: {message}</color>";
                    log.Add($"{DateTime.Now} * '{sender}': '{message}'");
                    break;

                case "red":
                    _msg.GetComponent<InputField>().text = $"<color=aqua>{sender}</color>: <color=red>{message}</color>";
                    log.Add($"{DateTime.Now} >!! '{sender}': '{message}'");
                    break;

                case "yellow":
                    _msg.GetComponent<InputField>().text = $"<color=aqua>{sender}</color>: <color=yellow>{message}</color>";
                    log.Add($"{DateTime.Now} >! '{sender}': '{message}'");
                    break;

                case "aqua":
                    _msg.GetComponent<InputField>().text = $"<color=aqua>{sender}</color>: {message}";
                    log.Add($"{DateTime.Now} > '/': '{message}'");
                    break;
            }

            _msg.SetActive(true);

            UpdateConsole(_value);
            WriteLog();
        }

        /// <summary>
        /// Logs a warning to the in-game console.
        /// </summary>
        /// <param name="sender">The message sender (use ModID or ModName)</param>
        /// <param name="message">The message to be sent</param>
        public static void LogWarning(object sender, object message)
        {
            if (SettingsManager.Instance.ConsoleMode == ConsoleModes.Disabled || SettingsManager.Instance.ConsoleMode == ConsoleModes.Errors)
                return;

            Instance.LogMessage(sender, message, "yellow");
        }

        /// <summary>
        /// Logs a warning to the in-game console.
        /// </summary>
        /// <param name="message">The message to be sent</param>
        public static void LogWarning(object message)
        {
            if (SettingsManager.Instance.ConsoleMode == ConsoleModes.Disabled || SettingsManager.Instance.ConsoleMode == ConsoleModes.Errors)
                return;

            Instance.LogMessage("/", message, "yellow");
        }

        /// <summary>
        /// Logs an error to the in-game console.
        /// </summary>
        /// <param name="sender">The message sender (use ModID or ModName)</param>
        /// <param name="message">The message to be sent</param>
        public static void LogError(object sender, object message)
        {
            if (SettingsManager.Instance.ConsoleMode == ConsoleModes.Disabled)
                return;

            Instance.LogMessage(sender, message, "red");
        }

        /// <summary>
        /// Logs an error to the in-game console.
        /// </summary>
        /// <param name="message">The message to be sent</param>
        public static void LogError(object message)
        {
            if (SettingsManager.Instance.ConsoleMode == ConsoleModes.Disabled)
                return;

            Instance.LogMessage("/", message, "red");
        }

        /// <summary>
        /// Logs a debug message to the in-game console. These are only visible if debug mode is enabled.
        /// </summary>
        /// <param name="sender">The message sender (use ModID or ModName)</param>
        /// <param name="message">The message to be sent</param>
        public static void LogDebug(object sender, object message)
        {
            if (SettingsManager.Instance.ConsoleMode == ConsoleModes.Disabled || !SettingsManager.Instance.DebugMode)
                return;

            Instance.LogMessage(sender, message, "grey");
        }

        /// <summary>
        /// Logs a debug message to the in-game console. These are only visible if debug mode is enabled.
        /// </summary>
        /// <param name="message">The message to be sent</param>
        public static void LogDebug(object message)
        {
            if (SettingsManager.Instance.ConsoleMode == ConsoleModes.Disabled || !SettingsManager.Instance.DebugMode)
                return;

            Instance.LogMessage("/", message, "grey");
        }

        private void OnApplicationQuit()
        {
            LogOnlyToFile("Game closed!");
            WriteLog();
        }

        private void WriteLog()
        {
            File.WriteAllLines(Path.Combine(Application.dataPath, @"..\JaLoader_log.log"), log.ToArray());
        }

        #endregion

        private void CheckInput(InputField inputField)
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

                    case "settings":
                        ToggleSettings();
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

                    case "gascan":
                        if (SceneManager.GetActiveScene().buildIndex != 3)
                        {
                            LogError("/", "This command only works in-game!");
                            break;
                        }
                        SpawnGasCan();
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

                    case "tolaika":
                        if (SceneManager.GetActiveScene().buildIndex != 3)
                        {
                            LogError("/", "This command only works in-game!");
                            break;
                        }
                        TeleportToLaika();
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
                        bool found = false;
                        foreach ((string, string, string) pair in customCommands.Keys)
                        {
                            if (pair.Item1.ToLower() == input)
                            {
                                found = true;
                                customCommands[pair].Invoke(pair.Item3, 0);
                                break;
                            }
                        }
                        if (!found)
                        {
                            LogError("/", $"Invalid command '{input}'.");
                            LogError("/", "Type 'help' for a list of valid commands.");
                        }
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

                        string time = inputWords[2].ToLower();
                        int hours = 0, minutes = 0;

                        if(time != "day" && time != "night")
                        {
                            if (!time.Contains(":"))
                            {
                                LogError("/", "Invalid time. Enter a valid hour:minute.");
                                break;
                            }

                            string[] timeSplit = time.Split(':');

                            if (!int.TryParse(timeSplit[0], out hours))
                            {
                                LogError("/", "Invalid time. Enter a valid hour.");
                                break;
                            }

                            if (!int.TryParse(timeSplit[1], out minutes))
                            {
                                LogError("/", "Invalid time. Enter a valid minute.");
                                break;
                            }
                        }

                        if (time == "day")
                            hours = 12;

                        if(time == "night")
                            hours = 20;

                        switch (inputWords[1])
                        {
                            case "set":
                                ModHelper.Instance.TimeSet(hours, minutes);
                                Log("/", $"Set time to {hours}:{minutes}!");
                                break;
                            default:
                                LogError("/", "Invalid command syntax. Usage: 'time set {value/day/night}'");
                                break;
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
                        //Log(FindObjectOfType<MainMenuC>().name); //223 objects
                        break;
                }
            }
        }

        private void TeleportLaika()
        {
            var car = ModHelper.Instance.laika;
            var player = ModHelper.Instance.player;
            var script = Camera.main.GetComponent<MainMenuC>();
            car.transform.position = player.transform.position + player.transform.forward * 8 + new Vector3(0, 2, 0);
            car.transform.rotation = player.transform.rotation;
            script.SendMessage("SavePause");
            if(script.isPaused == 0)
                script.SendMessage("UpdateTime", 1f);

            Log("/", "Teleported laika to player!");
        }

        private void TeleportToLaika()
        {
            var car = ModHelper.Instance.laika;
            var player = ModHelper.Instance.player;
            player.transform.position = car.transform.position + car.transform.right * -5;
            player.transform.rotation = car.transform.rotation;

            Log("/", "Teleported player to laika!");
        }

        private void ShowHelp()
        {
            LogMessage("'help' - Shows this");
            LogMessage("'clear' - Clears the console");
            LogMessage("'mods' - Toggles the mod list");
            LogMessage("'path' - Prints the mods path");
            LogMessage("'version' - Prints the modloader's version");
            LogMessage("'debug' - Toggle debug messages");
            LogMessage("Cheat commands (only work in-game)", "");
            LogMessage("'money add/set/remove {value}' - Add, set or remove money from your wallet");
            LogMessage("/", "'time set {hr:min/day/night}' - Sets the time to the specified hour/day/night");
            LogMessage("'repairkit' - Spawns a repair kit near you");
            LogMessage("'gascan' - Spawns a filled gas can near you");
            LogMessage("'repairall' - Restores every installed part to full condition");
            LogMessage("/", "'laika' - Teleports the Laika in front of you");
            LogMessage("/", "'tolaika' - Teleports you to the Laika");

            LogMessage("Mods commands", "");
            foreach ((string, string, string) pair in customCommands.Keys)
            {
                LogMessage($"'{pair.Item1.ToLower()}' - {pair.Item2}");
            }
        }

        private void SpawnRepairKit()
        {
            DebugObjectSpawner.Instance.SpawnVanillaObject(162);

            Log("/", $"Spawned a repair kit!");
        }

        private void SpawnGasCan()
        {
            DebugObjectSpawner.Instance.SpawnVanillaObject(158);

            Log("/", $"Spawned a gas can!");
        }

        private void RepairAll()
        {
            foreach (var component in FindObjectsOfType<EngineComponentC>())
            {
                if (component.GetComponent<ObjectPickupC>().isInEngine)
                {
                    component.Condition = component.durability;
                }
            }

            Log("/", $"Restored every installed part to full condition!");
        }

        private void SetMoney(int value)
        {
            ModHelper.Instance.wallet.TotalWealth = value;
            ModHelper.Instance.wallet.UpdateWealth();
            Log("/", $"Set money to {value}!");
        }

        private void AddMoney(int value)
        {
            ModHelper.Instance.wallet.TotalWealth += value;
            ModHelper.Instance.wallet.UpdateWealth();
            Log("/", $"Added {value} money!");
        }

        private void RemoveMoney(int value)
        {
            ModHelper.Instance.wallet.TotalWealth -= value;
            ModHelper.Instance.wallet.UpdateWealth();
            Log("/", $"Removed {value} money!");
        }

        private void ToggleDebug()
        {
            settingsManager.DebugMode = !settingsManager.DebugMode;
            settingsManager.SaveSettings();
            uiManager.SetOptionsValues();

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

        private void ToggleModList()
        {
            uiManager.ToggleModMenu();
        }

        private void ToggleSettings()
        {
            uiManager.ToggleModLoaderSettings_Main();
        }

        private void SendPath()
        {
            LogMessage("/", settingsManager.ModFolderLocation);
        }

        private void SendVersion()
        {
            LogMessage("/", $"a_{settingsManager.GetVersionString()}");
        }

        private void Clear()
        {
            foreach (Transform message in uiManager.modConsole.transform.GetChild(1).GetChild(0).GetChild(0))
                if (message.name != "MessageTemplate")
                    Destroy(message.gameObject);
        }
    }
}
