using BepInEx.Configuration;
using BepInEx.Logging;
using JaLoader;
using JaLoader.BepInExWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Console = JaLoader.Console;

namespace BepInEx
{
    //TODO: Optimize this shit ass file
    public class BaseUnityPlugin : MonoBehaviour
    {
        protected ManualLogSource Logger { get; }
        public ConfigFile Config { get; }

        public BepInPlugin PluginAttribute { get; }

        public List<string> BIXPlugin_settingsIDS = new List<string>();
        [Serializable] class BIXPlugin_SettingsValues : SerializableDictionary<string, float> { }

        public Dictionary<IConfigEntry, (string, string)> configEntries = new Dictionary<IConfigEntry, (string, string)>();

        public BaseUnityPlugin()
        {
            PluginAttribute = (BepInPlugin)Attribute.GetCustomAttribute(GetType(), typeof(BepInPlugin));

            Logger = new ManualLogSource(PluginAttribute.GUID);
            Config = new ConfigFile(this);
        }

        public void InstantiateBIXPluginSettings()
        {
            GameObject obj = Instantiate(UIManager.Instance.modOptionsHolder);
            obj.name = $"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder";
            obj.transform.SetParent(UIManager.Instance.modSettingsScrollViewContent.transform, false);

            GameObject name = Instantiate(UIManager.Instance.modOptionsNameTemplate);
            name.transform.SetParent(obj.transform, false);
            name.GetComponentInChildren<Text>().text = $"{PluginAttribute.Name} Settings";
            name.SetActive(true);

            obj.SetActive(true);
        }

        public void AddBIXPluginHeader(string text)
        {
            GameObject obj = Instantiate(UIManager.Instance.modOptionsHeaderTemplate);
            obj.transform.SetParent(UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder"), false);
            obj.GetComponentInChildren<Text>().text = text;
            obj.SetActive(true);
        }

        public void AddBIXPluginToggle(string ID, string name, bool defaultValue)
        {
            //Console.Instance.Log($"Adding toggle {ID} to {PluginAttribute.Name} settings");

            GameObject obj = Instantiate(UIManager.Instance.modOptionsToggleTemplate);
            obj.transform.SetParent(UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder"), false);
            obj.name = $"{ID}_Toggle";

            List<Dropdown.OptionData> optionData = new List<Dropdown.OptionData>
            {
                new Dropdown.OptionData($"{name}: On"),
                new Dropdown.OptionData($"{name}: Off")
            };

            obj.GetComponentInChildren<Dropdown>().options = optionData;
            obj.GetComponentInChildren<Dropdown>().value = defaultValue ? 0 : 1;
            obj.SetActive(true);

            BIXPlugin_settingsIDS.Add($"{ID}_Toggle");
        }

        public void AddBIXPluginKeybind(string ID, string name, KeyCode defaultPrimaryKey)
        {
            //Console.Instance.Log($"Adding keybind {ID} to {PluginAttribute.Name} settings");

            GameObject obj = Instantiate(UIManager.Instance.modOptionsKeybindTemplate);
            obj.transform.SetParent(UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder"), false);
            obj.name = $"{ID}_Keybind";

            obj.SetActive(true);

            obj.AddComponent<CustomKeybind>();
            obj.GetComponent<CustomKeybind>().SetPrimaryKey(defaultPrimaryKey);
            obj.GetComponent<CustomKeybind>().EnableAltKey = false;
            obj.transform.Find("HeaderText").GetComponent<Text>().text = name;

            AddBIXPluginHeader("Keybinds are not supported yet!");

            BIXPlugin_settingsIDS.Add($"{ID}_Keybind");
        }

        public Dropdown GetBIXPluginToggle(string ID)
        {
            if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}_Toggle"))
                return UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}_Toggle").GetComponentInChildren<Dropdown>();
            else
                return null;
        }

        public KeyCode GetBIXPluginKeybind(string ID)
        {
            //Console.Instance.Log(ID);
            //Console.Instance.Log(UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}_Keybind"));

            if (UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}_Keybind"))
                return UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}_Keybind").GetComponent<CustomKeybind>().SelectedKey;
            else
                return KeyCode.None;
        }

        public bool GetBIXPluginToggleValue(string ID)
        {
            var toggle = GetBIXPluginToggle(ID);

            if (toggle != null)
                return toggle.value == 0;

            return false;
        }

        public void SaveBIXPluginSettings()
        {
            BIXPlugin_SettingsValues values = new BIXPlugin_SettingsValues();

            Console.Instance.Log(BIXPlugin_settingsIDS.Count);

            foreach (var ID in BIXPlugin_settingsIDS)
            {
                //Console.Instance.Log($"Saving {ID} to {PluginAttribute.Name} settings with value {UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value}");
                string type = Regex.Match(ID, @"(.{6})\s*$").ToString();

                switch (type)
                {
                    case "opdown":
                        values.Add(ID, UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value);
                        break;

                    case "Toggle":
                        values.Add(ID, UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value);
                        break;

                    case "Slider":
                        values.Add(ID, UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Slider>().value);
                        break;

                    case "eybind":
                        values.Add($"{ID}_primary", (int)UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().SelectedKey);
                        break;

                    default:
                        break;
                }
            }

            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, $@"ModSaves")))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, $@"ModSaves"));
            }

            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, $@"ModSaves\{PluginAttribute.GUID}")))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, $@"ModSaves\{PluginAttribute.GUID}"));
            }

            File.WriteAllText(Path.Combine(Application.persistentDataPath, $@"ModSaves\{PluginAttribute.GUID}\{PluginAttribute.GUID}_save.json"), JsonUtility.ToJson(values, true));
        }

        public void LoadBIXPluginSettings()
        {
            BIXPlugin_SettingsValues values = new BIXPlugin_SettingsValues();

            if (File.Exists(Path.Combine(Application.persistentDataPath, $@"ModSaves\{PluginAttribute.GUID}\{PluginAttribute.GUID}_save.json")))
            {
                string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, $@"ModSaves\{PluginAttribute.GUID}\{PluginAttribute.GUID}_save.json"));
                values = JsonUtility.FromJson<BIXPlugin_SettingsValues>(json);

                foreach (var ID in BIXPlugin_settingsIDS)
                {
                    if (values.ContainsKey(ID))
                    {
                        string type = Regex.Match(ID, @"(.{6})\s*$").ToString();

                        switch (type)
                        {
                            case "opdown":
                                UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value = (int)values[ID];
                                break;

                            case "Toggle":
                                UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value = (int)values[ID];
                                break;

                            case "Slider":
                                UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Slider>().value = values[ID];
                                break;

                            default:
                                break;
                        }
                    }
                    else if (values.ContainsKey($"{ID}_primary"))
                    {
                        UIManager.Instance.modSettingsScrollViewContent.transform.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().SetPrimaryKey((KeyCode)values[$"{ID}_primary"]);
                    }
                }
            }
            else
            {
                return;
            }
        }
    }
}
