﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using JaLoader.Common;

namespace JaLoader
{
    public class LoadingScreen : MonoBehaviour
    {
        public bool showing = false;
        private bool isFading = false;
        private RectTransform rt;

        private CanvasGroup canvasGroup;

        public bool useCircle = true;
        public bool dontDestroyOnLoad = false;

        private int secondsPassed;

        public void ShowLoadingScreen()
        {
            GameObject canvasGO = new GameObject("JaLoader Loading Screen Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvas.sortingLayerName = "LoadingScreen";
            canvas.sortingOrder = 999;

            CanvasScaler canvasScaler = canvasGO.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            GameObject panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            RectTransform panelRectTransform = panelGO.AddComponent<RectTransform>();

            canvasGroup = panelGO.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1;

            panelRectTransform.anchorMin = Vector2.zero;
            panelRectTransform.anchorMax = Vector2.one;
            panelRectTransform.sizeDelta = Vector2.zero;

            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 1);

            if(useCircle)
            {
                var loadingSprite = GameObject.Find("Canvas").transform.Find("Loading Sprite").GetComponent<Image>().sprite;

                var loadingIcon = new GameObject("Loading Icon");
                loadingIcon.transform.SetParent(panelGO.transform, false);
                loadingIcon.AddComponent<Image>().sprite = loadingSprite;
                rt = loadingIcon.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1, 0);
                rt.anchorMax = new Vector2(1, 0);
                rt.position = new Vector2(rt.position.x - 100, rt.position.y + 100);
            }    
            
            showing = true;

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(canvasGO);

            StartCoroutine(AddSeconds());
        }

        private IEnumerator AddSeconds()
        {
            while (showing)
            {
                yield return new WaitForSeconds(1);
                secondsPassed++;
            }
        }

        public void DeleteLoadingScreen()
        {
            if (!isFading)
            {
                isFading = true;
                StartCoroutine(FadeOut());
            }
        }

        private void Update()
        {
            if (showing && useCircle)
                rt.Rotate(0, 0, -150 * Time.deltaTime);

            if(secondsPassed >= 30)
                DeleteLoadingScreen();

            if(Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
                DeleteLoadingScreen();
        }

        private IEnumerator FadeOut()
        {
            StopCoroutine(AddSeconds());

            EventsManager.Instance.OnMenuFade();
            float elapsedTime = 0;
            float startAlpha = canvasGroup.alpha;

            while (elapsedTime < 1)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, elapsedTime / 1);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            canvasGroup.alpha = 0;
            isFading = false;

            JaLoaderSettings.LoadedFirstTime = true;

            Destroy(GameObject.Find("JaLoader Loading Screen Canvas"));
            Destroy(gameObject.GetComponent<LoadingScreen>());
        }       
    }
}
