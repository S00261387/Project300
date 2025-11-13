using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        public float MoveSpeed = 2.0f;
        public float SprintSpeed = 5.335f;
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;
        public float CameraAngleOverride = 0.0f;

        // Player
        private float _speed;
        private float _animationBlend;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // Dash
        public float DashSpeed = 15f;
        public float DashDuration = 0.25f;
        public float DashCooldown = 1f;
        private bool _isDashing = false;
        private float _dashTime = 0f;
        private float _dashCooldownTime = 0f;
        private Vector3 _dashDirection;

        // Timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // Animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif

        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;
        private bool _hasAnimator;

        private const float _threshold = 0.01f;

        private void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _hasAnimator = TryGetComponent(out _animator);

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            AssignAnimationIDs();
        }

        private void Update()
        {
            GroundedCheck();
            JumpAndGravity();

            if (!_isDashing)
                Move();

            // Dash using right mouse button
            _input.dash = Mouse.current.rightButton.wasPressedThisFrame;
            HandleDash();

            // Rotate player to mouse position
            FaceMousePosition();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

            if (_hasAnimator)
                _animator.SetBool(_animIDGrounded, Grounded);
        }

        private void CameraRotation()
        {
            // For top-down rotation only: rotate camera target with mouse input
            if (_input.look.sqrMagnitude >= _threshold)
            {
                float deltaTimeMultiplier = 1f; // simple top-down rotation
                Vector3 angles = CinemachineCameraTarget.transform.rotation.eulerAngles;
                float yaw = angles.y + _input.look.x * deltaTimeMultiplier;
                float pitch = angles.x - _input.look.y * deltaTimeMultiplier;
                pitch = Mathf.Clamp(pitch, BottomClamp, TopClamp);

                CinemachineCameraTarget.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }
        }

        private void FaceMousePosition()
        {
            Ray ray = _mainCamera.GetComponent<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                Vector3 lookDir = hitPoint - transform.position;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
            }
        }

        private void Move()
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_input.move == Vector2.zero) targetSpeed = 0f;

            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            _speed = Mathf.Lerp(_speed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

            Vector3 inputDir = new Vector3(_input.move.x, 0f, _input.move.y).normalized;
            if (_input.move != Vector2.zero)
                transform.rotation = Quaternion.LookRotation(inputDir, Vector3.up);

            _controller.Move(inputDir * (_speed * Time.deltaTime) + new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _speed);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void HandleDash()
        {
            if (_dashCooldownTime > 0f) _dashCooldownTime -= Time.deltaTime;

            if (_input.dash && Grounded && !_isDashing && _dashCooldownTime <= 0f)
            {
                _isDashing = true;
                _dashTime = DashDuration;
                _dashCooldownTime = DashCooldown;
                _input.DashInput(false);

                Vector3 inputDir = new Vector3(_input.move.x, 0f, _input.move.y);
                _dashDirection = inputDir.sqrMagnitude > 0.01f ? inputDir.normalized : Vector3.zero;
            }

            if (_isDashing)
            {
                if (_dashDirection.sqrMagnitude > 0.01f)
                {
                    Vector3 dashMove = _dashDirection * DashSpeed * Time.deltaTime;
                    dashMove.y = _verticalVelocity * Time.deltaTime;
                    _controller.Move(dashMove);
                }

                _dashTime -= Time.deltaTime;
                if (_dashTime <= 0f) _isDashing = false;
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_verticalVelocity < 0f)
                    _verticalVelocity = -2f;

                if (_input.jump && _jumpTimeoutDelta <= 0f)
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                if (_jumpTimeoutDelta >= 0f)
                    _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                if (_fallTimeoutDelta >= 0f)
                    _fallTimeoutDelta -= Time.deltaTime;
                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
                _verticalVelocity += Gravity * Time.deltaTime;
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            Debug.Log("Footstep!");
        }

    }
}
