using UnityEngine;

public class BossManager : MonoBehaviour
{
    [SerializeField] GameObject bossPrefab;
    GameObject currentBoss;
    void Start()
    {
        SpawnBoss();
    }

    Vector2 GetSpawnLocation()
    {
        int x = Random.Range(-1, 21);
        int side = Random.Range(1, 5);
        switch(side)
        {
            case 1:
                return new Vector2(x, -1);
                
            case 2:
                return new Vector2(x, 20);
                
            case 3:
                return new Vector2(-1, x);
                
            case 4:
                return new Vector2(20, x);
        }
        return new Vector2(10, 0);
    }

    public void SpawnBoss()
    {
        Vector2 spawnLocation = GetSpawnLocation();
        Vector3 spawnLocation3D = new Vector3(spawnLocation.x, 0, spawnLocation.y);
        currentBoss = Instantiate(bossPrefab, spawnLocation3D, Quaternion.identity);
    }
    
    void Update()
    {
        if(currentBoss != null)
        {
            if(currentBoss.GetComponent<BossHealth>())
            {
                if (currentBoss.GetComponent<BossHealth>().IsAlive == false)
                {
                    currentBoss.SetActive(false);
                    Destroy(currentBoss);
                    currentBoss = null;
                }
            }
        }
    }
}
