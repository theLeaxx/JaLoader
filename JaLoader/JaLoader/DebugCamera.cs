using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JaLoader
{
    //Code taken from the SimpleCameraController script which is found in the Unity URP/LWRP Template.
    public class DebugCamera : MonoBehaviour
    {
        class CameraState
        {
            public float yaw;
            public float pitch;
            public float roll;
            public float x;
            public float y;
            public float z;

            public void SetFromTransform(Transform t)
            {
                pitch = t.eulerAngles.x;
                yaw = t.eulerAngles.y;
                roll = t.eulerAngles.z;
                x = t.position.x;
                y = t.position.y;
                z = t.position.z;
            }

            public void Translate(Vector3 translation)
            {
                Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;

                x += rotatedTranslation.x;
                y += rotatedTranslation.y;
                z += rotatedTranslation.z;
            }

            public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
            {
                yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
                pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
                roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);

                x = Mathf.Lerp(x, target.x, positionLerpPct);
                y = Mathf.Lerp(y, target.y, positionLerpPct);
                z = Mathf.Lerp(z, target.z, positionLerpPct);
            }

            public void UpdateTransform(Transform t)
            {
                t.eulerAngles = new Vector3(pitch, yaw, roll);
                t.position = new Vector3(x, y, z);
            }
        }

        CameraState m_TargetCameraState = new CameraState();
        CameraState m_InterpolatingCameraState = new CameraState();

        //Movement Settings
        //Exponential boost factor on translation, controllable by mouse wheel.
        public float boost = 3.5f;

        //Time it takes to interpolate camera position 99% of the way to the target.
        //Keep inbetween 0.001f and 1f
        public float positionLerpTime = 0.2f;

        //Rotation Settings
        //X = Change in mouse position.
        //Y = Multiplicative factor for camera rotation.
        public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

        //Time it takes to interpolate camera rotation 99% of the way to the target.
        //Keep inbetween 0.001f and 1f
        public float rotationLerpTime = 0.01f;

        //Whether or not to invert our Y axis for mouse input to rotation.
        public bool invertY = false;

        public bool isCameraEnabled = false;
        private bool lockedCursor;
        private bool postCamera;
        public GameObject cameraObj;

        private GameObject mainCameraObj;
        private bool createdInGamePPCamera;
        private GameObject cursors;

        private bool firstTimeOpening = true;

        void OnEnable()
        {
            cameraObj = ModHelper.Instance.debugCam;
            mainCameraObj = Camera.main.gameObject;

            ResetCameraPos();
            
            EventsManager.Instance.OnGameLoad += OnGameLoad;
            EventsManager.Instance.OnMenuLoad += OnMenuLoad;

            if (SceneManager.GetActiveScene().buildIndex == 3)
                cursors = GameObject.Find("Cursors");
        }

        public void ResetCameraPos()
        {
            cameraObj.transform.position = mainCameraObj.transform.position;
            cameraObj.transform.rotation = mainCameraObj.transform.rotation;

            m_InterpolatingCameraState.SetFromTransform(cameraObj.transform);
            m_TargetCameraState.SetFromTransform(cameraObj.transform);
        }

        private void TeleportPlayerToCam()
        {
            GameObject.Find("First Person Controller").transform.position = cameraObj.transform.position;
        }

        private void OnGameLoad()
        {
            firstTimeOpening = true;
            cameraObj.transform.GetChild(0).gameObject.SetActive(true);
            cameraObj.transform.GetChild(2).gameObject.SetActive(false);

            mainCameraObj = Camera.main.gameObject;

            createdInGamePPCamera = cameraObj.transform.GetChild(0).gameObject.GetComponent<FlareLayer>();

            if (!createdInGamePPCamera)
            {
                createdInGamePPCamera = true;

                var components = mainCameraObj.GetComponents<MonoBehaviour>();

                foreach (MonoBehaviour behaviour in components)
                {
                    cameraObj.transform.GetChild(0).gameObject.AddComponent(behaviour.GetType());
                    FieldInfo[] fields = behaviour.GetType().GetFields();
                    foreach (FieldInfo field in fields)
                    {
                        field.SetValue(cameraObj.transform.GetChild(0).GetComponent(behaviour.GetType()), field.GetValue(behaviour));
                    }
                }
                Destroy(cameraObj.transform.GetChild(0).GetComponent<DragRigidbodyC>());
                Destroy(cameraObj.transform.GetChild(0).GetComponent<MouseLook>());
                Destroy(cameraObj.transform.GetChild(0).GetComponent<MainMenuC>());
                Destroy(cameraObj.transform.GetChild(0).GetComponent<HeadBobberC>());
                Destroy(cameraObj.transform.GetChild(0).GetComponent<JoyStick_Example>());
                Destroy(cameraObj.transform.GetChild(0).GetComponent<CopyToScreenRT>());
                Destroy(cameraObj.transform.GetChild(0).GetComponent<DragRigidbodyC>());
                Destroy(cameraObj.transform.GetChild(0).GetComponent<DebugCamera>());
                Destroy(cameraObj.transform.GetChild(0).GetComponent<DragRigidbodyC_ModExtension>());
                Destroy(cameraObj.transform.GetChild(0).GetComponent<ModsPageToggle>());
                Destroy(cameraObj.transform.GetChild(0).GetComponent<MenuMouseInteractionsC>());
                Destroy(cameraObj.transform.GetChild(0).GetComponent<MainMenuCReceiver>());
                Destroy(cameraObj.transform.GetChild(0).GetComponent<MotelsReceiver>());
                Destroy(cameraObj.transform.GetChild(0).GetComponent<HarmonyManager>());
                Destroy(cameraObj.transform.GetChild(0).GetComponent<CoroutineManager>());
                Destroy(cameraObj.transform.GetChild(0).GetComponent<LaikaCatalogueExtension>());

                cameraObj.transform.GetChild(0).gameObject.AddComponent<FlareLayer>();
                cameraObj.transform.GetChild(0).gameObject.AddComponent<AudioListener>();
            }

            ResetCameraPos();
        }

        private void OnMenuLoad()
        {
            mainCameraObj = Camera.main.gameObject;

            ResetCameraPos();
        }

        Vector3 GetInputTranslationDirection()
        {
            Vector3 direction = new Vector3();
            if (Input.GetKey(KeyCode.W))
            {
                direction += Vector3.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                direction += Vector3.back;
            }
            if (Input.GetKey(KeyCode.A))
            {
                direction += Vector3.left;
            }
            if (Input.GetKey(KeyCode.D))
            {
                direction += Vector3.right;
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                direction += Vector3.down;
            }
            if (Input.GetKey(KeyCode.Space))
            {
                direction += Vector3.up;
            }

            return direction;
        }

        public void Enable()
        {
            isCameraEnabled = true;
            cameraObj.SetActive(true);
            mainCameraObj.GetComponent<Camera>().enabled = !isCameraEnabled;
            mainCameraObj.GetComponent<Camera>().Render();

            lockedCursor = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void Disable()
        {
            isCameraEnabled = false;
            cameraObj.SetActive(true);
            mainCameraObj.GetComponent<Camera>().enabled = !isCameraEnabled;
        }

        public void Toggle()
        {
            isCameraEnabled = !isCameraEnabled;
            cameraObj.SetActive(isCameraEnabled);
            mainCameraObj.GetComponent<Camera>().enabled = !isCameraEnabled;

            if (isCameraEnabled)
            {
                lockedCursor = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                Console.Log("JaLoader", "Debug Camera Enabled!");
            }
            else
                Console.Log("JaLoader", "Debug Camera Disabled!");

            if (SettingsManager.Instance.UseExperimentalCharacterController)
            {
                mainCameraObj.transform.parent.GetComponent<EnhancedMovement>().isDebugCameraEnabled = isCameraEnabled;
            }
            else
            {
                mainCameraObj.transform.parent.GetComponent<Rigidbody>().isKinematic = isCameraEnabled;
            }
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
            {
                if (SceneManager.GetActiveScene().buildIndex == 3 && firstTimeOpening)
                {
                    firstTimeOpening = false;
                    ResetCameraPos();
                }

                Toggle();
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                lockedCursor = !lockedCursor;
                Cursor.lockState = lockedCursor ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !lockedCursor;
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (SceneManager.GetActiveScene().buildIndex == 3)
                {
                    cursors.SetActive(!cursors.activeSelf);
                    Cursor.lockState = cursors.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
                    Cursor.visible = cursors.activeSelf;
                }
            }

            if (!isCameraEnabled || !lockedCursor)
                return;

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                ResetCameraPos();
                Console.Log("JaLoader", "Teleported Debug Camera to player!");
                return;
            }

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P))
            {
                postCamera = !postCamera;
                cameraObj.transform.GetChild(1).gameObject.SetActive(!postCamera);

                if (SceneManager.GetActiveScene().buildIndex == 3)
                {
                    cameraObj.transform.GetChild(0).gameObject.SetActive(postCamera);
                    cameraObj.transform.GetChild(2).gameObject.SetActive(false);
                }
                else
                {
                    cameraObj.transform.GetChild(2).gameObject.SetActive(postCamera);
                    cameraObj.transform.GetChild(0).gameObject.SetActive(false);
                }
            }

            if (Input.GetKeyDown(KeyCode.F7))
            {
                if (SceneManager.GetActiveScene().buildIndex == 3)
                {
                    TeleportPlayerToCam();
                    Console.Log("JaLoader", "Teleported player to Debug Camera!");
                }
            }

            Vector3 translation = Vector3.zero;

            var mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (invertY ? 1 : -1));

            var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

            m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
            m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;

            translation = GetInputTranslationDirection() * Time.deltaTime;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                translation *= 10.0f;
            }

            boost += Input.mouseScrollDelta.y * 0.2f;
            translation *= Mathf.Pow(2.0f, boost);

            m_TargetCameraState.Translate(translation);

            var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
            var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
            m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

            m_InterpolatingCameraState.UpdateTransform(cameraObj.transform);
        }
    }
}
