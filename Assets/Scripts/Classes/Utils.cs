using System;
using System.Collections;
using UnityEngine;

    public static class Utils
    {

        public static IEnumerator LerpVolume(AudioSource audioSource, float startValue, float endValue, float duration,
            bool pauseAtEnd = false, AnimationCurve curve = null)
        {
            float time = 0;
            curve ??= AnimationCurve.Linear(0, 0, 1, 1);

            while (time < duration)
            {
                float curveTime = curve.Evaluate(time / duration);

                audioSource.volume = (Mathf.Lerp(startValue, endValue, curveTime));

                time += Time.deltaTime;
                yield return null;
            }
            
            audioSource.volume = endValue;
            
            if (pauseAtEnd)
                audioSource.Pause();
        }
    }