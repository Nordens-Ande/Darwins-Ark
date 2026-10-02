using Assets.Scripts.Environment;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor.Rendering;
using UnityEngine;

public class AnimalAI : MonoBehaviour
{
    [Header("Utility points")]
    [Space]
    //Utility points
    [SerializeField] private float hunger = 0; //0-100
    [SerializeField] private float bossThreat = 0; //0-100
    [SerializeField] private float Happiness = 75; //0-100
    [SerializeField] private float matingSeason = 0;
    [SerializeField] private float tired = 0;

    [Header("Animal Stats")]
    [Space]
    //Animal stats
    [SerializeField] private float walkSpeed = 1f;
    [SerializeField] private float runSpeed = 3;
    [SerializeField] private float damage = 10;
    [SerializeField] private float health = 100; //0-100
    [SerializeField] private float hungerDeteration = 1;
    [SerializeField] private float MutationRate = 1;
    [SerializeField, ReadOnly] private float currentMutationValue = 0;
    [SerializeField] private float MutationMax = 100;
    [SerializeField] private float Defence = 1; //Do nothing at the moment


    [Header("Timers")]
    [Space]
    //InternalTimer
    [SerializeField] private float maxIdleTime = 5f;
    [SerializeField, ReadOnly] private float currentIdleTime = 0;

    [SerializeField] private float maxAttackTime = 2;
    [SerializeField, ReadOnly] private float currentAttackTime = 0;

    [SerializeField] private float clockCycleTime = 10; //How long before points deteriate
    [SerializeField,ReadOnly] private float clockCycleTimeCurrent;

    [Header("Boss")]
    [Space]
    //bossFight
    [SerializeField] private float attackDistance = 3;

    //Seeds that animal can spawn
    [Header("Seeds that can spawn")]
    [SerializeField] private List<GameObject> poop;

    [Header("DebugMode")]
    [SerializeField] private bool TestMutation = false;


    //Traversing
    private Vector3 walkPoint;
    private bool hasSetPath = false;

    //Find plant
    private int tileCheckSize = 2; //How many tiles animal should see plant
    private List<Tile> tileList;
    private Tile currentTile;
    private Plant choosenPlant = null;

    //Idle
    private bool isIdle = false;

    //Mutations
    private bool hasMutated = false;
    private bool isMutated = false;

    //MatingSeason
    private bool HasMate = false;
    private bool HasProcreated = false;
    private bool AwaitMate = false;

    //Properties
    public float BossThreat
    {
        get { return bossThreat; }
        set { bossThreat = value; }
    }

    public bool HasMutated 
    { 
        get {return hasMutated; }
        set { hasMutated = value; }
    }

    //Needed for the mutations 
    public float WalkSpeed 
    {
        get { return walkSpeed; }
        set { walkSpeed = value; }
    }

    public float RunSpeed 
    {
        get {return runSpeed; }
        set { runSpeed = value; }
    }

    public float DMG
    {
        get { return damage; }
        set { damage = value; }
    }

    public float Health
    {
        get { return health; }
        set { health = value; }
    }
    public float HungerDeteration
    {
        get {return hungerDeteration; }
        set { hungerDeteration = value; }
    }

    public float MatingSeason
    {
        get { return matingSeason; }
        set {  matingSeason = value; }
    }



    void Start()
    {
        maxAttackTime = 2;
        tileCheckSize = 1;
        BossThreat = 0;
        matingSeason = 0;
    }

    void Update()
    {
        CheckBestAction();
        ClockCycleDeteriation();

        currentAttackTime += Time.deltaTime;
    }

    //Simple deteriation of the utility points during game time
    void ClockCycleDeteriation() 
    {
        clockCycleTimeCurrent += Time.deltaTime;
        if (clockCycleTimeCurrent > clockCycleTime) 
        {
            hunger += hungerDeteration;
            tired++;

            Happiness = Happiness - hunger / 2;

            clockCycleTimeCurrent = 0;

            //Testing if the mutations work
            if (TestMutation)
            {
                MutationManager.instance.MutateAnimal_RandomMutation(this);
            }
        }

        if (hasMutated && !isMutated) 
        { 
            CheckMutationRate();
        }
    }

