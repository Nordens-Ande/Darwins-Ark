using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Environment
{
    public class Chunk
    {
        private TileManager tileManager = null;
        private IslandNoise noise = null;

        private Tile[,] tiles;
        private Dictionary<TileType, HashSet<Vector2Int>> tileTypes;

        public bool isDirty = false;

        public Vector2 position;
        private int size;

        public List<Vector3> vertices;
        public List<Color> colors;
        public List<int> triangles;

        public int Size => size; 
        public Tile[,] Tiles => tiles;

        [Obsolete] public HashSet<Vector2Int> OceanTiles => GetTiles(TileType.Ocean);
        [Obsolete] public HashSet<Vector2Int> BeachTiles => GetTiles(TileType.Beach);
        [Obsolete] public HashSet<Vector2Int> GrassTiles => GetTiles(TileType.Grass);
        [Obsolete] public HashSet<Vector2Int> RiverTiles => GetTiles(TileType.River);
        [Obsolete] public HashSet<Vector2Int> ConcreteTiles => GetTiles(TileType.Concrete);

        private bool useNoise => noise != null;
        private bool useSlops = true;
        private bool alwaysSlope = false;
        private float maxSlopeHeight = 0.1f;

        public Chunk(Vector2 position, int size)
        {
            tileManager = TileManager.Instance;
            noise = IslandNoise.Instance;

            this.position = position;
            this.size = size;

            //Create tileType hashsets
            tileTypes = new Dictionary<TileType, HashSet<Vector2Int>>();
            InitializeTileHashsets();

            //Creates all the tiles (and assigns to tile hashsets)
            tiles = new Tile[size, size];
            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    TileType type = TileType.Ocean; //type is assigned to avoid compile-time error

                    float y = useNoise ? noise.GetHeight(x + position.x, z + position.y, out type) : 0f;
                    tiles[x, z] = new Tile(new Vector3(x + position.x, y, z + position.y), new Vector3(x, y, z), type);

                    Vector2Int gridPosition = new Vector2Int((int)tiles[x, z].position.x, (int)tiles[x, z].position.z);
                    tileTypes[type].Add(gridPosition);
                }
            }
        }


        // The developer can grab specific tiles either by calling the method or using the thiss[TileType] indexer.
        // You use the indexer like this: Chunk chunk = new Chunk(); oceanTiles = chunk[TileType.Ocean];
        public HashSet<Vector2Int> this[TileType type] => tileTypes[type];
        public HashSet<Vector2Int> GetTiles(TileType type) => tileTypes[type];

        public Tile this[int x, int z, Space space = Space.World] => GetTile(x, z, space);
        public Tile this[Vector2Int positionXZ, Space space = Space.World] => GetTile(positionXZ, space);

        private float GetTileHeight(int x, int z, Space space = Space.World)
        {
            if (space == Space.World)
            {
                x -= (int)position.x;
                z -= (int)position.y;
            }

            //If the position is outside of the chunk, we return a low height inorder to ensure a border is created around every chunk
            if (x > tiles.GetLength(0) - 1 || z > tiles.GetLength(1) - 1 || x < 0 || z < 0)
                return -10f;

            return tiles[x, z].position.y;
        }
        private float GetTileHeight(Vector2Int positionXZ, Space space = Space.World) => GetTileHeight(positionXZ.x, positionXZ.y, space);

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
        public Tile GetTile(Vector2Int positionXZ, Space space = Space.World) => GetTile(positionXZ.x, positionXZ.y, space);


        public void InitializeTileHashsets()
        {
            foreach (TileType tileType in Enum.GetValues(typeof(TileType)))
            {
                tileTypes[tileType] = new HashSet<Vector2Int>();
            }
        }

        private enum Direction
        {
            North,
            NorthWest,
            West,
            SouthWest,
            South,
            SouthEast,
            East,
            NorthEast
        }

        // Divide the logic into more methods (refactoring), this is very annoying and hard to read
        public void GenerateMeshData(bool usePureNoise = false)
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
                    //float[] neighbourHeights = new float[4] 
                    //{ 
                    //    GetTileHeight(x, z + 1, Space.Self), // Noth
                    //    GetTileHeight(x - 1, z, Space.Self), // West
                    //    GetTileHeight(x, z - 1, Space.Self), // South
                    //    GetTileHeight(x + 1, z, Space.Self)  // East
                    //};
                    Dictionary<Direction, float> neighbourHeights = new Dictionary<Direction, float>();
                    foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                    {
                        float value = direction switch
                        {
                            Direction.North     => GetTileHeight(x,     z + 1, Space.Self),
                            Direction.NorthWest => GetTileHeight(x - 1, z + 1, Space.Self),
                            Direction.West      => GetTileHeight(x - 1, z,     Space.Self),
                            Direction.SouthWest => GetTileHeight(x - 1, z - 1, Space.Self),
                            Direction.South     => GetTileHeight(x,     z - 1, Space.Self),
                            Direction.SouthEast => GetTileHeight(x + 1, z - 1, Space.Self),
                            Direction.East      => GetTileHeight(x + 1, z,     Space.Self),
                            Direction.NorthEast => GetTileHeight(x + 1, z + 1, Space.Self),
                            _ => 0
                        };
                        neighbourHeights.Add(direction, value);
                    }

                    Color tileColor = useNoise ? noise.GetColor(new Vector2(tilePos.x, tilePos.z)) : Color.white;

                    //Top face
                    if (!useSlops)
                    {
                        AddFace(
                            new Vector3(-0.5f, 0, 0.5f) + tilePos,      // top-left
                            new Vector3(+0.5f, 0, 0.5f) + tilePos,      // top-right
                            new Vector3(+0.5f, 0, -0.5f) + tilePos,     // bottom-right
                            new Vector3(-0.5f, 0, -0.5f) + tilePos,     // bottom-left
                            tileColor
                        );
                    }
                    else
                    {
                        float currentHeight = tilePos.y;

                        //float nwHeight = Mathf.Max(currentHeight, neighbourHeights[Direction.North], neighbourHeights[Direction.West], neighbourHeights[Direction.NorthWest]);
                        //float neHeight = Mathf.Max(currentHeight, neighbourHeights[Direction.North], neighbourHeights[Direction.East], neighbourHeights[Direction.NorthEast]);
                        //float seHeight = Mathf.Max(currentHeight, neighbourHeights[Direction.South], neighbourHeights[Direction.East], neighbourHeights[Direction.SouthEast]);
                        //float swHeight = Mathf.Max(currentHeight, neighbourHeights[Direction.South], neighbourHeights[Direction.West], neighbourHeights[Direction.SouthWest]);
                        //if (alwaysSlope)
                        //{
                        //    nwHeight = Mathf.Min(currentHeight + maxSlopeHeight, nwHeight);
                        //    neHeight = Mathf.Min(currentHeight + maxSlopeHeight, neHeight);
                        //    seHeight = Mathf.Min(currentHeight + maxSlopeHeight, seHeight);
                        //    swHeight = Mathf.Min(currentHeight + maxSlopeHeight, swHeight);
                        //}
                        //else
                        //{
                        //    nwHeight = nwHeight - currentHeight < maxSlopeHeight ? nwHeight : currentHeight;
                        //    neHeight = neHeight - currentHeight < maxSlopeHeight ? neHeight : currentHeight;
                        //    seHeight = seHeight - currentHeight < maxSlopeHeight ? seHeight : currentHeight;
                        //    swHeight = swHeight - currentHeight < maxSlopeHeight ? swHeight : currentHeight;
                        //}

                        float nwHeight;
                        float neHeight;
                        float seHeight;
                        float swHeight;

                        nwHeight = Closest(currentHeight, neighbourHeights[Direction.North], neighbourHeights[Direction.West], neighbourHeights[Direction.NorthWest]);
                        neHeight = Closest(currentHeight, neighbourHeights[Direction.North], neighbourHeights[Direction.East], neighbourHeights[Direction.NorthEast]);
                        seHeight = Closest(currentHeight, neighbourHeights[Direction.South], neighbourHeights[Direction.East], neighbourHeights[Direction.SouthEast]);
                        swHeight = Closest(currentHeight, neighbourHeights[Direction.South], neighbourHeights[Direction.West], neighbourHeights[Direction.SouthWest]);

                        nwHeight = Mathf.Max(currentHeight, nwHeight);
                        neHeight = Mathf.Max(currentHeight, neHeight);
                        seHeight = Mathf.Max(currentHeight, seHeight);
                        swHeight = Mathf.Max(currentHeight, swHeight);


                        int heightCount = (Mathf.Approximately(nwHeight, currentHeight) ? 0 : 1)
                                        + (Mathf.Approximately(neHeight, currentHeight) ? 0 : 1)
                                        + (Mathf.Approximately(seHeight, currentHeight) ? 0 : 1)
                                        + (Mathf.Approximately(swHeight, currentHeight) ? 0 : 1);

                        //bool isFlat = Mathf.Approximately(nwHeight, currentHeight) 
                        //           && Mathf.Approximately(neHeight, currentHeight) 
                        //           && Mathf.Approximately(seHeight, currentHeight) 
                        //           && Mathf.Approximately(swHeight, currentHeight);

                        if (heightCount <= 1)
                        {
                            AddFace(
                                new Vector3(-0.5f, currentHeight, 0.5f) + new Vector3(tilePos.x, 0, tilePos.z),
                                new Vector3(+0.5f, currentHeight, 0.5f) + new Vector3(tilePos.x, 0, tilePos.z),
                                new Vector3(+0.5f, currentHeight, -0.5f) + new Vector3(tilePos.x, 0, tilePos.z),
                                new Vector3(-0.5f, currentHeight, -0.5f) + new Vector3(tilePos.x, 0, tilePos.z),
                                tileColor
                            );
                        }
                        else
                        {
                            AddSlopeTop(
                                new Vector3(tilePos.x, 0f, tilePos.z),
                                currentHeight,
                                nwHeight,
                                neHeight,
                                seHeight,
                                swHeight,
                                tileColor
                            );
                        }
                    }


                    float tileHeight = tilePos.y;

                    for (int i = 0; i < 4; i++)
                    {
                        float neighbourHeight = neighbourHeights[(Direction)(2*i)]; //PLEASE FIX THIS, this is so wrong

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

        private void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            int index = vertices.Count;

            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);

            colors.Add(color);
            colors.Add(color);
            colors.Add(color);

            triangles.Add(index + 0);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
        }

        private void AddSlopeTop(
            Vector3 tilePos,
            float currentHeight,
            float nwHeight,
            float neHeight,
            float seHeight,
            float swHeight,
            Color color)
        {
            //tilePos.y -= currentHeight;

            Vector3 nw = new Vector3(-0.5f, nwHeight, 0.5f) + tilePos;
            Vector3 ne = new Vector3(0.5f, neHeight, 0.5f) + tilePos;
            Vector3 se = new Vector3(0.5f, seHeight, -0.5f) + tilePos;
            Vector3 sw = new Vector3(-0.5f, swHeight, -0.5f) + tilePos;

            AddFace(nw, ne, se, sw, color);

            // West side.
            if (!Mathf.Approximately(nwHeight, swHeight))
            {
                AddTriangle(nw, sw, new Vector3(-0.5f, currentHeight, 0.5f) + tilePos, color);
            }

            // East side.
            if (!Mathf.Approximately(neHeight, seHeight))
            {
                AddTriangle(ne, new Vector3(0.5f, currentHeight, 0.5f) + tilePos, se, color);
            }

            // North side.
            if (!Mathf.Approximately(nwHeight, neHeight))
            {
                AddTriangle(nw, new Vector3(-0.5f, currentHeight, 0.5f) + tilePos, ne, color);
            }

            // South side.
            if (!Mathf.Approximately(swHeight, seHeight))
            {
                AddTriangle(sw, se, new Vector3(-0.5f, currentHeight, -0.5f) + tilePos, color);
            }
        }

        // Returns the Closest value to the target from the values list
        private float Closest(float target, params float[] values)
        {
            // Orders values depedning on the delta value, if the delta is smaller, the higher up it lands in the list (i.e giving us the closest value)
            return values.OrderBy(value => Mathf.Abs(value - target)).First();
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
        public void ModifyTile(Vector2Int positionXZ, float newY, Space space = Space.World) => ModifyTile(positionXZ.x, positionXZ.y, newY, space);





    }
}
