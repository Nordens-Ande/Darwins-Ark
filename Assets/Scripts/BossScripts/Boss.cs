using UnityEngine;
using Assets.Scripts.Environment;

public class Boss : MonoBehaviour, IBoatSpawning
{
    public Transform Transform => transform;

    BossHealth healthScript;
    BossMovement movementScript;

    Tile currentTile;

    public Tile CurrentTile => currentTile;

    void Start()
    {
        transform.localPosition = Vector3.zero;
        //healthScript = gameObject.GetComponent<BossHealth>();
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

    //util ai
    //behaviours

    //move from beach to playable area // override utility ai if on beach tiles
    //seek //too far from plants / no line of sight / no animals close
    //escape // low hp / animals too close / distance to target?
    //move // go to decided point
    //destroy tile // distance to target tile (plant, river etc depending on boss type) at tile basically
}
