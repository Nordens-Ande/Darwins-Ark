using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Environment;

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

    

    //public List<GameObject> Spawn(List<GameObject> objectsToSpawn, GameObject parent)
    //{
    //    List<Vector3> spawnPoints = GetSpawnPoint(objectsToSpawn.Count);

    //    if (spawnPoints.Count <= objectsToSpawn.Count)
    //    {
    //        Debug.Log("SpawnManager: could not find spawnpos for each object");
    //    }

    //    List<GameObject> instantiatedObjects = new List<GameObject>();
    //    for (int i = 0; i < spawnPoints.Count; i++)
    //    {
            
    //        BoatManager.Instance.SpawnBoat(objectsToSpawn[i], spawnPoints[i]);
    //        GameObject go = Instantiate
    //    }
    //}
    
    void Update()
    {
        
    }
}
