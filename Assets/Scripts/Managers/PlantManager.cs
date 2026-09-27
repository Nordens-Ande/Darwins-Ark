using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Environment;
using System.Linq;


public class PlantManager : MonoBehaviour
{
    public static PlantManager Instance = null;


    [Header("Varibles")]
    [SerializeField] float SecondsForPlantToGrow = 10.0f;
    [Space]

    [Header("Plant Spawning")]
    [SerializeField] private List<Plant> plantPrefabToSpawn = new List<Plant>(); //this is the plant that will be spawned
    //[SerializeField] private Tile targetTileToSpawnOn; //this is the tile that the plant will be spawned on
    //[SerializeField] bool spawnPlant = false; //this is used as a button to spawn a plant
    [Space]
    [SerializeField] bool spawnPlantAllTiles = false; //this is used as a button to spawn plants on all tiles
    [SerializeField] float spawnPlantAllTilesChance = 20; //chance of a plant spawning on a tile when spawning on all tiles
    [Space]
    [SerializeField] bool useRandomGrowthSpeed = true;
    [SerializeField] bool useRandomPlants = true;

    [Header("List of all Plants")]
    [SerializeField] private List<Plant> plants = new List<Plant>();


    private void Awake()
    {
        if (Instance == null)
            PlantManager.Instance = this;
    }

    void Start()
    {
        //find all plants in the scene
        if (plants.Count == 0)
        {
            plants.AddRange(FindObjectsByType<Plant>());
        }
    }

    void Update()
    {
        //lopp all plants
        for (int i = plants.Count - 1; i >= 0; i--)
        {
            //if the plant is null, remove it from the list
            if (plants[i] == null)
            {
                plants.RemoveAt(i);
                continue;
            }

            //check if the plant is ready to grow, depnding on growth speed and time since last growth
            if (plants[i].TimeSinceLastGrowth / plants[i].GrowthSpeed >= SecondsForPlantToGrow)
            {
                plants[i].GrowPlant();
            }
        }


        ////spawn in a plant
        //if (spawnPlant)
        //{
        //    if (targetTileToSpawnOn != null)
        //    {
        //        SpawnPlantOnThisTile(plantPrefabToSpawn, targetTileToSpawnOn);
        //    }
        //    else
        //    {
        //        Debug.LogWarning("Ingen Target Tile vald i PlantManager!");
        //    }
        //    spawnPlant = false;
        //}


        //try to spawn plants on all tiles
        if (spawnPlantAllTiles)
        {
            if (TileManager.Instance != null && TileManager.Instance.chunks != null)
            {

                foreach (Chunk chunk in TileManager.Instance.chunks) //we loop through all chunks
                {
                    foreach (Tile tile in chunk.Tiles) //loop through all tiles in the chunk
                    {
                        if (tile == null || tile.isWater || tile.HasPlant) continue;

                        float randomValue = Random.Range(0f, 100f);
                        if (randomValue <= spawnPlantAllTilesChance)
                        {

                            float randomGrowthSpeed = 1.0f;
                            Plant plant = plantPrefabToSpawn.First();

                            if (useRandomGrowthSpeed)
                            {
                                randomGrowthSpeed = Random.Range(0.5f, 2.0f);
                            }
                            if (useRandomPlants)
                            {
                                plant = plantPrefabToSpawn[Random.Range(0, plantPrefabToSpawn.Count)];
                            }

                            SpawnPlantOnThisTile(plant, tile, randomGrowthSpeed);
                        }

                    }

                }


            }
            else
            {
                Debug.Log("didnt find tilemanager or chunks when trying to spawn plants");
            }
            spawnPlantAllTiles = false;
        }

    }



    public Plant SpawnPlantOnThisTile(Plant prefab, Tile tile, float CustomGrowSpeed = 1.0f)
    {
        if (prefab == null || tile == null) return null;

        // if the tile alredy has a plant
        if (tile.HasPlant)
        {
            Debug.Log("Tilen har redan en planta!");
            return null;
        }

        //create a new plant on the tile
        Plant newPlant = Instantiate(prefab, tile.position, Quaternion.identity);

        //set the growth speed of the plant
        newPlant.GrowthSpeed = CustomGrowSpeed;

        //couple the plant to the tile
        newPlant.OccupyTile(tile);

        // Register in the manager
        RegisterPlant(newPlant);

        return newPlant;
    }


    // this is to plant a new plant while playing
    public void RegisterPlant(Plant newPlant)
    {
        if (!plants.Contains(newPlant))
        {
            plants.Add(newPlant);
        }
    }
}