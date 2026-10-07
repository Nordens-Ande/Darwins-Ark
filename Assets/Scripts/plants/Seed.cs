using UnityEngine;


/// <summary>
/// this script is used for every seed, it is what every seed needs
/// </summary>


public class Seed
{

    [Header("Basic Info")]
    [SerializeField] public string seedName = "name";
    [SerializeField] private Sprite icon; // the picture in the inventory
    [SerializeField] private string description;
    [SerializeField, ReadOnly] private int seedRarity = 100; // a value that can be calulated somehow and used to decide backgorund color in ui
    [SerializeField] private int maxStackSize = 99; // how many of one seed we can have in one slot
    [Space]




    [Header("Planting Reference")]
    public Plant plantPrefab; // Prefaben från PlantManager/Plant som ska gro




    // getters and setters
    public string SeedName => seedName;
    public Sprite Icon => icon;
    public string Description => description;
    public int MaxStackSize => maxStackSize;





}
