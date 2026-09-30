using UnityEngine;


/// <summary>
/// this script is what each sot in the inventory has
/// </summary>


[System.Serializable]
public class InventorySlot
{
    public Seed seed;
    public int count;

    public InventorySlot(Seed seed, int count)
    {
        this.seed = seed;
        this.count = count;
    }

    public bool IsEmpty
    {
        get
        {
            if (seed == null || count <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }



    public void Clear()
    {
        seed = null;
        count = 0;
    }
}