using JaLoader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;

namespace JaLoader
{
    /// <summary>
    /// The wiki is your best friend. Check it out here: https://github.com/theLeaxx/JaLoader/wiki
    /// You can also find a version of this template without any comments on the wiki.
    /// Highly recommended to check out the wiki before starting to code.
    /// Also suggest checking out the Order Of Execution page, to understand the order in which functions are called (https://github.com/theLeaxx/JaLoader/wiki/Order-of-Execution).
    /// </summary>
    public class Mod : MonoBehaviour, IMod
    {
        /// <summary>
        /// The mod's ID. Try making it as unique as possible, to avoid conflicting IDs.
        /// </summary>
        public virtual string ModID { get; set; }
        /// <summary>
        /// The mod's name. This is shown in the mods list. Does not need to be unique.
        /// </summary>
        public virtual string ModName { get; set; }
        /// <summary>
        /// The mod's author (you). Also shown in the mods list.
        /// </summary>
        public virtual string ModAuthor { get; set; }
        /// <summary>
        /// The mod's optional description. This is also shown in the mods list, upon clicking on "More Info".
        /// </summary>
        public virtual string ModDescription { get; set; }
        /// <summary>
        /// The mod's version. Also shown in the mods list. 
        /// If your mod is open-source on GitHub, make sure that you're using the same format as your release tags (for example, 1.0.0)
        /// For more information, check out the wiki page on versioning. (https://github.com/theLeaxx/JaLoader/wiki/Versioning-your-mod)
        /// </summary>
        public virtual string ModVersion { get; set; }
        /// <summary>
        /// If your mod is open-source on GitHub, you can link it here to allow for automatic update-checking in-game.
        /// It compares the current ModVersion with the tag of the latest release (ex. 1.0.0 compared with 1.0.1)
        /// For more information, check out the wiki page on versioning. (https://github.com/theLeaxx/JaLoader/wiki/Versioning-your-mod)
        /// </summary>
        public virtual string GitHubLink { get; set; }
        /// <summary>
        /// If your mod is published on NexusMods, you can link it here to allow for easy access to your mod's page.
        /// Automatic update-checking is not supported, as it is against NexusMods' ToS.
        /// </summary>
        public virtual string NexusModsLink { get; set; }
        /// <summary>
        /// If your mod uses custom assets, you need to set this to true.
        /// In other words, if your mod uses the "LoadAsset>T>" function, you need to set this to true.
        /// For more information, check out the wiki page on custom assets. (https://github.com/theLeaxx/JaLoader/wiki/Using-custom-assets)
        /// </summary>
        public virtual bool UseAssets { get; set; }
        [Obsolete("Use WhenToInitMod instead.")]
        public virtual WhenToInit WhenToInit { get; set; } = WhenToInit.None;
        /// <summary>
        /// When to initialize the mod.
        /// InGame: When the game is loaded, stops functioning in the main menu.
        /// InMenu: When the main menu is loaded, continues to function in-game too.
        /// </summary>
        public virtual Common.WhenToInit WhenToInitMod { get; set; }

        /// <summary>
        /// If you mod depends on a certain version of JaLoader, or another mod, you can specify it here. 
        /// The format is (ModID, ModAuthor, ModVersion), and for JaLoader it's ("JaLoader", "Leaxx", {version}).
        /// Versions are usually formatted in the (x.y.z) format (for example, 1.2.0), although certain mods may follow other formats.
        /// Enable Debug Mode in JaLoader settings to view ModIDs instead of ModNames in the mod list.
        /// If you don't have any dependencies, you can just return an empty list.
        /// For more information, check out the wiki page on dependencies. (https://github.com/theLeaxx/JaLoader/wiki/Using-dependencies)
        /// </summary>
        public virtual List<(string, string, string)> Dependencies { get; set; } = new List<(string, string, string)>();
        /// <summary>
        /// If your mod is incompatible with certain versions of JaLoader, or certain versions of mods, you can specify it here. 
        /// The format is (ModID, ModAuthor, ModVersionMin-ModVersionMax), and for JaLoader it's ("JaLoader", "Leaxx", {versionMin-versionMax}).
        /// Versions are usually formatted in the (x.y.z) format (for example, 1.2.0), although certain mods may follow other formats.
        /// Enable Debug Mode in JaLoader settings to view ModIDs instead of ModNames in the mod list.
        /// If you don't have any incompatibilities, you can just return an empty list.
        /// For more information, check out the wiki page on incompatibilities. (https://github.com/theLeaxx/JaLoader/wiki/Using-Incompatibilities)
        /// </summary>
        public virtual List<(string, string, string)> Incompatibilities { get; set; } = new List<(string, string, string)>();

