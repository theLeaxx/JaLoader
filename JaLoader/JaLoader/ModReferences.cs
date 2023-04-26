using JaLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace JaLoader
{
    public class ModReferences : MonoBehaviour
    {
        public static ModReferences Instance { get; private set; }

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
        }

        public GameObject player;
        public GameObject laika;
        public GameObject pickUpObjectTemplate;
        public GameObject enginePartTemplate;
        public GameObject wheelTemplate;

        bool addedObjFix;

        private void Update()
        {
            if (SceneManager.GetActiveScene().buildIndex > 2)
            {
                if (!addedObjFix)
                {
                    Camera.main.gameObject.AddComponent<DragRigidbodyC_ModExtension>();
                    addedObjFix = true;
                }
            }
            else
                addedObjFix = false;
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
        public void AddEnginePartLogic(GameObject obj, EnginePartTypes type, string objName, string objDescription, string companyName, int price, int weight, int durability, int condition, int topSpeed, int acceleration, bool canBuyInDealership, bool canFindInJunkCars)
        {
            if (!obj.GetComponent<Collider>())
            {
                obj.AddComponent<BoxCollider>();
            }

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
            ob.glowMat = obj.GetComponent<MeshRenderer>().materials;
            ob.glowMaterial = glowMat;
          
            List<GameObject> objToRender = new List<GameObject>();
            objToRender.Add(obj);
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
            ec._camera = Camera.main.gameObject;
            ec.weight = weight;
            ec.durability = durability;
            ec.condition = condition;
            //ec.loadID = 1;
            ec.uncle = GameObject.Find("Uncle");

            if (type == EnginePartTypes.Engine)
            {
                ob.engineString = "EngineBlock";

                ec.acceleration = acceleration;
                ec.topSpeed = topSpeed;
                ec.engineAudio = GameObject.Find("EngineBlock").GetComponent<EngineComponentC>().engineAudio;
                obj.name = "EngineBlock";

                AudioSource audio = obj.AddComponent<AudioSource>();
                audio.priority = 128;
                audio.pitch = 9.5f;
            }

            if (type == EnginePartTypes.Carburettor)
            {
                ob.engineString = "Carburettor";
                obj.name = "Carburettor";
                ec.fuelConsumptionRate = 5;
                AudioSource audio = obj.AddComponent<AudioSource>();
                audio.priority = 128;
                audio.pitch = 1f;
            }

            FixTextOnObjectPickup fix = obj.AddComponent<FixTextOnObjectPickup>();
            fix.objDescription = objDescription;
            fix.objName = objName;
            obj.SetActive(true);

            //ObjectIDManager.Instance.RegisterEngineComponent(obj, type);
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

            go.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);

            //Material mat = new Material(Shader.Find("Specular"));
            //mat.color = new Color32(128, 128, 128, 255);

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
            //go.AddComponent<ObjectInteractionsC>();
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.mass = weight;
            ObjectPickupC ob = go.AddComponent<ObjectPickupC>();
            ob.glowMat = go.GetComponent<MeshRenderer>().materials;
            ob.glowMaterial = glowMat;

            List<GameObject> objToRender = new List<GameObject>();
            objToRender.Add(go);
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

            ob.engineString = "EngineBlock";
            ob.isEngineComponent = true;

            EngineComponentC ec = go.AddComponent<EngineComponentC>();
            ec._camera = Camera.main.gameObject;
            ec.weight = weight;
            ec.durability = 4;
            ec.condition = 4;
            ec.acceleration = 4;
            ec.topSpeed = 150;
            ec.loadID = 1;
            ec.uncle = GameObject.Find("Uncle");
            ec.engineAudio = GameObject.Find("EngineBlock").GetComponent<EngineComponentC>().engineAudio;

            AudioSource audio = go.AddComponent<AudioSource>();
            audio.priority = 128;
            audio.pitch = 9.5f;

            FixTextOnObjectPickup fix = go.AddComponent<FixTextOnObjectPickup>();
            fix.objDescription = objDescription;
            fix.objName = objName;
            go.SetActive(true);
        }
    }

    #region Part Types

    public enum EnginePartTypes
    {
        Engine,
        FuelTank,
        Carburettor,
        AirFilter,
        IgnitionCoil,
        Battery,
        WaterTank,
        Extra
    }

    public enum WheelTypes
    {
        Normal,
        Wet,
        OffRoad
    }

    public enum EngineTypes
    {
        Stock,
        Naked,
        Ramshackle,
        Squash
    }

    #endregion
}
