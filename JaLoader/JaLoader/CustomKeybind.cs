using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


// WIP
namespace JaLoader
{
    public class CustomKeybind : MonoBehaviour
    {
        public KeyCode SelectedKey;
        public KeyCode AltSelectedKey;

        public bool EnableAltKey;

        bool waiting;

        private void Awake()
        {

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                waiting = !waiting;

                //StartCoroutine(WaitForKeyPress());
            }

            if (waiting && Input.anyKeyDown)
            {
                waiting = false;
                string strToCheck = Input.inputString.ToUpper();
                if (strToCheck != null || strToCheck != string.Empty)
                {
                    SelectedKey = (KeyCode)Enum.Parse(typeof(KeyCode), strToCheck);
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.LeftShift))
                        SelectedKey = KeyCode.LeftShift;
                }
                Console.Instance.Log(SelectedKey);
            }
        }

        IEnumerator WaitForKeyPress()
        {
            yield return new WaitForSeconds(0.25f);

            waiting = true;

            while (waiting)
            {
                if (Input.anyKeyDown)
                {
                    waiting = false;
                    SelectedKey = (KeyCode)Enum.Parse(typeof(KeyCode), Input.inputString);
                    Console.Instance.Log(SelectedKey);
                    yield break;
                }
            }
        }
    }
}
