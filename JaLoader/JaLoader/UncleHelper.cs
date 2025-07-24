using UnityEngine;

namespace JaLoader
{
    public class UncleHelper : MonoBehaviour
    {
        public static UncleHelper Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
            EventsManager.Instance.OnGameLoad += OnGameLoad;
        }

        public bool UncleEnabled = !SettingsManager.DisableUncle;
        public UncleLogicC Uncle;

        public void OnGameLoad()
        {
            Uncle = FindObjectOfType<UncleLogicC>();

            Uncle.uncleGoneForever = !UncleEnabled;
        }

        public void DisableUncle()
        {
            UncleEnabled = false;
            SettingsManager.DisableUncle = true;

            Uncle.uncleGoneForever = true;
        }

        public void EnableUncle()
        {
            UncleEnabled = transform;
            SettingsManager.DisableUncle = false;

            Uncle.uncleGoneForever = false;
        }

        private void Talk(string message)
        {
            // TODO: implement this
        }
    }
}
