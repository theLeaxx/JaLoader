using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    public class MenuCarRotate : MonoBehaviour
    {
        private readonly GameObject car = GameObject.Find("FrameHolder");
        private float speed = 1f;

        void Start()
        {
            if (ExtrasGarage.Instance.loadedGarage)
                return;

            GameObject.Find("R_Door").transform.parent = car.transform;
        }

        void Update() 
        {
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


            if (ExtrasGarage.Instance.loadedGarage)
                return;

            if (Input.GetKeyDown(KeyCode.R))
                car.transform.localEulerAngles = new Vector3(5.574578f, 276.8294f, 1.378559f);
        }
    }
}
