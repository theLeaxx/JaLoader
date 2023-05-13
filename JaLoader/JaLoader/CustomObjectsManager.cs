using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using Theraot.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using static UnityEngine.EventSystems.EventTrigger;

namespace JaLoader
{
    public class CustomObjectsManager : MonoBehaviour
    {
        #region Singleton
        public static CustomObjectsManager Instance { get; private set; }

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

            SceneManager.activeSceneChanged += OnSceneChange;
            Application.logMessageReceived += OnLog;
        }
        #endregion

        [SerializeField] private CustomObjectSave data = new CustomObjectSave();

        private List<GameObject> engineCatalogue = new List<GameObject>();

        private List<Transform> bootSlots = new List<Transform>();
        private GameObject boot;
        private bool allObjectsRegistered;

        private void Start()
        {

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Console.Instance.Log("test");
                SaveData();
            }

            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                Console.Instance.Log("test 2");
                LoadData(true);
            }
        }

        public void OnLog(string message, string stack, LogType type)
        {
            if (message == "Saved" && type == LogType.Log)
            {
                Console.Instance.Log(stack);
                SaveData();
            }
        }

        public void OnSceneChange(Scene current, Scene next)
        {
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                if (!allObjectsRegistered)
                {
                    StartCoroutine(WaitUntilLoadFinished());
                    return;
                }

                StartCoroutine(LoadDelay(0.01f, false));
            }

            if (SceneManager.GetActiveScene().buildIndex == 3)
            {
                ModReferences.Instance.RefreshPartHolders();

                var bootObjects = FindObjectsOfType<InventoryLogicC>();

                for (int i = 0; i < bootObjects.Length; i++)
                {
                    if (bootObjects[i].gameObject.name == "Boot" && bootObjects[i].transform.parent.parent.parent.name == "FrameHolder")
                    {
                        boot = bootObjects[i].gameObject;
                        break;
                    }
                }

                for (int i = 0; i < boot.transform.GetComponentsInChildren<InventoryRelayC>().Length; i++)
                {
                    bootSlots.Add(boot.transform.GetComponentsInChildren<InventoryRelayC>()[i].transform);
                }

                StartCoroutine(LoadDelay(1f, true));
            }
        }

        public int currentFreeID = 0;
        private Dictionary<string, GameObject> database = new Dictionary<string, GameObject>();
        private Dictionary<(string, int), GameObject> spawnedDatabase = new Dictionary<(string, int), GameObject>();

        public void RegisterObject(GameObject obj, string registryName)
        {
            obj.SetActive(false);

            database.Add($"{registryName}", obj);
            DontDestroyOnLoad(obj);
        }

        public GameObject GetObject(string registryName)
        {
            if (database.ContainsKey(registryName))
                return database[registryName];

            return null;
        }

        public GameObject SpawnObject(string registryName)
        {
            GameObject objToSpawn = GetObject(registryName);

            if(objToSpawn == null) return null;

            GameObject spawnedObj = Instantiate(objToSpawn);
            SceneManager.MoveGameObjectToScene(spawnedObj, SceneManager.GetActiveScene());
            spawnedObj.SetActive(true);

            IncrementID(registryName, spawnedObj);

            return spawnedObj;
        }

        public GameObject SpawnObject(string registryName, Vector3 position)
        {
            GameObject objToSpawn = GetObject(registryName);

            if (objToSpawn == null) return null;

            GameObject spawnedObj = Instantiate(objToSpawn);
            SceneManager.MoveGameObjectToScene(spawnedObj, SceneManager.GetActiveScene());
            spawnedObj.transform.position = position;
            spawnedObj.SetActive(true);

            IncrementID(registryName, spawnedObj);

            return spawnedObj;
        }

        public GameObject SpawnObject(string registryName, Vector3 position, Quaternion rotation)
        {
            GameObject objToSpawn = GetObject(registryName);

            if (objToSpawn == null) return null;

            GameObject spawnedObj = Instantiate(objToSpawn);
            SceneManager.MoveGameObjectToScene(spawnedObj, SceneManager.GetActiveScene());
            spawnedObj.transform.position = position;
            spawnedObj.transform.rotation = rotation;
            spawnedObj.SetActive(true);

            IncrementID(registryName, spawnedObj);

            return spawnedObj;
        }

        private void IncrementID(string registryName, GameObject obj)
        {
            currentFreeID++;
            spawnedDatabase.Add((registryName, currentFreeID), obj);
        }

        public void SaveData()
        {
            Console.Instance.Log("1");
            Console.Instance.Log(spawnedDatabase.Keys.Count);
            Console.Instance.Log(spawnedDatabase.Keys.ToArray()[0]);

            if (data != null)
                data.Clear();
            else
                data = new CustomObjectSave();

            Console.Instance.Log("1.5");     

            foreach (var entry in spawnedDatabase.Keys)
            {
                Console.Instance.Log("2");
                List<float> parameters = new List<float>();
                PartTypes type = PartTypes.Engine;
                bool inEngine = spawnedDatabase[entry].GetComponent<ObjectPickupC>().isInEngine;
                Console.Instance.Log("3");
                string json = "";
                Vector3 trunkPos = Vector3.zero;

                if (spawnedDatabase[entry].GetComponent<EngineComponentC>())
                {
                    parameters.Add(spawnedDatabase[entry].GetComponent<EngineComponentC>().condition);
                    Console.Instance.Log("4");

                    switch (spawnedDatabase[entry].GetComponent<ObjectPickupC>().engineString)
                    {
                        case "EngineBlock":
                            type = PartTypes.Engine;
                            break;

                        case "FuelTank":
                            type = PartTypes.FuelTank;
                            break;

                        case "Carburettor":
                            type = PartTypes.Carburettor;
                            break;

                        case "AirFilter":
                            type = PartTypes.AirFilter;
                            break;

                        case "IgnitionCoil":
                            type = PartTypes.IgnitionCoil;
                            break;

                        case "Battery":
                            type = PartTypes.Battery;
                            break;

                        case "WaterContainer":
                            type = PartTypes.WaterTank;
                            break;
                    }
                }
                Console.Instance.Log("5");
                Console.Instance.Log(spawnedDatabase[entry].GetComponent<ObjectPickupC>().inventoryPlacedAt);

                if(!inEngine)
                    trunkPos = spawnedDatabase[entry].GetComponent<ObjectPickupC>().inventoryPlacedAt.localPosition;

                Console.Instance.Log("6");

                json = TupleToString((inEngine, type, parameters.ToArray(), trunkPos));
                StringToTuple(json);

                Console.Instance.Log("7");
                string name = $"{entry.Item1}_{entry.Item2}";
                
                data.Add(name, json);
            }

            File.WriteAllText(Path.Combine(Application.persistentDataPath, @"CustomObjectsData.json"), JsonUtility.ToJson(data, true));
        }

        public void LoadData(bool full)
        {
            if (File.Exists(Path.Combine(Application.persistentDataPath, @"CustomObjectsData.json")))
            {
                string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, @"CustomObjectsData.json"));
                data = JsonUtility.FromJson<CustomObjectSave>(json);
                Console.Instance.Log("1");

                foreach (string entry in data.Keys)
                {
                    string name = entry.Split('_')[0];
                    string id = entry.Split('_')[1];

                    (bool, PartTypes, float[], Vector3) tuple = StringToTuple(data[entry]);
                    Console.Instance.Log("2");
                    GameObject obj = SpawnObject(name, Vector3.zero, Quaternion.identity);
                    obj.GetComponent<Rigidbody>().isKinematic = true;
                    obj.GetComponent<Collider>().isTrigger = true;
                    if (obj.GetComponent<EngineComponentC>())
                    {
                        obj.GetComponent<EngineComponentC>().condition = tuple.Item3[0];
                    }
                    Console.Instance.Log("3");
                    
                    Console.Instance.Log(tuple.Item1);
                    #region Load In Engine
                    if (tuple.Item1.Equals(true))
                    {
                        switch (tuple.Item2)
                        {
                            case PartTypes.Engine:
                                Console.Instance.Log("3.5");
                                //  Destroy(ModReferences.Instance.partHolders[PartTypes.Engine].Find("EngineBlock").gameObject);
                                obj.transform.parent = ModReferences.Instance.partHolders[PartTypes.Engine];
                                Console.Instance.Log("3.75");
                                PlaceObjectInEngine(obj);
                                Console.Instance.Log("4");
                                break;

                            case PartTypes.FuelTank:
                                obj.transform.parent = ModReferences.Instance.partHolders[PartTypes.FuelTank];
                                PlaceObjectInEngine(obj);
                                break;

                            case PartTypes.Carburettor:
                                obj.transform.parent = ModReferences.Instance.partHolders[PartTypes.Carburettor];
                                PlaceObjectInEngine(obj);
                                break;

                            case PartTypes.AirFilter:
                                obj.transform.parent = ModReferences.Instance.partHolders[PartTypes.AirFilter];
                                PlaceObjectInEngine(obj);
                                break;

                            case PartTypes.IgnitionCoil:
                                obj.transform.parent = ModReferences.Instance.partHolders[PartTypes.IgnitionCoil];
                                PlaceObjectInEngine(obj);
                                break;

                            case PartTypes.Battery:
                                obj.transform.parent = ModReferences.Instance.partHolders[PartTypes.Battery];
                                PlaceObjectInEngine(obj);
                                break;

                            case PartTypes.WaterTank:
                                obj.transform.parent = ModReferences.Instance.partHolders[PartTypes.WaterTank];
                                PlaceObjectInEngine(obj);
                                break;
                        }
                    }
                    #endregion
                    else
                    {
                        if (!full)
                            return;

                        for (int i = 0; i < bootSlots.Count; i++)
                        {
                            if (bootSlots[i].localPosition == tuple.Item4)
                            {
                                Console.Instance.Log(bootSlots[i].name);
                                bootSlots[i].GetComponent<InventoryRelayC>().Occupy();
                                obj.transform.parent = bootSlots[i].transform;
                                obj.GetComponent<ObjectPickupC>().inventoryPlacedAt = bootSlots[i].transform;
                                obj.transform.localPosition = obj.GetComponent<ObjectPickupC>().inventoryAdjustPosition;
                                obj.transform.localEulerAngles = obj.GetComponent<ObjectPickupC>().inventoryAdjustRotation;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void PlaceObjectInEngine(GameObject obj)
        {
            if (obj.transform.parent.GetComponent<HoldingLogicC>().isOccupied)
            {
                obj.transform.parent.GetComponent<HoldingLogicC>().isOccupied = false;
                obj.transform.parent.GetComponent<Collider>().enabled = true;
                Destroy(obj.transform.parent.GetChild(0).gameObject);
            }

            Console.Instance.Log(obj.name);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localEulerAngles = Vector3.zero;
            Console.Instance.Log(obj.GetComponent<ObjectPickupC>());
            obj.GetComponent<ObjectPickupC>().isInEngine = true;
            obj.GetComponent<ObjectPickupC>().placedAt = obj.transform.parent.gameObject;
            Console.Instance.Log(obj.transform.parent);
            obj.transform.parent.GetComponent<HoldingLogicC>().isOccupied = true;
            obj.transform.parent.GetComponent<Collider>().enabled = false;

            obj.SendMessage("SendStatsToCarPerf");
        }

        private static string TupleToString((bool, PartTypes, float[], Vector3) point)
        {
            string arrayStr = "";

            for (int i = 0; i < point.Item3.Length; i++)
            {
                if (i == point.Item3.Length - 1)
                {
                    arrayStr += $"{point.Item3[i]}";
                }
                else
                {
                    arrayStr += $"{point.Item3[i]} ";
                }
            }

            string str = $"{point.Item1}|{(int)point.Item2}|{arrayStr}|{point.Item4.x}|{point.Item4.y}|{point.Item4.z}";

            return str;
        }

        // isInEngine (if false then it's in trunk) | PartType | other parameters (condition, fuel level etc) | trunk position
        private static (bool, PartTypes, float[], Vector3) StringToTuple(string str)
        {
            string[] param = str.Split('|');
            string[] floatParam = param[2].Split();

            bool inEngine = bool.Parse(param[0]);
            PartTypes partType = (PartTypes)int.Parse(param[1]);
            float[] floatArrayParam = new float[floatParam.Length];
            Vector3 vector3 = new Vector3(float.Parse(param[3]), float.Parse(param[4]), float.Parse(param[5]));

            for (int i = 0; i < floatParam.Length; i++)
            {
                floatArrayParam[i] = float.Parse(floatParam[i]);
            }

            return (inEngine, partType, floatArrayParam, vector3);
        }

        private IEnumerator WaitUntilLoadFinished()
        {
            while (!ModLoader.Instance.finishedLoadingMods)
                yield return null;

            allObjectsRegistered = true;

            LoadData(false);

            yield return null;
        }

        private IEnumerator LoadDelay(float seconds, bool fullLoad)
        {
            ModReferences.Instance.RefreshPartHolders();
            
            yield return new WaitForSeconds(seconds);

            LoadData(fullLoad);
        }
    }


    [Serializable]
    public class CustomObjectSave : SerializableDictionary<string, string> { };
}