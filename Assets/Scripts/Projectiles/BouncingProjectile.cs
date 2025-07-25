using UnityEngine;

namespace Projectiles
{
    public class BouncingProjectile : Projectile
    {
        [SerializeField] private float gravity;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float bounceForce;


        private Vector2 velocity =  Vector2.zero;
        
        protected override void Awake()
        {
            base.Awake();
            GetComponent<Collider>().excludeLayers = groundLayer;
        }
        
        protected override void FixedUpdate()
        {
            velocity.y -= gravity * Time.deltaTime;
            velocity.x = speed * transform.right.x;
            
            //Colliding with walls and the ground
            Hits = Rb.SweepTestAll(velocity.normalized, velocity.magnitude * Time.deltaTime,
                QueryTriggerInteraction.Collide);
            
            if (Hits.Length > 0)
            {
                foreach (RaycastHit hit in Hits)
                {
                    //if it's ground, bounce
                    if (1 << hit.collider.gameObject.layer == groundLayer)
                    {
                        transform.position += (Vector3)velocity.normalized * hit.distance;
                    
                        Bounce(transform.position, hit.point);
                        return;
                    }
                    
                    if (hit.collider.gameObject.layer != OwnerLayer)
                        CollideWith(hit.collider);
                }
                
            }
            
            transform.position += (Vector3)velocity * Time.deltaTime;
        }

        protected override void CollideWith(Collider other)
        {
            if (other.TryGetComponent(out Entity entity))
            {
                entity.TakeDamage(damage, transform);
            }
            
            Destroy(gameObject);
        }

        private void Bounce(Vector3 position, Vector3 pointOfImpact)
        {
            Vector3 dir = pointOfImpact - position;
            switch (Vector3.Angle(dir, Vector3.down))
            {
                case < 80:
                    velocity.y = bounceForce;
                    break;
                case < 170:
                    if (transform.right.x * dir.x > 0)
                        transform.right *= -1;
                    break;
                default:
                    velocity.y = -bounceForce;
                    break;
            }
        }
    }
}