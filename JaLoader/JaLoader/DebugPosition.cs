using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JaLoader
{
    public class DebugPosition : MonoBehaviour
    {
        Text text;

        private void Awake()
        {
            text = GetComponent<Text>();
        }

        private void Update()
        {
            if(!SettingsManager.Instance.DebugMode)
                return;

            if (SceneManager.GetActiveScene().buildIndex == 3)
            {
                text.text = $"Pos: {ModHelper.Instance.player.transform.position} | Rot: {ModHelper.Instance.player.transform.eulerAngles}";
            }
            else
                text.text = "";
        }
    }//129 47 -551
}// 168.9, 1.2, -482.6