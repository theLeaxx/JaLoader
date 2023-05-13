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
    public class ModReferences : MonoBehaviour
    {
        public static ModReferences Instance { get; private set; }

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

        private bool addedDRCExtension;

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
                if (!addedDRCExtension)
                {
                    Camera.main.gameObject.AddComponent<DragRigidbodyC_ModExtension>();
                    addedDRCExtension = true;
                }
            }
            else
                addedDRCExtension = false;
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
        /// Add the scripts required to make a GameObject work as an engine part.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="objName"></param>
        /// <param name="objDescription"></param>
        /// <param name="price"></param>
        /// <param name="weight"></param>
        /// <param name="durability"></param>
        /// <param name="condition"></param>
        /// <param name="topSpeed"></param>
        /// <param name="acceleration"></param>
        /// <param name="canBuyInDealership"></param>
        /// <param name="canFindInJunkCars"></param>
        public void AddEnginePartLogic(GameObject obj, PartTypes type, string objName, string objDescription, string companyName, int price, int weight, int durability, int condition, int topSpeed, int acceleration, bool canBuyInDealership, bool canFindInJunkCars)
        {
            if (!obj.GetComponent<Collider>())
            {
                obj.AddComponent<BoxCollider>();
            }

            obj.AddComponent<AudioSource>();
            obj.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);

            Material mat = obj.GetComponent<MeshRenderer>().material;

            Material glowMat = new Material(Shader.Find("Toony Gooch/Toony Gooch RimLight"));
            glowMat.color = mat.color;
            glowMat.mainTexture = mat.mainTexture;
            glowMat.mainTextureOffset = mat.mainTextureOffset;
            glowMat.mainTextureScale = mat.mainTextureScale;

            obj.tag = "Pickup";
            obj.layer = 24;
            Rigidbody rb = obj.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.mass = weight;
            ObjectPickupC ob = obj.AddComponent<ObjectPickupC>();
            ob.objectID = 0;
            ob.glowMat = obj.GetComponent<MeshRenderer>().materials;
            ob.glowMaterial = glowMat;
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
            ob.dimensionX = 4;
            ob.dimensionY = 2;
            ob.dimensionZ = 3;

            ob.inventoryAdjustPosition = new Vector3(0.1f, 0.2f, 0.1f);
            ob.inventoryAdjustRotation = new Vector3(43.2f, -64.1f, 155.2f);
            ob.throwRotAdjustment = new Vector3(0, -180, 0);
            ob.positionAdjust = new Vector3(0, -0.3f, 0);
            ob.setRotation = new Vector3(0, -180, 0);

            ob.isEngineComponent = true;
            EngineComponentC ec = obj.AddComponent<EngineComponentC>();
            ec.loadID = 0;
            ec._camera = Camera.main.gameObject;
            ec.weight = weight;
            ec.durability = durability;
            ec.Condition = condition;
            ec.uncle = GameObject.Find("Uncle");

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

            FixTextOnObjectPickup fix = obj.AddComponent<FixTextOnObjectPickup>();
            fix.objDescription = objDescription;
            fix.objName = objName;
            obj.SetActive(true);
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

        /// <summary>
        /// Instantiate a GameObject that can be picked up.
        /// </summary>
        /// <param name="position">Where the part should spawn (use Vector3.zero if you want it to be buyable)</param>
        /// <param name="rotation">The rotation of the part (use Quaternion.identity if you want it to be buyable)</param>
        /// <param name="gameObjectName">The object name in the scene</param>
        /// <param name="objName">The object's name</param>
        /// <param name="objDescription">The object's description</param>
        /// <param name="price">The object's price</param>
        public void InstantiateNewPickUpAblePart(GameObject gameObject, Vector3 position, Quaternion rotation, Material mat,string gameObjectName, string objName, string objDescription, int price, int weight) //string companyName, string companyDescription, EngineTypes type, int durability, int weight, int topSpeed, int acceleration)
        {
            GameObject go = Instantiate(gameObject);
            go.SetActive(false);

            if (!go.GetComponent<Collider>())
            {
                go.AddComponent<BoxCollider>();
            }

            go.AddComponent<AudioSource>();
            go.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);

            Material glowMat = new Material(Shader.Find("Toony Gooch/Toony Gooch RimLight"));
            glowMat.color = mat.color;
            glowMat.mainTexture = mat.mainTexture;
            glowMat.mainTextureOffset = mat.mainTextureOffset;
            glowMat.mainTextureScale = mat.mainTextureScale;

            go.GetComponent<MeshRenderer>().material = mat;
            go.transform.position = position;
            go.transform.rotation = rotation;
            go.name = gameObjectName;
            go.tag = "Pickup";
            go.layer = 24;
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.mass = weight;
            ObjectPickupC ob = go.AddComponent<ObjectPickupC>();
            ob.glowMat = go.GetComponent<MeshRenderer>().materials;
            ob.glowMaterial = glowMat;
            ob._audio = defaultClips;

            List<GameObject> objToRender = new List<GameObject>
            {
                go
            };
            for (int i = 0; i < go.GetComponentsInChildren<Transform>().Length; i++)
            {
                objToRender.Add(go.GetComponentsInChildren<Transform>()[i].gameObject);
            }

            ob.renderTargets = objToRender.ToArray();
            //ob.dropOffPoints = referenceOP.dropOffPoints;
            //ob.adjustScale = referenceOP.adjustScale;
            //ob.positionAdjust = referenceOP.positionAdjust;
            //ob.adjustScale = new Vector3(1, 1, 1);
            ob.buyValue = price;
            ob.sellValue = price;
            ob.flavourText = string.Empty;
            ob.componentHeader = string.Empty;
            ob.rigidMass = weight;
            ob.dimensionX = 4;
            ob.dimensionY = 2;
            ob.dimensionZ = 3;

            ob.inventoryAdjustPosition = new Vector3(0.1f, 0.2f, 0.1f);
            ob.inventoryAdjustRotation = new Vector3(43.2f, -64.1f, 155.2f);
            ob.throwRotAdjustment = new Vector3(0, -180, 0);
            ob.positionAdjust = new Vector3(0, -0.3f, 0);
            ob.setRotation = new Vector3(0, -180, 0);

            FixTextOnObjectPickup fix = go.AddComponent<FixTextOnObjectPickup>();
            fix.objDescription = objDescription;
            fix.objName = objName;
            go.SetActive(true);
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
