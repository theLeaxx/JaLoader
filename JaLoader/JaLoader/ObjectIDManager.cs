using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JaLoader
{
    public class ObjectIDManager : MonoBehaviour
    {
        public static ObjectIDManager Instance { get; private set; }

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

            SceneManager.activeSceneChanged += RefreshEngineCatalogue;
        }

        private List<GameObject> engineCatalogue = new List<GameObject>();

        private void Start()
        {

        }

        public void RefreshEngineCatalogue(Scene current, Scene next)
        {
            if (SceneManager.GetActiveScene().buildIndex == 3)
            {
                LoadCustomEngineParts();

                CarPerformanceC carPerformance = FindObjectOfType<CarPerformanceC>();
                carPerformance.engineCatalogue = engineCatalogue.ToArray();
            }
        }

        public int highestID = 1000;
        private Dictionary<int, GameObject> objects = new Dictionary<int, GameObject>();
        private Dictionary<string, int> objectIDS = new Dictionary<string, int>();



        public void RegisterObject(GameObject obj, string registryName)
        {
            objects.Add(highestID, obj);
            objectIDS.Add(registryName, highestID);

            DontDestroyOnLoad(obj);

            CarPerformanceC carPerformance = FindObjectOfType<CarPerformanceC>();
            Console.Instance.Log(carPerformance.engineCatalogue.Length);
            engineCatalogue = carPerformance.engineCatalogue.ToList();
            obj.GetComponent<EngineComponentC>().loadID = engineCatalogue.Count + 1;
            Console.Instance.Log(obj.GetComponent<EngineComponentC>().loadID);
            engineCatalogue.Add(obj);
            carPerformance.engineCatalogue = engineCatalogue.ToArray();

            Console.Instance.Log(carPerformance.engineCatalogue.Length);
            Console.Instance.Log(GetObjectFromID(obj.GetComponent<EngineComponentC>().loadID + 970).GetComponent<FixTextOnObjectPickup>().objName);
            Console.Instance.Log(GetObjectID("JMLEngine"));

            highestID += 1;
        }

        private void LoadCustomEngineParts()
        {
            CarPerformanceC carPerformance = FindObjectOfType<CarPerformanceC>();

            Console.Instance.Log(carPerformance.engineLoadID);
            Console.Instance.Log(GetObjectFromID(carPerformance.engineLoadID + 970).GetComponent<FixTextOnObjectPickup>().objName);

            /*GameObject gameObject = Instantiate(engineCatalogue[engineLoadID], base.transform.position, base.transform.rotation);
            gameObject.transform.parent = InstalledEngine.transform.parent;
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localEulerAngles = Vector3.zero;
            Object.Destroy(InstalledEngine);
            InstalledEngine = gameObject;
            InstalledEngine.GetComponent<ObjectPickupC>().isInEngine = true;
            InstalledEngine.GetComponent<ObjectPickupC>().placedAt = InstalledEngine.transform.parent.gameObject;
            InstalledEngine.transform.parent.GetComponent<HoldingLogicC>().isOccupied = true;
            InstalledEngine.transform.parent.GetComponent<Collider>().enabled = false;
            InstalledEngine.SendMessage("SendStatsToCarPerf");*/
        }

        public int GetObjectID(string registryName)
        {
            if(objectIDS.ContainsKey(registryName))
                return objectIDS[registryName];

            return -1;
        }
        
        public GameObject GetObject(string registryName)
        {
            return GetObjectFromID(GetObjectID(registryName));
        }

        public GameObject GetObjectFromID(int id)
        {
            if (objects.ContainsKey(id))
                return objects[id];

            return null;
        }


        public void RegisterEngineComponent(GameObject obj, EnginePartTypes type)
        {
            CarPerformanceC carPerformance = FindObjectOfType<CarPerformanceC>();

            switch (type)
            {
                case EnginePartTypes.Engine:
                    obj.GetComponent<EngineComponentC>().loadID = engineCatalogue.Count;
                    engineCatalogue = carPerformance.engineCatalogue.ToList();
                    engineCatalogue.Add(obj);
                    carPerformance.engineCatalogue = engineCatalogue.ToArray();
                    //obj.GetComponent<ObjectPickupC>().objectID = engineCatalogue.Count;
                    Console.Instance.Log(carPerformance.engineCatalogue.Last().GetComponent<FixTextOnObjectPickup>().objName);
                    break;
            }
        }
    }
}