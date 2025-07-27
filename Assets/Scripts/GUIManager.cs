using TMPro;
using UnityEngine;

public class GUIManager : MonoBehaviour
    {
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private RectTransform playerOne;
        [SerializeField] private RectTransform playerTwo;
        public static GUIManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);

            UpdateScore();
        }
        
        public void UpdateScore() => scoreText.text = Player.Score.ToString();
    }