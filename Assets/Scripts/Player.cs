    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.Serialization;

    [RequireComponent(typeof(Rigidbody))]
    public class Player : Entity
    {
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Collider col;
        
        //player input
        private InputAction moveAction;
        private InputAction jumpAction;
        
        private Rigidbody rb;
        private bool isGrounded;
        private bool wallLeft;
        private bool wallRight;
        [SerializeField] private PhysicsMaterial normalMaterial;
        [SerializeField] private PhysicsMaterial slipperyMaterial;
        [SerializeField] private float jumpForce = 20f;
        [SerializeField] private float maxSpeed = 7f;
        [SerializeField] private float extraGravity = 50f;
        
        private Collider[] colliderCheck =  new Collider[1];

        
        //the value of the state determines its priority, low priority states will be overwritten
        public enum PlayerStates
        {
            Idle = 0,
            Walking = 5,
            Jumping = 10,
            Hit = 20
        }

        public PlayerStates playerState =  PlayerStates.Idle;
        
        private void Awake()
        {
            InputActionMap actionMap  = InputSystem.actions.FindActionMap("Player");
            moveAction = actionMap.FindAction("Move");
            jumpAction = actionMap.FindAction("Jump");
            
            if (!col)
                col = GetComponent<Collider>();
            
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
        }

        private void OnDrawGizmosSelected()
        {
            //drawing ground check box for debugging
            
            //Ground Gizmo
            Gizmos.DrawWireCube(transform.position + Vector3.down * (col.bounds.extents.y + 0.01f),
                new Vector3(col.bounds.size.x * 0.8f, 0.02f, 0));
            
            //Wall Left Gizmo
            Gizmos.DrawWireCube(transform.position + Vector3.left * (col.bounds.extents.x + 0.01f),
                new Vector3(0.02f, col.bounds.size.y * 0.6f, 0));
            
            //Wall Right Gizmo
            Gizmos.DrawWireCube(transform.position + Vector3.right * (col.bounds.extents.x + 0.01f),
                new Vector3(0.02f, col.bounds.size.y * 0.6f, 0));
        }

        private void Update()
        {
            col.material = isGrounded ? normalMaterial : slipperyMaterial;
        }
        
        private void FixedUpdate()
        {
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
            
            //Ground/Wall check and jumping
            ObstacleCollisionCheck();
            
            if (isGrounded && jumpAction.IsPressed())
            {
                col.material = slipperyMaterial;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
            
            //Character movement
            Move();
        }
        
        
        private void ObstacleCollisionCheck()
        {
            isGrounded = Physics.OverlapBoxNonAlloc(transform.position + Vector3.down * (col.bounds.extents.y + 0.01f),
                new Vector3(col.bounds.extents.x * 0.8f, 0.01f, 0), colliderCheck, Quaternion.identity, groundLayer) > 0;
            
            wallLeft = Physics.OverlapBoxNonAlloc(transform.position + Vector3.left * (col.bounds.extents.x + 0.01f),
                new Vector3(0.01f, col.bounds.extents.y * 0.6f, 0), colliderCheck, Quaternion.identity, groundLayer) > 0;
            
            wallRight = Physics.OverlapBoxNonAlloc(transform.position + Vector3.right * (col.bounds.extents.x + 0.01f),
                new Vector3(0.01f, col.bounds.extents.y * 0.6f, 0), colliderCheck, Quaternion.identity, groundLayer) > 0;
        }
        
        private void Move()
        {
            if (playerState > PlayerStates.Jumping)
                return;
            
            float desiredSpeed = moveAction.ReadValue<float>() * maxSpeed;

            switch (desiredSpeed)
            {
                case > 0:
                    if (wallRight) desiredSpeed = 0;
                    break;
                case < 0:
                    if (wallLeft) desiredSpeed = 0;
                    break;
            }
            
            //Checks if player is moving in same direction than its input, so that they don't gain speed while above moveSpeed
            if (desiredSpeed * rb.linearVelocity.x >= 0)
            {
                if (Mathf.Abs(rb.linearVelocity.x) <= maxSpeed)
                {
                    rb.AddForce(Vector3.right * (desiredSpeed -  rb.linearVelocity.x), ForceMode.VelocityChange);
                }
            }
            else
                rb.AddForce(Vector3.right * desiredSpeed, ForceMode.VelocityChange);
        }
    }