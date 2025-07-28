    using System.Collections;
    using System.Collections.Generic;
	using Powers;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.SceneManagement;

    [RequireComponent(typeof(Rigidbody))]
    public class Player : Entity
    {
        //References
        [SerializeField] private LayerMask groundLayer;
        public LayerMask GroundLayer => groundLayer;
        [SerializeField] private Collider col;
        [SerializeField] private GameObject projectile;
        public GameObject Projectile => projectile;
        public Collider Col => col;
        [SerializeField] private Material material;
        [SerializeField] private RectTransform playerGUI;
        
        public static int Score {get; private set;}
        
        public static void ResetScore() => Score = 0;
        
        //player input
        public InputActionAsset InputAsset;
        public InputAction MoveAction { get; private set; }
        public InputAction JumpAction { get; private set; }
        public InputAction DashAction { get; private set; }
        public InputAction ShootAction { get; private set; }
        
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
            CanWalk = 7, //anything lower than this value lets the player walk while in its state, anything higher prevents movement
            WallJumping = 8,
            Dashing = 9,
            Hit = 10
        }
        public PlayerStates playerState =  PlayerStates.Idle;
        
        
        //PowerUps
        private Stack<Power> powers = new Stack<Power>();
        private Dictionary<PowerTypes, PowerCell> powerCellsGUI = new(); 

        #region PowerUps
        public void AddPower(Power power)
        {
            foreach (Power playerPower in powers)
            {
                if (power.GetType() == playerPower.GetType())
                    return;
            }
            
            powers.Push(power);
            
            if (powerCellsGUI.TryGetValue(Power.GetPowerType(power), out PowerCell cell))
                cell.gameObject.SetActive(true);
            
            power.Activate(this);
        }

        private void RemoveLatestPower()
        {
            Power pwr = powers.Pop();
            
            if (powerCellsGUI.TryGetValue(Power.GetPowerType(pwr), out PowerCell cell))
                cell.gameObject.SetActive(false);
            
            pwr.Deactivate(this);
            
        }
        
        public bool CheckForPower<TPowerType>(out TPowerType power) where TPowerType : Power
        {
            power = null;
            foreach (Power pwr in powers)
            {
                if (pwr.GetType() == typeof(TPowerType))
                {
                    power = pwr as  TPowerType;
                    return true;
                }
            }

            return false;
        }
        
        public bool CheckForPower<TPowerType>() where TPowerType : Power
        {
            foreach (Power pwr in powers)
            {
                if (pwr.GetType() == typeof(TPowerType))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
        
        
        private void Awake()
        {
            MoveAction = InputAsset.FindAction("Move");
            JumpAction = InputAsset.FindAction("Jump");
            DashAction = InputAsset.FindAction("Dash");
            ShootAction = InputAsset.FindAction("Shoot");
            
            if (!col)
                col = GetComponent<Collider>();
            
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;

            foreach (PowerCell powerCell in playerGUI.GetComponentsInChildren<PowerCell>(true))
            {
                powerCellsGUI.TryAdd(powerCell.PowerType, powerCell);
                
                powerCell.gameObject.SetActive(false);
            }
        }

        private void OnEnable() => JumpAction.performed += Jump;
        private void OnDisable() => JumpAction.performed -= Jump;
        
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
            col.material = isGrounded ? normalMaterial : slipperyMaterial;

            if (playerState > PlayerStates.CanWalk)
            {
                transform.right = rb.linearVelocity.x switch
                {
                    > 0 => Vector3.right,
                    < 0 => Vector3.left,
                    _ => transform.right
                };
            }
            else
            {
                transform.right = MoveAction.ReadValue<float>() switch
                {
                    > 0 => Vector3.right,
                    < 0 => Vector3.left,
                    _ => transform.right
                };
            }
            

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
                
            }

            private void Jump(InputAction.CallbackContext context)
            {
                if (!isGrounded || playerState > PlayerStates.Jumping) return;
                
                playerState = PlayerStates.Jumping;
                col.material = slipperyMaterial;
                rb.linearVelocity = Vector3.right * rb.linearVelocity.x;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
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

        
        #region TakingDamage and Dying
        public override void TakeDamage(int damage, Transform attacker)
        {
            if (damage <= 0)
                return;
            
            if (powers.Count > 0)
            {
                if (playerState <= PlayerStates.Hit)
                {
                    playerState = PlayerStates.Idle;
                    StartCoroutine(DamageJump(attacker));
                    StartCoroutine(InvincibilityFrame());
                }
                
                
                RemoveLatestPower();
            }
            else
                Die();
        }

        protected override void Die()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
        #endregion
        
        public static void AddScore(int amount)
        {
            Score += amount;
            GUIManager.Instance.UpdateScore();
        }
    }