using JaLoader.Common;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JaLoader
{
    public class AdjustmentsEditor : MonoBehaviour
    {
        public static AdjustmentsEditor Instance;

        public bool loadedViewingEditor { get; private set; }
        private DebugCamera debugCamera;
        private float arrowOffset = 0.3f;
        private bool linesDone;

        #region Extras Garage

        private GameObject carPrefab;
        private GameObject upgradesList;
        private GameObject eg_infoBox;
        private GameObject eg_templateItem;
        private GameObject bodyPartsToggler;
        private InputField eg_inputField;
        private Text eg_currentlySelectedText;
        private Text eg_positionText;
        private Text eg_rotationText;
        private Text eg_scaleText;
        private Text eg_speedText;

        private bool hoodOpened = true;
        private bool leftDoorOpened;
        private bool rightDoorOpened = true;
        private bool trunkOpened;

        private GameObject eg_selectedObject;
        private Bounds eg_selectedObjectBounds;

        private float eg_speed = 0.01f;

        private GameObject eg_xAxisArrow;
        private GameObject eg_yAxisArrow;
        private GameObject eg_zAxisArrow;

        private LineRenderer eg_xAxisLine;
        private LineRenderer eg_yAxisLine;
        private LineRenderer eg_zAxisLine;

        private BoxCollider eg_xAxisCollider;
        private BoxCollider eg_yAxisCollider;
        private BoxCollider eg_zAxisCollider;

        private GameObject eg_camera;
        private Transform eg_UI;

        #endregion

        #region Part Icon Viewer

        private GameObject objectsList;
        private GameObject piv_infoBox;
        private GameObject piv_templateItem;
        private InputField piv_inputField;
        private Text piv_currentlySelectedText;
        private Text piv_positionText;
        private Text piv_rotationText;
        private Text piv_scaleText;
        private Text piv_speedText;

        private GameObject piv_selectedObject;
        private Bounds piv_selectedObjectBounds;

        private float piv_speed = 0.01f;

        private GameObject piv_xAxisArrow;
        private GameObject piv_yAxisArrow;
        private GameObject piv_zAxisArrow;

        private LineRenderer piv_xAxisLine;
        private LineRenderer piv_yAxisLine;
        private LineRenderer piv_zAxisLine;

        private BoxCollider piv_xAxisCollider;
        private BoxCollider piv_yAxisCollider;
        private BoxCollider piv_zAxisCollider;

        private GameObject piv_camera;
        private Transform piv_UI;

        private GameObject piv_template;

        #endregion

        private bool isInExtrasGarage = true;

        private GameObject selectedArrow;
        private Vector3 originalMousePosition;
        private Vector3 originalObjectPosition;
        private Vector3 originalObjectRotation;
        private Vector3 originalObjectScale;

        private PaintJob selectedPaintjob;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Update()
        {
            if (!JaLoaderSettings.DebugMode || !ModManager.FinishedLoadingMenuMods || SceneManager.GetActiveScene().buildIndex == 3 || SceneManager.GetActiveScene().buildIndex == 2)
                return;

            if (!loadedViewingEditor)
            {
                if (Input.GetKeyDown(KeyCode.F5))
                    StartCoroutine(LoadGarage());

                return;
            }

            if (Input.GetKeyDown(KeyCode.F5) || Input.GetKeyDown(KeyCode.Escape))
                Console.LogWarning("To exit the editor, restart the game.");

            if (Input.GetKeyDown(KeyCode.F4))
            {
                if(isInExtrasGarage)
                    eg_infoBox.SetActive(!eg_infoBox.activeSelf);
                else
                    piv_infoBox.SetActive(!piv_infoBox.activeSelf);
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                eg_UI.gameObject.SetActive(!eg_UI.gameObject.activeSelf);
                piv_UI.gameObject.SetActive(!piv_UI.gameObject.activeSelf);

                isInExtrasGarage = !isInExtrasGarage;

                carPrefab.SetActive(isInExtrasGarage);

                debugCamera.mainCameraObj = isInExtrasGarage ? eg_camera : piv_camera;
                selectedArrow = null;

                if (isInExtrasGarage)
                {
                    eg_xAxisArrow.SetActive(false);
                    eg_yAxisArrow.SetActive(false);
                    eg_zAxisArrow.SetActive(false);
                }
                else
                {
                    piv_xAxisArrow.SetActive(false);
                    piv_yAxisArrow.SetActive(false);
                    piv_zAxisArrow.SetActive(false);
                }
            }

            if(Input.GetKeyDown(KeyCode.F9))
                piv_template.SetActive(!piv_template.activeSelf);

            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (isInExtrasGarage)
                {
                    upgradesList.SetActive(!upgradesList.activeSelf);

                    if (upgradesList.activeSelf)
                        debugCamera.EnableCursor();
                    else
                        debugCamera.DisableCursor();
                }
                else
                {
                    objectsList.SetActive(!objectsList.activeSelf);

                    if (objectsList.activeSelf)
                        debugCamera.EnableCursor();
                    else
                        debugCamera.DisableCursor();
                }
            }

            if(isInExtrasGarage)
                ExtrasGarageUpdate();
            else
                PartIconViewerUpdate();
        }

        private void PartIconViewerUpdate()
        {
            if (piv_selectedObject != null)
            {
                if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
                    piv_speed += 0.001f;

                if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
                    piv_speed -= 0.001f;

                piv_speed = Mathf.Clamp(piv_speed, 0.001f, 0.5f);
                piv_speedText.text = $"Speed: {piv_speed}";

                piv_positionText.text = $"Position Adjustment: {piv_selectedObject.transform.localPosition.x:F3}, {piv_selectedObject.transform.localPosition.y:F3}, {piv_selectedObject.transform.localPosition.z:F3}";
                piv_scaleText.text = $"Scale Adjustment: {piv_selectedObject.transform.localScale.x:F3}, {piv_selectedObject.transform.localScale.y:F3}, {piv_selectedObject.transform.localScale.z:F3}";
                piv_rotationText.text = $"Euler Angles Adjustment: {piv_selectedObject.transform.rotation.eulerAngles.x:F3}, {piv_selectedObject.transform.rotation.eulerAngles.y:F3}, {piv_selectedObject.transform.rotation.eulerAngles.z:F3}";
            }

            if (!linesDone || piv_selectedObject == null)
                return;

            UpdateLinesPositions();

            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = debugCamera.cameraObj.transform.GetChild(1).GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, 100f))
                {
                    if (hit.collider == piv_xAxisCollider || hit.collider == piv_yAxisCollider || hit.collider == piv_zAxisCollider)
                    {
                        selectedArrow = hit.collider.gameObject;
                        originalMousePosition = Input.mousePosition;
                        originalObjectPosition = piv_selectedObject.transform.position;
                        originalObjectRotation = piv_selectedObject.transform.rotation.eulerAngles;
                        originalObjectScale = piv_selectedObject.transform.localScale;
                    }
                }
            }

            if (Input.GetMouseButton(0) && selectedArrow != null)
            {
                Vector3 mouseDelta = Input.mousePosition - originalMousePosition;

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    // change scale
                    Vector3 scale = Vector3.zero;

                    if (selectedArrow == piv_xAxisArrow)
                        scale.x += mouseDelta.x * piv_speed;
                    else if (selectedArrow == piv_yAxisArrow)
                        scale.y += mouseDelta.y * piv_speed;
                    else if (selectedArrow == piv_zAxisArrow)
                        scale.z += mouseDelta.x * piv_speed;

                    piv_selectedObject.transform.localScale = originalObjectScale + scale;
                }
                else if (Input.GetKey(KeyCode.LeftControl))
                {
                    // change rotation

                    Vector3 rotation = Vector3.zero;

                    if (selectedArrow == piv_xAxisArrow)
                        rotation.x += mouseDelta.x * piv_speed;
                    else if (selectedArrow == piv_yAxisArrow)
                        rotation.y += mouseDelta.y * piv_speed;
                    else if (selectedArrow == piv_zAxisArrow)
                        rotation.z += mouseDelta.x * piv_speed;

                    piv_selectedObject.transform.rotation = Quaternion.Euler(originalObjectRotation + rotation);
                }
                else
                {
                    // change movement
                    Vector3 movement = Vector3.zero;

                    if (selectedArrow == piv_xAxisArrow)
                        movement = new Vector3(mouseDelta.x * piv_speed, 0, 0);
                    else if (selectedArrow == piv_yAxisArrow)
                        movement = new Vector3(0, mouseDelta.y * piv_speed, 0);
                    else if (selectedArrow == piv_zAxisArrow)
                        movement = new Vector3(0, 0, mouseDelta.x * piv_speed);

                    piv_selectedObject.transform.position = originalObjectPosition + movement;
                }
            }

            if (Input.GetMouseButtonUp(0))
                selectedArrow = null;
        }

        private void ExtrasGarageUpdate()
        {
            if (selectedPaintjob != null)
            {
                if (Input.GetKeyDown(KeyCode.F4))
                    PaintJobManager.Instance.ReloadCurrentPaintjob();
            }

            if (eg_selectedObject != null)
            {
                if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
                    eg_speed += 0.001f;

                if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
                    eg_speed -= 0.001f;

                eg_speed = Mathf.Clamp(eg_speed, 0.001f, 0.5f);
                eg_speedText.text = $"Speed: {eg_speed}";

                eg_positionText.text = $"Local Position: {eg_selectedObject.transform.localPosition.x:F3}, {eg_selectedObject.transform.localPosition.y:F3}, {eg_selectedObject.transform.localPosition.z:F3}";
                eg_scaleText.text = $"Local Scale: {eg_selectedObject.transform.localScale.x:F3}, {eg_selectedObject.transform.localScale.y:F3}, {eg_selectedObject.transform.localScale.z:F3}";
                eg_rotationText.text = $"Local Rotation: {eg_selectedObject.transform.rotation.eulerAngles.x:F3}, {eg_selectedObject.transform.rotation.eulerAngles.y:F3}, {eg_selectedObject.transform.rotation.eulerAngles.z:F3}";
            }

            if (!linesDone || eg_selectedObject == null)
                return;

            UpdateLinesPositions();

            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = debugCamera.cameraObj.transform.GetChild(1).GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, 100f))
                {
                    if (hit.collider == eg_xAxisCollider || hit.collider == eg_yAxisCollider || hit.collider == eg_zAxisCollider)
                    {
                        selectedArrow = hit.collider.gameObject;
                        originalMousePosition = Input.mousePosition;
                        originalObjectPosition = eg_selectedObject.transform.position;
                        originalObjectRotation = eg_selectedObject.transform.rotation.eulerAngles;
                        originalObjectScale = eg_selectedObject.transform.localScale;
                    }
                }
            }

            if (Input.GetMouseButton(0) && selectedArrow != null)
            {
                Vector3 mouseDelta = Input.mousePosition - originalMousePosition;

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    // change scale
                    Vector3 scale = Vector3.zero;

                    if (selectedArrow == eg_xAxisArrow)
                        scale.x += mouseDelta.x * eg_speed;
                    else if (selectedArrow == eg_yAxisArrow)
                        scale.y += mouseDelta.y * eg_speed;
                    else if (selectedArrow == eg_zAxisArrow)
                        scale.z += mouseDelta.x * eg_speed;

                    eg_selectedObject.transform.localScale = originalObjectScale + scale;
                }
                else if (Input.GetKey(KeyCode.LeftControl))
                {
                    // change rotation

                    Vector3 rotation = Vector3.zero;

                    if (selectedArrow == eg_xAxisArrow)
                        rotation.x += mouseDelta.x * eg_speed;
                    else if (selectedArrow == eg_yAxisArrow)
                        rotation.y += mouseDelta.y * eg_speed;
                    else if (selectedArrow == eg_zAxisArrow)
                        rotation.z += mouseDelta.x * eg_speed;

                    eg_selectedObject.transform.rotation = Quaternion.Euler(originalObjectRotation + rotation);
                }
                else
                {
                    // change movement
                    Vector3 movement = Vector3.zero;

                    if (selectedArrow == eg_xAxisArrow)
                        movement = new Vector3(mouseDelta.x * eg_speed, 0, 0);
                    else if (selectedArrow == eg_yAxisArrow)
                        movement = new Vector3(0, mouseDelta.y * eg_speed, 0);
                    else if (selectedArrow ==  eg_zAxisArrow)
                        movement = new Vector3(0, 0, mouseDelta.x * eg_speed);

                    eg_selectedObject.transform.position = originalObjectPosition + movement;
                }
            }

            if (Input.GetMouseButtonUp(0))
                selectedArrow = null;
        }

        private IEnumerator LoadGarage()
        {
            var bundleLoadReq = AssetBundle.LoadFromFileAsync(Path.Combine(JaLoaderSettings.ModFolderLocation, @"Required\JaLoader_AdjustmentsEditor.unity3d"));

            yield return bundleLoadReq;

            AssetBundle ab = bundleLoadReq.assetBundle;
            yield return null;

            if (ab == null)
            {
                Console.LogError("Adjustments Editor failed to load!");

                yield break;
            }

            carPrefab = Instantiate(ModHelper.Instance.laika);
            DontDestroyOnLoad(carPrefab);

            yield return null;

            Console.LogWarning("Loading Adjustments Editor...");

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("AdjustmentsEditorScene", LoadSceneMode.Single);

            while (!asyncLoad.isDone)
                yield return null;

            var canvas = GameObject.Find("Canvas");
            eg_UI = canvas.transform.Find("ExtrasGarage");

            upgradesList = eg_UI.Find("UpgradesList").gameObject;
            bodyPartsToggler = eg_UI.Find("BodyPanelToggler").gameObject;
            eg_infoBox = eg_UI.Find("InfoBox").gameObject;
            eg_currentlySelectedText = eg_UI.Find("CurrentlySelectedText").GetComponent<Text>();
            eg_positionText = eg_UI.Find("PositionText").GetComponent<Text>();
            eg_rotationText = eg_UI.Find("RotationText").GetComponent<Text>();
            eg_scaleText = eg_UI.Find("ScaleText").GetComponent<Text>();
            eg_speedText = eg_UI.Find("SpeedText").GetComponent<Text>();
            eg_templateItem = upgradesList.transform.GetChild(0).Find("Scroll View").GetChild(0).GetChild(0).GetChild(0).gameObject;
            eg_inputField = upgradesList.transform.Find("ObjectsList/InputField").GetComponent<InputField>();
            eg_inputField.onValueChanged.AddListener(delegate { OnInputValueChanged(true); });

            eg_camera = GameObject.Find("EG_Camera");

            piv_UI = canvas.transform.Find("PartIconViewer");

            objectsList = piv_UI.Find("ObjectsList").gameObject;
            piv_infoBox = piv_UI.Find("InfoBox").gameObject;
            piv_currentlySelectedText = piv_UI.Find("CurrentlySelectedText").GetComponent<Text>();
            piv_positionText = piv_UI.Find("PositionText").GetComponent<Text>();
            piv_rotationText = piv_UI.Find("RotationText").GetComponent<Text>();
            piv_scaleText = piv_UI.Find("ScaleText").GetComponent<Text>();
            piv_speedText = piv_UI.Find("SpeedText").GetComponent<Text>();
            piv_templateItem = objectsList.transform.GetChild(0).Find("Scroll View").GetChild(0).GetChild(0).GetChild(0).gameObject;
            piv_inputField = objectsList.transform.Find("ObjectsList/InputField").GetComponent<InputField>();
            piv_inputField.onValueChanged.AddListener(delegate { OnInputValueChanged(false); });
            piv_template = piv_UI.Find("Template").gameObject;

            piv_camera = piv_UI.Find("PIV_Camera").gameObject;
            piv_camera.transform.SetParent(null);
            piv_camera.transform.position = new Vector3(0, 0, -10);

            Console.LogWarning("Adjustments Editor successfully loaded!");

            ab.Unload(false);

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            loadedViewingEditor = true;

            PrepareEditor();
        }

        private void PrepareEditor()
        {
            var newGO = Instantiate(new GameObject());
            newGO.name = "ScriptHolder";
            debugCamera = newGO.AddComponent<DebugCamera>();

            debugCamera.Toggle();
            Cursor.lockState = CursorLockMode.Confined;

            carPrefab.transform.position = Vector3.zero;
            carPrefab.transform.rotation = Quaternion.identity;

            carPrefab.name = "FrameHolder";

            ModHelper.Instance.GetAllBodyParts();

            RemoveExcessObjectsFromCar();

            newGO.AddComponent<MenuCarRotate>();
            newGO.AddComponent<MenuVolumeChanger>();

            BodyPanelButtonsLogic();

            PopulateLists();

            CreateAxisArrows();
        }

        private void BodyPanelButtonsLogic()
        {
            var holder = bodyPartsToggler.transform.GetChild(0);
            var bonnetLogic = FindObjectOfType<BonnetLogicC>();
            var field = bonnetLogic.GetType().GetField("open", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(bonnetLogic, true);

            holder.Find("Hood").GetComponent<Button>().onClick.AddListener(delegate { ToggleBodyPart("Hood"); });
            holder.Find("Trunk").GetComponent<Button>().onClick.AddListener(delegate { ToggleBodyPart("Trunk"); });
            holder.Find("LeftDoor").GetComponent<Button>().onClick.AddListener(delegate { ToggleBodyPart("LeftDoor"); });
            holder.Find("RightDoor").GetComponent<Button>().onClick.AddListener(delegate { ToggleBodyPart("RightDoor"); });
        }

        private void ToggleBodyPart(string part)
        {
            switch (part)
            {
                case "Hood":
                    var logicHood = FindObjectOfType<BonnetLogicC>();
                    if (hoodOpened)
                    {
                        iTween.RotateTo(logicHood.gameObject.transform.parent.gameObject, iTween.Hash("rotation", logicHood.xyzClosed, "time", 0.3, "islocal", true, "easetype", "easeInCirc"));
                        hoodOpened = false;
                    }
                    else
                    {
                        iTween.RotateTo(logicHood.gameObject.transform.parent.gameObject, iTween.Hash("rotation", logicHood.xyzOpen, "time", 1.2, "islocal", true));
                        hoodOpened = true;
                    }
                    break;

                case "Trunk":
                    var logicTrunk = ModHelper.Instance.carTrunk.GetComponentInChildren<DoorLogicC>();
                    if(trunkOpened)
                    {
                        iTween.RotateTo(logicTrunk.gameObject.transform.parent.gameObject, iTween.Hash("rotation", logicTrunk.xyzClosed, "time", 0.3, "islocal", true, "easetype", "easeInCirc"));
                        trunkOpened = false;
                    }
                    else
                    {
                        iTween.RotateTo(logicTrunk.gameObject.transform.parent.gameObject, iTween.Hash("rotation", logicTrunk.xyzOpen, "time", 1.2, "islocal", true));
                        trunkOpened = true;
                    }
                    break;

                case "LeftDoor":
                    var logicLDoor = ModHelper.Instance.carLeftDoor.GetComponentInChildren<DoorLogicC>();
                    if (leftDoorOpened)
                    {
                        iTween.RotateTo(logicLDoor.gameObject.transform.parent.gameObject, iTween.Hash("rotation", logicLDoor.xyzClosed, "time", 0.3, "islocal", true, "easetype", "easeInCirc"));
                        leftDoorOpened = false;
                    }
                    else
                    {
                        iTween.RotateTo(logicLDoor.gameObject.transform.parent.gameObject, iTween.Hash("rotation", logicLDoor.xyzOpen, "time", 1.2, "islocal", true));
                        leftDoorOpened = true;
                    }
                    break;

                case "RightDoor":
                    var doorTrigger = ModHelper.Instance.carRightDoor.transform.Find("DoorTrigger");
                    doorTrigger.gameObject.SetActive(true);
                    var logicRDoor = doorTrigger.GetComponent<DoorLogicC>();
                    if (rightDoorOpened)
                    {
                        iTween.RotateTo(logicRDoor.handle, iTween.Hash("rotation", logicRDoor.handleRotate, "islocal", true, "time", 0.25, "onComplete", "HandleComplete", "onCompleteTarget", logicRDoor.gameObject));
                        rightDoorOpened = false;
                    }
                    else
                    {
                        iTween.RotateTo(logicRDoor.gameObject.transform.parent.gameObject, iTween.Hash("rotation", logicRDoor.xyzOpen, "time", 1.2, "islocal", true));
                        rightDoorOpened = true;
                    }
                    break;
            }
        }

        private void CreateAxisArrows()
        {
            Material defaultMaterial = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));

            #region Extras Garage

            eg_xAxisArrow = new GameObject("XAxisArrow_ExtrasGarage");
            eg_yAxisArrow = new GameObject("YAxisArrow_ExtrasGarage");
            eg_zAxisArrow = new GameObject("ZAxisArrow_ExtrasGarage");

            eg_xAxisLine = eg_xAxisArrow.AddComponent<LineRenderer>();
            eg_yAxisLine = eg_yAxisArrow.AddComponent<LineRenderer>();
            eg_zAxisLine = eg_zAxisArrow.AddComponent<LineRenderer>();

            eg_xAxisCollider = eg_xAxisArrow.AddComponent<BoxCollider>();
            eg_yAxisCollider = eg_yAxisArrow.AddComponent<BoxCollider>();
            eg_zAxisCollider = eg_zAxisArrow.AddComponent<BoxCollider>();

            eg_xAxisCollider.size = eg_yAxisCollider.size = eg_zAxisCollider.size = Vector3.one;
            eg_xAxisCollider.center = eg_yAxisCollider.center = eg_zAxisCollider.center = new Vector3(0, 0.5f, 0.5f);

            eg_xAxisLine.positionCount = 2;
            eg_xAxisLine.SetPosition(0, Vector3.zero);
            eg_xAxisLine.SetPosition(1, Vector3.right);
            eg_xAxisLine.startColor = Color.red;
            eg_xAxisLine.endColor = Color.black;

            eg_yAxisLine.positionCount = 2;
            eg_yAxisLine.SetPosition(0, Vector3.zero);
            eg_yAxisLine.SetPosition(1, Vector3.up);
            eg_yAxisLine.startColor = Color.green;
            eg_yAxisLine.endColor = Color.black;

            eg_zAxisLine.positionCount = 2;
            eg_zAxisLine.SetPosition(0, Vector3.zero);
            eg_zAxisLine.SetPosition(1, Vector3.forward);
            eg_zAxisLine.startColor = Color.blue;
            eg_zAxisLine.endColor = Color.black;

            eg_xAxisLine.material = defaultMaterial;
            eg_yAxisLine.material = defaultMaterial;
            eg_zAxisLine.material = defaultMaterial;

            eg_xAxisLine.endWidth = eg_yAxisLine.endWidth = eg_zAxisLine.endWidth = 0;
            eg_xAxisLine.sortingOrder = eg_yAxisLine.sortingOrder = eg_zAxisLine.sortingOrder = 1000;

            eg_xAxisArrow.SetActive(false);
            eg_yAxisArrow.SetActive(false);
            eg_zAxisArrow.SetActive(false);

            #endregion

            #region Part Icon Viewer

            piv_xAxisArrow = new GameObject("XAxisArrow_PartIconViewer");
            piv_yAxisArrow = new GameObject("YAxisArrow_PartIconViewer");
            piv_zAxisArrow = new GameObject("ZAxisArrow_PartIconViewer");

            piv_xAxisLine = piv_xAxisArrow.AddComponent<LineRenderer>();
            piv_yAxisLine = piv_yAxisArrow.AddComponent<LineRenderer>();
            piv_zAxisLine = piv_zAxisArrow.AddComponent<LineRenderer>();

            piv_xAxisCollider = piv_xAxisArrow.AddComponent<BoxCollider>();
            piv_yAxisCollider = piv_yAxisArrow.AddComponent<BoxCollider>();
            piv_zAxisCollider = piv_zAxisArrow.AddComponent<BoxCollider>();

            piv_xAxisCollider.center = piv_yAxisCollider.center = piv_zAxisCollider.center = new Vector3(0, 0.5f, 0.5f);

            piv_xAxisLine.positionCount = 2;
            piv_xAxisLine.SetPosition(0, Vector3.zero);
            piv_xAxisLine.SetPosition(1, Vector3.right);
            piv_xAxisLine.startColor = Color.red;
            piv_xAxisLine.endColor = Color.black;

            piv_yAxisLine.positionCount = 2;
            piv_yAxisLine.SetPosition(0, Vector3.zero);
            piv_yAxisLine.SetPosition(1, Vector3.up);
            piv_yAxisLine.startColor = Color.green;
            piv_yAxisLine.endColor = Color.black;

            piv_zAxisLine.positionCount = 2;
            piv_zAxisLine.SetPosition(0, Vector3.zero);
            piv_zAxisLine.SetPosition(1, Vector3.forward);
            piv_zAxisLine.startColor = Color.blue;
            piv_zAxisLine.endColor = Color.black;

            piv_xAxisLine.material = defaultMaterial;
            piv_yAxisLine.material = defaultMaterial;
            piv_zAxisLine.material = defaultMaterial;

            piv_xAxisLine.endWidth = piv_yAxisLine.endWidth = piv_zAxisLine.endWidth = 0;
            piv_xAxisLine.sortingOrder = piv_yAxisLine.sortingOrder = piv_zAxisLine.sortingOrder = 1000;

            piv_xAxisArrow.SetActive(false);
            piv_yAxisArrow.SetActive(false);
            piv_zAxisArrow.SetActive(false);

            #endregion

            linesDone = true;
        }

        private void PopulateLists()
        {
            #region Vanilla extras

            var vanillaUpgrades = carPrefab.transform.Find("TweenHolder/Frame/UpgradeHolders");

            AddUpgradeToList("Mud Guards", vanillaUpgrades.Find("MudGuards/MudGuards").gameObject, true);
            AddUpgradeToList("Bull Bar", vanillaUpgrades.Find("BullBar/extras_Bullbar").gameObject, true);
            AddUpgradeToList("Roof Rack", vanillaUpgrades.Find("RoofRack/Mesh").gameObject, true);
            AddUpgradeToList("Light Rack", vanillaUpgrades.Find("LightRack").GetChild(0).gameObject, true);

            #endregion

            #region Custom extras

            #region Body

            var customBody = carPrefab.transform.Find("TweenHolder/Frame/BodyExtrasHolder");

            foreach(Transform child in customBody)
            {
                var splitName = child.name.Split('_');

                AddUpgradeToList(splitName[2], child.GetChild(0).gameObject, modName: splitName[1], bothLists: true);
            }

            #endregion

            #region Hood

            var customHood = carPrefab.transform.Find("TweenHolder/Frame/Bonnet/HoodExtrasHolder");

            foreach (Transform child in customHood)
            {
                var splitName = child.name.Split('_');

                AddUpgradeToList(splitName[2], child.GetChild(0).gameObject, modName: splitName[1], bothLists: true);
            }

            #endregion

            #region LDoor

            var customLDoor = carPrefab.transform.Find("TweenHolder/Frame/L_Door/LDoorExtrasHolder");

            foreach (Transform child in customLDoor)
            {
                var splitName = child.name.Split('_');

                AddUpgradeToList(splitName[2], child.GetChild(0).gameObject, modName: splitName[1], bothLists: true);
            }

            #endregion

            #region RDoor

            var customRDoor = carPrefab.transform.Find("R_Door/RDoorExtrasHolder");

            foreach (Transform child in customRDoor)
            {
                var splitName = child.name.Split('_');

                AddUpgradeToList(splitName[2], child.GetChild(0).gameObject, modName: splitName[1], bothLists: true);
            }

            #endregion

            #region Trunk

            var customTrunk = carPrefab.transform.Find("TweenHolder/Frame/Boot/TrunkExtrasHolder");

            foreach (Transform child in customTrunk)
            {
                var splitName = child.name.Split('_');

                AddUpgradeToList(splitName[2], child.GetChild(0).gameObject, modName: splitName[1], bothLists: true);
            }

            #endregion

            #endregion

            #region Custom Objects for PIV

            CustomObjectsManager objectsManager = CustomObjectsManager.Instance;

            foreach (var entry in objectsManager.database.Keys)
            {
                if (!objectsManager.GetObject(entry).GetComponent<EngineComponentC>() || objectsManager.GetObject(entry).GetComponent<ExtraComponentC_ModExtension>())
                    continue;

                var obj = objectsManager.SpawnObjectWithoutRegistering(entry, Vector2.zero, Vector3.zero, false);
                ModHelper.RemoveAllComponents(obj, typeof(MeshFilter), typeof(MeshRenderer), typeof(ObjectIdentification));

                var comp = obj.GetComponent<ObjectIdentification>();

                obj.transform.position += comp.PartIconPositionAdjustment;
                obj.transform.eulerAngles = comp.PartIconRotationAdjustment;
                obj.transform.localScale = comp.PartIconScaleAdjustment;

                obj.SetActive(false);

                AddObjectToList(entry, obj, comp.ModID);
            }

            foreach(PaintJob paintJob in PaintJobManager.Instance.PaintJobs)
                AddPaintjobToList(paintJob);
            

            #endregion
        }

        private void AddObjectToList(string name, GameObject toEnable, string modName, bool isExtra = false)
        {
            GameObject _obj = Instantiate(piv_templateItem);
            _obj.transform.SetParent(piv_templateItem.transform.parent, false);

            _obj.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = $"({modName})\n{name}\nDisabled";

            if (isExtra)
            {
                toEnable = null;

                foreach(var extraPart in PartIconManager.Instance.extraParts)
                {
                    if (extraPart.Value.name != $"{modName}_{name}(Clone)(Clone)_DUPLICATE")
                        continue;

                    toEnable = Instantiate(extraPart.Value);
                }

                if (toEnable == null)
                {
                    if (PartIconManager.Instance.extraParts.ContainsKey(name) == false)
                        return;

                    GameObject duplicate = PartIconManager.Instance.extraParts.FirstOrDefault(x => x.Key == name).Value;

                    toEnable = Instantiate(duplicate);
                }

                ModHelper.RemoveAllComponents(toEnable, typeof(MeshFilter), typeof(MeshRenderer), typeof(ObjectIdentification));

                toEnable.transform.SetParent(null);
                toEnable.transform.position = Vector3.zero;
                toEnable.transform.rotation = Quaternion.identity;
                toEnable.transform.localScale = Vector3.one;

                var comp = toEnable.GetComponent<ObjectIdentification>();
                toEnable.transform.position += comp.PartIconPositionAdjustment;
                toEnable.transform.eulerAngles = comp.PartIconRotationAdjustment;
                toEnable.transform.localScale = comp.PartIconScaleAdjustment;
            }

            _obj.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate { OnClickButton(toEnable, _obj.transform.GetChild(0).GetChild(0).GetComponent<Text>(), $"({modName}) {name}"); });
            _obj.SetActive(true);
        }

        private void AddUpgradeToList(string name, GameObject toEnable, bool isVanilla = false, string modName = "", bool bothLists = false)
        {
            GameObject _obj = Instantiate(eg_templateItem);
            _obj.transform.SetParent(eg_templateItem.transform.parent, false);

            if (isVanilla)
                _obj.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = $"(Vanilla)\n{name}\nDisabled";
            else
                _obj.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = $"({modName})\n{name}\nDisabled";

            _obj.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate { OnClickButton(toEnable, _obj.transform.GetChild(0).GetChild(0).GetComponent<Text>(), $"({(modName == "" ? "Vanilla" : modName)}) {name}"); });
            _obj.SetActive(true);

            if (bothLists == true)
                AddObjectToList(name, toEnable, modName, true);
        }

        private void AddPaintjobToList(PaintJob paintJob)
        {
            GameObject _obj = Instantiate(eg_templateItem);
            _obj.transform.SetParent(eg_templateItem.transform.parent, false);

            _obj.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = $"(Paintjob)\n{paintJob.Name}\nDisabled";
            _obj.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate { OnClickPaintjobButton(paintJob, _obj.transform.GetChild(0).GetChild(0).GetComponent<Text>()); });
            _obj.SetActive(true);
        }

        private void OnClickButton(GameObject obj, Text toChange, string name)
        {
            if(Input.GetKey(KeyCode.LeftShift))
                SelectObject(obj, name);
            else
                ToggleObject(obj, toChange);
        }

        private void OnClickPaintjobButton(PaintJob paintJob, Text toChange)
        {
            toChange.text = toChange.text.Contains("Disabled") ? toChange.text.Replace("Disabled", "Enabled") : toChange.text.Replace("Enabled", "Disabled");

            if (toChange.text.Contains("Enabled"))
            {
                PaintJobManager.Instance.ApplyPaintjob(paintJob);
                selectedPaintjob = paintJob;
            }
            else
            {
                PaintJobManager.Instance.ClearPaintjob();
                selectedPaintjob = null;
            }
        }

        private void ToggleObject(GameObject obj, Text toChange)
        {
            obj.SetActive(!obj.activeSelf);
            toChange.text = toChange.text.Contains("Disabled") ? toChange.text.Replace("Disabled", "Enabled") : toChange.text.Replace("Enabled", "Disabled");
        }

        private void SelectObject(GameObject obj, string text)
        {
            if (isInExtrasGarage)
            {
                eg_selectedObject = obj;
                eg_currentlySelectedText.text = $"Currently selected: <color=aqua>{text}</color>";

                eg_selectedObjectBounds = RendererBounds(obj);

                eg_xAxisArrow.transform.position = eg_selectedObjectBounds.center + new Vector3(eg_selectedObjectBounds.extents.x + arrowOffset, 0, 0);
                eg_yAxisArrow.transform.position = eg_selectedObjectBounds.center + new Vector3(0, eg_selectedObjectBounds.extents.y + arrowOffset, 0);
                eg_zAxisArrow.transform.position = eg_selectedObjectBounds.center + new Vector3(0, 0, eg_selectedObjectBounds.extents.z + arrowOffset);

                eg_xAxisArrow.SetActive(true);
                eg_yAxisArrow.SetActive(true);
                eg_zAxisArrow.SetActive(true);

                eg_xAxisArrow.transform.SetParent(eg_selectedObject.transform, true);
                eg_yAxisArrow.transform.SetParent(eg_selectedObject.transform, true);
                eg_zAxisArrow.transform.SetParent(eg_selectedObject.transform, true);

                return;
            }

            piv_selectedObject = obj;
            piv_currentlySelectedText.text = $"Currently selected: <color=aqua>{text}</color>";

            piv_selectedObjectBounds = RendererBounds(obj);

            piv_xAxisArrow.transform.position = piv_selectedObjectBounds.center + new Vector3(piv_selectedObjectBounds.extents.x + arrowOffset, 0, 0);
            piv_yAxisArrow.transform.position = piv_selectedObjectBounds.center + new Vector3(0, piv_selectedObjectBounds.extents.y + arrowOffset, 0);
            piv_zAxisArrow.transform.position = piv_selectedObjectBounds.center + new Vector3(0, 0, piv_selectedObjectBounds.extents.z + arrowOffset);

            piv_xAxisArrow.SetActive(true);
            piv_yAxisArrow.SetActive(true);
            piv_zAxisArrow.SetActive(true);

            piv_xAxisArrow.transform.SetParent(piv_selectedObject.transform, true);
            piv_yAxisArrow.transform.SetParent(piv_selectedObject.transform, true);
            piv_zAxisArrow.transform.SetParent(piv_selectedObject.transform, true);
        }

        private void UpdateLinesPositions()
        {
            if (isInExtrasGarage)
            {
                eg_xAxisLine.SetPosition(0, eg_xAxisArrow.transform.position);
                eg_xAxisLine.SetPosition(1, eg_xAxisArrow.transform.position + Vector3.right);

                eg_yAxisLine.SetPosition(0, eg_yAxisArrow.transform.position);
                eg_yAxisLine.SetPosition(1, eg_yAxisArrow.transform.position + Vector3.up);

                eg_zAxisLine.SetPosition(0, eg_zAxisArrow.transform.position);
                eg_zAxisLine.SetPosition(1, eg_zAxisArrow.transform.position + Vector3.forward);

                return;
            }

            piv_xAxisLine.SetPosition(0, piv_xAxisArrow.transform.position);
            piv_xAxisLine.SetPosition(1, piv_xAxisArrow.transform.position + Vector3.right);

            piv_yAxisLine.SetPosition(0, piv_yAxisArrow.transform.position);
            piv_yAxisLine.SetPosition(1, piv_yAxisArrow.transform.position + Vector3.up);

            piv_zAxisLine.SetPosition(0, piv_zAxisArrow.transform.position);
            piv_zAxisLine.SetPosition(1, piv_zAxisArrow.transform.position + Vector3.forward);
        }

        private void OnInputValueChanged(bool isEGVersion)
        {
            if (isEGVersion)
            {
                foreach (Transform child in eg_templateItem.transform.parent)
                {
                    if (child.name == "ItemTemplate") continue;

                    if (child.GetChild(0).GetChild(0).GetComponent<Text>().text.ToLower().Contains(eg_inputField.text.ToLower()))
                        child.gameObject.SetActive(true);
                    else
                        child.gameObject.SetActive(false);
                }

                return;
            }

            foreach (Transform child in piv_templateItem.transform.parent)
            {
                if (child.name == "ItemTemplate") continue;

                if (child.GetChild(0).GetChild(0).GetComponent<Text>().text.ToLower().Contains(piv_inputField.text.ToLower()))
                    child.gameObject.SetActive(true);
                else
                    child.gameObject.SetActive(false);
            }
        }

        private Bounds RendererBounds(GameObject obj)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                return renderer.bounds;
            else
                return new Bounds(obj.transform.position, Vector3.one);
        }

        private void RemoveExcessObjectsFromCar()
        {
            foreach (var obj in FindObjectsOfType<Collider>())
                Destroy(obj.GetComponent<Collider>());

            Destroy(ModHelper.Instance.carFrame.transform.Find("bonnetLamp").gameObject);
        }
    }
}
