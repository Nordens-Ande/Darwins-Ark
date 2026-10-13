using UnityEngine;

public class BossHealth
{
    float health;
    bool isAlive;

    public bool IsAlive => isAlive;
    
    public BossHealth(int health)
    {
        this.health = health;
    }

    public bool TakeDamage(float damage)
    {
        health -= damage;
        if(health < 0) return true;
        return false;
    }
}
