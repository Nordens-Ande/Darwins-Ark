using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Environment
{
    public class Chunk
    {
        public bool isDirty = false;

        public Vector2 position;

        private int size;
        private Tile[,] tiles;

        public List<Vector3> vertices;
        public List<int> triangles;

        public Chunk(Vector2 position, int size)
        {
            this.position = position;

            this.size = size;

            tiles = new Tile[size, size];
            for (int x = 0; x < size; x++)
                for (int z = 0; z < size; z++)
                    tiles[x, z] = new Tile(new Vector3(x, 0f/*UnityEngine.Random.Range(0f, 1f)*/, z));

            vertices = new List<Vector3>();
            triangles = new List<int>();
        }


        public void GenerateMeshData()
        {
            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    Vector3 tilePos = tiles[x, z].pos + new Vector3(position.x, 0, position.y);
                    float[] neighbourHeights = new float[4] { GetTileHeight(x, z + 1), GetTileHeight(x - 1, z), GetTileHeight(x, z - 1), GetTileHeight(x + 1, z) }; //NWSE 

                    //vertices.AddRange();

                    //Vector3[] topPlane = new Vector3[4]
                    //{
                    //    new Vector3(-0.5f, 0, -0.5f) + tilePos, //Bottom left
                    //    new Vector3(0.5f, 0, -0.5f) + tilePos,  //Bottom right
                    //    new Vector3(-0.5f, 0, 0.5f) + tilePos,  //Top left
                    //    new Vector3(0.5f, 0, 0.5f) + tilePos    //Top right
                    //};
                    //int[] topTriangle = new int[]
                    //{
                    //    (x + z) + 0, (x + z) + 2, (x + z) + 1,  //First triangle
                    //    (x + z) + 1, (x + z) + 2, (x + z) + 3   //Second triangle
                    //};

                    //Top face
                    AddFace(
                        new Vector3(-0.5f, 0, 0.5f) + tilePos,  //top-left
                        new Vector3(+0.5f, 0, 0.5f) + tilePos,   //top-right
                        new Vector3(+0.5f, 0, -0.5f) + tilePos,  //bottom-right
                        new Vector3(-0.5f, 0, -0.5f) + tilePos  //bottom-left
                    );

                    float tileHeight = tilePos.y;

                    for (int i = 0; i < 4; i++)
                    {
                        float neighbourHeight = neighbourHeights[i];

                        if (neighbourHeight >= tileHeight)
                            continue;

                        //float low = Mathf.Min(tileHeight, neighbourHeight);
                        //float high = Mathf.Max(tileHeight, neighbourHeight);

                        float low = neighbourHeight;
                        float high = tileHeight;

                        switch (i)
                        {
                            case 0: // +Z (North)
                                AddFace(
                                    new Vector3(tilePos.x - 0.5f, low, tilePos.z + 0.5f),
                                    new Vector3(tilePos.x + 0.5f, low, tilePos.z + 0.5f),
                                    new Vector3(tilePos.x + 0.5f, high, tilePos.z + 0.5f),
                                    new Vector3(tilePos.x - 0.5f, high, tilePos.z + 0.5f)
                                );
                                break;

                            case 1: // -X (West)
                                AddFace(
                                    new Vector3(tilePos.x - 0.5f, low, tilePos.z - 0.5f),
                                    new Vector3(tilePos.x - 0.5f, low, tilePos.z + 0.5f),
                                    new Vector3(tilePos.x - 0.5f, high, tilePos.z + 0.5f),
                                    new Vector3(tilePos.x - 0.5f, high, tilePos.z - 0.5f)
                                );
                                break;

                            case 2: // -Z (South)
                                AddFace(
                                    new Vector3(tilePos.x + 0.5f, low, tilePos.z - 0.5f),
                                    new Vector3(tilePos.x - 0.5f, low, tilePos.z - 0.5f),
                                    new Vector3(tilePos.x - 0.5f, high, tilePos.z - 0.5f),
                                    new Vector3(tilePos.x + 0.5f, high, tilePos.z - 0.5f)
                                );
                                break;

                            case 3: // +X (East)
                                AddFace(
                                    new Vector3(tilePos.x + 0.5f, low, tilePos.z + 0.5f),
                                    new Vector3(tilePos.x + 0.5f, low, tilePos.z - 0.5f),
                                    new Vector3(tilePos.x + 0.5f, high, tilePos.z - 0.5f),
                                    new Vector3(tilePos.x + 0.5f, high, tilePos.z + 0.5f)
                                );
                                break;
                        }

                    }
                }
            }
        }

        private float GetTileHeight(int x, int z)
        {
            //If the position is outside of the chunk, we return a low height inorder to ensure a border is created around every chunk
            if (x > tiles.GetLength(0) - 1 || z > tiles.GetLength(1) - 1 || x < 0 || z < 0)
                return -10f;

            return tiles[x, z].pos.y;
        }

        //Ideally we would not like to use abcd variables, but I cannot think of any other names that would work in this context
        private void AddFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int index = vertices.Count;

            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);

            triangles.Add(index + 0);
            triangles.Add(index + 1);
            triangles.Add(index + 2);

            triangles.Add(index + 0);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
        }
    }
}
