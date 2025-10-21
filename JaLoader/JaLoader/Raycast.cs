using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    public class Raycast : MonoBehaviour
    {
        public static Raycast Instance { get; private set; }
        public Camera CameraParent { get; private set; }
        public Transform CurrentlyLookingAt { get; private set; }
        private float maxRayDistance;
        private LayerMask layerMask;

        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);

            Instance = this;
            CameraParent = GetComponent<Camera>();
            DragRigidbodyC component = FindObjectOfType<DragRigidbodyC>();
            maxRayDistance = component.maxRayDistance;
            layerMask = component.myLayerMask;
        }

        void Update()
        {
            if (Physics.Raycast(CameraParent.ScreenPointToRay(Input.mousePosition), out var hitInfo, maxRayDistance, layerMask, QueryTriggerInteraction.Collide))
            {
                CurrentlyLookingAt = hitInfo.collider.transform;
            }
            else
                CurrentlyLookingAt = null;
        }
    }
}
