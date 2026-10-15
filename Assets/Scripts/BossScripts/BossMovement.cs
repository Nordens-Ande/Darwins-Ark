using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Assets.Scripts.Environment;
using static UnityEditor.PlayerSettings;
using UnityEditor.Experimental.GraphView;

public class BossMovement : MonoBehaviour
{
    TileManager tileManager = TileManager.Instance;

    PathFinding pathFinder;
    List<Tile> currentPath;

    [SerializeField] float movementSpeed;
    [SerializeField] float maxHeightDifference;

    int pathStage;

    public PathFinding PathFinder
    {
        get { return pathFinder; }
        set { pathFinder = value; }
    }

    public List<Tile> CurrentPath
    {
        get { return currentPath; }
        set { currentPath = value; Debug.Log("currentPath boss: " + currentPath.Count); pathStage = 0; }
    }
    
    public float MaxHeightDifference => maxHeightDifference;

    void Start()
    {
        currentPath = new List<Tile>();
        movementSpeed = 2;
        maxHeightDifference = 2;
        pathStage = 0;
    }

    public bool IsBossTraversableBeach(Tile tile) //allows traversing beach tiles
    {
        if (tile.Type == TileType.Ocean) return false;
        return true;
    }

    public bool IsBossTraversable(Tile tile)
    {
        if (tile.Type == TileType.Ocean) return false;
        if (tile.Type == TileType.Beach) return false;
        return true;
    }

    public bool SetPath(Tile targetTile, System.Func<Tile, bool> traversable)
    {
        currentPath = pathFinder.GetPathAStar(tileManager.GetTile(transform.position), targetTile, traversable, maxHeightDifference, 0);
        if(currentPath == null) return false;
        Debug.Log("currentPath boss: " + currentPath.Count);
        pathStage = 0;
        return true;
    }

    public bool SetPathFromBeachToGrass()
    {
        currentPath = pathFinder.GetPathFromBeachToGrass(tileManager.GetTile(transform.position), IsBossTraversableBeach, maxHeightDifference);
        if (currentPath == null) return false;
        pathStage = 0;
        return true;
    }

    bool CheckIfReachedNextTile(Tile nextTile)
    {
        return Vector3.Distance(transform.position, nextTile.position) < 0.1f;
    }

    bool CheckIfNextTileIsTargetTile(Tile nextTile)
    {
        return nextTile == currentPath[currentPath.Count - 1];
    }

    public (bool, bool) BossMove()
    {
        if(currentPath == null) return (false, false);
        if(currentPath.Count == 0) return (false, false);

        Tile target = currentPath[pathStage + 1];
        transform.position = Vector3.MoveTowards(transform.position, target.position, movementSpeed * Time.deltaTime);

        Rotate((target.position - currentPath[pathStage].position).normalized);

        if(CheckIfReachedNextTile(target))
        {
            transform.position = target.position;
            if(CheckIfNextTileIsTargetTile(target))
            {
                Debug.Log("Reached target tile");
                return (true, true);
            }
            pathStage++;
            return (false, true);
        }
        return (false, false);
    }

    void Rotate(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void Update()
    {

    }
}
