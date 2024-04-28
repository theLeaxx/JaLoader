using System;
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
        private readonly Dictionary<int, GameObject> SpawnedExtras = new Dictionary<int, GameObject>();
        private readonly Dictionary<(string, int), (GameObject, AttachExtraTo)> Extras = new Dictionary<(string, int), (GameObject, AttachExtraTo)>();

        [SerializeField] private ExtrasSave data = new ExtrasSave();

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
            return SpawnedExtras[GetExtraID(name)];
        }

        /// <summary>
        /// Get the extra holder of an extra by its ID
        /// </summary>
        /// <param name="ID">The ID of the holder</param>
        /// <returns></returns>
        public GameObject GetHolder(int ID)
        {
            return SpawnedExtras[ID];
        }

        /// <summary>
        /// Get the ID of an extra by its name
        /// </summary>
        /// <param name="name">The name of the extra</param>
        /// <returns></returns>
        public int GetExtraID(string name)
        {
            return Extras.First(x => x.Value.Item1.name == name).Key.Item2;
        }

        /// <summary>
        /// Get the ID of an extra by its registry name
        /// </summary>
        /// <param name="registryName">The registry name of the extra</param>
        /// <returns></returns>
        public int GetExtraIDByRegistryName(string registryName)
        {
            return Extras.First(x => x.Key.Item1 == registryName).Key.Item2;
        }

        /// <summary>
        /// Get the registry name of an extra by its ID
        /// </summary>
        /// <param name="ID">The ID of the extra</param>
        /// <returns></returns>
        public string GetExtraRegistryName(int ID)
        {
            return Extras.First(x => x.Key.Item2 == ID).Key.Item1;
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
                    Console.Log($"Extra with registry name {registryName} already exists!");
                    return;
                }
            }

            currentFreeID++;
            Extras.Add((registryName, currentFreeID), (obj, attachTo));
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
            SpawnedExtras[ID].GetComponent<ExtraReceiverC>().GlowMesh();
            SpawnedExtras[ID].GetComponent<ExtraReceiverC>().CollisionsOn();
            SpawnedExtras[ID].transform.GetChild(2).gameObject.SetActive(true);
        }

        public void StopGlow(int ID)
        {
            SpawnedExtras[ID].GetComponent<ExtraReceiverC>().GlowStop();
            SpawnedExtras[ID].GetComponent<ExtraReceiverC>().CollisionsOff();
            SpawnedExtras[ID].transform.GetChild(2).gameObject.SetActive(false);
        }

        public void Fitted(int ID)
        {
            SpawnedExtras[ID].GetComponent<ExtraReceiverC>().Action();
            if (SceneManager.GetActiveScene().buildIndex == 3)
            {
                FindObjectOfType<CarPerformanceC>().carExtrasWeight += SpawnedExtras[ID].GetComponent<HolderInformation>().Weight;
                FindObjectOfType<CarPerformanceC>().totalCarWeight += SpawnedExtras[ID].GetComponent<HolderInformation>().Weight;
            }
            SpawnedExtras[ID].GetComponent<ExtraReceiverC>().CollisionsOff();
            SpawnedExtras[ID].transform.GetChild(2).gameObject.SetActive(false);
            SpawnedExtras[ID].GetComponent<HolderInformation>().Installed = true;
        }

        private void AddExtras()
        {
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
                SpawnedExtras.Add(extra.Key.Item2, spawnedHolder);

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
                if (!SpawnedExtras[entry].GetComponent<HolderInformation>() || !SpawnedExtras[entry].GetComponent<HolderInformation>().Installed)
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
                    Fitted(GetExtraIDByRegistryName(entry));
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

        private void Start()
        {
            _camera = Camera.main.gameObject;
            carLogic = GameObject.FindWithTag("CarLogic");

            var prefab = DebugObjectSpawner.Instance.GetDuplicateVanillaObject(0, transform);
            prefab.SetActive(false);

            particlePrefab = Instantiate(prefab.GetComponent<ExtraComponentC>().particlePrefab, transform);
            particlePrefab.SetActive(false);
            animTarget = gameObject.transform.GetChild(0).gameObject;

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
        }

        private void Update()
        {
            if ((double)openTimer >= 0.7 && !debugOpen)
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
            ExtrasManager.Instance.StartGlow(componentID);   
        }

        public void DecalReload()
        {
            carLogic.GetComponent<ExtraUpgradesC>().decalColour = materialColour;
            carLogic.GetComponent<ExtraUpgradesC>().SetDecals(material, applyDecal: true);
            carLogic.GetComponent<ExtraUpgradesC>().DecalGlow();
        }

        public void MoveToSlot1()
        {
            ExtrasManager.Instance.StartGlow(componentID);
        }

        public void MoveToSlot2()
        {
            ExtrasManager.Instance.StopGlow(componentID);
        }

        public void MoveToSlot3()
        {
            ExtrasManager.Instance.StopGlow(componentID);
        }

        public void StopRendering()
        {
            ExtrasManager.Instance.StopGlow(componentID);
        }

        public void ThrowLogic()
        {
            ExtrasManager.Instance.StopGlow(componentID);
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
