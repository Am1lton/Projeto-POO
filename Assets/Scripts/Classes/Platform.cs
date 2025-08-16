using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Platform : MonoBehaviour
{
    private AudioSource audioSource;
    private Coroutine coroutine;
    private const float FADE_IN_OUT_DURATION = 1f;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Pause();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (coroutine != null) StopCoroutine(coroutine);
        if (!audioSource.isPlaying) audioSource.UnPause();
        coroutine = StartCoroutine(Utils.LerpVolume(audioSource, audioSource.volume, 1, FADE_IN_OUT_DURATION));
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(Utils.LerpVolume(audioSource, audioSource.volume, 0, FADE_IN_OUT_DURATION, true));
    }
}
