using Assets.Scripts.Environment;
using System.Collections.Generic;
using UnityEngine;

public class GridMesh : LineMesh
{
    private IslandNoise noise = null;
    private TileManager tileManager = null;

    public float heightOffset;
    [SerializeField, ReadOnly] private HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();

    private int chunkSize;

    [SerializeField, ReadOnly] private bool isDirty = false;

    private void Start()
    {
        noise = IslandNoise.Instance;
        tileManager = TileManager.Instance;
        chunkSize = TileManager.Instance.ChunkSize;

        //tiles = new Vector2Int[chunkSize, chunkSize];
        //Rebuild();
    }

    private void FixedUpdate()
    {
        if (isDirty)
        {
            Rebuild();

            isDirty = false;
        }

    }

    public void NewTiles(HashSet<Vector2Int> tiles)
    {
        isDirty = true;

        this.tiles = tiles;
    }

    public void AddTile(Vector2Int tile)
    {
        isDirty = true;

        tiles.Add(tile);
    }

    public void RemoveTile(Vector2Int tile)
    {
        isDirty = true;

        tiles.Remove(tile);
    }

    public void ClearTiles()
    {
        isDirty = true;

        tiles.Clear();
    }

    //// Generates all the grid points purely from noise (using IslandNoise)
    //public void GenerateGridPointsFromNoise(Vector2Int start, Vector2Int end, params TileType[] tileFilter)
    //{

    //}

    //// Generates all the grid points from the current chunk tiles (this can only be done if the chunks information has been generated)
    //public void GenerateGridPointsFromChunk(Vector2Int[,] tiles, params TileType[] tileFilter)
    //{

    //}

    public override void Rebuild()
    {
        tileManager = tileManager ?? TileManager.Instance;
        if (tileManager == null)
        {
            Debug.LogWarning("Cannot rebuild GridMesh, TileManager does not exist or could not be found");
            return;
        }

        TilesRebuild();

        base.Rebuild();
    }

    private void TilesRebuild()
    {
        //Debug.Log($"Rebuilding: {gameObject.name}, {gameObject.transform.parent.name}");

        //Debug.Log($"Parent null: {transform.parent == null}");
        //Debug.Log($"tiles null: {tiles == null}");
        //Debug.Log($"tileManager null: {tileManager == null}");
        //Debug.Log($"meshData null: {meshData == null}");

        ClearLines();

        foreach (Vector2Int gridPos in tiles)
        {
            Tile tile = tileManager.GetTile(gridPos);
            //Debug.Log(tile.position);
            //Debug.Log(tile == null);
            if (tile == null)
                continue;

            Vector3 tilePos = tile.Position;

            meshData.AddFace(
                new Vector3(-0.5f, heightOffset, 0.5f) + tilePos,   // top-left
                new Vector3(+0.5f, heightOffset, 0.5f) + tilePos,   // top-right
                new Vector3(+0.5f, heightOffset, -0.5f) + tilePos,  // bottom-right
                new Vector3(-0.5f, heightOffset, -0.5f) + tilePos   // bottom-left);
            );
        }
    }
}