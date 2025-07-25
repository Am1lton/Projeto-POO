using UnityEngine;

namespace Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] protected float speed = 10;
        [SerializeField] protected int damage = 1;

        protected Vector3 Velocity = Vector3.zero;
        protected Rigidbody rb;
        protected GameObject owner;


        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
        }
        
        protected virtual void FixedUpdate()
        {
            Velocity.x = speed * transform.right.x;
            
            if (rb.SweepTest(Velocity.normalized, out RaycastHit hit, Velocity.magnitude, QueryTriggerInteraction.Collide))
            {
                transform.position += Velocity.normalized * hit.distance;
                
                if (hit.collider.gameObject != owner)
                    CollideWith(hit.collider);
                return;
            }
            
            transform.Translate(Velocity * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == owner) return;
            
            CollideWith(other);
        }

        protected virtual void CollideWith(Collider other)
        {
            if (TryGetComponent(out Entity entity))
            {
                entity.TakeDamage(damage, transform);
            }
            
            Destroy(gameObject);
        }
    }
}