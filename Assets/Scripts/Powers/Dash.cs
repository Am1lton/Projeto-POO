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
        private InputAction playerDashAction;

        private bool canDash = true;

        private Coroutine coroutine;
        
        
        public override void Activate(Player player)
        {
            rb = player.GetComponent<Rigidbody>();
            plr = player;
            playerDashAction = player.InputAsset.FindAction("Dash");
            playerDashAction.performed += DashAbility;
        }

        public override void Deactivate(Player player)
        {
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
            float posX = 0f;
            Vector3 startPos = plr.transform.position;
            
            float desiredDir = plr.transform.right.x > 0 ? 1 : -1;
            
            Vector3 endPos = plr.transform.position + Vector3.right * (desiredDir * DashLenght);

            while (posX < DashLenght && plr.playerState == Player.PlayerStates.Dashing)
            {
                if (plr.IsWallRight  && desiredDir > 0)
                    break;
                if (plr.IsWallLeft  && desiredDir < 0)
                    break;
                
                float currentSpeed = DashSpeed * Time.deltaTime;
                Vector3 posLerp = Vector3.Lerp(startPos, endPos, posX / DashLenght);
                
                RaycastHit[] hit = new RaycastHit[1];

                if (Physics.BoxCastNonAlloc(plr.transform.position,
                        new Vector3(plr.Col.bounds.extents.x, plr.Col.bounds.extents.y * 0.9f, 1f), Vector3.right * desiredDir,
                        hit, Quaternion.identity, currentSpeed) > 0)
                {
                    plr.transform.position += Vector3.right * (hit[0].distance * desiredDir);
                    break;
                }
                
                plr.transform.position = posLerp;
                
                posX  = posX + currentSpeed > DashLenght ? DashLenght : posX + currentSpeed;
                yield return null;
            }
            
            if (plr.playerState == Player.PlayerStates.Dashing)
                plr.playerState = Player.PlayerStates.Idle;

            rb.linearVelocity = Vector3.zero;
            
            yield return plr.StartCoroutine(DashCooldownRoutine());
        }
    }
}