using System.Collections.Generic;
using UnityEngine;

public class SeedManager : SingletonManager<SeedManager>
{
    [SerializeField, ReadOnly] private List<Seed> allSeeds = new List<Seed>();


    [Header("Debug")]
    [SerializeField] private bool debugGetRandomSeed = false;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;

        LoadAllSeeds();
    }

    private void Update()
    {
        if (debugGetRandomSeed)
        {
            debugGetRandomSeed = false;

            Seed randomSeed = GetRandomSeed();
            if (randomSeed != null)
            {
                Debug.Log($"SeedManager gave you a random seed: {randomSeed.SeedName}");

                // we try to add it to the inventory too
                randomSeed.AddToInventory(1);
            }
        }
    }


    private void LoadAllSeeds()
    {
        // we send no path "", cause the seeds are direcly in the resource map
        // unity allows multible resource maps so this one in particular is here: Assets\Prefabs\seeds\Resources
        Seed[] loadedSeeds = Resources.LoadAll<Seed>("");

        if (loadedSeeds != null && loadedSeeds.Length > 0)
        {
            allSeeds = new List<Seed>(loadedSeeds);
        }
        else
        {
            Debug.LogWarning("didnt find any seeds in the resource map!");
        }
    }

    // we get a random seed to give to whoever asks
    public Seed GetRandomSeed()
    {
        if (allSeeds == null || allSeeds.Count == 0)
        {
            Debug.LogError("there is no seeds in the seedManager");
            return null;
        }

        int randomIndex = Random.Range(0, allSeeds.Count);
        return allSeeds[randomIndex];
    }


    // Getter
    public List<Seed> AllSeeds => allSeeds;
}