﻿using BepInEx.Configuration;
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
        private Dictionary<string, string> BIXPlugin_settingsValues = new Dictionary<string, string>();
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
            GameObject obj = Instantiate(UIManager.Instance.ModSettingsHolder);
            obj.name = $"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder";
            obj.transform.SetParent(UIManager.Instance.ModsSettingsContent.transform, false);

            GameObject name = Instantiate(UIManager.Instance.ModSettingsNameTemplate);
            name.transform.SetParent(obj.transform, false);
            name.GetComponentInChildren<Text>().text = $"{PluginAttribute.Name} Settings";
            name.SetActive(true);

            obj.SetActive(true);
        }

        public void AddBIXPluginHeader(string text)
        {
            GameObject obj = Instantiate(UIManager.Instance.ModSettingsHeaderTemplate);
            obj.transform.SetParent(UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder"), false);
            obj.GetComponentInChildren<Text>().text = text;
            obj.SetActive(true);
        }

        public void AddBIXPluginToggle(string ID, string name, bool defaultValue)
        {
            //Console.Log($"Adding toggle {ID} to {PluginAttribute.Name} settings");

            GameObject obj = Instantiate(UIManager.Instance.ModSettingsToggleTemplate);
            obj.transform.SetParent(UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder"), false);
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
            BIXPlugin_settingsValues.Add($"{ID}_Toggle", defaultValue ? "1" : "0");
        }

        public void AddBIXPluginKeybind(string ID, string name, KeyCode defaultPrimaryKey)
        {
            //Console.Log($"Adding keybind {ID} to {PluginAttribute.Name} settings");

            GameObject obj = Instantiate(UIManager.Instance.ModSettingsKeybindTemplate);
            obj.transform.SetParent(UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder"), false);
            obj.name = $"{ID}_Keybind";

            obj.SetActive(true);

            obj.AddComponent<CustomKeybind>();
            obj.GetComponent<CustomKeybind>().SetPrimaryKey(defaultPrimaryKey);
            obj.GetComponent<CustomKeybind>().EnableAltKey = false;
            obj.transform.Find("HeaderText").GetComponent<Text>().text = name;

            AddBIXPluginHeader("Keybinds are not supported yet!");

            BIXPlugin_settingsIDS.Add($"{ID}_Keybind");
            BIXPlugin_settingsValues.Add($"{ID}_Keybind", defaultPrimaryKey.ToString());
        }

        public Dropdown GetBIXPluginToggle(string ID)
        {
            if (UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}_Toggle"))
                return UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}_Toggle").GetComponentInChildren<Dropdown>();
            else
                return null;
        }

        public KeyCode GetBIXPluginKeybind(string ID)
        {
            //Console.Log(ID);
            //Console.Log(UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}_Keybind"));

            if (UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}_Keybind"))
                return UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}_Keybind").GetComponent<CustomKeybind>().SelectedKey;
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

            //Console.Log(BIXPlugin_settingsIDS.Count);

            foreach (var ID in BIXPlugin_settingsIDS)
            {
                //Console.Log($"Saving {ID} to {PluginAttribute.Name} settings with value {UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value}");
                string type = Regex.Match(ID, @"(.{6})\s*$").ToString();

                switch (type)
                {
                    case "opdown":
                        values.Add(ID, UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value);
                        break;

                    case "Toggle":
                        values.Add(ID, UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value);
                        break;

                    case "Slider":
                        values.Add(ID, UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Slider>().value);
                        break;

                    case "eybind":
                        values.Add($"{ID}_primary", (int)UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().SelectedKey);
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

        public void ResetBIXPluginSettings()
        {
            // iterate through settingIDS and reset them to default using the values dictionary
            foreach (var ID in BIXPlugin_settingsIDS)
            {
                string type = Regex.Match(ID, @"(.{6})\s*$").ToString();

                switch (type)
                {
                    case "opdown":
                        UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value = int.Parse(BIXPlugin_settingsValues[ID]);
                        break;

                    case "Toggle":
                        UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value = int.Parse(BIXPlugin_settingsValues[ID]);
                        break;

                    case "Slider":
                        UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Slider>().value = float.Parse(BIXPlugin_settingsValues[ID]);
                        break;

                    case "eybind":
                        UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().SetPrimaryKey((KeyCode)Enum.Parse(typeof(KeyCode), BIXPlugin_settingsValues[$"{ID}_primary"]));
                        break;

                    default:
                        break;
                }
            }
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
                                UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value = (int)values[ID];
                                break;

                            case "Toggle":
                                UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Dropdown>().value = (int)values[ID];
                                break;

                            case "Slider":
                                UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponentInChildren<Slider>().value = values[ID];
                                break;

                            default:
                                break;
                        }
                    }
                    else if (values.ContainsKey($"{ID}_primary"))
                    {
                        UIManager.Instance.ModsSettingsContent.Find($"BepInEx_CompatLayer_{PluginAttribute.GUID}-SettingsHolder/{ID}").GetComponent<CustomKeybind>().SetPrimaryKey((KeyCode)values[$"{ID}_primary"]);
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
