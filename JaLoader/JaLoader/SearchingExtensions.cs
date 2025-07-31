using UnityEngine;
using UnityEngine.UI;

public static class SearchingExtensions
{
    public static Transform FindDeepChild(this Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child;
        }
        return null;
    }

    public static Button FindButton(this GameObject parent, string name)
    {
        Transform buttonTransform = parent.transform.Find(name);
        if (buttonTransform != null)
        {
            Button button = buttonTransform.GetComponent<Button>();
            if (button != null)
            {
                return button;
            }
        }
        return null;
    }
    public static Button FindDeepButton(this GameObject parent, string name)
    {
        Transform child = parent.transform.FindDeepChild(name);
        if (child != null)
        {
            Button button = child.GetComponent<Button>();
            if (button != null)
            {
                return button;
            }
        }
        return null;
    }

    public static Transform Find(this MonoBehaviour parent, string name)
    {
        return parent.transform.Find(name);
    }

    public static Dropdown FindDropdown(this GameObject parent, string name)
    {
        Transform dropdownTransform = parent.transform.Find(name);
        if (dropdownTransform != null)
        {
            Dropdown dropdown = dropdownTransform.GetComponent<Dropdown>();
            if (dropdown != null)
            {
                return dropdown;
            }
        }
        return null;
    }

    public static Dropdown FindDeepDropdown(this GameObject parent, string name)
    {
        Transform child = parent.transform.FindDeepChild(name);
        if (child != null)
        {
            Dropdown dropdown = child.GetComponent<Dropdown>();
            if (dropdown != null)
            {
                return dropdown;
            }
        }
        return null;
    }
    public static Toggle FindToggle(this GameObject parent, string name)
    {
        Transform toggleTransform = parent.transform.Find(name);
        if (toggleTransform != null)
        {
            Toggle toggle = toggleTransform.GetComponent<Toggle>();
            if (toggle != null)
            {
                return toggle;
            }
        }
        return null;
    }

    public static Toggle FindDeepToggle(this GameObject parent, string name)
    {
        Transform child = parent.transform.FindDeepChild(name);
        if (child != null)
        {
            Toggle toggle = child.GetComponent<Toggle>();
            if (toggle != null)
            {
                return toggle;
            }
        }
        return null;
    }
    public static InputField FindInputField(this GameObject parent, string name)
    {
        Transform inputFieldTransform = parent.transform.Find(name);
        if (inputFieldTransform != null)
        {
            InputField inputField = inputFieldTransform.GetComponent<InputField>();
            if (inputField != null)
            {
                return inputField;
            }
        }
        return null;
    }

    public static InputField FindDeepInputField(this GameObject parent, string name)
    {
        Transform child = parent.transform.FindDeepChild(name);
        if (child != null)
        {
            InputField inputField = child.GetComponent<InputField>();
            if (inputField != null)
            {
                return inputField;
            }
        }
        return null;
    }

    public static Slider FindSlider(this GameObject parent, string name)
    {
        Transform sliderTransform = parent.transform.Find(name);
        if (sliderTransform != null)
        {
            Slider slider = sliderTransform.GetComponent<Slider>();
            if (slider != null)
            {
                return slider;
            }
        }
        return null;
    }

    public static Slider FindDeepSlider(this GameObject parent, string name)
    {
        Transform child = parent.transform.FindDeepChild(name);
        if (child != null)
        {
            Slider slider = child.GetComponent<Slider>();
            if (slider != null)
            {
                return slider;
            }
        }
        return null;
    }

    public static GameObject FindDeepChildObject(this Transform parent, string name)
    {
        Transform child = parent.FindDeepChild(name);
        return child != null ? child.gameObject : null;
    }

    public static GameObject FindObject(this GameObject parent, string name)
    {
        return parent.transform.Find(name).gameObject;
    }

    public static Transform Find(this GameObject parent, string name)
    {
        return parent.transform.Find(name);
    }

    public static Transform FindDeepChild(this GameObject parent, string name)
    {
        return parent.transform.FindDeepChild(name);
    }

    public static GameObject FindDeepChildObject(this GameObject parent, string name)
    {
        Transform child = parent.transform.FindDeepChild(name);
        return child != null ? child.gameObject : null;
    }
}