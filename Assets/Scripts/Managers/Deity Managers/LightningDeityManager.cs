using UnityEngine;
using Assets.Scripts.Environment;
using Assets.Scripts.Managers;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using System;

public class LightningDeityManager : SingletonManager<LightningDeityManager>
{
    private enum LightningCreationState
    {
        None,
        SelectTile
    }

    LightningCreationState state = LightningCreationState.None;

    [SerializeField] Material material;

    private List<Lightning> lightnings = new List<Lightning>();

    private GameManager gameManager;
    private MousePositionManager mousePositionManager;
    private TileManager tileManager;

    private float lightningEffectRadius = 2.5f;

    public event Action<LightningEventData> OnLightningTriggered;

    private Vector2Int? highlightedTile;

    void Start()
    {
        gameManager = GameManager.Instance;
        mousePositionManager = MousePositionManager.Instance;
        tileManager = TileManager.Instance;

        state = LightningCreationState.None;
    }

    public void OnCreateLightning()
    {
        SetGridVisible(true);
        state = LightningCreationState.SelectTile;
    }

    private void HandleSelectTile()
    {
        if (!mousePositionManager.TryGetWorldPosition(out Vector3 worldMousePos)) return;

        Tile tile = tileManager.GetTile(worldMousePos);
        if(tile == null) return;

        highlightedTile = tile.GridPosition;
        tileManager.SetHighlightGridTiles(new HashSet<Vector2Int> { highlightedTile.Value });

        if (Input.GetMouseButtonDown(0))
        {
            TriggerLightning(FindAffectedTiles(tile), tile);
        }
    }

    private void TriggerLightning(List<Tile> affectedTiles, Tile tile)
    {
        Lightning lightning = new Lightning(material, transform, tile, affectedTiles);
        lightnings.Add(lightning);
        SetGridVisible(false);
        state = LightningCreationState.None;
    }

    private List<Tile> FindAffectedTiles(Tile tile)
    {
        Vector2 tilePosition = new Vector2(tile.Position.x, tile.Position.z);
        
        List<Tile> affectedTiles = new List<Tile>();
        foreach(Chunk chunk in tileManager.chunks)
        {
            foreach(Tile otherTile in chunk.Tiles)
            {
                if (otherTile == null) continue;
                
                if (tile == otherTile) continue;

                Vector2 otherPos = new Vector2(otherTile.Position.x, otherTile.Position.z);

                if (Vector2.Distance(otherPos, tilePosition) > lightningEffectRadius) continue;
                
                //maybe add max height difference?
                
                affectedTiles.Add(otherTile);
            }
        }
        return affectedTiles;
    }

    public void SetGridVisible(bool visible)
    {
        if (tileManager == null)
        {
            Debug.LogError("Could not set grid visibility for tiles, TileManager.Instance does not exist or has not been found.");
            return;
        }

        GameObject cursorGrid = tileManager.transform.Find("Highlight Grid").gameObject;
        if (cursorGrid != null)
        {
            cursorGrid.SetActive(visible);
        }
        else
        {
            Debug.LogWarning($"{cursorGrid.name} object could not set visibility because it was not found.");
        }
    }

    void Update()
    {
        if(lightnings.Count > 0)
        {
            List<Tile> updatedTiles = new List<Tile>();
            foreach (Lightning lightning in lightnings)
            {
                updatedTiles = lightning.UpdateInstance(Time.deltaTime);
            }
            if(updatedTiles != null)
            {
                foreach (Tile tile in updatedTiles)
                {
                    OnLightningTriggered?.Invoke(new LightningEventData(tile));
                }
            }
        }
        
        if (state == LightningCreationState.None) return;
        else if (state == LightningCreationState.SelectTile) HandleSelectTile();
    }
}
