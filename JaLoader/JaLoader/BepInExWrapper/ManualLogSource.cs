using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace BepInEx.Logging
{
    public class ManualLogSource
    {
        public string Name { get; }

        public void Log(object message)
        {
            JaLoader.Console.Log(message);
        }

        public void LogFatal(object message)
        {
            JaLoader.Console.LogError(message);
        }

        public void LogError(object message)
        {
            JaLoader.Console.LogError(message);
        }

        public void LogWarning(object message)
        {
            JaLoader.Console.LogWarning(message);
        }

        public void LogMessage(object message)
        {
            JaLoader.Console.Log(message);
        }

        public void LogDebug(object message)
        {
            JaLoader.Console.LogDebug(message);
        }

        public void LogInfo(object message)
        {
            //JaLoader.Console.LogDebug(message);
        }

        public ManualLogSource(string name)
        {
            Name = name;
        }
    }
}
