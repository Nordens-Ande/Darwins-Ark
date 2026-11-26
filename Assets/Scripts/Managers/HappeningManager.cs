using Assets.Scripts.Environment;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.VFX;

public class HappeningManager : SingletonManager<SoundManager>
{
    //This class will handle small events to challange the player, they will be more frequent then biomechange
    //The class utilizes a FSM structure for the events and are triggered by a internal clockcycle

    [Header("Clock")]
    [SerializeField, ReadOnly] private float internalClockCycle;
    [SerializeField, ReadOnly] private float currentClockCycle;
    [SerializeField, ReadOnly] private float clockSpeed;
    [Header("Set interval between happenings in minutes")]
    [SerializeField] private int setIntervalSpeed; //Will be used instead of random intervals 
    [Header("Randomness in timer interval in minutes")]
    [SerializeField] private bool RandomHappeningInterval; //If random interval is true then there is a ranom minute interval between happenings
    [SerializeField] private int minRandomInterval;
    [SerializeField] private int maxRandomInterval;
    private bool HasCurrentHappening = false;
    [Header("Bird attack")]
    [SerializeField] private GameObject birdPrefab;
    [SerializeField] private float birdHealth;
    [SerializeField] private int birdAmount;
    [Header("Fog of war")]
    [SerializeField] private GameObject particleEffect;
    [SerializeField] private float intensity;
    [SerializeField] private float leangth;
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
        PopulateHappenings(possibleHappinings);
        HasCurrentHappening = false;
        internalClockCycle = setIntervalSpeed; // every five minutes
    }

    // Update is called once per frame
    void Update()
    {
        ClockCycle();
    }

    void ClockCycle() 
    {
        if (!HasCurrentHappening)
        {
            currentClockCycle += clockSpeed * Time.deltaTime;
            if (currentClockCycle > internalClockCycle)
            {
                SetNewHappening();
            }
        }
        else 
        {
            currentHappening.Update();
            currentHappening.Stop(HasCurrentHappening);
        }

    }

    void SetNewHappening() 
    {
        if (RandomHappeningInterval)
        {
            HasCurrentHappening = true;
            TriggerHappening();
            currentClockCycle = 0;
            int newClockCycleMax = UnityEngine.Random.Range(minRandomInterval * 60, maxRandomInterval * 60); // between 3min and 5min a happening will accure 
            internalClockCycle = newClockCycleMax;
        }
        else
        {
            HasCurrentHappening = true;
            TriggerHappening();
            currentClockCycle = 0;
        }
    }

    void TriggerHappening() 
    {
        if (possibleHappinings.Count > 0 && HasCurrentHappening)
        {
            int randomHappening = UnityEngine.Random.Range(0, possibleHappinings.Count);
            currentHappening = possibleHappinings[randomHappening];
            HasCurrentHappening = true;
        }
    }

    void PopulateHappenings(List<Happening> list) 
    {
        BirdAttack birdAttack = new BirdAttack(birdPrefab, birdAmount);
        FogOfWar fogOfWar = new FogOfWar(particleEffect, intensity, leangth);
        EarthQuake earthQuake = new EarthQuake(amountEffectedTiles);
        WildFire wildFire = new WildFire(effectedPlants, fireParticleEffect);
        Drought drought = new Drought(plantsWithDrought, droughtEffect);
        list.Add(birdAttack);
        list.Add(fogOfWar);
        list.Add(earthQuake);
        list.Add(wildFire);
        list.Add(drought);
    }
}

//All possible happenings classes
public abstract class Happening : MonoBehaviour
{
    //Updates the logic of the happening
    public abstract void Update();

    //Sees if the logic is done and then reset so the happening is marked as complete
    public abstract void Stop(bool reset);
}

//Fogs the screen for harder vision
public class FogOfWar : Happening
{
    private GameObject particleEffect;
    private float intensity;
    private float leangth;
    private bool HasInstatiatedEffect = false;
    private float currentTime = 0;
    private bool Done;
    private GameObject instantiatedObj = null;

    public FogOfWar(GameObject particleEffect, float intensity, float leangth)
    {
        this.particleEffect = particleEffect;
        this.intensity = intensity;
        this.leangth = leangth;
    }

    public override void Update()
    {
        if (!HasInstatiatedEffect) 
        { 
            Camera cam = Camera.main;
            GameObject effect = Instantiate(particleEffect, cam.transform);
            effect.transform.SetParent(cam.transform, false);
            HasInstatiatedEffect=true;
        }
        if (HasInstatiatedEffect) 
        {
            currentTime += Time.deltaTime;
            if (currentTime > leangth) 
            { 
                Done = true;
            }
        }
    }
    public override void Stop(bool reset)
    {
        if (Done) 
        {
            Destroy(instantiatedObj.gameObject);
            reset = false;
        }
    }
}

//Changes the tiles height
public class EarthQuake : Happening
{
    private int amountEffectedTiles;
    private List<Tile> effectedTiles = new List<Tile>();
    public EarthQuake(int amountEffectedTiles) 
    { 
        this.amountEffectedTiles = amountEffectedTiles;
    }
    public override void Update()
    {
        TileManager tilemanager = TileManager.Instance;
        VFXManager vfx = VFXManager.Instance;
        float time = 0.5f;
        float intensity = 1f;
        vfx.ShakeCamera(time, intensity); //Shake the camare to give simulation that the ground shakes
        StartCoroutine(EarthQuakeModify(tilemanager, effectedTiles)); 
    }

