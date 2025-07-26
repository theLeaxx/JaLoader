using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;
using Image = UnityEngine.UI.Image;
using UnityEngine.EventSystems;
using System;
using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel;
using System.Linq;

namespace JaLoader
{
    public class LaikaCatalogueExtension : MonoBehaviour
    {
        public static LaikaCatalogueExtension Instance { get; private set; }

        private bool setPriceImage;
        private bool addedAllComponents;
        private bool addedScrollViews;

        private readonly Dictionary<PartTypes, GameObject> scrollViews = new Dictionary<PartTypes, GameObject>();
        private readonly Dictionary<(Transform, PartTypes), GameObject> buttons = new Dictionary<(Transform, PartTypes), GameObject>();

        public MagazineLogicC currentOpenMagazine = null;

        #region Singleton & OnRouteGenerated
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

            EventsManager.Instance.OnRouteGenerated += AddPages;
        }
        #endregion

        public void AddPages(string start, string destination, int distance)
        {
            StartCoroutine(DelayThenAddPages());
        }

        private IEnumerator DelayThenAddPages()
        {
            yield return new WaitForSeconds(2f);

            AddModsPages();
        }

        private void AddModsPages()
        {
            var magazines = FindObjectsOfType<MagazineLogicC>().Where(m => m.name == "LaikaCatalogue").ToArray();

            if (magazines == null || magazines.Length == 0)
                return;

            foreach (var magazine in magazines)
            {
                if (!magazine.transform.Find("ModPages"))
                {
                    var receipt = magazine.transform.Find("Page2_EngineParts").Find("Receipt").gameObject;
                    receipt.transform.parent = magazine.transform;
                    receipt.SetActive(false);

                    var template = Instantiate(magazine.transform.Find("Page2_EngineParts"), magazine.transform);
                    template.name = "ModPages";

                    var buttonTemplate = Instantiate(template.Find("Extras_2"), magazine.transform);
                    buttonTemplate.name = "Mods";
                    buttonTemplate.transform.localPosition = new Vector3(buttonTemplate.transform.localPosition.x, buttonTemplate.transform.localPosition.y - 0.015f, buttonTemplate.transform.localPosition.z);
                    buttonTemplate.GetComponent<TextMeshPro>().text = "";

                    template.Find("Catalogue").GetComponent<TextMeshPro>().text = "MODS";

                    for (int i = 2; i <= 4; i++)
                    {
                        Destroy(template.Find($"Engine_{i}").gameObject);
                        Destroy(template.Find($"FUELTANKS_{i}").gameObject);
                        Destroy(template.Find($"CARBURETTOR_{i}").gameObject);
                        Destroy(template.Find($"AIRFILTERS_{i}").gameObject);
                        Destroy(template.Find($"IGNITIONCOIL_{i}").gameObject);
                    }

                    for (int i = 1; i <= 7; i++)
                        Destroy(template.Find($"Icon{i}").gameObject);

                    foreach (var price in template.GetComponentsInChildren<Transform>())
                    {
                        if (price.name == "price1")
                        {
                            if (!setPriceImage)
                            {
                                UIManager.Instance.catalogueEntryTemplate.transform.GetChild(3).GetComponent<Image>().sprite = price.GetChild(0).GetComponent<SpriteRenderer>().sprite;
                                setPriceImage = true;
                            }
                            Destroy(price.gameObject);
                        }

                        if (price.name == "backdrop")
                            Destroy(price.gameObject);
                    }

                    setPriceImage = false;

                    var desc = template.Find("Stats/ComponentDesc");
                    desc.parent = template;
                    desc.GetComponent<TextMeshPro>().text = "Various Laika-Certified modifications.";
                    Destroy(template.Find("Stats").gameObject);

                    Destroy(template.Find("Extras_2").gameObject);

                    var extrasTitle = Instantiate(template.Find("Extras"), template.transform);
                    extrasTitle.name = "Custom";
                    extrasTitle.GetComponent<TextMeshPro>().text = "CUSTOM";

                    AddButtons(template);

                    if (!addedScrollViews)
                        StartCoroutine(CreateScrollViews());

                    template.Find("Engine").localPosition = new Vector3(-0.05f, 0.2f, 0.0035f);
                    template.Find("FUEL TANKS").transform.localPosition = new Vector3(-0.05f, 0.17f, 0.0035f);
                    template.Find("CARBURETTORS").transform.localPosition = new Vector3(-0.05f, 0.14f, 0.0035f);
                    template.Find("AIR FILTERS").transform.localPosition = new Vector3(-0.05f, 0.11f, 0.0035f);
                    template.Find("IGNITIONCOILS").transform.localPosition = new Vector3(-0.05f, 0.08f, 0.0035f);
                    template.Find("BATTERY").transform.localPosition = new Vector3(-0.05f, 0.05f, 0.0035f);
                    template.Find("WATERTANKS").transform.localPosition = new Vector3(-0.05f, 0.02f, 0.0035f);
                    template.Find("Extras").transform.localPosition = new Vector3(-0.05f, -0.01f, 0.0035f);
                    template.Find("Custom").transform.localPosition = new Vector3(-0.05f, -0.04f, 0.0035f);

                    var extension = buttonTemplate.gameObject.AddComponent<ModsPageToggle>();
                    extension.scale = buttonTemplate.gameObject.GetComponent<MagazineCatalogueRelayC>().scale;

                    Destroy(buttonTemplate.gameObject.GetComponent<MagazineCatalogueRelayC>());

                    Destroy(magazine.transform.Find("Page4").GetChild(2).gameObject);

                    magazine.transform.Find("Page4").GetChild(1).GetComponent<SkinnedMeshRenderer>().material = template.transform.Find("Cube_985").GetComponent<SkinnedMeshRenderer>().material;
                }
            }
        }

