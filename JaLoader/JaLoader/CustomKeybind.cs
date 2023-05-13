using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    public class CustomKeybind : MonoBehaviour
    {
        public KeyCode SelectedKey;
        public KeyCode AltSelectedKey;

        bool waiting;
        WaitingFor waitingFor = WaitingFor.Nothing;

        private void Update()
        {
            if (waiting && Input.anyKeyDown)
            {
                KeyCode key = KeyCode.None;
                waiting = false;
                string strToCheck = Input.inputString.ToUpper();
                if (strToCheck != string.Empty)
                {
                    if (Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        key = KeyCode.KeypadEnter;
                        return;
                    }
                    else if (Input.GetKeyDown(KeyCode.Return))
                    {
                        key = KeyCode.Return;
                        return;
                    }

                    switch (strToCheck)
                    {
                        default:
                            key = (KeyCode)Enum.Parse(typeof(KeyCode), strToCheck);
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
                    else if (Input.GetKeyDown(KeyCode.Backspace))
                        key = KeyCode.Backspace;
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

                if (waitingFor == WaitingFor.Primary)
                {
                    SelectedKey = key;
                    waitingFor = WaitingFor.Nothing;
                }
                else if (waitingFor == WaitingFor.Secondary)
                {
                    AltSelectedKey = key;
                    waitingFor = WaitingFor.Nothing;
                }
            }
        }

        public void WaitForPrimary()
        {
            waitingFor = WaitingFor.Primary;
        }

        public void WaitForSecondary()
        {
            waitingFor = WaitingFor.Secondary;
        }
    }

    enum WaitingFor
    {
        Primary,
        Secondary,
        Nothing
    }
}
