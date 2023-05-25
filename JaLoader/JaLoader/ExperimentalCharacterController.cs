using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Security.Permissions;
using System.Text;
using UnityEngine;

namespace JaLoader
{
    public class ExperimentalCharacterController : MonoBehaviour
    {
        private CarLogicC carLogic;

        private CharacterController cc;
        private MouseLook mouseLook;
        private Rigidbody rb;
        private RigidbodyControllerC rbc;
        
        private GameObject _camera;
        private HeadBobberC headBobber;
        private FootstepsC footstepsC;

        private GameObject groundCheck;
        
        private Vector3 velocity;
        private LayerMask groundMask;
        private float groundDistance = 0.225f;
        private float jumpHeight = 2;
        private int speed;
        private bool canMove = true;
        private bool isGrounded;
        private bool crouching;

        private bool setParkingBrake;

        public bool isDebugCameraEnabled;

        /*bool lerping;
        bool lerpingTo0;
        bool coroutinesStoped;
        float x = 0;
        float z = 0;*/

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;

            rbc = GetComponent<RigidbodyControllerC>();
            rbc.enabled = false;

            GetComponent<CapsuleCollider>().enabled = false;
            GetComponent<CapsuleCollider>().isTrigger = true;


            _camera = Camera.main.gameObject;
            headBobber = _camera.GetComponent<HeadBobberC>();
            footstepsC = _camera.transform.parent.GetComponent<FootstepsC>();

            headBobber.enabled = true;

            carLogic = FindObjectOfType<CarLogicC>();
            mouseLook = FindObjectOfType<MouseLook>();

            cc = gameObject.AddComponent<CharacterController>();

            groundCheck = Instantiate(new GameObject());
            groundCheck.transform.parent = gameObject.transform;
            groundCheck.transform.localPosition = new Vector3(0, -1.25f, 0);
            groundCheck.layer = LayerMask.GetMask("Player");

            groundMask = (1 << 14) | (1 << 0) | (1 << 1) | (1 << 17) | (1 << 18) | (1 << 21) | (1 << 22);

            cc.height = 2.2f;
            cc.radius = 0.5f;
        }

        void Update()
        {
            if (isDebugCameraEnabled) return;

            if (!mouseLook.isSat && !carLogic.isPushingCar)
            {
                canMove = true;

                if (!setParkingBrake)
                {
                    setParkingBrake = true;

                    foreach (GameObject item in carLogic.wheelObjects)
                    {
                        WheelCollider wheelCollider = item.GetComponent<WheelScriptPCC>().GetComponent<WheelCollider>();
                        WheelFrictionCurve sidewaysFriction = wheelCollider.sidewaysFriction;
                        sidewaysFriction.stiffness = 10000f;
                        wheelCollider.sidewaysFriction = sidewaysFriction;
                        WheelFrictionCurve forwardFriction = wheelCollider.forwardFriction;
                        forwardFriction.stiffness = 10000f;
                        wheelCollider.forwardFriction = forwardFriction;
                    }
                }
            }

            if (mouseLook.isSat || carLogic.isPushingCar)
            {
                canMove = false;
                setParkingBrake = false;

                headBobber.bobbingSpeed = 0;
                headBobber.bobbingAmount = 0;
            }

            if (carLogic.isPushingCar)
            {
                headBobber.bobbingSpeed = 3f;
                headBobber.bobbingAmount = 0.025f;
            }

            if (!canMove)
                return;

            rb.isKinematic = true;
            rbc.enabled = false;

            isGrounded = Physics.CheckSphere(groundCheck.transform.position, groundDistance, groundMask);

            if(isGrounded && velocity.y < 0)
                velocity.y = -5f;

            /*if (Input.GetKey(MainMenuC.Global.assignedInputStrings[0]) || Input.GetKey(MainMenuC.Global.assignedInputStrings[1]))
            {
                if (z < 1)
                {
                    if ((lerping || lerpingTo0) && !coroutinesStoped)
                    {
                        lerping = false;
                        lerpingTo0 = false;
                        coroutinesStoped = true;
                        StopAllCoroutines();
                    }

                    StartCoroutine(LerpZ(z, 1));
                }
            }
            else if (Input.GetKey(MainMenuC.Global.assignedInputStrings[2]) || Input.GetKey(MainMenuC.Global.assignedInputStrings[3]))
            {
                if (z > -1)
                {
                    if ((lerping || lerpingTo0) && !coroutinesStoped)
                    {
                        lerping = false;
                        lerpingTo0 = false;
                        coroutinesStoped = true;
                        StopAllCoroutines();
                    }

                    StartCoroutine(LerpZ(z, -1));
                }
            }
            else
            {
                if (z != 0 && !lerpingTo0)
                {
                    StopAllCoroutines();
                    coroutinesStoped = true;
                    StartCoroutine(LerpTo0(z));
                }
            }*/
           
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            
            Vector3 move = transform.right * x + transform.forward * z;

            if (Input.GetKey(MainMenuC.Global.assignedInputStrings[28]) || Input.GetKey(MainMenuC.Global.assignedInputStrings[29]))
            {
                crouching = true;

                headBobber.midpoint = 0.15f;
                headBobber.bobbingSpeed = 1.5f;
                headBobber.bobbingAmount = 0.005f;

                _camera.transform.localPosition = new Vector3(_camera.transform.localPosition.x, 0.15f, _camera.transform.localPosition.z);
                speed = 3;
                jumpHeight = 0.75f;
                
                footstepsC.audioStepLength = 0.65f;
            }
            else
            {
                _camera.transform.localPosition = new Vector3(_camera.transform.localPosition.x, 0.8f, _camera.transform.localPosition.z);
                jumpHeight = 2;
                headBobber.midpoint = 0.8f;
                crouching = false;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (!crouching)
                {
                    speed = 25;

                    headBobber.bobbingSpeed = 8f;
                    headBobber.bobbingAmount = 0.045f;

                    if (isGrounded)
                    {
                        footstepsC.audioStepLength = 0.25f;
                    }
                    else
                    {
                        footstepsC.audioStepLength = 0;
                    }
                }
            }
            else
            {
                if (!crouching)
                {
                    speed = 8;

                    headBobber.bobbingSpeed = 5f;
                    headBobber.bobbingAmount = 0.025f;

                    if (isGrounded)
                    {
                        footstepsC.audioStepLength = 0.3f;
                    }
                    else
                    {
                        footstepsC.audioStepLength = 0;
                    }
                }
            }

            cc.Move(move * speed * Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * -35f);
            }

            velocity.y += -35f * Time.deltaTime;

            cc.Move(velocity * Time.deltaTime);
        }

        /*IEnumerator LerpZ(float startValue, float endValue)
        {
            lerpingTo0 = false;
            lerping = true;
            float timeElapsed = 0;
            while (timeElapsed < 0.5f)
            {
                z = Mathf.Lerp(startValue, endValue, timeElapsed / 0.25f);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            z = endValue;
            lerping = false;
            coroutinesStoped = false;
        }

        IEnumerator LerpTo0(float startValue)
        {
            lerpingTo0 = true;
            lerping = false;
            float timeElapsed = 0;
            while (timeElapsed < 0.5f)
            {
                z = Mathf.Lerp(startValue, 0, timeElapsed / 0.25f);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            z = 0;
            lerpingTo0 = false;
            coroutinesStoped = false;
        }*/
    }
}
