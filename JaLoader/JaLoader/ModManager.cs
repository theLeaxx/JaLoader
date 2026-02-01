using BepInEx;
using BepInEx.Configuration;
using JaLoader.BepInExWrapper;
using JaLoader.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Application = UnityEngine.Application;
using System.Web.Script.Serialization;

namespace JaLoader
{
    public static class ModManager
    {
        internal static Dictionary<MonoBehaviour, ModDataForManager> Mods = new Dictionary<MonoBehaviour, ModDataForManager>();
        internal static int ModFilesCount = 0;

        internal static List<MonoBehaviour> loadOrderList = new List<MonoBehaviour>();

        public static bool FinishedLoadingMenuMods;
        internal static bool reloadGameModsRequired = false;

        internal static void Initialize()
        {
            EventsManager.Instance.OnModsInitialized += LoadModOrder;
            EventsManager.Instance.OnGameLoad += EnableGameMods;
            EventsManager.Instance.OnGameUnload += UnloadGameMods;
            EventsManager.Instance.OnMenuLoad += UnloadGameMods;
        }

        internal static void RemoveAllNullMods()
        {
            var nullMods = Mods.Where(kv => kv.Key == null).Select(kv => kv.Key).ToList();
            foreach (var mod in nullMods)
            {
                Mods.Remove(mod);
                loadOrderList.Remove(mod);
            }

            var nullGenericMods = Mods.Where(kv => kv.Value.GenericModData.Mod == null).Select(kv => kv.Key).ToList();
            foreach (var mod in nullGenericMods)
                if (mod != null)
                    Mods[mod].GenericModData.Mod = mod;

            Console.LogDebug("JaLoader", $"Removed {nullMods.Count + nullGenericMods.Count} null mods caused by reloading from ModManager.");
        }

        internal static void UnloadGameMods()
        {
            RemoveAllNullMods();

            if (!FinishedLoadingMenuMods)
                return;

            reloadGameModsRequired = true;

            foreach (var script in Mods)
                if (script.Value.InitTime == Common.WhenToInit.InGame && script.Value.IsEnabled)
                    script.Key.gameObject.SetActive(false);
        }

        internal static void ReloadGameModsIfNecessary()
        {
            reloadGameModsRequired = true;
        }

        internal static void EnableGameMods()
        {
            if (reloadGameModsRequired)
            {
                CoroutineManager.StartStaticCoroutine(WaitForLaikaThenReload());
                return;
            }

            foreach (var script in Mods)
                if (script.Value.InitTime == Common.WhenToInit.InGame && script.Value.IsEnabled)
                    script.Key.gameObject.SetActive(true);
        }

        private static IEnumerator WaitForLaikaThenReload()
        {
            while (ModHelper.Instance.laika == null)
                yield return null;

            ReloadMods();

            yield break;
        }

        internal static void ReloadMods()
        {
            CoroutineManager.StartStaticCoroutine(ModLoader.Instance.ReloadAllMods());
        }

        internal static void LoadMods()
        {
            if (Mods.Count == 0)
            {
                Console.LogMessage("JaLoader", $"No mods found!");
                Console.Instance.ToggleVisibility(true);

                if(ModFilesCount == 0)
                    UIManager.Instance.NoMods();

                DebugUtils.SignalFinishedLoading();

                return;
            }

            LoadDisabledStatus();

            UIManager.Instance.ModsCountText.text = $"{Mods.Count} mods installed";

            foreach (var script in Mods)
            {
                if(script.Value.InitTime == Common.WhenToInit.InMenu)
                    FinishLoadingMod(script.Key);
                else
                    FinishLoadingMod(script.Key, false);
            }

            FinishedLoadingMenuMods = true;

            DebugUtils.SignalFinishedLoading();
            UIManager.Instance.WriteConsoleStartMessage();

            CheckAllModsForUpdates();
        }

        public static bool IsModEnabled(string author, string ID, string name = "")
        {
            if (string.IsNullOrEmpty(ID) || string.IsNullOrEmpty(author))
            {
                Console.LogError("JaLoader", "Invalid parameters provided to IsModEnabled. ID and author cannot be null or empty.");
                throw new ModException("Invalid parameters provided to IsModEnabled. ID and author cannot be null or empty.", "", 104);
            }

            MonoBehaviour mod = FindMod(author, ID, name, ignoreNull: true);
            if (mod == null)
                return false;

            return Mods[mod].IsEnabled;
        }

