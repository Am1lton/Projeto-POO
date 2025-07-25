using UnityEngine;

namespace Projectiles
{
    public class BouncingProjectile : Projectile
    {
        [SerializeField] private float gravity;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float bounceForce;

        protected override void Awake()
        {
            base.Awake();
            GetComponent<Collider>().excludeLayers = groundLayer;
        }
        
        protected override void FixedUpdate()
        {
            Velocity.y -= gravity * Time.deltaTime;
            Velocity.x = speed * transform.right.x;

            Vector3 desiredPosition = transform.position;
            
            //Colliding with walls and the ground
            if (rb.SweepTest(Velocity.normalized, out RaycastHit hit, Velocity.magnitude * Time.deltaTime, QueryTriggerInteraction.Collide))
            {
                //if it's ground, bounce
                if (1 << hit.collider.gameObject.layer == groundLayer)
                {
                    desiredPosition = transform.position + Velocity.normalized * hit.distance;
                    
                    Bounce(desiredPosition, hit.point);
                }
                else
                {
                    if (hit.collider.gameObject != owner)
                        CollideWith(hit.collider);
                }
            }
            else
            {
                desiredPosition = transform.position + Velocity * Time.deltaTime;
            }
            
            transform.position = desiredPosition;
        }

        protected override void CollideWith(Collider other)
        {
            if (other.gameObject == owner) return;
            
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
                    Velocity.y = bounceForce;
                    Debug.Log("Going Up: " + Vector3.Angle(dir, Vector3.down));
                    break;
                case < 170:
                    Debug.Log("Turning: " + Vector3.Angle(dir, Vector3.down));
                    transform.right *= -1;
                    break;
                default:
                    Velocity.y = -bounceForce;
                    break;
            }
        }
    }
}