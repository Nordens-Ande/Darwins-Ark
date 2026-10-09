using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Assets.Scripts.Environment;
using static UnityEditor.PlayerSettings;
using UnityEditor.Experimental.GraphView;

public class BossMovement : MonoBehaviour
{
    List<Tile> currentPath;

    [SerializeField] float movementSpeed;

    Tile currentTile;
    Tile nextTile;
    Vector3 moveDirection; //used to check what direction we moved last frame, when selecting new target tile we dont want to go in the negative of this direction

    void Start()
    {
        currentPath = new List<Tile>();
        movementSpeed = 2;

        moveDirection = Vector3.zero;
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
        transform.position = Vector3.MoveTowards(transform.position, nextTile.position, movementSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
        Rotate(moveDirection);
    }
}
