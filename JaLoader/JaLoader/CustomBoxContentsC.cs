using JaLoader.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace JaLoader
{
    public class CustomBoxContentsC : MonoBehaviour
    {
        private int boxSize;
        private GameObject[] supplies;
        private Transform[] slots;
        private bool customType = false;
        private int itemsInBox;
        private Dictionary<GoodType, List<GameObject>> itemList = new Dictionary<GoodType, List<GameObject>>();
        private Dictionary<GoodType, bool> usedTypes = new Dictionary<GoodType, bool>
        {
            { GoodType.Alcohol, false },
            { GoodType.Meds, false },
            { GoodType.Coffee, false },
            { GoodType.Textiles, false },
            { GoodType.Tobacco, false },
            { GoodType.Meat, false },
            { GoodType.None, false }
        };
        private BoxContentsC baseBox;

        public void Init(BoxContentsC _baseBox)
        {
            baseBox = _baseBox;

            boxSize = baseBox.boxSize;
            supplies = baseBox.supplies;
            slots = baseBox.slots;
            baseBox.spawnedItems = 0;

            Dictionary<GoodType, int> goodTypeMap = new Dictionary<GoodType, int>
            {
                { GoodType.Alcohol, 0 },
                { GoodType.Meds, 1 },
                { GoodType.Coffee, 2 },
                { GoodType.Textiles, 3 },
                { GoodType.Tobacco, 4 },
                { GoodType.Meat, 5 }
            };

            foreach (var entry in goodTypeMap)
            {
                GoodType type = entry.Key;
                int index = entry.Value;
                itemList[type] = new List<GameObject> { supplies[index] };
            }

            itemList[GoodType.None] = new List<GameObject>();

            foreach (GoodType type in Enum.GetValues(typeof(GoodType)))
                if (CustomObjectsManager.Instance.goodsObjects.ContainsKey(type))
                    itemList[type].AddRange(CustomObjectsManager.Instance.goodsObjects[type]);

            ClearAllSlotsBefore();

            StartLogic();
        }

        public void StartLogic()
        {
            int typeCount = Random.Range(1, 7);

            if((bool)SettingsManager.GetSettingValue("MultipleTypesInBoxes") == false)
                typeCount = 1;

            for (int i = 0; i < typeCount; i++)
                ChooseType();

            NewSpawnContents();
        }

        private void ClearAllSlotsBefore()
        {
            foreach (Transform slot in slots)
                foreach (Transform child in slot)
                    Destroy(child.gameObject);
        }

        private void ChooseType()
        {
            int contents = Random.Range(0, 7);

            switch (contents)
            {
                case 0: // alcohol
                    baseBox.alchol = true;
                    break;

                case 1: // meds
                    baseBox.pharma = true;
                    break;

                case 2: // coffee (used to be machine parts)
                    baseBox.machineParts = true;
                    break;

                case 3: // textiles
                    baseBox.textiles = true;
                    break;

                case 4: // tobacco
                    baseBox.tobacco = true;
                    break;

                case 5: // sausages/meat (used to be munitions)
                    baseBox.munitions = true;
                    break;

                case 6: // no type
                    customType = true;
                    break;
            }
        }

        private GameObject PickRandomObject(bool noTallOnes = false)
        {
            List<GoodType> enabledTypes = new List<GoodType>();
            if (baseBox.alchol) enabledTypes.Add(GoodType.Alcohol);
            if (baseBox.pharma) enabledTypes.Add(GoodType.Meds);
            if (baseBox.machineParts) enabledTypes.Add(GoodType.Coffee);
            if (baseBox.textiles) enabledTypes.Add(GoodType.Textiles);
            if (baseBox.tobacco) enabledTypes.Add(GoodType.Tobacco);
            if (baseBox.munitions) enabledTypes.Add(GoodType.Meat);
            if (customType && itemList[GoodType.None].Count > 0) enabledTypes.Add(GoodType.None);

            GoodType chosenType = enabledTypes[Random.Range(0, enabledTypes.Count)];
            usedTypes[chosenType] = true;
            List<GameObject> possibleItems = itemList[chosenType];

            if(!noTallOnes)
                return possibleItems[Random.Range(0, possibleItems.Count)];
            else
            {
                List<GameObject> filteredItems = new List<GameObject>();
                foreach (GameObject obj in possibleItems)
                {
                    ObjectPickupC pickup = obj.GetComponent<ObjectPickupC>();
                    if (pickup != null && pickup.dimensionY == 1)
                        filteredItems.Add(obj);
                }
                if (filteredItems.Count == 0)
                    return new GameObject();
                else
                    return filteredItems[Random.Range(0, filteredItems.Count)];
            }
        }

        private void RemoveTypeIfUnused()
        {
            if(baseBox.alchol && !usedTypes[GoodType.Alcohol])
                baseBox.alchol = false;

            if (baseBox.pharma && !usedTypes[GoodType.Meds])
                baseBox.pharma = false;

            if (baseBox.machineParts && !usedTypes[GoodType.Coffee])
                baseBox.machineParts = false;

            if (baseBox.textiles && !usedTypes[GoodType.Textiles])
                baseBox.textiles = false;

            if (baseBox.tobacco && !usedTypes[GoodType.Tobacco])
                baseBox.tobacco = false;

            if (baseBox.munitions && !usedTypes[GoodType.Meat])
                baseBox.munitions = false;

            if (customType && !usedTypes[GoodType.None])
                customType = false;
        }

        public void NewSpawnContents()
        {
            Console.LogDebug("JaLoader", $"Spawning {(boxSize == 0 ? "small" : boxSize == 1 ? "medium" : "large")} {(baseBox.padLock == null ? "box" : "crate")} contents");
            itemsInBox = Random.Range(0, slots.Length);
            if (itemsInBox == 0)
            {
                int num = Random.Range(0, 100);
                if (num <= 75)
                    itemsInBox++;
            }

            if (itemsInBox < slots.Length)
            {
                int num2 = Random.Range(0, 100);
                if (num2 <= 50)
                    itemsInBox++;
            }

            List<int> slotsToIgnoreDueToTallObjects = new List<int>();
            int maxTall = 0;
            switch (boxSize)
            {
                case 0:
                    maxTall = 1;
                    break;

                case 1:
                    maxTall = 3;
                    break;

                case 2:
                    maxTall = 6;
                    break;
            }
            for (int i = 0; i < slots.Length; i++)
            {
                if (slotsToIgnoreDueToTallObjects.Contains(i))
                {
                    itemsInBox--;
                    continue;
                }

                if (baseBox.spawnedItems >= itemsInBox)
                    break;

                GameObject randomObj = Instantiate(PickRandomObject(i >= maxTall));
                randomObj.transform.parent = slots[i];
                randomObj.transform.localPosition = Vector3.zero;
                randomObj.name = randomObj.name.Replace("(Clone)", "").Trim();
                randomObj.GetComponent<ObjectPickupC>().crateSpawned = true;
                baseBox.spawnedItems++;

                if (randomObj.GetComponent<ObjectPickupC>().dimensionY > 1)
                {
                    if (boxSize == 0)
                    {
                        randomObj.transform.localPosition = new Vector3(-0.04f, 0f, 0f);
                        randomObj.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
                    }
                    else
                    {
                        randomObj.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
                        if(i < slots.Length / 2)
                            slotsToIgnoreDueToTallObjects.Add(slots.Length / 2 + i);
                    }
                }
            }

            RemoveTypeIfUnused();
        }
    }
}
