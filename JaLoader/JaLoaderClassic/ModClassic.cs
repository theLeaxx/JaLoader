using JaLoader.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoaderClassic
{
    public class ModClassic : MonoBehaviour, IMod
    {
        public virtual string ModID { get; set; }
        public virtual string ModName { get; set; }
        public virtual string ModAuthor { get; set; }
        public virtual string ModDescription { get; set; }
        public virtual string ModVersion { get; set; }
        public virtual string GitHubLink { get; set; }
        public virtual string NexusModsLink { get; set; }
        public virtual bool UseAssets { get; set; }
        public virtual WhenToInit WhenToInitMod { get; set; }

        public virtual List<(string, string, string)> Dependencies { get; set; } = new List<(string, string, string)>();

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

        //[Serializable] class SettingsValues : SerializableDictionary<string, string> { }

        public virtual void EventsDeclaration() { }
        public virtual void SettingsDeclaration() { }
        public virtual void CustomObjectsRegistration() { }
        public virtual void Update() { }
        public virtual void Start() { }
        public virtual void Awake() { }
        public virtual void OnEnable() { }
        public virtual void Preload() { }
        public virtual void OnDisable() { }
        public virtual void OnDestroy() { }
        public virtual void OnReload() { }
        public virtual void OnSettingsSaved() { }
        public virtual void OnSettingValueChanged(string ID) { }
        public virtual void OnSettingsReset() { }
        public virtual void OnSettingsLoaded() { }
        public virtual void OnExtraAttached(string extraName) { }
    }
}
