using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Powers
{
    public class Dash : Power
    {
        private const float DashSpeed = 80f;
        private const float DashCooldown = 3f;
        private const float DashLenght = 6f;
        
        private Rigidbody rb;
        private Player plr;

        private bool canDash = true;

        private Coroutine coroutine;
        
        
        public override void Activate(Player player)
        {
            rb = player.GetComponent<Rigidbody>();
            plr = player;
            plr.DashAction.performed += DashAbility;
        }

        public override void Deactivate(Player player)
        {
            plr.DashAction.performed -= DashAbility;
            plr.StopCoroutine(coroutine);
        }

        private void DashAbility(InputAction.CallbackContext context)
        {
            if (!canDash || plr.playerState > Player.PlayerStates.Dashing)
                return;
            
            canDash = false;
            plr.playerState = Player.PlayerStates.Dashing;

            coroutine = plr.StartCoroutine(Dashing());
            
        }

        private IEnumerator DashCooldownRoutine()
        {
            float timer = 0f;
            
            while (timer < DashCooldown)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            canDash = true;
        }

        private IEnumerator Dashing()
        {
            float amountTravelled = 0f;
            float desiredDir = plr.transform.right.x > 0 ? 1 : -1;

            while (amountTravelled < DashLenght && plr.playerState == Player.PlayerStates.Dashing)
            {
                if (plr.IsWallRight  && desiredDir > 0)
                    break;
                if (plr.IsWallLeft  && desiredDir < 0)
                    break;
                
                float currentSpeed = DashSpeed * Time.deltaTime;
                

                if (rb.SweepTest(Vector3.right * desiredDir, out RaycastHit hit, currentSpeed))
                {
                    plr.transform.position += Vector3.right * (hit.distance * desiredDir);
                    break;
                }
                
                plr.transform.position += Vector3.right * (desiredDir * currentSpeed);
                
                amountTravelled += currentSpeed;
                yield return null;
            }
            
            if (plr.playerState == Player.PlayerStates.Dashing)
                plr.playerState = Player.PlayerStates.Idle;

            rb.linearVelocity = Vector3.zero;
            
            yield return plr.StartCoroutine(DashCooldownRoutine());
        }
    }
}