    //Enumarator for earthquake to have a little delay between camera shake and modification of tiles
    IEnumerator EarthQuakeModify(TileManager tilemanager,List<Tile> effectedTiles) 
    { 
        //The delay should allow the camera shake to start to give feeling that camera shakes first and then followed by ground shift
        yield return new WaitForSeconds(0.3f);
        foreach (Chunk chunk in tilemanager.chunks)
        {
            foreach (Tile tile in chunk.Tiles)
            {
                if (tile.Type == TileType.Grass)
                {
                    System.Random random = new System.Random();
                    int frequency = random.Next(0, 10);
                    if (effectedTiles.Count < amountEffectedTiles && frequency > 7)
                    {
                        effectedTiles.Add(tile);
                        chunk.ModifyTile((int)tile.Position.x, (int)tile.Position.z, UnityEngine.Random.Range(-2, 2));
                    }
                }
            }
        }
    }

    //Should stop immediatly after finishing so it will just execute then stop
    public override void Stop(bool reset)
    {
        reset = false;
    }
}

//Instantiates birds with logic to steal plants and hurt animals
public class BirdAttack : Happening
{
    private GameObject birdPrefab;
    private int birdAmount;
    private bool started = false;
    private Vector3 startPos;
    private List<GameObject> birdList = new List<GameObject>();

    public BirdAttack(GameObject birdPrefab, int birdAmount)
    {
        this.birdPrefab = birdPrefab;
        this.birdAmount = birdAmount;
    }

    public override void Update()
    {
        if (!started) 
        { 
            for(int i = 0; i < birdAmount; i++) 
            { 
                GameObject bird = Instantiate(birdPrefab, startPos, Quaternion.identity);
                birdList.Add(bird);
            }
            started = true;
        }
    }

    public override void Stop(bool reset)
    {
        if(birdList.Count == 0) 
        {
            reset = false;
        }
    }
}

//Effects the plants and damages them over time if there isnet rain on them
public class WildFire : Happening
{
    private int effectedPlants;
    private GameObject fireEffect;
    private bool canStart = false; 
    private List<Plant> effectedPlantsList = new List<Plant>();
    //private List<GameObject> effectsList = new List<GameObject>();
    private Dictionary<Plant, GameObject> EffectsDictonary = new Dictionary<Plant, GameObject>();
    private float currentTime = 0;
    private float deterioationTime = 10;

    public WildFire(int effectedPlants, GameObject fireEffect)
    { 
        this.effectedPlants = effectedPlants;
        this.fireEffect = fireEffect;
    }

    public override void Update()
    {
        if (!canStart) 
        {
            PlantManager plant = PlantManager.Instance;
            foreach(Plant activePlant in plant.Plants) 
            {
                if (activePlant.CanBeEaten && effectedPlants > effectedPlantsList.Count) 
                { 
                    effectedPlantsList.Add(activePlant);
                }

                if (effectedPlantsList.Count > 0) 
                {
                    GameObject effect = Instantiate(fireEffect, activePlant.transform);
                    EffectsDictonary.TryAdd(activePlant, effect);
                }
            }
        }
        if (canStart) 
        {
            currentTime += Time.deltaTime;
            if (currentTime > deterioationTime) 
            { 
                foreach(Plant plant in effectedPlantsList) 
                {
                    if (plant.IsCloseToWater) //Is this how rain was assured?
                    {
                        EffectsDictonary.TryGetValue(plant, out var obj);
                        if (obj != null) 
                        {
                            Destroy(obj.gameObject);
                            EffectsDictonary.Remove(plant);
                        }
                        effectedPlantsList.Remove(plant);
                    }

                    if(plant == null) 
                    { 
                        effectedPlantsList.Remove(plant);
                    }
                    plant.DamagePlant(10f);
                }
                currentTime = 0;
            }
        }

    }
    public override void Stop(bool reset)
    {
        if(effectedPlantsList.Count == 0) 
        {
            foreach(Plant plant in effectedPlantsList) 
            {
                EffectsDictonary.TryGetValue(plant, out var obj);
                Destroy(obj.gameObject);
            }
            effectedPlantsList.Clear();
            EffectsDictonary.Clear();
            reset = false;
        }
        reset = false;
    }
}

//Effects plants so you need more water for each plant
public class Drought : Happening
{
    private int plantsWithDrought;
    private float droughtEffect;
    private bool start = false;
    private float internalTimerMax = 240; //4 minutes
    private float currentTimer = 0; 

    public Drought(int plantWithDrought, float droughtEffect)
    {
        this.droughtEffect = droughtEffect;
        this.plantsWithDrought = plantWithDrought;
    }

    public override void Update()
    {
        if (!start) 
        { 
            PlantManager plantmanager = PlantManager.Instance;
            foreach(Plant plant in plantmanager.Plants) 
            {
                plant.IsCloseToWaterRange = 0.1f;
            }
        }
        currentTimer += Time.deltaTime;
      
    }
    public override void Stop(bool reset)
    {
        if (currentTimer > internalTimerMax)
        {
            PlantManager plantman = PlantManager.Instance;
            foreach(Plant p in plantman.Plants) 
            {
                p.IsCloseToWaterRange = p.InitialValues.IsCloseToWaterRange;
            }
            reset = false;
        }
    }
}


