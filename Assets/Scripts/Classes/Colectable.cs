using UnityEngine;

namespace Classes
{
    public abstract class Collectable<TContent, TCollector> : MonoBehaviour
    {
        [SerializeField] protected TContent content;
        protected abstract void Collect(TCollector collector);

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out TCollector collector))
                return;
            
            Collect(collector);
            OnCollected();
        }

        protected virtual void OnCollected()
        {
            Destroy(gameObject);
        }
    }
}