        public string AssetsPath
        {
            get { return _assetsPath; }
        }

        internal string _assetsPath;

        public List<string> settingsIDS
        {
            get { return _settingIDS; }
        }

#pragma warning disable CS0618
        internal JaLoader.Common.WhenToInit GetWhenToInit()
        {
            if (WhenToInit == WhenToInit.None)
                return WhenToInitMod;

            return (Common.WhenToInit)(int)WhenToInit;
        }
#pragma warning restore CS0618

        public Dictionary<string, string> settingsValues
        {
            get { return _settingsValues; }
        }

        public Dictionary<string, string> valuesAfterLoad
        {
            get { return _valuesAfterLoad; }
        }

        internal List<string> _settingIDS = new List<string>();
        internal Dictionary<string, string> _settingsValues = new Dictionary<string, string>();
        internal Dictionary<string, string> _valuesAfterLoad = new Dictionary<string, string>();

        [Serializable] class SettingsValues : SerializableDictionary<string, string> { }

        /// <summary>
        /// Earliest function to be called, usually used for loading assets.
        /// </summary>
        public virtual void Preload() { }
        /// <summary>
        /// Declare all of your events here.
        /// Events are used to call functions when certain things happen in-game.
        /// They are held by the script "EventsManager". You can use "EventsManager.Instance.{event} += FunctionName()" to subscribe to them.
        /// For more information, check out the wiki page on events. (https://github.com/theLeaxx/JaLoader/wiki/Using-events)
        /// </summary>
        public virtual void EventsDeclaration() { }
        /// <summary>
        /// Declare all of your settings here.
        /// Make sure to call "InstantiateSettings()" in here before declaring your settings.
        /// For more information, check out the wiki page on settings. (https://github.com/theLeaxx/JaLoader/wiki/Adding-settings-for-mods)
        /// </summary>
        public virtual void SettingsDeclaration() { }
        /// <summary>
        /// Register all of your custom objects here.
        /// Custom objects are objects that are not part of the game's default objects, but act like them.
        /// Basically, if you want to add a new object to the game that can be picked up/placed/etc, you need to register it here.
        /// For more information, check out the wiki page on custom objects. (https://github.com/theLeaxx/JaLoader/wiki/Using-Custom-Objects)
        /// </summary>
        public virtual void CustomObjectsRegistration() { }
        /// <summary>
        /// This is the default Unity OnEnable() function, called as soon as the mod is enabled, before Awake() and Start().
        /// </summary>
        public virtual void OnEnable() { }
        /// <summary>
        /// This is the default Unity Awake() function, called as soon as the mod is enabled, before Start().
        /// </summary>
        public virtual void Awake() { }
        /// <summary>
        /// This is the default Unity Start() function, called when the mod is enabled.
        /// </summary>
        public virtual void Start() { }
        /// <summary>
        /// This is the default Unity Update() function, called every frame after the mod is enabled.
        /// </summary>
        public virtual void Update() { }
        /// <summary>
        /// This is the default Unity OnDisable() function, called when the mod is disabled.
        /// </summary>
        public virtual void OnDisable() { }
        /// <summary>
        /// This is the default Unity OnDestroy() function, called when the mod is destroyed.
        /// </summary>
        public virtual void OnDestroy() { }
        /// <summary>
        /// This function is ran when the mod is reloaded, called only if WhenToInit.InGame and when the user went to the menu and back in game.
        /// </summary>
        public virtual void OnReload() { }
        /// <summary>
        /// This function is ran when the mod's settings are saved.
        /// </summary>
        public virtual void OnSettingsSaved() { }
        /// <summary>
        /// This function is ran when the setting assigned with the ID is saved.
        /// </summary>
        public virtual void OnSettingValueChanged(string ID) { }
        /// <summary>
        /// This function is ran when the mod's settings are reset.
        /// </summary>
        public virtual void OnSettingsReset() { }
        /// <summary>
        /// This function is ran when the mod's settings are loaded.
        /// </summary>
        public virtual void OnSettingsLoaded() { }
        /// <summary>
        /// This function is ran when the extra "extraName" is attached to the car.
        /// </summary>
        public virtual void OnExtraAttached(string extraName) {}

