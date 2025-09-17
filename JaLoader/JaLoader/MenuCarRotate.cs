using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace JaLoader
{
    public class MenuCarRotate : MonoBehaviour
    {
        private GameObject car;
        private float speed = 1f;
        private bool isInMenu = false;

        void Awake()
        {
            EventsManager.Instance.OnMenuLoad += OnMenuLoad;
            EventsManager.Instance.OnLoadStart += OnLoadStart;
            isInMenu = true;
        }

        private void OnLoadStart()
        {
            isInMenu = false;
        }

        private void OnMenuLoad()
        {
            isInMenu = true;
            car = GameObject.Find("FrameHolder");

            if (AdjustmentsEditor.Instance.loadedViewingEditor)
                return;

            GameObject.Find("R_Door").transform.parent = car.transform;
        }

        void Update() 
        {
            if (!isInMenu)
                return;

            if (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.RightShift))
                return;

            if (Input.GetKey(KeyCode.LeftShift))
                speed = 2f;
            else
                speed = 1f;

            if (Input.GetKey(KeyCode.RightArrow))
                car.transform.Rotate(Vector3.up * speed);

            if (Input.GetKey(KeyCode.LeftArrow))
                car.transform.Rotate(Vector3.down * speed);

            if (AdjustmentsEditor.Instance.loadedViewingEditor)
                return;

            if (Input.GetKeyDown(KeyCode.R))
                car.transform.localEulerAngles = new Vector3(5.574578f, 276.8294f, 1.378559f);
        }
    }
}
