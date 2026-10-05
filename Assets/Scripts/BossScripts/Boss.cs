using UnityEngine;
using Assets.Scripts.Environment;

public class Boss : MonoBehaviour, IBoatSpawning
{
    public Transform Transform => transform;

    BossHealth healthScript;
    BossMovement movementScript;

    void Start()
    {
        transform.localPosition = Vector3.zero;
        healthScript = gameObject.GetComponent<BossHealth>();
        movementScript = gameObject.GetComponent<BossMovement>();
        movementScript.enabled = false;
    }

    public bool Unload(Tile beachTile)
    {
        transform.position = beachTile.position;
        movementScript.enabled = true;
        return true;
    }

    void Update()
    {
        
    }
}
