using System.Collections.Generic;
using Assets.Scripts.Environment;
using UnityEngine;

public class BoatManager : MonoBehaviour
{
    public static BoatManager Instance;

    [SerializeField] GameObject BoatPrefab;

    List<Boat> boats;
    bool spawnedBoat;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        spawnedBoat = false;
        boats = new List<Boat>();
        
    }

    Tile GetSpawnTile(Tile targetTilePos)
    {
        //tileManager.get water y level
        return TileManager.Instance.GetTile(40, 0);
    }

    public void SpawnBoat(GameObject objectOnBoat, Tile targetTilePos)
    {
        Tile spawnTile = GetSpawnTile(targetTilePos);
        GameObject boat = Instantiate(BoatPrefab, spawnTile.position, Quaternion.identity);
        if(boat != null)
        {
            Boat boatScript = boat.GetComponent<Boat>();
            if(boatScript != null)
            {
                boatScript.Initialize(targetTilePos, spawnTile);
                boats.Add(boatScript);
            }
        }
    }

    void Update()
    {
        if (!spawnedBoat)
        {
            SpawnBoat(BoatPrefab, TileManager.Instance.GetTile(0, 30));
            spawnedBoat = true;
        }
    }
}
