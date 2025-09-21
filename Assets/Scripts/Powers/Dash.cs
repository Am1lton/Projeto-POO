using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Powers
{
    public class Dash : Power
    {
        private const float DASH_SPEED = 80f;
        private const float DASH_COOLDOWN = 3f;
        private const float DASH_LENGHT = 6f;

        private float _currentDashCooldown;

        private float CurrentDashCooldown
        {
            get => _currentDashCooldown;
            set
            {
                _currentDashCooldown = value;
                if (_currentDashCooldown > 0 && !cooldownActive)
                    coroutine = plr.StartCoroutine(DashCooldownRoutine());
            }
        }
        
        private Rigidbody rb;
        private Player plr;

        private bool cooldownActive = false;
        
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
            if (CurrentDashCooldown > 0 || plr.PlayerState > Player.PlayerStates.Dashing)
                return;
            
            CurrentDashCooldown = DASH_COOLDOWN;
            plr.PlayerState = Player.PlayerStates.Dashing;

            coroutine = plr.StartCoroutine(Dashing());
            
        }

        private IEnumerator DashCooldownRoutine()
        {
            cooldownActive = true;
            plr.DashIcon.gameObject.SetActive(true);
            
            while (CurrentDashCooldown > 0)
            {
                CurrentDashCooldown -= Time.deltaTime;
                plr.DashIcon.fillAmount = CurrentDashCooldown > 0.01f ? 1 - CurrentDashCooldown / DASH_COOLDOWN : 0;
                yield return null;
            }

            plr.VibrateController(0.05f, 0.4f, 0.1f, true);
            plr.DashIcon.gameObject.SetActive(false);
            cooldownActive = false;
            plr.SfxAudioSource.PlayOneShot(plr.DashReadySound);
        }

        private IEnumerator Dashing()
        {
            float amountTravelled = 0f;
            float desiredDir = plr.transform.right.x > 0 ? 1 : -1;

            while (amountTravelled < DASH_LENGHT && plr.PlayerState == Player.PlayerStates.Dashing)
            {
                if (plr.IsWallRight  && desiredDir > 0)
                    break;
                if (plr.IsWallLeft  && desiredDir < 0)
                    break;
                
                float currentSpeed = DASH_SPEED * Time.deltaTime;
                

                if (rb.SweepTest(Vector3.right * desiredDir, out RaycastHit hit, currentSpeed))
                {
                    plr.transform.position += Vector3.right * (hit.distance * desiredDir);
                    break;
                }
                
                plr.transform.position += Vector3.right * (desiredDir * currentSpeed);
                
                amountTravelled += currentSpeed;
                yield return null;
            }
            
            if (plr.PlayerState == Player.PlayerStates.Dashing)
                plr.PlayerState = Player.PlayerStates.Idle;

            rb.linearVelocity = Vector3.zero;
        }
    }
}