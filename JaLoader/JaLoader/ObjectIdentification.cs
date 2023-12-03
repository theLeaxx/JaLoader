using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    public class ObjectIdentification : MonoBehaviour
    {
        public string ModID = "";
        public string ModName = "";
        public string Author = "";
        public string Version = "";

        public bool HasReceivedPartLogic;
        public bool HasReceivedBasicLogic;
        public bool IsExtra;
        public bool IsCustom;
        public int ExtraID;
        public int CustomID;
        public BoxSizes BoxSize;

        public Vector3 PartIconPositionAdjustment = Vector3.zero;
        public Vector3 PartIconRotationAdjustment = Vector3.zero;
        public Vector3 PartIconScaleAdjustment = Vector3.one;
    }

    public class HolderInformation : MonoBehaviour
    {
        public bool Installed;
        public int Weight;
    }
}
