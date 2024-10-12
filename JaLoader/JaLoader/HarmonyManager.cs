using HarmonyLib;
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
        public static void Prefix(MainMenuC __instance)
        {
            __instance.BroadcastMessage("Slept", SendMessageOptions.DontRequireReceiver);
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

            Transform[] allObjects = GameObject.FindObjectsOfType<Transform>();

            foreach (Transform obj in allObjects)
            {
                if (obj.name == "JaLoader" && obj.gameObject.layer == 0 && obj.tag == "Untagged" && obj.transform.parent == null)
                {
                    var component = obj.GetComponents<MonoBehaviour>()[1];
                    radioAds = (bool)component.GetType().GetField("RadioAds", BindingFlags.Instance | BindingFlags.Public).GetValue(component);
                }
            }

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
}
