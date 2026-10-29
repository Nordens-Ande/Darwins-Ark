using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private Material waterMaterial;

        // -----------------------------------------------
        //[SerializeField] private Material gridLineMaterial;
        // -----------------------------------------------

        public int ChunkSize => chunkSize;
        public Vector2Int ChunkGridSize => chunkGridSize;

        private string chunkGridName = "Chunk Grid";
        private string selectionGridName = "Selection Grid";
        private string heighlightGridName = "Highlight Grid";

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

            GenerateWater();
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

            for (int i = 0; i < chunks.Count; i++)
            {
                if (!chunks[i].isDirty)
                    continue;

                chunks[i].GenerateMeshData();
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

        private void GenerateWater()
        {
            if (!createWater)
                return;

            GameObject waterObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            waterObject.name = "Water Quad";
            waterObject.transform.parent = transform;
            
            waterObject.transform.localPosition = waterObject.transform.localPosition + new Vector3(0, waterLevel, 0);
            waterObject.transform.localScale = new Vector3(chunkGridSize.x * chunkSize, chunkGridSize.y * chunkSize, 1f);
            waterObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            
            waterObject.GetComponent<MeshRenderer>().material = waterMaterial;
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

