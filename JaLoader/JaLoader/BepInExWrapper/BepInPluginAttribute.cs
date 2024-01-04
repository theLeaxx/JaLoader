using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BepInEx
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class BepInPlugin : Attribute
    {
        public string GUID { get; }
        public string Name { get; }

        public string Version { get; }

        public BepInPlugin(string guid, string name, string ver)
        {
            GUID = guid;
            Name = name;
            Version = ver;
        }
    }
}