        internal static void CheckAllModsForUpdates()
        {
            if (UpdateUtils.CanCheckForUpdates() && Mods.Count > 0)
            {
                var modsNeedUpdate = 0;

                foreach (var script in Mods.Keys)
                {
                    if (script is Mod mod)
                    {
                        if (UpdateUtils.CheckForModUpdate(mod, out string latestVersion))
                        {
                            modsNeedUpdate++;
                            UIManager.Instance.ShowUpdateAvailableForMod(Mods[mod].GenericModData, latestVersion);
                        }
                    }
                }

                if(modsNeedUpdate > 0)
                    Console.LogMessage("JaLoader", $"{modsNeedUpdate} mods have available updates!");
            }
        }

        internal static int GetDisabledModsCount()
        {
            return JaLoaderSettings.DisabledMods.Count;
        }

        internal static int GetBepinExModsCount()
        {
            return Mods.Values.Count(data => data.GenericModData.IsBepInExMod);
        }

        internal static void FinishLoadingMod(MonoBehaviour script, bool enableMod = true)
        {
            bool anyError = false;
            Exception ex = null;
            string modName = "";
            bool isBepInEx = false;

            if (script is Mod mod)
            {
                CheckForDependencies(mod);

                CheckForIncompatibilities(mod);

                try
                {
                    modName = mod.ModName;

                    mod.Preload();

                    mod.EventsDeclaration();

                    mod.SettingsDeclaration();

                    mod.CustomObjectsRegistration();

                    if (mod.settingsIDS.Count > 0)
                    {
                        mod.LoadModSettings();
                        mod.SaveModSettings();
                    }

                    Console.LogDebug("JaLoader", $"Part 2/2 of initialization for mod {mod.ModName} completed");

                    if (!enableMod)
                        return;

                    if (Mods[mod].IsEnabled)
                        mod.gameObject.SetActive(true);

                    Console.LogDebug("JaLoader", $"Loaded mod {mod.ModName}");
                }
                catch (Exception _ex)
                {
                    anyError = true;
                    ex = _ex;
                }

            }
            else if (script is BaseUnityPlugin plugin)
            {
                isBepInEx = true;
                ModInfo modInfo = plugin.gameObject.GetComponent<ModInfo>();
                modName = modInfo.Name;

                try
                {
                    plugin.InstantiateBIXPluginSettings();

                    Console.LogDebug("JaLoader", $"Part 2/2 of initialization for BepInEx mod {modInfo.Name} completed");

                    if (Mods[plugin].IsEnabled)
                        plugin.gameObject.SetActive(true);

                    foreach (IConfigEntry entry in plugin.configEntries.Keys)
                    {
                        if (entry is ConfigEntry<bool> boolEntry)
                        {
                            bool value = boolEntry._typedValue;
                            plugin.AddBIXPluginToggle(plugin.configEntries[entry].Item1, plugin.configEntries[entry].Item2, value);
                        }
                        else if (entry is ConfigEntry<KeyboardShortcut> keyEntry)
                        {
                            KeyboardShortcut keyboardShortcut = keyEntry._typedValue;
                            plugin.AddBIXPluginKeybind(plugin.configEntries[entry].Item1, plugin.configEntries[entry].Item2, keyboardShortcut.Key);
                        }
                    }

                    if (plugin.configEntries.Count > 0)
                        plugin.LoadBIXPluginSettings();

                    Console.LogDebug("JaLoader", $"Loaded BepInEx mod {modInfo.Name}");

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

                Console.LogError("JaLoader", $"Part 2/2 of initialization for {(isBepInEx == true ? "BepInEx " : "")}mod {modName} failed");
                Console.LogError("JaLoader", $"Failed to load {(isBepInEx == true ? "BepInEx " : "")}mod {modName}. An error occoured while enabling the mod.");
                Console.LogError("JaLoader", ex);

                Mods.Remove(script);
            }
        }

        internal static void LoadDisabledStatus()
        {
            foreach (var mod in JaLoaderSettings.DisabledMods)
            {
                string[] modInfo = mod.Split('_');

                var foundMod = FindMod(modInfo[0], modInfo[1], modInfo[2], true);

                if (foundMod != null)
                    ToggleMod(Mods[foundMod].GenericModData, true, forceStatusDisabled: true);
            }
        }

        internal static void AddMod(MonoBehaviour mod, Common.WhenToInit initTime, Text displayText, GenericModData data)
        {
            if (Mods.ContainsKey(mod))
            {
                Console.LogWarning("JaLoader", $"Mod {mod.name} is already registered in the ModManager.");
                return;
            }

            Mods.Add(mod, new ModDataForManager(initTime, true, displayText, data));
            loadOrderList.Add(mod);
        }

        internal static void ToggleMod(GenericModData data, bool dontSave = false, bool forceStatusDisabled = false)
        {
            var modEntry = UIManager.Instance.modEntries[data];
            var toggleBtn = UIManager.Instance.modEntries[data].transform.Find("Buttons").Find("ToggleButton").GetComponent<Button>().GetComponentInChildren<Text>();

            var managerData = GetManagerDataFromGeneric(data);

            bool enabledStatus = data.IsEnabled;
            if(forceStatusDisabled)
                enabledStatus = true;

            if (enabledStatus)
            {
                data.IsEnabled = managerData.IsEnabled = false;
                toggleBtn.text = "Enable";
                modEntry.transform.Find("BasicInfo").Find("ModName").GetComponent<Text>().color = CommonColors.DisabledModColor.ToColor();
                modEntry.transform.Find("BasicInfo").Find("ModAuthor").GetComponent<Text>().color = CommonColors.DisabledModColor.ToColor();

                if (managerData.InitTime == Common.WhenToInit.InMenu)
                    managerData.GenericModData.Mod.gameObject.SetActive(false);
            }
            else if (!enabledStatus)
            {
                data.IsEnabled = managerData.IsEnabled = true;
                toggleBtn.text = "Disable";
                modEntry.transform.Find("BasicInfo").Find("ModName").GetComponent<Text>().color = CommonColors.EnabledModColor.ToColor();
                modEntry.transform.Find("BasicInfo").Find("ModAuthor").GetComponent<Text>().color = CommonColors.EnabledModColor.ToColor();

                if (managerData.InitTime == Common.WhenToInit.InMenu)
                    managerData.GenericModData.Mod.gameObject.SetActive(true);
            }

            if(!dontSave)
                SettingsManager.SaveSettings(true);
        }

        internal static void CheckForIncompatibilities(Mod mod)
        {
            if (mod.Incompatibilities.Count <= 0)
                return;

            foreach (var incompatibility in mod.Incompatibilities)
            {
                var foundMod = FindMod(incompatibility.Item2, incompatibility.Item1, ignoreNull: true);

                if (foundMod == null)
                    continue;

                var incompatibleMod = (Mod)foundMod;

                //version is of format x.y.z-a.b.c, representing a range of versions from x.y.z to a.b.c
                // separate it into 2 ints, one for the lower bound and one for the upper bound

                string[] version = incompatibility.Item3.Split('-');
                int lowerBound = int.Parse(version[0].Replace(".", ""));
                int upperBound = int.Parse(version[1].Replace(".", ""));
                int modVersion = int.Parse(incompatibleMod.ModVersion.Replace(".", ""));

                if (modVersion >= lowerBound && modVersion <= upperBound)
                {
                    UIManager.Instance.ShowNotice("Incompatibility detected", $"The mod \"{mod.ModName}\" is incompatible with the mod \"{incompatibleMod.ModName}\". The mod may still load, but not function correctly.");
                    UIManager.Instance.AddWarningToMod(UIManager.Instance.modEntries[Mods[mod].GenericModData], $"Incompatible with {incompatibleMod.ModName}!");
                }
            }
        }

        internal static void CheckForDependencies(Mod mod)
        {
            if (mod.Dependencies.Count <= 0)
                return;

            foreach (var dependency in mod.Dependencies)
            {
                int version = int.Parse(dependency.Item3.Replace(".", ""));

                if (dependency.Item1 == "JaLoader" && dependency.Item2 == "Leaxx")
                {
                    if (JaLoaderSettings.GetVersion() < version)
                    {
                        UIManager.Instance.ShowNotice("Dependency required", $"The mod \"{mod.ModName}\" requires JaLoader version {dependency.Item3} or higher. You are currently using version {JaLoaderSettings.GetVersionString()}. The mod may still load, but not function correctly.");

                        UIManager.Instance.AddWarningToMod(UIManager.Instance.modEntries[Mods[mod].GenericModData], $"Requires JaLoader >= {dependency.Item3}!");
                    }

                    continue;
                }

                var foundMod = FindMod(dependency.Item2, dependency.Item1, ignoreNull: true);

                if (foundMod == null)
                {
                    UIManager.Instance.ShowNotice("Dependency required", $"The mod \"{mod.ModName}\" requires the mod \"{dependency.Item1}\" to be installed. The mod may still load, but not function correctly.");

                    UIManager.Instance.AddWarningToMod(UIManager.Instance.modEntries[Mods[mod].GenericModData], $"Requires {dependency.Item1}!");

                    continue;
                }

                var dependentMod = (Mod)foundMod;

                if (dependentMod.ModVersion != dependency.Item3)
                {
                    UIManager.Instance.ShowNotice("Dependency required", $"The mod \"{mod.ModName}\" requires the mod \"{dependency.Item1}\" to be version {dependency.Item3} or higher. You are currently using version {dependentMod.ModVersion}. The mod may still load, but not function correctly.");

                    UIManager.Instance.AddWarningToMod(UIManager.Instance.modEntries[Mods[mod].GenericModData], $"Requires {dependency.Item1} >= {dependency.Item3}!");
                }

                if(loadOrderList.IndexOf(dependentMod) > loadOrderList.IndexOf(mod))
                {
                    UIManager.Instance.ShowNotice("Dependency required", $"The mod \"{mod.ModName}\" requires the mod \"{dependency.Item1}\" to be loaded before it. Adjust its load order in the mods list.");
                    UIManager.Instance.AddWarningToMod(UIManager.Instance.modEntries[Mods[mod].GenericModData], $"Requires {dependency.Item1} to be loaded before it!");
                }
            }
        }

        internal static ModDataForManager GetManagerDataFromGeneric(GenericModData data)
        {
            if (Mods.TryGetValue(data.Mod, out ModDataForManager modData))
            {
                return modData;
            }
            else
            {
                Console.LogError("JaLoader", $"Mod {data.ModName} not found in ModManager.");
                return null;
            }
        }

        /// <summary>
        /// Searches for a mod with the specified ID, name and author and returns it if found, otherwise returns null.
        /// </summary>
        /// <param name="author">The mod's author</param>
        /// <param name="ID">The mod's ID</param>
        /// <param name="name">The mod's name</param>
        /// <param name="ignoreNull">Do not throw an error if nothing is found</param>
        /// <returns>The searched Mod if found, otherwise null</returns>
        public static MonoBehaviour FindMod(string author, string ID, string name = "", bool ignoreNull = false)
        {
            if (string.IsNullOrEmpty(author) || string.IsNullOrEmpty(ID))
            {
                Console.LogError("JaLoader", "Invalid parameters provided to FindMod.");
                throw new ModException("Invalid parameters provided to FindMod. Author and ID cannot be null or empty.", "", 104);
            }

            if (author == "BepInEx")
            {
                var bepinex_found = Mods.Values.FirstOrDefault(data =>
                    data.GenericModData.ModID == ID);

                return bepinex_found?.GenericModData.Mod;
            }
            
            if(name == "")
            {
                var foundEntryDataByIDAndAuthor = Mods.Values.FirstOrDefault(data =>
                    data.GenericModData.ModID == ID &&
                    data.GenericModData.ModAuthor.Equals(author, StringComparison.OrdinalIgnoreCase) &&
                    data.GenericModData.Mod != null);

                if (foundEntryDataByIDAndAuthor == null && !ignoreNull)
                    Console.LogError("JaLoader", $"Mod with ID '{ID}' and author '{author}' not found.");

                return foundEntryDataByIDAndAuthor?.GenericModData.Mod;
            }

            var foundEntryData = Mods.Values.FirstOrDefault(data =>
                data.GenericModData.ModID == ID &&
                data.GenericModData.ModAuthor.Equals(author, StringComparison.OrdinalIgnoreCase) &&
                data.GenericModData.ModName.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                data.GenericModData.Mod != null);

            if(foundEntryData == null && !ignoreNull)
                Console.LogError("JaLoader", $"Mod with ID '{ID}', author '{author}' and name '{name}' not found.");

            return foundEntryData?.GenericModData.Mod;
        }

        public static Type GetTypeFromMod(string author, string ID, string name, string typeName)
        {
            MonoBehaviour mod = FindMod(author, ID, name);

            if (mod != null)
                return Type.GetType($"{mod.GetType().Namespace}.{typeName}, {mod.GetType().Assembly.GetName().Name}");

            return null;
        }

        internal static void MoveModOrderUp(GameObject modListObj)
        {
            if (modListObj.transform.GetSiblingIndex() > 1)
                modListObj.transform.SetSiblingIndex(modListObj.transform.GetSiblingIndex() - 1);

            SaveModsOrder();
        }

        internal static void MoveModOrderDown(GameObject modListObj)
        {
            if (modListObj.transform.GetSiblingIndex() < modListObj.transform.parent.childCount - 1)
                modListObj.transform.SetSiblingIndex(modListObj.transform.GetSiblingIndex() + 1);

            SaveModsOrder();
        }

        internal static void MoveModOrderTop( GameObject modListObj)
        {
            modListObj.transform.SetSiblingIndex(1);

            SaveModsOrder();
        }

        internal static void MoveModOrderBottom(GameObject modListObj)
        {
            modListObj.transform.SetSiblingIndex(modListObj.transform.parent.childCount - 1);

            SaveModsOrder();
        }

        private static void SaveModsOrder()
        {
            string orderFilePath = Path.Combine(Application.persistentDataPath, "ModsOrder.txt");

            using (StreamWriter writer = new StreamWriter(orderFilePath))
            {
                for (int i = 1; i < UIManager.Instance.ModsListContent.childCount; i++)
                {
                    GameObject modObj = UIManager.Instance.ModsListContent.GetChild(i).gameObject;
                    string[] modInfo = modObj.name.Split('_');
                    if (modInfo.Length < 3) return;

                    writer.WriteLine($"{modInfo[0]}_{modInfo[1]}_{modInfo[2]}_{i}");
                }
            }

            Console.LogDebug("JaLoader", "Saved mods order");
        }

        internal static void LoadModOrder()
        {
            LoadModOrder(true);
        }

        internal static void SaveModSettings()
        {
            if (RuntimeVariables.NoModsFlag)
                return;

            for (int i = 0; i < UIManager.Instance.ModsSettingsContent.transform.childCount; i++)
            {
                if (UIManager.Instance.ModsSettingsContent.transform.GetChild(i).gameObject.activeSelf && Regex.Match(UIManager.Instance.ModsSettingsContent.transform.GetChild(i).gameObject.name, @"(.{15})\s*$").ToString() == "-SettingsHolder")
                {
                    string fullName = UIManager.Instance.ModsSettingsContent.transform.GetChild(i).gameObject.name;

                    string modAuthor;
                    string modID;
                    string modName;

                    string[] parts = fullName.Split('_');

                    modAuthor = parts[0];
                    modID = parts[1];

                    modName = parts[2];
                    modName = modName.Remove(modName.Length - 15);

                    if (string.IsNullOrEmpty(modName))
                        modName = modID;

                    var mod = FindMod(modAuthor, modID, modName, ignoreNull: true);

                    if (mod != null && mod is Mod)
                    {
                        var modClass = mod as Mod;
                        modClass.SaveModSettings();
                    }
                    else if (mod != null && mod is BaseUnityPlugin)
                    {
                        var modClass = mod as BaseUnityPlugin;
                        modClass.SaveBIXPluginSettings();
                    }
                }
            }
        }

        internal static void ResetModSettings()
        {
            for (int i = 0; i < UIManager.Instance.ModsSettingsContent.transform.childCount; i++)
            {
                if (UIManager.Instance.ModsSettingsContent.transform.GetChild(i).gameObject.activeSelf && Regex.Match(UIManager.Instance.ModsSettingsContent.transform.GetChild(i).gameObject.name, @"(.{15})\s*$").ToString() == "-SettingsHolder")
                {
                    string fullName = UIManager.Instance.ModsSettingsContent.transform.GetChild(i).gameObject.name;

                    string modAuthor;
                    string modID;
                    string modName;

                    string[] parts = fullName.Split('_');

                    modAuthor = parts[0];
                    modID = parts[1];

                    modName = parts[2];
                    modName = modName.Remove(modName.Length - 15);

                    if (string.IsNullOrEmpty(modName))
                        modName = modID;

                    var mod = FindMod(modAuthor, modID, modName, ignoreNull: true);

                    if (mod != null && mod is Mod)
                    {
                        var modClass = mod as Mod;
                        modClass.ResetModSettings();
                    }
                    else if (mod != null && mod is BaseUnityPlugin)
                    {
                        var modClass = mod as BaseUnityPlugin;
                        modClass.SaveBIXPluginSettings();
                    }
                }
            }
        }

        internal static void ExportModList()
        {
            SerializableModList list = new SerializableModList();
            list.Mods = new List<SerializableModListEntry>();

            foreach (var mb in loadOrderList)
            {
                if (mb is Mod mod)
                {
                    list.Mods.Add(new SerializableModListEntry
                    {
                        Name = mod.ModName,
                        ID = mod.ModID,
                        Author = mod.ModAuthor,
                        Version = mod.ModVersion,
                        GitHubLink = mod.GitHubLink,
                        NexusModsLink = mod.NexusModsLink
                    });
                }
                else if (mb is BaseUnityPlugin plugin)
                {
                    var info = Mods[mb].GenericModData;
                    list.Mods.Add(new SerializableModListEntry
                    {
                        Name = info.ModName,
                        ID = info.ModID,
                        Author = info.ModAuthor,
                        Version = info.ModVersion,
                        GitHubLink = info.GitHubLink,
                        NexusModsLink = info.NexusModsLink
                    });
                }
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = Path.Combine(JaLoaderSettings.ModFolderLocation, "ExportedModLists"),
                Filter = "JSON files (*.json)|*.json",
                Title = "Export mod list",
                RestoreDirectory = true,
                FileName = $"ModList_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.json"
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string jsonString = serializer.Serialize(list);

                File.WriteAllText(saveFileDialog.FileName, jsonString);
            }            
        }

        internal static void ImportModList()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Path.Combine(JaLoaderSettings.ModFolderLocation, "ExportedModLists"),
                Filter = "JSON files (*.json)|*.json",
                Title = "Select a mod list to load",
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                SerializableModList list;
                try
                {
                    string jsonString = File.ReadAllText(filePath);
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    list = serializer.Deserialize<SerializableModList>(jsonString);
                }
                catch (Exception)
                {
                    UIManager.Instance.ShowNotice("Import mod list", "The selected file is not a valid mod list.", false, true);
                    return;
                }

                if(list?.Mods?.Count == 0 || list == null || list.Mods == null)
                {
                    UIManager.Instance.ShowNotice("Import mod list", "The selected mod list is empty.", false, true);
                    return;
                }

                List<MonoBehaviour> newLoadOrder = new List<MonoBehaviour>();
                int missingMods = 0;
                foreach (var mod in list.Mods)
                {
                    var _mod = FindMod(mod.Author, mod.ID, mod.Name, ignoreNull: true);
                    if (_mod != null)
                    {
                        newLoadOrder.Add(_mod);
                        continue;
                    }

                    missingMods++;
                    UIManager.Instance.CreateModListEntry(mod, true);
                }

                for (int i = 0; i < newLoadOrder.Count; i++)
                {
                    var mod = newLoadOrder[i];
                    UIManager.Instance.modEntries[Mods[mod].GenericModData].transform.SetSiblingIndex(i + 1);
                }

                loadOrderList = newLoadOrder.Concat(loadOrderList.Except(newLoadOrder)).ToList();

                SaveModsOrder();

                if (missingMods == 0)
                    UIManager.Instance.ShowNotice("Import mod list", "No mods imported.", false, true);
                else
                    UIManager.Instance.ShowNotice("Import mod list", $"Imported {missingMods} missing mods.", false, true);
            }
        }

        internal static void LoadModOrder(bool loadMods = true)
        {
            DebugUtils.StartCounting();

            string orderFilePath = Path.Combine(Application.persistentDataPath, "ModsOrder.txt");

            if (File.Exists(orderFilePath))
            {
                string[] lines = File.ReadAllLines(orderFilePath);

                List<MonoBehaviour> newLoadOrder = new List<MonoBehaviour>();

                foreach (string line in lines)
                {
                    string[] modInfo = line.Split('_');

                    string modID = modInfo[0];
                    string modAuthor = modInfo[1];
                    string modName = modInfo[2];
                    int loadOrder = int.Parse(modInfo[3]);

                    MonoBehaviour mod = FindMod(modAuthor, modID, modName, true);

                    if (mod != null)
                    {
                        UIManager.Instance.modEntries[Mods[mod].GenericModData].transform.SetSiblingIndex(loadOrder);

                        newLoadOrder.Add(mod);
                    }
                }

                loadOrderList = newLoadOrder.Concat(loadOrderList.Except(newLoadOrder)).ToList();

                SaveModsOrder();
            }
            else
                SaveModsOrder();

            Console.LogDebug("JaLoader", "Loaded mods order");

            if(loadMods)
                LoadMods();
        }
    }
}