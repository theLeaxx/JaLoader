using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Xml.Linq;
using System.Data.SqlTypes;
using System.Linq;
using System.Collections;

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
        public WalletC wallet;
        public DirectorC director;

        public Material defaultEngineMaterial;
        private AudioClip[] defaultClips;

        public Dictionary<PartTypes, Transform> partHolders = new Dictionary<PartTypes, Transform>();
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

        public GameObject carLeftDoor;
        public GameObject carRightDoor;
        public GameObject carHood;
        public GameObject carTrunk;
        public GameObject carRoof;
        public GameObject carFrame;

        public bool patchedEverything = false;

        private Material defaultGlowMaterial = new Material(Shader.Find("Legacy Shaders/Transparent/Specular"))
        {
            color = new Color(1, 1, 1, 0.17f)
        };

        private readonly List<(GameObject, string, string, int, int)> boxesToCreateInGame = new List<(GameObject, string, string, int, int)>();

        private void OnMenuLoad()
        {
            RefreshPartHolders();

            GetAllBodyParts();
            laika.AddComponent<LicensePlateCustomizer>();

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

        private void GetAllBodyParts()
        {
            laika = GameObject.Find("FrameHolder");
            carFrame = laika.transform.Find("TweenHolder").Find("Frame").gameObject;

            carLeftDoor = carFrame.transform.Find("L_Door").gameObject;
            carHood = carFrame.transform.Find("Bonnet").gameObject;
            carTrunk = carFrame.transform.Find("Boot").gameObject;
            carRoof = carFrame.transform.Find("Roof").gameObject;

            StartCoroutine(GetRightDoor());
        }

        private IEnumerator GetRightDoor()
        {
            yield return new WaitForSeconds(3);

            carRightDoor = carFrame.transform.Find("DoorHolder/R_Door").gameObject;
        }

        private void OnGameLoad()
        {
            if (SettingsManager.Instance.DebugMode)
                Camera.main.gameObject.AddComponent<DebugCamera>();

            RefreshPartHolders();

            if (!addedExtensions)
            {
                Camera.main.gameObject.AddComponent<DragRigidbodyC_ModExtension>();
                player = Camera.main.transform.parent.gameObject;
                GetAllBodyParts();
                wallet = FindObjectOfType<WalletC>();
                director = FindObjectOfType<DirectorC>();
                laika.AddComponent<LicensePlateCustomizer>();
                addedExtensions = true;

                RouteGeneratorC route = FindObjectOfType<RouteGeneratorC>();
                CardboardBoxBig = route.cratePrefabs[0];
                CardboardBoxMed = route.cratePrefabs[1];
                CardboardBoxSmall = route.cratePrefabs[2];
                CrateBig = route.cratePrefabs[3];
                CrateMed = route.cratePrefabs[4];
                CrateSmall = route.cratePrefabs[5];

                OverwriteBoxObjects();

                Camera.main.gameObject.AddComponent<LaikaCatalogueExtension>();

                if (SettingsManager.IsPreReleaseVersion)
                {
                    var obj = Instantiate(new GameObject());
                    obj.name = "JaLoader Game Scripts";
                    obj.AddComponent<MarketManager>();
                }
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
            if (obj == null)
            {
                Console.LogError("The object you're trying to add logic to is null!");
                return;
            }

            obj.SetActive(false);

            if (obj.GetComponent<ObjectIdentification>() && obj.GetComponent<ObjectIdentification>().HasReceivedBasicLogic)
                return;

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

            for (int i = 0; i < obj.GetComponentsInChildren<Renderer>().Length; i++)
            {
                objToRender.Add(obj.GetComponentsInChildren<Renderer>()[i].gameObject);
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
            obj.GetComponent<ObjectIdentification>().HasReceivedBasicLogic = true;
        }

        private void AddBoxLogic(GameObject obj, string objName, string objDescription, int price, int weight)
        {
            if (obj == null)
            {
                Console.LogError("The object you're trying to add logic to is null!");
                return;
            }

            obj.SetActive(false);

            if (obj.GetComponent<ObjectIdentification>() && obj.GetComponent<ObjectIdentification>().HasReceivedBasicLogic)
                return;

            if (!obj.GetComponent<Collider>())
            {
                obj.AddComponent<BoxCollider>();
            }

            obj.AddComponent<AudioSource>();
            //obj.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);

            obj.tag = "Pickup";
            obj.layer = 0;
            Rigidbody rb = obj.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.mass = weight;
            ObjectPickupC ob = obj.AddComponent<ObjectPickupC>();
            ob.objectID = 24;
            ob._audio = defaultClips;

            ob.buyValue = price;
            ob.sellValue = price;
            ob.flavourText = string.Empty;
            ob.componentHeader = string.Empty;
            ob.rigidMass = weight;

            CustomObjectInfo fix = obj.AddComponent<CustomObjectInfo>();
            fix.objDescription = objDescription;
            fix.objName = objName;
            obj.GetComponent<ObjectIdentification>().HasReceivedBasicLogic = true;
        }

        private void ConvertToBox(GameObject obj, BoxSizes size, GameObject box)
        {
            ObjectPickupC ob = obj.GetComponent<ObjectPickupC>();
            ob.objectID = 0;

            ob.renderTargets = box.GetComponent<ObjectPickupC>().renderTargets;
            
            ob.glowMat = ob.renderTargets[0].GetComponent<Renderer>().materials;
            ob.glowMaterial = GetGlowMaterial(ob.renderTargets[0].GetComponent<Renderer>().material);
            ob._audio = defaultClips;

            ob.adjustScale = box.GetComponent<ObjectPickupC>().adjustScale;
            ob.positionAdjust = box.GetComponent<ObjectPickupC>().positionAdjust;
            ob.setRotation = box.GetComponent<ObjectPickupC>().setRotation;
            ob.inventoryAdjustPosition = box.GetComponent<ObjectPickupC>().inventoryAdjustPosition;
            ob.inventoryAdjustRotation = box.GetComponent<ObjectPickupC>().inventoryAdjustRotation;
            ob.dimensionX = box.GetComponent<ObjectPickupC>().dimensionX;
            ob.dimensionY = box.GetComponent<ObjectPickupC>().dimensionY;
            ob.dimensionZ = box.GetComponent<ObjectPickupC>().dimensionZ;
        }

        public void AddWheelLogic(GameObject obj, int durability, float roadGrip, float wetGrip, float offRoadGrip, int tyreType, int compoundType, GameObject bolt)
        {
            if (obj == null)
            {
                Console.LogError("The object you're trying to add logic to is null!");
                return;
            }

            if (obj.GetComponent<ObjectIdentification>() && obj.GetComponent<ObjectIdentification>().HasReceivedPartLogic)
                return;

            if (!obj.GetComponent<ObjectPickupC>())
            {
                Console.LogError("You need to add basic object logic before adding engine part logic!");
                return;
            }

            ObjectPickupC ob = obj.GetComponent<ObjectPickupC>();
            ob.isEngineComponent = true;

            EngineComponentC ec = obj.AddComponent<EngineComponentC>();
            ec.loadID = 0;
            ec._camera = Camera.main.gameObject;
            ec.weight = ob.rigidMass;
            ec.durability = durability;
            ec.carLogic = FindObjectOfType<CarLogicC>().gameObject;

            var identif = obj.GetComponent<ObjectIdentification>();
            identif.PartIconScaleAdjustment = obj.transform.localScale;

            ec.bolt = new GameObject();
            ec.roadGrip = roadGrip;
            ec.wetGrip = wetGrip;
            ec.offRoadGrip = offRoadGrip;
            ec.tyreType = tyreType;
            ec.blowOutAudio = defaultClips[0];
            ec.wheelDirtTarget = new GameObject();
            ec.rubberMesh = new GameObject();
            //ec.rubberLibrary = new GameObject();
            ec.compoundType = compoundType;
            ec.compoundTarget = new GameObject();
            ec.dirtTarget = new GameObject();
            //ec.dirtLibrary = new GameObject();
            ec.sprites = new Sprite[0];
            ec.spriteTarget = new GameObject();

            var template = GameObject.Find("EngineBlock");
            var template_ec = template.GetComponent<EngineComponentC>();
            var toCopy = template.transform.Find("DamageSprite");

            ec.sprites = template_ec.sprites;
            var clone = Instantiate(toCopy, obj.transform);
            ec.spriteTarget = clone.gameObject;

            obj.GetComponent<ObjectIdentification>().HasReceivedPartLogic = true;
        }

        /// <summary>
        /// Make a custom object be able to be used as an engine part
        /// </summary>
        /// <param name="obj">The object in question</param>
        /// <param name="type">What type of engine component is this?</param>
        /// <param name="durability">Max durability</param>
        /// <param name="canBuyInDealership">Can this object be bought in laika dealerships?</param>
        /// <param name="canFindInJunkCars">(Not implemented yet) Can this object be found at scrapyards/abandoned cars?</param>
        public void AddEnginePartLogic(GameObject obj, PartTypes type, int durability, bool canBuyInDealership, bool canFindInJunkCars)
        {
            if(type == PartTypes.Wheel)
            {
                Console.LogError("To add wheels, use 'AddWheelLogic'!");
                return;
            }

            if (obj == null)
            {
                Console.LogError("The object you're trying to add logic to is null!");
                return;
            }

            if (obj.GetComponent<ObjectIdentification>() && obj.GetComponent<ObjectIdentification>().HasReceivedPartLogic)
                return;

            if (!obj.GetComponent<ObjectPickupC>())
            {
                Console.LogError("You need to add basic object logic before adding engine part logic!");
                return;
            }

            ObjectPickupC ob = obj.GetComponent<ObjectPickupC>();
            ob.isEngineComponent = true;

            EngineComponentC ec = obj.AddComponent<EngineComponentC>();
            ec.loadID = 0;
            ec._camera = Camera.main.gameObject;
            ec.weight = ob.rigidMass;
            ec.durability = durability;

            var identif = obj.GetComponent<ObjectIdentification>();
            identif.PartIconScaleAdjustment = obj.transform.localScale;

            var template = GameObject.Find("EngineBlock");
            var template_ec = template.GetComponent<EngineComponentC>();
            var toCopy = template.transform.Find("DamageSprite");

            ec.sprites = template_ec.sprites;
            var clone = Instantiate(toCopy, obj.transform);
            ec.spriteTarget = clone.gameObject;

            switch (type)
            {
                case PartTypes.Engine:
                    ob.engineString = "EngineBlock";
                    obj.name = "EngineBlock";
                    identif.PartIconRotationAdjustment = new Vector3(-40, -190, 0);
                    break;

                case PartTypes.FuelTank:
                    Console.LogWarning("Water tanks are not fully supported yet!");
                    ob.engineString = "FuelTank";
                    obj.name = "FuelTank";
                    identif.PartIconPositionAdjustment = new Vector3(0.5f, -0.05f, 0);
                    identif.PartIconRotationAdjustment = new Vector3(-140, -4, -210);;
                    break;

                case PartTypes.Carburettor:
                    ob.engineString = "Carburettor";
                    obj.name = "Carburettor";
                    identif.PartIconPositionAdjustment = new Vector3(-0.5f, 1f, 0);
                    identif.PartIconRotationAdjustment = new Vector3(-40, 170, 25);
                    break;

                case PartTypes.AirFilter:
                    ob.engineString = "AirFilter";
                    obj.name = "AirFilter";
                    identif.PartIconPositionAdjustment = new Vector3(-0.35f, 0f, 0);
                    identif.PartIconRotationAdjustment = new Vector3(240, 12, 210);
                    break;

                case PartTypes.IgnitionCoil:
                    ob.engineString = "IgnitionCoil";
                    obj.name = "IgnitionCoil";
                    identif.PartIconPositionAdjustment = new Vector3(-1.1f, -0.8f, 0);
                    identif.PartIconRotationAdjustment = new Vector3(-145, -3, -48);
                    break;

                case PartTypes.Battery:
                    ob.engineString = "Battery";
                    ec.isBattery = true;
                    obj.name = "Battery";
                    identif.PartIconPositionAdjustment = new Vector3(0, 0.1f, 0);
                    identif.PartIconRotationAdjustment = new Vector3(-75, -190, 330);
                    break;

                case PartTypes.WaterTank:
                    Console.LogWarning("Water tanks are not fully supported yet!");
                    ob.engineString = "WaterContainer";
                    obj.name = "WaterContainer";
                    obj.AddComponent<ObjectInteractionsC>();
                    obj.GetComponent<ObjectInteractionsC>().targetObjectStringName = "WaterBottle";
                    obj.GetComponent<ObjectInteractionsC>().handInteractive = true;
                    break;

                case PartTypes.Extra:
                    ob.engineString = "";
                    ob.isEngineComponent = false;
                    var interactions = obj.AddComponent<ObjectInteractionsC>();
                    interactions.handInteractive = true;
                    interactions.targetObjectStringName = obj.name;
                    obj.AddComponent<ExtraComponentC_ModExtension>();
                    obj.AddComponent<ExtraInformation>();
                    break;

                case PartTypes.Custom:
                    ob.engineString = "";
                    ob.isEngineComponent = false;
                    Console.LogError("Custom components are not supported yet!");
                    break;
            }

            obj.GetComponent<ObjectIdentification>().HasReceivedPartLogic = true;
        }

        /// <summary>
        /// Create a part icon for an extra part
        /// </summary>
        /// <param name="objOnCar">The object that will appear on the car, this should have the desired positions set already</param>
        /// <param name="position">The position differences</param>
        /// <param name="scale">The scale differences</param>
        /// <param name="rotation">The rotation differences</param>
        /// <param name="registryName">Internal extra name</param>
        public void CreateIconForExtra(GameObject objOnCar, Vector3 position, Vector3 scale, Vector3 rotation, string registryName)
        {
            if (CustomObjectsManager.Instance.ignoreAlreadyExists)
                return;

            var duplicate = Instantiate(objOnCar);
            duplicate.name = $"{duplicate.name}_DUPLICATE";
            duplicate.SetActive(false);
            duplicate.transform.position = Vector3.zero;

            ObjectIdentification identif = duplicate.GetComponent<ObjectIdentification>();
            identif.PartIconPositionAdjustment = position;
            identif.PartIconRotationAdjustment = rotation;
            identif.PartIconScaleAdjustment = scale;

            DontDestroyOnLoad(duplicate);
            PartIconManager.Instance.extraParts.Add(registryName, duplicate);
        }

        /// <summary>
        /// Create a custom extra object, that can be attached to the car
        /// </summary>
        /// <param name="objOnCar">The object that will appear on the car, this should have the desired positions set already</param>
        /// <param name="size">The size of the box</param>
        /// <param name="name">The name of the extra</param>
        /// <param name="description">The description of the extra</param>
        /// <param name="price">The price of the extra</param>
        /// <param name="weight">How much weight does this add to the car</param>
        /// <param name="registryName">Internal extra name</param>
        /// <param name="attachTo">What should the object attach to?</param>
        /// <returns></returns>
        public GameObject CreateExtraObject(GameObject objOnCar, BoxSizes size, string name, string description, int price, int weight, string registryName, AttachExtraTo attachTo)
        {
            return CreateExtraObject(objOnCar, size, name, description, price, weight, registryName, attachTo, null);
        }

        /// <summary>
        /// Create a custom extra object, that can be attached to the car
        /// </summary>
        /// <param name="objOnCar">The object that will appear on the car, this should have the desired positions set already</param>
        /// <param name="size">The size of the box</param>
        /// <param name="name">The name of the extra</param>
        /// <param name="description">The description of the extra</param>
        /// <param name="price">The price of the extra</param>
        /// <param name="weight">How much weight does this add to the car</param>
        /// <param name="registryName">Internal extra name</param>
        /// <param name="attachTo">What should the object attach to?</param>
        /// <param name="blockedBy">Parts that may block the installation of this extra part, or replace it (registryName, completely block (true) or replace current part (false))</param>
        /// <returns></returns>
        public GameObject CreateExtraObject(GameObject objOnCar, BoxSizes size, string name, string description, int price, int weight, string registryName, AttachExtraTo attachTo, Dictionary<string, bool> blockedBy = null)
        {
            blockedBy = blockedBy ?? new Dictionary<string, bool>();

            if(ExtrasManager.Instance.ExtraExists(registryName))
            {
                if (!CustomObjectsManager.Instance.ignoreAlreadyExists)
                {
                    Console.LogError($"An extra with the registry name {registryName} already exFists!");
                }
                else
                {
                    objOnCar.SetActive(false);
                }
                return null;
            }

            var identif = objOnCar.GetComponent<ObjectIdentification>();
            var modID = identif.ModID;
            var author = identif.Author;
            var modName = identif.ModName;
            var version = identif.Version;

            var extraHolder = Instantiate(new GameObject());
            extraHolder.name = $"Extra_{objOnCar.name.Substring(0, objOnCar.name.Length - 7)}_{identif.ModID}_{identif.Author}";
            extraHolder.SetActive(false);
            extraHolder.tag = "Interactor";
            //extraHolder.transform.parent = ExtrasManager.Instance.ExtrasHolder.transform;
            extraHolder.transform.position = new Vector3(0, -3, 0);
            extraHolder.AddComponent<BoxCollider>();
            extraHolder.GetComponent<BoxCollider>().isTrigger = true;
            extraHolder.AddComponent<BoxCollider>().enabled = false;
            var receiver = extraHolder.AddComponent<ExtraReceiverC>();
            var info = extraHolder.AddComponent<HolderInformation>();
            info.Weight = weight;

            objOnCar.transform.SetParent(extraHolder.transform, false);
            objOnCar.name = "Mesh";
            objOnCar.SetActive(false);

            var meshesHolder = Instantiate(new GameObject());
            meshesHolder.name = "MeshReceiver";
            meshesHolder.transform.SetParent(extraHolder.transform, true);
            meshesHolder.transform.localPosition = Vector3.zero;

            GameObject meshes = Instantiate(objOnCar);
            meshes.transform.SetParent(meshesHolder.transform, true);
            var renderer = meshes.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material[] mats = renderer.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = defaultGlowMaterial;
                }
                renderer.materials = mats;
                renderer.enabled = false;
            }
            Flatten(meshes.transform, meshesHolder.transform);

            meshes.SetActive(true);
            RemoveAllComponents(meshes, typeof(MeshRenderer), typeof(MeshFilter));

            if (!meshes.GetComponent<MeshRenderer>())
            {
                DestroyImmediate(meshes);
            }

            List<GameObject> children = new List<GameObject>();

            foreach (Transform child in meshesHolder.transform)
            {
                children.Add(child.gameObject);
                child.transform.position = new Vector3(child.transform.position.x, child.transform.position.y - 3f, child.transform.position.z);
            }

            receiver.glowMesh = children.ToArray();
            receiver.extraComponent = objOnCar;

            var colliderObj = new GameObject();
            colliderObj.name = extraHolder.name;
            colliderObj.transform.SetParent(extraHolder.transform, false);
            colliderObj.transform.localPosition = Vector3.zero;
            var collider = colliderObj.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = new Vector3(1, 0, 0);
            collider.size = new Vector3(15, 5, 7);
            collider.tag = "Interactor";
            var interactions = colliderObj.AddComponent<ObjectInteractionsC>();
            interactions.targetObjectStringName = extraHolder.name;
            interactions.handInteractive = false;
            colliderObj.SetActive(false);

            var copy = Instantiate(meshesHolder, meshesHolder.transform.parent);
            copy.name = "MeshReceiverClone";
            copy.SetActive(false);
            copy.transform.position = meshesHolder.transform.position;
            copy.transform.rotation = meshesHolder.transform.rotation;
            copy.transform.localScale = meshesHolder.transform.localScale;

            DontDestroyOnLoad(extraHolder);
            ExtrasManager.Instance.AddExtraObject(extraHolder, extraHolder.transform.localPosition, registryName, attachTo, blockedBy);

            var boxObject = Instantiate(new GameObject());
            boxObject.name = extraHolder.name;
            boxObject.SetActive(false);
            var boxIdentif = boxObject.AddComponent<ObjectIdentification>();
            boxIdentif.ModID = modID;
            boxIdentif.Author = author;
            boxIdentif.ModName = modName;
            boxIdentif.Version = version;
            boxIdentif.IsExtra = true;
            boxIdentif.BoxSize = size;
            boxIdentif.ExtraID = ExtrasManager.Instance.GetExtraID(extraHolder.name);

            AddBoxLogic(boxObject, name, description, price, weight);
            AddEnginePartLogic(boxObject, PartTypes.Extra, 3, true, false);

            DontDestroyOnLoad(boxObject);
            boxesToCreateInGame.Add((boxObject, name, description, price, weight));
 
            return boxObject;
        }

        /// <summary>
        /// Adjust a custom part's location, rotation and scale for the part icon
        /// </summary>
        /// <param name="obj">The object in question</param>
        /// <param name="position">The position differences</param>
        /// <param name="rotation">The rotation differences</param>
        public void AdjustPartIconLocation(GameObject obj, Vector3 position, Vector3 rotation)
        {
            ObjectIdentification identif = obj.GetComponent<ObjectIdentification>();
            identif.PartIconPositionAdjustment = position;
            identif.PartIconRotationAdjustment = rotation;
        }

        /// <summary>
        /// Adjust a custom part's location, rotation and scale for the part icon
        /// </summary>
        /// <param name="obj">The object in question</param>
        /// <param name="position">The position differences</param>
        /// <param name="rotation">The rotation differences</param>
        /// <param name="scale">The scale differences</param>
        public void AdjustPartIconLocation(GameObject obj, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            ObjectIdentification identif = obj.GetComponent<ObjectIdentification>();
            identif.PartIconPositionAdjustment = position;
            identif.PartIconRotationAdjustment = rotation;
            identif.PartIconScaleAdjustment = scale;
        }

        private void OverwriteBoxObjects()
        {
            foreach (var pair in boxesToCreateInGame)
            {
                var originalObject = pair.Item1;

                var obj = pair.Item1;

                var size = obj.GetComponent<ObjectIdentification>().BoxSize;

                GameObject box = null;

                switch (size)
                {
                    case BoxSizes.Small:
                        box = Instantiate(CardboardBoxSmall);
                        break;

                    case BoxSizes.Medium:
                        box = Instantiate(CardboardBoxMed);
                        break;

                    case BoxSizes.Big:
                        box = Instantiate(CardboardBoxBig);
                        break;
                }

                var boxCol = obj.GetComponent<BoxCollider>();
                box.transform.SetParent(obj.transform, false);
                box.transform.position = Vector3.zero;
                box.transform.eulerAngles = Vector3.zero;
                boxCol.center = box.GetComponent<BoxCollider>().center;
                boxCol.size = box.GetComponent<BoxCollider>().size;
                DestroyImmediate(box.GetComponent<BoxCollider>());
                DestroyImmediate(box.GetComponent<Rigidbody>());
                DestroyImmediate(box.GetComponent<BoxContentsC>());

                ConvertToBox(obj, size, box);
                DestroyImmediate(box.GetComponent<ObjectPickupC>());
                obj.GetComponent<ExtraComponentC_ModExtension>().componentID = ExtrasManager.Instance.GetExtraID(obj.name);

                CustomObjectsManager.Instance.OverwriteObject(CustomObjectsManager.Instance.GetRegistryNameByObject(originalObject), obj);
            }

            boxesToCreateInGame.Clear();
        }

        private void Flatten(Transform parent, Transform parentTo)
        {
            List<Transform> children = new List<Transform>();

            foreach (Transform child in parent)
            {
                children.Add(child);
            }

            foreach (Transform child in children)
            {
                Flatten(child, parentTo);
                RemoveAllComponents(child.gameObject, typeof(MeshFilter), typeof(MeshRenderer));
                if (child.gameObject.GetComponents<Component>().Length == 1 || !child.gameObject.GetComponent<MeshRenderer>())
                {
                    DestroyImmediate(child.gameObject);
                    continue;
                }
                var renderer = child.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    Material[] mats = renderer.materials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        mats[i] = defaultGlowMaterial;
                    }
                    renderer.materials = mats;
                    renderer.enabled = false;
                }

                child.parent = parentTo;
            }
        }

        /// <summary>
        /// Remove all the components from a gameobject, except the specified componentTypes
        /// </summary>
        /// <param name="go">The object in question</param>
        /// <param name="componentTypes">The component types that shouldn't be removed</param>
        public static void RemoveAllComponents(GameObject go, params Type[] componentTypes)
        {
            Component[] components = go.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component.GetType() == typeof(Transform))
                    continue;

                if (componentTypes != null && Array.IndexOf(componentTypes, component.GetType()) == -1)
                {
                    DestroyImmediate(component);
                }
            }
        }

        /// <summary>
        /// Adjust how the object will sit in the trunk
        /// </summary>
        /// <param name="obj">The object in question</param>
        /// <param name="position">Adjustments you may need to make so the object doesn't clip through the body panels/roof rack/through other objects</param>
        /// <param name="rotation">Adjustments you may need to make so the object doesn't clip through the body panels/roof rack/through other objects</param>
        /// <param name="dimensions">Dimensions of the object. The trunk has 2 4x2x3 slots (this are also the default dimensions for most of the engine parts). Try to experiment with whatever works for your object</param>
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
        /// Adjust the scale of the object
        /// </summary>
        /// <param name="obj">The object in question</param>
        /// <param name="scale">The desired scale</param>
        public void AdjustCustomObjectSize(GameObject obj, Vector3 scale)
        {
            obj.transform.localScale = scale;
        }

        /// <summary>
        /// Adjust how the object looks like when being held
        /// </summary>
        /// <param name="obj">The object in question</param>
        /// <param name="throwRotation">The euler angles that the object will have, relative to the player once the item is no longer being held (dropped)</param>
        /// <param name="position">The object's position relative to the player while being held</param>
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
                mainTextureScale = originalMaterial.mainTextureScale,
                name = originalMaterial.name + "_Glow"
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
        /// This method is not implemented yet
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="rarity"></param>
        public void ConfigureCustomExtra(GameObject obj, int rarity)
        {
            //ExtraComponentC
        }

        /// <summary>
        /// Configure a custom engine to your likings
        /// </summary>
        /// <param name="obj">The object in question</param>
        /// <param name="acceleration">The 0-80 speed, in seconds</param>
        /// <param name="topSpeed">The max speed achievable with this engine</param>
        /// <param name="customAudio">The audio this engine should use</param>
        /// <param name="audioPitch">The pitch of the audio (Default is 9.5f)</param>
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
        /// <param name="engineWearRate">How should the engine's wear rate be affected? (ideally, make this lower than 1, as the default value is 0.0001; the formula for calculating the wear rate is 0.0001 - (0.0001 * engineWearRate))</param>
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

        /// <summary>
        /// Configure a custom fuel tank to your likings
        /// </summary>
        /// <param name="obj">The object in question</param>
        /// <param name="fuelCapacity">The maximum fuel capacity it can hold</param>
        /// <param name="initialFuelCapacity">How many liters of fuel should the object have when it's spawned?</param>
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

            if (request.isHttpError || request.error == "Generic/unknown HTTP error")
                return "0";

            string tagName = null;

            if (!request.isNetworkError)
            {
                string json = request.downloadHandler.text;
                Release release = JsonUtility.FromJson<Release>(json);
                tagName = release.tag_name;
            }
            else if (request.isNetworkError)
                return "-1";
            else
            {
                Console.LogError(modName, $"Error getting response for URL \"{URL}\": {request.error}");
                return "-1";
            }

            return tagName;
        }

        public void OpenURL(string URL)
        {
            Application.OpenURL(URL);
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

            // probably rate limited by github
            if (request.isHttpError || request.error == "Generic/unknown HTTP error")
                return "0";

            string tagName = null;

            if (!request.isNetworkError && !request.isHttpError)
            {
                string json = request.downloadHandler.text;
                Release release = JsonUtility.FromJson<Release>(json);
                tagName = release.tag_name;
            }
            else if (request.isNetworkError)
                return "-1";
            else
            {
                Console.LogError($"Error getting response for URL \"{URL}\": {request.error}");
                return "-1";
            }

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
        Custom,
        Default,
        Wheel
    }

    public enum BoxSizes
    {
        Small,
        Medium,
        Big
    }

    public enum AttachExtraTo
    {
        Trunk,
        Hood,
        Body,
        LeftDoor,
        RightDoor
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
