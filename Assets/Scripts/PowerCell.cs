using Powers;
using UnityEngine;


    public class PowerCell : MonoBehaviour
    {
        [SerializeField] private PowerTypes powerType;
        public PowerTypes PowerType => powerType;
    }