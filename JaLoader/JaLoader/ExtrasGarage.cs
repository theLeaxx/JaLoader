using System.Collections;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JaLoader
{
    public class ExtrasGarage : MonoBehaviour
    {
        public static ExtrasGarage Instance;

        private SettingsManager settingsManager;
        public bool loadedGarage { get; private set; }

        private GameObject carPrefab;
        private GameObject upgradesList;
        private GameObject templateItem;
        private InputField inputField;
        private Text currentlySelectedText;

        private DebugCamera debugCamera;

        public GameObject selectedObject;

        private float speed = 1;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            settingsManager = SettingsManager.Instance;
        }

        private void Update()
        {
            if (!settingsManager.DebugMode || !ModLoader.Instance.finishedInitializingPartOneMods || SceneManager.GetActiveScene().buildIndex == 3 || SceneManager.GetActiveScene().buildIndex == 2)
                return;

            if (Input.GetKeyDown(KeyCode.F5) || Input.GetKeyDown(KeyCode.Escape))
                Console.LogWarning("To exit the garage, restart the game.");

            if (Input.GetKeyDown(KeyCode.Space))
            {
                upgradesList.SetActive(!upgradesList.activeSelf);

                if(upgradesList.activeSelf)
                    debugCamera.EnableCursor();
                else
                    debugCamera.DisableCursor();
            }

            if (!loadedGarage)
                if(Input.GetKeyDown(KeyCode.F5))
                    StartCoroutine(LoadGarage());

            if (selectedObject != null)
            {
                if(Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
                    speed += 0.1f;

                if(Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
                    speed -= 0.1f;

                // by default, all movements are done positivetely, and if the CAPS LOCK button is held down, they are done negatively. therefore, we can use the up arrow key, right arrow key, nd left arrow key for XYZ respectively.
                var direction = Input.GetKey(KeyCode.CapsLock) ? -1 : 1;

                if (Input.GetKey(KeyCode.RightControl))
                {
                    if (Input.GetKey(KeyCode.UpArrow))
                        selectedObject.transform.position += Vector3.up * speed * direction;
                    if (Input.GetKey(KeyCode.LeftArrow))
                        selectedObject.transform.position += Vector3.left * speed * direction;
                    if (Input.GetKey(KeyCode.RightArrow))
                        selectedObject.transform.position += Vector3.forward * speed * direction;
                }

                if (Input.GetKey(KeyCode.RightAlt))
                {
                    if (Input.GetKey(KeyCode.UpArrow))
                        selectedObject.transform.Rotate(Vector3.up * speed * direction);
                    if (Input.GetKey(KeyCode.LeftArrow))
                        selectedObject.transform.Rotate(Vector3.left * speed * direction);
                    if (Input.GetKey(KeyCode.RightArrow))
                        selectedObject.transform.Rotate(Vector3.forward * speed * direction);
                }

                if (Input.GetKey(KeyCode.RightShift))
                {
                    if (Input.GetKey(KeyCode.UpArrow))
                        selectedObject.transform.localScale += Vector3.up * speed * direction;
                    if (Input.GetKey(KeyCode.LeftArrow))
                        selectedObject.transform.localScale += Vector3.left * speed * direction;
                    if (Input.GetKey(KeyCode.RightArrow))
                        selectedObject.transform.localScale += Vector3.forward * speed * direction;
                }
            }
        }

        private IEnumerator LoadGarage()
        {
            var bundleLoadReq = AssetBundle.LoadFromFileAsync(Path.Combine(settingsManager.ModFolderLocation, @"Required\JaLoader_ExtrasGarage.unity3d"));

            yield return bundleLoadReq;

            AssetBundle ab = bundleLoadReq.assetBundle;

            if (ab == null)
            {
                Console.LogError("Extras Garage failed to load!");

                yield break;
            }

            carPrefab = Instantiate(ModHelper.Instance.laika);
            DontDestroyOnLoad(carPrefab);

            Console.LogWarning("Loading Extras Garage...");

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("CustomPartsCheckingOnCar", LoadSceneMode.Single);

            while (!asyncLoad.isDone)
                yield return null;

            upgradesList = GameObject.Find("Canvas").transform.Find("UpgradesList").gameObject;
            currentlySelectedText = GameObject.Find("Canvas").transform.Find("CurrentlySelectedText").GetComponent<Text>();
            templateItem = upgradesList.transform.GetChild(0).Find("Scroll View").GetChild(0).GetChild(0).GetChild(0).gameObject;

            inputField = upgradesList.transform.Find("ObjectsList/InputField").GetComponent<InputField>();
            inputField.onValueChanged.AddListener(delegate { OnInputValueChanged(); });

            Console.LogWarning("Extras Garage successfully loaded!");

            Console.LogWarning("To toggle the debug camera, press Shift + C.");
            Console.LogWarning("You can rotate the vehicle using the left and right arrows.");
            Console.LogWarning("Use the menu on the right side to choose what part you want to spawn.");
            Console.LogWarning("To select a part, click on it");
            Console.LogWarning("To move a part, select it, hold RIGHT CTRL and use the arrow keys.");
            Console.LogWarning("To rotate a part, select it, hold RIGHT ALT and use the arrow keys.");
            Console.LogWarning("To scale a part, select it, hold RIGHT SHIFT and use the arrow keys.");
            Console.LogWarning("To delete a part, select it and press DELETE.");
            Console.LogWarning("To unlock the cursor, press F2.");

            debugCamera = Camera.main.gameObject.AddComponent<DebugCamera>();

            ab.Unload(false);

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            loadedGarage = true;

            PrepareGarage();
        }

        private void PrepareGarage()
        {
            debugCamera.Toggle();
            Cursor.lockState = CursorLockMode.Confined;

            carPrefab.transform.position = Vector3.zero;
            carPrefab.transform.rotation = Quaternion.identity;

            carPrefab.name = "FrameHolder";

            RemoveExcessObjectsFromCar();

            var newGO = Instantiate(new GameObject());
            newGO.name = "ScriptHolder";
            newGO.AddComponent<MenuCarRotate>();
            newGO.AddComponent<MenuVolumeChanger>();

            PopulateUpgradesList();
        }

        private void PopulateUpgradesList()
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

                AddUpgradeToList(splitName[2], child.GetChild(0).gameObject, modName: splitName[1]);

                if (!child.GetChild(0).GetComponent<Collider>())
                    child.GetChild(0).gameObject.AddComponent<BoxCollider>();
            }

            #endregion

            #region Hood

            var customHood = carPrefab.transform.Find("TweenHolder/Frame/Bonnet/HoodExtrasHolder");

            foreach (Transform child in customHood)
            {
                var splitName = child.name.Split('_');

                AddUpgradeToList(splitName[2], child.GetChild(0).gameObject, modName: splitName[1]);

                if (!child.GetChild(0).GetComponent<Collider>())
                    child.GetChild(0).gameObject.AddComponent<BoxCollider>();
            }

            #endregion

            #region LDoor

            var customLDoor = carPrefab.transform.Find("TweenHolder/Frame/L_Door/LDoorExtrasHolder");

            foreach (Transform child in customLDoor)
            {
                var splitName = child.name.Split('_');

                AddUpgradeToList(splitName[2], child.GetChild(0).gameObject, modName: splitName[1]);

                if (!child.GetChild(0).GetComponent<Collider>())
                    child.GetChild(0).gameObject.AddComponent<BoxCollider>(); AddUpgradeToList(splitName[2], child.GetChild(0).gameObject, modName: splitName[1]);
            }

            #endregion

            #region RDoor

            var customRDoor = carPrefab.transform.Find("R_Door/RDoorExtrasHolder");

            foreach (Transform child in customRDoor)
            {
                var splitName = child.name.Split('_');

                AddUpgradeToList(splitName[2], child.GetChild(0).gameObject, modName: splitName[1]);

                if (!child.GetChild(0).GetComponent<Collider>())
                    child.GetChild(0).gameObject.AddComponent<BoxCollider>();
            }

            #endregion

            #region Trunk

            var customTrunk = carPrefab.transform.Find("TweenHolder/Frame/Boot/TrunkExtrasHolder");

            foreach (Transform child in customTrunk)
            {
                var splitName = child.name.Split('_');

                AddUpgradeToList(splitName[2], child.GetChild(0).gameObject, modName: splitName[1]);

                if (!child.GetChild(0).GetComponent<Collider>())
                    child.GetChild(0).gameObject.AddComponent<BoxCollider>();
            }

            #endregion

            #endregion
        }

        private void AddUpgradeToList(string name, GameObject toEnable, bool isVanilla = false, string modName = "")
        {
            GameObject _obj = Instantiate(templateItem);
            _obj.transform.SetParent(templateItem.transform.parent, false);

            if (isVanilla)
                _obj.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = $"(Vanilla)\n{name}\nDisabled";
            else
                _obj.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = $"({modName})\n{name}\nDisabled";

            _obj.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate { OnClickButton(toEnable, _obj.transform.GetChild(0).GetChild(0).GetComponent<Text>(), $"({(modName == "" ? "Vanilla" : modName)}) {name}"); });
            _obj.SetActive(true);
        }

        private void OnClickButton(GameObject obj, Text toChange, string name)
        {
            if(Input.GetKey(KeyCode.LeftShift))
                SelectObject(obj, name);
            else
                ToggleObject(obj, toChange);
        }

        private void ToggleObject(GameObject obj, Text toChange)
        {
            obj.SetActive(!obj.activeSelf);
            toChange.text = toChange.text.Contains("Disabled") ? toChange.text.Replace("Disabled", "Enabled") : toChange.text.Replace("Enabled", "Disabled");
        }

        private void SelectObject(GameObject obj, string text)
        {
            selectedObject = obj;
            currentlySelectedText.text = $"Currently selected: <color=aqua>{text}</color>";
        }

        private void OnInputValueChanged()
        {
            foreach (Transform child in templateItem.transform.parent)
            {
                if (child.name == "ItemTemplate") continue;

                if (child.GetChild(0).GetChild(0).GetComponent<Text>().text.ToLower().Contains(inputField.text.ToLower()))
                    child.gameObject.SetActive(true);
                else
                    child.gameObject.SetActive(false);
            }
        }

        private void RemoveExcessObjectsFromCar()
        {
            foreach (var obj in FindObjectsOfType<Transform>())
            {
                if(obj.name != "Mesh")
                    Destroy(obj.GetComponent<Collider>());
            }
        }
    }
}