        private void AddButtons(Transform template)
        {
            var text = ".................VARIOUS";

            buttons.Add((template, PartTypes.Engine), template.Find("Engine_1").gameObject);
            buttons.Add((template, PartTypes.FuelTank), template.Find("FUELTANKS_1").gameObject);
            buttons.Add((template, PartTypes.Carburettor), template.Find("CARBURETTOR_1").gameObject);
            buttons.Add((template, PartTypes.AirFilter), template.Find("AIR FILTERS_1").gameObject);
            buttons.Add((template, PartTypes.IgnitionCoil), template.Find("IGNITIONCOIL_1").gameObject);
            buttons.Add((template, PartTypes.Battery), template.Find("BATTERY_1").gameObject);
            buttons.Add((template, PartTypes.WaterTank), template.Find("WATERTANKS_1").gameObject);
            buttons.Add((template, PartTypes.Extra), template.Find("Extras_1").gameObject);
            buttons.Add((template, PartTypes.Custom), Instantiate(buttons[(template, PartTypes.Extra)], template.transform).gameObject);

            foreach (var entry in buttons)
            {
                if (entry.Key.Item1 != template)
                    continue;

                var button = entry.Value;

                button.GetComponent<TextMeshPro>().text = text;
                button.AddComponent<ModsPageNavigator>();
                button.GetComponent<ModsPageNavigator>().scale = button.GetComponent<MagazineCatalogueRelayC>().scale;
                Destroy(button.GetComponent<MagazineCatalogueRelayC>());
            }

            buttons[(template, PartTypes.Engine)].GetComponent<ModsPageNavigator>().partType = PartTypes.Engine;
            buttons[(template, PartTypes.FuelTank)].GetComponent<ModsPageNavigator>().partType = PartTypes.FuelTank;
            buttons[(template, PartTypes.Carburettor)].GetComponent<ModsPageNavigator>().partType = PartTypes.Carburettor;
            buttons[(template, PartTypes.AirFilter)].GetComponent<ModsPageNavigator>().partType = PartTypes.AirFilter;
            buttons[(template, PartTypes.IgnitionCoil)].GetComponent<ModsPageNavigator>().partType = PartTypes.IgnitionCoil;
            buttons[(template, PartTypes.Battery)].GetComponent<ModsPageNavigator>().partType = PartTypes.Battery;
            buttons[(template, PartTypes.WaterTank)].GetComponent<ModsPageNavigator>().partType = PartTypes.WaterTank;
            buttons[(template, PartTypes.Extra)].GetComponent<ModsPageNavigator>().partType = PartTypes.Extra;
            buttons[(template, PartTypes.Custom)].GetComponent<ModsPageNavigator>().partType = PartTypes.Custom;

            buttons[(template, PartTypes.Custom)].name = "Custom_1";

            // y - 0.015f each time
            // simulate vertical layout group
            buttons[(template, PartTypes.Engine)].transform.localPosition = new Vector3(-0.1325f, 0.185f, 0.0035f);
            buttons[(template, PartTypes.FuelTank)].transform.localPosition = new Vector3(-0.1325f, 0.155f, 0.0035f);
            buttons[(template, PartTypes.Carburettor)].transform.localPosition = new Vector3(-0.1325f, 0.125f, 0.0035f);
            buttons[(template, PartTypes.AirFilter)].transform.localPosition = new Vector3(-0.1325f, 0.095f, 0.0035f);
            buttons[(template, PartTypes.IgnitionCoil)].transform.localPosition = new Vector3(-0.1325f, 0.065f, 0.0035f);
            buttons[(template, PartTypes.Battery)].transform.localPosition = new Vector3(-0.1325f, 0.035f, 0.0035f);
            buttons[(template, PartTypes.WaterTank)].transform.localPosition = new Vector3(-0.1325f, 0.005f, 0.0035f);
            buttons[(template, PartTypes.Extra)].transform.localPosition = new Vector3(-0.1325f, -0.025f, 0.0035f);
            buttons[(template, PartTypes.Custom)].transform.localPosition = new Vector3(-0.1325f, -0.055f, 0.0035f);
        }

