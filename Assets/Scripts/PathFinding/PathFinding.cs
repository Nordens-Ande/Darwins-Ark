using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Environment;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine.Assemblies;

public class PathFinding
{
    abstract class TileNode
    {
        public Vector2Int prevTile;
    }

    class TileNodeAStar : TileNode
    {
        public float f, g, h;
        public TileNodeAStar()
        {
            prevTile = new Vector2Int();
            f = float.MaxValue; //total cost, g+h
            g = float.MaxValue; //cost to reach the tile from start
            h = float.MaxValue; //cost to reach the target tile from this tile
        }
    }

    class TileNodeDijkstra : TileNode
    {
        public float totalCost;
        public TileNodeDijkstra()
        {
            prevTile = new Vector2Int();
            totalCost = float.MaxValue;
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

    List<Tile> BuildPath(Tile targetTile, Tile startTile, Dictionary<Vector2Int, TileNodeAStar> tileNodes)
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
        //Debug.Log("pathfinding: path length: " + path.Count);
        return path;
    }

    List<Tile> BuildPath(Tile targetTile, Tile startTile, Dictionary<Vector2Int, TileNodeDijkstra> tileNodes)
    {
        List<Tile> path = new List<Tile>();

        Vector2Int current = TileToVector2Int(targetTile);
        Vector2Int start = TileToVector2Int(startTile);

        while (current != start)
        {
            path.Add(tileManager.GetTile(current));
            current = tileNodes[current].prevTile;
        }

        path.Add(startTile);
        path.Reverse();
        //Debug.Log("pathfinding: path length: " + path.Count);
        return path;
    }

    List<Vector2Int> GetNeighbours(Vector2Int currentTile)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();
        foreach(Vector2Int direction in directions)
        {
            if(tileManager.GetTile(currentTile + direction) == null) continue;
            neighbours.Add(currentTile + direction);
        }
        return neighbours;
    }

    float DistanceToTargetTile(Vector2Int currentTile, Vector2Int targetTile)
    {
        float dx = Mathf.Abs(currentTile.x - targetTile.x);
        float dy = Mathf.Abs(currentTile.y - targetTile.y);

        float diagonal = Mathf.Min(dx, dy);
        float straight = Mathf.Max(dx, dy) - diagonal;

        return diagonal * Mathf.Sqrt(2f) + straight;
    }

