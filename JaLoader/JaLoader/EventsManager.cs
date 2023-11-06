using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JaLoader
{
    public class EventsManager : MonoBehaviour
    {
        #region Singleton & OnSceneChange
        public static EventsManager Instance { get; private set; }

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

            SceneManager.sceneLoaded += OnSceneLoad;
            SceneManager.sceneUnloaded += OnSceneUnload;
            Application.logMessageReceived += OnLog;
        }
        #endregion

        #region Events
        public delegate void MiscEvents();
        public delegate void GameEvents();
        public delegate void LogEvents(string message, string stack);
        public delegate void TravelEvents(string startLocation, string endLocation, int distance);

        public event GameEvents OnMenuLoad;
        public event GameEvents OnLoadStart;
        public event GameEvents OnGameLoad;
        public event GameEvents OnGameUnload;

        public event GameEvents OnSave;
        public event LogEvents OnException;
        
        //public event GameEvents OnStoreTransaction;
        //public event GameEvents OnDealershipOrder;
        //public event GameEvents OnMotelCheckIn;
        public event GameEvents OnTransaction;

        //public event GameEvents OnSleep;

        public event TravelEvents OnRouteGenerated;
        //public event TravelEvents OnBorderPass;
        //public event TravelEvents OnBorderRevoke;

        public event MiscEvents OnSettingsLoaded;
        public event MiscEvents OnSettingsSaved;
        public event MiscEvents OnCustomObjectsRegisterFinished;
        public event MiscEvents OnUILoadFinished;
        #endregion

        private void Update()
        {
            //CatalogueBuyButtonC

            //ShopC
        }

        public void OnUILoadFinish()
        {
            OnUILoadFinished?.Invoke();
        }

        public void OnSettingsLoad()
        {
            OnSettingsLoaded?.Invoke();
        }

        public void OnSettingsSave()
        {
            OnSettingsSaved?.Invoke();
        }

        public void OnCustomObjectsRegisterFinish()
        {
            OnCustomObjectsRegisterFinished?.Invoke();
        }

        public void OnSceneUnload(Scene unloadedScene)
        {
            if (OnGameUnload != null && unloadedScene.buildIndex == 3)
                OnGameUnload();
        }

        public void OnSceneLoad(Scene current, LoadSceneMode mode)
        {
            //Console.Instance?.Log(current.buildIndex);

            if (OnLoadStart != null && current.buildIndex == 2)
            {
                OnLoadStart.Invoke();
                return;
            }

            if (OnMenuLoad != null && current.buildIndex == 1)
            {
                OnMenuLoad();
                return;
            }

            if (OnGameLoad != null && current.buildIndex == 3)
            {
                OnGameLoad();
                FindObjectOfType<DirectorC>().gameObject.AddComponent<RouteReceiver>();
                FindObjectOfType<WalletC>().gameObject.AddComponent<ShopReceiver>();
                return;
            }
        }

        public void OnLog(string message, string stack, LogType type)
        {
            if (OnSave != null && message == "Saved" && type == LogType.Log)
                OnSave();

            if (OnException != null && type == LogType.Exception)
                OnException(message, stack);
        }

        public void CallRoute(string start, string destination, int distance)
        {
            OnRouteGenerated?.Invoke(start, destination, distance);

            //Console.Instance.Log(cityName);
        }

        public void CallTransaction(string type)
        {
            OnTransaction?.Invoke();

            switch (type)
            {
                case "shop":
                    //OnStoreTransaction?.Invoke();
                    break;

                case "motel":
                    //OnMotelCheckIn?.Invoke();
                    break;

                case "laika":
                    //OnDealershipOrder?.Invoke();
                    break;
            }
        }
    }

    public class ShopReceiver : MonoBehaviour
    {
        Dictionary<MotelLogicC, bool> paidMotel = new Dictionary<MotelLogicC, bool>();

        List<MotelLogicC> motels = new List<MotelLogicC>();
        List<ShopC> shops = new List<ShopC>();
        List<MagazineLogicC> dealerships = new List<MagazineLogicC>();

        private void Start()
        {
            //StartCoroutine(WaitForMotelCheck());

            //RefreshList(true);

            //EventsManager.Instance.OnRouteGenerated += OnRouteGenerated;
        }

        private void OnRouteGenerated(string start, string destination, int distance)
        {
            RefreshList(false);
        }

        private void RefreshList(bool firstTime)
        {
            Dictionary<MotelLogicC, bool> paidMotel = new Dictionary<MotelLogicC, bool>();

            List<MotelLogicC> motels = new List<MotelLogicC>();
            List<ShopC> shops = new List<ShopC>();
            List<MagazineLogicC> dealerships = new List<MagazineLogicC>();

            foreach (var motel in FindObjectsOfType<MotelLogicC>())
            {
                motels.Add(motel);
                paidMotel.Add(motel, motel.hasPaid);
            }
            foreach (var shop in FindObjectsOfType<ShopC>())
                shops.Add(shop);
            foreach (var dealership in FindObjectsOfType<MagazineLogicC>())
                dealerships.Add(dealership);

            Console.Instance.Log(motels.Count);
            Console.Instance.Log(shops.Count);
            Console.Instance.Log(dealerships.Count);

            if (firstTime) return;

            Console.Instance.Log("---");

            /*paidMotel.Remove(motels[0]);
            motels.RemoveAt(0);
            shops.RemoveAt(0);
            dealerships.RemoveAt(0);

            Console.Instance.Log(motels.Count);
            Console.Instance.Log(shops.Count);
            Console.Instance.Log(dealerships.Count);*/
        }

        public void ChangeMoney()
        {
            EventsManager.Instance.CallTransaction("generic");
            /*for (int i = 0; i < motels.Count; i++)
            {
                if (motels[i].hasPaid && !paidMotel[motels[i]])
                {
                    paidMotel[motels[i]] = true;
                    Console.Instance.Log($"paid for motel");
                    EventsManager.Instance.CallTransaction("motel");
                }
                else if (shops[i].shutterOpen)
                {
                    Console.Instance.Log($"paid for shop");
                    EventsManager.Instance.CallTransaction("shop");
                }
                else if (dealerships[i].isBookOpen)
                {
                    Console.Instance.Log($"paid for dealership");
                    EventsManager.Instance.CallTransaction("laika");
                }
            }*/
        }

        IEnumerator WaitForMotelCheck()
        {
            while (FindObjectOfType<MotelLogicC>() == null)
                yield return null;

            RefreshList(true);
        }
    }

    public class RouteReceiver : MonoBehaviour
    {
        private RouteGeneratorC routeGenerator;
        private bool called;

        private MotelLogicC[] motels;
        private bool slept;

        private void Awake()
        {
            motels = FindObjectsOfType<MotelLogicC>();
            routeGenerator = FindObjectOfType<RouteGeneratorC>();
        }

        private void Update()
        {
            if (called)
            {
                foreach (var motel in motels)
                {
                    if (!motel.hasSlept)
                    {
                        slept = false;
                        break;
                    }
                    else
                    {
                        slept = true;
                    }
                }

                if (slept)
                    called = false;
            }

            if (routeGenerator.routeGenerated && !called && routeGenerator.routeChosenLength != 0)
            {
                slept = false;
                called = true;
                string[] info = Camera.main.transform.Find("MapHolder/Location").GetComponent<TextMesh>().text.Split(' ');
                if(info.Length > 0 && info[0] != "" && info[0] != string.Empty)
                {
                    string destination = info[2];
                    string start = info[0];
                    if (destination == "M.") destination = "Malko Tarnovo";
                    EventsManager.Instance.CallRoute(start, destination, routeGenerator.routeChosenLength * 70);
                }
                else
                {
                    EventsManager.Instance.CallRoute("Berlin", "Dresden", routeGenerator.routeChosenLength * 70);
                }

                StartCoroutine(WaitThenCheck());
            }
        }

        private IEnumerator WaitThenCheck()
        {
            yield return new WaitForSeconds(5);

            motels = FindObjectsOfType<MotelLogicC>();
        }
    }
}
