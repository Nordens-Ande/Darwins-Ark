using Assets.Scripts.Environment;
using Assets.Scripts.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WindCreationManager : MonoBehaviour
{
    private enum WindCreationState
    {
        None,
        SelectTile,
        SelectDirection
    }

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
    }

    public void OnCreateWind() // call from ui
    {
        if (state != WindCreationState.None) return;

        initialTile = null;
        directionTile = null;

        SetGridVisible(true);

        state = WindCreationState.SelectTile;
    }

    private void HandleSelectTile()
    {
        if (mousePositionManager == null) Debug.Log("mouseposmanager null. from wind creation manager");

        if (!mousePositionManager.TryGetWorldPosition(out Vector3 pos)) return;

        Tile tile = tileManager.GetTile(pos);
        if (tile == null) return;

        highlightedTileInitial = tile.GridPosition;
        tileManager.SetHighlightGridTiles(new HashSet<Vector2Int> { highlightedTileInitial.Value });

        if (!Input.GetMouseButtonDown(0)) return;

        initialTile = tile;

        state = WindCreationState.SelectDirection;

    }

    private void HandleSelectDirection()
    {
        if (!mousePositionManager.TryGetWorldPosition(out Vector3 pos)) return;

        Tile tile = tileManager.GetTile(pos);
        if (tile == null) return;

        highlightedTileDirection = tile.GridPosition;
        tileManager.SetHighlightGridTiles(new HashSet<Vector2Int> { highlightedTileInitial.Value, highlightedTileDirection.Value });

        if (tile == initialTile) return;

        if (!Input.GetMouseButtonDown(0)) return;

        directionTile = tile;

        Vector3 windDirection = directionTile.Position - initialTile.Position;
        windDirection.y = 0;

        windDeityManager.TriggerWind(tile, windDirection.normalized);
        initialTile = null;
        directionTile = null;
        state = WindCreationState.None;
        SetGridVisible(false);
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
        if(state == WindCreationState.None) return;

        if (state == WindCreationState.SelectTile) HandleSelectTile();
        if (state == WindCreationState.SelectDirection) HandleSelectDirection();
    }
}
