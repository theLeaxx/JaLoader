using System;
using System.Diagnostics;

namespace JaLoader.Common
{
    public static class DebugUtils
    {
        private readonly static Stopwatch _stopwatch = new Stopwatch();
        internal static double timePassed = 0;
        internal static double totalTimePassed = 0;

        internal static void SignalFinishedLoading()
        {
            StopCounting();

            RuntimeVariables.Logger.ILogDebug("JaLoader", $"Loaded JaLoader mods! ({timePassed}s)");
            RuntimeVariables.Logger.ILogDebug("JaLoader", $"JaLoader successfully loaded! ({totalTimePassed}s)");
        }

        internal static void SignalFinishedInit()
        {
            StopCounting();
            RuntimeVariables.Logger.ILogDebug("JaLoader", $"Finished initializing JaLoader mods! ({timePassed}s)");
        }

        internal static void SignalStartInit()
        {
            RuntimeVariables.Logger.ILogDebug("JaLoader", "Initializing JaLoader mods...");
            StartCounting();
        }
        internal static void SignalStartUI()
        {
            RuntimeVariables.Logger.ILogDebug("JaLoader", "Loading JaLoader UI...");
            StartCounting();
        }
        internal static void SignalFinishedUI()
        {
            StopCounting();
            RuntimeVariables.Logger.ILogDebug("JaLoader", $"Loaded JaLoader UI! ({timePassed}s)");
        }

        internal static void SignalStartRefLoading()
        {
            RuntimeVariables.Logger.ILogDebug("JaLoader", "Loading external mod assemblies...");
            StartCounting();
        }

        internal static void SignalFinishedRefLoading()
        {
            StopCounting();
            RuntimeVariables.Logger.ILogDebug("JaLoader", $"Loaded JaLoader assemblies! ({timePassed}s)");
        }

        internal static void StartCounting()
        {
            _stopwatch.Stop();
            _stopwatch.Reset();
            _stopwatch.Start();
        }

        internal static void StopCounting()
        {
            _stopwatch.Stop();

            timePassed = Math.Round(_stopwatch.Elapsed.TotalSeconds, 3);

            totalTimePassed += timePassed;
            totalTimePassed = Math.Round(totalTimePassed, 3);
        }
    }
}