        private IEnumerator CreateScrollViews()
        {
            UIManager.Instance.catalogueEntryTemplate.transform.GetChild(2).gameObject.AddComponent<ModsPageItem>();

            scrollViews.Add(PartTypes.Engine, Instantiate(UIManager.Instance.catalogueTemplate));
            scrollViews.Add(PartTypes.FuelTank, Instantiate(UIManager.Instance.catalogueTemplate));
            scrollViews.Add(PartTypes.Carburettor, Instantiate(UIManager.Instance.catalogueTemplate));
            scrollViews.Add(PartTypes.AirFilter, Instantiate(UIManager.Instance.catalogueTemplate));
            scrollViews.Add(PartTypes.IgnitionCoil, Instantiate(UIManager.Instance.catalogueTemplate));
            scrollViews.Add(PartTypes.Battery, Instantiate(UIManager.Instance.catalogueTemplate));
            scrollViews.Add(PartTypes.WaterTank, Instantiate(UIManager.Instance.catalogueTemplate));
            scrollViews.Add(PartTypes.Extra, Instantiate(UIManager.Instance.catalogueTemplate));
            scrollViews.Add(PartTypes.Custom, Instantiate(UIManager.Instance.catalogueTemplate));

            scrollViews[PartTypes.Engine].name = "EnginesScroll";
            scrollViews[PartTypes.FuelTank].name = "FuelTanksScroll";
            scrollViews[PartTypes.Carburettor].name = "CarburettorsScroll";
            scrollViews[PartTypes.AirFilter].name = "AirFiltersScroll";
            scrollViews[PartTypes.IgnitionCoil].name = "IgnitionCoilsScroll";
            scrollViews[PartTypes.Battery].name = "BatteriesScroll";
            scrollViews[PartTypes.WaterTank].name = "WaterTanksScroll";
            scrollViews[PartTypes.Extra].name = "ExtrasScroll";
            scrollViews[PartTypes.Custom].name = "CustomsScroll";

            foreach (var view in scrollViews.Values)
            {
                view.transform.SetParent(UIManager.Instance.catalogueTemplate.transform.parent, false);
                view.transform.position = UIManager.Instance.catalogueTemplate.transform.position;
            }

            StartCoroutine(AddAllEntries());

            while (!addedAllComponents)
                yield return null;

            foreach (var page in scrollViews.Values)
            {
                if (page.transform.Find("Viewport/Content").childCount <= 1)
                {
                    page.transform.Find("NoneFound").gameObject.SetActive(true);
                }
            }

            addedScrollViews = true;

            yield return null;
        }

