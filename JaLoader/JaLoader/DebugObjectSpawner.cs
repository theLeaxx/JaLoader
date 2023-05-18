using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    public class DebugObjectSpawner : MonoBehaviour
    {
        //TODO: Get all vanilla objects
        //TODO: Make UI

        private CustomObjectsManager customObjects;
        
        public static DebugObjectSpawner Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            customObjects = CustomObjectsManager.Instance;
        }
    }
}
