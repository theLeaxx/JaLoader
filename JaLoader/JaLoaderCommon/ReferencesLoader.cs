using JaLoader.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JaLoader.Common
{
    public static class ReferencesLoader
    {
        internal static List<string> LoadedReferences = new List<string>();

        internal static bool LoadedAlready = false;
        public static bool CanLoadMods = false;

        public static IEnumerator LoadAssemblies()
        {
            if (RuntimeVariables.NoModsFlag)
            {
                RuntimeVariables.ModLoader.StartInitializeMods();
                yield break;
            }

            CanLoadMods = false;
            
            DebugUtils.SignalStartRefLoading();

            if (!Directory.Exists($@"{JaLoaderSettings.ModFolderLocation}\Assemblies"))
                Directory.CreateDirectory($@"{JaLoaderSettings.ModFolderLocation}\Assemblies");

            DirectoryInfo d = new DirectoryInfo($@"{JaLoaderSettings.ModFolderLocation}\Assemblies");
            List<FileInfo> asm = d.GetFiles("*.dll").ToList();

            for (int i = 0; i < asm.Count; i++)
            {
                for (int j = 0; j < LoadedReferences.Count; j++)
                {
                    if (asm[i].Name == LoadedReferences[j])
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
                    RuntimeVariables.Logger.ILogDebug("JaLoader", $"Loaded assembly {asmFile.Name}!");
                    loadedAsm++;

                    LoadedReferences.Add(asmFile.Name);

                }
                catch (Exception ex)
                {
                    RuntimeVariables.Logger.ILogError(ex);
                    RuntimeVariables.Logger.ILogError(asmFile.Name, $"Assembly {asmFile.Name} is not a valid assembly!");
                    validAsm--;
                }
            }
            CanLoadMods = true;

            if (LoadedAlready)
                yield break;

            if (loadedAsm == 1)
                RuntimeVariables.Logger.ILogMessage("JaLoader", $"1 assembly found and loaded!");
            else if (loadedAsm > 1)
                RuntimeVariables.Logger.ILogMessage("JaLoader", $"{loadedAsm} assemblies found and loaded!");
            
            LoadedAlready = true;

            DebugUtils.SignalFinishedRefLoading();

            RuntimeVariables.ModLoader.StartInitializeMods();
            yield return null;
        }
    }
}
