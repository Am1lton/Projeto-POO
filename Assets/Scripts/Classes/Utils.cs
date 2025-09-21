using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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

        public static IEnumerator VibrateController(Gamepad controller, float duration, float lowFrequency,
            float highFrequency, bool fade = false)

        {
            float timer = 0;
            controller.SetMotorSpeeds(lowFrequency, highFrequency);
            while (timer < duration)
            {
                if (fade)
                {
                    float t = timer/duration;
                    controller.SetMotorSpeeds(Mathf.Lerp(lowFrequency, 0, t),
                        Mathf.Lerp(highFrequency, 0, t));
                }
                
                timer += Time.deltaTime;
                yield return null;
            }
            controller.SetMotorSpeeds(0, 0);
        }
    }