using System.Collections.Generic;
using Assets.Scripts.Environment;
using UnityEngine;

public class Boat : MonoBehaviour
{
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
    float minMovementSpeed = 0.5f;

    public void Initialize(Tile beachTile, Tile startTile) // constructor called from BoatManager after a boat is instantiated
    {
        path = new List<Tile>();
        pathStage = 0;

        this.beachTile = beachTile;
        this.startTile = startTile;
        currentTile = startTile;
        targetTile = SetTargetTile();
        rotationTile = SetRotationTile();

        CreatePath();
        SetNextTile(path[pathStage]);
    }

    void SetNextTile(Tile nextTile)
    {
        this.nextTile = nextTile;
    }

    Tile SetTargetTile()
    {
        return TileManager.Instance.GetTile(beachTile.position.x + 1, beachTile.position.z);
    }

    Tile SetRotationTile()
    {
        return TileManager.Instance.GetTile(targetTile.position.x + 5, targetTile.position.z);
    }

    List<Tile> CalculatePath()
    {
        List<Tile> path = new List<Tile>();

        Vector3Int[] directions =
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 0, -1),
        };

        Tile currentTile = rotationTile;

        int iterationCount = 0;
        int maxIterations = TileManager.Instance.ChunkGridSize.x * TileManager.Instance.ChunkSize * 20;
        while (true)
        {
            iterationCount++;
            if (iterationCount > maxIterations)
            {
                Debug.Log("max iterations reached when building path for boat");
                break;
            }

            float closestDistance = -1;
            Tile nextTile = null;
            foreach(Vector3 direction in directions)
            {
                Tile tile = TileManager.Instance.GetTile(currentTile.position.x + direction.x, currentTile.position.z + direction.z);
                if (tile.Type != TileType.Ocean)
                    continue;

                float distance = Vector3.Distance(tile.position, startTile.position);
                if(distance < closestDistance || closestDistance == -1)
                {
                    closestDistance = distance;
                    nextTile = tile;
                }
            }

            if (nextTile == null)
            {
                Debug.Log("unable to find next tile when building path for boat");
                break;
            }

            if (nextTile == startTile)//path finished
            {
                Debug.Log("Found start tile when building path for boat");
                break;
            }
            
            path.Add(nextTile);
            currentTile = nextTile;
        }

        for(int i = path.Count - 1; i >= 0; i--) // remove all tiles except every 5th to make movement nicer!
        {
            if(i == 0)
                path.RemoveAt(i);
            if(i != 0)
            {
                if(i % 5 != 0)
                {
                    path.RemoveAt(i);
                }
            }
        }

        path.Reverse(); // since we start from rotation tile
        
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

    bool CheckIfReachedNextTile()
    {
        float distance = Vector3.Distance(nextTile.position, transform.position);
        if (distance < 0.01f)
        {
            return true;
        }
        return false;
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
                return Mathf.Lerp(minMovementSpeed, maxMovementSpeed, (1f - d) * 2f) / 2;
            }
            else
            {
                return Mathf.Lerp(minMovementSpeed, maxMovementSpeed, d * 2f) / 2;
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
        //decide speed
        float movementSpeed = DecideSpeed();

        //move and rotate
        transform.position = Vector3.MoveTowards(transform.position, nextTile.position, movementSpeed * Time.deltaTime);

        Vector3 direction = nextTile.position - transform.position;
        direction.Normalize();
        if (step == BoatSequence.LeavingTowardsRotationTile)
        {
            direction = -direction;
        }
        transform.rotation = Quaternion.LookRotation(direction);

        //check if reached tile
        if(CheckIfReachedNextTile())
        {
            currentTile = nextTile;
            pathStage++;
            if (pathStage < path.Count - 1)
                SetNextTile(path[pathStage]);
            UpdateStepFromMove();
        }
    }

    bool CheckIfReachedRotation(Quaternion rotation)
    {
        if(Quaternion.Angle(transform.rotation, rotation) < 1f)
        {
            return true;
        }
        return false;
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
        UpdateStepFromUnload();
    }

    void Update()
    {
        DecideAction();
    }
}
