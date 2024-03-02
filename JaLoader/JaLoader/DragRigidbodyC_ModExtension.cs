using JaLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//most of the code here is taken from the original DragRigidbodyC script
namespace JaLoader
{
    public class DragRigidbodyC_ModExtension : MonoBehaviour
    {
        private DragRigidbodyC dragRigidbodyC;

        private GameObject _camera;
        private bool uiIsOn;

        private GameObject[] componentTitles;
        private GameObject componentHeader;
        private GameObject componentUI;
        private GameObject hitComponent;
        private GameObject carLogic;

        private AudioClip paperSFX;

        private Vector3[] componentUIPos;
        private Color gray;
        private Color orange;
        private Color green;
        private Color red;
        private Color blue;
        private LayerMask myLayerMask;

        private bool[] valueUsed = new bool[23];
        private float maxRayDistance;

        public Transform lookingAt;

        private void Start()
        {
            _camera = Camera.main.gameObject;
            dragRigidbodyC = gameObject.GetComponent<DragRigidbodyC>();

            maxRayDistance = dragRigidbodyC.maxRayDistance;
            myLayerMask = dragRigidbodyC.myLayerMask;

            componentHeader = dragRigidbodyC.componentHeader;
            componentTitles = dragRigidbodyC.componentTitles;
            componentUI = dragRigidbodyC.componentUI;
            componentUIPos = dragRigidbodyC.componentUIPos;
            gray = dragRigidbodyC.gray;
            paperSFX = dragRigidbodyC.paperSFX;
            valueUsed = dragRigidbodyC.valueUsed;
            orange = dragRigidbodyC.orange;
            green = dragRigidbodyC.green;
            red = dragRigidbodyC.red;
            blue = dragRigidbodyC.blue;
            carLogic = dragRigidbodyC.carLogic;
        }

        private void HideUI()
        {
            uiIsOn = false;

            iTween.Stop(componentUI);
            iTween.MoveTo(componentUI, iTween.Hash(new object[]
            {
                    "y",
                    componentUIPos[1].y,
                    "time",
                    0.15,
                    "islocal",
                    true,
                    "easetype",
                    "easeinback",
                    "oncomplete",
                    "ComponentUIOff",
                    "oncompletetarget",
                    gameObject
            }));
        }

        private void ShowUI()
        {
            uiIsOn = true;
            iTween.Stop(componentUI);
            iTween.MoveTo(componentUI, iTween.Hash(new object[]
            {
                    "y",
                    componentUIPos[0].y,
                    "time",
                    0.3,
                    "islocal",
                    true,
                    "easetype",
                    "easeoutback",
                    "oncomplete",
                    "ComponentUIOn",
                    "oncompletetarget",
                    gameObject
            }));
            GetComponent<AudioSource>().PlayOneShot(paperSFX, 0.8f);
        }

        private void Update()
        {
            Ray ray = _camera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;

            if (!Physics.Raycast(ray, out raycastHit, maxRayDistance, myLayerMask, QueryTriggerInteraction.Collide) && uiIsOn)
            {
                HideUI();
                lookingAt = null;
                return;
            }

            lookingAt = raycastHit.transform;

            if (raycastHit.collider && raycastHit.collider.tag == "Pickup" && raycastHit.collider.GetComponent<ObjectPickupC>() && raycastHit.collider.GetComponent<CustomObjectInfo>())
            {
                if (!uiIsOn)
                {
                    ShowUI();
                }

                dragRigidbodyC.hitComponent = raycastHit.collider.gameObject;
                hitComponent = raycastHit.collider.gameObject;
                componentUI.SetActive(true);
                ObjectPickupC component = raycastHit.collider.GetComponent<ObjectPickupC>();
                componentHeader.GetComponent<UILabel>().text = raycastHit.collider.GetComponent<CustomObjectInfo>().objName;
                componentHeader.GetComponent<UILabel>().color = gray;
                componentTitles[7].GetComponent<UILabel>().text = raycastHit.collider.GetComponent<CustomObjectInfo>().objDescription;
                componentTitles[7].GetComponent<UILabel>().color = gray;
                componentTitles[0].GetComponent<UILabel>().text = string.Empty;
                componentTitles[1].GetComponent<UILabel>().text = string.Empty;
                componentTitles[2].GetComponent<UILabel>().text = string.Empty;
                componentTitles[3].GetComponent<UILabel>().text = string.Empty;
                componentTitles[4].GetComponent<UILabel>().text = string.Empty;
                if (component.isPurchased)
                {
                    componentTitles[5].GetComponent<UILabel>().text = string.Concat(new string[]
                    {
                    Language.Get("ui_pickup_14", "Inspector_UI"),
                    " : ",
                    component.sellValue.ToString(),
                    "\n(",
                    Language.Get("ui_pickup_15", "Inspector_UI"),
                    " :  : ",
                    component.buyValue.ToString(),
                    ")"
                    });
                }
                else
                {
                    componentTitles[5].GetComponent<UILabel>().text = Language.Get("ui_pickup_15", "Inspector_UI") + " : " + component.buyValue.ToString();
                }
                componentTitles[6].GetComponent<UILabel>().text = string.Empty;
                componentTitles[8].GetComponent<UILabel>().text = string.Empty;

                if (raycastHit.collider.GetComponent<EngineComponentC>())
                {
                    SetEngineCompUI1();
                    dragRigidbodyC.valueUsed = valueUsed;
                }

                return;
            }
            else
            {
                if (uiIsOn)
                    HideUI();
            }    
        }

