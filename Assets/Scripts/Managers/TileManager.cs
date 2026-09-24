using NUnit.Framework.Internal;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager Instance = null;

    private Mesh mesh;
    private MeshFilter meshFilter;

    public List<Tile> tiles = new List<Tile>();

    private void Awake()
    {
        if (Instance == null)
            TileManager.Instance = this;
    }

    void Start()
    {
        //GameObject tile = new GameObject("testTile", typeof(Tile));

        meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        for (int x = 0; x < 20; x++)
        {
            for (int z = 0; z < 20; z++)
            {
                //GameObject tile = new GameObject($"Tile (x:{x}, z:{z})", typeof(Tile));
                //tile.transform.parent = transform;
                //tile.transform.localPosition = new Vector3(x, 0, z);
                
                tiles.Add(new Tile(new Vector3(x, 0/*Random.Range(0f, 1f)*/, z)));
            }
        }

        BuildMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void BuildMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        foreach (Tile tile in tiles)
        {
            int vertexOffset = vertices.Count;

            vertices.AddRange(tile.vertices);

            foreach (int index in tile.triangles)
            {
                triangles.Add(index + vertexOffset);
            }
        }

        Mesh mesh = new Mesh();

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
    }


    //public Tile GetTile(float x, float z)
    //{
    //    foreach (Tile tile in tiles)
    //    {
    //        if (tile.transform.localPosition.x == x && tile.transform.position.z == z)
    //            return tile;
    //    }
    //    return null;
    //}
    //public Tile GetTile(Vector2 posXZ)
    //{
    //    return GetTile(posXZ.x, posXZ.y);
    //}
    //public Tile GetTile(Vector3 pos)
    //{
    //    return GetTile(pos.x, pos.z);
    //}
}
