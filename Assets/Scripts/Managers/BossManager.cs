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
        SpawnBoss();
    }

    Vector2 GetSpawnLocation() //only works for square maps
    {
        int maxSpawn = TileManager.Instance.ChunkGridSize.x * TileManager.Instance.ChunkSize;
        maxSpawn--;
        int x = Random.Range(0, maxSpawn);
        int side = Random.Range(1, 5);
        switch(side)
        {
            case 1:
                return new Vector2(x, 0);
                
            case 2:
                return new Vector2(x, maxSpawn);
                
            case 3:
                return new Vector2(0, x);
                
            case 4:
                return new Vector2(maxSpawn, x);
        }
        return new Vector2(0, 0);
    }

    public void SpawnBoss()
    {
        Vector2 spawnLocation = GetSpawnLocation();
        Vector3 spawnLocation3D = new Vector3(spawnLocation.x, 0, spawnLocation.y);
        currentBoss = Instantiate(bossPrefab, spawnLocation3D, Quaternion.identity);
        AnimalManager.Instance.BossSpawned();
    }
    
    void BossDied()
    {
        //trigger event
        //tell animals to chill
        AnimalManager.Instance.BossDied();
        currentBoss.SetActive(false);
        Destroy(currentBoss);
        currentBoss = null;
    }

    void Update()
    {
        if(currentBoss != null)
        {
            if(currentBoss.GetComponent<BossHealth>())
            {
                if (currentBoss.GetComponent<BossHealth>().IsAlive == false)
                {
                    BossDied();
                }
            }
        }
    }
}
