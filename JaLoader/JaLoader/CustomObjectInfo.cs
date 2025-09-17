using JaLoader.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    public class CustomObjectInfo : MonoBehaviour
    {
        public string objName;
        public string objDescription;
        public string objRegistryName;
        public bool isPaintJob;

        public bool SpawnNoRegister = false;
        public GoodType SupplyType = GoodType.None;

        public bool CanFindInCrates;
        public bool CanBuyInDealership;
        public bool CanBuyInShop;
        public bool CanFindInJunkCars;

        private void Awake()
        {
            if (SpawnNoRegister)
                return;

            CustomObjectsManager.Instance.AddObjectToSpawned(gameObject, objRegistryName);
        }
    }
}
