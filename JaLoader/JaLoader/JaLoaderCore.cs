using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JaLoader
{
    public class JaLoaderCore : MonoBehaviour
    {
        public static JaLoaderCore Instance { get; private set; }

        #region Declarations

        #endregion

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

            if (AnyMissingDLLs() != "None")
            {
                CreateErrorMessage($"\n\nDLL {AnyMissingDLLs()} was not found. You can try:", "Reinstalling JaLoader with JaPatcher\n\n\nCopying the files from JaPatcher's directory/Assets/Managed to Jalopy_Data/Managed");
                SceneManager.LoadScene("MainMenu");
                return;
            }

            AddEssentialScripts();

            Debug.Log("JaLoader Core initialized!");

            EventsManager.Instance.OnMenuLoad += OnMenuLoad;

            GameTweaks.Instance.SkipLanguage();
        }

        private void OnMenuLoad()
        {
            Debug.Log($"Selected language: {Language.CurrentLanguage()}");
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
            gameObject.AddComponent<ReferencesLoader>();
            gameObject.AddComponent<UIManager>();
            gameObject.AddComponent<ModManager>();
            gameObject.AddComponent<ModLoader>();
            gameObject.AddComponent<CustomObjectsManager>();
            gameObject.AddComponent<ExtrasManager>();

            utilities.AddComponent<ModHelper>();
            utilities.AddComponent<UncleHelper>();
            utilities.AddComponent<CustomRadioController>();
            utilities.AddComponent<PaintJobManager>();
            utilities.AddComponent<PartIconManager>();
            utilities.AddComponent<DiscordController>();
            utilities.AddComponent<AdjustmentsEditor>();
            utilities.AddComponent<GameTweaks>();
            utilities.AddComponent<Stopwatch>();
        }

        internal static string AnyMissingDLLs()
        {
            string[] requiredDLLs = new string[]
            {
                "0Harmony.dll",
                "HarmonyXInterop.dll",
                "NLayer.dll",
                "Mono.Cecil.dll",
                "Mono.Cecil.Mdb.dll",
                "Mono.Cecil.Pdb.dll",
                "Mono.Cecil.Rocks.dll",
                "MonoMod.Backports.dll",
                "MonoMod.RuntimeDetour.dll",
                "MonoMod.Utils.dll",
                "MonoMod.ILHelpers.dll"
            };

            var path = $@"{Application.dataPath}\Managed";

            foreach (string dll in requiredDLLs)
            {
                if (!File.Exists($@"{path}\{dll}"))
                    return dll;
            }

            return "None";
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

        internal IEnumerator CreateErrorMesageAfterLoadingMenu(string issue, string possibleFixes)
        {
            while (SceneManager.GetActiveScene().buildIndex != 1)
                yield return null;

            CreateErrorMessage(issue, possibleFixes);

            yield return null;
        }
    }
}
