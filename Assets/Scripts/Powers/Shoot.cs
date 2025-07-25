using UnityEngine.InputSystem;

namespace Powers
{
    public class Shoot : Power
    {
        private InputAction playerShootAction;
        
        public override void Activate(Player player)
        {
            playerShootAction = player.InputAsset.FindAction("Shoot");
        }

        public override void Deactivate(Player player)
        {
            throw new System.NotImplementedException();
        }
    }
}