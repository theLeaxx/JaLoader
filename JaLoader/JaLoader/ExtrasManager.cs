using Steamworks;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JaLoader
{
    public class ExtrasManager : MonoBehaviour
    {
        #region Singleton
        public static ExtrasManager Instance { get; private set; }

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

            EventsManager.Instance.OnGameLoad += AddExtras;
            EventsManager.Instance.OnSave += SaveData;
            EventsManager.Instance.OnNewGame += DeleteData;
            EventsManager.Instance.OnCustomObjectsRegisterFinished += AddExtras;
        }
        #endregion
        private bool firstTime = false;
        private int currentFreeID = 0;

        private ModHelper modHelper;
        public Dictionary<AttachExtraTo, GameObject> ExtrasHolders = new Dictionary<AttachExtraTo, GameObject>();
        private readonly Dictionary<int, (GameObject, string, Dictionary<string, bool>)> SpawnedExtras = new Dictionary<int, (GameObject, string, Dictionary<string, bool>)>();
        private readonly Dictionary<(string, int), (GameObject, AttachExtraTo, Dictionary<string, bool>)> Extras = new Dictionary<(string, int), (GameObject, AttachExtraTo, Dictionary<string, bool>)>();
        private readonly Dictionary<(string, int), Mod> ExtraMod = new Dictionary<(string, int), Mod>();

        [SerializeField] private ExtrasSave data = new ExtrasSave();

        public Color red = new Color32(255, 0, 0, 43);
        public Color def = new Color32(255, 255, 255, 43);
        public Color orange = new Color32(255, 78, 0, 43);

        private GameObject DecalGlow;

        private void AddExtrasHolders()
        {
            ExtrasHolders.Clear();

            var position = new Vector3(0.1f, 0.9f, -0.1f);

            var ExtrasHolder = new GameObject("BodyExtrasHolder");
            ExtrasHolder.transform.SetParent(GameObject.Find("FrameHolder").transform.Find("TweenHolder/Frame"));
            ExtrasHolder.transform.localPosition = position;
            ExtrasHolder.transform.localEulerAngles = Vector3.zero;
            ExtrasHolder.transform.localScale = Vector3.one;
            ExtrasHolders.Add(AttachExtraTo.Body, ExtrasHolder);

            ExtrasHolder = new GameObject("TrunkExtrasHolder");
            ExtrasHolder.transform.SetParent(GameObject.Find("FrameHolder").transform.Find("TweenHolder/Frame/Boot"));
            ExtrasHolder.transform.localPosition = position;
            ExtrasHolder.transform.localEulerAngles = Vector3.zero;
            ExtrasHolder.transform.localScale = Vector3.one; 
            ExtrasHolders.Add(AttachExtraTo.Trunk, ExtrasHolder);

            ExtrasHolder = new GameObject("HoodExtrasHolder");
            ExtrasHolder.transform.SetParent(GameObject.Find("FrameHolder").transform.Find("TweenHolder/Frame/Bonnet"));
            ExtrasHolder.transform.localPosition = position;
            ExtrasHolder.transform.localEulerAngles = Vector3.zero;
            ExtrasHolder.transform.localScale = Vector3.one;
            ExtrasHolders.Add(AttachExtraTo.Hood, ExtrasHolder);

            ExtrasHolder = new GameObject("LDoorExtrasHolder");
            ExtrasHolder.transform.SetParent(GameObject.Find("FrameHolder").transform.Find("TweenHolder/Frame/L_Door"));
            ExtrasHolder.transform.localPosition = position;
            ExtrasHolder.transform.localEulerAngles = Vector3.zero;
            ExtrasHolder.transform.localScale = Vector3.one;
            ExtrasHolders.Add(AttachExtraTo.LeftDoor, ExtrasHolder);

            ExtrasHolder = new GameObject("RDoorExtrasHolder");
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                ExtrasHolder.transform.SetParent(GameObject.Find("FrameHolder").transform.Find("R_Door"));
            }
            else if (SceneManager.GetActiveScene().buildIndex == 3)
            {
                ExtrasHolder.transform.SetParent(GameObject.Find("FrameHolder").transform.Find("TweenHolder/Frame/DoorHolder"));
            }
            ExtrasHolder.transform.localPosition = position;
            ExtrasHolder.transform.localEulerAngles = Vector3.zero;
            ExtrasHolder.transform.localScale = Vector3.one;
            ExtrasHolders.Add(AttachExtraTo.RightDoor, ExtrasHolder);
            //ExtrasHolder.transform.localEulerAngles = new Vector3(359.2f, 186.7f, 174.3f);
            //ExtrasHolder.transform.localScale = new Vector3(-1.4f, -1.4f, -1.4f);

            SpawnedExtras.Clear();

            if(!firstTime)
            {
                firstTime = true;
                EventsManager.Instance.OnCustomObjectsRegisterFinished -= AddExtras;
                EventsManager.Instance.OnMenuLoad += AddExtras;
                modHelper = ModHelper.Instance;
            }
        }

        /// <summary>
        /// Get the extra holder of an extra by its name
        /// </summary>
        /// <param name="name">The name of the holder</param>
        /// <returns></returns>
        public GameObject GetHolder(string name)
        {
            var id = GetExtraID(name);

            if (id == -1)
            {
                return null;
            }

            return SpawnedExtras[id].Item1;
        }

        internal void AddModExtra(string registryName, int ID, Mod mod)
        {
            ExtraMod.Add((registryName, ID), mod);
        }

        /// <summary>
        /// Get the extra holder of an extra by its registry name
        /// </summary>
        /// <param name="registryName">The registry name of the extra</param>
        /// <returns></returns>
        public GameObject GetHolderByRegistryName(string registryName)
        {
            var id = GetExtraIDByRegistryName(registryName);

            if (id == -1)
            {
                Console.LogError($"Extra with registry name {registryName} does not exist!");
                return null;
            }

            return SpawnedExtras[id].Item1;
        }

        /// <summary>
        /// Is an extra installed?
        /// </summary>
        /// <param name="registryName">The registry name of the extra</param>
        /// <returns></returns>
        public bool IsInstalled(string registryName)
        {
            var id = GetExtraIDByRegistryName(registryName);

            if (id == -1)
            {
                Console.LogError($"Extra with registry name {registryName} does not exist!");
                return false;
            }

            return SpawnedExtras[id].Item1.GetComponent<HolderInformation>().Installed;
        }

        /// <summary>
        /// Is an extra installed?
        /// </summary>
        /// <param name="ID">The ID of the extra</param>
        /// <returns></returns>
        public bool IsInstalled(int ID)
        {
            if (SpawnedExtras.ContainsKey(ID) == false)
            {
                Console.LogError($"Extra with ID {ID} does not exist!");
                return false;
            }

            return SpawnedExtras[ID].Item1.GetComponent<HolderInformation>().Installed;
        }

        /// <summary>
        /// Get the extra holder of an extra by its ID
        /// </summary>
        /// <param name="ID">The ID of the holder</param>
        /// <returns></returns>
        public GameObject GetHolder(int ID)
        {
            if(SpawnedExtras.ContainsKey(ID) == false)
            {
                Console.LogError($"Extra with ID {ID} does not exist!");
                return null;
            }

            return SpawnedExtras[ID].Item1;
        }

        /// <summary>
        /// Get the ID of an extra by its name
        /// </summary>
        /// <param name="name">The name of the extra</param>
        /// <returns></returns>
        public int GetExtraID(string name)
        {
            foreach (var pair in Extras)
            {
                if (pair.Value.Item1.name == name)
                    return pair.Key.Item2;
            }

            return -1;
        }

        /// <summary>
        /// Get the ID of an extra by its registry name
        /// </summary>
        /// <param name="registryName">The registry name of the extra</param>
        /// <returns></returns>
        public int GetExtraIDByRegistryName(string registryName)
        {
            foreach(var pair in Extras)
            {
                if (pair.Key.Item1 == registryName)
                    return pair.Key.Item2;
            }

            Console.LogError($"Extra with registry name {registryName} does not exist!");
            return -1;
        }

        /// <summary>
        /// Get the registry name of an extra by its ID
        /// </summary>
        /// <param name="ID">The ID of the extra</param>
        /// <returns></returns>
        public string GetExtraRegistryName(int ID)
        {
            foreach(var pair in Extras)
            {
                if (pair.Key.Item2 == ID)
                    return pair.Key.Item1;
            }

            Console.LogError($"Extra with ID {ID} does not exist!");
            return "";
        }

        /// <summary>
        /// Add an extra to the game
        /// </summary>
        /// <param name="obj">The object in question</param>
        /// <param name="pos">The position of the object</param>
        /// <param name="registryName">The registry name of the extra</param>
        /// <param name="attachTo">What should it attach to?</param>
        public void AddExtraObject(GameObject obj, Vector3 pos, string registryName, AttachExtraTo attachTo)
        {
            foreach(var pair in Extras)
            {
                if (pair.Key.Item1 == registryName)
                {
                    Console.LogError($"Extra with registry name {registryName} already exists!");
                    return;
                }
            }

            currentFreeID++;
            Extras.Add((registryName, currentFreeID), (obj, attachTo, null));
        }

        /// <summary>
        /// Add an extra to the game
        /// </summary>
        /// <param name="obj">The object in question</param>
        /// <param name="pos">The position of the object</param>
        /// <param name="registryName">The registry name of the extra</param>
        /// <param name="attachTo">What should it attach to?</param>
        /// <param name="blockedBy">Parts that may block the installation of this extra part, or replace it (registryName, completely block (true) or replace current part (false))</param>
        public void AddExtraObject(GameObject obj, Vector3 pos, string registryName, AttachExtraTo attachTo, Dictionary<string, bool> blockedBy)
        {
            foreach (var pair in Extras)
            {
                if (pair.Key.Item1 == registryName)
                {
                    Console.LogError($"Extra with registry name {registryName} already exists!");
                    return;
                }
            }

            currentFreeID++;
            Extras.Add((registryName, currentFreeID), (obj, attachTo, blockedBy));
        }

        internal void DeleteExtra(string registryName)
        {
            foreach (var pair in Extras)
            {
                if (pair.Key.Item1 == registryName)
                {
                    Extras.Remove(pair.Key);
                    return;
                }
            }

            currentFreeID--;
        }

        /// <summary>
        /// Does an extra exist already?
        /// </summary>
        /// <param name="registryName">The registry name of the extra</param>
        /// <returns></returns>
        public bool ExtraExists(string registryName)
        {
            foreach (var pair in Extras)
            {
                if (pair.Key.Item1 == registryName)
                {
                    return true;
                }
            }

            return false;
        }

        public void StartGlow(int ID)
        {
            if (ID == -2)
            {
                DecalGlow.GetComponent<ExtraReceiverC>().GlowDecal();
                DecalGlow.GetComponent<ExtraReceiverC>().CollisionsOn();

                return;
            }

            SpawnedExtras[ID].Item1.GetComponent<ExtraReceiverC>().GlowMesh();
            SpawnedExtras[ID].Item1.GetComponent<ExtraReceiverC>().CollisionsOn();
            SpawnedExtras[ID].Item1.transform.Find(SpawnedExtras[ID].Item1.name).gameObject.SetActive(true);
        }

        public void StopGlow(int ID)
        {
            if (ID == -2)
            {
                DecalGlow.GetComponent<ExtraReceiverC>().GlowStop();
                DecalGlow.GetComponent<ExtraReceiverC>().CollisionsOff();

                return;
            }

            SpawnedExtras[ID].Item1.GetComponent<ExtraReceiverC>().GlowStop();
            SpawnedExtras[ID].Item1.GetComponent<ExtraReceiverC>().CollisionsOff();
            SpawnedExtras[ID].Item1.transform.Find(SpawnedExtras[ID].Item1.name).gameObject.SetActive(false);
        }

        public void Fitted(int ID) // make red if cant install, orange if replace
        {
            if(ID == -2)
            {
                var paintJob = PaintJobManager.Instance.GetPaintJobByMaterialName(ModHelper.Instance.carFrame.GetComponent<MeshRenderer>().materials[1].name.Replace(" (Instance)", ""));
                PaintJobManager.Instance.ApplyPaintjob(paintJob);
                var comp = FindObjectOfType<ExtraUpgradesC>();
                comp.installedDecal = paintJob.Material;
                comp.installedDecalColor = Color.white;
                DecalGlow.GetComponent<ExtraReceiverC>().CollisionsOff();
                DecalGlow.GetComponent<ExtraReceiverC>().glowGo = false;

                return;
            }

            bool canInstall = true;

            foreach(KeyValuePair<string, bool> pair in GetBlockedBy(ID))
            {
                if (pair.Key == GetExtraRegistryName(ID))
                    continue;

                if (Replace(pair, ID) == false)
                {
                    canInstall = false;
                    return;
                }
            }

            if (SpawnedExtras[ID].Item1.GetComponent<HolderInformation>().Installed == true)
                canInstall = false;

            if (!canInstall)
                return;

            SpawnedExtras[ID].Item1.GetComponent<ExtraReceiverC>().Action();

            if (SceneManager.GetActiveScene().buildIndex == 3)
            {
                FindObjectOfType<CarPerformanceC>().carExtrasWeight += SpawnedExtras[ID].Item1.GetComponent<HolderInformation>().Weight;
                FindObjectOfType<CarPerformanceC>().totalCarWeight += SpawnedExtras[ID].Item1.GetComponent<HolderInformation>().Weight;

            }
            SpawnedExtras[ID].Item1.GetComponent<ExtraReceiverC>().CollisionsOff();

            SpawnedExtras[ID].Item1.transform.GetChild(2).gameObject.SetActive(false);

            SpawnedExtras[ID].Item1.GetComponent<HolderInformation>().Installed = true;

            SpawnedExtras[ID].Item1.GetComponent<HolderInformation>().CurrentlyInstalledPart = SpawnedExtras[ID].Item2;
        }

        /*public void Uninstall(int ID)
        {
            if (SpawnedExtras[ID].Item1.GetComponent<HolderInformation>().Installed == false)
                return;

            var extraObj = SpawnedExtras[ID].Item1;
            extraObj.transform.Find("Mesh").gameObject.SetActive(false);
            var clone = extraObj.transform.Find("MeshReceiverClone");
            var newReceiver = Instantiate(clone, clone.transform.parent);
            Destroy(clone.transform.parent.Find("MeshReceiver").gameObject);
            newReceiver.name = "MeshReceiver";

            newReceiver.position = clone.position;
            newReceiver.rotation = clone.rotation;
            newReceiver.localScale = clone.localScale;

            List<GameObject> children = new List<GameObject>();

            foreach (Transform child in newReceiver.transform)
                children.Add(child.gameObject);

            newReceiver.gameObject.SetActive(true);

            newReceiver.transform.parent.GetComponent<ExtraReceiverC>().glowMesh = children.ToArray();
            newReceiver.transform.parent.GetComponent<ExtraReceiverC>().stopGlow = false;
            newReceiver.transform.parent.GetComponent<HolderInformation>().Installed = false;
            newReceiver.transform.parent.GetComponent<HolderInformation>().CurrentlyInstalledPart = "";

            FindObjectOfType<CarPerformanceC>().carExtrasWeight -= extraObj.GetComponent<HolderInformation>().Weight;
            FindObjectOfType<CarPerformanceC>().totalCarWeight -= extraObj.GetComponent<HolderInformation>().Weight;

            SpawnedExtras[ID].Item1.GetComponent<ExtraReceiverC>().Action();

            SpawnedExtras[ID].Item1.GetComponent<ExtraReceiverC>().CollisionsOff();
            SpawnedExtras[ID].Item1.transform.GetChild(2).gameObject.SetActive(false);
            SpawnedExtras[ID].Item1.GetComponent<HolderInformation>().Installed = true;
            SpawnedExtras[ID].Item1.GetComponent<HolderInformation>().CurrentlyInstalledPart = SpawnedExtras[ID].Item2;
            SpawnedExtras[ID].Item1.transform.Find(SpawnedExtras[ID].Item1.name).gameObject.SetActive(false);
        }*/

        public Dictionary<string, bool> GetBlockedBy(int ID)
        {
            var toReturn = new Dictionary<string, bool>();

            if (SpawnedExtras.ContainsKey(ID) == false)
            {
                Console.LogError($"Extra with ID {ID} does not exist!");
                return toReturn;
            }

            if (SpawnedExtras[ID].Item3 != null && SpawnedExtras[ID].Item3.Count > 0)
            {
                foreach (var pair in SpawnedExtras[ID].Item3)
                {
                    if (ExtraExists(pair.Key))
                    {
                        if (GetHolder(GetExtraIDByRegistryName(pair.Key))?.GetComponent<HolderInformation>())
                        {
                            toReturn.Add(pair.Key, pair.Value);
                        }
                    }
                }
            }

            foreach(var entry in SpawnedExtras)
            {
                if(entry.Value.Item3 != null && entry.Value.Item3.Count > 0)
                {
                    foreach(var pair in entry.Value.Item3)
                    {
                        if (pair.Key == GetExtraRegistryName(ID))
                        {
                            toReturn.Add(entry.Value.Item2, pair.Value);
                        }
                    }
                }
            }

            return toReturn;
        }

        private bool Replace(KeyValuePair<string, bool> pair, int ID)
        {
            if (pair.Value == true)
            {
                return false;
            }
            else
            {
                var extraObj = SpawnedExtras[GetExtraIDByRegistryName(pair.Key)].Item1;
                extraObj.transform.Find("Mesh").gameObject.SetActive(false);
                var clone = extraObj.transform.Find("MeshReceiverClone");
                var newReceiver = Instantiate(clone, clone.transform.parent);
                Destroy(clone.transform.parent.Find("MeshReceiver").gameObject);
                newReceiver.name = "MeshReceiver";

                newReceiver.position = clone.position;
                newReceiver.rotation = clone.rotation;
                newReceiver.localScale = clone.localScale;

                List<GameObject> children = new List<GameObject>();

                foreach (Transform child in newReceiver.transform)
                    children.Add(child.gameObject);

                newReceiver.gameObject.SetActive(true);

                newReceiver.transform.parent.GetComponent<ExtraReceiverC>().glowMesh = children.ToArray();
                newReceiver.transform.parent.GetComponent<ExtraReceiverC>().stopGlow = false;
                newReceiver.transform.parent.GetComponent<HolderInformation>().Installed = false;
                newReceiver.transform.parent.GetComponent<HolderInformation>().CurrentlyInstalledPart = "";

                FindObjectOfType<CarPerformanceC>().carExtrasWeight -= extraObj.GetComponent<HolderInformation>().Weight;
                FindObjectOfType<CarPerformanceC>().totalCarWeight -= extraObj.GetComponent<HolderInformation>().Weight;

                SpawnedExtras[ID].Item1.GetComponent<ExtraReceiverC>().Action();

                SpawnedExtras[ID].Item1.GetComponent<ExtraReceiverC>().CollisionsOff();
                SpawnedExtras[ID].Item1.transform.GetChild(2).gameObject.SetActive(false);
                SpawnedExtras[ID].Item1.GetComponent<HolderInformation>().Installed = true;
                SpawnedExtras[ID].Item1.GetComponent<HolderInformation>().CurrentlyInstalledPart = SpawnedExtras[ID].Item2;
                SpawnedExtras[ID].Item1.transform.Find(SpawnedExtras[ID].Item1.name).gameObject.SetActive(false);

                return true;
            }
        }

        private void AddExtras()
        {
            try
            {
                DecalGlow = GameObject.Find("FrameHolder/TweenHolder/Frame/UpgradeHolders/DecalGlow").gameObject;
            }
            catch (Exception)
            {

            }

            AddExtrasHolders();

            foreach(var extra in Extras)
            {
                GameObject spawnedHolder = Instantiate(extra.Value.Item1);
                SceneManager.MoveGameObjectToScene(spawnedHolder, SceneManager.GetActiveScene());

                spawnedHolder.transform.SetParent(ExtrasHolders[extra.Value.Item2].transform, true);
                spawnedHolder.name = spawnedHolder.name.Substring(0, spawnedHolder.name.Length - 7);
                spawnedHolder.transform.localPosition = new Vector3(-0.1f, -0.9f, 0.1f);
                spawnedHolder.transform.localEulerAngles = new Vector3(0, 0, 0);
                spawnedHolder.transform.localScale = new Vector3(1, 1, 1);
                SpawnedExtras.Add(extra.Key.Item2, (spawnedHolder, extra.Key.Item1, extra.Value.Item3));

                if (SceneManager.GetActiveScene().buildIndex == 1)
                {
                    ModHelper.RemoveAllComponents(spawnedHolder.transform.GetChild(0).gameObject, typeof(MeshFilter), typeof(MeshRenderer));
                }

                spawnedHolder.SetActive(true);
            }

            StartCoroutine(WaitUntilLoadFinished());
        }

        private void Update()
        {
            if (!SettingsManager.Instance.DebugMode)
                return;

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKeyDown(KeyCode.S))
                {
                    Console.LogDebug("JaLoader", "Saved extra upgrades status!");
                    SaveData();
                }
                else if (Input.GetKeyDown(KeyCode.L))
                {
                    Console.LogDebug("JaLoader", "Loaded extra upgrades status!");
                    LoadData();
                }
            }
        }
        private IEnumerator WaitUntilLoadFinished()
        {
            while (!ModLoader.Instance.finishedInitializingPartTwoMods)
                yield return null;

            LoadData();

            yield return null;
        }

        private void SaveData()
        {
            if (data != null)
                data.Clear();
            else
                data = new ExtrasSave();

            foreach (var entry in SpawnedExtras.Keys)
            {
                if (!SpawnedExtras[entry].Item1.GetComponent<HolderInformation>() || !SpawnedExtras[entry].Item1.GetComponent<HolderInformation>().Installed)
                    continue;

                string name = $"{GetExtraRegistryName(entry)}";

                data.Add(name, true);
            }

            File.WriteAllText(Path.Combine(Application.persistentDataPath, @"ExtrasData.json"), JsonUtility.ToJson(data, true));
        }

        private void LoadData()
        {
            if (Extras.Count == 0 || Extras == null)
                return;

            if (File.Exists(Path.Combine(Application.persistentDataPath, @"ExtrasData.json")))
            {
                string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, @"ExtrasData.json"));
                data = JsonUtility.FromJson<ExtrasSave>(json);

                foreach (string entry in data.Keys)
                {
                    int ID = GetExtraIDByRegistryName(entry);

                    if(ExtraMod.ContainsKey((entry, ID)))
                        if (ModLoader.Instance.disabledMods.Contains(ExtraMod[(entry, ID)]))
                            continue;

                    if (ID == -1)
                        continue;

                    Fitted(ID);
                }
            }
        }

        public void DeleteData()
        {
            File.Delete(Path.Combine(Application.persistentDataPath, @"ExtrasData.json"));
        }
    }

    [Serializable]
    public class ExtrasSave : SerializableDictionary<string, bool> { };

    public class ExtraComponentC_ModExtension : MonoBehaviour
    {
        private GameObject _camera;

        public GameObject carLogic;

        public int componentID;

        public bool debugFitCar;

        public GameObject particlePrefab;

        public GameObject animTarget;

        public AudioClip audioOpen;

        public float openTimer;

        public bool readyToOpen;

        public bool debugOpen;

        public bool debugDie;

        public bool isCustomDecal;

        public Material material;

        public Material noDecalMaterial;

        public Color materialColour;

        public GameObject[] sprites = new GameObject[0];

        public int ID;
        public string registryName;
        private ExtrasManager extrasManager;
        private ExtraInformation extraInformation;

        private void Start()
        {
            extrasManager = ExtrasManager.Instance;
            _camera = Camera.main.gameObject;
            carLogic = GameObject.FindWithTag("CarLogic");

            var prefab = DebugObjectSpawner.Instance.GetDuplicateVanillaObject(0, transform);
            prefab.SetActive(false);

            particlePrefab = Instantiate(prefab.GetComponent<ExtraComponentC>().particlePrefab, transform);
            particlePrefab.SetActive(false);
            animTarget = gameObject.transform.GetChild(1).gameObject;

            var original = prefab.GetComponent<ExtraComponentC>().audioOpen;
            AudioClip clonedClip = AudioClip.Create(
                original.name + "_clone",
                original.samples,
                original.channels,
                original.frequency,
                false
            );

            float[] data = new float[original.samples * original.channels];
            original.GetData(data, 0);

            clonedClip.SetData(data, 0);

            audioOpen = clonedClip;
            Destroy(prefab);

            ID = GetComponent<ObjectIdentification>().ExtraID;

            if (ID == -2)
                return;

            registryName = extrasManager.GetExtraRegistryName(ID);

            extraInformation = GetComponent<ExtraInformation>();
            extraInformation.ID = ID;
            extraInformation.RegistryName = registryName;
            extraInformation.BlockedBy = extrasManager.GetBlockedBy(ID);
        }

        private void Update()
        {
            if (openTimer >= 0.7 && !debugOpen)
            {
                debugOpen = true;
                ActionPart2();
            }

            if (Input.GetButton("Fire1") && (double)openTimer < 0.7 && readyToOpen)
            {
                openTimer += Time.deltaTime;
            }
            else if (Input.GetKey(MainMenuC.Global.assignedInputStrings[16]) && (double)openTimer < 0.7 && readyToOpen)
            {
                openTimer += Time.deltaTime;
            }
            else if (Input.GetKey(MainMenuC.Global.assignedInputStrings[17]) && (double)openTimer < 0.7 && readyToOpen)
            {
                openTimer += Time.deltaTime;
            }

            if (Input.GetButtonUp("Fire1") && !debugOpen && readyToOpen)
            {
                StopOpen();
            }
            else if (Input.GetKeyUp(MainMenuC.Global.assignedInputStrings[16]) && !debugOpen && readyToOpen)
            {
                StopOpen();
            }
            else if (Input.GetKeyUp(MainMenuC.Global.assignedInputStrings[17]) && !debugOpen && readyToOpen)
            {
                StopOpen();
            }
        }

        public void PickUp()
        {
            StartRendering();
        }

        public void DecalReload()
        {
            carLogic.GetComponent<ExtraUpgradesC>().decalColour = materialColour;
            carLogic.GetComponent<ExtraUpgradesC>().SetDecals(material, applyDecal: true);
            carLogic.GetComponent<ExtraUpgradesC>().DecalGlow();
        }

        public void MoveToSlot1()
        {
            StartRendering();
        }

        public void StartRendering()
        {
            if (ID == -2)
            {
                if (carLogic == null)
                    return;

                var extraUpgradesC = carLogic.GetComponent<ExtraUpgradesC>();
                Console.LogDebug("JaLoader", "Extra ID: " + ID);

                extraUpgradesC.decalColour = materialColour; Console.LogDebug("2JaLoader", "Extra ID: " + ID);

                extraUpgradesC.SetDecals(material, applyDecal: false); Console.LogDebug("3JaLoader", "Extra ID: " + ID);

                extraUpgradesC.DecalGlow(); Console.LogDebug("4JaLoader", "Extra ID: " + ID);

            }

            bool caseRed = false;
            bool caseOrange = false;

            if(extraInformation.BlockedBy.Count > 0)
            {
                foreach(var pair in extraInformation.BlockedBy)
                {
                    var thisID = extrasManager.GetExtraIDByRegistryName(pair.Key);

                    if (!extrasManager.GetHolder(thisID).GetComponent<HolderInformation>().Installed)
                        continue;

                    var renderers = extrasManager.GetHolder(thisID).transform.Find("MeshReceiverClone").GetComponentsInChildren<Renderer>();

                    if (pair.Value)
                    {
                        caseRed = true;

                        foreach (var renderer in renderers)
                        {
                            foreach (var material in renderer.materials)
                                material.color = extrasManager.red;

                            renderer.enabled = true;
                        }
                    }
                    else
                    {
                        caseOrange = true;

                        foreach (var renderer in renderers)
                        {
                            foreach(var material in renderer.materials)
                                material.color = extrasManager.orange;
 
                            renderer.enabled = true;
                        }
                    }

                    var origMeshRenderers = extrasManager.GetHolder(thisID).transform.Find("Mesh").GetComponentsInChildren<Renderer>();
                    foreach (var renderer in origMeshRenderers)
                            renderer.enabled = false;
                    if(extrasManager.GetHolder(thisID).transform.Find("Mesh").GetComponent<MeshRenderer>() != null)
                        extrasManager.GetHolder(thisID).transform.Find("Mesh").GetComponent<MeshRenderer>().enabled = false;

                    extrasManager.GetHolder(thisID).transform.Find("MeshReceiverClone").gameObject.SetActive(true);
                }
            }

            if(caseRed)
                UIManager.Instance.ShowTooltip("This part is incompatible with the parts highlighted in red!");
            else if(caseOrange)
                UIManager.Instance.ShowTooltip("Installing this part will remove the parts highlighted in orange!");

            ExtrasManager.Instance.StartGlow(componentID);
        }

        public void MoveToSlot2()
        {
            StopRendering();
        }

        public void MoveToSlot3()
        {
            StopRendering();
        }

        public void StopRendering()
        {
            UIManager.Instance.HideTooltip();

            if (ID == -2)
            {
                var extraUpgradesC = carLogic.GetComponent<ExtraUpgradesC>();

                extraUpgradesC.SetDecals(extraUpgradesC.currentDecal);
                extraUpgradesC.DecalGlowStop();
            }

            ExtrasManager.Instance.StopGlow(componentID);
        }

        private void RevertColors()
        {
            if (extraInformation?.BlockedBy.Count > 0)
            {
                foreach (var pair in extraInformation.BlockedBy)
                {
                    var thisID = extrasManager.GetExtraIDByRegistryName(pair.Key);

                    var renderers = extrasManager.GetHolder(thisID).transform.Find("MeshReceiverClone").GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers)
                    {
                        foreach (var material in renderer.materials)
                            material.color = extrasManager.def;

                        renderer.enabled = false;
                    }

                    extrasManager.GetHolder(thisID).transform.Find("MeshReceiverClone").gameObject.SetActive(false);

                    var renderers_new = extrasManager.GetHolder(thisID).transform.Find("MeshReceiver").GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers_new)
                    {
                        foreach (var material in renderer.materials)
                            material.color = extrasManager.def;

                        renderer.enabled = false;
                    }

                    var origMeshRenderers = extrasManager.GetHolder(thisID).transform.Find("Mesh").GetComponentsInChildren<Renderer>();
                    foreach (var renderer in origMeshRenderers)
                        renderer.enabled = true;

                    if (extrasManager.GetHolder(thisID).transform.Find("Mesh").GetComponent<MeshRenderer>() != null)
                        extrasManager.GetHolder(thisID).transform.Find("Mesh").GetComponent<MeshRenderer>().enabled = true;
                }
            }

            UIManager.Instance.HideTooltip();
        }

        public void ThrowLogic()
        {
            StopRendering();
        }

        public void Action()
        {
            if (!debugOpen && !readyToOpen)
            {
                readyToOpen = true;
                animTarget.GetComponent<Animator>().SetBool("Open", value: true);
                gameObject.GetComponent<AudioSource>().clip = audioOpen;
                gameObject.GetComponent<AudioSource>().Play();
            }
        }

        public void DestroySprites()
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].SetActive(value: false);
            }
        }

        public void StopOpen()
        {
            openTimer = 0f;
            GetComponent<AudioSource>().Stop();
            animTarget.GetComponent<Animator>().SetBool("Open", value: false);
            readyToOpen = false;
        }

        public void ActionPart2()
        {
            RevertColors();
            ExtrasManager.Instance.Fitted(componentID);

            StartCoroutine("ParticlesAndAnim");
        }

        private IEnumerator ParticlesAndAnim()
        {
            GameObject particleInstance = Instantiate(particlePrefab, transform.position, Quaternion.identity);
            particleInstance.SetActive(true);
            Destroy(particleInstance, 0.55f);
            carLogic.GetComponent<ExtraUpgradesC>().ParticlesAndAnim(particleInstance);
            animTarget.GetComponent<Animator>().SetBool("Die", value: true);
            DestroySprites();
            yield return new WaitForSeconds(0.6f);
            _camera.GetComponent<DragRigidbodyC>().isHolding1 = null;
            _camera.GetComponent<DragRigidbodyC>().MoveItemsRightInventory();
            Destroy(base.gameObject);
        }
    }
}
