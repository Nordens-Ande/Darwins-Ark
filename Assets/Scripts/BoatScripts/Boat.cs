using System.Collections.Generic;
using Assets.Scripts.Environment;
using UnityEngine;

public interface IBoatSpawning
{
    public Transform Transform
    {  get; }

    public bool Unload(Tile beachTile);
}

public class Boat : MonoBehaviour
{
    PathFinding pathFinder;

    enum BoatSequence // what step of the journey we are at
    {
        MoveTowardsRotationTile,
        Rotate,
        MoveTowardsTargetTile,
        AtTargetTile,
        LeavingTowardsRotationTile,
        RotateToLeave,
        Leave
    }

    BoatSequence step = BoatSequence.MoveTowardsRotationTile;

    Tile beachTile; // beach tile where object will be dropped off
    Tile targetTile; // tile where the boat will stop, 1 or 2 tiles away from beach
    Tile rotationTile; //tile where the boat will stop and rotate towards the beach

    Tile startTile; // start and end tile of boat 

    Tile currentTile;
    Tile nextTile;

    List<Tile> path;
    int pathStage = 0;

    float maxMovementSpeed = 5;
    float minMovementSpeed = 1f;

    float distanceFromCoast = 3f; // make not hardcoded if weird later

    [SerializeField] GameObject loadPos;
    IBoatSpawning loadObject; //object on boat, animal or boss

    public GameObject LoadPos => loadPos;
    public IBoatSpawning LoadObject
    {
        get => loadObject;
        set => loadObject = value;
    }
    
    public void Initialize(Tile beachTile, Tile startTile) // constructor called from BoatManager after a boat is instantiated
    {
        pathFinder = new PathFinding();
        path = new List<Tile>();
        pathStage = 0;

        this.beachTile = beachTile;
        this.startTile = startTile;
        currentTile = startTile;
        targetTile = SetTargetTile();
        rotationTile = SetRotationTile();

        CreatePath();
        SetNextTile(path[pathStage]);
        transform.rotation = Quaternion.LookRotation(nextTile.position - currentTile.position);
    }

    void SetNextTile(Tile nextTile)
    {
        this.nextTile = nextTile;
    }

    bool IsBoatTraversable(Tile tile)
    {
        if (tile.Type != TileType.Ocean)
        {
            return false;
        }

        float minimumDistanceFromCenter = IslandNoise.Instance.MaxIslandRadius + distanceFromCoast;
        float distanceFromCenter = Vector3.Distance(tile.position, Vector3.zero);
        if (distanceFromCenter < minimumDistanceFromCenter)
        {
            return false;
        }

        return true;
    }

    Tile SetTargetTile()
    {
        Vector3 bPos = beachTile.position;
        float x = Mathf.Abs(bPos.x);
        float z = Mathf.Abs(bPos.z);

        if (x > z)
        {
            if(bPos.x < 0)
            {
                return TileManager.Instance.GetTile(bPos.x - 1, bPos.z);
            }
            else
            {
                return TileManager.Instance.GetTile(bPos.x + 1, bPos.z);
            }
            
        }
        else
        {
            if (bPos.z < 0)
            {
                return TileManager.Instance.GetTile(bPos.x, bPos.z - 1);
            }
            else
            {
                return TileManager.Instance.GetTile(bPos.x, bPos.z + 1);
            }
        }
    }

    Tile SetRotationTile()
    {
        Vector3 tPos = targetTile.position;
        float x = Mathf.Abs(tPos.x);
        float z = Mathf.Abs(tPos.z);

        Vector3 direction;
        if(x > z)
        {
            //rotationTile = TileManager.Instance.GetTile(tPos.x + (((Mathf.Abs(tPos.x) / tPos.x) * distanceFromCoast) + 2), tPos.z);
            direction = new Vector3((Mathf.Abs(tPos.x) / tPos.x), 0, 0);
        }
        else
        {
            //rotationTile = TileManager.Instance.GetTile(tPos.x, tPos.z + (((Mathf.Abs(tPos.z) / tPos.z) * distanceFromCoast) + 2));
            direction = new Vector3(0, 0, (Mathf.Abs(tPos.z) / tPos.z));
        }

        Tile tile = null;
        for(int i = 1; i < 20; i++)
        {
            tile = TileManager.Instance.GetTile(tPos + direction * i);
            if(tile == null)
            {
                continue;
            }
            if(!IsBoatTraversable(tile))
            {
                continue;
            }

            return tile;
        }
        return tile;
    }

