using JaLoader.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoaderClassic
{
    public class Console : MonoBehaviour, ILogger
    {
        public static Console Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private readonly List<string> logMessages = new List<string>();

        private Vector2 scrollPosition;

        private bool showConsole = true;

        private Rect consoleWindowRect = new Rect(20, 20, 500, 300);
        private string inputString = "";

        private void OnEnable()
        {
            Application.RegisterLogCallback(LogCallback);
        }

        private void LogCallback(string logString, string stackTrace, LogType type)
        {
            logMessages.Add(logString);

            scrollPosition.y = float.MaxValue;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                showConsole = !showConsole;
        }

        private void OnGUI()
        {
            if (showConsole)
            {
                consoleWindowRect = GUILayout.Window(
                    1,
                    consoleWindowRect,
                    DrawConsoleWindow,
                    "Console"
                );
            }
        }

        private void DrawConsoleWindow(int windowID)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.BeginVertical();

            foreach (string message in logMessages)
            {
                GUILayout.Label(message);
            }

            GUILayout.EndVertical();

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            inputString = GUILayout.TextField(inputString);

            if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
            {
                ProcessCommand(inputString);
                inputString = "";
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0, 0, consoleWindowRect.width, 20));
        }

        private void ProcessCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                return;
            }

            logMessages.Add($"> {command}");

            if (command.ToLowerInvariant() == "help")
            {
                logMessages.Add("Available commands: help, clear, exit");
            }
            else if (command.ToLowerInvariant() == "clear")
            {
                logMessages.Clear();
            }
            else if (command.ToLowerInvariant() == "exit")
            {
                showConsole = false;
            }
            else if (command.ToLowerInvariant() == "load")
            {
                InternalLog("Loading settings, before");
                SettingsManager.Initialize();
                InternalLog("Loading settings");
            }
            else
            {
                logMessages.Add($"Unknown command: {command}");
            }

            scrollPosition.y = float.MaxValue;
        }

        public static void Example()
        {
            InternalLogDebug("Console", "This is a debug message.");
            InternalLogError("Console", "This is an error message.");
            InternalLogWarning("Console", "This is a warning message.");
            InternalLogMessage("Console", "This is an info message.");
        }

        public static void InternalLogDebug(object author, object message)
        {
            Debug.Log($"[DEBUG] [{author}] {message}");
        }
        public static void InternalLogError(object author, object message)
        {
            Debug.LogError($"[ERROR] [{author}] {message}");
        }
        public static void InternalLogWarning(object author, object message)
        {
            Debug.LogWarning($"[WARNING] [{author}] {message}");
        }
        public static void InternalLogMessage(object author, object message)
        {
            Debug.Log($"[INFO] [{author}] {message}");
        }
        public static void InternalLog(object author, object message)
        {
            Debug.Log($"[INFO] [{author}] {message}");
        }
        public static void InternalLog(object message)
        {
            Debug.Log($"[INFO] {message}");
        }
        public static void InternalLogMessage(object message)
        {
            Debug.Log($"[INFO] {message}");
        }
        public static void InternalLogWarning(object message)
        {
            Debug.LogWarning($"[WARNING] {message}");
        }
        public static void InternalLogError(object message)
        {
            Debug.LogError($"[ERROR] {message}");
        }
        public static void InternalLogDebug(object message)
        {
            Debug.Log($"[DEBUG] {message}");
        }

        public void ILog(object message)
        {
            InternalLog("/", message);
        }

        public void ILogDebug(object message)
        {
            InternalLogDebug("/", message);
        }
        public void ILogError(object message)
        {
            InternalLogError("/", message);
        }
        public void ILogWarning(object message)
        {
            InternalLogWarning("/", message);
        }
        public void ILogMessage(object message)
        {
            InternalLogMessage("/", message);
        }
        public void ILog(object author, object message)
        {
            InternalLog(author, message);
        }
        public void ILogMessage(object author, object message)
        {
            InternalLogMessage(author, message);
        }
        public void ILogWarning(object author, object message)
        {
            InternalLogWarning(author, message);
        }
        public void ILogError(object author, object message)
        {
            InternalLogError(author, message);
        }
        public void ILogDebug(object author, object message)
        {
            InternalLogDebug(author, message);
        }
    }
}
