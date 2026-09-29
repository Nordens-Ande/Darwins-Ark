using Assets.Scripts.Environment;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelectionManager : MonoBehaviour
{
    [SerializeField] private UIStateManager uiStateManager;
    //[SerializeField] private float doubleTapTime = 0.3f;
    [SerializeField]
        private TileGridVisibilityMode visibilityMode =
                TileGridVisibilityMode.AllTiles;

    private Tile highlightedTile;

    private HashSet<Tile> selectedTiles =
        new HashSet<Tile>();

    //// Select Button Settings-------------------
    //[SerializeField] private Key selectionKey = Key.LeftCtrl;
    //private float lastSelectKeyPressTime = -1f;
    ////------------------------------------------
    private bool selectHeld;

    private bool wasTerraforming = false;

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

        highlightedTile =
            TileManager.Instance.GetTile(middleX, middleZ);

        if (highlightedTile != null)
        {
            SetTileColor(
                highlightedTile, 
                highlightColor
                );
        }
    }

    private void MoveHighlight(int xDirection, int zDirection)
    {
        if (highlightedTile == null)
            return;

        Tile oldHighlightedTile = highlightedTile;

        int newX =
            Mathf.RoundToInt(highlightedTile.position.x)
            + xDirection;

        int newZ =
            Mathf.RoundToInt(highlightedTile.position.z)
            + zDirection;

        Tile newTile =
            TileManager.Instance.GetTile(
                newX,
                newZ
            );

        if (newTile == null)
            return;

        // Restore old tile
        if (selectedTiles.Contains(oldHighlightedTile))
        {
            SetTileColor(
                oldHighlightedTile,
                selectedColor
            );
        }
        else
        {
            SetTileColor(
                oldHighlightedTile,
                normalColor
            );
        }

        highlightedTile = newTile;

        // Holding select = toggle the tile we move onto
        if (selectHeld)
        {
            if (selectedTiles.Contains(highlightedTile))
            {
                selectedTiles.Remove(highlightedTile);

                SetTileColor(
                    highlightedTile,
                    highlightColor
                );
            }
            else
            {
                selectedTiles.Add(highlightedTile);

                SetTileColor(
                    highlightedTile,
                    selectedColor
                );
            }
        }
        else
        {
            if (selectedTiles.Contains(highlightedTile))
            {
                SetTileColor(
                    highlightedTile,
                    selectedColor
                );
            }
            else
            {
                SetTileColor(
                    highlightedTile,
                    highlightColor
                );
            }
        }

        UpdateTileVisibility(oldHighlightedTile);
        UpdateTileVisibility(highlightedTile);
    }
 
    // Clear selection
    public void ClearSelection()
    {
        foreach (Tile tile in selectedTiles)
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
                highlightedTile,
                highlightColor
            );
        }

        RefreshAllTileVisibility();
    }
    private void ClearEverything()
    {
        foreach (Tile tile in selectedTiles)
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
                highlightedTile,
                normalColor
            );
        }

        highlightedTile = null;

        selectHeld = false;

        RefreshAllTileVisibility();
    }

    // Grid color and visibility
    public void SetTileColor(Tile tile, Color color)
    {
        TerraformTileGridOverlay overlay =
        GetOverlay(tile);

        if (overlay == null)
            return;

        overlay.SetTileColor(tile, color);
    }
    private TerraformTileGridOverlay GetOverlay(Tile tile)
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
    private void UpdateTileVisibility(Tile tile)
    {
        TerraformTileGridOverlay overlay =
        GetOverlay(tile);

        if (overlay == null)
            return;

        switch (visibilityMode)
        {
            case TileGridVisibilityMode.AllTiles:

                overlay.SetTileVisible(
                    tile,
                    true
                );

                break;


            case TileGridVisibilityMode.HighlightedAndSelected:

                bool shouldBeVisible =
                    tile == highlightedTile ||
                    selectedTiles.Contains(tile);

                overlay.SetTileVisible(
                    tile,
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
                UpdateTileVisibility(tile);
            }
        }
    }
    // Selected tiles getter -----------------------
    public IEnumerable<Tile> GetSelectedTiles()
    {
        return selectedTiles;
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
        Debug.Log("TIleMove event: " + context.phase);

        if (!uiStateManager.IsTerraforming())
            return;

        if(!context.performed)
            return;

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

        if (selectedTiles.Contains(highlightedTile))
        {
            selectedTiles.Remove(highlightedTile);

            SetTileColor(
                highlightedTile,
                highlightColor
            );
        }
        else
        {
            selectedTiles.Add(highlightedTile);

            SetTileColor(
                highlightedTile,
                selectedColor
            );
        }

        UpdateTileVisibility(highlightedTile);
    }
    // --------------------------------------------------
}
