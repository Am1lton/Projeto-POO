using System.Collections;
using Classes;
using Powers;
using UnityEngine;

namespace Collectables
{
    public class PowerUp : Collectable<PowerTypes, Player>
    {
        protected override void Collect(Player player)
        {
            player.AddPower(Power.GetPower(content));
        }

        protected override void OnCollected()
        {
            if (TryGetComponent( out MeshRenderer meshRenderer))
                meshRenderer.enabled = false;
            if (TryGetComponent( out Collider col))
                col.enabled = false;
            
            StartCoroutine(Cooldown());
        }

        private IEnumerator Cooldown()
        {
            yield return new WaitForSeconds(5);

            if (TryGetComponent( out MeshRenderer meshRenderer))
                meshRenderer.enabled = true;
            if (TryGetComponent( out Collider col))
                col.enabled = true;
        }
    }
}