using JaLoader.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace JaLoader
{
    public class CustomKeybind : MonoBehaviour
    {
        public KeyCode SelectedKey = KeyCode.None;
        public KeyCode AltSelectedKey = KeyCode.None;
        private bool enableAltKey;
        public bool EnableAltKey
        {
            get { return enableAltKey; }
            set
            {
                enableAltKey = value;
                secondaryKey?.SetActive(value);
            }
        }

        bool waiting;
        WaitingFor waitingFor = WaitingFor.Nothing;

        private GameObject primaryKey;
        private GameObject secondaryKey;

        private Text primaryText;
        private Text secondaryText;

        bool updatePrimary;
        bool updateSecondary;
        bool addedReferences;

        readonly Dictionary<KeyCode, string> keyDisplayNames = new Dictionary<KeyCode, string>()
        {
            { KeyCode.Alpha0, "0" },
            { KeyCode.Alpha1, "1" },
            { KeyCode.Alpha2, "2" },
            { KeyCode.Alpha3, "3" },
            { KeyCode.Alpha4, "4" },
            { KeyCode.Alpha5, "5" },
            { KeyCode.Alpha6, "6" },
            { KeyCode.Alpha7, "7" },
            { KeyCode.Alpha8, "8" },
            { KeyCode.Alpha9, "9" },

            { KeyCode.Minus, "-" },
            { KeyCode.Plus, "+" },
            { KeyCode.Equals, "=" },
            { KeyCode.LeftBracket, "[" },
            { KeyCode.RightBracket, "]" },
            { KeyCode.Backslash, "\\" },
            { KeyCode.Slash, "/" },
            { KeyCode.Semicolon, ";" },
            { KeyCode.Quote, "'" },
            { KeyCode.BackQuote, "`" },
            { KeyCode.Comma, "," },
            { KeyCode.Period, "." },
            { KeyCode.Asterisk, "*" },

            { KeyCode.LeftShift, "LShift" },
            { KeyCode.RightShift, "RShift" },
            { KeyCode.CapsLock, "Caps" },
            { KeyCode.LeftControl, "LCtrl" },
            { KeyCode.RightControl, "RCtrl" },
            { KeyCode.LeftAlt, "LAlt" },
            { KeyCode.RightAlt, "RAlt" },
            { KeyCode.AltGr, "AltGr" },
            { KeyCode.UpArrow, "↑" },
            { KeyCode.DownArrow, "↓" },
            { KeyCode.LeftArrow, "←" },
            { KeyCode.RightArrow, "→" },

            { KeyCode.Mouse0, "M1" },
            { KeyCode.Mouse1, "M2" },
            { KeyCode.Mouse2, "M3" }
        };

        private string GetKeyDisplayName(KeyCode key)
        {
            if (keyDisplayNames.ContainsKey(key))
            {
                return keyDisplayNames[key];
            }
            return key.ToString();
        }

        private void AddReferences()
        {
            primaryKey = transform.GetChild(1).gameObject;
            secondaryKey = transform.GetChild(2).gameObject;

            primaryText = primaryKey.transform.GetChild(0).GetComponent<Text>();

            secondaryText = secondaryKey.transform.GetChild(0).GetComponent<Text>();

            primaryKey.GetComponent<Button>().onClick.AddListener(WaitForPrimary);
            secondaryKey.GetComponent<Button>().onClick.AddListener(WaitForSecondary);

            addedReferences = true;
        }

        private void Update()
        {
            if(!addedReferences)
            {
                AddReferences();
            }

            if (waiting && Input.anyKeyDown)
            {
                KeyCode key = KeyCode.None;
                waiting = false;
                string strToCheck = Input.inputString.ToUpper();
                //Console.Log($"'{strToCheck}'");
                if (strToCheck != string.Empty)
                {
                    switch (strToCheck)
                    {
                        default:
                            try
                            {
                                key = (KeyCode)Enum.Parse(typeof(KeyCode), strToCheck);
                            }
                            catch (Exception)
                            {
                                waiting = true;
                                return;
                            }
                            break;

                        case "0":
                            key = KeyCode.Alpha0;
                            break;

                        case "-":
                            key = KeyCode.Minus;
                            break;

                        case "+":
                            key = KeyCode.Plus;
                            break;

                        case "=":
                            key = KeyCode.Equals;
                            break;

                        case "[":
                            key = KeyCode.LeftBracket;
                            break;

                        case "]":
                            key = KeyCode.RightBracket;
                            break;

                        case "\\":
                            key = KeyCode.Backslash;
                            break;

                        case "/":
                            key = KeyCode.Slash;
                            break;

                        case ";":
                            key = KeyCode.Semicolon;
                            break;

                        case "'":
                            key = KeyCode.Quote;
                            break;

                        case "`":
                            key = KeyCode.BackQuote;
                            break;

                        case ",":
                            key = KeyCode.Comma;
                            break;

                        case ".":
                            key = KeyCode.Period;
                            break;

                        case "*":
                            key = KeyCode.Asterisk;
                            break;
                    }

                    if (Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        key = KeyCode.KeypadEnter;
                    }
                    else if (strToCheck == " ")
                    {
                        key = KeyCode.Space;
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.LeftShift))
                        key = KeyCode.LeftShift;
                    else if (Input.GetKeyDown(KeyCode.RightShift))
                        key = KeyCode.RightShift;
                    else if (Input.GetKeyDown(KeyCode.CapsLock))
                        key = KeyCode.CapsLock;
                    else if (Input.GetKeyDown(KeyCode.Tab))
                        key = KeyCode.Tab;
                    else if (Input.GetKeyDown(KeyCode.LeftControl))
                        key = KeyCode.LeftControl;
                    else if (Input.GetKeyDown(KeyCode.RightControl))
                        key = KeyCode.RightControl;
                    else if (Input.GetKeyDown(KeyCode.LeftAlt))
                        key = KeyCode.LeftAlt;
                    else if (Input.GetKeyDown(KeyCode.RightAlt))
                        key = KeyCode.RightAlt;
                    else if (Input.GetKeyDown(KeyCode.AltGr))
                        key = KeyCode.AltGr;
                    else if (Input.GetKeyDown(KeyCode.UpArrow))
                        key = KeyCode.UpArrow;
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                        key = KeyCode.DownArrow;
                    else if (Input.GetKeyDown(KeyCode.LeftArrow))
                        key = KeyCode.LeftArrow;
                    else if (Input.GetKeyDown(KeyCode.RightArrow))
                        key = KeyCode.RightArrow;
                    else if (Input.GetMouseButtonDown(0))
                        key = KeyCode.Mouse0;
                    else if (Input.GetMouseButtonDown(1))
                        key = KeyCode.Mouse1;
                    else if (Input.GetMouseButtonDown(2))
                        key = KeyCode.Mouse2;
                }

                CheckConflictingKeybinds(key);

                if (waitingFor == WaitingFor.Primary)
                    SetPrimaryKey(key);
                else if (waitingFor == WaitingFor.Secondary)
                    SetSecondaryKey(key);

                waitingFor = WaitingFor.Nothing;
            }

            if (updatePrimary && primaryText != null)
            {
                primaryText.text = GetKeyDisplayName(SelectedKey);

                updatePrimary = false;
            }

            if (updateSecondary && secondaryText != null)
            {
                secondaryText.text = GetKeyDisplayName(AltSelectedKey);

                updateSecondary = false;
            }

            if (!EnableAltKey && secondaryKey != null && secondaryKey.activeSelf)
            {
                secondaryKey.SetActive(false);
            }
        }

        private bool CheckConflictingKeybinds(KeyCode keyToCheckFor)
        {
            if (SelectedKey == keyToCheckFor)
                return false;

            if (enableAltKey == true && AltSelectedKey == keyToCheckFor)
                return false;

            bool foundAnyConflicts = false;

            foreach (MonoBehaviour mb in ModManager.Mods.Keys)
            {
                if (mb is Mod mod)
                {
                    var list = mod.GetAllUsedKeycodes(JaLoaderSettings.DebugMode);
                    var allMatches = list.Where(tuple => tuple.Item2 == keyToCheckFor);

                    foreach (var match in allMatches)
                    {
                        foundAnyConflicts = true;
                        UIManager.Instance.ShowNotice("CONFLICTING KEYBIND", $"The key you are currently setting ({keyToCheckFor}) is already used in Mod '{(JaLoaderSettings.DebugMode ? mod.ModID : mod.ModName)}', for setting '{match.Item1}'.", false, true);
                    }
                }
            }

            return foundAnyConflicts;
        }

        public void WaitForPrimary()
        {
            waitingFor = WaitingFor.Primary;
            waiting = true;
        }

        public void WaitForSecondary()
        {
            waitingFor = WaitingFor.Secondary;
            waiting = true;
        }

        public void SetPrimaryKey(KeyCode key)
        {
            SelectedKey = key;
            if (primaryText == null) updatePrimary = true;
            else primaryText.text = GetKeyDisplayName(key);
        }

        public void SetSecondaryKey(KeyCode key)
        {
            AltSelectedKey = key;
            if (secondaryText == null) updateSecondary = true;
            else secondaryText.text = GetKeyDisplayName(key);
        }
    }

    enum WaitingFor
    {
        Primary,
        Secondary,
        Nothing
    }
}
