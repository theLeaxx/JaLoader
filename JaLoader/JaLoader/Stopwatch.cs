﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    public class Stopwatch : MonoBehaviour
    {
        internal static Stopwatch Instance;

        private void Awake()
        {
            Instance = this;
        }

        bool counting = false;
        float timePassedRaw = 0;

        public double timePassed = 0;
        public double totalTimePassed = 0;

        public void StartCounting()
        {
            counting = true;
            timePassed = 0;
            timePassedRaw = 0;
        }

        public void StopCounting() 
        {
            counting = false;
            timePassed = Math.Round(timePassedRaw, 3);

            totalTimePassed += timePassed;
            totalTimePassed = Math.Round(totalTimePassed, 3);
        }

        void Update()
        {
            if (counting)
                timePassedRaw += Time.deltaTime;
        }
    }
}
