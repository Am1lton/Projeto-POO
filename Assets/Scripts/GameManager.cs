    using UnityEngine;


    public class GameManager : MonoBehaviour
    {
        [SerializeField] public int playerLayer;
        [SerializeField] public int invincibleLayer;
        [SerializeField] private Transform centerOfScreen;
        
        public Transform  CenterOfScreen => centerOfScreen;
        public LayerMask PlayerMask => 1 << playerLayer;
        public LayerMask InvincibleMask =>  1 << invincibleLayer;
        
        public static GameManager Instance {get; private set;}

        public void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
            {
                Destroy(this);
                return;
            }

            Player.ResetScore();
        }
        
        private void OnValidate()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
            {
                Destroy(this);
            }
        }
    }