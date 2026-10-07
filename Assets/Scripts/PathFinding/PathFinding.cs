using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Environment;
using Unity.VisualScripting;
using System.Linq;

public class PathFinding
{

    struct TileNode
    {
        public Vector2Int prevTile;
        public float f, g, h;
    }

    TileManager tileManager = TileManager.Instance;

    Vector2Int[] directions =
    {
        new Vector2Int(1, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
    };

    Vector2Int TileToVector2Int(Tile tile)
    {
        return new Vector2Int((int)tile.position.x, (int)tile.position.z);
    }

    HashSet<Vector2Int> GetTraversableTiles(List<TileType> traversableTileTypes)
    {
        HashSet<Vector2Int> traversableTiles = new HashSet<Vector2Int>();

        foreach(Chunk chunk in tileManager.chunks)
        {
            foreach(TileType type in traversableTileTypes)
            {
                traversableTiles.AddRange(chunk.GetTiles(type));
            }
        }

        return traversableTiles;
    }

    List<Tile> BuildPath(Tile TargetTile)
    {
        return new List<Tile>();
    }

    List<Vector2Int> GetNeighbours(Vector2Int currentTile)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();
        foreach(Vector2Int direction in directions)
        {
            neighbours.Add(currentTile + direction);
        }
        return neighbours;
    }

    float DistanceToTargetTile(Vector2Int currentTile, Vector2Int targetTile) //incorrect probably
    {
        return Vector2.Distance(currentTile, targetTile);
    }

    public List<Tile> GetPath(Tile startTile, Tile targetTile, List<TileType> traversableTileTypes, float maxHeightDifference)
    {
        HashSet<Vector2Int> traversableTiles = GetTraversableTiles(traversableTileTypes);

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, TileNode> tileNodes = new Dictionary<Vector2Int, TileNode>();

        TileNode startNode = tileNodes[TileToVector2Int(startTile)];
        startNode.g = 0;
        startNode.h = DistanceToTargetTile(TileToVector2Int(startTile), TileToVector2Int(targetTile));

        foreach(Vector2Int tile in traversableTiles)
        {
            tileNodes[tile] = new TileNode();
        }

        queue.Enqueue(TileToVector2Int(startTile));

        bool foundTarget = false;
        while(queue.Count > 0)
        {
            queue.OrderBy(pos => tileNodes[pos].f);

            Vector2Int currentTile = queue.Dequeue();
            List<Vector2Int> neighbourTiles = GetNeighbours(currentTile);
            foreach(Vector2Int tile in neighbourTiles)
            {
                if(tile == TileToVector2Int(targetTile))
                {
                    TileNode targetNode = tileNodes[TileToVector2Int(targetTile)];
                    targetNode.prevTile = currentTile;
                    foundTarget = true;
                    break;
                }
                if(visited.Contains(tile))
                {
                    continue;
                }
                if(!traversableTiles.Contains(tile))
                {
                    continue;
                }
                if(Mathf.Abs(tileManager.GetTile(currentTile).position.y) - Mathf.Abs(tileManager.GetTile(tile).position.y) > maxHeightDifference)
                {
                    continue;
                }

                TileNode node = tileNodes[tile];
                node.g = tileNodes[currentTile].g + 1; // calculate for real 
                node.h = DistanceToTargetTile(tile, TileToVector2Int(targetTile)); //incorrect for now, fix method
                node.f = node.g + node.h;
                node.prevTile = currentTile; // uhuh
            }
            visited.Add(currentTile);
            if(foundTarget)
            {
                break;
            }
        }

        return BuildPath(targetTile);
    }

}
