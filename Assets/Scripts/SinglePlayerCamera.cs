using UnityEngine;

    public class SinglePlayerCamera : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float yOffset;
        
        private void LateUpdate()
        {
            transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y + yOffset, transform.position.z);
        }
    }