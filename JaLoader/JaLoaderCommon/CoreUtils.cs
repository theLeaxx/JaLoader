using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace JaLoader.Common
{
    public static class CoreUtils
    {
        public static string AnyMissingDLLs()
        {
            string[] requiredDLLs = new string[]
            {
                "0Harmony.dll",
                "HarmonyXInterop.dll",
                "NLayer.dll",
                "Mono.Cecil.dll",
                "Mono.Cecil.Mdb.dll",
                "Mono.Cecil.Pdb.dll",
                "Mono.Cecil.Rocks.dll",
                "MonoMod.Backports.dll",
                "MonoMod.RuntimeDetour.dll",
                "MonoMod.Utils.dll",
                "MonoMod.ILHelpers.dll"
            };

            var path = $@"{RuntimeVariables.ApplicationDataPath}\Managed";

            foreach (string dll in requiredDLLs)
            {
                if (!File.Exists($@"{path}\{dll}"))
                    return dll;
            }

            return "None";
        }
    }
}
