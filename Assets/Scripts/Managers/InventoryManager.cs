using Assets.Scripts.Environment;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonManager<InventoryManager>
{
    //public static InventoryManager Instance = null; // we dont need this anymore since we inherit from SingletonManager

    [SerializeField, ReadOnly] public Inventory playerInventory;  


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
