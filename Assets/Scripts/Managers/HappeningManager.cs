using Assets.Scripts.Environment;
using System;
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
        
    }

    // Update is called once per frame
    void Update()
    {
        ClockCycle();
        currentHappening.Update();
        currentHappening.Stop(HasCurrentHappening);
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
            int randomHappening = UnityEngine.Random.Range(0, possibleHappinings.Count);
            currentHappening = possibleHappinings[randomHappening];
        }
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
    private bool canStart = false;

    public EarthQuake(int amountEffectedTiles) 
    { 
        this.amountEffectedTiles = amountEffectedTiles;
    }
    public override void Update()
    {
        if (!canStart) 
        {
            TileManager tilemanager = TileManager.Instance;
            foreach(Chunk chunk in tilemanager.chunks) 
            { 
                foreach(Tile tile in chunk.Tiles) 
                { 
                    System.Random random = new System.Random();
                    int frequency = random.Next(0, 10);
                    if(effectedTiles.Count < amountEffectedTiles && frequency > 7) 
                    { 
                        effectedTiles.Add(tile);
                    }
                    
                }
            }
            canStart = true;
        }
        if (canStart) 
        { 
            foreach (Tile tile in effectedTiles) 
            {
                //Modify y axis
            }
        }
    }
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
    public Drought(int plantWithDrought, float droughtEffect)
    {
        this.droughtEffect = droughtEffect;
        this.plantsWithDrought = plantWithDrought;
    }

    public override void Update()
    {

    }
    public override void Stop(bool reset)
    {
        reset = false;
    }
}


