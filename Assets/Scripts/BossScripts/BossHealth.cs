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

    private void Awake()
    {
        health = 100;
        isAlive = true;
    }

    void Start()
    {
        
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
