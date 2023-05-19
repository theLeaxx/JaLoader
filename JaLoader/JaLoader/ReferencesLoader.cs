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
            DirectoryInfo d = new DirectoryInfo($@"{settingsManager.ModFolderLocation}\Assemblies");
            FileInfo[] asm = d.GetFiles("*.dll");

            int validAsm = asm.Length;
            int loadedAsm = 0;

            foreach (FileInfo asmFile in asm)
            {
                try
                {
                    Assembly.LoadFrom(asmFile.FullName);
                    loadedAsm++;
                }
                catch (Exception ex)
                {
                    Console.Instance.Log(ex);
                    Console.Instance.Log(asmFile.Name, $"Assembly {asmFile.Name} is not a valid assembly!");
                    validAsm--;
                    throw;
                }
            }

            if (loadedAsm == 1)
                Console.Instance.LogMessage("JaLoader", $"1 assembly found and loaded!");
            else if (loadedAsm > 1)
                Console.Instance.LogMessage("JaLoader", $"{loadedAsm} assemblies found and loaded!");

            StartCoroutine(modLoader.LoadMods());
            yield return null;
        }
    }
}
