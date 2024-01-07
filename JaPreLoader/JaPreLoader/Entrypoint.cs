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
using System.Windows.Forms;

// THE FUCKING PRELOADER DOESN'T LOAD EVERYTIME AGAIN
// TODO: find a better, reliable way to inject the main dll, maybe merge the two DLLs together?
namespace Doorstop
{
    class Entrypoint
    {
        private static Timer timer;
        static string unityVersion = "";

        public static void Start()
        {
            string unityExePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            System.Diagnostics.FileVersionInfo fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(unityExePath);
            unityVersion = fileVersionInfo.ProductVersion;

            if (unityVersion.StartsWith("4"))
            {
                MessageBox.Show("JaLoader is currently not compatible with versions of Jalopy prior to v1.1!", "JaLoader", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            timer = new Timer(3000);
            timer.Elapsed += RunTimer;
            timer.Enabled = true;
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