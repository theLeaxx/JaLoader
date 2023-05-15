using JaLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Diagnostics;

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
            SceneManager.sceneLoaded += OnSceneChange;
        }
        #endregion

        public GameObject player;
        public GameObject laika;
        public GameObject pickUpObjectTemplate;
        public GameObject enginePartTemplate;
        public GameObject wheelTemplate;

        public Material defaultEngineMaterial;
        private AudioClip[] defaultClips;

        public Dictionary<PartTypes, Transform> partHolders = new Dictionary<PartTypes, Transform>();

        private bool addedExtensions;

        private void OnEnable()
        {
            GameObject go = GameObject.Find("EngineBlock");

            defaultEngineMaterial = go.GetComponent<MeshRenderer>().material;
            defaultClips = go.GetComponent<ObjectPickupC>()._audio;

            RefreshPartHolders();
        }

        public void OnSceneChange(Scene current, LoadSceneMode mode)
        {
            if (SceneManager.GetActiveScene().buildIndex == 3)
            {
                if (!addedExtensions)
                {
                    Camera.main.gameObject.AddComponent<DragRigidbodyC_ModExtension>();
                    Camera.main.gameObject.AddComponent<LaikaCatalogueExtension>();
                    addedExtensions = true;
                }
            }
            else
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

        public void AddBasicObjectLogic(GameObject obj, string objName, string objDescription, int price, int weight, bool canFindInCrates, bool canBuyInStore)
        {
            if (!obj.GetComponent<Collider>())
            {
                obj.AddComponent<BoxCollider>();
            }

            obj.AddComponent<AudioSource>();
            obj.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);

            obj.tag = "Pickup";
            obj.layer = 24;
            Rigidbody rb = obj.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.mass = weight;
            ObjectPickupC ob = obj.AddComponent<ObjectPickupC>();
            ob.objectID = 0;
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

            FixTextOnObjectPickup fix = obj.AddComponent<FixTextOnObjectPickup>();
            fix.objDescription = objDescription;
            fix.objName = objName;
            obj.SetActive(true);
        }

        /// <summary>
        /// Add the scripts required to make a GameObject work as an engine part.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="objName"></param>
        /// <param name="objDescription"></param>
        /// <param name="price"></param>
        /// <param name="weight"></param>
        /// <param name="durability"></param>
        /// <param name="condition"></param>
        /// <param name="canBuyInDealership"></param>
        /// <param name="canFindInJunkCars"></param>
        public void AddEnginePartLogic(GameObject obj, PartTypes type, string companyName, int price, int durability, int condition, bool canBuyInDealership, bool canFindInJunkCars)
        {
            ObjectPickupC ob = obj.GetComponent<ObjectPickupC>();
            ob.isEngineComponent = true;

            EngineComponentC ec = obj.AddComponent<EngineComponentC>();
            ec.loadID = 0;
            ec._camera = Camera.main.gameObject;
            ec.weight = ob.rigidMass;
            ec.durability = durability;
            ec.Condition = condition;
            ec.uncle = UncleHelper.Instance.Uncle;

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
                    obj.name = "Battery";
                    break;

                case PartTypes.WaterTank:
                    ob.engineString = "WaterContainer";
                    obj.name = "WaterContainer";
                    break;

                case PartTypes.Extra:
                    ob.engineString = "Carburettor";
                    obj.name = "Carburettor";
                    break;

                case PartTypes.Custom:
                    break;
            }
        }

        public void AdjustCustomObjectTrunkPosition(GameObject obj, Vector3 position, Vector3 rotation, Vector3 dimensions)
        {
            ObjectPickupC ob = obj.GetComponent<ObjectPickupC>();

            ob.dimensionX = (int)dimensions.x;//4;
            ob.dimensionY = (int)dimensions.y;//2;
            ob.dimensionZ = (int)dimensions.z;//3;

            ob.inventoryAdjustPosition = position;//new Vector3(0.1f, 0.2f, 0.1f);
            ob.inventoryAdjustRotation = rotation;//new Vector3(43.2f, -64.1f, 155.2f);
        }

        public void AdjustCustomObjectPosition(GameObject obj, Vector3 throwRotation, Vector3 position)
        {
            ObjectPickupC ob = obj.GetComponent<ObjectPickupC>();

            ob.throwRotAdjustment = throwRotation;//new Vector3(0, -180, 0);
            ob.positionAdjust = position;//new Vector3(0, -0.3f, 0);
            ob.setRotation = throwRotation;//new Vector3(0, -180, 0);
        }

        public Material GetGlowMaterial(Material originalMaterial)
        {
            Material glowMat = new Material(Shader.Find("Toony Gooch/Toony Gooch RimLight"));
            glowMat.color = originalMaterial.color;
            glowMat.mainTexture = originalMaterial.mainTexture;
            glowMat.mainTextureOffset = originalMaterial.mainTextureOffset;
            glowMat.mainTextureScale = originalMaterial.mainTextureScale;

            return glowMat;
        }

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

        public void ConfigureCustomCarburettor(GameObject obj, int fuelConsumptionRate)
        {
            EngineComponentC ec = obj.GetComponent<EngineComponentC>();

            ec.fuelConsumptionRate = fuelConsumptionRate;

            AudioSource audio = obj.GetComponent<AudioSource>();
            audio.priority = 128;
        }

        public void ConfigureCustomEngine(GameObject obj, int acceleration, int topSpeed, AudioClip customAudio)
        {
            EngineComponentC ec = obj.GetComponent<EngineComponentC>();
            AudioSource audio = obj.GetComponent<AudioSource>();

            ec.acceleration = acceleration;
            ec.topSpeed = topSpeed;

            ec.engineAudio = customAudio;
            audio.priority = 128;
            audio.pitch = 9.5f;
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

    #endregion
}
