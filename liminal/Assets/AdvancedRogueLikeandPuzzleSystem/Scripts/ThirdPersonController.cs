using UnityEngine;
using System.Collections;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Movement Speeds")]
        public float MoveSpeed = 2.0f;
        public float SprintSpeed = 5.335f;
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;
        public float SpeedChangeRate = 10.0f;

        [Header("Jumping & Gravity")]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;

        [Header("Ground Checking")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        [Header("Animation IDs")]
        public int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        [Header("References & Audio")]
        public Animator _animator;
        public ParticleSystem SwimmingParticle;
        public bool isSwimming = false;

        [Header("Misc")]
        public float _speed;
        public float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private CharacterController _controller;
        private GameObject _mainCamera;
        public static ThirdPersonController Instance;

        // Additional variables for input and autopilot
        private Vector2 vectorMove = Vector2.zero;
        private bool isAutoWalking = false;

        // Whether we are currently inside a "tunnel" allowing Z-axis movement
        private bool inTunnel = false;

        private const float _threshold = 0.01f;
        private LadderScript currentLadder;
        private bool _hasAnimator;
        private Transform waterInTouch;
        float lastTime = 0;

        // Additional Sprint references
        public TrailRenderer SprinttrailRenderer;
        public ParticleSystem SprintParticle;
        public int ManaSpending_Sprint = 20;

        // ---------------------------
        // FLY MODE / DOUBLE-JUMP FIELDS
        // ---------------------------
        [Header("Flying")]
        [Tooltip("Whether or not the player is currently flying.")]
        public bool isFlying = false;

        [Tooltip("Threshold time (seconds) for detecting a quick second jump to enable flight.")]
        public float doubleJumpThreshold = 0.3f;

        [Tooltip("A simple timer to detect two quick jumps.")]
        private float lastJumpTime = 0f;

        [Tooltip("Number of jumps performed since last grounded.")]
        private int jumpCount = 0;

        [Tooltip("Speed at which the player moves in flight mode.")]
        public float flySpeed = 5f;

        [Tooltip("Vertical speed while flying upward/downward.")]
        public float flyVerticalSpeed = 5f;

        [Tooltip("Optional mana drain for flight, per second.")]
        public int manaDrainPerSecond = 5;

        private float manaDrainTimer = 0f;

        [Header("Z Reset Settings")]
        [Tooltip("Speed at which the player's z-position is reset to 0 when grounded.")]
        public float resetZSpeed = 10f;
        [Tooltip("The y-level considered to be the ground (e.g. 0).")]
        public float groundYThreshold = 0f;

        private void Awake()
        {
            Instance = this;
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();

            AssignAnimationIDs();

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            if (HeroController.instance.Health <= 0) return;

            if (isFlying)
            {
                HandleFlightMode();
            }
            else
            {
                JumpAndGravity();
                GroundedCheck();
                Move();
            }

            if (isFlying && (Grounded || HeroController.instance.Mana <= 0))
            {
                DisableFlyMode();
            }
        }

        /// <summary>
        /// In LateUpdate, if the player is grounded, not in a tunnel, and their y position
        /// is at or below the defined ground level, their z-position is smoothly moved to 0.
        /// </summary>
        private void LateUpdate()
        {
            if (Grounded && !inTunnel && transform.position.y <= groundYThreshold)
            {
                Vector3 pos = transform.position;
                pos.z = Mathf.MoveTowards(pos.z, 0f, resetZSpeed * Time.deltaTime);
                transform.position = pos;
            }
        }

        /// <summary>
        /// Call this externally to force the player to move forward automatically.
        /// </summary>
        public void RunInToArea()
        {
            isAutoWalking = true;
        }

        /// <summary>
        /// Call this externally to restore manual control.
        /// </summary>
        public void ActivatePlayer()
        {
            isAutoWalking = false;
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
            if (!isSwimming)
            {
                Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
                Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDGrounded, Grounded);
                }

                if (Grounded && !isFlying)
                {
                    jumpCount = 0;
                }
            }
        }

        private void Move()
        {
            if (GameManager.Instance.controllerType == ControllerType.KeyboardMouse)
            {
                vectorMove = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                float targetSpeedTemp = Input.GetKey(GameManager.Instance.Keycode_Sprint) ? SprintSpeed : MoveSpeed;
                _speed = Mathf.Lerp(_speed, targetSpeedTemp, 0.2f);
            }
            else if (GameManager.Instance.controllerType == ControllerType.Mobile)
            {
                vectorMove = new Vector2(SimpleJoystick.Instance.HorizontalValue, SimpleJoystick.Instance.VerticalValue);

                if (Mathf.Abs(SimpleJoystick.Instance.VerticalValue) > 0.75f ||
                    Mathf.Abs(SimpleJoystick.Instance.HorizontalValue) > 0.75f)
                {
                    _speed = Mathf.Lerp(_speed, SprintSpeed, 0.1f);
                }
                else
                {
                    _speed = Mathf.Lerp(_speed, MoveSpeed, 0.1f);
                }
            }

            // In non-tunnel mode, restrict any z-axis input.
            if (!inTunnel)
            {
                vectorMove.y = 0f;
            }

            if (vectorMove == Vector2.zero) _speed = 0.0f;

            float currentHorizontalSpeed = new Vector3(vectorMove.x, 0.0f, vectorMove.y).magnitude;
            float speedOffset = Time.deltaTime;

            _animationBlend = Mathf.Lerp(_animationBlend, _speed, Time.deltaTime * SpeedChangeRate);

            if (vectorMove != Vector2.zero)
            {
                if (Time.time > lastTime + 0.35f && !HeroController.instance.inDefendMode && !isSwimming)
                {
                    lastTime = Time.time;
                    HeroController.instance.audio_AyakSesi.clip = AudioManager.instance.audioClip_Footsteps[
                        Random.Range(0, AudioManager.instance.audioClip_Footsteps.Length)];
                    HeroController.instance.audio_AyakSesi.Play();
                }
            }
            else
            {
                HeroController.instance.audio_AyakSesi.Stop();
            }

            if (vectorMove != Vector2.zero)
            {
                Vector3 inputDir = new Vector3(vectorMove.x, 0.0f, vectorMove.y).normalized;
                _targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, 1.0f);
            }

            if (_speed == 0 && (_verticalVelocity < 0 && Grounded))
                return;

            if (HeroController.instance.isHitting || HeroController.instance.inDefendMode)
                return;

            if (isSwimming)
            {
                _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime)
                                 + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

                if (transform.position.y < waterInTouch.transform.position.y - 0.75f)
                {
                    transform.position = new Vector3(transform.position.x,
                        waterInTouch.transform.position.y - 0.75f,
                        transform.position.z);
                }
            }
            else
            {
                _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime)
                                 + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }

            if ((Input.GetKeyDown(GameManager.Instance.Keycode_Sprint) || Input.GetButtonDown("Sprint")) &&
                HeroController.instance.Mana >= ManaSpending_Sprint)
            {
                GameCanvas_Controller.instance.Update_Mana_Bar(ManaSpending_Sprint);
                StartCoroutine(TrailRendererShow());
            }
        }

        public void Sprint_Now()
        {
            if (HeroController.instance.Mana >= ManaSpending_Sprint && vectorMove != Vector2.zero)
            {
                GameCanvas_Controller.instance.Update_Mana_Bar(ManaSpending_Sprint);
                StartCoroutine(TrailRendererShow());
            }
        }

        IEnumerator TrailRendererShow()
        {
            SprinttrailRenderer.enabled = true;
            SprintParticle.Play();
            AudioManager.instance.Play_Sprint();
            yield return new WaitForSeconds(0.1f);
            HeroController.instance.characterController.Move(transform.forward * 5);
            yield return new WaitForSeconds(0.2f);
            SprintParticle.Stop();
            SprinttrailRenderer.enabled = false;
        }

        private void JumpAndGravity()
        {
            if (!this.enabled) return;
            if (isSwimming) return;

            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (Input.GetKeyDown(GameManager.Instance.Keycode_Jump) && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                    jumpCount = 1;
                    lastJumpTime = Time.time;
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                if (!isFlying && Input.GetKeyDown(GameManager.Instance.Keycode_Jump))
                {
                    if (jumpCount == 1 && (Time.time - lastJumpTime <= doubleJumpThreshold))
                    {
                        EnableFlyMode();
                    }
                    else
                    {
                        jumpCount = 2;
                    }
                }
            }

            if (!isFlying && _verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void EnableFlyMode()
        {
            isFlying = true;
            jumpCount = 2;
            _verticalVelocity = 0f;

            if (_hasAnimator)
            {
                _animator.SetBool("IsFlying", true);
            }
        }

        private void DisableFlyMode()
        {
            isFlying = false;
            _verticalVelocity = 0f;

            if (_hasAnimator)
            {
                _animator.SetBool("IsFlying", false);
            }
        }

        private void HandleFlightMode()
        {
            if (manaDrainPerSecond > 0)
            {
                manaDrainTimer += Time.deltaTime;
                if (manaDrainTimer >= 1f)
                {
                    HeroController.instance.Mana -= manaDrainPerSecond;
                    GameCanvas_Controller.instance.Update_Mana_Bar(manaDrainPerSecond);
                    manaDrainTimer = 0f;
                }
            }

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 moveDirection = new Vector3(horizontal, 0f, vertical);

            bool moveUp = Input.GetKey(KeyCode.UpArrow);
            bool moveDown = Input.GetKey(KeyCode.DownArrow);

            float upDown = 0f;
            if (moveUp) upDown = 1f;
            if (moveDown) upDown = -1f;

            Vector3 flightVelocity = (transform.TransformDirection(moveDirection) * flySpeed)
                                     + (Vector3.up * upDown * flyVerticalSpeed);

            _controller.Move(flightVelocity * Time.deltaTime);

            if (moveDirection.magnitude > 0.01f)
            {
                float targetYaw = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                float smoothed = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetYaw, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothed, 0f);
            }

            if (_hasAnimator)
            {
                float flightSpeed = flightVelocity.magnitude;
                _animator.SetFloat(_animIDSpeed, flightSpeed);
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
            Gizmos.color = Grounded ? transparentGreen : transparentRed;
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Water"))
            {
                AudioManager.instance.Play_Swimming();
                SwimmingParticle.Play();
            }
            if (other.CompareTag("WaterForSwimming"))
            {
                _animator.SetBool("Swim", true);
                waterInTouch = other.transform;
                AudioManager.instance.Play_Swimming();
                isSwimming = true;
                SwimmingParticle.Play();
                Grounded = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Water"))
            {
                AudioManager.instance.Stop_Swimming();
                SwimmingParticle.Stop();
            }
            if (other.CompareTag("WaterForSwimming"))
            {
                AudioManager.instance.Stop_Swimming();
                _animator.SetBool("Swim", false);
                SwimmingParticle.Stop();
                isSwimming = false;
            }
        }

        /// <summary>
        /// Called from the TunnelScript when entering or exiting the tunnel.
        /// </summary>
        /// <param name="value">True if inside tunnel, false otherwise.</param>
        public void SetInTunnel(bool value)
        {
            inTunnel = value;
            Debug.Log("Tunnel state changed. Now inTunnel = " + inTunnel);
        }

        public void JumpNow()
        {
            if (!this.enabled) return;
            if (isSwimming) return;

            if (Grounded && _jumpTimeoutDelta <= 0.0f)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                Grounded = false;
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, true);
                }
                jumpCount = 1;
                lastJumpTime = Time.time;
            }
            else if (!Grounded && !isFlying)
            {
                if (jumpCount == 1 && (Time.time - lastJumpTime <= doubleJumpThreshold))
                {
                    EnableFlyMode();
                }
                else
                {
                    jumpCount = 2;
                }
            }
        }
    }
}
