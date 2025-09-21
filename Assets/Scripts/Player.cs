    using System;
    using System.Collections;
    using System.Collections.Generic;
	using Powers;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.SceneManagement;
    using UnityEngine.Serialization;
    using UnityEngine.UI;

    [RequireComponent(typeof(Rigidbody))]
    public class Player : Entity
    {
        //References
        [Header("Audio/Sound Effects")]
        [SerializeField] private AudioSource ledgeAudioSource;
        [SerializeField] public AudioSource SfxAudioSource;
        [SerializeField] private AudioClip jumpStartSound;
        [SerializeField] private AudioClip jumpApexSound;
        [SerializeField] private AudioClip landingSound;
        [SerializeField] private AudioClip powerCollectSound;
        [SerializeField] public AudioClip DashReadySound;

        [Header("General References")]
        [SerializeField] private GameObject projectile;
        [SerializeField] private Material material;
        [SerializeField] private RectTransform playerGUI;
        
        [Header("UI References")]
        public Image DashIcon;
        
        [Header("Physics and Movement")]
        [SerializeField] private Collider col;
        [SerializeField] private PhysicsMaterial normalMaterial;
        [SerializeField] private PhysicsMaterial slipperyMaterial;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float jumpForce = 20f;
        [SerializeField] private float maxSpeed = 7f;
        [SerializeField] private float extraGravity = 50f;
        [SerializeField] private float invincibilityTime = 1f;
        private Rigidbody rb;
        private bool isGrounded;
        public bool IsGrounded => isGrounded;
        private bool wallLeft;
        public bool IsWallLeft => wallLeft;
        private bool wallRight;
        public bool IsWallRight => wallRight;
        private bool ledgeForward;
        private bool ledgeBack;
        private const float LEDGE_DETECTION_RANGE = 2f;
        private float lastLedgeX;
        public LayerMask GroundLayer => groundLayer;
        public float MaxSpeed => maxSpeed;
        private Collider[] colliderCheck =  new Collider[1];


        private Coroutine controllerVibration;
        public GameObject Projectile => projectile;
        public Collider Col => col;
        public static int Score {get; private set;}
        
        public static void ResetScore() => Score = 0;
        
        //player input
        public InputActionAsset InputAsset;
        public InputAction MoveAction { get; private set; }
        public InputAction JumpAction { get; private set; }
        public InputAction DashAction { get; private set; }
        public InputAction ShootAction { get; private set; }
        
        //Player States system
        public enum PlayerStates
        {
            Idle = 0,
            Walking = 40,
            Jumping = 50,
            Falling = 49,
            CanWalk = 70, //anything lower than this value lets the player walk while in its state, anything higher prevents movement
            WallJumping = 80,
            Dashing = 90,
            Hit = 100
        }
        [NonSerialized] public PlayerStates PlayerState =  PlayerStates.Idle;
        
        
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
            
            SfxAudioSource.PlayOneShot(powerCollectSound);
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

        private void Start()
        {
            ledgeAudioSource.Play();
            ledgeAudioSource.loop = true;
            ledgeAudioSource.Pause();
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
            
            //LedgeDetectionLine
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position + new Vector3(
                LEDGE_DETECTION_RANGE * -transform.right.x,
                -col.bounds.extents.y + 0.1f, 0), new Vector3(0.2f, 0.2f, 0));
            Gizmos.DrawWireCube(transform.position + new Vector3(
                LEDGE_DETECTION_RANGE * transform.right.x,
                -col.bounds.extents.y + 0.1f, 0), new Vector3(0.2f, 0.2f, 0));
            
            Gizmos.DrawLine(new Vector3(lastLedgeX, 0, 0), new Vector3(lastLedgeX, 1000, 0));
        }

        private void Update()
        {
            col.material = isGrounded ? normalMaterial : slipperyMaterial;

            if (PlayerState > PlayerStates.CanWalk)
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
                if (PlayerState == PlayerStates.Jumping && rb.linearVelocity.y < 0.1f)
                {
                    PlayerState = PlayerStates.Falling;
                    SfxAudioSource.PlayOneShot(jumpApexSound, 0.5f);
                }
                
                if (rb.linearVelocity.y < -10f && PlayerState < PlayerStates.Falling)
                    PlayerState = PlayerStates.Falling;
                
                rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
                
                //Ground & Wall check
                ObstacleCollisionCheck();
                
                //checking for ledge
                {
                    if (isGrounded)
                    {
                        ledgeBack = !Physics.CheckBox(transform.position + new Vector3(
                                LEDGE_DETECTION_RANGE * -transform.right.x,
                                -col.bounds.extents.y + 0.1f, 0), new Vector3(0.1f, 0.1f, 0), Quaternion.identity,
                            groundLayer);
                        
                        ledgeForward = !Physics.CheckBox(transform.position + new Vector3(
                                LEDGE_DETECTION_RANGE * transform.right.x,
                                -col.bounds.extents.y + 0.1f, 0), new Vector3(0.1f, 0.1f, 0), Quaternion.identity,
                            groundLayer);
                    }
                    else
                    {
                        ledgeForward = false;
                        ledgeBack = false;
                    }
                    

                    if (ledgeBack != ledgeForward)
                    {
                        Vector3 dir = ledgeBack ? -transform.right : transform.right;
                        
                        if (Physics.Raycast(transform.position + new Vector3(
                                    LEDGE_DETECTION_RANGE * dir.x,
                                    -col.bounds.extents.y - 0.1f, 0), -dir, out RaycastHit hit, LEDGE_DETECTION_RANGE,
                                groundLayer))
                        {
                            lastLedgeX = hit.point.x;
                        }
                    }
                    
                    //setting volume based on distance
                    if (ledgeForward != ledgeBack)
                    {
                        float dist = Mathf.Abs(transform.position.x - lastLedgeX) / LEDGE_DETECTION_RANGE;
                        ledgeAudioSource.volume = 1 - dist;
                    }
                }
                
                //playing ledge audio
                switch (ledgeForward != ledgeBack)
                {
                    case false when ledgeAudioSource.isPlaying:
                        ledgeAudioSource.Pause();
                        break;
                    case true when !ledgeAudioSource.isPlaying:
                        ledgeAudioSource.UnPause();
                        break;
                }
                
                
                //Character movement
                float desiredSpeed = MoveAction.ReadValue<float>() * maxSpeed;
                
                if (PlayerState <= PlayerStates.CanWalk)
                    Move(desiredSpeed);
                
            }

            private void Jump(InputAction.CallbackContext context)
            {
                if (!isGrounded || PlayerState > PlayerStates.Jumping) return;
                
                PlayerState = PlayerStates.Jumping;
                col.material = slipperyMaterial;
                rb.linearVelocity = Vector3.right * rb.linearVelocity.x;
                rb.linearVelocity += Vector3.up * jumpForce;
                SfxAudioSource.PlayOneShot(jumpStartSound, 0.7f);
            }

            private void OnGrounded()
            {
                if (PlayerState == PlayerStates.Falling)
                {
                    SfxAudioSource.PlayOneShot(landingSound, 0.5f);
                    VibrateController(0.1f, 0.5f, 0.1f);
                }
                if (PlayerState <= PlayerStates.Jumping)
                    PlayerState = PlayerStates.Idle;
            }
            
            private void ObstacleCollisionCheck()
            {
                bool wasGrounded = isGrounded;
                isGrounded = Physics.OverlapBoxNonAlloc(transform.position + Vector3.down * (col.bounds.extents.y + 0.01f),
                    new Vector3(col.bounds.extents.x * 0.7f, 0.01f, 0), colliderCheck, Quaternion.identity, groundLayer) > 0;
                if (!wasGrounded && isGrounded)
                    OnGrounded();
                
                wallLeft = Physics.OverlapBoxNonAlloc(transform.position + Vector3.left * (col.bounds.extents.x + 0.01f),
                    new Vector3(0.01f, col.bounds.extents.y * 0.6f, 0), colliderCheck, Quaternion.identity, groundLayer) > 0;
                
                wallRight = Physics.OverlapBoxNonAlloc(transform.position + Vector3.right * (col.bounds.extents.x + 0.01f),
                    new Vector3(0.01f, col.bounds.extents.y * 0.6f, 0), colliderCheck, Quaternion.identity, groundLayer) > 0;
            }

            private void Move(float desiredSpeed)
            {
                if (isGrounded && PlayerState <= PlayerStates.Walking) PlayerState = PlayerStates.Walking;

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
            VibrateController(0.2f, 0.2f, 0.5f);
            
            if (damage <= 0)
                return;
            
            if (powers.Count > 0)
            {
                if (PlayerState <= PlayerStates.Hit)
                {
                    PlayerState = PlayerStates.Idle;
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

            PlayerState = PlayerStates.Hit;
            yield return new WaitForSeconds(0.1f);
            
            while (!IsGrounded)
            {
                yield return null;
            }
            
            PlayerState = PlayerStates.Idle;
        }

        private IEnumerator InvincibilityFrame()
        {
            gameObject.layer = GameManager.Instance.invincibleLayer;
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
            
            gameObject.layer = GameManager.Instance.playerLayer;
        }
        #endregion
        
        public static void AddScore(int amount)
        {
            Score += amount;
            GUIManager.Instance.UpdateScore();
        }

        private void OnDestroy()
        {
            if (controllerVibration != null)
            {
                StopAllCoroutines();
                Gamepad.current.SetMotorSpeeds(0, 0);
            }
            
        }

        public void VibrateController(float duration, float lowFrequency, float highFrequency, bool fade = false)
        {
            if (controllerVibration != null)
                StopCoroutine(controllerVibration);
            if (Gamepad.current != null) 
                controllerVibration = StartCoroutine(Utils.VibrateController(Gamepad.current, duration,
                    lowFrequency, highFrequency, fade));
        }
    }