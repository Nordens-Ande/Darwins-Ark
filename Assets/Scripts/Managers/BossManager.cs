using Assets.Scripts.Environment;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BossManager : SingletonManager<BossManager>
{
    private enum BossTypes
    {
        PlantAttacker,
        ConcreteMaker,
        AnimalAttacker
    }

    private PlantManager plantManager;
    private AnimalManager animalManager;
    private TileManager tileManager;

    [SerializeField] GameObject bossPrefab;

    private List<Boss> bossList;

    private float reevaluateNextWaveTimer;
    private int reevaluateNextWaveTimerThreshold = 60; // 1 minutes
    private List<BossTypes> nextWave;
    private int minBossesPerWave = 1;
    private int maxBossesPerWave = 6;

    public event Action<BossSpawnedEventData> OnBossSpawned;

    public List<Boss> BossList => bossList;

    void Start()
    {
        plantManager = PlantManager.Instance;
        animalManager = AnimalManager.Instance;
        tileManager = TileManager.Instance;
        bossList = new List<Boss>();
        StartCoroutine(InitialBossWave());
    }

    private IEnumerator InitialBossWave()
    {
        yield return new WaitForSeconds(1);
        nextWave = DecideNextBossWave();
    }

    public void SpawnBossWave()
    {
        if (nextWave == null) return;
        if (nextWave.Count <= 0) return;

        foreach(BossTypes bossType in nextWave)
        {
            switch(bossType)
            {
                case BossTypes.PlantAttacker:
                    SpawnBoss(bossPrefab);
                    break;
                case BossTypes.AnimalAttacker:
                    SpawnBoss(bossPrefab);
                    break;
                case BossTypes.ConcreteMaker: 
                    SpawnBoss(bossPrefab);
                    break;
            }
        }
        nextWave = DecideNextBossWave();
        reevaluateNextWaveTimer = 0;
    }

    private void SpawnBoss(GameObject prefab)
    {
        GameObject boat = BoatManager.Instance.SpawnBoat();
        
        Boat boatScript = boat.GetComponent<Boat>();
        GameObject boss = Instantiate(bossPrefab, new Vector3(0, 0, 0), boatScript.InitialRotation, boatScript.LoadPos.transform);
        if (boss == null) return;

        BossHealth bossHealth = boss.GetComponent<BossHealth>();
        Boss bossScript = boss.GetComponent<Boss>(); 
        if (bossHealth != null)
        {
            bossHealth.OnBossDeath += BossDied;
            bossHealth.DeityPoints = 10; // randomize later?
        }

        OnBossSpawned?.Invoke(new BossSpawnedEventData(bossScript));
        boatScript.LoadObject = bossHealth.gameObject.GetComponent<Boss>();
    }
    
    void BossDied(BossDeathEventData data)
    {
        data.Boss.OnBossDeath -= BossDied;
        bossList.Remove(data.Boss.gameObject.GetComponent<Boss>());
        Destroy(data.Boss.gameObject);
    }

    private List<BossTypes> DecideNextBossWave()
    {
        int animalCount = animalManager.Animals.Count;
        int plantCount = plantManager.Plants.Count;

        int playableTiles = 0;
        int concreteTiles = 0;
        int riverTiles = 0;

        foreach (Chunk chunk in tileManager.chunks)
        {
            playableTiles += chunk.GetTiles(TileType.Grass).Count;
            concreteTiles += chunk.GetTiles(TileType.Concrete).Count;
            playableTiles += concreteTiles;
            riverTiles += chunk.GetTiles(TileType.River).Count;
            playableTiles += riverTiles;
            playableTiles += chunk.GetTiles(TileType.Lake).Count;
        }

        float animalsPerTile = (float)animalCount / playableTiles;
        float plantsPerTile = (float)plantCount / playableTiles;
        float concreteTilesPercentage = (float)concreteTiles / playableTiles;
        float riverTilesPercentage = (float)riverTiles / playableTiles;

        float playerProgressEval = Mathf.Clamp01(animalsPerTile + plantsPerTile);
        int amountOfBosses = Mathf.RoundToInt(Mathf.Lerp(minBossesPerWave, maxBossesPerWave, playerProgressEval));

        Dictionary<BossTypes, float> bossProbability = new Dictionary<BossTypes, float>();
        //maybe add additional weights here for balancing later
        bossProbability[BossTypes.PlantAttacker] = plantsPerTile; // * weight?
        bossProbability[BossTypes.ConcreteMaker] = concreteTilesPercentage;// * some other weight?
        bossProbability[BossTypes.PlantAttacker] = riverTilesPercentage;// * perhaps another weight?

        float totalWeight = 0;
        foreach(float prob in bossProbability.Values)
        {
            totalWeight += prob;
        }

        List<BossTypes> bossTypesToSpawn = new List<BossTypes>();
        for(int i = 0; i < amountOfBosses; i++)
        {
            float randomValue = UnityEngine.Random.Range(0, totalWeight);
            BossTypes bossType = BossTypes.PlantAttacker;
            foreach(var boss in bossProbability)
            {
                randomValue -= boss.Value;
                if(randomValue <= 0)
                {
                    bossType = boss.Key;
                    break;
                }
            }
            bossTypesToSpawn.Add(bossType);
        }

        return bossTypesToSpawn;
    }

    void Update()
    {
        if(reevaluateNextWaveTimer >= reevaluateNextWaveTimerThreshold)
        {
            nextWave = DecideNextBossWave();
            reevaluateNextWaveTimer = 0;
        }
        else
        {
            reevaluateNextWaveTimer += Time.deltaTime;
        }
    }
}
