using System.Collections.Generic;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    [Header("Varibles")]
    [SerializeField] float SecondsForPlantToGrow = 10.0f;
    [Space]

    [Header("Plant Spawning")]
    [SerializeField] private Plant plantPrefabToSpawn; //this is the plant that will be spawned
    [SerializeField] private Vector3 spawnPosition = Vector3.zero; //where to spawn the plant
    [SerializeField] bool spawnPlant = false; //this is used as a button to spawn a plant

    [Header("List of all Plants")]
    [SerializeField] private List<Plant> plants = new List<Plant>();

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
            if (plantPrefabToSpawn != null)
            {
                //spawn a new plant at the spawn position
                Plant newPlant = Instantiate(plantPrefabToSpawn, spawnPosition, Quaternion.identity);

                //add the new plant to the list of plants
                RegisterPlant(newPlant);
            }
            spawnPlant = false;
        }

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