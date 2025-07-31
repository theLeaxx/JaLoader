using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoaderClassic
{
    public class ModClassic : MonoBehaviour
    {
        public virtual string ModID { get; set; }
        public virtual string ModName { get; set; }
        public virtual string ModAuthor { get; set; }
        public virtual string ModDescription { get; set; }
        public virtual string ModVersion { get; set; }
        public virtual string GitHubLink { get; set; }
        public virtual bool UseAssets { get; set; }
        //public virtual WhenToInit WhenToInit { get; set; }

        //public virtual List<(string, string, string)> Dependencies { get; set; } = new List<(string, string, string)>();

        public string AssetsPath { get; set; }

        public List<string> settingsIDS = new List<string>();
        //[Serializable] class SettingsValues : SerializableDictionary<string, float> { }

        public virtual void EventsDeclaration() { }

        public virtual void SettingsDeclaration() { }

        public virtual void CustomObjectsRegistration() { }

        public virtual void Update() { }
        public virtual void Start() { }
        public virtual void Awake() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void OnDestroy() { }
    }
}
