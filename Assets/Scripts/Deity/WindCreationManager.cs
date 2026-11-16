using Assets.Scripts.Environment;
using Assets.Scripts.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Assets.Enums.GameTransitions;
using Assets.Scripts.Events.Data;

public class WindCreationManager : MonoBehaviour
{
    private enum WindCreationState
    {
        None,
        SelectTile,
        SelectDirection
    }

    private bool isActive = false;
    WindCreationState state = WindCreationState.None;

    private GameManager gameManager;
    private MousePositionManager mousePositionManager;
    private TileManager tileManager;
    private WindDeityManager windDeityManager;

    private Tile initialTile;
    private Tile directionTile;

    private Vector2Int? highlightedTileInitial;
    private Vector2Int? highlightedTileDirection;

    void Start()
    {
        gameManager = GameManager.Instance;
        mousePositionManager = MousePositionManager.Instance;
        tileManager = TileManager.Instance;
        windDeityManager = WindDeityManager.Instance;

        gameManager.OnGameModeChanged += HandleModeChanged;
    }

    private void HandleModeChanged(GameModeChangedData data)
    {
        GameMode mode = data.NewValue;
        switch (mode)
        {
            case GameMode.Wind:
                isActive = true;
                SetGridVisible(true);
                initialTile = null;
                directionTile = null;
                state = WindCreationState.SelectTile;
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
        if (state != WindCreationState.SelectTile) return;

        if (mousePositionManager == null) Debug.Log("mouseposmanager null. from wind creation manager");

        if (!mousePositionManager.TryGetWorldPosition(out Vector3 pos)) return;

        Tile tile = tileManager.GetTile(pos);
        if (tile == null) return;

        if (!context.performed) return;

        initialTile = tile;

        state = WindCreationState.SelectDirection;

    }

    public void HandleSelectDirection(InputAction.CallbackContext context)
    {
        if (!isActive) return;
        if (state != WindCreationState.SelectDirection) return;
        
        if (!mousePositionManager.TryGetWorldPosition(out Vector3 pos)) return;

        Tile tile = tileManager.GetTile(pos);
        if (tile == null) return;

        if (tile == initialTile) return;

        if (!context.performed) return;

        directionTile = tile;

        Vector3 windDirection = directionTile.Position - initialTile.Position;
        windDirection.y = 0;

        windDeityManager.TriggerWind(tile, windDirection.normalized);
        initialTile = null;
        directionTile = null;
        state = WindCreationState.None;
        gameManager.ChangeMode(gameObject, GameMode.Game);
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

    private void UpdateHighlightedTile()
    {
        if (!mousePositionManager.TryGetWorldPosition(out Vector3 worldMousePos)) return;

        Tile tile = tileManager.GetTile(worldMousePos);
        if (tile == null) return;

        SetGridVisible(true);
        if(state == WindCreationState.SelectTile)
        {
            highlightedTileInitial = tile.GridPosition;
            tileManager.SetHighlightGridTiles(new HashSet<Vector2Int> { highlightedTileInitial.Value });
        }
        else if (state == WindCreationState.SelectDirection)
        {
            highlightedTileDirection = tile.GridPosition;
            tileManager.SetHighlightGridTiles(new HashSet<Vector2Int> { highlightedTileInitial.Value, highlightedTileDirection.Value });
        }
    }

    void Update()
    {
        if (!isActive) return;
        UpdateHighlightedTile();
    }
}
