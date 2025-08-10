using JaLoader.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JaLoader
{
    public class PartIconManager : MonoBehaviour
    {
        #region Singleton
        public static PartIconManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
            EventsManager.Instance.OnLoadStart += OnLoadStart;
            EventsManager.Instance.OnGameLoad += OnGameLoad;
        }
        #endregion

        public bool ReCacheRequired;
        public int CachedItemsCount { get; private set; }

        private CustomObjectsManager objectsManager = CustomObjectsManager.Instance;

        private Camera camera;
        private Light lightComponent;

        public Dictionary<string, GameObject> extraParts = new Dictionary<string, GameObject>();

        private bool cachedItems = false;

        public Texture2D DefaultExtraTexture;

        public Dictionary<string, Texture2D> extrasCustomIcons = new Dictionary<string, Texture2D>();

        private void Start()
        {
            var go = Instantiate(new GameObject());
            DontDestroyOnLoad(go);
            go.name = "Object Screenshot Camera";

            camera = go.AddComponent<Camera>();
            camera.cullingMask &= ~(1 << 5) | ~(1 << 0);
            camera.cullingMask |= (1 << 21);
            camera.orthographic = true;
            camera.orthographicSize = 1f;
            camera.farClipPlane = 100;
            camera.clearFlags = CameraClearFlags.Depth;
            camera.enabled = false;

            var light = Instantiate(new GameObject());
            light.transform.position = new Vector3(0, 3, 0);
            light.transform.eulerAngles = new Vector3(50, -30);
            lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            light.transform.parent = go.transform;
            lightComponent.enabled = false;

            go.transform.position = new Vector3(1000, 1000, 990);
        }
        private void OnLoadStart()
        {
            if(DefaultExtraTexture == null)
            {
                byte[] bytes = File.ReadAllBytes(Path.Combine(JaLoaderSettings.ModFolderLocation, @"Required\defaultExtraTexture.png"));

                Texture2D texture = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                texture.LoadImage(bytes);

                DefaultExtraTexture = texture;
            }

            if(cachedItems)
                return;

            if (!Directory.Exists($@"{JaLoaderSettings.ModFolderLocation}\CachedImages"))
                Directory.CreateDirectory($@"{JaLoaderSettings.ModFolderLocation}\CachedImages");

            lightComponent.enabled = true;

            foreach (var entry in objectsManager.database.Keys)
            {
                if (!objectsManager.GetObject(entry).GetComponent<EngineComponentC>() || objectsManager.GetObject(entry).GetComponent<ExtraComponentC_ModExtension>())
                    continue;

                var obj = objectsManager.SpawnObjectWithoutRegistering(entry, new Vector3(1000, 1000, 1000), Vector3.zero, false);
                ModHelper.RemoveAllComponents(obj, typeof(MeshFilter), typeof(MeshRenderer), typeof(ObjectIdentification));

                var comp = obj.GetComponent<ObjectIdentification>();
                if (!comp.CanBuyInDealership)
                {
                    DestroyImmediate(obj);
                    continue;
                }

                obj.layer = 21;

                /*switch (obj.GetComponent<ObjectPickupC>().engineString)
                {
                    case "EngineBlock":
                        obj.transform.eulerAngles = //new Vector3(-40, -190, 0);
                        break;

                    case "FuelTank":
                        obj.transform.position += new Vector3(0.5f, -0.05f, 0);
                        obj.transform.eulerAngles = new Vector3(-140, -4, -210);
                        break;

                    case "Carburettor":
                        obj.transform.position += new Vector3(-0.5f, 1f, 0);
                        obj.transform.eulerAngles = new Vector3(-40, 170, 25);
                        break;

                    case "AirFilter":
                        obj.transform.position += new Vector3(-0.35f, 0f, 0);
                        obj.transform.eulerAngles = new Vector3(240, 12, 210);
                        break;

                    case "IgnitionCoil":
                        obj.transform.position += new Vector3(-1.1f, -0.8f, 0);
                        obj.transform.eulerAngles = new Vector3(-145, -3, -48);
                        break;

                    case "Battery":
                        obj.transform.position += new Vector3(0, 0.1f, 0);
                        obj.transform.eulerAngles = new Vector3(-75, -190, 330);
                        break;

                    case "WaterContainer":
                        break;
                }
                DestroyImmediate(obj.GetComponent<ObjectPickupC>());*/

                obj.transform.position += comp.PartIconPositionAdjustment;
                obj.transform.eulerAngles = comp.PartIconRotationAdjustment;
                obj.transform.localScale = comp.PartIconScaleAdjustment;

                obj.SetActive(true);

                SaveScreenshot($"{comp.ModID}_{entry}");

                DestroyImmediate(obj);
            }

            foreach (var entry in extraParts.Keys)
            {
                var objToSpawn = Instantiate(extraParts[entry]);
                SceneManager.MoveGameObjectToScene(objToSpawn, SceneManager.GetActiveScene());
                ModHelper.RemoveAllComponents(objToSpawn, typeof(MeshFilter), typeof(MeshRenderer), typeof(ObjectIdentification));
                objToSpawn.transform.position = new Vector3(1000, 1000, 1000);
                objToSpawn.layer = 21;

                var comp = objToSpawn.GetComponent<ObjectIdentification>();
                objToSpawn.transform.position += comp.PartIconPositionAdjustment;
                objToSpawn.transform.eulerAngles = comp.PartIconRotationAdjustment;
                objToSpawn.transform.localScale = comp.PartIconScaleAdjustment;

                objToSpawn.SetActive(true);
                SaveScreenshot($"{comp.ModID}_{entry}");
                objToSpawn.SetActive(false);

                DestroyImmediate(objToSpawn);
                DestroyImmediate(extraParts[entry]);
            }

            extraParts.Clear();

            cachedItems = true;
        }

        private void OnGameLoad()
        {
            lightComponent.enabled = false;
        }

        public void SetExtraToUseDefaultIcon(string registryName)
        {
            extrasCustomIcons.Add(registryName, null);
        }

        public void AddExtraCustomIcon(string registryName, Texture2D texture)
        {
            extrasCustomIcons.Add(registryName, texture);
        }

        public Texture2D GetTexture(string name)
        {
            if (File.Exists($@"{JaLoaderSettings.ModFolderLocation}\CachedImages\{name}.png") == false)
            {
                if (extrasCustomIcons.ContainsKey(name.Split('_')[1]))
                {
                    if (extrasCustomIcons[name.Split('_')[1]] == null)
                        return DefaultExtraTexture;

                    return extrasCustomIcons[name.Split('_')[1]];
                }

                Console.LogError($"Texture {name} does not exist!");
                return DefaultExtraTexture;
            }

            byte[] bytes = File.ReadAllBytes($@"{JaLoaderSettings.ModFolderLocation}\CachedImages\{name}.png");

            Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            texture.LoadImage(bytes);

            return texture;
        }

        private void SaveScreenshot(string entry)
        {
            RenderTexture screenTexture = new RenderTexture(Screen.width, Screen.height, 16);
            camera.targetTexture = screenTexture;
            RenderTexture.active = screenTexture;
            camera.Render();
            Texture2D renderedTexture = new Texture2D(Screen.width, Screen.height);
            renderedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            RenderTexture.active = null;
            byte[] byteArray = renderedTexture.EncodeToPNG();
            byte[] existingPhotoBytes = null;

            if(File.Exists($@"{JaLoaderSettings.ModFolderLocation}\CachedImages\{entry}.png"))
                existingPhotoBytes = File.ReadAllBytes($@"{JaLoaderSettings.ModFolderLocation}\CachedImages\{entry}.png");

            if (existingPhotoBytes != null)
            {
                if (byteArray.SequenceEqual(existingPhotoBytes))
                {
                    Console.LogDebug($"Cached image for {entry} already exists, skipping");
                    CachedItemsCount++;
                    return;
                }
            }

            Console.LogDebug($"Cached image for {entry}");
            File.WriteAllBytes($@"{JaLoaderSettings.ModFolderLocation}\CachedImages\{entry}.png", byteArray);
        }
    }
}
