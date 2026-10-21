using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [SerializeField] float maxHealth;
    float health;
    bool isAlive;

    public float MaxHealth => maxHealth;
    public float Health => health;
    public bool IsAlive => isAlive;

    private void Start()
    {
        maxHealth = 100;
        health = maxHealth;
    }

    public bool TakeDamage(float damage)
    {
        health -= damage;
        if(health < 0) return true;
        return false;
    }
}
