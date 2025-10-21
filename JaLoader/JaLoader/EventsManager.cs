using JaLoader.Common;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
        public delegate void ObjectEvents(ObjectEventArgs args);

        public event GameEvents OnMenuLoad;
        public event GameEvents OnLoadStart;
        public event GameEvents OnGameLoad;
        public event GameEvents OnGameUnload;

        public event GameEvents OnSave;
        public event GameEvents OnLoadSave;
        public event GameEvents OnNewGame;
        public event GameEvents OnPause;
        public event GameEvents OnUnpause;
        public event GameEvents OnSleep;
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
        public event MiscEvents OnCustomObjectsLoaded;
        public event MiscEvents OnCustomObjectsSaved;
        public event MiscEvents OnMenuFadeOut;
        public event MiscEvents OnModsInitialized;

        public event ObjectEvents OnObjectPickedUp;
        //public event ObjectEvents OnObjectPlaced; later update
        public event ObjectEvents OnHeldObjectPositionChanged;
        public event ObjectEvents OnObjectDropped;
        #endregion

        internal static void SafeInvoke<T>(T multicastDelegate, Action<T> invokeAction) where T : Delegate
        {
            if(multicastDelegate == null)
                return;

            Delegate[] invocationList = multicastDelegate.GetInvocationList();

            foreach (T subscriber in invocationList.Cast<T>())
            {
                if (subscriber.Target is MonoBehaviour mb)
                    if(mb == null || (mb != null && !mb.isActiveAndEnabled))
                    {
                        if (!(mb is DebugCamera))
                        {
                            Console.LogDebug("JaLoader", $"Skipping event `{multicastDelegate.Method.Name}`, by method `{subscriber.Method.Name}`, from script `{subscriber.Method.DeclaringType?.Name ?? "Unknown"}` because the MonoBehaviour is not active or has been destroyed.");
                            continue;
                        }
                    }

                try
                {
                    invokeAction(subscriber);
                }
                catch (Exception ex)
                {
                    Console.LogError("JaLoader", $"An error occured while invoking event `{multicastDelegate.Method.Name}`, by method `{subscriber.Method.Name}`, from script `{subscriber.Method.DeclaringType?.Name ?? "Unknown"}`");
                    Console.LogError("JaLoader", ex);
                    UnityEngine.Debug.LogError(ex);
                }
            }
        }

        public void OnUILoadFinish()
        {
            StartCoroutine(ReferencesLoader.LoadAssemblies());

            SafeInvoke(OnUILoadFinished, (handler) => handler());
        }

        public void OnModsInit()
        {
            SafeInvoke(OnModsInitialized, (handler) => handler());
        }

        public void OnObjectPickup(ObjectEventArgs args)
        {
            SafeInvoke(OnObjectPickedUp, (handler) => handler(args));
        }

        //public void OnObjectPlace(ObjectEventArgs args)
        //{
            //OnObjectPlaced?.Invoke(args);
        //}

        public void OnObjectDrop(ObjectEventArgs args)
        {
            SafeInvoke(OnObjectDropped, (handler) => handler(args));
        }

        public void OnHeldObjectPositionChange(ObjectEventArgs args)
        {
            SafeInvoke(OnHeldObjectPositionChanged, (handler) => handler(args));
        }

        public void OnSleepTrigger()
        {
            SafeInvoke(OnSleep, (handler) => handler());
        }

        public void OnMenuFade()
        {
            SafeInvoke(OnMenuFadeOut, (handler) => handler());
        }

        public void OnPauseGame()
        {
            Console.LogOnlyToFile("Paused game!");
            SafeInvoke(OnPause, (handler) => handler());
        }

        public void OnUnPauseGame()
        {
            Console.LogOnlyToFile("Unpaused game!");
            SafeInvoke(OnUnpause, (handler) => handler());
        }

        public void OnSettingsLoad()
        {
            //Console.LogOnlyToFile("Loaded JaLoader settings!");
            SafeInvoke(OnSettingsLoaded, (handler) => handler());
        }

        public void OnSettingsSave()
        {
            Console.LogOnlyToFile("Saved JaLoader settings!");
            SafeInvoke(OnSettingsSaved, (handler) => handler());
        }

        public void OnNewGameStart()
        {
            Console.LogOnlyToFile("New game started!");
            SafeInvoke(OnNewGame, (handler) => handler());
        }

        public void OnCustomObjectsRegisterFinish()
        {
            Console.LogOnlyToFile("Finished registering custom objects!");
            SafeInvoke(OnCustomObjectsRegisterFinished, (handler) => handler());
        }

        public void OnCustomObjectsLoad()
        {
            Console.LogOnlyToFile("Loaded custom objects!");
            SafeInvoke(OnCustomObjectsLoaded, (handler) => handler());
        }

        public void OnCustomObjectsSave()
        {
            Console.LogOnlyToFile("Saved custom objects!");
            SafeInvoke(OnCustomObjectsSaved, (handler) => handler());
        }

        public void OnSceneUnload(Scene unloadedScene)
        {
            if (OnGameUnload != null && unloadedScene.buildIndex == 3)
                OnGameUnloadFunc();
        }

        public void OnGameUnloadFunc()
        {
            SafeInvoke(OnGameUnload, (handler) => handler());
        }

        public void OnSceneLoad(Scene current, LoadSceneMode mode)
        {
            if (OnLoadStart != null && current.buildIndex == 2)
            {
                SafeInvoke(OnLoadStart, (handler) => handler());
                return;
            }

            if (OnMenuLoad != null && current.buildIndex == 1)
            {
                SafeInvoke(OnMenuLoad, (handler) => handler());
                return;
            }

            if (OnGameLoad != null && current.buildIndex == 3)
            {      
                OnGameLoadFunc();
                return;
            }

            if(current.buildIndex == 0 && JaLoaderSettings.SkipLanguage && JaLoaderSettings.SelectedLanguage)
                SceneManager.LoadScene("MainMenu");
        }

        public void OnGameLoadFunc(bool addComps = true)
        {
            if(addComps)
            {
                //FindObjectOfType<DirectorC>().gameObject.AddComponent<RouteReceiver>();
                FindObjectOfType<WalletC>().gameObject.AddComponent<ShopReceiver>();
                Camera.main.gameObject.AddComponent<MainMenuCReceiver>();
                Camera.main.gameObject.AddComponent<MotelsReceiver>();
                //Camera.main.gameObject.AddComponent<HarmonyManager>();
            }

            SafeInvoke(OnGameLoad, (handler) => handler());
        }

        public void OnLog(string message, string stack, LogType type)
        {
            if (OnException != null && type == LogType.Exception)
                OnException(message, stack);
        }

        public void OnLoad()
        {
            Console.LogOnlyToFile("Loaded save!");
            SafeInvoke(OnLoadSave, (handler) => handler());
        }

        public void OnSaved()
        {
            Console.LogOnlyToFile("Saved game!");
            SafeInvoke(OnSave, (handler) => handler());
        }

        public void CallRoute(string start, string destination, int distance)
        {
            try
            {
                SafeInvoke(OnRouteGenerated, (handler) => handler(start, destination, distance));
            }
            catch (Exception ex)
            {
                var method = ex.TargetSite;
                var typeName = method?.DeclaringType.FullName;

                if(method != null)
                {
                    if (typeName.Contains("UnityEngine"))
                        return;

                    Console.LogError($"Mod {typeName} had an error related to route generation, in method {method.Name}. Route generation may be broken!");
                    Console.LogError($"Please report this to the mod author.");
                }
                else
                {
                    Console.LogError(ex.Message);
                    Console.LogError(ex.StackTrace);
                }
            }
        }

        public void CallTransaction(string type)
        {
            SafeInvoke(OnTransaction, (handler) => handler());

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

            Console.Log(motels.Count);
            Console.Log(shops.Count);
            Console.Log(dealerships.Count);

            if (firstTime) return;

            Console.Log("---");

            /*paidMotel.Remove(motels[0]);
            motels.RemoveAt(0);
            shops.RemoveAt(0);
            dealerships.RemoveAt(0);

            Console.Log(motels.Count);
            Console.Log(shops.Count);
            Console.Log(dealerships.Count);*/
        }

        public void ChangeMoney()
        {
            EventsManager.Instance.CallTransaction("generic");
            /*for (int i = 0; i < motels.Count; i++)
            {
                if (motels[i].hasPaid && !paidMotel[motels[i]])
                {
                    paidMotel[motels[i]] = true;
                    Console.Log($"paid for motel");
                    EventsManager.Instance.CallTransaction("motel");
                }
                else if (shops[i].shutterOpen)
                {
                    Console.Log($"paid for shop");
                    EventsManager.Instance.CallTransaction("shop");
                }
                else if (dealerships[i].isBookOpen)
                {
                    Console.Log($"paid for dealership");
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

    public class MainMenuCReceiver : MonoBehaviour
    {
        public void LoadedGame()
        {
            EventsManager.Instance.OnLoad();
        }

        public void SavedGame()
        {
            EventsManager.Instance.OnSaved();
        }

        public void PausedGame()
        {
            EventsManager.Instance.OnPauseGame();
        }

        public void UnPausedGame()
        {
            EventsManager.Instance.OnUnPauseGame();
        }

        public void SavedStolenGoods()
        {
            if (ES3.Exists("savedStolenGoods"))
            {
                if (ES3.LoadBool("savedStolenGoods") == true)
                    ES3.Save(false, "savedStolenGoods");
            }
        }
    }

    public class MotelsReceiver : MonoBehaviour
    {
        public void Slept()
        {
            EventsManager.Instance.OnSleepTrigger();
        }
    }

    public struct ObjectEventArgs
    {
        public GameObject gameObject;
        public string gameObjectName;
        public string objectID;
        public float objectValue;
        public bool isEngineComponent;
        public ObjectPickupC pickupScript;
        public Transform movedFrom;
        public Transform movedTo;
    }
}
