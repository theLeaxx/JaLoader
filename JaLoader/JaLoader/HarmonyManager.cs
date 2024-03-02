using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace JaLoader
{
    public class HarmonyManager : MonoBehaviour
    {
        #region Singleton
        public static HarmonyManager Instance { get; private set; }
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
        }
        #endregion

        private static Harmony harmony;

        public void PatchAll()
        {
            if (harmony == null)
            {
                harmony = new Harmony("JaLoader.Leaxx");
                harmony.PatchAll();
            }
        }

        private void Start()
        {
            PatchAll();
        }

        public void PatchBuyButton()
        {
             if (harmony == null)
             {
                harmony = new Harmony("JaLoader.Leaxx");
                harmony.Patch(typeof(CatalogueBuyButtonC).GetMethod("Trigger"), prefix: new HarmonyMethod(typeof(CatalogueBuyButtonC_Trigger_Patch).GetMethod("Prefix")) ,postfix: new HarmonyMethod(typeof(CatalogueBuyButtonC_Trigger_Patch).GetMethod("Postfix")));
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

    [HarmonyPatch(typeof(MainMenuC), "SaveInventory")]
    public static class MainMenuC_SaveInventory_Patch
    {
        [HarmonyPostfix]
        public static void PostFix()
        {
            GameObject.FindObjectOfType<MainMenuC>().BroadcastMessage("SavedGame", SendMessageOptions.DontRequireReceiver);
        }
    }

    [HarmonyPatch(typeof(MainMenuC), "LateStart")]
    public static class MainMenuC_LateStart_Patch
    {
        [HarmonyPostfix]
        public static void PostFix()
        {
            GameObject.FindObjectOfType<MainMenuC>().BroadcastMessage("LoadedGame", SendMessageOptions.DontRequireReceiver);
        }
    }

    [HarmonyPatch(typeof(BedLogicC), "Later")]
    public static class BedLogicC_Later_Patch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            GameObject.FindObjectOfType<MainMenuC>().BroadcastMessage("Slept", SendMessageOptions.DontRequireReceiver);
        }
    }

    [HarmonyPatch(typeof(MainMenuC), "Pause")]
    public static class MainMenuC_Pause_Patch
    {
        [HarmonyPrefix]
        public static void PreFix(MainMenuC __instance)
        {  
            if (__instance.hasWokenUp)
            {
                GameObject.FindObjectOfType<MainMenuC>().BroadcastMessage("PausedGame", SendMessageOptions.DontRequireReceiver);
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
                GameObject.FindObjectOfType<MainMenuC>().BroadcastMessage("UnPausedGame", SendMessageOptions.DontRequireReceiver);
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
                GameObject.FindObjectOfType<MainMenuC>().BroadcastMessage("PausedGame", SendMessageOptions.DontRequireReceiver);
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
        public static void PostFix()
        {
            GameObject.FindObjectOfType<MainMenuC>().BroadcastMessage("SavedStolenGoods", SendMessageOptions.DontRequireReceiver);

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

    [HarmonyPatch(typeof(CatalogueBuyButtonC), "Trigger")]
    public static class CatalogueBuyButtonC_Trigger_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(CatalogueBuyButtonC __instance)
        {
            //__instance.gameObject.AddComponent<PatchedBuyButton>();
            __instance.pageLogic.SetActive(true);
        }

        [HarmonyPostfix]
        public static void Postfix(CatalogueBuyButtonC __instance)
        {
            //__instance.gameObject.AddComponent<PatchedBuyButton>();
            __instance.pageLogic.SetActive(false);
        }
    }
}
