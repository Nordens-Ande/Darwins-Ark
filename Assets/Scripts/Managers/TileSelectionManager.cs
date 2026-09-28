using Assets.Scripts.Environment;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelectionManager : MonoBehaviour
{
    [SerializeField] private UIStateManager uiStateManager;
    [SerializeField] private float doubleTapTime = 0.3f;

    private Tile highlightedTile;

    private HashSet<Tile> selectedTiles =
        new HashSet<Tile>();

    private float lastCtrlPressTime = -1f;

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
        }
        
        // Just existed terraform mode
        if(!isTerraforming && wasTerraforming)
        {
            ClearEverything();
        }

        wasTerraforming = isTerraforming;

        // Do nothing outside terraform mode 
        if (!isTerraforming)
            return;

        if (Keyboard.current == null)
            return;

        HandleMovement();
        HandleCtrl();
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

    // Movement with arrow keys
    private void HandleMovement()
    {
        int xDirection = 0;
        int zDirection = 0;

        // UP
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            zDirection = 1;  

        // DOWN
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            zDirection = -1;

        // RIGHT
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            xDirection = 1;

        // LEFT
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            xDirection = -1;

        // STILL
        if (xDirection == 0 && zDirection == 0)
            return;

        Debug.Log("x: " + xDirection + ", z: " +  zDirection);

        MoveHighlight(xDirection, zDirection);
    }

    private void MoveHighlight(int xDirection, int zDirection)
    {
        if (highlightedTile == null)
            return;

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

        // Outside map
        if (newTile == null)
            return;


        // OLD highlighted tile
        // If it was selected, keep it yellow.
        // Otherwise return it to normal red.
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
                normalColor
            );
        }


        // Move highlight to new tile
        highlightedTile = newTile;


        bool ctrlHeld =
            Keyboard.current.leftCtrlKey.isPressed;


        // CTRL + arrow:
        // select the tile we move onto
        if (ctrlHeld)
        {
            selectedTiles.Add(highlightedTile);

            SetTileColor(
                highlightedTile,
                selectedColor
            );
        }
        else
        {
            // No CTRL:
            // Do NOT select anything.

            if (selectedTiles.Contains(highlightedTile))
            {
                // Already selected from earlier -> stay yellow
                SetTileColor(
                    highlightedTile,
                    selectedColor
                );
            }
            else
            {
                // Not selected -> only highlighted
                SetTileColor(
                    highlightedTile,
                    highlightColor
                );
            }
        }

        Debug.Log(
            "Selected tiles: " +
            selectedTiles.Count
        );
    }

    // CTRL selection
    private void HandleCtrl()
    {
        if (!Keyboard.current.leftCtrlKey.wasPressedThisFrame)
            return;

        float currentTime = Time.unscaledTime;

        // Double tap CTRL = clear all selected tiles
        if (currentTime - lastCtrlPressTime <= doubleTapTime)
        {
            ClearSelection();
            lastCtrlPressTime = -1f;
            return;
        }

        lastCtrlPressTime = currentTime;

        if (highlightedTile == null)
            return;

        // Toggle selection, like Ctrl in File Explorer
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
        lastCtrlPressTime = -1f;
    }

    // Grid color
    private void SetTileColor(Tile tile, Color color)
    {
        Chunk chunk =
            TileManager.Instance.GetChunk(
                tile.position.x,
                tile.position.z
            );

        if (chunk == null) 
            return;

        int chunkIndex = 
            TileManager.Instance.chunks.IndexOf(chunk);

        if (chunkIndex < 0)
            return;

        Transform grid =
            TileManager.Instance.chunkObjects[chunkIndex]
            .transform.Find("TileGrid");

        if (grid == null)
            return;

        TerraformTileGridOverlay overlay =
            grid.GetComponent<TerraformTileGridOverlay>();

        if (overlay != null)
        {
            overlay.SetTileColor(tile, color);
        }
    }

    // Selected tiles getter
    public IEnumerable<Tile> GetSelectedTiles()
    {
        return selectedTiles;
    }
}
