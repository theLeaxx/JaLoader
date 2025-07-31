using JaLoader.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace JaLoaderClassic
{
    public class JaLoaderCore : MonoBehaviour
    {
        public static JaLoaderCore Instance { get; private set; }

        private void Start()
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
                //CreateErrorMessage($"\n\nDLL {CoreUtils.AnyMissingDLLs()} was not found. You can try:", "Reinstalling JaLoader with JaPatcher\n\n\nCopying the files from JaPatcher's directory/Assets/Managed to Jalopy_Data/Managed");
                Application.LoadLevel(Application.loadedLevel + 1);
                return;
            }

            //AddEssentialScripts();

            Debug.Log("JaLoader Core initialized!");
            Application.LoadLevel(Application.loadedLevel + 1);

            Assembly modAssembly = Assembly.LoadFrom(@"C:\Users\gdlea\Documents\Jalopy\Mods\RuntimeInspector.dll");

            Type[] allModTypes = modAssembly.GetTypes();

            Type modType = allModTypes.FirstOrDefault(t => t.BaseType != null && t.BaseType.Name == "ModClassic");
            GameObject ModObject = Instantiate(new GameObject()) as GameObject;
            ModObject.transform.parent = null;
            ModObject.SetActive(true);
            DontDestroyOnLoad(ModObject);

            ModObject.AddComponent(modType);

            StartCoroutine(Wait());

            //EventsManager.Instance.OnMenuLoad += OnMenuLoad;

            //GameTweaks.Instance.SkipLanguage();
        }

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(1);
            Debug.Log("Adding Mods button to the main menu...");

            var obj = GameObject.Find("UI Root").transform.Find("FrontPage/Continue");
            var ModsButton = Instantiate(obj.gameObject) as GameObject;
            ModsButton.transform.parent = obj.transform.parent;
            ModsButton.transform.localPosition = new Vector3(0, -80, 0);
            ModsButton.transform.localScale = obj.transform.localScale;
            ModsButton.transform.rotation = obj.transform.rotation;
            ModsButton.GetComponent<UILabel>().text = "Mods";
            ModsButton.GetComponent<UILabel>().ProcessText();
            ModsButton.GetComponent<UIButton>().onClick.Clear();
            ModsButton.GetComponent<UIButton>().onClick.Add(new EventDelegate(() =>
            {
                Debug.Log("Mods button clicked!");
            }));

            MainMenuBook book = FindObjectOfType<MainMenuBook>();
            var list = book.frontPageContents.ToList();
            list.Add(ModsButton);
            book.frontPageContents = list.ToArray();

            yield return new WaitForSeconds(2);
        }
    }
}
