using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Platform : MonoBehaviour
{
    [SerializeField] private BoxCollider playerCheckArea;
    [SerializeField] private float ledgeSoundThreshold = 0.15f;
    
    private AudioSource audioSource;
    private bool isPlayerOnTop = false;
    private Transform player;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0;
        audioSource.Stop();
    }
    
    private void FixedUpdate()
    {
        if (!isPlayerOnTop) return;
        
        
        float distance = Mathf.Abs(player.position.x - transform.position.x) / playerCheckArea.bounds.extents.x;
        Debug.Log(distance);
        audioSource.volume = Mathf.Clamp01((distance - (1 - ledgeSoundThreshold)) / ledgeSoundThreshold);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        isPlayerOnTop = true;
        player = other.transform;
        audioSource.Play();
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        isPlayerOnTop = false;
        player = null;
        audioSource.Pause();
    }
}
