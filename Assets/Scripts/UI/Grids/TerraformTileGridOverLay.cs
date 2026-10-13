using Assets.Scripts.Environment;
using System.Collections.Generic;
using UnityEngine;

public enum TileGridVisibilityMode
{
    AllTiles,
    HighlightedAndSelected
}
public class TerraformTileGridOverlay : MonoBehaviour
{
    //[SerializeField]
    //private TileGridVisibilityMode visibilityMode =
    //    TileGridVisibilityMode.AllTiles;

    //public TileGridVisibilityMode VisibilityMode;

    [SerializeField] private float heightOffset = 0.02f;
    [SerializeField] private float lineWidth = 0.025f;

    private Material lineMaterial;
    private Chunk chunk;

    private Dictionary<Tile, LineRenderer> tileLines =
        new Dictionary<Tile, LineRenderer>();

    public void Setup(Chunk chunkData, Material material)
    {
        chunk = chunkData;
        lineMaterial = material;

        RefreshGrid();
    }

    public void RefreshGrid()
    {
        tileLines.Clear();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        Tile[,] tiles = chunk.Tiles;

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int z = 0; z < tiles.GetLength(1); z++)
            {
                CreateTileSquare(tiles[x, z]);
            }
        }
    }

    private void CreateTileSquare(Tile tile)
    {
        Vector3 tilePos = tile.position;

        float y = tilePos.y + heightOffset;

        Vector3 topLeft =
            new Vector3(tilePos.x - 0.5f, y, tilePos.z + 0.5f);

        Vector3 topRight =
            new Vector3(tilePos.x + 0.5f, y, tilePos.z + 0.5f);

        Vector3 bottomRight =
            new Vector3(tilePos.x + 0.5f, y, tilePos.z - 0.5f);

        Vector3 bottomLeft =
            new Vector3(tilePos.x - 0.5f, y, tilePos.z - 0.5f);

        GameObject lineObject =
            new GameObject("TileSquare");

        lineObject.transform.SetParent(
            transform,
            false
        );

        LineRenderer line =
            lineObject.AddComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.positionCount = 4;
        line.loop = true;

        line.SetPosition(0, topLeft);
        line.SetPosition(1, topRight);
        line.SetPosition(2, bottomRight);
        line.SetPosition(3, bottomLeft);

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        if (lineMaterial != null)
        {
            line.material = lineMaterial;
        }

        tileLines.Add(tile, line);

        // Default grid color
        SetTileColor(tile, Color.red);
    }

    public void ClearSelectionColors()
    {
        foreach (Tile tile in tileLines.Keys)
        {
            SetTileColor(tile, Color.red);
        }
    }

    public void SetTileVisible(Tile tile, bool visible)
    {
        if (!tileLines.TryGetValue(tile, out LineRenderer line))
            return;

        line.enabled = visible;
    }
    public void SetTileVisible(Vector2 posXZ, bool visible) 
        => SetTileVisible(TileManager.Instance.GetTile(posXZ), visible);

    public void SetTileVisible(Vector3 posXZ, bool visible) 
        => SetTileVisible(TileManager.Instance.GetTile(posXZ), visible);

    public void SetTileColor(Tile tile, Color color)
    {
        if (!tileLines.TryGetValue(tile, out LineRenderer line))
            return;

        // LineRenderer vertex color
        line.startColor = color;
        line.endColor = color;

        // Material color
        Material material = line.material;

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }
    public void SetTileColor(Vector2 posXZ, Color color)
        => SetTileColor(TileManager.Instance.GetTile(posXZ), color);

    public void SetTileColor(Vector3 posXZ, Color color)
        => SetTileColor(TileManager.Instance.GetTile(posXZ), color);
}
    
