#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine;

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

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;

        [Header("Dash Settings")]
        public float DashSpeed = 12f;
        public float DashDistance = 4f;
        public float DashCooldown = 1f;

        [Header("Dash Visuals")]
        public GameObject AfterimagePrefab;
        public float AfterimageSpawnRate = 0.05f;

        // Player state
        private float _speed;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // Dash state
        private bool _isDashing = false;
        private float _dashCooldownTime = 0f;
        private Vector3 _dashDirection;
        private float _dashDistanceTraveled = 0f;
        private float _afterimageTimer = 0f;

        // Animator IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
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

            AssignAnimationIDs();
        }

        private void Update()
        {
            GroundedCheck();
            HandleDash();

            if (!_isDashing)
                Move();

            FaceMousePosition();
        }

        private void LateUpdate() => CameraRotation();

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
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
            if (_input.look.sqrMagnitude >= _threshold)
            {
                float deltaTimeMultiplier = 1f;
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

            _controller.Move(inputDir * (_speed * Time.deltaTime) + new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _speed);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void HandleDash()
        {
            if (_dashCooldownTime > 0f)
                _dashCooldownTime -= Time.deltaTime;

            if (Input.GetMouseButtonDown(1) && !_isDashing && _dashCooldownTime <= 0f)
            {
                Vector3 moveDir = new Vector3(_input.move.x, 0f, _input.move.y);
                if (moveDir.sqrMagnitude > 0.01f)
                {
                    _dashDirection = moveDir.normalized;
                    _dashDistanceTraveled = 0f;
                    _isDashing = true;
                    _dashCooldownTime = DashCooldown;
                    _afterimageTimer = 0f;
                }
            }

            if (_isDashing)
            {
                Vector3 dashStep = _dashDirection * (DashSpeed * Time.deltaTime);
                _controller.Move(dashStep);
                _dashDistanceTraveled += dashStep.magnitude;

                _afterimageTimer += Time.deltaTime;
                if (_afterimageTimer >= AfterimageSpawnRate && AfterimagePrefab != null)
                {
                    Instantiate(AfterimagePrefab, transform.position, transform.rotation);
                    _afterimageTimer = 0f;
                }

                if (_dashDistanceTraveled >= DashDistance)
                    _isDashing = false;
            }
        }

        public void OnFootstep()
        {
            Debug.Log("Footstep event triggered");
        }
    }
}
