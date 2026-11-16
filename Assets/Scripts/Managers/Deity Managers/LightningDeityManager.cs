using UnityEngine;
using Assets.Scripts.Environment;
using Assets.Scripts.Managers;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using System;
using Assets.Enums.GameTransitions;
using Assets.Scripts.Events.Data;
using UnityEngine.InputSystem;

public class LightningDeityManager : SingletonManager<LightningDeityManager>
{
    private enum LightningCreationState
    {
        None,
        SelectTile
    }

    private bool isActive = false;
    private LightningCreationState state = LightningCreationState.None;

    [SerializeField] private Material material;

    private List<Lightning> lightnings = new List<Lightning>();

    private GameManager gameManager;
    private MousePositionManager mousePositionManager;
    private TileManager tileManager;

    private float lightningEffectRadius = 2.5f;

    public event Action<LightningEventData> OnLightningTriggered;

    private Vector2Int highlightedTile;
    private Vector2Int highlightedTilePrev;
    private HashSet<Vector2Int> highlights = new HashSet<Vector2Int>();

    void Start()
    {
        gameManager = GameManager.Instance;
        mousePositionManager = MousePositionManager.Instance;
        tileManager = TileManager.Instance;

        state = LightningCreationState.None;
        gameManager.OnGameModeChanged += HandleModeChanged;
    }

    private void HandleModeChanged(GameModeChangedData data)
    {
        GameMode mode = data.NewValue;
        switch(mode)
        {
            case GameMode.Lightning:
                isActive = true;
                SetGridVisible(true);
                state = LightningCreationState.SelectTile;
                break;
            case GameMode.Game:
                SetGridVisible(false);
                isActive = false;
                break;
        }
    }

    public void HandleSelectTile(InputAction.CallbackContext context)
    {
        if (!isActive) return;
        if (state != LightningCreationState.SelectTile) return;

        if (!mousePositionManager.TryGetWorldPosition(out Vector3 worldMousePos)) return;

        Tile tile = tileManager.GetTile(worldMousePos);
        if(tile == null) return;

        if (context.performed)
        {
            TriggerLightning(FindAffectedTiles(tile), tile);
        }
    }

    private void TriggerLightning(List<Tile> affectedTiles, Tile tile)
    {
        Lightning lightning = new Lightning(material, transform, tile, affectedTiles);
        lightnings.Add(lightning);

        state = LightningCreationState.None;
        gameManager.ChangeMode(gameObject, GameMode.Game);
    }

    private List<Tile> FindAffectedTiles(Tile tile)
    {
        Vector2 tilePosition = new Vector2(tile.Position.x, tile.Position.z);
        
        List<Tile> affectedTiles = new List<Tile>();

        int radInt = Mathf.CeilToInt(lightningEffectRadius);
        float radSquared = lightningEffectRadius * lightningEffectRadius;
        for(int x = -radInt; x <= radInt; x++)
        {
            for (int z = -radInt; z <= radInt; z++)
            {
                if (x * x + z * z > radSquared) continue;
                Tile otherTile = tileManager.GetTile(tilePosition.x + x, tilePosition.y + z);
                if(otherTile != null) affectedTiles.Add(otherTile);
            }
        }

        //foreach(Chunk chunk in tileManager.chunks)
        //{
        //    foreach(Tile otherTile in chunk.Tiles)
        //    {
        //        if (otherTile == null) continue;
                
        //        if (tile == otherTile) continue;

        //        Vector2 otherPos = new Vector2(otherTile.Position.x, otherTile.Position.z);

        //        if (Vector2.Distance(otherPos, tilePosition) > lightningEffectRadius) continue;
                
        //        //maybe add max height difference?
                
        //        affectedTiles.Add(otherTile);
        //    }
        //}
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

    private void UpdateTileHighlights()
    {
        if (!mousePositionManager.TryGetWorldPosition(out Vector3 worldMousePos)) return;

        Tile tile = tileManager.GetTile(worldMousePos);
        if (tile == null) return;

        highlightedTile = tile.GridPosition;

        if (highlightedTile == highlightedTilePrev) return;

        //get affected tiles
        List<Tile> affectedTiles = FindAffectedTiles(tile);

        //highlight them
        highlights.Clear();
        foreach (Tile affectedTile in affectedTiles)
        {
            highlights.Add(affectedTile.GridPosition);
        }
        highlights.Add(highlightedTile);

        tileManager.SetHighlightGridTiles(highlights);
        highlightedTilePrev = highlightedTile;
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
        
        if (!isActive) return;
        if (state != LightningCreationState.SelectTile) return;
        UpdateTileHighlights();
    }
}
