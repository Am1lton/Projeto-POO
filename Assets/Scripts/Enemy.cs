using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
    public class Enemy : Entity
    {
        [SerializeField] private Collider nonTriggerCollider;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float moveSpeed;

        private Vector3 extents;
        private Rigidbody rb;
        private bool wall = false;
        private readonly Collider[] wallCheck = new Collider[1];
        
        
        protected void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            extents = nonTriggerCollider.bounds.extents;
            extents.z = 0;
            nonTriggerCollider.excludeLayers = ~groundLayer;
        }

        #region Debug
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position + new Vector3((extents.x + 0.05f) * transform.right.x, extents.y * 0.03f, 0),
                new Vector3(0.05f, extents.y * 0.97f, 0) * 2);
            
            Gizmos.DrawWireSphere(transform.position, nonTriggerCollider.bounds.extents.x * 0.5f);
        }
        
        #endregion
        
        protected virtual void FixedUpdate()
        {
            Move();
        }

        private void Move()
        {
            //WallCheck
            Vector3 wallCheckPositionOffset = new ((extents.x + 0.05f) * transform.right.x, extents.y * 0.03f, 0);
            wall = Physics.OverlapBoxNonAlloc(transform.position + wallCheckPositionOffset,
                new Vector3(0.05f, extents.y * 0.97f, 0), wallCheck, Quaternion.identity, groundLayer) > 0;
            
            
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

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != GameManager.Instance.PlayerLayer) return;

            if (CheckIfHitFromAbove(other))
            {
                TakeDamage(1, other.transform);
                return;
            }
            
            if (other.gameObject.TryGetComponent(out Player player))
                player.TakeDamage(1, transform);
        }

        private bool CheckIfHitFromAbove(Collider other)
        {
            Vector3 dir = Vector3.Normalize(other.transform.position - transform.position);

            //Checks if the player's origin is above the enemy
            if (Vector3.Dot(transform.up, dir) < 0.85f)
                return false;

            //Checks if player is not inside the enemy
            return !Physics.CheckSphere(transform.position, nonTriggerCollider.bounds.extents.x * 0.5f, 1 << GameManager.Instance.PlayerLayer);
        }
    }