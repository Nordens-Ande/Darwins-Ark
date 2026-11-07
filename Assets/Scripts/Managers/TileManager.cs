using Assets.Scripts.Utility;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Environment
{
    public class TileManager : SingletonManager<TileManager>
    {
        private RainDeityManager rainDeityManager;

        [HideInInspector] public List<Chunk> chunks;
        [HideInInspector] public List<GameObject> chunkObjects;
        //[HideInInspector] public List<>

        [SerializeField] private int chunkSize = 8; 
        [SerializeField] private Vector2Int chunkGridSize = Vector2Int.one;

        [Space]

        [SerializeField] private bool createWater = true;
        [SerializeField] private float waterLevel = 0.5f;
        [SerializeField] private int waterFlowDistance = 6;
        [SerializeField] private Material waterMaterial;
        [SerializeField, ReadOnly] private GameObject oceanObject;
        [SerializeField, ReadOnly] private GameObject waterObject;

        // -----------------------------------------------
        //[SerializeField] private Material gridLineMaterial;
        // -----------------------------------------------

        public int ChunkSize => chunkSize;
        public Vector2Int ChunkGridSize => chunkGridSize;

        private string chunkGridName = "Chunk Grid";
        private string selectionGridName = "Selection Grid";
        private string heighlightGridName = "Highlight Grid";


        public event Action<Chunk[]> ChunksCleaned;


        void Start()
        {
            rainDeityManager = RainDeityManager.Instance;

            chunks = new List<Chunk>();
            chunkObjects = new List<GameObject>();

            for (int x = -chunkGridSize.x / 2; x < Mathf.Ceil(chunkGridSize.x / 2f); x++)
            {
                for (int z = -chunkGridSize.y / 2; z < Mathf.Ceil(chunkGridSize.y / 2f); z++)
                {
                    chunks.Add(new Chunk(new Vector2(x * chunkSize, z * chunkSize), chunkSize));
                }
            }

            for (int i = 0; i < chunks.Count; i++)
            {
                Chunk chunk = chunks[i];

                // Generate gameObject
                Type[] components = new Type[]
                {
                    typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider)
                };
                GameObject chunkObject = new GameObject($"Chunk (x:{chunk.position.x}, z:{chunk.position.y})", components);
                chunkObject.transform.SetParent(transform);
                chunkObjects.Add(chunkObject);
                
                // Generate mesh for chunk
                GenerateMesh(chunk, chunkObject);
            }

            for (int i = 0; i < chunks.Count; i++)
            {
                GenerateGrid(chunkObjects[i].transform, 0.001f, chunkGridName, Color.white, chunks[i][TileType.Grass]);
            }
            GenerateGrid(transform, 0.002f, selectionGridName, Color.red);
            GenerateGrid(transform, 0.003f, heighlightGridName, Color.blue);

            //SetChunkGridVisible(false);
            SubscribeToRainDeity();

            GenerateSaltWater();

            // Temp water creation
            //Tile[] sourceWaters = new Tile[] { GetTile(new Vector2Int(0, 0)), GetTile(new Vector2Int(1, 0)), GetTile(new Vector2Int(-1, 0)), GetTile(new Vector2Int(0, 1)), GetTile(new Vector2Int(0, -1)) };
            //foreach (Tile sourceWater in sourceWaters)
            //{
            //    sourceWater.WaterHeight = sourceWater.Height - 0.15f;
            //    sourceWater.Height = 0;
            //    sourceWater.SetType(TileType.Lake);
            //}

            GenerateSweetWater();
        }


        //[SerializeField, ReadOnly] int updateCounter = 0;
        //[SerializeField, ReadOnly] int index = 0;
        void FixedUpdate()
        {
            //updateCounter++;

            //if (updateCounter > 30)
            //{
            //    updateCounter = 0;

            //    chunks[0].ModifyTile(index % chunkSize, index / chunkSize, Random.Range(0, 1f), Space.Self);

            //    index++;
            //    if (index >= 64)
            //        index = 0;
            //}

            bool hasCleaned = false;
            List<Chunk> cleanedChunks = new List<Chunk>();
            for (int i = 0; i < chunks.Count; i++)
            {
                if (!chunks[i].isDirty)
                    continue;
                cleanedChunks.Add(chunks[i]);
                hasCleaned = true;

                //chunks[i].GenerateMeshData();
                chunks[i].CleanChunk();
                Mesh mesh = new Mesh();

                mesh.SetVertices(chunks[i].vertices);
                mesh.SetTriangles(chunks[i].triangles, 0);
                mesh.SetColors(chunks[i].colors);

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                chunkObjects[i].GetComponent<MeshFilter>().sharedMesh = mesh;
                chunkObjects[i].GetComponent<MeshCollider>().sharedMesh = mesh;

                GetGridMesh(chunkObjects[i].transform, chunkGridName).Rebuild();
                GetGridMesh(transform, heighlightGridName).Rebuild();
                GetGridMesh(transform, selectionGridName).Rebuild();

                GenerateSweetWater();
            }

            if (hasCleaned)
            {
                Debug.Log("We just cleaned the tiles");
                ChunksCleaned?.Invoke(cleanedChunks.ToArray());
            }
        }

        //All Getters here can be refactored for more perfomance, currently they are not super efficient
        public Chunk GetChunk(float x, float z)
        {
            if (chunks == null)
            {
                Debug.LogError("TileManager.chunks is NULL");
                return null;
            }

            int chunkX = Mathf.FloorToInt((float)x / chunkSize);
            int chunkZ = Mathf.FloorToInt((float)z / chunkSize);

            foreach (Chunk chunk in chunks)
            {
                int existingX = Mathf.RoundToInt(chunk.position.x / chunkSize);
                int existingZ = Mathf.RoundToInt(chunk.position.y / chunkSize);

                if (existingX == chunkX && existingZ == chunkZ)
                    return chunk;
            }

            //Debug.LogWarning($"No chunk found for world position ({x}, {z})");
            return null;
        }
        public Chunk GetChunk(Vector2 positionXZ) => GetChunk(positionXZ.x, positionXZ.y);
        public Chunk GetChunk(Vector3 positionXZ) => GetChunk(positionXZ.x, positionXZ.y);

        //public Tile GetTile(float x, float z) => GetChunk(x, z).GetTile((int)x, (int)z);
        public Tile GetTile(float x, float z) //new version with bug fixes
        {
            // coords of tile closest to parameter values
            int tileX = Mathf.RoundToInt(x);
            int tileZ = Mathf.RoundToInt(z);

            Chunk chunk = GetChunk(tileX, tileZ);
            if (chunk == null) return null;

            return chunk.GetTile(tileX, tileZ);
        }
        public Tile GetTile(Vector2 positionXZ) => GetTile(positionXZ.x, positionXZ.y);
        public Tile GetTile(Vector3 positionXZ) => GetTile(positionXZ.x, positionXZ.z);

        public (int min, int max) GetTilePositionRangeX()
        {
            float minX = chunks.Min(chunk => chunk.position.x);
            float maxX = chunks.Max(chunk => chunk.position.x + chunk.Size);

            return ((int)minX, (int)maxX);
        }

        public (int min, int max) GetTilePositionRangeZ()
        {
            float minZ = chunks.Min(chunk => chunk.position.y);
            float maxZ = chunks.Max(chunk => chunk.position.y + chunk.Size);

            return ((int)minZ, (int)maxZ);
        }

        private void GenerateMesh(Chunk chunk, GameObject chunkObject)
        {
            chunk.GenerateMeshData();
            chunkObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Shader Graphs/Lit Color"));

            Mesh mesh = new Mesh() 
            { 
                name = "Chunk Mesh" 
            };

            mesh.SetVertices(chunk.vertices);
            mesh.SetTriangles(chunk.triangles, 0);
            mesh.SetColors(chunk.colors);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            chunkObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            chunkObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        }

        private void GenerateGrid(Transform parent, float heightOffset, string name, Color color, HashSet<Vector2Int> tiles = null)
        {
            Type[] components = new Type[]
            {
                typeof(MeshFilter), typeof(MeshRenderer), typeof(GridMesh)
            };

            GameObject chunkGridObject = new GameObject(name, components);
            chunkGridObject.transform.SetParent(parent, false);

            MeshRenderer meshRenderer = chunkGridObject.GetComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            meshRenderer.material.color = color;

            GridMesh gridMesh = chunkGridObject.GetComponent<GridMesh>();
            gridMesh.heightOffset = heightOffset;
            if (tiles != null)
            {
                gridMesh.NewTiles(tiles);

                gridMesh.Rebuild();
            }
        }

        public void GenerateSaltWater()
        {
            if (!createWater)
                return;

            if (oceanObject != null)
                Destroy(oceanObject);

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            void AddFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
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

            foreach (Chunk chunk in chunks)
            {
                HashSet<Vector2Int> saltWater = chunk[TileType.Ocean];
                foreach (Vector2Int tile in saltWater)
                {
                    Vector3 tilePos = new Vector3(tile.x, 0/*waterLevel*/, tile.y);
                    AddFace(
                        new Vector3(-0.5f, 0, 0.5f) + tilePos,  // top-left
                        new Vector3(+0.5f, 0, 0.5f) + tilePos,  // top-right
                        new Vector3(+0.5f, 0, -0.5f) + tilePos, // bottom-right
                        new Vector3(-0.5f, 0, -0.5f) + tilePos  // bottom-left
                    );
                }


            }

            //waterObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            oceanObject = new GameObject("Salt Water Object", typeof(MeshRenderer), typeof(MeshFilter));
            oceanObject.transform.parent = transform;

            oceanObject.transform.localPosition = oceanObject.transform.localPosition + new Vector3(0, waterLevel, 0);
            //waterObject.transform.localScale = new Vector3(chunkGridSize.x * chunkSize, chunkGridSize.y * chunkSize, 1f);
            //waterObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);


            // Water Object mesh and material
            oceanObject.GetComponent<MeshRenderer>().material = waterMaterial;

            Mesh mesh = new Mesh()
            {
                name = "Chunk Mesh"
            };

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            oceanObject.GetComponent<MeshFilter>().sharedMesh = mesh;
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

        public void GenerateSweetWater()
        {
            if (!createWater)
                return;

            //if (waterObject != null)
            //    Destroy(waterObject);

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            HashSet<Vector2Int> lakes = new HashSet<Vector2Int>();
            HashSet<Vector2Int> rivers = new HashSet<Vector2Int>();
            
            foreach (Chunk chunk in chunks)
            {
                lakes.AddRange(chunk[TileType.Lake]);
                rivers.AddRange(chunk[TileType.River]);
            }

            GenerateRivers(lakes, rivers, waterFlowDistance);

            HashSet<Vector2Int> sweetWater = new HashSet<Vector2Int>(lakes);
            sweetWater.UnionWith(rivers);

            float? GetWaterHeight(int x, int z)
            {
                //if (sweetWater.Contains(new Vector2Int(x, z)))
                //    Debug.Log(GetTile(x, z).WaterHeight);
                return sweetWater.Contains(new Vector2Int(x, z)) ? GetTile(x, z)?.WaterHeight : null;
            }

            foreach (Vector2Int water in sweetWater)
            {
                Tile tile = GetTile(water);

                Vector3 tilePos = new Vector3(water.x, tile.WaterHeight, water.y);
                //AddFace(
                //    new Vector3(-0.5f, 0, 0.5f) + tilePos,  // top-left
                //    new Vector3(+0.5f, 0, 0.5f) + tilePos,  // top-right
                //    new Vector3(+0.5f, 0, -0.5f) + tilePos, // bottom-right
                //    new Vector3(-0.5f, 0, -0.5f) + tilePos  // bottom-left
                //);

                float currentHeight = tilePos.y;

                Dictionary<Direction, float> neighbourHeights = new Dictionary<Direction, float>();
                foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                {
                    float value = direction switch
                    {
                        Direction.North     => GetWaterHeight(water.x, water.y + 1) ?? currentHeight,
                        Direction.NorthWest => GetWaterHeight(water.x - 1, water.y + 1) ?? currentHeight,
                        Direction.West      => GetWaterHeight(water.x - 1, water.y) ?? currentHeight,
                        Direction.SouthWest => GetWaterHeight(water.x - 1, water.y - 1) ?? currentHeight,
                        Direction.South     => GetWaterHeight(water.x, water.y - 1) ?? currentHeight,
                        Direction.SouthEast => GetWaterHeight(water.x + 1, water.y - 1) ?? currentHeight,
                        Direction.East      => GetWaterHeight(water.x + 1, water.y) ?? currentHeight,
                        Direction.NorthEast => GetWaterHeight(water.x + 1, water.y + 1) ?? currentHeight,
                        _ => 0
                    };
                    neighbourHeights.Add(direction, value);
                }

                float nwHeight = Mathf.Max(currentHeight, neighbourHeights[Direction.North], neighbourHeights[Direction.West], neighbourHeights[Direction.NorthWest]);
                float neHeight = Mathf.Max(currentHeight, neighbourHeights[Direction.North], neighbourHeights[Direction.East], neighbourHeights[Direction.NorthEast]);
                float seHeight = Mathf.Max(currentHeight, neighbourHeights[Direction.South], neighbourHeights[Direction.East], neighbourHeights[Direction.SouthEast]);
                float swHeight = Mathf.Max(currentHeight, neighbourHeights[Direction.South], neighbourHeights[Direction.West], neighbourHeights[Direction.SouthWest]);

                //int heightCount = (Mathf.Approximately(nwHeight, currentHeight) ? 0 : 1)
                //                + (Mathf.Approximately(neHeight, currentHeight) ? 0 : 1)
                //                + (Mathf.Approximately(seHeight, currentHeight) ? 0 : 1)
                //                + (Mathf.Approximately(swHeight, currentHeight) ? 0 : 1);

                //Debug.Log(heightCount);

                TileMeshBuilder.AddFace(
                    vertices,
                    triangles,
                    new Vector3(-0.5f, nwHeight, 0.5f) + new Vector3(tilePos.x, 0, tilePos.z),
                    new Vector3(+0.5f, neHeight, 0.5f) + new Vector3(tilePos.x, 0, tilePos.z),
                    new Vector3(+0.5f, seHeight, -0.5f) + new Vector3(tilePos.x, 0, tilePos.z),
                    new Vector3(-0.5f, swHeight, -0.5f) + new Vector3(tilePos.x, 0, tilePos.z)
                );
            }

            //foreach (Chunk chunk in chunks)
            //{
            //    HashSet<Vector2Int> sweetWater = chunk[TileType.River, TileType.Lake];
            //    foreach (Vector2Int tileGridPos in sweetWater)
            //    {
            //        Tile tile = GetTile(tileGridPos);
            //        Tile[] neighbours = new Tile[4]
            //        {
            //            GetTile(tileGridPos + Vector2Int.up),
            //            GetTile(tileGridPos + Vector2Int.right),
            //            GetTile(tileGridPos + Vector2Int.down),
            //            GetTile(tileGridPos + Vector2Int.left)
            //        };

            //        Vector3 tilePos = new Vector3(tileGridPos.x, tile.WaterHeight, tileGridPos.y);
            //        AddFace(
            //            new Vector3(-0.5f, 0, 0.5f) + tilePos,  // top-left
            //            new Vector3(+0.5f, 0, 0.5f) + tilePos,  // top-right
            //            new Vector3(+0.5f, 0, -0.5f) + tilePos, // bottom-right
            //            new Vector3(-0.5f, 0, -0.5f) + tilePos  // bottom-left
            //        );
            //    }
            //}

            if (waterObject == null)
            {
                waterObject = new GameObject("Sweet Water Object", typeof(MeshRenderer), typeof(MeshFilter));
                waterObject.transform.parent = transform;
            }

            // Water Object mesh and material
            waterObject.GetComponent<MeshRenderer>().material = waterMaterial;

            Mesh mesh = new Mesh()
            {
                name = "Chunk Mesh"
            };

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            waterObject.GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        // Creating a simple BFS alogrithm to simulate the water progation for the water (we assume lakes are source water and the rivers are the progation)
        private void GenerateRivers(HashSet<Vector2Int> sourceWater, HashSet<Vector2Int> riverTiles, int maxFlowDistance)
        {
            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            Queue<(Vector2Int position, float height, float waterHeight, int distance)> queue = new Queue<(Vector2Int position, float height, float waterHeight, int distance)>();

            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            // Add all source (lake) water tiles.
            foreach (Vector2Int source in sourceWater)
            {
                Tile sourceTile = GetTile(source);
                queue.Enqueue((source, sourceTile.Height, sourceTile.WaterHeight, 0));
                visited.Add(source);
                riverTiles.Add(source);
            }

            while (queue.Count > 0)
            {
                (Vector2Int position, float height, float waterHeight, int distance) current = queue.Dequeue();

                if (current.height >= current.waterHeight)
                    continue;

                if (current.distance > maxFlowDistance)
                    continue;

                foreach (Vector2Int direction in directions)
                {
                    Vector2Int neighbour = current.position + direction;

                    if (visited.Contains(neighbour))
                        continue;

                    Tile neighbourTile = GetTile(neighbour);

                    // Check whether the tile exists.
                    if (neighbourTile == null)
                        continue;

                    //if (neighbourTile.Type == TileType.River)
                    //    continue;

                    float neighbourHeight = neighbourTile.Height;

                    // If neighbour tile is below water then it can flow
                    if (neighbourHeight > current.waterHeight)
                        continue;

                    visited.Add(neighbour);
                    riverTiles.Add(neighbour);

                    float heightDecrease = maxFlowDistance > 0 ? current.distance / (maxFlowDistance * 0.5f) : 0.1f;
                    //float newHeight = Mathf.Max(current.waterHeight - 0.1f, neighbourHeight);
                    float newHeight = current.distance == maxFlowDistance ? neighbourHeight : current.waterHeight;

                    neighbourTile.SetType(TileType.River);

                    if (!Mathf.Approximately(neighbourTile.WaterHeight, newHeight))
                    {
                        //neighbourTile.SetType(TileType.River);
                        neighbourTile.WaterHeight = newHeight;
                    }
                    //if (neighbourTile.Type != TileType.River)
                    //{
                    //    neighbourTile.SetType(TileType.River);
                    //}
                    
                    queue.Enqueue((neighbour, neighbourTile.Height, neighbourTile.WaterHeight, current.distance + 1));
                }
            }
        }


        // Booleans for when chunk/tile grids are visible
        public void SetChunkGridVisible(bool visible)
        {
            foreach (GameObject chunkObject in chunkObjects)
            {
                Transform grid = chunkObject.transform.Find("ChunkGrid");

                if (grid != null)
                    grid.gameObject.SetActive(visible);
            }
        }

        public void SetTileGridVisible(bool visible)
        {
            foreach (GameObject chunkObject in chunkObjects)
            {
                Transform grid = chunkObject.transform.Find("TileGrid");

                if (grid != null)
                    grid.gameObject.SetActive(visible);
            }
        }


        private GridMesh GetGridMesh(Transform transform, string gameObjectName)
        {
            GameObject gridObject = transform.Find(gameObjectName).gameObject;
            if (gridObject == null)
            {
                Debug.LogWarning($"Grid object {gameObjectName} could not be found.");
                return null;
            }

            GridMesh gridMesh = gridObject.GetComponent<GridMesh>();
            if (gridMesh == null)
            {
                Debug.LogWarning($"Grid Mesh in {gridObject.name} could not be found.");
                return null;
            }

            return gridMesh;
        }
        public void SetSelectionGridTiles(HashSet<Vector2Int> tiles)
        {
            GridMesh gridMesh = GetGridMesh(transform, selectionGridName);

            if (gridMesh != null)
                gridMesh.NewTiles(tiles);
        }
        public void AddSelectionGridTile(Vector2Int tile)
        {
            GridMesh gridMesh = GetGridMesh(transform, selectionGridName);

            if (gridMesh != null)
                gridMesh.AddTile(tile);
        }
        public void RemoveSelectionGridTile(Vector2Int tile)
        {
            GridMesh gridMesh = GetGridMesh(transform, selectionGridName);

            if (gridMesh != null)
                gridMesh.RemoveTile(tile);
        }

        public void SetHighlightGridTiles(HashSet<Vector2Int> tiles)
        {
            GridMesh gridMesh = GetGridMesh(transform, heighlightGridName);

            if (gridMesh != null)
                gridMesh.NewTiles(tiles);
        }
        public void AddHeighlightGridTile(Vector2Int tile)
        {
            GridMesh gridMesh = GetGridMesh(transform, heighlightGridName);

            if (gridMesh != null)
                gridMesh.AddTile(tile);
        }
        public void RemoveHeighlightGridTile(Vector2Int tile)
        {
            GridMesh gridMesh = GetGridMesh(transform, heighlightGridName);

            if (gridMesh != null)
                gridMesh.RemoveTile(tile);
        }

        private void SubscribeToRainDeity()
        {
            if (rainDeityManager == null) return;

            foreach (Chunk chunk in chunks)
            {
                foreach (Tile tile in chunk.Tiles)
                {
                    rainDeityManager.OnRainStarted += tile.RainStarted;
                    rainDeityManager.OnRainStopped += tile.RainStopped;
                }
            }
        }

        private void Update()
        {
            foreach(Chunk chunk in chunks)
            {
                foreach(Tile tile in chunk.Tiles)
                {
                    tile.Update(Time.deltaTime);
                }
            }
        }
    }

}

