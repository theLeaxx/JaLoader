using UnityEngine;
using JaLoader;
using System.Timers;
using Object = UnityEngine.Object;
using Timer = System.Timers.Timer;

namespace Doorstop
{
    class Entrypoint
    {
        private static Timer timer;

        public static void Start()
        {
            timer = new Timer(2000);
            timer.Elapsed += RunTimer;
            timer.Enabled = true;
        }   

        private static void RunTimer(object sender, ElapsedEventArgs e)
        {
            GameObject obj = new GameObject();
            obj.name = "JaLoader";

            GameObject insObj = Object.Instantiate(obj);
            insObj.AddComponent<AwakeClass>();
            Object.DontDestroyOnLoad(insObj);

            timer.Enabled = false;
            timer.Dispose();
            timer.Elapsed -= RunTimer;
            timer = null;
        }
    }

    class AwakeClass : MonoBehaviour
    {
        void Awake()
        {
            gameObject.AddComponent<ModLoader>();

            Destroy(gameObject.GetComponent<AwakeClass>());
        }
    }
}