    List<Tile> CalculatePath()
    {
        List<Tile> path = new List<Tile>();
        //HashSet<Tile> visited = new HashSet<Tile>();

        //Vector3Int[] directions =
        //{
        //    new Vector3Int(1, 0, 0),
        //    new Vector3Int(0, 0, 1),
        //    new Vector3Int(-1, 0, 0),
        //    new Vector3Int(0, 0, -1),
        //};

        //Tile currentTile = rotationTile;
        //visited.Add(currentTile);

        //int iterationCount = 0;
        //int maxIterations = TileManager.Instance.ChunkGridSize.x * TileManager.Instance.ChunkSize * 20;
        //while (true)
        //{
        //    iterationCount++;
        //    if (iterationCount > maxIterations)
        //    {
        //        Debug.Log("max iterations reached when building path for boat");
        //        break;
        //    }

        //    float closestDistance = -1;
        //    Tile nextTile = null;
        //    foreach(Vector3 direction in directions)
        //    {
        //        Tile tile = TileManager.Instance.GetTile(currentTile.position.x + direction.x, currentTile.position.z + direction.z);
        //        if (tile == null)
        //        {
        //            continue;
        //        }

        //        if (visited.Contains(tile))
        //        {
        //            continue;
        //        }

        //        if(!IsBoatTraversable(tile))
        //        {
        //            continue;
        //        }

        //        float distance = Vector3.Distance(tile.position, startTile.position);
        //        if (distance < closestDistance || closestDistance == -1)
        //        {
        //            closestDistance = distance;
        //            nextTile = tile;
        //        }
        //    }

        //    if (nextTile == null)
        //    {
        //        Debug.Log("unable to find next tile when building path for boat");
        //        break;
        //    }

        //    if (nextTile == startTile)//path finished
        //    {
        //        break;
        //    }
            
        //    path.Add(nextTile);
        //    visited.Add(nextTile);
        //    currentTile = nextTile;
        //}

        List<TileType> traversableTiles = new List<TileType>();
        traversableTiles.Add(TileType.Ocean);
        path = pathFinder.GetPath(startTile, rotationTile, IsBoatTraversable, 1);

        for(int i = path.Count - 1; i >= 0; i--) // remove all tiles except every 10th to make movement nicer!
        {
            if(i == 0)
                path.RemoveAt(i);
            if(i != 0)
            {
                if(i % 10 != 0)
                {
                    path.RemoveAt(i);
                }
            }
        }

        //path.Reverse(); // since we start from rotation tile
        
        return path;
    }

    void CreatePath()
    {
        List<Tile> calculatedPath = CalculatePath(); // return a path from spawn to the rotationTile, consisting of a few Vector3 points
        path.Clear();
        path.AddRange(calculatedPath);
        path.Add(rotationTile);
        path.Add(targetTile);
        path.Add(rotationTile);
        calculatedPath.Reverse();
        path.AddRange(calculatedPath);
        path.Add(startTile);
    }

    void DecideAction()
    {
        if (step == BoatSequence.MoveTowardsRotationTile || step == BoatSequence.MoveTowardsTargetTile || step == BoatSequence.LeavingTowardsRotationTile || step == BoatSequence.Leave)
        {
            BoatMove();
        }
        else if (step == BoatSequence.Rotate || step == BoatSequence.RotateToLeave)
        {
            BoatRotate();
        }
        else if (step == BoatSequence.AtTargetTile)
        {
            BoatUnload();
        }
    }