        /// <summary>
        /// (Deprecated, use LoadAsset<![CDATA[<T>]]>())
        /// Loads the specified asset from an assetbundle
        /// <param name="assetName">The file's name</param>
        /// <param name="prefabName">The prefab in question</param>
        /// <param name="fileSuffix">The file's suffix (usually .unity3d, you can leave empty too)</param>
        /// <returns>The loaded GameObject</returns>
        /// </summary>
        [Obsolete]
        public GameObject LoadAsset(string assetName, string prefabName, string fileSuffix)
        {
            if (!UseAssets)
            {
                Console.LogError(ModID, "Tried to call LoadAssets, but UseAssets is false.");
                return null;
            }

            if (!File.Exists(Path.Combine(AssetsPath, $"{assetName}{fileSuffix}")))
            {
                Console.LogError(ModID, $"Tried to load asset '{assetName}{fileSuffix}', but it does not exist.");
                return null;
            }

            var ab = AssetBundle.LoadFromFile(Path.Combine(AssetsPath, $"{assetName}{fileSuffix}"));
            var prefab = ab.LoadAsset<GameObject>($"{prefabName}.prefab");

            if (prefab == null)
            {
                Console.LogError(ModID, $"Tried to load prefab {prefabName} from asset {assetName}, but it does not exist.");
                return null;
            }

            var obj = prefab;

            obj.name = $"{ModID}_{prefabName}";

            var identification = obj.AddComponent<ObjectIdentification>();
            identification.ModID = ModID;
            identification.ModName = ModName;
            identification.Author = ModAuthor;
            identification.Version = ModVersion;

            ab.Unload(false);
            return obj;
        }

