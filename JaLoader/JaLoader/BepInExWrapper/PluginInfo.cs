using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BepInEx
{
    public static class PluginInfo
    {
        public const string PLUGIN_GUID = "PLUGIN_GUID";
        public const string PLUGIN_NAME = "PLUGIN_NAME";
        public const string PLUGIN_VERSION = "1.0.0";
    }
}

namespace JaLoader.BepInExWrapper
{
    public class ModInfo : MonoBehaviour
    {
        public string GUID;
        public string Name;
        public string Version;
    }
}
