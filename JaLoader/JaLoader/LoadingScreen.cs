using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine;

namespace JaLoader
{
    public class LoadingScreen : MonoBehaviour
    {
        public bool showing = false;
        RectTransform rt;

        public void ShowLoadingScreen()
        {
            GameObject canvasGO = new GameObject("JaLoader Loading Screen Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler canvasScaler = canvasGO.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            GameObject panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            RectTransform panelRectTransform = panelGO.AddComponent<RectTransform>();

            panelRectTransform.anchorMin = Vector2.zero;
            panelRectTransform.anchorMax = Vector2.one;
            panelRectTransform.sizeDelta = Vector2.zero;

            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 1);

            var loadingSprite = GameObject.Find("Canvas").transform.Find("Loading Sprite").GetComponent<Image>().sprite;

            var loadingIcon = new GameObject("Loading Icon");
            loadingIcon.transform.SetParent(panelGO.transform, false);
            loadingIcon.AddComponent<Image>().sprite = loadingSprite;
            rt = loadingIcon.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.position = new Vector2(rt.position.x - 100, rt.position.y + 100);
            
            showing = true;
        }

        public void DeleteLoadingScreen()
        {
            Destroy(GameObject.Find("JaLoader Loading Screen Canvas"));
            Destroy(gameObject.GetComponent<LoadingScreen>());
        }

        private void Update()
        {
            if (showing)
            {
                rt.Rotate(0, 0, -250 * Time.deltaTime);
            }
        }
    }
}
