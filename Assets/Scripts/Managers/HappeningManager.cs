using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class HappeningManager : SingletonManager<SoundManager>
{
    //This class will handle small events to challange the player, they will be more frequent then biomechange
    //The class utilizes a FSM structure for the events and are triggered by a internal clockcycle

    [Header("Clock")]
    [SerializeField, ReadOnly] private float internalClockCycle;
    [SerializeField, ReadOnly] private float currentClockCycle;
    [SerializeField, ReadOnly] private float clockSpeed;


    private bool HasCurrentHappening = false;

    [Header("Bird attack")]
    [SerializeField] private GameObject birdPrefab;
    [SerializeField] private float birdHealth;

    [Header("Fog of war")]
    [SerializeField] private GameObject particleEffect;
    [SerializeField] private float intensity;

    [Header("EarthQuake")]
    [SerializeField] int amountEffectedTiles;

    [Header("Wild fire")]
    [SerializeField] int effectedPlants;
    [SerializeField] GameObject fireParticleEffect;

    [Header("Drought")]
    [SerializeField] private int plantsWithDrought;
    [SerializeField] private float droughtEffect;


    private List<Happening> possibleHappinings = new List<Happening>();
    private Happening currentHappening = null;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ClockCycle();
        currentHappening.Update();
        currentHappening.Reset(HasCurrentHappening);
    }

    void ClockCycle() 
    {
        if (!HasCurrentHappening)
        {
            currentClockCycle += clockSpeed * Time.deltaTime;
            if (currentClockCycle > internalClockCycle)
            {
                HasCurrentHappening = true;
                TriggerHappening();
            }
        }

    }

    void TriggerHappening() 
    {
        if (possibleHappinings.Count > 0 && HasCurrentHappening)
        {
            int randomHappening = Random.Range(0, possibleHappinings.Count);
            currentHappening = possibleHappinings[randomHappening];
        }
    }
}

//All possible happenings classes
public abstract class Happening 
{
    //Updates the logic of the happening
    public abstract void Update();

    //Sees if the logic is done and then reset so the happening is marked as complete
    public abstract void Reset(bool reset);
}

public class FogOfWar : Happening
{
    private GameObject particleEffect;
    private float intensity;

    public FogOfWar(GameObject particleEffect, float intensity)
    {
        this.particleEffect = particleEffect;
        this.intensity = intensity;
    }

    public override void Update()
    {

    }
    public override void Reset(bool reset)
    {
        reset = false;
    }
}
public class EarthQuake : Happening
{
    private int amountEffectedTiles;

    public EarthQuake(int amountEffectedTiles) 
    { 
        this.amountEffectedTiles = amountEffectedTiles;
    }
    public override void Update()
    {

    }
    public override void Reset(bool reset)
    {
        reset = false;
    }
}

public class BirdAttack : Happening
{
    private GameObject birdPrefab;
    private float birdHealth;

    public BirdAttack(GameObject birdPrefab, float birdHealth)
    {
        this.birdPrefab = birdPrefab;
        this.birdHealth = birdHealth;
    }

    public override void Update()
    {
    }

    public override void Reset(bool reset)
    {
        reset = false;
    }
}

public class WildFire : Happening
{
    private int effectedPlants;
    private GameObject fireEffect;

    public WildFire(int effectedPlants, GameObject fireEffect)
    { 
        this.effectedPlants = effectedPlants;
        this.fireEffect = fireEffect;
    }

    public override void Update()
    {

    }
    public override void Reset(bool reset)
    {
        reset = false;
    }
}
public class Drought : Happening
{
    private int plantsWithDrought;
    private float droughtEffect;
    public Drought(int plantWithDrought, float droughtEffect)
    {
        this.droughtEffect = droughtEffect;
        this.plantsWithDrought = plantWithDrought;
    }

    public override void Update()
    {

    }
    public override void Reset(bool reset)
    {
        reset = false;
    }
}


