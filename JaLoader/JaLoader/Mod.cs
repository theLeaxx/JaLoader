﻿using JetBrains.Annotations;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;

namespace JaLoader
{
    public class Mod : MonoBehaviour
    {
        public virtual string ModID { get; set; }
        public virtual string ModName { get; set; }
        public virtual string ModAuthor { get; set; }
        public virtual string ModDescription { get; set; }
        public virtual string ModVersion { get; set; }
        public virtual string GitHubLink { get; set; }
        public virtual string NexusModsLink { get; set; }
        public virtual bool UseAssets { get; set; }
        public virtual WhenToInit WhenToInit { get; set; }

        public virtual List<(string, string, string)> Dependencies { get; set; } = new List<(string, string, string)>();

        public virtual List<(string, string, string)> Incompatibilities { get; set; } = new List<(string, string, string)>();

        public string AssetsPath { get; set; }

        public List<string> settingsIDS = new List<string>();
        private Dictionary<string, string> settingsValues = new Dictionary<string, string>();
        [Serializable] class SettingsValues : SerializableDictionary<string, string> { }

        private Dictionary<string, string> valuesAfterLoad = new Dictionary<string, string>();

        public virtual void EventsDeclaration() { }
        public virtual void SettingsDeclaration() { }
        public virtual void CustomObjectsRegistration() { }

        public virtual void Update() { }
        public virtual void Start() { }
        public virtual void Awake() { }
        public virtual void OnEnable() { }
        public virtual void RunOnPreloader() { }
        public virtual void OnDisable() { }
        public virtual void OnDestroy() { }
        public virtual void OnReload() { }

        public virtual void OnSettingsSaved() { }
        public virtual void OnSettingValueChanged(string ID) { }
        public virtual void OnSettingsReset() { }
        public virtual void OnSettingsLoaded() { }

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
            Type type = obj.GetType();
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

            Application.logMessageReceived += (string condition, string stackTrace, LogType type) =>
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

            Application.logMessageReceived -= (string condition, string stackTrace, LogType type) =>
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
            if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Settings are already instantiated!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.modOptionsHolder);
            obj.name = $"{ModAuthor}_{ModID}_{ModName}-SettingsHolder";
            obj.transform.SetParent(UIManager.Instance.modSettingsScrollViewContent.transform, false);

            GameObject name = Instantiate(UIManager.Instance.modOptionsNameTemplate);
            name.transform.SetParent(obj.transform, false);
            name.GetComponentInChildren<Text>().text = $"{ModName} Settings";
            name.SetActive(true);

