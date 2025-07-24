using Classes;
using UnityEngine;

namespace Collectables
{
    [RequireComponent(typeof(Collider))]
    public class Coin : Collectable<int, Player>
    {
        protected override void Collect(Player player)
        {
            player.gold += content;
        }
    }
}