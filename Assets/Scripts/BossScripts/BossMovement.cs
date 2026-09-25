using System.Collections.Generic;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    float movementSpeed;
    [SerializeField] Vector3 targetTile;
    int lastDirection;

    void Start()
    {
        movementSpeed = 3;
        NewTargetTile();
    }
    void NewTargetTile()
    {
        Debug.Log("choosing new tile");
        int currentX = Mathf.RoundToInt(transform.position.x);
        int currentZ = Mathf.RoundToInt(transform.position.z);

        List<Tile> possibleTiles = new List<Tile>();

        foreach (Tile tile in TileManager.Instance.tiles)
        {
            int x = Mathf.RoundToInt(tile.transform.localPosition.x);
            int z = Mathf.RoundToInt(tile.transform.localPosition.z);

            if (z == currentZ && x != currentX)
            {
                possibleTiles.Add(tile);
            }

            if (x == currentX && z != currentZ)
            {
                possibleTiles.Add(tile);
            }
        }

        if (possibleTiles.Count == 0)
        {
            Debug.Log("No possible target tiles!");
            return;
        }

        Tile chosenTile = possibleTiles[
            Random.Range(0, possibleTiles.Count)
        ];

        targetTile = chosenTile.transform.position;
        targetTile = new Vector3(targetTile.x, 0, targetTile.z);
    }

    bool ReachedTargetTile(Vector3 targetTile)
    {
        float distance = Vector3.Distance(targetTile, transform.position);
        if (distance < 0.1f)
        {
            Debug.Log("reached tile");
            return true;
        }
        return false;
    }

    void Rotate()
    {
        Vector3 direction = targetTile - transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void Update()
    {
        if(targetTile != null)
        {
            if(ReachedTargetTile(targetTile))
            {
                transform.position = targetTile;
                NewTargetTile();
                return;
            }
            
            Vector3 direction = targetTile - transform.position;
            direction.Normalize();
            transform.position = Vector3.MoveTowards(transform.position, targetTile, movementSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
            Rotate();
        }
    }
}
