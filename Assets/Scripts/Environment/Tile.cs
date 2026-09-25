using System;
using UnityEngine;


namespace Assets.Scripts.Environment
{
    public class Tile
    {
        public Vector3 pos;

        //private Mesh mesh;
        //private MeshFilter meshFilter;

        public Vector3[] vertices;
        public int[] triangles;


        //private void Start()
        //{
        //    meshFilter = gameObject.AddComponent<MeshFilter>();
        //    MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        //    meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        //    mesh = GenerateMesh();
        //    meshFilter.mesh = mesh;
        //}

        public Tile(Vector3 pos)
        {
            this.pos = pos;

            GenerateMeshData();
        }

        private void GenerateMeshData()
        {
            //Vector3[] vertices = new Vector3[]
            //{
            //    new Vector3(0, 0, 0), //Bottom left
            //    new Vector3(1, 0, 0), //Bottom right
            //    new Vector3(0, 0, 1), //Top left
            //    new Vector3(1, 0, 1)  //Top right
            //};
            vertices = new Vector3[]
            {
            new Vector3(-0.5f, 0, -0.5f) + pos, //Bottom left
            new Vector3(0.5f, 0, -0.5f) + pos,  //Bottom right
            new Vector3(-0.5f, 0, 0.5f) + pos,  //Top left
            new Vector3(0.5f, 0, 0.5f)+ pos     //Top right
            };
            triangles = new int[]
            {
            0, 2, 1, //First triangle
            1, 2, 3  //Second triangle
            };
            //mesh.vertices = vertices;
            //mesh.triangles = triangles;
            //mesh.RecalculateNormals();

            //return mesh;
        }

        public float GetHeight()
        {
            return pos.y;
        }
    }
}