using UnityEngine;

public class Boss : MonoBehaviour
{
    BossHealth healthScript;
    BossMovement movementScript;

    public Boss()
    {
        
    }

    void Start()
    {
        healthScript = gameObject.GetComponent<BossHealth>();
        movementScript = gameObject.GetComponent<BossMovement>();
    }

    void Update()
    {
        
    }
}
