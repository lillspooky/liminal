using UnityEngine;

namespace liminal
{
    [RequireComponent(typeof(CharacterController))]
    public class LiminalPlayerCharacterController : MonoBehaviour
    {
        [SerializeField] bool _isLocalPlayer;

        [Header("Movement Stats")]
        [SerializeField] float _moveSpeed = 10;
        [SerializeField] float _jumpSpeed = 10;
        [SerializeField] int _maxJumps = 1;
        [SerializeField] float _jumpSpeedModifier;

        CharacterController _characterController;
        PlayerInput _playerInput;
        float _verticalVelocity;
        float _modifiedJumpSpeed;
        int _jumpsRemaining;

        public bool IsLocalPlayer
        {
            get => _isLocalPlayer;
            set => _isLocalPlayer = value;
        }

        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = value;
        }

        public float JumpSpeed
        {
            get => _jumpSpeed;
            set => _jumpSpeed = value;
        }

        void Awake()
        {
            _playerInput = new();
            _characterController = GetComponent<CharacterController>();

            // NB: We assume you never want to have jumps push you downward. Maybe we allow this as an intended mechanic?
            if (_maxJumps * _jumpSpeedModifier >= _jumpSpeed)
                Debug.LogWarning($"{name} will end up with a negative jump speed");
        }

        void OnEnable()
        {
            _playerInput.Enable();
        }

        void OnDisable()
        {
            _playerInput.Disable();
        }

        void Update()
        {
            var movementThisFrame = Vector2.zero;
            if (_isLocalPlayer)
            {
                var deltaTime = Time.deltaTime;
                var defaultActions = _playerInput.Default;
                var moveInput = defaultActions.Move.ReadValue<Vector2>();
                var horizontalMovement = moveInput.x * _moveSpeed * deltaTime;
                movementThisFrame += horizontalMovement * Vector2.right;

                if (_characterController.isGrounded)
                {
                    _modifiedJumpSpeed = _jumpSpeed;
                    _jumpsRemaining = _maxJumps;
                    _verticalVelocity = 0;
                }
                else
                {
                    _verticalVelocity += Physics.gravity.y * deltaTime;
                }

                if (defaultActions.Jump.WasPressedThisFrame() && _jumpsRemaining-- > 0)
                {
                    _verticalVelocity += _modifiedJumpSpeed;
                    _modifiedJumpSpeed += _jumpSpeedModifier;
                }

                movementThisFrame += _verticalVelocity * deltaTime * Vector2.up;
            }

            _characterController.Move(movementThisFrame);
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!_isLocalPlayer)
                return;

            var hitRigidBody = hit.collider.attachedRigidbody;
            if (hitRigidBody != null && !hitRigidBody.isKinematic)
            {
                hitRigidBody.AddForce(_characterController.velocity, ForceMode.Impulse);
            }
        }
    }
}
