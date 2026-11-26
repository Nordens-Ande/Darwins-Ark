using UnityEngine;
using System;
using Assets.Scripts.Environment;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.VisualScripting;

public class Boss : MonoBehaviour, IBoatSpawning
{
    private enum BossType
    {
        Plant,
        Animal,
        Terraform
    }

    [SerializeField] private BossType bossType;

    private BossTypeManager bossTypeManager;

    private TileManager tileManager = TileManager.Instance;
    private AnimalManager animalManager = AnimalManager.Instance;
    public Transform Transform => transform;

    private BossHealth healthScript;

    private PathFinding pathFinder;
    private BossMovement movementScript;

    private bool unloaded;

    //ai
    [SerializeReference]
    private List<BossAIState> AIStates;
    [SerializeField, ReadOnly] private BossAIState currentState;
    
    private float sightDistanceToAnimals;
    private float sightDistanceToTiles;
    private float attackTime;

    private List<Tile> closestAttackTilePath; //used to cache the path to the closest plant, river or animal, decreases A star pathfinding calls #optimization

    public BossTypeManager BossTypeManager => bossTypeManager;
    public PathFinding PathFinder => pathFinder;
    public BossHealth HealthScript => healthScript;
    public BossMovement MovementScript => movementScript;
    public float SightDistanceToAnimals => sightDistanceToAnimals;
    public float SightDistanceToTiles => sightDistanceToTiles;
    public float AttackTime => attackTime;
    public List<Tile> ClosestAttackTilePath
    {
        get { return closestAttackTilePath; }
        set { closestAttackTilePath = value; }
    }


    public bool DebugScores => false;



    void Start()
    {
        transform.localPosition = Vector3.zero;
        pathFinder = new PathFinding();
        healthScript = gameObject.GetComponent<BossHealth>();
        movementScript = gameObject.GetComponent<BossMovement>();
        movementScript.PathFinder = pathFinder;
        unloaded = false;
        sightDistanceToAnimals = 10;
        sightDistanceToTiles = 20;
        attackTime = 3;

        switch(bossType)
        {
            case BossType.Plant:
                bossTypeManager = gameObject.GetComponent<BossPlantManager>();
                break;

            //case BossType.Animal:
            //    bossTypeManager = gameObject.GetComponent<BossAnimalManager>();
            //    break;

            case BossType.Terraform:
                bossTypeManager = gameObject.GetComponent<BossTerraManager>();
                break;
        }

        AIStates = gameObject.GetComponents<BossAIState>().ToList();
        if(AIStates == null)
            AIStates = new List<BossAIState>();

        foreach (BossAIState state in AIStates)
            state?.Initialize(this);

        currentState = null;
    }

    public bool Unload(Tile beachTile)
    {
        transform.position = beachTile.Position;
        unloaded = true;

        return true;
    }

    //move to bossanimalmanager
    public List<AnimalAI> FindClosestAnimals(int amountOfAnimals)
    {
        List<AnimalAI> orderedList = animalManager.Animals.OrderBy(animal => Vector3.Distance(transform.position, animal.transform.position)).ToList();

        int amountAdded = 0;
        List<AnimalAI> closestAnimals = new List<AnimalAI>();
        for (int i = 0; i < orderedList.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, orderedList[i].transform.position);
            if (distance > SightDistanceToAnimals) continue;

            //change to animal traversable func later? (not implemented yet)
            if(movementScript.PathFinder.GetPathAStar(tileManager.GetTile(transform.position), tileManager.GetTile(orderedList[i].transform.position), movementScript.IsBossTraversable, 2f, (int)(distance * 2)) != null)
            {
                closestAnimals.Add(orderedList[i]);
                amountAdded++;
                if (amountAdded >= amountOfAnimals)
                {
                    break;
                }
            }
        }
        //Debug.Log("amount of animals parameter: " + amountOfAnimals + " list count: "+ closestAnimals.Count);
        return closestAnimals;
    }

    List<BossAIState> EvaluateStates()
    {
        List<BossAIState> states = AIStates;
        states = states.OrderByDescending(state => state.Evaluate()).ToList();
        return states;
    }

    void Update()
    {
        if(!unloaded) return;

        if(currentState == null) // as the boss arrives on beach, hopefully not after that (:
        {
            bossTypeManager.OnTick();
            List<BossAIState> statesOrdered = EvaluateStates();

            for (int i = 0; i < statesOrdered.Count; i++)
            {
                if (statesOrdered[i].OnEnterState())
                {
                    currentState = statesOrdered[i];
                    
                    return;
                }
            }
        }

        if (currentState == null)
        {
            Debug.Log("bossAI state null");
            return;
        }

        if (!currentState.CanBeInterupted)
        {
            currentState.Tick();
            return;
        }

        bossTypeManager.OnTick();
        List<BossAIState> statesOrderedByScore = EvaluateStates();

        for (int i = 0; i < statesOrderedByScore.Count; i++)
        {
            if (currentState == statesOrderedByScore[i]) //if first iteration is same state as current no need to continue;
            {
                if(currentState.TaskCompleted)
                {
                    currentState.OnExitState();
                    if(currentState.OnEnterState())
                    {
                        //Debug.Log("reentered same state " + currentState.ToString());   
                        break;
                    }
                }
                else
                {
                    currentState.Tick();
                    break;
                }
            }
            else if (statesOrderedByScore[i].OnEnterState())
            {
                //Debug.Log("new boss state " + statesOrderedByScore[i].ToString());
                currentState.OnExitState();
                currentState = statesOrderedByScore[i];
                //Debug.Log("currentState changed to: " + currentState.ToString());
                break;
            }
            else
            {
                //Debug.Log("cant enter state " + statesOrderedByScore[i].ToString());
            }
        }
    }
}
