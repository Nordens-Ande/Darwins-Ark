using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Environment
{
    public class TileManager : MonoBehaviour
    {
        public static TileManager Instance = null;

        [HideInInspector] public List<Chunk> chunks;
        [HideInInspector] public List<GameObject> chunkObjects;

        [SerializeField] private int chunkSize = 8; 
        [SerializeField] private Vector2Int chunkGridSize = Vector2Int.one;

        // -----------------------------------------------
        [SerializeField] private Material gridLineMaterial;
        // -----------------------------------------------

        public int ChunkSize => chunkSize;
        public Vector2Int ChunkGridSize => chunkGridSize;

        private string chunkGridName = "Chunk Grid";
        private string selectionGridName = "Selection Grid";
        private string cursorGridName = "Cursor Grid";

        private void Awake()
        {
            if (Instance == null)
                TileManager.Instance = this;
        }

        void Start()
        {
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
                //Chunk chunk = chunks[i];
                //chunk.GenerateMeshData();

                //GameObject chunkObject = new GameObject($"Chunk (x:{chunk.position.x}, z:{chunk.position.y})", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                //chunkObject.transform.parent = transform;
                //chunkObjects.Add(chunkObject);

                //chunkObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Shader Graphs/Lit Color"));

                //Mesh mesh = new Mesh();

                //mesh.SetVertices(chunk.vertices);
                //mesh.SetTriangles(chunk.triangles, 0);
                //mesh.SetColors(chunk.colors);

                //mesh.RecalculateNormals();
                //mesh.RecalculateBounds();

                //chunkObject.GetComponent<MeshFilter>().sharedMesh = mesh;
                //chunkObject.GetComponent<MeshCollider>().sharedMesh = mesh;

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


                //// Grid for chunk and tiles in TerraformUI -------------
                ////  Chunk Grid
                //GameObject chunkGridObject = 
                //    new GameObject("ChunkGrid"
                //    );

                //chunkGridObject.transform.SetParent(
                //    chunkObject.transform, false
                //    );

                //ChunkGridOverlay chunkGrid = chunkGridObject.AddComponent<ChunkGridOverlay>();

                //chunkGrid.Setup(chunk, chunkSize, gridLineMaterial);

                //chunkGridObject.SetActive(false);

                ////  Tile Grid
                //GameObject tileGridObject = new GameObject("TileGrid");
                //tileGridObject.transform.SetParent(chunkObject.transform, false);

                //TerraformTileGridOverlay tileGrid = tileGridObject.AddComponent<TerraformTileGridOverlay>();

                //tileGrid.Setup(chunk, gridLineMaterial);
                //tileGridObject.SetActive(false);
                ////-------------------------------------------------------
                
                //GenerateGrid(chunk, chunkObject);

            }

            for (int i = 0; i < chunks.Count; i++)
            {
                GenerateGrid(chunkObjects[i].transform, 0.001f, chunkGridName, Color.white, chunks[i][TileType.Grass]);
            }
            GenerateGrid(transform, 0.002f, selectionGridName, Color.red);
            GenerateGrid(transform, 0.003f, cursorGridName, Color.blue);

            //SetChunkGridVisible(false);
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

            foreach (Chunk chunk in chunks)
            {
                Vector2 chunkGridPos = chunk.position / chunkSize;
                Vector2 targetGridPos = new Vector2(Mathf.Floor(x / chunkSize), Mathf.Floor(z / chunkSize));

                if (chunkGridPos == targetGridPos)
                    return chunk;
            }

            Debug.LogWarning($"No chunk found for world position ({x}, {z})");
            return null;
        }
        public Chunk GetChunk(Vector2 positionXZ) => GetChunk(positionXZ.x, positionXZ.y);
        public Chunk GetChunk(Vector3 positionXZ) => GetChunk(positionXZ.x, positionXZ.y);

        public Tile GetTile(float x, float z) => GetChunk(x, z).GetTile((int)x, (int)z);
        public Tile GetTile(Vector2 positionXZ) => GetTile(positionXZ.x, positionXZ.y);
        public Tile GetTile(Vector3 positionXZ) => GetTile(positionXZ.x, positionXZ.z);


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

        //private void GenerateGrid(Chunk chunk, GameObject chunkObject)
        //{
        //    // Chunk grid object instansiate
        //    GameObject chunkGridObject = new GameObject("ChunkGrid");
        //    chunkGridObject.transform.SetParent(chunkObject.transform, false);

        //    // Chunk grid setup
        //    ChunkGridOverlay chunkGrid = chunkGridObject.AddComponent<ChunkGridOverlay>();
        //    chunkGrid.Setup(chunk, chunkSize, gridLineMaterial);
        //    chunkGridObject.SetActive(false);

        //    // Tile grid instansiate
        //    GameObject tileGridObject = new GameObject("TileGrid");
        //    tileGridObject.transform.SetParent(chunkObject.transform, false);

        //    // Tile grid setup
        //    TerraformTileGridOverlay tileGrid = tileGridObject.AddComponent<TerraformTileGridOverlay>();
        //    tileGrid.Setup(chunk, gridLineMaterial);
        //    tileGridObject.SetActive(false);
        //}
        //private void GenerateGrid(Chunk chunk, GameObject chunkObject)
        //{
        //    Type[] components = new Type[]
        //    {
        //        typeof(MeshFilter), typeof(MeshRenderer), typeof(LineMesh)
        //    };

        //    GameObject chunkGridObject = new GameObject("ChunkGrid", components);
        //    chunkGridObject.transform.SetParent(chunkObject.transform, false);

        //    MeshRenderer meshRenderer = chunkGridObject.GetComponent<MeshRenderer>();
        //    meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        //    meshRenderer.material.color = Color.red;

        //    LineMesh lineMesh = chunkGridObject.GetComponent<LineMesh>();
        //    lineMesh.meshData = chunk.gridMeshData;

        //    lineMesh.Rebuild();
        //}
        //private void GenerateGrid(Chunk chunk, GameObject chunkObject)
        //{
        //    Type[] components = new Type[]
        //    {
        //        typeof(MeshFilter), typeof(MeshRenderer), typeof(GridMesh)
        //    };

        //    GameObject chunkGridObject = new GameObject("ChunkGrid", components);
        //    chunkGridObject.transform.SetParent(chunkObject.transform, false);

        //    MeshRenderer meshRenderer = chunkGridObject.GetComponent<MeshRenderer>();
        //    meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        //    meshRenderer.material.color = Color.red;

        //    GridMesh gridMesh = chunkGridObject.GetComponent<GridMesh>();
        //    gridMesh.NewTiles(chunk.GetTiles(TileType.Grass));

        //    gridMesh.Rebuild();
        //}
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
            GameObject gridObject = transform.Find(selectionGridName).gameObject;
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

        public void SetCursorGridTiles(HashSet<Vector2Int> tiles)
        {
            GridMesh gridMesh = GetGridMesh(transform, cursorGridName);

            if (gridMesh != null)
                gridMesh.NewTiles(tiles);
        }
        public void AddCursorGridTile(Vector2Int tile)
        {
            GridMesh gridMesh = GetGridMesh(transform, cursorGridName);

            if (gridMesh != null)
                gridMesh.AddTile(tile);
        }
        public void RemoveCursorGridTile(Vector2Int tile)
        {
            GridMesh gridMesh = GetGridMesh(transform, cursorGridName);

            if (gridMesh != null)
                gridMesh.RemoveTile(tile);
        }
    }

}