        /// <summary>
        /// Loads the specified asset from an assetbundle
        /// </summary>
        /// <typeparam name="T">The type of the asset you want to load</typeparam>
        /// <param name="assetName">The file's name</param>
        /// <param name="prefabName">The prefab in question</param>
        /// <param name="fileSuffix">The file's suffix (usually .unity3d, you can leave empty too)</param>
        /// <param name="prefabSuffix">The prefab's suffix (varies on T, for example .mp3 or .png)</param>
        /// <returns>The loaded GameObject</returns>
        public T LoadAsset<T>(string assetName, string prefabName, string fileSuffix, string prefabSuffix) where T: UnityEngine.Object
        {
            if (!UseAssets)
            {
                Console.LogError(ModID, "Tried to call LoadAssets, but UseAssets is false.");
                return null;
            }

            if (!File.Exists(Path.Combine(AssetsPath, $"{assetName} {fileSuffix}")))
            {
                Console.LogError(ModID, $"Tried to load asset '{assetName}{fileSuffix}', but it does not exist.");
                return null;
            }

            Application.logMessageReceived += (condition, stackTrace, type) =>
            {
                if (condition.Contains("The file can not be loaded because it was created for another build target that is not compatible with this platform."))
                    Console.LogError(ModID, $"Asset '{assetName}{fileSuffix}' could not be loaded because it was built for another platform.");
            };

            var ab = AssetBundle.LoadFromFile(Path.Combine(AssetsPath, $"{assetName}{fileSuffix}"));

            if(ab == null)
            {
                Console.LogError(ModID, $"An error occured while loading asset '{assetName}{fileSuffix}'.");
                return null;
            }

            Application.logMessageReceived -= (condition, stackTrace, type) =>
            {
                if (condition.Contains("The file can not be loaded because it was created for another build target that is not compatible with this platform."))
                    Console.LogError(ModID, $"Asset '{assetName}{fileSuffix}' could not be loaded because it was built for another platform.");
            };

            var asset = ab.LoadAsset<T>($"{prefabName}{prefabSuffix}");

            if (asset == null)
            {
                Console.LogError(ModID, $"Tried to load {typeof(T).Name} '{prefabName}{prefabSuffix}' from asset '{assetName}{fileSuffix}', but it does not exist.");
                ab.Unload(true);
                return null;
            }

            if (typeof(T) == typeof(GameObject))
            {
                GameObject obj = asset as GameObject;

                obj.name = $"{ModID}_{prefabName}";

                var identification = obj.AddComponent<ObjectIdentification>();
                identification.ModID = ModID;
                identification.ModName = ModName;
                identification.Author = ModAuthor;
                identification.Version = ModVersion;

                ab.Unload(false);

                return (T)(object)obj;
            }

            ab.Unload(false);

            return asset;
        }

        /// <summary>
        /// Converts a PNG file to a Texture2D, usable in Materials and UI.
        /// </summary>
        /// <param name="name">The file's name</param>
        /// <returns>A new Texture2D, containing your PNG file</returns>
        public Texture2D PNGToTexture(string name)
        {
            if (!UseAssets)
            {
                Console.LogError(ModID, "Tried to call PNGToTexture, but UseAssets is false.");
                return null;
            }

            if (!File.Exists($@"{AssetsPath}\{name}.png"))
            {
                Console.LogError(ModID, $"Tried to load PNG {name}.png, but it does not exist.");
                return null;
            }

            byte[] bytes = File.ReadAllBytes($@"{AssetsPath}\{name}.png");

            Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            texture.LoadImage(bytes);

            return texture;
        }

