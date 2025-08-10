using HarmonyLib;
using JaLoader.Common;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using Random = UnityEngine.Random;

namespace JaLoader
{
    public class HarmonyManager : MonoBehaviour
    {
        #region Singleton
        public static HarmonyManager Instance { get; private set; }
        private static Harmony harmony;
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

            gameObject.AddComponent<CoroutineManager>();

            harmony = new Harmony("Leaxx.JaLoader");
            PatchAll();
        }
        #endregion

        public void PatchAll()
        {
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void SetArgumentsFromScript(ref ObjectEventArgs args, ObjectPickupC script)
        {
            args.gameObject = script.gameObject;
            args.gameObjectName = script.gameObject.name;
            args.pickupScript = script;
            args.objectValue = script.sellValue;
            args.isEngineComponent = script.gameObject.GetComponent<EngineComponentC>() != null;

            if (script.gameObject.GetComponent<CustomObjectInfo>() == null && script.gameObject.GetComponent<ExtraInformation>() == null)
                args.objectID = script.objectID.ToString();
            else
            {
                if (script.gameObject.GetComponent<CustomObjectInfo>() != null)
                    args.objectID = script.gameObject.GetComponent<CustomObjectInfo>().objRegistryName;
                else
                    args.objectID = script.gameObject.GetComponent<ExtraInformation>().RegistryName;
            }
        }
    }

    public class CoroutineManager : MonoBehaviour
    {
        private static CoroutineManager instance;

        #region Singleton
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        #endregion

        public static Coroutine StartStaticCoroutine(IEnumerator routine)
        {
            return instance.StartCoroutine(routine);
        }

        public static void StopStaticCoroutine(Coroutine routine)
        {
            instance.StopCoroutine(routine);
        }
    }

