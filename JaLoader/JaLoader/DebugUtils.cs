using JaLoader.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace JaLoader
{
    public class DebugUtils : MonoBehaviour
    {
        private static bool counting = false;
        private static float timePassedRaw = 0;
        public static double timePassed = 0;
        public static double totalTimePassed = 0;

        private void Update()
        {
            if (counting)
                timePassedRaw += Time.deltaTime;

            if (!JaLoaderSettings.DebugMode)
                return;

            if (Input.GetKeyDown(KeyCode.F5))
                JaLoaderSettings.ReadSettings();
        }

        internal static void SignalFinishedLoading()
        {
            StopCounting();
            Debug.Log($"Loaded JaLoader mods! ({timePassed}s)");
            Debug.Log($"JaLoader successfully loaded! ({totalTimePassed}s)");
        }

        internal static void SignalFinishedInit()
        {
            StopCounting();
            Debug.Log($"Finished initializing JaLoader mods! ({timePassed}s)");
        }

        internal static void SignalStartInit()
        {
            Debug.Log("Initializing JaLoader mods...");
            StartCounting();
        }

        internal static void SignalStartUI()
        {
            Debug.Log("Loading JaLoader UI...");
            StartCounting();
        }

        internal static void SignalFinishedUI()
        {
            StopCounting();
            Debug.Log($"Loaded JaLoader UI! ({timePassed}s)");
        }

        internal static void SignalStartRefLoading()
        {
            Debug.Log("Loading external mod assemblies...");
            StartCounting();
        }

        internal static void SignalFinishedRefLoading()
        {
            StopCounting();
            Debug.Log($"Loaded JaLoader assemblies! ({timePassed}s)");
        }

        internal static void StartCounting()
        {
            counting = true;
            timePassed = 0;
            timePassedRaw = 0;
        }

        internal static void StopCounting()
        {
            counting = false;
            timePassed = Math.Round(timePassedRaw, 3);

            totalTimePassed += timePassed;
            totalTimePassed = Math.Round(totalTimePassed, 3);
        }
    }
}
