using System;
using UnityEngine;


    public class GameManager : MonoBehaviour
    {
        public int PlayerLayer { get; private set; }
        public int InvincibleLayer { get; private set; }

        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private LayerMask invincibleLayer;
        
        public static GameManager Instance {get; private set;}

        public void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
            
            PlayerLayer = (int)Mathf.Log(playerLayer.value, 2);
            InvincibleLayer = (int)Mathf.Log(invincibleLayer.value, 2);
        }
    }