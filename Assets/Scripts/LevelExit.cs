using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelExit : MonoBehaviour
    {
        private float time = 0f;
        
        private void Update()
        {
            transform.localRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(90, 90, -90), time);
            
            if (time >= 1)
                time = 0;
            
            time += Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            SceneManager.LoadScene("GameOver");
        }
    }