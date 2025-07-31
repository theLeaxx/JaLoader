using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JaLoader
{
    public class GameTweaks : MonoBehaviour
    {
        #region Singleton
        public static GameTweaks Instance { get; private set; }

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

            EventsManager.Instance.OnGameLoad += OnGameLoad;
            EventsManager.Instance.OnRouteGenerated += OnRouteGenerated;
            EventsManager.Instance.OnSleep += OnSleep;
        }

        #endregion

        public bool ResettingPickingUp = false;

        private Texture2D defaultCursorTexture;
        private Texture2D emptyTexture;
        private VfCursorManager cursorManager;
        private VfAnimCursor cursorToChange;

        private bool skippedIntro = false;

        public bool isDialogueActive = false;
        private void OnSleep()
        {
            ResetBeds();
        }

        private void OnGameLoad()
        {
            StartCoroutine(OnGameLoadDelay());

            if (SettingsManager.UseExperimentalCharacterController)
                GameObject.Find("First Person Controller").AddComponent<EnhancedMovement>();

            GameObject.Find("UI Root").transform.Find("UncleStuff").gameObject.AddComponent<EnableCursorOnEnable>();
        }

        internal void SkipLanguage()
        {
            if (SettingsManager.SkipLanguage && !skippedIntro)
            {
                skippedIntro = true;
                SettingsManager.selectedLanguage = true;
                SceneManager.LoadScene("MainMenu");
            }
        }

        private void ResetBeds()
        {
            var beds = FindObjectsOfType<BedLogicC>();

            foreach (var bed in beds)
                bed.GetType().GetField("block", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(bed, true);
        }

        private IEnumerator OnGameLoadDelay()
        {
            ResetBeds();
            
            cursorManager = FindObjectOfType<VfCursorManager>();
            cursorToChange = cursorManager.Cursors[2].GetComponent<VfAnimCursor>();

            defaultCursorTexture = cursorToChange.FrameTextures[0];
            emptyTexture = new Texture2D(1, 1, TextureFormat.Alpha8, false);
            emptyTexture.SetPixel(0, 0, Color.clear);

            ChangeCursor(SettingsManager.CursorMode);

            yield return new WaitForSeconds(3);
            UpdateMirrors();
            FixBorderFlags();

            if (SettingsManager.FixLaikaShopMusic)
            {
                yield return new WaitForSeconds(2);
                FixLaikaDealershipSong();
            }

            if (SettingsManager.FixBorderGuardsFlags)
            {
                yield return new WaitForSeconds(2);
                FixBorderGuardsFlags();
            }

            yield return null;
        }

        public void ResetPickingUp()
        {
            ResettingPickingUp = false;
            FindObjectOfType<DragRigidbodyC>().pickingUp = false;
        }

        private void FixBorderFlags()
        {
            var hubs = FindObjectsOfType<Hub_CitySpawnC>();

            foreach (var hub in hubs)
            {
                switch (hub.countryHUBCode)
                {
                    case 1: // Germany

                        // find all objects in hub children called "Hungary_Flag"

                        var flags = hub.GetComponentsInChildren<Transform>().Where(t => t.name == "Hungary_Flag").ToList();

                        flags[2].localPosition = new Vector3(7.3742f, 13, -335.6782f);
                        flags[3].localPosition = new Vector3(-8.9649f, 13, -334.6782f);

                        break;
                }
            }
        }

        private void OnRouteGenerated(string startLocation, string endLocation, int distance)
        {
            FixBorderFlags();

            if (SettingsManager.FixLaikaShopMusic)
                Invoke("FixLaikaDealershipSong", 5);

            if (SettingsManager.FixBorderGuardsFlags)
                Invoke("FixBorderGuardsFlags", 5);

            SceneManager.GetActiveScene().GetRootGameObjects().ToList().ForEach(go =>
            {
                if (go.transform.position == Vector3.zero)
                {
                    var listOfNames = new List<string> {"EngineBlock", "FuelTank", "Carburettor", "AirFilter", "IgnitionCoil", "Battery", "WaterContainer"};

                    foreach (var name in listOfNames)
                        if (go.name == name)
                            go.SetActive(false);
                }
            });
        }

        private void FixLaikaDealershipSong()
        {
            var laikabuildings = FindObjectsOfType<LaikaBuildingC>();

            if(laikabuildings == null)
                return;

            foreach (var laikabuilding in laikabuildings)
            {
                laikabuilding.transform.Find("speakers_shop_01").GetComponent<AudioSource>().loop = true;

                if(!laikabuilding.transform.Find("speakers_shop_01").GetComponent<AudioSource>().isPlaying)
                    laikabuilding.transform.Find("speakers_shop_01").GetComponent<AudioSource>().Play();
            }
        }

        public void ChangeCursor(CursorMode cursorMode)
        {
            if (cursorManager == null)
                return;

            cursorManager.SetCursor(0);
            cursorToChange.CursorOff();

            switch (cursorMode)
            {
                case CursorMode.Default:
                    cursorToChange.FrameTextures[0] = cursorToChange.FrameTexturesStorage[0] = defaultCursorTexture;
                    cursorToChange.HotSpot = new Vector2(16, 16);
                    break;

                case CursorMode.Circle:
                    cursorToChange.FrameTextures[0] = cursorToChange.FrameTexturesStorage[0] = UIManager.Instance.knobTexture;
                    cursorToChange.HotSpot = new Vector2(0, 0);
                    break;

                case CursorMode.Hidden:
                    cursorToChange.FrameTextures[0] = cursorToChange.FrameTexturesStorage[0] = emptyTexture;
                    cursorToChange.HotSpot = new Vector2(0, 0);
                    break;
            }

            cursorToChange.CursorOn();
            cursorManager.SetCursor(2);
        }

        public void UpdateMirrors()
        {
            UpdateMirrors(SettingsManager.MirrorDistances);
        }

        public void UpdateMirrors(MirrorDistances value)
        {
            var mirrors = FindObjectsOfType<MirrorReflection>();

            if(mirrors == null)
                return;

            int distance = 0;

            switch(value)
            {
                case MirrorDistances.m250:
                    distance = 250;
                    break;

                case MirrorDistances.m500:
                    distance = 500;
                    break;

                case MirrorDistances.m750:
                    distance = 750;
                    break;

                case MirrorDistances.m1000:
                    distance = 1000;
                    break;

                case MirrorDistances.m1500:
                    distance = 1500;
                    break;

                case MirrorDistances.m2000:
                    distance = 2000;
                    break;

                case MirrorDistances.m2500:
                    distance = 2500;
                    break;

                case MirrorDistances.m3000:
                    distance = 3000;
                    break;

                case MirrorDistances.m3500:
                    distance = 3500;
                    break;

                case MirrorDistances.m4000:
                    distance = 4000;
                    break;

                case MirrorDistances.m5000:
                    distance = 5000;
                    break;
            }

            foreach (MirrorReflection mirror in mirrors)
                mirror.m_FarClipDistance = distance;
        }

        public void FixBorderGuardsFlags()
        {
            var hubs = FindObjectsOfType<Hub_CitySpawnC>();

            foreach (var hub in hubs)
            {
                List<SkinnedMeshRenderer> guardsList1 = new List<SkinnedMeshRenderer>();
                List<SkinnedMeshRenderer> guardsList2 = new List<SkinnedMeshRenderer>();
                Transform borderReturn;
                Transform border;

                switch (hub.countryHUBCode)
                {
                    case 1: // Germany
                        borderReturn = hub.transform.Find("BorderLogicReturn");

                        guardsList1.Add(borderReturn.Find("BorderGuard/Dantes_Body_007").GetComponent<SkinnedMeshRenderer>());
                        guardsList1.Add(borderReturn.Find("CzechBorderNPC_01/Dantes_Body_007").GetComponent<SkinnedMeshRenderer>());

                        foreach (var guard in guardsList1)
                            guard.materials = NewMaterialsArray(guard.materials, Country.Germany);

                        break;

                    case 2: // Czechoslovakia
                        border = hub.transform.Find("BorderLogic");
                        borderReturn = hub.transform.Find("BorderLogicReturn");

                        guardsList1.Add(border.Find("BorderGuard/Dantes_Body_007").GetComponent<SkinnedMeshRenderer>());
                        guardsList1.Add(hub.transform.Find("CzechBorderNPC_01/Dantes_Body_007").GetComponent<SkinnedMeshRenderer>());

                        foreach (var guard in guardsList1)
                            guard.materials = NewMaterialsArray(guard.materials, Country.Hungary);

                        break;

                    case 3: // Hungary
                        border = hub.transform.Find("GameObject/BorderLogic");
                        borderReturn = hub.transform.Find("GameObject/BorderLogicReturn");

                        guardsList1.Add(border.Find("BorderGuard/Dantes_Body_007").GetComponent<SkinnedMeshRenderer>());
                        guardsList1.Add(hub.transform.Find("CzechBorderNPC_01/Dantes_Body_007").GetComponent<SkinnedMeshRenderer>());

                        guardsList2.Add(borderReturn.Find("BorderGuard/Dantes_Body_007").GetComponent<SkinnedMeshRenderer>());
                        guardsList2.Add(borderReturn.Find("CzechBorderNPC_01/Dantes_Body_007").GetComponent<SkinnedMeshRenderer>());

                        foreach (var guard in guardsList1)
                            guard.materials = NewMaterialsArray(guard.materials, Country.Yugoslavia);

                        foreach (var guard in guardsList2)
                            guard.materials = NewMaterialsArray(guard.materials, Country.Hungary);

                        break;

                    case 4: // Yugoslavia
                        border = hub.transform.Find("BorderLogic");
                        borderReturn = hub.transform.Find("BorderLogicReturn");

                        guardsList1.Add(border.Find("BorderGuard/Dantes_Body_007").GetComponent<SkinnedMeshRenderer>());
                        guardsList1.Add(hub.transform.Find("CzechBorderNPC_01/Dantes_Body_007").GetComponent<SkinnedMeshRenderer>());

                        guardsList2.Add(borderReturn.Find("BorderGuard/Dantes_Body_007").GetComponent<SkinnedMeshRenderer>());
                        guardsList2.Add(borderReturn.Find("CzechBorderNPC_01/Dantes_Body_007").GetComponent<SkinnedMeshRenderer>());

                        foreach (var guard in guardsList1)
                            guard.materials = NewMaterialsArray(guard.materials, Country.Bulgaria);

                        foreach (var guard in guardsList2)
                            guard.materials = NewMaterialsArray(guard.materials, Country.Yugoslavia);

                        break;

                    case 5: // Bulgaria
                        border = hub.transform.Find("Border");
                        borderReturn = hub.transform.Find("BorderLogicReturn");

                        guardsList1.Add(border.Find("BorderLogic/BorderGuard/Dantes_Body_007").GetComponent<SkinnedMeshRenderer>());
                        guardsList1.Add(border.Find("CzechBorderNPC_01/Dantes_Body_007").GetComponent<SkinnedMeshRenderer>());

                        guardsList2.Add(borderReturn.Find("BorderGuard/Dantes_Body_007").GetComponent<SkinnedMeshRenderer>());
                        guardsList2.Add(borderReturn.Find("CzechBorderNPC_01/Dantes_Body_007").GetComponent<SkinnedMeshRenderer>());

                        foreach (var guard in guardsList1)
                            guard.materials = NewMaterialsArray(guard.materials, Country.Turkey);

                        foreach (var guard in guardsList2)
                            guard.materials = NewMaterialsArray(guard.materials, Country.Bulgaria);

                        break;
                }
            }
        }

        private Material OffsetFlagsMaterial(Material material, Country country)
        {
            var newMat = new Material(material);

            switch (country)
            {
                case Country.Germany:
                    newMat.mainTextureOffset = new Vector2(0, -0.21f);
                    break;

                case Country.Hungary:
                    newMat.mainTextureOffset = new Vector2(0.34f, -0.45f);
                    break;

                case Country.Yugoslavia:
                    newMat.mainTextureOffset = new Vector2(0.34f, -0.66f);
                    break;

                case Country.Bulgaria:
                    newMat.mainTextureOffset = new Vector2(0, -0.63f);
                    break;

                case Country.Turkey:
                    newMat.mainTextureOffset = new Vector2(0, -0.42f);
                    break;
            }

            return newMat;
        }

        private Material[] NewMaterialsArray(Material[] materials, Country country)
        {
            var mats = materials;

            mats[1] = OffsetFlagsMaterial(mats[1], country);

            return mats;
        }
    }

    public class DialogueReceiver : MonoBehaviour
    {
        public void TextFinished()
        {
            GameTweaks.Instance.isDialogueActive = false;

            Destroy(this);
        }
    }
}