        #region Original DragRigidbodyC code with slight modifications
        public void SetEngineCompUI1()
        {
            if (hitComponent == null)
            {
                return;
            }

            EngineComponentC component = hitComponent.GetComponent<EngineComponentC>();
            ObjectPickupC component2 = hitComponent.GetComponent<ObjectPickupC>();
            for (int i = 0; i < valueUsed.Length; i++)
            {
                valueUsed[i] = false;
            }

            componentTitles[0].GetComponent<UILabel>().text = string.Empty;
            componentTitles[1].GetComponent<UILabel>().text = string.Empty;
            componentTitles[2].GetComponent<UILabel>().text = string.Empty;
            componentTitles[3].GetComponent<UILabel>().text = string.Empty;
            componentTitles[4].GetComponent<UILabel>().text = string.Empty;
            componentTitles[5].GetComponent<UILabel>().text = Language.Get((!component2.isPurchased) ? "ui_pickup_15" : "ui_pickup_14", "Inspector_UI") + " : " + component2.sellValue;
            if (component2.sellValue < component2.originalSellValue * 0.67f && component2.sellValue > 0f)
            {
                componentTitles[5].GetComponent<UILabel>().color = orange;
            }
            else if (component2.sellValue == 0f)
            {
                componentTitles[5].GetComponent<UILabel>().color = red;
            }
            else
            {
                componentTitles[5].GetComponent<UILabel>().color = blue;
            }

            if (hitComponent == null || !hitComponent.GetComponent<EngineComponentC>())
            {
                return;
            }

            componentTitles[6].GetComponent<UILabel>().text = Language.Get("ui_pickup_21", "Inspector_UI") + " : " + Mathf.Round(component.Condition) + "/" + component.durability;
            double num = (double)component.durability * 0.34;
            double num2 = (double)component.durability * 0.67;
            if (component.Condition == 0f)
            {
                componentTitles[6].GetComponent<UILabel>().color = red;
            }

            if ((double)component.Condition < num)
            {
                componentTitles[6].GetComponent<UILabel>().color = red;
            }
            else if ((double)component.Condition > num && (double)component.Condition < num2)
            {
                componentTitles[6].GetComponent<UILabel>().color = orange;
            }
            else if ((double)component.Condition > num2)
            {
                componentTitles[6].GetComponent<UILabel>().color = green;
            }

            componentTitles[8].GetComponent<UILabel>().text = component.uniqueProperty;
            if (component.totalFuelAmount > 0f)
            {
                float f = component.totalFuelAmount;
                if (carLogic.GetComponent<CarPerformanceC>().installedFuelTank != null)
                {
                    f = component.totalFuelAmount - carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount;
                }

                if ((bool)hitComponent.transform.parent && (bool)hitComponent.transform.parent.GetComponent<HoldingLogicC>() && hitComponent.transform.parent.GetComponent<HoldingLogicC>().gateDropOff)
                {
                    componentTitles[0].GetComponent<UILabel>().text = Mathf.Round(component.currentFuelAmount) + " / " + component.totalFuelAmount + " " + Language.Get("ui_pickup_22", "Inspector_UI");
                    valueUsed[0] = true;
                    SetEngineCompUI2();
                    return;
                }

                if (carLogic.GetComponent<CarPerformanceC>().installedFuelTank != null)
                {
                    if (component.totalFuelAmount < carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_23", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = red;
                    }
                    else if (component.totalFuelAmount == carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount)
                    {
                        if (component2.isInEngine)
                        {
                            componentTitles[0].GetComponent<UILabel>().text = Mathf.Round(component.currentFuelAmount) + " / " + component.totalFuelAmount + " " + Language.Get("ui_pickup_50", "Inspector_UI");
                        }
                        else
                        {
                            componentTitles[0].GetComponent<UILabel>().text = f + " " + Language.Get("ui_pickup_23", "Inspector_UI");
                        }

                        componentTitles[0].GetComponent<UILabel>().color = blue;
                    }
                    else if (component.totalFuelAmount > carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount)
                    {
                        if (component.totalFuelAmount == 0f)
                        {
                            componentTitles[0].GetComponent<UILabel>().text = Mathf.Abs(f) + " " + Language.Get("ui_pickup_23", "Inspector_UI");
                            componentTitles[0].GetComponent<UILabel>().color = green;
                        }
                        else
                        {
                            componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_23", "Inspector_UI");
                            componentTitles[0].GetComponent<UILabel>().color = green;
                        }
                    }
                }
                else
                {
                    componentTitles[0].GetComponent<UILabel>().text = f + " " + Language.Get("ui_pickup_23", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = green;
                }

                valueUsed[0] = true;
                SetEngineCompUI2();
            }
            else if (component.totalFuelAmount > 0f && !component2.isInEngine && !valueUsed[17])
            {
                float currentFuelAmount = component.currentFuelAmount;
                if (carLogic.GetComponent<CarPerformanceC>().installedFuelTank != null)
                {
                    currentFuelAmount = component.currentFuelAmount - carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount;
                    if (component.currentFuelAmount < carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_24", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = red;
                    }
                    else if (component.currentFuelAmount == carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = Mathf.Round(currentFuelAmount) + " " + Language.Get("ui_pickup_24", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = blue;
                    }
                    else if (component.currentFuelAmount > carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount)
                    {
                        if (component.currentFuelAmount == 0f)
                        {
                            componentTitles[0].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_24", "Inspector_UI");
                            componentTitles[0].GetComponent<UILabel>().color = green;
                        }
                        else
                        {
                            componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_24", "Inspector_UI");
                            componentTitles[0].GetComponent<UILabel>().color = green;
                        }
                    }
                }
                else
                {
                    componentTitles[0].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_24", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = green;
                }

                valueUsed[17] = true;
                SetEngineCompUI2();
            }
            else if (component.totalFuelAmount > 0f && !valueUsed[19])
            {
                if (component.fuelMix == 0)
                {
                    componentTitles[0].GetComponent<UILabel>().text = Language.Get("ui_pickup_25", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = red;
                }

                if (component.fuelMix == 1)
                {
                    componentTitles[0].GetComponent<UILabel>().text = Language.Get("ui_pickup_26", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = orange;
                }

                if (component.fuelMix == 2)
                {
                    componentTitles[0].GetComponent<UILabel>().text = Language.Get("ui_pickup_27", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = green;
                }

                if (component.fuelMix == 3)
                {
                    componentTitles[0].GetComponent<UILabel>().text = Language.Get("ui_pickup_28", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = orange;
                }

                valueUsed[19] = true;
                SetEngineCompUI2();
            }
            else if (component.fuelConsumptionRate > 0f && !valueUsed[1])
            {
                float f2 = component.fuelConsumptionRate - carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate;
                if (component.fuelConsumptionRate < carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = red;
                }
                else if (component.fuelConsumptionRate == carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = component.fuelConsumptionRate + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = f2 + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                    }

                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (component.fuelConsumptionRate > carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate == 0f)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = Mathf.Abs(f2) + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[1] = true;
                SetEngineCompUI2();
            }
            else if (component.initialFuelConsumptionAmount > 0f)
            {
                float f3 = component.initialFuelConsumptionAmount - carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount;
                if (component.initialFuelConsumptionAmount < carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = green;
                }
                else if (component.initialFuelConsumptionAmount == carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = component.initialFuelConsumptionAmount + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = f3 + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                    }

                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (component.initialFuelConsumptionAmount > carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount == 0f)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = Mathf.Abs(f3) + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = red;
                    }
                }

                valueUsed[2] = true;
                SetEngineCompUI2();
            }
            else if (component.ignitionTimer != 0f)
            {
                float f4 = component.ignitionTimer - carLogic.GetComponent<CarPerformanceC>().carIgnitionTime;
                if (component.ignitionTimer < carLogic.GetComponent<CarPerformanceC>().carIgnitionTime)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = green;
                }
                else if (component.ignitionTimer == carLogic.GetComponent<CarPerformanceC>().carIgnitionTime)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = component.ignitionTimer + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = f4 + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                    }

                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (component.ignitionTimer > carLogic.GetComponent<CarPerformanceC>().carIgnitionTime)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carIgnitionTime == 0f)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = Mathf.Abs(f4) + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = red;
                    }
                }

                valueUsed[3] = true;
                SetEngineCompUI2();
            }
            else if (component.acceleration > 0f)
            {
                float f5 = component.acceleration - carLogic.GetComponent<CarPerformanceC>().carAcceleration;
                if (component.acceleration < carLogic.GetComponent<CarPerformanceC>().carAcceleration)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = green;
                }
                else if (component.acceleration == carLogic.GetComponent<CarPerformanceC>().carAcceleration)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = component.acceleration + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = f5 + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                    }

                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (component.acceleration > carLogic.GetComponent<CarPerformanceC>().carAcceleration)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carAcceleration == 0f)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = Mathf.Abs(f5) + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = red;
                    }
                }

                valueUsed[4] = true;
                SetEngineCompUI2();
            }
            else if (component.TopSpeed > 0f)
            {
                float f6 = component.TopSpeed - carLogic.GetComponent<CarPerformanceC>().carTopSpeed;
                if (component.TopSpeed < carLogic.GetComponent<CarPerformanceC>().carTopSpeed)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = red;
                }
                else if (component.TopSpeed == carLogic.GetComponent<CarPerformanceC>().carTopSpeed)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = component.TopSpeed + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = f6 + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                    }

                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (component.TopSpeed > carLogic.GetComponent<CarPerformanceC>().carTopSpeed)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carTopSpeed == 0f)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = Mathf.Abs(f6) + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[5] = true;
                SetEngineCompUI2();
            }
            else if (component.turnRate > 0f)
            {
                componentTitles[0].GetComponent<UILabel>().text = "+ " + component.turnRate + " " + Language.Get("ui_pickup_35", "Inspector_UI");
                componentTitles[0].GetComponent<UILabel>().color = green;
                valueUsed[6] = true;
                SetEngineCompUI2();
            }
            else if (component.turnRate < 0f)
            {
                componentTitles[0].GetComponent<UILabel>().text = "- " + component.turnRate + " " + Language.Get("ui_pickup_35", "Inspector_UI");
                componentTitles[0].GetComponent<UILabel>().color = red;
                valueUsed[6] = true;
                SetEngineCompUI2();
            }
            else if (component.roadGrip > 0f)
            {
                componentTitles[0].GetComponent<UILabel>().text = (component.roadGrip / 0.0044f).ToString("F0") + " " + Language.Get("ui_pickup_53", "Inspector_UI");
                componentTitles[4].GetComponent<UILabel>().color = blue;
                valueUsed[7] = true;
                SetEngineCompUI2();
            }
            else if (component.wetGrip > 0f && !valueUsed[21])
            {
                componentTitles[0].GetComponent<UILabel>().text = (component.wetGrip / 0.0044f).ToString("F0") + " " + Language.Get("ui_pickup_36", "Inspector_UI");
                componentTitles[0].GetComponent<UILabel>().color = blue;
                valueUsed[21] = true;
                SetEngineCompUI2();
            }
            else if (component.offRoadGrip > 0f && !valueUsed[22])
            {
                componentTitles[0].GetComponent<UILabel>().text = (component.offRoadGrip / 0.0044f).ToString("F0") + " " + Language.Get("ui_pickup_54", "Inspector_UI");
                componentTitles[0].GetComponent<UILabel>().color = blue;
                valueUsed[22] = true;
                SetEngineCompUI2();
            }
            else if (component.compoundType > 0)
            {
                if (component.compoundType == 1)
                {
                    componentTitles[0].GetComponent<UILabel>().text = " " + Language.Get("ui_pickup_37", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (component.compoundType == 2)
                {
                    componentTitles[0].GetComponent<UILabel>().text = " " + Language.Get("ui_pickup_38", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (component.compoundType == 3)
                {
                    componentTitles[0].GetComponent<UILabel>().text = " " + Language.Get("ui_pickup_39", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }

                valueUsed[20] = true;
                SetEngineCompUI2();
            }
            else if (component.dirtAccumilation > 0f)
            {
                componentTitles[0].GetComponent<UILabel>().text = "+ " + component.dirtAccumilation + " " + Language.Get("ui_pickup_40", "Inspector_UI");
                componentTitles[0].GetComponent<UILabel>().color = green;
                valueUsed[10] = true;
                SetEngineCompUI2();
            }
            else if (component.dirtAccumilation < 0f)
            {
                componentTitles[0].GetComponent<UILabel>().text = "- " + component.dirtAccumilation + " " + Language.Get("ui_pickup_40", "Inspector_UI");
                componentTitles[0].GetComponent<UILabel>().color = red;
                valueUsed[10] = true;
                SetEngineCompUI2();
            }
            else if (component.totalWaterCharges > 0 && !valueUsed[11])
            {
                float num3 = 0.083f * (float)component.totalWaterCharges;
                float num4 = 0.083f * (float)component.currentWaterCharges;
                float num5 = 0f;
                float num6 = 0f;
                float num7 = num3 - num5;
                if (carLogic.GetComponent<CarPerformanceC>().waterTankObj != null)
                {
                    num5 = 0.083f * (float)carLogic.GetComponent<CarPerformanceC>().waterTankObj.GetComponent<EngineComponentC>().totalWaterCharges;
                    num6 = 0.083f * (float)carLogic.GetComponent<CarPerformanceC>().waterTankObj.GetComponent<EngineComponentC>().currentWaterCharges;
                    num7 = num3 - num5;
                }

                if (num3 < num5)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(num7 * 10f) / 10f) + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = red;
                }
                else if (num3 == num5)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(num4 * 10f) / 10f) + " / " + Mathf.Abs(Mathf.Round(num3 * 10f) / 10f) + " " + Language.Get("ui_pickup_42", "Inspector_UI");
                    }
                    else
                    {
                        float num8 = Mathf.Round(num7 * 10f) / 10f;
                        componentTitles[0].GetComponent<UILabel>().text = num8 + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                    }

                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (num3 > num5)
                {
                    if (component.totalWaterCharges == 0)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(num3 * 10f) / 10f) + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(num7 * 10f) / 10f) + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[11] = true;
                SetEngineCompUI2();
            }
            else if (component.totalWaterCharges > 0 && !valueUsed[18])
            {
                float num9 = 0.083f * (float)component.currentWaterCharges;
                float num10 = 0f;
                if (carLogic.GetComponent<CarPerformanceC>().waterTankObj != null)
                {
                    num10 = 0.083f * (float)carLogic.GetComponent<CarPerformanceC>().waterTankObj.GetComponent<EngineComponentC>().currentWaterCharges;
                }

                float num11 = num9 - num10;
                if (num9 < num10)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(num11 * 10f) / 10f) + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = red;
                }
                else if (num9 == num10)
                {
                    float num12 = Mathf.Round(num11 * 10f) / 10f;
                    componentTitles[0].GetComponent<UILabel>().text = num12 + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (num9 > num10)
                {
                    if (component.totalWaterCharges == 0)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(num9 * 10f) / 10f) + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(num11 * 10f) / 10f) + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[18] = true;
                SetEngineCompUI2();
            }
            else if (component.storage > 0f)
            {
                componentTitles[0].GetComponent<UILabel>().text = "+ " + component.storage + " " + Language.Get("ui_pickup_44", "Inspector_UI");
                componentTitles[0].GetComponent<UILabel>().color = green;
                valueUsed[12] = true;
                SetEngineCompUI2();
            }
            else if (component.storage < 0f)
            {
                componentTitles[0].GetComponent<UILabel>().text = "- " + component.storage + " " + Language.Get("ui_pickup_44", "Inspector_UI");
                componentTitles[0].GetComponent<UILabel>().color = red;
                valueUsed[12] = true;
                SetEngineCompUI2();
            }
            else if (component.damageResistance > 0f)
            {
                componentTitles[0].GetComponent<UILabel>().text = "+ " + component.damageResistance + " " + Language.Get("ui_pickup_45", "Inspector_UI");
                componentTitles[0].GetComponent<UILabel>().color = green;
                valueUsed[13] = true;
                SetEngineCompUI2();
            }
            else if (component.damageResistance < 0f)
            {
                componentTitles[0].GetComponent<UILabel>().text = "- " + component.damageResistance + " " + Language.Get("ui_pickup_45", "Inspector_UI");
                componentTitles[0].GetComponent<UILabel>().color = red;
                valueUsed[13] = true;
                SetEngineCompUI2();
            }
            else if (component.engineWearRate > 0f)
            {
                float num13 = component.engineWearRate - carLogic.GetComponent<CarPerformanceC>().carEngineWearRate;
                if (component.engineWearRate < carLogic.GetComponent<CarPerformanceC>().carEngineWearRate)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(num13 * 100f) + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = red;
                }
                else if (component.engineWearRate == carLogic.GetComponent<CarPerformanceC>().carEngineWearRate)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = Mathf.Abs(component.engineWearRate * 100f) + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = num13 + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                    }

                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (component.engineWearRate > carLogic.GetComponent<CarPerformanceC>().carEngineWearRate)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carEngineWearRate == 0f)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = Mathf.Abs(num13 * 100f) + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(num13 * 100f) + " " + Language.Get("ui_pickup_47", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[14] = true;
                SetEngineCompUI2();
            }
            else if (component.isBattery && !valueUsed[16])
            {
                float f7 = 0f;
                float num14 = 0f;
                if (carLogic.GetComponent<CarPerformanceC>().installedBattery != null)
                {
                    f7 = component.charge - carLogic.GetComponent<CarPerformanceC>().installedBattery.GetComponent<EngineComponentC>().charge;
                    num14 = carLogic.GetComponent<CarPerformanceC>().installedBattery.GetComponent<EngineComponentC>().charge;
                }

                if ((bool)hitComponent.transform.parent && (bool)hitComponent.transform.parent.GetComponent<HoldingLogicC>() && hitComponent.transform.parent.GetComponent<HoldingLogicC>().gateDropOff)
                {
                    componentTitles[0].GetComponent<UILabel>().text = Mathf.Round(component.charge) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                    valueUsed[16] = true;
                    SetEngineCompUI2();
                    return;
                }

                if (component.charge < num14)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(f7)) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = red;
                }
                else if (component.charge == num14)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = Mathf.Round(component.charge) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = Mathf.Round(f7) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                    }

                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (component.charge > num14)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carEngineWearRate == 0f)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(f7)) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(f7)) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                        componentTitles[0].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[16] = true;
                SetEngineCompUI2();
            }
            else if (component.weight > 0f)
            {
                valueUsed[15] = true;
                SetEngineCompUI2();
                SetEngineComponentWeight1();
            }
        }

        public void SetEngineComponentWeight1()
        {
            EngineComponentC component = hitComponent.GetComponent<EngineComponentC>();
            ObjectPickupC component2 = hitComponent.GetComponent<ObjectPickupC>();
            if (component.ignitionTimer != 0f)
            {
                float f = component.weight - carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = f + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.acceleration != 0f)
            {
                float f2 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedEngineWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedEngineWeight)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedEngineWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = f2 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedEngineWeight)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.fuelConsumptionRate != 0f)
            {
                float f3 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = f3 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.totalFuelAmount != 0f)
            {
                float f4 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = f4 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.engineWearRate != 0f)
            {
                float f5 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = f5 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.isBattery)
            {
                float f6 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = f6 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = red;
                }
            }
            else
            {
                if (component.totalWaterCharges == 0)
                {
                    return;
                }

                float f7 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "- " + Mathf.Abs(f7) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[0].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[0].GetComponent<UILabel>().text = f7 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[0].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight)
                {
                    componentTitles[0].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f7) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[0].GetComponent<UILabel>().color = red;
                }
            }
        }

        public void SetEngineComponentWeight2()
        {
            EngineComponentC component = hitComponent.GetComponent<EngineComponentC>();
            ObjectPickupC component2 = hitComponent.GetComponent<ObjectPickupC>();
            if (component.ignitionTimer != 0f)
            {
                float f = component.weight - carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = f + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.acceleration != 0f)
            {
                float f2 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedEngineWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedEngineWeight)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedEngineWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = f2 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedEngineWeight)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.fuelConsumptionRate != 0f)
            {
                float f3 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = f3 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.totalFuelAmount != 0f)
            {
                float f4 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = f4 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.engineWearRate != 0f)
            {
                float f5 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = f5 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.isBattery)
            {
                float f6 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = f6 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = red;
                }
            }
            else
            {
                if (component.totalWaterCharges == 0)
                {
                    return;
                }

                float f7 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(f7) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = f7 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f7) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = red;
                }
            }
        }

        public void SetEngineComponentWeight3()
        {
            EngineComponentC component = hitComponent.GetComponent<EngineComponentC>();
            ObjectPickupC component2 = hitComponent.GetComponent<ObjectPickupC>();
            if (component.ignitionTimer != 0f)
            {
                float f = component.weight - carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = f + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.acceleration != 0f)
            {
                float f2 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedEngineWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedEngineWeight)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedEngineWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = f2 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedEngineWeight)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.fuelConsumptionRate != 0f)
            {
                float f3 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = f3 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.totalFuelAmount != 0f)
            {
                float f4 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = f4 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.engineWearRate != 0f)
            {
                float f5 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = f5 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.isBattery)
            {
                float f6 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = f6 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = red;
                }
            }
            else
            {
                if (component.totalWaterCharges == 0)
                {
                    return;
                }

                float f7 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(f7) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = f7 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f7) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = red;
                }
            }
        }

        public void SetEngineComponentWeight4()
        {
            EngineComponentC component = hitComponent.GetComponent<EngineComponentC>();
            ObjectPickupC component2 = hitComponent.GetComponent<ObjectPickupC>();
            if (component.ignitionTimer != 0f)
            {
                float f = component.weight - carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = f + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.acceleration != 0f)
            {
                float f2 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedEngineWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedEngineWeight)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedEngineWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = f2 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedEngineWeight)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.fuelConsumptionRate != 0f)
            {
                float f3 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = f3 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.totalFuelAmount != 0f)
            {
                float f4 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = f4 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.engineWearRate != 0f)
            {
                float f5 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = f5 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.isBattery)
            {
                float f6 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = f6 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = red;
                }
            }
            else
            {
                if (component.totalWaterCharges == 0)
                {
                    return;
                }

                float f7 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(f7) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = f7 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f7) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = red;
                }
            }
        }

        public void SetEngineComponentWeight5()
        {
            EngineComponentC component = hitComponent.GetComponent<EngineComponentC>();
            ObjectPickupC component2 = hitComponent.GetComponent<ObjectPickupC>();
            if (component.ignitionTimer != 0f)
            {
                float f = component.weight - carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = f + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedIgnitionCoilWeight)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.acceleration != 0f)
            {
                float f2 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedEngineWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedEngineWeight)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedEngineWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = f2 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedEngineWeight)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.fuelConsumptionRate != 0f)
            {
                float f3 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = f3 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedCarburettorWeight)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.totalFuelAmount != 0f)
            {
                float f4 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = f4 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedFuelTankWeight)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.engineWearRate != 0f)
            {
                float f5 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = f5 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedAirFilterWeight)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = red;
                }
            }
            else if (component.isBattery)
            {
                float f6 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = f6 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedBatteryWeight)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = red;
                }
            }
            else
            {
                if (component.totalWaterCharges == 0)
                {
                    return;
                }

                float f7 = component.weight - carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight;
                if (component.weight < carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(f7) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = green;
                }
                else if (component.weight == carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = component.weight + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = f7 + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    }

                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (component.weight > carLogic.GetComponent<CarPerformanceC>().installedWaterTankWeight)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f7) + " " + Language.Get("ui_pickup_49", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = red;
                }
            }
        }

        public void SetEngineCompUI2()
        {
            EngineComponentC component = hitComponent.GetComponent<EngineComponentC>();
            ObjectPickupC component2 = hitComponent.GetComponent<ObjectPickupC>();
            if (component.totalFuelAmount > 0f && !valueUsed[0] && component.totalFuelAmount > 0f)
            {
                float f = component.totalFuelAmount;
                if (carLogic.GetComponent<CarPerformanceC>().installedFuelTank != null)
                {
                    f = component.totalFuelAmount - carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount;
                }

                if ((bool)hitComponent.transform.parent && (bool)hitComponent.transform.parent.GetComponent<HoldingLogicC>() && hitComponent.transform.parent.GetComponent<HoldingLogicC>().gateDropOff)
                {
                    componentTitles[1].GetComponent<UILabel>().text = Mathf.Round(component.currentFuelAmount) + " / " + component.totalFuelAmount + " " + Language.Get("ui_pickup_50", "Inspector_UI");
                    valueUsed[0] = true;
                    SetEngineCompUI2();
                    return;
                }

                if (carLogic.GetComponent<CarPerformanceC>().installedFuelTank != null)
                {
                    if (component.totalFuelAmount < carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = red;
                    }
                    else if (component.totalFuelAmount == carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount)
                    {
                        if (component2.isInEngine)
                        {
                            componentTitles[1].GetComponent<UILabel>().text = Mathf.Round(component.currentFuelAmount) + " / " + component.totalFuelAmount + " " + Language.Get("ui_pickup_50", "Inspector_UI");
                        }
                        else
                        {
                            componentTitles[1].GetComponent<UILabel>().text = f + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                        }

                        componentTitles[1].GetComponent<UILabel>().color = blue;
                    }
                    else if (component.totalFuelAmount > carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount)
                    {
                        if (component.totalFuelAmount == 0f)
                        {
                            componentTitles[1].GetComponent<UILabel>().text = Mathf.Abs(f) + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                            componentTitles[1].GetComponent<UILabel>().color = green;
                        }
                        else
                        {
                            componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                            componentTitles[1].GetComponent<UILabel>().color = green;
                        }
                    }
                }
                else
                {
                    componentTitles[1].GetComponent<UILabel>().text = f + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = green;
                }

                valueUsed[0] = true;
                SetEngineCompUI3();
            }
            else if (component.totalFuelAmount > 0f && !component2.isInEngine && !valueUsed[17])
            {
                float currentFuelAmount = component.currentFuelAmount;
                if (carLogic.GetComponent<CarPerformanceC>().installedFuelTank != null)
                {
                    currentFuelAmount = component.currentFuelAmount - carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount;
                    if (component.currentFuelAmount < carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = red;
                    }
                    else if (component.currentFuelAmount == carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = Mathf.Round(currentFuelAmount) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = blue;
                    }
                    else if (component.currentFuelAmount > carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount)
                    {
                        if (component.currentFuelAmount == 0f)
                        {
                            componentTitles[1].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                            componentTitles[1].GetComponent<UILabel>().color = green;
                        }
                        else
                        {
                            componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                            componentTitles[1].GetComponent<UILabel>().color = green;
                        }
                    }
                }
                else
                {
                    componentTitles[1].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = green;
                }

                valueUsed[17] = true;
                SetEngineCompUI3();
            }
            else if (component.totalFuelAmount > 0f && !valueUsed[19])
            {
                if (component.fuelMix == 0)
                {
                    componentTitles[1].GetComponent<UILabel>().text = Language.Get("ui_pickup_25", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = red;
                }

                if (component.fuelMix == 1)
                {
                    componentTitles[1].GetComponent<UILabel>().text = Language.Get("ui_pickup_26", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = orange;
                }

                if (component.fuelMix == 2)
                {
                    componentTitles[1].GetComponent<UILabel>().text = Language.Get("ui_pickup_27", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = green;
                }

                if (component.fuelMix == 3)
                {
                    componentTitles[1].GetComponent<UILabel>().text = Language.Get("ui_pickup_28", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = orange;
                }

                valueUsed[19] = true;
                SetEngineCompUI3();
            }
            else if (component.fuelConsumptionRate > 0f && !valueUsed[1])
            {
                float f2 = component.fuelConsumptionRate - carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate;
                if (component.fuelConsumptionRate < carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = red;
                }
                else if (component.fuelConsumptionRate == carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = component.fuelConsumptionRate + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = f2 + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                    }

                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (component.fuelConsumptionRate > carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate == 0f)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = Mathf.Abs(f2) + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[1] = true;
                SetEngineCompUI3();
            }
            else if (component.initialFuelConsumptionAmount > 0f && !valueUsed[2])
            {
                float f3 = component.initialFuelConsumptionAmount - carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount;
                if (component.initialFuelConsumptionAmount < carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = green;
                }
                else if (component.initialFuelConsumptionAmount == carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = component.initialFuelConsumptionAmount + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = f3 + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                    }

                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (component.initialFuelConsumptionAmount > carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount == 0f)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = Mathf.Abs(f3) + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = red;
                    }
                }

                valueUsed[2] = true;
                SetEngineCompUI3();
            }
            else if (component.ignitionTimer != 0f && !valueUsed[3])
            {
                float f4 = component.ignitionTimer - carLogic.GetComponent<CarPerformanceC>().carIgnitionTime;
                if (component.ignitionTimer < carLogic.GetComponent<CarPerformanceC>().carIgnitionTime)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = green;
                }
                else if (component.ignitionTimer == carLogic.GetComponent<CarPerformanceC>().carIgnitionTime)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = component.ignitionTimer + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = f4 + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                    }

                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (component.ignitionTimer > carLogic.GetComponent<CarPerformanceC>().carIgnitionTime)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carIgnitionTime == 0f)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = Mathf.Abs(f4) + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = red;
                    }
                }

                valueUsed[3] = true;
                SetEngineCompUI3();
            }
            else if (component.acceleration > 0f && !valueUsed[4])
            {
                float f5 = component.acceleration - carLogic.GetComponent<CarPerformanceC>().carAcceleration;
                if (component.acceleration < carLogic.GetComponent<CarPerformanceC>().carAcceleration)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = green;
                }
                else if (component.acceleration == carLogic.GetComponent<CarPerformanceC>().carAcceleration)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = component.acceleration + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = f5 + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                    }

                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (component.acceleration > carLogic.GetComponent<CarPerformanceC>().carAcceleration)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carAcceleration == 0f)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = Mathf.Abs(f5) + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = red;
                    }
                }

                valueUsed[4] = true;
                SetEngineCompUI3();
            }
            else if (component.TopSpeed > 0f && !valueUsed[5])
            {
                float f6 = component.TopSpeed - carLogic.GetComponent<CarPerformanceC>().carTopSpeed;
                if (component.TopSpeed < carLogic.GetComponent<CarPerformanceC>().carTopSpeed)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = red;
                }
                else if (component.TopSpeed == carLogic.GetComponent<CarPerformanceC>().carTopSpeed)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = component.TopSpeed + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = f6 + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                    }

                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (component.TopSpeed > carLogic.GetComponent<CarPerformanceC>().carTopSpeed)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carTopSpeed == 0f)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = Mathf.Abs(f6) + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[5] = true;
                SetEngineCompUI3();
            }
            else if (component.turnRate > 0f && !valueUsed[6])
            {
                componentTitles[1].GetComponent<UILabel>().text = "+ " + component.turnRate + " " + Language.Get("ui_pickup_35", "Inspector_UI");
                componentTitles[1].GetComponent<UILabel>().color = green;
                valueUsed[6] = true;
                SetEngineCompUI3();
            }
            else if (component.turnRate < 0f && !valueUsed[6])
            {
                componentTitles[1].GetComponent<UILabel>().text = "- " + component.turnRate + " " + Language.Get("ui_pickup_35", "Inspector_UI");
                componentTitles[1].GetComponent<UILabel>().color = red;
                valueUsed[6] = true;
                SetEngineCompUI3();
            }
            else if (component.roadGrip > 0f && !valueUsed[7])
            {
                componentTitles[1].GetComponent<UILabel>().text = (component.roadGrip / 0.0044f).ToString("F0") + " " + Language.Get("ui_pickup_53", "Inspector_UI");
                componentTitles[4].GetComponent<UILabel>().color = blue;
                valueUsed[7] = true;
                SetEngineCompUI3();
            }
            else if (component.wetGrip > 0f && !valueUsed[21])
            {
                componentTitles[1].GetComponent<UILabel>().text = (component.wetGrip / 0.0044f).ToString("F0") + " " + Language.Get("ui_pickup_36", "Inspector_UI");
                componentTitles[1].GetComponent<UILabel>().color = blue;
                valueUsed[21] = true;
                SetEngineCompUI3();
            }
            else if (component.offRoadGrip > 0f && !valueUsed[22])
            {
                componentTitles[1].GetComponent<UILabel>().text = (component.offRoadGrip / 0.0044f).ToString("F0") + " " + Language.Get("ui_pickup_54", "Inspector_UI");
                componentTitles[1].GetComponent<UILabel>().color = blue;
                valueUsed[22] = true;
                SetEngineCompUI3();
            }
            else if (component.compoundType > 0 && !valueUsed[20])
            {
                if (component.compoundType == 1)
                {
                    componentTitles[1].GetComponent<UILabel>().text = " " + Language.Get("ui_pickup_37", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (component.compoundType == 2)
                {
                    componentTitles[1].GetComponent<UILabel>().text = " " + Language.Get("ui_pickup_38", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (component.compoundType == 3)
                {
                    componentTitles[1].GetComponent<UILabel>().text = " " + Language.Get("ui_pickup_39", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }

                valueUsed[20] = true;
                SetEngineCompUI3();
            }
            else if (component.dirtAccumilation > 0f && !valueUsed[10])
            {
                componentTitles[1].GetComponent<UILabel>().text = "+ " + component.dirtAccumilation + " " + Language.Get("ui_pickup_40", "Inspector_UI");
                componentTitles[1].GetComponent<UILabel>().color = green;
                valueUsed[10] = true;
                SetEngineCompUI3();
            }
            else if (component.dirtAccumilation < 0f && !valueUsed[10])
            {
                componentTitles[1].GetComponent<UILabel>().text = "- " + component.dirtAccumilation + " " + Language.Get("ui_pickup_40", "Inspector_UI");
                componentTitles[1].GetComponent<UILabel>().color = red;
                valueUsed[10] = true;
                SetEngineCompUI3();
            }
            else if (component.totalWaterCharges > 0 && !valueUsed[11])
            {
                float num = 0.083f * (float)component.totalWaterCharges;
                float num2 = 0.083f * (float)component.currentWaterCharges;
                float num3 = 0f;
                float num4 = 0f;
                float num5 = num - num3;
                if (carLogic.GetComponent<CarPerformanceC>().waterTankObj != null)
                {
                    num3 = 0.083f * (float)carLogic.GetComponent<CarPerformanceC>().waterTankObj.GetComponent<EngineComponentC>().totalWaterCharges;
                    num4 = 0.083f * (float)carLogic.GetComponent<CarPerformanceC>().waterTankObj.GetComponent<EngineComponentC>().currentWaterCharges;
                    num5 = num - num3;
                }

                if (num < num3)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(num5 * 10f) / 10f) + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = red;
                }
                else if (num == num3)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(num2 * 10f) / 10f) + " / " + Mathf.Abs(Mathf.Round(num * 10f) / 10f) + " " + Language.Get("ui_pickup_42", "Inspector_UI");
                    }
                    else
                    {
                        float num6 = Mathf.Round(num5 * 10f) / 10f;
                        componentTitles[1].GetComponent<UILabel>().text = num6 + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                    }

                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (num > num3)
                {
                    if (component.totalWaterCharges == 0)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(num * 10f) / 10f) + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(num5 * 10f) / 10f) + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[11] = true;
                SetEngineCompUI3();
            }
            else if (component.totalWaterCharges > 0 && !valueUsed[18])
            {
                float num7 = 0.083f * (float)component.currentWaterCharges;
                float num8 = 0f;
                if (carLogic.GetComponent<CarPerformanceC>().waterTankObj != null)
                {
                    num8 = 0.083f * (float)carLogic.GetComponent<CarPerformanceC>().waterTankObj.GetComponent<EngineComponentC>().currentWaterCharges;
                }

                float num9 = num7 - num8;
                if (num7 < num8)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(num9 * 10f) / 10f) + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = red;
                }
                else if (num7 == num8)
                {
                    float num10 = Mathf.Round(num9 * 10f) / 10f;
                    componentTitles[1].GetComponent<UILabel>().text = num10 + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (num7 > num8)
                {
                    if (component.totalWaterCharges == 0)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(num7 * 10f) / 10f) + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(num9 * 10f) / 10f) + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[18] = true;
                SetEngineCompUI3();
            }
            else if (component.storage > 0f && !valueUsed[12])
            {
                componentTitles[1].GetComponent<UILabel>().text = "+ " + component.storage + " " + Language.Get("ui_pickup_44", "Inspector_UI");
                componentTitles[1].GetComponent<UILabel>().color = green;
                valueUsed[12] = true;
                SetEngineCompUI3();
            }
            else if (component.storage < 0f && !valueUsed[12])
            {
                componentTitles[1].GetComponent<UILabel>().text = "- " + component.storage + " " + Language.Get("ui_pickup_44", "Inspector_UI");
                componentTitles[1].GetComponent<UILabel>().color = red;
                valueUsed[12] = true;
                SetEngineCompUI3();
            }
            else if (component.damageResistance > 0f && !valueUsed[13])
            {
                componentTitles[1].GetComponent<UILabel>().text = "+ " + component.damageResistance + " " + Language.Get("ui_pickup_45", "Inspector_UI");
                componentTitles[1].GetComponent<UILabel>().color = green;
                valueUsed[13] = true;
                SetEngineCompUI3();
            }
            else if (component.damageResistance < 0f && !valueUsed[13])
            {
                componentTitles[1].GetComponent<UILabel>().text = "- " + component.damageResistance + " " + Language.Get("ui_pickup_45", "Inspector_UI");
                componentTitles[1].GetComponent<UILabel>().color = red;
                valueUsed[13] = true;
                SetEngineCompUI3();
            }
            else if (component.engineWearRate > 0f && !valueUsed[14])
            {
                float num11 = component.engineWearRate - carLogic.GetComponent<CarPerformanceC>().carEngineWearRate;
                if (component.engineWearRate < carLogic.GetComponent<CarPerformanceC>().carEngineWearRate)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(num11 * 100f) + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = red;
                }
                else if (component.engineWearRate == carLogic.GetComponent<CarPerformanceC>().carEngineWearRate)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = Mathf.Abs(component.engineWearRate * 100f) + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = num11 + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                    }

                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (component.engineWearRate > carLogic.GetComponent<CarPerformanceC>().carEngineWearRate)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carEngineWearRate == 0f)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = Mathf.Abs(num11 * 100f) + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(num11 * 100f) + " " + Language.Get("ui_pickup_47", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[14] = true;
                SetEngineCompUI3();
            }
            else if (component.isBattery && !valueUsed[16])
            {
                float f7 = 0f;
                float num12 = 0f;
                if (carLogic.GetComponent<CarPerformanceC>().installedBattery != null)
                {
                    f7 = component.charge - carLogic.GetComponent<CarPerformanceC>().installedBattery.GetComponent<EngineComponentC>().charge;
                    num12 = carLogic.GetComponent<CarPerformanceC>().installedBattery.GetComponent<EngineComponentC>().charge;
                }

                if (component.charge < num12)
                {
                    componentTitles[1].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(f7)) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                    componentTitles[1].GetComponent<UILabel>().color = red;
                }
                else if (component.charge == num12)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = Mathf.Round(component.charge) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = Mathf.Round(f7) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                    }

                    componentTitles[1].GetComponent<UILabel>().color = blue;
                }
                else if (component.charge > num12)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carEngineWearRate == 0f)
                    {
                        componentTitles[1].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(f7)) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[1].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(f7)) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                        componentTitles[1].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[16] = true;
                SetEngineCompUI3();
            }
            else if (component.weight > 0f && !valueUsed[15])
            {
                valueUsed[15] = true;
                SetEngineCompUI3();
                SetEngineComponentWeight2();
            }
        }

        public void SetEngineCompUI3()
        {
            EngineComponentC component = hitComponent.GetComponent<EngineComponentC>();
            ObjectPickupC component2 = hitComponent.GetComponent<ObjectPickupC>();
            if (component.totalFuelAmount > 0f && !valueUsed[0] && component.totalFuelAmount > 0f)
            {
                float f = component.totalFuelAmount;
                if (carLogic.GetComponent<CarPerformanceC>().installedFuelTank != null)
                {
                    f = component.totalFuelAmount - carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount;
                }

                if ((bool)hitComponent.transform.parent && (bool)hitComponent.transform.parent.GetComponent<HoldingLogicC>() && hitComponent.transform.parent.GetComponent<HoldingLogicC>().gateDropOff)
                {
                    componentTitles[2].GetComponent<UILabel>().text = Mathf.Round(component.currentFuelAmount) + " / " + component.totalFuelAmount + " " + Language.Get("ui_pickup_50", "Inspector_UI");
                    valueUsed[0] = true;
                    SetEngineCompUI2();
                    return;
                }

                if (carLogic.GetComponent<CarPerformanceC>().installedFuelTank != null)
                {
                    if (component.totalFuelAmount < carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = red;
                    }
                    else if (component.totalFuelAmount == carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount)
                    {
                        if (component2.isInEngine)
                        {
                            componentTitles[2].GetComponent<UILabel>().text = Mathf.Round(component.currentFuelAmount) + " / " + component.totalFuelAmount + " " + Language.Get("ui_pickup_50", "Inspector_UI");
                        }
                        else
                        {
                            componentTitles[2].GetComponent<UILabel>().text = f + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                        }

                        componentTitles[2].GetComponent<UILabel>().color = blue;
                    }
                    else if (component.totalFuelAmount > carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount)
                    {
                        if (component.totalFuelAmount == 0f)
                        {
                            componentTitles[2].GetComponent<UILabel>().text = Mathf.Abs(f) + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                            componentTitles[2].GetComponent<UILabel>().color = green;
                        }
                        else
                        {
                            componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                            componentTitles[2].GetComponent<UILabel>().color = green;
                        }
                    }
                }
                else
                {
                    componentTitles[2].GetComponent<UILabel>().text = f + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = green;
                }

                valueUsed[0] = true;
                SetEngineCompUI4();
            }
            else if (component.totalFuelAmount > 0f && !component2.isInEngine && !valueUsed[17])
            {
                float currentFuelAmount = component.currentFuelAmount;
                if (carLogic.GetComponent<CarPerformanceC>().installedFuelTank != null)
                {
                    currentFuelAmount = component.currentFuelAmount - carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount;
                    if (component.currentFuelAmount < carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = red;
                    }
                    else if (component.currentFuelAmount == carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = Mathf.Round(currentFuelAmount) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = blue;
                    }
                    else if (component.currentFuelAmount > carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount)
                    {
                        if (component.currentFuelAmount == 0f)
                        {
                            componentTitles[2].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                            componentTitles[2].GetComponent<UILabel>().color = green;
                        }
                        else
                        {
                            componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                            componentTitles[2].GetComponent<UILabel>().color = green;
                        }
                    }
                }
                else
                {
                    componentTitles[2].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = green;
                }

                valueUsed[17] = true;
                SetEngineCompUI4();
            }
            else if (component.totalFuelAmount > 0f && !valueUsed[19])
            {
                if (component.fuelMix == 0)
                {
                    componentTitles[2].GetComponent<UILabel>().text = Language.Get("ui_pickup_25", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = red;
                }

                if (component.fuelMix == 1)
                {
                    componentTitles[2].GetComponent<UILabel>().text = Language.Get("ui_pickup_26", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = orange;
                }

                if (component.fuelMix == 2)
                {
                    componentTitles[2].GetComponent<UILabel>().text = Language.Get("ui_pickup_27", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = green;
                }

                if (component.fuelMix == 3)
                {
                    componentTitles[2].GetComponent<UILabel>().text = Language.Get("ui_pickup_28", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = orange;
                }

                valueUsed[19] = true;
                SetEngineCompUI4();
            }
            else if (component.fuelConsumptionRate > 0f && !valueUsed[1])
            {
                float f2 = component.fuelConsumptionRate - carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate;
                if (component.fuelConsumptionRate < carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = red;
                }
                else if (component.fuelConsumptionRate == carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = component.fuelConsumptionRate + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = f2 + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                    }

                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (component.fuelConsumptionRate > carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate == 0f)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = Mathf.Abs(f2) + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[1] = true;
                SetEngineCompUI4();
            }
            else if (component.initialFuelConsumptionAmount > 0f && !valueUsed[2])
            {
                float f3 = component.initialFuelConsumptionAmount - carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount;
                if (component.initialFuelConsumptionAmount < carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = green;
                }
                else if (component.initialFuelConsumptionAmount == carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = component.initialFuelConsumptionAmount + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = f3 + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                    }

                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (component.initialFuelConsumptionAmount > carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount == 0f)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = Mathf.Abs(f3) + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = red;
                    }
                }

                valueUsed[2] = true;
                SetEngineCompUI4();
            }
            else if (component.ignitionTimer != 0f && !valueUsed[3])
            {
                float f4 = component.ignitionTimer - carLogic.GetComponent<CarPerformanceC>().carIgnitionTime;
                if (component.ignitionTimer < carLogic.GetComponent<CarPerformanceC>().carIgnitionTime)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = green;
                }
                else if (component.ignitionTimer == carLogic.GetComponent<CarPerformanceC>().carIgnitionTime)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = component.ignitionTimer + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = f4 + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                    }

                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (component.ignitionTimer > carLogic.GetComponent<CarPerformanceC>().carIgnitionTime)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carIgnitionTime == 0f)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = Mathf.Abs(f4) + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = red;
                    }
                }

                valueUsed[3] = true;
                SetEngineCompUI4();
            }
            else if (component.acceleration > 0f && !valueUsed[4])
            {
                float f5 = component.acceleration - carLogic.GetComponent<CarPerformanceC>().carAcceleration;
                if (component.acceleration < carLogic.GetComponent<CarPerformanceC>().carAcceleration)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = green;
                }
                else if (component.acceleration == carLogic.GetComponent<CarPerformanceC>().carAcceleration)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = component.acceleration + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = f5 + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                    }

                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (component.acceleration > carLogic.GetComponent<CarPerformanceC>().carAcceleration)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carAcceleration == 0f)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = Mathf.Abs(f5) + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = red;
                    }
                }

                valueUsed[4] = true;
                SetEngineCompUI4();
            }
            else if (component.TopSpeed > 0f && !valueUsed[5])
            {
                float f6 = component.TopSpeed - carLogic.GetComponent<CarPerformanceC>().carTopSpeed;
                if (component.TopSpeed < carLogic.GetComponent<CarPerformanceC>().carTopSpeed)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = red;
                }
                else if (component.TopSpeed == carLogic.GetComponent<CarPerformanceC>().carTopSpeed)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = component.TopSpeed + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = f6 + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                    }

                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (component.TopSpeed > carLogic.GetComponent<CarPerformanceC>().carTopSpeed)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carTopSpeed == 0f)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = Mathf.Abs(f6) + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[5] = true;
                SetEngineCompUI4();
            }
            else if (component.turnRate > 0f && !valueUsed[6])
            {
                componentTitles[2].GetComponent<UILabel>().text = "+ " + component.turnRate + " " + Language.Get("ui_pickup_35", "Inspector_UI");
                componentTitles[2].GetComponent<UILabel>().color = green;
                valueUsed[6] = true;
                SetEngineCompUI4();
            }
            else if (component.turnRate < 0f && !valueUsed[6])
            {
                componentTitles[2].GetComponent<UILabel>().text = "- " + component.turnRate + " " + Language.Get("ui_pickup_35", "Inspector_UI");
                componentTitles[2].GetComponent<UILabel>().color = red;
                valueUsed[6] = true;
                SetEngineCompUI4();
            }
            else if (component.roadGrip > 0f && !valueUsed[7])
            {
                componentTitles[2].GetComponent<UILabel>().text = (component.roadGrip / 0.0044f).ToString("F0") + " " + Language.Get("ui_pickup_53", "Inspector_UI");
                componentTitles[4].GetComponent<UILabel>().color = blue;
                valueUsed[7] = true;
                SetEngineCompUI4();
            }
            else if (component.wetGrip > 0f && !valueUsed[21])
            {
                componentTitles[2].GetComponent<UILabel>().text = (component.wetGrip / 0.0044f).ToString("F0") + " " + Language.Get("ui_pickup_36", "Inspector_UI");
                componentTitles[2].GetComponent<UILabel>().color = blue;
                valueUsed[21] = true;
                SetEngineCompUI4();
            }
            else if (component.offRoadGrip > 0f && !valueUsed[22])
            {
                componentTitles[2].GetComponent<UILabel>().text = (component.offRoadGrip / 0.0044f).ToString("F0") + " " + Language.Get("ui_pickup_54", "Inspector_UI");
                componentTitles[2].GetComponent<UILabel>().color = blue;
                valueUsed[22] = true;
                SetEngineCompUI4();
            }
            else if (component.compoundType > 0 && !valueUsed[20])
            {
                if (component.compoundType == 1)
                {
                    componentTitles[2].GetComponent<UILabel>().text = " " + Language.Get("ui_pickup_37", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (component.compoundType == 2)
                {
                    componentTitles[2].GetComponent<UILabel>().text = " " + Language.Get("ui_pickup_38", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (component.compoundType == 3)
                {
                    componentTitles[2].GetComponent<UILabel>().text = " " + Language.Get("ui_pickup_39", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }

                valueUsed[20] = true;
                SetEngineCompUI4();
            }
            else if (component.dirtAccumilation > 0f && !valueUsed[10])
            {
                componentTitles[2].GetComponent<UILabel>().text = "+ " + component.dirtAccumilation + " " + Language.Get("ui_pickup_40", "Inspector_UI");
                componentTitles[2].GetComponent<UILabel>().color = green;
                valueUsed[10] = true;
                SetEngineCompUI4();
            }
            else if (component.dirtAccumilation < 0f && !valueUsed[10])
            {
                componentTitles[2].GetComponent<UILabel>().text = "- " + component.dirtAccumilation + " " + Language.Get("ui_pickup_40", "Inspector_UI");
                componentTitles[2].GetComponent<UILabel>().color = red;
                valueUsed[10] = true;
                SetEngineCompUI4();
            }
            else if (component.totalWaterCharges > 0 && !valueUsed[11])
            {
                float num = 0.083f * (float)component.totalWaterCharges;
                float num2 = 0.083f * (float)component.currentWaterCharges;
                float num3 = 0f;
                float num4 = 0f;
                float num5 = num - num3;
                if (carLogic.GetComponent<CarPerformanceC>().waterTankObj != null)
                {
                    num3 = 0.083f * (float)carLogic.GetComponent<CarPerformanceC>().waterTankObj.GetComponent<EngineComponentC>().totalWaterCharges;
                    num4 = 0.083f * (float)carLogic.GetComponent<CarPerformanceC>().waterTankObj.GetComponent<EngineComponentC>().currentWaterCharges;
                    num5 = num - num3;
                }

                if (num < num3)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(num5 * 10f) / 10f) + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = red;
                }
                else if (num == num3)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(num2 * 10f) / 10f) + " / " + Mathf.Abs(Mathf.Round(num * 10f) / 10f) + " " + Language.Get("ui_pickup_42", "Inspector_UI");
                    }
                    else
                    {
                        float num6 = Mathf.Round(num5 * 10f) / 10f;
                        componentTitles[2].GetComponent<UILabel>().text = num6 + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                    }

                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (num > num3)
                {
                    if (component.totalWaterCharges == 0)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(num * 10f) / 10f) + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(num5 * 10f) / 10f) + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[11] = true;
                SetEngineCompUI4();
            }
            else if (component.totalWaterCharges > 0 && !valueUsed[18])
            {
                float num7 = 0.083f * (float)component.currentWaterCharges;
                float num8 = 0f;
                if (carLogic.GetComponent<CarPerformanceC>().waterTankObj != null)
                {
                    num8 = 0.083f * (float)carLogic.GetComponent<CarPerformanceC>().waterTankObj.GetComponent<EngineComponentC>().currentWaterCharges;
                }

                float num9 = num7 - num8;
                if (num7 < num8)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(num9 * 10f) / 10f) + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = red;
                }
                else if (num7 == num8)
                {
                    float num10 = Mathf.Round(num9 * 10f) / 10f;
                    componentTitles[2].GetComponent<UILabel>().text = num10 + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (num7 > num8)
                {
                    if (component.totalWaterCharges == 0)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(num7 * 10f) / 10f) + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(num9 * 10f) / 10f) + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[18] = true;
                SetEngineCompUI4();
            }
            else if (component.storage > 0f && !valueUsed[12])
            {
                componentTitles[2].GetComponent<UILabel>().text = "+ " + component.storage + " " + Language.Get("ui_pickup_44", "Inspector_UI");
                componentTitles[2].GetComponent<UILabel>().color = green;
                valueUsed[12] = true;
                SetEngineCompUI4();
            }
            else if (component.storage < 0f && !valueUsed[12])
            {
                componentTitles[2].GetComponent<UILabel>().text = "- " + component.storage + " " + Language.Get("ui_pickup_44", "Inspector_UI");
                componentTitles[2].GetComponent<UILabel>().color = red;
                valueUsed[12] = true;
                SetEngineCompUI4();
            }
            else if (component.damageResistance > 0f && !valueUsed[13])
            {
                componentTitles[2].GetComponent<UILabel>().text = "+ " + component.damageResistance + " " + Language.Get("ui_pickup_45", "Inspector_UI");
                componentTitles[2].GetComponent<UILabel>().color = green;
                valueUsed[13] = true;
                SetEngineCompUI4();
            }
            else if (component.damageResistance < 0f && !valueUsed[13])
            {
                componentTitles[2].GetComponent<UILabel>().text = "- " + component.damageResistance + " " + Language.Get("ui_pickup_45", "Inspector_UI");
                componentTitles[2].GetComponent<UILabel>().color = red;
                valueUsed[13] = true;
                SetEngineCompUI4();
            }
            else if (component.engineWearRate > 0f && !valueUsed[14])
            {
                float num11 = component.engineWearRate - carLogic.GetComponent<CarPerformanceC>().carEngineWearRate;
                if (component.engineWearRate < carLogic.GetComponent<CarPerformanceC>().carEngineWearRate)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(num11 * 100f) + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = red;
                }
                else if (component.engineWearRate == carLogic.GetComponent<CarPerformanceC>().carEngineWearRate)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = Mathf.Abs(component.engineWearRate * 100f) + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = num11 + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                    }

                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (component.engineWearRate > carLogic.GetComponent<CarPerformanceC>().carEngineWearRate)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carEngineWearRate == 0f)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = Mathf.Abs(num11 * 100f) + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(num11 * 100f) + " " + Language.Get("ui_pickup_47", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[14] = true;
                SetEngineCompUI4();
            }
            else if (component.isBattery && !valueUsed[16])
            {
                float f7 = component.charge - carLogic.GetComponent<CarPerformanceC>().carCharge;
                if (component.charge < carLogic.GetComponent<CarPerformanceC>().carCharge)
                {
                    componentTitles[2].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(f7)) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                    componentTitles[2].GetComponent<UILabel>().color = red;
                }
                else if (component.charge == carLogic.GetComponent<CarPerformanceC>().carCharge)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = Mathf.Round(component.charge) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = Mathf.Round(f7) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                    }

                    componentTitles[2].GetComponent<UILabel>().color = blue;
                }
                else if (component.charge > carLogic.GetComponent<CarPerformanceC>().carCharge)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carEngineWearRate == 0f)
                    {
                        componentTitles[2].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(f7)) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[2].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(f7)) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                        componentTitles[2].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[16] = true;
                SetEngineCompUI4();
            }
            else if (component.weight > 0f && !valueUsed[15])
            {
                valueUsed[15] = true;
                SetEngineCompUI4();
                SetEngineComponentWeight3();
            }
        }

        public void SetEngineCompUI4()
        {
            EngineComponentC component = hitComponent.GetComponent<EngineComponentC>();
            ObjectPickupC component2 = hitComponent.GetComponent<ObjectPickupC>();
            if (component.totalFuelAmount > 0f && !valueUsed[0] && component.totalFuelAmount > 0f)
            {
                float f = component.totalFuelAmount;
                if (carLogic.GetComponent<CarPerformanceC>().installedFuelTank != null)
                {
                    f = component.totalFuelAmount - carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount;
                }

                if ((bool)hitComponent.transform.parent && (bool)hitComponent.transform.parent.GetComponent<HoldingLogicC>() && hitComponent.transform.parent.GetComponent<HoldingLogicC>().gateDropOff)
                {
                    componentTitles[3].GetComponent<UILabel>().text = Mathf.Round(component.currentFuelAmount) + " / " + component.totalFuelAmount + " " + Language.Get("ui_pickup_50", "Inspector_UI");
                    valueUsed[0] = true;
                    SetEngineCompUI2();
                    return;
                }

                if (carLogic.GetComponent<CarPerformanceC>().installedFuelTank != null)
                {
                    if (component.totalFuelAmount < carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = red;
                    }
                    else if (component.totalFuelAmount == carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount)
                    {
                        if (component2.isInEngine)
                        {
                            componentTitles[3].GetComponent<UILabel>().text = Mathf.Round(component.currentFuelAmount) + " / " + component.totalFuelAmount + " " + Language.Get("ui_pickup_50", "Inspector_UI");
                        }
                        else
                        {
                            componentTitles[3].GetComponent<UILabel>().text = f + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                        }

                        componentTitles[3].GetComponent<UILabel>().color = blue;
                    }
                    else if (component.totalFuelAmount > carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount)
                    {
                        if (component.totalFuelAmount == 0f)
                        {
                            componentTitles[3].GetComponent<UILabel>().text = Mathf.Abs(f) + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                            componentTitles[3].GetComponent<UILabel>().color = green;
                        }
                        else
                        {
                            componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                            componentTitles[3].GetComponent<UILabel>().color = green;
                        }
                    }
                }
                else
                {
                    componentTitles[3].GetComponent<UILabel>().text = f + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = green;
                }

                valueUsed[0] = true;
                SetEngineCompUI5();
            }
            else if (component.totalFuelAmount > 0f && !component2.isInEngine && !valueUsed[17])
            {
                float currentFuelAmount = component.currentFuelAmount;
                if (carLogic.GetComponent<CarPerformanceC>().installedFuelTank != null)
                {
                    currentFuelAmount = component.currentFuelAmount - carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount;
                    if (component.currentFuelAmount < carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = red;
                    }
                    else if (component.currentFuelAmount == carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = Mathf.Round(currentFuelAmount) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = blue;
                    }
                    else if (component.currentFuelAmount > carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount)
                    {
                        if (component.currentFuelAmount == 0f)
                        {
                            componentTitles[3].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                            componentTitles[3].GetComponent<UILabel>().color = green;
                        }
                        else
                        {
                            componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                            componentTitles[3].GetComponent<UILabel>().color = green;
                        }
                    }
                }
                else
                {
                    componentTitles[3].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = green;
                }

                valueUsed[17] = true;
                SetEngineCompUI5();
            }
            else if (component.totalFuelAmount > 0f && !valueUsed[19])
            {
                if (component.fuelMix == 0)
                {
                    componentTitles[3].GetComponent<UILabel>().text = Language.Get("ui_pickup_25", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = red;
                }

                if (component.fuelMix == 1)
                {
                    componentTitles[3].GetComponent<UILabel>().text = Language.Get("ui_pickup_26", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = orange;
                }

                if (component.fuelMix == 2)
                {
                    componentTitles[3].GetComponent<UILabel>().text = Language.Get("ui_pickup_27", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = green;
                }

                if (component.fuelMix == 3)
                {
                    componentTitles[3].GetComponent<UILabel>().text = Language.Get("ui_pickup_28", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = orange;
                }

                valueUsed[19] = true;
                SetEngineCompUI5();
            }
            else if (component.fuelConsumptionRate > 0f && !valueUsed[1])
            {
                float f2 = component.fuelConsumptionRate - carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate;
                if (component.fuelConsumptionRate < carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = red;
                }
                else if (component.fuelConsumptionRate == carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = component.fuelConsumptionRate + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = f2 + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                    }

                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (component.fuelConsumptionRate > carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate == 0f)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = Mathf.Abs(f2) + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[1] = true;
                SetEngineCompUI4();
            }
            else if (component.initialFuelConsumptionAmount > 0f && !valueUsed[2])
            {
                float f3 = component.initialFuelConsumptionAmount - carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount;
                if (component.initialFuelConsumptionAmount < carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = green;
                }
                else if (component.initialFuelConsumptionAmount == carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = component.initialFuelConsumptionAmount + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = f3 + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                    }

                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (component.initialFuelConsumptionAmount > carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount == 0f)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = Mathf.Abs(f3) + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = red;
                    }
                }

                valueUsed[2] = true;
                SetEngineCompUI5();
            }
            else if (component.ignitionTimer != 0f && !valueUsed[3])
            {
                float f4 = component.ignitionTimer - carLogic.GetComponent<CarPerformanceC>().carIgnitionTime;
                if (component.ignitionTimer < carLogic.GetComponent<CarPerformanceC>().carIgnitionTime)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = green;
                }
                else if (component.ignitionTimer == carLogic.GetComponent<CarPerformanceC>().carIgnitionTime)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = component.ignitionTimer + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = f4 + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                    }

                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (component.ignitionTimer > carLogic.GetComponent<CarPerformanceC>().carIgnitionTime)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carIgnitionTime == 0f)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = Mathf.Abs(f4) + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = red;
                    }
                }

                valueUsed[3] = true;
                SetEngineCompUI5();
            }
            else if (component.acceleration > 0f && !valueUsed[4])
            {
                float f5 = component.acceleration - carLogic.GetComponent<CarPerformanceC>().carAcceleration;
                if (component.acceleration < carLogic.GetComponent<CarPerformanceC>().carAcceleration)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = green;
                }
                else if (component.acceleration == carLogic.GetComponent<CarPerformanceC>().carAcceleration)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = component.acceleration + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = f5 + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                    }

                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (component.acceleration > carLogic.GetComponent<CarPerformanceC>().carAcceleration)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carAcceleration == 0f)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = Mathf.Abs(f5) + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = red;
                    }
                }

                valueUsed[4] = true;
                SetEngineCompUI5();
            }
            else if (component.TopSpeed > 0f && !valueUsed[5])
            {
                float f6 = component.TopSpeed - carLogic.GetComponent<CarPerformanceC>().carTopSpeed;
                if (component.TopSpeed < carLogic.GetComponent<CarPerformanceC>().carTopSpeed)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = red;
                }
                else if (component.TopSpeed == carLogic.GetComponent<CarPerformanceC>().carTopSpeed)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = component.TopSpeed + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = f6 + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                    }

                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (component.TopSpeed > carLogic.GetComponent<CarPerformanceC>().carTopSpeed)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carTopSpeed == 0f)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = Mathf.Abs(f6) + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[5] = true;
                SetEngineCompUI5();
            }
            else if (component.turnRate > 0f && !valueUsed[6])
            {
                componentTitles[3].GetComponent<UILabel>().text = "+ " + component.turnRate + " " + Language.Get("ui_pickup_35", "Inspector_UI");
                componentTitles[3].GetComponent<UILabel>().color = green;
                valueUsed[6] = true;
                SetEngineCompUI5();
            }
            else if (component.turnRate < 0f && !valueUsed[6])
            {
                componentTitles[3].GetComponent<UILabel>().text = "- " + component.turnRate + " " + Language.Get("ui_pickup_35", "Inspector_UI");
                componentTitles[3].GetComponent<UILabel>().color = red;
                valueUsed[6] = true;
                SetEngineCompUI5();
            }
            else if (component.roadGrip > 0f && !valueUsed[7])
            {
                componentTitles[3].GetComponent<UILabel>().text = (component.roadGrip / 0.0044f).ToString("F0") + " " + Language.Get("ui_pickup_53", "Inspector_UI");
                componentTitles[4].GetComponent<UILabel>().color = blue;
                valueUsed[7] = true;
                SetEngineCompUI5();
            }
            else if (component.wetGrip > 0f && !valueUsed[21])
            {
                componentTitles[3].GetComponent<UILabel>().text = (component.wetGrip / 0.0044f).ToString("F0") + " " + Language.Get("ui_pickup_36", "Inspector_UI");
                componentTitles[3].GetComponent<UILabel>().color = blue;
                valueUsed[21] = true;
                SetEngineCompUI5();
            }
            else if (component.offRoadGrip > 0f && !valueUsed[22])
            {
                componentTitles[3].GetComponent<UILabel>().text = (component.offRoadGrip / 0.0044f).ToString("F0") + " " + Language.Get("ui_pickup_54", "Inspector_UI");
                componentTitles[3].GetComponent<UILabel>().color = blue;
                valueUsed[22] = true;
                SetEngineCompUI5();
            }
            else if (component.compoundType > 0 && !valueUsed[20])
            {
                if (component.compoundType == 1)
                {
                    componentTitles[3].GetComponent<UILabel>().text = " " + Language.Get("ui_pickup_37", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (component.compoundType == 2)
                {
                    componentTitles[3].GetComponent<UILabel>().text = " " + Language.Get("ui_pickup_38", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (component.compoundType == 3)
                {
                    componentTitles[3].GetComponent<UILabel>().text = " " + Language.Get("ui_pickup_39", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }

                valueUsed[20] = true;
                SetEngineCompUI5();
            }
            else if (component.dirtAccumilation > 0f && !valueUsed[10])
            {
                componentTitles[3].GetComponent<UILabel>().text = "+ " + component.dirtAccumilation + " " + Language.Get("ui_pickup_40", "Inspector_UI");
                componentTitles[3].GetComponent<UILabel>().color = green;
                valueUsed[10] = true;
                SetEngineCompUI5();
            }
            else if (component.dirtAccumilation < 0f && !valueUsed[10])
            {
                componentTitles[3].GetComponent<UILabel>().text = "- " + component.dirtAccumilation + " " + Language.Get("ui_pickup_40", "Inspector_UI");
                componentTitles[3].GetComponent<UILabel>().color = red;
                valueUsed[10] = true;
                SetEngineCompUI5();
            }
            else if (component.totalWaterCharges > 0 && !valueUsed[11])
            {
                float num = 0.083f * (float)component.totalWaterCharges;
                float num2 = 0.083f * (float)component.currentWaterCharges;
                float num3 = 0f;
                float num4 = 0f;
                float num5 = num - num3;
                if (carLogic.GetComponent<CarPerformanceC>().waterTankObj != null)
                {
                    num3 = 0.083f * (float)carLogic.GetComponent<CarPerformanceC>().waterTankObj.GetComponent<EngineComponentC>().totalWaterCharges;
                    num4 = 0.083f * (float)carLogic.GetComponent<CarPerformanceC>().waterTankObj.GetComponent<EngineComponentC>().currentWaterCharges;
                    num5 = num - num3;
                }

                if (num < num3)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(num5 * 10f) / 10f) + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = red;
                }
                else if (num == num3)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(num2 * 10f) / 10f) + " / " + Mathf.Abs(Mathf.Round(num * 10f) / 10f) + " " + Language.Get("ui_pickup_42", "Inspector_UI");
                    }
                    else
                    {
                        float num6 = Mathf.Round(num5 * 10f) / 10f;
                        componentTitles[3].GetComponent<UILabel>().text = num6 + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                    }

                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (num > num3)
                {
                    if (component.totalWaterCharges == 0)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(num * 10f) / 10f) + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(num5 * 10f) / 10f) + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[11] = true;
                SetEngineCompUI5();
            }
            else if (component.totalWaterCharges > 0 && !valueUsed[18])
            {
                float num7 = 0.083f * (float)component.currentWaterCharges;
                float num8 = 0f;
                if (carLogic.GetComponent<CarPerformanceC>().waterTankObj != null)
                {
                    num8 = 0.083f * (float)carLogic.GetComponent<CarPerformanceC>().waterTankObj.GetComponent<EngineComponentC>().currentWaterCharges;
                }

                float num9 = num7 - num8;
                if (num7 < num8)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(num9 * 10f) / 10f) + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = red;
                }
                else if (num7 == num8)
                {
                    float num10 = Mathf.Round(num9 * 10f) / 10f;
                    componentTitles[3].GetComponent<UILabel>().text = num10 + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (num7 > num8)
                {
                    if (component.totalWaterCharges == 0)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(num7 * 10f) / 10f) + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(num9 * 10f) / 10f) + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[18] = true;
                SetEngineCompUI5();
            }
            else if (component.storage > 0f && !valueUsed[12])
            {
                componentTitles[3].GetComponent<UILabel>().text = "+ " + component.storage + " " + Language.Get("ui_pickup_44", "Inspector_UI");
                componentTitles[3].GetComponent<UILabel>().color = green;
                valueUsed[12] = true;
                SetEngineCompUI5();
            }
            else if (component.storage < 0f && !valueUsed[12])
            {
                componentTitles[3].GetComponent<UILabel>().text = "- " + component.storage + " " + Language.Get("ui_pickup_44", "Inspector_UI");
                componentTitles[3].GetComponent<UILabel>().color = red;
                valueUsed[12] = true;
                SetEngineCompUI5();
            }
            else if (component.damageResistance > 0f && !valueUsed[13])
            {
                componentTitles[3].GetComponent<UILabel>().text = "+ " + component.damageResistance + " " + Language.Get("ui_pickup_45", "Inspector_UI");
                componentTitles[3].GetComponent<UILabel>().color = green;
                valueUsed[13] = true;
                SetEngineCompUI5();
            }
            else if (component.damageResistance < 0f && !valueUsed[13])
            {
                componentTitles[3].GetComponent<UILabel>().text = "- " + component.damageResistance + " " + Language.Get("ui_pickup_45", "Inspector_UI");
                componentTitles[3].GetComponent<UILabel>().color = red;
                valueUsed[13] = true;
                SetEngineCompUI5();
            }
            else if (component.engineWearRate > 0f && !valueUsed[14])
            {
                float num11 = component.engineWearRate - carLogic.GetComponent<CarPerformanceC>().carEngineWearRate;
                if (component.engineWearRate < carLogic.GetComponent<CarPerformanceC>().carEngineWearRate)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(num11 * 100f) + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = red;
                }
                else if (component.engineWearRate == carLogic.GetComponent<CarPerformanceC>().carEngineWearRate)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = Mathf.Abs(component.engineWearRate * 100f) + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = num11 + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                    }

                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (component.engineWearRate > carLogic.GetComponent<CarPerformanceC>().carEngineWearRate)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carEngineWearRate == 0f)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = Mathf.Abs(num11 * 100f) + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(num11 * 100f) + " " + Language.Get("ui_pickup_47", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[14] = true;
                SetEngineCompUI5();
            }
            else if (component.isBattery && !valueUsed[16])
            {
                float f7 = component.charge - carLogic.GetComponent<CarPerformanceC>().carCharge;
                if (component.charge < carLogic.GetComponent<CarPerformanceC>().carCharge)
                {
                    componentTitles[3].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(f7)) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                    componentTitles[3].GetComponent<UILabel>().color = red;
                }
                else if (component.charge == carLogic.GetComponent<CarPerformanceC>().carCharge)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = Mathf.Round(component.charge) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = Mathf.Round(f7) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                    }

                    componentTitles[3].GetComponent<UILabel>().color = blue;
                }
                else if (component.charge > carLogic.GetComponent<CarPerformanceC>().carCharge)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carEngineWearRate == 0f)
                    {
                        componentTitles[3].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(f7)) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[3].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(f7)) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                        componentTitles[3].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[16] = true;
                SetEngineCompUI5();
            }
            else if (component.weight > 0f && !valueUsed[15])
            {
                valueUsed[15] = true;
                SetEngineCompUI5();
                SetEngineComponentWeight4();
            }
        }

        public void SetEngineCompUI5()
        {
            EngineComponentC component = hitComponent.GetComponent<EngineComponentC>();
            ObjectPickupC component2 = hitComponent.GetComponent<ObjectPickupC>();
            if (component.totalFuelAmount > 0f && !valueUsed[0] && component.totalFuelAmount > 0f)
            {
                float f = component.totalFuelAmount;
                if (carLogic.GetComponent<CarPerformanceC>().installedFuelTank != null)
                {
                    f = component.totalFuelAmount - carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount;
                }

                if ((bool)hitComponent.transform.parent && (bool)hitComponent.transform.parent.GetComponent<HoldingLogicC>() && hitComponent.transform.parent.GetComponent<HoldingLogicC>().gateDropOff)
                {
                    componentTitles[4].GetComponent<UILabel>().text = Mathf.Round(component.currentFuelAmount) + " / " + component.totalFuelAmount + " " + Language.Get("ui_pickup_50", "Inspector_UI");
                    valueUsed[0] = true;
                    SetEngineCompUI2();
                    return;
                }

                if (carLogic.GetComponent<CarPerformanceC>().installedFuelTank != null)
                {
                    if (component.totalFuelAmount < carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = red;
                    }
                    else if (component.totalFuelAmount == carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount)
                    {
                        if (component2.isInEngine)
                        {
                            componentTitles[4].GetComponent<UILabel>().text = Mathf.Round(component.currentFuelAmount) + " / " + component.totalFuelAmount + " " + Language.Get("ui_pickup_50", "Inspector_UI");
                        }
                        else
                        {
                            componentTitles[4].GetComponent<UILabel>().text = f + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                        }

                        componentTitles[4].GetComponent<UILabel>().color = blue;
                    }
                    else if (component.totalFuelAmount > carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().totalFuelAmount)
                    {
                        if (component.totalFuelAmount == 0f)
                        {
                            componentTitles[4].GetComponent<UILabel>().text = Mathf.Abs(f) + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                            componentTitles[4].GetComponent<UILabel>().color = green;
                        }
                        else
                        {
                            componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f) + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                            componentTitles[4].GetComponent<UILabel>().color = green;
                        }
                    }
                }
                else
                {
                    componentTitles[4].GetComponent<UILabel>().text = f + " " + Language.Get("ui_pickup_51", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = green;
                }

                valueUsed[0] = true;
            }
            else if (component.totalFuelAmount > 0f && !component2.isInEngine && !valueUsed[17])
            {
                float currentFuelAmount = component.currentFuelAmount;
                if (carLogic.GetComponent<CarPerformanceC>().installedFuelTank != null)
                {
                    currentFuelAmount = component.currentFuelAmount - carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount;
                    if (component.currentFuelAmount < carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = red;
                    }
                    else if (component.currentFuelAmount == carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = Mathf.Round(currentFuelAmount) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = blue;
                    }
                    else if (component.currentFuelAmount > carLogic.GetComponent<CarPerformanceC>().installedFuelTank.GetComponent<EngineComponentC>().currentFuelAmount)
                    {
                        if (component.currentFuelAmount == 0f)
                        {
                            componentTitles[4].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                            componentTitles[4].GetComponent<UILabel>().color = green;
                        }
                        else
                        {
                            componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                            componentTitles[4].GetComponent<UILabel>().color = green;
                        }
                    }
                }
                else
                {
                    componentTitles[4].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(currentFuelAmount)) + " " + Language.Get("ui_pickup_52", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = green;
                }

                valueUsed[17] = true;
            }
            else if (component.totalFuelAmount > 0f && !valueUsed[19])
            {
                if (component.fuelMix == 0)
                {
                    componentTitles[4].GetComponent<UILabel>().text = Language.Get("ui_pickup_25", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = red;
                }

                if (component.fuelMix == 1)
                {
                    componentTitles[4].GetComponent<UILabel>().text = Language.Get("ui_pickup_26", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = orange;
                }

                if (component.fuelMix == 2)
                {
                    componentTitles[4].GetComponent<UILabel>().text = Language.Get("ui_pickup_27", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = green;
                }

                if (component.fuelMix == 3)
                {
                    componentTitles[4].GetComponent<UILabel>().text = Language.Get("ui_pickup_28", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = orange;
                }

                valueUsed[19] = true;
            }
            else if (component.fuelConsumptionRate > 0f && !valueUsed[1])
            {
                float f2 = component.fuelConsumptionRate - carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate;
                if (component.fuelConsumptionRate < carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = red;
                }
                else if (component.fuelConsumptionRate == carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = component.fuelConsumptionRate + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = f2 + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                    }

                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (component.fuelConsumptionRate > carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carFuelConsumptionRate == 0f)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = Mathf.Abs(f2) + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f2) + " " + Language.Get("ui_pickup_30", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[1] = true;
                SetEngineCompUI5();
            }
            else if (component.initialFuelConsumptionAmount > 0f && !valueUsed[2])
            {
                float f3 = component.initialFuelConsumptionAmount - carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount;
                if (component.initialFuelConsumptionAmount < carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = green;
                }
                else if (component.initialFuelConsumptionAmount == carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = component.initialFuelConsumptionAmount + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = f3 + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                    }

                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (component.initialFuelConsumptionAmount > carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carInitialFuelConsumptionAmount == 0f)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = Mathf.Abs(f3) + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f3) + " " + Language.Get("ui_pickup_31", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = red;
                    }
                }

                valueUsed[2] = true;
            }
            else if (component.ignitionTimer != 0f && !valueUsed[3])
            {
                float f4 = component.ignitionTimer - carLogic.GetComponent<CarPerformanceC>().carIgnitionTime;
                if (component.ignitionTimer < carLogic.GetComponent<CarPerformanceC>().carIgnitionTime)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = green;
                }
                else if (component.ignitionTimer == carLogic.GetComponent<CarPerformanceC>().carIgnitionTime)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = component.ignitionTimer + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = f4 + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                    }

                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (component.ignitionTimer > carLogic.GetComponent<CarPerformanceC>().carIgnitionTime)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carIgnitionTime == 0f)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = Mathf.Abs(f4) + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f4) + " " + Language.Get("ui_pickup_32", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = red;
                    }
                }

                valueUsed[3] = true;
            }
            else if (component.acceleration > 0f && !valueUsed[4])
            {
                float f5 = component.acceleration - carLogic.GetComponent<CarPerformanceC>().carAcceleration;
                if (component.acceleration < carLogic.GetComponent<CarPerformanceC>().carAcceleration)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = green;
                }
                else if (component.acceleration == carLogic.GetComponent<CarPerformanceC>().carAcceleration)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = component.acceleration + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = f5 + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                    }

                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (component.acceleration > carLogic.GetComponent<CarPerformanceC>().carAcceleration)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carAcceleration == 0f)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = Mathf.Abs(f5) + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f5) + " " + Language.Get("ui_pickup_33", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = red;
                    }
                }

                valueUsed[4] = true;
                SetEngineCompUI5();
            }
            else if (component.TopSpeed > 0f && !valueUsed[5])
            {
                float f6 = component.TopSpeed - carLogic.GetComponent<CarPerformanceC>().carTopSpeed;
                if (component.TopSpeed < carLogic.GetComponent<CarPerformanceC>().carTopSpeed)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = red;
                }
                else if (component.TopSpeed == carLogic.GetComponent<CarPerformanceC>().carTopSpeed)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = component.TopSpeed + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = f6 + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                    }

                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (component.TopSpeed > carLogic.GetComponent<CarPerformanceC>().carTopSpeed)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carTopSpeed == 0f)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = Mathf.Abs(f6) + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(f6) + " " + Language.Get("ui_pickup_34", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[5] = true;
            }
            else if (component.turnRate > 0f && !valueUsed[6])
            {
                componentTitles[4].GetComponent<UILabel>().text = "+ " + component.turnRate + " " + Language.Get("ui_pickup_35", "Inspector_UI");
                componentTitles[4].GetComponent<UILabel>().color = green;
                valueUsed[6] = true;
            }
            else if (component.turnRate < 0f && !valueUsed[6])
            {
                componentTitles[4].GetComponent<UILabel>().text = "- " + component.turnRate + " " + Language.Get("ui_pickup_35", "Inspector_UI");
                componentTitles[4].GetComponent<UILabel>().color = red;
                valueUsed[6] = true;
            }
            else if (component.roadGrip > 0f && !valueUsed[7])
            {
                componentTitles[4].GetComponent<UILabel>().text = (component.roadGrip / 0.0044f).ToString("F0") + " " + Language.Get("ui_pickup_53", "Inspector_UI");
                componentTitles[4].GetComponent<UILabel>().color = blue;
                valueUsed[7] = true;
            }
            else if (component.wetGrip > 0f && !valueUsed[21])
            {
                componentTitles[4].GetComponent<UILabel>().text = (component.wetGrip / 0.0044f).ToString("F0") + " " + Language.Get("ui_pickup_36", "Inspector_UI");
                componentTitles[4].GetComponent<UILabel>().color = blue;
                valueUsed[21] = true;
            }
            else if (component.offRoadGrip > 0f && !valueUsed[22])
            {
                componentTitles[4].GetComponent<UILabel>().text = (component.offRoadGrip / 0.0044f).ToString("F0") + " " + Language.Get("ui_pickup_54", "Inspector_UI");
                componentTitles[4].GetComponent<UILabel>().color = blue;
                valueUsed[22] = true;
            }
            else if (component.compoundType > 0 && !valueUsed[20])
            {
                if (component.compoundType == 1)
                {
                    componentTitles[4].GetComponent<UILabel>().text = " " + Language.Get("ui_pickup_37", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (component.compoundType == 2)
                {
                    componentTitles[4].GetComponent<UILabel>().text = " " + Language.Get("ui_pickup_38", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (component.compoundType == 3)
                {
                    componentTitles[4].GetComponent<UILabel>().text = " " + Language.Get("ui_pickup_39", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }

                valueUsed[20] = true;
            }
            else if (component.dirtAccumilation > 0f && !valueUsed[10])
            {
                componentTitles[4].GetComponent<UILabel>().text = "+ " + component.dirtAccumilation + " " + Language.Get("ui_pickup_40", "Inspector_UI");
                componentTitles[4].GetComponent<UILabel>().color = green;
                valueUsed[10] = true;
            }
            else if (component.dirtAccumilation < 0f && !valueUsed[10])
            {
                componentTitles[4].GetComponent<UILabel>().text = "- " + component.dirtAccumilation + " " + Language.Get("ui_pickup_40", "Inspector_UI");
                componentTitles[4].GetComponent<UILabel>().color = red;
                valueUsed[10] = true;
            }
            else if (component.totalWaterCharges > 0 && !valueUsed[11])
            {
                float num = 0.083f * (float)component.totalWaterCharges;
                float num2 = 0.083f * (float)component.currentWaterCharges;
                float num3 = 0f;
                float num4 = 0f;
                float num5 = num - num3;
                if (carLogic.GetComponent<CarPerformanceC>().waterTankObj != null)
                {
                    num3 = 0.083f * (float)carLogic.GetComponent<CarPerformanceC>().waterTankObj.GetComponent<EngineComponentC>().totalWaterCharges;
                    num4 = 0.083f * (float)carLogic.GetComponent<CarPerformanceC>().waterTankObj.GetComponent<EngineComponentC>().currentWaterCharges;
                    num5 = num - num3;
                }

                if (num < num3)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(num5 * 10f) / 10f) + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = red;
                }
                else if (num == num3)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(num2 * 10f) / 10f) + " / " + Mathf.Abs(Mathf.Round(num * 10f) / 10f) + " " + Language.Get("ui_pickup_42", "Inspector_UI");
                    }
                    else
                    {
                        float num6 = Mathf.Round(num5 * 10f) / 10f;
                        componentTitles[4].GetComponent<UILabel>().text = num6 + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                    }

                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (num > num3)
                {
                    if (component.totalWaterCharges == 0)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(num * 10f) / 10f) + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(num5 * 10f) / 10f) + " " + Language.Get("ui_pickup_41", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[11] = true;
            }
            else if (component.totalWaterCharges > 0 && !valueUsed[18])
            {
                float num7 = 0.083f * (float)component.currentWaterCharges;
                float num8 = 0f;
                if (carLogic.GetComponent<CarPerformanceC>().waterTankObj != null)
                {
                    num8 = 0.083f * (float)carLogic.GetComponent<CarPerformanceC>().waterTankObj.GetComponent<EngineComponentC>().currentWaterCharges;
                }

                float num9 = num7 - num8;
                if (num7 < num8)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(num9 * 10f) / 10f) + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = red;
                }
                else if (num7 == num8)
                {
                    float num10 = Mathf.Round(num9 * 10f) / 10f;
                    componentTitles[4].GetComponent<UILabel>().text = num10 + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (num7 > num8)
                {
                    if (component.totalWaterCharges == 0)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(num7 * 10f) / 10f) + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(num9 * 10f) / 10f) + " " + Language.Get("ui_pickup_43", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[18] = true;
            }
            else if (component.storage > 0f && !valueUsed[12])
            {
                componentTitles[4].GetComponent<UILabel>().text = "+ " + component.storage + " " + Language.Get("ui_pickup_44", "Inspector_UI");
                componentTitles[4].GetComponent<UILabel>().color = green;
                valueUsed[12] = true;
            }
            else if (component.storage < 0f && !valueUsed[12])
            {
                componentTitles[4].GetComponent<UILabel>().text = "- " + component.storage + " " + Language.Get("ui_pickup_44", "Inspector_UI");
                componentTitles[4].GetComponent<UILabel>().color = red;
                valueUsed[12] = true;
            }
            else if (component.damageResistance > 0f && !valueUsed[13])
            {
                componentTitles[4].GetComponent<UILabel>().text = "+ " + component.damageResistance + " " + Language.Get("ui_pickup_45", "Inspector_UI");
                componentTitles[4].GetComponent<UILabel>().color = green;
                valueUsed[13] = true;
            }
            else if (component.damageResistance < 0f && !valueUsed[13])
            {
                componentTitles[4].GetComponent<UILabel>().text = "- " + component.damageResistance + " " + Language.Get("ui_pickup_45", "Inspector_UI");
                componentTitles[4].GetComponent<UILabel>().color = red;
                valueUsed[13] = true;
            }
            else if (component.engineWearRate > 0f && !valueUsed[14])
            {
                float num11 = component.engineWearRate - carLogic.GetComponent<CarPerformanceC>().carEngineWearRate;
                if (component.engineWearRate < carLogic.GetComponent<CarPerformanceC>().carEngineWearRate)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(num11 * 100f) + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = red;
                }
                else if (component.engineWearRate == carLogic.GetComponent<CarPerformanceC>().carEngineWearRate)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = Mathf.Abs(component.engineWearRate * 100f) + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = num11 + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                    }

                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (component.engineWearRate > carLogic.GetComponent<CarPerformanceC>().carEngineWearRate)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carEngineWearRate == 0f)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = Mathf.Abs(num11 * 100f) + " " + Language.Get("ui_pickup_46", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(num11 * 100f) + " " + Language.Get("ui_pickup_47", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[14] = true;
            }
            else if (component.isBattery && !valueUsed[16])
            {
                float f7 = component.charge - carLogic.GetComponent<CarPerformanceC>().carCharge;
                if (component.charge < carLogic.GetComponent<CarPerformanceC>().carCharge)
                {
                    componentTitles[4].GetComponent<UILabel>().text = "- " + Mathf.Abs(Mathf.Round(f7)) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                    componentTitles[4].GetComponent<UILabel>().color = red;
                }
                else if (component.charge == carLogic.GetComponent<CarPerformanceC>().carCharge)
                {
                    if (component2.isInEngine)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = Mathf.Round(component.charge) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = Mathf.Round(f7) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                    }

                    componentTitles[4].GetComponent<UILabel>().color = blue;
                }
                else if (component.charge > carLogic.GetComponent<CarPerformanceC>().carCharge)
                {
                    if (carLogic.GetComponent<CarPerformanceC>().carEngineWearRate == 0f)
                    {
                        componentTitles[4].GetComponent<UILabel>().text = Mathf.Abs(Mathf.Round(f7)) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = green;
                    }
                    else
                    {
                        componentTitles[4].GetComponent<UILabel>().text = "+ " + Mathf.Abs(Mathf.Round(f7)) + " " + Language.Get("ui_pickup_48", "Inspector_UI");
                        componentTitles[4].GetComponent<UILabel>().color = green;
                    }
                }

                valueUsed[16] = true;
            }
            else if (component.weight > 0f && !valueUsed[15])
            {
                valueUsed[15] = true;
                SetEngineComponentWeight5();
            }
        }
        #endregion
    }
}
