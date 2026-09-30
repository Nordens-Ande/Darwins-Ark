using UnityEngine;
using Assets.Scripts.Environment;

public class ChunkGridOverlay : MonoBehaviour
{
    [SerializeField] private float lineHeight = 0.03f;
    [SerializeField] private float lineWidth = 0.08f;

    private Material lineMaterial;

    public void Setup(Chunk chunk, int chunkSize, Material material)
    {
        lineMaterial = material;
        CreateBorder(
            chunk.position,
            chunkSize);
    }

    private void CreateBorder(Vector2 chunkPosition, int size)
    {
        float minX = chunkPosition.x - 0.5f;
        float maxX = chunkPosition.x + size - 0.5f;

        float minZ = chunkPosition.y - 0.5f;
        float maxZ = chunkPosition.y + size - 0.5f;

        CreateLine(
            new Vector3(minX, lineHeight, minZ),
            new Vector3(maxX, lineHeight, minZ)
        );

        CreateLine(
            new Vector3(maxX, lineHeight, minZ),
            new Vector3(maxX, lineHeight, maxZ)
        );

        CreateLine(
            new Vector3(maxX, lineHeight, maxZ),
            new Vector3(minX, lineHeight, maxZ)
        );

        CreateLine(
            new Vector3(minX, lineHeight, maxZ),
            new Vector3(minX, lineHeight, minZ)
        ); ;
    }

    private void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObject = new GameObject("ChunkLine");
        lineObject.transform.SetParent(transform, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();

        line.positionCount = 2;
        line.useWorldSpace = true;

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