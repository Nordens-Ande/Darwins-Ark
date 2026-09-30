using Assets.Scripts.Environment;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelectionManager : MonoBehaviour
{
    [SerializeField] private UIStateManager uiStateManager;
    //[SerializeField] private float doubleTapTime = 0.3f;
    [SerializeField]
        private TileGridVisibilityMode visibilityMode =
                TileGridVisibilityMode.AllTiles;

    private Vector2Int? highlightedTile;

    private HashSet<Vector2Int> selectedTiles =
        new HashSet<Vector2Int>();

    //// Select Button Settings-------------------
    //[SerializeField] private Key selectionKey = Key.LeftCtrl;
    //private float lastSelectKeyPressTime = -1f;
    ////------------------------------------------
    private bool selectHeld;

    private bool wasTerraforming = false;

    //Flag used to activate/deactive keyboard/mouse inputs
    private bool usingMouse = true;

    private Color normalColor = Color.red;
    private Color highlightColor = Color.cyan;
    private Color selectedColor = Color.yellow;

    private void Update()
    {
        if (uiStateManager == null)
            return;

        bool isTerraforming =
            uiStateManager.IsTerraforming();

        // Just entered terraform mode
        if (isTerraforming && !wasTerraforming)
        {
            SetMiddleTile();
            RefreshAllTileVisibility();
        }
        
        // Just existed terraform mode
        if(!isTerraforming && wasTerraforming)
        {
            ClearEverything();
        }

        wasTerraforming = isTerraforming;
    }

    // Initial highlight
    private void SetMiddleTile()
    {
        int mapwidth =
            TileManager.Instance.ChunkGridSize.x * 
            TileManager.Instance.ChunkSize;

        int mapDepth =
            TileManager.Instance.ChunkGridSize.y *
            TileManager.Instance.ChunkSize;

        int middleX = mapwidth / 2;
        int middleZ = mapDepth / 2;

        //highlightedTile =
        //    TileManager.Instance.GetTile(middleX, middleZ);

        highlightedTile = TileManager.Instance.GetTile(middleX, middleZ) != null ? new Vector2Int(middleX, middleZ) : null;

        if (highlightedTile != null) //kan ändra till "highlighetTile.HasValue" eller "highlighetTile is Vector2Int tilePos", null funkar ändå dock
        {
            SetTileColor(
                highlightedTile.Value, 
                highlightColor
                );
        }
    }

    private void MoveHighlight(int xDirection, int zDirection)
    {
        if (highlightedTile == null)
            return;

        Vector2Int oldHighlightedTilePos = highlightedTile.Value;

        int newX =
            Mathf.RoundToInt(highlightedTile.Value.x)
            + xDirection;

        int newZ =
            Mathf.RoundToInt(highlightedTile.Value.y)
            + zDirection;

        Tile newTile =
            TileManager.Instance.GetTile(
                newX,
                newZ
            );

        if (newTile == null)
            return;

        // Restore old tile
        if (selectedTiles.Contains(oldHighlightedTilePos))
        {
            SetTileColor(
                oldHighlightedTilePos,
                selectedColor
            );
        }
        else
        {
            SetTileColor(
                oldHighlightedTilePos,
                normalColor
            );
        }

        highlightedTile = newTile != null ? newTile.GridPosition : null;

        // Holding select = toggle the tile we move onto
        if (selectHeld)
        {
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
        }
        else
        {
            if (selectedTiles.Contains(highlightedTile.Value))
            {
                SetTileColor(
                    highlightedTile.Value,
                    selectedColor
                );
            }
            else
            {
                SetTileColor(
                    highlightedTile.Value,
                    highlightColor
                );
            }
        }

        UpdateTileVisibility(oldHighlightedTilePos);
        UpdateTileVisibility(highlightedTile.Value);
    }


    //----------------------------------------------
    private void SetHighlight(int x, int z)
    {
        Tile newTile = TileManager.Instance.GetTile(x, z);
        if (newTile == null)
            return;

        Vector2Int oldHighlightedTile = highlightedTile.Value;

        if (selectedTiles.Contains(oldHighlightedTile))
        {
            SetTileColor(oldHighlightedTile, selectedColor);
        }
        else
        {
            SetTileColor(oldHighlightedTile, normalColor);
        }

        highlightedTile = newTile != null ? newTile.GridPosition : null;

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

        UpdateTileVisibility(oldHighlightedTile);
        UpdateTileVisibility(highlightedTile.Value);
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
        TerraformTileGridOverlay overlay =
        GetOverlay(tile);

        if (overlay == null)
            return;

        overlay.SetTileColor(tile, color);
    }
    public void SetTileColor(Vector2Int tilePos, Color color)
    {
        TerraformTileGridOverlay overlay =
        GetOverlay(tilePos);

        if (overlay == null)
            return;

        overlay.SetTileColor(tilePos, color);
    }


    private TerraformTileGridOverlay GetOverlay(Tile tile) //make this an overload to GetOverlay(Vector2Int)
    {
        Chunk chunk =
            TileManager.Instance.GetChunk(
                tile.position.x,
                tile.position.z
            );

        if (chunk == null)
            return null;

        int chunkIndex =
            TileManager.Instance.chunks.IndexOf(chunk);

        if (chunkIndex < 0)
            return null;

        Transform tileGrid =
            TileManager.Instance.chunkObjects[chunkIndex]
            .transform.Find("TileGrid");

        if (tileGrid == null)
            return null;

        return tileGrid.GetComponent<TerraformTileGridOverlay>();
    }

    private TerraformTileGridOverlay GetOverlay(Vector2Int tilePos)
    {
        Chunk chunk = TileManager.Instance.GetChunk(tilePos);

        if (chunk == null)
            return null;

        int chunkIndex =
            TileManager.Instance.chunks.IndexOf(chunk);

        if (chunkIndex < 0)
            return null;

        Transform tileGrid =
            TileManager.Instance.chunkObjects[chunkIndex]
            .transform.Find("TileGrid");

        if (tileGrid == null)
            return null;

        return tileGrid.GetComponent<TerraformTileGridOverlay>();
    }

    //private void UpdateTileVisibility(Tile tile)
    //{
    //    TerraformTileGridOverlay overlay =
    //    GetOverlay(tile);

    //    if (overlay == null)
    //        return;

    //    switch (visibilityMode)
    //    {
    //        case TileGridVisibilityMode.AllTiles:

    //            overlay.SetTileVisible(
    //                tile,
    //                true
    //            );

    //            break;


    //        case TileGridVisibilityMode.HighlightedAndSelected:

    //            bool shouldBeVisible =
    //                tile == highlightedTile ||
    //                selectedTiles.Contains(tile);

    //            overlay.SetTileVisible(
    //                tile,
    //                shouldBeVisible
    //            );

    //            break;
    //    }
    //}
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
        foreach (Chunk chunk in TileManager.Instance.chunks)
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
            .Select(pos => TileManager.Instance.GetTile(pos))
            .Where(tile => tile != null);
    }
    //----------------------------------------------

    // Player Input Map
    public void OnTileSelect(InputAction.CallbackContext context)
    {
        if (!uiStateManager.IsTerraforming())
            return;

        if (context.started)
        {
            selectHeld = true;
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


    // --------------------------------------------------
}
