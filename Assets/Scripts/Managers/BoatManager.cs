using System.Collections.Generic;
using Assets.Scripts.Environment;
using UnityEngine;

public class BoatManager : MonoBehaviour
{
    public static BoatManager Instance;

    [SerializeField] GameObject BoatPrefab;

    List<Boat> boats;

    float spawnDistanceFromCenter;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        boats = new List<Boat>();
    }

    Tile GetSpawnTile()
    {
        spawnDistanceFromCenter = IslandNoise.Instance.MaxIslandRadius * 2;
        float angle = Random.Range(0, Mathf.PI * 2);
        Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        Tile spawnTile = TileManager.Instance.GetTile(direction * spawnDistanceFromCenter);
        return spawnTile;
    }

    Tile GetBeachTile()
    {
        Vector3Int[] directions =
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 0, -1),
        };

        List<Tile> possibleTiles = new List<Tile>();

        foreach (Chunk chunk in TileManager.Instance.chunks)
        {
            foreach (Vector2Int tilePos in chunk.BeachTiles)
            {
                foreach(Vector3Int direction in directions)
                {
                    Tile tile = TileManager.Instance.GetTile(tilePos.x + direction.x, tilePos.y + direction.z);
                    if(tile != null)
                    {
                        if(tile.Type is TileType.Ocean)
                        {
                            possibleTiles.Add(TileManager.Instance.GetTile(tilePos.x, tilePos.y));
                            break;
                        }
                    }
                }
            }
        }

        if(possibleTiles.Count > 0)
        {
            return possibleTiles[Random.Range(0, possibleTiles.Count)];
        }
        return TileManager.Instance.GetTile(-10, -40);
    }

    public GameObject SpawnBoat()
    {
        Tile spawnTile = GetSpawnTile();
        Tile beachTile = GetBeachTile();
        GameObject boat = Instantiate(BoatPrefab, spawnTile.position, Quaternion.identity);
        if(boat != null)
        {
            Boat boatScript = boat.GetComponent<Boat>();
            if(boatScript != null)
            {
                boatScript.Initialize(beachTile, spawnTile);
                boats.Add(boatScript);
            }
        }
        return boat;
    }

    void Update()
    {

    }
}
