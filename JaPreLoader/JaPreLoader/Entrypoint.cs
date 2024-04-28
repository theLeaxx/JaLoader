using UnityEngine;
using JaLoader;
using Object = UnityEngine.Object;
using System.Reflection;
using System.IO;
using System;
using System.Windows.Forms;

namespace Doorstop
{
    class Entrypoint
    {
        static string unityVersion = "";
        static int i = 0;

        public static void Start()
        {
            string unityExePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            System.Diagnostics.FileVersionInfo fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(unityExePath);
            unityVersion = fileVersionInfo.ProductVersion;

            if (unityVersion.StartsWith("4"))
            {
                MessageBox.Show("JaLoader is currently not compatible with versions of Jalopy prior to v1.1!", "JaLoader", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoadUnity4;
            }
            else
            {
                AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoadUnity5;
            }
        }

        static void OnAssemblyLoadUnity4(object sender, AssemblyLoadEventArgs args)
        {
            i++;

            if (i == 17)
            {
                GameObject obj = (GameObject)Object.Instantiate(new GameObject());
                Debug.Log("CREATED OBJECT");
                obj.name = "TEST TEST TEST";
                obj.AddComponent<AddModLoaderComponentUnity4>();
                Object.DontDestroyOnLoad(obj);
                Debug.Log("ADDED COMPONENT");
            }
        }

        static void OnAssemblyLoadUnity5(object sender, AssemblyLoadEventArgs args)
        {
            i++;

            if(i == 66)
            {
                Assembly.LoadFile($@"{UnityEngine.Application.dataPath}\Managed\JaLoader.dll");

                GameObject obj = (GameObject)Object.Instantiate(new GameObject());
                obj.AddComponent<AddModLoaderComponent>();
                Object.DontDestroyOnLoad(obj);
            } 
        }
    }

    public class AddModLoaderComponent : MonoBehaviour
    {
        private EventInfo logMessageReceivedEvent;
        private Delegate delegateInstance;

        void Awake()
        {
            Debug.Log("JaLoader found!");

            // alternative to
            //UnityEngine.Application.logMessageReceived += LogMessageReceived;
            // using reflection, since it doesnt exist in unity 4 and below, but required for unity 5 and above

            Type applicationType = typeof(UnityEngine.Application);

            logMessageReceivedEvent = applicationType.GetEvent("logMessageReceived");

            Type delegateType = logMessageReceivedEvent.EventHandlerType;

            delegateInstance = Delegate.CreateDelegate(delegateType, this, typeof(AddModLoaderComponent).GetMethod("LogMessageReceived"));

            logMessageReceivedEvent.AddEventHandler(null, delegateInstance);
        }

        public void LogMessageReceived(string message, string stack, LogType type)
        {
            if (message == "Received stats and achievements from Steam\n" && type == LogType.Log)
            {
                GameObject obj = (GameObject)Instantiate(new GameObject());
                obj.AddComponent<ModLoader>();
                Debug.Log("ADDED JALOADER");

                //UnityEngine.Application.logMessageReceived -= LogMessageReceived;
                logMessageReceivedEvent.RemoveEventHandler(null, delegateInstance);
            }
        }    
    }

    public class AddModLoaderComponentUnity4 : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("Compatibility mode enabled! JaLoader is not yet fully stable on Unity 4, please report any issues on GitHub!");
            Debug.Log("JaLoader found! (Compatibility mode, Unity 4 detected)");

            
        }
    }
}