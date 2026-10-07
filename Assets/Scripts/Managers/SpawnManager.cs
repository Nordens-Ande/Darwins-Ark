using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    List<Vector3> GetSpawnPoint(int amount) // need to know beach tiles that is next to ocean
    {
        return null;
    }

    Vector3 GetSpawnPoint()
    {
        List<Vector3> spawnPoints = GetSpawnPoint(1);
        return spawnPoints[0];
    }

    public void SpawnSingle(GameObject objectToSpawn)
    {
        Vector3 spawnPoint = GetSpawnPoint();
        //spawn boat with objectToSpawn at with target spawnPoint
    }

    public void SpawnMultiple(List<GameObject> objectsToSpawn)
    {
        List<Vector3> spawnPoints = GetSpawnPoint(objectsToSpawn.Count);
        int iterationCount = 0;
        foreach(GameObject obj in objectsToSpawn)
        {
            //spawn object on boat
            iterationCount++;
        }
    }
    
    void Update()
    {
        
    }
}
