using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Powers
{
    public class WallJump : Power
    {
        private Rigidbody rb;
        private Player plr;

        private const float WALL_JUMP_FORCE = 20f;
        private const float WALL_DISTANCE_CHECK = 2.5f;
        
        public override void Activate(Player player)
        {
            plr = player;
            rb = player.GetComponent<Rigidbody>();
            plr.JumpAction.performed += WallJumpAbility;
            
            if (plr.CheckForPower(out DoubleJump power))
            {
                power.HasWallJump = true;
            }
        }

        public override void Deactivate(Player player)
        {
            plr.JumpAction.performed -= WallJumpAbility;
            
            if (plr.CheckForPower(out DoubleJump power))
            {
                power.HasWallJump = false;
            }
        }

        private void WallJumpAbility(InputAction.CallbackContext context)
        {
            if (plr.IsGrounded || (!plr.IsWallLeft && !plr.IsWallRight) ||
                plr.playerState > Player.PlayerStates.WallJumping) 
                return;
            
            rb.linearVelocity = plr.IsWallLeft ? new Vector3(1, 0.7f, 0) * WALL_JUMP_FORCE :
                new Vector3(-1, 0.7f, 0) * WALL_JUMP_FORCE;
            
            plr.playerState = Player.PlayerStates.WallJumping;
            plr.StartCoroutine(WallDistanceCheck(plr.IsWallLeft));

        }

        
        private IEnumerator WallDistanceCheck(bool goingRight)
        {
            Vector3 dir = goingRight ? Vector3.left : Vector3.right;
            float time = 0f;
            
            while (!plr.IsGrounded && Physics.Raycast(plr.transform.position, dir, WALL_DISTANCE_CHECK, plr.GroundLayer))
            {
                if (goingRight && plr.IsWallRight) break;
                if (!goingRight && plr.IsWallLeft) break;
                
                if (time > 2f) break; //just in case
                time += Time.deltaTime;
                yield return null;
            }
            
            plr.playerState = Player.PlayerStates.Idle;
        }
    }
}