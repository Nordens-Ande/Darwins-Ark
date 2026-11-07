using Assets.Scripts.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Assets.Scripts.Environment
{
    public class IslandNoise : SingletonManager<IslandNoise>
    {
        [Serializable]
        private struct NoiseDetailLevels
        {
            //public float largeNoise, mediumNoise, smallNoise;
            //public float largeOffset, mediumOffset, smallOffset;

            public float largeScaler;
            public float largeOffset;
            [Space]
            public float mediumScaler;
            public float mediumOffset;
            [Space]
            public float smallScaler;
            public float smallOffset;

            public NoiseDetailLevels(float largeScaler, float largeOffset, float mediumScaler, float mediumOffset, float smallScaler, float smallOffset)
            {
                this.largeScaler = largeScaler;
                this.largeOffset = largeOffset;

                this.mediumScaler = mediumScaler;
                this.mediumOffset = mediumOffset;

                this.smallScaler = smallScaler;
                this.smallOffset = smallOffset;
            }
        }

        private TileManager tileManager;

        [SerializeField] private int seed = -1;

        [Space]

        [Header("Island")]
        [SerializeField] private float islandRadius = 30;
        //[SerializeField] private float coastlineAngleStep = 30;

        [Space]

        [Header("Coastline Noise")]
        [SerializeField] private float coastlineFrequency = 1f;
        [SerializeField] private float coastlineVariation = 0.15f; //percent variation from islandRadius
        [SerializeField] private NoiseDetailLevels coastlineNoiseLevels = new NoiseDetailLevels(2f, 0, 4f, 100f, 12f, 200f);

        [Space]

        [Header("Beach")]
        [SerializeField] private float beachWidth = 4;
        [SerializeField] private float beachMinHeight = 0.5f;

        [Space]

        [Header("Terrain")]
        [SerializeField] private NoiseDetailLevels terrainNoiseLevels = new NoiseDetailLevels(0.01f, 0, 0.03f, 100f, 0.08f, 200f);

        [Space]

        [Header("Ocean")]
        [SerializeField] private bool slopeOceanFloor = true;

        public float MaxIslandRadius => islandRadius * (1 + coastlineVariation);

        protected override void Awake()
        {
            base.Awake();

            if (seed < 0)
            {
                //seed = (int)System.DateTime.Now.Ticks;
                seed = UnityEngine.Random.Range(short.MinValue, short.MaxValue);
            }

            UnityEngine.Random.InitState(seed);
        }

        void Start()
        {
            tileManager = TileManager.Instance;

            //if (seed < 0)
            //{
            //    //seed = (int)System.DateTime.Now.Ticks;
            //    seed = UnityEngine.Random.Range(short.MinValue, short.MaxValue);
            //}

            IntializeLakeMap();
            for (int i = 0; i < lakeIterations; i++)
            {
                SimulateLakeStep();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        float GetCoastlineRadius(float angle)
        {
            float x = Mathf.Cos(angle) * coastlineFrequency;
            float y = Mathf.Sin(angle) * coastlineFrequency;

            //Different detail levels (large is the coast shape while the small is the details)
            NoiseDetailLevels levels = coastlineNoiseLevels; //make a method for this part

            float large = Mathf.PerlinNoise(x * levels.largeScaler + seed + levels.largeOffset, y * levels.largeScaler + seed + levels.largeOffset);
            float medium = Mathf.PerlinNoise(x * levels.mediumScaler + seed + levels.mediumOffset, y * levels.mediumScaler + seed + levels.mediumOffset);
            float small = Mathf.PerlinNoise(x * levels.smallScaler + seed + levels.smallOffset, y * levels.smallScaler + seed + levels.smallOffset);

            //resulting noise
            float noise = large * 0.60f + medium * 0.30f + small * 0.10f; //add ratio drawer for this

            //changing noise range from 0..1 to -1..1
            float centeredNoise = (noise - 0.5f) * 2f;

            //return Mathf.Lerp(islandRadius * (1f - coastlineVariation), islandRadius * (1f + coastlineVariation), noise);
            return islandRadius * (1f + centeredNoise * coastlineVariation);
        }


        //[Obsolete] float GetIslandMask(Vector2 worldPos)
        //{
        //    float distance = worldPos.magnitude;
        //    float angle = Mathf.Atan2(worldPos.y, worldPos.x);

        //    float coastlineRadius = GetCoastlineRadius(angle);

        //    float distance01 = distance / coastlineRadius;

        //    return 1f - Mathf.SmoothStep(0.98f, 1.0f, distance01);
        //}

        private float GetTerrainHeight(Vector2 worldPos)
        {
            //float large = Mathf.PerlinNoise(worldPos.x * 0.1f + seed, worldPos.y * 0.1f + seed);

            //float medium = Mathf.PerlinNoise(worldPos.x * 0.03f + seed + 100, worldPos.y * 0.03f + seed + 100);

            //float small = Mathf.PerlinNoise(worldPos.x * 0.08f + seed + 200, worldPos.y * 0.08f + seed + 200);

            float x = worldPos.x;
            float y = worldPos.y;

            NoiseDetailLevels levels = terrainNoiseLevels;

            float large = Mathf.PerlinNoise(x * levels.largeScaler + seed + levels.largeOffset, y * levels.largeScaler + seed + levels.largeOffset);
            float medium = Mathf.PerlinNoise(x * levels.mediumScaler + seed + levels.mediumOffset, y * levels.mediumScaler + seed + levels.mediumOffset);
            float small = Mathf.PerlinNoise(x * levels.smallScaler + seed + levels.smallOffset, y * levels.smallScaler + seed + levels.smallOffset);

            return large * 0.65f + medium * 0.25f + small * 0.10f;
        }

        public float GetHeight(Vector2 worldPos)
        {
            float distance = worldPos.magnitude;
            float angle = Mathf.Atan2(worldPos.y, worldPos.x);

            float coastlineRadius = GetCoastlineRadius(angle);

            //islandMask
            if (distance < coastlineRadius - beachWidth * 2)
                return 1f + GetTerrainHeight(worldPos);
            if (distance < coastlineRadius - beachWidth)
                return Mathf.SmoothStep(1f, 1f + GetTerrainHeight(worldPos), (coastlineRadius - distance - beachWidth) / beachWidth);
            else if (distance < coastlineRadius)
                return Mathf.SmoothStep(beachMinHeight, 1f, (coastlineRadius - distance) / beachWidth);

            return 0f;
        }

        public Color GetColor(Vector2 worldPos)
        {
            float distance = worldPos.magnitude;
            float angle = Mathf.Atan2(worldPos.y, worldPos.x);

            float coastlineRadius = GetCoastlineRadius(angle);

            if (distance < coastlineRadius - beachWidth)
                return Color.lawnGreen;
            else
                return Color.softYellow;
        }

        public float GetHeight(float x, float z)
        {
            return GetHeight(new Vector2(x, z));
        }

        public float GetHeight(float x, float z, out TileType type)
        {
            return GetHeight(new Vector2(x, z), out type);
        }

        public float GetHeight(Vector2 worldPos, out TileType type)
        {
            float heightResult = 0;

            float distance = worldPos.magnitude;
            float angle = Mathf.Atan2(worldPos.y, worldPos.x);

            float coastlineRadius = GetCoastlineRadius(angle);

            //islandMask
            if (distance < coastlineRadius - beachWidth * 2)
            {
                heightResult = 1f + GetTerrainHeight(worldPos);
                type = TileType.Grass;
            }
               
            else if (distance < coastlineRadius - beachWidth)
            {
                heightResult = Mathf.SmoothStep(1f, 1f + GetTerrainHeight(worldPos), (coastlineRadius - distance - beachWidth) / beachWidth);
                type = TileType.Grass;
            }
                
            else if (distance < coastlineRadius)
            {
                heightResult = Mathf.SmoothStep(beachMinHeight, 1f, (coastlineRadius - distance) / beachWidth);
                type = TileType.Beach;
            }
            else
            {
                type = TileType.Ocean;
                //heightResult = coastlineRadius / distance;
                if (slopeOceanFloor)
                    heightResult = - (distance - coastlineRadius) / coastlineRadius;
            }

            return heightResult;
        }

        // ------------ THE METHODS BELOW WERE CREATED FOR THE PCG CELLULAR AUTOMATA ASSIGNMENT //Elliot Nord -------------------

        private enum LakeNeighborhood
        {
            Moore,
            VonNeumann,
            VonNeumannExtended,
            Custom
        }

        // True = Lake Tile
        private bool[,] lakeMap;

        [Header("Lakes")]
        // What tiles the lake can be generated on
        [SerializeField] private TileType[] lakeMask = new TileType[] { TileType.Grass, TileType.Beach };

        [Space]

        [SerializeField] private int lakeIterations = 6;
        [SerializeField] private float initialLakeChance = 0.4f;

        [Space]
        
        [SerializeField] private LakeNeighborhood lakeNeighborhood;
        [SerializeField] private List<Vector2Int> customNeighborhood = new List<Vector2Int>();
        [SerializeField] private int MaxLakeNeighborsForLakeToLand = 3;
        [SerializeField] private int MinLakeNeighborsForLandToLake = 5;


        private void IntializeLakeMap()
        {
            // Setting the size (so that the map only covers the actual island, uneccesary calculations otherwise, the ocean cannot be a lake DUH)
            int size = Mathf.CeilToInt(MaxIslandRadius * 2);
            int halfSize = size / 2;
            //size = size % 2 == 0 ? size : size + 1;

            lakeMap = new bool[size, size];

            for (int x = -halfSize; x < halfSize; x++)
            {
                for (int z = -halfSize; z < halfSize; z++)
                {
                    float height = GetHeight(x, z, out TileType type);

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
                    float height = GetHeight(x, z, out TileType type);

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

        private int CountLakeNeighbours(int x, int z, List<Vector2Int> neighborhood)
        {
            int size = lakeMap.GetLength(0);
            int halfSize = size / 2;

            int count = 0;

            foreach (Vector2Int neighbor in neighborhood)
            {
                int nx = x + neighbor.x;
                int nz = z + neighbor.y;

                if (nx < -halfSize || nx >= halfSize || nz < -halfSize || nz >= halfSize )
                    continue;

                if (lakeMap[nx + halfSize, nz + halfSize])
                    count++;
            }

            return count;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            int size = lakeMap.GetLength(0);
            int halfSize = size / 2;

            Gizmos.color = new Color(1, 1, 1, 0.5f);
            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    if (lakeMap[x, z])
                        Gizmos.DrawCube(new Vector3(x - halfSize, 2.5f, z - halfSize), new Vector3(1f, 0.01f, 1f));
                }
            }
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying || tileManager == null)
                return;

            UnityEngine.Random.InitState(seed);

            IntializeLakeMap();
            for (int i = 0; i < lakeIterations; i++)
            {
                SimulateLakeStep();
            }

            for (int i = 0; i < tileManager.chunks.Count; i++)
            {
                Chunk chunk = tileManager.chunks[i];

                //for (int x = 0; x < chunk.Size; x++)
                //{
                //    for (int z = 0; z < chunk.Size; z++)
                //    {
                //        TileType type = TileType.Ocean;
                //        float y = useNoise ? IslandNoise.Instance.GetHeight(x + chunk.position.x, z + position.y, out type) : 0f;
                //        tiles[x, z] = new Tile(new Vector3(x + position.x, y, z + position.y), new Vector3(x, y, z), type);
                //    }
                //}
                tileManager.chunks[i] = new Chunk(chunk.position, chunk.Size);

                tileManager.chunks[i].isDirty = true;
            }

            tileManager.GenerateSaltWater();
        }
#endif
    }
}

