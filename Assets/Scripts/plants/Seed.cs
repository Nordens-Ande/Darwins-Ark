using UnityEngine;


/// <summary>
/// this script is used for every seed, it is what every seed needs
/// </summary>


public class Seed : MonoBehaviour
{

    [Header("Basic Info")]
    [SerializeField] public string seedName = "name";
    [SerializeField] Sprite icon; //the picture in the inventory
    [SerializeField] string description;
    [SerializeField] int maxStackSize = 99; //how many of one seed we can have in one slot
    [Space]

    [Header("Planting Reference")]
    public Plant plantPrefab; // Prefaben från PlantManager/Plant som ska gro




    //getters and setters
    public string SeedName 
    { 
        get 
        { 
            return seedName; 
        } 
    }

    public Sprite Icon
    {
        get
        {
            return icon;
        }
    }

    public string Description
    {
        get
        {
            return description;
        }
    }

    public int MaxStackSize
    {
        get
        {
            return maxStackSize;
        }
    }
}
