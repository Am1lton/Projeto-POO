using UnityEngine;
using UnityEngine.Serialization;

namespace Classes
{
    public abstract class Collectable<TContent, TCollector> : MonoBehaviour
    {
        [SerializeField] protected TContent content;
        protected abstract void Collect(TCollector collector);

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<TCollector>(out TCollector collector))
                return;
            
            Collect(collector);
            Destroy(gameObject);
        }
    }
}