    //Now animals mutation dosent happen instantly, this method checks if the timer is done and then mutates after the MutationRate hits the MutationMax
    void CheckMutationRate() 
    {
        currentMutationValue += MutationRate;
        if(currentMutationValue > MutationMax) 
        { 
            isMutated = true;
            MutationManager.instance.MutateAnimal_RandomMutation(this);
        }
    }

    void CheckBestAction() 
    {
        if (bossThreat == 1)
        {
            BossFigth();
        }
        else if (matingSeason == 1) 
        {
            Procreate();
        }
        else if (Happiness < 0)
        {
            LeaveIsland();
        }
        else if (hunger > 50)
        {
            SearchForFood();
        }
        else if (tired > 75) 
        { 
            
        }
        else
        {
            WalkAround();
        }
    }
    
    //Action methods
    void Idle() 
    {
        currentIdleTime += Time.deltaTime;
        if (currentIdleTime > maxIdleTime) 
        {
            currentIdleTime = 0;
            isIdle = false;
            WalkAround();
        }
    }

    //Updated movement to chunk logic / will need modification if we want something other then square chunks so animals dont hoover between chunks 
    Vector3 FindWalkPoint()
    {
        if (TileManager.Instance.chunks.Count > 0)
        {
            Chunk choosenChunk = TileManager.Instance.chunks[Random.Range(0, TileManager.Instance.chunks.Count)];
            Tile choosenTile = choosenChunk.Tiles[Random.Range(0, TileManager.Instance.ChunkSize), Random.Range(0, TileManager.Instance.ChunkSize)];
            Vector3 point = new Vector3(choosenTile.position.x, choosenTile.position.y + transform.localScale.y / 2, choosenTile.position.z);
            return point;
        }
        return new Vector3(0, 0, 0);
    }

    void WalkAround() 
    {
        if (isIdle) Idle();
        else 
        {
            if (!hasSetPath)
            {
                hasSetPath = true;
                walkPoint = FindWalkPoint();
            }

            AnimalWalkMoveTowards(walkPoint);

            if (Vector3.Distance(transform.position, walkPoint) < 2f)
            {
                hasSetPath = false;
                isIdle = true;
                Idle();
            }
        }
    }

    /// <summary>
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////// SEARCHING FOOD METHODS
    /// </summary>
    /// <returns></returns>

    //Checks if animal has changed tile
    bool HasAnimalMovedTile() 
    { 
        if(currentTile == TileManager.Instance.GetTile(transform.position)) 
        {
            return false;
        }
        else 
        {
            return true;
        }
    }

    //Getting the surronding tiles around the animal
    List<Tile> GetSurroundingTiles() 
    {
        List<Tile> tileList = new List<Tile>();
        for(int i = -tileCheckSize; i < tileCheckSize; i++) 
        {
            for (int y = -tileCheckSize; y < tileCheckSize; y++) 
            {
                Vector2 position = new Vector2(transform.position.x + i, transform.position.z + y);
                Tile tile = TileManager.Instance.GetTile(position);
                if(tile != null) 
                {
                    tileList.Add(tile);
                }
            }
        }
        return tileList;
    }

    //Checks if surrounding tiles have plants and then sends them to be evaluated
    void SurroundingPlants() 
    {
        if (GetSurroundingTiles().Count == 0) return;

        List<Plant> currentPlantChoices = new List<Plant>();

        foreach(Tile tile in GetSurroundingTiles()) 
        {
            if (!tile.HasPlant) continue;

            if(!tile.CurrentPlant.CanBeEaten) continue;

            Plant plant = tile.CurrentPlant;

            float distance = Vector3.Distance(transform.position, plant.transform.position);

            if (plant.PlantSmellRadiusValue > distance)
            {
                currentPlantChoices.Add(plant);
            }
        }
        if(currentPlantChoices.Count != 0) 
        {
            NearestPlantInList(currentPlantChoices);
        }
    }

    //Gets the plant that is closest to the animal
    void NearestPlantInList(List<Plant> currentPlantChoices) 
    {
        Plant optimalPlant = currentPlantChoices[0];
        float optimalDistance = Vector3.Distance(transform.position, optimalPlant.transform.position);

        foreach (Plant plant in currentPlantChoices)
        {
            float plantDistance = Vector3.Distance(transform.position, plant.transform.position);
            if (optimalDistance > plantDistance)
            {
                optimalPlant = plant;
                optimalDistance = plantDistance;
            }
        }

        SetPlantWalkPoint(optimalPlant);
    }

