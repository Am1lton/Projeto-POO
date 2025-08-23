using UnityEngine;

namespace Enemies
{
    public class RangedEnemy : Enemy
    {
        [Header("Ranged")]
        [SerializeField] private float range = 5;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileCooldown = 1f;
        [SerializeField] private Transform projectileSpawn;
        
        private float projectileCooldownTimer;
        protected override void FixedUpdate()
        {
            if (Physics.Raycast(transform.position, transform.right, out RaycastHit hit, range, GameManager.Instance.PlayerMask | groundLayer))
            {
                if (hit.collider.gameObject.layer == GameManager.Instance.playerLayer)
                {
                    Shoot();
                    return;
                }
            }
            
            base.FixedUpdate();
        }


        private void Shoot()
        {
            if (projectileCooldownTimer < projectileCooldown)
            {
                projectileCooldownTimer += Time.deltaTime;
                return;
            }
            
            projectileCooldownTimer = 0;
            Instantiate(projectilePrefab, projectileSpawn.position, transform.rotation, transform);
        }
    }
}