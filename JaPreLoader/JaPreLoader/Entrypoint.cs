using UnityEngine;
using JaLoader;
using System.Timers;
using Object = UnityEngine.Object;
using Timer = System.Timers.Timer;
using System.Reflection;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Doorstop
{
    class Entrypoint
    {
        private static Timer timer;
        static string unityVersion = "";
        static List<string> allLoaded = new List<string>();

        public static void Start()
        {
            string unityExePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            System.Diagnostics.FileVersionInfo fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(unityExePath);
            unityVersion = fileVersionInfo.ProductVersion;

            //UnityEngine.Debug.Log("Attempting to load!");
            //File.WriteAllText("doorstop_hello.log", unityVersion);

            if (unityVersion.StartsWith("4"))
            {
                File.WriteAllText("unity version starts with 4.log", unityVersion);

                System.Threading.Timer timedr = new System.Threading.Timer(TimerCallback, null, 3000, Timeout.Infinite);

                return;
            }

            timer = new Timer(3000);
            timer.Elapsed += RunTimer;
            timer.Enabled = true;
        }

        static void TimerCallback(object state)
        {
            // Code to be executed when the timer elapses

            // Your specific code after the delay
            File.WriteAllText("doorstop_timer.log", "Hello from Unity!");
            //Debug.Log("Unity 4 is not supported!");

            // Optionally: Restart the timer for periodic execution
            // timer.Change(interval, Timeout.Infinite);
        }

        private static void RunTimer(object sender, ElapsedEventArgs e)
        {
            GameObject obj = new GameObject();

            GameObject insObj = Object.Instantiate(obj);
            insObj.AddComponent<AddModLoaderComponent>();
            Object.DontDestroyOnLoad(insObj);

            timer.Enabled = false;
            timer.Dispose();
            timer.Elapsed -= RunTimer;
            timer = null;
        }
    }

    public class AddModLoaderComponent : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("JaLoader found!");
        }

        void Update()
        {
            if (!gameObject.GetComponent<ModLoader>())
            {
                gameObject.AddComponent<ModLoader>();
            }
            else
            {
                gameObject.name = "JaLoader";
                Destroy(this);
            }
        }
    }
}