    [HarmonyPatch(typeof(InventoryLogicC), "Update")]
    public static class InventoryLogicC_Update_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(InventoryLogicC __instance)
        {
            if (__instance.gameObject.name == "Boot")
            {
                var inventoryClickBlock = __instance.GetType().GetField("inventoryClickBlock", BindingFlags.Instance | BindingFlags.NonPublic);
                if (inventoryClickBlock != null)
                    inventoryClickBlock.SetValue(__instance, false);
            }
        }
    }

    [HarmonyPatch(typeof(MainMenuC), "SaveInventory")]
    public static class MainMenuC_SaveInventory_Patch
    {
        [HarmonyPostfix]
        public static void PostFix(MainMenuC __instance)
        {
            __instance.BroadcastMessage("SavedGame", SendMessageOptions.DontRequireReceiver);
        }
    }

    [HarmonyPatch(typeof(MainMenuC), "LateStart")]
    public static class MainMenuC_LateStart_Patch
    {
        [HarmonyPostfix]
        public static void PostFix(MainMenuC __instance)
        {
            __instance.BroadcastMessage("LoadedGame", SendMessageOptions.DontRequireReceiver);
        }
    }

    [HarmonyPatch(typeof(BedLogicC), "Later")]
    public static class BedLogicC_Later_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(BedLogicC __instance)
        {
            GameObject.FindObjectOfType<MotelsReceiver>().Slept();
        }
    }

    /*[HarmonyPatch(typeof(WheelScriptPCC), "SteerControle")]
    public static class WheelScriptPCC_SteerControle_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(WheelScriptPCC __instance)
        {
            double numToSteer = 1;
            if (!__instance.noClip && MainMenuC.Global.padInput == 2)
            {
                numToSteer *= Input.GetAxis("JoypadX");
            }
            Debug.Log(numToSteer);

            __instance.wheelCollider.steerAngle = Mathf.Lerp(__instance.wheelCollider.steerAngle, (float)numToSteer, Time.deltaTime * 5f);
        }
    }*/

    [HarmonyPatch(typeof(EngineComponentsCataloguePageC), "PurchaseGo")]
    public static class EngineComponentsCataloguePageC_PurchaseGo_Patch
    {
        [HarmonyPostfix]
        public static void PostFix(EngineComponentsCataloguePageC __instance)
        {
            ModHelper.Instance.wallet.GetComponent<ObjectPickupC>().ThrowLogic();
            EventsManager.Instance.CallTransaction("laika");
        }
    }

    [HarmonyPatch(typeof(ObjectPickupC), "PickUp")]
    public static class ObjectPickupC_PickUp_Patch
    {
        private static ObjectEventArgs objectEventArgs;

        [HarmonyPrefix]
        public static void Prefix(ObjectPickupC __instance)
        {
            objectEventArgs = new ObjectEventArgs();

            HarmonyManager.SetArgumentsFromScript(ref objectEventArgs, __instance);

            objectEventArgs.movedFrom = __instance.transform.parent;
        }

        [HarmonyPostfix]
        public static void PostFix(ObjectPickupC __instance)
        {
            objectEventArgs.movedTo = __instance.transform.parent;

            EventsManager.Instance.OnObjectPickup(objectEventArgs);
        }
    }

    [HarmonyPatch(typeof(ObjectPickupC), "ThrowLogic")]
    public static class ObjectPickupC_ThrowLogic_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(ObjectPickupC __instance)
        {
            var objectEventArgs = new ObjectEventArgs();

            HarmonyManager.SetArgumentsFromScript(ref objectEventArgs, __instance);

            EventsManager.Instance.OnObjectDrop(objectEventArgs);
        }
    }

    [HarmonyPatch(typeof(ObjectPickupC), "MoveToSlot1")]
    public static class ObjectPickupC_MoveToSlot1_Patch
    {
        private static ObjectEventArgs objectEventArgs;

        [HarmonyPrefix]
        public static void Prefix(ObjectPickupC __instance)
        {
            objectEventArgs = new ObjectEventArgs();

            HarmonyManager.SetArgumentsFromScript(ref objectEventArgs, __instance);

            objectEventArgs.movedFrom = __instance.transform.parent;
        }

        [HarmonyPostfix]
        public static void PostFix(ObjectPickupC __instance)
        {
            objectEventArgs.movedTo = GameObject.FindObjectOfType<DragRigidbodyC>().holdingParent1;
         
            EventsManager.Instance.OnHeldObjectPositionChange(objectEventArgs);
        }
    }

    [HarmonyPatch(typeof(ObjectPickupC), "MoveToSlot2")]
    public static class ObjectPickupC_MoveToSlot2_Patch
    {
        private static ObjectEventArgs objectEventArgs;

        [HarmonyPrefix]
        public static void Prefix(ObjectPickupC __instance)
        {
            objectEventArgs = new ObjectEventArgs();

            HarmonyManager.SetArgumentsFromScript(ref objectEventArgs, __instance);

            objectEventArgs.movedFrom = __instance.transform.parent;
        }

        [HarmonyPostfix]
        public static void PostFix(ObjectPickupC __instance)
        {
            objectEventArgs.movedTo = GameObject.FindObjectOfType<DragRigidbodyC>().holdingParent2;

            EventsManager.Instance.OnHeldObjectPositionChange(objectEventArgs);
        }
    }

    [HarmonyPatch(typeof(ObjectPickupC), "MoveToSlot3")]
    public static class ObjectPickupC_MoveToSlot3_Patch
    {
        private static ObjectEventArgs objectEventArgs;

        [HarmonyPrefix]
        public static void Prefix(ObjectPickupC __instance)
        {
            objectEventArgs = new ObjectEventArgs();

            HarmonyManager.SetArgumentsFromScript(ref objectEventArgs, __instance);

            objectEventArgs.movedFrom = __instance.transform.parent;
        }

        [HarmonyPostfix]
        public static void PostFix(ObjectPickupC __instance)
        {
            objectEventArgs.movedTo = GameObject.FindObjectOfType<DragRigidbodyC>().holdingParent3;

            EventsManager.Instance.OnHeldObjectPositionChange(objectEventArgs);
        }
    }

    [HarmonyPatch(typeof(MainMenuClickersC), "StartNewGame")]
    public static class MainMenuClickersC_StartNewGame_Patch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            EventsManager.Instance.OnNewGameStart();
        }
    }

    [HarmonyPatch(typeof(MainMenuClickersC), "StartNewGameSkipTutorial")]
    public static class MainMenuClickersC_StartNewGameSkipTutorial_Patch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            EventsManager.Instance.OnNewGameStart();
        }
    }

    [HarmonyPatch(typeof(MainMenuC), "Pause")]
    public static class MainMenuC_Pause_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(MainMenuC __instance)
        {
            if (__instance.hasWokenUp)
            {
                __instance.BroadcastMessage("PausedGame", SendMessageOptions.DontRequireReceiver);
                var uiRoot = GameObject.Find("UI Root");
                
                var modsButton = uiRoot.transform.Find("Mods").gameObject;
                modsButton.SetActive(true);
                TweenAlpha.Begin(modsButton, 0.8f, 1f);

                var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
                optionsButton.SetActive(true);
                TweenAlpha.Begin(optionsButton, 0.8f, 1f);
            }
        }

        [HarmonyPostfix]
        public static void PostFix()
        {
            var uiRoot = GameObject.Find("UI Root");
            var modsButton = uiRoot.transform.Find("Mods").gameObject;  
            modsButton.GetComponent<Collider>().enabled = true;

            var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
            optionsButton.GetComponent<Collider>().enabled = true;
        }
    }

    [HarmonyPatch(typeof(MainMenuC), "UnPause")]
    public static class MainMenuC_UnPause_Patch
    {
        [HarmonyPrefix]
        public static void PreFix(MainMenuC __instance)
        {
            if (__instance.isPaused == 1)
                __instance.BroadcastMessage("UnPausedGame", SendMessageOptions.DontRequireReceiver);
        }

        [HarmonyPostfix]
        public static void PostFix()
        {
            var uiRoot = GameObject.Find("UI Root");

            var modsButton = uiRoot.transform.Find("Mods").gameObject;
            TweenAlpha.Begin(modsButton, 0.8f, 0f);   
            modsButton.GetComponent<Collider>().enabled = false;

            var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
            TweenAlpha.Begin(optionsButton, 0.8f, 0f);
            optionsButton.GetComponent<Collider>().enabled = false;

            CoroutineManager.StartStaticCoroutine(DisableButtons());
        }

        public static IEnumerator DisableButtons()
        {
            yield return new WaitForSeconds(0.8f);
            var uiRoot = GameObject.Find("UI Root");

            var modsButton = uiRoot.transform.Find("Mods").gameObject;
            modsButton.SetActive(false);

            var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
            optionsButton.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(MainMenuC), "OpenOptions")]
    public static class MainMenuC_OpenOptions_Patch
    {
        [HarmonyPostfix]
        public static void PostFix()
        {
            var uiRoot = GameObject.Find("UI Root");

            var modsButton = uiRoot.transform.Find("Mods").gameObject;
            TweenAlpha.Begin(modsButton, 0.2f, 0f);
            modsButton.GetComponent<Collider>().enabled = false;

            var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
            TweenAlpha.Begin(optionsButton, 0.2f, 0f);
            optionsButton.GetComponent<Collider>().enabled = false;

            CoroutineManager.StartStaticCoroutine(DisableButtons());
        }

        public static IEnumerator DisableButtons()
        {
            yield return new WaitForSeconds(0.2f);
            var uiRoot = GameObject.Find("UI Root");

            var modsButton = uiRoot.transform.Find("Mods").gameObject;
            modsButton.SetActive(false);

            var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
            optionsButton.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(MainMenuC), "CloseOptionsPart2")]
    public static class MainMenuC_CloseOptionsPart2_Patch
    {
        [HarmonyPrefix]
        public static void PreFix(MainMenuC __instance)
        {
            if (__instance.hasWokenUp)
            {
                __instance.BroadcastMessage("PausedGame", SendMessageOptions.DontRequireReceiver);
                var uiRoot = GameObject.Find("UI Root");

                var modsButton = uiRoot.transform.Find("Mods").gameObject;
                modsButton.SetActive(true);
                TweenAlpha.Begin(modsButton, 0.8f, 1f);

                var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
                optionsButton.SetActive(true);
                TweenAlpha.Begin(optionsButton, 0.8f, 1f);
            }
        }

        [HarmonyPostfix]
        public static void PostFix()
        {
            var uiRoot = GameObject.Find("UI Root");
            var modsButton = uiRoot.transform.Find("Mods").gameObject;
            modsButton.GetComponent<Collider>().enabled = true;

            var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
            optionsButton.GetComponent<Collider>().enabled = true;
        }
    }

    [HarmonyPatch(typeof(MainMenuC), "OpenRestart")]
    public static class MainMenuC_OpenRestart_Patch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            var uiRoot = GameObject.Find("UI Root");

            var modsButton = uiRoot.transform.Find("Mods").gameObject;
            modsButton.SetActive(false);

            var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
            optionsButton.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(MainMenuC), "CloseRestartOptions")]
    public static class MainMenuC_CloseRestart_Patch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            var uiRoot = GameObject.Find("UI Root");

            var modsButton = uiRoot.transform.Find("Mods").gameObject;
            modsButton.SetActive(true);

            var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
            optionsButton.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(MainMenuC), "OpenReturnHome")]
    public static class MainMenuC_OpenReturnHome_Patch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            var uiRoot = GameObject.Find("UI Root");

            var modsButton = uiRoot.transform.Find("Mods").gameObject;
            modsButton.SetActive(false);

            var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
            optionsButton.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(MainMenuC), "CloseReturnHomeOptions")]
    public static class MainMenuC_CloseReturnHomeOptions_Patch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            var uiRoot = GameObject.Find("UI Root");

            var modsButton = uiRoot.transform.Find("Mods").gameObject;
            modsButton.SetActive(true);

            var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
            optionsButton.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(MainMenuC), "OpenSaveQuit")]
    public static class MainMenuC_OpenSaveQuit_Patch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            var uiRoot = GameObject.Find("UI Root");

            var modsButton = uiRoot.transform.Find("Mods").gameObject;
            modsButton.SetActive(false);

            var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
            optionsButton.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(MainMenuC), "OpenSaveQuitDesktop")]
    public static class MainMenuC_OpenSaveQuitDesktop_Patch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            var uiRoot = GameObject.Find("UI Root");

            var modsButton = uiRoot.transform.Find("Mods").gameObject;
            modsButton.SetActive(false);

            var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
            optionsButton.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(MainMenuC), "CloseSaveQuitOptions")]
    public static class MainMenuC_CloseSaveQuitOptions_Patch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            var uiRoot = GameObject.Find("UI Root");

            var modsButton = uiRoot.transform.Find("Mods").gameObject;
            modsButton.SetActive(true);

            var optionsButton = uiRoot.transform.Find("ModLoader Settings").gameObject;
            optionsButton.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(MainMenuC), "SavingStolenGoods")]
    public static class MainMenuC_SavingStolenGoods_Patch
    {
        [HarmonyPostfix]
        public static void PostFix(MainMenuC __instance)
        {
            double _num = 0.0;
            float stolenGoodsValue = __instance.GetType().GetField("stolenGoodsValue", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as float? ?? 0; 
            _num = ((double)stolenGoodsValue == 0.0) ? (-1.0) : (((double)stolenGoodsValue <= 10.0) ? 10.0 : (((double)stolenGoodsValue <= 25.0) ? 40.0 : ((!((double)stolenGoodsValue <= 50.0)) ? 100.0 : 80.0)));
            float _num2 = Random.Range(0f, 100f);

            if ((double)_num2 < _num)
            {
                __instance.BroadcastMessage("SavedStolenGoods", SendMessageOptions.DontRequireReceiver);

                // Try to give the achievement for stealing if the game doesn't do it
                SteamManager steamManager = GameObject.FindObjectOfType<SteamManager>();
                var targetObject = steamManager.gameObject;

                if (targetObject != null)
                {
                    object internalComponent = targetObject.GetComponent("SteamStatsAndAchievements");

                    if (internalComponent != null)
                    {
                        MethodInfo methodInfo = internalComponent.GetType().GetMethod("ThiefAchieve", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        methodInfo?.Invoke(internalComponent, null);
                    }
                }
            }            
        }
    }

    [HarmonyPatch(typeof(CatalogueBuyButtonC), "Trigger")]
    public static class CatalogueBuyButtonC_Trigger_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(CatalogueBuyButtonC __instance)
        {
            if (__instance.transform.parent.parent.gameObject.GetComponent<HomeStorageClipboardC>() == null)
            {
                __instance.pageLogic.SetActive(true);
                LaikaCatalogueExtension.Instance.currentOpenMagazine.dropOffPoint.parent.GetComponent<LaikaBuildingC>().StartCoroutine("PartsOrdered");
            }
        }

        [HarmonyPostfix]
        public static void Postfix(CatalogueBuyButtonC __instance)
        {
            if (__instance.transform.parent.parent.gameObject.GetComponent<HomeStorageClipboardC>() == null)
            {
                LaikaCatalogueExtension.Instance.currentOpenMagazine.transform.Find("Mods").GetComponent<ModsPageToggle>().Close();
                __instance.pageLogic.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(RadioFreqLogicC), "ArrangeShuffle")]
    public static class RadioFreqLogicC_ArrangeShuffle_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(RadioFreqLogicC __instance)
        {
            bool radioAds = false;

            radioAds = JaLoaderSettings.RadioAds;
            /*Transform[] allObjects = GameObject.FindObjectsOfType<Transform>();

            foreach (Transform obj in allObjects)
            {
                if (obj.name == "JaLoader" && obj.gameObject.layer == 0 && obj.tag == "Untagged" && obj.transform.parent == null)
                {
                    var utilitiesObj = obj.transform.Find("JaLoader Utilities");
                    var component = utilitiesObj.GetComponents<MonoBehaviour>()[1];
                    radioAds = (bool)component.GetType().GetField("RadioAds", BindingFlags.Instance | BindingFlags.Public).GetValue(component);
                }
            }*/

            if (!radioAds)
            {
                __instance.gameObject.GetComponent<AudioSource>().Stop();

                __instance.songShuffle.RemoveAll(x => x == __instance.countrySpecificIndents[0] || x == __instance.countrySpecificIndents[1] || x == __instance.countrySpecificIndents[2] || x == __instance.countrySpecificIndents[3] || x == __instance.countrySpecificIndents[4] || x == __instance.countrySpecificIndents[5]);

                __instance.gameObject.GetComponent<AudioSource>().clip = __instance.songShuffle[0];
                __instance.gameObject.GetComponent<AudioSource>().Play();
            }
        }
    }

    [HarmonyPatch(typeof(MapLogicC), "Update")]
    public static class MapLogicC_Update_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(MapLogicC __instance)
        {
            if (__instance.isGlowing)
            {
                float value = Mathf.PingPong(Time.time, 0.75f) + 1.25f;
                __instance.book.GetComponent<Renderer>().material.SetFloat("_RimPower", value);
            }

            if (Input.GetMouseButtonDown(0) && __instance.isBookOpen)
            {
                UIManager.Instance.CanCloseMap = false;
                UIManager.Instance.Invoke("ResetCanCloseMap", 3.5f);
            }

            if(!UIManager.Instance.CanCloseMap)
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(DragRigidbodyC), "Update")]
    public static class DragRigidbodyC_Update_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(DragRigidbodyC __instance)
        {
            if (__instance.pickingUp && GameTweaks.Instance.ResettingPickingUp == false)
            {
                GameTweaks.Instance.ResettingPickingUp = true;
                GameTweaks.Instance.Invoke("ResetPickingUp", 0.25f);
            }
        }
    }

    [HarmonyPatch(typeof(MagazineLogicC), "PickUp")]
    public static class MagazineLogicC_PickUp_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(MagazineLogicC __instance)
        {
            LaikaCatalogueExtension.Instance.currentOpenMagazine = __instance;
        }
    }

    [HarmonyPatch(typeof(RouteGeneratorC), "SetRoadConditions")]
    public static class RouteGeneratorC_SetRoadConditions_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(RouteGeneratorC __instance)
        {
            try
            {
                string[] info = Camera.main.transform.Find("MapHolder/Location").GetComponent<TextMesh>().text.Split(' ');
                if (info.Length > 0 && info[0] != "" && info[0] != string.Empty)
                {
                    string destination = info[2];
                    string start = info[0];
                    if (destination == "M.")
                    {
                        destination = "Malko Tarnovo";
                    }
                    else if (start == "M.")
                    {
                        start = "Malko Tarnovo";
                        destination = info[3];
                    }
                    EventsManager.Instance.CallRoute(start, destination, __instance.routeChosenLength * 70);
                }
                else
                    EventsManager.Instance.CallRoute("Berlin", "Dresden", __instance.routeChosenLength * 70);
            }
            catch (Exception ex)
            {
                Console.LogError("JaLoader", "Failed to get route information: " + ex.Message);
                Console.LogError("JaLoader", "Using default route information. Please report this issue.");
                Console.LogError("JaLoader", Camera.main.transform.Find("MapHolder/Location").GetComponent<TextMesh>().text);
                EventsManager.Instance.CallRoute("Berlin", "Dresden", __instance.routeChosenLength * 70);
            }
        }
    }

    [HarmonyPatch(typeof(DialogueStuffsC), "StartTypeText")]
    public static class DialogueStuffsC_StartTypeText_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(DialogueStuffsC __instance, GameObject target1)
        {
            if (GameTweaks.Instance.isDialogueActive)
                return false;

            GameTweaks.Instance.isDialogueActive = true;
            target1.AddComponent<DialogueReceiver>();
            return true;
        }
    }

    [HarmonyPatch(typeof(ScrapYardC), "Start")]
    public static class ScrapYardC_Start_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(ScrapYardC __instance)
        {
            foreach (var objName in CustomObjectsManager.Instance.database.Keys)
            {
                var obj = CustomObjectsManager.Instance.GetObject(objName);

                if (!obj.GetComponent<EngineComponentC>())
                    continue;

                if (!obj.GetComponent<ObjectIdentification>().CanFindInJunkCars)
                    continue;

                __instance.spawnCatalogue.Add(obj);
            }
        }
    }

    [HarmonyPatch(typeof(AbandonCarC), "Start")]
    public static class AbandonCarC_Start_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(AbandonCarC __instance)
        {
            var enginesList = __instance.engineBlocks.ToList();
            var fuelTanksList = __instance.fuelTanks.ToList();
            var carburettorsList = __instance.carburettors.ToList();
            var airFiltersList = __instance.airFilters.ToList();
            var ignitionCoilsList = __instance.ignitionCoils.ToList();
            var batteriesList = __instance.batteries.ToList();
            var waterTanksList = __instance.waterTanks.ToList();

            foreach (var objName in CustomObjectsManager.Instance.database.Keys)
            {
                var obj = CustomObjectsManager.Instance.GetObject(objName);

                if (!obj.GetComponent<EngineComponentC>())
                    continue;

                if (!obj.GetComponent<ObjectIdentification>().CanFindInJunkCars)
                    continue;

                switch (obj.GetComponent<ObjectPickupC>().engineString)
                {
                    case "EngineBlock":
                        enginesList.Add(obj);
                        break;

                    case "FuelTank":
                        fuelTanksList.Add(obj);
                        break;

                    case "Carburettor":
                        carburettorsList.Add(obj);
                        break;

                    case "AirFilter":
                        airFiltersList.Add(obj);
                        break;

                    case "IgnitionCoil":
                        ignitionCoilsList.Add(obj);
                        break;

                    case "Battery":
                        batteriesList.Add(obj);
                        break;

                    case "WaterContainer":
                        waterTanksList.Add(obj);
                        break;
                }
            }

            __instance.engineBlocks = enginesList.ToArray();
            __instance.fuelTanks = fuelTanksList.ToArray();
            __instance.carburettors = carburettorsList.ToArray();
            __instance.airFilters = airFiltersList.ToArray();
            __instance.ignitionCoils = ignitionCoilsList.ToArray();
            __instance.batteries = batteriesList.ToArray();
            __instance.waterTanks = waterTanksList.ToArray();
        }
    }

    [HarmonyPatch(typeof(Language), "Get", new Type[] { typeof(string), typeof(string) })]
    public static class Language_Get_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(string key, string sheetTitle, ref string __result)
        {
            if (key.StartsWith("MOD_"))
            {
                __result = key.Substring(4);

                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(DialogueStuffsC), "StopTypeText")]
    public static class DialogueStuffsC_StopTypeText_Patch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            GameTweaks.Instance.isDialogueActive = false;

            if (GameObject.FindObjectOfType<DialogueReceiver>() != null)
                GameObject.FindObjectOfType<DialogueReceiver>().TextFinished();
        }
    }

    [HarmonyPatch(typeof(SpawnContinueC), "SpawnAICars")]
    public static class SpawnContinueC_SpawnAICars_Patch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            foreach(GameObject car in GameObject.FindGameObjectsWithTag("AICar"))
            {
                var transform = car.transform;

                // if localEulerAngles.z > 60, then the car is most likely flipped, so rotate it to 0
                if (transform.localEulerAngles.z > 60)
                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
            }

            CoroutineManager.StartStaticCoroutine(CheckCarsAfter10Seconds());
        }

        public static IEnumerator CheckCarsAfter10Seconds()
        {
            yield return new WaitForSeconds(10);

            foreach (GameObject car in GameObject.FindGameObjectsWithTag("AICar"))
            {
                var transform = car.transform;

                // if localEulerAngles.z > 60, then the car is most likely flipped, so rotate it to 0
                if (transform.localEulerAngles.z > 60)
                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
            }
        }
    }
}
