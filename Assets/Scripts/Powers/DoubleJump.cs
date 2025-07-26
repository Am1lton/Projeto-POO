using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Powers
{
    public class DoubleJump : Power
    {
        private Rigidbody rb;
        private Player plr;
        private bool canDoubleJump;
        private bool hasWallJump = false;

        private const float JUMP_FORCE = 20;
        
        public override void Activate(Player player)
        {
            canDoubleJump = true;
            plr = player;
            rb = player.GetComponent<Rigidbody>();
            plr.JumpAction.performed += DoubleJumpAction;
        }

        public override void Deactivate(Player player)
        {
            plr.JumpAction.performed -= DoubleJumpAction;
        }

        private void DoubleJumpAction(InputAction.CallbackContext context)
        {
            if (hasWallJump && (plr.IsWallLeft || plr.IsWallRight))
                return;
            if (plr.IsGrounded || !canDoubleJump)
                return;
            
            
            
            canDoubleJump = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, JUMP_FORCE, 0);
            plr.StartCoroutine(WaitUntilGrounded());
        }

        private IEnumerator WaitUntilGrounded()
        {
            while (!plr.IsGrounded)
                yield return null;
            canDoubleJump = true;
        }
    }
}