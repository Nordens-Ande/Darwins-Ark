using System;
using UnityEngine;

public class Tile : MonoBehaviour
{
    //public Vector3 pos;

    private Mesh mesh;
    private MeshFilter meshFilter;


    //plant varibles
    [SerializeField] private Plant currentPlant = null; //this is the plant that is currently on this tile, if any
    public Plant CurrentPlant => currentPlant;
    public bool HasPlant => currentPlant != null;



    private void Start()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        mesh = GenerateMesh();
        meshFilter.mesh = mesh;
    }

    public Tile(Vector3 pos)
    {

    }

    private Mesh GenerateMesh()
    {
        mesh = new Mesh();

        //Vector3[] vertices = new Vector3[]
        //{
        //    new Vector3(0, 0, 0), //Bottom left
        //    new Vector3(1, 0, 0), //Bottom right
        //    new Vector3(0, 0, 1), //Top left
        //    new Vector3(1, 0, 1)  //Top right
        //};
        Vector3[] vertices = new Vector3[]
{
            new Vector3(-0.5f, 0, -0.5f), //Bottom left
            new Vector3(0.5f, 0, -0.5f), //Bottom right
            new Vector3(-0.5f, 0, 0.5f), //Top left
            new Vector3(0.5f, 0, 0.5f)  //Top right
};
        int[] triangles = new int[]
        {
        0, 2, 1, //First triangle
        1, 2, 3  //Second triangle
        };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    public float GetHeight()
    {
        return 0f;
    }


    ///plant functions
    public void SetPlant(Plant plant)
    {
        currentPlant = plant;
    }

    public void ClearPlant()
    {
        currentPlant = null;
    }





}
