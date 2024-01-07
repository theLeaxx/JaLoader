using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JaLoader
{
    public class DebugInfo : MonoBehaviour
    {
        private Text positionText;
        private Text lookingAtText;

        private bool showing = true;
        private DragRigidbodyC dragRigidbodyC;

        private void Awake()
        {
            positionText = transform.GetChild(0).GetComponent<Text>();
            lookingAtText = transform.GetChild(1).GetComponent<Text>();
            EventsManager.Instance.OnGameLoad += OnGameLoad;
        }

        private void Update()
        {
            if(!SettingsManager.Instance.DebugMode)
                return;

            if(Input.GetKeyDown(KeyCode.F3))
            {
                showing = !showing;
                positionText.gameObject.SetActive(showing);
                lookingAtText.gameObject.SetActive(showing);

                if (!showing)
                    return;
            }

            if (SceneManager.GetActiveScene().buildIndex == 3 && dragRigidbodyC != null)
            {
                positionText.text = $"Pos: {ModHelper.Instance.player.transform.position} | Rot: {ModHelper.Instance.player.transform.eulerAngles}";
                lookingAtText.text = $"Looking at: {dragRigidbodyC.debugLookingAt}";
            }
            else
            {
                positionText.text = "";
                lookingAtText.text = "";
            }
        }

        private void OnGameLoad()
        {
            dragRigidbodyC = FindObjectOfType<DragRigidbodyC>();
        }
    }//129 47 -551
}// 168.9, 1.2, -482.6