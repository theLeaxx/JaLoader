using JaLoader.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace JaLoaderClassic
{
    public static class ModManager
    {
        internal static Dictionary<MonoBehaviour, ModDataForManager> Mods = new Dictionary<MonoBehaviour, ModDataForManager>();
        internal static int ModFilesCount = 0;

        internal static List<MonoBehaviour> loadOrderList = new List<MonoBehaviour>();

        public static bool FinishedLoadingMenuMods;
        internal static bool reloadGameModsRequired = false;

        internal static void LoadMods()
        {
            if (Mods.Count == 0)
            {
                Console.InternalLogMessage("JaLoader", $"No mods found!");
                // Console.Instance.ToggleVisibility(true);

                //if (ModFilesCount == 0)
                //    UIManager.Instance.NoMods();

                DebugUtils.SignalFinishedLoading();

                return;
            }

            //LoadDisabledStatus();

            //UIManager.Instance.ModsCountText.text = $"{Mods.Count} mods installed";

            foreach (var script in Mods)
            {
                if (script.Value.InitTime == WhenToInit.InMenu)
                    FinishLoadingMod(script.Key);
                else
                    FinishLoadingMod(script.Key, false);
            }

            FinishedLoadingMenuMods = true;

            DebugUtils.SignalFinishedLoading();
            //UIManager.Instance.WriteConsoleStartMessage();

            //CheckAllModsForUpdates();
        }

        internal static void AddMod(MonoBehaviour mod, WhenToInit initTime, /*Text displayText,*/ GenericModData data)
        {
            if (Mods.ContainsKey(mod))
            {
                Debug.LogWarning($"Mod {mod.name} is already registered in the ModManager.");
                return;
            }

            Mods.Add(mod, new ModDataForManager(initTime, true, /*displayText,*/ data));
            loadOrderList.Add(mod);
        }

        internal static void FinishLoadingMod(MonoBehaviour script, bool enableMod = true)
        {
            bool anyError = false;
            Exception ex = null;
            string modName = "";
            bool isBepInEx = false;

            if (script is ModClassic mod)
            {
                //CheckForDependencies(mod);

                //CheckForIncompatibilities(mod);

                try
                {
                    modName = mod.ModName;

                    mod.Preload();

                    mod.EventsDeclaration();

                    mod.SettingsDeclaration();

                    mod.CustomObjectsRegistration();

                    if (mod.settingsIDS.Count > 0)
                    {
                        //mod.LoadModSettings();
                        //mod.SaveModSettings();
                    }

                    Debug.Log($"Part 2/2 of initialization for mod {mod.ModName} completed");

                    if (!enableMod)
                        return;

                    if (Mods[mod].IsEnabled)
                        mod.gameObject.SetActive(true);

                    Debug.Log($"Loaded mod {mod.ModName}");
                }
                catch (Exception _ex)
                {
                    anyError = true;
                    ex = _ex;
                }

            }

            if (anyError)
            {
                script.gameObject.SetActive(false);

                Debug.Log($"Part 2/2 of initialization for {(isBepInEx == true ? "BepInEx " : "")}mod {modName} failed");
                Debug.Log($"Failed to load {(isBepInEx == true ? "BepInEx " : "")}mod {modName}. An error occoured while enabling the mod.");
                Debug.Log(ex);

                Console.InternalLogError("JaLoader", $"An error occured while trying to load {(isBepInEx == true ? "BepInEx " : "")}mod \"{modName}\"");

                Mods.Remove(script);
            }
        }
    }

    internal class ModDataForManager
    {
        public WhenToInit InitTime { get; set; }
        public bool IsEnabled { get; set; }
        //public Text DisplayText { get; set; }

        public GenericModData GenericModData { get; }

        public ModDataForManager(WhenToInit initTime, bool isEnabled, /*Text displayText,*/ GenericModData data)
        {
            InitTime = initTime;
            IsEnabled = isEnabled;
            //DisplayText = displayText;
            GenericModData = data;
        }
    }

    public class GenericModData
    {
        public string ModID { get; set; }
        public string ModName { get; set; }
        public string ModVersion { get; set; }
        public string ModDescription { get; set; }
        public string ModAuthor { get; set; }
        public string GitHubLink { get; set; }
        public MonoBehaviour Mod { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsBepInExMod { get; set; }

        public GenericModData(string modID, string modName, string modVersion, string modDescription, string modAuthor, string gitHubLink, MonoBehaviour mod, bool isEnabled = true, bool isBIXMod = false)
        {
            ModID = modID;
            ModName = modName;
            ModVersion = modVersion;
            ModDescription = modDescription;
            ModAuthor = modAuthor;
            GitHubLink = gitHubLink;
            Mod = mod;
            IsEnabled = isEnabled;
            IsBepInExMod = isBIXMod;
        }

        public override string ToString()
        {
            return $"[ModMetadata: ID={ModID}, ModName='{ModName}', ModAuthor='{ModAuthor}']";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return Equals(obj as GenericModData);
        }

        public bool Equals(GenericModData other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return ModID == other.ModID &&
                   ModName.Equals(other.ModName, StringComparison.OrdinalIgnoreCase) &&
                   ModAuthor.Equals(other.ModAuthor, StringComparison.OrdinalIgnoreCase) &&
                   ReferenceEquals(Mod, other.Mod);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + ModID.GetHashCode();
                hash = hash * 23 + (ModName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(ModName) : 0);
                hash = hash * 23 + (ModAuthor != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(ModAuthor) : 0);
                hash = hash * 23 + (Mod != null ? Mod.GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(GenericModData left, GenericModData right)
        {
            if (left is null)
                return right is null;

            return left.Equals(right);
        }

        public static bool operator !=(GenericModData left, GenericModData right)
        {
            return !(left == right);
        }

    }
}
