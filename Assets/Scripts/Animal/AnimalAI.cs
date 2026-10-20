using Assets.Scripts.Environment;

using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AnimalAI : MonoBehaviour
{
    [Header("Utility points")]
    [Space]
    //Utility points
    [SerializeField] private float hunger = 0; //0-100
    [SerializeField] private float bossThreat = 0; //0-100
    [SerializeField] private float Happiness = 75; //0-100
    [SerializeField] private float matingSeason = 0; // 0-100
    [SerializeField] private float tired = 0; // 0-100

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
    [SerializeField, ReadOnly] private float clockCycleTimeCurrent;

    [SerializeField] private float sleepTimeMax = 10;
    [SerializeField, ReadOnly] private float currentSleepTime = 0;

    [Header("Boss")]
    [Space]
    //bossFight
    [SerializeField] private float attackDistance = 3;

    //Seeds that animal can spawn
    [Header("Seeds that can spawn")]
    [SerializeField] private List<GameObject> poop;

    [Header("DebugMode")]
    [SerializeField] private bool TestMutation = false;

    //Manager instances 
    private TileManager tileManager;
    private MutationManager mutationManager;


    //Traversing
    private Vector3 walkPoint;
    private bool hasSetPath = false;

    //Find plant
    private int tileCheckSize = 2; //How many tiles animal should see plant
    private Tile currentTile;
    private Plant choosenPlant = null;

    //Idle
    private bool isIdle = false;

    //Mutations
    private bool hasMutated = false;
    private bool isMutated = false;

    //MatingSeason
    private bool hasMate = false;
    private bool hasProcreated = false;
    private bool awaitMate = false;
    private AnimalAI assignedMate = null;

    //Utility AI
    private UtilityBrain<AnimalAI> utilityBrain;
    private List<UtilityAction<AnimalAI>> utilityChoosesList;

    //Actions for utility AI
    System.Action bossFightAction;
    System.Action procreateAction;
    System.Action leaveIslandAction;
    System.Action searchForFoodAction;
    System.Action walkAroundAction;
    System.Action sleepAction;

    //Properties
    public float BossThreat
    {
        get => bossThreat;
        set => bossThreat = value;
    }

    public bool HasMutated 
    { 
        get => hasMutated;
        set=> hasMutated = value;
    }

    public float WalkSpeed 
    {
        get => walkSpeed;
        set => walkSpeed = value;
    }

    public float RunSpeed 
    {
        get => runSpeed;
        set => runSpeed = value;   
    }

    public float DMG
    {
        get => damage;
        set => damage = value;
    }

    public float Health
    {
        get => health;
        set => health = value;
    }
    public float HungerDeteration
    {
        get => hungerDeteration;
        set => hungerDeteration = value;    
    }

    public float MatingSeason
    {
        get => matingSeason;
        set => matingSeason = value;
    }

    public float Tired 
    {
        get => tired;
        set => tired = value;
    }

    public float Hunger 
    {
        get => hunger;
        set => hunger = value;
    }

    public AnimalAI AssingedMate 
    {
        get => assignedMate;
        set => assignedMate = value;
    }

    public bool HasMate 
    { 
        get => hasMate;
        set => hasMate = value;
    }
    public bool AwaitMate 
    {
        get => awaitMate;
        set => awaitMate = value;
    }

    public bool HasProcreated 
    { 
        get=> hasProcreated;
        set => hasProcreated = value;
    }

    void Start()
    {
        tileManager = TileManager.Instance;
        mutationManager = MutationManager.instance;
        maxAttackTime = 2;
        tileCheckSize = 1;
        BossThreat = 0;
        matingSeason = 0;

        InstantiateUtilityBrain();
    }

    void Update()
    {
        utilityBrain.DecisionProcess();
        ClockCycleDeteriation();
    }

    //Creates and feeds all the possible action in relation to its value to the utility brain
    void InstantiateUtilityBrain() 
    {
        //Utility action defined
        bossFightAction = () => { BossFigth(); };
        procreateAction = () => { Procreate(); };
        leaveIslandAction = () => { LeaveIsland(); };
        searchForFoodAction = () => { SearchForFood(); };
        walkAroundAction = () => { WalkAround(); };
        sleepAction = () => {Sleep(); };

        //UtilityActions with its containers 
        UtilityAction<AnimalAI> bossAction = new UtilityAction<AnimalAI>(new BossValueContainer(), bossFightAction);
        UtilityAction<AnimalAI> walkAction = new UtilityAction<AnimalAI>(new WalkAroundContainer(), walkAroundAction);
        UtilityAction<AnimalAI> leaveAction = new UtilityAction<AnimalAI>(new LeaveContainer(), leaveIslandAction);
        UtilityAction<AnimalAI> hungerAction = new UtilityAction<AnimalAI>(new HungryContainer(), searchForFoodAction);
        UtilityAction<AnimalAI> tiredAction = new UtilityAction<AnimalAI>(new TiredContainer(), sleepAction);
        utilityChoosesList = new List<UtilityAction<AnimalAI>> { bossAction, walkAction, leaveAction, hungerAction, tiredAction };

        utilityBrain = new UtilityBrain<AnimalAI>(utilityChoosesList, this);

    }

    //Simple deteriation of the utility points during game time
    void ClockCycleDeteriation() 
    {
        clockCycleTimeCurrent += Time.deltaTime;
        if (clockCycleTimeCurrent > clockCycleTime) 
        {
            hunger += hungerDeteration;
            tired += 2;

            Happiness = Happiness - hunger / 10;

            clockCycleTimeCurrent = 0;

            if(hunger > 60) 
            {
                Happiness++;
            }

            //Testing if the mutations work
            if (TestMutation)
            {
                mutationManager.MutateAnimal_RandomMutation(this);
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
        currentMutationValue += MutationRate * Time.deltaTime;
        if(currentMutationValue > MutationMax) 
        { 
            isMutated = true;
            mutationManager.MutateAnimal_RandomMutation(this);
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
        if (tileManager.chunks.Count > 0)
        {
            //New solution that takes in all possible grasspaths, instead of having to rekursion find another walkpoint
            //Now the issue seems to be fixed 
            //Might be better to have this as a standalone method that updates everytime the island does, right now this updates every time the animal chooses a new path
            List<Vector2Int> tileList = new List<Vector2Int>();

            foreach (Chunk chunk in tileManager.chunks) 
            {
                List<Vector2Int> temp = chunk.GetTiles(TileType.Grass).ToList();
                if(temp.Count != 0) 
                { 
                    tileList.AddRange(temp);
                }
            }
            int randomPos = Random.Range(0, tileList.Count);
            Vector2Int choosenPosition = tileList[randomPos];
            Tile choosenTile = tileManager.GetTile(choosenPosition);
            Vector3 point = new Vector3(choosenTile.position.x, choosenTile.position.y + transform.localScale.y / 2, choosenTile.position.z);
            return point;


            //Old solution
            //Chunk choosenChunk = tileManager.chunks[Random.Range(0, tileManager.chunks.Count)];
            ////Vector2Int[] choosenTiles = choosenChunk.GetTiles(TileType.Grass).ToArray();
            //Vector2Int[] choosenTiles = choosenChunk.GetTiles(TileType.Grass).ToArray<Vector2Int>();
            //Debug.LogWarning(choosenTiles.Length);

            //if (choosenTiles == null || choosenTiles.Length > 0)
            //{
            //    //Try again
            //    return FindWalkPoint(); //fixed
            //}
            //else
            //{
            //    int randomPos = Random.Range(0, choosenTiles.Length - 1);
            //    Vector2Int choosenPosition = choosenTiles[randomPos];
            //    Tile choosenTile = tileManager.GetTile(choosenPosition);
            //    Vector3 point = new Vector3(choosenTile.position.x, choosenTile.position.y + transform.localScale.y / 2, choosenTile.position.z);
            //    return point;
            //}
        }
        else 
        {
            return new Vector3(0, 0, 0);
        }
    }

    //Checks so the animals dosent collide
    bool WillCollideWithNeighbour() 
    { 
        if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 1)) 
        {
            AnimalAI animal =hit.transform.GetComponent<AnimalAI>();
            if (animal == null)
            {
                return false;
            }
            else 
            {
                transform.position += Vector3.right * Time.deltaTime;
                hit.transform.position += Vector3.left * Time.deltaTime;
                return true;
            }
        }
        else return false;
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
            //AnimalWalkMoveTowards(walkPoint);

            if (Vector3.Distance(transform.position, walkPoint) < 2f)
            {
                hasSetPath = false;
                isIdle = true;
                Idle();
            }

            if (!WillCollideWithNeighbour())
            {
                AnimalWalkMoveTowards(walkPoint);
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
        if(currentTile == tileManager.GetTile(transform.position)) 
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
                Tile tile = tileManager.GetTile(position);
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
            currentTile = tileManager.GetTile(transform.position);
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
        if (Vector3.Distance(transform.position, leaveVec) < 3)
        {
            Debug.LogWarning("Im out of here: ", this);
            Destroy(gameObject);
        }

    }
    //Prototype can only handle one enemy in the sceen, can be changed later
    void BossFigth() 
    {
        if (health > 25)
        {
            currentAttackTime += Time.deltaTime;
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

    //Overload incase of wanting to play VFX on the animal when they stand still
    void StandStill(ParticleSystem VFX) 
    { 
    }

    void Sleep() 
    {
        StandStill();
        currentSleepTime += Time.deltaTime;
        if (currentSleepTime > sleepTimeMax)
        {
            tired = 0;
            currentSleepTime = 0;
        }
    }


    //Future method for offspring and procreation

    //Can be called in the animalManager to check mating status of every animal
    public bool CanMate() 
    { 
        if(hasProcreated || awaitMate || hasMate) 
        { 
            return false;
        }
        else return true;
    }

    //Can be called to assign mates in the animalManager, choices over animal procreation maybe chall be called in animalManager
    public void AssignMate(AnimalAI animal) 
    {
        hasMate = true;
        awaitMate = true;
        assignedMate = animal;
        animal.HasMate = true;
        animal.AssingedMate = this;
        animal.AwaitMate = false;
    } 

    //First draft over the procreate method
    void Procreate() 
    {
        if (hasMate && !awaitMate)
        {
            if (assignedMate == null) return;
            walkPoint = assignedMate.transform.position;
            float distance = Vector3.Distance(transform.position, assignedMate.transform.position);

            if (distance < 2)
            {
                //Instantiate new animal
                matingSeason = 0;
                hasProcreated = true;
                assignedMate.MatingSeason = 0;
                assignedMate.HasProcreated = true;
                assignedMate = null;
            }
            WalkAround();
        }
        else if (hasProcreated) matingSeason = 0;
        else if (awaitMate) StandStill();
        else WalkAround();

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

        //Makes sure the animal stays at the tileheight its on
        Vector2 currentPostion = new Vector2((int)transform.position.x,(int) transform.position.z);
        Tile currentTile = tileManager.GetTile(currentPostion);
        //Vector3 newPoint = new Vector3(point.x, currentTile.position.y + transform.localScale.y / 2, point.z);
        Vector3 newPoint = new Vector3(point.x, currentTile.GetHeight() + transform.localScale.y / 2, point.z);
        transform.position = Vector3.MoveTowards(transform.position, newPoint, walkSpeed * Time.deltaTime);

        //Makes sure the animal moves fast enough to the top of the tile, otherwise it will move through the tile

        if (transform.position.y < currentTile.GetHeight() + transform.localScale.y / 2)
        {
            transform.position += new Vector3(0, 0.5f, 0) * Time.deltaTime;
        }
        else if (transform.position.y > currentTile.GetHeight() + transform.localScale.y / 2)
        {
            transform.position -= new Vector3(0, 0.5f, 0) * Time.deltaTime;
        }
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

    //For later standstill when deity to call it
    public void InvokeStandStill() 
    {
        StandStill();
    }
}
