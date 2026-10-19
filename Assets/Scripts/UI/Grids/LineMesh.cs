using Assets.Scripts.Environment;
using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class LineMeshData
{
    public readonly List<Vector3> vertices = new List<Vector3>();
    public readonly List<int> indices = new List<int>();

    //public LineMeshData()
    //{
    //    vertices ;
    //    indices ;
    //}

    public void AddLine(Vector3 start, Vector3 end)
    {
        int index = vertices.Count;

        vertices.Add(start);
        vertices.Add(end);

        indices.Add(index);
        indices.Add(index + 1);
    }

    public void AddFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        int index = vertices.Count;

        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        vertices.Add(d);

        indices.Add(index + 0);
        indices.Add(index + 1);

        indices.Add(index + 1);
        indices.Add(index + 2);

        indices.Add(index + 2);
        indices.Add(index + 3);
        
        indices.Add(index + 3);
        indices.Add(index + 0);
    }

    public void Clear()
    {
        vertices.Clear();
        indices.Clear();
    }
}

public class LineMesh : MonoBehaviour
{
    private Mesh mesh;
    public LineMeshData meshData = new LineMeshData();
    //private readonly List<Vector3> vertices = new List<Vector3>();
    //private readonly List<int> indices = new List<int>();

    private void Awake()
    {
        mesh = new Mesh()
        {
            name = "Line Mesh"
        };

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    //public void AddLine(Vector3 start, Vector3 end)
    //{
    //    int index = vertices.Count;

    //    vertices.Add(start);
    //    vertices.Add(end);

    //    indices.Add(index);
    //    indices.Add(index + 1);
    //}

    public virtual void Rebuild()
    {
        //mesh.Clear();

        //mesh.SetVertices(meshData.vertices);
        //mesh.SetIndices(meshData.indices, MeshTopology.Lines, 0);

        //mesh.RecalculateBounds();


        //TilesRebuild();

        MeshRebuild();
    }

    //private void TilesRebuild()
    //{

    //}
    private void MeshRebuild()
    {
        mesh.Clear();

        mesh.SetVertices(meshData.vertices);
        mesh.SetIndices(meshData.indices, MeshTopology.Lines, 0);

        mesh.RecalculateBounds();
    }

    public void ClearLines()
    {
        //vertices.Clear();
        //indices.Clear();
        meshData.Clear();
        mesh.Clear();
    }

    void OnDestroy()
    {
        if (mesh != null)
            Destroy(mesh);
    }
}

