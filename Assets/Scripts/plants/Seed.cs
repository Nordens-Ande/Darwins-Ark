using UnityEngine;


/// <summary>
/// this script is used for every seed, it is what every seed needs
/// </summary>


[CreateAssetMenu(fileName = "NewSeed", menuName = "Inventory/Seed")] // we cab access this in the unity menu
public class Seed : ScriptableObject
{

    [Header("Basic Info")]
    [SerializeField] public string seedName = "name";
    [SerializeField] private Sprite icon; // the picture in the inventory
    [SerializeField] private string description;
    [SerializeField, ReadOnly] private int seedRarity = 100; // a value that can be calulated somehow and used to decide backgorund color in ui
    [SerializeField] private int maxStackSize = 99; // how many of one seed we can have in one slot
    [Space]

    [Header("Planting Reference")]
    public Plant plantPrefab; // Prefaben from PlantManager, the Plant which will grow from this seed


    // getters and setters
    public string SeedName => seedName;
    public Sprite Icon => icon;
    public string Description => description;
    public int MaxStackSize => maxStackSize;





}
