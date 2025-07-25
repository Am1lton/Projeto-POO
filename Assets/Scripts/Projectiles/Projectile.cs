using UnityEngine;

namespace Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] protected float speed = 10;
        [SerializeField] protected int damage = 1;
        [SerializeField] protected float lifeTime = 3f;

        protected Rigidbody Rb;
        protected int OwnerLayer;

        protected RaycastHit[] Hits;

        protected virtual void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            Rb.isKinematic = true;

            if (transform.parent != null)
            {
                OwnerLayer = transform.parent.gameObject.layer;
                transform.parent = null;
            }
            
            Destroy(gameObject, lifeTime);
        }
        
        protected virtual void FixedUpdate()
        {
            Hits = Rb.SweepTestAll(transform.right, speed * Time.deltaTime,
                QueryTriggerInteraction.Collide);
            
            if (Hits.Length > 0)
            {
                foreach (RaycastHit hit in Hits)
                {
                    if (hit.collider.gameObject.layer != OwnerLayer)
                        CollideWith(hit.collider);
                    else
                    {
                        transform.position += transform.right * hit.distance;
                        break;
                    }
                }
                
                
            }
            
            transform.position += transform.right * (speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == OwnerLayer) return;
            
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