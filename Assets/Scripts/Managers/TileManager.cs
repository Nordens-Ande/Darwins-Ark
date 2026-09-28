using NUnit.Framework.Internal;
using System.Collections.Generic;
using UnityEngine;

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

        public int ChunkSize
        { get { return chunkSize; } }

        public Vector2Int ChunkGridSize
        { get { return chunkGridSize; } }

        private void Awake()
        {
            if (Instance == null)
                TileManager.Instance = this;
        }

        void Start()
        {
            chunks = new List<Chunk>();
            chunkObjects = new List<GameObject>();

            //chunks.Add(new Chunk(Vector2.zero, 8));
            //chunks.Add(new Chunk(new Vector2(-8, 0), 8));
            //chunks.Add(new Chunk(new Vector2(-8, -8), 8));
            //chunks.Add(new Chunk(new Vector2(0, -8), 8));

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
                chunk.GenerateMeshData();
                
                GameObject chunkObject = new GameObject($"Chunk (x:{chunk.position.x}, z:{chunk.position.y})", typeof(MeshFilter), typeof(MeshRenderer));
                chunkObject.transform.parent = transform;
                chunkObjects.Add(chunkObject);

                chunkObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Shader Graphs/Lit Color"));
                
                Mesh mesh = new Mesh();

                mesh.SetVertices(chunk.vertices);
                mesh.SetTriangles(chunk.triangles, 0);
                mesh.SetColors(chunk.colors);

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                chunkObject.GetComponent<MeshFilter>().sharedMesh = mesh;

                // Grid for chunk and tiles in TerraformUI -------------
                //  Chunk Grid
                GameObject chunkGridObject = new GameObject("ChunkGrid");
                chunkGridObject.transform.SetParent(chunkObject.transform, false);

                ChunkGridOverlay chunkGrid =
                    chunkGridObject.AddComponent<ChunkGridOverlay>();

                chunkGrid.Setup(chunkSize, gridLineMaterial);
                chunkGridObject.SetActive(false);

                //  Tile Grid
                GameObject tileGridObject = new GameObject("TileGrid");
                tileGridObject.transform.SetParent(chunkObject.transform, false);

                TerraformTileGridOverlay tileGrid = 
                    tileGridObject.AddComponent<TerraformTileGridOverlay>();

                tileGrid.Setup(chunk, gridLineMaterial);
                tileGridObject.SetActive(false);
                //-------------------------------------------------------
            }
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

        public Chunk GetChunk(float x, float z)
        {
            foreach(Chunk chunk in chunks)
            {
                if (chunk.position / chunkSize == new Vector2(Mathf.Floor(x / chunkSize), Mathf.Floor(z / chunkSize)))
                    return chunk;
            }
            return null;
        }

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
        public Tile GetTile(Vector2 posXZ)
        {
            return GetTile(posXZ.x, posXZ.y);
        }
        public Tile GetTile(Vector3 pos)
        {
            return GetTile(pos.x, pos.z);
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

