using Assets.Scripts.Environment;
using System.Collections.Generic;
using UnityEngine;

public class AnimalManager : MonoBehaviour
{
    //singleton
    public static AnimalManager Instance;

    //fields
    private BossManager bossManager = BossManager.Instance;
    private BoatManager boatManager = BoatManager.Instance;
    [SerializeField] GameObject animalPrefab;
    List<AnimalAI> animals;

    //timer
    float spawnTimer = 10000;
    int spawnTimerThreshold = 10000;

    public List<AnimalAI> Animals => animals;

    private void Awake()
    {
        if(Instance == null)
            AnimalManager.Instance = this;
    }


    void Start()
    {
        animals = new List<AnimalAI>();
        bossManager = BossManager.Instance;
        boatManager = BoatManager.Instance;
        
        //SpawnAnimal();
    }

    Vector3 SpawnPosition()
    {
        int maxSpawn = TileManager.Instance.ChunkGridSize.x * TileManager.Instance.ChunkSize / 2;
        maxSpawn = Mathf.RoundToInt(maxSpawn);
        int minSpawn = -maxSpawn;
        maxSpawn--;

        if (TileManager.Instance.ChunkGridSize.x % 2 != 0) // ojämnt med chunks
        {
            maxSpawn += TileManager.Instance.ChunkSize / 2;
            minSpawn += TileManager.Instance.ChunkSize / 2;
            if (TileManager.Instance.ChunkSize % 2 != 0) // ojämnt med antal tiles i chunk
            {
                maxSpawn++;
            }
        }

        int x = Random.Range(minSpawn, maxSpawn);
        int side = Random.Range(1, 5);
        switch (side)
        {
            case 1:
                return new Vector3(x, 0.5f, minSpawn);

            case 2:
                return new Vector3(x, 0.5f, maxSpawn);

            case 3:
                return new Vector3(minSpawn, 0.5f, x);

            case 4:
                return new Vector3(maxSpawn, 0.5f, x);
        }
        return new Vector3(0, 0.5f, 0);
    }

    void SpawnAnimal()
    {
        if(animalPrefab != null)
        {
            GameObject boat = boatManager.SpawnBoat();
            Boat boatScript = boat.GetComponent<Boat>();

            GameObject spawnedAnimal = Instantiate(animalPrefab, new Vector3(0, 0, 0), boatScript.InitialRotation, boatScript.LoadPos.transform);
            AnimalAI aiScript = spawnedAnimal.GetComponent<AnimalAI>();
            if(aiScript != null)
            { 
                boatScript.LoadObject = aiScript;
                bossManager.OnBossSpawned += aiScript.BossSpawned;
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
        if (spawnTimer >= spawnTimerThreshold)
        {
            SpawnAnimal();
            spawnTimer = 0;
        }
    }
}
