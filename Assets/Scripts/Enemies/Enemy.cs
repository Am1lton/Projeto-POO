using System.Collections;
using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
    public class Enemy : Entity
    {
        [Header("Physics and Movement")]
        [SerializeField] private Collider nonTriggerCollider;
        [SerializeField] protected LayerMask groundLayer;
        [SerializeField] private float moveSpeed;
        
        [Header("Score")]
        [SerializeField] private int scoreGivenOnDeath;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip deathSound;
        
        private const float MIN_AUDIO_DISTANCE = 25;
        private const float AUDIO_PAN_SWITCH_DISTANCE = 10f;

        private Vector3 extents;
        private Rigidbody rb;
        private bool wall;
        private readonly Collider[] wallCheck = new Collider[1];
        
        
        protected void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            extents = nonTriggerCollider.bounds.extents;
            extents.z = 0;
            nonTriggerCollider.excludeLayers = ~groundLayer;

            audioSource.loop = true;
            if (!audioSource.playOnAwake) audioSource.Play();
            audioSource.Pause();
        }

        #region Debug
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position + new Vector3((extents.x + 0.05f) * transform.right.x, extents.y * 0.03f, 0),
                new Vector3(0.05f, extents.y * 0.97f, 0) * 2);
            
            Gizmos.DrawWireSphere(transform.position, nonTriggerCollider.bounds.extents.x * 0.5f);
        }
        
        #endregion
        
        protected virtual void FixedUpdate()
        {
            if (Vector3.Distance(GameManager.Instance.CenterOfScreen.position, transform.position) < MIN_AUDIO_DISTANCE)
            {
                audioSource.UnPause();
                
                float panStereo = transform.position.x - GameManager.Instance.CenterOfScreen.position.x;
                if (Mathf.Abs(panStereo) > AUDIO_PAN_SWITCH_DISTANCE)
                    audioSource.panStereo = panStereo > 0 ? 1 : -1;
                else
                {
                    audioSource.panStereo = panStereo != 0 ? panStereo / AUDIO_PAN_SWITCH_DISTANCE : 0;    
                }
                
                audioSource.volume = 
                    (MIN_AUDIO_DISTANCE - Vector3.Distance(GameManager.Instance.CenterOfScreen.position, transform.position)) / (MIN_AUDIO_DISTANCE * 0.9f);
            }
            else
                audioSource.Pause();
            
            Move();
        }

        private void Move()
        {
            //WallCheck
            Vector3 wallCheckPositionOffset = new ((extents.x + 0.05f) * transform.right.x, extents.y * 0.03f, 0);
            wall = Physics.OverlapBoxNonAlloc(transform.position + wallCheckPositionOffset,
                new Vector3(0.05f, extents.y * 0.97f, 0), wallCheck, Quaternion.identity, groundLayer) > 0;
            
            
            //check if on ledge or facing wall
            if (!Physics.Raycast(transform.position - (Vector3.up * extents.y - Vector3.up * 0.05f) + transform.right * extents.x, Vector3.down, 0.2f,
                    groundLayer) || wall)
            {
                transform.right *= -1;
                return;
            }

            if (rb.linearVelocity.x * transform.right.x > 0)
            {
                if (Mathf.Abs(rb.linearVelocity.x) <= moveSpeed)
                {
                    rb.AddForce(Vector3.right * (transform.right.x * moveSpeed -  rb.linearVelocity.x), ForceMode.VelocityChange);
                }
            }
            else
            {
                rb.AddForce(transform.right * moveSpeed, ForceMode.VelocityChange);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != GameManager.Instance.playerLayer) return;

            if (CheckIfHitFromAbove(other))
            {
                TakeDamage(1, other.transform);
                if (other.TryGetComponent(out Rigidbody rigidBody))
                    rigidBody.linearVelocity = new Vector3(rigidBody.linearVelocity.x, 20, 0);
                return;
            }
            
            if (other.gameObject.TryGetComponent(out Player player))
                player.TakeDamage(1, transform);
        }

        private bool CheckIfHitFromAbove(Collider other)
        {
            Vector3 dir = Vector3.Normalize(other.transform.position - transform.position);

            //Checks if the player's origin is above the enemy
            if (Vector3.Dot(transform.up, dir) < 0.7f)
            {
                return false;
            }

            //Checks if player is not inside the enemy, not working properly because it's a trigger
            return true;//!Physics.CheckSphere(transform.position, nonTriggerCollider.bounds.extents.x * 0.3f, 1 << GameManager.Instance.PlayerLayer);
        }

        protected override void Die()
        {
            Player.AddScore(scoreGivenOnDeath);
            audioSource.clip = deathSound;
            audioSource.loop = false;
            audioSource.Play();
            audioSource.time = 0;
            GetComponent<Collider>().enabled = false;
            GetComponent<MeshRenderer>().enabled = false;
            StartCoroutine(WaitForAudio());
        }

        private IEnumerator WaitForAudio()
        {
            while (audioSource.isPlaying)
                yield return null;
            Destroy(audioSource.gameObject);
        }
    }
}