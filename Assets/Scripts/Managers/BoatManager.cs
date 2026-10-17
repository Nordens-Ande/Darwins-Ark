using System.Collections.Generic;
using Assets.Scripts.Environment;
using UnityEngine;

public class BoatManager : MonoBehaviour
{
    public static BoatManager Instance;

    private TileManager tileManager;
    private IslandNoise islandNoise;

    [SerializeField] GameObject BoatPrefab;

    List<Boat> boats = new List<Boat>();

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
        tileManager = TileManager.Instance;
        islandNoise = IslandNoise.Instance;
    }

    Tile GetSpawnTile()
    {   
        spawnDistanceFromCenter = (tileManager.ChunkGridSize.x * tileManager.ChunkSize / 2) - 3;
        float angle = Random.Range(0, Mathf.PI * 2);
        Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        Vector3 spawnPos = direction.normalized * spawnDistanceFromCenter;
        //spawnPos = new Vector3((int)spawnPos.x, 0, (int)spawnPos.z);
        Tile spawnTile = tileManager.GetTile(spawnPos);
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
        
        foreach (Chunk chunk in tileManager.chunks)
        {
            foreach (Vector2Int tilePos in chunk.GetTiles(TileType.Beach))
            {
                foreach(Vector3Int direction in directions)
                {
                    Tile tile = tileManager.GetTile(tilePos.x + direction.x, tilePos.y + direction.z);
                    if(tile != null)
                    {
                        if(tile.Type is TileType.Ocean)
                        {
                            possibleTiles.Add(tileManager.GetTile(tilePos.x, tilePos.y));
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
        return tileManager.GetTile(-10, -40);
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
