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
    public class EnhancedMovement : MonoBehaviour
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
        
        public Vector3 velocity;
        private LayerMask groundMask;
        private float groundDistance = 0.225f;
        public float jumpHeight = 2;
        private float speed;
        private bool canMove = true;
        public bool canSprint = true;
        public bool isMoving = false;
        public bool isSprinting = false;
        public bool canJump = true;
        public bool isGrounded;
        private bool crouching;

        private bool setParkingBrake;

        public bool isDebugCameraEnabled;

        private Vector3 currentVelocity;
        private Vector3 targetVelocity;

        public float maxJumpHeight = 2f;
        public float maxSprintSpeed = 10f;
        public float maxWalkSpeed = 8f;
        public float maxCrouchSpeed = 3f;

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
                headBobber.enabled = true;

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

            if (mouseLook.isSat)
            {
                canMove = false;
                setParkingBrake = false;

                headBobber.enabled = false;
                headBobber.bobbingSpeed = 0;
                headBobber.bobbingAmount = 0;
            }

            if (carLogic.isPushingCar)
            {
                canMove = false;
                setParkingBrake = false;

                headBobber.enabled = true;
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

            float z = 0;
            float x = 0;

            if (Input.GetKey(MainMenuC.Global.assignedInputStrings[0]) || Input.GetKey(MainMenuC.Global.assignedInputStrings[1]))
            {
                z = 1f;
            }
            else if (Input.GetKey(MainMenuC.Global.assignedInputStrings[2]) || Input.GetKey(MainMenuC.Global.assignedInputStrings[3]))
            {
                z = -1f;
            }

            if (Input.GetKey(MainMenuC.Global.assignedInputStrings[4]) || Input.GetKey(MainMenuC.Global.assignedInputStrings[5]))
            {
                x = -1f;
            }
            else if (Input.GetKey(MainMenuC.Global.assignedInputStrings[6]) || Input.GetKey(MainMenuC.Global.assignedInputStrings[7]))
            {
                x = 1f;
            }

            if (x == 0 && z == 0)
            {
                isMoving = false;
            }
            else
            {
                isMoving = true;
            }

            Vector3 _move = transform.right * x + transform.forward * z;
            targetVelocity = _move.normalized;
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.deltaTime * 15f);
            Vector3 move = currentVelocity;

            if (Input.GetKey(MainMenuC.Global.assignedInputStrings[28]) || Input.GetKey(MainMenuC.Global.assignedInputStrings[29]))
            {
                crouching = true;

                headBobber.midpoint = 0.15f;
                headBobber.bobbingSpeed = 1.5f;
                headBobber.bobbingAmount = 0.005f;

                _camera.transform.localPosition = new Vector3(_camera.transform.localPosition.x, 0.15f, _camera.transform.localPosition.z);
                speed = maxCrouchSpeed;
                jumpHeight = 0.75f;
                
                footstepsC.audioStepLength = 0.65f;
            }
            else
            {
                _camera.transform.localPosition = new Vector3(_camera.transform.localPosition.x, 0.8f, _camera.transform.localPosition.z);
                jumpHeight = maxJumpHeight;
                headBobber.midpoint = 0.8f;
                crouching = false;
            }

            if (Input.GetKey(KeyCode.LeftShift) && canSprint)
            {
                if (!crouching)
                {
                    isSprinting = true;

                    speed = maxSprintSpeed;

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
                isSprinting = false;

                if (!crouching)
                {                    
                    speed = maxWalkSpeed;

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

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded && canJump)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * -35f);
            }

            velocity.y += -35f * Time.deltaTime;

            cc.Move(velocity * Time.deltaTime);

            if(x == 0 && z == 0)
                currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, Time.deltaTime * 15f);
        }
    }
}
