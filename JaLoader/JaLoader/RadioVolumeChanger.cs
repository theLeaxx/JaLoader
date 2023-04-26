using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    class RadioVolumeChanger : MonoBehaviour
    {
        AudioSource radioSource;
        public float volume;

        void Start()
        {
            radioSource = GameObject.Find("RadioFreq").GetComponent<AudioSource>();
        }

        void Update()
        {
            if (!radioSource.gameObject.activeSelf)
                return;

            if(Input.GetKeyDown(KeyCode.F6))
                FindObjectOfType<RadioFreqLogicC>().NextSong();

            radioSource.volume = volume;
        }
    }
}
