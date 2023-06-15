using System.Collections;
using Cinemachine;
using Newtonsoft.Json;
using StarterAssets;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace VRTown.Game
{
    [RequireComponent(typeof(CharacterController), typeof(GUserMover))]
    // #if ENABLE_INPUT_SYSTEM 
    //     [RequireComponent(typeof(PlayerInput))]
    // #endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 5.335f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 3f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -30.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.2f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]

        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        Cinemachine3rdPersonFollow _threePersonCam;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        float? rotation = null;


        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDFly;
        private int _animIDMotionSpeed;

        public Animator Animator
        {
            get
            {
                if (_animator == null)
                {
                    AssignAnimationIDs();
                }
                return _animator;
            }
        }

        private CharacterController _controller;
        private PlayerController _input;
        private GameObject _mainCamera;
        ICharacterListener _listener = null;
        Animator _animator = null;
        StateData _state = new StateData();
        // Throttle _throttle;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;
        private bool _isFlying = false;
        private Vector3 _requestPos;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _input.Input.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
            // _throttle = new Throttle(this, 500);
        }

        public void Setup(PlayerController controller)
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _hasAnimator = Animator != null;
            _controller = GetComponent<CharacterController>();
            _input = controller;
#if ENABLE_INPUT_SYSTEM 
            // _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        public void SetListener(ICharacterListener listener)
        {
            _listener = listener;
        }

        private void Update()
        {
            if (_input == null) return;
            _hasAnimator = Animator != null;

            JumpAndGravity();
            GroundedCheck();
            Move();
            ChangeView();

            if (_requestPos != this.transform.position)
                _listener.OnTransformChanged(this.transform.position, new Vector3(0, 0, rotation ?? 0f), _state);

        }

        public void SetCamera(GCamera camera)
        {
            _threePersonCam = camera.CinemachineCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            CinemachineCameraTarget = camera.CinemachineCamera.Follow.gameObject;
        }

        public void SetPosition(Vector3 position, float rotation = 0f)
        {
            _controller.enabled = false;
            _controller.transform.position = position;
            StartCoroutine(SendMove(position, rotation));
            _controller.enabled = true;
        }

        private IEnumerator SendMove(Vector3 position, float rotation, StateData state = null)
        {
            if (_requestPos != position)
                _listener.OnTransformChanged(position, new Vector3(0, 0, rotation), state);
            yield return null;
        }

        private void LateUpdate()
        {
            if (_input == null) return;
            // _throttle?.Check();
            CameraRotation();
        }

        public void AssignAnimationIDs()
        {
            _animator = GetComponentInChildren<Animator>();
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDFly = Animator.StringToHash("Fly");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);
            // update animator if using character
            if (_hasAnimator)
            {
                Animator.SetBool(_animIDGrounded, Grounded);
                _state.Grounded = Grounded;
                // _throttle.Add(SendMove(_controller.transform.position, rotation ?? 0, _state));
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_isFlying)
            {
                targetSpeed *= 2f;
            }
            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation ?? 0f, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            var flyingPush = 0;
            if (_isFlying)
            {
                Debug.Log("[Character Pos_Fly]: " + this.transform.position);
                Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
                forward.y = 0.0f;
                forward = forward.normalized;
                Vector3 right = new Vector3(forward.z, 0, -forward.x);
                targetDirection = forward * inputDirection.z + right * inputDirection.x;
            }

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity + flyingPush, 0.0f) * Time.deltaTime);
            if (_isFlying && this.transform.position.y > 18)
            {
                _verticalVelocity = 0f;
                _controller.transform.position = new Vector3(_controller.transform.position.x, 18f, _controller.transform.position.z);
            }


            // update animator if using character
            if (_hasAnimator)
            {
                Animator.SetFloat(_animIDSpeed, _animationBlend);
                Animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
                _state.Speed = _animationBlend;
                _state.MotionSpeed = inputMagnitude;
            }

            if (_speed > 0.01)
            {
                if (rotation == null)
                {
                    transform.rotation.ToAngleAxis(out float angle, out Vector3 axis);
                    rotation = angle;
                }
                // _throttle.Add(SendMove(_controller.transform.position, rotation ?? 0, _state));

            }
        }

        private void ChangeView()
        {
            if (_input.changeView)
            {
                _threePersonCam.CameraDistance = 0;
                _threePersonCam.ShoulderOffset = new Vector3(0.4f, 0.5f, 0);
                // DisplayCharacter(false);
                // isHideCharacter = true;
            }
            else
            {
                _threePersonCam.CameraDistance = 4;
                _threePersonCam.ShoulderOffset = new Vector3(1, 0, 0);
                // if (isHideCharacter)
                // {
                // DisplayCharacter(true);
                // }

                //_input.changeView = true;
            }
        }

        private void JumpAndGravity()
        {
            if (!_input.flying && _isFlying)
            {
                _isFlying = false;
                _animator.SetBool(_animIDFly, false);
                _state.Fly = false;
                // _throttle.Add(SendMove(_controller.transform.position, rotation ?? 0, _state));


            }

            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    Animator.SetBool(_animIDJump, false);
                    _state.Jump = false;
                    Animator.SetBool(_animIDFreeFall, false);
                    _state.FreeFall = false;
                    // _throttle.Add(SendMove(_controller.transform.position, rotation ?? 0, _state));

                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (!_isFlying && _input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        Animator.SetBool(_animIDJump, true);
                        _state.Jump = true;
                        // _throttle.Add(SendMove(_controller.transform.position, rotation ?? 0, _state));

                    }
                }

                if (_input.flying && !_isFlying)
                {
                    Grounded = false;
                    _isFlying = true;
                    _verticalVelocity = Mathf.Sqrt(50 * JumpHeight * -2f * Gravity);
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFly, true);
                        _state.Fly = true;
                        // _throttle.Add(SendMove(_controller.transform.position, rotation ?? 0, _state));

                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        Animator.SetBool(_animIDFreeFall, true);
                        _state.FreeFall = true;
                        // _throttle.Add(SendMove(_controller.transform.position, rotation ?? 0, _state));

                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_isFlying && _controller.position().y > 18f)
            {
                _verticalVelocity = 0f;
                _controller.transform.position = new Vector3(_controller.transform.position.x, 18f, _controller.transform.position.z);
            }
            else if (_verticalVelocity < _terminalVelocity && !_isFlying)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }



        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}