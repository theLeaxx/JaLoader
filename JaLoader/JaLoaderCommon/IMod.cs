using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JaLoader.Common
{
    public interface IMod
    {
        string ModID { get; }
        string ModName { get; }
        string ModAuthor { get; }
        string ModDescription { get; }
        string ModVersion { get; }
        string GitHubLink { get; }
        string NexusModsLink { get; }
        bool UseAssets { get; }
        WhenToInit WhenToInitMod { get; }

        List<(string, string, string)> Dependencies { get; }

        List<(string, string, string)> Incompatibilities { get; }

        string AssetsPath { get; }

        List<string> settingsIDS { get; }
        Dictionary<string, string> settingsValues { get; }
        //[Serializable] class SettingsValues : SerializableDictionary<string, string> { }

        Dictionary<string, string> valuesAfterLoad { get; }

        void EventsDeclaration();
        void SettingsDeclaration();
        void CustomObjectsRegistration();
        void Update();
        void Start();
        void Awake();
        void OnEnable();
        void Preload();
        void OnDisable();
        void OnDestroy();
        void OnReload();
        void OnSettingsSaved();
        void OnSettingValueChanged(string ID);
        void OnSettingsReset();
        void OnSettingsLoaded();
        void OnExtraAttached(string extraName);

    }
}
