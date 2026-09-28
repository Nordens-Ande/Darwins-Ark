using Assets.Scripts.Environment;
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

    [Header("Animal Stats")]
    [Space]
    //Animal stats
    [SerializeField] private float walkSpeed = 1f;
    [SerializeField] private float runSpeed = 3;
    [SerializeField] private float damage = 10;
    [SerializeField] private float health = 100; //0-100
    [SerializeField] private float hungerDeteration = 1;

    [Header("Timers")]
    [Space]
    //InternalTimer
    [SerializeField] private float maxIdleTime = 5f;
    [SerializeField] private float currentIdleTime = 0;

    [SerializeField] private float maxAttackTime = 2;
    [SerializeField] private float currentAttackTime = 0;

    [SerializeField] private float clockCycleTime = 10; //How long before points deteriate
    [SerializeField] private float clockCycleTimeCurrent;

    //Traversing
    private Vector3 walkPoint;
    private bool hasSetPath = false;
    private int minIslandSize = 0;
    private int maxIslandSize = 20;

    [Header("Boss")]
    [Space]
    //bossFight
    [SerializeField] private float attackDistance = 3;

    
    //[Header("Happiness indicator")]
    //[Space]
    ////DisplayHappiness
    //[SerializeField] private Color colorHappy;
    //[SerializeField] private Color colorIndiferent;
    //[SerializeField] private Color colorUnhappy;
    //private Transform happinessMeter;
    //private Renderer currentColor;
    //[Range(0f,1f)]
    //[SerializeField] private float colorTransparancy;

    private bool isIdle = false;

    Plant choosenPlant = null;

    private bool hasMutated = false;

    [Header("DebugMode")]
    [SerializeField] private bool TestMutation = true;

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



    void Start()
    {
        maxAttackTime = 2;
        //happinessMeter = gameObject.transform.GetChild(0);

        ////Guard if gameobject dosent have sphere attacted then it will create one
        //if(happinessMeter == null) 
        //{ 
        //    GameObject happinessSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    happinessSphere.transform.position = new Vector3 (transform.position.x, transform.position.y + 1, transform.position.z);
        //    happinessSphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        //    happinessSphere.transform.SetParent(transform);
        //    happinessMeter = happinessSphere.transform;
        //}

        //currentColor = happinessMeter.GetComponent<Renderer>();
        //DisplayHappiness();
        //colorHappy.a = colorTransparancy;
        //colorIndiferent.a = colorTransparancy;
        //colorUnhappy.a = colorTransparancy;
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
            //hunger++;
            hunger += hungerDeteration;
            Happiness = Happiness - hunger;
            clockCycleTimeCurrent = 0;
            //DisplayHappiness();

            //Testing if the mutations work
            if (TestMutation)
            {
                MutationManager.instance.MutateAnimal_RandomMutation(this);
            }
        }
    }

    void CheckBestAction() 
    {
        if (bossThreat == 1)
        {
            BossFigth();
        }
        else if(Happiness < 0) 
        { 
            LeaveIsland();
        }
        else if (hunger > 50)
        {
            SearchForFood();
        }
        else 
        {
            WalkAround();
        }
    }

    //Will show animals happines dynamicly and change it during runtime. Will be called for optimazation in start and clockcycleDeteriation 
    //void DisplayHappiness() 
    //{
    //    if(Happiness > 50) 
    //    {
    //        currentColor.material.color = Color.Lerp(colorIndiferent, colorHappy, (Happiness-50) / 50);
    //    }
    //    else 
    //    {
    //        currentColor.material.color = Color.Lerp(colorIndiferent, colorUnhappy, Happiness/100/0.5f);
    //    }
        
    //}
    
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

    //Check surroundings for food, makes sure the animals still moves if it dosent find any
    void SearchForFood() 
    {
        WalkAround();

        if (choosenPlant = null)
        {
            Collider[] collliders = Physics.OverlapSphere(transform.position, 5);
            foreach (Collider hit in collliders)
            {
                Plant plant = hit.gameObject.GetComponent<Plant>();
                if (plant != null && plant.CanBeEaten)
                {
                    walkPoint = hit.transform.position;
                    choosenPlant = plant;
                    hasSetPath = true;
                    return;
                }
            }
        }
        else
        {
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

    //Make raycast check here to see if plant is in reach
    }

    //eatplant 
    void EatFood(Plant plant) 
    {
        plant.EatPlant();
        hunger = 0;
        //Destroy(plant.gameObject);
    }


    //Basiclly set a random walkpoint
    //Vector3 FindWalkPoint() 
    //{ 
    //    Vector3 point = new Vector3 (Random.Range(minIslandSize, maxIslandSize), transform.localScale.y / 2, Random.Range(minIslandSize, maxIslandSize));
    //    return point;
    //}

    //Updated movement to chunk logic / will need modification if we want something other then square chunks so animals dont hoover between chunks 
    Vector3 FindWalkPoint()
    {
        if(TileManager.Instance.chunks.Count > 0) 
        {
            Chunk choosenChunk = TileManager.Instance.chunks[Random.Range(0, TileManager.Instance.chunks.Count)];
            Tile choosenTile = choosenChunk.Tiles[Random.Range(0, TileManager.Instance.ChunkSize), Random.Range(0,TileManager.Instance.ChunkSize)];
            Vector3 point = new Vector3(choosenTile.position.x, choosenTile.position.y + transform.localScale.y / 2, choosenTile.position.z);
            return point;
        }
        return new Vector3(0,0,0);
    }

    //Will walk to corner and die if unhappy
    void LeaveIsland() 
    {
        Vector3 leaveVec = new Vector3(0, 0, 0);
        AnimalWalkMoveTowards(leaveVec);
        if(Vector3.Distance(transform.position, leaveVec) < 3) 
        {
            Debug.Log("Im out of here");
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

    //Will spawn plantseeds
    void Poop() 
    { 
        //Instantiateseed
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

    //If the map changes the animalmovment will registrer it
    public void MapChange_SetNewMinMax(int min, int max) 
    {
        minIslandSize = min;
        maxIslandSize = max;
    }

    // Makes happiness public for AnimalMoodIndicator.cs
    public float GetHappiness()
    {
        return Happiness;
    }
}
