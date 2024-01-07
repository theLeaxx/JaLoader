using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BepInEx.Configuration
{
    // TODO: Make the code not shit
    public class ConfigFile
    {
        public BaseUnityPlugin Plugin { get; }
        public BepInPlugin PluginAttribute { get; }

        public ConfigEntry<T> Bind<T>(ConfigDefinition configDefinition, T defaultValue, ConfigDescription configDescription = null)
        {
            //JaLoader.Console.Instance.Log($"E 2 {(defaultValue as KeyboardShortcut).Key.ToString()}");
            var entry = new ConfigEntry<T>(configDefinition.section, configDefinition.key, defaultValue, Plugin, PluginAttribute, "");

            return entry;
        }

        public ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, ConfigDescription configDescription = null)
        {
            //JaLoader.Console.Instance.Log($"E {(defaultValue as KeyboardShortcut).Key.ToString()}");
            return Bind(new ConfigDefinition(section, key), defaultValue, configDescription);
        }

        public ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, string description)
        {
            return Bind(new ConfigDefinition(section, key), defaultValue, new ConfigDescription(description, null));
        }

        public ConfigFile(BaseUnityPlugin plugin)
        {
            Plugin = plugin;
            PluginAttribute = plugin.PluginAttribute;
        }
    }

    public class ConfigDefinition
    {
        public string section { get; }
        public string key { get; }

        public ConfigDefinition(string section, string key)
        {
            this.section = section;
            this.key = key;
        }
    }

    public class ConfigDescription
    {
        public string description { get; }
        private object value;

        public ConfigDescription(string description, object value)
        {
            this.description = description;
            this.value = value;
        }
    }

    public class ConfigEntry<T> : IConfigEntry
    {
        public T _typedValue;
        private BaseUnityPlugin Plugin;
        private BepInPlugin PluginAttribute;
        private string Name;

        private KeyboardShortcut shortcut;

        public ConfigEntry(T defaultValue)
        {
            _typedValue = defaultValue;
        }

        public ConfigEntry(string section, string name, T defaultValue, BaseUnityPlugin plugin, BepInPlugin pluginAttribute, string description = null)
        {
            _typedValue = defaultValue;

            Plugin = plugin;
            PluginAttribute = pluginAttribute;
            Name = name;

            if (typeof(T) == typeof(bool))
            {
                Plugin.configEntries.Add(this, (name, name));
                //plugin.AddBIXPluginToggle(name, name, (bool)(object)_typedValue);
            }
            else if (typeof(T) == typeof(KeyboardShortcut))
            {
                //JaLoader.Console.Instance.Log($"E 3 Value Type: {_typedValue.GetType().FullName}");
                //JaLoader.Console.Instance.Log($"E 3 Constructed with Key: {(_typedValue as KeyboardShortcut).Key.ToString()}");

                shortcut = new KeyboardShortcut((defaultValue as KeyboardShortcut).Key);

                //JaLoader.Console.Instance.Log(_typedValue);
                //(_typedValue as KeyboardShortcut).keyName = name;
                //(_typedValue as KeyboardShortcut).plugin = plugin;
                //(_typedValue as KeyboardShortcut).Key = (defaultValue as KeyboardShortcut).Key;
                Plugin.configEntries.Add(this, (name, name));
            }
        }

        public T Value
        {
            get
            {
                if (typeof(T) == typeof(bool))
                {
                    return (T)(object)Plugin.GetBIXPluginToggleValue(Name);
                }
                else if(typeof(T) == typeof(KeyboardShortcut))
                {
                    //JaLoader.Console.Instance.Log(_typedValue);
                    //(_typedValue as KeyboardShortcut).Key = Plugin.GetBIXPluginKeybind(Name);
                    //JaLoader.Console.Instance.Log((_typedValue as KeyboardShortcut).Key.ToString());
                    //JaLoader.Console.Instance.Log((_typedValue as KeyboardShortcut).keyName);
                    //JaLoader.Console.Instance.Log((_typedValue as KeyboardShortcut).plugin.Config.PluginAttribute.GUID);
                    //JaLoader.Console.Instance.Log($"E 3 Accessed Value with Key: {(shortcut as KeyboardShortcut).Key}");
                    return (T)(object)shortcut;
                }
                else
                {
                    return _typedValue;
                }
                    
            }
            set
            {
                if (!Equals(_typedValue, value))
                {
                    _typedValue = value;

                    if (typeof(T) == typeof(bool))
                    {
                        Plugin.GetBIXPluginToggle(Name).value = (bool)(object)_typedValue ? 0 : 1;
                    }
                    else if (typeof(T) == typeof(KeyboardShortcut))
                    {
                        //(_typedValue as KeyboardShortcut).Key = Plugin.GetBIXPluginKeybind(Name);
                    }
                }
            }
        }
    }

    public interface IConfigEntry
    {

    }

    public class SimpleKeyboardShortcut
    {
        public KeyCode Key;
        public BaseUnityPlugin plugin;
        public string keyName;

        public SimpleKeyboardShortcut(KeyCode key, BaseUnityPlugin _plugin, string _keyName)
        {
            Key = key;
            plugin = _plugin;
            keyName = _keyName;
        }

        public bool IsDown()
        {
            return false;

            /*if (Key == KeyCode.None || plugin == null || keyName == "")
            {
                return false;
            }

            KeyCode keyToCheck = plugin.GetBIXPluginKeybind(keyName);
            return Input.GetKeyDown(keyToCheck);*/
        }
    }

    // this does not work yet
    public class KeyboardShortcut
    {
        public KeyCode Key { get; set; }

        public readonly KeyboardShortcut None;
        private KeyCode[] keyCodes = new KeyCode[0];

        public BaseUnityPlugin plugin;
        public string keyName = "";

        public KeyboardShortcut()
        {
            None = new KeyboardShortcut(KeyCode.None);
        }

        public KeyboardShortcut(KeyCode mainKey, params KeyCode[] modifiers): this(new KeyCode[1] { mainKey }.Concat(modifiers).ToArray())
        {
            Key = mainKey;
            keyCodes = new KeyCode[1] { mainKey }.Concat(modifiers).ToArray();
            //JaLoader.Console.Instance.Log($"Constructed KeyboardShortcut with Key: {Key}");
            //keyName = "";
            //JaLoader.Console.Instance.Log(Key.ToString());
        }

        public KeyboardShortcut(KeyCode[] keyCodes)
        {
            Key = keyCodes[0];
            this.keyCodes = keyCodes;
        }

        // fucking fuck this portion i spent 3 days trying to get it to work but it kept returning random ass keycodes that did not exist
        // burn it in flames.
        public bool IsDown()
        {
            return false;

#pragma warning disable CS0162 // Unreachable code detected
            return Input.GetKeyDown(KeyCode.F1);

            string callingMethod = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;
            //string callingClass = this.GetType().FullName;

            JaLoader.Console.Instance.Log($"IsDown method called from: {callingMethod}");
            JaLoader.Console.Instance.Log($"Key before check: {Key}, Type: {Key.GetType().FullName}");

            bool isDown = Key != KeyCode.None && Input.GetKeyDown(Key);

            JaLoader.Console.Instance.Log($"Key after check: {Key}, Type: {Key.GetType().FullName}");

            return isDown;
            return false;

            JaLoader.Console.Instance.Log($"Key in IsDown: {Key}");
            //return Input.GetKeyDown(key));
            return false;

            //Debug.Log(keyName);

            /*if(Key == KeyCode.None || plugin == null || keyName == "")
            {
                //JaLoader.Console.Instance.Log(keyName);
                return false;
            }

            KeyCode keyToCheck = plugin.GetBIXPluginKeybind(keyName);
            //JaLoader.Console.Instance.Log(plugin.GetBIXPluginKeybind(keyName).ToString());
            return Input.GetKeyDown(keyToCheck);
            return false;*/
        }
    }

    public class TypeConverter
    {
        public Func<object, Type, string> ConvertToString { get; set; }

        public Func<string, Type, object> ConvertToObject { get; set; }
    }

    public static class TomlTypeConverter
    {
        private static bool _lazyLoadedConverters;

        private static Dictionary<Type, TypeConverter> TypeConverters { get; } = new Dictionary<Type, TypeConverter>
        {
            [typeof(string)] = new TypeConverter
            {
                ConvertToString = (object obj, Type type) => ((string)obj).Escape(),
                ConvertToObject = (string str, Type type) => Regex.IsMatch(str, "^\"?\\w:\\\\(?!\\\\)(?!.+\\\\\\\\)") ? str : str.Unescape()
            },
            [typeof(bool)] = new TypeConverter
            {
                ConvertToString = (object obj, Type type) => obj.ToString().ToLowerInvariant(),
                ConvertToObject = (string str, Type type) => bool.Parse(str)
            },
            [typeof(byte)] = new TypeConverter
            {
                ConvertToString = (object obj, Type type) => obj.ToString(),
                ConvertToObject = (string str, Type type) => byte.Parse(str)
            },
            [typeof(sbyte)] = new TypeConverter
            {
                ConvertToString = (object obj, Type type) => obj.ToString(),
                ConvertToObject = (string str, Type type) => sbyte.Parse(str)
            },
            [typeof(byte)] = new TypeConverter
            {
                ConvertToString = (object obj, Type type) => obj.ToString(),
                ConvertToObject = (string str, Type type) => byte.Parse(str)
            },
            [typeof(short)] = new TypeConverter
            {
                ConvertToString = (object obj, Type type) => obj.ToString(),
                ConvertToObject = (string str, Type type) => short.Parse(str)
            },
            [typeof(ushort)] = new TypeConverter
            {
                ConvertToString = (object obj, Type type) => obj.ToString(),
                ConvertToObject = (string str, Type type) => ushort.Parse(str)
            },
            [typeof(int)] = new TypeConverter
            {
                ConvertToString = (object obj, Type type) => obj.ToString(),
                ConvertToObject = (string str, Type type) => int.Parse(str)
            },
            [typeof(uint)] = new TypeConverter
            {
                ConvertToString = (object obj, Type type) => obj.ToString(),
                ConvertToObject = (string str, Type type) => uint.Parse(str)
            },
            [typeof(long)] = new TypeConverter
            {
                ConvertToString = (object obj, Type type) => obj.ToString(),
                ConvertToObject = (string str, Type type) => long.Parse(str)
            },
            [typeof(ulong)] = new TypeConverter
            {
                ConvertToString = (object obj, Type type) => obj.ToString(),
                ConvertToObject = (string str, Type type) => ulong.Parse(str)
            },
            [typeof(float)] = new TypeConverter
            {
                ConvertToString = (object obj, Type type) => ((float)obj).ToString(NumberFormatInfo.InvariantInfo),
                ConvertToObject = (string str, Type type) => float.Parse(str, NumberFormatInfo.InvariantInfo)
            },
            [typeof(double)] = new TypeConverter
            {
                ConvertToString = (object obj, Type type) => ((double)obj).ToString(NumberFormatInfo.InvariantInfo),
                ConvertToObject = (string str, Type type) => double.Parse(str, NumberFormatInfo.InvariantInfo)
            },
            [typeof(decimal)] = new TypeConverter
            {
                ConvertToString = (object obj, Type type) => ((decimal)obj).ToString(NumberFormatInfo.InvariantInfo),
                ConvertToObject = (string str, Type type) => decimal.Parse(str, NumberFormatInfo.InvariantInfo)
            },
            [typeof(Enum)] = new TypeConverter
            {
                ConvertToString = (object obj, Type type) => obj.ToString(),
                ConvertToObject = (string str, Type type) => Enum.Parse(type, str, ignoreCase: true)
            }
        };


        public static string ConvertToString(object value, Type valueType)
        {
            return (GetConverter(valueType) ?? throw new InvalidOperationException($"Cannot convert from type {valueType}")).ConvertToString(value, valueType);
        }

        public static T ConvertToValue<T>(string value)
        {
            return (T)ConvertToValue(value, typeof(T));
        }

        public static object ConvertToValue(string value, Type valueType)
        {
            return (GetConverter(valueType) ?? throw new InvalidOperationException("Cannot convert to type " + valueType.Name)).ConvertToObject(value, valueType);
        }

        public static TypeConverter GetConverter(Type valueType)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            if (valueType.IsEnum)
            {
                return TypeConverters[typeof(Enum)];
            }

            if (!TypeConverters.TryGetValue(valueType, out var value) && !_lazyLoadedConverters)
            {
                _lazyLoadedConverters = true;
                TypeConverters.TryGetValue(valueType, out value);
            }

            return value;
        }

        public static bool AddConverter(Type type, TypeConverter converter)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }

            if (CanConvert(type))
            {
                JaLoader.Console.Instance.LogWarning("Tried to add a TomlConverter when one already exists for type " + type.FullName);
                return false;
            }

            TypeConverters.Add(type, converter);
            return true;
        }

        public static bool CanConvert(Type type)
        {
            return GetConverter(type) != null;
        }

        public static IEnumerable<Type> GetSupportedTypes()
        {
            return TypeConverters.Keys;
        }

        private static string Escape(this string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = new StringBuilder(txt.Length + 2);
            foreach (char c in txt)
            {
                switch (c)
                {
                    case '\0':
                        stringBuilder.Append("\\0");
                        break;
                    case '\a':
                        stringBuilder.Append("\\a");
                        break;
                    case '\b':
                        stringBuilder.Append("\\b");
                        break;
                    case '\t':
                        stringBuilder.Append("\\t");
                        break;
                    case '\n':
                        stringBuilder.Append("\\n");
                        break;
                    case '\v':
                        stringBuilder.Append("\\v");
                        break;
                    case '\f':
                        stringBuilder.Append("\\f");
                        break;
                    case '\r':
                        stringBuilder.Append("\\r");
                        break;
                    case '\'':
                        stringBuilder.Append("\\'");
                        break;
                    case '\\':
                        stringBuilder.Append("\\");
                        break;
                    case '"':
                        stringBuilder.Append("\\\"");
                        break;
                    default:
                        stringBuilder.Append(c);
                        break;
                }
            }

            return stringBuilder.ToString();
        }

        private static string Unescape(this string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return txt;
            }

            StringBuilder stringBuilder = new StringBuilder(txt.Length);
            int num = 0;
            while (num < txt.Length)
            {
                int num2 = txt.IndexOf('\\', num);
                if (num2 < 0 || num2 == txt.Length - 1)
                {
                    num2 = txt.Length;
                }

                stringBuilder.Append(txt, num, num2 - num);
                if (num2 >= txt.Length)
                {
                    break;
                }

                char c = txt[num2 + 1];
                switch (c)
                {
                    case '0':
                        stringBuilder.Append('\0');
                        break;
                    case 'a':
                        stringBuilder.Append('\a');
                        break;
                    case 'b':
                        stringBuilder.Append('\b');
                        break;
                    case 't':
                        stringBuilder.Append('\t');
                        break;
                    case 'n':
                        stringBuilder.Append('\n');
                        break;
                    case 'v':
                        stringBuilder.Append('\v');
                        break;
                    case 'f':
                        stringBuilder.Append('\f');
                        break;
                    case 'r':
                        stringBuilder.Append('\r');
                        break;
                    case '\'':
                        stringBuilder.Append('\'');
                        break;
                    case '"':
                        stringBuilder.Append('"');
                        break;
                    case '\\':
                        stringBuilder.Append('\\');
                        break;
                    default:
                        stringBuilder.Append('\\').Append(c);
                        break;
                }

                num = num2 + 2;
            }

            return stringBuilder.ToString();
        }
    }
}
