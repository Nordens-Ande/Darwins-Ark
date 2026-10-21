using Assets.Scripts.Environment;
using UnityEngine;

public class BossManager : SingletonManager<BossManager>
{
    [SerializeField] GameObject bossPrefab;
    GameObject currentBoss;

    public GameObject CurrentBoss
    {
        get { return currentBoss; }
    }

    bool spawnedBoss = false;

    void Start()
    {
        
    }

    public void SpawnBoss()
    {
        //if (currentBoss != null) return;

        //get boat
        GameObject boat = BoatManager.Instance.SpawnBoat();
        
        Boat boatScript = boat.GetComponent<Boat>();
        currentBoss = Instantiate(bossPrefab, new Vector3(0, 0, 0), Quaternion.identity, boatScript.LoadPos.transform);
        boatScript.LoadObject = currentBoss.GetComponent<Boss>();
        
    }
    
    void BossDied()
    {
        currentBoss.SetActive(false);
        Destroy(currentBoss);
        currentBoss = null;
    }

    void Update()
    {
        if (!spawnedBoss)
        {
            SpawnBoss();
            spawnedBoss = true;
        }

        if (currentBoss != null)
        {
            //if (currentBoss.GetComponent<BossHealth>())
            //{
            //    if (currentBoss.GetComponent<BossHealth>().IsAlive == false)
            //    {
            //        BossDied();
            //    }
            //}
        }
    }
}
