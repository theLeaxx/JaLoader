using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    public class MarketManager : MonoBehaviour
    {
        private CustomObjectsManager customObjectsManager = CustomObjectsManager.Instance;

        private List<GameObject> modifiedMarkets = new List<GameObject>();

        void Awake()
        {
            EventsManager.Instance.OnRouteGenerated += AddRafts;
        }

        void Start()
        {
            AddRafts("", "", 0);
        }

        public void AddRafts(string startLocation, string endLocation, int distance)
        {

        }

        private void AddRaftsLogic()
        {

        }
    }
}
