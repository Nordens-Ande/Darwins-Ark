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

    void Awake()
    {
        if( Instance == null)
            BossManager.Instance = this;
    }

    void Start()
    {
        //SpawnBoss();
    }

    Vector2 GetSpawnLocation() //only works for square maps
    {
        int maxSpawn = 
            TileManager.Instance.ChunkGridSize.x * 
            TileManager.Instance.ChunkSize;

        maxSpawn--;

        // Debug
        Debug.Log("ChunkGridSize: " + TileManager.Instance.ChunkGridSize);
        Debug.Log("ChunkSize: " + TileManager.Instance.ChunkSize);
        Debug.Log("MaxSpawn: " + maxSpawn);
        //-----------------------------

        int x = Random.Range(0, maxSpawn + 1);
        int side = Random.Range(1, 5);

        // Debug
        Debug.Log("Random x: " + x);
        Debug.Log("Side: "+ side);
        //----------------------------

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
        Debug.Log("SpawnBoss was called!");

        if (currentBoss != null) 
            return;

        Vector2 spawnLocation = GetSpawnLocation();
        Vector3 spawnLocation3D = 
            new Vector3(spawnLocation.x, 2f, spawnLocation.y);

        // Debug
        Debug.Log("Requested spawn position: " + spawnLocation3D);
        //--------------------------
        currentBoss = 
            Instantiate(bossPrefab, spawnLocation3D, Quaternion.identity);

        // Debug
        Debug.Log("Actual boss position: " + currentBoss.transform.position);
        Debug.Log("Boss scale: " + currentBoss.transform.localScale);
        //--------------------------

        if(AnimalManager.Instance != null)
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
