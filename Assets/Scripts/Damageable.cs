using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    [SerializeField] float health = 200f;
    public UnityEvent OnDamaged;
    public UnityEvent OnDead;

    public float GetHealth() {
        return health;
    }
    
    public void TakeDamage(float amount) {

        health -= amount;
        if (health < 0) {
            OnDead.Invoke();
        } else {
            OnDamaged.Invoke();
        }

    }   

}