using System;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    private int deityPoints; //how much deity points will be rewarded on kill.

    [SerializeField] float maxHealth;
    private float health;
    private bool isAlive;

    public event Action<BossDeathEventData> OnBossDeath;

    public int DeityPoints
    {
        get { return deityPoints; }
        set { deityPoints = value; }
    }
    public float MaxHealth => maxHealth;
    public float Health => health;
    public bool IsAlive => isAlive;

    private void Start()
    {
        maxHealth = 100;
        health = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health < 0)
        {
            OnBossDeath?.Invoke(new BossDeathEventData(this, deityPoints));
        }
    }
}