    //Sets the animal walkpoint to the plant and sets the current plant to be consumed
    void SetPlantWalkPoint(Plant plant) 
    {
        choosenPlant = plant;
        walkPoint = plant.transform.position;
        hasSetPath = true;
    }

    //Check surroundings for food, makes sure the animals still moves if it dosent find any
    void SearchForFood() 
    {
        //checks if animal have moved and sets this tile to the current
        if (choosenPlant == null && HasAnimalMovedTile())
        {
            SurroundingPlants();
            currentTile = TileManager.Instance.GetTile(transform.position);
        }
        else 
        {
            //If plant is already set then just walk towards it and check if its in eating range
            WalkAround();
            if (choosenPlant != null)
            {
                float distance = Vector3.Distance(transform.position, choosenPlant.transform.position);
                if (distance < 3)
                {
                    EatFood(choosenPlant);
                    hasSetPath = false;
                }
            }
        }
    }

    //eatplant 
    void EatFood(Plant plant) 
    {
        Debug.LogWarning("Has eaten", gameObject);
        choosenPlant = null;
        plant.EatPlant();
        hunger = 0;
        Poop();
    }

    //Will spawn plantseeds
    void Poop()
    {
        if (poop.Count > 0)
        {
            int choosenPoop = Random.Range(0, poop.Count);
            GameObject waste = Instantiate(poop[choosenPoop], transform.position, Quaternion.identity);
        }
    }


    //Will walk to corner and die if unhappy
    void LeaveIsland() 
    {
        Vector3 leaveVec = new Vector3(0, 0, 0);

        AnimalWalkMoveTowards(leaveVec);
        if(Vector3.Distance(transform.position, leaveVec) < 3) 
        {
            Debug.LogWarning("Im out of here");
            Destroy(gameObject);
        }
    }
    //Prototype can only handle one enemy in the sceen, can be changed later
    void BossFigth() 
    {
        if (health > 25)
        {
            Vector3 bossPosition = BossManager.Instance.CurrentBoss.transform.position;
            bossPosition.y = 0.5f;
            Vector3 directionToBoss = Vector3.Normalize(bossPosition - transform.position);
            
            transform.LookAt(bossPosition);
            transform.position = Vector3.MoveTowards(transform.position, bossPosition - directionToBoss, runSpeed * Time.deltaTime);
            float distanceToBoss = Vector3.Distance(bossPosition, transform.position);

            if(distanceToBoss < attackDistance)
            {
                if(currentAttackTime >= maxAttackTime)
                {
                    BossHealth bossHealth = BossManager.Instance.CurrentBoss.GetComponent<BossHealth>();
                    if(bossHealth != null)
                    {
                        bossHealth.TakeDamage(damage);
                        currentAttackTime = 0;
                    }
                }
            }
        }
    }

    //Method for making the animal stand in place
    void StandStill()
    {
        //Await the mate
        //Sleep
    }


    //Future method for offspring and procreation
    void Procreate() 
    {
        //Need the list from animalManager to get which animals that can procreate
        if (HasMate) WalkAround();
        if(HasProcreated) matingSeason = 0;
        if (AwaitMate) StandStill();
    }

    //Can be called to invoke bossfight
    public void SetThreatLevelMax() 
    {
        bossThreat = 1;
    }

    //Can be called to make animals have a set point
    public void AnimalWalkMoveTowards(Vector3 point) 
    {
        transform.LookAt(point);
        transform.position = Vector3.MoveTowards(transform.position, point, walkSpeed * Time.deltaTime);
    }

    //If we decide to have animals lose health
    public void TakeDMG(float amount) 
    { 
        health -= amount;
        if(health < 0) 
        {
            Happiness = 0;
        }
    }

    // Makes happiness public for AnimalMoodIndicator.cs
    public float GetHappiness()
    {
        return Happiness;
    }

    public void InvokeStandStill() 
    {
        StandStill();
    }
}
