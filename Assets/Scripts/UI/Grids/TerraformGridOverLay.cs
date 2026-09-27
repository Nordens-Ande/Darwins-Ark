using UnityEngine;
using Assets.Scripts.Environment;

public class TerraformTileGridOverlay : MonoBehaviour
{
    [SerializeField] private float heightOffset = 0.02f;
    [SerializeField] private float lineWidth = 0.025f;

    private Material lineMaterial;
    private Chunk chunk;


    public void Setup(Chunk chunkData, Material material)
    {
        chunk = chunkData;
        lineMaterial = material;

        RefreshGrid();
    }

    public void RefreshGrid()
    {
        // Remove old squares
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

        GameObject lineObject = new GameObject("TileSquare");
        lineObject.transform.SetParent(transform, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();

        line.useWorldSpace = true;

        // 4 corners
        line.positionCount = 4;

        line.SetPosition(0, topLeft);
        line.SetPosition(1, topRight);
        line.SetPosition(2, bottomRight);
        line.SetPosition(3, bottomLeft);

        // Connect last point back to first
        line.loop = true;

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        if (lineMaterial != null)
            line.material = lineMaterial;

        line.startColor = Color.red;
        line.endColor = Color.red;

    }
}
    
