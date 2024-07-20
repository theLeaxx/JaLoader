using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    public class EnableCursorOnEnable : MonoBehaviour
    {
        private void OnEnable()
        {
            Cursor.visible = true;
            FindObjectOfType<VfCursorManager>().Cursors[0].SetActive(true);
            FindObjectOfType<VfCursorManager>().SetCursor(0);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void OnDisable()
        {
            FindObjectOfType<VfCursorManager>().Cursors[0].SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
