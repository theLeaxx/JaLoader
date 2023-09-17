using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace JaLoader
{
    public class FPSCounter : MonoBehaviour    
    {
        private Text text;
        private float refreshRate = 0.5f; // in seconds

        private float timer;

        private void Awake()
        {
            text = GetComponent<Text>();
        }

        private void Update()
        {
            if (Time.unscaledTime > timer)
            {
                int fps = (int)(1f / Time.unscaledDeltaTime);
                text.text = $"{fps} FPS";
                timer = Time.unscaledTime + refreshRate;
            }
        }
    }
}
