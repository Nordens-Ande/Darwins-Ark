using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Environment
{

    public class LakeGenerator : SingletonManager<LakeGenerator>
    {
        private enum LakeNeighborhood
        {
            Moore,
            VonNeumann,
            VonNeumannExtended,
            Custom
        }
        public class Lake
        {
            public HashSet<Vector2Int> lakeTiles = new HashSet<Vector2Int>();
            public float lowestNeighborGroundTile;
            public bool hasTouchedOcean;

            public Lake(HashSet<Vector2Int> lakeTiles, float lowestNeighborGroundTile, bool hasTouchedOcean)
            {
                this.lakeTiles = lakeTiles;
                this.lowestNeighborGroundTile = lowestNeighborGroundTile;
                this.hasTouchedOcean = hasTouchedOcean;
            }
        }

        private IslandNoise noise;

        // True = Lake Tile
        private bool[,] lakeMap;
        //public bool[,] LakeMap => lakeMap;

        [SerializeField, ReadOnly] private int seedOffset = 0;

        [Header("Lakes")]
        // What tiles the lake can be generated on
        [SerializeField] private List<Lake> lakeObjects = new List<Lake>();
        [SerializeField] private TileType[] lakeMask = new TileType[] { TileType.Grass, TileType.Beach };

        [Space]

        [SerializeField] private int lakeIterations = 6;
        [SerializeField] private float initialLakeChance = 0.4f;

        [Space]

        public bool resolveLakeAmount = true;
        [SerializeField] private int minLakeObjectCount = 2;
        [SerializeField] private int maxLakeTileCount = 18;

        [Space]

        [SerializeField] private LakeNeighborhood lakeNeighborhood;
        [SerializeField] private List<Vector2Int> customNeighborhood = new List<Vector2Int>();
        [SerializeField] private int MaxLakeNeighborsForLakeToLand = 3;
        [SerializeField] private int MinLakeNeighborsForLandToLake = 5;

        [Space]

        [SerializeField] private bool drawGizmo = false;


        // Neighborhoods
        private List<Vector2Int> mooreNeighborhood = new List<Vector2Int>()
        {
            new Vector2Int(-1, 1),  new Vector2Int(0, 1),   new Vector2Int(1, 1),
            new Vector2Int(-1, 0),  /*WE IGNORE MIDDLE*/    new Vector2Int(1, 0),
            new Vector2Int(-1, -1), new Vector2Int(0, -1),  new Vector2Int(1, -1)
        };
        private List<Vector2Int> vonNeumannNeighborhood = new List<Vector2Int>()
        {
                                    new Vector2Int(0, 1),
            new Vector2Int(-1, 0),  /*WE IGNORE MIDDLE*/    new Vector2Int(1, 0),
                                    new Vector2Int(0, -1)
        };
        private List<Vector2Int> vonNeumannNeighborhoodExtended = new List<Vector2Int>()
        {
                                                            new Vector2Int(0, 2),
                                                            new Vector2Int(0, 1),
            new Vector2Int(-2, 0),  new Vector2Int(-1, 0),  /*WE IGNORE MIDDLE*/    new Vector2Int(1, 0),   new Vector2Int(2, 0),
                                                            new Vector2Int(0, -1),
                                                            new Vector2Int(0, -2),

        };


        //public LakeGenerator(IslandNoise noise)
        //{
        //    this.noise = noise;

        //    GenerateLakes();

        //    if (resolveLakeAmount)
        //        ResolveLakes();
        //}

        private void Start()
        {
            noise = IslandNoise.Instance;

            GenerateLakes();

            if (resolveLakeAmount)
                ResolveLakes();
        }


        public bool IsLake(int x, int z, Space space = Space.World)
        {
            int size = lakeMap.GetLength(0);

            if (space == Space.World)
            {
                int halfSize = size / 2;
                x += halfSize;
                z += halfSize;

                //if (x < -halfSize || x >= halfSize || z < -halfSize || z >= halfSize)
                //    return false;
            }

            if (x < 0 || x >= size || z < 0 || z >= size)
                return false;


            return lakeMap[x, z];
        }

        public float GetLakeWaterHeight(int x, int z, Space space = Space.World)
        {
            int size = lakeMap.GetLength(0);

            if (space == Space.World)
            {
                int halfSize = size / 2;
                x += halfSize;
                z += halfSize;
            }

            if (x < 0 || x >= size || z < 0 || z >= size)
                throw new IndexOutOfRangeException();

            return lakeObjects.Find(lake => lake.lakeTiles.Contains(new Vector2Int(x, z))).lowestNeighborGroundTile;
        }


        public void GenerateLakes()
        {
            if (seedOffset != 0)
                UnityEngine.Random.InitState(noise.Seed + seedOffset);

            IntializeLakeMap();

            for (int i = 0; i < lakeIterations; i++)
            {
                SimulateLakeStep();
            }

            CreateLakeObjects();

            if (seedOffset != 0)
                UnityEngine.Random.InitState(noise.Seed);
        }

        // Broken solution, this attempts to change the rules to guide the generation towards a more ideal lake map. Will offset seed instead.
        //public void ResolveLakes()
        //{
        //    if (!resolveLakeAmount)
        //        return;

        //    int attempt = 0;
        //    int maxAttempts = 500;

        //    int GetLargestLakeTileCount()
        //    {
        //        return lakeObjects.Max(lake => lake.lakeTiles.Count);
        //    }

        //    while (lakeObjects.Count == 0)
        //    {
        //        lakeIterations--;

        //        attempt++;
        //        if (attempt > maxAttempts || lakeIterations <= 1)
        //        {
        //            Debug.LogWarning("Failed to resolve lakes. Could not increase lake count.");
        //            break;
        //        }

        //        GenerateLakes();
        //    }

        //    attempt = 0;
        //    while (lakeObjects.Count < minLakeObjectCount)
        //    {
        //        //lakeIterations++;
        //        initialLakeChance += 0.02f;

        //        attempt++;
        //        if (attempt > maxAttempts || initialLakeChance > 0.90f)
        //        {
        //            Debug.LogWarning("Failed to resolve lakes. Could not increase lake count.");
        //            break;
        //        }

        //        GenerateLakes();
        //    }

        //    attempt = 0;
        //    while (GetLargestLakeTileCount() > maxLakeTileCount)
        //    {
        //        initialLakeChance -= 0.01f;

        //        attempt++;
        //        if (attempt > maxAttempts || initialLakeChance <= 0f)
        //        {
        //            Debug.LogWarning("Failed to resolve lakes. Could not shrink lake size.");
        //            break;
        //        }

        //        GenerateLakes();
        //    }
        //}

        public void ResolveLakes()
        {
            if (!resolveLakeAmount)
                return;

            seedOffset = 0;

            int attempt = 0;
            int maxAttempts = 100;

            int GetLargestLakeTileCount()
            {
                return lakeObjects.Max(lake => lake.lakeTiles.Count);
            }

            while (lakeObjects.Count < minLakeObjectCount || GetLargestLakeTileCount() > maxLakeTileCount)
            {
                seedOffset++;
                GenerateLakes();

                attempt++;
                if (attempt > maxAttempts)
                {
                    Debug.LogWarning("Failed to resolve lakes.");
                    break;
                }
            }
        }

        private void IntializeLakeMap()
        {
            // Setting the size (so that the map only covers the actual island, uneccesary calculations otherwise, the ocean cannot be a lake DUH)
            int size = Mathf.CeilToInt(noise.MaxIslandRadius * 2);
            int halfSize = size / 2;
            //size = size % 2 == 0 ? size : size + 1;

            lakeMap = new bool[size, size];

            for (int x = -halfSize; x < halfSize; x++)
            {
                for (int z = -halfSize; z < halfSize; z++)
                {
                    float height = noise.GetHeight(x, z, out TileType type);

                    // Checks if the position is a viable spot for a lake (according to the lakeMask)
                    if (!lakeMask.Contains(type))
                        continue;

                    //if (height > maxLakeElevation)
                    //    continue;

                    lakeMap[x + halfSize, z + halfSize] = UnityEngine.Random.value < initialLakeChance;
                }
            }
        }

        // This is the Cellular Automata calculations/simulation
        private void SimulateLakeStep()
        {
            int size = lakeMap.GetLength(0);
            int halfSize = size / 2;

            bool[,] nextMap = new bool[size, size];

            for (int x = -halfSize; x < halfSize; x++)
            {
                for (int z = -halfSize; z < halfSize; z++)
                {
                    float height = noise.GetHeight(x, z, out TileType type);

                    // Checks if the position is a viable spot for a lake (according to the lakeMask)
                    if (!lakeMask.Contains(type))
                        continue;

                    List<Vector2Int> neighborhood = lakeNeighborhood switch
                    {
                        LakeNeighborhood.Moore => mooreNeighborhood,
                        LakeNeighborhood.VonNeumann => vonNeumannNeighborhood,
                        LakeNeighborhood.VonNeumannExtended => vonNeumannNeighborhoodExtended,
                        _ => customNeighborhood
                    };

                    int waterNeighbours = CountLakeNeighbours(x, z, neighborhood);

                    // if lake true? => if there is less than 4 neighbor lakes => land
                    if (lakeMap[x + halfSize, z + halfSize])
                    {
                        //nextMap[x + halfSize, z + halfSize] = waterNeighbours >= 4;
                        nextMap[x + halfSize, z + halfSize] = waterNeighbours > MaxLakeNeighborsForLakeToLand;
                    }
                    // if land true? => if there is equal or more than 5 neighbor lakes => water
                    else
                    {
                        nextMap[x + halfSize, z + halfSize] = waterNeighbours >= MinLakeNeighborsForLandToLake;
                    }
                }
            }

            lakeMap = nextMap;
        }


        private int CountLakeNeighbours(int x, int z, List<Vector2Int> neighborhood)
        {
            int size = lakeMap.GetLength(0);
            int halfSize = size / 2;

            int count = 0;

            foreach (Vector2Int neighbor in neighborhood)
            {
                int nx = x + neighbor.x;
                int nz = z + neighbor.y;

                if (nx < -halfSize || nx >= halfSize || nz < -halfSize || nz >= halfSize)
                    continue;

                if (lakeMap[nx + halfSize, nz + halfSize])
                    count++;
            }

            return count;
        }

        private void CreateLakeObjects()
        {
            lakeObjects = new List<Lake>();

            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            int size = lakeMap.GetLength(0);
            int halfSize = size / 2;

            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            HashSet<Vector2Int> allLakeTiles = new HashSet<Vector2Int>();
            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    if (lakeMap[x, z])
                        allLakeTiles.Add(new Vector2Int(x, z));
                }
            }

            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            while (allLakeTiles.Count > 0)
            {
                HashSet<Vector2Int> lakeTileSet = new HashSet<Vector2Int>();
                Vector2Int startTile = allLakeTiles.ElementAt(UnityEngine.Random.Range(0, allLakeTiles.Count));

                queue.Enqueue(startTile);
                visited.Add(startTile);
                lakeTileSet.Add(startTile);
                allLakeTiles.Remove(startTile);

                bool hasTouchedOcean = false;
                float lowestNeighborGroundTile = 10f;

                while (queue.Count > 0)
                {
                    Vector2Int current = queue.Dequeue();

                    foreach (Vector2Int direction in directions)
                    {
                        Vector2Int neighbour = current + direction;

                        if (visited.Contains(neighbour))
                            continue;

                        if (neighbour.x < 0 || neighbour.x >= size || neighbour.y < 0 || neighbour.y >= size)
                            continue;

                        visited.Add(neighbour);

                        if (lakeMap[neighbour.x, neighbour.y])
                        {
                            lakeTileSet.Add(neighbour);
                            allLakeTiles.Remove(neighbour);

                            queue.Enqueue(neighbour);
                        }
                        else
                        {
                            float neighborHeight = noise.GetHeight(new Vector2Int(neighbour.x - halfSize, neighbour.y - halfSize), out TileType type);
                            if (type == TileType.Ocean)
                            {
                                hasTouchedOcean = true;
                                lowestNeighborGroundTile = 0.45f; // Hardcoded water height, please resolve this (move water height from TileManager to IslandNoise?)
                            }
                            else if (!hasTouchedOcean)
                            {
                                lowestNeighborGroundTile = Mathf.Min(lowestNeighborGroundTile, neighborHeight - 0.15f); // Hardcoded lake waterHeight
                            }
                        }
                    }
                }

                lakeObjects.Add(new Lake(lakeTileSet, lowestNeighborGroundTile, hasTouchedOcean));
            }
        }


        //private void OnDrawGizmos()
        //{
        //    if (!Application.isPlaying)
        //        return;

        //    int size = lakeMap.GetLength(0);
        //    int halfSize = size / 2;

        //    Gizmos.color = new Color(1, 1, 1, 0.5f);
        //    for (int x = 0; x < size; x++)
        //    {
        //        for (int z = 0; z < size; z++)
        //        {
        //            if (lakeMap[x, z])
        //                Gizmos.DrawCube(new Vector3(x - halfSize, 2.5f, z - halfSize), new Vector3(1f, 0.01f, 1f));
        //        }
        //    }
        //}

    }

}
