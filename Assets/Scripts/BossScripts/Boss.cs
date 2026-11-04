using UnityEngine;
using System;
using Assets.Scripts.Environment;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.VisualScripting;

public class Boss : MonoBehaviour, IBoatSpawning
{
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

    private List<Tile> closestAttackTilePath; //used to cache the path to the closest plant, river or animal, minimizes A star pathfinding calls #optimization

    public PathFinding PathFinder => pathFinder;
    public BossHealth HealthScript => healthScript;
    public BossMovement MovementScript => movementScript;
    public float SightDistanceToAnimals => sightDistanceToAnimals;
    public float SightDistanceToTiles => sightDistanceToTiles;
    public float AttackTime => attackTime;
    public List<Tile> ClosestAttackTilePath => closestAttackTilePath;


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

    List<Tile> FindAllTilesWithPlants()
    {
        List<Tile> tilesWithPlants = new List<Tile>();
        foreach(Chunk chunk in tileManager.chunks)
        {
            foreach(Tile tile in chunk.Tiles)
            {
                if(tile.HasPlant)
                {
                    tilesWithPlants.Add(tile);
                }
            }
        }
        //Debug.Log("tile with plants: " + tilesWithPlants.Count);
        return tilesWithPlants;
    }

    /// <summary>
    /// Finds the closest reachable plant by the boss.
    /// </summary>
    /// <returns>Returns the Tile the plant is on, or null if tile is unreachable</returns>
    public void FindClosestPlantTile()
    {
        List<Tile> tilesWithPlants = FindAllTilesWithPlants();
        if (tilesWithPlants.Count <= 0)
        {
            closestAttackTilePath = null;
            return;
        }

        tilesWithPlants = tilesWithPlants.OrderBy(tile => Vector3.Distance(transform.position, tile.Position)).ToList();

        for(int i = 0; i < tilesWithPlants.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, tilesWithPlants[i].Position);
            if (distance > sightDistanceToTiles) continue;

            closestAttackTilePath = movementScript.PathFinder.GetPathAStar(tileManager.GetTile(transform.position), tilesWithPlants[i], movementScript.IsBossTraversable, 2f, (int)(sightDistanceToTiles * 2));
            if (closestAttackTilePath != null)
            {
                return;
            }
        }
    }

    public void FindClosestRiverTile()
    {
        List<Tile> riverTiles = new List<Tile>();

        foreach(Chunk chunk in tileManager.chunks)
        {
            HashSet<Vector2Int> riverTilePositions = chunk.GetTiles(TileType.River);
            foreach(Vector2Int riverTilePosition in riverTilePositions)
            {
                Tile tile = tileManager.GetTile(riverTilePosition);
                if(tile == null) continue;

                riverTiles.Add(tile);
            }
        }

        if (riverTiles.Count <= 0)
        {
            closestAttackTilePath = null;
            return;
        }

        riverTiles = riverTiles.OrderBy(tile => Vector3.Distance(transform.position, tile.Position)).ToList();

        for (int i = 0; i < riverTiles.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, riverTiles[i].Position);
            if (distance > sightDistanceToTiles) continue;

            closestAttackTilePath = movementScript.PathFinder.GetPathAStar(tileManager.GetTile(transform.position), riverTiles[i], movementScript.IsBossTraversable, 2f, (int)(sightDistanceToTiles * 2));
            if (closestAttackTilePath != null)
            {
                return;
            }
        }
    }

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
            FindClosestPlantTile();
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

        foreach(BossAIState state in AIStates)
        {
            if(state is BossAttackPlantState)
            {
                FindClosestPlantTile();
            }
            else if(state is BossAttackRiverState)
            {
                FindClosestRiverTile();
            }
            //else if(state is BossAttackAnimalState) to be implemented
            //{
            //    FindClosestAnimalTile();
            //}
        }
        
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
                        break;
                    }
                }
                else
                {
                    currentState.Tick();
                    break;
                }
            }

            if (statesOrderedByScore[i].OnEnterState())
            {
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
