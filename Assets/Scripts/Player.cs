	using System.Collections;
    using System.Collections.Generic;
	using Powers;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.Serialization;

    [RequireComponent(typeof(Rigidbody))]
    public class Player : Entity
    {
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Collider col;
        [SerializeField] private GameObject projectile;
        public GameObject Projectile => projectile;
        public Collider Col => col;
        [SerializeField] private Material material;
        
        [FormerlySerializedAs("Gold")] public int gold;
        
        //player input
        // ReSharper disable once InconsistentNaming
        public InputActionAsset InputAsset;
        public InputAction MoveAction { get; private set; }
        private InputAction jumpAction;
        
        //Physics and movement
        private Rigidbody rb;
        private bool isGrounded;
        public bool IsGrounded => isGrounded;
        private bool wallLeft;
        public bool IsWallLeft => wallLeft;
        private bool wallRight;
        public bool IsWallRight => wallRight;
        [SerializeField] private PhysicsMaterial normalMaterial;
        [SerializeField] private PhysicsMaterial slipperyMaterial;
        [SerializeField] private float jumpForce = 20f;
        [SerializeField] private float maxSpeed = 7f;
        [SerializeField] private float extraGravity = 50f;
        [SerializeField] private float invincibilityTime = 1f;
        public float MaxSpeed => maxSpeed;
        private Collider[] colliderCheck =  new Collider[1];
        
        //Player States system
        public enum PlayerStates
        {
            Idle = 0,
            Jumping = 5,
            Walking = 5, 
            Dashing = 8,
            CanWalk = 7, //anything lower than this value lets the player walk while in its state, anything higher prevents movement
            Hit = 10
        }
        public PlayerStates playerState =  PlayerStates.Idle;
        
        
        //PowerUps
        private Stack<Power> powers = new Stack<Power>();

        #region PowerUps
        public void AddPower(Power power)
        {
            foreach (Power playerPower in powers)
            {
                if (power.GetType() == playerPower.GetType())
                    return;
            }
            powers.Push(power);
            power.Activate(this);
        }

        private void RemoveLatestPower()
        {
            powers.Pop().Deactivate(this);
        }
        #endregion
        
        
        private void Awake()
        {
            MoveAction = InputAsset.FindAction("Move");
            jumpAction = InputAsset.FindAction("Jump");
            
            if (!col)
                col = GetComponent<Collider>();
            
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
        }

        //Debug
        private void OnDrawGizmosSelected()
        {
            //drawing ground check box for debugging
            
            //Ground Gizmo
            Gizmos.DrawWireCube(transform.position + Vector3.down * (col.bounds.extents.y + 0.01f),
                new Vector3(col.bounds.size.x * 0.7f, 0.02f, 0));
            
            //Wall Left Gizmo
            Gizmos.DrawWireCube(transform.position + Vector3.left * (col.bounds.extents.x + 0.01f),
                new Vector3(0.02f, col.bounds.size.y * 0.6f, 0));
            
            //Wall Right Gizmo
            Gizmos.DrawWireCube(transform.position + Vector3.right * (col.bounds.extents.x + 0.01f),
                new Vector3(0.02f, col.bounds.size.y * 0.6f, 0));
        }

        private void Update()
        {
            transform.right = MoveAction.ReadValue<float>() switch
            {
                > 0 => Vector3.right,
                < 0 => Vector3.left,
                _ => transform.right
            };

            col.material = isGrounded ? normalMaterial : slipperyMaterial;
        }
        
        #region Movement and Collision
            private void FixedUpdate()
            {
                rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
                
                //Ground & Wall check
                ObstacleCollisionCheck();
                
                //Character movement
                float desiredSpeed = MoveAction.ReadValue<float>() * maxSpeed;
                
                if (playerState <= PlayerStates.CanWalk)
                    Move(desiredSpeed);
                
                if (isGrounded && jumpAction.IsPressed() && playerState <= PlayerStates.Jumping)
                {
                    playerState = PlayerStates.Jumping;
                    col.material = slipperyMaterial;
                    rb.linearVelocity = Vector3.right * rb.linearVelocity.x;
                    rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
                }
                
            }
            
            
            private void ObstacleCollisionCheck()
            {
                isGrounded = Physics.OverlapBoxNonAlloc(transform.position + Vector3.down * (col.bounds.extents.y + 0.01f),
                    new Vector3(col.bounds.extents.x * 0.7f, 0.01f, 0), colliderCheck, Quaternion.identity, groundLayer) > 0;
                
                wallLeft = Physics.OverlapBoxNonAlloc(transform.position + Vector3.left * (col.bounds.extents.x + 0.01f),
                    new Vector3(0.01f, col.bounds.extents.y * 0.6f, 0), colliderCheck, Quaternion.identity, groundLayer) > 0;
                
                wallRight = Physics.OverlapBoxNonAlloc(transform.position + Vector3.right * (col.bounds.extents.x + 0.01f),
                    new Vector3(0.01f, col.bounds.extents.y * 0.6f, 0), colliderCheck, Quaternion.identity, groundLayer) > 0;
            }

            private void Move(float desiredSpeed)
            {
                if (isGrounded && playerState <= PlayerStates.Walking) playerState = PlayerStates.Walking;

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
        #endregion

        public override void TakeDamage(int damage, Transform attacker)
        {
            if (damage <= 0)
                return;
            
            //while whe don't have a lot of powers
            StartCoroutine(DamageJump(attacker));
            StartCoroutine(InvincibilityFrame());
            
            /*
            if (powers.Count > 0)
            {
                if (playerState <= PlayerStates.Hit)
                {
                    playerState = PlayerStates.Idle;
                }
                
                
                RemoveLatestPower();
            }
            else
                Die();
            */
        }

        private IEnumerator DamageJump(Transform attacker)
        {
            
            Vector3 dir = new Vector3(attacker.position.x > transform.position.x ? -0.5f : 0.5f, 1, 0);
            dir *= jumpForce;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(dir, ForceMode.Impulse);

            playerState = PlayerStates.Hit;
            yield return new WaitForSeconds(0.1f);
            
            while (!IsGrounded)
            {
                yield return null;
            }

            playerState = PlayerStates.Idle;
        }

        private IEnumerator InvincibilityFrame()
        {
            gameObject.layer = GameManager.Instance.InvincibleLayer;
            float timer = 0f;
            Color startColor = material.color;
            
            while (timer < invincibilityTime)
            {
                float t = Mathf.Sin(timer * Mathf.PI * 10f) * 0.5f + 0.5f;
                
                material.color = Color.Lerp(startColor, Color.red, t);
                
                timer += Time.deltaTime;
                yield return null;
            }

            material.color = startColor;
            
            gameObject.layer = GameManager.Instance.PlayerLayer;
        }
    }