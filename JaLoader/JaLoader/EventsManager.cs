using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        public delegate void GameEvents();
        public delegate void LogEvents(string message, string stack);
        public delegate void TravelEvents(string cityName);

        public event GameEvents OnMenuLoad;
        public event GameEvents OnLoadStart;
        public event GameEvents OnGameLoad;
        public event GameEvents OnGameUnload;

        public event GameEvents OnSave;
        public event LogEvents OnException;

        //public event GameEvents OnStoreTransaction;
        public event GameEvents OnDealershipOrder;
        //public event GameEvents OnMotelCheckIn;
        public event GameEvents OnTransaction;

        //public event GameEvents OnSleep;

        public event TravelEvents OnRouteGenerated;
        //public event TravelEvents OnBorderPass;
        //public event TravelEvents OnBorderRevoke;
        #endregion

        private void Update()
        {
            //CatalogueBuyButtonC

            //ShopC
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
                //FindObjectOfType<WalletC>().gameObject.AddComponent<ShopReceiver>();
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

        public void CallRoute(string cityName)
        {
            OnRouteGenerated?.Invoke(cityName);

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
            }
        }
    }

    public class ShopReceiver : MonoBehaviour
    {
        bool paidMotel;

        private void Start()
        {
            StartCoroutine(WaitForMotelCheck());
        }

        public void ChangeMoney()
        {
            if (FindObjectOfType<MotelLogicC>().hasPaid && !paidMotel)
            {
                paidMotel = true;
                //Console.Instance.Log($"paid for motel");
                EventsManager.Instance.CallTransaction("motel");
            }
            else if (FindObjectOfType<ShopC>().shutterOpen)
            {
                //Console.Instance.Log($"paid for shop");
                EventsManager.Instance.CallTransaction("shop");
            }
            
        }

        IEnumerator WaitForMotelCheck()
        {
            while (FindObjectOfType<MotelLogicC>() == null)
                yield return null;

            paidMotel = FindObjectOfType<MotelLogicC>().hasPaid;
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

            if (routeGenerator.routeGenerated && !called)
            {
                slept = false;
                called = true;
                string cityName = Camera.main.transform.Find("MapHolder/Location").GetComponent<TextMesh>().text.Split(' ')[2];
                if (cityName == "M.") cityName = "Malko Tarnovo";
                EventsManager.Instance.CallRoute(cityName);

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
