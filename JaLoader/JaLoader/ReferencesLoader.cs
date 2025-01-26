using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JaLoader
{
    public class ReferencesLoader : MonoBehaviour
    {
        public static ReferencesLoader Instance { get; private set; }

        private ModLoader modLoader;
        private SettingsManager settingsManager;

        private List<string> loadedReferences = new List<string>();
        private bool loadedAlready = false;

        public bool canLoadMods = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            modLoader = ModLoader.Instance;
            settingsManager = SettingsManager.Instance;
        }

        public IEnumerator LoadAssemblies()
        {
            canLoadMods = false;
            Debug.Log("Loading external mod assemblies...");
            gameObject.GetComponent<Stopwatch>()?.StartCounting();

            if(!Directory.Exists($@"{settingsManager.ModFolderLocation}\Assemblies"))
                Directory.CreateDirectory($@"{settingsManager.ModFolderLocation}\Assemblies");

            DirectoryInfo d = new DirectoryInfo($@"{settingsManager.ModFolderLocation}\Assemblies");
            List<FileInfo> asm = d.GetFiles("*.dll").ToList();

            for (int i = 0; i < asm.Count; i++)
            {
                for (int j = 0; j < loadedReferences.Count; j++)
                {
                    if (asm[i].Name == loadedReferences[j])
                    {
                        asm.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }

            int validAsm = asm.Count;
            int loadedAsm = 0;

            foreach (FileInfo asmFile in asm)
            {
                try
                {
                    Assembly.LoadFrom(asmFile.FullName);
                    Debug.Log($"Loaded assembly {asmFile.Name}!");
                    loadedAsm++;

                    loadedReferences.Add(asmFile.Name);

                }
                catch (Exception ex)
                {
                    Console.LogError(ex);
                    Console.LogError(asmFile.Name, $"Assembly {asmFile.Name} is not a valid assembly!");
                    validAsm--;
                    throw;
                }
            }
            canLoadMods = true;

            if (loadedAlready)
                yield break;

            if (loadedAsm == 1)
                Console.LogMessage("JaLoader", $"1 assembly found and loaded!");
            else if (loadedAsm > 1)
                Console.LogMessage("JaLoader", $"{loadedAsm} assemblies found and loaded!");
            
            loadedAlready = true;

            gameObject.GetComponent<Stopwatch>().StopCounting();
            Debug.Log($"Loaded JaLoader assemblies! ({gameObject.GetComponent<Stopwatch>().timePassed}s)");

            StartCoroutine(modLoader.InitializeMods());
            yield return null;
        }
    }
}