        private void AddEntry(PartTypes partType, string title, string description, Texture2D image, string price, string registryName, bool useSquareImage = false)
        {
            var entry = Instantiate(UIManager.Instance.catalogueEntryTemplate, scrollViews[partType].transform.Find("Viewport/Content")).gameObject;
            entry.transform.GetChild(0).GetComponent<Text>().text = title;
            entry.transform.GetChild(1).GetComponent<Text>().text = description;
            entry.transform.GetChild(2).GetComponent<RawImage>().texture = image;
            entry.transform.GetChild(4).GetComponent<Text>().text = price;
            entry.transform.GetChild(2).GetComponent<ModsPageItem>().itemName = registryName;
            entry.SetActive(true);

            if(useSquareImage == true)
                entry.transform.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(75, 75);

            StartCoroutine(RefreshPage(partType));
        }

        private IEnumerator AddAllEntries()
        {
            CustomObjectsManager manager = CustomObjectsManager.Instance;

            foreach (var objName in manager.database.Keys)
            {
                var obj = manager.GetObject(objName);

                if (obj.GetComponent<EngineComponentC>())
                {
                    PartTypes type = PartTypes.Engine;

                    switch (obj.GetComponent<ObjectPickupC>().engineString)
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

                    if (obj.GetComponent<ExtraComponentC_ModExtension>())
                    {
                        type = PartTypes.Extra;
                    }

                    var identif = obj.GetComponent<ObjectIdentification>();
                    var objInfo = obj.GetComponent<CustomObjectInfo>();
                    var mod = ModManager.FindMod(identif.Author, identif.ModID, identif.ModName);

                    if(!ModManager.Mods[mod].IsEnabled)
                        continue;

                    Texture2D tex = null;
                    bool useSquareImage = false;
                    if (type == PartTypes.Extra)
                    {
                        var comp = obj.GetComponent<ExtraComponentC_ModExtension>();
                        if(comp.ID == -2)
                        {
                            var pj = PaintJobManager.Instance.GetPaintJobByMaterial(comp.material);
                            tex = pj.PreviewIcon;
                            useSquareImage = true;
                        }
                    }

                    if(tex == null)
                        tex = PartIconManager.Instance.GetTexture($"{identif.ModID}_{objName}");

                    if((tex.width == tex.height) && tex.width == 128)
                        useSquareImage = true;

                    AddEntry(type, objInfo.objName, objInfo.objDescription, tex, obj.GetComponent<ObjectPickupC>().buyValue.ToString(), objName, useSquareImage);
                }
            }

            addedAllComponents = true;
            yield return null;
        }

