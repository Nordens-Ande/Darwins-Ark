using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Environment;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine.Assemblies;

public class PathFinding
{
    class TileNode
    {
        public Vector2Int prevTile;
        public float f, g, h;
        public TileNode()
        {
            prevTile = new Vector2Int();
            f = float.MaxValue; //total cost, g+h
            g = float.MaxValue; //cost to reach the tile from start
            h = float.MaxValue; //cost to reach the target tile from this tile
        }
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

    List<Tile> BuildPath(Tile targetTile, Tile startTile, Dictionary<Vector2Int, TileNode> tileNodes)
    {
        List<Tile> path = new List<Tile>();

        Vector2Int current = TileToVector2Int(targetTile);
        Vector2Int start = TileToVector2Int(startTile);
        
        while(current != start)
        {
            path.Add(tileManager.GetTile(current));
            current = tileNodes[current].prevTile;
        }

        path.Add(startTile);
        path.Reverse();
        Debug.Log("pathfinding: path length: " + path.Count);
        return path;
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

    public List<Tile> GetPath(Tile startTile, Tile targetTile, Func<Tile, bool> traversable, float maxHeightDifference)
    {
        Dictionary<Vector2Int, TileNode> tileNodes = new Dictionary<Vector2Int, TileNode>();
        SortedSet<Vector2Int> openSet = new SortedSet<Vector2Int>(
            Comparer<Vector2Int>.Create((a, b) =>
            {
                int comparison = tileNodes[a].f.CompareTo(tileNodes[b].f);
                if (comparison == 0)
                {
                    comparison = a.x.CompareTo(b.x);

                    if (comparison == 0)
                        comparison = a.y.CompareTo(b.y);
                }
                return comparison;
            })
        );

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        foreach (Chunk chunk in tileManager.chunks)
        {
            foreach(Tile tile in chunk.Tiles)
            {
                tileNodes[TileToVector2Int(tile)] = new TileNode();
            }
        }

        TileNode startNode = tileNodes[TileToVector2Int(startTile)];
        startNode.g = 0;
        startNode.h = DistanceToTargetTile(TileToVector2Int(startTile), TileToVector2Int(targetTile));
        startNode.f = startNode.g + startNode.h;

        openSet.Add(TileToVector2Int(startTile));

        while(openSet.Count > 0)
        {
            Vector2Int currentTile = openSet.Min;
            openSet.Remove(currentTile);
            visited.Add(currentTile);

            if (currentTile == TileToVector2Int(targetTile))
            {
                break;
            }

            List<Vector2Int> neighbourTiles = GetNeighbours(currentTile);
            foreach(Vector2Int tile in neighbourTiles)
            {
                if(visited.Contains(tile))
                {
                    continue;
                }
                if(!traversable(tileManager.GetTile(tile)))
                {
                    continue;
                }
                if(Mathf.Abs(tileManager.GetTile(currentTile).position.y - tileManager.GetTile(tile).position.y) > maxHeightDifference)
                {
                    continue;
                }

                TileNode node = tileNodes[tile];
                float g = tileNodes[currentTile].g + 1;
                float h = DistanceToTargetTile(tile, TileToVector2Int(targetTile));
                float f = g + h;
                if(g < node.g)
                {
                    openSet.Remove(tile);
                    node.g = g;
                    node.h = h;
                    node.f = f;
                    node.prevTile = currentTile;
                    openSet.Add(tile);
                }
            }
        }

        return BuildPath(targetTile, startTile, tileNodes);
    }

}
