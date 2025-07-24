using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    public class DebugUtils : MonoBehaviour
    {
        private void Update()
        {
            if (!SettingsManager.DebugMode)
                return;

            if (Input.GetKeyDown(KeyCode.F5))
                SettingsManager.ReadSettings();
        }
    }
}
