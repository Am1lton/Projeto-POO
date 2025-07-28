    using UnityEngine;


    public class GameManager : MonoBehaviour
    {
        [SerializeField] public int playerLayer;
        [SerializeField] public int invincibleLayer;
        
        public int PlayerLayer => playerLayer;
        public int InvincibleLayer =>  invincibleLayer;
        
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

            Player.Score = 0;
        }
    }