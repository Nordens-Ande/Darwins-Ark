using Assets.Scripts.Environment;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonManager<InventoryManager>
{
    //public static InventoryManager Instance = null; // we dont need this anymore since we inherit from SingletonManager

    [SerializeField, ReadOnly] public Inventory playerInventory;

    [Header("Debug / Testing")]
    [Tooltip("All different seeds that can be chosen")]
    [SerializeField] private List<Seed> availableSeeds = new List<Seed>();

    [Tooltip("randomly fill invenotry with seeds")]
    [SerializeField] bool debugAddRandomSeeds = false;
    [SerializeField] int amountToSpawn = 7; // the amount of seeds we try to add to the inventory

    protected override void Awake()
    {
        base.Awake(); // we call the base awake to make sure the singleton is set up correctly

        if (Instance != this)
        {
            return;
        }

        // Vi försöker hämta inventory om det finns på samma GameObject
        playerInventory = GetComponent<Inventory>();

        // Om det inte finns så skapar vi det
        if (playerInventory == null)
        {
            playerInventory = gameObject.AddComponent<Inventory>();
        }

    }


    private void Update()
    {
        if (debugAddRandomSeeds)
        {
            debugAddRandomSeeds = false;

            for (int i = 0; i < amountToSpawn; i++)
            {
                AddRandomSeedsToInventory();
            }
        }
    }


    // helper function to test seeds in inventory, it spawns randoms seeds and adds them to the inventory
    private void AddRandomSeedsToInventory()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("There is no inventory to add seeds to");
            return;
        }

        if (availableSeeds == null || availableSeeds.Count == 0)
        {
            Debug.LogWarning("you tried to add seeds, but there is no seeds in the list to be added");
            return;
        }

        // we pick a random seed
        Seed randomSeed = availableSeeds[Random.Range(0, availableSeeds.Count)];

        // we pick a random amount for the stack to be depending on it's max stack
        int randomAmount = Random.Range(1, Mathf.Min(randomSeed.MaxStackSize, 10));

        bool success = playerInventory.AddSeed(randomSeed, randomAmount);

        if (success)
        {
            Debug.Log($"<color=green> added {randomAmount} seeds '{randomSeed.SeedName}' in the inventory </color>");
        }
        else
        {
            Debug.LogWarning("<color=red> could not add seed to inventory </color>");
        }
    }



    //returns true if it managed to plant, and false if it didnt
    public bool TryPlantFromSlot(int slotIndex, Tile tile)
    {
        if (playerInventory == null)
        {
            return false;
        }
          
        List<InventorySlot> slots = playerInventory.Slots;
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            Debug.LogWarning("You tried to plant a seed from the iventory which was outside of the possible indexes");
            return false;
        }
            

        InventorySlot slot = slots[slotIndex];
        if (slot.IsEmpty || slot.seed == null || slot.seed.plantPrefab == null)
        {
            Debug.LogWarning("You tried to use an empty slots seed to plant, or there was a missing prefab for that seed");
            return false;
        }
          

        //we tell the plantmanager to plant
        Plant spawnedPlant = PlantManager.Instance.SpawnPlantOnThisTile(slot.seed.plantPrefab, tile);

        if (spawnedPlant != null) //if we manage to create the plant from the seed
        {
            playerInventory.RemoveSeedFromSlot(slotIndex, 1);
            return true;
        }

        return false;
    }


}
