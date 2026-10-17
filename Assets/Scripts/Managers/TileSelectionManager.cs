using Assets.Scripts.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

// This class currenlty lacks the ability to change the color of tiles, this should be addressed and fixed in future.
// Curently you can find the colors of the grids in TileManager where the grids are instansiated.
public class TileSelectionManager : MonoBehaviour
{
    [SerializeField] private UIStateManager uiStateManager;
    [SerializeField] private TileManager tileManager;

    //[SerializeField] private float doubleTapTime = 0.3f;
    [SerializeField] private TileGridVisibilityMode visibilityMode = TileGridVisibilityMode.AllTiles;

    [SerializeField, ReadOnly] private Vector2Int? highlightedTile;

    private HashSet<Vector2Int> selectedTiles = new HashSet<Vector2Int>();

    //Mouse input
    [SerializeField] private float mouseMoveThreshold = 20f;
    private Vector2 mouseAccumulator;
    //---------------------------------------

    [SerializeField, ReadOnly] private bool selectHeld;
    [SerializeField, ReadOnly] private bool isAddingTiles;

    private bool wasTerraforming = false;

    //Flag used to activate/deactive keyboard/mouse inputs

    private Color normalColor = Color.red;
    private Color highlightColor = Color.cyan;
    private Color selectedColor = Color.yellow;

    private void Start()
    {
        //uiStateManager = UIStateManager.Instance;
        tileManager = TileManager.Instance;

        SetGridVisible(false);
    }

    //private void Update()
    //{
    //    if (uiStateManager == null)
    //        return;

    //    bool isTerraforming = uiStateManager.IsTerraforming();

    //    // Just entered terraform mode
    //    //if (isTerraforming && !wasTerraforming)
    //    //{
    //    //    SetMiddleTile();
    //    //    RefreshAllTileVisibility();
    //    //}

    //    // Just existed terraform mode
    //    //if(!isTerraforming && wasTerraforming)
    //    //{
    //    //    ClearEverything();
    //    //}

    //    wasTerraforming = isTerraforming;
    //}

    public IEnumerable<Tile> GetSelectedTiles()
    {
        return selectedTiles
            .Select(pos => tileManager.GetTile(pos))
            .Where(tile => tile != null);
    }


    // Initial highlight
    public void SetMiddleTile()
    {
        MousePositionManager.Instance.TryGetWorldPosition(out Vector3 cameraMid);

        Tile middleTile = tileManager.GetTile(cameraMid.x, cameraMid.y);

        highlightedTile = middleTile != null ? middleTile.GridPosition : Vector2Int.zero;
    }

    private void SetHighlight(int x, int z)
    {
        Tile newTile = tileManager.GetTile(x, z);

        if (newTile == null)
            return;

        if (!highlightedTile.HasValue)
        {
            highlightedTile = newTile.GridPosition;
        }
        else if (newTile.GridPosition == highlightedTile.Value)
        {
            return;
        }
        else
        {
            highlightedTile = newTile.GridPosition;
            tileManager.SetHighlightGridTiles(new HashSet<Vector2Int> { highlightedTile.Value });
        }

        if (selectHeld)
        {
            if (!isAddingTiles)
            {
                selectedTiles.Remove(highlightedTile.Value);
            }
            else
            {
                selectedTiles.Add(highlightedTile.Value);
            }

            tileManager.SetSelectionGridTiles(selectedTiles);
        }
    }

    private void MoveHighlight(int xDirection, int zDirection)
    {
        Vector2Int oldHighlightedTilePos = highlightedTile ?? Vector2Int.zero;

        int newX = Mathf.RoundToInt(highlightedTile.Value.x) + xDirection;
        int newZ = Mathf.RoundToInt(highlightedTile.Value.y) + zDirection;

        SetHighlight(newX, newZ);
    }


    public void SetGridVisible(bool visible)
    {
        RefreshGrids();

        if (tileManager == null)
        {
            Debug.LogError("Could not set grid visibility for tiles, TileManager.Instance does not exist or has not been found.");
            return;
        }

        // Visibility of the Chunk Grid (i.e all tiles)
        if (visibilityMode == TileGridVisibilityMode.AllTiles || !visible)
        {
            List<GameObject> chunks = tileManager.chunkObjects;
            foreach (GameObject chunk in chunks)
            {
                GameObject chunkGrid = chunk.transform.Find("Chunk Grid").gameObject;
                if (chunkGrid != null)
                {
                    chunkGrid.gameObject.SetActive(visible);
                }
                else
                {
                    Debug.LogWarning($"Grid object in {chunk.name} could not set visibility because it was not found.");
                }
            }
        }

        // Visibility of the selection
        GameObject selectionGrid = tileManager.transform.Find("Selection Grid").gameObject;
        if (selectionGrid != null)
        {
            selectionGrid.SetActive(visible);
        }
        else
        {
            Debug.LogWarning($"{selectionGrid.name} object could not set visibility because it was not found.");
        }

        // Visibility of the cursor
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


    private void RefreshGrids()
    {
        if (highlightedTile.HasValue)
            tileManager.SetHighlightGridTiles(new HashSet<Vector2Int> { highlightedTile.Value });

        tileManager.SetSelectionGridTiles(selectedTiles);
    }


    // Clear selection
    public void ClearSelection()
    {
        selectedTiles.Clear();
        
        RefreshGrids();
    }
    public void ClearEverything()
    {
        selectedTiles.Clear();

        highlightedTile = null;

        selectHeld = false;

        RefreshGrids();
    }


    // Player Input Map
    public void OnTileSelect(InputAction.CallbackContext context)
    {
        if (!uiStateManager.IsTerraforming())
            return;

        if (!highlightedTile.HasValue)
            return;

        if (context.started)
        {
            selectHeld = true;

            isAddingTiles = !selectedTiles.Contains(highlightedTile.Value);

            // If a current highlight already exists, we add it
            if(highlightedTile.HasValue)
            {
                if (!isAddingTiles)
                {
                    selectedTiles.Remove(highlightedTile.Value);
                }
                else
                {
                    selectedTiles.Add(highlightedTile.Value);
                }
            }

            //ToggleHighlightedTile();
        }

        if (context.canceled)
        {
            selectHeld = false;
        }
    }
    public void OnTileMove(InputAction.CallbackContext context)
    {
        // Debug
        //Debug.Log("TIleMove event: " + context.phase);

        if (!uiStateManager.IsTerraforming())
            return;

        if(!context.performed)
            return;

        // Handles Mouse input
        if (context.control.device is Mouse)
        {
            Vector3 mouseWorldPosition = MousePositionManager.Instance.WorldPosition;
            Vector2Int mouseGridPosition = new Vector2Int(Mathf.RoundToInt(mouseWorldPosition.x), Mathf.RoundToInt(mouseWorldPosition.z));
            
            SetHighlight(mouseGridPosition.x, mouseGridPosition.y);
            return;
        }
        // Handles Keyboard input
        Vector2 direction = context.ReadValue<Vector2>();

        // Debug
        Debug.Log("Direction: " + direction);

        int xDirection =
            Mathf.RoundToInt(direction.x);
        int zDirection =
            Mathf.RoundToInt(direction.y);

        MoveHighlight(xDirection, zDirection);
    }
}
