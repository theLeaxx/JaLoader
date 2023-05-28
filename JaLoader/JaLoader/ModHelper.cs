using System.Collections.Generic;
using UnityEngine;
using Theraot.Collections;
using System.Reflection;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System;

namespace JaLoader
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

        public Material defaultEngineMaterial;
        private AudioClip[] defaultClips;

        public Dictionary<PartTypes, Transform> partHolders = new Dictionary<PartTypes, Transform>();
        //public Dictionary<WheelPositions, Transform> wheelHolders = new Dictionary<WheelPositions, Transform>(); TODO

        private bool addedExtensions;
        private bool createdDebugCamera;
        public GameObject debugCam;

        private void OnMenuLoad()
        {
            RefreshPartHolders();

            if (defaultEngineMaterial == null)
            {
                GameObject go = GameObject.Find("EngineBlock");

                defaultEngineMaterial = go.GetComponent<MeshRenderer>().material;
                defaultClips = go.GetComponent<ObjectPickupC>()._audio;

                if (SettingsManager.Instance.DebugMode)
                {
                    if (!createdDebugCamera)
                    {
                        debugCam = Instantiate(new GameObject());
                        debugCam.name = "JaLoader Debug Camera";
                        debugCam.SetActive(false);

                        var effectsCam = Instantiate(new GameObject());
                        effectsCam.name = "JaLoader Menu Post Processing Camera";

                        var inGameEffectsCam = Instantiate(new GameObject());
                        inGameEffectsCam.name = "JaLoader In-Game Post Processing Camera";

                        var normalCam = Instantiate(new GameObject());
                        normalCam.name = "JaLoader Normal Camera";

                        effectsCam.transform.parent = normalCam.transform.parent = inGameEffectsCam.transform.parent = debugCam.transform;

                        var components = Camera.main.GetComponents<MonoBehaviour>();
                        components.RemoveLast();
                        components.RemoveLast();
                        components.RemoveLast();
                        components.RemoveFirst();

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
            if (SettingsManager.Instance.DebugMode)
                Camera.main.gameObject.AddComponent<DebugCamera>();

            if (!addedExtensions)
            {
                Camera.main.gameObject.AddComponent<DragRigidbodyC_ModExtension>();
                Camera.main.gameObject.AddComponent<LaikaCatalogueExtension>();
                player = Camera.main.transform.parent.gameObject;
                laika = GameObject.Find("FrameHolder");
                addedExtensions = true;
            }
        }

        private void OnGameUnload()
        {
            addedExtensions = false;
        }

        public void RefreshPartHolders()
        {
            if(partHolders != null)
                partHolders.Clear();
            else
                partHolders = new Dictionary<PartTypes, Transform>();

            var holder = GameObject.Find("FrameHolder").transform.Find("TweenHolder").Find("Frame").Find("EngineHolders");

            if (holder != null)
            {
                partHolders.Add(PartTypes.Engine, holder.Find("EngineHolder"));
                partHolders.Add(PartTypes.FuelTank, holder.Find("TankHolder"));
                partHolders.Add(PartTypes.Carburettor, holder.Find("Carburettor_Holder"));
                partHolders.Add(PartTypes.AirFilter, holder.Find("airFilterHolder"));
                partHolders.Add(PartTypes.IgnitionCoil, holder.Find("IgnitionHolder"));
                partHolders.Add(PartTypes.Battery, holder.Find("BatteryHolder"));
                partHolders.Add(PartTypes.WaterTank, holder.Find("waterHolding"));
            }
        }

        /// <summary>
        /// Make the specified object able to be used in-game
        /// </summary>
        /// <param name="obj">The object you want to bring to life</param>
        /// <param name="objName">The name that will pop up in the notebook</param>
        /// <param name="objDescription">The description that will pop up in the notebook</param>
        /// <param name="price">The price of the object in stores (only effective if it is buyable)</param>
        /// <param name="weight">The weight of the object</param>
        /// <param name="canFindInCrates">(Not implemented yet) Should this object be findable in crates?</param>
        /// <param name="canBuyInStore">(Not implemented yet) Is this object buyable?</param>
        public void AddBasicObjectLogic(GameObject obj, string objName, string objDescription, int price, int weight, bool canFindInCrates, bool canBuyInStore)
        {
            if(obj == null)
                Console.Instance.LogError("Error");

            if (!obj.GetComponent<Collider>())
            {
                obj.AddComponent<BoxCollider>();
            }

            obj.AddComponent<AudioSource>();
            obj.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);

            obj.tag = "Pickup";
            obj.layer = 24; 
            Console.Instance.Log("11");
            Rigidbody rb = obj.AddComponent<Rigidbody>();
            Console.Instance.Log(rb);
            Console.Instance.Log("11.5");
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            Console.Instance.Log("11.8");
            rb.mass = weight;
            Console.Instance.Log("12");
            ObjectPickupC ob = obj.AddComponent<ObjectPickupC>();
            ob.objectID = 0; Console.Instance.Log("10");
            ob.glowMat = obj.GetComponent<MeshRenderer>().materials;
            ob.glowMaterial = GetGlowMaterial(obj.GetComponent<MeshRenderer>().material);
            ob._audio = defaultClips;

            List<GameObject> objToRender = new List<GameObject>
            {
                obj
            };

            for (int i = 0; i < obj.GetComponentsInChildren<Transform>().Length; i++)
            {
                objToRender.Add(obj.GetComponentsInChildren<Transform>()[i].gameObject);
            }

            ob.renderTargets = objToRender.ToArray();

            ob.buyValue = price;
            ob.sellValue = price;
            ob.flavourText = string.Empty;
            ob.componentHeader = string.Empty;
            ob.rigidMass = weight;

            CustomObjectInfo fix = obj.AddComponent<CustomObjectInfo>();
            fix.objDescription = objDescription;
            fix.objName = objName;
            obj.SetActive(true);
        }

        /// <summary>
        /// Make a custom object be able to be used as an engine part
        /// </summary>
        /// <param name="obj">The object in question</param>
        /// <param name="type">What type of engine component is this?</param>
        /// <param name="companyName">What company made this part? (shown in the laika catalogue)</param>
        /// <param name="price">The price of the object</param>
        /// <param name="durability">Max durability</param>
        /// <param name="canBuyInDealership">Can this object be bought in laika dealerships?</param>
        /// <param name="canFindInJunkCars">(Not implemented yet) Can this object be found at scrapyards/abandoned cars?</param>
        public void AddEnginePartLogic(GameObject obj, PartTypes type, int durability, bool canBuyInDealership, bool canFindInJunkCars)
        {
            if (!obj.GetComponent<ObjectPickupC>())
            {
                Console.Instance.LogError("You need to add basic object logic before adding engine part logic!");
                return;
            }

            ObjectPickupC ob = obj.GetComponent<ObjectPickupC>();
            ob.isEngineComponent = true;

            EngineComponentC ec = obj.AddComponent<EngineComponentC>();
            ec.loadID = 0;
            ec._camera = Camera.main.gameObject;
            ec.weight = ob.rigidMass;
            ec.durability = durability;

            switch (type)
            {
                case PartTypes.Engine:
                    ob.engineString = "EngineBlock";
                    obj.name = "EngineBlock";
                    break;

                case PartTypes.FuelTank:
                    ob.engineString = "FuelTank";
                    obj.name = "FuelTank";
                    break;

                case PartTypes.Carburettor:
                    ob.engineString = "Carburettor";
                    obj.name = "Carburettor";
                    break;

                case PartTypes.AirFilter:
                    ob.engineString = "AirFilter";
                    obj.name = "AirFilter";
                    break;

                case PartTypes.IgnitionCoil:
                    ob.engineString = "IgnitionCoil";
                    obj.name = "IgnitionCoil";
                    break;

                case PartTypes.Battery:
                    ob.engineString = "Battery";
                    ec.isBattery = true;
                    obj.name = "Battery";
                    break;

                case PartTypes.WaterTank:
                    Console.Instance.LogWarning("Water tanks are not fully supported yet!");
                    ob.engineString = "WaterContainer";
                    obj.name = "WaterContainer";
                    obj.AddComponent<ObjectInteractionsC>();
                    obj.GetComponent<ObjectInteractionsC>().targetObjectStringName = "WaterBottle";
                    obj.GetComponent<ObjectInteractionsC>().handInteractive = true;
                    break;

                case PartTypes.Extra:
                    Console.Instance.LogError("Extra components are not supported yet!");
                    //ob.engineString = "";
                    //obj.name = "";
                    break;

                case PartTypes.Custom:
                    Console.Instance.LogError("Custom components are not supported yet!");
                    break;
            }
        }

        /// <summary>
        /// Adjust how the object will sit in the trunk
        /// </summary>
        public void AdjustCustomObjectTrunkPosition(GameObject obj, Vector3 position, Vector3 rotation, Vector3 dimensions)
        {
            ObjectPickupC ob = obj.GetComponent<ObjectPickupC>();

            ob.dimensionX = (int)dimensions.x;
            ob.dimensionY = (int)dimensions.y;
            ob.dimensionZ = (int)dimensions.z;

            ob.inventoryAdjustPosition = position;
            ob.inventoryAdjustRotation = rotation;
        }

        /// <summary>
        /// Adjust how the object looks like when being held
        /// </summary>
        public void AdjustCustomObjectPosition(GameObject obj, Vector3 throwRotation, Vector3 position)
        {
            ObjectPickupC ob = obj.GetComponent<ObjectPickupC>();

            ob.throwRotAdjustment = throwRotation;
            ob.positionAdjust = position;
            ob.setRotation = throwRotation;
        }

        /// <summary>
        /// Create a glowing version of the specified material
        /// </summary>
        public Material GetGlowMaterial(Material originalMaterial)
        {
            Material glowMat = new Material(Shader.Find("Toony Gooch/Toony Gooch RimLight"))
            {
                color = originalMaterial.color,
                mainTexture = originalMaterial.mainTexture,
                mainTextureOffset = originalMaterial.mainTextureOffset,
                mainTextureScale = originalMaterial.mainTextureScale
            };

            return glowMat;
        }

        /// <summary>
        /// Configure a custom engine to your likings
        /// </summary>
        /// <param name="obj">The object in question</param>
        /// <param name="acceleration">The 0-80 speed, in seconds</param>
        /// <param name="topSpeed">The max speed achievable with this engine</param>
        /// <param name="useDefaultAudio">Should it use the default engine audio?</param>
        public void ConfigureCustomEngine(GameObject obj, int acceleration, int topSpeed, bool useDefaultAudio)
        {
            EngineComponentC ec = obj.GetComponent<EngineComponentC>();
            AudioSource audio = obj.GetComponent<AudioSource>();

            ec.acceleration = acceleration;
            ec.topSpeed = topSpeed;

            if (!useDefaultAudio)
                return;

            ec.engineAudio = GameObject.Find("EngineBlock").GetComponent<EngineComponentC>().engineAudio;
            audio.priority = 128;
            audio.pitch = 9.5f;
        }

        /// <summary>
        /// Configure a custom engine to your likings
        /// </summary>
        /// <param name="obj">The object in question</param>
        /// <param name="acceleration">The 0-80 speed, in seconds</param>
        /// <param name="topSpeed">The max speed achievable with this engine</param>
        /// <param name="customAudio">The audio this engine should use</param>
        public void ConfigureCustomEngine(GameObject obj, int acceleration, int topSpeed, AudioClip customAudio, float audioPitch)
        {
            EngineComponentC ec = obj.GetComponent<EngineComponentC>();
            AudioSource audio = obj.GetComponent<AudioSource>();

            ec.acceleration = acceleration;
            ec.topSpeed = topSpeed;

            ec.engineAudio = customAudio;
            audio.priority = 128;
            ec.engineAudioPitch = audioPitch;//9.5f;
        }

        /// <summary>
        /// Configure a custom carburettor to your likings
        /// </summary>
        /// <param name="obj">The object in question</param>
        /// <param name="fuelConsumptionRate">How many liters of fuel per 100km will this consume?</param>
        public void ConfigureCustomCarburettor(GameObject obj, float fuelConsumptionRate)
        {
            EngineComponentC ec = obj.GetComponent<EngineComponentC>();

            ec.fuelConsumptionRate = fuelConsumptionRate;

            AudioSource audio = obj.GetComponent<AudioSource>();
            audio.priority = 128;
        }

        /// <summary>
        /// Configure a custom air filter to your likings
        /// </summary>
        /// <param name="obj">The object in question</param>
        /// <param name="engineWearRate">How </param>
        public void ConfigureCustomAirFilter(GameObject obj, float engineWearRate)
        {
            EngineComponentC ec = obj.GetComponent<EngineComponentC>();

            ec.engineWearRate = engineWearRate;

            AudioSource audio = obj.GetComponent<AudioSource>();
            audio.priority = 128;
        }

        /// <summary>
        /// Configure a custom ignition coil to your likings
        /// </summary>
        /// <param name="obj">The object in question</param>
        /// <param name="initialFuelConsumptionRate">How many liters of fuel will starting the car use?</param>
        /// <param name="ignitionTime">How long does the car take to start?</param>
        public void ConfigureCustomIgnitionCoil(GameObject obj, float initialFuelConsumptionRate, float ignitionTime)
        {
            EngineComponentC ec = obj.GetComponent<EngineComponentC>();

            ec.initialFuelConsumptionAmount = initialFuelConsumptionRate;
            ec.ignitionTimer = ignitionTime;

            AudioSource audio = obj.GetComponent<AudioSource>();
            audio.priority = 128;
        }

        public void ConfigureCustomFuelTank(GameObject obj, int fuelCapacity, int initialFuelCapacity)
        {
            EngineComponentC ec = obj.GetComponent<EngineComponentC>();

            ec.totalFuelAmount = fuelCapacity;
            ec.currentFuelAmount = initialFuelCapacity;

            AudioSource audio = obj.GetComponent<AudioSource>();
            audio.priority = 128;
        }

        public string GetLatestTagFromApiUrl(string URL, string modName)
        {
            UnityWebRequest request = UnityWebRequest.Get(URL);
            request.SetRequestHeader("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)");
            request.SetRequestHeader("Accept", "application/vnd.github.v3+json");

            request.SendWebRequest();

            while (!request.isDone)
            {
                // wait for the request to complete
            }

            if (request.isHttpError)
                return "0";

            string tagName = null;

            if (!request.isNetworkError)
            {
                string json = request.downloadHandler.text;
                Release release = JsonUtility.FromJson<Release>(json);
                tagName = release.tag_name;
            }
            else if (request.isNetworkError)
                return "0";
            else
                Console.Instance.LogError(modName, $"Error getting response for URL \"{URL}\": {request.error}");

            return tagName;
        }

        public string GetLatestTagFromApiUrl(string URL)
        {
            UnityWebRequest request = UnityWebRequest.Get(URL);
            request.SetRequestHeader("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)");
            request.SetRequestHeader("Accept", "application/vnd.github.v3+json");

            request.SendWebRequest();

            while (!request.isDone)
            {
                // wait for the request to complete
            }

            string tagName = null;

            if (!request.isNetworkError && !request.isHttpError)
            {
                string json = request.downloadHandler.text;
                Release release = JsonUtility.FromJson<Release>(json);
                tagName = release.tag_name;
            }
            else if (request.isNetworkError)
                return "0";
            else
                Console.Instance.LogError($"Error getting response for URL \"{URL}\": {request.error}");

            return tagName;
        }

    }

    #region Part Types

    public enum PartTypes
    {
        Engine,
        FuelTank,
        Carburettor,
        AirFilter,
        IgnitionCoil,
        Battery,
        WaterTank,
        Extra,
        Custom
    }

    public enum WheelTypes
    {
        Normal,
        Wet,
        OffRoad,
        Custom
    }

    public enum WheelPositions
    {
        FL,
        FR,
        RL,
        RR
    }

    #endregion

    [Serializable]
    class Release
    {
        public string tag_name = "";
    }
}
