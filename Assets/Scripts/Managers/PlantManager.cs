using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Environment;
using System.Linq;
using System.Collections;


public class PlantManager : MonoBehaviour
{
    public static PlantManager Instance = null;
    TileManager tileManager;


    [Header("Varibles")]
    [SerializeField] float secondsForPlantToGrow = 10.0f;
    [Space]

    [Header("Plant Spawning")]
    [SerializeField] private List<Plant> plantPrefabToSpawn = new List<Plant>(); //this is the plant that will be spawned
    [Space]
    [SerializeField] bool spawnPlantAllTiles = false; //this is used as a button to spawn plants on all tiles
    [SerializeField] float spawnPlantAllTilesChance = 20; //chance of a plant spawning on a tile when spawning on all tiles
    [Space]
    [SerializeField] bool useRandomGrowthSpeed = true;
    [SerializeField] bool useRandomPlants = true;
    [SerializeField] bool useRandomPlantRotation = true;
    [SerializeField] bool useRandomPlantScale = true;
    [SerializeField] bool useRandomOffsetsFromTile = true;
    [SerializeField] float maxTileOffset = 0.25f;
    [Space]
    [SerializeField] bool massPlantPlantsAsGrown = true;

    [Header("List of all Plants")]
    [SerializeField] private List<Plant> plants = new List<Plant>();


    private void Awake()
    {
        if (Instance == null)
            PlantManager.Instance = this;
    }

    void Start()
    {
        tileManager = TileManager.Instance;

        ////find all plants in the scene
        //if (plants.Count == 0)
        //{
        //    plants.AddRange(FindObjectsByType<Plant>());
        //}
        StartCoroutine(SpawnPlantsAfter1Frame());
    }

    IEnumerator SpawnPlantsAfter1Frame()
    {
        yield return new WaitForEndOfFrame(); // we wait one frame
        MassPlantPlants();
    }


    void Update()
    {
        //loop all plants
        for (int i = plants.Count - 1; i >= 0; i--)
        {
            //if the plant is null, remove it from the list
            if (plants[i] == null)
            {
                plants.RemoveAt(i);
                continue;
            }

            //check if the plant is ready to grow, depnding on growth speed and time since last growth
            if (plants[i].TimeSinceLastGrowth / plants[i].GrowthSpeed >= secondsForPlantToGrow)
            {
                plants[i].GrowPlant();
            }
        }



        //try to spawn plants on all tiles
        if (spawnPlantAllTiles)
        {
            MassPlantPlants();
            spawnPlantAllTiles = false;
        }

    }



    public void MassPlantPlants()
    {
        if (tileManager != null && tileManager.chunks != null)
        {

            foreach (Chunk chunk in tileManager.chunks) // we loop through all chunks
            {

                HashSet<Vector2Int> grassTiles = chunk[TileType.Grass];
                if (grassTiles == null || grassTiles.Count() == 0)
                {
                    continue;
                }

                foreach (Vector2Int tilePos in grassTiles) // loop through all tiles in the chunk
                {
                  
                  
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

                       // plant.SetGrowthStage(plant.grow);

                        SpawnPlantOnThisTile(plant, chunk.GetTile(tilePos.x, tilePos.y), randomGrowthSpeed);
                    }

                }

            }


        }
        else
        {
            Debug.Log("didnt find tilemanager or chunks when trying to spawn plants");
        }
    }



    public Plant SpawnPlantOnThisTile(Plant prefab, Tile tile, float CustomGrowSpeed = 1.0f)
    {
        if (prefab == null || tile == null) return null;

        // if the tile alredy has a plant
        if (tile.HasPlant)
        {
            Debug.LogWarning("You tried to plant a plant on a tile which already has a plant");
            return null;
        }


        // the initial pos of the plant on the tile
        Vector3 spawnPos = tile.position;


        // if we want to use a random offset for the lant on the tile
        if(useRandomOffsetsFromTile)
        {
            float offsetX = Random.Range(-maxTileOffset, maxTileOffset);
            float offsetZ = Random.Range(-maxTileOffset, maxTileOffset);

            spawnPos.x = spawnPos.x + offsetX;
            spawnPos.z = spawnPos.z + offsetZ;
        }

        // create a new plant on the tile
        Plant newPlant = Instantiate(prefab, spawnPos, Quaternion.identity, transform);

        // if we want to randomize y rotation so the plants dont all look the same
        if (useRandomPlantRotation)
        {
            float randomYAngle = Random.Range(0f, 360f);
            newPlant.transform.localRotation = Quaternion.Euler(0f, randomYAngle, 0f);
        }
        
        // if we want to random scale all the plants so they aren't all the same0
        if (useRandomPlantScale)
        {
            float randomScale = Random.Range(0.75f, 1.35f);
            newPlant.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        }
        

        // set the growth speed of the plant
        newPlant.GrowthSpeed = CustomGrowSpeed;

        // couple the plant to the tile
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