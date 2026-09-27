using UnityEngine;

public class TileGridOverlay : MonoBehaviour
{
    [SerializeField] private float lineHeight = 0.02f;
    [SerializeField] private float lineWidth = 0.025f;

    private Material lineMaterial;

    public void Setup(int chunkSize, Material material)
    {
        lineMaterial = material;
        CreateGrid(chunkSize);
    }

    private void CreateGrid(int size)
    {
        // Vertical lines
        for (int x = 0; x <= size; x++)
        {
            CreateLine(
                new Vector3(x, lineHeight, 0),
                new Vector3(x, lineHeight, size)
            );
        }

        // Horizontal lines
        for (int z = 0; z <= size; z++)
        {
            CreateLine(
                new Vector3(0, lineHeight, z),
                new Vector3(size, lineHeight, z)
            );
        }
    }

    private void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObject = new GameObject("TileLine");
        lineObject.transform.SetParent(transform, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();

        line.positionCount = 2;
        line.useWorldSpace = false;

        line.SetPosition(0, start);
        line.SetPosition(1, end);

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        if (lineMaterial != null)
        {
            line.material = lineMaterial;
        }

        line.startColor = Color.white;
        line.endColor = Color.white;
    }
}