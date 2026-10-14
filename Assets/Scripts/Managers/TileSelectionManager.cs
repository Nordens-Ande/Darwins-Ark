using Assets.Scripts.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private void Update()
    {
        if (uiStateManager == null)
            return;

        bool isTerraforming = uiStateManager.IsTerraforming();

        // Just entered terraform mode
        //if (isTerraforming && !wasTerraforming)
        //{
        //    SetMiddleTile();
        //    RefreshAllTileVisibility();
        //}
        
        // Just existed terraform mode
        //if(!isTerraforming && wasTerraforming)
        //{
        //    ClearEverything();
        //}

        wasTerraforming = isTerraforming;
    }


    public void SetGridVisible(bool visible)
    {
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
        GameObject cursorGrid = tileManager.transform.Find("Cursor Grid").gameObject;
        if (cursorGrid != null)
        {
            cursorGrid.SetActive(visible);
        }
        else
        {
            Debug.LogWarning($"{cursorGrid.name} object could not set visibility because it was not found.");
        }
    }


    // Initial highlight
    private void SetMiddleTile()
    {
        int mapwidth = tileManager.ChunkGridSize.x * tileManager.ChunkSize;

        int mapDepth = tileManager.ChunkGridSize.y * tileManager.ChunkSize;

        int middleX = mapwidth / 2;
        int middleZ = mapDepth / 2;

        //highlightedTile = tileManager.GetTile(middleX, middleZ);

        highlightedTile = tileManager.GetTile(middleX, middleZ) != null ? new Vector2Int(middleX, middleZ) : null;

        if (highlightedTile != null) //kan ändra till "highlighetTile.HasValue" eller "highlighetTile is Vector2Int tilePos", null funkar ändå dock
        {
            SetTileColor(highlightedTile.Value, highlightColor);
        }
    }

    private void MoveHighlight(int xDirection, int zDirection)
    {
        if (highlightedTile == null)
            return;

        Vector2Int oldHighlightedTilePos = highlightedTile.Value;

        int newX = Mathf.RoundToInt(highlightedTile.Value.x) + xDirection;
        int newZ = Mathf.RoundToInt(highlightedTile.Value.y) + zDirection;

        Tile newTile = tileManager.GetTile(newX, newZ);

        if (newTile == null)
            return;

        // Restore old tile
        if (selectedTiles.Contains(oldHighlightedTilePos))
        {
            SetTileColor(oldHighlightedTilePos, selectedColor);
        }
        else
        {
            SetTileColor(oldHighlightedTilePos, normalColor);
        }

        highlightedTile = newTile != null ? newTile.GridPosition : null;

        // Holding select = toggle the tile we move onto
        if (selectHeld)
        {
            if (selectedTiles.Contains(highlightedTile.Value))
            {
                selectedTiles.Remove(highlightedTile.Value);

                SetTileColor(highlightedTile.Value, highlightColor);
            }
            else
            {
                selectedTiles.Add(highlightedTile.Value);

                SetTileColor(highlightedTile.Value, selectedColor);
            }
        }
        else
        {
            if (selectedTiles.Contains(highlightedTile.Value))
            {
                SetTileColor(highlightedTile.Value, selectedColor);
            }
            else
            {
                SetTileColor(highlightedTile.Value, highlightColor);
            }
        }

        UpdateTileVisibility(oldHighlightedTilePos);
        UpdateTileVisibility(highlightedTile.Value);
    }


    //----------------------------------------------
    //private void SetHighlight(int x, int z)
    //{
    //    Tile newTile = tileManager.GetTile(x, z);

    //    if (newTile == null)
    //        return;

    //    if (!highlightedTile.HasValue)
    //    {
    //        highlightedTile = newTile.GridPosition;

    //        SetTileColor(highlightedTile.Value, highlightColor);

    //        UpdateTileVisibility(highlightedTile.Value);

    //        return;
    //    }

    //    Vector2Int oldHighlightedTile = highlightedTile.Value;

    //    if (selectedTiles.Contains(oldHighlightedTile))
    //    {
    //        SetTileColor(oldHighlightedTile, selectedColor);
    //    }
    //    else
    //    {
    //        SetTileColor(oldHighlightedTile, normalColor);
    //    }

    //    highlightedTile = newTile != null ? newTile.GridPosition : null;

    //    if (selectHeld)
    //    {
    //        if (!isAddingTiles)
    //        {
    //            selectedTiles.Remove(highlightedTile.Value);

    //            SetTileColor(highlightedTile.Value, highlightColor);
    //        }
    //        else
    //        {
    //            selectedTiles.Add(highlightedTile.Value);

    //            SetTileColor(highlightedTile.Value, selectedColor);
    //        }
    //    }
    //    else
    //    {
    //        if (selectedTiles.Contains(highlightedTile.Value))
    //        {
    //            SetTileColor(highlightedTile.Value, selectedColor);
    //        }
    //        else
    //        {
    //            SetTileColor(highlightedTile.Value, highlightColor);
    //        }
    //    }

    //    UpdateTileVisibility(oldHighlightedTile);
    //    UpdateTileVisibility(highlightedTile.Value);
    //}


    private void SetHighlight(int x, int z)
    {
        Tile newTile = tileManager.GetTile(x, z);

        if (newTile == null)
            return;

        if (!highlightedTile.HasValue)
        {
            highlightedTile = newTile.GridPosition;

            //SetTileColor(highlightedTile.Value, highlightColor);

            //UpdateTileVisibility(highlightedTile.Value);

            tileManager.SetCursorGridTiles(new HashSet<Vector2Int> { highlightedTile.Value });

            return;
        }

        Vector2Int oldHighlightedTile = highlightedTile.Value;

        highlightedTile = newTile != null ? newTile.GridPosition : null;

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
        }

        //UpdateTileVisibility(oldHighlightedTile);
        //UpdateTileVisibility(highlightedTile.Value);
        tileManager.SetSelectionGridTiles(selectedTiles);
    }
    //----------------------------------------------


    // Clear selection
    public void ClearSelection()
    {
        foreach (Vector2Int tile in selectedTiles)
        {
            SetTileColor(
                tile,
                normalColor
            );
        }

        selectedTiles.Clear();

        // Giving tile grids their old color back
        if (highlightedTile != null)
        {
            SetTileColor(
                highlightedTile.Value,
                highlightColor
            );
        }

        RefreshAllTileVisibility();
    }
    private void ClearEverything()
    {
        foreach (Vector2Int tile in selectedTiles)
        {
            SetTileColor(
                tile,
                normalColor
            );
        }

        selectedTiles.Clear();

        if (highlightedTile != null)
        {
            SetTileColor(
                highlightedTile.Value,
                normalColor
            );
        }

        highlightedTile = null;

        selectHeld = false;

        RefreshAllTileVisibility();
    }

    // Grid color and visibility
    public void SetTileColor(Tile tile, Color color) //make this to overload to SetTileColor(Vector2Int..)
    {
        TerraformTileGridOverlay overlay = GetOverlay(tile);

        if (overlay == null)
            return;

        overlay.SetTileColor(tile, color);
    }
    public void SetTileColor(Vector2Int tilePos, Color color)
    {
        TerraformTileGridOverlay overlay = GetOverlay(tilePos);

        if (overlay == null)
            return;

        overlay.SetTileColor(tilePos, color);
    }
    //public void SetTileColor(Vector2Int tilePos, Color color)
    //{
    //    if (tileManager == null)
    //    {
    //        Debug.LogError("Could not select tile. TileManager.Instance does not exist or could not be found.");
    //        return;
    //    }

    //    tileManager.SetSelectionGridTiles
    //}


    private TerraformTileGridOverlay GetOverlay(Tile tile) //make this an overload to GetOverlay(Vector2Int)
    {
        Chunk chunk =
            tileManager.GetChunk(
                tile.position.x,
                tile.position.z
            );

        if (chunk == null)
            return null;

        int chunkIndex =
            tileManager.chunks.IndexOf(chunk);

        if (chunkIndex < 0)
            return null;

        Transform tileGrid =
            tileManager.chunkObjects[chunkIndex]
            .transform.Find("TileGrid");

        if (tileGrid == null)
            return null;

        return tileGrid.GetComponent<TerraformTileGridOverlay>();
    }

    private TerraformTileGridOverlay GetOverlay(Vector2Int tilePos)
    {
        Chunk chunk = tileManager.GetChunk(tilePos);

        if (chunk == null)
            return null;

        int chunkIndex =
            tileManager.chunks.IndexOf(chunk);

        if (chunkIndex < 0)
            return null;

        Transform tileGrid =
            tileManager.chunkObjects[chunkIndex]
            .transform.Find("TileGrid");

        if (tileGrid == null)
            return null;

        return tileGrid.GetComponent<TerraformTileGridOverlay>();
    }

    private void UpdateTileVisibility(Vector2Int tilePos)
    {
        TerraformTileGridOverlay overlay =
        GetOverlay(tilePos);

        if (overlay == null)
            return;

        switch (visibilityMode)
        {
            case TileGridVisibilityMode.AllTiles:

                overlay.SetTileVisible(
                    tilePos,
                    true
                );

                break;


            case TileGridVisibilityMode.HighlightedAndSelected:

                bool shouldBeVisible =
                    tilePos == highlightedTile ||
                    selectedTiles.Contains(tilePos);

                overlay.SetTileVisible(
                    tilePos,
                    shouldBeVisible
                );

                break;
        }
    }

    private void RefreshAllTileVisibility()
    {
        foreach (Chunk chunk in tileManager.chunks)
        {
            foreach (Tile tile in chunk.Tiles)
            {
                UpdateTileVisibility(tile.GridPosition);
            }
        }
    }
    // Selected tiles getter -----------------------
    public IEnumerable<Tile> GetSelectedTiles()
    {
        return selectedTiles
            .Select(pos => tileManager.GetTile(pos))
            .Where(tile => tile != null);
    }
    //----------------------------------------------

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

            isAddingTiles =
                !selectedTiles.Contains(highlightedTile.Value);

            ToggleHighlightedTile();
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

        //Handles Mouse input
        if (context.control.device is Mouse)
        {
            Vector3 mouseWorldPosition = MousePositionManager.Instance.WorldPosition;
            Vector2Int mouseGridPosition = new Vector2Int(Mathf.RoundToInt(mouseWorldPosition.x), Mathf.RoundToInt(mouseWorldPosition.z));
            
            SetHighlight(mouseGridPosition.x, mouseGridPosition.y);
            return;
        }
        //Handles Keyboard input
        Vector2 direction = context.ReadValue<Vector2>();

        // Debug
        Debug.Log("Direction: " + direction);

        int xDirection =
            Mathf.RoundToInt(direction.x);
        int zDirection =
            Mathf.RoundToInt(direction.y);

        MoveHighlight(xDirection, zDirection);

    }
    private void ToggleHighlightedTile()
    {
        if (highlightedTile == null)
            return;

        if (selectedTiles.Contains(highlightedTile.Value))
        {
            selectedTiles.Remove(highlightedTile.Value);

            SetTileColor(
                highlightedTile.Value,
                highlightColor
            );
        }
        else
        {
            selectedTiles.Add(highlightedTile.Value);

            SetTileColor(
                highlightedTile.Value,
                selectedColor
            );
        }

        UpdateTileVisibility(highlightedTile.Value);
    }
}
