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

    public class BiomeManager : SingletonManager<BiomeManager>
    {
        //public static BiomeManager Instance = null; //we dont need this because we inherit from SingletonManager

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

        [Tooltip("Higher = rounder patches with fewer holes. 1 = fully random growth.")]
        [SerializeField, Range(1, 10)] private int compactness = 3;
        [Tooltip("Fill leftover single-tile holes")]
        [SerializeField] private bool fillHoles = true;

        [Header("Biome Cycle")]
        [SerializeField] private float minutesUntilFirstChange = 5f;
        [SerializeField] private float minutesBetweenChanges = 5f;
        [SerializeField] private float transitionSeconds = 30f; // how long one swap takes to spread
        [SerializeField, ReadOnly] private float timerSeconds = 0f; // elapsed time
        [SerializeField] private bool triggerChangeNow = false; // debug button

        private readonly List<BiomeType> activeBiomes = new List<BiomeType> { BiomeType.Normal };
        private bool isTransitioning = false;
        private bool tilesRegistered = false;



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


        private void Start()
        {
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
            if (triggerChangeNow)
            {
                triggerChangeNow = false;
                if (!isTransitioning) StartBiomeChange();
            }

            if (isTransitioning) return;

            timerSeconds += Time.deltaTime;
            float minutes = activeBiomes.Count < 2 ? minutesUntilFirstChange : minutesBetweenChanges;
            if (timerSeconds >= minutes * 60f) StartBiomeChange();
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
        public void SpreadBiome(BiomeType biome, int targetTileCount, float durationSeconds, BiomeType source, Vector2Int? origin = null)
        {
            StartCoroutine(SpreadBiomeRoutine(biome, targetTileCount, durationSeconds, source, origin));
        }


        private IEnumerator SpreadBiomeRoutine(BiomeType biome, int targetTileCount, float durationSeconds, BiomeType source, Vector2Int? origin)
        {
            if (biome == source || TileManager.Instance == null || TileManager.Instance.chunks == null)
            {
                yield break;
            }

            List<Vector2Int> frontier = new List<Vector2Int>(); // this is the tiles that lay at the edge and are the next to be taken over
            HashSet<Vector2Int> inFrontier = new HashSet<Vector2Int>(); // hashset to check if a tile already is in fronttier so we dont add same tiles multible times

            // if we set a startpos we check if it can be changed
            if (origin.HasValue)
            {
                if (CanConvert(origin.Value, biome, source))
                {
                    // we add it as the start and let it spread from here
                    AddToFrontier(origin.Value, frontier, inFrontier);
                }
            }

            int converted = 0;
            float elapsed = 0f;

            // we keep going until we have converted enough tiles or there are no more tiles to convert
            while (converted < targetTileCount)
            {
                if (frontier.Count == 0)
                {
                    Vector2Int? newSeed = PickCoastalSeed(biome, source);
                    if (!newSeed.HasValue) break;   // nothing left to convert
                    AddToFrontier(newSeed.Value, frontier, inFrontier);
                }

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

                    if (!CanConvert(pos, biome, source))
                    {
                        continue;
                    }

                    SetBiome(pos, biome);
                    converted++;

                    AddNeighbours(pos, biome, frontier, inFrontier, source);
                    if (fillHoles)
                    {
                        foreach (Vector2Int dir in Directions4)
                        {
                            Vector2Int n = pos + dir;
                            if (converted < targetTileCount
                                && CanConvert(n, biome, source)
                                && CountBiomeNeighbours(n, biome, false) >= 3)
                            {
                                SetBiome(n, biome);
                                converted++;
                                AddNeighbours(n, biome, frontier, inFrontier, source);
                            }
                        }
                    }
                }

                yield return null;
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


            // we always set the biome
            tileBiomes[gridPos] = biome;
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
                // we check if the neighbour tile is in the dictionary and if it has the same biome
                if (tileBiomes.TryGetValue(pos + dir, out BiomeType b) && b == biome)
                {
                    count++;
                } 
            }

            return count;
        }

       

        // adds the neighbours of a tile to the frontier if they can be converted to the biome
        private void AddNeighbours(Vector2Int pos, BiomeType biome, List<Vector2Int> frontier, HashSet<Vector2Int> inFrontier, BiomeType source)
        {
            foreach (Vector2Int dir in Directions4)
            {
                Vector2Int neighbour = pos + dir;
                if (!inFrontier.Contains(neighbour) && CanConvert(neighbour, biome, source))
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
        private bool CanConvert(Vector2Int gridPos, BiomeType biome, BiomeType source)
        {
            Tile tile = GetTileAt(gridPos);
            if (tile == null || tile.Type != TileType.Grass)
            {
                return false;
            }

            return GetBiome(gridPos) == source;
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
        private Vector2Int? PickCoastalSeed(BiomeType biome, BiomeType source)
        {
            List<Vector2Int> coastal = new List<Vector2Int>();
            List<Vector2Int> any = new List<Vector2Int>();

            foreach (Chunk chunk in TileManager.Instance.chunks)
            {
                foreach (Vector2Int gridPos in chunk.GetTiles(TileType.Grass))
                {
                    if (!CanConvert(gridPos, biome, source))
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

        // runs once at the start to register all grass tiles in the world so we can change their biomes later
        private void RegisterGrassTiles()
        {
            if (tilesRegistered || TileManager.Instance == null || TileManager.Instance.chunks == null)
                return;

            foreach (Chunk chunk in TileManager.Instance.chunks)
            {
                foreach (Vector2Int pos in chunk.GetTiles(TileType.Grass))
                {

                    tileBiomes[pos] = BiomeType.Normal;
                }

                tilesRegistered = true;
            }
        }


        private int CountTiles(BiomeType biome)
        {
            int count = 0;
            foreach (KeyValuePair<Vector2Int, BiomeType> kv in tileBiomes)
                if (kv.Value == biome) count++;
            return count;
        }

        // Any defined biome plus Normal, minus the ones we exclude
        private BiomeType? PickNewBiome(params BiomeType[] exclude)
        {
            List<BiomeType> pool = new List<BiomeType> { BiomeType.Normal };
            foreach (BiomeDefinition def in biomes) pool.Add(def.type);
            pool.RemoveAll(b => System.Array.IndexOf(exclude, b) >= 0);

            if (pool.Count == 0) return null;
            return pool[Random.Range(0, pool.Count)];
        }

        private void StartBiomeChange()
        {
            RegisterGrassTiles();
            timerSeconds = 0f;

            if (activeBiomes.Count < 2)
            {
                // First change: take exactly half of the grass
                BiomeType source = activeBiomes[0];
                BiomeType? newBiome = PickNewBiome(source);
                if (!newBiome.HasValue) return;

                int half = CountTiles(source) / 2;
                activeBiomes.Add(newBiome.Value);
                StartCoroutine(BiomeChangeRoutine(newBiome.Value, source, half));
            }
            else
            {
                // Later changes: pick one of the two, replace ALL of it
                int replaceIndex = Random.Range(0, 2);
                BiomeType replaced = activeBiomes[replaceIndex];
                BiomeType survivor = activeBiomes[1 - replaceIndex];

                BiomeType? newBiome = PickNewBiome(replaced, survivor);
                if (!newBiome.HasValue) return;

                activeBiomes[replaceIndex] = newBiome.Value;
                StartCoroutine(BiomeChangeRoutine(newBiome.Value, replaced, CountTiles(replaced)));
            }
        }

        private IEnumerator BiomeChangeRoutine(BiomeType newBiome, BiomeType source, int count)
        {
            isTransitioning = true;
            yield return SpreadBiomeRoutine(newBiome, count, transitionSeconds, source, null);
            isTransitioning = false;
            timerSeconds = 0f;   // the interval counts from when the swap finishes
        }
    }
}
