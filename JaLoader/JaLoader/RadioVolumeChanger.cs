using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    class RadioVolumeChanger : MonoBehaviour
    {
        private AudioSource radioSource;
        public float volume;
        private bool muted;

        void Start()
        {
            radioSource = GameObject.Find("RadioFreq").GetComponent<AudioSource>();
        }

        void Update()
        {
            if (!radioSource.gameObject.activeSelf)
                return;

            if (Input.GetKeyDown(KeyCode.F6))
                FindObjectOfType<RadioFreqLogicC>().NextSong();

            if (Input.GetKeyDown(KeyCode.F7))
                muted = !muted;

            radioSource.volume = muted ? 0 : volume;
        }
    }
}