    /// <summary>
    /// Gives a path of tiles to a specified target tile.
    /// </summary>
    /// <param name="startTile">The current tile the object is on</param>
    /// <param name="targetTile">The target tile we want to find a path to</param>
    /// <param name="traversable">A method that returns a boolean describing if a tile is traversable, needs Tile as a parameter. Check Boss script IsBossTraversable() method for example</param>
    /// <param name="maxHeightDifference">The maximum height difference between two tiles to disallow traversing if difference is to big</param>
    /// <param name="maxIterations">Max iterations the A star algorithm will do, if this is reached "the path is to long" use 0 if you dont want this to be taken into consideration</param>
    /// <returns>A List<Tile> describing the path</returns>
    public List<Tile> GetPathAStar(Tile startTile, Tile targetTile, Func<Tile, bool> traversable, float maxHeightDifference, int maxIterations)
    {
        //Debug.Log("astar initiated");
        Dictionary<Vector2Int, TileNodeAStar> tileNodes = new Dictionary<Vector2Int, TileNodeAStar>();
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
                tileNodes[TileToVector2Int(tile)] = new TileNodeAStar();
            }
        }

        TileNodeAStar startNode = tileNodes[TileToVector2Int(startTile)];
        startNode.g = 0;
        startNode.h = DistanceToTargetTile(TileToVector2Int(startTile), TileToVector2Int(targetTile));
        startNode.f = startNode.g + startNode.h;

        openSet.Add(TileToVector2Int(startTile));

        bool foundTarget = false;
        int iterations = 0;
        while(openSet.Count > 0)
        {
            if(maxIterations != 0)
            {
                if(iterations >= maxIterations)
                {
                    return null;
                }
            }
            Vector2Int currentTile = openSet.Min;
            openSet.Remove(currentTile);
            visited.Add(currentTile);

            if (currentTile == TileToVector2Int(targetTile))
            {
                foundTarget = true;
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

                Vector2 direction = tile - currentTile;
                float movementCost;

                //check if diagonal movement
                if(direction.x != 0 && direction.y != 0)
                {
                    //make sure both of the orthogonal tiles are traversable aswell regarding height, prevents clipping the corner hopefully
                    Tile tileXDir = tileManager.GetTile(currentTile.x + direction.x, currentTile.y);
                    Tile tileZDir = tileManager.GetTile(currentTile.x, currentTile.y + direction.y);

                    if (Mathf.Abs(tileManager.GetTile(currentTile).position.y - tileXDir.position.y) > maxHeightDifference) continue;
                    if (Mathf.Abs(tileManager.GetTile(currentTile).position.y - tileZDir.position.y) > maxHeightDifference) continue;

                    movementCost = Mathf.Sqrt(2f);
                }
                else
                {
                    movementCost = 1f;
                }

                TileNodeAStar node = tileNodes[tile];
                float g = tileNodes[currentTile].g + movementCost;
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
            iterations++;
        }
        if (!foundTarget) return null;

        return BuildPath(targetTile, startTile, tileNodes);
    }

    public List<Tile> GetPathDijkstra(Tile startTile, Tile targetTile, Func<Tile, bool> traversable, float maxHeightDifference)
    {
        Dictionary<Vector2Int, TileNodeDijkstra> tileNodes = new Dictionary<Vector2Int, TileNodeDijkstra>();
        SortedSet<Vector2Int> openSet = new SortedSet<Vector2Int>(
            Comparer<Vector2Int>.Create((a, b) =>
            {
                int comparison = tileNodes[a].totalCost.CompareTo(tileNodes[b].totalCost);
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
            foreach (Tile tile in chunk.Tiles)
            {
                tileNodes[TileToVector2Int(tile)] = new TileNodeDijkstra();
            }
        }

        TileNodeDijkstra startNode = tileNodes[TileToVector2Int(startTile)];
        startNode.totalCost = 0;

        openSet.Add(TileToVector2Int(startTile));

        bool foundTarget = false;
        while (openSet.Count > 0)
        {
            Vector2Int currentTile = openSet.Min;
            openSet.Remove(currentTile);
            visited.Add(currentTile);

            if (currentTile == TileToVector2Int(targetTile))
            {
                foundTarget = true;
                break;
            }

            List<Vector2Int> neighbourTiles = GetNeighbours(currentTile);
            foreach (Vector2Int tile in neighbourTiles)
            {
                if (visited.Contains(tile))
                {
                    continue;
                }
                if (!traversable(tileManager.GetTile(tile)))
                {
                    continue;
                }
                if (Mathf.Abs(tileManager.GetTile(currentTile).position.y - tileManager.GetTile(tile).position.y) > maxHeightDifference)
                {
                    continue;
                }

                Vector2 direction = tile - currentTile;
                float movementCost;

                //check if diagonal movement
                if (direction.x != 0 && direction.y != 0)
                {
                    //make sure both of the orthogonal tiles are traversable aswell regarding height, prevents clipping the corner hopefully
                    Tile tileXDir = tileManager.GetTile(currentTile.x + direction.x, currentTile.y);
                    Tile tileZDir = tileManager.GetTile(currentTile.x, currentTile.y + direction.y);

                    if (Mathf.Abs(tileManager.GetTile(currentTile).position.y - tileXDir.position.y) > maxHeightDifference) continue;
                    if (Mathf.Abs(tileManager.GetTile(currentTile).position.y - tileZDir.position.y) > maxHeightDifference) continue;

                    movementCost = Mathf.Sqrt(2f);
                }
                else
                {
                    movementCost = 1f;
                }

                //update node if lower cost than previous paths
                TileNodeDijkstra node = tileNodes[tile];
                float totalCost = tileNodes[currentTile].totalCost + movementCost;

                if (totalCost < node.totalCost)
                {
                    openSet.Remove(tile);
                    node.totalCost = totalCost;
                    node.prevTile = currentTile;
                    openSet.Add(tile);
                }
            }
        }
        if (!foundTarget) return null;

        return BuildPath(targetTile, startTile, tileNodes);
    }

    public List<Tile> GetPathFromBeachToGrass(Tile startTile, System.Func<Tile, bool> traversable, float maxHeightDifference)
    {
        List<Tile> availableTiles = new List<Tile>();
        foreach(Chunk chunk in tileManager.chunks)
        {
            foreach(Tile tile in chunk.Tiles)
            {
                if(tile.Type == TileType.Grass || tile.Type == TileType.Concrete)
                {
                    availableTiles.Add(tile);
                }
            }
        }
        availableTiles = availableTiles.OrderBy(tile => Vector3.Distance(startTile.position, tile.position)).ToList();

        for(int i = 0; i < availableTiles.Count; i++)
        {
            List<Tile> path = GetPathAStar(startTile, availableTiles[i], traversable, maxHeightDifference, 0);
            if (path != null)
            {
                return path;
            }
        }
        return null;
    }
}