        /// <summary>
        /// Loads a scene from an assetbundle
        /// </summary>
        /// <param name="assetName">The file's name</param>
        /// <param name="sceneName">The scene's name</param>
        /// <param name="fileSuffix">The file's suffix (usually .unity3d, you can leave empty too)</param>
        public void LoadSceneFromAsset(string assetName, string sceneName, string fileSuffix)
        {
            if (!UseAssets)
            {
                Console.LogError(ModID, "Tried to call LoadAssets, but UseAssets is false.");
            }

            if (!File.Exists(Path.Combine(AssetsPath, $"{assetName} {fileSuffix}")))
            {
                Console.LogError(ModID, $"Tried to load scene '{assetName}{fileSuffix}', but it does not exist.");
            }

            AssetBundle.LoadFromFile($@"{AssetsPath}\{assetName}{fileSuffix}");

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public void InstantiateSettings()
        {
            if (UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Settings are already instantiated!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.ModSettingsHolder);
            obj.name = $"{ModAuthor}_{ModID}_{ModName}-SettingsHolder";
            obj.transform.SetParent(UIManager.Instance.ModsSettingsContent.transform, false);

            GameObject name = Instantiate(UIManager.Instance.ModSettingsNameTemplate);
            name.transform.SetParent(obj.transform, false);
            name.GetComponentInChildren<Text>().text = $"{ModName} Settings";
            name.SetActive(true);

            obj.SetActive(true);
        }

        public void AddHeader(string text)
        {
            if (!UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Tried adding header, but settings aren't instantiated!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.ModSettingsHeaderTemplate);
            obj.transform.SetParent(UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"), false);
            obj.GetComponentInChildren<Text>().text = text;
            obj.SetActive(true);
        }

        public void AddDisclaimer(string text)
        {
            if (!UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Tried adding disclaimer, but settings aren't instantiated!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.ModSettingsHeaderTemplate);
            obj.transform.SetParent(UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"), false);
            obj.GetComponentInChildren<Text>().text = text;
            obj.GetComponentInChildren<Text>().resizeTextMaxSize = 20;
            obj.SetActive(true);
        }

        public void AddDropdown(string ID, string name, string[] values, int defaultValue)
        {
            if (!UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Tried adding dropdown, but settings aren't instantiated!");
                return;
            }

            if (UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Dropdown"))
            {
                Console.LogError(ModID, $"Dropdown with ID {ID} already exists!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.ModSettingsDropdownTemplate);
            obj.transform.SetParent(UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"), false);
            obj.name = $"{ID}_Dropdown";

            List<Dropdown.OptionData> optionData = new List<Dropdown.OptionData>();
            foreach (string value in values)
            {
                optionData.Add(new Dropdown.OptionData($"{name}: {value}"));
            }

            obj.GetComponentInChildren<Dropdown>().options = optionData;
            obj.GetComponentInChildren<Dropdown>().value = defaultValue;
            obj.SetActive(true);

            settingsIDS.Add($"{ID}_Dropdown");
            settingsValues.Add($"{ID}_Dropdown", defaultValue.ToString());
        }

        public void AddToggle(string ID, string name, bool defaultValue)
        {
            if (!UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Tried adding toggle, but settings aren't instantiated!");
                return;
            }

            if (UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Toggle"))
            {
                Console.LogError(ModID, $"Toggle with ID {ID} already exists!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.ModSettingsToggleTemplate);
            obj.transform.SetParent(UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"), false);
            obj.name = $"{ID}_Toggle";

            List<Dropdown.OptionData> optionData = new List<Dropdown.OptionData>
            {
                new Dropdown.OptionData($"{name}: On"),
                new Dropdown.OptionData($"{name}: Off")
            };

            obj.GetComponentInChildren<Dropdown>().options = optionData;
            obj.GetComponentInChildren<Dropdown>().value = defaultValue ? 0 : 1;
            obj.SetActive(true);

            settingsIDS.Add($"{ID}_Toggle");
            settingsValues.Add($"{ID}_Toggle", defaultValue ? "0" : "1");
        }

        public void AddSlider(string ID, string name, int minValue, int maxValue, int defaultValue, bool wholeNumbers)
        {
            if (!UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Tried adding slider, but settings aren't instantiated!");
                return;
            }

            if (UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Slider"))
            {
                Console.LogError(ModID, $"Slider with ID {ID} already exists!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.ModSettingsSliderTemplate);
            obj.transform.SetParent(UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"), false);
            obj.name = $"{ID}_Slider";

            var scr = obj.GetComponentInChildren<Slider>().gameObject.AddComponent<TooltipOnHover>();
            scr.slider = obj.GetComponentInChildren<Slider>();
            obj.GetComponentInChildren<Slider>().minValue = minValue;
            obj.GetComponentInChildren<Slider>().maxValue = maxValue;
            obj.GetComponentInChildren<Slider>().value = defaultValue;
            obj.GetComponentInChildren<Slider>().wholeNumbers = wholeNumbers;
            obj.GetComponentInChildren<Text>().text = name;
            obj.SetActive(true);

            settingsIDS.Add($"{ID}_Slider");
            settingsValues.Add($"{ID}_Slider", defaultValue.ToString());
        }

        public void AddInputField(string ID, string name, string defaultValue, string placeholderValue = null)
        {
            if (!UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Tried adding input field, but settings aren't instantiated!");
                return;
            }

            if (UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_InputField"))
            {
                Console.LogError(ModID, $"Input field with ID {ID} already exists!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.ModSettingsInputTemplate);
            obj.transform.SetParent(UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"), false);
            obj.name = $"{ID}_InputField";

            obj.GetComponentInChildren<InputField>().text = defaultValue;
            obj.GetComponentInChildren<InputField>().placeholder.GetComponent<Text>().text = placeholderValue ?? "Enter text...";
            obj.transform.Find("HeaderText").GetComponent<Text>().text = name;
            obj.SetActive(true);

            settingsIDS.Add($"{ID}_InputField");
            settingsValues.Add($"{ID}_InputField", defaultValue);
        }

        public void AddKeybind(string ID, string name, KeyCode defaultPrimaryKey)
        {
            if (!UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Tried adding keybind, but settings aren't instantiated!");
                return;
            }

            if (UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Keybind"))
            {
                Console.LogError(ModID, $"Keybind with ID {ID} already exists!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.ModSettingsKeybindTemplate);
            obj.transform.SetParent(UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"), false);
            obj.name = $"{ID}_Keybind";

            obj.SetActive(true);

            obj.AddComponent<CustomKeybind>();
            obj.GetComponent<CustomKeybind>().SetPrimaryKey(defaultPrimaryKey);
            obj.GetComponent<CustomKeybind>().EnableAltKey = false;
            obj.transform.Find("HeaderText").GetComponent<Text>().text = name;

            settingsIDS.Add($"{ID}_Keybind");
            settingsValues.Add($"{ID}_Keybind", ((int)defaultPrimaryKey).ToString() + "|");
        }

        public void AddKeybind(string ID, string name, KeyCode defaultPrimaryKey, KeyCode defaultSecondaryKey)
        {
            if (!UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Tried adding keybind, but settings aren't instantiated!");
                return;
            }

            if (UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Keybind"))
            {
                Console.LogError(ModID, $"Keybind with ID {ID} already exists!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.ModSettingsKeybindTemplate);
            obj.transform.SetParent(UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"), false);
            obj.name = $"{ID}_Keybind";

            obj.SetActive(true);

            obj.AddComponent<CustomKeybind>();
            obj.GetComponent<CustomKeybind>().SetPrimaryKey(defaultPrimaryKey);
            obj.GetComponent<CustomKeybind>().EnableAltKey = true;
            obj.GetComponent<CustomKeybind>().SetSecondaryKey(defaultSecondaryKey);
            obj.transform.Find("HeaderText").GetComponent<Text>().text = name;

            settingsIDS.Add($"{ID}_Keybind");
            settingsValues.Add($"{ID}_Keybind", ((int)defaultPrimaryKey).ToString() + "|" + ((int)defaultSecondaryKey).ToString());
        }

        public Dropdown GetDropdown(string ID)
        {
            if (UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Dropdown"))
                return UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Dropdown").GetComponentInChildren<Dropdown>();

            return null;
        }

        public int GetDropdownValue(string ID)
        {
            var dropdown = GetDropdown(ID);

            if(dropdown != null)
                return dropdown.value;

            return 0;
        }

        public Dropdown GetToggle(string ID)
        {
            if (UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Toggle"))
                return UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Toggle").GetComponentInChildren<Dropdown>();
            else
                return null;
        }

        public bool GetToggleValue(string ID)
        {
            var toggle = GetToggle(ID);

            if (toggle != null)
                return toggle.value == 0;

            return false;
        }

        public Slider GetSlider(string ID)
        {
            if (UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Slider"))
                return UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Slider").GetComponentInChildren<Slider>();
            else
                return null;
        }

        public InputField GetInputField(string ID)
        {
            if (UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_InputField"))
                return UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_InputField").GetComponentInChildren<InputField>();
            else
                return null;
        }

        public float GetSliderValue(string ID)
        {
            var slider = GetSlider(ID);

            if (slider != null)
                return slider.value;

            return 0;
        }

        public string GetInputFieldValue(string ID)
        {
            var inputField = GetInputField(ID);

            if (inputField != null)
                return inputField.text;

            return "";
        }

        public KeyCode GetPrimaryKeybind(string ID)
        {
            if (UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Keybind"))
                return UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Keybind").GetComponent<CustomKeybind>().SelectedKey;
            else
                return KeyCode.None;
        }

        public KeyCode GetSecondaryKeybind(string ID)
        {
            if (UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Keybind"))
                return UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Keybind").GetComponent<CustomKeybind>().AltSelectedKey;
            else
                return KeyCode.None;
        }

        public void SaveModSettings()
        {
            SettingsValues values = new SettingsValues();

            foreach (var ID in settingsIDS)
            {
                string type = Regex.Match(ID, @"(.{6})\s*$").ToString();

                switch (type)
                {
                    case "opdown":
                        values.Add(ID, UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value.ToString());
                        break;

                    case "Toggle":
                        values.Add(ID, UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value.ToString());
                        break;

                    case "Slider":
                        values.Add(ID, UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Slider>().value.ToString());
                        break;

                    case "eybind":
                        values.Add($"{ID}_primary", ((int)UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().SelectedKey).ToString());
                        if (UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().EnableAltKey)
                        {
                            values.Add($"{ID}_secondary", ((int)UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().AltSelectedKey).ToString());
                        }
                        break;

                    case "tField":
                        values.Add(ID, UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<InputField>().text);
                        break;

                    default:
                        break;
                }
            }

            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, $@"ModSaves")))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, $@"ModSaves"));
            }

            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, $@"ModSaves\{ModID}")))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, $@"ModSaves\{ModID}"));
            }

            File.WriteAllText(Path.Combine(Application.persistentDataPath, $@"ModSaves\{ModID}\{ModID}_save.json"), JsonUtility.ToJson(values, true));

            SafeOnSettingsSaved();

            // compare values with valuesAfterLoad, and if they are different, call OnSettingValueChanged(ID)
            foreach (var ID in settingsIDS)
            {
                if (valuesAfterLoad.ContainsKey(ID))
                {
                    if (valuesAfterLoad[ID] != values[ID])
                    {
                        SafeOnSettingValueChanged(ID);

                        valuesAfterLoad[ID] = values[ID];
                    }
                }
                else if (valuesAfterLoad.ContainsKey($"{ID}_primary"))
                {
                    if (valuesAfterLoad[$"{ID}_primary"] != values[$"{ID}_primary"])
                    {
                        SafeOnSettingValueChanged(ID);
                        valuesAfterLoad[ID] = values[ID];
                    }
                }
                else if(valuesAfterLoad.ContainsKey($"{ID}_secondary"))
                {
                    if (valuesAfterLoad[$"{ID}_secondary"] != values[$"{ID}_secondary"])
                    {
                        SafeOnSettingValueChanged(ID);
                        valuesAfterLoad[ID] = values[ID];
                    }
                }
            }
        }

        internal void SafeOnSettingValueChanged(string ID)
        {
            try
            {
                OnSettingValueChanged(ID);
            }
            catch (Exception ex)
            {
                Console.LogError("JaLoader", $"Mod {ModID} threw an exception in OnSettingValueChanged for setting {ID}:\n{ex}");
            }
        }

        internal void SafeOnSettingsSaved()
        {
            try
            {
                OnSettingsSaved();
            }
            catch (Exception ex)
            {
                Console.LogError("JaLoader", $"Mod {ModID} threw an exception in OnSettingsSaved:\n{ex}");
            }
        }

        internal void SafeOnSettingsReset()
        {
            try
            {
                OnSettingsReset();
            }
            catch (Exception ex)
            {
                Console.LogError("JaLoader", $"Mod {ModID} threw an exception in OnSettingsReset:\n{ex}");
            }
        }

        internal void SafeOnSettingsLoaded()
        {
            try
            {
                OnSettingsLoaded();
            }
            catch (Exception ex)
            {
                Console.LogError("JaLoader", $"Mod {ModID} threw an exception in OnSettingsLoaded:\n{ex}");
            }
        }

        /// <summary>
        /// Returns a list of all used keycodes in the mod's settings, alongside their setting name, or ID if useIds = true.
        /// </summary>
        /// <returns></returns>
        public List<(string, KeyCode)> GetAllUsedKeycodes(bool useIds = false)
        {
            List<(string, KeyCode)> keys = new List<(string, KeyCode)>();

            foreach (var ID in settingsIDS)
            {
                string type = Regex.Match(ID, @"(.{6})\s*$").ToString();

                if (type == "eybind")
                {
                    string str = settingsValues[$"{ID}"];

                    var parent = UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}");
                    var mainKey = parent.GetComponent<CustomKeybind>().SelectedKey;
                    KeyCode altKey = KeyCode.None;
                    bool useAltKey = false;
                    if (parent.GetComponent<CustomKeybind>().EnableAltKey)
                    {
                        altKey = parent.GetComponent<CustomKeybind>().AltSelectedKey;
                        useAltKey = true;
                    }

                    string name = useIds ? Regex.Replace(ID, "_Keybind$", "") : parent.Find("HeaderText").GetComponent<Text>().text;

                    keys.Add((name, mainKey));
                    if(useAltKey)
                        keys.Add((name, altKey));
                }
            }

            return keys;
        }

        public void ResetModSettings()
        {
            foreach (var ID in settingsIDS)
            {
                string type = Regex.Match(ID, @"(.{6})\s*$").ToString();

                switch (type)
                {
                    case "opdown":
                        UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value = int.Parse(settingsValues[ID]);
                        break;

                    case "Toggle":
                        UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value = int.Parse(settingsValues[ID]);
                        break;

                    case "Slider":
                        UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Slider>().value = float.Parse(settingsValues[ID]);
                        break;

                    case "eybind":
                        string str = settingsValues[$"{ID}"];
                        // str is formatted as "int|int" so we split it and parse it as int, if there is no |, it will just parse the first part
                        UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().SetPrimaryKey((KeyCode)int.Parse(str.Split('|')[0]));
                        if (UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().EnableAltKey)
                            UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().SetSecondaryKey((KeyCode)int.Parse(str.Split('|')[1]));
                        break;

                    case "tField":
                        UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<InputField>().text = settingsValues[ID];
                        break;

                    default:
                        break;
                }
            }

            SafeOnSettingsReset();

            SaveModSettings();

            LoadModSettings();
        }

        public void LoadModSettings()
        {
            SettingsValues values = new SettingsValues();

            if (File.Exists(Path.Combine(Application.persistentDataPath, $@"ModSaves\{ModID}\{ModID}_save.json")))
            {
                string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, $@"ModSaves\{ModID}\{ModID}_save.json"));
                values = JsonUtility.FromJson<SettingsValues>(json);

                foreach (var ID in settingsIDS)
                {
                    if (values.ContainsKey(ID))
                    {
                        string type = Regex.Match(ID, @"(.{6})\s*$").ToString();

                        switch (type)
                        {
                            case "opdown":
                                UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value = (int)float.Parse(values[ID]);
                                break;

                            case "Toggle":
                                UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value = (int)float.Parse(values[ID]);
                                break;

                            case "Slider":
                                UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Slider>().value = float.Parse(values[ID]);
                                break;

                            case "tField":
                                UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<InputField>().text = values[ID];
                                break;

                            default:
                                break;
                        }
                    }
                    else if (values.ContainsKey($"{ID}_primary"))
                    {
                        UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().SetPrimaryKey((KeyCode)float.Parse(values[$"{ID}_primary"]));
                        
                        if (values.ContainsKey($"{ID}_secondary"))
                            UIManager.Instance.ModsSettingsContent.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().SetSecondaryKey((KeyCode)float.Parse(values[$"{ID}_secondary"]));
                    }
                }
            }

            _valuesAfterLoad = values;

            SafeOnSettingsLoaded();
        }
    } 
}
