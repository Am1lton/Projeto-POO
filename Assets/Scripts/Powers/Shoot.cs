using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Powers
{
    public class Shoot : Power
    {
        private GameObject projectile;
        private float xOffset;
        private Player plr;
        private bool canShoot = true;
        private Coroutine coroutine;
        
        private static float shootCooldown = 3;
        
        public override void Activate(Player player)
        {
            plr = player;
            player.InputAsset.FindAction("Shoot").performed += ShootProjectile;
            xOffset = player.Col.bounds.extents.x;
            projectile = player.Projectile;
        }

        public override void Deactivate(Player player)
        {
            player.InputAsset.FindAction("Shoot").performed -= ShootProjectile;
            if (coroutine != null)
                plr.StopCoroutine(coroutine);
        }

        private void ShootProjectile(InputAction.CallbackContext context)
        {
            if (!canShoot) return;
            canShoot = false;
            
            Object.Instantiate(projectile, plr.transform.position + plr.transform.right * xOffset, plr.transform.rotation, plr.transform);
            coroutine = plr.StartCoroutine(ShootCooldown());
        }

        private IEnumerator ShootCooldown()
        {
            float timer = 0f;
            while (timer < shootCooldown)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            canShoot = true;
        }
    }
}