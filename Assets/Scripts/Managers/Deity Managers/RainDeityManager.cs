using UnityEngine;
using System.Collections.Generic;
using System.Resources;
using UnityEngine.Assemblies;
using System;
using System.Runtime.CompilerServices;
using UnityEngine.Android;
using System.Threading;
using Assets.Scripts.Environment;
using Assets.Enums.GameTransitions;
using Assets.Scripts.Events.Data;
using Assets.Scripts.Managers;
using UnityEngine.InputSystem;
using UnityEditor;

public class RainDeityManager : SingletonManager<RainDeityManager>
{
    private enum RainCreationState
    {
        None,
        SelectTile
    }

    private bool isActive = false;
    private RainCreationState state = RainCreationState.None;

    private float rainRadius = 6;

    private GameManager gameManager;
    private CurrencyManager currencyManager;
    private MousePositionManager mousePositionManager;
    private TileManager tileManager;

    private int baseCost;

    private Vector2Int highlightedTile;
    private Vector2Int highlightedTilePrev;
    private HashSet<Vector2Int> highlights = new HashSet<Vector2Int>();

    [SerializeField] public bool triggerRain = false; // for testing

    public string GetCostString => baseCost.ToString(); //for ui?

    private List<Rain> rainInstances = new List<Rain>();


    private void Start()
    {
        gameManager = GameManager.Instance;
        mousePositionManager = MousePositionManager.Instance;
        currencyManager = CurrencyManager.Instance;
        tileManager = TileManager.Instance;
        baseCost = 10;
        
        gameManager.OnGameModeChanged += HandleModeChanged;
    }

    private void HandleModeChanged(GameModeChangedData data)
    {
        GameMode mode = data.NewValue;
        switch (mode)
        {
            case GameMode.Rain:
                isActive = true;
                SetGridVisible(true);
                state = RainCreationState.SelectTile;
                break;
            case GameMode.Game:
                SetGridVisible(false);
                isActive = false;
                state = RainCreationState.None;
                break;
        }
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

    public void HandleSelectTile(InputAction.CallbackContext context)
    {
        if (!isActive) return;
        if (state != RainCreationState.SelectTile) return;

        if (!mousePositionManager.TryGetWorldPosition(out Vector3 worldMousePos)) return;

        Tile tile = tileManager.GetTile(worldMousePos);
        if (tile == null) return;

        if (context.performed)
        {
            TriggerRain(tile, FindAffectedTiles(tile));
        }
    }

    private List<Tile> FindAffectedTiles(Tile tile)
    {
        Vector2 tilePosition = new Vector2(tile.Position.x, tile.Position.z);

        List<Tile> affectedTiles = new List<Tile>();

        int radInt = Mathf.CeilToInt(rainRadius);
        float radSquared = rainRadius * rainRadius;
        for (int x = -radInt; x <= radInt; x++)
        {
            for (int z = -radInt; z <= radInt; z++)
            {
                if (x * x + z * z > radSquared) continue;
                Tile otherTile = tileManager.GetTile(tilePosition.x + x, tilePosition.y + z);
                if (otherTile != null) affectedTiles.Add(otherTile);
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

    public void TriggerRain(Tile tile, List<Tile> affectedTiles) // on button press
    {
        
        //uncomment this later after testing
        //if (!currencyManager.SpendDeityPoints(baseCost)) return;


        rainInstances.Add(new Rain(rainRadius, tile, affectedTiles));

        gameManager.ChangeMode(gameObject, GameMode.Game);
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
        SetGridVisible(true); // for some reason doesnt work without this ?
        foreach (Tile affectedTile in affectedTiles)
        {
            highlights.Add(affectedTile.GridPosition);
        }
        highlights.Add(highlightedTile);
        tileManager.SetHighlightGridTiles(highlights);
        
        highlightedTilePrev = highlightedTile;
    }

    private void Update()
    {
        for (int i = rainInstances.Count - 1; i >= 0; i--)
        {
            Rain rain = rainInstances[i];
            if (rain == null) continue;

            if (rain.RainComplete == true)
            {
                rainInstances.RemoveAt(i);
                continue;
            }

            rain.UpdateInstance(Time.deltaTime);
        }

        if (!isActive) return;
        if (state != RainCreationState.SelectTile) return;
        UpdateTileHighlights();
    }
}
