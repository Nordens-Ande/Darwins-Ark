using UnityEngine;

public class ChunkGridOverlay : MonoBehaviour
{
    [SerializeField] private float lineHeight = 0.03f;
    [SerializeField] private float lineWidth = 0.08f;

    private Material lineMaterial;

    public void Setup(int chunkSize, Material material)
    {
        lineMaterial = material;
        CreateBorder(chunkSize);
    }

    private void CreateBorder(int size)
    {
        // Bottom
        CreateLine(
            new Vector3(0, lineHeight, 0),
            new Vector3(size, lineHeight, 0)
        );

        // Right
        CreateLine(
            new Vector3(size, lineHeight, 0),
            new Vector3(size, lineHeight, size)
        );

        // Top
        CreateLine(
            new Vector3(size, lineHeight, size),
            new Vector3(0, lineHeight, size)
        );

        // Left
        CreateLine(
            new Vector3(0, lineHeight, size),
            new Vector3(0, lineHeight, 0)
        );
    }

    private void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObject = new GameObject("ChunkLine");
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

        line.startColor = Color.yellow;
        line.endColor = Color.yellow;
    }
}