using UnityEngine;

public class Boss : MonoBehaviour
{
    BossHealth healthScript;
    BossMovement movementScript;

    public Boss()
    {
        healthScript = new BossHealth(100);
        movementScript = new BossMovement();
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        if(movementScript != null)
        {
            movementScript.Update();
        }
    }
}