    void UpdateStepFromMove()
    {
        if(currentTile == rotationTile && step == BoatSequence.MoveTowardsRotationTile)
        {
            step = BoatSequence.Rotate;
        }
        else if (currentTile == targetTile && step == BoatSequence.MoveTowardsTargetTile)
        {
            loadObject.Transform.SetParent(null);
            step = BoatSequence.AtTargetTile;
        }
        else if (currentTile == rotationTile && step == BoatSequence.LeavingTowardsRotationTile)
        {
            step = BoatSequence.RotateToLeave;
        }
    }

    void UpdateStepFromRotate()
    {
        if(currentTile == rotationTile && step == BoatSequence.Rotate)
        {
            step = BoatSequence.MoveTowardsTargetTile;
        }
        else if(currentTile == rotationTile && step == BoatSequence.RotateToLeave)
        {
            step = BoatSequence.Leave;
        }
    }

    void UpdateStepFromUnload()
    {
        step = BoatSequence.LeavingTowardsRotationTile;
    }

    bool CheckIfReachedNextTile(bool goingBackwards)
    {
        Vector3 toTarget = nextTile.position - transform.position;

        if (goingBackwards)
        {
            return Vector3.Dot(transform.forward, toTarget) >= 0f;
        }
        return Vector3.Dot(transform.forward, toTarget) <= 0f;
    }

    float DecideSpeed()
    {
        float movementSpeed = maxMovementSpeed;

        float maxDistance = Vector3.Distance(currentTile.position, nextTile.position);
        float distance = Vector3.Distance(transform.position, nextTile.position);
        float d = distance / maxDistance;
        if (nextTile == rotationTile && step == BoatSequence.MoveTowardsRotationTile) //slow down as we are arriving to rotation tile
        {
            return Mathf.Lerp(minMovementSpeed, maxMovementSpeed, d);
        }
        else if(step == BoatSequence.LeavingTowardsRotationTile || step == BoatSequence.MoveTowardsTargetTile) //speed up until halfway then slowdown
        {
            if (d >= 0.5f)
            {
                return Mathf.Lerp(minMovementSpeed, maxMovementSpeed, (1f - d) * 2f);
            }
            else
            {
                return Mathf.Lerp(minMovementSpeed, maxMovementSpeed, d * 2f);
            }
        }
        else if(currentTile == rotationTile && step == BoatSequence.Leave) // accelerate for 3 tiles
        {
            maxDistance = 3f;
            distance = Vector3.Distance(transform.position, currentTile.position);
            d = Mathf.Clamp01(distance / maxDistance);
            return Mathf.Lerp(minMovementSpeed, maxMovementSpeed, d);
        }

        return movementSpeed;
    }

    void BoatMove()
    {
        bool goingBackwards = false;

        //decide speed
        float movementSpeed = DecideSpeed();

        //move and rotate
        Vector3 direction = nextTile.position - transform.position;
        direction.Normalize();

        Vector3 moveDirection = transform.forward;

        if (step == BoatSequence.LeavingTowardsRotationTile)
        {
            goingBackwards = true;
            direction = -direction;
            moveDirection = -moveDirection;
        }
        if(direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 20f * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(transform.position, transform.position + moveDirection, movementSpeed * Time.deltaTime);

        //check if reached tile
        if(CheckIfReachedNextTile(goingBackwards))
        {
            currentTile = nextTile;
            pathStage++;

            if (pathStage < path.Count)
            {
                SetNextTile(path[pathStage]);
            }
                
            UpdateStepFromMove();
        }
    }

    bool CheckIfReachedRotation(Quaternion rotation)
    {
        return Quaternion.Angle(transform.rotation, rotation) < 1f;
    }

    void BoatRotate()
    {
        Vector3 direction = nextTile.position - transform.position;
        direction.Normalize();
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 30f * Time.deltaTime);

        if(CheckIfReachedRotation(targetRotation))
        {
            UpdateStepFromRotate();
        }
    }

    void BoatUnload()
    {
        if(loadObject.Unload(beachTile))
        {
            UpdateStepFromUnload();
        }
    }

    void Update()
    {
        DecideAction();
    }
}
