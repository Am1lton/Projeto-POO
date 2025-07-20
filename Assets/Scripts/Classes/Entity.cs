using UnityEngine;

public class Entity : MonoBehaviour
{
    private int health;

    public virtual void TakeDamage(int damage)
    {
        if (health - damage <= 0)
        { health = 0; Die(); }
        else
            health -= damage;
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}
