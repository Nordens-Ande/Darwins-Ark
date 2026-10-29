using Assets.Scripts.Environment;
using UnityEngine;
using System;
using System.Collections.Generic;

public class BossManager : SingletonManager<BossManager>
{
    [SerializeField] GameObject bossPrefab;
    List<Boss> bossList;

    bool spawnedBoss = false;

    public event Action<BossSpawnedEventData> OnBossSpawned;

    public List<Boss> BossList => bossList;

    void Start()
    {
        bossList = new List<Boss>();
    }

    public void SpawnBoss()
    {
        GameObject boat = BoatManager.Instance.SpawnBoat();
        
        Boat boatScript = boat.GetComponent<Boat>();
        GameObject boss = Instantiate(bossPrefab, new Vector3(0, 0, 0), Quaternion.identity, boatScript.LoadPos.transform);
        if (boss == null) return;

        BossHealth bossHealth = boss.GetComponent<BossHealth>();
        Boss bossScript = boss.GetComponent<Boss>(); 
        if (bossHealth != null)
        {
            bossHealth.OnBossDeath += BossDied;
            bossHealth.DeityPoints = 10; //////// randomize later?
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

    void Update()
    {
        //if (!spawnedBoss)
        //{
        //    SpawnBoss();
        //    spawnedBoss = true;
        //}
    }
}
