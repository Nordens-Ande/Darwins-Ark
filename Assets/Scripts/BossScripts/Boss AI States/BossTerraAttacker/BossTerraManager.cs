using Assets.Scripts.Environment;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// this class will be instantiated for each concrete type boss, it is used to save data that all states will utilize
/// </summary>
public class BossTerraManager : BossTypeManager
{
    private TileManager tileManager;

    private Boss boss;

    private float concreteHeight;

    private const float heightDelta = 0.5f; //how much each "attack" changes the height

    private const float areaRadius = 5;
    //private const float areaRadiusVariety = 1; if time
    private List<Tile> attackArea;

    private Tile currentAttackTile;

    public float ConcreteHeight => concreteHeight;
    public float HeightDelta => heightDelta;  
    public Tile CurrentAttackTile => currentAttackTile;
    public bool AttackTileIsLevel => currentAttackTile.Height == concreteHeight;


    Vector2Int[] directions = { Vector2Int.down, Vector2Int.up, Vector2Int.right, Vector2Int.left };

    public override void Initialize(Boss boss)
    {
        this.boss = boss;
        tileManager = TileManager.Instance;
        concreteHeight = FindConcreteHeight();
        FindAttackArea();
    }

    private float FindConcreteHeight()
    {
        if(tileManager == null)
        {
            Debug.LogWarning("tileManager null in BossTerraManager.FindConcreteHeight()");
            return 1;
        }

        HashSet<Vector2Int> concreteTiles = new HashSet<Vector2Int>();
        HashSet<Vector2Int> beachTiles = new HashSet<Vector2Int>();
        foreach (Chunk chunk in tileManager.chunks)
        {
            concreteTiles = chunk.GetTiles(TileType.Concrete);
            if (concreteTiles.Count > 0)
            {
                return tileManager.GetTile(concreteTiles.First()).GetHeight();
            }

            beachTiles.AddRange(chunk.GetTiles(TileType.Beach));
        }

        float highest = -1;
        foreach (Vector2Int beachTile in beachTiles)
        {
            Tile tile = tileManager.GetTile(beachTile);
            float height = tile.GetHeight();
            if(height > highest) highest = height;
        }
        return highest + 0.1f; // 0.1 higher than the highest beach tile
    }

    private List<Tile> FindAttackArea() // need to be remade so that each boss finds an own area. but some percentage of concrete in area to connect it?
    {
        HashSet<Vector2Int> concreteTiles = new HashSet<Vector2Int>();
        foreach (Chunk chunk in tileManager.chunks)
        {
            concreteTiles.AddRange(chunk.GetTiles(TileType.Concrete));
        }

        List<Tile> tiles = new List<Tile>();
        if (concreteTiles.Count > 0)
        {
            List<Vector2Int> concreteTilesList = new List<Vector2Int>(concreteTiles);
            Tile middleTile = tileManager.GetTile(concreteTilesList[Random.Range(0, concreteTilesList.Count)]);

            int radInt = Mathf.CeilToInt(areaRadius);
            float radSquared = areaRadius * areaRadius;
            for (int x = -radInt; x <= radInt; x++)
            {
                for (int z = -radInt; z <= radInt; z++)
                {
                    if (x * x + z * z > radSquared) continue;
                    Tile otherTile = tileManager.GetTile(middleTile.Position.x + x, middleTile.Position.y + z);
                    if (otherTile == null) continue;
                    if (otherTile.Type == TileType.Concrete || otherTile.Type == TileType.Beach || otherTile.Type == TileType.Ocean) continue; //lake???
                    tiles.Add(otherTile);
                }
            }
            return tiles;
        }

        HashSet<Vector2Int> beachTiles = new HashSet<Vector2Int>();
        foreach (Chunk chunk in tileManager.chunks)
        {
            beachTiles.AddRange(chunk.GetTiles(TileType.Beach));
        }

        

        foreach (Vector2Int vec2 in beachTiles)
        {
            foreach(Vector2Int dir in directions)
            {
                Tile tile = tileManager.GetTile(vec2 + dir);
                if(tile == null) continue;
                if (tile.Type != TileType.Grass && tile.Type != TileType.River) continue;

                int radInt = Mathf.CeilToInt(areaRadius);
                float radSquared = areaRadius * areaRadius;
                for (int x = -radInt; x <= radInt; x++)
                {
                    for (int z = -radInt; z <= radInt; z++)
                    {
                        if (x * x + z * z > radSquared) continue;
                        Tile otherTile = tileManager.GetTile(tile.Position.x + x, tile.Position.y + z);
                        if (otherTile == null) continue;
                        if (otherTile.Type == TileType.Concrete || otherTile.Type == TileType.Beach || otherTile.Type == TileType.Ocean) continue; //lake???
                        tiles.Add(otherTile);
                    }
                }
            }
        }
        return tiles;
    }

    public override void OnTick()
    {
        currentAttackTile = FindAttackTile(tileManager.GetTile(transform.position));
    }

    public Tile FindAttackTile(Tile bossTile) // haha yea remake this buddy
    {
        if(attackArea.Contains(bossTile)) // in attackArea
        {
            foreach(Vector2Int dir in directions)
            {
                Tile tile = tileManager.GetTile(bossTile.GridPosition + dir);
                
                if(tile == null) continue;
                if (tile.Type == TileType.Concrete) continue; //lake?
                if(!attackArea.Contains(tile)) continue;
                
                return tile;
            }
        }



        foreach(Tile tile in attackArea) // find a tile next to beach. ensures a path hopefully
        {
            if (tile.Type == TileType.Concrete) continue; //lake?
            foreach (Vector2Int dir in directions)
            {
                Tile checkTile = tileManager.GetTile(tile.GridPosition + dir);
                if(checkTile == null) continue;
                if (checkTile.Type != TileType.Beach) continue;

                List<Tile> path = boss.MovementScript.PathFinder.GetPathAStar(bossTile, tile, boss.MovementScript.IsBossTraversableBeach, boss.MovementScript.MaxHeightDifference, 0);
                if(path != null)
                {
                    boss.MovementScript.CurrentPath = path;
                }
            }
        }
        Debug.LogWarning("no attack tile was found for bossTerraManager");
        return null;


        //Tile closestTile = null;
        //float closestDistance = float.MaxValue;

        //foreach(Tile tile in attackArea)
        //{
        //    if (tile.Type == TileType.Concrete) continue;

        //    float distance = Vector2.Distance(bossTile.GridPosition, tile.GridPosition);
        //    if (distance < closestDistance)
        //    {
        //        closestDistance = distance;
        //        closestTile = tile;
        //    }
        //}
        //return closestTile;
    }

    void Update()
    {
        
    }
}
