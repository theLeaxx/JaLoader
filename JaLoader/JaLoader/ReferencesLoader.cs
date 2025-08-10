using JaLoader.Common;
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
        }

        public IEnumerator LoadAssemblies()
        {
            canLoadMods = false;
            
            DebugUtils.SignalStartRefLoading();

            if (!Directory.Exists($@"{JaLoaderSettings.ModFolderLocation}\Assemblies"))
                Directory.CreateDirectory($@"{JaLoaderSettings.ModFolderLocation}\Assemblies");

            DirectoryInfo d = new DirectoryInfo($@"{JaLoaderSettings.ModFolderLocation}\Assemblies");
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

            DebugUtils.SignalFinishedRefLoading();

            StartCoroutine(ModLoader.Instance.InitializeMods());
            yield return null;
        }
    }
}
