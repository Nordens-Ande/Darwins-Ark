using System.Collections.Generic;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    public static PlantManager Instance = null;


    [Header("Varibles")]
    [SerializeField] float SecondsForPlantToGrow = 10.0f;
    [Space]

    [Header("Plant Spawning")]
    [SerializeField] private Plant plantPrefabToSpawn; //this is the plant that will be spawned
    [SerializeField] private Tile targetTileToSpawnOn; //this is the tile that the plant will be spawned on
    [SerializeField] bool spawnPlant = false; //this is used as a button to spawn a plant

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


        //spawn in a plant
        if (spawnPlant)
        {
            if (targetTileToSpawnOn != null)
            {
                SpawnPlantOnThisTile(plantPrefabToSpawn, targetTileToSpawnOn);
            }
            else
            {
                Debug.LogWarning("Ingen Target Tile vald i PlantManager!");
            }
            spawnPlant = false;
        }
    }



    public Plant SpawnPlantOnThisTile(Plant prefab, Tile tile)
    {
        if (prefab == null || tile == null) return null;

        // if the tile alredy has a plant
        if (tile.HasPlant)
        {
            Debug.Log("Tilen har redan en planta!");
            return null;
        }

        //create a new plant on the tile
        Plant newPlant = Instantiate(prefab, tile.transform.position, Quaternion.identity);

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