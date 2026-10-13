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
                
                GenerateGrid(chunk, chunkObject);

            }

            SetChunkGridVisible(false);
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
            foreach(Chunk chunk in chunks)
            {
                if (chunk.position / chunkSize == new Vector2(Mathf.Floor(x / chunkSize), Mathf.Floor(z / chunkSize)))
                    return chunk;
            }
            return null;
        }

        public Chunk GetChunk(Vector2 positionXZ) => GetChunk(positionXZ.x, positionXZ.y);

        public Chunk GetChunk(Vector3 position)
        {
            return GetChunk(position.x, position.y);
        }

        public Tile GetTile(float x, float z)
        {
            foreach (Chunk chunk in chunks)
            {
                if (chunk.position / chunkSize == new Vector2(Mathf.Floor(x / chunkSize), Mathf.Floor(z / chunkSize)))
                    return chunk.GetTile((int)x, (int)z);
            }
            return null;
        }
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
        private void GenerateGrid(Chunk chunk, GameObject chunkObject)
        {
            Type[] components = new Type[]
            {
                typeof(MeshFilter), typeof(MeshRenderer), typeof(LineMesh)
            };

            GameObject chunkGridObject = new GameObject("ChunkGrid", components);
            chunkGridObject.transform.SetParent(chunkObject.transform, false);

            MeshRenderer meshRenderer = chunkGridObject.GetComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            meshRenderer.material.color = Color.red;

            LineMesh lineMesh = chunkGridObject.GetComponent<LineMesh>();
            lineMesh.meshData = chunk.lineMeshData;

            lineMesh.Rebuild();
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
    }

}

