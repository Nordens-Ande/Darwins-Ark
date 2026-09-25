using Unity.IO.LowLevel.Unsafe;
using UnityEditor.Rendering;
using UnityEngine;

public class AnimalAI : MonoBehaviour
{
    //Utility points
    [SerializeField] private float hunger = 0; //0-100
    [SerializeField] private float bossThreat = 0; //0-100
    [SerializeField] private float Happiness = 75; //0-100

    //Animal stats
    [SerializeField] private float walkSpeed = 0.1f;
    [SerializeField] private float runSpeed = 2;
    [SerializeField] private float damage = 10;
    [SerializeField] private float health = 100; //0-100

    //InternalTimer
    [SerializeField] private float maxIdleTime = 5f;
    [SerializeField] private float currentIdleTime = 0;

    [SerializeField] private float clockCycleTime = 10; //How long before points deteriate
    [SerializeField] private float clockCycleTimeCurrent;

    //Traversing
    private Vector3 walkPoint;
    private bool hasSetPath = false;
    private int minIslandSize = 0;
    private int maxIslandSize = 20;

    //bossFight
    private float attackDistance = 3;

    //DisplayHappiness
    //[SerializeField] private GameObject happinessUI;

    private bool isIdle = false;


    void Start()
    {
    }

    void Update()
    {
        DisplayHappiness();
        CheckBestAction();
        ClockCycleDeteriation();
    }

    //Simple deteriation of the utility points during game time
    void ClockCycleDeteriation() 
    {
        clockCycleTimeCurrent += Time.deltaTime;
        if (clockCycleTimeCurrent > clockCycleTime) 
        {
            hunger++;
            Happiness = Happiness - hunger;
            clockCycleTimeCurrent = 0;
            Debug.Log(Happiness);
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

    void DisplayHappiness() 
    { 
        
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

            if (Vector3.Distance(transform.position, walkPoint) < 3f)
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
        Collider[] collliders = Physics.OverlapSphere(transform.position, 5);
        foreach(Collider hit in collliders) 
        { 
            //gameobj = get component food
            //if gameobj!=null then setfoodposition
        }

        //Make raycast check here to see if plant is in reach
    }

    //eatplant 
    void EatFood(GameObject plant) 
    {
        hunger = 0;
        Destroy(plant);
    }


    //Basiclly set a random walkpoint
    Vector3 FindWalkPoint() 
    { 
        Vector3 point = new Vector3 (Random.Range(minIslandSize, maxIslandSize), transform.localScale.y / 2, Random.Range(minIslandSize, maxIslandSize));
        return point;
    }

    //Will walk to corner and die if unhappy
    void LeaveIsland() 
    {
        Vector3 leaveVec = new Vector3(0, 0, 0);
        AnimalWalkMoveTowards(leaveVec);
        if(Vector3.Distance(transform.position, leaveVec) < 3) 
        {
            Destroy(gameObject);
        }
    }
    //Prototype can only handle one enemy in the sceen, can be changed later
    void BossFigth() 
    {
        //if (health > 25)
        //{
        //    //transform.LookAt(enemyinstansemanager.instance.transform.position)
        //    //transform.position = Vector3.MoveTowards(transform.position, enemyinstancemanager.transform.position, runSpeed * time.deltatime)
        //    if(Physics.Raycast(transform.position, Vector3.forward, out RaycastHit hit, attackDistance))
        //    { 
        //        //DMG enemy
        //    }
        //}
        //if (allenemydead) 
        //{ 
        //    bossThreat = 0
        //}
    
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
}
