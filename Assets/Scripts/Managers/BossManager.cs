using Assets.Scripts.Environment;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance = null;

    [SerializeField] GameObject bossPrefab;
    GameObject currentBoss;

    public GameObject CurrentBoss
    {
        get { return currentBoss; }
    }

    private void Awake()
    {
        if(Instance == null)
            BossManager.Instance = this;
    }

    void Start()
    {
        //SpawnBoss();
    }

    Vector2 GetSpawnLocation() //only works for square maps
    {
        int maxSpawn = TileManager.Instance.ChunkGridSize.x * TileManager.Instance.ChunkSize / 2;
        maxSpawn = Mathf.RoundToInt(maxSpawn);
        int minSpawn = -maxSpawn;
        maxSpawn--;

        int x = Random.Range(minSpawn, maxSpawn);
        int side = Random.Range(1, 5);
        switch(side)
        {
            case 1:
                return new Vector2(x, minSpawn);
                
            case 2:
                return new Vector2(x, maxSpawn);
                
            case 3:
                return new Vector2(minSpawn, x);
                
            case 4:
                return new Vector2(maxSpawn, x);
        }
        return new Vector2(0, 0);
    }

    public void SpawnBoss()
    {
        if (currentBoss != null) return;
        Vector2 spawnLocation = GetSpawnLocation();
        Vector3 spawnLocation3D = new Vector3(spawnLocation.x, 0, spawnLocation.y);
        currentBoss = Instantiate(bossPrefab, spawnLocation3D, Quaternion.identity);
        if (AnimalManager.Instance != null)
            AnimalManager.Instance.BossSpawned();
    }
    
    void BossDied()
    {
        if(AnimalManager.Instance != null)
            AnimalManager.Instance.BossDied();
        currentBoss.SetActive(false);
        Destroy(currentBoss);
        currentBoss = null;
    }

    void Update()
    {
        if (currentBoss != null)
        {
            if (currentBoss.GetComponent<BossHealth>())
            {
                if (currentBoss.GetComponent<BossHealth>().IsAlive == false)
                {
                    BossDied();
                }
            }
        }
    }
}
