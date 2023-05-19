using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

namespace JaLoader
{
    public class LaikaCatalogueExtension : MonoBehaviour
    {
        public static LaikaCatalogueExtension Instance { get; private set; }

        #region Singleton & OnSceneChange
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
            //SceneManager.sceneLoaded += OnSceneChange;
        }
        #endregion

        public void AddModsPage()
        {
            var magazines = FindObjectsOfType<MagazineLogicC>();

            foreach (var magazine in magazines)
            {
                if (!magazine.transform.Find("ModPages"))
                {
                    var receipt = magazine.transform.Find("Page2_EngineParts").Find("Receipt");
                    receipt.parent = magazine.transform;
                    receipt.gameObject.SetActive(false);
                    var template = Instantiate(magazine.transform.Find("Page2_EngineParts"), magazine.transform);
                    template.name = "ModPages";
                    SkinnedMeshRenderer skin = template.transform.Find("Cube_985").GetComponent<SkinnedMeshRenderer>();
                    var buttonTemplate = Instantiate(template.Find("Extras_2"), magazine.transform);
                    buttonTemplate.name = "Mods";
                    buttonTemplate.transform.localPosition = new Vector3(buttonTemplate.transform.localPosition.x, buttonTemplate.transform.localPosition.y - 0.015f, buttonTemplate.transform.localPosition.z);
                    var extension = buttonTemplate.gameObject.AddComponent<MagazineCatalogueRelayC_ModExtension>();
                    extension.scale = buttonTemplate.gameObject.GetComponent<MagazineCatalogueRelayC>().scale;
                    Destroy(buttonTemplate.gameObject.GetComponent<MagazineCatalogueRelayC>());
                    extension.magazineParent = magazine;
                    extension.modPage = template.gameObject;
                    extension.modPageBG = magazine.transform.Find("Page4").gameObject;
                    extension.defaultPage = magazine.transform.Find("Page2_EngineParts").gameObject;
                    extension.receipt = receipt.gameObject;

                    Destroy(magazine.transform.Find("Page4").GetChild(2).gameObject);
                    magazine.transform.Find("Page4").GetChild(1).GetComponent<SkinnedMeshRenderer>().material = skin.material;
                    buttonTemplate.GetComponent<TextMeshPro>().text = "";
                }
            }
        }

        private void ResetSkinnedMeshRenderer(Transform rootBone, SkinnedMeshRenderer skin)
        {
            Transform[] newBones = new Transform[skin.bones.Length];
            for (int i = 0; i < skin.bones.Length; i++)
            {
                Console.Instance.Log(skin.bones[i].name);

                foreach (var newBone in rootBone.GetComponentsInChildren<Transform>())
                {
                    if (newBone.name == skin.bones[i].name)
                    {
                        newBones[i] = newBone;
                        continue;
                    }
                }
            }
            Console.Instance.Log(newBones.Length);
            skin.bones = newBones;
            skin.sharedMesh.RecalculateBounds();
        }
    }

    public class MagazineCatalogueRelayC_ModExtension : MonoBehaviour
    {
        public Vector3[] scale;

        private bool visible;

        private bool showingMods;

        public MagazineLogicC magazineParent;
        public GameObject modPage;
        public GameObject modPageBG;
        public GameObject defaultPage;
        public GameObject receipt;

        public void Trigger()
        {
            defaultPage.SetActive(false);
            modPage.SetActive(true);
            modPageBG.SetActive(true);
            showingMods = true;
        }

        void Update()
        {
            if (magazineParent.isBookOpen)
            {
                receipt.SetActive(true);

                if (!visible)
                {
                    GetComponent<BoxCollider>().enabled = false;
                    GetComponent<TextMeshPro>().text = "";
                }
                else
                {
                    GetComponent<BoxCollider>().enabled = true;
                    GetComponent<TextMeshPro>().text = ".......................MODS";
                }

                if (defaultPage.activeSelf)
                {
                    visible = true;
                    modPage.SetActive(false);
                    modPageBG.SetActive(false);
                    showingMods = false;
                }
                else if (showingMods)
                    visible = true;
                else
                {
                    receipt.SetActive(false);
                    visible = false;
                }
            }

            if (showingMods && Input.GetKeyDown(KeyCode.Escape))
            {
                receipt.SetActive(false);
                visible = false;
                showingMods = false;
                modPage.SetActive(false);
                modPageBG.SetActive(false);

                defaultPage.SetActive(true);
                defaultPage.GetComponent<EngineComponentsCataloguePageC>().PageClose();
                StartCoroutine(WaitFrameThenHide());
            }
            else if (defaultPage.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            {
                receipt.SetActive(false);
            }
        }

        public void RaycastEnter()
        {
            transform.localScale = scale[1];
        }

        public void RaycastExit()
        {
            transform.localScale = scale[0];
        }

        IEnumerator WaitFrameThenHide()
        {
            yield return new WaitForEndOfFrame();
            defaultPage.SetActive(false);
        }
    }
}
