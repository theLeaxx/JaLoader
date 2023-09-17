using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JaLoader
{
    public class LicensePlateCustomizer : MonoBehaviour
    {
        private ModHelper modHelper = ModHelper.Instance;
        private SettingsManager settingsManager = SettingsManager.Instance;

        private GameObject rearText;
        private GameObject frontText;

        private MeshRenderer frontPlate;
        private MeshRenderer rearPlate;

        private Color32 defaultWhiteColor;
        private Color32 blueColor = new Color32(0, 137, 255, 255);
        private Color32 redColor = new Color32(186, 35, 35, 255);

        private bool isInMenu = SceneManager.GetActiveScene().buildIndex == 1;

        private void Start()
        {
            if (settingsManager.ChangeLicensePlateText == LicensePlateStyles.None)
                return;

            var frame = transform.Find("TweenHolder/Frame");

            if (!isInMenu) rearText = frame.Find("R_LicensePlate/LicensePlate1").gameObject;
            frontText = frame.Find("F_LicensePlate").GetChild(0).gameObject;

            if (!isInMenu) rearPlate = frame.Find("").gameObject.GetComponent<MeshRenderer>();
            frontPlate = frame.Find("F_LicensePlate").GetChild(2).gameObject.GetComponent<MeshRenderer>();

            defaultWhiteColor = frontPlate.material.color;

            SetPlateText(settingsManager.LicensePlateText, settingsManager.ChangeLicensePlateText);

            EventsManager.Instance.OnSettingsSaved += OnSettingsSave;
        }

        private void OnSettingsSave()
        {
            SetPlateText(settingsManager.LicensePlateText, settingsManager.ChangeLicensePlateText);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5) && settingsManager.DebugMode)
            {
                SetPlateText(settingsManager.LicensePlateText, settingsManager.ChangeLicensePlateText);
            }
        }

        public void SetPlateText(string plateText, LicensePlateStyles style)
        {
            if (!isInMenu)
                rearText.GetComponent<TextMeshPro>().text = frontText.GetComponent<TextMeshPro>().text = plateText;
            else
                frontText.GetComponent<TextMeshPro>().text = plateText;

            switch (style)
            {
                case LicensePlateStyles.Default:
                    if (!isInMenu)
                    {
                        frontPlate.material.color = rearPlate.material.color = defaultWhiteColor;
                        rearText.GetComponent<TextMeshPro>().fontMaterial.color = frontText.GetComponent<TextMeshPro>().fontMaterial.color = Color.black;
                    }
                    else
                    {
                        frontPlate.material.color = defaultWhiteColor;
                        frontText.GetComponent<TextMeshPro>().fontMaterial.color = Color.black;
                    }
                    break;

                case LicensePlateStyles.DiplomaticRed:
                    if (!isInMenu)
                    {
                        frontPlate.material.color = rearPlate.material.color = redColor;
                        rearText.GetComponent<TextMeshPro>().fontMaterial.color = frontText.GetComponent<TextMeshPro>().fontMaterial.color = defaultWhiteColor;
                    }
                    else
                    {
                        frontPlate.material.color = redColor;
                        frontText.GetComponent<TextMeshPro>().fontMaterial.color = defaultWhiteColor;
                    }
                    break;

                case LicensePlateStyles.DiplomaticBlue:
                    if (!isInMenu)
                    {
                        frontPlate.material.color = rearPlate.material.color = blueColor;
                        rearText.GetComponent<TextMeshPro>().fontMaterial.color = frontText.GetComponent<TextMeshPro>().fontMaterial.color = defaultWhiteColor;
                    }
                    else
                    {
                        frontPlate.material.color = blueColor;
                        frontText.GetComponent<TextMeshPro>().fontMaterial.color = defaultWhiteColor;
                    }
                    break;
            }
        }
    }

    public enum LicensePlateStyles
    {
        None,
        Default,
        DiplomaticRed,
        DiplomaticBlue
    }
}
