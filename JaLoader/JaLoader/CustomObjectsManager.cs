using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization; 

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
        }
        #endregion

        [SerializeField] private CustomObjectSave data = new CustomObjectSave();

        private List<GameObject> engineCatalogue = new List<GameObject>();

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
                LoadData();
            }
        }

        public void OnSceneChange(Scene current, Scene next)
        {
            if (SceneManager.GetActiveScene().buildIndex == 3)
            {
                //GameObject.Find("UI Root").transform.Find("Exit").GetComponent<UIButton>().onClick.Add(SaveData());

                CarPerformanceC carPerformance = FindObjectOfType<CarPerformanceC>();
                carPerformance.engineCatalogue = engineCatalogue.ToArray();
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
            data.Clear();

            foreach ((string, int) entry in spawnedDatabase.Keys)
            {
                List<float> parameters = new List<float>();
                PartTypes type = PartTypes.Engine;
                bool inEngine = spawnedDatabase[entry].GetComponent<ObjectPickupC>().isInEngine;
                string json;

                if (spawnedDatabase[entry].GetComponent<EngineComponentC>())
                {
                    parameters.Add(spawnedDatabase[entry].GetComponent<EngineComponentC>().condition);

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

                json = TupleToString((inEngine, type, parameters.ToArray(), Vector2.zero));
                StringToTuple(json);

                string name = $"{entry.Item1}_{entry.Item2}";
                
                data.Add(name, json);
            }

            File.WriteAllText(Path.Combine(Application.persistentDataPath, @"CustomObjectsData.json"), JsonUtility.ToJson(data, true));
        }

        public void LoadData()
        {
            if (File.Exists(Path.Combine(Application.persistentDataPath, @"CustomObjectsData.json")))
            {
                string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, @"CustomObjectsData.json"));
                data = JsonUtility.FromJson<CustomObjectSave>(json);

                foreach (string entry in data.Keys)
                {
                    string name = entry.Split('_')[0];
                    string id = entry.Split('_')[1];

                    (bool, PartTypes, float[], Vector2) tuple = StringToTuple(data[entry]);

                    GameObject obj = SpawnObject(name, Vector3.zero, Quaternion.identity);
                    obj.GetComponent<Rigidbody>().isKinematic = true;
                    obj.GetComponent<Collider>().isTrigger = true;

                    if (tuple.Item1 == true)
                    {
                        switch (tuple.Item2)
                        {
                            case PartTypes.Engine:
                                obj.transform.parent = ModReferences.Instance.partHolders[PartTypes.Engine];
                                obj.GetComponent<EngineComponentC>().condition = tuple.Item3[0];
                                PlaceObjectInEngine(obj);
                                break;

                            case PartTypes.FuelTank:
                                obj.transform.parent = ModReferences.Instance.partHolders[PartTypes.FuelTank];
                                obj.GetComponent<EngineComponentC>().condition = tuple.Item3[0];
                                PlaceObjectInEngine(obj);
                                break;

                            case PartTypes.Carburettor:
                                obj.transform.parent = ModReferences.Instance.partHolders[PartTypes.Carburettor];
                                obj.GetComponent<EngineComponentC>().condition = tuple.Item3[0];
                                PlaceObjectInEngine(obj);
                                break;

                            case PartTypes.AirFilter:
                                obj.transform.parent = ModReferences.Instance.partHolders[PartTypes.AirFilter];
                                obj.GetComponent<EngineComponentC>().condition = tuple.Item3[0];
                                PlaceObjectInEngine(obj);
                                break;

                            case PartTypes.IgnitionCoil:
                                obj.transform.parent = ModReferences.Instance.partHolders[PartTypes.IgnitionCoil];
                                obj.GetComponent<EngineComponentC>().condition = tuple.Item3[0];
                                PlaceObjectInEngine(obj);
                                break;

                            case PartTypes.Battery:
                                obj.transform.parent = ModReferences.Instance.partHolders[PartTypes.Battery];
                                obj.GetComponent<EngineComponentC>().condition = tuple.Item3[0];
                                PlaceObjectInEngine(obj);
                                break;

                            case PartTypes.WaterTank:
                                obj.transform.parent = ModReferences.Instance.partHolders[PartTypes.WaterTank];
                                obj.GetComponent<EngineComponentC>().condition = tuple.Item3[0];
                                PlaceObjectInEngine(obj);
                                break;
                        }
                    }
                }
            }
        }

        private void PlaceObjectInEngine(GameObject obj)
        {
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.GetComponent<ObjectPickupC>().isInEngine = true;
            obj.GetComponent<ObjectPickupC>().placedAt = obj.transform.parent.gameObject;
            obj.transform.parent.GetComponent<HoldingLogicC>().isOccupied = true;
            obj.transform.parent.GetComponent<Collider>().enabled = false;
            obj.SendMessage("SendStatsToCarPerf");
        }

        private static string TupleToString((bool, PartTypes, float[], Vector2) point)
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

            string str = $"{point.Item1}|{(int)point.Item2}|{arrayStr}|{point.Item4.x}|{point.Item4.y}";

            return str;
        }

        private static (bool, PartTypes, float[], Vector2) StringToTuple(string str)
        {
            string[] param = str.Split('|');
            string[] floatParam = param[2].Split();

            bool inEngine = bool.Parse(param[0]);
            PartTypes partType = (PartTypes)int.Parse(param[1]); 
            float[] floatArrayParam = new float[floatParam.Length];
            Vector2 vector2 = new Vector2(float.Parse(param[3]), float.Parse(param[4]));

            for (int i = 0; i < floatParam.Length; i++)
            {
                floatArrayParam[i] = float.Parse(floatParam[i]);
            }

            return (inEngine, partType, floatArrayParam, vector2);
        }
    }


    [Serializable]
    public class CustomObjectSave : SerializableDictionary<string, string> { };
}