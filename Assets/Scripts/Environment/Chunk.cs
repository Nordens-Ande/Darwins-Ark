using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Environment
{
    public class Chunk
    {
        HashSet<Vector2Int> oceanTiles;
        HashSet<Vector2Int> beachTiles;
        HashSet<Vector2Int> grassTiles;
        HashSet<Vector2Int> riverTiles;
        HashSet<Vector2Int> concreteTiles;

        public bool isDirty = false;

        public Vector2 position;

        private int size;
        private Tile[,] tiles;

        public List<Vector3> vertices;
        public List<Color> colors;
        public List<int> triangles;

        public int Size => size;
        public Tile[,] Tiles => tiles;
        public HashSet<Vector2Int> OceanTiles => oceanTiles;
        public HashSet<Vector2Int> BeachTiles => beachTiles;
        public HashSet<Vector2Int> GrassTiles => grassTiles;
        public HashSet<Vector2Int> RiverTiles => riverTiles;
        public HashSet<Vector2Int> ConcreteTiles => concreteTiles;

        public bool useNoise = true;

        public Chunk(Vector2 position, int size)
        {
            InstantiateHashsets();
            this.position = position;

            this.size = size;

            tiles = new Tile[size, size];
            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    TileType type = TileType.Ocean;
                    float y = useNoise ? IslandNoise.Instance.GetHeight(x + position.x, z + position.y, out type) : 0f;
                    tiles[x, z] = new Tile(new Vector3(x + position.x, y, z + position.y), new Vector3(x, y, z), type);

                    if(type is TileType.Ocean)
                    {
                        oceanTiles.Add(new Vector2Int((int)tiles[x, z].position.x, (int)tiles[x, z].position.z));
                    }
                    else if (type is TileType.Beach)
                    {
                        beachTiles.Add(new Vector2Int((int)tiles[x, z].position.x, (int)tiles[x, z].position.z));
                    }
                    else if (type is TileType.Grass)
                    {
                        grassTiles.Add(new Vector2Int((int)tiles[x, z].position.x, (int)tiles[x, z].position.z));
                    }
                    else if (type is TileType.River)
                    {
                        riverTiles.Add(new Vector2Int((int)tiles[x, z].position.x, (int)tiles[x, z].position.z));
                    }
                    else if (type is TileType.Concrete)
                    {
                        concreteTiles.Add(new Vector2Int((int)tiles[x, z].position.x, (int)tiles[x, z].position.z));
                    }
                }
            }
        }
        // Using for ChunkGrid access -----------------
        public int size_
        {
            get { return size; }
        }
        // ----------------------------------------------

        public void InstantiateHashsets()
        {
            oceanTiles = new HashSet<Vector2Int>();
            beachTiles = new HashSet<Vector2Int>();
            grassTiles = new HashSet<Vector2Int>();
            riverTiles = new HashSet<Vector2Int>();
            concreteTiles = new HashSet<Vector2Int>();
        }

        public void GenerateMeshData()
        {
            vertices = new List<Vector3>();
            colors = new List<Color>();
            triangles = new List<int>();

            isDirty = false;

            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    Vector3 tilePos = tiles[x, z].position/* + new Vector3(position.x, 0, position.y)*/;
                    float[] neighbourHeights = new float[4] { GetTileHeight(x, z + 1), GetTileHeight(x - 1, z), GetTileHeight(x, z - 1), GetTileHeight(x + 1, z) }; //NWSE 
                    Color tileColor = useNoise ? IslandNoise.Instance.GetColor(new Vector2(tilePos.x, tilePos.z)) : Color.white;

                    //Top face
                    AddFace(
                        new Vector3(-0.5f, 0, 0.5f) + tilePos,  //top-left
                        new Vector3(+0.5f, 0, 0.5f) + tilePos,  //top-right
                        new Vector3(+0.5f, 0, -0.5f) + tilePos, //bottom-right
                        new Vector3(-0.5f, 0, -0.5f) + tilePos,  //bottom-left
                        tileColor
                    );

                    float tileHeight = tilePos.y;

                    for (int i = 0; i < 4; i++)
                    {
                        float neighbourHeight = neighbourHeights[i];

                        if (neighbourHeight >= tileHeight)
                            continue;

                        float low = neighbourHeight;
                        float high = tileHeight;

                        switch (i)
                        {
                            case 0: // +Z (North)
                                AddFace(
                                    new Vector3(tilePos.x - 0.5f, low, tilePos.z + 0.5f),
                                    new Vector3(tilePos.x + 0.5f, low, tilePos.z + 0.5f),
                                    new Vector3(tilePos.x + 0.5f, high, tilePos.z + 0.5f),
                                    new Vector3(tilePos.x - 0.5f, high, tilePos.z + 0.5f),
                                    tileColor
                                );
                                break;

                            case 1: // -X (West)
                                AddFace(
                                    new Vector3(tilePos.x - 0.5f, low, tilePos.z - 0.5f),
                                    new Vector3(tilePos.x - 0.5f, low, tilePos.z + 0.5f),
                                    new Vector3(tilePos.x - 0.5f, high, tilePos.z + 0.5f),
                                    new Vector3(tilePos.x - 0.5f, high, tilePos.z - 0.5f),
                                    tileColor
                                );
                                break;

                            case 2: // -Z (South)
                                AddFace(
                                    new Vector3(tilePos.x + 0.5f, low, tilePos.z - 0.5f),
                                    new Vector3(tilePos.x - 0.5f, low, tilePos.z - 0.5f),
                                    new Vector3(tilePos.x - 0.5f, high, tilePos.z - 0.5f),
                                    new Vector3(tilePos.x + 0.5f, high, tilePos.z - 0.5f),
                                    tileColor
                                );
                                break;

                            case 3: // +X (East)
                                AddFace(
                                    new Vector3(tilePos.x + 0.5f, low, tilePos.z + 0.5f),
                                    new Vector3(tilePos.x + 0.5f, low, tilePos.z - 0.5f),
                                    new Vector3(tilePos.x + 0.5f, high, tilePos.z - 0.5f),
                                    new Vector3(tilePos.x + 0.5f, high, tilePos.z + 0.5f),
                                    tileColor
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

            return tiles[x, z].position.y;
        }

        public Tile GetTile(int x, int z, Space space = Space.World)
        {
            if (space == Space.World)
            {
                x -= (int)position.x;
                z -= (int)position.y;
            }

            if (x > tiles.GetLength(0) - 1 || z > tiles.GetLength(1) - 1 || x < 0 || z < 0)
                return null;

            return tiles[x, z];
        }

        //Ideally we would not like to use abcd variables, but I cannot think of any other names that would work in this context
        private void AddFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color)
        {
            int index = vertices.Count;

            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);

            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);

            triangles.Add(index + 0);
            triangles.Add(index + 1);
            triangles.Add(index + 2);

            triangles.Add(index + 0);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
        }

        public void ModifyTile(int x, int z, float newY, Space space = Space.World)
        {
            if (space == Space.World)
            {
                x -= (int)position.x;
                z -= (int)position.y;
            }

            if (x > size - 1 || z > size - 1 || x < 0 || z < 0)
                return;

            isDirty = true;

            //Vector3 localPosition = new Vector3();
            tiles[x, z].position.y = newY;
            tiles[x, z].localPosition.y = newY;
        }
        //public void ModifyTile(Vector2 tilePos)
        //{

        //}

        
    }
}