            obj.SetActive(true);
        }

        public void AddHeader(string text)
        {
            if (!UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Tried adding header, but settings aren't instantiated!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.modOptionsHeaderTemplate);
            obj.transform.SetParent(UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"), false);
            obj.GetComponentInChildren<Text>().text = text;
            obj.SetActive(true);
        }

        public void AddDisclaimer(string text)
        {
            if (!UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Tried adding disclaimer, but settings aren't instantiated!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.modOptionsHeaderTemplate);
            obj.transform.SetParent(UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"), false);
            obj.GetComponentInChildren<Text>().text = text;
            obj.GetComponentInChildren<Text>().resizeTextMaxSize = 20;
            obj.SetActive(true);
        }

        public void AddDropdown(string ID, string name, string[] values, int defaultValue)
        {
            if (!UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Tried adding dropdown, but settings aren't instantiated!");
                return;
            }

            if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Dropdown"))
            {
                Console.LogError(ModID, $"Dropdown with ID {ID} already exists!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.modOptionsDropdownTemplate);
            obj.transform.SetParent(UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"), false);
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
            if (!UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Tried adding toggle, but settings aren't instantiated!");
                return;
            }

            if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Toggle"))
            {
                Console.LogError(ModID, $"Toggle with ID {ID} already exists!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.modOptionsToggleTemplate);
            obj.transform.SetParent(UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"), false);
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
            if (!UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Tried adding slider, but settings aren't instantiated!");
                return;
            }

            if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Slider"))
            {
                Console.LogError(ModID, $"Slider with ID {ID} already exists!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.modOptionsSliderTemplate);
            obj.transform.SetParent(UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"), false);
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
            if (!UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Tried adding input field, but settings aren't instantiated!");
                return;
            }

            if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_InputField"))
            {
                Console.LogError(ModID, $"Input field with ID {ID} already exists!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.modOptionsInputTemplate);
            obj.transform.SetParent(UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"), false);
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
            if (!UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Tried adding keybind, but settings aren't instantiated!");
                return;
            }

            if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Keybind"))
            {
                Console.LogError(ModID, $"Keybind with ID {ID} already exists!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.modOptionsKeybindTemplate);
            obj.transform.SetParent(UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"), false);
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
            if (!UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"))
            {
                Console.LogError(ModID, "Tried adding keybind, but settings aren't instantiated!");
                return;
            }

            if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Keybind"))
            {
                Console.LogError(ModID, $"Keybind with ID {ID} already exists!");
                return;
            }

            GameObject obj = Instantiate(UIManager.Instance.modOptionsKeybindTemplate);
            obj.transform.SetParent(UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder"), false);
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
            if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Dropdown"))
                return UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Dropdown").GetComponentInChildren<Dropdown>();

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
            if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Toggle"))
                return UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Toggle").GetComponentInChildren<Dropdown>();
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
            if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Slider"))
                return UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Slider").GetComponentInChildren<Slider>();
            else
                return null;
        }

        public InputField GetInputField(string ID)
        {
            if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_InputField"))
                return UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_InputField").GetComponentInChildren<InputField>();
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
            if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Keybind"))
                return UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Keybind").GetComponent<CustomKeybind>().SelectedKey;
            else
                return KeyCode.None;
        }

        public KeyCode GetSecondaryKeybind(string ID)
        {
            if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Keybind"))
                return UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}_Keybind").GetComponent<CustomKeybind>().AltSelectedKey;
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
                        values.Add(ID, UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value.ToString());
                        break;

                    case "Toggle":
                        values.Add(ID, UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value.ToString());
                        break;

                    case "Slider":
                        values.Add(ID, UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Slider>().value.ToString());
                        break;

                    case "eybind":
                        values.Add($"{ID}_primary", ((int)UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().SelectedKey).ToString());
                        if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().EnableAltKey)
                        {
                            values.Add($"{ID}_secondary", ((int)UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().AltSelectedKey).ToString());
                        }
                        break;

                    case "tField":
                        values.Add(ID, UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<InputField>().text);
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

            OnSettingsSaved();

            // compare values with valuesAfterLoad, and if they are different, call OnSettingValueChanged(ID)
            foreach (var ID in settingsIDS)
            {
                if (valuesAfterLoad.ContainsKey(ID))
                {
                    if (valuesAfterLoad[ID] != values[ID])
                    {
                        Console.LogDebug(ModID, $"Setting {ID} has changed!");
                        OnSettingValueChanged(ID);

                        valuesAfterLoad[ID] = values[ID];
                    }
                }
                else if (valuesAfterLoad.ContainsKey($"{ID}_primary"))
                {
                    if (valuesAfterLoad[$"{ID}_primary"] != values[$"{ID}_primary"])
                    {
                        OnSettingValueChanged(ID);
                        valuesAfterLoad[ID] = values[ID];
                    }
                }
                else if(valuesAfterLoad.ContainsKey($"{ID}_secondary"))
                {
                    if (valuesAfterLoad[$"{ID}_secondary"] != values[$"{ID}_secondary"])
                    {
                        OnSettingValueChanged(ID);
                        valuesAfterLoad[ID] = values[ID];
                    }
                }
            }
        }

        public void ResetModSettings()
        {
            foreach (var ID in settingsIDS)
            {
                string type = Regex.Match(ID, @"(.{6})\s*$").ToString();

                switch (type)
                {
                    case "opdown":
                        UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value = int.Parse(settingsValues[ID]);
                        break;

                    case "Toggle":
                        UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value = int.Parse(settingsValues[ID]);
                        break;

                    case "Slider":
                        UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Slider>().value = float.Parse(settingsValues[ID]);
                        break;

                    case "eybind":
                        string str = settingsValues[$"{ID}"];
                        // str is formatted as "int|int" so we split it and parse it as int, if there is no |, it will just parse the first part
                        UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().SetPrimaryKey((KeyCode)int.Parse(str.Split('|')[0]));
                        if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().EnableAltKey)
                        {
                            UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().SetSecondaryKey((KeyCode)int.Parse(str.Split('|')[1]));
                        }
                        break;

                    case "tField":
                        UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<InputField>().text = settingsValues[ID];
                        break;

                    default:
                        break;
                }
            }

            OnSettingsReset();

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
                                UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value = (int)float.Parse(values[ID]);
                                break;

                            case "Toggle":
                                UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value = (int)float.Parse(values[ID]);
                                break;

                            case "Slider":
                                UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<Slider>().value = float.Parse(values[ID]);
                                break;

                            case "tField":
                                UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponentInChildren<InputField>().text = values[ID];
                                break;

                            default:
                                break;
                        }
                    }
                    else if (values.ContainsKey($"{ID}_primary"))
                    {
                        //Debug.Log($"Primary! ({ID})");
                        UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().SetPrimaryKey((KeyCode)float.Parse(values[$"{ID}_primary"]));
                        
                        if (values.ContainsKey($"{ID}_secondary"))
                        {
                            //Debug.Log($"Secondary! ({ID})");
                            UIManager.Instance.modSettingsScrollViewContent.transform.Find($"{ModAuthor}_{ModID}_{ModName}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().SetSecondaryKey((KeyCode)float.Parse(values[$"{ID}_secondary"]));
                        }
                    }
                }
            }

            valuesAfterLoad = values;

            OnSettingsLoaded();
        }
    } 

    public enum WhenToInit
    {
        InMenu,
        InGame
    }
}
