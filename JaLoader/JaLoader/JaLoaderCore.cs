using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using JaLoader.Common;

namespace JaLoader
{
    public class JaLoaderCore : MonoBehaviour, ICore
    {
        public static JaLoaderCore Instance { get; private set; }

        public void Start()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            }

            gameObject.name = "JaLoader";
            RuntimeVariables.ApplicationDataPath = Application.dataPath;

            if (CoreUtils.AnyMissingDLLs() != "None")
            {
                CreateErrorMessage($"\n\nDLL {CoreUtils.AnyMissingDLLs()} was not found. You can try:", "Reinstalling JaLoader with JaPatcher\n\n\nCopying the files from JaPatcher's directory/Assets/Managed to Jalopy_Data/Managed");
                SceneManager.LoadScene("MainMenu");
                return;
            }

            AddEssentialScripts();

            Debug.Log("JaLoader Core initialized!");

            EventsManager.Instance.OnMenuLoad += OnMenuLoad;
            EventsManager.Instance.OnUILoadFinished += OnUILoadFinished;

            GameTweaks.Instance.SkipLanguage();
        }

        private void OnMenuLoad()
        {
            Debug.Log($"Selected language: {Language.CurrentLanguage()}");
        }

        private void OnUILoadFinished()
        {
            UIManager.Instance.JLFPSText.AddComponent<FPSCounter>();
            UIManager.Instance.JLDebugText.AddComponent<DebugInfo>();
        }

        private void AddEssentialScripts()
        {
            GameObject utilities = new GameObject("JaLoader Utilities");
            DontDestroyOnLoad(utilities);
            utilities.transform.SetParent(gameObject.transform);

            GameObject console = Instantiate(new GameObject());
            console.AddComponent<Console>();
            console.name = "JaLoader Console";
            DontDestroyOnLoad(console);
            console.transform.SetParent(gameObject.transform);

            gameObject.AddComponent<HarmonyManager>();
            gameObject.AddComponent<EventsManager>();
            SettingsManager.Initialize();
            ModManager.Initialize();
            utilities.AddComponent<CustomRadioController>();
            gameObject.AddComponent<ReferencesLoader>();
            gameObject.AddComponent<UIManager>();
            gameObject.AddComponent<ModLoader>();
            gameObject.AddComponent<CustomObjectsManager>();
            gameObject.AddComponent<ExtrasManager>();

            gameObject.AddComponent<DebugUtils>();
            utilities.AddComponent<ModHelper>();
            utilities.AddComponent<UncleHelper>();
            utilities.AddComponent<DebugObjectSpawner>();
            utilities.AddComponent<PaintJobManager>();
            utilities.AddComponent<PartIconManager>();
            utilities.AddComponent<DiscordController>();
            utilities.AddComponent<AdjustmentsEditor>();
            utilities.AddComponent<GameTweaks>();
            utilities.AddComponent<MenuCarRotate>();
        }

        internal void CreateErrorMessage(string issue, string possibleFixes)
        {
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                Debug.Log("JaLoader encounted an error!");
                Debug.Log($"JaLoader: {issue} {possibleFixes}");

                FindObjectOfType<MenuMouseInteractionsC>().enabled = false;

                GameObject notice = Instantiate(GameObject.Find("UI Root").transform.Find("Notice").gameObject);
                notice.name = "Error";
                notice.transform.parent = GameObject.Find("UI Root").transform;
                notice.transform.localPosition = Vector3.zero;
                notice.transform.position = new Vector3(notice.transform.position.x, notice.transform.position.y - 0.15f, notice.transform.position.z);
                notice.transform.localRotation = Quaternion.identity;
                notice.transform.localScale = Vector3.one;
                notice.SetActive(true);

                notice.transform.GetChild(5).gameObject.SetActive(false);
                notice.transform.GetChild(1).GetComponent<UITexture>().height = 600;
                notice.transform.GetChild(1).position = new Vector3(notice.transform.GetChild(1).position.x, notice.transform.GetChild(1).position.y + 0.2f, notice.transform.GetChild(1).position.z);
                notice.transform.GetChild(0).GetComponent<UILabel>().text = "JaLoader encountered an error!";
                notice.transform.GetChild(0).GetComponent<UILabel>().ProcessText();
                notice.transform.GetChild(3).GetComponent<UILabel>().text = "\nWHAT WENT WRONG";
                notice.transform.GetChild(3).GetComponent<UILabel>().ProcessText();
                notice.transform.GetChild(2).GetComponent<UILabel>().text = issue;
                notice.transform.GetChild(2).GetComponent<UILabel>().height = 550;
                notice.transform.GetChild(2).GetComponent<UILabel>().ProcessText();
                notice.transform.GetChild(4).GetComponent<UILabel>().text = possibleFixes;
                notice.transform.GetChild(4).GetComponent<UILabel>().fontSize = 24;
                notice.transform.GetChild(4).GetComponent<UILabel>().ProcessText();
                return;
            }

            StartCoroutine(CreateErrorMesageAfterLoadingMenu(issue, possibleFixes));
        }

        internal void DestroyJaLoader()
        {
            var loadingScreenCanvas = GameObject.Find("JaLoader Loading Screen Canvas");
            if(loadingScreenCanvas != null)
                Destroy(loadingScreenCanvas);
            FindObjectOfType<LoadingScreen>()?.DeleteLoadingScreen();
            Destroy(gameObject);
        }

        internal IEnumerator CreateErrorMesageAfterLoadingMenu(string issue, string possibleFixes)
        {
            while (SceneManager.GetActiveScene().buildIndex != 1)
                yield return null;

            CreateErrorMessage(issue, possibleFixes);

            yield return null;
        }
    }
}
