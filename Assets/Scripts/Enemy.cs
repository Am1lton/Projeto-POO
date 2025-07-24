    using System;
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class Enemy : Entity
    {
        [SerializeField] private Collider col;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float attackRange;

        private bool inRange = false;


        private Vector3 extents;
        private Rigidbody rb;
        private bool wall = false;
        private readonly Collider[] playerCol = new Collider[1];
        private readonly Collider[] wallCheck = new Collider[1];
        
        
        protected void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            extents = col.bounds.extents;
            extents.z = 0;
        }

        #region Debug
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position + new Vector3((extents.x + 0.05f) * transform.right.x, extents.y * 0.03f, 0),
                new Vector3(0.05f, extents.y * 0.97f, 0) * 2);
        }
        
        #endregion
        
        protected virtual void FixedUpdate()
        {
            inRange = Physics.OverlapSphereNonAlloc(transform.position, attackRange, playerCol, GameManager.Instance.PlayerLayer) > 0;
            
            //WallCheck
            Vector3 wallCheckPositionOffset = new ((extents.x + 0.05f) * transform.right.x, extents.y * 0.03f, 0);
            wall = Physics.OverlapBoxNonAlloc(transform.position + wallCheckPositionOffset,
                new Vector3(0.05f, extents.y * 0.97f, 0), wallCheck, Quaternion.identity, groundLayer) > 0;

            if (!inRange) 
                Move();
        }

        private void Move()
        {
            //check if on ledge or facing wall
            if (!Physics.Raycast(transform.position - (Vector3.up * extents.y - Vector3.up * 0.05f) + transform.right * extents.x, Vector3.down, 0.2f,
                    groundLayer) || wall)
            {
                transform.right *= -1;
                return;
            }

            if (rb.linearVelocity.x * transform.right.x > 0)
            {
                if (Mathf.Abs(rb.linearVelocity.x) <= moveSpeed)
                {
                    rb.AddForce(Vector3.right * (transform.right.x * moveSpeed -  rb.linearVelocity.x), ForceMode.VelocityChange);
                }
            }
            else
            {
                rb.AddForce(transform.right * moveSpeed, ForceMode.VelocityChange);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer != GameManager.Instance.PlayerLayer) return;
            
            if (collision.gameObject.TryGetComponent(out Player player))
                player.TakeDamage(1, transform);
        }
    }