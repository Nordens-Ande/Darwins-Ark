using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Environment
{
    public enum BiomeType
    {
        Normal, // just the green grass
        Infected,
        Autumn,

        // if you add a new biome you have to make it in List<BiomeDefinition> 
    }

    [System.Serializable]
    public struct BiomeDefinition
    {
        public BiomeType type;
        public Color color;

        public BiomeDefinition(BiomeType type, Color color)
        {
            this.type = type;
            this.color = color;
        }
    }

    public class BiomeManager : MonoBehaviour
    {
        public static BiomeManager Instance = null;

        [Header("Base Colours")]
        [SerializeField] private Color grassColor = Color.lawnGreen;
        [SerializeField] private Color sandColor = Color.yellow; // beach

        [Header("Biome Settings")]
        [SerializeField]
        private List<BiomeDefinition> biomes = new List<BiomeDefinition>
        {
            new BiomeDefinition(BiomeType.Infected, Color.purple),
            new BiomeDefinition(BiomeType.Autumn, new Color(1f, 0.55f, 0.1f)),
        };

        [Tooltip("Spreading biome can take tiles that already belong to another biome.")]
        [SerializeField] private bool overwriteOtherBiomes = false;

        [Tooltip("Higher = rounder patches with fewer holes. 1 = fully random growth.")]
        [SerializeField, Range(1, 10)] private int compactness = 3;

        [Tooltip("Fill leftover single-tile holes")]
        [SerializeField] private bool fillHoles = true;

        [Header("Debug")]
        [SerializeField] private bool tryToSpreadBiome = false;
        [SerializeField] private BiomeType debugBiome = BiomeType.Infected;
        [SerializeField] private int debugTileCount = 100;
        [SerializeField] private float debugDuration = 5f;

        // Only tiles that are NOT Normal are stored. Key = world grid position.
        private readonly Dictionary<Vector2Int, BiomeType> tileBiomes = new Dictionary<Vector2Int, BiomeType>();
        private readonly Dictionary<BiomeType, Color> biomeColors = new Dictionary<BiomeType, Color>();

        private static readonly Vector2Int[] Directions4 =
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };

        private static readonly Vector2Int[] Directions8 =
        {
            new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(1, -1),
            new Vector2Int(0, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1)
        };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            RebuildColorLookup();
        }

        private void OnValidate()
        {
            // if we change values in the unity editor they change when we do it
            if (Application.isPlaying)
            {
                RebuildColorLookup();
            }
        }

        // rebuilds the dictionary of colors 
        private void RebuildColorLookup()
        {
            biomeColors.Clear();
            foreach (BiomeDefinition def in biomes)
            {
                biomeColors[def.type] = def.color;
            }
        }

        private void Update()
        {
            // debug button for biome switch test
            if (tryToSpreadBiome)
            {
                tryToSpreadBiome = false;
                SpreadBiome(debugBiome, debugTileCount, debugDuration);
            }
        }

        /// 
        /// reading biomes
        /// 

        // returns what biometype is on a gird position
        public BiomeType GetBiome(Vector2Int gridPos)
        {
            BiomeType biome;
            if (tileBiomes.TryGetValue(gridPos, out biome))
            {
                return biome;
            }
            return BiomeType.Normal;
        }

        // returns biome based on a tile
        public BiomeType GetBiome(Tile tile)
        {
            return GetBiome(tile.GridPosition);
        }


        // returns biome based on coordinates
        public BiomeType GetBiome(float x, float z)
        {
            int convertedX = Mathf.FloorToInt(x);
            int convertedZ = Mathf.FloorToInt(z);
            return GetBiome(new Vector2Int(convertedX, convertedZ));
        }



        ///
        /// changing biomes
        ///

        // this is the funtion that starts the biome change
        public void SpreadBiome(BiomeType biome, int targetTileCount, float durationSeconds, Vector2Int? origin = null)
        {
            StartCoroutine(SpreadBiomeRoutine(biome, targetTileCount, durationSeconds, origin));
        }

       
        private IEnumerator SpreadBiomeRoutine(BiomeType biome, int targetTileCount, float durationSeconds, Vector2Int? origin)
        {
            if (biome == BiomeType.Normal || TileManager.Instance == null || TileManager.Instance.chunks == null)
            {
                yield break;
            }

            List<Vector2Int> frontier = new List<Vector2Int>(); // this is the tiles that lay at the edge and are the next to be taken over
            HashSet<Vector2Int> inFrontier = new HashSet<Vector2Int>(); // hashset to check if a tile already is in fronttier so we dont add same tiles multible times

            // if we set a startpos we check if it can be changed
            if (origin.HasValue) 
            {
                if (CanConvert(origin.Value, biome))
                {
                    // we add it as the start and let it spread from here
                    AddToFrontier(origin.Value, frontier, inFrontier);
                }
            }
            // we need to find a spot to start from
            else
            {
                foreach (KeyValuePair<Vector2Int, BiomeType> keyValue in tileBiomes)
                {
                    if (keyValue.Value == biome)
                    {
                        AddNeighbours(keyValue.Key, biome, frontier, inFrontier);
                    }
                }

                if (frontier.Count == 0)
                {
                    Vector2Int? seed = PickCoastalSeed(biome);
                    if (seed.HasValue)
                    {
                        AddToFrontier(seed.Value, frontier, inFrontier);
                    }
                }
            }

            int converted = 0;
            float elapsed = 0f;

            // we keep going until we have converted enough tiles or there are no more tiles to convert
            while (converted < targetTileCount && frontier.Count > 0)
            {
                elapsed += Time.deltaTime;

                // we calculate how many tiles should be converted based on the time and the duration
                int shouldBeConverted;
                if (durationSeconds <= 0f)
                {
                    shouldBeConverted = targetTileCount;
                }
                else
                {
                    float progress = elapsed / durationSeconds;
                    shouldBeConverted = Mathf.Min(targetTileCount, Mathf.CeilToInt(progress * targetTileCount));
                }

                // we convert tiles until we reach the amount that should be converted
                while (converted < shouldBeConverted && frontier.Count > 0)
                {
                    int i = PickFrontierIndex(frontier, biome);
                    Vector2Int pos = frontier[i];

                    frontier[i] = frontier[frontier.Count - 1];
                    frontier.RemoveAt(frontier.Count - 1);
                    inFrontier.Remove(pos);

                    if (!CanConvert(pos, biome))
                    {
                        continue;
                    }

                    SetBiome(pos, biome);
                    converted++;

                    AddNeighbours(pos, biome, frontier, inFrontier);
                }

                yield return null;
            }

            if (fillHoles)
            {
                FillHoles(biome);
            }
        }


        // sets the biome of a tile at a grid position, returns true if successful
        private bool SetBiome(Vector2Int gridPos, BiomeType biome)
        {
            Chunk chunk = GetChunkAt(gridPos);
            if (chunk == null)
            {
                return false;
            }

            Tile tile = chunk.GetTile(gridPos.x, gridPos.y);
            if (tile == null || tile.Type != TileType.Grass)
            {
                return false;
            }

            if (GetBiome(gridPos) == biome)
            {
                return false;
            }

            if (biome == BiomeType.Normal)
            {
                tileBiomes.Remove(gridPos);
            }
            else
            {
                tileBiomes[gridPos] = biome;
            }

            chunk.isDirty = true;
            return true;
        }



        // Spread helpers


        // picks a tile from the frontier list to convert next, based on the compactness setting
        private int PickFrontierIndex(List<Vector2Int> frontier, BiomeType biome)
        {
            int best = Random.Range(0, frontier.Count);
            if (compactness <= 1 || frontier.Count == 1)
            {
                return best;
            }

            int bestScore = CountBiomeNeighbours(frontier[best], biome, true);

            // we pick a random tile from the frontier and check how many neighbours it has of the same biome
            // we do this compactness times and pick the one with the most neighbours
            for (int k = 1; k < compactness; k++)
            {
                int i = Random.Range(0, frontier.Count);
                int score = CountBiomeNeighbours(frontier[i], biome, true);
                if (score > bestScore)
                {
                    best = i;
                    bestScore = score;
                }
            }

            return best;
        }

        // counts how many neighbours of a tile are of the same biome
        private int CountBiomeNeighbours(Vector2Int pos, BiomeType biome, bool includeDiagonals)
        {
            int count = 0;
            Vector2Int[] dirs;
            if (includeDiagonals)
            {
                dirs = Directions8;
            }
            else
            {
                dirs = Directions4;
            }

            foreach (Vector2Int dir in dirs)
            {
                if (GetBiome(pos + dir) == biome)
                {
                    count++;
                }
            }

            return count;
        }

        // fills in single-tile holes of a biome by checking if a tile has 3 or more neighbours of the same biome
        private void FillHoles(BiomeType biome)
        {
            for (int pass = 0; pass < 10; pass++)
            {
                List<Vector2Int> toFill = new List<Vector2Int>();
                HashSet<Vector2Int> seen = new HashSet<Vector2Int>();

                foreach (KeyValuePair<Vector2Int, BiomeType> kv in tileBiomes)
                {
                    if (kv.Value != biome)
                    {
                        continue;
                    }

                    foreach (Vector2Int dir in Directions4)
                    {
                        Vector2Int n = kv.Key + dir;
                        if (seen.Add(n) && CanConvert(n, biome) && CountBiomeNeighbours(n, biome, false) >= 3)
                        {
                            toFill.Add(n);
                        }
                    }
                }

                if (toFill.Count == 0)
                {
                    break;
                }

                foreach (Vector2Int pos in toFill)
                {
                    SetBiome(pos, biome);
                }
            }
        }

        // adds the neighbours of a tile to the frontier if they can be converted to the biome
        private void AddNeighbours(Vector2Int pos, BiomeType biome, List<Vector2Int> frontier, HashSet<Vector2Int> inFrontier)
        {
            foreach (Vector2Int dir in Directions4)
            {
                Vector2Int neighbour = pos + dir;
                if (!inFrontier.Contains(neighbour) && CanConvert(neighbour, biome))
                {
                    AddToFrontier(neighbour, frontier, inFrontier);
                }
            }
        }

        // adds a tile to the frontier if it is not already in the frontier
        private void AddToFrontier(Vector2Int pos, List<Vector2Int> frontier, HashSet<Vector2Int> inFrontier)
        {
            if (inFrontier.Add(pos))
            {
                frontier.Add(pos);
            }
        }

        // checks if a tile can be converted to a biome, based on its type and current biome
        private bool CanConvert(Vector2Int gridPos, BiomeType biome)
        {
            Tile tile = GetTileAt(gridPos);
            if (tile == null || tile.Type != TileType.Grass)
            {
                return false;
            }

            BiomeType current = GetBiome(gridPos);
            if (current == biome)
            {
                return false;
            }

            if (overwriteOtherBiomes)
            {
                return true;
            }
            return current == BiomeType.Normal;
        }

        // checks if a tile is coastal, meaning it has at least one neighbour that is not grass (e.g., water or beach)
        private bool IsCoastal(Vector2Int gridPos)
        {
            foreach (Vector2Int dir in Directions4)
            {
                Tile tile = GetTileAt(gridPos + dir);
                if (tile == null || tile.Type != TileType.Grass)
                {
                    return true;
                }
            }
            return false;
        }

        // picks a random tile that is coastal to start spreading the biome from
        private Vector2Int? PickCoastalSeed(BiomeType biome)
        {
            List<Vector2Int> coastal = new List<Vector2Int>();
            List<Vector2Int> any = new List<Vector2Int>();

            foreach (Chunk chunk in TileManager.Instance.chunks)
            {
                foreach (Vector2Int gridPos in chunk.GetTiles(TileType.Grass))
                {
                    if (!CanConvert(gridPos, biome))
                    {
                        continue;
                    }

                    any.Add(gridPos);
                    if (IsCoastal(gridPos))
                    {
                        coastal.Add(gridPos);
                    }
                }
            }

            List<Vector2Int> pool;
            if (coastal.Count > 0)
            {
                pool = coastal;
            }
            else
            {
                pool = any;
            }

            if (pool.Count == 0)
            {
                return null;
            }

            return pool[Random.Range(0, pool.Count)];
        }

        // helper functions to get the chunk and tile at a grid position
        private Chunk GetChunkAt(Vector2Int gridPos)
        {
            if (TileManager.Instance == null)
            {
                return null;
            }

            return TileManager.Instance.GetChunk(gridPos.x, gridPos.y);
        }

        // helper function to get the tile at a grid position
        private Tile GetTileAt(Vector2Int gridPos)
        {
            Chunk chunk = GetChunkAt(gridPos);
            if (chunk == null)
            {
                return null;
            }
            return chunk.GetTile(gridPos.x, gridPos.y);
        }


        // returns the color for a biome
        public Color GetBiomeColor(BiomeType biome, Color defaultColor)
        {
            Color color;
            if (biomeColors.TryGetValue(biome, out color))
            {
                return color;
            }
            return defaultColor;
        }

        // returns the color for a tile based on its type and biome
        public Color GetTileColor(Tile tile)
        {
            if (tile.Type != TileType.Grass)
            {
                return sandColor;
            }

            BiomeType biome = GetBiome(tile.GridPosition);
            return GetBiomeColor(biome, grassColor);
        }
    }
}