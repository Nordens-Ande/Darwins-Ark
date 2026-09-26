using UnityEngine;

public class BossHealth : MonoBehaviour
{
    float health;
    bool isAlive;

    public bool IsAlive 
    { 
        get { return isAlive; }
        set { isAlive = value; } 
    }

    void Start()
    {
        health = 100;
        isAlive = true;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if(health < 0)
        {
            isAlive = false;
        }
    }
}