        private IEnumerator RefreshPage(PartTypes partType)
        {
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollViews[partType].transform as RectTransform);
        }
    }

    public class ModsPageToggle : MonoBehaviour
    {
        public Vector3[] scale;

        private bool visible;
        private bool showingMods;
        private bool modsPagesHidden;

        private GameObject defaultPage;
        private GameObject modPage;
        private GameObject modPageBG;
        private GameObject receipt;
        private BoxCollider col;
        private TextMeshPro text;

        public void Trigger()
        {
            if (LaikaCatalogueExtension.Instance.currentOpenMagazine == null)
                return;

            var magazine = LaikaCatalogueExtension.Instance.currentOpenMagazine.transform;

            var defaultPage = magazine.Find("Page2_EngineParts").gameObject;
            var modPage = magazine.Find("ModPages").gameObject;
            var modPageBG = magazine.Find("Page4").gameObject;

            if (!showingMods)
            {
                defaultPage.SetActive(false);
                modPage.SetActive(true);
                modPageBG.SetActive(true);
                GetComponent<TextMeshPro>().text = ".................GO BACK";
                UIManager.Instance.catalogueTemplate.transform.parent.Find("EnginesScroll").gameObject.SetActive(true);
                showingMods = true;
            }
            else
            {
                defaultPage.SetActive(true);
                modPage.SetActive(false);
                modPageBG.SetActive(false);
                GetComponent<TextMeshPro>().text = ".......................MODS";
                HideAllModsPages();
                showingMods = false;
            }
        }

        void Update()
        {
            if (LaikaCatalogueExtension.Instance.currentOpenMagazine == null)
                return;

            if (LaikaCatalogueExtension.Instance.currentOpenMagazine != transform.parent.GetComponent<MagazineLogicC>())
                return;

            var magazine = LaikaCatalogueExtension.Instance.currentOpenMagazine.transform;

            if(defaultPage == null)
                defaultPage = magazine.Find("Page2_EngineParts").gameObject;

            if(modPage == null)
                modPage = magazine.Find("ModPages").gameObject;

            if (modPageBG == null)
                modPageBG = magazine.Find("Page4").gameObject;

            if (receipt == null)
                receipt = magazine.Find("Receipt").gameObject;

            if(col == null)
                col = GetComponent<BoxCollider>();

            if (text == null)
                text = GetComponent<TextMeshPro>();

            if (LaikaCatalogueExtension.Instance.currentOpenMagazine.isBookOpen)
            {
                receipt.SetActive(true);

                if (!visible)
                {
                    col.enabled = false;
                    text.text = "";
                    HideAllModsPages();
                }
                else
                {
                    col.enabled = true;
                    if (showingMods)
                    {
                        text.text = ".................GO BACK";
                        modsPagesHidden = false;
                    }
                    else
                    {
                        text.text = ".......................MODS";
                        HideAllModsPages();
                    }
                }

                if (defaultPage.activeSelf)
                {
                    visible = true;
                    modPage.SetActive(false);
                    modPageBG.SetActive(false);
                    HideAllModsPages();
                    showingMods = false;
                }
                else if (showingMods)
                {
                    visible = true;
                    modsPagesHidden = false;
                }
                else
                {
                    receipt.SetActive(false);
                    visible = false;
                    HideAllModsPages();
                }
            }
            else
                receipt.SetActive(false);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        public void Close()
        {
            if (LaikaCatalogueExtension.Instance.currentOpenMagazine == null)
                return;

            var magazine = LaikaCatalogueExtension.Instance.currentOpenMagazine.transform;

            if (showingMods)
            {
                receipt.SetActive(false);
                visible = false;
                showingMods = false;
                modPage.SetActive(false);
                modPageBG.SetActive(false);
                HideAllModsPages();

                defaultPage.SetActive(true);
                defaultPage.GetComponent<EngineComponentsCataloguePageC>().PageClose();
                StartCoroutine(WaitFrameThenHide());
            }
            else
            {
                visible = false;
                receipt.SetActive(false);

                LaikaCatalogueExtension.Instance.currentOpenMagazine.isBookOpen = false;
                iTween.Stop(LaikaCatalogueExtension.Instance.currentOpenMagazine.gameObject);
                LaikaCatalogueExtension.Instance.currentOpenMagazine.ZoomOutClose();
                LaikaCatalogueExtension.Instance.currentOpenMagazine.gameObject.GetComponent<Collider>().enabled = true;
                MainMenuC.Global.restrictPause = false;
                LaikaCatalogueExtension.Instance.currentOpenMagazine = null;
                defaultPage = modPage = modPageBG = receipt = null;
                col = null;
                text = null;
            }

            MakeAllItemsSellingDisabled();
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

            if (LaikaCatalogueExtension.Instance.currentOpenMagazine == null)
                yield return null;

            var magazine = LaikaCatalogueExtension.Instance.currentOpenMagazine.transform;

            var defaultPage = magazine.Find("Page2_EngineParts").gameObject;

            defaultPage.SetActive(false);

            LaikaCatalogueExtension.Instance.currentOpenMagazine.isBookOpen = false;
            iTween.Stop(LaikaCatalogueExtension.Instance.currentOpenMagazine.gameObject);
            LaikaCatalogueExtension.Instance.currentOpenMagazine.ZoomOutClose();
            LaikaCatalogueExtension.Instance.currentOpenMagazine.gameObject.GetComponent<Collider>().enabled = true;
            MainMenuC.Global.restrictPause = false;
            LaikaCatalogueExtension.Instance.currentOpenMagazine = null;
            MakeAllItemsSellingDisabled();
        }

        public void HideAllModsPages()
        {
            if(modsPagesHidden)
                return;

            MakeAllItemsSellingDisabled();
            var pagesHolder = UIManager.Instance.catalogueTemplate.transform.parent.gameObject;

            for (int i = 1; i < pagesHolder.transform.childCount; i++)
                pagesHolder.transform.GetChild(i).gameObject.SetActive(false);

            modsPagesHidden = true;
        }

        public void MakeAllItemsSellingDisabled()
        {
            var list = FindObjectsOfType<ModsPageItem>();

            foreach (var item in list)
                item.sellingItem.SetActive(false);
        }
    }

    public class ModsPageNavigator : MonoBehaviour
    {
        public GameObject pagesHolder;

        public PartTypes partType;

        public Vector3[] scale;

        private void Awake()
        {
            pagesHolder = UIManager.Instance.catalogueTemplate.transform.parent.gameObject;
        }

        private void HideAllPages()
        {
            for (int i = 1; i < pagesHolder.transform.childCount; i++)
            {
                pagesHolder.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        public void Trigger()
        {
            HideAllPages();
            switch (partType)
            {
                case PartTypes.Engine:
                    pagesHolder.transform.Find("EnginesScroll").gameObject.SetActive(true);
                    break;

                case PartTypes.FuelTank:
                    pagesHolder.transform.Find("FuelTanksScroll").gameObject.SetActive(true);
                    break;

                case PartTypes.Carburettor:
                    pagesHolder.transform.Find("CarburettorsScroll").gameObject.SetActive(true);
                    break;

                case PartTypes.AirFilter:
                    pagesHolder.transform.Find("AirFiltersScroll").gameObject.SetActive(true);
                    break;

                case PartTypes.IgnitionCoil:
                    pagesHolder.transform.Find("IgnitionCoilsScroll").gameObject.SetActive(true);
                    break;

                case PartTypes.Battery:
                    pagesHolder.transform.Find("BatteriesScroll").gameObject.SetActive(true);
                    break;

                case PartTypes.WaterTank:
                    pagesHolder.transform.Find("WaterTanksScroll").gameObject.SetActive(true);
                    break;

                case PartTypes.Extra:
                    pagesHolder.transform.Find("ExtrasScroll").gameObject.SetActive(true);
                    break;

                case PartTypes.Custom:
                    pagesHolder.transform.Find("CustomsScroll").gameObject.SetActive(true);
                    break;
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
    }

    public class ModsPageItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public string itemName;
        private bool isHovering;
        private Transform priceImage;
        public Transform priceText;

        public GameObject sellingItem;

        private bool loadedData = false;
        private void Awake()
        {
            if(!loadedData)
                LoadData();
        }
        
        public void LoadData()
        {
            if (loadedData)
                return;

            loadedData = true;

            priceImage = transform.parent.GetChild(3);
            priceText = transform.parent.GetChild(4);

            sellingItem = CustomObjectsManager.Instance.SpawnObject(itemName);
            sellingItem.SetActive(false);
            sellingItem.GetComponent<Rigidbody>().isKinematic = true;

            sellingItem.GetComponent<EngineComponentC>().Condition = sellingItem.GetComponent<EngineComponentC>().durability;
            sellingItem.GetComponent<ObjectPickupC>().sellValue = sellingItem.GetComponent<ObjectPickupC>().buyValue - 15;

            if (sellingItem.GetComponent<EngineComponentC>().isBattery)
            {
                sellingItem.GetComponent<EngineComponentC>().charge = 100;
            }

            if(sellingItem.GetComponent<EngineComponentC>().totalFuelAmount > 0)
            {
                sellingItem.GetComponent<EngineComponentC>().currentFuelAmount = sellingItem.GetComponent<EngineComponentC>().totalFuelAmount / 2;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Console.Log($"Added item {itemName} to cart");

            if (LaikaCatalogueExtension.Instance.currentOpenMagazine == null)
                return;

            sellingItem.SetActive(true);
            var list = LaikaCatalogueExtension.Instance.currentOpenMagazine.transform.Find("Page2_EngineParts").GetComponent<EngineComponentsCataloguePageC>();
            list.AddToShoppingList(sellingItem);

            int num2 = list.shoppingList.Count - 1;
            list.receiptStrings[num2].GetComponent<TextMeshPro>().text = CustomObjectsManager.Instance.GetObject(itemName).GetComponent<CustomObjectInfo>().objName;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;

            transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            priceImage.localScale = new Vector3(1.25f, 1.25f, 1.25f);
            priceText.localScale = new Vector3(1.25f, 1.25f, 1.25f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;

            transform.localScale = Vector3.one;
            priceImage.localScale = Vector3.one;
            priceImage.localScale = Vector3.one;
            priceText.localScale = Vector3.one;
            priceImage.eulerAngles = Vector3.zero;
        }

        private void Update()
        {
            if (!isHovering)
                return;

            priceImage.Rotate(0f, 0f, 40 * Time.deltaTime, Space.Self);
        }
    }
}
