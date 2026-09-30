using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField, ReadOnly] int maxSlots = 16;
    [SerializeField, ReadOnly] List<InventorySlot> slots = new List<InventorySlot>();



    private void Awake()
    {
        //we init the inventory
        if (slots.Count != maxSlots)
        {
            slots.Clear();
            for (int i = 0; i < maxSlots; i++)
            {
                slots.Add(new InventorySlot(null, 0));
            }
        }
    }


    //true if the full amount we wanted to add could be added, else false
    public bool AddSeed(Seed seedToAdd, int amount = 1)
    {

        //we try to add the seed to an existing slot that has the same seed
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.seed == seedToAdd)
            {
                int spaceLeftInStack = seedToAdd.MaxStackSize - slot.count;
                if (spaceLeftInStack > 0)
                {
                    int amountToAdd = Mathf.Min(spaceLeftInStack, amount);
                    slot.count += amountToAdd;
                    amount -= amountToAdd;

                    if (amount <= 0)
                    {
                        return true;
                    }
                }
            }
        }

        //if there is seeds left or there was no matching seed already there
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                int amountToAdd = Mathf.Min(seedToAdd.MaxStackSize, amount);
                slot.seed = seedToAdd;
                slot.count = amountToAdd;
                amount -= amountToAdd;

                if (amount <= 0)
                {
                    return true;
                }
            }
        }

        //if the inventory is full
        return amount == 0;

    }


    //returns true if we were able to remove the seed, otherwise false
    public bool RemoveSeedFromSlot(int slotIndex, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            Debug.Log("You tried to remove a seed from a slotindex that doesn't exsit");
            return false;
        }

        InventorySlot slot = slots[slotIndex];
        if (slot.IsEmpty || slot.count < amount)
        {
            Debug.Log("You tried to remove a seed from an empty slot, or more seeds than that slot had");
            return false;
        }

        slot.count -= amount;
        if (slot.count <= 0)
        {
            slot.Clear();
        }



        return true;
    }








    //getters and setters
    public List<InventorySlot> Slots
    {
        get 
        { 
            return slots;
        }
    }

    public int MaxSlots
    {
        get
        {
            return maxSlots;
        }
    }



}
