using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoaderUnity4
{
    public class ModUnity4 : MonoBehaviour
    {
        public virtual string ModID { get; set; }
        public virtual string ModName { get; set; }
        public virtual string ModAuthor { get; set; }
        public virtual string ModDescription { get; set; }
        public virtual string ModVersion { get; set; }
        public virtual string GitHubLink { get; set; }
        public virtual bool UseAssets { get; set; }
        public virtual WhenToInit WhenToInit { get; set; }

        public virtual List<(string, string, string)> Dependencies { get; set; } = new List<(string, string, string)>();

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

        public virtual void OnReload() { }

        public T LoadAsset<T>(string assetName, string prefabName, string fileSuffix, string prefabSuffix) where T : UnityEngine.Object
        {
            /*if (!UseAssets)
            {
                Console.LogError(ModID, "Tried to call LoadAssets, but UseAssets is false.");
                return null;
            }

            if (!File.Exists(Path.Combine(AssetsPath, $"{assetName}{fileSuffix}")))
            {
                Console.LogError(ModID, $"Tried to load asset {assetName}{fileSuffix}, but it does not exist.");
                return null;
            }*/

            Debug.Log($"Loading asset" + assetName + fileSuffix + "(from path " + Path.Combine(AssetsPath, assetName + fileSuffix) + ") with prefab " + prefabName + prefabSuffix);
            var ab = AssetBundle.CreateFromFile(Path.Combine(AssetsPath, assetName + fileSuffix));
            if(ab == null)
                Debug.Log("ab is null");
            var asset = ab.Load(prefabName);CarControleScript

            if (asset == null)
            {
                Debug.Log("asset is null");
                //Console.LogError(ModID, $"Tried to load {typeof(T).Name} {prefabName}{prefabSuffix} from asset {assetName}, but it does not exist.");
                ab.Unload(true);
                return null;
            }

            if (typeof(T) == typeof(GameObject))
            {
                GameObject obj = Instantiate(asset) as GameObject;

                //obj.name = $"{ModID}_{prefabName}";

                /*var identification = obj.AddComponent<ObjectIdentification>();
                identification.ModID = ModID;
                identification.ModName = ModName;
                identification.Author = ModAuthor;
                identification.Version = ModVersion;*/

                ab.Unload(false);

                return (T)(object)obj;
            }

            ab.Unload(false);

            return (T)asset;
        }

    }

    public enum WhenToInit
    {
        InMenu,
        InGame
    }
}
