using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    float movementSpeed;
    [SerializeField] Tile targetTile;
    Tile currentTile;
    Tile nextTile;
    Vector3 moveDirection; //used to check what direction we moved last frame, when selecting new target tile we dont want to go in the negative of this direction

    void Start()
    {
        movementSpeed = 3;
        targetTile = NewTargetTile();

        Vector3 direction = targetTile.transform.position - transform.position;
        direction.y = 0;
        direction.Normalize();

        nextTile = GetNextTile(direction);
        moveDirection = Vector3.zero;
    }

    Tile NewTargetTile()
    {
        Debug.Log("choosing new target tile");
        int currentX = Mathf.RoundToInt(transform.position.x);
        int currentZ = Mathf.RoundToInt(transform.position.z);

        List<Tile> possibleTiles = new List<Tile>();

        foreach (Tile tile in TileManager.Instance.tiles)
        {
            int x = Mathf.RoundToInt(tile.transform.localPosition.x);
            int z = Mathf.RoundToInt(tile.transform.localPosition.z);

            Vector3 direction = tile.transform.position - transform.position;
            direction.Normalize();
            if (direction == -moveDirection) continue; //dont move backwards

            if (z == currentZ && x != currentX)
            {
                possibleTiles.Add(tile);
            }

            if (x == currentX && z != currentZ)
            {
                possibleTiles.Add(tile);
            }
        }

        Tile chosenTile = possibleTiles[Random.Range(0, possibleTiles.Count)];
        return chosenTile;
    }

    Tile GetNextTile(Vector3 direction)
    {
        Tile tile = TileManager.Instance.GetTile(transform.position + direction);
        if(tile != null)
        {
            Debug.Log("returned next tile");
            return tile;
        }
        else
        {
            Debug.Log("didnt find next tile");
            return null;
        }
    }

    bool CheckIfReachedTargetTile(Tile tile)
    {
        if (tile == targetTile) return true;
        else return false;
    }

    (bool, bool) ReachedNextTile()
    {
        float distance = Vector3.Distance(nextTile.transform.position, transform.position);
        if (distance < 0.1f)
        {
            if(CheckIfReachedTargetTile(nextTile))
            {
                Debug.Log("reached target tile");
                return (true, true);
            }
            else
            {
                Debug.Log("reached tile");
                return (true, false);
            }
        }
        return (false, false);
    }

    void Rotate(Vector3 direction)
    {
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void Update()
    {
        if(nextTile != null)
        {
            moveDirection = targetTile.transform.position - transform.position;
            moveDirection.Normalize();

            (bool reachedNextTile, bool reachedTargetTile) = ReachedNextTile();
            
            if(reachedNextTile)
            {
                //deal damage to plants on tile
                //if tile.HasPlant
                //{
                //    PlantManager.KillPlant(nextTile);
                //} etc
                transform.position = nextTile.transform.position;
                if (reachedTargetTile)
                {
                    targetTile = NewTargetTile();
                    return;
                }
                nextTile = GetNextTile(moveDirection);
            }
            
            transform.position = Vector3.MoveTowards(transform.position, nextTile.transform.position, movementSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
            Rotate(moveDirection);
        }
    }
}
