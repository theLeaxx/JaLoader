using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace JaLoaderUnity4
{
    public class ModHelper : MonoBehaviour
    {
        public static ModHelper Instance { get; private set; }

        #region Singleton & OnSceneChange
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

            EventsManager.Instance.OnGameLoad += OnGameLoad;
            EventsManager.Instance.OnGameUnload += OnGameUnload;
            EventsManager.Instance.OnMenuLoad += OnMenuLoad;
        }
        #endregion

        public GameObject player;
        public GameObject laika;
        public Wallet wallet;
        public Director director;

        public Material defaultEngineMaterial;
        private AudioClip[] defaultClips;

        //public Dictionary<PartTypes, Transform> partHolders = new Dictionary<PartTypes, Transform>();
        //public Dictionary<WheelPositions, Transform> wheelHolders = new Dictionary<WheelPositions, Transform>(); TODO

        private bool addedExtensions;
        private bool createdDebugCamera;
        public GameObject debugCam;

        public GameObject CardboardBoxBig;
        public GameObject CardboardBoxMed;
        public GameObject CardboardBoxSmall;

        public GameObject CrateBig;
        public GameObject CrateMed;
        public GameObject CrateSmall;

        public bool patchedEverything = false;

        /*private Material defaultGlowMaterial = new Material(Shader.Find("Legacy Shaders/Transparent/Specular"))
        {
            color = new Color(1, 1, 1, 0.17f)
        };*/

        private readonly List<(GameObject, string, string, int, int)> boxesToCreateInGame = new List<(GameObject, string, string, int, int)>();

        private void OnMenuLoad()
        {
            //RefreshPartHolders();

            laika = GameObject.Find("FrameHolder");
            //laika.AddComponent<LicensePlateCustomizer>();

            if (defaultEngineMaterial == null)
            {
                //GameObject go = GameObject.Find("EngineBlock");

                //defaultEngineMaterial = go.GetComponent<MeshRenderer>().material;
                //defaultClips = go.GetComponent<ObjectPickup>()._audio;

                if (SettingsManager.Instance.DebugMode)
                {
                    if (!createdDebugCamera)
                    {
                        debugCam = Instantiate(new GameObject()) as GameObject;
                        debugCam.name = "JaLoader Debug Camera";
                        debugCam.SetActive(false);

                        var effectsCam = Instantiate(new GameObject()) as GameObject;
                        effectsCam.name = "JaLoader Menu Post Processing Camera";

                        var inGameEffectsCam = Instantiate(new GameObject()) as GameObject;
                        inGameEffectsCam.name = "JaLoader In-Game Post Processing Camera";

                        var normalCam = Instantiate(new GameObject()) as GameObject;
                        normalCam.name = "JaLoader Normal Camera";

                        effectsCam.transform.parent = normalCam.transform.parent = inGameEffectsCam.transform.parent = debugCam.transform;

                        var components = Camera.main.GetComponents<MonoBehaviour>().ToList();
                        components.RemoveAt(components.Count - 1);
                        components.RemoveAt(components.Count - 1);
                        components.RemoveAt(components.Count - 1);
                        components.RemoveAt(0);

                        effectsCam.AddComponent<Camera>();
                        inGameEffectsCam.AddComponent<Camera>();
                        normalCam.AddComponent<Camera>();

                        foreach (MonoBehaviour behaviour in components)
                        {
                            effectsCam.AddComponent(behaviour.GetType());
                            FieldInfo[] fields = behaviour.GetType().GetFields();
                            foreach (FieldInfo field in fields)
                            {
                                field.SetValue(effectsCam.GetComponent(behaviour.GetType()), field.GetValue(behaviour));
                            }
                        }

                        effectsCam.GetComponent<Camera>().nearClipPlane = 0.025f;
                        normalCam.GetComponent<Camera>().nearClipPlane = 0.025f;
                        inGameEffectsCam.GetComponent<Camera>().nearClipPlane = 0.025f;

                        effectsCam.GetComponent<Camera>().fieldOfView = 80;
                        normalCam.GetComponent<Camera>().fieldOfView = 80;
                        inGameEffectsCam.GetComponent<Camera>().fieldOfView = 80;

                        normalCam.SetActive(false);
                        inGameEffectsCam.SetActive(false);
                        debugCam.SetActive(false);

                        DontDestroyOnLoad(debugCam);

                        createdDebugCamera = true;
                    }

                    Camera.main.gameObject.AddComponent<DebugCamera>();
                }
            }
        }

        private void OnGameLoad()
        {
            //if (SettingsManager.Instance.DebugMode)
                Camera.main.gameObject.AddComponent<DebugCamera>();

            //RefreshPartHolders();

            if (!addedExtensions)
            {
                //Camera.main.gameObject.AddComponent<DragRigidbodyC_ModExtension>();
                player = Camera.main.transform.parent.gameObject;
                laika = GameObject.Find("FrameHolder");
                wallet = FindObjectOfType<Wallet>();
                director = FindObjectOfType<Director>();
                //laika.AddComponent<LicensePlateCustomizer>();
                addedExtensions = true;

                /*RouteGeneratorC route = FindObjectOfType<RouteGeneratorC>();
                CardboardBoxBig = route.cratePrefabs[0];
                CardboardBoxMed = route.cratePrefabs[1];
                CardboardBoxSmall = route.cratePrefabs[2];
                CrateBig = route.cratePrefabs[3];
                CrateMed = route.cratePrefabs[4];
                CrateSmall = route.cratePrefabs[5];*/



                //OverwriteBoxObjects();

                /*Camera.main.gameObject.AddComponent<LaikaCatalogueExtension>();

                if (SettingsManager.IsPreReleaseVersion)
                {
                    var obj = Instantiate(new GameObject());
                    obj.name = "JaLoader Game Scripts";
                    obj.AddComponent<MarketManager>();
                }*/
            }
        }

        private void OnGameUnload()
        {
            addedExtensions = false;
        }

    }
}
