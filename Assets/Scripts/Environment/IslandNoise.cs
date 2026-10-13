using Assets.Scripts.Environment;
using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Environment
{
    public class IslandNoise : MonoBehaviour
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

        public static IslandNoise Instance = null;
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

        public float MaxIslandRadius => islandRadius * (1 + coastlineVariation);

        private void Awake()
        {
            if (Instance == null)
                IslandNoise.Instance = this;
        }

        void Start()
        {
            if (seed < 0)
            {
                //seed = (int)System.DateTime.Now.Ticks;
                seed = UnityEngine.Random.Range(short.MinValue, short.MaxValue);
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

        float GetTerrainHeight(Vector2 worldPos)
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
            }

            return heightResult;
        }
#if UNITY_EDITOR
        //private void OnValidate()
        //{
        //    if (!Application.isPlaying || TileManager.Instance == null)
        //        return;

        //    foreach (Chunk chunk in TileManager.Instance.chunks)
        //    {
        //        for (int x = 0; x < chunk.Size; x++)
        //        {
        //            for (int z = 0; z < chunk.Size; z++)
        //            {
        //                TileType type = TileType.Ocean;
        //                float y = useNoise ? IslandNoise.Instance.GetHeight(x + chunk.position.x, z + position.y, out type) : 0f;
        //                tiles[x, z] = new Tile(new Vector3(x + position.x, y, z + position.y), new Vector3(x, y, z), type);
        //            }
        //        }

        //        chunk.isDirty = true;
        //    }
        //}
#endif
    }
}

