using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LineMesh : MonoBehaviour
{
    private Mesh mesh;
    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<int> indices = new List<int>();
    
    private void Awake()
    {
        Mesh mesh = new Mesh()
        {
            name = "Line Mesh"
        };

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    public void AddLine(Vector3 start, Vector3 end)
    {
        int index = vertices.Count;

        vertices.Add(start);
        vertices.Add(end);

        indices.Add(index);
        indices.Add(index + 1);
    }

    public void Rebuild()
    {
        mesh.Clear();

        mesh.SetVertices(vertices);
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        
        mesh.RecalculateBounds();
    }

    public void ClearLines()
    {
        vertices.Clear();
        indices.Clear();
        mesh.Clear();
    }

    void OnDestroy()
    {
        if (mesh != null)
            Destroy(mesh);
    }
}
