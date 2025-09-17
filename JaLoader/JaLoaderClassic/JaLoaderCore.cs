using JaLoader.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoaderClassic
{
    public class JaLoaderCore : MonoBehaviour
    {
        public static JaLoaderCore Instance { get; private set; }

        private void Start()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            }

            gameObject.name = "JaLoader";
            RuntimeVariables.ApplicationDataPath = Application.dataPath;

            gameObject.AddComponent<Console>();
            Application.LoadLevel(Application.loadedLevel + 1);
            Debug.Log("JaLoader Core initialized!");

            Console.InternalLog("Hello! JaLoaderCore is running!");

            StartCoroutine(SayHiLater());
        }

        private IEnumerator SayHiLater()
        {
            yield return new WaitForSeconds(3f);
            RuntimeVariables.Logger = Console.Instance;
            gameObject.AddComponent<ModLoader>();
            RuntimeVariables.ModLoader = GetComponent<ModLoader>();
            SettingsManager.Initialize();

            StartCoroutine(ReferencesLoader.LoadAssemblies());
            Console.InternalLogDebug("Hello again! JaLoaderCore is running!");
        }
    }
}
