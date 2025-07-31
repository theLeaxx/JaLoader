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
                MessageBox.Show("JaLoader is not yet fully compatible with versions of Jalopy prior to v1.1!", "JaLoader", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoadUnity4;

                Assembly.LoadFile($@"{UnityEngine.Application.dataPath}\Managed\JaLoader.Common.dll");
                Assembly.LoadFile($@"{UnityEngine.Application.dataPath}\Managed\JaLoaderClassic.dll");

                GameObject obj = (GameObject)Object.Instantiate(new GameObject());
                Debug.Log("Created object");
                obj.name = "JaPreLoader";
                obj.AddComponent<AddJaLoaderClassicCore>();
                Object.DontDestroyOnLoad(obj);
            }
        }

        static void OnAssemblyLoadUnity5(object sender, AssemblyLoadEventArgs args)
        {
            i++;

            if(i == 66)
            {
                AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoadUnity5;

                Assembly.LoadFile($@"{UnityEngine.Application.dataPath}\Managed\JaLoader.Common.dll");
                Assembly.LoadFile($@"{UnityEngine.Application.dataPath}\Managed\JaLoader.dll");

                GameObject obj = (GameObject)Object.Instantiate(new GameObject());
                obj.AddComponent<AddJaLoaderCore>();
                Object.DontDestroyOnLoad(obj);
            } 
        }
    }

    public class AddJaLoaderCore : MonoBehaviour
    {
        private EventInfo logMessageReceivedEvent;
        private Delegate delegateInstance;

        void Awake()
        {
            Debug.Log("JaLoader found!");

            var jaLoaderDll = $"{UnityEngine.Application.dataPath}/Managed/JaLoader.dll";
            var jaLoaderDllFileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(jaLoaderDll).FileVersion;

            Debug.Log($"JaLoader version: {jaLoaderDllFileVersion}");

            Assembly unityAssembly = Assembly.Load("UnityEngine.CoreModule");

            Type applicationType = unityAssembly.GetType("UnityEngine.SceneManagement.SceneManager", false, true);

            var sceneLoadedEvent = applicationType.GetEvent("sceneLoaded");

            Type delegateType = sceneLoadedEvent.EventHandlerType;

            var delegateInstance = Delegate.CreateDelegate(delegateType, this, typeof(AddJaLoaderCore).GetMethod("LoadModLoader"));

            sceneLoadedEvent.AddEventHandler(null, delegateInstance);
        }

        public void LoadModLoader(object scene, object loadSceneMode)
        {
            GameObject obj = (GameObject)Instantiate(new GameObject());

            // we have to use reflection to add the mod loader component to the object
            // since we can't reference the JaLoader assembly directly, due to it being built on Unity 5, which separates UnityEngine.dll into multiple assemblies
            // and Unity 4 and below has UnityEngine.dll as a single assembly
            // using obj.AddComponent<ModLoader>() would cause this entire script to fail to load on Unity 4 and below

            Assembly assembly = null;

            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a.GetName().Name == "JaLoader")
                {
                    assembly = a;
                    break;
                }
            }

            Type modLoaderType = assembly.GetType("JaLoader.JaLoaderCore");

            MethodInfo addComponentMethod = typeof(GameObject).GetMethod("AddComponent", new[] { typeof(Type) });

            addComponentMethod.Invoke(obj, new object[] { modLoaderType });
        }
    }

    public class AddJaLoaderClassicCore : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("Compatibility mode enabled! JaLoader is not yet fully stable on Unity 4, please report any issues on GitHub!");
            Debug.Log("JaLoader found! (Compatibility mode, Unity 4 detected)");

            GameObject obj = (GameObject)Object.Instantiate(new GameObject());

            Assembly assembly = null;

            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a.GetName().Name == "JaLoaderClassic")
                {
                    assembly = a;
                    break;
                }
            }

            Type modLoaderType = assembly.GetType("JaLoaderClassic.JaLoaderCore");

            MethodInfo addComponentMethod = typeof(GameObject).GetMethod("AddComponent", new[] { typeof(Type) });

            addComponentMethod.Invoke(gameObject, new object[] { modLoaderType });
        }
    }
}