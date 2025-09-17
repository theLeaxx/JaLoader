using JaLoader.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace JaLoader
{
    public class DebugObjectSpawner : MonoBehaviour
    {
        #region Assigning names to all the vanilla ids
        public Dictionary<int, string> vanillaIdsNames = new Dictionary<int, string>
        {
            {0, "Upgrade01_BullBar"},
            {1, "Upgrade01_MudGuards"},
            {2, "Upgrade01_RoofRack"},
            {3, "AirFilter_01_Stock"},
            {4, "AirFilter_02_Green"},
            {5, "AirFilter_03_Brown"},
            {6, "AirFilter_04_Red"},
            {7, "AirFilter_05_Kronekong"},
            {8, "AirFilter_06_HKS"},
            {9, "AirFilter_07_Carbolyte"},
            {10, "AirFilter_08"},
            {11, "AirFilter_09_Green"},
            {12, "AirFilter_10_Brown"},
            {13, "AirFilter_11_Red"},
            {14, "AirFilter_12_Kronekong"},
            {15, "AirFilter_13_HKS"},
            {16, "AirFilter_14_Carbolyte"},
            {17, "AirFilter_15"},
            {18, "AirFilter_16_Green"},
            {19, "AirFilter_17_Brown"},
            {20, "AirFilter_18_Red"},
            {21, "AirFilter_19_KroneKong"},
            {22, "AirFilter_20_HKS"},
            {23, "AirFilter_21_Carbolyte"},
            {24, "AirFilter_29"},
            {25, "AirFilter_30_Green"},
            {26, "AirFilter 31_Brown"},
            {27, "AirFilter_32_Red"},
            {28, "AirFilter_33_KroneKong"},
            {29, "AirFilter_34_HKS"},
            {30, "AirFilter_35_Carbolyte"},
            {31, "Battery_01"},
            {32, "Battery_02"},
            {33, "Battery_03"},
            {34, "Battery_04"},
            {35, "Carburettor_01"},
            {36, "Carburettor_02_Green"},
            {37, "Carburettor_03_Brown"},
            {38, "Carburettor_04_Red"},
            {39, "Carburettor_05_KroneKong"},
            {40, "Carburettor_06_HKS"},
            {41, "Carburettor_07_Carbolyte"},
            {42, "Carburettor_08"},
            {43, "Carburettor_09_Green"},
            {44, "Carburettor_10_Brown"},
            {45, "Carburettor_11_Red"},
            {46, "Carburettor_12_KroneKong"},
            {47, "Carburettor_13_HKS"},
            {48, "Carburettor_14_Carbolyte"},
            {49, "Carburettor_15"},
            {50, "Carburettor_16_Green"},
            {51, "Carburettor_17_Brown"},
            {52, "Carburettor_18_Red"},
            {53, "Carburettor_19_KroneKong"},
            {54, "Carburettor_20_HKS"},
            {55, "Carburettor_21_Carbolyte"},
            {56, "Carburettor_22"},
            {57, "Carburettor_23_Green"},
            {58, "Carburettor_24_Brown"},
            {59, "Carburettor_25_Red"},
            {60, "Carburettor_26_KroneKong"},
            {61, "Carburettor_27_HKS"},
            {62, "Carburettor_28_Carbolyte"},
            {63, "EngineBlock_01"},
            {64, "EngineBlock 02_Green"},
            {65, "EngineBlock_03_Brown"},
            {66, "EngineBlock_04_Red"},
            {67, "EngineBlock_05_KroneKong"},
            {68, "EngineBlock_06_HKS"},
            {69, "EngineBlock_07_Carbolyte"},
            {70, "EngineBlock_08"},
            {71, "EngineBlock_09_Green"},
            {72, "EngineBlock_10_Brown"},
            {73, "EngineBlock_11_Red"},
            {74, "EngineBlock_12_KroneKong"},
            {75, "EngineBlock_13_HKS"},
            {76, "EngineBlock_14_Carbolyte"},
            {77, "EngineBlock_15"},
            {78, "EngineBlock_16_Green"},
            {79, "EngineBlock_17_Brown"},
            {80, "EngineBlock_18_Red"},
            {81, "EngineBlock_19_KroneKong"},
            {82, "EngineBlock_20_HKS"},
            {83, "EngineBlock_21_Carbolyte"},
            {84, "EngineBlock_22"},
            {85, "EngineBlock_23_Green"},
            {86, "EngineBlock_24_Brown"},
            {87, "EngineBlock_25_Red"},
            {88, "EngineBlock_26_Kronekong"},
            {89, "EngineBlock_27_HKS"},
            {90, "EngineBlock_28_Carbolyte"},
            {91, "FuelTank_01"},
            {92, "FuelTank_02_Green"},
            {93, "FuelTank_03_Brown"},
            {94, "FuelTank_04_Red"},
            {95, "FuelTank_05_KroneKong"},
            {96, "FuelTank_06_HKS"},
            {97, "FuelTank_07_Carbolyte"},
            {98, "FuelTank_08"},
            {99, "FuelTank_09_Green"},
            {100, "FuelTank_10_Brown"},
            {101, "FuelTank_11_Red"},
            {102, "FuelTank_12_KroneKong"},
            {103, "FuelTank_13_HKS"},
            {104, "FuelTank_14_Carbolyte"},
            {105, "FuelTank_15"},
            {106, "FuelTank_16_Green"},
            {107, "FuelTank_17_Brown"},
            {108, "FuelTank_18_Red"},
            {109, "FuelTank_19_KroneKong"},
            {110, "FuelTank_20_HKS"},
            {111, "FuelTank_21_Carbolyte"},
            {112, "FuelTank_22"},
            {113, "FuelTank_23_Green"},
            {114, "FuelTank_24_Brown"},
            {115, "FuelTank_25_Red"},
            {116, "FuelTank_26_KroneKong"},
            {117, "FuelTank_27_HKS"},
            {118, "FuelTank_28_Carbolyte"},
            {119, "IgnitionCoil_01"},
            {120, "IgnitionCoil_02_Green"},
            {121, "IgnitionCoil_03_Brown"},
            {122, "IgnitionCoil_04_Red"},
            {123, "IgnitionCoil_05_KroneKong"},
            {124, "IgnitionCoil_06_HKS"},
            {125, "IgnitionCoil_07_Carbolyte"},
            {126, "IgnitionCoil_08"},
            {127, "IgnitionCoil_09_Green"},
            {128, "IgnitionCoil_10_Brown"},
            {129, "IgnitionCoil_11_Red"},
            {130, "IgnitionCoil_12_KroneKong"},
            {131, "IgnitionCoil_13_HKS"},
            {132, "IgnitionCoil_14_Carbolyte"},
            {133, "IgnitionCoil_15"},
            {134, "IgnitionCoil_16_Green"},
            {135, "IgnitionCoil_17_Brown"},
            {136, "IgnitionCoil_18_Red"},
            {137, "IgnitionCoil_19_KroneKong"},
            {138, "Ignition Coil_20_HKS"},
            {139, "IgnitionCoil_21_Carbolyte"},
            {140, "IgnitionCoil_22"},
            {141, "IgnitionCoil_23_Green"},
            {142, "IgnitionCoil_24_Brown"},
            {143, "IgnitionCoil_25_Red"},
            {144, "IgnitionCoil_26_KroneKong"},
            {145, "IgnitionCoil_27_HKS"},
            {146, "IgnitionCoil_28_Carbolyte"},
            {147, "WaterContainer"},
            {148, "WaterContainer_02"},
            {149, "WaterContainer_03"},
            {150, "WaterContainer_04"},
            {151, "Alcohol"},
            {152, "Coffee"},
            {153, "Meat"},
            {154, "Pharmaceuticals"},
            {155, "Textiles"},
            {156, "Tobacco"},
            {157, "2-Stroke"},
            {158, "GasCan"},
            {159, "WaterBottle"},
            {160, "CarJack"},
            {161, "crowbar"},
            {162, "EngineRepairKit"},
            {163, "Road Tyre_1"},
            {164, "Bucket"},
            {165, "CardboardBox_Big"},
            {166, "CardboardBox_Med"},
            {167, "CardboardBox_Small"},
            {168, "WoodenBox_Big"},
            {169, "WoodenBox_Med"},
            {170, "WoodenBox_Small"},
            {171, "TrashBag"},
            {172, "NoStealingSign"},
            {173, "PaintCan_Creme"},
            {174, "PaintCanGreen"},
            {175, "PaintCanBlue"},
            {176, "PaintCanRed"},
            {177, "PaintCanBlack"},
            {178, "PaintCanBlue Metallic"},
            {179, "PaintCanMetallicRed"},
            {180, "PaintCan_Yellow"},
            {181, "PaintCanGreenMetallic"},
            {182, "PaintCan_YellowMetallics"},
            {183, "PaintCan_Black Metallics"},
            {184, "PaintCan_CremeMetallics"},
            {185, "UpgradeDecal_GoFastRed"},
            {186, "UpgradeDecal_GoFastBlue"},
            {187, "UpgradeDecal_GoFastCreme"},
            {188, "UpgradeDecal_GoFastGreen"},
            {189, "UpgradeDecal_GoFastBlack"},
            {190, "UpgradeDecal_GoFasYellow"},
            {191, "UpgradeDecal_Flames"},
            {192, "UpgradeDecal_Flames_blue"},
            {193, "UpgradeDecal_Flames_Black"},
            {194, "UpgradeDecal_RudeBoyBlack"},
            {195, "UpgradeDecal_RudeBoyWhite"},
            {196, "UpgradeDecal_TwoTone_Creme"},
            {197, "UpgradeDecal_TwoTone_Green"},
            {198, "UpgradeDecal_Two Tone_Red"},
            {199, "UpgradeDecal_Two Tone_Blue"},
            {200, "UpgradeDecal_Two Tone_Black"},
            {201, "UpgradeDecal_TwoTone_Yellow"},
            {202, "UpgradeDecal_Cigaretten"},
            {203, "UpgradeDecal_Empty"},
            {204, "UpgradeDecal_Cigaretten2"},
            {205, "UpgradeDecal_Cigaretten3"},
            {206, "Upgrade04_DigitalDash"},
            {207, "Upgrade05_LightRack"},
            {208, "Newspaper"},
            {209, "PlasticCup"},
            {210, "Basket"},
            {211, "Upgrade06_ToolRack"},
            {212, "Upgrade06_ToolRack_Lvl2"},
            {213, "Upgrade06_ToolRack_LvI3"},
            {214, "off_Tyre_1"},
            {215, "Wet_Tyre_1"},
            {216, "Road Tyre_2"},
            {217, "Road Tyre_3"},
            {218, "off_Tyre_2"},
            {219, "Wet_Tyre_2"},
            {220, "Wet_Tyre_3"},
            {221, "off_Tyre_3"},
            {222, "TyreBricks"},
            {223, "TyreRepairkit"}
        };
        #endregion

        private CustomObjectsManager customObjects;
        private UIManager uiManager;

        private string currentlySelected = "0";
        private InputField inputField;

        public List<string> addedCustomObjects = new List<string>();

        #region Singleton
        public static DebugObjectSpawner Instance { get; private set; }
        
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

            customObjects = CustomObjectsManager.Instance;
            uiManager = UIManager.Instance;

            EventsManager.Instance.OnUILoadFinished += OnUILoadFinished;
            EventsManager.Instance.OnCustomObjectsRegisterFinished += OnCustomObjectsRegisterFinished;
        }
        #endregion

        public void SpawnVanillaObject(int ID)
        {
            if(ID < 0 || ID > 233) return;

            var obj = Instantiate(FindObjectOfType<MainMenuC>().objectIDCatalogue[ID], ModHelper.Instance.player.transform.position, ModHelper.Instance.player.transform.rotation);
            obj.transform.localScale = obj.GetComponent<ObjectPickupC>().adjustScale == Vector3.zero ? obj.transform.localScale : obj.GetComponent<ObjectPickupC>().adjustScale;
        }

        public GameObject GetDuplicateVanillaObject(int ID, Transform parent)
        {
            if (ID < 0 || ID > 233) return null;

            var obj = Instantiate(FindObjectOfType<MainMenuC>().objectIDCatalogue[ID], parent);
            obj.transform.localScale = obj.GetComponent<ObjectPickupC>().adjustScale == Vector3.zero ? obj.transform.localScale : obj.GetComponent<ObjectPickupC>().adjustScale;

            return obj;
        }

        public void SpawnVanillaObject(int ID, Transform parent)
        {
            if (ID < 0 || ID > 233) return;

            var obj = Instantiate(FindObjectOfType<MainMenuC>().objectIDCatalogue[ID], parent);
            obj.transform.localScale = obj.GetComponent<ObjectPickupC>().adjustScale == Vector3.zero ? obj.transform.localScale : obj.GetComponent<ObjectPickupC>().adjustScale;
        }

        public void SpawnVanillaObject(int ID, Vector3 position, Quaternion rotation)
        {
            if (ID < 0 || ID > 233) return;

            var obj = Instantiate(FindObjectOfType<MainMenuC>().objectIDCatalogue[ID], position, rotation);
            obj.transform.localScale = obj.GetComponent<ObjectPickupC>().adjustScale == Vector3.zero ? obj.transform.localScale : obj.GetComponent<ObjectPickupC>().adjustScale;
        }

        public void SpawnVanillaObject(int ID, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (ID < 0 || ID > 233) return;

            var obj = Instantiate(FindObjectOfType<MainMenuC>().objectIDCatalogue[ID], position, rotation, parent);
            obj.transform.localScale = obj.GetComponent<ObjectPickupC>().adjustScale == Vector3.zero ? obj.transform.localScale : obj.GetComponent<ObjectPickupC>().adjustScale;
        }

        public string GetNameOfVanillaObject(int ID)
        {
            return vanillaIdsNames[ID];
        }

        private void OnUILoadFinished()
        {
            inputField = uiManager.ObjectsList.transform.Find("SearchField").GetComponent<InputField>();

            inputField.onValueChanged.AddListener(delegate { OnInputValueChanged(); });

            foreach (KeyValuePair<int, string> keyValuePair in vanillaIdsNames)
            {
                AddObjectToList(keyValuePair.Key.ToString(), keyValuePair.Value);
            }

            uiManager.ObjectsList.transform.Find("SpawnButton").GetComponent<Button>().onClick.AddListener(delegate { SpawnObject(currentlySelected); });
        }

        private void OnCustomObjectsRegisterFinished()
        {
            foreach (KeyValuePair<string, GameObject> keyValuePair in customObjects.database)
            {
                AddObjectToList(keyValuePair.Key.ToString(), "");
            }

            EventsManager.Instance.OnCustomObjectsRegisterFinished -= OnCustomObjectsRegisterFinished;
        }

        private void Update()
        {
            if (JaLoaderSettings.DebugMode && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S))
            {
                uiManager.ObjectsList.transform.parent.gameObject.SetActive(!uiManager.ObjectsList.transform.parent.gameObject.activeSelf);
            }
        }

        public void AddObjectToList(string id, string name)
        {
            GameObject _obj = Instantiate(uiManager.ObjectEntryTemplate);
            _obj.transform.SetParent(uiManager.ObjectEntryTemplate.transform.parent, false);
            if (name != "")
            {
                _obj.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = $"(Vanilla) - {id}\n{name}";
            }
            else
            {
                _obj.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = $"(Custom) - No ID\n{id}";
                addedCustomObjects.Add(id);
            }
            _obj.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate { SelectObject(_obj.transform.GetChild(0).GetChild(0).GetComponent<Text>().text); });
            _obj.SetActive(true);
        }

        private void SelectObject(string text)
        {
            string[] lines = text.Split('\n');
            currentlySelected = lines[0];

            if (currentlySelected.Contains("(Vanilla)"))
            {
                if (int.TryParse(currentlySelected.Replace("(Vanilla) - ", ""), out int parsedID))
                    currentlySelected = parsedID.ToString();

                uiManager.CurrentSelectedObjectText.text = $"Currently selected: <color=aqua>{GetNameOfVanillaObject(int.Parse(currentlySelected))} ({currentlySelected})</color>";

                currentlySelected = lines[0];
            }
            else
            {
                currentlySelected = lines[1];
                uiManager.CurrentSelectedObjectText.text = $"Currently selected: <color=aqua>{currentlySelected} ({customObjects.GetObject(currentlySelected).GetComponent<ObjectIdentification>().ModID})</color>";
            }
        }

        private void OnInputValueChanged()
        {
            foreach(Transform child in uiManager.ObjectsList.Find("Content/Viewport/Content"))
            {
                if (child.name == "ItemTemplate") continue;

                if (child.GetChild(0).GetChild(0).GetComponent<Text>().text.ToLower().Contains(inputField.text.ToLower()))
                {
                    child.gameObject.SetActive(true);
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private void SpawnObject(string currentlySelected)
        {
            if (currentlySelected.Contains("(Vanilla)"))
            {
                if (int.TryParse(currentlySelected.Replace("(Vanilla) - ", ""), out int parsedID))
                    SpawnVanillaObject(parsedID);
            }       
            else
            {
                customObjects.SpawnObject(currentlySelected, ModHelper.Instance.player.transform.position, ModHelper.Instance.player.transform.rotation);
            }
        }
    }
}
