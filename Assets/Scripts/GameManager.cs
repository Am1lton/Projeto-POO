using System;
using UnityEngine;


    public class GameManager : MonoBehaviour
    {
        public LayerMask playerMask;
        
        
        public static GameManager Instance {get; private set;}

        public void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }
    }