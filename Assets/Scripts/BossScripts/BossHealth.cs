using UnityEngine;

public class BossHealth
{
    int health;

    public BossHealth(int startingHealth)
    {
        health = startingHealth;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
    
}
