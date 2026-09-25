using Assets.Scripts.Environment;
using System.Collections.Generic;
using UnityEngine;

public class AnimalManager : MonoBehaviour
{
    //singleton
    public static AnimalManager Instance;

    //fields
    [SerializeField] GameObject animalPrefab;
    List<AnimalAI> animals;

    //timer
    float spawnTimer = 0;
    int spawnTimerThreshold = 10;

    private void Awake()
    {
        if(Instance == null)
            AnimalManager.Instance = this;
    }


    void Start()
    {
        animals = new List<AnimalAI>();
        SpawnAnimal();
    }

    Vector3 SpawnPosition()
    {
        int maxSpawn = TileManager.Instance.ChunkGridSize.x * TileManager.Instance.ChunkSize;
        maxSpawn--;
        int x = Random.Range(0, maxSpawn);
        int side = Random.Range(1, 5);
        switch (side)
        {
            case 1:
                return new Vector3(x, 0.5f, 0);

            case 2:
                return new Vector3(x, 0.5f, maxSpawn);

            case 3:
                return new Vector3(0, 0.5f, x);

            case 4:
                return new Vector3(maxSpawn, 0.5f, x);
        }
        return new Vector3(0, 0.5f, 0);
    }

    void SpawnAnimal()
    {
        if(animalPrefab != null)
        {
            GameObject spawnedAnimal = Instantiate(animalPrefab, SpawnPosition(), Quaternion.identity);
            AnimalAI aiScript = spawnedAnimal.GetComponent<AnimalAI>();
            if(aiScript != null)
            { 
                animals.Add(aiScript);
            }
        }
    }

    public void BossSpawned()
    {
        foreach(var animal in animals)
        {
            animal.BossThreat = 1;
        }
    }

    public void BossDied()
    {
        foreach(var animal in animals)
        {
            animal.BossThreat = 0;
        }
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if(spawnTimer >= spawnTimerThreshold)
        {
            SpawnAnimal();
            spawnTimer = 0;
        }
    }
}
